﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

using PSDFile.Compression;

namespace PSDFile
{
    public class ChannelList : List<Channel>
    {
        /// <summary>
        /// Returns channels with nonnegative IDs as an array, so that accessing
        /// a channel by Id can be optimized into pointer arithmetic rather than
        /// being implemented as a List scan.
        /// </summary>
        /// <remarks>
        /// This optimization is crucial for blitting lots of pixels back and
        /// forth between Photoshop's per-channel representation, and Paint.NET's
        /// per-pixel BGRA representation.
        /// </remarks>
        public Channel[] ToIdArray()
        {
            var maxId = this.Max(x => x.ID);
            var idArray = new Channel[maxId + 1];
            foreach (var channel in this)
            {
                if (channel.ID >= 0)
                    idArray[channel.ID] = channel;
            }
            return idArray;
        }

        public ChannelList()
          : base()
        {
        }

        public Channel GetId(int id)
        {
            return this.Single(x => x.ID == id);
        }

        public bool ContainsId(int id)
        {
            return this.Exists(x => x.ID == id);
        }

        public byte[] MergeChannels(int width, int height)
        {
            int length = this.Count;
            int num2 = this[0].ImageData.Length;

            byte[] buffer = new byte[(width * height) * length];
            int num3 = 0;
            for (int i = 0; i < num2; i++)
            {
                for (int j = length - 1; j >= 0; j--)
                {
                    buffer[num3++] = this[j].ImageData[i];
                }
            }
            return buffer;
        }
    }

    ///////////////////////////////////////////////////////////////////////////

    public enum EChannelType : short
    {
        UserMask = -3,
        LayerMask = -2,
        Alpha = -1,
        R = 0,
        G = 1,
        B = 2,
    }

    [DebuggerDisplay("ID = {ID}")]
    public class Channel
    {
        /// <summary>
        /// The layer to which this channel belongs
        /// </summary>
        public Layer Layer { get; private set; }

        /// <summary>
        /// Channel ID.
        /// <list type="bullet">
        /// <item>-1 = transparency mask</item>
        /// <item>-2 = user-supplied layer mask, or vector mask</item>
        /// <item>-3 = user-supplied layer mask, if channel -2 contains a vector mask</item>
        /// <item>
        /// Nonnegative channel IDs give the actual image channels, in the
        /// order defined by the colormode.  For example, 0, 1, 2 = R, G, B.
        /// </item>
        /// </list>
        /// </summary>
        public short ID { get; set; }

        public EChannelType ChannelType { get { return (EChannelType)ID; } }

        public Rectangle Rect
        {
            get
            {
                switch (ID)
                {
                    case -2:
                        return Layer.Masks.LayerMask.Rect;
                    case -3:
                        return Layer.Masks.UserMask.Rect;
                    default:
                        return Layer.Rect;
                }
            }
        }

        /// <summary>
        /// Total length of the channel data, including compression headers.
        /// <para>Updated by <see cref="CompressImageData"/></para>
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// Raw image data for this color channel, in compressed on-disk format.
        /// </summary>
        /// <remarks>
        /// If null, the ImageData will be automatically compressed during save.
        /// </remarks>
        public byte[] ImageDataRaw { get; set; }

        /// <summary>
        /// Decompressed image data for this color channel.
        /// </summary>
        /// <remarks>
        /// When making changes to the ImageData, set ImageDataRaw to null so that
        /// the correct data will be compressed during save.
        /// </remarks>
        public byte[] ImageData { get; set; }

        /// <summary>
        /// Image compression method used.
        /// </summary>
        public ImageCompression ImageCompression { get; set; }

        /// <summary>
        /// RLE-compressed length of each row.
        /// </summary>
        public RleRowLengths RleRowLengths { get; set; }

        //////////////////////////////////////////////////////////////////

        internal Channel(short id, Layer layer)
        {
            ID = id;
            Layer = layer;
        }

