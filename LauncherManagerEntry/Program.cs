using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Xna;
using EntryEngine;
using LauncherManager;
using System.Threading;
using EntryEngine.UI;

namespace LauncherManagerEntry
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                var wait = new AutoResetEvent(false);
                new Thread((obj) =>
                {
                    try
                    {
                        Run();
                    }
                    finally
                    {
                        wait.Set();
                    }
                })
                {
                    ApartmentState = ApartmentState.STA,
                    IsBackground = true,
                }.Start();
                wait.WaitOne();
            }
            else
            {
                Run();
            }
        }

        static void Run()
        {
            using (XnaGate gate = new XnaGate())
            {
                gate.BGColor = Microsoft.Xna.Framework.Graphics.Color.White;
                gate.OnInitialized += new Action<Entry>(gate_OnInitialized);
                gate.Run();
            }
        }
        static void gate_OnInitialized(Entry obj)
        {
            obj.GRAPHICS.ScreenSize = new VECTOR2(1280, 720);
            obj.GRAPHICS.GraphicsSize = obj.GRAPHICS.ScreenSize;
            FONT.Default = obj.NewFONT("黑体", 16);
            //obj.ShowMainScene(new SEntryPoint());
            obj.ShowMainScene<TestScene>();
        }
    }

    public class TestScene : UIScene
    {
        protected override IEnumerable<ICoroutine> Loading()
        {
            //this.Background = TEXTURE.Pixel;
            //this.Color = COLOR.Black;

            //Button _1圆角框_内部透明 = new Button();
            //_1圆角框_内部透明.Clip = new RECT(100, 100, 100, 40);
            ////_1圆角框_内部透明.SourceNormal = new PATCH(Content.Load<TEXTURE>("圆角框_内部透明.png"), new RECT(2, 3, 1, 1), COLOR.TransparentWhite, COLOR.White);
            //_1圆角框_内部透明.SourceNormal = PATCH.GetNinePatch(COLOR.TransparentWhite, new COLOR(220, 223, 230), 1);
            //_1圆角框_内部透明.SourceHover = PATCH.GetNinePatch(COLOR.TransparentWhite, new COLOR(64, 158, 255), 1);
            //_1圆角框_内部透明.SourceClick = PATCH.GetNinePatch(COLOR.TransparentWhite, new COLOR(58, 142, 230), 1);
            //_1圆角框_内部透明.Text = "默认按钮";
            //_1圆角框_内部透明.UIText.FontColor = new COLOR(96, 98, 102);
            //_1圆角框_内部透明.UnHover += (sender, e) =>
            //{
            //    _1圆角框_内部透明.UIText.FontColor = new COLOR(96, 98, 102);
            //};
            //_1圆角框_内部透明.Hover += (sender, e) =>
            //{
            //    if (sender.IsClick)
            //    {
            //        _1圆角框_内部透明.UIText.FontColor = new COLOR(58, 142, 230);
            //    }
            //    else
            //    {
            //        _1圆角框_内部透明.UIText.FontColor = new COLOR(64, 158, 255);
            //    }
            //};
            //Add(_1圆角框_内部透明);

            //this.Hover += (sender, e) =>
            //{
            //    var position = e.INPUT.Pointer.Position;
            //    _1圆角框_内部透明.Width = position.X - 100;
            //    _1圆角框_内部透明.Height = position.Y - 100;
            //};

            //UIStyle.Style = new UIStyle();
            //UIStyle.Style.AddStyle(EUIType.Button,
            //    (e) =>
            //    {
            //        Button item = (Button)e;
            //        UIStyle.StyleButtonPatchBody2(item, new COLOR(230, 162, 60), COLOR.White, 1);
            //    });

            #region Button
            //Button button = new Button();
            //button.X = 100;
            //button.Y = 100;
            //button.Text = "默认按钮";
            //StyleButtonPatchBorder(button, new COLOR(220, 223, 230), new COLOR(103, 194, 58), 1, new COLOR(96, 98, 102));
            //Add(button);

            //button = new Button();
            //button.X = 100;
            //button.Y = 160;
            //button.Text = "朴素按钮";
            //StyleButtonPatchBody(button, new COLOR(220, 223, 230), new COLOR(103, 194, 58), 1, new COLOR(96, 98, 102));
            //Add(button);

            //button = new Button();
            //button.X = 220;
            //button.Y = 100;
            //button.Text = "成功按钮";
            //StyleButtonPatchBody2(button, new COLOR(103, 194, 58), COLOR.White);
            //Add(button);

            //button = new Button();
            //button.X = 220;
            //button.Y = 160;
            //button.Text = "成功按钮";
            //StyleButtonPatchBody3(button, new COLOR(103, 194, 58), COLOR.White, 1);
            //Add(button);
            #endregion

            #region CheckBox

            CheckBox item = new CheckBox();
            item.X = 100;
            item.Y = 100;
            item.Text = "备选项";
            item.UIText.FontColor = COLOR.Black;
            StyleCheckBox(item, new COLOR(220, 223, 230), new COLOR(17, 131, 238));
            Add(item);

            #endregion

            return base.Loading();
        }
        protected override void InternalUpdate(Entry e)
        {
            base.InternalUpdate(e);
        }
        protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
        {
            base.InternalDraw(spriteBatch, e);
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
        public static string BuildBytesString(byte[] array)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('{');
            for (int i = 0; i < array.Length; i++)
            {
                if (i > 0)
                    builder.Append(',');
                builder.Append(array[i]);
            }
            builder.Append('}');
            return builder.ToString();
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
    }
}
