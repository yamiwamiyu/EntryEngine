using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S服务器面板
{
    public EntryEngine.UI.Button B全选 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Panel P服务器信息面板 = new EntryEngine.UI.Panel();
    public EntryEngine.UI.TextureBox  TB服务器单选1 = new EntryEngine.UI.TextureBox();
    public EntryEngine.UI.TextureBox  TB服务器单选2 = new EntryEngine.UI.TextureBox();
    public EntryEngine.UI.Button B数据统计 = new EntryEngine.UI.Button();
    
    
    private void Initialize()
    {
        this.Name = "S服务器面板";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 1200f;
        this_Clip.Height = 490f;
        this.Clip = this_Clip;
        
        this.Anchor = (EAnchor)15;
        B全选.Name = "B全选";
        EntryEngine.RECT B全选_Clip = new EntryEngine.RECT();
        B全选_Clip.X = 15f;
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
        this.Add(B全选);
        P服务器信息面板.Name = "P服务器信息面板";
        EntryEngine.RECT P服务器信息面板_Clip = new EntryEngine.RECT();
        P服务器信息面板_Clip.X = 15f;
        P服务器信息面板_Clip.Y = 70f;
        P服务器信息面板_Clip.Width = 1158f;
        P服务器信息面板_Clip.Height = 406.875f;
        P服务器信息面板.Clip = P服务器信息面板_Clip;
        
        P服务器信息面板.Anchor = (EAnchor)15;
        this.Add(P服务器信息面板);
        TB服务器单选1.Name = " TB服务器单选1";
        EntryEngine.RECT  TB服务器单选1_Clip = new EntryEngine.RECT();
        TB服务器单选1_Clip.X = 32f;
        TB服务器单选1_Clip.Y = 25f;
        TB服务器单选1_Clip.Width = 40f;
        TB服务器单选1_Clip.Height = 40f;
        TB服务器单选1.Clip =  TB服务器单选1_Clip;
        
        TB服务器单选2.Name = " TB服务器单选2";
        EntryEngine.RECT  TB服务器单选2_Clip = new EntryEngine.RECT();
        TB服务器单选2_Clip.X = 32f;
        TB服务器单选2_Clip.Y = 135f;
        TB服务器单选2_Clip.Width = 40f;
        TB服务器单选2_Clip.Height = 40f;
        TB服务器单选2.Clip =  TB服务器单选2_Clip;
        
        B数据统计.Name = "B数据统计";
        EntryEngine.RECT B数据统计_Clip = new EntryEngine.RECT();
        B数据统计_Clip.X = 1038f;
        B数据统计_Clip.Y = 12f;
        B数据统计_Clip.Width = 135f;
        B数据统计_Clip.Height = 58f;
        B数据统计.Clip = B数据统计_Clip;
        
        B数据统计.Anchor = (EAnchor)9;
        B数据统计.UIText = new EntryEngine.UI.UIText();
        B数据统计.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B数据统计.UIText.TextAlignment = (EPivot)17;
        B数据统计.UIText.TextShader = null;
        B数据统计.UIText.Padding.X = 0f;
        B数据统计.UIText.Padding.Y = 0f;
        B数据统计.UIText.FontSize = 20f;
        this.Add(B数据统计);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0000_登陆全选-6.png", ___c => B全选.SourceNormal = ___c));
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => P服务器信息面板.Background = ___c));
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c =>  TB服务器单选1.Texture = ___c));
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c =>  TB服务器单选2.Texture = ___c));
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0000_登陆全选-6.png", ___c => B数据统计.SourceNormal = ___c));
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    public void Show(UIScene __scene)
    {
        B全选.UIText.Text = _LANGUAGE.GetString("27");
        B全选.Text = _LANGUAGE.GetString("27");
        B数据统计.UIText.Text = _LANGUAGE.GetString("23");
        B数据统计.Text = _LANGUAGE.GetString("23");
        
    }
}
