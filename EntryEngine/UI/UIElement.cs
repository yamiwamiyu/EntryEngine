#if CLIENT

using System;
using System.Collections.Generic;
using System.Linq;

namespace EntryEngine.UI
{
    [Flags]
    public enum EAnchor
    {
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
        Center = 16,
        Middle = 32,
    }
    public enum EPivot
    {
        TopLeft = 0x00,
        TopCenter = 0x01,
        TopRight = 0x02,
        MiddleLeft = 0x10,
        MiddleCenter = 0x11,
        MiddleRight = 0x12,
        BottomLeft = 0x20,
        BottomCenter = 0x21,
        BottomRight = 0x22,
    }
    public delegate void DUpdate<T>(T sender, Entry e) where T : UIElement;
    public delegate void DDraw<T>(T sender, GRAPHICS spriteBatch, Entry e) where T : UIElement;
    public delegate void DUpdateGlobal(UIElement sender, bool senderEventInvoke, Entry e);

    /// <summary>子类控件会默认采用父类样式，例如0x11会采用0x10的样式</summary>
    public enum EUIType
    {
        UIElement = 0,

        TextureBox = 0x10,
        AnimationBox = 0x11,

        Panel = 0x20,
        UIScene = 0x21,
        Selectable = 0x22,

        Button = 0x30,
        
        Label = 0x40,

        TextBox = 0x50,

        CheckBox = 0x60,
        TabPage = 0x61,

        DropDown = 0x70,

        //Slider = 0x80,
        ScrollBar = 0x80,
    }
    public class UIStyle
    {
        public static UIStyle Style;

        internal Dictionary<EUIType, List<Action<UIElement>>> styles = new Dictionary<EUIType, List<Action<UIElement>>>();
        /// <summary>添加某类UI控件的默认样式</summary>
        /// <param name="uiType">UI控件类型</param>
        /// <param name="setStyle">设置UI样式的方法，返回true则设置了样式（例如CheckBox的单选框和复选框就可能类型都是CheckBox但是样式不同）</param>
        public void AddStyle(EUIType uiType, Action<UIElement> setStyle)
        {
            List<Action<UIElement>> sets;
            if (!styles.TryGetValue(uiType, out sets))
            {
                sets = new List<Action<UIElement>>();
                styles.Add(uiType, sets);
            }
            sets.Add(setStyle);
        }
        public bool FitStyle(UIElement element)
        {
            int uiType = (int)element.UIType;
            int value = uiType & 15;

            List<Action<UIElement>> sets;
            for (int v = 0; v <= value; v++)
            {
                EUIType type = (EUIType)((uiType >> 4 << 4) | v);
                if (styles.TryGetValue(type, out sets))
                {
                    for (int i = 0; i < sets.Count; i++)
                    {
                        sets[i](element);
                    }
                }
            }

            return false;
        }