        internal Channel(PsdBinaryReader reader, Layer layer)
        {
            ID = reader.ReadInt16();
            Length = (layer.PsdFile.IsLargeDocument)
              ? reader.ReadInt64()
              : reader.ReadInt32();
            Layer = layer;
        }

        internal void Save(PsdBinaryWriter writer)
        {
            writer.Write(ID);
            if (Layer.PsdFile.IsLargeDocument)
            {
                writer.Write(Length);
            }
            else
            {
                writer.Write((Int32)Length);
            }
        }

        //////////////////////////////////////////////////////////////////

        internal void LoadPixelData(PsdBinaryReader reader)
        {
            if (Length == 0)
            {
                ImageCompression = ImageCompression.Raw;
                ImageDataRaw = new byte[0];
                return;
            }

            var endPosition = reader.BaseStream.Position + this.Length;
            ImageCompression = (ImageCompression)reader.ReadInt16();
            var longDataLength = this.Length - 2;
            Util.CheckByteArrayLength(longDataLength);
            var dataLength = (int)longDataLength;

            switch (ImageCompression)
            {
                case ImageCompression.Raw:
                    ImageDataRaw = reader.ReadBytes(dataLength);
                    break;
                case ImageCompression.Rle:
                    // RLE row lengths
                    RleRowLengths = new RleRowLengths(reader, Rect.Height,
                      Layer.PsdFile.IsLargeDocument);
                    var rleDataLength = (int)(endPosition - reader.BaseStream.Position);
                    Debug.Assert(rleDataLength == RleRowLengths.Total,
                      "RLE row lengths do not sum to length of channel image data.");

                    // The PSD specification states that rows are padded to even sizes.
                    // However, Photoshop doesn't actually do this.  RLE rows can have
                    // odd lengths in the header, and there is no padding between rows.
                    ImageDataRaw = reader.ReadBytes(rleDataLength);
                    break;
                case ImageCompression.Zip:
                case ImageCompression.ZipPrediction:
                    ImageDataRaw = reader.ReadBytes(dataLength);
                    break;
            }

            Debug.Assert(reader.BaseStream.Position == endPosition,
              "Pixel data was not fully read in.");
        }

        /// <summary>
        /// Decodes the raw image data from the compressed on-disk format into
        /// an uncompressed bitmap, in native byte order.
        /// </summary>
        public void DecodeImageData()
        {
            if ((ImageCompression == ImageCompression.Raw)
              && (Layer.PsdFile.BitDepth <= 8))
            {
                ImageData = ImageDataRaw;
                return;
            }

            var image = ImageDataFactory.Create(this, ImageDataRaw);
            var longLength = (long)image.BytesPerRow * Rect.Height;
            Util.CheckByteArrayLength(longLength);
            ImageData = new byte[longLength];
            image.Read(ImageData);
        }

        /// <summary>
        /// Compresses the image data.
        /// </summary>
        public void CompressImageData()
        {
            // Do not recompress if compressed data is already present.
            if (ImageDataRaw != null)
                return;

            if (ImageData == null)
                return;

            if (ImageCompression == ImageCompression.Rle)
            {
                RleRowLengths = new RleRowLengths(Rect.Height);
            }

            var compressor = ImageDataFactory.Create(this, null);
            compressor.Write(ImageData);
            ImageDataRaw = compressor.ReadCompressed();

            Length = 2 + ImageDataRaw.Length;
            if (ImageCompression == ImageCompression.Rle)
            {
                var rowLengthSize = Layer.PsdFile.IsLargeDocument ? 4 : 2;
                Length += rowLengthSize * Rect.Height;
            }
        }

        internal void SavePixelData(PsdBinaryWriter writer)
        {
            writer.Write((short)ImageCompression);
            if (ImageDataRaw == null)
            {
                return;
            }

            if (ImageCompression == PSDFile.ImageCompression.Rle)
            {
                RleRowLengths.Write(writer, Layer.PsdFile.IsLargeDocument);
            }
            writer.Write(ImageDataRaw);
        }
    }
}
