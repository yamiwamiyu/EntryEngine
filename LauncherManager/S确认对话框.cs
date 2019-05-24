using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using System;

public partial class S确认对话框 : UIScene
{
    private Func<bool> waitOver;
    private CorEnumerator<ICoroutine> waitOver2;
    private Action<bool> confirm;

    public S确认对话框()
    {
        Initialize();

        this.ShowPosition = EShowPosition.ParentCenter;

        B确定.Clicked += new DUpdate<UIElement>(B确定_Clicked);
        B取消.Clicked += new DUpdate<UIElement>(B取消_Clicked);
    }

    void B取消_Clicked(UIElement sender, Entry e)
    {
        State = EState.Break;
        confirm(false);
        confirm = null;
    }
    void B确定_Clicked(UIElement sender, Entry e)
    {
        State = EState.Break;
        // Hint时为null
        if (confirm != null)
        {
            confirm(true);
            confirm = null;
        }
    }
    
    private IEnumerable<ICoroutine> MyLoading()
    {
        return null;
    }

    protected override void InternalEvent(Entry e)
    {
        base.InternalEvent(e);

        if ((TBText.Readonly || !TBText.Focused) && e.INPUT.Keyboard != null && e.INPUT.Keyboard.IsClick(PCKeys.Enter))
            B确定_Clicked(B确定, e);
    }
    protected override void InternalUpdate(Entry e)
    {
        base.InternalUpdate(e);
        if (waitOver != null && waitOver())
        {
            this.State = EState.Break;
            waitOver = null;
        }
        if (waitOver2 != null)
        {
            if (waitOver2.IsEnd)
            {
                this.State = EState.Break;
                waitOver2 = null;
            }
            else
            {
                ICoroutine cor;
                waitOver2.Update(out cor);
            }
        }
    }

    private static S确认对话框 Open(Text.ETextID id, params string[] param)
    {
        var scene = Entry.Instance.ShowDialogScene<S确认对话框>(EState.Block);
        scene.State = EState.Block;
        scene.TBText.Text = string.Format(_TABLE._TextByID[id].Content, param);
        scene.TBText.Readonly = true;
        return scene;
    }
    public static void Wait(Text.ETextID id, Func<bool> waitOver, params string[] param)
    {
        var scene = Open(id, param);
        scene.B确定.Visible = false;
        scene.B取消.Visible = false;
        scene.waitOver = waitOver;
    }
    public static void Wait(Text.ETextID id, IEnumerable<ICoroutine> waitOver, params string[] param)
    {
        var scene = Open(id, param);
        scene.B确定.Visible = false;
        scene.B取消.Visible = false;
        scene.waitOver2 = new CorEnumerator<ICoroutine>(waitOver);
    }
    public static void Hint(Text.ETextID id, params string[] param)
    {
        var scene = Open(id, param);
        scene.B确定.Visible = true;
        scene.B取消.Visible = false;
        scene.waitOver = null;
        scene.waitOver2 = null;
    }
    public static void Confirm(Text.ETextID id, Action<bool> callback, params string[] param)
    {
        var scene = Open(id, param);
        scene.B确定.Visible = true;
        scene.B取消.Visible = true;
        scene.confirm = callback;
    }
    public static void Input(Action<string> action)
    {
        var scene = Entry.Instance.ShowDialogScene<S确认对话框>(EState.Block);
        scene.TBText.Text = string.Empty;
        scene.TBText.Readonly = false;
        scene.B确定.Visible = true;
        scene.B取消.Visible = true;
        scene.confirm = (result) =>
        {
            if (result)
                action(scene.TBText.Text);
        };
    }
}
