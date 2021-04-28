using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.UI;
using EntryEngine;

class Tip : UIScene
{
    const float MAX_WIDTH = 400;

    private Dictionary<UIElement, Func<string>> tips = new Dictionary<UIElement, Func<string>>();
    private Dictionary<string, UIElement> tips2 = new Dictionary<string, UIElement>();
    private Label label = new Label();
    private TEXTURE bg;

    public Tip()
    {
        Color = new COLOR(228, 224, 110);
        Clip = RECT.Empty;
        bg = PATCH.GetNinePatch(new COLOR(242, 255, 172), COLOR.Black, 1);

        label.BreakLine = true;
        label.UIText.FontColor = new COLOR(16, 16, 0);
        Add(label);
    }

    public void SetTip(UIElement element, TEXT.ETEXTKey tip)
    {
        tips[element] = () => _TABLE._TEXTByKey[tip].Value;
    }
    public void SetTip(UIElement element, string text)
    {
        tips[element] = () => text;
    }
    public void SetTip(UIElement element, Func<string> getText)
    {
        tips[element] = getText;
    }
    /// <summary>设置唯一字符串，另外有这个字符串，会把UI控件替换掉</summary>
    public void SetTip(string value, UIElement e)
    {
        tips2[value] = e;
    }

    private void ShowTip(string value)
    {
        label.Visible = true;
        label.Text = value;
        var size = label.TextContentSize;
        if (size.X > MAX_WIDTH)
        {
            label.Width = MAX_WIDTH;
            label.ResetDisplay();
            size = label.TextContentSize;
        }
        else
            label.Width = 0;

        var position = __INPUT.PointerPosition;
        var graphics = __GRAPHICS.GraphicsSize;
        if (position.X + size.X > graphics.X)
            position.X -= size.X;
        else
            position.X += 10;
        if (position.Y + size.Y > graphics.Y)
            position.Y -= size.Y;
        Location = position;
    }
    protected override void InternalEvent(EntryEngine.Entry e)
    {
        base.InternalEvent(e);

        label.Visible = false;
        foreach (var item in tips)
        {
            if (item.Key.IsHover)
            {
                ShowTip(item.Value());
                break;
            }
        }
        foreach (var item in tips2)
        {
            if (!item.Value.IsHover) continue;
            ShowTip(item.Key);
            break;
        }

        Background = label.Visible ? bg : null;
    }
}
