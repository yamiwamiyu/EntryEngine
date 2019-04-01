using System;
namespace PhotoshopFile
{
	public abstract class LayerInfo
	{
		public abstract string Key
		{
			get;
		}
		protected abstract void WriteData(PsdBinaryWriter writer);
		public void Save(PsdBinaryWriter writer)
		{
			writer.WriteAsciiChars("8BIM");
			writer.WriteAsciiChars(this.Key);
			long position = writer.BaseStream.Position;
			using (new PsdBlockLengthWriter(writer))
			{
				this.WriteData(writer);
			}
			writer.WritePadding(position, 4);
		}
	}
}
