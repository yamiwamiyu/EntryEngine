using System;
using System.Diagnostics;
using System.Drawing;
namespace PhotoshopFile
{
	public static class Util
	{
		[DebuggerDisplay("Top = {Top}, Bottom = {Bottom}, Left = {Left}, Right = {Right}")]
		public struct RectanglePosition
		{
			public int Top
			{
				get;
				set;
			}
			public int Bottom
			{
				get;
				set;
			}
			public int Left
			{
				get;
				set;
			}
			public int Right
			{
				get;
				set;
			}
		}
		public unsafe static void Fill(byte* ptr, byte* ptrEnd, byte value)
		{
			while (ptr < ptrEnd)
			{
				*ptr = value;
				ptr += 1 / 1;
			}
		}
		public unsafe static void SwapBytes2(byte* ptr)
		{
			byte b = *ptr;
			*ptr = ptr[1 / 1];
			ptr[1 / 1] = b;
		}
		public unsafe static void SwapBytes4(byte* ptr)
		{
			byte b = *ptr;
			byte b2 = ptr[1 / 1];
			*ptr = ptr[3 / 1];
			ptr[1 / 1] = *(ptr + 2 / 1);
			ptr[2 / 1] = b2;
			ptr[3 / 1] = b;
		}
		public unsafe static void SwapBytes(byte* ptr, int nLength)
		{
			for (long num = 0L; num < (long)(nLength / 2); num += 1L)
			{
				byte b = ptr[num / 1];
				ptr[num / 1] = *(ptr + nLength / 1 - num / 1 - 1 / 1);
				*(ptr + nLength / 1 - num / 1 - 1 / 1) = b;
			}
		}
		public unsafe static void SwapByteArray2(byte[] byteArray, int startIdx, int count)
		{
			int num = startIdx + count * 2;
			if (byteArray.Length < num)
			{
				throw new IndexOutOfRangeException();
			}
			fixed (byte* ptr = &byteArray[0])
			{
				byte* ptr2 = ptr + startIdx / 1;
				byte* ptr3 = ptr + num / 1;
				while (ptr2 < ptr3)
				{
					Util.SwapBytes2(ptr2);
					ptr2 += 2 / 1;
				}
			}
		}
		public unsafe static void SwapByteArray4(byte[] byteArray, int startIdx, int count)
		{
			int num = startIdx + count * 4;
			if (byteArray.Length < num)
			{
				throw new IndexOutOfRangeException();
			}
			fixed (byte* ptr = &byteArray[0])
			{
				byte* ptr2 = ptr + startIdx / 1;
				byte* ptr3 = ptr + num / 1;
				while (ptr2 < ptr3)
				{
					Util.SwapBytes4(ptr2);
					ptr2 += 4 / 1;
				}
			}
		}
		public static int BytesPerRow(Rectangle rect, int depth)
		{
			if (depth == 1)
			{
				return (rect.Width + 7) / 8;
			}
			return rect.Width * Util.BytesFromBitDepth(depth);
		}
		public static int RoundUp(int value, int multiple)
		{
			if (value == 0)
			{
				return 0;
			}
			if (Math.Sign(value) != Math.Sign(multiple))
			{
				throw new ArgumentException("value and multiple cannot have opposite signs.");
			}
			int num = value % multiple;
			if (num > 0)
			{
				value += multiple - num;
			}
			return value;
		}
		public static int GetPadding(int length, int padMultiple)
		{
			if (length < 0 || padMultiple < 0)
			{
				throw new ArgumentException();
			}
			int num = length % padMultiple;
			if (num == 0)
			{
				return 0;
			}
			return padMultiple - num;
		}
		public static int BytesFromBitDepth(int depth)
		{
			if (depth <= 8)
			{
				if (depth == 1 || depth == 8)
				{
					return 1;
				}
			}
			else
			{
				if (depth == 16)
				{
					return 2;
				}
				if (depth == 32)
				{
					return 4;
				}
			}
			throw new ArgumentException("Invalid bit depth.");
		}
		public static short MinChannelCount(this PsdColorMode colorMode)
		{
			switch (colorMode)
			{
			case PsdColorMode.Bitmap:
			case PsdColorMode.Grayscale:
			case PsdColorMode.Indexed:
			case PsdColorMode.Multichannel:
			case PsdColorMode.Duotone:
				return 1;
			case PsdColorMode.RGB:
			case PsdColorMode.Lab:
				return 3;
			case PsdColorMode.CMYK:
				return 4;
			}
			throw new ArgumentException("Unknown color mode.");
		}
		public static bool CheckBufferBounds(byte[] data, int offset, int count)
		{
			return offset >= 0 && count >= 0 && offset + count <= data.Length;
		}
	}
}
