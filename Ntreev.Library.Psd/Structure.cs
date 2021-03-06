﻿using System;
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

        public static bool operator ==(PsdColor p1, PsdColor p2)
        {
            return p1.R == p2.R && p1.G == p2.G && p1.B == p2.B && p1.A == p2.A;
        }
        public static bool operator !=(PsdColor p1, PsdColor p2)
        {
            return p1.R != p2.R || p1.G != p2.G || p1.B != p2.B || p1.A != p2.A;
        }
    }
    public struct PsdRect
    {
        public double Left;
        public double Top;
        public double Right;
        public double Bottom;
        public double Width
        {
            get { return Right - Left; }
            set { Right = Left + value; }
        }
        public double Height
        {
            get { return Bottom - Top; }
            set { Bottom = Top + value; }
        }
        public double Center { get { return (Left + Right) * 0.5; } }
        public double Middle { get { return (Top + Bottom) * 0.5; } }
        public bool IsEmpty { get { return Left == 0 && Top == 0 && Right == 0 && Bottom == 0; } }

        public PsdRect(double left, double top, double right, double bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

        public bool Contains(PsdRect value)
        {
            return value.Left >= this.Left && value.Right <= this.Right && value.Top >= this.Top && value.Bottom <= this.Bottom;
        }
        public bool Intersects(PsdRect value)
        {
            return value.Left < this.Right && this.Left < value.Right && value.Top < this.Bottom && this.Top < value.Bottom;
        }
        public void Union(PsdRect value)
        {
            if (value.Left < this.Left) this.Left = value.Left;
            if (value.Top < this.Top) this.Top = value.Top;
            if (value.Right > this.Right) this.Right = value.Right;
            if (value.Bottom > this.Bottom) this.Bottom = value.Bottom;
        }
        public override string ToString()
        {
            return string.Format("L: {0} T: {1} R: {2} B: {3}", Left, Top, Right, Bottom);
        }
    }
    public struct PsdPoint
    {
        public double X;
        public double Y;

        public override string ToString()
        {
            return string.Format("X:{0}, Y:{1}", X, Y);
        }

        public static void Distance(ref PsdPoint p1, ref PsdPoint p2, out double distance)
        {
            var y = p2.Y - p1.Y;
            var x = p2.X - p1.X;
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
            public double FontSize;
            /// <summary>向上偏移的像素值，暂时不使用</summary>
            public double BaselineShift;
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
        protected double Scale { get { return Transforms[0]; } }

        public string Text;

        public PsdRect Bounds;
        //public PsdRect BoundingBox;

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

            double scale = result.Scale;

            result.Bounds.Left = resource.Value<double>("Text.bounds.Left.Value") * scale + result.Transforms[4];
            result.Bounds.Top = resource.Value<double>("Text.bounds.Top.Value") * scale + result.Transforms[5];
            result.Bounds.Right = resource.Value<double>("Text.bounds.Rght.Value") * scale + result.Transforms[4];
            result.Bounds.Bottom = resource.Value<double>("Text.bounds.Btom.Value") * scale + result.Transforms[5];

            //result.BoundingBox.Left = resource.Value<double>("Text.boundingBox.Left.Value") * scale + result.Transforms[4];
            //result.BoundingBox.Top = resource.Value<double>("Text.boundingBox.Top.Value") * scale + result.Transforms[5];
            //result.BoundingBox.Right = resource.Value<double>("Text.boundingBox.Rght.Value") * scale + result.Transforms[4];
            //result.BoundingBox.Bottom = resource.Value<double>("Text.boundingBox.Btom.Value") * scale + result.Transforms[5];

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
                    portion.FontSize = Math.Round(style.Value<double>("FontSize") * scale, 1);
                    portion.BaselineShift = Math.Round(style.Value<double>("BaselineShift") * scale, 1);
                    portion.FauxBold = style.Value<bool>("FauxBold");
                    portion.FauxItalic = style.Value<bool>("FauxItalic");
                    portion.Underline = style.Value<bool>("Underline");
                    var color = style.Value<System.Collections.ArrayList>("FillColor.Values");
                    portion.FillColor.A = (byte)((float)color[0] * 255);
                    portion.FillColor.R = (byte)((float)color[1] * 255);
                    portion.FillColor.G = (byte)((float)color[2] * 255);
                    portion.FillColor.B = (byte)((float)color[3] * 255);
                    result.Portion[i] = portion;
                }
            }

            return result;
        }
    }
    /// <summary>目前主要是实现圆角，内容分别是4个圆角的像素值</summary>
    public class ResourceVectorPath
    {
        public int LT;
        public int RT;
        public int LB;
        public int RB;

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
                    p3[j].X = (double)(pointList[j << 1]);
                    p3[j].Y = (double)(pointList[(j << 1) + 1]);
                }
                points[i] = p3;
            }

            // 圆角像素：圆角点相对于矩形框的像素
            // 例如正方形100x100：50像素即可变为圆形
            // 左上，右上，右下，左下 4个矩形点的坐标
            double[] corner = new double[4];
            if (points.Length == 8 || points.Length == 4)
            {
                corner[0] = points[0][2].X;
                corner[1] = points[1][0].X;
                corner[2] = points[4][2].X;
                corner[3] = points[5][0].X;
            }
            else if (points.Length == 6)
            {
                corner[0] = points[0][2].X;
                corner[1] = points[1][0].X;
                corner[2] = points[4][2].X;
                corner[3] = points[5][0].X;
            }

            result.LT = (int)Math.Round(corner[0] * layer.Document.Width - layer.Left);
            result.RT = (int)Math.Round(layer.Right - corner[1] * layer.Document.Width);
            result.RB = (int)Math.Round(layer.Right - corner[2] * layer.Document.Width);
            result.LB = (int)Math.Round(corner[3] * layer.Document.Width - layer.Left);

            return result;
        }
    }
    /// <summary>形状图层相关信息</summary>
    public class ResourceFill
    {
        public enum EShapeType
        {
            /// <summary>矩形也可以做到圆角矩形</summary>
            Rectangle = 1,
            /// <summary>旧版本的圆角矩形</summary>
            RoundedRectangle = 2,
            /// <summary>线段</summary>
            Line = 4,
            /// <summary>圆/椭圆</summary>
            Circle = 5,
            /// <summary>三角形</summary>
            Triangle = 7,
            /// <summary>多边形</summary>
            Polygon = 8,
        }
        /// <summary>图形</summary>
        public class Shape
        {
            public bool keyShapeInvalidated;

            public EShapeType ShapeType;

            /// <summary>圆角像素值</summary>
            public int RRectRadiiTopLeft;
            /// <summary>圆角像素值</summary>
            public int RRectRadiiTopRight;
            /// <summary>圆角像素值</summary>
            public int RRectRadiiBottomLeft;
            /// <summary>圆角像素值</summary>
            public int RRectRadiiBottomRight;
            /// <summary>是否有圆角</summary>
            public bool HasRadii
            {
                get
                {
                    return RRectRadiiTopLeft != 0
                        || RRectRadiiTopRight != 0
                        || RRectRadiiBottomLeft != 0
                        || RRectRadiiBottomRight != 0;
                }
            }
            /// <summary>是否每个角的圆角值都一样</summary>
            public bool IsSameRadii
            {
                get
                {
                    return RRectRadiiTopLeft == RRectRadiiTopRight
                        && RRectRadiiTopLeft == RRectRadiiBottomLeft
                        && RRectRadiiTopLeft == RRectRadiiBottomRight;
                }
            }

            /// <summary>图形的包围盒，PS中Ctrl+T显示的矩形，但是也有不准确的时候</summary>
            public PsdRect ShapeBoundingBox;
            public bool HasBoundingBox
            {
                get
                {
                    return ShapeBoundingBox.Left != 0
                        || ShapeBoundingBox.Top != 0
                        || ShapeBoundingBox.Right != 0
                        || ShapeBoundingBox.Bottom != 0;
                }
            }

            /// <summary>形状为线段时，线段的粗度</summary>
            public double LineWeight;
            /// <summary>形状为线段时，线段的起始点</summary>
            public PsdPoint LineStartPoint;
            /// <summary>形状为线段时，线段的结束点</summary>
            public PsdPoint LineEndPoint;
            public bool IsLine { get { return ShapeType == EShapeType.Line; } }
            public bool IsHorizontalLine { get { return IsLine && LineEndPoint.Y == LineStartPoint.Y; } }
            public bool IsVerticalLine { get { return IsLine && LineEndPoint.X == LineStartPoint.X; } }

            public bool IsEllipse
            {
                get { return ShapeType == EShapeType.Circle && !IsCircle; }
            }
            public bool IsCircle
            {
                get
                {
                    if (ShapeType != EShapeType.Circle || !HasBoundingBox) return false;
                    return ShapeBoundingBox.Width == ShapeBoundingBox.Height;
                }
            }
        }

        /// <summary>填充形状图层的颜色</summary>
        public PsdColor FillColor;
        public bool HasFillColor { get { return FillColor.A != 0; } }
        
        /// <summary>描边宽度（描边会在图形内测，不会扩大包围盒）</summary>
        public double BorderWidth;
        public PsdColor BorderColor;
        public bool HasBorder { get { return BorderWidth > 0; } }
        public bool HasBorderColor { get { return BorderColor.A != 0; } }

        public Shape[] Shapes;
        public PsdRect BoundingBox
        {
            get
            {
                if (Shapes == null || Shapes.Length == 0)
                    return new PsdRect();
                PsdRect box = new PsdRect();
                for (int i = 0; i < Shapes.Length; i++)
                {
                    var shape = Shapes[i];
                    if (shape.HasBoundingBox && !shape.keyShapeInvalidated)
                    {
                        if (box.IsEmpty)
                        {
                            box = shape.ShapeBoundingBox;
                        }
                        else
                        {
                            if (shape.ShapeBoundingBox.Left < box.Left) box.Left = shape.ShapeBoundingBox.Left;
                            if (shape.ShapeBoundingBox.Top < box.Top) box.Top = shape.ShapeBoundingBox.Top;
                            if (shape.ShapeBoundingBox.Right > box.Right) box.Right = shape.ShapeBoundingBox.Right;
                            if (shape.ShapeBoundingBox.Bottom > box.Bottom) box.Bottom = shape.ShapeBoundingBox.Bottom;
                        }
                    }
                }
                return box;
            }
        }

        public static bool Is(IPsdLayer layer)
        {
            return layer.Resources.Any(r => r.Key == "SoCo" || r.Key == "vscg");
        }
        public static ResourceFill CreateShapeOnly(IPsdLayer layer)
        {
            ResourceFill result = new ResourceFill();
            // 形状
            var resource = layer.Resources.Value<IProperties>("vogk");
            if (resource != null)
            {
                var array = resource.Value<IList>("keyDescriptorList.Items");
                int count = array.Count;
                result.Shapes = new Shape[count];
                for (int i = 0; i < count; i++)
                {
                    var item = (IProperties)array[i];
                    Shape shape = new Shape();
                    shape.keyShapeInvalidated = item.Value<bool>("keyShapeInvalidated");
                    shape.ShapeType = (EShapeType)item.Value<int>("keyOriginType");
                    // 三角形：边数keyOriginPolySides / 圆角值keyOriginPolyCornerRadius / 星形keyOriginPolyIndentBy.Value(单位%)
                    // 线段：宽度keyOriginLineWeight / 起始点keyOriginLineStart.Hrzn|Vrtc / 结束点keyOriginLineEnd.Hrzn|Vrtc
                    if (shape.ShapeType == EShapeType.Line)
                    {
                        shape.LineWeight = item.Value<double>("keyOriginLineWeight");
                        shape.LineStartPoint.X = item.Value<double>("keyOriginLineStart.Hrzn");
                        shape.LineStartPoint.Y = item.Value<double>("keyOriginLineStart.Vrtc");
                        shape.LineEndPoint.X = item.Value<double>("keyOriginLineEnd.Hrzn");
                        shape.LineEndPoint.Y = item.Value<double>("keyOriginLineEnd.Vrtc");
                    }
                    // 矩形圆角
                    var radii = item.Value<IProperties>("keyOriginRRectRadii");
                    if (radii != null)
                    {
                        shape.RRectRadiiTopLeft = radii.Value<int>("topLeft.Value");
                        shape.RRectRadiiTopRight = radii.Value<int>("topRight.Value");
                        shape.RRectRadiiBottomLeft = radii.Value<int>("bottomLeft.Value");
                        shape.RRectRadiiBottomRight = radii.Value<int>("bottomRight.Value");
                    }
                    // 形状包围盒
                    var bbox = item.Value<IProperties>("keyOriginShapeBBox");
                    if (bbox != null)
                    {
                        shape.ShapeBoundingBox.Left = bbox.Value<int>("Left.Value");
                        shape.ShapeBoundingBox.Top = bbox.Value<int>("Top.Value");
                        shape.ShapeBoundingBox.Right = bbox.Value<int>("Rght.Value");
                        shape.ShapeBoundingBox.Bottom = bbox.Value<int>("Btom.Value");
                    }
                    result.Shapes[i] = shape;
                }
            }
            return result;
        }
        public static ResourceFill Create(IPsdLayer layer)
        {
            ResourceFill result = CreateShapeOnly(layer);
            result.FillColor.A = 255;

            IProperties resource;

            // 描边 & 是否填充
            resource = layer.Resources.Value<IProperties>("vstk");
            if (resource != null)
            {
                if (resource.Value<bool>("strokeEnabled"))
                {
                    result.BorderWidth = resource.Value<double>("strokeStyleLineWidth.Value");
                    switch (resource.Value<string>("strokeStyleContent.ClassID"))
                    {
                        // 描边固定颜色
                        case "solidColorLayer":
                            result.BorderColor.R = (byte)resource.Value<double>("strokeStyleContent.Clr.Rd");
                            result.BorderColor.G = (byte)resource.Value<double>("strokeStyleContent.Clr.Grn");
                            result.BorderColor.B = (byte)resource.Value<double>("strokeStyleContent.Clr.Bl");
                            result.BorderColor.A = 255;
                            break;

                        // todo: 描边渐变颜色
                        case "gradientLayer":
                            break;

                        default:
                            break;
                    }
                }
                // 是否填充颜色
                if (!resource.Value<bool>("fillEnabled"))
                {
                    result.FillColor.A = 0;
                }
            }

            if (result.FillColor.A != 0)
            {
                resource = layer.Resources.Value<IProperties>("SoCo", "vscg");
                if (resource != null)
                {
                    // 形状图层填充颜色
                    resource = resource.Value<IProperties>("Clr");
                    if (resource != null)
                    {
                        result.FillColor.R = (byte)resource.Value<double>("Rd");
                        result.FillColor.G = (byte)resource.Value<double>("Grn");
                        result.FillColor.B = (byte)resource.Value<double>("Bl");
                        result.FillColor.A = 255;
                    }

                    resource = resource.Value<IProperties>("Grad");
                    if (resource != null)
                    {
                        // todo: 填充渐变颜色
                    }
                }
            }

            return result;
        }
    }
}
