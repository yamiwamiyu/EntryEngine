using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
namespace PhotoshopFile
{
	public class Thumbnail : RawImageResource
	{
		public Bitmap Image
		{
			get;
			private set;
		}
		public Thumbnail(ResourceID id, string name) : base(id, name)
		{
		}
		public Thumbnail(PsdBinaryReader psdReader, ResourceID id, string name, int numBytes) : base(psdReader, "8BIM", id, name, numBytes)
		{
			using (MemoryStream memoryStream = new MemoryStream(base.Data))
			{
				using (PsdBinaryReader psdBinaryReader = new PsdBinaryReader(memoryStream, psdReader))
				{
					uint num = psdBinaryReader.ReadUInt32();
					uint width = psdBinaryReader.ReadUInt32();
					uint height = psdBinaryReader.ReadUInt32();
					psdBinaryReader.ReadUInt32();
					psdBinaryReader.ReadUInt32();
					psdBinaryReader.ReadUInt32();
					psdBinaryReader.ReadUInt16();
					psdBinaryReader.ReadUInt16();
					if (num == 0u)
					{
						this.Image = new Bitmap((int)width, (int)height, PixelFormat.Format24bppRgb);
					}
					else
					{
						if (num != 1u)
						{
							throw new PsdInvalidException("Unknown thumbnail format.");
						}
						byte[] buffer = psdBinaryReader.ReadBytes(numBytes - 28);
						using (MemoryStream memoryStream2 = new MemoryStream(buffer))
						{
							Bitmap bitmap = new Bitmap(memoryStream2);
							this.Image = (Bitmap)bitmap.Clone();
						}
						if (id == ResourceID.ThumbnailBgr)
						{
						}
					}
				}
			}
		}
	}
}
