using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S日志信息
{
    public EntryEngine.UI.Label L时间 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L整合条数 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L日志内容 = new EntryEngine.UI.Label();
    
    
    private void Initialize()
    {
        this.Name = "S日志信息";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 1200f;
        this_Clip.Height = 720f;
        this.Clip = this_Clip;
        
        L时间.Name = "L时间";
        EntryEngine.RECT L时间_Clip = new EntryEngine.RECT();
        L时间_Clip.X = 25f;
        L时间_Clip.Y = 0f;
        L时间_Clip.Width = 160f;
        L时间_Clip.Height = 18.25f;
        L时间.Clip = L时间_Clip;
        
        L时间.UIText = new EntryEngine.UI.UIText();
        L时间.UIText.Text = "#2016/08/09 16:50:55";
        L时间.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L时间.UIText.TextAlignment = (EPivot)0;
        L时间.UIText.TextShader = null;
        L时间.UIText.Padding.X = 0f;
        L时间.UIText.Padding.Y = 0f;
        L时间.UIText.FontSize = 16f;
        L时间.Text = "#2016/08/09 16:50:55";
        this.Add(L时间);
        L整合条数.Name = "L整合条数";
        EntryEngine.RECT L整合条数_Clip = new EntryEngine.RECT();
        L整合条数_Clip.X = 1078f;
        L整合条数_Clip.Y = 0f;
        L整合条数_Clip.Width = 48f;
        L整合条数_Clip.Height = 18.25f;
        L整合条数.Clip = L整合条数_Clip;
        
        L整合条数.UIText = new EntryEngine.UI.UIText();
        L整合条数.UIText.Text = "#99999";
        L整合条数.UIText.FontColor = new COLOR()
        {
            R = 255,
            G = 32,
            B = 32,
            A = 255,
        };
        L整合条数.UIText.TextAlignment = (EPivot)0;
        L整合条数.UIText.TextShader = null;
        L整合条数.UIText.Padding.X = 0f;
        L整合条数.UIText.Padding.Y = 0f;
        L整合条数.UIText.FontSize = 16f;
        L整合条数.Text = "#99999";
        this.Add(L整合条数);
        L日志内容.Name = "L日志内容";
        EntryEngine.RECT L日志内容_Clip = new EntryEngine.RECT();
        L日志内容_Clip.X = 240f;
        L日志内容_Clip.Y = 0f;
        L日志内容_Clip.Width = 791f;
        L日志内容_Clip.Height = 18.25f;
        L日志内容.Clip = L日志内容_Clip;
        
        L日志内容.UIText = new EntryEngine.UI.UIText();
        L日志内容.UIText.Text = "#日志文字内容";
        L日志内容.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L日志内容.UIText.TextAlignment = (EPivot)0;
        L日志内容.UIText.TextShader = null;
        L日志内容.UIText.Padding.X = 0f;
        L日志内容.UIText.Padding.Y = 0f;
        L日志内容.UIText.FontSize = 16f;
        L日志内容.Text = "#日志文字内容";
        L日志内容.BreakLine = true;
        this.Add(L日志内容);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => this.Background = ___c));
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
