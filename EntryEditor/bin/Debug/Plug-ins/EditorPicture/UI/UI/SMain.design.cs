using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class SMain
{
    public EntryEngine.UI.Label LDirectory = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label LSize = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L缩放 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label LPosition = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label LHelp = new EntryEngine.UI.Label();
    
    
    private void Initialize()
    {
        this.Name = "SMain";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 1280f;
        this_Clip.Height = 720f;
        this.Clip = this_Clip;
        
        this.Color = new COLOR()
        {
            B = 77,
            G = 77,
            R = 77,
            A = 255,
        };
        this.DragMode = (EDragMode)1;
        LDirectory.Name = "LDirectory";
        EntryEngine.RECT LDirectory_Clip = new EntryEngine.RECT();
        LDirectory_Clip.X = 15f;
        LDirectory_Clip.Y = 13.375f;
        LDirectory_Clip.Width = 0;
        LDirectory_Clip.Height = 0;
        LDirectory.Clip = LDirectory_Clip;
        
        LDirectory.UIText = new EntryEngine.UI.UIText();
        LDirectory.UIText.Text = "工作目录";
        LDirectory.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        LDirectory.UIText.TextAlignment = (EPivot)0;
        LDirectory.UIText.TextShader = null;
        LDirectory.UIText.Padding.X = 0f;
        LDirectory.UIText.Padding.Y = 0f;
        LDirectory.UIText.FontSize = 14f;
        LDirectory.Text = "工作目录";
        this.Add(LDirectory);
        LSize.Name = "LSize";
        EntryEngine.RECT LSize_Clip = new EntryEngine.RECT();
        LSize_Clip.X = 11f;
        LSize_Clip.Y = 15.375f;
        LSize_Clip.Width = 0;
        LSize_Clip.Height = 0;
        LSize.Clip = LSize_Clip;
        
        LSize.UIText = new EntryEngine.UI.UIText();
        LSize.UIText.Text = "画布尺寸";
        LSize.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        LSize.UIText.TextAlignment = (EPivot)0;
        LSize.UIText.TextShader = null;
        LSize.UIText.Padding.X = 0f;
        LSize.UIText.Padding.Y = 0f;
        LSize.UIText.FontSize = 12f;
        LSize.Text = "画布尺寸";
        this.Add(LSize);
        L缩放.Name = "L缩放";
        EntryEngine.RECT L缩放_Clip = new EntryEngine.RECT();
        L缩放_Clip.X = 5f;
        L缩放_Clip.Y = 24.25f;
        L缩放_Clip.Width = 0;
        L缩放_Clip.Height = 0;
        L缩放.Clip = L缩放_Clip;
        
        L缩放.UIText = new EntryEngine.UI.UIText();
        L缩放.UIText.Text = "缩放";
        L缩放.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        L缩放.UIText.TextAlignment = (EPivot)0;
        L缩放.UIText.TextShader = null;
        L缩放.UIText.Padding.X = 0f;
        L缩放.UIText.Padding.Y = 0f;
        L缩放.UIText.FontSize = 12f;
        L缩放.Text = "缩放";
        this.Add(L缩放);
        LPosition.Name = "LPosition";
        EntryEngine.RECT LPosition_Clip = new EntryEngine.RECT();
        LPosition_Clip.X = 5f;
        LPosition_Clip.Y = 42.9375f;
        LPosition_Clip.Width = 0;
        LPosition_Clip.Height = 0;
        LPosition.Clip = LPosition_Clip;
        
        LPosition.UIText = new EntryEngine.UI.UIText();
        LPosition.UIText.Text = "坐标";
        LPosition.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        LPosition.UIText.TextAlignment = (EPivot)0;
        LPosition.UIText.TextShader = null;
        LPosition.UIText.Padding.X = 0f;
        LPosition.UIText.Padding.Y = 0f;
        LPosition.UIText.FontSize = 12f;
        LPosition.Text = "坐标";
        this.Add(LPosition);
        LHelp.Name = "LHelp";
        EntryEngine.RECT LHelp_Clip = new EntryEngine.RECT();
        LHelp_Clip.X = 5f;
        LHelp_Clip.Y = 3f;
        LHelp_Clip.Width = 0;
        LHelp_Clip.Height = 0;
        LHelp.Clip = LHelp_Clip;
        
        LHelp.UIText = new EntryEngine.UI.UIText();
        LHelp.UIText.Text = "操作方法";
        LHelp.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        LHelp.UIText.TextAlignment = (EPivot)0;
        LHelp.UIText.TextShader = null;
        LHelp.UIText.Padding.X = 0f;
        LHelp.UIText.Padding.Y = 0f;
        LHelp.UIText.FontSize = 14f;
        LHelp.Text = "操作方法";
        this.Add(LHelp);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine async;
        ICoroutine ___async;
        this.Background = TEXTURE.Pixel;
        
        
        
        
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    private void Show(EntryEngine.UI.UIScene __scene)
    {
        
    }
}
