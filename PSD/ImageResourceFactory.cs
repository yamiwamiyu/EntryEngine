using System;
namespace PhotoshopFile
{
	public static class ImageResourceFactory
	{
		public static ImageResource CreateImageResource(PsdBinaryReader reader)
		{
			string signature = reader.ReadAsciiChars(4);	// 8BIM
			ushort num = reader.ReadUInt16();
			string name = reader.ReadPascalString(2);
			int num2 = (int)reader.ReadUInt32();
			int num3 = Util.RoundUp(num2, 2);
			long num4 = reader.BaseStream.Position + (long)num3;
			ResourceID resourceID = (ResourceID)num;
			ResourceID resourceID2 = resourceID;
			ImageResource result;
			if (resourceID2 <= ResourceID.ThumbnailBgr)
			{
				switch (resourceID2)
				{
				case ResourceID.ResolutionInfo:
					result = new ResolutionInfo(reader, name);
					goto IL_D1;
				case ResourceID.AlphaChannelNames:
					result = new AlphaChannelNames(reader, name, num2);
					goto IL_D1;
				default:
					if (resourceID2 != ResourceID.ThumbnailBgr)
					{
						goto IL_C4;
					}
					break;
				}
			}
			else
			{
				if (resourceID2 != ResourceID.ThumbnailRgb)
				{
					if (resourceID2 == ResourceID.UnicodeAlphaNames)
					{
						result = new UnicodeAlphaNames(reader, name, num2);
						goto IL_D1;
					}
					if (resourceID2 != ResourceID.VersionInfo)
					{
						goto IL_C4;
					}
					result = new VersionInfo(reader, name);
					goto IL_D1;
				}
			}
			result = new Thumbnail(reader, resourceID, name, num2);
			goto IL_D1;
			IL_C4:
			result = new RawImageResource(reader, signature, resourceID, name, num2);
			IL_D1:
			if (reader.BaseStream.Position < num4)
			{
				reader.BaseStream.Position = num4;
			}
			if (reader.BaseStream.Position > num4)
			{
				throw new PsdInvalidException("Corruption detected in resource.");
			}
			return result;
		}
	}
}
