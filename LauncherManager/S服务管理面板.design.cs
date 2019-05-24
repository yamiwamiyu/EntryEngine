using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S服务管理面板
{
    public EntryEngine.UI.Button B执行命令 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B开启服务 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B关闭服务 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B更新服务 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B新设更改服务 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B删除服务 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Label L名字 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L服务版本 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L服务器端口版本 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L时间 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L状态 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L服务个数 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L服务 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Panel P服务信息面板 = new EntryEngine.UI.Panel();
    public EntryEngine.UI.TextureBox  TB服务信息面板2 = new EntryEngine.UI.TextureBox();
    public EntryEngine.UI.TextureBox  TB服务信息面板 = new EntryEngine.UI.TextureBox();
    public EntryEngine.UI.Button B同服务器 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B同类型 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B全选 = new EntryEngine.UI.Button();
    
    
    private void Initialize()
    {
        this.Name = "S服务管理面板";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 1200f;
        this_Clip.Height = 490f;
        this.Clip = this_Clip;
        
        this.Anchor = (EAnchor)15;
        B执行命令.Name = "B执行命令";
        EntryEngine.RECT B执行命令_Clip = new EntryEngine.RECT();
        B执行命令_Clip.X = 620f;
        B执行命令_Clip.Y = 16.875f;
        B执行命令_Clip.Width = 130f;
        B执行命令_Clip.Height = 35f;
        B执行命令.Clip = B执行命令_Clip;
        
        B执行命令.UIText = new EntryEngine.UI.UIText();
        B执行命令.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B执行命令.UIText.TextAlignment = (EPivot)17;
        B执行命令.UIText.TextShader = null;
        B执行命令.UIText.Padding.X = 0f;
        B执行命令.UIText.Padding.Y = 0f;
        B执行命令.UIText.FontSize = 18f;
        this.Add(B执行命令);
        B开启服务.Name = "B开启服务";
        EntryEngine.RECT B开启服务_Clip = new EntryEngine.RECT();
        B开启服务_Clip.X = 449f;
        B开启服务_Clip.Y = 16.875f;
        B开启服务_Clip.Width = 130f;
        B开启服务_Clip.Height = 35f;
        B开启服务.Clip = B开启服务_Clip;
        
        B开启服务.UIText = new EntryEngine.UI.UIText();
        B开启服务.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B开启服务.UIText.TextAlignment = (EPivot)17;
        B开启服务.UIText.TextShader = null;
        B开启服务.UIText.Padding.X = 0f;
        B开启服务.UIText.Padding.Y = 0f;
        B开启服务.UIText.FontSize = 18f;
        this.Add(B开启服务);
        B关闭服务.Name = "B关闭服务";
        EntryEngine.RECT B关闭服务_Clip = new EntryEngine.RECT();
        B关闭服务_Clip.X = 282f;
        B关闭服务_Clip.Y = 16.875f;
        B关闭服务_Clip.Width = 130f;
        B关闭服务_Clip.Height = 35f;
        B关闭服务.Clip = B关闭服务_Clip;
        
        B关闭服务.UIText = new EntryEngine.UI.UIText();
        B关闭服务.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B关闭服务.UIText.TextAlignment = (EPivot)17;
        B关闭服务.UIText.TextShader = null;
        B关闭服务.UIText.Padding.X = 0f;
        B关闭服务.UIText.Padding.Y = 0f;
        B关闭服务.UIText.FontSize = 18f;
        this.Add(B关闭服务);
        B更新服务.Name = "B更新服务";
        EntryEngine.RECT B更新服务_Clip = new EntryEngine.RECT();
        B更新服务_Clip.X = 113f;
        B更新服务_Clip.Y = 16.875f;
        B更新服务_Clip.Width = 130f;
        B更新服务_Clip.Height = 35f;
        B更新服务.Clip = B更新服务_Clip;
        
        B更新服务.UIText = new EntryEngine.UI.UIText();
        B更新服务.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B更新服务.UIText.TextAlignment = (EPivot)17;
        B更新服务.UIText.TextShader = null;
        B更新服务.UIText.Padding.X = 0f;
        B更新服务.UIText.Padding.Y = 0f;
        B更新服务.UIText.FontSize = 18f;
        this.Add(B更新服务);
        B新设更改服务.Name = "B新设更改服务";
        EntryEngine.RECT B新设更改服务_Clip = new EntryEngine.RECT();
        B新设更改服务_Clip.X = 790f;
        B新设更改服务_Clip.Y = 16.875f;
        B新设更改服务_Clip.Width = 130f;
        B新设更改服务_Clip.Height = 35f;
        B新设更改服务.Clip = B新设更改服务_Clip;
        
        B新设更改服务.UIText = new EntryEngine.UI.UIText();
        B新设更改服务.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B新设更改服务.UIText.TextAlignment = (EPivot)17;
        B新设更改服务.UIText.TextShader = null;
        B新设更改服务.UIText.Padding.X = 0f;
        B新设更改服务.UIText.Padding.Y = 0f;
        B新设更改服务.UIText.FontSize = 18f;
        this.Add(B新设更改服务);
        B删除服务.Name = "B删除服务";
        EntryEngine.RECT B删除服务_Clip = new EntryEngine.RECT();
        B删除服务_Clip.X = 963f;
        B删除服务_Clip.Y = 16.875f;
        B删除服务_Clip.Width = 130f;
        B删除服务_Clip.Height = 35f;
        B删除服务.Clip = B删除服务_Clip;
        
        B删除服务.UIText = new EntryEngine.UI.UIText();
        B删除服务.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B删除服务.UIText.TextAlignment = (EPivot)17;
        B删除服务.UIText.TextShader = null;
        B删除服务.UIText.Padding.X = 0f;
        B删除服务.UIText.Padding.Y = 0f;
        B删除服务.UIText.FontSize = 18f;
        this.Add(B删除服务);
        L名字.Name = "L名字";
        EntryEngine.RECT L名字_Clip = new EntryEngine.RECT();
        L名字_Clip.X = 119.5f;
        L名字_Clip.Y = 69f;
        L名字_Clip.Width = 65f;
        L名字_Clip.Height = 45f;
        L名字.Clip = L名字_Clip;
        
        L名字.UIText = new EntryEngine.UI.UIText();
        L名字.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L名字.UIText.TextAlignment = (EPivot)17;
        L名字.UIText.TextShader = null;
        L名字.UIText.Padding.X = 0f;
        L名字.UIText.Padding.Y = 0f;
        L名字.UIText.FontSize = 22f;
        this.Add(L名字);
        L服务版本.Name = "L服务版本";
        EntryEngine.RECT L服务版本_Clip = new EntryEngine.RECT();
        L服务版本_Clip.X = 251.6285f;
        L服务版本_Clip.Y = 69f;
        L服务版本_Clip.Width = 197.3715f;
        L服务版本_Clip.Height = 45f;
        L服务版本.Clip = L服务版本_Clip;
        
        L服务版本.UIText = new EntryEngine.UI.UIText();
        L服务版本.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L服务版本.UIText.TextAlignment = (EPivot)17;
        L服务版本.UIText.TextShader = null;
        L服务版本.UIText.Padding.X = 0f;
        L服务版本.UIText.Padding.Y = 0f;
        L服务版本.UIText.FontSize = 22f;
        this.Add(L服务版本);
        L服务器端口版本.Name = "L服务器端口版本";
        EntryEngine.RECT L服务器端口版本_Clip = new EntryEngine.RECT();
        L服务器端口版本_Clip.X = 501f;
        L服务器端口版本_Clip.Y = 69f;
        L服务器端口版本_Clip.Width = 320f;
        L服务器端口版本_Clip.Height = 45f;
        L服务器端口版本.Clip = L服务器端口版本_Clip;
        
        L服务器端口版本.UIText = new EntryEngine.UI.UIText();
        L服务器端口版本.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L服务器端口版本.UIText.TextAlignment = (EPivot)17;
        L服务器端口版本.UIText.TextShader = null;
        L服务器端口版本.UIText.Padding.X = 0f;
        L服务器端口版本.UIText.Padding.Y = 0f;
        L服务器端口版本.UIText.FontSize = 22f;
        this.Add(L服务器端口版本);
        L时间.Name = "L时间";
        EntryEngine.RECT L时间_Clip = new EntryEngine.RECT();
        L时间_Clip.X = 990f;
        L时间_Clip.Y = 69f;
        L时间_Clip.Width = 159f;
        L时间_Clip.Height = 45f;
        L时间.Clip = L时间_Clip;
        
        L时间.UIText = new EntryEngine.UI.UIText();
        L时间.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L时间.UIText.TextAlignment = (EPivot)17;
        L时间.UIText.TextShader = null;
        L时间.UIText.Padding.X = 0f;
        L时间.UIText.Padding.Y = 0f;
        L时间.UIText.FontSize = 22f;
        this.Add(L时间);
        L状态.Name = "L状态";
        EntryEngine.RECT L状态_Clip = new EntryEngine.RECT();
        L状态_Clip.X = 871.25f;
        L状态_Clip.Y = 69f;
        L状态_Clip.Width = 75.5f;
        L状态_Clip.Height = 45f;
        L状态.Clip = L状态_Clip;
        
        L状态.UIText = new EntryEngine.UI.UIText();
        L状态.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L状态.UIText.TextAlignment = (EPivot)17;
        L状态.UIText.TextShader = null;
        L状态.UIText.Padding.X = 0f;
        L状态.UIText.Padding.Y = 0f;
        L状态.UIText.FontSize = 22f;
        this.Add(L状态);
        L服务个数.Name = "L服务个数";
        EntryEngine.RECT L服务个数_Clip = new EntryEngine.RECT();
        L服务个数_Clip.X = 946.75f;
        L服务个数_Clip.Y = 438.875f;
        L服务个数_Clip.Width = 240f;
        L服务个数_Clip.Height = 40f;
        L服务个数.Clip = L服务个数_Clip;
        
        L服务个数.Anchor = (EAnchor)9;
        L服务个数.UIText = new EntryEngine.UI.UIText();
        L服务个数.UIText.Text = "#1/35";
        L服务个数.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L服务个数.UIText.TextAlignment = (EPivot)17;
        L服务个数.UIText.TextShader = null;
        L服务个数.UIText.Padding.X = 0f;
        L服务个数.UIText.Padding.Y = 0f;
        L服务个数.UIText.FontSize = 20f;
        L服务个数.Text = "#1/35";
        this.Add(L服务个数);
        L服务.Name = "L服务";
        EntryEngine.RECT L服务_Clip = new EntryEngine.RECT();
        L服务_Clip.X = 846.75f;
        L服务_Clip.Y = 438.875f;
        L服务_Clip.Width = 100f;
        L服务_Clip.Height = 40f;
        L服务.Clip = L服务_Clip;
        
        L服务.Anchor = (EAnchor)9;
        L服务.UIText = new EntryEngine.UI.UIText();
        L服务.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L服务.UIText.TextAlignment = (EPivot)17;
        L服务.UIText.TextShader = null;
        L服务.UIText.Padding.X = 0f;
        L服务.UIText.Padding.Y = 0f;
        L服务.UIText.FontSize = 22f;
        this.Add(L服务);
        P服务信息面板.Name = "P服务信息面板";
        EntryEngine.RECT P服务信息面板_Clip = new EntryEngine.RECT();
        P服务信息面板_Clip.X = 15f;
        P服务信息面板_Clip.Y = 118.875f;
        P服务信息面板_Clip.Width = 1170f;
        P服务信息面板_Clip.Height = 320f;
        P服务信息面板.Clip = P服务信息面板_Clip;
        
        P服务信息面板.Anchor = (EAnchor)15;
        P服务信息面板.DragMode = (EDragMode)1;
        this.Add(P服务信息面板);
        TB服务信息面板2.Name = " TB服务信息面板2";
        EntryEngine.RECT  TB服务信息面板2_Clip = new EntryEngine.RECT();
        TB服务信息面板2_Clip.X = 0f;
        TB服务信息面板2_Clip.Y = 64.375f;
        TB服务信息面板2_Clip.Width = 100f;
        TB服务信息面板2_Clip.Height = 34.875f;
        TB服务信息面板2.Clip =  TB服务信息面板2_Clip;
        
        TB服务信息面板.Name = " TB服务信息面板";
        EntryEngine.RECT  TB服务信息面板_Clip = new EntryEngine.RECT();
        TB服务信息面板_Clip.X = 0f;
        TB服务信息面板_Clip.Y = 0f;
        TB服务信息面板_Clip.Width = 100f;
        TB服务信息面板_Clip.Height = 34.875f;
        TB服务信息面板.Clip =  TB服务信息面板_Clip;
        
        B同服务器.Name = "B同服务器";
        EntryEngine.RECT B同服务器_Clip = new EntryEngine.RECT();
        B同服务器_Clip.X = 324f;
        B同服务器_Clip.Y = 438.875f;
        B同服务器_Clip.Width = 130f;
        B同服务器_Clip.Height = 40f;
        B同服务器.Clip = B同服务器_Clip;
        
        B同服务器.Anchor = (EAnchor)9;
        B同服务器.UIText = new EntryEngine.UI.UIText();
        B同服务器.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B同服务器.UIText.TextAlignment = (EPivot)17;
        B同服务器.UIText.TextShader = null;
        B同服务器.UIText.Padding.X = 0f;
        B同服务器.UIText.Padding.Y = 0f;
        B同服务器.UIText.FontSize = 18f;
        this.Add(B同服务器);
        B同类型.Name = "B同类型";
        EntryEngine.RECT B同类型_Clip = new EntryEngine.RECT();
        B同类型_Clip.X = 186.6285f;
        B同类型_Clip.Y = 438.875f;
        B同类型_Clip.Width = 130f;
        B同类型_Clip.Height = 40f;
        B同类型.Clip = B同类型_Clip;
        
        B同类型.Anchor = (EAnchor)9;
        B同类型.UIText = new EntryEngine.UI.UIText();
        B同类型.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B同类型.UIText.TextAlignment = (EPivot)17;
        B同类型.UIText.TextShader = null;
        B同类型.UIText.Padding.X = 0f;
        B同类型.UIText.Padding.Y = 0f;
        B同类型.UIText.FontSize = 18f;
        this.Add(B同类型);
        B全选.Name = "B全选";
        EntryEngine.RECT B全选_Clip = new EntryEngine.RECT();
        B全选_Clip.X = 48f;
        B全选_Clip.Y = 438.875f;
        B全选_Clip.Width = 130f;
        B全选_Clip.Height = 40f;
        B全选.Clip = B全选_Clip;
        
        B全选.Anchor = (EAnchor)9;
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
        B全选.UIText.FontSize = 18f;
        this.Add(B全选);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => this.Background = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B执行命令.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B开启服务.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B关闭服务.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B更新服务.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B新设更改服务.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B删除服务.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L名字.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L服务版本.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L服务器端口版本.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L时间.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L状态.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L服务个数.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L服务.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => P服务信息面板.Background = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c =>  TB服务信息面板2.Texture = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c =>  TB服务信息面板.Texture = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B同服务器.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B同类型.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B全选.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    public void Show(UIScene __scene)
    {
        B执行命令.UIText.Text = _LANGUAGE.GetString("12");
        B执行命令.Text = _LANGUAGE.GetString("12");
        B开启服务.UIText.Text = _LANGUAGE.GetString("32");
        B开启服务.Text = _LANGUAGE.GetString("32");
        B关闭服务.UIText.Text = _LANGUAGE.GetString("33");
        B关闭服务.Text = _LANGUAGE.GetString("33");
        B更新服务.UIText.Text = _LANGUAGE.GetString("34");
        B更新服务.Text = _LANGUAGE.GetString("34");
        B新设更改服务.UIText.Text = _LANGUAGE.GetString("35");
        B新设更改服务.Text = _LANGUAGE.GetString("35");
        B删除服务.UIText.Text = _LANGUAGE.GetString("75");
        B删除服务.Text = _LANGUAGE.GetString("75");
        L名字.UIText.Text = _LANGUAGE.GetString("6");
        L名字.Text = _LANGUAGE.GetString("6");
        L服务版本.UIText.Text = _LANGUAGE.GetString("80");
        L服务版本.Text = _LANGUAGE.GetString("80");
        L服务器端口版本.UIText.Text = _LANGUAGE.GetString("79");
        L服务器端口版本.Text = _LANGUAGE.GetString("79");
        L时间.UIText.Text = _LANGUAGE.GetString("78");
        L时间.Text = _LANGUAGE.GetString("78");
        L状态.UIText.Text = _LANGUAGE.GetString("30");
        L状态.Text = _LANGUAGE.GetString("30");
        L服务.UIText.Text = _LANGUAGE.GetString("16");
        L服务.Text = _LANGUAGE.GetString("16");
        B同服务器.UIText.Text = _LANGUAGE.GetString("107");
        B同服务器.Text = _LANGUAGE.GetString("107");
        B同类型.UIText.Text = _LANGUAGE.GetString("108");
        B同类型.Text = _LANGUAGE.GetString("108");
        B全选.UIText.Text = _LANGUAGE.GetString("27");
        B全选.Text = _LANGUAGE.GetString("27");
        
    }
}
