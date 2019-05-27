using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S账号管理菜单
{
    public EntryEngine.UI.Label L平台 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L密码 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L账号 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Button B同平台 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B同名 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B全选 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Label L权限 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Button B关闭 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B删除 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B新建 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Panel P账号管理信息面板 = new EntryEngine.UI.Panel();
    public EntryEngine.UI.TextureBox  TB账号管理信息1 = new EntryEngine.UI.TextureBox();
    public EntryEngine.UI.TextureBox  TB账号管理信息2 = new EntryEngine.UI.TextureBox();
    
    
    private void Initialize()
    {
        this.Name = "S账号管理菜单";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 1000f;
        this_Clip.Height = 500f;
        this.Clip = this_Clip;
        
        this.Anchor = (EAnchor)15;
        this.ShowPosition = (EShowPosition)1;
        L平台.Name = "L平台";
        EntryEngine.RECT L平台_Clip = new EntryEngine.RECT();
        L平台_Clip.X = 470f;
        L平台_Clip.Y = 20.87506f;
        L平台_Clip.Width = 100f;
        L平台_Clip.Height = 50f;
        L平台.Clip = L平台_Clip;
        
        L平台.UIText = new EntryEngine.UI.UIText();
        L平台.UIText.Text = "平台";
        L平台.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L平台.UIText.TextAlignment = (EPivot)17;
        L平台.UIText.TextShader = null;
        L平台.UIText.Padding.X = 0f;
        L平台.UIText.Padding.Y = 0f;
        L平台.UIText.FontSize = 16f;
        L平台.Text = "平台";
        this.Add(L平台);
        L密码.Name = "L密码";
        EntryEngine.RECT L密码_Clip = new EntryEngine.RECT();
        L密码_Clip.X = 271f;
        L密码_Clip.Y = 20.87506f;
        L密码_Clip.Width = 100f;
        L密码_Clip.Height = 50f;
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
        L账号.Name = "L账号";
        EntryEngine.RECT L账号_Clip = new EntryEngine.RECT();
        L账号_Clip.X = 72f;
        L账号_Clip.Y = 20.87506f;
        L账号_Clip.Width = 100f;
        L账号_Clip.Height = 50f;
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
        B同平台.Name = "B同平台";
        EntryEngine.RECT B同平台_Clip = new EntryEngine.RECT();
        B同平台_Clip.X = 370f;
        B同平台_Clip.Y = 433.8751f;
        B同平台_Clip.Width = 130f;
        B同平台_Clip.Height = 45f;
        B同平台.Clip = B同平台_Clip;
        
        B同平台.Anchor = (EAnchor)9;
        B同平台.UIText = new EntryEngine.UI.UIText();
        B同平台.UIText.Text = "同平台";
        B同平台.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        B同平台.UIText.TextAlignment = (EPivot)17;
        B同平台.UIText.TextShader = null;
        B同平台.UIText.Padding.X = 0f;
        B同平台.UIText.Padding.Y = 0f;
        B同平台.UIText.FontSize = 16f;
        B同平台.Text = "同平台";
        this.Add(B同平台);
        B同名.Name = "B同名";
        EntryEngine.RECT B同名_Clip = new EntryEngine.RECT();
        B同名_Clip.X = 220f;
        B同名_Clip.Y = 433.8751f;
        B同名_Clip.Width = 130f;
        B同名_Clip.Height = 45f;
        B同名.Clip = B同名_Clip;
        
        B同名.Anchor = (EAnchor)9;
        B同名.UIText = new EntryEngine.UI.UIText();
        B同名.UIText.Text = "同名";
        B同名.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        B同名.UIText.TextAlignment = (EPivot)17;
        B同名.UIText.TextShader = null;
        B同名.UIText.Padding.X = 0f;
        B同名.UIText.Padding.Y = 0f;
        B同名.UIText.FontSize = 16f;
        B同名.Text = "同名";
        this.Add(B同名);
        B全选.Name = "B全选";
        EntryEngine.RECT B全选_Clip = new EntryEngine.RECT();
        B全选_Clip.X = 72f;
        B全选_Clip.Y = 433.8751f;
        B全选_Clip.Width = 130f;
        B全选_Clip.Height = 45f;
        B全选.Clip = B全选_Clip;
        
        B全选.Anchor = (EAnchor)9;
        B全选.UIText = new EntryEngine.UI.UIText();
        B全选.UIText.Text = "全选";
        B全选.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        B全选.UIText.TextAlignment = (EPivot)17;
        B全选.UIText.TextShader = null;
        B全选.UIText.Padding.X = 0f;
        B全选.UIText.Padding.Y = 0f;
        B全选.UIText.FontSize = 16f;
        B全选.Text = "全选";
        this.Add(B全选);
        L权限.Name = "L权限";
        EntryEngine.RECT L权限_Clip = new EntryEngine.RECT();
        L权限_Clip.X = 835f;
        L权限_Clip.Y = 20.87506f;
        L权限_Clip.Width = 100f;
        L权限_Clip.Height = 50f;
        L权限.Clip = L权限_Clip;
        
        L权限.Anchor = (EAnchor)6;
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
        B关闭.Name = "B关闭";
        EntryEngine.RECT B关闭_Clip = new EntryEngine.RECT();
        B关闭_Clip.X = 848f;
        B关闭_Clip.Y = 433.8751f;
        B关闭_Clip.Width = 130f;
        B关闭_Clip.Height = 45f;
        B关闭.Clip = B关闭_Clip;
        
        B关闭.Anchor = (EAnchor)9;
        B关闭.UIText = new EntryEngine.UI.UIText();
        B关闭.UIText.Text = "关闭";
        B关闭.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        B关闭.UIText.TextAlignment = (EPivot)17;
        B关闭.UIText.TextShader = null;
        B关闭.UIText.Padding.X = 0f;
        B关闭.UIText.Padding.Y = 0f;
        B关闭.UIText.FontSize = 16f;
        B关闭.Text = "关闭";
        this.Add(B关闭);
        B删除.Name = "B删除";
        EntryEngine.RECT B删除_Clip = new EntryEngine.RECT();
        B删除_Clip.X = 705f;
        B删除_Clip.Y = 433.8751f;
        B删除_Clip.Width = 130f;
        B删除_Clip.Height = 45f;
        B删除.Clip = B删除_Clip;
        
        B删除.Anchor = (EAnchor)9;
        B删除.UIText = new EntryEngine.UI.UIText();
        B删除.UIText.Text = "删除";
        B删除.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        B删除.UIText.TextAlignment = (EPivot)17;
        B删除.UIText.TextShader = null;
        B删除.UIText.Padding.X = 0f;
        B删除.UIText.Padding.Y = 0f;
        B删除.UIText.FontSize = 16f;
        B删除.Text = "删除";
        this.Add(B删除);
        B新建.Name = "B新建";
        EntryEngine.RECT B新建_Clip = new EntryEngine.RECT();
        B新建_Clip.X = 559f;
        B新建_Clip.Y = 433.8751f;
        B新建_Clip.Width = 130f;
        B新建_Clip.Height = 45f;
        B新建.Clip = B新建_Clip;
        
        B新建.Anchor = (EAnchor)9;
        B新建.UIText = new EntryEngine.UI.UIText();
        B新建.UIText.Text = "新建";
        B新建.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        B新建.UIText.TextAlignment = (EPivot)17;
        B新建.UIText.TextShader = null;
        B新建.UIText.Padding.X = 0f;
        B新建.UIText.Padding.Y = 0f;
        B新建.UIText.FontSize = 16f;
        B新建.Text = "新建";
        this.Add(B新建);
        P账号管理信息面板.Name = "P账号管理信息面板";
        EntryEngine.RECT P账号管理信息面板_Clip = new EntryEngine.RECT();
        P账号管理信息面板_Clip.X = 0f;
        P账号管理信息面板_Clip.Y = 81.87509f;
        P账号管理信息面板_Clip.Width = 1000f;
        P账号管理信息面板_Clip.Height = 340f;
        P账号管理信息面板.Clip = P账号管理信息面板_Clip;
        
        P账号管理信息面板.Anchor = (EAnchor)15;
        P账号管理信息面板.DragMode = (EDragMode)1;
        this.Add(P账号管理信息面板);
        TB账号管理信息1.Name = " TB账号管理信息1";
        EntryEngine.RECT  TB账号管理信息1_Clip = new EntryEngine.RECT();
        TB账号管理信息1_Clip.X = 3f;
        TB账号管理信息1_Clip.Y = 13f;
        TB账号管理信息1_Clip.Width = 100f;
        TB账号管理信息1_Clip.Height = 24f;
        TB账号管理信息1.Clip =  TB账号管理信息1_Clip;
        
        TB账号管理信息2.Name = " TB账号管理信息2";
        EntryEngine.RECT  TB账号管理信息2_Clip = new EntryEngine.RECT();
        TB账号管理信息2_Clip.X = 3f;
        TB账号管理信息2_Clip.Y = 92.00003f;
        TB账号管理信息2_Clip.Width = 100f;
        TB账号管理信息2_Clip.Height = 24f;
        TB账号管理信息2.Clip =  TB账号管理信息2_Clip;
        
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine async;
        ICoroutine ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1半透明.mtpatch", ___c => this.Background = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L平台.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L密码.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L账号.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B同平台.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B同名.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B全选.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L权限.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B关闭.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B删除.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B新建.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => P账号管理信息面板.Background = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c =>  TB账号管理信息1.Texture = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c =>  TB账号管理信息2.Texture = ___c));
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
