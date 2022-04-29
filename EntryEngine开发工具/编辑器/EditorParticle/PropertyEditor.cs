using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.UI;
using EntryEngine.Serialize;
using EntryEngine;
using System.Reflection;
using EntryEditor;

public abstract class EditMode
{
    public static FONT FONT;

    /// <summary>
    /// 在编辑器内进行自定义编辑操作
    /// </summary>
    /// <returns>是否操作完成</returns>
    protected internal abstract bool Edit(ref MATRIX2x3 matrix, GRAPHICS spriteBatch, Entry e);
    public void Cancel()
    {
        EditorParticle.edit = null;
    }
    public void Start()
    {
        EditorParticle.edit = this;
        InternalStart();
    }
    protected virtual void InternalStart()
    {
    }
}
public abstract class EditSelectValue<T> : EditMode
{
    public Action<T> OnEdit;
}
public class EditSelectPoint : EditSelectValue<VECTOR2>
{
    protected internal override bool Edit(ref MATRIX2x3 matrix, GRAPHICS spriteBatch, Entry e)
    {
        VECTOR2 p2 = __INPUT.PointerPosition;
        VECTOR2.Transform(ref p2, ref matrix);
        spriteBatch.Draw(TEXTURE.Pixel, p2, GRAPHICS.NullSource, COLOR.Red, 0, new VECTOR2(0.5f), new VECTOR2(4), EFlip.None);
        spriteBatch.Draw(FONT, p2.ToString(), p2, COLOR.Red);

        if (__INPUT.PointerIsClick(1))
        {
            return true;
        }
        else if (__INPUT.PointerIsTap())
        {
            OnEdit(p2);
            return true;
        }
        return false;
    }
}
public class EditSelectRect : EditSelectValue<RECT>
{
    static PATCH PATCH = PATCH.GetNinePatch(COLOR.TransparentBlack, COLOR.Lime, 2);

    private bool clicked;

    protected internal override bool Edit(ref MATRIX2x3 matrix, GRAPHICS spriteBatch, Entry e)
    {
        VECTOR2 p1 = __INPUT.PointerPosition;
        VECTOR2 p2 = __INPUT.PointerClickPosition;
        VECTOR2.Transform(ref p1, ref matrix);
        VECTOR2.Transform(ref p2, ref matrix);

        if (clicked)
        {
            RECT area;
            RECT.CreateRectangle(ref p1, ref p2, out area);
            if (__INPUT.PointerIsRelease(0))
            {
                OnEdit(area);
                return true;
            }
            spriteBatch.Draw(PATCH, area);
            spriteBatch.Draw(FONT, area.ToString(), area, COLOR.Red, EPivot.MiddleCenter);
        }
        else
        {
            spriteBatch.Draw(TEXTURE.Pixel, p1, GRAPHICS.NullSource, COLOR.Red, 0, new VECTOR2(0.5f), new VECTOR2(4), EFlip.None);
            spriteBatch.Draw(FONT, p1.ToString(), p1, COLOR.Red);
            if (__INPUT.PointerIsClick(0))
                clicked = true;
        }

        if (__INPUT.PointerIsClick(1))
            return true;

        return false;
    }
    protected override void InternalStart()
    {
        clicked = false;
    }
}
public class EditSelectCircle : EditSelectValue<CIRCLE>
{
    protected internal override bool Edit(ref MATRIX2x3 matrix, GRAPHICS spriteBatch, Entry e)
    {
        VECTOR2 p1 = __INPUT.PointerPosition;
        VECTOR2 p2 = __INPUT.PointerClickPosition;
        VECTOR2.Transform(ref p1, ref matrix);
        VECTOR2.Transform(ref p2, ref matrix);
        CIRCLE circle;
        circle.C = p2;
        circle.R = VECTOR2.Distance(p1, p2);
        //spriteBatch.Draw(TEXTURE.Pixel, area, null, new COLOR(255, 20, 16, 128));

        if (__INPUT.PointerIsClick(1))
        {
            return true;
        }
        else if (__INPUT.PointerIsRelease(0))
        {
            OnEdit(circle);
            return true;
        }
        return false;
    }
}
public class EditSelectLine : EditSelectValue<List<PSPosMotionPath.BonePoint>>
{
    List<PSPosMotionPath.BonePoint> result = new List<PSPosMotionPath.BonePoint>();
    /// <summary>编辑的总时间</summary>
    float time = 0;

    // 绘制路径动画
    float ptime = 0;

