using System;
using System.Collections.Generic;
namespace PhotoshopFile
{
	public enum EBlendModeKey
	{
		PassThrough,
		Normal,
		Dissolve,
		Darken,
		Multiply,
		ColorBurn,
		LinearBurn,
		DarkerColor,
		Lighten,
		Screen,
		ColorDodge,
		LinearDodge,
		LighterColor,
		Overlay,
		SoftLight,
		HardLight,
		VividLight,
		LinearLight,
		PinLight,
		HardMix,
		Difference,
		Exclusion,
		Subtract,
		Divide,
		Hue,
		Saturation,
		Color,
		Luminosity,
	}
	public static class BlendModeKeyTable
	{
		private static Dictionary<EBlendModeKey, string> blendModeKeyTable;
		private static Dictionary<string, EBlendModeKey> blendModeKeyTable2;
		static BlendModeKeyTable()
		{
			blendModeKeyTable = new Dictionary<EBlendModeKey, string>();
			blendModeKeyTable2 = new Dictionary<string, EBlendModeKey>();

			blendModeKeyTable[EBlendModeKey.PassThrough] = "pass";
			blendModeKeyTable[EBlendModeKey.Normal] = "norm";
			blendModeKeyTable[EBlendModeKey.Dissolve] = "diss";
			blendModeKeyTable[EBlendModeKey.Darken] = "dark";
			blendModeKeyTable[EBlendModeKey.Multiply] = "mul ";
			blendModeKeyTable[EBlendModeKey.ColorBurn] = "idiv";
			blendModeKeyTable[EBlendModeKey.LinearBurn] = "lbrn";
			blendModeKeyTable[EBlendModeKey.DarkerColor] = "dkCl";
			blendModeKeyTable[EBlendModeKey.Lighten] = "lite";
			blendModeKeyTable[EBlendModeKey.Screen] = "scrn";
			blendModeKeyTable[EBlendModeKey.ColorDodge] = "div ";
			blendModeKeyTable[EBlendModeKey.LinearDodge] = "lddg";
			blendModeKeyTable[EBlendModeKey.LighterColor] = "lgCl";
			blendModeKeyTable[EBlendModeKey.Overlay] = "over";
			blendModeKeyTable[EBlendModeKey.SoftLight] = "sLit";
			blendModeKeyTable[EBlendModeKey.HardLight] = "hLit";
			blendModeKeyTable[EBlendModeKey.VividLight] = "vLit";
			blendModeKeyTable[EBlendModeKey.LinearLight] = "lLit";
			blendModeKeyTable[EBlendModeKey.PinLight] = "pLit";
			blendModeKeyTable[EBlendModeKey.HardMix] = "hMix";
			blendModeKeyTable[EBlendModeKey.Difference] = "diff";
			blendModeKeyTable[EBlendModeKey.Exclusion] = "smud";
			blendModeKeyTable[EBlendModeKey.Subtract] = "fsub";
			blendModeKeyTable[EBlendModeKey.Divide] = "fdiv";
			blendModeKeyTable[EBlendModeKey.Hue] = "hue ";
			blendModeKeyTable[EBlendModeKey.Saturation] = "sat ";
			blendModeKeyTable[EBlendModeKey.Color] = "colr";
			blendModeKeyTable[EBlendModeKey.Luminosity] = "lum ";

			foreach (var item in blendModeKeyTable)
			{
				blendModeKeyTable2[item.Value] = item.Key;
			}
		}
		public static EBlendModeKey GetBlendModeKey(string value)
		{
			return blendModeKeyTable2[value];
		}
		public static string GetBlendModeKey(EBlendModeKey value)
		{
			return blendModeKeyTable[value];
		}
	}

	public class LayerSectionInfo : LayerInfo
	{
		private string key;
		private LayerSectionSubtype? subtype;
		private string blendModeKey;
		public override string Key
		{
			get
			{
				return this.key;
			}
		}
		public LayerSectionType SectionType
		{
			get;
			set;
		}
		public LayerSectionSubtype Subtype
		{
			get
			{
				LayerSectionSubtype? layerSectionSubtype = this.subtype;
				if (!layerSectionSubtype.HasValue)
				{
					return LayerSectionSubtype.Normal;
				}
				return layerSectionSubtype.GetValueOrDefault();
			}
			set
			{
				this.subtype = new LayerSectionSubtype?(value);
			}
		}
		public string BlendModeKey
		{
			get
			{
				return this.blendModeKey;
			}
			set
			{
				if (value.Length != 4)
				{
					throw new ArgumentException("Blend mode key must have a length of 4.");
				}
				this.blendModeKey = value;
			}
		}
		public EBlendModeKey BlendMode
		{
			get
			{
				return BlendModeKeyTable.GetBlendModeKey(blendModeKey);
			}
		}
		public LayerSectionInfo(PsdBinaryReader reader, string key, int dataLength)
		{
			this.key = key;
			this.SectionType = (LayerSectionType)reader.ReadInt32();
			if (dataLength >= 12)
			{
				string a = reader.ReadAsciiChars(4);
				if (a != "8BIM")
				{
					throw new PsdInvalidException("Invalid section divider signature.");
				}
				this.BlendModeKey = reader.ReadAsciiChars(4);
				if (dataLength >= 16)
				{
					this.Subtype = (LayerSectionSubtype)reader.ReadInt32();
				}
			}
		}
		protected override void WriteData(PsdBinaryWriter writer)
		{
			writer.Write((int)this.SectionType);
			if (this.BlendModeKey != null)
			{
				writer.WriteAsciiChars("8BIM");
				writer.WriteAsciiChars(this.BlendModeKey);
				if (this.subtype.HasValue)
				{
					writer.Write((int)this.Subtype);
				}
			}
		}
	}
}
