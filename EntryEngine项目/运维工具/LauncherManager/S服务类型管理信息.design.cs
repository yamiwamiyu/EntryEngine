using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S服务类型管理信息
{
    public EntryEngine.UI.Button B修改 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B删除 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Label L服务类型名字 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L服务地址 = new EntryEngine.UI.Label();
    
    
    private void Initialize()
    {
        this.Name = "S服务类型管理信息";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 990f;
        this_Clip.Height = 70f;
        this.Clip = this_Clip;
        
        B修改.Name = "B修改";
        EntryEngine.RECT B修改_Clip = new EntryEngine.RECT();
        B修改_Clip.X = 772.0001f;
        B修改_Clip.Y = 12.5f;
        B修改_Clip.Width = 90f;
        B修改_Clip.Height = 45f;
        B修改.Clip = B修改_Clip;
        
        B修改.UIText = new EntryEngine.UI.UIText();
        B修改.UIText.Text = "修改";
        B修改.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        B修改.UIText.TextAlignment = (EPivot)17;
        B修改.UIText.TextShader = null;
        B修改.UIText.Padding.X = 0f;
        B修改.UIText.Padding.Y = 0f;
        B修改.UIText.FontSize = 16f;
        B修改.Text = "修改";
        this.Add(B修改);
        B删除.Name = "B删除";
        EntryEngine.RECT B删除_Clip = new EntryEngine.RECT();
        B删除_Clip.X = 874f;
        B删除_Clip.Y = 12.5f;
        B删除_Clip.Width = 90f;
        B删除_Clip.Height = 45f;
        B删除.Clip = B删除_Clip;
        
        B删除.UIText = new EntryEngine.UI.UIText();
        B删除.UIText.Text = "删除";
        B删除.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        B删除.UIText.TextAlignment = (EPivot)17;
        B删除.UIText.TextShader = null;
        B删除.UIText.Padding.X = 0f;
        B删除.UIText.Padding.Y = 0f;
        B删除.UIText.FontSize = 16f;
        B删除.Text = "删除";
        this.Add(B删除);
        L服务类型名字.Name = "L服务类型名字";
        EntryEngine.RECT L服务类型名字_Clip = new EntryEngine.RECT();
        L服务类型名字_Clip.X = 15.00012f;
        L服务类型名字_Clip.Y = 12.5f;
        L服务类型名字_Clip.Width = 222.9999f;
        L服务类型名字_Clip.Height = 45f;
        L服务类型名字.Clip = L服务类型名字_Clip;
        
        L服务类型名字.UIText = new EntryEngine.UI.UIText();
        L服务类型名字.UIText.Text = "#GameCrossServer";
        L服务类型名字.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L服务类型名字.UIText.TextAlignment = (EPivot)17;
        L服务类型名字.UIText.TextShader = null;
        L服务类型名字.UIText.Padding.X = 0f;
        L服务类型名字.UIText.Padding.Y = 0f;
        L服务类型名字.UIText.FontSize = 16f;
        L服务类型名字.Text = "#GameCrossServer";
        this.Add(L服务类型名字);
        L服务地址.Name = "L服务地址";
        EntryEngine.RECT L服务地址_Clip = new EntryEngine.RECT();
        L服务地址_Clip.X = 256f;
        L服务地址_Clip.Y = 12.5f;
        L服务地址_Clip.Width = 501f;
        L服务地址_Clip.Height = 45f;
        L服务地址.Clip = L服务地址_Clip;
        
        L服务地址.UIText = new EntryEngine.UI.UIText();
        L服务地址.UIText.Text = "#Http://YamiwaStudio/svn/WWC";
        L服务地址.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L服务地址.UIText.TextAlignment = (EPivot)17;
        L服务地址.UIText.TextShader = null;
        L服务地址.UIText.Padding.X = 0f;
        L服务地址.UIText.Padding.Y = 0f;
        L服务地址.UIText.FontSize = 16f;
        L服务地址.Text = "#Http://YamiwaStudio/svn/WWC";
        L服务地址.BreakLine = true;
        this.Add(L服务地址);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine async;
        ICoroutine ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B修改.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B删除.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L服务类型名字.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L服务地址.SourceNormal = ___c));
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
