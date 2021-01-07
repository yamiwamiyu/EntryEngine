using System;
using System.Drawing;
using System.IO;
using System.Text;
namespace PhotoshopFile
{
	public class PsdBinaryWriter : IDisposable
	{
		private BinaryWriter writer;
		private Encoding encoding;
		private bool disposed;
		public Stream BaseStream
		{
			get
			{
				return this.writer.BaseStream;
			}
		}
		public bool AutoFlush
		{
			get;
			set;
		}
		public PsdBinaryWriter(Stream stream, Encoding encoding)
		{
			this.encoding = encoding;
			this.writer = new BinaryWriter(stream, Encoding.ASCII);
		}
		public void Flush()
		{
			this.writer.Flush();
		}
		public void Write(Rectangle rect)
		{
			this.Write(rect.Top);
			this.Write(rect.Left);
			this.Write(rect.Bottom);
			this.Write(rect.Right);
		}
		public void WritePadding(long startPosition, int padMultiple)
		{
			long num = this.writer.BaseStream.Position - startPosition;
			int padding = Util.GetPadding((int)num, padMultiple);
			for (long num2 = 0L; num2 < (long)padding; num2 += 1L)
			{
				this.writer.Write((byte)0);
			}
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}
		public void WriteAsciiChars(string s)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(s);
			this.writer.Write(bytes);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}
		public void WritePascalString(string s, int padMultiple, byte maxBytes = 255)
		{
			long position = this.writer.BaseStream.Position;
			byte[] array = this.encoding.GetBytes(s);
			if (array.Length > (int)maxBytes)
			{
				byte[] array2 = new byte[(int)maxBytes];
				Array.Copy(array, array2, (int)maxBytes);
				array = array2;
			}
			this.writer.Write((byte)array.Length);
			this.writer.Write(array);
			this.WritePadding(position, padMultiple);
		}
		public void WriteUnicodeString(string s)
		{
			this.Write(s.Length);
			byte[] bytes = Encoding.BigEndianUnicode.GetBytes(s);
			this.Write(bytes);
		}
		public void Write(bool value)
		{
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}
		public void Write(byte[] value)
		{
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}
		public void Write(byte value)
		{
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}
		public unsafe void Write(short value)
		{
			Util.SwapBytes2((byte*)(&value));
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}
		public unsafe void Write(int value)
		{
			Util.SwapBytes4((byte*)(&value));
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}
		public unsafe void Write(long value)
		{
			Util.SwapBytes((byte*)(&value), 8);
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}
		public unsafe void Write(ushort value)
		{
			Util.SwapBytes2((byte*)(&value));
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}
		public unsafe void Write(uint value)
		{
			Util.SwapBytes4((byte*)(&value));
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}
		public unsafe void Write(ulong value)
		{
			Util.SwapBytes((byte*)(&value), 8);
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}
		public unsafe void Write(float value)
		{
			Util.SwapBytes((byte*)(&value), 4);
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}
		public unsafe void Write(double value)
		{
			Util.SwapBytes((byte*)(&value), 8);
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
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
			if (disposing && this.writer != null)
			{
				this.writer.Close();
				this.writer = null;
			}
			this.disposed = true;
		}
	}
}
