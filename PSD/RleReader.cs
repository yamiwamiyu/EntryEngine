using System;
using System.IO;
namespace PhotoshopFile
{
	public class RleReader
	{
		private Stream stream;
		public RleReader(Stream stream)
		{
			this.stream = stream;
		}
		public unsafe int Read(byte[] buffer, int offset, int count)
		{
			if (!Util.CheckBufferBounds(buffer, offset, count))
			{
				throw new ArgumentOutOfRangeException();
			}
			int var_0_cp_1 = 0;
			int i = count;
			int num = offset;
            fixed (byte* buf = buffer)
            {
                while (i > 0)
                {
                    sbyte b = (sbyte)this.stream.ReadByte();
                    if (b > 0)
                    {
                        int num2 = (int)(b + 1);
                        if (i < num2)
                        {
                            throw new RleException("Raw packet overruns the decode window.");
                        }
                        this.stream.Read(buffer, num, num2);
                        num += num2;
                        i -= num2;
                    }
                    else
                    {
                        if (b > -128)
                        {
                            int num3 = (int)(1 - b);
                            byte b2 = (byte)this.stream.ReadByte();
                            if (num3 > i)
                            {
                                throw new RleException("RLE packet overruns the decode window.");
                            }
                            byte* ptr = &buf[var_0_cp_1] + num / 1;
                            byte* ptr2 = ptr + num3 / 1;
                            while (ptr < ptr2)
                            {
                                *ptr = b2;
                                ptr += 1;
                            }
                            num += num3;
                            i -= num3;
                        }
                    }
                }
            }
			return count - i;
		}
	}
}