        public static COLOR ToLight(COLOR originColor)
        {
            return ToLight(originColor, 0.2f);
        }
        public static COLOR ToLight(COLOR originColor, float percent)
        {
            return new COLOR((byte)(originColor.R + (255 - originColor.R) * percent), (byte)(originColor.G + (255 - originColor.G) * percent), (byte)(originColor.B + (255 - originColor.B) * percent), originColor.A);
        }
        public static COLOR ToDark(COLOR originColor)
        {
            return ToDark(originColor, 0.2f);
        }
        public static COLOR ToDark(COLOR originColor, float percent)
        {
            float b = 1 / (1 - percent);
            return new COLOR(
                (byte)_MATH.Round((originColor.R - 255 * percent) * b),
                (byte)_MATH.Round((originColor.G - 255 * percent) * b),
                (byte)_MATH.Round((originColor.B - 255 * percent) * b),
                originColor.A);
        }
        public static void StyleButtonPatchBorder(Button item, COLOR defaultColor, COLOR hoverColor, byte bold, COLOR defaultFontColor)
        {
            COLOR fontColorUnable = ToLight(defaultFontColor, 0.6f);
            COLOR colorUnable = ToLight(defaultFontColor, 0.2f);
            COLOR colorClick = ToDark(hoverColor, 0.2f);

            item.Width = 100;
            item.Height = 40;
            item.SourceNormal = PATCH.GetNinePatch(COLOR.TransparentWhite, defaultColor, bold);
            item.SourceHover = PATCH.GetNinePatch(COLOR.TransparentWhite, hoverColor, bold);
            item.SourceClick = PATCH.GetNinePatch(COLOR.TransparentWhite, colorClick, bold);
            item.UpdateBegin = (sender, e) =>
            {
                if (sender.FinalEventable)
                {
                    if (sender.IsClick && sender.IsHover)
                    {
                        item.UIText.FontColor = colorClick;
                    }
                    else
                    {
                        if (sender.IsHover)
                        {
                            item.UIText.FontColor = hoverColor;
                        }
                        else
                        {
                            item.UIText.FontColor = defaultFontColor;
                        }
                    }
                }
                else
                {
                    item.UIText.FontColor = fontColorUnable;
                }
            };
        }
        public static void StyleButtonPatchBody(Button item, COLOR defaultColor, COLOR hoverColor, byte bold, COLOR defaultFontColor)
        {
            StyleButtonPatchBorder(item, defaultColor, hoverColor, bold, defaultFontColor);
            COLOR colorBorder = ToLight(hoverColor, 0.6f);
            COLOR colorBody = ToLight(hoverColor, 0.9f);
            item.SourceHover = PATCH.GetNinePatch(colorBody, colorBorder, bold);
            //item.SourceClick = item.SourceHover;
            item.SourceClick = PATCH.GetNinePatch(ToDark(colorBody), ToDark(colorBorder), bold);
        }
        public static void StyleButtonPatchBody2(Button item, COLOR defaultColor, COLOR defaultFontColor)
        {
            COLOR colorHover = ToLight(defaultColor);
            COLOR colorClick = ToDark(defaultColor);
            COLOR colorUnable = ToLight(defaultColor, 0.5f);

            item.Width = 100;
            item.Height = 40;
            item.SourceNormal = TEXTURE.Pixel;
            item.SourceHover = item.SourceNormal;
            item.SourceClick = item.SourceNormal;

            item.UIText.FontColor = defaultFontColor;

            item.UpdateBegin = (sender, e) =>
            {
                if (sender.FinalEventable)
                {
                    if (sender.IsClick && sender.IsHover)
                    {
                        item.Color = colorClick;
                    }
                    else
                    {
                        if (sender.IsHover)
                        {
                            item.Color = colorHover;
                        }
                        else
                        {
                            item.Color = defaultColor;
                        }
                    }
                }
                else
                {
                    item.Color = colorUnable;
                }
            };
        }
        public static void StyleButtonPatchBody3(Button item, COLOR defaultColor, COLOR contrastFontColor, byte bold)
        {
            COLOR colorBorder = ToLight(defaultColor, 0.6f);
            COLOR colorBody = ToLight(defaultColor, 0.9f);

            COLOR colorHover = ToLight(defaultColor);
            COLOR colorClick = ToDark(defaultColor);
            COLOR colorUnable = ToLight(defaultColor, 0.5f);
            COLOR colorUnableHover = ToLight(contrastFontColor, 0.5f);

            item.Width = 100;
            item.Height = 40;
            item.SourceNormal = PATCH.GetNinePatch(colorBody, colorBorder, bold);
            item.SourceHover = PATCH.GetNinePatch(colorHover, COLOR.TransparentBlack, 0);
            item.SourceClick = PATCH.GetNinePatch(colorClick, COLOR.TransparentBlack, 0);

            item.UIText.FontColor = contrastFontColor;

            item.UpdateBegin = (sender, e) =>
            {
                if (sender.FinalEventable)
                {
                    if (sender.IsHover)
                    {
                        item.UIText.FontColor = contrastFontColor;
                    }
                    else
                    {
                        item.UIText.FontColor = defaultColor;
                    }
                }
                else
                {
                    if (sender.IsHover)
                    {
                        item.UIText.FontColor = colorUnableHover;
                    }
                    else
                    {
                        item.UIText.FontColor = colorUnable;
                    }
                }
            };
        }
        static TEXTURE CheckBox_Normal;
        static TEXTURE CheckBox_Checked;
        public static void StyleCheckBox(CheckBox item, COLOR defaultColor, COLOR checkedFontColor)
        {
            if (CheckBox_Normal == null)
            {
                string key = "*.png";
                foreach (var pipeline in Entry._ContentManager.ContentPipelines)
                {
                    if (pipeline.Processable(ref key) && pipeline is ContentPipelineBinary)
                    {
                        //byte[] bytes1 = _IO.ReadByte("icon/fangxingweixuanzhong.png");
                        //byte[] bytes2 = _IO.ReadByte("icon/fangxingxuanzhongfill.png");
                        //string s1 = BuildBytesString(bytes1);
                        //string s2 = BuildBytesString(bytes2);
                        CheckBox_Normal = (TEXTURE)((ContentPipelineBinary)pipeline).LoadFromBytes(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 64, 0, 0, 0, 64, 16, 6, 0, 0, 0, 250, 249, 173, 157, 0, 0, 0, 4, 103, 65, 77, 65, 0, 0, 177, 143, 11, 252, 97, 5, 0, 0, 0, 1, 115, 82, 71, 66, 0, 174, 206, 28, 233, 0, 0, 0, 32, 99, 72, 82, 77, 0, 0, 122, 38, 0, 0, 128, 132, 0, 0, 250, 0, 0, 0, 128, 232, 0, 0, 117, 48, 0, 0, 234, 96, 0, 0, 58, 152, 0, 0, 23, 112, 156, 186, 81, 60, 0, 0, 0, 6, 98, 75, 71, 68, 0, 0, 0, 0, 0, 0, 249, 67, 187, 127, 0, 0, 0, 9, 112, 72, 89, 115, 0, 0, 0, 72, 0, 0, 0, 72, 0, 70, 201, 107, 62, 0, 0, 2, 43, 73, 68, 65, 84, 120, 218, 237, 221, 49, 78, 235, 64, 16, 198, 241, 153, 232, 9, 10, 184, 1, 112, 3, 16, 80, 1, 71, 64, 130, 11, 208, 3, 119, 178, 40, 195, 17, 194, 21, 32, 77, 64, 2, 46, 128, 196, 5, 8, 20, 80, 120, 94, 49, 56, 213, 139, 95, 108, 18, 89, 225, 251, 255, 154, 209, 38, 214, 218, 94, 127, 94, 185, 218, 53, 3, 0, 0, 0, 160, 197, 127, 218, 65, 148, 81, 70, 185, 186, 154, 173, 203, 203, 172, 103, 103, 89, 183, 183, 205, 205, 205, 215, 214, 186, 190, 209, 165, 21, 22, 22, 31, 31, 217, 120, 122, 202, 218, 239, 103, 45, 10, 239, 121, 207, 123, 95, 95, 109, 187, 111, 29, 128, 136, 136, 136, 141, 141, 108, 13, 6, 89, 119, 119, 187, 30, 47, 25, 97, 97, 241, 240, 144, 47, 216, 201, 137, 187, 187, 251, 235, 107, 211, 110, 26, 7, 96, 242, 198, 187, 185, 249, 112, 152, 191, 242, 224, 59, 19, 22, 22, 247, 247, 217, 56, 60, 108, 58, 35, 244, 218, 157, 181, 154, 234, 121, 240, 157, 115, 115, 243, 253, 253, 172, 231, 231, 11, 63, 95, 78, 253, 195, 97, 212, 26, 12, 114, 166, 216, 220, 236, 122, 124, 150, 93, 53, 142, 89, 111, 110, 166, 14, 121, 25, 101, 148, 183, 183, 139, 191, 160, 136, 136, 24, 143, 235, 47, 132, 7, 63, 111, 57, 174, 91, 91, 245, 47, 222, 219, 91, 211, 126, 155, 127, 3, 124, 155, 218, 225, 183, 174, 7, 236, 183, 154, 247, 248, 183, 252, 6, 192, 111, 65, 0, 196, 17, 0, 113, 4, 64, 28, 1, 16, 71, 0, 196, 17, 0, 113, 4, 64, 28, 1, 16, 71, 0, 196, 17, 0, 113, 4, 64, 28, 1, 16, 71, 0, 196, 17, 0, 113, 4, 64, 28, 1, 16, 71, 0, 196, 17, 0, 113, 4, 64, 28, 1, 16, 71, 0, 196, 17, 0, 113, 4, 64, 28, 1, 16, 71, 0, 196, 17, 0, 113, 4, 64, 28, 1, 16, 71, 0, 196, 17, 0, 113, 4, 64, 28, 1, 16, 71, 0, 196, 17, 0, 113, 4, 64, 28, 1, 16, 71, 0, 196, 17, 0, 113, 45, 3, 240, 254, 62, 237, 31, 22, 138, 92, 140, 106, 161, 200, 250, 163, 198, 227, 166, 253, 54, 15, 64, 88, 88, 60, 63, 215, 31, 84, 20, 179, 93, 48, 254, 103, 50, 142, 110, 110, 94, 20, 245, 71, 87, 203, 201, 207, 238, 79, 227, 43, 114, 115, 243, 235, 235, 108, 28, 28, 252, 251, 255, 227, 227, 108, 188, 188, 212, 175, 107, 137, 249, 170, 246, 17, 152, 93, 251, 229, 226, 205, 204, 236, 238, 46, 31, 248, 222, 94, 215, 183, 174, 109, 52, 202, 153, 249, 232, 104, 225, 203, 197, 231, 9, 62, 63, 171, 141, 10, 38, 27, 23, 160, 3, 163, 81, 214, 211, 211, 182, 59, 135, 204, 105, 203, 152, 149, 149, 108, 93, 92, 100, 48, 170, 45, 99, 118, 118, 178, 174, 175, 119, 61, 84, 203, 173, 250, 232, 126, 124, 204, 218, 239, 231, 139, 119, 117, 245, 211, 45, 99, 0, 0, 0, 0, 168, 249, 11, 158, 191, 71, 200, 104, 30, 246, 84, 0, 0, 0, 37, 116, 69, 88, 116, 100, 97, 116, 101, 58, 99, 114, 101, 97, 116, 101, 0, 50, 48, 49, 57, 45, 48, 53, 45, 50, 51, 84, 49, 51, 58, 49, 48, 58, 48, 48, 43, 48, 56, 58, 48, 48, 51, 147, 206, 23, 0, 0, 0, 37, 116, 69, 88, 116, 100, 97, 116, 101, 58, 109, 111, 100, 105, 102, 121, 0, 50, 48, 49, 57, 45, 48, 53, 45, 50, 51, 84, 49, 51, 58, 49, 48, 58, 48, 48, 43, 48, 56, 58, 48, 48, 66, 206, 118, 171, 0, 0, 0, 86, 116, 69, 88, 116, 115, 118, 103, 58, 98, 97, 115, 101, 45, 117, 114, 105, 0, 102, 105, 108, 101, 58, 47, 47, 47, 104, 111, 109, 101, 47, 97, 100, 109, 105, 110, 47, 105, 99, 111, 110, 45, 102, 111, 110, 116, 47, 116, 109, 112, 47, 105, 99, 111, 110, 95, 119, 113, 57, 101, 118, 106, 111, 98, 119, 49, 47, 102, 97, 110, 103, 120, 105, 110, 103, 119, 101, 105, 120, 117, 97, 110, 122, 104, 111, 110, 103, 46, 115, 118, 103, 101, 1, 101, 36, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 });
                        CheckBox_Checked = (TEXTURE)((ContentPipelineBinary)pipeline).LoadFromBytes(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 64, 0, 0, 0, 64, 16, 6, 0, 0, 0, 250, 249, 173, 157, 0, 0, 0, 4, 103, 65, 77, 65, 0, 0, 177, 143, 11, 252, 97, 5, 0, 0, 0, 1, 115, 82, 71, 66, 0, 174, 206, 28, 233, 0, 0, 0, 32, 99, 72, 82, 77, 0, 0, 122, 38, 0, 0, 128, 132, 0, 0, 250, 0, 0, 0, 128, 232, 0, 0, 117, 48, 0, 0, 234, 96, 0, 0, 58, 152, 0, 0, 23, 112, 156, 186, 81, 60, 0, 0, 0, 6, 98, 75, 71, 68, 0, 0, 0, 0, 0, 0, 249, 67, 187, 127, 0, 0, 0, 9, 112, 72, 89, 115, 0, 0, 0, 72, 0, 0, 0, 72, 0, 70, 201, 107, 62, 0, 0, 4, 24, 73, 68, 65, 84, 120, 218, 237, 157, 205, 107, 19, 65, 24, 198, 103, 214, 166, 165, 68, 69, 11, 165, 20, 233, 63, 32, 138, 122, 179, 158, 122, 171, 7, 75, 17, 74, 4, 237, 213, 84, 189, 247, 92, 5, 21, 47, 22, 107, 181, 66, 245, 82, 73, 241, 16, 90, 26, 154, 92, 252, 160, 69, 69, 234, 193, 40, 180, 189, 244, 238, 65, 188, 105, 196, 47, 152, 215, 195, 147, 105, 77, 204, 199, 206, 54, 155, 49, 59, 239, 239, 242, 48, 155, 221, 157, 247, 221, 247, 217, 205, 146, 157, 236, 8, 193, 48, 12, 195, 48, 12, 195, 48, 140, 91, 200, 221, 238, 128, 20, 41, 82, 29, 29, 104, 141, 141, 65, 207, 159, 135, 30, 57, 34, 164, 144, 66, 198, 227, 182, 19, 109, 89, 72, 144, 160, 111, 223, 208, 216, 216, 128, 166, 82, 208, 217, 89, 233, 73, 79, 122, 191, 126, 5, 221, 125, 96, 3, 16, 17, 17, 29, 58, 132, 86, 46, 7, 61, 118, 204, 246, 241, 114, 6, 18, 36, 232, 195, 7, 156, 96, 103, 206, 72, 41, 165, 148, 31, 63, 154, 238, 198, 216, 0, 219, 103, 188, 20, 82, 200, 183, 111, 177, 148, 11, 111, 13, 18, 36, 232, 253, 123, 52, 78, 158, 52, 189, 34, 120, 193, 122, 213, 151, 122, 46, 188, 117, 164, 144, 66, 158, 56, 1, 189, 120, 209, 116, 115, 115, 3, 72, 33, 133, 188, 112, 193, 118, 222, 76, 25, 36, 72, 208, 232, 168, 233, 102, 230, 95, 1, 68, 68, 244, 245, 43, 90, 123, 247, 218, 206, 155, 249, 155, 66, 1, 247, 2, 251, 246, 249, 221, 34, 160, 1, 136, 108, 167, 202, 84, 70, 22, 241, 187, 126, 192, 123, 0, 38, 42, 176, 1, 28, 135, 13, 224, 56, 108, 0, 199, 97, 3, 56, 14, 27, 192, 113, 216, 0, 142, 195, 6, 112, 28, 54, 128, 227, 176, 1, 28, 135, 13, 224, 56, 108, 128, 80, 200, 102, 241, 116, 174, 175, 15, 237, 222, 94, 104, 38, 99, 59, 178, 114, 248, 97, 80, 67, 89, 90, 66, 225, 19, 9, 12, 204, 248, 253, 91, 127, 130, 129, 52, 221, 221, 120, 156, 254, 233, 19, 150, 250, 127, 104, 227, 23, 126, 24, 100, 133, 229, 101, 20, 254, 220, 185, 242, 194, 151, 226, 21, 143, 119, 227, 11, 31, 148, 54, 219, 1, 180, 54, 185, 28, 10, 63, 50, 82, 109, 40, 22, 174, 151, 186, 240, 119, 239, 218, 142, 184, 28, 190, 2, 4, 226, 233, 83, 104, 189, 194, 235, 51, 253, 193, 3, 104, 34, 97, 59, 242, 114, 216, 0, 126, 33, 65, 130, 158, 61, 67, 99, 120, 24, 223, 180, 63, 126, 252, 179, 90, 73, 225, 103, 102, 160, 201, 164, 237, 240, 171, 193, 6, 168, 135, 46, 188, 20, 82, 72, 191, 133, 191, 127, 31, 122, 233, 146, 237, 240, 235, 193, 6, 168, 201, 171, 87, 40, 252, 217, 179, 40, 252, 247, 239, 229, 107, 148, 22, 254, 222, 61, 232, 229, 203, 182, 35, 247, 203, 127, 106, 128, 173, 45, 232, 192, 0, 244, 240, 97, 156, 137, 233, 116, 115, 250, 95, 89, 129, 158, 62, 141, 194, 235, 127, 230, 236, 80, 249, 140, 191, 114, 197, 226, 65, 107, 14, 20, 54, 138, 20, 169, 193, 193, 202, 253, 122, 30, 244, 241, 227, 112, 58, 95, 93, 133, 86, 255, 43, 155, 46, 60, 116, 122, 58, 244, 227, 97, 72, 235, 27, 128, 136, 136, 78, 157, 170, 221, 255, 158, 61, 208, 39, 79, 26, 211, 223, 203, 151, 48, 94, 245, 97, 238, 88, 79, 23, 126, 106, 170, 57, 199, 193, 156, 214, 55, 128, 34, 69, 234, 249, 115, 104, 123, 123, 213, 56, 20, 41, 82, 109, 109, 208, 133, 133, 96, 157, 189, 126, 141, 237, 235, 143, 163, 199, 122, 147, 147, 77, 174, 167, 49, 173, 111, 128, 18, 22, 23, 113, 224, 99, 177, 218, 133, 137, 197, 176, 126, 38, 227, 111, 191, 111, 222, 96, 187, 253, 251, 253, 21, 254, 230, 205, 230, 230, 29, 156, 136, 25, 64, 147, 78, 235, 51, 190, 118, 161, 218, 219, 177, 126, 54, 91, 121, 63, 239, 222, 65, 15, 30, 244, 151, 231, 141, 27, 118, 242, 13, 78, 68, 13, 160, 153, 155, 131, 122, 94, 237, 248, 58, 59, 97, 136, 23, 47, 160, 249, 60, 150, 119, 117, 249, 203, 239, 250, 117, 187, 121, 6, 39, 226, 6, 160, 226, 61, 194, 195, 135, 104, 84, 127, 168, 130, 207, 227, 113, 232, 129, 3, 81, 47, 188, 38, 250, 6, 208, 248, 52, 130, 191, 124, 174, 93, 179, 157, 78, 163, 112, 199, 0, 37, 220, 185, 19, 44, 143, 137, 9, 219, 145, 55, 26, 71, 13, 64, 197, 43, 194, 237, 219, 117, 227, 87, 164, 72, 141, 143, 219, 14, 55, 44, 220, 53, 64, 9, 183, 110, 109, 255, 78, 64, 68, 59, 191, 32, 94, 189, 106, 59, 178, 176, 49, 173, 103, 196, 135, 132, 125, 254, 12, 85, 10, 218, 211, 99, 59, 162, 176, 49, 29, 18, 22, 113, 3, 184, 7, 143, 9, 100, 140, 96, 3, 56, 14, 27, 192, 113, 216, 0, 142, 195, 6, 112, 28, 54, 128, 227, 4, 52, 64, 161, 96, 59, 112, 166, 18, 95, 190, 152, 110, 97, 110, 0, 18, 36, 104, 115, 211, 118, 170, 76, 37, 204, 235, 18, 240, 93, 193, 243, 243, 182, 83, 101, 42, 161, 231, 17, 240, 79, 240, 215, 197, 11, 33, 132, 88, 91, 131, 33, 142, 31, 183, 157, 186, 219, 228, 243, 184, 50, 247, 247, 135, 254, 186, 120, 116, 240, 243, 167, 158, 168, 96, 123, 226, 2, 198, 2, 249, 60, 116, 104, 40, 232, 204, 33, 13, 154, 50, 70, 143, 222, 77, 38, 97, 12, 61, 101, 204, 209, 163, 80, 126, 171, 248, 238, 208, 55, 221, 235, 235, 208, 84, 10, 39, 222, 163, 71, 187, 157, 50, 134, 97, 24, 134, 97, 24, 134, 97, 24, 215, 248, 3, 165, 16, 163, 27, 241, 201, 62, 149, 0, 0, 0, 37, 116, 69, 88, 116, 100, 97, 116, 101, 58, 99, 114, 101, 97, 116, 101, 0, 50, 48, 49, 57, 45, 48, 53, 45, 50, 51, 84, 49, 51, 58, 49, 48, 58, 48, 48, 43, 48, 56, 58, 48, 48, 51, 147, 206, 23, 0, 0, 0, 37, 116, 69, 88, 116, 100, 97, 116, 101, 58, 109, 111, 100, 105, 102, 121, 0, 50, 48, 49, 57, 45, 48, 53, 45, 50, 51, 84, 49, 51, 58, 49, 48, 58, 48, 48, 43, 48, 56, 58, 48, 48, 66, 206, 118, 171, 0, 0, 0, 87, 116, 69, 88, 116, 115, 118, 103, 58, 98, 97, 115, 101, 45, 117, 114, 105, 0, 102, 105, 108, 101, 58, 47, 47, 47, 104, 111, 109, 101, 47, 97, 100, 109, 105, 110, 47, 105, 99, 111, 110, 45, 102, 111, 110, 116, 47, 116, 109, 112, 47, 105, 99, 111, 110, 95, 119, 113, 57, 101, 118, 106, 111, 98, 119, 49, 47, 102, 97, 110, 103, 120, 105, 110, 103, 120, 117, 97, 110, 122, 104, 111, 110, 103, 102, 105, 108, 108, 46, 115, 118, 103, 56, 44, 126, 56, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 });
                    }
                }
            }

