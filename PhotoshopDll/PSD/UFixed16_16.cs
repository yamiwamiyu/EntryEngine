using System;
namespace PhotoshopFile
{
	public class UFixed16_16
	{
		public ushort Integer
		{
			get;
			set;
		}
		public ushort Fraction
		{
			get;
			set;
		}
		public UFixed16_16(ushort integer, ushort fraction)
		{
			this.Integer = integer;
			this.Fraction = fraction;
		}
		public UFixed16_16(uint value)
		{
			this.Integer = (ushort)(value >> 16);
			this.Fraction = (ushort)(value & 65535u);
		}
		public UFixed16_16(double value)
		{
			if (value >= 65536.0)
			{
				throw new OverflowException();
			}
			if (value < 0.0)
			{
				throw new OverflowException();
			}
			this.Integer = (ushort)value;
			this.Fraction = (ushort)((value - (double)this.Integer) * 65536.0 + 0.5);
		}
		public static implicit operator double(UFixed16_16 value)
		{
			return (double)value.Integer + (double)value.Fraction / 65536.0;
		}
	}
}
