using System;
using System.Collections.Generic;
namespace PhotoshopFile
{
	public class UnicodeAlphaNames : ImageResource
	{
		private List<string> channelNames = new List<string>();
		public override ResourceID ID
		{
			get
			{
				return ResourceID.UnicodeAlphaNames;
			}
		}
		public List<string> ChannelNames
		{
			get
			{
				return this.channelNames;
			}
		}
		public UnicodeAlphaNames() : base(string.Empty)
		{
		}
		public UnicodeAlphaNames(PsdBinaryReader reader, string name, int resourceDataLength) : base(name)
		{
			long num = reader.BaseStream.Position + (long)resourceDataLength;
			while (reader.BaseStream.Position < num)
			{
				string item = reader.ReadUnicodeString();
				this.ChannelNames.Add(item);
			}
		}
		protected override void WriteData(PsdBinaryWriter writer)
		{
			foreach (string current in this.ChannelNames)
			{
				writer.WriteUnicodeString(current);
			}
		}
	}
}
