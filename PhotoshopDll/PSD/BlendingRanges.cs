using System;
namespace PhotoshopFile
{
	public class BlendingRanges
	{
		public Layer Layer
		{
			get;
			private set;
		}
		public byte[] Data
		{
			get;
			set;
		}
		public BlendingRanges(Layer layer)
		{
			this.Layer = layer;
			this.Data = new byte[0];
		}
		public BlendingRanges(PsdBinaryReader reader, Layer layer)
		{
			this.Layer = layer;
			int num = reader.ReadInt32();
			if (num <= 0)
			{
				return;
			}
			this.Data = reader.ReadBytes(num);
		}
		public void Save(PsdBinaryWriter writer)
		{
			writer.Write((uint)this.Data.Length);
			writer.Write(this.Data);
		}
	}
}
