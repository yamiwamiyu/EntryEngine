using System;
namespace PhotoshopFile
{
	public class LayerUnicodeName : LayerInfo
	{
		public override string Key
		{
			get
			{
				return "luni";
			}
		}
		public string Name
		{
			get;
			set;
		}
		public LayerUnicodeName(string name)
		{
			this.Name = name;
		}
		public LayerUnicodeName(PsdBinaryReader reader)
		{
			this.Name = reader.ReadUnicodeString();
		}
		protected override void WriteData(PsdBinaryWriter writer)
		{
			long position = writer.BaseStream.Position;
			writer.WriteUnicodeString(this.Name);
			writer.WritePadding(position, 4);
		}
	}
}
