using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using PSDFile.Compression;

namespace PSDFile
{
    [DebuggerDisplay("Name = {Name}")]
    public class Layer
    {
        internal PsdFile PsdFile { get; private set; }

        /// <summary>
        /// The rectangle containing the contents of the layer.
        /// </summary>
        public Rectangle Rect { get; set; }

        /// <summary>
        /// Image channels.
        /// </summary>
        public ChannelList Channels { get; private set; }

        /// <summary>
        /// Returns alpha channel if it exists, otherwise null.
        /// </summary>
        public Channel AlphaChannel { get { return Channels.SingleOrDefault(x => x.ID == -1); } }

        private string blendModeKey;
        /// <summary>
        /// Photoshop blend mode key for the layer
        /// </summary>
        public string BlendModeKey
        {
            get { return  blendModeKey; }
            set
            {
                if (value.Length != 4)
                {
                    throw new ArgumentException(
                      "BlendModeKey must be 4 characters in length.");
                }
                blendModeKey = value;
            }
        }

        /// <summary>
        /// 0 = transparent ... 255 = opaque
        /// </summary>
        public byte Opacity { get; set; }

        /// <summary>
        /// false = base, true = non-base
        /// </summary>
        public bool Clipping { get; set; }

        private static int protectTransBit = BitVector32.CreateMask();
        private static int visibleBit = BitVector32.CreateMask(protectTransBit);
        BitVector32 flags = new BitVector32();

        /// <summary>
        /// If true, the layer is visible.
        /// </summary>
        public bool Visible
        {
            get { return !flags[visibleBit]; }
            set { flags[visibleBit] = !value; }
        }

        /// <summary>
        /// Protect the transparency
        /// </summary>
        public bool ProtectTrans
        {
            get { return flags[protectTransBit]; }
            set { flags[protectTransBit] = value; }
        }

        /// <summary>
        /// The descriptive layer name
        /// </summary>
        public string Name { get; set; }

        public BlendingRanges BlendingRangesData { get; set; }

        public MaskInfo Masks { get; set; }

        public List<LayerInfo> AdditionalInfo { get; set; }

        public LayerText LayerText { get; private set; }

        public LayerSectionInfo LayerSection { get; private set; }
        /// <summary>图层分隔符，PS中不可见</summary>
        public bool IsGroupDivider { get { return LayerSection != null && LayerSection.SectionType == LayerSectionType.SectionDivider; } }
        /// <summary>图层组</summary>
        public bool IsGroup { get { return LayerSection != null && (LayerSection.SectionType == LayerSectionType.OpenFolder || LayerSection.SectionType == LayerSectionType.ClosedFolder); } }
        /// <summary>图层</summary>
        public bool IsLayer { get { return LayerSection == null || LayerSection.SectionType == LayerSectionType.Layer; } }

        ///////////////////////////////////////////////////////////////////////////

        public Layer(PsdFile psdFile)
        {
            PsdFile = psdFile;
            Rect = Rectangle.Empty;
            Channels = new ChannelList();
            BlendModeKey = PsdBlendMode.Normal;
            AdditionalInfo = new List<LayerInfo>();
        }

