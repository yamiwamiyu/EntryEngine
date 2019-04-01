using System;
namespace PhotoshopFile
{
	public class RawImageResource : ImageResource
	{
		private ResourceID id;
		public byte[] Data
		{
			get;
			private set;
		}
		public override ResourceID ID
		{
			get
			{
				return this.id;
			}
		}
		public RawImageResource(ResourceID resourceId, string name) : base(name)
		{
			this.id = resourceId;
		}
		public RawImageResource(PsdBinaryReader reader, string signature, ResourceID resourceId, string name, int numBytes) : base(name)
		{
			base.Signature = signature;
			this.id = resourceId;
			this.Data = reader.ReadBytes(numBytes);
		}
		protected override void WriteData(PsdBinaryWriter writer)
		{
			writer.Write(this.Data);
		}
	}
}