    protected internal override bool Edit(ref MATRIX2x3 matrix, GRAPHICS spriteBatch, Entry e)
    {
        // 鼠标右键取消
        if (__INPUT.PointerIsClick(1))
            return true;

        VECTOR2 p1 = __INPUT.PointerPosition;
        VECTOR2.Transform(ref p1, ref matrix);

        if (__INPUT.Pointer.IsClick(0))
        {
            // 鼠标点击开始
            PSPosMotionPath.BonePoint point = new PSPosMotionPath.BonePoint();
            point.X = p1.X;
            point.Y = p1.Y;
            result.Add(point);
        }
        else if (result.Count > 0 && __INPUT.Pointer.IsPressed(0))
        {
            time += GameTime.Time.ElapsedSecond;
            // 持续绘制路径
            var last = result.Last();
            if (last.X != p1.X || last.Y != p1.Y)
            {
                PSPosMotionPath.BonePoint point = new PSPosMotionPath.BonePoint();
                point.Time = time;
                point.X = p1.X;
                point.Y = p1.Y;
                result.Add(point);

                // 还在绘制时，动画实时跟上运动轨迹
                ptime = time;
            }
        }
        else if (result.Count > 0 && __INPUT.Pointer.IsRelease(0))
        {
            OnEdit(result);
            return true;
        }

        // 还没开始画
        spriteBatch.Draw(TEXTURE.Pixel, p1, GRAPHICS.NullSource, COLOR.Red, 0, new VECTOR2(0.5f), new VECTOR2(4), EFlip.None);
        spriteBatch.Draw(FONT, p1.ToString(), p1, COLOR.Red);

        if (result.Count > 0)
        {
            // 动画绘制路径
            bool flagOver = true;
            int last = result.Count - 1;
            for (int i = 0; i < last; i++)
            {
                var p = result[i];
                var p2 = result[i + 1];

                float y = p.Y - p2.Y;
                float x = p.X - p2.X;
                float distance = (float)Math.Sqrt(y * y + x * x);
                float radian = (float)Math.Atan2(y, x);
                if (ptime >= p.Time)
                {
                    spriteBatch.BaseDraw(TEXTURE.Pixel, p.X, p.Y,
                        distance, 3, false, 0, 0, 1, 1, true,
                        0, 255, 0, 255, radian, 1f, 0.5f, EFlip.None);
                }
                else
                {
                    flagOver = false;
                    break;
                }
            }
            // 结束后停顿以下再重新播放动画
            if (flagOver && ptime >= result[last].Time + 1)
                ptime = 0;
            else
                ptime += e.GameTime.ElapsedSecond;
        }
        return false;
    }
    protected override void InternalStart()
    {
        result.Clear();
        time = 0;
        ptime = 0;
    }
}

// Editor
public class EditorCommon : Label
{
    public static COLOR BGBorderColor = new COLOR(160, 160, 160, 222);
    public static COLOR BGBodyColor = new COLOR(96, 96, 96, 255);
    public static PATCH PatchLabel;
    public static float WIDTH = 80;

    public Label Label = new Label();

    public EPivot LayoutMode
    {
        get { return Label.UIText.TextAlignment; }
        set { Label.UIText.TextAlignment = value; }
    }
    protected float EditableHeight
    {
        get { return Label.ContentSize.Y; }
    }

    static EditorCommon()
    {
        PatchLabel = PATCH.GetNinePatch(COLOR.TransparentBlack, BGBorderColor, 1);
        PatchLabel.Left = 0;
        PatchLabel.Top = 0;
        PatchLabel.Bottom = PatchLabel.Height;

        //fontLabel.FontSize = 14;
    }

    public EditorCommon(EditorVariable editor)
    {
        editor.Background = PATCH.GetNinePatch(BGBodyColor, BGBorderColor, 1);
        //editor.Hover += HoverArea;
        //editor.UnHover += UnHoverArea;
        SourceNormal = PatchLabel;
        Font = EditorVariable.FONT;
        Width = WIDTH;
        UIText.Padding.X = 12;


        //editor.OnSetVariable += new Action<IVariable>(editor_OnSetVariable);
        foreach (var item in editor)
            if (!(item is EditorVariable))
                item.X += WIDTH;
        editor.Add(this);
    }

    //private static void HoverArea(UIElement sender, Entry e)
    //{
    //    ((Panel)sender).Color = BGBorderHoverColor;
    //}
    //private static void UnHoverArea(UIElement sender, Entry e)
    //{
    //    ((Panel)sender).Color = BGBorderColor;
    //}
}

public class EditorEditMode<T, Edit> : EditorAllObject where Edit : EditSelectValue<T>, new()
{
    static T __copy;

    public EditorEditMode() : base(new Type[] { typeof(T) }, false)
    {        
        // 编辑模式选择
        Hover += new DUpdate<UIElement>(EditorEditMode_Hover);
    }

    void EditorEditMode_Hover(UIElement sender, Entry e)
    {
        if (e.INPUT.Pointer.IsTap(1))
        {
            _S<Edit>.Value.OnEdit = Select;
            _S<Edit>.Value.Start();
        }
        if (e.INPUT.Keyboard.Ctrl)
        {
            if (e.INPUT.Keyboard.IsClick(PCKeys.C))
            {
                __copy = (T)VariableValue;
                STextHint.ShowHint("复制成功");
                Handle();
            }
            if (e.INPUT.Keyboard.IsClick(PCKeys.V) && __copy != null)
            {
                VariableValue = __copy;
                STextHint.ShowHint("粘贴成功");
                Handle();
            }
        }
    }
    void Select(T result)
    {
        VariableValue = result;
    }
}
