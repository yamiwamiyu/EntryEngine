using System;
using System.Collections;
using System.Linq;

namespace Ntreev.Library.Psd
{
    public struct PsdColor
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;
    }
    public struct PsdRect
    {
        public double Left;
        public double Top;
        public double Right;
        public double Bottom;
        public double Width { get { return Right - Left; } }
        public double Height { get { return Bottom - Top; } }
    }
    public struct PsdPoint
    {
        public int X;
        public int Y;

        public override string ToString()
        {
            return string.Format("X:{0}, Y:{1}", X, Y);
        }

        public static void Distance(ref PsdPoint p1, ref PsdPoint p2, out double distance)
        {
            int y = p2.Y - p1.Y;
            int x = p2.X - p1.X;
            distance = Math.Sqrt(y * y - x * x);
        }
    }

    /// <summary>文字图层信息</summary>
    public class ResourceText
    {
        public enum EJustification
        {
            Left,
            Right,
            Center,
        }
        public class TextPortion
        {
            public string Text;
            public float FontSize;
            /// <summary>向下偏移的像素值</summary>
            public float BaselineShift;
            public bool FauxBold;
            public bool FauxItalic;
            public bool Underline;
            public PsdColor FillColor;

            public override string ToString()
            {
                return Text;
            }
        }

        public double[] Transforms;
        public bool IsScale { get { return Transforms[0] == Transforms[3]; } }
        public double Scale { get { return Transforms[0]; } }

        public string Text;

        public PsdRect Bounds;
        public PsdRect BoundingBox;

        /// <summary>暂不支持使用多个段落对齐方式</summary>
        public EJustification Justification;
        public TextPortion[] Portion;

        public static bool Is(IPsdLayer layer)
        {
            return layer.Resources.Any(r => r.Key == "TySh");
        }
        public static ResourceText Create(IPsdLayer layer)
        {
            var resource = (IProperties)layer.Resources["TySh"];

            ResourceText result = new ResourceText();
            result.Transforms = resource.Value<double[]>("Transforms");
            result.Text = resource.Value<string>("Text.Txt");

            // todo: 单位是点，而不是像素，应该转换成像素
            result.Bounds.Left = resource.Value<double>("Text.bounds.Left.Value");
            result.Bounds.Top = resource.Value<double>("Text.bounds.Top.Value");
            result.Bounds.Right = resource.Value<double>("Text.bounds.Rght.Value");
            result.Bounds.Bottom = resource.Value<double>("Text.bounds.Btom.Value");

            result.BoundingBox.Left = resource.Value<double>("Text.boundingBox.Left.Value");
            result.BoundingBox.Top = resource.Value<double>("Text.boundingBox.Top.Value");
            result.BoundingBox.Right = resource.Value<double>("Text.boundingBox.Rght.Value");
            result.BoundingBox.Bottom = resource.Value<double>("Text.boundingBox.Btom.Value");

            var engineData = resource.ToValue<IProperties>("Text", "EngineData", "EngineDict");
            result.Justification = (EJustification)engineData.Value<int>("ParagraphRun.RunArray[0].ParagraphSheet.Properties.Justification");

            {
                int start = 0;
                var temp = engineData.Value<System.Collections.ArrayList>("StyleRun.RunLengthArray");
                var styles = engineData.Value<System.Collections.ArrayList>("StyleRun.RunArray");
                result.Portion = new TextPortion[temp.Count];
                for (int i = 0; i < temp.Count; i++)
                {
                    var portion = new TextPortion();
                    int length = (int)temp[i];
                    // 最后一般有个\r
                    if (start + length > result.Text.Length)
                        length--;
                    portion.Text = result.Text.Substring(start, length);
                    start += length;
                    var style = ((IProperties)styles[i]).Value<IProperties>("StyleSheet.StyleSheetData");
                    portion.FontSize = style.Value<float>("FontSize");
                    portion.BaselineShift = style.Value<float>("BaselineShift");
                    portion.FauxBold = style.Value<bool>("FauxBold");
                    portion.FauxItalic = style.Value<bool>("FauxItalic");
                    portion.Underline = style.Value<bool>("Underline");
                    var color = style.Value<System.Collections.ArrayList>("FillColor.Values");
                    portion.FillColor.R = (byte)((float)color[0] * 255);
                    portion.FillColor.G = (byte)((float)color[1] * 255);
                    portion.FillColor.B = (byte)((float)color[2] * 255);
                    portion.FillColor.A = (byte)((float)color[3] * 255);
                    result.Portion[i] = portion;
                }
            }

            return result;
        }
    }
    /// <summary>目前主要是实现圆角，内容分别是4个圆角的像素值</summary>
    public class ResourceVectorPath
    {
        public double LT;
        public double RT;
        public double LB;
        public double RB;

        public static bool Is(IPsdLayer layer)
        {
            return layer.Resources.Any(r => r.Key == "vsms" || r.Key == "vmsk");
        }
        public static ResourceVectorPath Create(IPsdLayer layer)
        {
            var resource = layer.Resources.Value<IProperties>("vsms", "vmsk");

            ResourceVectorPath result = new ResourceVectorPath();
            var array = resource.Value<ArrayList>("Path");
            PsdPoint[][] points = new PsdPoint[array.Count][];
            for (int i = 0; i < points.Length; i++)
            {
                PsdPoint[] p3 = new PsdPoint[3];
                ArrayList pointList = (ArrayList)((ArrayList)array)[i];
                for (int j = 0; j < p3.Length; j++)
                {
                    p3[j].X = (int)(pointList[j << 1]);
                    p3[j].Y = (int)(pointList[(j << 1) + 1]);
                }
                points[i] = p3;
            }

            // 左上角左棱上面为第一个顶点，逆时针的8个点坐标
            PsdPoint[] corner = new PsdPoint[8];
            if (corner.Length == 8)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    if ((i & 1) == 0)
                    {
                        corner[i].X = points[i][0].X;
                        corner[i].Y = points[i][0].Y;
                    }
                    else
                    {
                        corner[i].X = points[i][2].X;
                        corner[i].Y = points[i][2].Y;
                    }
                }
            }
            else if (corner.Length == 6)
            {
                // 直接呈现椭圆的情况下，椭圆的边第一和最后一个点分别用做两个点
                for (int i = 0; i < points.Length; i++)
                {
                    if (i % 3 == 2)
                    {
                        corner[i].X = points[i][0].X;
                        corner[i].Y = points[i][0].Y;
                        i++;
                        corner[i].X = points[i][2].X;
                        corner[i].Y = points[i][2].Y;
                    }
                    else if (i % 3 == 1)
                    {
                        corner[i].X = points[i][2].X;
                        corner[i].Y = points[i][2].Y;
                    }
                    else
                    {
                        corner[i].X = points[i][0].X;
                        corner[i].Y = points[i][0].Y;
                    }
                }
            }
            else
            {
                for (int i = 0; i < points.Length; i++)
                {
                    corner[i].X = points[i][0].X;
                    corner[i].Y = points[i][0].Y;
                    i++;
                    corner[i].X = points[i][2].X;
                    corner[i].Y = points[i][2].Y;
                }
            }
            int shortLength = layer.Document.Width < layer.Document.Height ? layer.Document.Width : layer.Document.Height;
            shortLength <<= 1;
            // 四个角的圆角像素值
            PsdPoint.Distance(ref corner[0], ref corner[7], out result.LT);
            PsdPoint.Distance(ref corner[1], ref corner[2], out result.RT);
            PsdPoint.Distance(ref corner[3], ref corner[4], out result.RB);
            PsdPoint.Distance(ref corner[5], ref corner[6], out result.LB);

            return result;
        }
    }
}
