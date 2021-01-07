using System;
using System.Diagnostics;
namespace PhotoshopFile
{
	[DebuggerDisplay("Layer Info: { key }")]
	public class RawLayerInfo : LayerInfo
	{
		private string key;
		public override string Key
		{
			get
			{
				return this.key;
			}
		}
		public byte[] Data
		{
			get;
			private set;
		}
		public RawLayerInfo(string key)
		{
			this.key = key;
		}
		public RawLayerInfo(PsdBinaryReader reader, string key, int dataLength)
		{
			this.key = key;
			this.Data = reader.ReadBytes(dataLength);
		}
		protected override void WriteData(PsdBinaryWriter writer)
		{
			writer.Write(this.Data);
		}
	}

	public struct OSType
	{
		public string Key;
		public string OSTypeKey;
		public object Data;
	}
	public struct Descriptor
	{
		public string ClassIDName;
		public string ClassID;
		public OSType[] OSTypeDatas;

		public Descriptor(PsdBinaryReader reader)
		{
			ClassIDName = reader.ReadUnicodeString();
			ClassID = reader.ReadClassID();
			int ItemCount = reader.ReadInt32();
			OSTypeDatas = new OSType[ItemCount];
			for (int i = 0; i < ItemCount; i++)
			{
				OSType ostype = new OSType();
				//ostype.Key = reader.ReadClassID();
				ostype.Key = reader.ReadAsciiChars(8);
				ostype.OSTypeKey = reader.ReadAsciiChars(4);

				switch (ostype.OSTypeKey)
				{
					case "obj ":
						break;

					case "Objc":
						break;

					case "VlLs":
						break;

					case "doub":
						break;

					case "UntF":
						break;

					case "TEXT":
						ostype.Data = reader.ReadUnicodeString();
						break;

					case "enum":
						break;

					case "long":
						break;

					case "bool":
						break;

					case "GlbO":
						break;

					case "type":
					case "GlbC":
						break;

					case "alis":
						break;

					case "tdta":
						break;

					default:
						break;
						//throw new ArgumentException("error ostype! " + ostype.OSTypeKey);
				}

				reader.ReadPadding(reader.BaseStream.Position, 4);

				OSTypeDatas[i] = ostype;
			}
		}
	}
	public class LayerText : LayerInfo
	{
		public ushort Version = 1;
		public double[] Transform = new double[6];
		public ushort TextVersion = 50;
		public int DescriptorVersion = 16;
		public string ClassIDName;
		public string ClassID;
		public int ItemCount;
		public string OSTypeKey;
		public string OSType;
		public string Text;
		public byte[] Raw;

		private string key;

		public LayerText(PsdBinaryReader reader, int length)
		{
			long position = reader.BaseStream.Position;

			this.Version = reader.ReadUInt16();
			for (int i = 0; i < 6; i++)
			{
				Transform[i] = BitConverter.ToDouble(reader.ReadBytes(8), 0);
			}
			this.TextVersion = reader.ReadUInt16();
			this.DescriptorVersion = reader.ReadInt32();
			this.ClassIDName = reader.ReadUnicodeString();
			this.ClassID = reader.ReadClassID();
			this.ItemCount = reader.ReadInt32();
			this.OSTypeKey = reader.ReadClassID();
			this.OSType = reader.ReadAsciiChars(4);
			if (OSType == "TEXT")
			{
				key = "TySh";
				this.Text = reader.ReadUnicodeString();
				this.Raw = reader.ReadBytes((int)(length - (reader.BaseStream.Position - position)));
			}
			else
			{
				key = null;
			}
		}

		public override string Key
		{
			get { return key; }
		}
		protected override void WriteData(PsdBinaryWriter writer)
		{
			writer.Write(Version);
			for (int i = 0; i < Transform.Length; i++)
			{
				writer.Write(Transform[i]);
			}
			writer.Write(TextVersion);
			writer.Write(DescriptorVersion);
			writer.WriteUnicodeString(ClassIDName);
			writer.Write(4);
			writer.WriteAsciiChars(ClassID);
			writer.Write(ItemCount);
			writer.Write(4);
			writer.WriteAsciiChars(OSTypeKey);
			writer.WriteAsciiChars(OSType);
			writer.WriteUnicodeString(Text);
			writer.Write(Raw);
		}
	}
}