            item.SourceNormal = CheckBox_Normal;
            item.SourceClicked = CheckBox_Checked;
            item.Width = 24;
            item.Height = 24;
            item.CheckedOverlayNormal = true;

            COLOR colorUnable = ToLight(defaultColor, 0.6f);
            COLOR colorUnableChecked = ToLight(checkedFontColor, 0.6f);

            bool fontColorAuto = item.UIText.FontColor == COLOR.White;

            item.UpdateBegin = (sender, e) =>
            {
                if (sender.FinalEventable)
                {
                    if (sender.IsHover || item.Checked)
                    {
                        item.Color = checkedFontColor;
                    }
                    else
                    {
                        item.Color = defaultColor;
                    }
                }
                else
                {
                    if (sender.IsHover || item.Checked)
                    {
                        item.Color = colorUnableChecked;
                    }
                    else
                    {
                        item.Color = colorUnable;
                    }
                }
                if (fontColorAuto)
                    item.UIText.FontColor = item.Color;
            };
        }
    }

    /// <summary>
    /// 简单，高性能绘制
    /// 1. 不用支持旋转、镜像
    /// 2. 非特殊情况下，不使用Transform、Shader和Scissor
    /// 
    /// 4种状态的Rectangle
    /// 1. 相对于 父容器 / 屏幕
    /// 2. 完整 / 裁剪
    /// 
    /// 更新顺序，绘制顺序，绘制覆盖顺序为逆序，所以可能需要将更新改为逆序
    /// 
    /// 每帧动作顺序
    /// 1. Childs从后往前Event, Parent.Event
    /// 2. Parent.Update, Childs从后往前Update
    /// 3. Parent.Draw, Childs从前往后Draw
    /// 
    /// IsClip及其相关参数的有不明确的地方，需要修改
    /// </summary>
    [Code(ECode.ToBeContinue)]
    public abstract class UIElement : Tree<UIElement>, IDisposable
    {
        internal static UIElement __PrevHandledElement;
        internal static UIElement __HandledElement;
        public bool Handled
        {
            get { return __HandledElement != null; }
        }
        public void Handle()
        {
            __HandledElement = this;
            //_LOG.Debug("Handle {0} Stack: {1}", this.GetType().Name, Environment.StackTrace);
        }

        protected internal static UIElement FocusedElement { get; private set; }
        public static DUpdateGlobal GlobalEnter;
        public static DUpdateGlobal GlobalHover;
        public static DUpdateGlobal GlobalUnHover;
        public static DUpdateGlobal GlobalClick;
        public static DUpdateGlobal GlobalClicked;

        public string Name;
        private RECT clip;
		private MATRIX2x3 model = MATRIX2x3.Identity;
        public SHADER Shader;
        public bool Enable = true;
        public bool Eventable = true;
        public bool Visible = true;
        public EAnchor Anchor = EAnchor.Left | EAnchor.Top;
        public COLOR Color = COLOR.Default;
        private bool isClip = true;
        private EPivot pivot;
        public object Tag;
        public event Action<VECTOR2> ContentSizeChanged;
        public DUpdate<UIElement> UpdateBegin;
        public DUpdate<UIElement> UpdateEnd;
        public DUpdate<UIElement> EventBegin;
        public DUpdate<UIElement> EventEnd;
        public DDraw<UIElement> DrawBeforeBegin;
        public DDraw<UIElement> DrawAfterBegin;
        public DDraw<UIElement> DrawBeforeChilds;
        public DDraw<UIElement> DrawBeforeEnd;
        public DDraw<UIElement> DrawFocus;
        public DDraw<UIElement> DrawAfterEnd;
        private List<Action<Entry>> events = new List<Action<Entry>>();
        /// <summary>鼠标进入区域内</summary>
        public event DUpdate<UIElement> Enter;
        /// <summary>鼠标在区域内移动</summary>
        public event DUpdate<UIElement> Move;
        /// <summary>鼠标离开区域</summary>
        public event DUpdate<UIElement> Exit;
        /// <summary>获得焦点</summary>
        public event DUpdate<UIElement> Focus;
        /// <summary>失去焦点</summary>
        public event DUpdate<UIElement> Blur;
        /// <summary>鼠标在区域内（不包含进入区域的一次）</summary>
        public event DUpdate<UIElement> Hover;
        /// <summary>鼠标不在区域内</summary>
        public event DUpdate<UIElement> UnHover;
        /// <summary>鼠标左键按下</summary>
        public event DUpdate<UIElement> Click;
        /// <summary>鼠标左键按住拖拽，并指针在目标范围内</summary>
        public event DUpdate<UIElement> Pressed;
        /// <summary>鼠标左键按住拖拽</summary>
        public event DUpdate<UIElement> Drag;
        /// <summary>鼠标左键抬起，需要触发过点击</summary>
        public event DUpdate<UIElement> Clicked;
        /// <summary>鼠标左键抬起</summary>
        public event DUpdate<UIElement> Released;
        /// <summary>鼠标左键双击</summary>
        public event DUpdate<UIElement> DoubleClick;
        /// <summary>键盘按键状态改变</summary>
        public event DUpdate<UIElement> Keyboard;

        private bool needUpdateLocalToWorld = true;
        /// <summary>
        /// 当一个子场景在主场景中时
        /// Entry对场景的更新是从后往前的，即子场景会先更新
        /// 对于Touch来说，前一帧没有按下时，自场景和父场景的Hover状态都是false
        /// 当前帧按下时，父场景由于Hover是false，会导致子场景的Hover也是false
        /// 所以此时应该像needUpdateLocalToWorld时先去更新父场景的Hover状态
        /// </summary>
        private bool needUpdateHover = true;
		private MATRIX2x3 world = MATRIX2x3.Identity;
		private MATRIX2x3 worldInvert = MATRIX2x3.Identity;
        protected VECTOR2 contentSize;
        private int sortZ = -1;
        private bool needSort;
        protected bool isHover;
        private bool isClick;
        /// <summary>viewport in Parent</summary>
        private RECT finalClip;
        /// <summary>graphics viewport in screen</summary>
        private RECT finalViewClip;
        private UIElement[] drawOrder;
        private bool isTopMost;

        internal virtual bool IsScene
        {
            get { return false; }
        }
        protected internal bool NeedUpdateLocalToWorld
        {
            get { return needUpdateLocalToWorld; }
            set
            {
                if (!needUpdateLocalToWorld && value)
                {
                    //foreach (UIElement child in this)
                    for (int i = 0; i < Childs.Count; i++)
                    {
                        Childs[i].NeedUpdateLocalToWorld = value;
                    }
                    needUpdateLocalToWorld = value;
                }
                needUpdateLocalToWorld = value;
            }
        }
        public MATRIX2x3 Model
        {
            get { return model; }
            set
            {
                model = value;
                NeedUpdateLocalToWorld = true;
            }
        }
		public MATRIX2x3 World
        {
            get { return world; }
        }
        public UIScene Scene
        {
            get
            {
                UIElement i = this;
                while (true)
                {
                    if (i.Parent == null)
                    {
                        if (i.IsScene)
                            return (UIScene)i;
                        else
                            return null;
                    }
                    else
                    {
                        if (i.Parent.IsScene)
                            return (UIScene)i.Parent;
                        else
                            i = i.Parent;
                    }
                }
            }
        }
        public UIScene SceneIsRunning
        {
            get
            {
                UIElement i = this;
                while (true)
                {
                    if (i.Parent == null)
                        if (i.IsScene)
                            return (UIScene)i;
                        else
                            return null;
                    i = i.Parent;
                }
            }
        }
        public virtual bool CanFocused
        {
            get { return false; }
        }
        public bool Focused
        {
            get { return FocusedElement == this; }
            set { SetFocus(value); }
        }
        public UIElement NextFocusedElement
        {
            get
            {
                // 检查子对象是否可以设置焦点
                UIElement result = this.CanFocusChild;
                if (result != null)
                {
                    return result;
                }

                for (UIElement parent = Parent, child = this; parent != null; child = parent, parent = parent.Parent)
                {
                    int index = parent.Childs.IndexOf(child);
                    for (int i = index + 1; i < parent.Childs.Count; i++)
                    {
                        // 检查父对象是否可以设置焦点
                        if (parent.Childs[i].CanFocused && parent.Childs[i].IsEventable)
                        {
                            return parent.Childs[i];
                        }
                        // 检查父对象的所有子对象是否可以设置焦点
                        result = parent.Childs[i].CanFocusChild;
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }

                // 最后的焦点控件到头则为无焦点
                return null;
            }
        }
        private UIElement CanFocusChild
        {
            get
            {
                if (!IsEventable)
                    return null;

                UIElement result;
                for (int i = 0; i < Childs.Count; i++)
                {
                    result = Childs[i];
                    if (result.CanFocused && result.IsEventable)
                    {
                        return result;
                    }

                    result = result.CanFocusChild;
                    if (result != null)
                    {
                        return result;
                    }
                }
                return null;
            }
        }

        public float X
        {
            get { return clip.X; }
            set
            {
                if (X != value)
                {
                    clip.X = value;
                    NeedUpdateLocalToWorld = true;
                }
            }
        }
        public float Y
        {
            get { return clip.Y; }
            set
            {
                if (Y != value)
                {
                    clip.Y = value;
                    NeedUpdateLocalToWorld = true;
                }
            }
        }
        public VECTOR2 Location
        {
            get { return new VECTOR2(X, Y); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        public virtual float Width
        {
            get { return Clip.Width; }
            set
            {
                if (value == 0)
                    clip.Width = 0;
                UpdateWidth(Width, value);
                clip.Width = value;
                NeedUpdateLocalToWorld = true;
            }
        }
        public virtual float Height
        {
            get { return Clip.Height; }
            set
            {
                if (value == 0)
                    clip.Height = 0;
                UpdateHeight(Height, value);
                clip.Height = value;
                NeedUpdateLocalToWorld = true;
            }
        }
        public VECTOR2 Size
        {
            get { return new VECTOR2(Width, Height); }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }
        public RECT Clip
        {
            get
            {
                if (!IsAutoClip)
                {
                    return clip;
                }
                else
                {
                    RECT autoClip = new RECT();
                    autoClip.X = X;
                    autoClip.Y = Y;

                    if (clip.Width == 0)
                        autoClip.Width = contentSize.X;
                    else
                        autoClip.Width = clip.Width;

                    if (clip.Height == 0)
                        autoClip.Height = contentSize.Y;
                    else
                        autoClip.Height = clip.Height;

                    return autoClip;
                }
            }
            set
            {
                X = value.X;
                Y = value.Y;
                Width = value.Width;
                Height = value.Height;
            }
        }
        public RECT InParentClip
        {
            get { return InParent(Clip); }
        }
        public virtual VECTOR2 ContentSize
        {
            get
            {
                RECT clip = ChildClip;
                return new VECTOR2(clip.Right, clip.Bottom);
                //return ChildClip.Size;
            }
        }
        public virtual RECT ChildClip
        {
            get
            {
                return CalcChildClip(this, DefaultChildClip);
            }
        }
        public RECT InParentChildClip
        {
            get { return InParent(ChildClip); }
        }
        public EPivot Pivot
        {
            get { return pivot; }
            set
            {
                if (pivot != value)
                {
                    pivot = value;
                    NeedUpdateLocalToWorld = true;
                }
            }
        }
        public VECTOR2 PivotPoint
        {
            get
            {
                VECTOR2 size = Size;
                return new VECTOR2(PivotAlignmentX * size.X * 0.5f, PivotAlignmentY * size.Y * 0.5f);
            }
        }
        /// <summary>左0/中1/右2</summary>
        public int PivotAlignmentX
        {
            get { return (int)pivot & 0x0f; }
            set { Pivot = (EPivot)(value + (PivotAlignmentY >> 4)); }
        }
        /// <summary>上0/中1/下2</summary>
        public int PivotAlignmentY
        {
            get { return ((int)pivot & 0xf0) >> 4; }
            set { Pivot = (EPivot)(PivotAlignmentX + (value >> 4)); }
        }
        public bool IsAutoClip
        {
            get { return clip.Width == 0 || clip.Height == 0; }
        }
        public bool IsAutoWidth
        {
            get { return clip.Width == 0; }
        }
        public bool IsAutoHeight
        {
            get { return clip.Height == 0; }
        }
        /// <summary>约束子控件是否在自己的可视范围内才让有效</summary>
        public bool IsClip
        {
            get { return isClip; }
            set
            {
                if (isClip != value)
                {
                    isClip = value;
                    NeedUpdateLocalToWorld = true;
                }
            }
        }
        public bool IsHover
        {
            get { return isHover; }
        }
        public bool IsClick
        {
            get { return isClick; }
        }
        public int SortZ
        {
            get { return sortZ; }
            set
            {
                if (sortZ != value)
                {
                    sortZ = value;
                    if (Parent != null)
                    {
                        Parent.needSort = true;
                    }
                }
            }
        }
        public bool FinalHover
        {
            get
            {
                for (int i = 0; i < Childs.Count; i++)
                {
                    if (Childs[i].FinalHover)
                    {
                        return true;
                    }
                }
                return isHover;
            }
        }
        public bool FinalEnable
        {
            get
            {
                for (UIElement i = this; i != null; i = i.Parent)
                {
                    if (!i.IsEnable)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public bool FinalEventable
        {
            get
            {
                for (UIElement i = this; i != null; i = i.Parent)
                {
                    if (!i.IsEventable)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public bool FinalVisible
        {
            get
            {
                for (UIElement i = this; i != null; i = i.Parent)
                {
                    if (!i.IsVisible)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public bool IsEnable
        {
            get { return Enable && IsVisible; }
        }
        public bool IsEventable
        {
            get { return Eventable && IsVisible; }
        }
        public bool IsVisible
        {
            //get { return Visible && Color.A > 0; }
            get { return Visible; }
        }
        protected bool NeedDrawChild
        {
            get
            {
                if (drawOrder == null || drawOrder.Length == 0)
                    return false;

                if (!IsVisible)
                    return false;

                //for (int i = 0; i < drawOrder.Length; i++)
                //{
                //    if (drawOrder[i].IsVisible && !drawOrder[i].drawTopMost)
                //    {
                //        return true;
                //    }
                //}
                //return false;

                return true;
            }
        }
        public virtual RECT ViewClip
        {
            get
            {
                if (needUpdateLocalToWorld)
                {
                    var temp = finalClip;
                    temp.Width = Width;
                    temp.Height = Height;
                    return temp;
                }
                return finalClip;
            }
        }
        public RECT FinalViewClip
        {
            get { return finalViewClip; }
        }
        public virtual EUIType UIType { get { return EUIType.UIElement; } }

        public UIElement()
        {
            if (UIStyle.Style != null)
            {
                UIStyle.Style.FitStyle(this);
            }
            RegistEvent(DoEnter);
            RegistEvent(DoMove);
            RegistEvent(DoExit);
            RegistEvent(DoHover);
            RegistEvent(DoUnHover);
            RegistEvent(DoDoubleClick);
            RegistEvent(DoClick);
            RegistEvent(DoPressed);
            RegistEvent(DoDrag);
            RegistEvent(DoClicked);
            RegistEvent(DoReleased);
            RegistEvent(DoKeyboard);
        }

        public void Update(Entry e)
        {
            UpdateLocalToWorld();

            if (!IsEnable)
                return;

            OnUpdateBegin(e);

            InternalUpdate(e);
            for (int i = Childs.Count - 1; i >= 0 && i < Childs.Count; i--)
            {
                //if (Handled)
                //    break;
                if (!Childs[i].IsScene || ((UIScene)Childs[i]).Entry == null)
                    Childs[i].Update(e);
            }

            OnUpdateEnd(e);
        }
        public void Event(Entry e)
        {
            UpdateLocalToWorld();

            var pointer = e.INPUT.Pointer;
            UpdateHoverState(pointer);

            if (!isClick)
            {
                isClick = isHover && pointer.IsClick(pointer.DefaultKey);
            }

            OnEventBegin(e);

            for (int i = Childs.Count - 1; i >= 0 && i < Childs.Count; i--)
            {
                //if (Handled)
                //    break;
                if (!Childs[i].IsScene || ((UIScene)Childs[i]).Entry == null)
                    Childs[i].Event(e);
            }

            if (FinalEventable && !Handled)
            {
                for (int i = 0; i < events.Count; i++)
                {
                    if (Handled)
                        break;
                    events[i](e);
                }
                if (!Handled)
                    InternalEvent(e);
            }

            OnEventEnd(e);

            if (isClick)
            {
                //if ((__HandledElement != null && __HandledElement != this) || (__PrevHandledElement != null && __PrevHandledElement != this))
                //{
                //    isClick = false;
                //}
                //else
                {
                    isClick = pointer.IsPressed(pointer.DefaultKey) ||
                        // Invoke "IsClick" is true in parent InternalEvent
                        pointer.IsRelease(pointer.DefaultKey);
                }
            }

            needUpdateHover = true;
        }
        public void Draw(GRAPHICS spriteBatch, Entry e)
        {
            UpdateLocalToWorld();
            isTopMost = false;

            if (!IsVisible)
                return;

            UpdateSort();
            UpdateContent();

            if (DrawBeforeBegin != null)
            {
                DrawBeforeBegin(this, spriteBatch, e);
            }

            DrawBegin(spriteBatch, ref model, ref finalViewClip, Shader);

            if (DrawAfterBegin != null)
            {
                DrawAfterBegin(this, spriteBatch, e);
            }

            InternalDraw(spriteBatch, e);
            if (DrawBeforeChilds != null)
            {
                DrawBeforeChilds(this, spriteBatch, e);
            }
            if (NeedDrawChild)
            {
                for (int i = 0; i < drawOrder.Length; i++)
                {
                    if (drawOrder[i].isTopMost)
                        continue;
                    if (drawOrder[i].IsScene)
                    {
                        var scene = (UIScene)drawOrder[i];
                        if (!(scene.Entry == null || (!scene.DrawState && scene.IsDrawable)))
                            continue;
                    }
                    drawOrder[i].Draw(spriteBatch, e);
                    //var scene = drawOrder[i] as UIScene;
                    //if (scene == null || scene.Entry == null || (!scene.DrawState && scene.IsDrawable))
                    //    drawOrder[i].Draw(spriteBatch, e);
                }
            }
            InternalDrawAfter(spriteBatch, e);
			if (DrawBeforeEnd != null)
			{
				DrawBeforeEnd(this, spriteBatch, e);
			}

            DrawEnd(spriteBatch, ref model, ref finalViewClip, Shader);

            if (DrawAfterEnd != null)
            {
                DrawAfterEnd(this, spriteBatch, e);
				if (DrawFocus != null)
				{
					DrawFocus(this, spriteBatch, e);
				}
            }
        }
        protected virtual void OnUpdateBegin(Entry e)
        {
            if (UpdateBegin != null)
            {
                UpdateBegin(this, e);
            }
        }
        protected virtual void OnUpdateEnd(Entry e)
        {
            if (UpdateEnd != null)
            {
                UpdateEnd(this, e);
            }
        }
        protected virtual void OnEventBegin(Entry e)
        {
            if (EventBegin != null)
            {
                EventBegin(this, e);
            }
        }
        protected virtual void OnEventEnd(Entry e)
        {
            if (EventEnd != null && !Handled)
            {
                EventEnd(this, e);
            }
        }
		protected virtual void DrawBegin(GRAPHICS spriteBatch, ref MATRIX2x3 transform, ref RECT view, SHADER shader)
        {
        }
		protected virtual void DrawEnd(GRAPHICS spriteBatch, ref MATRIX2x3 transform, ref RECT view, SHADER shader)
        {
        }
        private void UpdateSort()
        {
            if (needSort)
            {
                drawOrder = Childs.ToArray();
				Utility.SortOrderAsc(drawOrder, e => e.sortZ);
                needSort = false;
                OnUpdateSort(drawOrder);
            }
        }
        protected virtual void OnUpdateSort(UIElement[] drawOrder)
        {
        }
        internal void UpdateHoverState(IPointer pointer)
        {
            if (needUpdateHover)
            {
                if (!pointer.Position.IsNaN())
                {
                    isHover = IsContains(pointer.Position);
                }
                else if (!pointer.PositionPrevious.IsNaN())
                {
                    isHover = IsContains(pointer.PositionPrevious);
                }
                else
                {
                    isHover = false;
                }
                needUpdateHover = false;
            }
        }
        private void UpdateLocalToWorld()
        {
            UpdateContent();
            if (NeedUpdateLocalToWorld)
            {
                if (Parent != null)
                    Parent.UpdateLocalToWorld();

                UpdateTranslation(ref model);

                if (Parent != null)
                    world = model * Parent.world;
                else
                    world = model;

                UpdateTransformEnd(ref model, ref world);

                UpdateClip(ref finalClip, ref finalViewClip, ref model, ref world);

				MATRIX2x3.Invert(ref world, out worldInvert);
                needUpdateLocalToWorld = false;
            }
        }
		protected virtual void UpdateTranslation(ref MATRIX2x3 transform)
        {
            transform.M31 = X;
            transform.M32 = Y;

            float pivotX = PivotAlignmentX * Width * 0.5f;
            float pivotY = PivotAlignmentY * Height * 0.5f;
            transform.M31 -= transform.M11 * pivotX + transform.M21 * pivotY;
            transform.M32 -= transform.M12 * pivotX + transform.M22 * pivotY;
        }
		protected virtual void UpdateClip(ref RECT finalClip, ref RECT finalViewClip, ref MATRIX2x3 transform, ref MATRIX2x3 localToWorld)
        {
            RECT clip = Clip;
            finalClip = clip;
            finalClip.X = localToWorld.M31;
            finalClip.Y = localToWorld.M32;

            bool scissor = Parent != null && Parent.isClip;
            finalViewClip = finalClip;
            if (scissor)
            {
				RECT.Intersect(ref finalViewClip, ref Parent.finalViewClip, out finalViewClip);
            }
        }
		protected virtual void UpdateTransformEnd(ref MATRIX2x3 transform, ref MATRIX2x3 localToWorld)
        {
        }
        public void ResetContentSize()
        {
            contentSize.X = 0;
            contentSize.Y = 0;
        }
        private void UpdateContent()
        {
            if (IsNeedUpdateContent())
            {
                VECTOR2 size = ContentSize;
                if (contentSize.X != size.X || contentSize.Y != size.Y)
                {
                    NeedUpdateLocalToWorld = true;
                    contentSize.X = size.X;
                    contentSize.Y = size.Y;
                    if (ContentSizeChanged != null)
                    {
                        ContentSizeChanged(size);
                    }
                }
            }
        }
        protected virtual bool IsNeedUpdateContent()
        {
            return IsAutoClip;
        }
        protected virtual void InternalUpdate(Entry e)
        {
        }
        protected virtual void InternalEvent(Entry e)
        {
        }
        protected virtual void InternalDraw(GRAPHICS spriteBatch, Entry e)
        {
        }
        protected virtual void InternalDrawAfter(GRAPHICS spriteBatch, Entry e)
        {
        }
        public void DrawTopMost()
        {
            DrawTopMost(Scene);
        }
        public void DrawTopMost(UIScene scene)
        {
            if (isTopMost || scene == this || scene == null)
                return;
            isTopMost = true;
            scene.TopMost.Enqueue(this);
        }
        public bool SkipDraw()
        {
            UIScene top = Scene;
            if (top == this || top == null)
                return false;

            if (isTopMost && top.TopMost.Count > 0)
            {
                UIElement peek = top.TopMost.Peek();
                if (peek == this)
                {
                    top.TopMost.Dequeue();
                }
                else
                {
                    throw new InvalidOperationException("SkipDraw should be called before DrawTopMost and can't invoke repeated");
                }
            }
            else
            {
                isTopMost = true;
            }
            return isTopMost;
        }
        public virtual void ToFront()
        {
            if (Parent == null)
                return;

            if (Parent.Childs.Remove(this))
            {
                Parent.Childs.Add(this);
                needSort = true;
            }
        }
        public virtual void ToBack()
        {
            if (Parent == null)
                return;

            if (Parent.Childs.Remove(this))
            {
                Parent.Childs.Insert(0, this);
                needSort = true;
            }
        }

        public void AddChildFirst(UIElement node)
        {
            Insert(node, 0);
        }
        protected override bool CheckAdd(UIElement node)
        {
            return Childs.IndexOf(node) == -1;
        }
        protected override void OnAdded(UIElement node, int index)
        {
            //child.UpdateLocalToWorld();
            needSort = true;
            NeedUpdateLocalToWorld = true;
        }
        protected override void OnRemoved(UIElement node)
        {
            needSort = true;
            NeedUpdateLocalToWorld = true;
        }
        protected void InsertChildBefore(UIElement element, UIElement target)
        {
            int index = Childs.IndexOf(target);
            if (index == -1)
                Insert(element, 0);
            else
                Insert(element, index);
        }
        protected void InsertChildAfter(UIElement element, UIElement target)
        {
            int index = Childs.IndexOf(target);
            if (index == -1)
                Add(element);
            else
                Insert(element, index + 1);
        }
        private void UpdateWidth(float srcWidth, float dstWidth)
        {
            // UI编辑器 -> 读取TabPage.Page -> 读取TabPage.Parent.Clip -> 导致Page尺寸拉大
            if (srcWidth == 0)
                return;

            float add = dstWidth - srcWidth;
            float mul = srcWidth == 0 ? 0 : dstWidth / srcWidth;
            for (int i = 0; i < Childs.Count; i++)
            {
                var child = Childs[i];
                bool left = (child.Anchor & EAnchor.Left) == EAnchor.Left;
                bool right = (child.Anchor & EAnchor.Right) == EAnchor.Right;
                bool center = (child.Anchor & EAnchor.Center) == EAnchor.Center;

                if (center)
                {
                    if (left && right)
                    {
                        child.X *= mul;
                        child.Width *= mul;
                    }
                    else if (right)
                    {
                        child.X = (child.X + child.Width) * mul - child.Width;
                        //child.X = child.X * mul + child.Width * (mul - 1);
                    }
                    else
                    {
                        child.X *= mul;
                    }
                }
                else
                {
                    if (left && right)
                    {
                        child.Width += add;
                    }
                    else if (right)
                    {
                        child.X += add;
                    }
                }
            }
        }
        private void UpdateHeight(float srcHeight, float dstHeight)
        {
            if (srcHeight == 0)
                return;

            float add = dstHeight - srcHeight;
            float mul = srcHeight == 0 ? 0 : dstHeight / srcHeight;
            for (int i = 0; i < Childs.Count; i++)
            {
                var child = Childs[i];
                bool top = (child.Anchor & EAnchor.Top) == EAnchor.Top;
                bool bottom = (child.Anchor & EAnchor.Bottom) == EAnchor.Bottom;
                bool middle = (child.Anchor & EAnchor.Middle) == EAnchor.Middle;

                if (middle)
                {
                    if (top && bottom)
                    {
                        child.Y *= mul;
                        child.Height *= mul;
                    }
                    else if (bottom)
                    {
                        child.Y = (child.Y + child.Height) * mul - child.Height;
                        //child.Y = child.Y * mul + child.Height * (mul - 1);
                    }
                    else
                    {
                        child.Y *= mul;
                    }
                }
                else
                {
                    if (top && bottom)
                    {
                        child.Height += add;
                    }
                    else if (bottom)
                    {
                        child.Y += add;
                    }
                }
            }
        }
        public virtual bool IsContains(VECTOR2 graphicsPosition)
        {
            return InParentClip.Contains(ConvertGraphicsToLocalView(graphicsPosition));
        }
        public RECT InParent(RECT clip)
        {
            clip.X -= PivotAlignmentX * Width * 0.5f;
            clip.Y -= PivotAlignmentY * Height * 0.5f;
            if (!needUpdateLocalToWorld && (clip.X != model.M31 || clip.Y != model.M32))
            {
                clip.X = model.M31;
                clip.Y = model.M32;
            }
            return clip;
        }
        public VECTOR2 ConvertGraphicsToLocalView(VECTOR2 point)
        {
            if (Parent != null)
            {
                if (Parent.isClip && !Parent.isHover)
                {
                    point = VECTOR2.NaN;
                }
                else
                {
                    point = Parent.ConvertGraphicsToLocal(point);
                }
            }
            return point;
        }
        public VECTOR2 ConvertGraphicsToLocal(VECTOR2 point)
        {
            VECTOR2.Transform(ref point, ref worldInvert);
			return point;
        }
        public VECTOR2 ConvertLocalToGraphics(VECTOR2 point)
        {
            VECTOR2.Transform(ref point, ref world);
			return point;
        }
        public VECTOR2 ConvertLocalToOther(VECTOR2 point, UIElement other)
        {
            VECTOR2 result;
            result = ConvertLocalToGraphics(point);
            if (other != null)
                result = other.ConvertGraphicsToLocal(result);
            return result;
        }
        public bool SetFocus(bool focus)
        {
            if (focus)
            {
                if (FocusedElement != null && FocusedElement != this)
                {
                    //focusedElement.OnBlur();
                    FocusedElement.SetFocus(false);
                }
                if (!CanFocused)
                {
                    return false;
                }
                FocusedElement = this;
                this.OnFocus();
                return true;
            }
            else
            {
                if (Focused)
                {
                    FocusedElement = null;
                    OnBlur();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public virtual void Dispose()
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                Childs[i].Dispose();
            }
        }

        protected void RegistEvent(Action<Entry> e)
        {
            if (e == null)
                throw new ArgumentNullException("Event");
            events.Add(e);
        }
        protected bool OnEnter(Entry e)
        {
            return isHover && !IsContains(e.INPUT.Pointer.PositionPrevious);
        }
        private void DoEnter(Entry e)
        {
            if (GlobalEnter == null && Enter == null) return;
            bool enter = OnEnter(e);
            if (Enter != null && enter)
            {
                Enter(this, e);
            }
            if (GlobalEnter != null && enter)
            {
                GlobalEnter(this, Enter != null, e);
            }
        }
        protected bool OnMove(Entry e)
        {
            if (isHover)
            {
                VECTOR2 moved = e.INPUT.Pointer.DeltaPosition;
                if (!moved.IsNaN() && moved.X != 0 && moved.Y != 0)
                {
                    return true;
                }
            }
            return false;
        }
        private void DoMove(Entry e)
        {
            if (Move != null && OnMove(e))
            {
                Move(this, e);
            }
        }
        protected bool OnExit(Entry e)
        {
            return !isHover && IsContains(e.INPUT.Pointer.PositionPrevious);
        }
        private void DoExit(Entry e)
        {
            if (Exit != null && OnExit(e))
            {
                Exit(this, e);
            }
        }
        protected virtual void OnFocus()
        {
            if (Focus != null)
            {
                Focus(this, Entry.Instance);
            }
        }
        protected virtual void OnBlur()
        {
            if (Blur != null)
            {
                Blur(this, Entry.Instance);
            }
        }
        private void DoHover(Entry e)
        {
            if (Hover != null && isHover)
            {
                Hover(this, e);
            }
            if (GlobalHover != null && isHover)
            {
                GlobalHover(this, Hover != null, e);
            }
        }
        private void DoUnHover(Entry e)
        {
            if (UnHover != null && !isHover)
            {
                UnHover(this, e);
            }
            if (GlobalUnHover != null && !isHover)
            {
                GlobalUnHover(this, UnHover != null, e);
            }
        }
        protected bool OnClick(Entry e)
        {
            return isHover && e.INPUT.Pointer.IsClick(e.INPUT.Pointer.DefaultKey);
        }
        private void DoClick(Entry e)
        {
            if (Focused && e.INPUT.Pointer.IsClick(e.INPUT.Pointer.DefaultKey) && !isHover)
            {
                SetFocus(false);
            }
            if (Click == null && GlobalClick == null) return;
            bool flag = OnClick(e);
            if (Click != null && flag)
            {
                Click(this, e);
                Handle();
            }
            if (GlobalClick != null && flag)
            {
                GlobalClick(this, Click != null, e);
                Handle();
            }
        }
        protected bool OnPressed(Entry e)
        {
            return isClick && isHover && e.INPUT.Pointer.IsPressed(e.INPUT.Pointer.DefaultKey);
        }
        private void DoPressed(Entry e)
        {
            if (Pressed != null && OnPressed(e))
            {
                Pressed(this, e);
            }
        }
        protected bool OnDrag(Entry e)
        {
            return isClick && e.INPUT.Pointer.IsPressed(e.INPUT.Pointer.DefaultKey);
        }
        private void DoDrag(Entry e)
        {
            if (Drag != null && OnDrag(e))
            {
                Drag(this, e);
            }
        }
        protected bool OnClicked(Entry e)
        {
            return isHover && isClick && e.INPUT.Pointer.IsRelease(e.INPUT.Pointer.DefaultKey);
        }
        private void DoClicked(Entry e)
        {
            if (Clicked == null && GlobalClicked == null) return;
            bool flag = OnClicked(e);
            if (Clicked != null && flag)
            {
                Clicked(this, e);
                Handle();
            }
            if (GlobalClicked != null && flag)
            {
                GlobalClicked(this, Clicked != null, e);
                Handle();
            }
        }
        protected bool OnReleased(Entry e)
        {
            return isHover && e.INPUT.Pointer.IsRelease(e.INPUT.Pointer.DefaultKey);
        }
        private void DoReleased(Entry e)
        {
            if (Released != null && OnReleased(e))
            {
                Released(this, e);
                Handle();
            }
        }
        protected bool OnDoubleClick(Entry e)
        {
            return isHover && e.INPUT.Pointer.ComboClick.IsDoubleClick;
        }
        private void DoDoubleClick(Entry e)
        {
            if (DoubleClick != null && OnDoubleClick(e))
            {
                DoubleClick(this, e);
                Handle();
            }
        }
        protected bool OnKeyboard(Entry e)
        {
            return e.INPUT.Keyboard != null && e.INPUT.Keyboard.Focused;
        }
        private void DoKeyboard(Entry e)
        {
            if (Keyboard != null && OnKeyboard(e))
            {
                Keyboard(this, e);
            }
        }

        public static RECT CalcChildClip(UIElement parent, Func<UIElement, RECT> clipGenerator)
        {
            if (parent.Childs.Count == 0)
                return RECT.Empty;
            RECT clip = new RECT(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
            foreach (UIElement child in parent.Childs)
            {
                RECT temp = clipGenerator(child);
                if (temp.Width == 0 || temp.Height == 0)
                    continue;
                clip.X = _MATH.Min(temp.X, clip.X);
                clip.Y = _MATH.Min(temp.Y, clip.Y);
                clip.Width = _MATH.Max(temp.Right, clip.Width);
                clip.Height = _MATH.Max(temp.Bottom, clip.Height);
            }
            clip.Width = clip.Width - clip.X;
            clip.Height = clip.Height - clip.Y;
            return clip;
        }
        protected static RECT DefaultChildClip(UIElement child)
        {
            if (!child.Visible)
                return RECT.Empty;
            RECT rect = child.InParentClip;
            if (!child.isClip)
            {
                RECT clip = child.ChildClip;
                if (clip.X < 0)
                {
                    rect.X += clip.X;
                    rect.Width -= clip.X;
                }
                if (clip.Y < 0)
                {
                    rect.Y += clip.Y;
                    rect.Height -= clip.Y;
                }
                if (clip.Right > rect.Width)
                {
                    rect.Width = clip.Right;
                }
                if (clip.Bottom > rect.Height)
                {
                    rect.Height = clip.Bottom;
                }
            }
            return rect;
            //return RECT.Union(child.Clip, child.InParentChildClip);
        }
        public static VECTOR2 CalcPivotPoint(VECTOR2 size, EPivot pivot)
        {
            return new VECTOR2(
                Utility.EnumLow4((int)pivot) * size.X * 0.5f,
                Utility.EnumHigh4((int)pivot) * size.Y * 0.5f);
        }
        public static int PivotX(EPivot pivot)
        {
            return Utility.EnumLow4((int)pivot);
        }
        public static int PivotY(EPivot pivot)
        {
            return Utility.EnumHigh4((int)pivot);
        }
        public static VECTOR2 TextAlign(RECT bound, VECTOR2 textSize, EPivot alignment)
        {
            VECTOR2 location = bound.Location;
            location = VECTOR2.Add(location, CalcPivotPoint(bound.Size, alignment));
            location = VECTOR2.Subtract(location, CalcPivotPoint(textSize, alignment));
            return location;
        }
        public static UIElement FindElementByPosition(UIElement Parent, VECTOR2 screenPosition)
        {
            return FindChildPriority(Parent, e => !e.IsVisible, e => e.IsContains(screenPosition));
        }
        public static bool FindSkipInvisible(UIElement target)
        {
            return !target.Visible;
        }
        public static bool FindSkipUnhover(UIElement target)
        {
            return !target.isHover;
        }
    }

    /// <summary>
    /// 流程状态
    /// 1. Ending & Loading同时进行
    /// 2. 所有Ending结束，Loading完成的菜单率先进入Preparing，Preparing需要进行绘制，但不进行更新
    /// 3. 所有Preparing结束，进入Showing
    /// 4. 所有Beginning结束，进入Running
    /// 
    /// 流程状态2
    /// Ending: Update, Draw
    /// Loading:
    /// Preparing: Draw
    /// Showing: Update, Draw
    /// Running: Event, Update, Draw
    /// </summary>
    public enum EPhase
    {
        None,
        Ending,
        Loading,
        Preparing,
        Prepared,
        Showing,
        Running,
    }
    /// <summary>
    /// 场景更新的状态
    /// 
    /// 参数
    ///     None:
    ///         继续更新
    ///         
    ///     Dialog:
    ///         对话框，不更新其它场景事件
    ///         
    ///     Block:
    ///         对话框，完全跳过其它场景更新
    ///         
    ///     Cover:
    ///         遮罩，覆盖除主菜单外的所有自己下面的对话框，使其不绘制也不更新
    ///         
    ///     CoverAll:
    ///         完全跳过其它菜单的绘制和更新
    ///         
    ///     Break:
    ///         可以移除此场景
    ///         
    ///		Dispose:
    ///			移除场景并释放资源
    ///			
    ///		Release:
    ///			移除场景并释放资源及其在Entry内的缓存
    /// </summary>
    [Code(ECode.ToBeContinue)]
    public enum EState
    {
        None,
        Dialog,
        Block,
        Break,
        Dispose,
        Release,
        Cover,
        CoverAll,
    }
    public enum EContent
    {
        New,
        Inherit,
        System
    }
    /// <summary>
    /// 只保留居中和自定义
    /// </summary>
    [Code(ECode.MayBeReform)]
    public enum EShowPosition
    {
        Default,
        ParentCenter,
        [Obsolete]
        GraphicsCenter,
    }
    public class UIScene : Panel, IDisposable
    {
        internal EPhase Phase;
        internal COROUTINE Phasing;
        public EState State = EState.None;
        public EContent ContentType = EContent.Inherit;
        public EShowPosition ShowPosition;
        public PCKeys FocusNextKey = PCKeys.Tab;
        public event Action<UIScene, ContentManager> PhaseLoading;
        public event Action<UIScene> PhasePreparing;
        public event Action<UIScene> PhasePrepared;
        public event Action<UIScene> PhaseShowing;
        public event Action<UIScene> PhaseShown;
        public event Action<UIScene> PhaseEnding;
        public event Action<UIScene> PhaseEnded;
        public event Action<UIScene, ContentManager> LoadCompleted;
        internal Queue<UIElement> TopMost = new Queue<UIElement>();
        private List<AsyncLoadContent> loadings = new List<AsyncLoadContent>();
        internal bool IsDrawable;

        internal override bool IsScene
        {
            get { return true; }
        }
        internal bool DrawState
        {
            get { return State != EState.Break && State != EState.Dispose && State != EState.Release; }
        }
        public Entry Entry
        {
            get;
            internal set;
        }
        public ContentManager Content
        {
            get;
            protected set;
        }
        public bool IsDisposed
        {
            get
            {
                if (Content == null || Content.IsDisposed)
                {
					return true;
                }
                else
                {
					if (ContentType == EContent.Inherit && Parent != null && Parent.Scene != null)
                    {
                        return Parent.Scene.IsDisposed;
                    }
                    return false;
                }
            }
        }
        protected IEnumerable<AsyncLoadContent> Loadings
        {
            get { return loadings.Enumerable(); }
        }
        public EPhase RunningState
        {
            get { return Phase; }
        }
        public bool IsInStage
        {
            get { return Entry != null; }
        }
        public override EUIType UIType
        {
            get { return EUIType.UIScene; }
        }

        public UIScene()
        {
            if (Entry._GRAPHICS == null)
                this.Size = new VECTOR2(1280, 720);
            else
                this.Size = Entry._GRAPHICS.GraphicsSize;
            this.Keyboard += DoKeyboard;
        }
        public UIScene(string name)
            : this()
        {
            this.Name = name;
        }

        internal void SetPhase(IEnumerable<ICoroutine> coroutine)
        {
            if (Phasing != null)
                Phasing.Dispose();
            if (coroutine == null)
                Phasing = null;
            else
                Phasing = new COROUTINE(coroutine);
        }
        internal void OnPhaseLoading()
        {
            Phase = EPhase.Loading;
            SetPhase(Loading());
            if (PhaseLoading != null)
                PhaseLoading(this, Content);
        }
        internal void OnLoadCompleted()
        {
            if (loadings.Count == 0)
                if (LoadCompleted != null)
                    LoadCompleted(this, Content);
        }
        internal void OnPhasePreparing()
        {
            OnLoadCompleted();
            Phase = EPhase.Preparing;
            SetPhase(Preparing());
            if (PhasePreparing != null)
                PhasePreparing(this);
        }
        internal void OnPhasePrepared()
        {
            Phase = EPhase.Prepared;
            if (PhasePrepared != null)
                PhasePrepared(this);
        }
        /// <summary>
        /// Scene进入到Entry
        /// </summary>
        /// <param name="previous">切换菜单则是前一个主菜单，二级菜单则为当前主菜单</param>
        internal void OnPhaseShowing()
        {
            Phase = EPhase.Showing;
            SetPhase(Showing());
            if (PhaseShowing != null)
                PhaseShowing(this);
        }
        internal void OnPhaseShown()
        {
            Phase = EPhase.Running;
            SetPhase(Running());
            if (PhaseShown != null)
                PhaseShown(this);
        }
        internal void OnPhaseEnding()
        {
            Phase = EPhase.Ending;
            SetPhase(Ending());
            if (PhaseEnding != null)
                PhaseEnding(this);
        }
        /// <summary>
        /// Scene从Entry移除
        /// </summary>
        /// <param name="next">换菜单则是即将切换到的菜单，否则为null</param>
        internal void OnPhaseEnded()
        {
            Phase = EPhase.None;
            if (PhaseEnded != null)
                PhaseEnded(this);
            Entry = null;
        }

        protected internal virtual IEnumerable<ICoroutine> Ending()
        {
            return null;
        }
        protected internal virtual IEnumerable<ICoroutine> Loading()
        {
            return null;
        }
        protected internal virtual IEnumerable<ICoroutine> Preparing()
        {
            return null;
        }
        protected internal virtual IEnumerable<ICoroutine> Showing()
        {
            return null;
        }
        protected internal virtual IEnumerable<ICoroutine> Running()
        {
            return null;
        }
        /// <summary>
        /// <para>异步加载协程，在Load中调用</para>
        /// <para>1. LoadAsync 完全不阻断协程</para>
        /// <para>2. yield return LoadAsync 阻断协程直到异步加载完成，可以自定义ICoroutine来实现加载条</para>
        /// </summary>
        /// <param name="async">异步加载状态</param>
        /// <returns>阻断协程</returns>
        protected virtual ICoroutine LoadAsync(AsyncLoadContent async)
        {
            if (!async.IsEnd)
            {
                loadings.Add(async);
                return async;
            }
            return null;
        }
        internal void Show(Entry entry)
        {
            this.Entry = entry;

            SetPhase(null);

            switch (ShowPosition)
            {
                case EShowPosition.ParentCenter:
                    Pivot = EPivot.MiddleCenter;
                    if (Parent != null)
                        Location = Parent.Size * 0.5f;
                    else
                    {
                        //goto case EShowPosition.GraphicsCenter;
                        Pivot = EPivot.MiddleCenter;
                        Location = Entry.GRAPHICS.GraphicsSize * 0.5f;
                    }
                    break;

                case EShowPosition.GraphicsCenter:
                    Pivot = EPivot.MiddleCenter;
                    Location = Entry.GRAPHICS.GraphicsSize * 0.5f;
                    break;
            }

            if (IsDisposed)
            {
                if (ContentType == EContent.Inherit)
                {
                    // inherit from parent scene
                    if (Parent != null && Parent.Scene != null)
                    {
                        Content = Parent.Scene.Content;
                    }

                    // inherit from current main scene
                    if (Content == null && Entry.Scene != null)
                    {
                        Content = Entry.Scene.Content;
                    }
                }
                else if (ContentType == EContent.System)
                {
                    if (entry.ContentManager != null)
                    {
                        Content = entry.ContentManager;
                    }
                }

                if (Content == null)
                {
                    Content = entry.NewContentManager();
                }
            }
        }
        public void Close(bool immediately)
        {
            Close(State, immediately);
        }
        public void Close(EState state, bool immediately)
        {
            if (Entry == null)
                return;

            if (immediately)
                Entry.CloseImmediately(this, state);
            else
                Entry.Close(this, state);
        }
        /// <summary>
        /// 场景在其它场景里时，被Remove或Clear时需要关闭此场景
        /// </summary>
        protected override void OnRemovedBy(UIElement parent)
        {
            base.OnRemovedBy(parent);
            Close(true);
        }
        public override void ToFront()
        {
            if (Entry == null)
                base.ToFront();
            else
                Entry.ToFront(this);
        }
        public override void ToBack()
        {
            if (Entry == null)
                base.ToBack();
            else
                Entry.ToBack(this);
        }
        protected override void InternalUpdate(Entry e)
        {
            if (loadings.Count > 0)
            {
                loadings = loadings.Where(l => !l.IsEnd).ToList();
                OnLoadCompleted();
            }
            base.InternalUpdate(e);
        }
        private  void DoKeyboard(UIElement sender, Entry e)
        {
            if (Parent == null && IsInStage && Entry.Scene == this && e.INPUT.Keyboard.IsClick(FocusNextKey))
            {
                UIElement next = FocusedElement;
                if (next == null)
                    // 第一个可以设置焦点的控件
                    next = NextFocusedElement;
                else
                    // 当前焦点的下一个焦点控件
                    next = next.NextFocusedElement;

                if (next != null)
                    next.SetFocus(true);
                else if (FocusedElement != null)
                    // 最后的焦点控件后设置为无焦点
                    FocusedElement.SetFocus(false);
            }
        }
		protected override void DrawEnd(GRAPHICS spriteBatch, ref MATRIX2x3 transform, ref RECT view, SHADER shader)
        {
            while (TopMost.Count > 0)
                TopMost.Dequeue().Draw(spriteBatch, Entry.Instance);

            base.DrawEnd(spriteBatch, ref transform, ref view, shader);
        }
        public override void Dispose()
        {
            foreach (var loading in loadings)
                if (!loading.IsEnd)
                    loading.Cancel();
            loadings.Clear();
            base.Dispose();
            if (Content != null && Content != Entry.Instance.ContentManager)
            {
                Content.Dispose();
                Content = null;
            }
            State = EState.Dispose;
            SetPhase(null);
            TopMost.Clear();
        }
    }

    public class UIText
    {
        public string Text = "";
        public FONT Font = FONT.Default;
        public COLOR FontColor = COLOR.Default;
        public EPivot TextAlignment;
        public TextShader TextShader;
        public VECTOR2 Padding;
        public float Scale = 1f;

        public float FontSize
        {
            get { return Font == null ? 0 : Font.FontSize; }
            set { if (Font != null) Font.FontSize = value; }
        }

        public void GetPaddingClip(ref RECT rect)
        {
            int x = UIElement.PivotX(TextAlignment);
            int y = UIElement.PivotY(TextAlignment);
            rect.X += Padding.X * 0.5f;
            rect.Width -= Padding.X;
            rect.Y += Padding.Y * 0.5f;
            rect.Height -= Padding.Y;
        }
        public void GetAlignmentClip(ref RECT rect, out float offsetX, out float offsetY)
        {
            int x = UIElement.PivotX(TextAlignment);
            int y = UIElement.PivotY(TextAlignment);
            VECTOR2 size = Font.MeasureString(Text);
            offsetX = (rect.Width - size.X) * 0.5f * x;
            offsetY = (rect.Height - size.Y) * 0.5f * y;
            rect.X += offsetX;
            rect.Y += offsetY;
            if (offsetX < 0)
                rect.Width += -offsetX * 2;
            if (offsetY < 0)
                rect.Height += -offsetY * 2;
        }
        public RECT GetTextClip(RECT rect)
        {
            int x = UIElement.PivotX(TextAlignment);
            int y = UIElement.PivotY(TextAlignment);
            rect.X += Padding.X * 0.5f;
            rect.Width -= Padding.X;
            rect.Y += Padding.Y * 0.5f;
            rect.Height -= Padding.Y;

            VECTOR2 size = Font.MeasureString(Text) * Scale;
            float offsetX = (rect.Width - size.X) * 0.5f * x;
            float offsetY = (rect.Height - size.Y) * 0.5f * y;
            rect.X += offsetX;
            rect.Y += offsetY;
            if (offsetX < 0)
                rect.Width += -offsetX * 2;
            if (offsetY < 0)
                rect.Height += -offsetY * 2;
            return rect;
        }
        public void Draw(GRAPHICS spriteBatch, RECT rect)
        {
            if (Font != null && !string.IsNullOrEmpty(Text))
            {
                VECTOR2 location = GetTextClip(rect).Location;
                bool effect = false;
                if (TextShader != null)
                {
                    FontTexture ft = Font as FontTexture;
                    if (ft == null)
                    {
                        if (TextShader.IsShader)
                        {
                            spriteBatch.Draw(Font, Text, VECTOR2.Add(location, TextShader.Offset), TextShader.Color, Scale);
                        }
                        // 不支持描边
                    }
                    else
                    {
                        ft.Effect = TextShader;
                    }
                }
                spriteBatch.Draw(Font, Text, location, FontColor, Scale);
                if (effect)
                {
                    ((FontTexture)Font).Effect = null;
                }
            }
        }
    }
}

#endif