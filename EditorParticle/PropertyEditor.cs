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
public class EditSelectPoint : EditMode
{
    public Action<VECTOR2> Action;

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
            Action(p2);
            return true;
        }
        return false;
    }
}
public class EditSelectRect : EditMode
{
    static PATCH PATCH = PATCH.GetNinePatch(COLOR.TransparentBlack, COLOR.Lime, 2);

    public Action<RECT> Action;
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
                Action(area);
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
public class EditSelectCircle : EditMode
{
    public Action<CIRCLE> Action;

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
            Action(circle);
            return true;
        }
        return false;
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

public class EditorClip : EditorAllObject
{
    private static EditSelectRect edit = new EditSelectRect();

    public EditorClip()
        : base(new Type[] { typeof(RECT) })
    {
        // 编辑模式选择
        Hover += new DUpdate<UIElement>(EditorClip_Hover);
    }

    void EditorClip_Hover(UIElement sender, Entry e)
    {
        if (e.INPUT.Pointer.IsTap(1))
        {
            edit.Action = Select;
            edit.Start();
        }
    }
    void Select(RECT result)
    {
        VariableValue = result;
    }
}
public class EditorPoint : EditorAllObject
{
    private static EditSelectPoint edit = new EditSelectPoint();

    public EditorPoint()
        : base(new Type[] { typeof(VECTOR2) })
    {
        // 编辑模式选择
        Hover += new DUpdate<UIElement>(EditorPoint_Hover);
    }

    void EditorPoint_Hover(UIElement sender, Entry e)
    {
        if (e.INPUT.Pointer.IsTap(1))
        {
            edit.Action = Select;
            edit.Start();
        }
    }
    void Select(VECTOR2 result)
    {
        VariableValue = result;
    }
}
