using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S服务器管理
{
    public EntryEngine.UI.Panel P服务器管理面板 = new EntryEngine.UI.Panel();
    public EntryEngine.UI.DropDown DD用户名 = new EntryEngine.UI.DropDown();
    public EntryEngine.UI.Selectable S160613103204 = new EntryEngine.UI.Selectable();
    public EntryEngine.UI.TextBox TB用户名 = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.Button   B用户名1 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B160613103427 = new EntryEngine.UI.Button();
    public EntryEngine.UI.CheckBox CB服务 = new EntryEngine.UI.CheckBox();
    public EntryEngine.UI.CheckBox CB服务器 = new EntryEngine.UI.CheckBox();
    public EntryEngine.UI.Button B账号管理 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B服务管理 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B中断连接 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B显示日志 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B更新管理器 = new EntryEngine.UI.Button();
    private EntryEngine.UI.Button ___B用户名1()
    {
        var B用户名1 = new EntryEngine.UI.Button();
        B用户名1.Name = "  B用户名1";
        EntryEngine.RECT   B用户名1_Clip = new EntryEngine.RECT();
        B用户名1_Clip.X = 126.5f;
        B用户名1_Clip.Y = 6f;
        B用户名1_Clip.Width = 100f;
        B用户名1_Clip.Height = 24f;
        B用户名1.Clip =   B用户名1_Clip;
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1半透明.mtpatch", ___c =>   B用户名1.SourceHover = ___c));
        B用户名1.UIText = new EntryEngine.UI.UIText();
        B用户名1.UIText.Text = "";
        B用户名1.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B用户名1.UIText.TextAlignment = (EPivot)17;
        B用户名1.UIText.TextShader = null;
        B用户名1.UIText.Padding.X = 0f;
        B用户名1.UIText.Padding.Y = 0f;
        B用户名1.UIText.FontSize = 22f;
        
        return B用户名1;
    }
    
    
    private void Initialize()
    {
        this.Name = "S服务器管理";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 1280f;
        this_Clip.Height = 720f;
        this.Clip = this_Clip;
        
        this.Anchor = (EAnchor)15;
        P服务器管理面板.Name = "P服务器管理面板";
        EntryEngine.RECT P服务器管理面板_Clip = new EntryEngine.RECT();
        P服务器管理面板_Clip.X = 40f;
        P服务器管理面板_Clip.Y = 138.875f;
        P服务器管理面板_Clip.Width = 1200f;
        P服务器管理面板_Clip.Height = 544f;
        P服务器管理面板.Clip = P服务器管理面板_Clip;
        
        P服务器管理面板.Anchor = (EAnchor)15;
        this.Add(P服务器管理面板);
        DD用户名.Name = "DD用户名";
        EntryEngine.RECT DD用户名_Clip = new EntryEngine.RECT();
        DD用户名_Clip.X = 64f;
        DD用户名_Clip.Y = 9.875f;
        DD用户名_Clip.Width = 373f;
        DD用户名_Clip.Height = 55f;
        DD用户名.Clip = DD用户名_Clip;
        
        DD用户名.UIText = new EntryEngine.UI.UIText();
        DD用户名.UIText.Text = "";
        DD用户名.UIText.FontColor = new COLOR()
        {
            R = 255,
            G = 255,
            B = 255,
            A = 255,
        };
        DD用户名.UIText.TextAlignment = (EPivot)17;
        DD用户名.UIText.TextShader = null;
        DD用户名.UIText.Padding.X = 0f;
        DD用户名.UIText.Padding.Y = 0f;
        DD用户名.UIText.FontSize = 16f;
        DD用户名.DropDownText = TB用户名;
        DD用户名.DropDownList = S160613103204;
        this.Add(DD用户名);
        S160613103204.Name = "S160613103204";
        EntryEngine.RECT S160613103204_Clip = new EntryEngine.RECT();
        S160613103204_Clip.X = 0f;
        S160613103204_Clip.Y = 46f;
        S160613103204_Clip.Width = 373f;
        S160613103204_Clip.Height = 90f;
        S160613103204.Clip = S160613103204_Clip;
        
        S160613103204.Visible = false;
        S160613103204.DragMode = (EDragMode)1;
        DD用户名.Add(S160613103204);
        TB用户名.Name = "TB用户名";
        EntryEngine.RECT TB用户名_Clip = new EntryEngine.RECT();
        TB用户名_Clip.X = 3f;
        TB用户名_Clip.Y = 4f;
        TB用户名_Clip.Width = 321f;
        TB用户名_Clip.Height = 40f;
        TB用户名.Clip = TB用户名_Clip;
        
        TB用户名.Color = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TB用户名.DefaultText = new EntryEngine.UI.UIText();
        TB用户名.DefaultText.Text = "";
        TB用户名.DefaultText.FontColor = new COLOR()
        {
            R = 211,
            G = 211,
            B = 211,
            A = 255,
        };
        TB用户名.DefaultText.TextAlignment = (EPivot)0;
        TB用户名.DefaultText.TextShader = null;
        TB用户名.DefaultText.Padding.X = 0f;
        TB用户名.DefaultText.Padding.Y = 0f;
        TB用户名.DefaultText.FontSize = 16f;
        TB用户名.UIText = new EntryEngine.UI.UIText();
        TB用户名.UIText.Text = "";
        TB用户名.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TB用户名.UIText.TextAlignment = (EPivot)17;
        TB用户名.UIText.TextShader = null;
        TB用户名.UIText.Padding.X = 0f;
        TB用户名.UIText.Padding.Y = 0f;
        TB用户名.UIText.FontSize = 22f;
        DD用户名.Add(TB用户名);
        B160613103427.Name = "B160613103427";
        EntryEngine.RECT B160613103427_Clip = new EntryEngine.RECT();
        B160613103427_Clip.X = 332.5f;
        B160613103427_Clip.Y = 3f;
        B160613103427_Clip.Width = 38f;
        B160613103427_Clip.Height = 40f;
        B160613103427.Clip = B160613103427_Clip;
        
        B160613103427.UIText = new EntryEngine.UI.UIText();
        B160613103427.UIText.Text = "";
        B160613103427.UIText.FontColor = new COLOR()
        {
            R = 255,
            G = 255,
            B = 255,
            A = 255,
        };
        B160613103427.UIText.TextAlignment = (EPivot)17;
        B160613103427.UIText.TextShader = null;
        B160613103427.UIText.Padding.X = 0f;
        B160613103427.UIText.Padding.Y = 0f;
        B160613103427.UIText.FontSize = 16f;
        DD用户名.Add(B160613103427);
        CB服务.Name = "CB服务";
        EntryEngine.RECT CB服务_Clip = new EntryEngine.RECT();
        CB服务_Clip.X = 468f;
        CB服务_Clip.Y = 83.875f;
        CB服务_Clip.Width = 220f;
        CB服务_Clip.Height = 55f;
        CB服务.Clip = CB服务_Clip;
        
        CB服务.UIText = new EntryEngine.UI.UIText();
        CB服务.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        CB服务.UIText.TextAlignment = (EPivot)17;
        CB服务.UIText.TextShader = null;
        CB服务.UIText.Padding.X = 0f;
        CB服务.UIText.Padding.Y = 0f;
        CB服务.UIText.FontSize = 22f;
        CB服务.IsRadioButton = true;
        CB服务.CheckedOverlayNormal = true;
        this.Add(CB服务);
        CB服务器.Name = "CB服务器";
        EntryEngine.RECT CB服务器_Clip = new EntryEngine.RECT();
        CB服务器_Clip.X = 227.5f;
        CB服务器_Clip.Y = 83.875f;
        CB服务器_Clip.Width = 220f;
        CB服务器_Clip.Height = 55f;
        CB服务器.Clip = CB服务器_Clip;
        
        CB服务器.UIText = new EntryEngine.UI.UIText();
        CB服务器.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        CB服务器.UIText.TextAlignment = (EPivot)17;
        CB服务器.UIText.TextShader = null;
        CB服务器.UIText.Padding.X = 0f;
        CB服务器.UIText.Padding.Y = 0f;
        CB服务器.UIText.FontSize = 22f;
        CB服务器.IsRadioButton = true;
        CB服务器.CheckedOverlayNormal = true;
        this.Add(CB服务器);
        B账号管理.Name = "B账号管理";
        EntryEngine.RECT B账号管理_Clip = new EntryEngine.RECT();
        B账号管理_Clip.X = 1087.5f;
        B账号管理_Clip.Y = 98.875f;
        B账号管理_Clip.Width = 152f;
        B账号管理_Clip.Height = 40f;
        B账号管理.Clip = B账号管理_Clip;
        
        B账号管理.UIText = new EntryEngine.UI.UIText();
        B账号管理.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B账号管理.UIText.TextAlignment = (EPivot)17;
        B账号管理.UIText.TextShader = null;
        B账号管理.UIText.Padding.X = 0f;
        B账号管理.UIText.Padding.Y = 0f;
        B账号管理.UIText.FontSize = 20f;
        this.Add(B账号管理);
        B服务管理.Name = "B服务管理";
        EntryEngine.RECT B服务管理_Clip = new EntryEngine.RECT();
        B服务管理_Clip.X = 912f;
        B服务管理_Clip.Y = 98.875f;
        B服务管理_Clip.Width = 152f;
        B服务管理_Clip.Height = 40f;
        B服务管理.Clip = B服务管理_Clip;
        
        B服务管理.UIText = new EntryEngine.UI.UIText();
        B服务管理.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B服务管理.UIText.TextAlignment = (EPivot)17;
        B服务管理.UIText.TextShader = null;
        B服务管理.UIText.Padding.X = 0f;
        B服务管理.UIText.Padding.Y = 0f;
        B服务管理.UIText.FontSize = 20f;
        this.Add(B服务管理);
        B中断连接.Name = "B中断连接";
        EntryEngine.RECT B中断连接_Clip = new EntryEngine.RECT();
        B中断连接_Clip.X = 468f;
        B中断连接_Clip.Y = 12.875f;
        B中断连接_Clip.Width = 152f;
        B中断连接_Clip.Height = 40f;
        B中断连接.Clip = B中断连接_Clip;
        
        B中断连接.UIText = new EntryEngine.UI.UIText();
        B中断连接.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B中断连接.UIText.TextAlignment = (EPivot)17;
        B中断连接.UIText.TextShader = null;
        B中断连接.UIText.Padding.X = 0f;
        B中断连接.UIText.Padding.Y = 0f;
        B中断连接.UIText.FontSize = 20f;
        this.Add(B中断连接);
        B显示日志.Name = "B显示日志";
        EntryEngine.RECT B显示日志_Clip = new EntryEngine.RECT();
        B显示日志_Clip.X = 737.5f;
        B显示日志_Clip.Y = 98.875f;
        B显示日志_Clip.Width = 152f;
        B显示日志_Clip.Height = 40f;
        B显示日志.Clip = B显示日志_Clip;
        
        B显示日志.UIText = new EntryEngine.UI.UIText();
        B显示日志.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B显示日志.UIText.TextAlignment = (EPivot)17;
        B显示日志.UIText.TextShader = null;
        B显示日志.UIText.Padding.X = 0f;
        B显示日志.UIText.Padding.Y = 0f;
        B显示日志.UIText.FontSize = 20f;
        this.Add(B显示日志);
        B更新管理器.Name = "B更新管理器";
        EntryEngine.RECT B更新管理器_Clip = new EntryEngine.RECT();
        B更新管理器_Clip.X = 1088f;
        B更新管理器_Clip.Y = 9.875f;
        B更新管理器_Clip.Width = 152f;
        B更新管理器_Clip.Height = 40f;
        B更新管理器.Clip = B更新管理器_Clip;
        
        B更新管理器.UIText = new EntryEngine.UI.UIText();
        B更新管理器.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B更新管理器.UIText.TextAlignment = (EPivot)17;
        B更新管理器.UIText.TextShader = null;
        B更新管理器.UIText.Padding.X = 0f;
        B更新管理器.UIText.Padding.Y = 0f;
        B更新管理器.UIText.FontSize = 20f;
        this.Add(B更新管理器);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine ___async;
        Entry.ShowDialogScene(S160613103204);
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => this.Background = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => P服务器管理面板.Background = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1半透明.mtpatch", ___c => S160613103204.Background = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TB用户名.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1半透明.mtpatch", ___c =>   B用户名1.SourceHover = ___c));
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0004_登陆下拉-2.png", ___c => B160613103427.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_页签.png", ___c => CB服务.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame2.mtpatch", ___c => CB服务.SourceClicked = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_页签.png", ___c => CB服务器.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame2.mtpatch", ___c => CB服务器.SourceClicked = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_中断连接.png", ___c => B账号管理.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_中断连接.png", ___c => B服务管理.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_中断连接.png", ___c => B中断连接.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_中断连接.png", ___c => B显示日志.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_中断连接.png", ___c => B更新管理器.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    public void Show(UIScene __scene)
    {
        CB服务.UIText.Text = _LANGUAGE.GetString("16");
        CB服务.Text = _LANGUAGE.GetString("16");
        CB服务器.UIText.Text = _LANGUAGE.GetString("13");
        CB服务器.Text = _LANGUAGE.GetString("13");
        B账号管理.UIText.Text = _LANGUAGE.GetString("20");
        B账号管理.Text = _LANGUAGE.GetString("20");
        B服务管理.UIText.Text = _LANGUAGE.GetString("21");
        B服务管理.Text = _LANGUAGE.GetString("21");
        B中断连接.UIText.Text = _LANGUAGE.GetString("22");
        B中断连接.Text = _LANGUAGE.GetString("22");
        B显示日志.UIText.Text = _LANGUAGE.GetString("110");
        B显示日志.Text = _LANGUAGE.GetString("110");
        B更新管理器.UIText.Text = _LANGUAGE.GetString("111");
        B更新管理器.Text = _LANGUAGE.GetString("111");
        
    }
}
