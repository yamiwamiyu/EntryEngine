using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S新开服菜单
{
    public EntryEngine.UI.Label L名字 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L执行命令 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Button B确定 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B取消 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Label L服务器 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L类型 = new EntryEngine.UI.Label();
    public EntryEngine.UI.TextBox TB名字 = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.DropDown DD服务类型 = new EntryEngine.UI.DropDown();
    public EntryEngine.UI.Selectable[] S160613103204 = new EntryEngine.UI.Selectable[2]
    {
        new EntryEngine.UI.Selectable(),
        new EntryEngine.UI.Selectable(),
    };
    public EntryEngine.UI.Button[] B160613103427 = new EntryEngine.UI.Button[2]
    {
        new EntryEngine.UI.Button(),
        new EntryEngine.UI.Button(),
    };
    public EntryEngine.UI.DropDown DD服务器 = new EntryEngine.UI.DropDown();
    public EntryEngine.UI.TextBox TB命令 = new EntryEngine.UI.TextBox();
    
    
    private void Initialize()
    {
        this.Name = "S新开服菜单";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 600f;
        this_Clip.Height = 450f;
        this.Clip = this_Clip;
        
        this.ShowPosition = (EShowPosition)2;
        L名字.Name = "L名字";
        EntryEngine.RECT L名字_Clip = new EntryEngine.RECT();
        L名字_Clip.X = 20f;
        L名字_Clip.Y = 20.87482f;
        L名字_Clip.Width = 110f;
        L名字_Clip.Height = 50f;
        L名字.Clip = L名字_Clip;
        
        L名字.UIText = new EntryEngine.UI.UIText();
        L名字.UIText.Text = "名字";
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
        L名字.Text = "名字";
        this.Add(L名字);
        L执行命令.Name = "L执行命令";
        EntryEngine.RECT L执行命令_Clip = new EntryEngine.RECT();
        L执行命令_Clip.X = 20f;
        L执行命令_Clip.Y = 235.8748f;
        L执行命令_Clip.Width = 110f;
        L执行命令_Clip.Height = 50f;
        L执行命令.Clip = L执行命令_Clip;
        
        L执行命令.UIText = new EntryEngine.UI.UIText();
        L执行命令.UIText.Text = "执行命令";
        L执行命令.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L执行命令.UIText.TextAlignment = (EPivot)17;
        L执行命令.UIText.TextShader = null;
        L执行命令.UIText.Padding.X = 0f;
        L执行命令.UIText.Padding.Y = 0f;
        L执行命令.UIText.FontSize = 16f;
        L执行命令.Text = "执行命令";
        this.Add(L执行命令);
        B确定.Name = "B确定";
        EntryEngine.RECT B确定_Clip = new EntryEngine.RECT();
        B确定_Clip.X = 20f;
        B确定_Clip.Y = 305.8748f;
        B确定_Clip.Width = 110f;
        B确定_Clip.Height = 50f;
        B确定.Clip = B确定_Clip;
        
        B确定.UIText = new EntryEngine.UI.UIText();
        B确定.UIText.Text = "确定";
        B确定.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        B确定.UIText.TextAlignment = (EPivot)17;
        B确定.UIText.TextShader = null;
        B确定.UIText.Padding.X = 0f;
        B确定.UIText.Padding.Y = 0f;
        B确定.UIText.FontSize = 16f;
        B确定.Text = "确定";
        this.Add(B确定);
        B取消.Name = "B取消";
        EntryEngine.RECT B取消_Clip = new EntryEngine.RECT();
        B取消_Clip.X = 20f;
        B取消_Clip.Y = 380.8748f;
        B取消_Clip.Width = 110f;
        B取消_Clip.Height = 50f;
        B取消.Clip = B取消_Clip;
        
        B取消.UIText = new EntryEngine.UI.UIText();
        B取消.UIText.Text = "取消";
        B取消.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        B取消.UIText.TextAlignment = (EPivot)17;
        B取消.UIText.TextShader = null;
        B取消.UIText.Padding.X = 0f;
        B取消.UIText.Padding.Y = 0f;
        B取消.UIText.FontSize = 16f;
        B取消.Text = "取消";
        this.Add(B取消);
        L服务器.Name = "L服务器";
        EntryEngine.RECT L服务器_Clip = new EntryEngine.RECT();
        L服务器_Clip.X = 20f;
        L服务器_Clip.Y = 92.87482f;
        L服务器_Clip.Width = 110f;
        L服务器_Clip.Height = 50f;
        L服务器.Clip = L服务器_Clip;
        
        L服务器.UIText = new EntryEngine.UI.UIText();
        L服务器.UIText.Text = "服务器";
        L服务器.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L服务器.UIText.TextAlignment = (EPivot)17;
        L服务器.UIText.TextShader = null;
        L服务器.UIText.Padding.X = 0f;
        L服务器.UIText.Padding.Y = 0f;
        L服务器.UIText.FontSize = 16f;
        L服务器.Text = "服务器";
        this.Add(L服务器);
        L类型.Name = "L类型";
        EntryEngine.RECT L类型_Clip = new EntryEngine.RECT();
        L类型_Clip.X = 20f;
        L类型_Clip.Y = 163.875f;
        L类型_Clip.Width = 110f;
        L类型_Clip.Height = 50f;
        L类型.Clip = L类型_Clip;
        
        L类型.UIText = new EntryEngine.UI.UIText();
        L类型.UIText.Text = "服务类型";
        L类型.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L类型.UIText.TextAlignment = (EPivot)17;
        L类型.UIText.TextShader = null;
        L类型.UIText.Padding.X = 0f;
        L类型.UIText.Padding.Y = 0f;
        L类型.UIText.FontSize = 16f;
        L类型.Text = "服务类型";
        this.Add(L类型);
        TB名字.Name = "TB名字";
        EntryEngine.RECT TB名字_Clip = new EntryEngine.RECT();
        TB名字_Clip.X = 149f;
        TB名字_Clip.Y = 25.87482f;
        TB名字_Clip.Width = 365f;
        TB名字_Clip.Height = 40f;
        TB名字.Clip = TB名字_Clip;
        
        TB名字.CursorAreaColor = new COLOR()
        {
            B = 51,
            G = 153,
            R = 255,
            A = 255,
        };
        TB名字.DefaultText = new EntryEngine.UI.UIText();
        TB名字.DefaultText.Text = "";
        TB名字.DefaultText.FontColor = new COLOR()
        {
            B = 211,
            G = 211,
            R = 211,
            A = 255,
        };
        TB名字.DefaultText.TextAlignment = (EPivot)0;
        TB名字.DefaultText.TextShader = null;
        TB名字.DefaultText.Padding.X = 0f;
        TB名字.DefaultText.Padding.Y = 0f;
        TB名字.DefaultText.FontSize = 16f;
        TB名字.UIText = new EntryEngine.UI.UIText();
        TB名字.UIText.Text = "";
        TB名字.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TB名字.UIText.TextAlignment = (EPivot)17;
        TB名字.UIText.TextShader = null;
        TB名字.UIText.Padding.X = 0f;
        TB名字.UIText.Padding.Y = 0f;
        TB名字.UIText.FontSize = 16f;
        this.Add(TB名字);
        DD服务类型.Name = "DD服务类型";
        EntryEngine.RECT DD服务类型_Clip = new EntryEngine.RECT();
        DD服务类型_Clip.X = 149f;
        DD服务类型_Clip.Y = 168.875f;
        DD服务类型_Clip.Width = 365f;
        DD服务类型_Clip.Height = 40f;
        DD服务类型.Clip = DD服务类型_Clip;
        
        DD服务类型.UIText = new EntryEngine.UI.UIText();
        DD服务类型.UIText.Text = "";
        DD服务类型.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        DD服务类型.UIText.TextAlignment = (EPivot)17;
        DD服务类型.UIText.TextShader = null;
        DD服务类型.UIText.Padding.X = 0f;
        DD服务类型.UIText.Padding.Y = 0f;
        DD服务类型.UIText.FontSize = 16f;
        DD服务类型.DropDownList = S160613103204[0];
        this.Add(DD服务类型);
        S160613103204[0].Name = "S160613103204";
        EntryEngine.RECT S160613103204_0__Clip = new EntryEngine.RECT();
        S160613103204_0__Clip.X = 0f;
        S160613103204_0__Clip.Y = 46f;
        S160613103204_0__Clip.Width = 373f;
        S160613103204_0__Clip.Height = 90f;
        S160613103204[0].Clip = S160613103204_0__Clip;
        
        S160613103204[0].DragMode = (EDragMode)1;
        DD服务类型.Add(S160613103204[0]);
        B160613103427[0].Name = "B160613103427";
        EntryEngine.RECT B160613103427_0__Clip = new EntryEngine.RECT();
        B160613103427_0__Clip.X = 316f;
        B160613103427_0__Clip.Y = 0f;
        B160613103427_0__Clip.Width = 38f;
        B160613103427_0__Clip.Height = 40f;
        B160613103427[0].Clip = B160613103427_0__Clip;
        
        B160613103427[0].UIText = new EntryEngine.UI.UIText();
        B160613103427[0].UIText.Text = "";
        B160613103427[0].UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        B160613103427[0].UIText.TextAlignment = (EPivot)17;
        B160613103427[0].UIText.TextShader = null;
        B160613103427[0].UIText.Padding.X = 0f;
        B160613103427[0].UIText.Padding.Y = 0f;
        B160613103427[0].UIText.FontSize = 16f;
        DD服务类型.Add(B160613103427[0]);
        DD服务器.Name = "DD服务器";
        EntryEngine.RECT DD服务器_Clip = new EntryEngine.RECT();
        DD服务器_Clip.X = 149f;
        DD服务器_Clip.Y = 97.87482f;
        DD服务器_Clip.Width = 365f;
        DD服务器_Clip.Height = 40f;
        DD服务器.Clip = DD服务器_Clip;
        
        DD服务器.UIText = new EntryEngine.UI.UIText();
        DD服务器.UIText.Text = "";
        DD服务器.UIText.FontColor = new COLOR()
        {
            B = 9,
            G = 9,
            R = 9,
            A = 255,
        };
        DD服务器.UIText.TextAlignment = (EPivot)17;
        DD服务器.UIText.TextShader = null;
        DD服务器.UIText.Padding.X = 0f;
        DD服务器.UIText.Padding.Y = 0f;
        DD服务器.UIText.FontSize = 16f;
        DD服务器.DropDownList = S160613103204[1];
        this.Add(DD服务器);
        S160613103204[1].Name = "S160613103204";
        EntryEngine.RECT S160613103204_1__Clip = new EntryEngine.RECT();
        S160613103204_1__Clip.X = 0f;
        S160613103204_1__Clip.Y = 46f;
        S160613103204_1__Clip.Width = 373f;
        S160613103204_1__Clip.Height = 90f;
        S160613103204[1].Clip = S160613103204_1__Clip;
        
        S160613103204[1].DragMode = (EDragMode)1;
        DD服务器.Add(S160613103204[1]);
        B160613103427[1].Name = "B160613103427";
        EntryEngine.RECT B160613103427_1__Clip = new EntryEngine.RECT();
        B160613103427_1__Clip.X = 316f;
        B160613103427_1__Clip.Y = 0f;
        B160613103427_1__Clip.Width = 38f;
        B160613103427_1__Clip.Height = 40f;
        B160613103427[1].Clip = B160613103427_1__Clip;
        
        B160613103427[1].UIText = new EntryEngine.UI.UIText();
        B160613103427[1].UIText.Text = "";
        B160613103427[1].UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        B160613103427[1].UIText.TextAlignment = (EPivot)17;
        B160613103427[1].UIText.TextShader = null;
        B160613103427[1].UIText.Padding.X = 0f;
        B160613103427[1].UIText.Padding.Y = 0f;
        B160613103427[1].UIText.FontSize = 16f;
        DD服务器.Add(B160613103427[1]);
        TB命令.Name = "TB命令";
        EntryEngine.RECT TB命令_Clip = new EntryEngine.RECT();
        TB命令_Clip.X = 149f;
        TB命令_Clip.Y = 235.8748f;
        TB命令_Clip.Width = 365f;
        TB命令_Clip.Height = 190f;
        TB命令.Clip = TB命令_Clip;
        
        TB命令.Color = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TB命令.CursorAreaColor = new COLOR()
        {
            B = 51,
            G = 153,
            R = 255,
            A = 255,
        };
        TB命令.DefaultText = new EntryEngine.UI.UIText();
        TB命令.DefaultText.Text = "";
        TB命令.DefaultText.FontColor = new COLOR()
        {
            B = 211,
            G = 211,
            R = 211,
            A = 255,
        };
        TB命令.DefaultText.TextAlignment = (EPivot)0;
        TB命令.DefaultText.TextShader = null;
        TB命令.DefaultText.Padding.X = 0f;
        TB命令.DefaultText.Padding.Y = 0f;
        TB命令.DefaultText.FontSize = 16f;
        TB命令.UIText = new EntryEngine.UI.UIText();
        TB命令.UIText.Text = "";
        TB命令.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TB命令.UIText.TextAlignment = (EPivot)17;
        TB命令.UIText.TextShader = null;
        TB命令.UIText.Padding.X = 20f;
        TB命令.UIText.Padding.Y = 20f;
        TB命令.UIText.FontSize = 16f;
        TB命令.BreakLine = true;
        TB命令.Multiple = true;
        this.Add(TB命令);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine async;
        ICoroutine ___async;
        Entry.ShowDialogScene(S160613103204[0], EState.Break);
        Entry.ShowDialogScene(S160613103204[1], EState.Break);
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1半透明.mtpatch", ___c => this.Background = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L名字.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L执行命令.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B确定.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B取消.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L服务器.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L类型.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0003_登陆输入框-3.png", ___c => TB名字.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0003_登陆输入框-3.png", ___c => DD服务类型.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1半透明.mtpatch", ___c => S160613103204[0].Background = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0004_登陆下拉-2.png", ___c => B160613103427[0].SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0003_登陆输入框-3.png", ___c => DD服务器.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1半透明.mtpatch", ___c => S160613103204[1].Background = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0004_登陆下拉-2.png", ___c => B160613103427[1].SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TB命令.SourceNormal = ___c));
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
