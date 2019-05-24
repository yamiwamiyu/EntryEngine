using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S账号管理信息
{
    public EntryEngine.UI.CheckBox CB单选 = new EntryEngine.UI.CheckBox();
    public EntryEngine.UI.Label L密码 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L平台 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L账号 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L权限 = new EntryEngine.UI.Label();
    
    
    private void Initialize()
    {
        this.Name = "S账号管理信息";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 950f;
        this_Clip.Height = 75f;
        this.Clip = this_Clip;
        
        CB单选.Name = "CB单选";
        EntryEngine.RECT CB单选_Clip = new EntryEngine.RECT();
        CB单选_Clip.X = 19f;
        CB单选_Clip.Y = 17.5f;
        CB单选_Clip.Width = 40f;
        CB单选_Clip.Height = 40f;
        CB单选.Clip = CB单选_Clip;
        
        CB单选.UIText = new EntryEngine.UI.UIText();
        CB单选.UIText.Text = "";
        CB单选.UIText.FontColor = new COLOR()
        {
            R = 255,
            G = 255,
            B = 255,
            A = 255,
        };
        CB单选.UIText.TextAlignment = (EPivot)18;
        CB单选.UIText.TextShader = null;
        CB单选.UIText.Padding.X = 0f;
        CB单选.UIText.Padding.Y = 0f;
        CB单选.UIText.FontSize = 16f;
        this.Add(CB单选);
        L密码.Name = "L密码";
        EntryEngine.RECT L密码_Clip = new EntryEngine.RECT();
        L密码_Clip.X = 267f;
        L密码_Clip.Y = 12.5f;
        L密码_Clip.Width = 150f;
        L密码_Clip.Height = 50f;
        L密码.Clip = L密码_Clip;
        
        L密码.UIText = new EntryEngine.UI.UIText();
        L密码.UIText.Text = "#123456";
        L密码.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L密码.UIText.TextAlignment = (EPivot)17;
        L密码.UIText.TextShader = null;
        L密码.UIText.Padding.X = 0f;
        L密码.UIText.Padding.Y = 0f;
        L密码.UIText.FontSize = 16f;
        L密码.Text = "#123456";
        this.Add(L密码);
        L平台.Name = "L平台";
        EntryEngine.RECT L平台_Clip = new EntryEngine.RECT();
        L平台_Clip.X = 466f;
        L平台_Clip.Y = 12.5f;
        L平台_Clip.Width = 330f;
        L平台_Clip.Height = 50f;
        L平台.Clip = L平台_Clip;
        
        L平台.UIText = new EntryEngine.UI.UIText();
        L平台.UIText.Text = "#腾讯-192.168.0.100";
        L平台.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L平台.UIText.TextAlignment = (EPivot)17;
        L平台.UIText.TextShader = null;
        L平台.UIText.Padding.X = 0f;
        L平台.UIText.Padding.Y = 0f;
        L平台.UIText.FontSize = 16f;
        L平台.Text = "#腾讯-192.168.0.100";
        this.Add(L平台);
        L账号.Name = "L账号";
        EntryEngine.RECT L账号_Clip = new EntryEngine.RECT();
        L账号_Clip.X = 70f;
        L账号_Clip.Y = 12.5f;
        L账号_Clip.Width = 150f;
        L账号_Clip.Height = 50f;
        L账号.Clip = L账号_Clip;
        
        L账号.UIText = new EntryEngine.UI.UIText();
        L账号.UIText.Text = "#账号123";
        L账号.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L账号.UIText.TextAlignment = (EPivot)17;
        L账号.UIText.TextShader = null;
        L账号.UIText.Padding.X = 0f;
        L账号.UIText.Padding.Y = 0f;
        L账号.UIText.FontSize = 16f;
        L账号.Text = "#账号123";
        this.Add(L账号);
        L权限.Name = "L权限";
        EntryEngine.RECT L权限_Clip = new EntryEngine.RECT();
        L权限_Clip.X = 831f;
        L权限_Clip.Y = 12.5f;
        L权限_Clip.Width = 116f;
        L权限_Clip.Height = 50f;
        L权限.Clip = L权限_Clip;
        
        L权限.UIText = new EntryEngine.UI.UIText();
        L权限.UIText.Text = "#运维";
        L权限.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L权限.UIText.TextAlignment = (EPivot)17;
        L权限.UIText.TextShader = null;
        L权限.UIText.Padding.X = 0f;
        L权限.UIText.Padding.Y = 0f;
        L权限.UIText.FontSize = 16f;
        L权限.Text = "#运维";
        this.Add(L权限);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_单选未选中.png", ___c => CB单选.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_登陆单选.png", ___c => CB单选.SourceClicked = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L密码.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L平台.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L账号.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L权限.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    public void Show(UIScene __scene)
    {
        
    }
}
