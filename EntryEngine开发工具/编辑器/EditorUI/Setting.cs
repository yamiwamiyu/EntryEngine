using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using EntryEngine.Serialize;
using System.Reflection;
using EntryEngine.UI;

namespace EditorUI
{
    class Project
    {
        public string Name;
        public Document Document;
        public string TranslateTable;
        public string ContentDirectory;

        public bool HasTranslateTable
        {
            get { return !string.IsNullOrEmpty(TranslateTable); }
        }
    }
    class Document
    {
        public static event Action<float> ScaleChanged;

        private float scale = 1;
        public string File;

        public float Scale
        {
            get { return scale; }
            set
            {
                if (scale == 0)
                    throw new ArgumentException("scale can't be 0.");
                if (scale != value)
                {
                    scale = value;
                    ScaleChanged(value);
                }
            }
        }
		public MATRIX2x3 ScaleMatrix
        {
            get { return MATRIX2x3.CreateScale(Scale, Scale); }
        }

        public Document()
        {
        }
        public Document(string file)
        {
            this.File = file;
        }

        public float Expand(int value)
        {
            if (value == 0)
                return Scale;

            int last = C.Scale.Length - 1;
            float index = last + 0.5f;
            for (int i = 0; i <= last; i++)
            {
                if (Scale == C.Scale[i])
                {
                    index = i;
                    break;
                }
                else if (Scale < C.Scale[i])
                {
                    index = i - 0.5f;
                    break;
                }
            }

            int result;
            if (value > 0)
                result = _MATH.Min((int)index + value, last);
            else
                result = _MATH.Max(0, _MATH.Ceiling(index) + value);

            Scale = C.Scale[result];
            return Scale;
        }
    }
    static class C
    {
        public static string TypeEditorGenerator;
        public static float[] Scale = { 0.25f, 0.5f, 0.625f, 0.75f, 1.0f, 1.25f, 1.5f, 1.66f, 2.0f, 4.0f, };
        public static COLOR ViewportBGColor = new COLOR(224, 224, 224, 255);
        public static COLOR ViewportColor = COLOR.White;
        public static COLOR PreSelectedColor = new COLOR(22, 22, 233, 64);
        public static COLOR PreViewColor = new COLOR(255, 255, 255, 128);
        public static COLOR BorderColor = COLOR.Black;
        public static float LineBold = 1;
        public static COLOR ColorBorder = COLOR.Black;
        public static COLOR ColorFocusBorder = COLOR.Lime;
        public static COLOR ColorFocusBody = new COLOR(222, 222, 222, 64);
        public static COLOR ColorAlignLine = COLOR.Pink;
        public static float NearDistance = 10;
        public static float ScaleAnchorRadius = 5;
        public static string EntryBuilder;
    }
}