        public Layer(PsdBinaryReader reader, PsdFile psdFile)
          : this(psdFile)
        {
            Rect = reader.ReadRectangle();

            //-----------------------------------------------------------------------
            // Read channel headers.  Image data comes later, after the layer header.

            int numberOfChannels = reader.ReadUInt16();
            for (int channel = 0; channel < numberOfChannels; channel++)
            {
                var ch = new Channel(reader, this);
                Channels.Add(ch);
            }

            //-----------------------------------------------------------------------
            // 

            var signature = reader.ReadAsciiChars(4);
            if (signature != "8BIM")
                throw (new PsdInvalidException("Invalid signature in layer header."));

            BlendModeKey = reader.ReadAsciiChars(4);
            Opacity = reader.ReadByte();
            Clipping = reader.ReadBoolean();

            var flagsByte = reader.ReadByte();
            flags = new BitVector32(flagsByte);
            reader.ReadByte(); //padding

            //-----------------------------------------------------------------------

            // This is the total size of the MaskData, the BlendingRangesData, the 
            // Name and the AdjustmentLayerInfo.
            var extraDataSize = reader.ReadUInt32();
            var extraDataStartPosition = reader.BaseStream.Position;

            Masks = new MaskInfo(reader, this);
            BlendingRangesData = new BlendingRanges(reader, this);
            Name = reader.ReadPascalString(4);

            //-----------------------------------------------------------------------
            // Process Additional Layer Information

            long adjustmentLayerEndPos = extraDataStartPosition + extraDataSize;
            while (reader.BaseStream.Position < adjustmentLayerEndPos)
            {
                var layerInfo = LayerInfoFactory.Load(reader,
                  psdFile: this.PsdFile,
                  globalLayerInfo: false);
                AdditionalInfo.Add(layerInfo);
            }

            foreach (var adjustmentInfo in AdditionalInfo)
            {
                if (adjustmentInfo is LayerUnicodeName)
                    Name = ((LayerUnicodeName)adjustmentInfo).Name;
                else if (adjustmentInfo is LayerText)
                    LayerText = (LayerText)adjustmentInfo;
                else if (adjustmentInfo is LayerSectionInfo)
                    LayerSection = (LayerSectionInfo)adjustmentInfo;
            }

            PsdFile.LoadContext.OnLoadLayerHeader(this);
        }

