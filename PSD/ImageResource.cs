using System;
using System.Globalization;
namespace PhotoshopFile
{
	public abstract class ImageResource
	{
		private string signature;
		public string Signature
		{
			get
			{
				return this.signature;
			}
			set
			{
				if (value.Length != 4)
				{
					throw new ArgumentException("Signature must have length of 4");
				}
				this.signature = value;
			}
		}
		public string Name
		{
			get;
			set;
		}
		public abstract ResourceID ID
		{
			get;
		}
		protected ImageResource(string name)
		{
			this.Signature = "8BIM";
			this.Name = name;
		}
		public void Save(PsdBinaryWriter writer)
		{
			writer.WriteAsciiChars(this.Signature);
			writer.Write((ushort)this.ID);
			writer.WritePascalString(this.Name, 2, 255);
			long position = writer.BaseStream.Position;
			using (new PsdBlockLengthWriter(writer))
			{
				this.WriteData(writer);
			}
			writer.WritePadding(position, 2);
		}
		protected abstract void WriteData(PsdBinaryWriter writer);
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} {1}", new object[]
			{
				this.ID,
				this.Name
			});
		}
	}
}
