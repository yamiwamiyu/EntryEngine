using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S服务类型管理
{
    public EntryEngine.UI.Panel P服务类型信息面板 = new EntryEngine.UI.Panel();
    public EntryEngine.UI.TextureBox  TB服务类管理信息 = new EntryEngine.UI.TextureBox();
    public EntryEngine.UI.TextureBox  TB服务类管理信息2 = new EntryEngine.UI.TextureBox();
    public EntryEngine.UI.Button B新建 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B关闭 = new EntryEngine.UI.Button();
    
    
    private void Initialize()
    {
        this.Name = "S服务类型管理";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 1000f;
        this_Clip.Height = 500f;
        this.Clip = this_Clip;
        
        this.Anchor = (EAnchor)15;
        this.ShowPosition = (EShowPosition)1;
        P服务类型信息面板.Name = "P服务类型信息面板";
        EntryEngine.RECT P服务类型信息面板_Clip = new EntryEngine.RECT();
        P服务类型信息面板_Clip.X = 5f;
        P服务类型信息面板_Clip.Y = 8.875f;
        P服务类型信息面板_Clip.Width = 990f;
        P服务类型信息面板_Clip.Height = 410f;
        P服务类型信息面板.Clip = P服务类型信息面板_Clip;
        
        P服务类型信息面板.Anchor = (EAnchor)15;
        this.Add(P服务类型信息面板);
        TB服务类管理信息.Name = " TB服务类管理信息";
        EntryEngine.RECT  TB服务类管理信息_Clip = new EntryEngine.RECT();
        TB服务类管理信息_Clip.X = 0f;
        TB服务类管理信息_Clip.Y = 21f;
        TB服务类管理信息_Clip.Width = 100f;
        TB服务类管理信息_Clip.Height = 24f;
        TB服务类管理信息.Clip =  TB服务类管理信息_Clip;
        
        TB服务类管理信息2.Name = " TB服务类管理信息2";
        EntryEngine.RECT  TB服务类管理信息2_Clip = new EntryEngine.RECT();
        TB服务类管理信息2_Clip.X = 0f;
        TB服务类管理信息2_Clip.Y = 73f;
        TB服务类管理信息2_Clip.Width = 100f;
        TB服务类管理信息2_Clip.Height = 24f;
        TB服务类管理信息2.Clip =  TB服务类管理信息2_Clip;
        
        B新建.Name = "B新建";
        EntryEngine.RECT B新建_Clip = new EntryEngine.RECT();
        B新建_Clip.X = 287.0002f;
        B新建_Clip.Y = 431.875f;
        B新建_Clip.Width = 165f;
        B新建_Clip.Height = 50f;
        B新建.Clip = B新建_Clip;
        
        B新建.Anchor = (EAnchor)9;
        B新建.UIText = new EntryEngine.UI.UIText();
        B新建.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B新建.UIText.TextAlignment = (EPivot)17;
        B新建.UIText.TextShader = null;
        B新建.UIText.Padding.X = 0f;
        B新建.UIText.Padding.Y = 0f;
        B新建.UIText.FontSize = 20f;
        this.Add(B新建);
        B关闭.Name = "B关闭";
        EntryEngine.RECT B关闭_Clip = new EntryEngine.RECT();
        B关闭_Clip.X = 516f;
        B关闭_Clip.Y = 431.875f;
        B关闭_Clip.Width = 165f;
        B关闭_Clip.Height = 50f;
        B关闭.Clip = B关闭_Clip;
        
        B关闭.Anchor = (EAnchor)9;
        B关闭.UIText = new EntryEngine.UI.UIText();
        B关闭.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B关闭.UIText.TextAlignment = (EPivot)17;
        B关闭.UIText.TextShader = null;
        B关闭.UIText.Padding.X = 0f;
        B关闭.UIText.Padding.Y = 0f;
        B关闭.UIText.FontSize = 20f;
        this.Add(B关闭);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1半透明.mtpatch", ___c => this.Background = ___c));
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => P服务类型信息面板.Background = ___c));
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c =>  TB服务类管理信息.Texture = ___c));
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c =>  TB服务类管理信息2.Texture = ___c));
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B新建.SourceNormal = ___c));
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B关闭.SourceNormal = ___c));
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    public void Show(UIScene __scene)
    {
        B新建.UIText.Text = _LANGUAGE.GetString("37");
        B新建.Text = _LANGUAGE.GetString("37");
        B关闭.UIText.Text = _LANGUAGE.GetString("39");
        B关闭.Text = _LANGUAGE.GetString("39");
        
    }
}
