using System;
namespace PhotoshopFile
{
	public static class LayerInfoFactory
	{
		public static LayerInfo Load(PsdBinaryReader reader)
		{
			string a = reader.ReadAsciiChars(4);
			if (a != "8BIM")
			{
				throw new PsdInvalidException("Could not read LayerInfo due to signature mismatch.");
			}
			string text = reader.ReadAsciiChars(4);
			int num = reader.ReadInt32();
			long position = reader.BaseStream.Position;
			string a2;
			LayerInfo result;
			if ((a2 = text) != null)
			{
				bool flag = true;
				switch (a2)
				{
					case "lsct":
					case "lsdk":
						result = new LayerSectionInfo(reader, text, num);
						break;

					case "luni":
						result = new LayerUnicodeName(reader);
						break;

					case "TySh":
						result = new LayerText(reader, num);
						if (result.Key == null)
						{
							flag = false;
						}
						break;

					default:
						result = null;
						flag = false;
						break;
				}
				if (flag)
				{
					goto IL_8D;
				}
			}
			result = new RawLayerInfo(reader, text, num);
			IL_8D:
			long num2 = position + (long)num;
			if (reader.BaseStream.Position < num2)
			{
				reader.BaseStream.Position = num2;
			}
			reader.ReadPadding(position, 4);
			return result;
		}
	}
}
