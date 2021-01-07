using System;
using System.Diagnostics;
using System.Globalization;

namespace PSDFile
{
    public class BlendingRanges
    {
        /// <summary>
        /// The layer to which this channel belongs
        /// </summary>
        public Layer Layer { get; private set; }

        public byte[] Data { get; set; }

        ///////////////////////////////////////////////////////////////////////////

        public BlendingRanges(Layer layer)
        {
            Layer = layer;
            Data = new byte[0];
        }

        ///////////////////////////////////////////////////////////////////////////

        public BlendingRanges(PsdBinaryReader reader, Layer layer)
        {

            Layer = layer;
            var dataLength = reader.ReadInt32();
            if (dataLength <= 0)
                return;

            Data = reader.ReadBytes(dataLength);
        }

        ///////////////////////////////////////////////////////////////////////////

        public void Save(PsdBinaryWriter writer)
        {
            if (Data == null)
            {
                writer.Write((UInt32)0);
                return;
            }

            writer.Write((UInt32)Data.Length);
            writer.Write(Data);
        }
    }
}
