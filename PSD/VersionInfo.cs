using System;
namespace PhotoshopFile
{
	public class VersionInfo : ImageResource
	{
		public override ResourceID ID
		{
			get
			{
				return ResourceID.VersionInfo;
			}
		}
		public uint Version
		{
			get;
			set;
		}
		public bool HasRealMergedData
		{
			get;
			set;
		}
		public string ReaderName
		{
			get;
			set;
		}
		public string WriterName
		{
			get;
			set;
		}
		public uint FileVersion
		{
			get;
			set;
		}
		public VersionInfo() : base(string.Empty)
		{
		}
		public VersionInfo(PsdBinaryReader reader, string name) : base(name)
		{
			this.Version = reader.ReadUInt32();
			this.HasRealMergedData = reader.ReadBoolean();
			this.ReaderName = reader.ReadUnicodeString();
			this.WriterName = reader.ReadUnicodeString();
			this.FileVersion = reader.ReadUInt32();
		}
		protected override void WriteData(PsdBinaryWriter writer)
		{
			writer.Write(this.Version);
			writer.Write(this.HasRealMergedData);
			writer.WriteUnicodeString(this.ReaderName);
			writer.WriteUnicodeString(this.WriterName);
			writer.Write(this.FileVersion);
		}
	}
}
