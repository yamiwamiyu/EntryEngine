using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class SManagerLog
{
    public EntryEngine.UI.Label   LLog = new EntryEngine.UI.Label();
    private EntryEngine.UI.Label ___LLog()
    {
        var LLog = new EntryEngine.UI.Label();
        LLog.Name = "  LLog";
        EntryEngine.RECT   LLog_Clip = new EntryEngine.RECT();
        LLog_Clip.X = 0f;
        LLog_Clip.Y = 0f;
        LLog_Clip.Width = 480f;
        LLog_Clip.Height = 22.875f;
        LLog.Clip =   LLog_Clip;
        
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c =>   LLog.SourceNormal = ___c));
        LLog.UIText = new EntryEngine.UI.UIText();
        LLog.UIText.Text = "#Label";
        LLog.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        LLog.UIText.TextAlignment = (EPivot)0;
        LLog.UIText.TextShader = null;
        LLog.UIText.Padding.X = 0f;
        LLog.UIText.Padding.Y = 0f;
        LLog.UIText.FontSize = 16f;
        
        LLog.Text = "#Label";
        LLog.BreakLine = true;
        
        return LLog;
    }
    
    
    private void Initialize()
    {
        this.Name = "SManagerLog";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 480f;
        this_Clip.Height = 200f;
        this.Clip = this_Clip;
        
        this.DragMode = (EDragMode)2;
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine async;
        ICoroutine ___async;
        LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c =>   LLog.SourceNormal = ___c));
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    private void Show(EntryEngine.UI.UIScene __scene)
    {
        
    }
}
