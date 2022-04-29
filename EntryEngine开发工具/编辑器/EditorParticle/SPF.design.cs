using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class SPF
{
    public EntryEngine.UI.TextureBox TBFlowType = new EntryEngine.UI.TextureBox();
    public EntryEngine.UI.Button BFlowName = new EntryEngine.UI.Button();
    
    
    private void Initialize()
    {
        this.Name = "SPF";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 200f;
        this_Clip.Height = 40f;
        this.Clip = this_Clip;
        
        TBFlowType.Name = "TBFlowType";
        EntryEngine.RECT TBFlowType_Clip = new EntryEngine.RECT();
        TBFlowType_Clip.X = 4f;
        TBFlowType_Clip.Y = 4f;
        TBFlowType_Clip.Width = 32f;
        TBFlowType_Clip.Height = 32f;
        TBFlowType.Clip = TBFlowType_Clip;
        
        this.Add(TBFlowType);
        BFlowName.Name = "BFlowName";
        EntryEngine.RECT BFlowName_Clip = new EntryEngine.RECT();
        BFlowName_Clip.X = 40f;
        BFlowName_Clip.Y = 4f;
        BFlowName_Clip.Width = 160f;
        BFlowName_Clip.Height = 32f;
        BFlowName.Clip = BFlowName_Clip;
        
        BFlowName.UIText = new EntryEngine.UI.UIText();
        BFlowName.UIText.Text = "#检测：生命";
        BFlowName.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        BFlowName.UIText.TextAlignment = (EPivot)16;
        BFlowName.UIText.TextShader = null;
        BFlowName.UIText.Padding.X = 0f;
        BFlowName.UIText.Padding.Y = 0f;
        BFlowName.UIText.FontSize = 14f;
        BFlowName.Text = "#检测：生命";
        this.Add(BFlowName);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine ___async;
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    private void Show(UIScene __scene)
    {
        
    }
}
