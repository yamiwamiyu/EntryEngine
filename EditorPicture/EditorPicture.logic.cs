using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using EntryEditor;
using EntryEngine.Serialize;
using System.IO;
using System;
using System.Linq;

public partial class EditorPicture : SceneEditorEntry
{
    const string CONSTANT = "C.xml";

    PICTURE picture;
    MATRIX2x3 transform;
    MATRIX2x3 transformInvert;
    TEXTURE brush;
    bool hideGrid;
    PICTURE.Part drag;
    bool deleteBrushFlag;
    EPivot stretchPivot = EPivot.MiddleCenter;
    EPivot[] StretchPivots = new EPivot[]
    {
        EPivot.TopLeft,
        EPivot.TopCenter,
        EPivot.TopRight,
        EPivot.MiddleLeft,
        EPivot.MiddleRight,
        EPivot.BottomLeft,
        EPivot.BottomCenter,
        EPivot.BottomRight,
    };
    VECTOR2 stretchSize;

    public override string DirectoryProject
    {
        get { return _C.Directory; }
        protected set { _C.Directory = value; }
    }

    public EditorPicture()
    {
        Initialize();
        TBSizeWidth.TextChanged += new Action<Label>(TBSizeWidth_TextChanged);
        TBSizeHeight.TextChanged += new Action<Label>(TBSizeHeight_TextChanged);
    }
    void TBSizeHeight_TextChanged(Label obj)
    {
        int size;
        if (int.TryParse(obj.Text, out size))
            picture.Data.Height = size;
    }
    void TBSizeWidth_TextChanged(Label obj)
    {
        int size;
        if (int.TryParse(obj.Text, out size))
            picture.Data.Width = size;
    }
    
    private IEnumerable<ICoroutine> MyLoading()
    {
        if (File.Exists(CONSTANT))
            new XmlReader(File.ReadAllText(CONSTANT)).ReadStatic(typeof(_C));
        UtilityEditor.OnExit += () =>
        {
            File.WriteAllText(CONSTANT, new XmlWriter().WriteStatic(typeof(_C)));
        };

        Content = Entry.NewContentManager();
        Content.RootDirectory = _IO.DirectoryWithEnding(Path.GetFullPath(_C.Directory));
        if (!string.IsNullOrEmpty(_C.Directory))
            LDirectory.Text = "工作目录：" + Content.RootDirectory;

        LDirectory.Clicked += new DUpdate<UIElement>(LDirectory_Clicked);
        UtilityEditor.DragFiles = DragFile;

        SetTip();
        New();

        return null;
    }

