using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;
namespace PhotoshopFile
{
	[DebuggerDisplay("ID = {ID}")]
	public class Channel
	{
		private byte[] data;
		private bool dataDecompressed;
		private byte[] imageData;
		private bool imageDataCompressed;
		public Layer Layer
		{
			get;
			private set;
		}
		public short ID
		{
			get;
			set;
		}
		public Rectangle Rect
		{
			get
			{
				switch (this.ID)
				{
				case -3:
					return this.Layer.Masks.UserMask.Rect;
				case -2:
					return this.Layer.Masks.LayerMask.Rect;
				default:
					return this.Layer.Rect;
				}
			}
		}
		public int Length
		{
			get;
			set;
		}
		public byte[] Data
		{
			get
			{
				return this.data;
			}
			set
			{
				this.data = value;
				this.dataDecompressed = false;
				this.imageData = null;
				this.imageDataCompressed = true;
			}
		}
		public byte[] ImageData
		{
			get
			{
				return this.imageData;
			}
			set
			{
				this.imageData = value;
				this.imageDataCompressed = false;
				this.data = null;
				this.dataDecompressed = true;
			}
		}
		public ImageCompression ImageCompression
		{
			get;
			set;
		}
		public byte[] RleHeader
		{
			get;
			set;
		}
		internal Channel(short id, Layer layer)
		{
			this.ID = id;
			this.Layer = layer;
		}
		internal Channel(PsdBinaryReader reader, Layer layer)
		{
			this.ID = reader.ReadInt16();
			this.Length = reader.ReadInt32();
			this.Layer = layer;
		}
		internal void Save(PsdBinaryWriter writer)
		{
			writer.Write(this.ID);
			writer.Write(this.Length);
		}
		internal void LoadPixelData(PsdBinaryReader reader)
		{
			long arg_0B_0 = reader.BaseStream.Position;
			int arg_12_0 = this.Length;
			this.ImageCompression = (ImageCompression)reader.ReadInt16();
			this.imageDataCompressed = true;
			int num = this.Length - 2;
			switch (this.ImageCompression)
			{
			case ImageCompression.Raw:
				this.ImageData = reader.ReadBytes(num);
				return;
			case ImageCompression.Rle:
			{
				this.RleHeader = reader.ReadBytes(2 * this.Rect.Height);
				int count = num - 2 * this.Rect.Height;
				this.Data = reader.ReadBytes(count);
				return;
			}
			case ImageCompression.Zip:
			case ImageCompression.ZipPrediction:
				this.Data = reader.ReadBytes(num);
				return;
			default:
				return;
			}
		}
		public void DecompressImageData()
		{
			if (this.dataDecompressed)
			{
				return;
			}
			if (this.ImageCompression == ImageCompression.Raw)
			{
				this.imageData = this.data;
			}
			else
			{
				int num = Util.BytesPerRow(this.Rect, this.Layer.PsdFile.BitDepth);
				int num2 = this.Rect.Height * num;
				this.imageData = new byte[num2];
				MemoryStream memoryStream = new MemoryStream(this.Data);
				switch (this.ImageCompression)
				{
				case ImageCompression.Rle:
				{
					RleReader rleReader = new RleReader(memoryStream);
					for (int i = 0; i < this.Rect.Height; i++)
					{
						int offset = i * num;
						rleReader.Read(this.imageData, offset, num);
					}
					break;
				}
				case ImageCompression.Zip:
				case ImageCompression.ZipPrediction:
				{
					memoryStream.ReadByte();
					memoryStream.ReadByte();
					DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
					deflateStream.Read(this.imageData, 0, num2);
					break;
				}
				default:
					throw new PsdInvalidException("Unknown image compression method.");
				}
			}
			if (this.ImageCompression == ImageCompression.ZipPrediction)
			{
				this.UnpredictImageData(this.Rect);
			}
			else
			{
				this.ReverseEndianness(this.imageData, this.Rect);
			}
			this.dataDecompressed = true;
		}
		private void ReverseEndianness(byte[] buffer, Rectangle rect)
		{
			int num = Util.BytesFromBitDepth(this.Layer.PsdFile.BitDepth);
			int num2 = rect.Width * rect.Height;
			if (num2 == 0)
			{
				return;
			}
			if (num == 2)
			{
				Util.SwapByteArray2(buffer, 0, num2);
				return;
			}
			if (num == 4)
			{
				Util.SwapByteArray4(buffer, 0, num2);
				return;
			}
			if (num > 1)
			{
				throw new NotImplementedException("Byte-swapping implemented only for 16-bit and 32-bit depths.");
			}
		}
		private unsafe void UnpredictImageData(Rectangle rect)
		{
			if (this.Layer.PsdFile.BitDepth == 16)
			{
				this.ReverseEndianness(this.imageData, rect);
				fixed (byte* ptr = &this.imageData[0])
				{
					for (int i = 0; i < rect.Height; i++)
					{
						ushort* ptr2 = (ushort*)ptr + (i * rect.Width);
						ushort* ptr3 = (ushort*)ptr + ((i + 1) * rect.Width);
						for (ptr2 += 2 / 2; ptr2 < ptr3; ptr2 += 2 / 2)
						{
							*ptr2 += *(ptr2 - 2 / 2);
						}
					}
				}
				return;
			}
			if (this.Layer.PsdFile.BitDepth == 32)
			{
				byte[] array = new byte[this.imageData.Length];
				fixed (byte* ptr4 = &this.imageData[0])
				{
					for (int j = 0; j < rect.Height; j++)
					{
						byte* ptr5 = ptr4 + (j * rect.Width) * 4 / 1;
						byte* ptr6 = ptr4 + ((j + 1) * rect.Width) * 4 / 1;
						for (ptr5 += 1 / 1; ptr5 < ptr6; ptr5 += 1 / 1)
						{
							*ptr5 += *(ptr5 - 1 / 1);
						}
					}
					int width = rect.Width;
					int num = 2 * width;
					int num2 = 3 * width;
					fixed (byte* ptr7 = &array[0])
					{
						for (int k = 0; k < rect.Height; k++)
						{
							byte* ptr8 = ptr7 + (k * rect.Width) * 4 / 1;
							byte* ptr9 = ptr7 + ((k + 1) * rect.Width) * 4 / 1;
							byte* ptr10 = ptr4 + (k * rect.Width) * 4 / 1;
							while (ptr8 < ptr9)
							{
								byte* expr_17D = ptr8;
								ptr8 = expr_17D + 1 / 1;
								*expr_17D = ptr10[num2 / 1];
								byte* expr_18C = ptr8;
								ptr8 = expr_18C + 1 / 1;
								*expr_18C = ptr10[num / 1];
								byte* expr_19B = ptr8;
								ptr8 = expr_19B + 1 / 1;
								*expr_19B = ptr10[width / 1];
								byte* expr_1AA = ptr8;
								ptr8 = expr_1AA + 1 / 1;
								*expr_1AA = *ptr10;
								ptr10 += 1 / 1;
							}
						}
					}
				}
				this.imageData = array;
				return;
			}
			throw new PsdInvalidException("ZIP with prediction is only available for 16 and 32 bit depths.");
		}
		public void CompressImageData()
		{
			if (this.imageDataCompressed)
			{
				return;
			}
			if (this.ImageCompression == ImageCompression.Rle)
			{
				MemoryStream memoryStream = new MemoryStream();
				MemoryStream memoryStream2 = new MemoryStream();
				RleWriter rleWriter = new RleWriter(memoryStream);
				PsdBinaryWriter psdBinaryWriter = new PsdBinaryWriter(memoryStream2, Encoding.ASCII);
				ushort[] array = new ushort[this.Layer.Rect.Height];
				int count = Util.BytesPerRow(this.Layer.Rect, this.Layer.PsdFile.BitDepth);
				for (int i = 0; i < this.Layer.Rect.Height; i++)
				{
					int offset = i * this.Layer.Rect.Width;
					array[i] = (ushort)rleWriter.Write(this.ImageData, offset, count);
				}
				for (int j = 0; j < array.Length; j++)
				{
					psdBinaryWriter.Write(array[j]);
				}
				memoryStream2.Flush();
				this.RleHeader = memoryStream2.ToArray();
				memoryStream2.Close();
				memoryStream.Flush();
				this.data = memoryStream.ToArray();
				memoryStream.Close();
				this.Length = 2 + this.RleHeader.Length + this.Data.Length;
			}
			else
			{
				this.data = this.ImageData;
				this.Length = 2 + this.Data.Length;
			}
			this.imageDataCompressed = true;
		}
		internal void SavePixelData(PsdBinaryWriter writer)
		{
			writer.Write((short)this.ImageCompression);
			if (this.Data == null)
			{
				return;
			}
			if (this.ImageCompression == ImageCompression.Rle)
			{
				writer.Write(this.RleHeader);
			}
			writer.Write(this.Data);
		}
	}
}