        ///////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create ImageData for any missing channels.
        /// </summary>
        public void CreateMissingChannels()
        {
            var channelCount = this.PsdFile.ColorMode.MinChannelCount();
            for (short id = 0; id < channelCount; id++)
            {
                if (!this.Channels.ContainsId(id))
                {
                    var size = this.Rect.Height * this.Rect.Width;

                    var ch = new Channel(id, this);
                    ch.ImageData = new byte[size];
                    unsafe
                    {
                        fixed (byte* ptr = &ch.ImageData[0])
                        {
                            Util.Fill(ptr, ptr + size, (byte)255);
                        }
                    }

                    this.Channels.Add(ch);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        public void PrepareSave()
        {
            foreach (var ch in Channels)
            {
                ch.CompressImageData();
            }

            // Create or update the Unicode layer name to be consistent with the
            // ANSI layer name.
            var layerUnicodeNames = AdditionalInfo.Where(x => x is LayerUnicodeName);
            if (layerUnicodeNames.Count() > 1)
            {
                throw new PsdInvalidException(
                  "Layer can only have one {nameof(LayerUnicodeName)}.");
            }

            var layerUnicodeName = (LayerUnicodeName)layerUnicodeNames.FirstOrDefault();
            if (layerUnicodeName == null)
            {
                layerUnicodeName = new LayerUnicodeName(Name);
                AdditionalInfo.Add(layerUnicodeName);
            }
            else if (layerUnicodeName.Name != Name)
            {
                layerUnicodeName.Name = Name;
            }
        }

        public void Save(PsdBinaryWriter writer)
        {
            writer.Write(Rect);

            //-----------------------------------------------------------------------

            writer.Write((short)Channels.Count);
            foreach (var ch in Channels)
                ch.Save(writer);

            //-----------------------------------------------------------------------

            writer.WriteAsciiChars("8BIM");
            writer.WriteAsciiChars(BlendModeKey);
            writer.Write(Opacity);
            writer.Write(Clipping);

            writer.Write((byte)flags.Data);
            writer.Write((byte)0);

            //-----------------------------------------------------------------------

            using (new PsdBlockLengthWriter(writer))
            {
                Masks.Save(writer);
                BlendingRangesData.Save(writer);

                var namePosition = writer.BaseStream.Position;

                // Legacy layer name is limited to 31 bytes.  Unicode layer name
                // can be much longer.
                writer.WritePascalString(Name, 4, 31);

                foreach (LayerInfo info in AdditionalInfo)
                {
                    info.Save(writer,
                      globalLayerInfo: false,
                      isLargeDocument: PsdFile.IsLargeDocument);
                }
            }
        }


        public bool HasImage
        {
            get
            {
                var sectionInfo = (LayerSectionInfo)this.AdditionalInfo.FirstOrDefault(info => info is LayerSectionInfo);
                if (sectionInfo != null && sectionInfo.SectionType != LayerSectionType.Layer)
                    return false;
                if (Rect.Width == 0 || Rect.Height == 0)
                    return false;
                return true;
            }
        }

        public int Width { get { return Rect.Width; }}
        public int Height { get { return  Rect.Height;}}

        public unsafe Bitmap GetBitmap()
        {
            if (HasImage == false)
                return null;

            return DecodeImage(this);
        }


        private struct PixelData
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;
        }
        public static Bitmap DecodeImage(Layer layer)
        {
            if (layer.Rect.Width == 0 || layer.Rect.Height == 0)
            {
                return null;
            }

            Bitmap bitmap = new Bitmap(layer.Rect.Width, layer.Rect.Height, PixelFormat.Format32bppArgb);

            BitmapData bd = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            unsafe
            {
                byte* pCurrRowPixel = (byte*)bd.Scan0.ToPointer();

                Mask mask = null;
                Channel r, g, b, a;
                r = g = b = a = null;
                foreach (var item in layer.Channels)
                {
                    switch (item.ChannelType)
                    {
                        case EChannelType.UserMask:
                            mask = layer.Masks.UserMask;
                            break;
                        case EChannelType.LayerMask:
                            if (mask == null)
                                mask = layer.Masks.LayerMask;
                            break;
                        case EChannelType.Alpha: a = item; break;
                        case EChannelType.R: r = item; break;
                        case EChannelType.G: g = item; break;
                        case EChannelType.B: b = item; break;
                        default: throw new NotImplementedException("未知通道类型：" + item.ChannelType);
                    }
                }

                PixelData pixelColor;
                for (int y = 0; y < layer.Rect.Height; y++)
                {
                    int rowIndex = y * layer.Rect.Width;
                    PixelData* pCurrPixel = (PixelData*)pCurrRowPixel;
                    for (int x = 0; x < layer.Rect.Width; x++)
                    {
                        int pos = rowIndex + x;

                        if (r == null) pixelColor.Red = 0; else pixelColor.Red = r.ImageData[pos];
                        if (g == null) pixelColor.Green = 0; else pixelColor.Green = g.ImageData[pos];
                        if (b == null) pixelColor.Blue = 0; else pixelColor.Blue = b.ImageData[pos];
                        if (a == null) pixelColor.Alpha = 0; else pixelColor.Alpha = a.ImageData[pos];

                        if (mask != null)
                        {
                            int maskAlpha = GetColor(mask, x, y);
                            pixelColor.Alpha = (byte)((pixelColor.Alpha * maskAlpha) / 255);
                        }

                        *pCurrPixel = pixelColor;

                        pCurrPixel += 1;
                    }
                    pCurrRowPixel += bd.Stride;
                }
            }

            bitmap.UnlockBits(bd);

            return bitmap;
        }
        private static int GetColor(Mask mask, int x, int y)
        {
            int c = 255;

            if (mask.PositionVsLayer)
            {
                x -= mask.Rect.X;
                y -= mask.Rect.Y;
            }
            else
            {
                x = (x + mask.Layer.Rect.X) - mask.Rect.X;
                y = (y + mask.Layer.Rect.Y) - mask.Rect.Y;
            }

            if (y >= 0 && y < mask.Rect.Height &&
                 x >= 0 && x < mask.Rect.Width)
            {
                int pos = y * mask.Rect.Width + x;
                if (pos < mask.ImageData.Length)
                    c = mask.ImageData[pos];
                else
                    c = 255;
            }

            return c;
        }
    }
}
