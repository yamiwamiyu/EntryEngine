using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
namespace PhotoshopFile
{
	[DebuggerDisplay("Name = {Name}")]
	public class Layer
	{
		private string blendModeKey;
		private static int protectTransBit = BitVector32.CreateMask();
		private static int visibleBit = BitVector32.CreateMask(Layer.protectTransBit);
		private BitVector32 flags = default(BitVector32);
		internal PsdFile PsdFile
		{
			get;
			private set;
		}
		public Rectangle Rect
		{
			get;
			set;
		}
		public ChannelList Channels
		{
			get;
			private set;
		}
		public Channel AlphaChannel
		{
			get
			{
				if (this.Channels.ContainsId(-1))
				{
					return this.Channels.GetId(-1);
				}
				return null;
			}
		}
		public string BlendModeKey
		{
			get
			{
				return this.blendModeKey;
			}
			set
			{
				if (value.Length != 4)
				{
					throw new ArgumentException("Key length must be 4");
				}
				this.blendModeKey = value;
			}
		}
		public EBlendModeKey BlendMode
		{
			get { return BlendModeKeyTable.GetBlendModeKey(blendModeKey); }
		}
		public byte Opacity
		{
			get;
			set;
		}
		public bool Clipping
		{
			get;
			set;
		}
		public bool Visible
		{
			get
			{
				return !this.flags[Layer.visibleBit];
			}
			set
			{
				this.flags[Layer.visibleBit] = !value;
			}
		}
		public bool ProtectTrans
		{
			get
			{
				return this.flags[Layer.protectTransBit];
			}
			set
			{
				this.flags[Layer.protectTransBit] = value;
			}
		}
		public string Name
		{
			get;
			set;
		}
		public BlendingRanges BlendingRangesData
		{
			get;
			set;
		}
		public MaskInfo Masks
		{
			get;
			set;
		}
		public List<LayerInfo> AdditionalInfo
		{
			get;
			set;
		}
		public Layer(PsdFile psdFile)
		{
			this.PsdFile = psdFile;
			this.Rect = Rectangle.Empty;
			this.Channels = new ChannelList();
			this.BlendModeKey = "norm";
			this.AdditionalInfo = new List<LayerInfo>();
		}
		public Layer(PsdBinaryReader reader, PsdFile psdFile) : this(psdFile)
		{
			this.Rect = reader.ReadRectangle();
			int num = (int)reader.ReadUInt16();
			for (int i = 0; i < num; i++)
			{
				Channel item = new Channel(reader, this);
				this.Channels.Add(item);
			}
			string a = reader.ReadAsciiChars(4);
			if (a != "8BIM")
			{
				throw new PsdInvalidException("Invalid signature in layer header.");
			}
			this.BlendModeKey = reader.ReadAsciiChars(4);
			this.Opacity = reader.ReadByte();
			this.Clipping = reader.ReadBoolean();
			byte data = reader.ReadByte();
			this.flags = new BitVector32((int)data);
			reader.ReadByte();
			uint num2 = reader.ReadUInt32();
			long position = reader.BaseStream.Position;
			this.Masks = new MaskInfo(reader, this);
			this.BlendingRangesData = new BlendingRanges(reader, this);
			this.Name = reader.ReadPascalString(4);
			long num3 = position + (long)((ulong)num2);
			while (reader.BaseStream.Position < num3)
			{
				LayerInfo item2 = LayerInfoFactory.Load(reader);
				this.AdditionalInfo.Add(item2);
			}
			foreach (LayerInfo current in this.AdditionalInfo)
			{
				string key;
				if ((key = current.Key) != null && key == "luni")
				{
					this.Name = ((LayerUnicodeName)current).Name;
				}
			}
		}
		public unsafe void CreateMissingChannels()
		{
			short num = this.PsdFile.ColorMode.MinChannelCount();
			for (short num2 = 0; num2 < num; num2 += 1)
			{
				if (!this.Channels.ContainsId((int)num2))
				{
					int num3 = this.Rect.Height * this.Rect.Width;
					Channel channel = new Channel(num2, this);
					channel.ImageData = new byte[num3];
					fixed (byte* ptr = &channel.ImageData[0])
					{
						Util.Fill(ptr, ptr + num3 / 1, 255);
					}
					this.Channels.Add(channel);
				}
			}
		}
		public void PrepareSave()
		{
			foreach (Channel current in this.Channels)
			{
				current.CompressImageData();
			}
			IEnumerable<LayerInfo> source = 
				from x in this.AdditionalInfo
				where x is LayerUnicodeName
				select x;
			if (source.Count<LayerInfo>() > 1)
			{
				throw new PsdInvalidException("Layer has more than one LayerUnicodeName.");
			}
			LayerUnicodeName layerUnicodeName = (LayerUnicodeName)source.FirstOrDefault<LayerInfo>();
			if (layerUnicodeName == null)
			{
				layerUnicodeName = new LayerUnicodeName(this.Name);
				this.AdditionalInfo.Add(layerUnicodeName);
				return;
			}
			if (layerUnicodeName.Name != this.Name)
			{
				layerUnicodeName.Name = this.Name;
			}
		}
		public void Save(PsdBinaryWriter writer)
		{
			writer.Write(this.Rect);
			writer.Write((short)this.Channels.Count);
			foreach (Channel current in this.Channels)
			{
				current.Save(writer);
			}
			writer.WriteAsciiChars("8BIM");
			writer.WriteAsciiChars(this.BlendModeKey);
			writer.Write(this.Opacity);
			writer.Write(this.Clipping);
			writer.Write((byte)this.flags.Data);
			writer.Write((byte)0);
			using (new PsdBlockLengthWriter(writer))
			{
				this.Masks.Save(writer);
				this.BlendingRangesData.Save(writer);
				long arg_C6_0 = writer.BaseStream.Position;
				writer.WritePascalString(this.Name, 4, 31);
				foreach (LayerInfo current2 in this.AdditionalInfo)
				{
					current2.Save(writer);
				}
			}
		}
	}
}
