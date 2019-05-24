using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S登陆菜单
{
    public EntryEngine.UI.Label L服务端端口 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L密码 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Panel P登陆按钮面板 = new EntryEngine.UI.Panel();
    public EntryEngine.UI.Button B一键连接 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B一键删除 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B全选 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Panel P登陆信息面板 = new EntryEngine.UI.Panel();
    public EntryEngine.UI.TextureBox  TB登陆平台信息1 = new EntryEngine.UI.TextureBox();
    public EntryEngine.UI.TextureBox  TB登陆平台信息2 = new EntryEngine.UI.TextureBox();
    public EntryEngine.UI.Button B连接服务端 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B用户名删除 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Label L用户名 = new EntryEngine.UI.Label();
    public EntryEngine.UI.DropDown DD用户名 = new EntryEngine.UI.DropDown();
    public EntryEngine.UI.Selectable S160613103204 = new EntryEngine.UI.Selectable();
    public EntryEngine.UI.TextBox TB用户名 = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.Button   B用户名1 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B160613103427 = new EntryEngine.UI.Button();
    public EntryEngine.UI.TextBox TB密码 = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.TextBox TB服务端 = new EntryEngine.UI.TextBox();
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
        this.Name = "S登陆菜单";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 1280f;
        this_Clip.Height = 720f;
        this.Clip = this_Clip;
        
        this.Anchor = (EAnchor)15;
        L服务端端口.Name = "L服务端端口";
        EntryEngine.RECT L服务端端口_Clip = new EntryEngine.RECT();
        L服务端端口_Clip.X = 141f;
        L服务端端口_Clip.Y = 168.9817f;
        L服务端端口_Clip.Width = 150f;
        L服务端端口_Clip.Height = 40f;
        L服务端端口.Clip = L服务端端口_Clip;
        
        L服务端端口.UIText = new EntryEngine.UI.UIText();
        L服务端端口.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L服务端端口.UIText.TextAlignment = (EPivot)17;
        L服务端端口.UIText.TextShader = null;
        L服务端端口.UIText.Padding.X = 0f;
        L服务端端口.UIText.Padding.Y = 0f;
        L服务端端口.UIText.FontSize = 22f;
        this.Add(L服务端端口);
        L密码.Name = "L密码";
        EntryEngine.RECT L密码_Clip = new EntryEngine.RECT();
        L密码_Clip.X = 141f;
        L密码_Clip.Y = 98.98169f;
        L密码_Clip.Width = 150f;
        L密码_Clip.Height = 40f;
        L密码.Clip = L密码_Clip;
        
        L密码.UIText = new EntryEngine.UI.UIText();
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
        L密码.UIText.FontSize = 22f;
        this.Add(L密码);
        P登陆按钮面板.Name = "P登陆按钮面板";
        EntryEngine.RECT P登陆按钮面板_Clip = new EntryEngine.RECT();
        P登陆按钮面板_Clip.X = 90f;
        P登陆按钮面板_Clip.Y = 233.9817f;
        P登陆按钮面板_Clip.Width = 1100f;
        P登陆按钮面板_Clip.Height = 470f;
        P登陆按钮面板.Clip = P登陆按钮面板_Clip;
        
        P登陆按钮面板.Anchor = (EAnchor)15;
        this.Add(P登陆按钮面板);
        B一键连接.Name = "B一键连接";
        EntryEngine.RECT B一键连接_Clip = new EntryEngine.RECT();
        B一键连接_Clip.X = 136f;
        B一键连接_Clip.Y = 401f;
        B一键连接_Clip.Width = 169f;
        B一键连接_Clip.Height = 50f;
        B一键连接.Clip = B一键连接_Clip;
        
        B一键连接.Anchor = (EAnchor)9;
        B一键连接.UIText = new EntryEngine.UI.UIText();
        B一键连接.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B一键连接.UIText.TextAlignment = (EPivot)17;
        B一键连接.UIText.TextShader = null;
        B一键连接.UIText.Padding.X = 0f;
        B一键连接.UIText.Padding.Y = 0f;
        B一键连接.UIText.FontSize = 22f;
        P登陆按钮面板.Add(B一键连接);
        B一键删除.Name = "B一键删除";
        EntryEngine.RECT B一键删除_Clip = new EntryEngine.RECT();
        B一键删除_Clip.X = 426f;
        B一键删除_Clip.Y = 401f;
        B一键删除_Clip.Width = 169f;
        B一键删除_Clip.Height = 50f;
        B一键删除.Clip = B一键删除_Clip;
        
        B一键删除.Anchor = (EAnchor)9;
        B一键删除.UIText = new EntryEngine.UI.UIText();
        B一键删除.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B一键删除.UIText.TextAlignment = (EPivot)17;
        B一键删除.UIText.TextShader = null;
        B一键删除.UIText.Padding.X = 0f;
        B一键删除.UIText.Padding.Y = 0f;
        B一键删除.UIText.FontSize = 22f;
        P登陆按钮面板.Add(B一键删除);
        B全选.Name = "B全选";
        EntryEngine.RECT B全选_Clip = new EntryEngine.RECT();
        B全选_Clip.X = 54f;
        B全选_Clip.Y = 12f;
        B全选_Clip.Width = 82f;
        B全选_Clip.Height = 50f;
        B全选.Clip = B全选_Clip;
        
        B全选.UIText = new EntryEngine.UI.UIText();
        B全选.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B全选.UIText.TextAlignment = (EPivot)17;
        B全选.UIText.TextShader = null;
        B全选.UIText.Padding.X = 0f;
        B全选.UIText.Padding.Y = 0f;
        B全选.UIText.FontSize = 22f;
        P登陆按钮面板.Add(B全选);
        P登陆信息面板.Name = "P登陆信息面板";
        EntryEngine.RECT P登陆信息面板_Clip = new EntryEngine.RECT();
        P登陆信息面板_Clip.X = 51f;
        P登陆信息面板_Clip.Y = 77.5f;
        P登陆信息面板_Clip.Width = 1015f;
        P登陆信息面板_Clip.Height = 315f;
        P登陆信息面板.Clip = P登陆信息面板_Clip;
        
        P登陆信息面板.Anchor = (EAnchor)15;
        P登陆按钮面板.Add(P登陆信息面板);
        TB登陆平台信息1.Name = " TB登陆平台信息1";
        EntryEngine.RECT  TB登陆平台信息1_Clip = new EntryEngine.RECT();
        TB登陆平台信息1_Clip.X = 25f;
        TB登陆平台信息1_Clip.Y = 15f;
        TB登陆平台信息1_Clip.Width = 40f;
        TB登陆平台信息1_Clip.Height = 40f;
        TB登陆平台信息1.Clip =  TB登陆平台信息1_Clip;
        
        TB登陆平台信息2.Name = " TB登陆平台信息2";
        EntryEngine.RECT  TB登陆平台信息2_Clip = new EntryEngine.RECT();
        TB登陆平台信息2_Clip.X = 25f;
        TB登陆平台信息2_Clip.Y = 83.5f;
        TB登陆平台信息2_Clip.Width = 40f;
        TB登陆平台信息2_Clip.Height = 40f;
        TB登陆平台信息2.Clip =  TB登陆平台信息2_Clip;
        
        B连接服务端.Name = "B连接服务端";
        EntryEngine.RECT B连接服务端_Clip = new EntryEngine.RECT();
        B连接服务端_Clip.X = 708f;
        B连接服务端_Clip.Y = 168.9817f;
        B连接服务端_Clip.Width = 169f;
        B连接服务端_Clip.Height = 50f;
        B连接服务端.Clip = B连接服务端_Clip;
        
        B连接服务端.UIText = new EntryEngine.UI.UIText();
        B连接服务端.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B连接服务端.UIText.TextAlignment = (EPivot)17;
        B连接服务端.UIText.TextShader = null;
        B连接服务端.UIText.Padding.X = 0f;
        B连接服务端.UIText.Padding.Y = 0f;
        B连接服务端.UIText.FontSize = 22f;
        this.Add(B连接服务端);
        B用户名删除.Name = "B用户名删除";
        EntryEngine.RECT B用户名删除_Clip = new EntryEngine.RECT();
        B用户名删除_Clip.X = 708f;
        B用户名删除_Clip.Y = 27.98169f;
        B用户名删除_Clip.Width = 169f;
        B用户名删除_Clip.Height = 50f;
        B用户名删除.Clip = B用户名删除_Clip;
        
        B用户名删除.UIText = new EntryEngine.UI.UIText();
        B用户名删除.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B用户名删除.UIText.TextAlignment = (EPivot)17;
        B用户名删除.UIText.TextShader = null;
        B用户名删除.UIText.Padding.X = 0f;
        B用户名删除.UIText.Padding.Y = 0f;
        B用户名删除.UIText.FontSize = 22f;
        this.Add(B用户名删除);
        L用户名.Name = "L用户名";
        EntryEngine.RECT L用户名_Clip = new EntryEngine.RECT();
        L用户名_Clip.X = 141f;
        L用户名_Clip.Y = 27.98169f;
        L用户名_Clip.Width = 150f;
        L用户名_Clip.Height = 40f;
        L用户名.Clip = L用户名_Clip;
        
        L用户名.UIText = new EntryEngine.UI.UIText();
        L用户名.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L用户名.UIText.TextAlignment = (EPivot)17;
        L用户名.UIText.TextShader = null;
        L用户名.UIText.Padding.X = 0f;
        L用户名.UIText.Padding.Y = 0f;
        L用户名.UIText.FontSize = 22f;
        this.Add(L用户名);
        DD用户名.Name = "DD用户名";
        EntryEngine.RECT DD用户名_Clip = new EntryEngine.RECT();
        DD用户名_Clip.X = 301.5f;
        DD用户名_Clip.Y = 24.98169f;
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
        TB密码.Name = "TB密码";
        EntryEngine.RECT TB密码_Clip = new EntryEngine.RECT();
        TB密码_Clip.X = 291f;
        TB密码_Clip.Y = 98.98169f;
        TB密码_Clip.Width = 345f;
        TB密码_Clip.Height = 40f;
        TB密码.Clip = TB密码_Clip;
        
        TB密码.DefaultText = new EntryEngine.UI.UIText();
        TB密码.DefaultText.Text = "";
        TB密码.DefaultText.FontColor = new COLOR()
        {
            R = 211,
            G = 211,
            B = 211,
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
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TB密码.UIText.TextAlignment = (EPivot)17;
        TB密码.UIText.TextShader = null;
        TB密码.UIText.Padding.X = 0f;
        TB密码.UIText.Padding.Y = 0f;
        TB密码.UIText.FontSize = 22f;
        TB密码.IsPasswordMode = true;
        this.Add(TB密码);
        TB服务端.Name = "TB服务端";
        EntryEngine.RECT TB服务端_Clip = new EntryEngine.RECT();
        TB服务端_Clip.X = 291f;
        TB服务端_Clip.Y = 168.9817f;
        TB服务端_Clip.Width = 345f;
        TB服务端_Clip.Height = 40f;
        TB服务端.Clip = TB服务端_Clip;
        
        TB服务端.DefaultText = new EntryEngine.UI.UIText();
        TB服务端.DefaultText.Text = "";
        TB服务端.DefaultText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TB服务端.DefaultText.TextAlignment = (EPivot)17;
        TB服务端.DefaultText.TextShader = null;
        TB服务端.DefaultText.Padding.X = 0f;
        TB服务端.DefaultText.Padding.Y = 0f;
        TB服务端.DefaultText.FontSize = 24f;
        TB服务端.UIText = new EntryEngine.UI.UIText();
        TB服务端.UIText.Text = "";
        TB服务端.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TB服务端.UIText.TextAlignment = (EPivot)17;
        TB服务端.UIText.TextShader = null;
        TB服务端.UIText.Padding.X = 0f;
        TB服务端.UIText.Padding.Y = 0f;
        TB服务端.UIText.FontSize = 22f;
        this.Add(TB服务端);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine ___async;
        Entry.ShowDialogScene(S160613103204);
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => this.Background = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0002_用户名-4.png", ___c => L服务端端口.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0002_用户名-4.png", ___c => L密码.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => P登陆按钮面板.Background = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B一键连接.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B一键删除.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0000_登陆全选-6.png", ___c => B全选.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => P登陆信息面板.Background = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B连接服务端.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B用户名删除.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0002_用户名-4.png", ___c => L用户名.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1半透明.mtpatch", ___c => S160613103204.Background = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TB用户名.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1半透明.mtpatch", ___c =>   B用户名1.SourceHover = ___c));
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0004_登陆下拉-2.png", ___c => B160613103427.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0003_登陆输入框-3.png", ___c => TB密码.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0003_登陆输入框-3.png", ___c => TB服务端.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    public void Show(UIScene __scene)
    {
        L服务端端口.UIText.Text = _LANGUAGE.GetString("41");
        L服务端端口.Text = _LANGUAGE.GetString("41");
        L密码.UIText.Text = _LANGUAGE.GetString("9");
        L密码.Text = _LANGUAGE.GetString("9");
        B一键连接.UIText.Text = _LANGUAGE.GetString("42");
        B一键连接.Text = _LANGUAGE.GetString("42");
        B一键删除.UIText.Text = _LANGUAGE.GetString("43");
        B一键删除.Text = _LANGUAGE.GetString("43");
        B全选.UIText.Text = _LANGUAGE.GetString("27");
        B全选.Text = _LANGUAGE.GetString("27");
        B连接服务端.UIText.Text = _LANGUAGE.GetString("44");
        B连接服务端.Text = _LANGUAGE.GetString("44");
        B用户名删除.UIText.Text = _LANGUAGE.GetString("38");
        B用户名删除.Text = _LANGUAGE.GetString("38");
        L用户名.UIText.Text = _LANGUAGE.GetString("45");
        L用户名.Text = _LANGUAGE.GetString("45");
        
    }
}
