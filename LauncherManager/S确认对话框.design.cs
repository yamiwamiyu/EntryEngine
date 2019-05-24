using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S确认对话框
{
    public EntryEngine.UI.Panel P确认面板 = new EntryEngine.UI.Panel();
    public EntryEngine.UI.Button B取消 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B确定 = new EntryEngine.UI.Button();
    public EntryEngine.UI.TextBox TBText = new EntryEngine.UI.TextBox();
    
    
    private void Initialize()
    {
        this.Name = "S确认对话框";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 600f;
        this_Clip.Height = 350f;
        this.Clip = this_Clip;
        
        this.ShowPosition = (EShowPosition)1;
        P确认面板.Name = "P确认面板";
        EntryEngine.RECT P确认面板_Clip = new EntryEngine.RECT();
        P确认面板_Clip.X = 10f;
        P确认面板_Clip.Y = 4f;
        P确认面板_Clip.Width = 580f;
        P确认面板_Clip.Height = 340f;
        P确认面板.Clip = P确认面板_Clip;
        
        this.Add(P确认面板);
        B取消.Name = "B取消";
        EntryEngine.RECT B取消_Clip = new EntryEngine.RECT();
        B取消_Clip.X = 291f;
        B取消_Clip.Y = 278f;
        B取消_Clip.Width = 120f;
        B取消_Clip.Height = 55f;
        B取消.Clip = B取消_Clip;
        
        B取消.UIText = new EntryEngine.UI.UIText();
        B取消.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B取消.UIText.TextAlignment = (EPivot)17;
        B取消.UIText.TextShader = null;
        B取消.UIText.Padding.X = 0f;
        B取消.UIText.Padding.Y = 0f;
        B取消.UIText.FontSize = 22f;
        P确认面板.Add(B取消);
        B确定.Name = "B确定";
        EntryEngine.RECT B确定_Clip = new EntryEngine.RECT();
        B确定_Clip.X = 152f;
        B确定_Clip.Y = 278f;
        B确定_Clip.Width = 120f;
        B确定_Clip.Height = 55f;
        B确定.Clip = B确定_Clip;
        
        B确定.UIText = new EntryEngine.UI.UIText();
        B确定.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B确定.UIText.TextAlignment = (EPivot)17;
        B确定.UIText.TextShader = null;
        B确定.UIText.Padding.X = 0f;
        B确定.UIText.Padding.Y = 0f;
        B确定.UIText.FontSize = 22f;
        P确认面板.Add(B确定);
        TBText.Name = "TBText";
        EntryEngine.RECT TBText_Clip = new EntryEngine.RECT();
        TBText_Clip.X = 16f;
        TBText_Clip.Y = 13.875f;
        TBText_Clip.Width = 548f;
        TBText_Clip.Height = 252f;
        TBText.Clip = TBText_Clip;
        
        TBText.Color = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TBText.DefaultText = new EntryEngine.UI.UIText();
        TBText.DefaultText.Text = "";
        TBText.DefaultText.FontColor = new COLOR()
        {
            R = 211,
            G = 211,
            B = 211,
            A = 255,
        };
        TBText.DefaultText.TextAlignment = (EPivot)0;
        TBText.DefaultText.TextShader = null;
        TBText.DefaultText.Padding.X = 0f;
        TBText.DefaultText.Padding.Y = 0f;
        TBText.DefaultText.FontSize = 16f;
        TBText.UIText = new EntryEngine.UI.UIText();
        TBText.UIText.Text = "#Text";
        TBText.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TBText.UIText.TextAlignment = (EPivot)17;
        TBText.UIText.TextShader = null;
        TBText.UIText.Padding.X = 0f;
        TBText.UIText.Padding.Y = 0f;
        TBText.UIText.FontSize = 22f;
        TBText.Text = "#Text";
        TBText.BreakLine = true;
        TBText.Multiple = true;
        P确认面板.Add(TBText);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1半透明.mtpatch", ___c => P确认面板.Background = ___c));
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B取消.SourceNormal = ___c));
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B确定.SourceNormal = ___c));
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TBText.SourceNormal = ___c));
        
        
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    public void Show(UIScene __scene)
    {
        B取消.UIText.Text = _LANGUAGE.GetString("8");
        B取消.Text = _LANGUAGE.GetString("8");
        B确定.UIText.Text = _LANGUAGE.GetString("7");
        B确定.Text = _LANGUAGE.GetString("7");
        
    }
}
