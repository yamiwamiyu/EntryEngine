using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S新建管理账号菜单
{
    public EntryEngine.UI.Label L密码 = new EntryEngine.UI.Label();
    public EntryEngine.UI.TextBox TB账号 = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.TextBox TB密码 = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.Label L账号 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L权限 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Button B确定 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B取消 = new EntryEngine.UI.Button();
    public EntryEngine.UI.DropDown DD权限 = new EntryEngine.UI.DropDown();
    public EntryEngine.UI.Selectable S160613103204 = new EntryEngine.UI.Selectable();
    public EntryEngine.UI.Button B160613103427 = new EntryEngine.UI.Button();
    
    
    private void Initialize()
    {
        this.Name = "S新建管理账号菜单";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 600f;
        this_Clip.Height = 300f;
        this.Clip = this_Clip;
        
        this.ShowPosition = (EShowPosition)2;
        L密码.Name = "L密码";
        EntryEngine.RECT L密码_Clip = new EntryEngine.RECT();
        L密码_Clip.X = 28f;
        L密码_Clip.Y = 91.875f;
        L密码_Clip.Width = 115f;
        L密码_Clip.Height = 45f;
        L密码.Clip = L密码_Clip;
        
        L密码.UIText = new EntryEngine.UI.UIText();
        L密码.UIText.Text = "密码";
        L密码.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L密码.UIText.TextAlignment = (EPivot)17;
        L密码.UIText.TextShader = null;
        L密码.UIText.Padding.X = 0f;
        L密码.UIText.Padding.Y = 0f;
        L密码.UIText.FontSize = 16f;
        L密码.Text = "密码";
        this.Add(L密码);
        TB账号.Name = "TB账号";
        EntryEngine.RECT TB账号_Clip = new EntryEngine.RECT();
        TB账号_Clip.X = 168f;
        TB账号_Clip.Y = 23.875f;
        TB账号_Clip.Width = 356f;
        TB账号_Clip.Height = 45f;
        TB账号.Clip = TB账号_Clip;
        
        TB账号.Color = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TB账号.CursorAreaColor = new COLOR()
        {
            B = 51,
            G = 153,
            R = 255,
            A = 255,
        };
        TB账号.DefaultText = new EntryEngine.UI.UIText();
        TB账号.DefaultText.Text = "";
        TB账号.DefaultText.FontColor = new COLOR()
        {
            B = 211,
            G = 211,
            R = 211,
            A = 255,
        };
        TB账号.DefaultText.TextAlignment = (EPivot)0;
        TB账号.DefaultText.TextShader = null;
        TB账号.DefaultText.Padding.X = 0f;
        TB账号.DefaultText.Padding.Y = 0f;
        TB账号.DefaultText.FontSize = 16f;
        TB账号.UIText = new EntryEngine.UI.UIText();
        TB账号.UIText.Text = "";
        TB账号.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TB账号.UIText.TextAlignment = (EPivot)17;
        TB账号.UIText.TextShader = null;
        TB账号.UIText.Padding.X = 0f;
        TB账号.UIText.Padding.Y = 0f;
        TB账号.UIText.FontSize = 16f;
        this.Add(TB账号);
        TB密码.Name = "TB密码";
        EntryEngine.RECT TB密码_Clip = new EntryEngine.RECT();
        TB密码_Clip.X = 168f;
        TB密码_Clip.Y = 91.875f;
        TB密码_Clip.Width = 356f;
        TB密码_Clip.Height = 45f;
        TB密码.Clip = TB密码_Clip;
        
        TB密码.Color = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TB密码.CursorAreaColor = new COLOR()
        {
            B = 51,
            G = 153,
            R = 255,
            A = 255,
        };
        TB密码.DefaultText = new EntryEngine.UI.UIText();
        TB密码.DefaultText.Text = "";
        TB密码.DefaultText.FontColor = new COLOR()
        {
            B = 211,
            G = 211,
            R = 211,
            A = 255,
        };
        TB密码.DefaultText.TextAlignment = (EPivot)0;
        TB密码.DefaultText.TextShader = null;
        TB密码.DefaultText.Padding.X = 0f;
        TB密码.DefaultText.Padding.Y = 0f;
        TB密码.DefaultText.FontSize = 16f;
        TB密码.UIText = new EntryEngine.UI.UIText();
        TB密码.UIText.Text = "";
        TB密码.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TB密码.UIText.TextAlignment = (EPivot)17;
        TB密码.UIText.TextShader = null;
        TB密码.UIText.Padding.X = 0f;
        TB密码.UIText.Padding.Y = 0f;
        TB密码.UIText.FontSize = 16f;
        this.Add(TB密码);
        L账号.Name = "L账号";
        EntryEngine.RECT L账号_Clip = new EntryEngine.RECT();
        L账号_Clip.X = 28f;
        L账号_Clip.Y = 23.875f;
        L账号_Clip.Width = 115f;
        L账号_Clip.Height = 45f;
        L账号.Clip = L账号_Clip;
        
        L账号.UIText = new EntryEngine.UI.UIText();
        L账号.UIText.Text = "账号";
        L账号.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L账号.UIText.TextAlignment = (EPivot)17;
        L账号.UIText.TextShader = null;
        L账号.UIText.Padding.X = 0f;
        L账号.UIText.Padding.Y = 0f;
        L账号.UIText.FontSize = 16f;
        L账号.Text = "账号";
        this.Add(L账号);
        L权限.Name = "L权限";
        EntryEngine.RECT L权限_Clip = new EntryEngine.RECT();
        L权限_Clip.X = 28f;
        L权限_Clip.Y = 160.875f;
        L权限_Clip.Width = 115f;
        L权限_Clip.Height = 45f;
        L权限.Clip = L权限_Clip;
        
        L权限.UIText = new EntryEngine.UI.UIText();
        L权限.UIText.Text = "权限";
        L权限.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L权限.UIText.TextAlignment = (EPivot)17;
        L权限.UIText.TextShader = null;
        L权限.UIText.Padding.X = 0f;
        L权限.UIText.Padding.Y = 0f;
        L权限.UIText.FontSize = 16f;
        L权限.Text = "权限";
        this.Add(L权限);
        B确定.Name = "B确定";
        EntryEngine.RECT B确定_Clip = new EntryEngine.RECT();
        B确定_Clip.X = 143f;
        B确定_Clip.Y = 235.875f;
        B确定_Clip.Width = 155f;
        B确定_Clip.Height = 45f;
        B确定.Clip = B确定_Clip;
        
        B确定.UIText = new EntryEngine.UI.UIText();
        B确定.UIText.Text = "确定";
        B确定.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        B确定.UIText.TextAlignment = (EPivot)17;
        B确定.UIText.TextShader = null;
        B确定.UIText.Padding.X = 0f;
        B确定.UIText.Padding.Y = 0f;
        B确定.UIText.FontSize = 16f;
        B确定.Text = "确定";
        this.Add(B确定);
        B取消.Name = "B取消";
        EntryEngine.RECT B取消_Clip = new EntryEngine.RECT();
        B取消_Clip.X = 346f;
        B取消_Clip.Y = 235.875f;
        B取消_Clip.Width = 155f;
        B取消_Clip.Height = 45f;
        B取消.Clip = B取消_Clip;
        
        B取消.UIText = new EntryEngine.UI.UIText();
        B取消.UIText.Text = "取消";
        B取消.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        B取消.UIText.TextAlignment = (EPivot)17;
        B取消.UIText.TextShader = null;
        B取消.UIText.Padding.X = 0f;
        B取消.UIText.Padding.Y = 0f;
        B取消.UIText.FontSize = 16f;
        B取消.Text = "取消";
        this.Add(B取消);
        DD权限.Name = "DD权限";
        EntryEngine.RECT DD权限_Clip = new EntryEngine.RECT();
        DD权限_Clip.X = 168f;
        DD权限_Clip.Y = 160.875f;
        DD权限_Clip.Width = 356f;
        DD权限_Clip.Height = 46f;
        DD权限.Clip = DD权限_Clip;
        
        DD权限.UIText = new EntryEngine.UI.UIText();
        DD权限.UIText.Text = "";
        DD权限.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        DD权限.UIText.TextAlignment = (EPivot)16;
        DD权限.UIText.TextShader = null;
        DD权限.UIText.Padding.X = 40f;
        DD权限.UIText.Padding.Y = 0f;
        DD权限.UIText.FontSize = 16f;
        DD权限.DropDownList = S160613103204;
        this.Add(DD权限);
        S160613103204.Name = "S160613103204";
        EntryEngine.RECT S160613103204_Clip = new EntryEngine.RECT();
        S160613103204_Clip.X = 0f;
        S160613103204_Clip.Y = 46f;
        S160613103204_Clip.Width = 373f;
        S160613103204_Clip.Height = 90f;
        S160613103204.Clip = S160613103204_Clip;
        
        S160613103204.DragMode = (EDragMode)1;
        DD权限.Add(S160613103204);
        B160613103427.Name = "B160613103427";
        EntryEngine.RECT B160613103427_Clip = new EntryEngine.RECT();
        B160613103427_Clip.X = 310f;
        B160613103427_Clip.Y = 0f;
        B160613103427_Clip.Width = 46f;
        B160613103427_Clip.Height = 46f;
        B160613103427.Clip = B160613103427_Clip;
        
        B160613103427.UIText = new EntryEngine.UI.UIText();
        B160613103427.UIText.Text = "";
        B160613103427.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        B160613103427.UIText.TextAlignment = (EPivot)17;
        B160613103427.UIText.TextShader = null;
        B160613103427.UIText.Padding.X = 0f;
        B160613103427.UIText.Padding.Y = 0f;
        B160613103427.UIText.FontSize = 16f;
        DD权限.Add(B160613103427);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine async;
        ICoroutine ___async;
        Entry.ShowDialogScene(S160613103204, EState.Break);
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1半透明.mtpatch", ___c => this.Background = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L密码.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TB账号.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TB密码.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L账号.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L权限.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B确定.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B取消.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => DD权限.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1半透明.mtpatch", ___c => S160613103204.Background = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0004_登陆下拉-2.png", ___c => B160613103427.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    private void Show(EntryEngine.UI.UIScene __scene)
    {
        
    }
}
