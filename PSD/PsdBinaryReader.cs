using System;
using System.Drawing;
using System.IO;
using System.Text;
namespace PhotoshopFile
{
	public class PsdBinaryReader : IDisposable
	{
		private BinaryReader reader;
		private Encoding encoding;
		private bool disposed;
		public Stream BaseStream
		{
			get
			{
				return this.reader.BaseStream;
			}
		}
		public PsdBinaryReader(Stream stream, PsdBinaryReader reader) : this(stream, reader.encoding)
		{
		}
		public PsdBinaryReader(Stream stream, Encoding encoding)
		{
			this.encoding = encoding;
			this.reader = new BinaryReader(stream, Encoding.ASCII);
		}
		public byte ReadByte()
		{
			return this.reader.ReadByte();
		}
		public byte[] ReadBytes(int count)
		{
			return this.reader.ReadBytes(count);
		}
		public bool ReadBoolean()
		{
			return this.reader.ReadBoolean();
		}
		public unsafe short ReadInt16()
		{
			short result = this.reader.ReadInt16();
			Util.SwapBytes((byte*)(&result), 2);
			return result;
		}
		public unsafe int ReadInt32()
		{
			int result = this.reader.ReadInt32();
			Util.SwapBytes((byte*)(&result), 4);
			return result;
		}
		public unsafe long ReadInt64()
		{
			long result = this.reader.ReadInt64();
			Util.SwapBytes((byte*)(&result), 8);
			return result;
		}
		public unsafe ushort ReadUInt16()
		{
			ushort result = this.reader.ReadUInt16();
			Util.SwapBytes((byte*)(&result), 2);
			return result;
		}
		public unsafe uint ReadUInt32()
		{
			uint result = this.reader.ReadUInt32();
			Util.SwapBytes((byte*)(&result), 4);
			return result;
		}
		public unsafe ulong ReadUInt64()
		{
			ulong result = this.reader.ReadUInt64();
			Util.SwapBytes((byte*)(&result), 8);
			return result;
		}
		public unsafe float ReadFloat()
		{
			float result = this.reader.ReadSingle();
			Util.SwapBytes((byte*)(&result), 4);
			return result;
		}
		public unsafe double ReadDouble()
		{
			double result = this.reader.ReadSingle();
			Util.SwapBytes((byte*)(&result), 8);
			return result;
		}
		public void ReadPadding(long startPosition, int padMultiple)
		{
			long num = this.reader.BaseStream.Position - startPosition;
			int padding = Util.GetPadding((int)num, padMultiple);
			this.ReadBytes(padding);
		}
		public Rectangle ReadRectangle()
		{
			Rectangle result = default(Rectangle);
			result.Y = this.ReadInt32();
			result.X = this.ReadInt32();
			result.Height = this.ReadInt32() - result.Y;
			result.Width = this.ReadInt32() - result.X;
			return result;
		}
		public string ReadAsciiChars(int count)
		{
			byte[] bytes = this.reader.ReadBytes(count);
			return Encoding.ASCII.GetString(bytes);
		}
		public string ReadPascalString(int padMultiple)
		{
			long position = this.reader.BaseStream.Position;
			byte count = this.ReadByte();
			byte[] bytes = this.ReadBytes((int)count);
			this.ReadPadding(position, padMultiple);
			return this.encoding.GetString(bytes);
		}
		public string ReadUnicodeString()
		{
			int num = this.ReadInt32();
			int count = 2 * num;
			byte[] bytes = this.ReadBytes(count);
			return Encoding.BigEndianUnicode.GetString(bytes, 0, count);
		}
		public string ReadClassID()
		{
			string classID;
			int length = reader.ReadInt32();
			//if (length == 0)
			//{
			//    classID = ReadAsciiChars(4);
			//}
			//else
			//{
			//    //classID = ReadUnicodeString();
			//    classID = ReadPascalString(length);
			//}
			classID = ReadAsciiChars(4);
			return classID;
		}
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}
			if (disposing && this.reader != null)
			{
				this.reader.Close();
				this.reader = null;
			}
			this.disposed = true;
		}
	}
}
