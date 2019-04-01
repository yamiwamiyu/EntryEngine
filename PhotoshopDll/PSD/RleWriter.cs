using System;
using System.IO;
using System.Threading;
namespace PhotoshopFile
{
	public class RleWriter
	{
		private int maxPacketLength = 128;
		private object rleLock;
		private Stream stream;
		private byte[] data;
		private int offset;
		private bool isRepeatPacket;
		private int idxPacketStart;
		private int packetLength;
		private byte runValue;
		private int runLength;
		public RleWriter(Stream stream)
		{
			this.rleLock = new object();
			this.stream = stream;
		}
		public unsafe int Write(byte[] data, int offset, int count)
		{
			if (!Util.CheckBufferBounds(data, offset, count))
			{
				throw new ArgumentOutOfRangeException();
			}
			if (count == 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			object obj;
			Monitor.Enter(obj = this.rleLock);
			int result;
			try
			{
				long position = this.stream.Position;
				this.data = data;
				this.offset = offset;
				fixed (byte* ptr = &data[0])
				{
					byte* ptr2 = ptr + offset / 1;
					byte* ptrEnd = ptr2 + count / 1;
					this.EncodeToStream(ptr2, ptrEnd);
				}
				result = (int)(this.stream.Position - position);
			}
			finally
			{
				Monitor.Exit(obj);
			}
			return result;
		}
		private void ClearPacket()
		{
			this.isRepeatPacket = false;
			this.packetLength = 0;
		}
		private void WriteRepeatPacket(int length)
		{
			byte value = (byte)(1 - length);
			this.stream.WriteByte(value);
			this.stream.WriteByte(this.runValue);
		}
		private void WriteLiteralPacket(int length)
		{
			byte value = (byte)(length - 1);
			this.stream.WriteByte(value);
			this.stream.Write(this.data, this.idxPacketStart, length);
		}
		private void WritePacket()
		{
			if (this.isRepeatPacket)
			{
				this.WriteRepeatPacket(this.packetLength);
				return;
			}
			this.WriteLiteralPacket(this.packetLength);
		}
		private void StartPacket(int count, bool isRepeatPacket, int runLength, byte value)
		{
			this.isRepeatPacket = isRepeatPacket;
			this.packetLength = runLength;
			this.runLength = runLength;
			this.runValue = value;
			this.idxPacketStart = this.offset + count;
		}
		private void ExtendPacketAndRun(byte value)
		{
			this.packetLength++;
			this.runLength++;
		}
		private void ExtendPacketStartNewRun(byte value)
		{
			this.packetLength++;
			this.runLength = 1;
			this.runValue = value;
		}
		private unsafe int EncodeToStream(byte* ptr, byte* ptrEnd)
		{
			this.StartPacket(0, false, 1, *ptr);
			int num = 1;
			ptr += 1 / 1;
			while (ptr < ptrEnd)
			{
				byte b = *ptr;
				if (this.packetLength == 1)
				{
					this.isRepeatPacket = (b == this.runValue);
					if (this.isRepeatPacket)
					{
						this.ExtendPacketAndRun(b);
					}
					else
					{
						this.ExtendPacketStartNewRun(b);
					}
				}
				else
				{
					if (this.packetLength == this.maxPacketLength)
					{
						this.WritePacket();
						this.StartPacket(num, false, 1, b);
					}
					else
					{
						if (this.isRepeatPacket)
						{
							if (b == this.runValue)
							{
								this.ExtendPacketAndRun(b);
							}
							else
							{
								this.WriteRepeatPacket(this.packetLength);
								this.StartPacket(num, false, 1, b);
							}
						}
						else
						{
							if (b == this.runValue)
							{
								this.ExtendPacketAndRun(b);
								if (this.runLength == 3)
								{
									this.WriteLiteralPacket(this.packetLength - 3);
									this.StartPacket(num - 2, true, 3, b);
								}
							}
							else
							{
								this.ExtendPacketStartNewRun(b);
							}
						}
					}
				}
				ptr += 1 / 1;
				num++;
			}
			this.WritePacket();
			this.ClearPacket();
			return num;
		}
	}
}
