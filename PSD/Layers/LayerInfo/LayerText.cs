using System;
using System.Collections.Generic;
using PSDFile.Text;
using System.Linq;
using System.Drawing;

namespace PSDFile
{
	public class LayerText : LayerInfo
	{
		public override string Key
		{
			get { return "TySh"; }
		}

		public byte[] Data { get; private set; }

		public Matrix2D Transform;
		public DynVal TxtDescriptor;
		public TdTaStylesheetReader StylesheetReader;
		public Dictionary<string, object> engineData;
		public Boolean isTextHorizontal
		{
			get
			{
                if (TxtDescriptor == null || TxtDescriptor.Children == null)
                    return false;

                var text = TxtDescriptor.Children.FirstOrDefault(c => c.Name.ToLower() == "orientation");
                if (text == null || text.Value == null)
                    return false;

                return text.Value.ToString().ToLower() == "orientation.horizontal";
			}
		}

		public string Text
		{
			get;
			private set;
		}

		public double FontSize
		{
			get;
			private set;
		}
        public double FontSizePx
        {
            get { return FontSize / 72 * 96; }
        }

		public string FontName
		{
			get;
			private set;
		}

		public bool FauxBold
		{
			get;
			private set;
		}

		public bool FauxItalic
		{
			get;
			private set;
		}

		public bool Underline
		{
			get;
			private set;
		}

		public Color FillColor
		{
			get;
			private set;
		}

        /// <summary>文字段落排序，0居左 / 1居右 / 2居中</summary>
        public int Alignment { get; private set; }

		public LayerText()
		{

		}

		public LayerText(PsdBinaryReader psdReader, int dataLength)
		{
			Data = psdReader.ReadBytes((int)dataLength);
			var reader = new PsdBinaryReader(new System.IO.MemoryStream(Data), psdReader);

			// PhotoShop version
			reader.ReadUInt16();

			Transform = new Matrix2D(reader);

			// TextVersion
			reader.ReadUInt16(); //2 bytes, =50. For Photoshop 6.0.

			// DescriptorVersion
			reader.ReadUInt32(); //4 bytes,=16. For Photoshop 6.0.

			TxtDescriptor = DynVal.ReadDescriptor(reader); //Text descriptor

			// WarpVersion
			reader.ReadUInt16(); //2 bytes, =1. For Photoshop 6.0.

			engineData = (Dictionary<string, object>)TxtDescriptor.Children.Find(c => c.Name == "EngineData").Value;
			StylesheetReader = new TdTaStylesheetReader(engineData);

            object value;
			Dictionary<string, object> d = StylesheetReader.GetStylesheetDataFromLongestRun();
            // 文字属性
			Text = StylesheetReader.Text.Trim();
			FontName = TdTaParser.getString(StylesheetReader.getFontSet()[(int)TdTaParser.query(d, "Font")], "Name$");
			FontSize = (double)TdTaParser.query(d, "FontSize");
            if (d.TryGetValue("FauxBold", out value))
                FauxBold = (bool)value;
            if (d.TryGetValue("FauxItalic", out value))
                FauxItalic = (bool)value;
            if (d.TryGetValue("Underline", out value))
                Underline = (bool)value;
            if (d.TryGetValue("FillColor", out value))
			    FillColor = TdTaParser.getColor(d, "FillColor");

            // 段落属性
            d = StylesheetReader.GetParagraphDataFromLongestRun();
            if (d.TryGetValue("Justification", out value))
                Alignment = (int)value;
		}

		protected override void WriteData(PsdBinaryWriter writer)
		{
			writer.Write(Data);
		}
    }
}