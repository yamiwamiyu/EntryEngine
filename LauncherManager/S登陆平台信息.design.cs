using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S登陆平台信息
{
    public EntryEngine.UI.CheckBox CB单选 = new EntryEngine.UI.CheckBox();
    public EntryEngine.UI.Label L平台信息 = new EntryEngine.UI.Label();
    
    
    private void Initialize()
    {
        this.Name = "S登陆平台信息";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 550f;
        this_Clip.Height = 60f;
        this.Clip = this_Clip;
        
        CB单选.Name = "CB单选";
        EntryEngine.RECT CB单选_Clip = new EntryEngine.RECT();
        CB单选_Clip.X = 11f;
        CB单选_Clip.Y = 10f;
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
        L平台信息.Name = "L平台信息";
        EntryEngine.RECT L平台信息_Clip = new EntryEngine.RECT();
        L平台信息_Clip.X = 61f;
        L平台信息_Clip.Y = 10f;
        L平台信息_Clip.Width = 470f;
        L平台信息_Clip.Height = 40f;
        L平台信息.Clip = L平台信息_Clip;
        
        L平台信息.UIText = new EntryEngine.UI.UIText();
        L平台信息.UIText.Text = "";
        L平台信息.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L平台信息.UIText.TextAlignment = (EPivot)16;
        L平台信息.UIText.TextShader = null;
        L平台信息.UIText.Padding.X = 0f;
        L平台信息.UIText.Padding.Y = 0f;
        L平台信息.UIText.FontSize = 20f;
        this.Add(L平台信息);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_单选未选中.png", ___c => CB单选.SourceNormal = ___c));
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_登陆单选.png", ___c => CB单选.SourceClicked = ___c));
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L平台信息.SourceNormal = ___c));
        
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    public void Show(UIScene __scene)
    {
        
    }
}
