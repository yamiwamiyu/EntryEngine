using System;
using System.Collections.Specialized;
using System.Drawing;
namespace PhotoshopFile
{
	public class MaskInfo
	{
		public Mask LayerMask
		{
			get;
			set;
		}
		public Mask UserMask
		{
			get;
			set;
		}
		public MaskInfo()
		{
		}
		public MaskInfo(PsdBinaryReader reader, Layer layer)
		{
			uint num = reader.ReadUInt32();
			if (num <= 0u)
			{
				return;
			}
			long position = reader.BaseStream.Position;
			long position2 = position + (long)((ulong)num);
			Rectangle rect = reader.ReadRectangle();
			byte color = reader.ReadByte();
			byte data = reader.ReadByte();
			this.LayerMask = new Mask(layer, rect, color, new BitVector32((int)data));
			if (num == 36u)
			{
				byte data2 = reader.ReadByte();
				byte color2 = reader.ReadByte();
				Rectangle rect2 = reader.ReadRectangle();
				this.UserMask = new Mask(layer, rect2, color2, new BitVector32((int)data2));
			}
			reader.BaseStream.Position = position2;
		}
		public void Save(PsdBinaryWriter writer)
		{
			if (this.LayerMask == null)
			{
				writer.Write(0u);
				return;
			}
			using (new PsdBlockLengthWriter(writer))
			{
				writer.Write(this.LayerMask.Rect);
				writer.Write(this.LayerMask.BackgroundColor);
				writer.Write((byte)this.LayerMask.Flags.Data);
				if (this.UserMask == null)
				{
					writer.Write(0);
				}
				else
				{
					writer.Write((byte)this.UserMask.Flags.Data);
					writer.Write(this.UserMask.BackgroundColor);
					writer.Write(this.UserMask.Rect);
				}
			}
		}
	}
}
