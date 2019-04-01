using System;
namespace PhotoshopFile
{
	public class ResolutionInfo : ImageResource
	{
		public enum ResUnit
		{
			PxPerInch = 1,
			PxPerCm
		}
		public enum Unit
		{
			Inches = 1,
			Centimeters,
			Points,
			Picas,
			Columns
		}
		public override ResourceID ID
		{
			get
			{
				return ResourceID.ResolutionInfo;
			}
		}
		public UFixed16_16 HDpi
		{
			get;
			set;
		}
		public UFixed16_16 VDpi
		{
			get;
			set;
		}
		public ResolutionInfo.ResUnit HResDisplayUnit
		{
			get;
			set;
		}
		public ResolutionInfo.ResUnit VResDisplayUnit
		{
			get;
			set;
		}
		public ResolutionInfo.Unit WidthDisplayUnit
		{
			get;
			set;
		}
		public ResolutionInfo.Unit HeightDisplayUnit
		{
			get;
			set;
		}
		public ResolutionInfo() : base(string.Empty)
		{
		}
		public ResolutionInfo(PsdBinaryReader reader, string name) : base(name)
		{
			this.HDpi = new UFixed16_16(reader.ReadUInt32());
			this.HResDisplayUnit = (ResolutionInfo.ResUnit)reader.ReadInt16();
			this.WidthDisplayUnit = (ResolutionInfo.Unit)reader.ReadInt16();
			this.VDpi = new UFixed16_16(reader.ReadUInt32());
			this.VResDisplayUnit = (ResolutionInfo.ResUnit)reader.ReadInt16();
			this.HeightDisplayUnit = (ResolutionInfo.Unit)reader.ReadInt16();
		}
		protected override void WriteData(PsdBinaryWriter writer)
		{
			writer.Write(this.HDpi.Integer);
			writer.Write(this.HDpi.Fraction);
			writer.Write((short)this.HResDisplayUnit);
			writer.Write((short)this.WidthDisplayUnit);
			writer.Write(this.VDpi.Integer);
			writer.Write(this.VDpi.Fraction);
			writer.Write((short)this.VResDisplayUnit);
			writer.Write((short)this.HeightDisplayUnit);
		}
	}
}