    void DragFile(string[] files)
    {
        if (string.IsNullOrEmpty(_C.Directory))
        {
            STextHint.ShowHint("请先设定工作目录");
            return;
        }
        if (_IO.RelativePath(files[0], _C.Directory) == null)
        {
            STextHint.ShowHint("必须选择工作目录中的图片");
            return;
        }
        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            if (Path.GetExtension(file).EndsWith(PipelinePicture.FILE_TYPE))
            {
                picture = Content.Load<PICTURE>(_IO.RelativePath(file, Content.RootDirectory));
                OnSetPicture();
            }
            else
            {
                TEXTURE texture = Content.Load<TEXTURE>(_IO.RelativePath(file, Content.RootDirectory));
                // 放在鼠标位置
                //brush = texture;
                SetTexture(texture);
            }
            Handle();
            break;
        }
    }
    void LDirectory_Clicked(UIElement sender, Entry e)
    {
        string dir = _C.Directory;
        dir = UtilityEditor.OpenFolder(null, LDirectory.Text);
        if (!string.IsNullOrEmpty(dir))
        {
            dir = _IO.DirectoryWithEnding(dir);
            _C.Directory = _IO.RelativePath(dir, DirectoryEditor);
            LDirectory.Text = "工作目录：" + dir;
            Content.RootDirectory = dir;
        }
    }

    void New()
    {
        picture = new PICTURE();
        picture.Data = new PICTURE.Graphics();
        picture.Data.Width = 1280;
        picture.Data.Height = 720;
        picture.Data.Parts = new PICTURE.Part[10];
        OnSetPicture();
    }
    void OnSetPicture()
    {
        transform = MATRIX2x3.Identity;
        // 居中显示
        VECTOR2 size = Entry.GRAPHICS.GraphicsSize;
        transform.M31 = (int)((size.X - picture.Width) * 0.5f);
        transform.M32 = (int)((size.Y - picture.Height) * 0.5f);
        MATRIX2x3.Invert(ref transform, out transformInvert);
    }
    void SetTip()
    {
        var tip = Entry.ShowDialogScene<STip>(EState.None);
        tip.SetTip(LDirectory, "导入和保存的图片都应该在这个目录下，一般是项目的Content根目录");
        tip.SetTip(LSize, "完整图片的尺寸，缩放可以从图片四周选中锚点后拖拽缩放");
        tip.SetTip(LScale, "视口的缩放比例，通过鼠标滑轮控制，按下鼠标中建恢复100%");
        tip.SetTip(LPosition, "鼠标在图片中的坐标");
        tip.SetTip(LHelp,
@"N新建一张图片
Space+左键拖拽画布
拖拽图片文件到视口放入图片
Alt+左键点击已有图片则将图片作为笔刷，有笔刷则放置图片
左键点击已有图片并拖拽可以修改图片位置
左键点击没有图片的区域可以放置图片，按住左键拖拽可快速批量放置图片
Ctrl+左键可以无视网格放置图片
右键取消笔刷
没有笔刷右键拖拽可以快速删除图片
Ctrl+S保存
Ctrl+Shift+S另存为
Delete清空图片，保持原有图片大小
G显示/隐藏网格线");
    }
    RECT GetstretchArea(EPivot pivot)
    {
        int x = UIElement.PivotX(pivot);
        int y = UIElement.PivotY(pivot);
        RECT area;
        area.Width = 10;
        area.Height = 10;
        area.X = picture.Width * x * 0.5f - area.Width * x * 0.5f;
        area.Y = picture.Height * y * 0.5f - area.Height * y * 0.5f;
        return area;
    }
    VECTOR2 GetMouseInPicture()
    {
        VECTOR2 pos = __INPUT.Pointer.Position;
        VECTOR2.Transform(ref pos, ref transformInvert);
        return pos;
    }
    HashSet<int> TextureSizes
    {
        get
        {
            HashSet<int> sizes = new HashSet<int>();
            if (brush != null)
            {
                sizes.Add(brush.Width);
                sizes.Add(brush.Height);
            }
            for (int i = 0; i < picture.Data.Parts.Length; i++)
                if (picture.Data.Parts[i] == null)
                    break;
                else
                {
                    sizes.Add(picture.Data.Parts[i].Texture.Width);
                    sizes.Add(picture.Data.Parts[i].Texture.Height);
                }
            return sizes;
        }
    }
    int GridSize
    {
        get
        {
            var sizes = TextureSizes;
            if (sizes.Count == 0) return 1;
            return _MATH.MaxDivisor(sizes);
        }
    }
    bool GetMouseWillSet(ref VECTOR2 pos)
    {
        //if (pos.X < 0 || pos.Y < 0 || pos.X >= picture.Width || pos.Y >= picture.Height) return false;
        int grid = GridSize;
        if (grid != 1 && !__INPUT.Keyboard.Ctrl)
        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            pos.X = x / grid * grid;
            pos.Y = y / grid * grid;
        }
        return true;
    }
    void SetTexture(TEXTURE texture)
    {
        VECTOR2 pos = GetMouseInPicture();
        if (!GetMouseWillSet(ref pos)) return;
        // 通过计算最大公约数为一格计算来摆放图片
        int nullIndex = picture.Data.Parts.IndexOf(p => p == null);
        if (nullIndex == -1)
        {
            nullIndex = picture.Data.Parts.Length;
            Array.Resize(ref picture.Data.Parts, picture.Data.Parts.Length * 2);
        }
        picture.Data.Parts[nullIndex] = new PICTURE.Part()
        {
            Source = texture.Key,
            Texture = texture,
            X = (int)pos.X,
            Y = (int)pos.Y,
        };
    }
    PICTURE.Part GetMousePart()
    {
        VECTOR2 pos = GetMouseInPicture();
        if (pos.X < 0 || pos.Y < 0 || pos.X >= picture.Width || pos.Y >= picture.Height) return null;
        for (int i = picture.Data.Parts.Length - 1; i >= 0; i--)
        {
            var part = picture.Data.Parts[i];
            if (part == null)
                continue;
            else
            {
                if (new RECT(part.X, part.Y, part.Texture.Width, part.Texture.Height).Contains(pos))
                    return part;
            }
        }
        return null;
    }

    protected override void InternalEvent(Entry e)
    {
        base.InternalEvent(e);

        if (e.INPUT.Keyboard.IsClick(PCKeys.N))
            New();
        else if (e.INPUT.Keyboard.IsClick(PCKeys.Delete))
        {
            Content.Dispose();
            picture.Data.Parts = new PICTURE.Part[10];
        }
        else if (e.INPUT.Keyboard.Ctrl && e.INPUT.Keyboard.IsClick(PCKeys.S))
        {
            bool saveAs = e.INPUT.Keyboard.Shift;
            // 新建的图片默认另存为
            saveAs |= string.IsNullOrEmpty(picture.Key);
            string result = picture.Key;
            if (saveAs)
            {
                result = Content.RootDirectory + result;
                if (!UtilityEditor.SaveFile(ref result, PipelinePicture.FILE_TYPE))
                    return;
                result = _IO.RelativePath(result, Content.RootDirectory);
                if (result == null)
                {
                    STextHint.ShowHint("图片必须保存在工作目录中");
                    return;
                }
            }

            // 保存
            int nullIndex = picture.Data.Parts.IndexOf(p => p == null);
            if (nullIndex != -1)
                Array.Resize(ref picture.Data.Parts, nullIndex);
            Content.IODevice.WriteText(result, JsonWriter.Serialize(picture.Data));

            // 重新加载图片保证Ctrl+S时直接保存
            if (string.IsNullOrEmpty(picture.Key))
                picture = Content.Load<PICTURE>(result);
        }
        else if (e.INPUT.Keyboard.IsClick(PCKeys.G))
            hideGrid = !hideGrid;

        // 缩放
        if (e.INPUT.Mouse.ScrollWheelValue != 0)
        {
            VECTOR2 posPrev = GetMouseInPicture();
            //if (e.INPUT.Keyboard.Ctrl)
            //{
            //    // 缩放图片尺寸
            //    int[] sizes = new int[] { picture.Width, picture.Height };
            //    int d = _MATH.MaxDivisor(sizes);
            //    sizes[0] /= d;
            //    sizes[1] /= d;
            //    sizes[0] = sizes[0] * (int)(e.INPUT.Mouse.ScrollWheelValue * 1);
            //    sizes[1] = sizes[1] * (int)(e.INPUT.Mouse.ScrollWheelValue * 1);
            //    picture.Data.Width += sizes[0];
            //    picture.Data.Height += sizes[1];
            //    sizes[0] >>= 1;
            //    sizes[1] >>= 1;
            //    for (int i = 0; i < picture.Data.Parts.Length; i++)
            //    {
            //        if (picture.Data.Parts[i] == null) break;
            //        picture.Data.Parts[i].X -= sizes[0];
            //        picture.Data.Parts[i].Y -= sizes[1];
            //    }
            //}
            //else
            {
                // 缩放视图尺寸
                transform.M11 = _MATH.Clamp(transform.M11 + e.INPUT.Mouse.ScrollWheelValue * 0.1f, 0.1f, 4);
                transform.M22 = transform.M11;
                MATRIX2x3.Invert(ref transform, out transformInvert);
            }
            VECTOR2 posNext = GetMouseInPicture();
            transform.M31 += (posNext.X - posPrev.X) * transform.M11;
            transform.M32 += (posNext.Y - posPrev.Y) * transform.M11;
            MATRIX2x3.Invert(ref transform, out transformInvert);
        }
        // 取消缩放
        if (e.INPUT.Pointer.IsClick(2))
        {
            transform.M11 = 1;
            transform.M22 = 1;
            MATRIX2x3.Invert(ref transform, out transformInvert);
        }

        if (e.INPUT.Pointer.IsPressed(0))
        {
            // 拖拽地图
            if (e.INPUT.Keyboard.IsPressed(PCKeys.Space))
            {
                var move = e.INPUT.Pointer.DeltaPosition;
                transform.M31 += move.X;
                transform.M32 += move.Y;
                MATRIX2x3.Invert(ref transform, out transformInvert);
                return;
            }
        }

        if (e.INPUT.Pointer.IsPressed(0))
        {
            if (stretchPivot != EPivot.MiddleCenter)
            {
                // 拉升图片大小
                VECTOR2 previous = e.INPUT.Pointer.ClickPosition;
                VECTOR2.Transform(ref previous, ref transformInvert);
                VECTOR2 current = GetMouseInPicture();
                VECTOR2 delta = current - previous;
                int pivotX = UIElement.PivotX(stretchPivot) - 1;
                int pivotY = UIElement.PivotY(stretchPivot) - 1;
                int scaleX = (int)delta.X * pivotX;
                int scaleY = (int)delta.Y * pivotY;
                int width = picture.Data.Width;
                int height = picture.Data.Height;
                picture.Data.Width = (int)(stretchSize.X + scaleX);
                picture.Data.Height = (int)(stretchSize.Y + scaleY);
                scaleX = picture.Data.Width - width;
                scaleY = picture.Data.Height - height;
                if (pivotX == -1 || pivotY == -1)
                {
                    if (pivotX == -1)
                        transform.M31 -= scaleX * transform.M11;
                    if (pivotY == -1)
                        transform.M32 -= scaleY * transform.M11;
                    MATRIX2x3.Invert(ref transform, out transformInvert);
                    foreach (var part in picture.Data.Parts)
                    {
                        if (part == null) break;
                        if (pivotX == -1) part.X += scaleX;
                        if (pivotY == -1) part.Y += scaleY;
                    }
                }
            }
            else
            {
                if (e.INPUT.Pointer.IsClick(0))
                {
                    var pos = GetMouseInPicture();
                    for (int i = 0; i < StretchPivots.Length; i++)
                        if (GetstretchArea(StretchPivots[i]).Contains(pos))
                        {
                            stretchPivot = StretchPivots[i];
                            stretchSize = new VECTOR2(picture.Data.Width, picture.Data.Height);
                            _LOG.Debug("拉伸图片大小：{0}", stretchPivot);
                            return;
                        }
                }
            }

            if (drag != null)
            {
                // 移动拖拽的图块
                VECTOR2 pos = GetMouseInPicture();
                if (!GetMouseWillSet(ref pos)) return;
                drag.X = (int)pos.X;
                drag.Y = (int)pos.Y;
            }
            else
            {
                // 准备拖拽地形
                var part = GetMousePart();
                if (part == null)
                {
                    if (brush != null)
                    {
                        // 放置图片
                        SetTexture(brush);
                        _LOG.Debug("刷图块");
                    }
                }
                else
                {
                    if (e.INPUT.Pointer.IsClick(0))
                    {
                        if (e.INPUT.Keyboard.Alt)
                        {
                            if (brush == null)
                            {
                                // 设置笔刷
                                brush = part.Texture;
                                _LOG.Debug("提取笔刷");
                            }
                            else
                            {
                                // 叠加放置图片
                                SetTexture(brush);
                                _LOG.Debug("首次叠加刷图块");
                            }
                        }
                        else
                        {
                            // 拖拽图块
                            drag = part;
                            _LOG.Debug("拖拽图块");
                        }
                    }
                    else
                    {
                        if (e.INPUT.Keyboard.Alt || brush == null) return;
                        if (part.Texture != brush)
                        {
                            SetTexture(brush);
                            _LOG.Debug("叠加刷图块");
                        }
                    }
                }
            }
        }
        else
        {
            drag = null;
            stretchPivot = EPivot.MiddleCenter;
        }

        // 取消笔刷
        if (brush != null && e.INPUT.Pointer.IsClick(1))
        {
            brush = null;
            deleteBrushFlag = true;
            _LOG.Debug("取消笔刷");
        }
        else if (e.INPUT.Pointer.IsPressed(1))
        {
            if (!deleteBrushFlag)
            {
                // 删除地形
                var part = GetMousePart();
                if (part != null)
                {
                    picture.Data.Parts = picture.Data.Parts.Remove(p => p == part);
                    _LOG.Debug("删除图块：{0},{1} - {2}", part.X, part.Y, part.Source);
                }
            }
        }
        else
            deleteBrushFlag = false;
    }
    protected override void InternalUpdate(Entry e)
    {
        base.InternalUpdate(e);

        //LSize.Text = string.Format("画布尺寸：{0} x {1}", picture.Width, picture.Height);
        if (!TBSizeWidth.Focused) TBSizeWidth.Text = picture.Width.ToString();
        if (!TBSizeHeight.Focused) TBSizeHeight.Text = picture.Height.ToString();
        LScale.Text = string.Format("缩放：{0}%", Utility.LengthFloat(transform.M11 * 100, 2));
        var pos = GetMouseInPicture();
        LPosition.Text = string.Format("坐标：{0},{1}", (int)pos.X, (int)pos.Y);
    }
    protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
    {
        base.InternalDraw(spriteBatch, e);

        spriteBatch.Begin(transform);

        float one = 1 / transform.M11;

        // 绘制图片尺寸
        spriteBatch.Draw(TEXTURE.Pixel, new RECT(0, 0, picture.Width, one));
        spriteBatch.Draw(TEXTURE.Pixel, new RECT(0, picture.Height, picture.Width, one));
        spriteBatch.Draw(TEXTURE.Pixel, new RECT(0, 0, one, picture.Height));
        spriteBatch.Draw(TEXTURE.Pixel, new RECT(picture.Width, 0, one, picture.Height));

        // 网格线
        if (!hideGrid && (brush != null || drag != null) && !e.INPUT.Keyboard.Ctrl)
        {
            int grid = GridSize;
            if (grid != 1)
            {
                int width = picture.Width;
                int height = picture.Height;
                int pos = grid;
                COLOR gridColor = new COLOR(0, 255, 0, 128);
                while (pos < width)
                {
                    spriteBatch.Draw(TEXTURE.Pixel, new RECT(pos, 0, one, height), gridColor);
                    pos += grid;
                }
                pos = grid;
                while (pos < height)
                {
                    spriteBatch.Draw(TEXTURE.Pixel, new RECT(0, pos, width, one), gridColor);
                    pos += grid;
                }
            }
        }

        // 绘制图片
        spriteBatch.Draw(picture, VECTOR2.Zero);
        // 绘制画刷
        if (brush != null && drag == null)
        {
            VECTOR2 pos = GetMouseInPicture();
            if (GetMouseWillSet(ref pos))
                spriteBatch.Draw(brush, pos, new COLOR(255, 255, 255, 128));
        }

        // 绘制拉伸区域
        COLOR color = COLOR.Yellow;
        color.A = 196;
        for (int i = 0; i < StretchPivots.Length; i++)
            spriteBatch.Draw(TEXTURE.Pixel, GetstretchArea(StretchPivots[i]), color);
        //spriteBatch.Draw(TEXTURE.Pixel, GetStrengthArea(EPivot.TopLeft), color);
        //spriteBatch.Draw(TEXTURE.Pixel, GetStrengthArea(EPivot.TopCenter), color);
        //spriteBatch.Draw(TEXTURE.Pixel, GetStrengthArea(EPivot.TopRight), color);
        //spriteBatch.Draw(TEXTURE.Pixel, GetStrengthArea(EPivot.MiddleLeft), color);
        //spriteBatch.Draw(TEXTURE.Pixel, GetStrengthArea(EPivot.MiddleRight), color);
        //spriteBatch.Draw(TEXTURE.Pixel, GetStrengthArea(EPivot.BottomLeft), color);
        //spriteBatch.Draw(TEXTURE.Pixel, GetStrengthArea(EPivot.BottomCenter), color);
        //spriteBatch.Draw(TEXTURE.Pixel, GetStrengthArea(EPivot.BottomRight), color);

        spriteBatch.End();
    }
}
