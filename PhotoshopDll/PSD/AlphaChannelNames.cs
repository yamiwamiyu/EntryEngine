using System;
using System.Collections.Generic;
namespace PhotoshopFile
{
	public class AlphaChannelNames : ImageResource
	{
		private List<string> channelNames = new List<string>();
		public override ResourceID ID
		{
			get
			{
				return ResourceID.AlphaChannelNames;
			}
		}
		public List<string> ChannelNames
		{
			get
			{
				return this.channelNames;
			}
		}
		public AlphaChannelNames() : base(string.Empty)
		{
		}
		public AlphaChannelNames(PsdBinaryReader reader, string name, int resourceDataLength) : base(name)
		{
			long num = reader.BaseStream.Position + (long)resourceDataLength;
			while (reader.BaseStream.Position < num)
			{
				string item = reader.ReadPascalString(1);
				this.ChannelNames.Add(item);
			}
		}
		protected override void WriteData(PsdBinaryWriter writer)
		{
			foreach (string current in this.ChannelNames)
			{
				writer.WritePascalString(current, 1, 255);
			}
		}
	}
}
