using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S服务信息
{
    public EntryEngine.UI.CheckBox CB单选 = new EntryEngine.UI.CheckBox();
    public EntryEngine.UI.Label L名字 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L服务版本 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L服务器端口版本 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L服务状态 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L时间 = new EntryEngine.UI.Label();
    
    
    private void Initialize()
    {
        this.Name = "S服务信息";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 1160f;
        this_Clip.Height = 65f;
        this.Clip = this_Clip;
        
        CB单选.Name = "CB单选";
        EntryEngine.RECT CB单选_Clip = new EntryEngine.RECT();
        CB单选_Clip.X = 14f;
        CB单选_Clip.Y = 12.5f;
        CB单选_Clip.Width = 40f;
        CB单选_Clip.Height = 40f;
        CB单选.Clip = CB单选_Clip;
        
        CB单选.UIText = new EntryEngine.UI.UIText();
        CB单选.UIText.Text = "";
        CB单选.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        CB单选.UIText.TextAlignment = (EPivot)18;
        CB单选.UIText.TextShader = null;
        CB单选.UIText.Padding.X = 0f;
        CB单选.UIText.Padding.Y = 0f;
        CB单选.UIText.FontSize = 16f;
        this.Add(CB单选);
        L名字.Name = "L名字";
        EntryEngine.RECT L名字_Clip = new EntryEngine.RECT();
        L名字_Clip.X = 71f;
        L名字_Clip.Y = 12.5f;
        L名字_Clip.Width = 119f;
        L名字_Clip.Height = 40f;
        L名字.Clip = L名字_Clip;
        
        L名字.UIText = new EntryEngine.UI.UIText();
        L名字.UIText.Text = "#S1";
        L名字.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L名字.UIText.TextAlignment = (EPivot)17;
        L名字.UIText.TextShader = null;
        L名字.UIText.Padding.X = 0f;
        L名字.UIText.Padding.Y = 0f;
        L名字.UIText.FontSize = 16f;
        L名字.Text = "#S1";
        this.Add(L名字);
        L服务版本.Name = "L服务版本";
        EntryEngine.RECT L服务版本_Clip = new EntryEngine.RECT();
        L服务版本_Clip.X = 203f;
        L服务版本_Clip.Y = 12.5f;
        L服务版本_Clip.Width = 248f;
        L服务版本_Clip.Height = 40f;
        L服务版本.Clip = L服务版本_Clip;
        
        L服务版本.UIText = new EntryEngine.UI.UIText();
        L服务版本.UIText.Text = "#GameServer-1021";
        L服务版本.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L服务版本.UIText.TextAlignment = (EPivot)17;
        L服务版本.UIText.TextShader = null;
        L服务版本.UIText.Padding.X = 0f;
        L服务版本.UIText.Padding.Y = 0f;
        L服务版本.UIText.FontSize = 16f;
        L服务版本.Text = "#GameServer-1021";
        this.Add(L服务版本);
        L服务器端口版本.Name = "L服务器端口版本";
        EntryEngine.RECT L服务器端口版本_Clip = new EntryEngine.RECT();
        L服务器端口版本_Clip.X = 472f;
        L服务器端口版本_Clip.Y = 12.5f;
        L服务器端口版本_Clip.Width = 346f;
        L服务器端口版本_Clip.Height = 40f;
        L服务器端口版本.Clip = L服务器端口版本_Clip;
        
        L服务器端口版本.UIText = new EntryEngine.UI.UIText();
        L服务器端口版本.UIText.Text = "#192.168.1.101:1008-1002";
        L服务器端口版本.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L服务器端口版本.UIText.TextAlignment = (EPivot)17;
        L服务器端口版本.UIText.TextShader = null;
        L服务器端口版本.UIText.Padding.X = 0f;
        L服务器端口版本.UIText.Padding.Y = 0f;
        L服务器端口版本.UIText.FontSize = 16f;
        L服务器端口版本.Text = "#192.168.1.101:1008-1002";
        this.Add(L服务器端口版本);
        L服务状态.Name = "L服务状态";
        EntryEngine.RECT L服务状态_Clip = new EntryEngine.RECT();
        L服务状态_Clip.X = 848f;
        L服务状态_Clip.Y = 12.5f;
        L服务状态_Clip.Width = 82f;
        L服务状态_Clip.Height = 40f;
        L服务状态.Clip = L服务状态_Clip;
        
        L服务状态.UIText = new EntryEngine.UI.UIText();
        L服务状态.UIText.Text = "#运行中";
        L服务状态.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L服务状态.UIText.TextAlignment = (EPivot)17;
        L服务状态.UIText.TextShader = null;
        L服务状态.UIText.Padding.X = 0f;
        L服务状态.UIText.Padding.Y = 0f;
        L服务状态.UIText.FontSize = 16f;
        L服务状态.Text = "#运行中";
        this.Add(L服务状态);
        L时间.Name = "L时间";
        EntryEngine.RECT L时间_Clip = new EntryEngine.RECT();
        L时间_Clip.X = 951f;
        L时间_Clip.Y = 12.5f;
        L时间_Clip.Width = 203f;
        L时间_Clip.Height = 40f;
        L时间.Clip = L时间_Clip;
        
        L时间.UIText = new EntryEngine.UI.UIText();
        L时间.UIText.Text = "#yyyy-MM-dd HH:mm:ss";
        L时间.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L时间.UIText.TextAlignment = (EPivot)17;
        L时间.UIText.TextShader = null;
        L时间.UIText.Padding.X = 0f;
        L时间.UIText.Padding.Y = 0f;
        L时间.UIText.FontSize = 16f;
        L时间.Text = "#yyyy-MM-dd HH:mm:ss";
        this.Add(L时间);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine async;
        ICoroutine ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_单选未选中.png", ___c => CB单选.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_登陆单选.png", ___c => CB单选.SourceClicked = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L名字.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L服务版本.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L服务器端口版本.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L服务状态.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L时间.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    private void Show(EntryEngine.UI.UIScene __scene)
    {
        
    }
}
