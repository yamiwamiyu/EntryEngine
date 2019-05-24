using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S服务器信息
{
    public EntryEngine.UI.CheckBox CB单选 = new EntryEngine.UI.CheckBox();
    public EntryEngine.UI.Panel P服务器信息面板 = new EntryEngine.UI.Panel();
    public EntryEngine.UI.Label[] L160602101048 = new EntryEngine.UI.Label[5]
    {
        new EntryEngine.UI.Label(),
        new EntryEngine.UI.Label(),
        new EntryEngine.UI.Label(),
        new EntryEngine.UI.Label(),
        new EntryEngine.UI.Label(),
    };
    public EntryEngine.UI.Label LIP = new EntryEngine.UI.Label();
    public EntryEngine.UI.Button BUpdate = new EntryEngine.UI.Button();
    public EntryEngine.UI.Label L内存 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label LCPU = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L人数 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L服务 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L硬盘 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L160602101049 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L网络 = new EntryEngine.UI.Label();
    
    
    private void Initialize()
    {
        this.Name = "S服务器信息";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 1050f;
        this_Clip.Height = 120f;
        this.Clip = this_Clip;
        
        CB单选.Name = "CB单选";
        EntryEngine.RECT CB单选_Clip = new EntryEngine.RECT();
        CB单选_Clip.X = 27f;
        CB单选_Clip.Y = 42.875f;
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
        P服务器信息面板.Name = "P服务器信息面板";
        EntryEngine.RECT P服务器信息面板_Clip = new EntryEngine.RECT();
        P服务器信息面板_Clip.X = 92f;
        P服务器信息面板_Clip.Y = 7.5f;
        P服务器信息面板_Clip.Width = 940f;
        P服务器信息面板_Clip.Height = 105f;
        P服务器信息面板.Clip = P服务器信息面板_Clip;
        
        this.Add(P服务器信息面板);
        L160602101048[0].Name = "L160602101048";
        EntryEngine.RECT L160602101048_0__Clip = new EntryEngine.RECT();
        L160602101048_0__Clip.X = 602f;
        L160602101048_0__Clip.Y = 8f;
        L160602101048_0__Clip.Width = 80f;
        L160602101048_0__Clip.Height = 25f;
        L160602101048[0].Clip = L160602101048_0__Clip;
        
        L160602101048[0].UIText = new EntryEngine.UI.UIText();
        L160602101048[0].UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L160602101048[0].UIText.TextAlignment = (EPivot)17;
        L160602101048[0].UIText.TextShader = null;
        L160602101048[0].UIText.Padding.X = 0f;
        L160602101048[0].UIText.Padding.Y = 0f;
        L160602101048[0].UIText.FontSize = 20f;
        P服务器信息面板.Add(L160602101048[0]);
        L160602101048[1].Name = "L160602101048";
        EntryEngine.RECT L160602101048_1__Clip = new EntryEngine.RECT();
        L160602101048_1__Clip.X = 602f;
        L160602101048_1__Clip.Y = 41.375f;
        L160602101048_1__Clip.Width = 80f;
        L160602101048_1__Clip.Height = 25f;
        L160602101048[1].Clip = L160602101048_1__Clip;
        
        L160602101048[1].UIText = new EntryEngine.UI.UIText();
        L160602101048[1].UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L160602101048[1].UIText.TextAlignment = (EPivot)17;
        L160602101048[1].UIText.TextShader = null;
        L160602101048[1].UIText.Padding.X = 0f;
        L160602101048[1].UIText.Padding.Y = 0f;
        L160602101048[1].UIText.FontSize = 20f;
        P服务器信息面板.Add(L160602101048[1]);
        L160602101048[2].Name = "L160602101048";
        EntryEngine.RECT L160602101048_2__Clip = new EntryEngine.RECT();
        L160602101048_2__Clip.X = 255f;
        L160602101048_2__Clip.Y = 8f;
        L160602101048_2__Clip.Width = 80f;
        L160602101048_2__Clip.Height = 25f;
        L160602101048[2].Clip = L160602101048_2__Clip;
        
        L160602101048[2].UIText = new EntryEngine.UI.UIText();
        L160602101048[2].UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L160602101048[2].UIText.TextAlignment = (EPivot)17;
        L160602101048[2].UIText.TextShader = null;
        L160602101048[2].UIText.Padding.X = 0f;
        L160602101048[2].UIText.Padding.Y = 0f;
        L160602101048[2].UIText.FontSize = 20f;
        P服务器信息面板.Add(L160602101048[2]);
        LIP.Name = "LIP";
        EntryEngine.RECT LIP_Clip = new EntryEngine.RECT();
        LIP_Clip.X = 25f;
        LIP_Clip.Y = 8f;
        LIP_Clip.Width = 200f;
        LIP_Clip.Height = 25f;
        LIP.Clip = LIP_Clip;
        
        LIP.UIText = new EntryEngine.UI.UIText();
        LIP.UIText.Text = "#192.168.1.101";
        LIP.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        LIP.UIText.TextAlignment = (EPivot)17;
        LIP.UIText.TextShader = null;
        LIP.UIText.Padding.X = 0f;
        LIP.UIText.Padding.Y = 0f;
        LIP.UIText.FontSize = 20f;
        LIP.Text = "#192.168.1.101";
        P服务器信息面板.Add(LIP);
        BUpdate.Name = "BUpdate";
        EntryEngine.RECT BUpdate_Clip = new EntryEngine.RECT();
        BUpdate_Clip.X = 40.5f;
        BUpdate_Clip.Y = 41.375f;
        BUpdate_Clip.Width = 169f;
        BUpdate_Clip.Height = 50f;
        BUpdate.Clip = BUpdate_Clip;
        
        BUpdate.UIText = new EntryEngine.UI.UIText();
        BUpdate.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        BUpdate.UIText.TextAlignment = (EPivot)17;
        BUpdate.UIText.TextShader = null;
        BUpdate.UIText.Padding.X = 0f;
        BUpdate.UIText.Padding.Y = 0f;
        BUpdate.UIText.FontSize = 22f;
        P服务器信息面板.Add(BUpdate);
        L内存.Name = "L内存";
        EntryEngine.RECT L内存_Clip = new EntryEngine.RECT();
        L内存_Clip.X = 682f;
        L内存_Clip.Y = 8f;
        L内存_Clip.Width = 200f;
        L内存_Clip.Height = 25f;
        L内存.Clip = L内存_Clip;
        
        L内存.UIText = new EntryEngine.UI.UIText();
        L内存.UIText.Text = "#3578MB/8192MB";
        L内存.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L内存.UIText.TextAlignment = (EPivot)17;
        L内存.UIText.TextShader = null;
        L内存.UIText.Padding.X = 0f;
        L内存.UIText.Padding.Y = 0f;
        L内存.UIText.FontSize = 20f;
        L内存.Text = "#3578MB/8192MB";
        P服务器信息面板.Add(L内存);
        LCPU.Name = "LCPU";
        EntryEngine.RECT LCPU_Clip = new EntryEngine.RECT();
        LCPU_Clip.X = 682f;
        LCPU_Clip.Y = 41.375f;
        LCPU_Clip.Width = 200f;
        LCPU_Clip.Height = 25f;
        LCPU.Clip = LCPU_Clip;
        
        LCPU.UIText = new EntryEngine.UI.UIText();
        LCPU.UIText.Text = "#35%";
        LCPU.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        LCPU.UIText.TextAlignment = (EPivot)17;
        LCPU.UIText.TextShader = null;
        LCPU.UIText.Padding.X = 0f;
        LCPU.UIText.Padding.Y = 0f;
        LCPU.UIText.FontSize = 20f;
        LCPU.Text = "#35%";
        P服务器信息面板.Add(LCPU);
        L160602101048[3].Name = "L160602101048";
        EntryEngine.RECT L160602101048_3__Clip = new EntryEngine.RECT();
        L160602101048_3__Clip.X = 255f;
        L160602101048_3__Clip.Y = 41.375f;
        L160602101048_3__Clip.Width = 80f;
        L160602101048_3__Clip.Height = 25f;
        L160602101048[3].Clip = L160602101048_3__Clip;
        
        L160602101048[3].UIText = new EntryEngine.UI.UIText();
        L160602101048[3].UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L160602101048[3].UIText.TextAlignment = (EPivot)17;
        L160602101048[3].UIText.TextShader = null;
        L160602101048[3].UIText.Padding.X = 0f;
        L160602101048[3].UIText.Padding.Y = 0f;
        L160602101048[3].UIText.FontSize = 20f;
        P服务器信息面板.Add(L160602101048[3]);
        L人数.Name = "L人数";
        EntryEngine.RECT L人数_Clip = new EntryEngine.RECT();
        L人数_Clip.X = 335f;
        L人数_Clip.Y = 41.375f;
        L人数_Clip.Width = 200f;
        L人数_Clip.Height = 25f;
        L人数.Clip = L人数_Clip;
        
        L人数.UIText = new EntryEngine.UI.UIText();
        L人数.UIText.Text = "#1254";
        L人数.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L人数.UIText.TextAlignment = (EPivot)17;
        L人数.UIText.TextShader = null;
        L人数.UIText.Padding.X = 0f;
        L人数.UIText.Padding.Y = 0f;
        L人数.UIText.FontSize = 20f;
        L人数.Text = "#1254";
        P服务器信息面板.Add(L人数);
        L服务.Name = "L服务";
        EntryEngine.RECT L服务_Clip = new EntryEngine.RECT();
        L服务_Clip.X = 335f;
        L服务_Clip.Y = 8f;
        L服务_Clip.Width = 200f;
        L服务_Clip.Height = 25f;
        L服务.Clip = L服务_Clip;
        
        L服务.UIText = new EntryEngine.UI.UIText();
        L服务.UIText.Text = "#4 / 5";
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
        L服务.UIText.FontSize = 20f;
        P服务器信息面板.Add(L服务);
        L硬盘.Name = "L硬盘";
        EntryEngine.RECT L硬盘_Clip = new EntryEngine.RECT();
        L硬盘_Clip.X = 682f;
        L硬盘_Clip.Y = 75.375f;
        L硬盘_Clip.Width = 200f;
        L硬盘_Clip.Height = 25f;
        L硬盘.Clip = L硬盘_Clip;
        
        L硬盘.UIText = new EntryEngine.UI.UIText();
        L硬盘.UIText.Text = "#3.2Mpbs/s";
        L硬盘.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L硬盘.UIText.TextAlignment = (EPivot)17;
        L硬盘.UIText.TextShader = null;
        L硬盘.UIText.Padding.X = 0f;
        L硬盘.UIText.Padding.Y = 0f;
        L硬盘.UIText.FontSize = 20f;
        L硬盘.Text = "#3.2Mpbs/s";
        P服务器信息面板.Add(L硬盘);
        L160602101049.Name = "L160602101049";
        EntryEngine.RECT L160602101049_Clip = new EntryEngine.RECT();
        L160602101049_Clip.X = 602f;
        L160602101049_Clip.Y = 75.375f;
        L160602101049_Clip.Width = 80f;
        L160602101049_Clip.Height = 25f;
        L160602101049.Clip = L160602101049_Clip;
        
        L160602101049.UIText = new EntryEngine.UI.UIText();
        L160602101049.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L160602101049.UIText.TextAlignment = (EPivot)17;
        L160602101049.UIText.TextShader = null;
        L160602101049.UIText.Padding.X = 0f;
        L160602101049.UIText.Padding.Y = 0f;
        L160602101049.UIText.FontSize = 20f;
        P服务器信息面板.Add(L160602101049);
        L160602101048[4].Name = "L160602101048";
        EntryEngine.RECT L160602101048_4__Clip = new EntryEngine.RECT();
        L160602101048_4__Clip.X = 255f;
        L160602101048_4__Clip.Y = 75.375f;
        L160602101048_4__Clip.Width = 80f;
        L160602101048_4__Clip.Height = 25f;
        L160602101048[4].Clip = L160602101048_4__Clip;
        
        L160602101048[4].UIText = new EntryEngine.UI.UIText();
        L160602101048[4].UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L160602101048[4].UIText.TextAlignment = (EPivot)17;
        L160602101048[4].UIText.TextShader = null;
        L160602101048[4].UIText.Padding.X = 0f;
        L160602101048[4].UIText.Padding.Y = 0f;
        L160602101048[4].UIText.FontSize = 20f;
        P服务器信息面板.Add(L160602101048[4]);
        L网络.Name = "L网络";
        EntryEngine.RECT L网络_Clip = new EntryEngine.RECT();
        L网络_Clip.X = 335f;
        L网络_Clip.Y = 75.375f;
        L网络_Clip.Width = 200f;
        L网络_Clip.Height = 25f;
        L网络.Clip = L网络_Clip;
        
        L网络.UIText = new EntryEngine.UI.UIText();
        L网络.UIText.Text = "#3.2Mpbs/s";
        L网络.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L网络.UIText.TextAlignment = (EPivot)17;
        L网络.UIText.TextShader = null;
        L网络.UIText.Padding.X = 0f;
        L网络.UIText.Padding.Y = 0f;
        L网络.UIText.FontSize = 20f;
        L网络.Text = "#3.2Mpbs/s";
        P服务器信息面板.Add(L网络);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_单选未选中.png", ___c => CB单选.SourceNormal = ___c));
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_登陆单选.png", ___c => CB单选.SourceClicked = ___c));
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => P服务器信息面板.Background = ___c));
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L160602101048[0].SourceNormal = ___c));
        
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L160602101048[1].SourceNormal = ___c));
        
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L160602101048[2].SourceNormal = ___c));
        
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => LIP.SourceNormal = ___c));
        
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => BUpdate.SourceNormal = ___c));
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L内存.SourceNormal = ___c));
        
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => LCPU.SourceNormal = ___c));
        
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L160602101048[3].SourceNormal = ___c));
        
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L人数.SourceNormal = ___c));
        
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L服务.SourceNormal = ___c));
        
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L硬盘.SourceNormal = ___c));
        
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L160602101049.SourceNormal = ___c));
        
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L160602101048[4].SourceNormal = ___c));
        
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L网络.SourceNormal = ___c));
        
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    public void Show(UIScene __scene)
    {
        L160602101048[0].UIText.Text = _LANGUAGE.GetString("17");
        L160602101048[0].Text = _LANGUAGE.GetString("17");
        L160602101048[1].UIText.Text = _LANGUAGE.GetString("18");
        L160602101048[1].Text = _LANGUAGE.GetString("18");
        L160602101048[2].UIText.Text = _LANGUAGE.GetString("16");
        L160602101048[2].Text = _LANGUAGE.GetString("16");
        BUpdate.UIText.Text = _LANGUAGE.GetString("83");
        BUpdate.Text = _LANGUAGE.GetString("83");
        L160602101048[3].UIText.Text = _LANGUAGE.GetString("84");
        L160602101048[3].Text = _LANGUAGE.GetString("84");
        L160602101049.UIText.Text = _LANGUAGE.GetString("85");
        L160602101049.Text = _LANGUAGE.GetString("85");
        L160602101048[4].UIText.Text = _LANGUAGE.GetString("19");
        L160602101048[4].Text = _LANGUAGE.GetString("19");
        
    }
}
