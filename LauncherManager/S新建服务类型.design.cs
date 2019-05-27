using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S新建服务类型
{
    public EntryEngine.UI.Label L启动命令 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L启动文件 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label LSVN密码 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label LSVN账号 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label LSVN目录 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label L名字 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Button B确定 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B取消 = new EntryEngine.UI.Button();
    public EntryEngine.UI.TextBox TB名字 = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.TextBox TBSVN目录 = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.TextBox TBSVN账号 = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.TextBox TBSVN密码 = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.TextBox TB启动文件 = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.TextBox TB启动命令 = new EntryEngine.UI.TextBox();
    
    
    private void Initialize()
    {
        this.Name = "S新建服务类型";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 600f;
        this_Clip.Height = 450f;
        this.Clip = this_Clip;
        
        this.ShowPosition = (EShowPosition)2;
        L启动命令.Name = "L启动命令";
        EntryEngine.RECT L启动命令_Clip = new EntryEngine.RECT();
        L启动命令_Clip.X = 16f;
        L启动命令_Clip.Y = 284.8748f;
        L启动命令_Clip.Width = 110f;
        L启动命令_Clip.Height = 40f;
        L启动命令.Clip = L启动命令_Clip;
        
        L启动命令.UIText = new EntryEngine.UI.UIText();
        L启动命令.UIText.Text = "启动命令";
        L启动命令.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L启动命令.UIText.TextAlignment = (EPivot)17;
        L启动命令.UIText.TextShader = null;
        L启动命令.UIText.Padding.X = 0f;
        L启动命令.UIText.Padding.Y = 0f;
        L启动命令.UIText.FontSize = 16f;
        L启动命令.Text = "启动命令";
        this.Add(L启动命令);
        L启动文件.Name = "L启动文件";
        EntryEngine.RECT L启动文件_Clip = new EntryEngine.RECT();
        L启动文件_Clip.X = 16f;
        L启动文件_Clip.Y = 229.8748f;
        L启动文件_Clip.Width = 110f;
        L启动文件_Clip.Height = 40f;
        L启动文件.Clip = L启动文件_Clip;
        
        L启动文件.UIText = new EntryEngine.UI.UIText();
        L启动文件.UIText.Text = "启动文件";
        L启动文件.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L启动文件.UIText.TextAlignment = (EPivot)17;
        L启动文件.UIText.TextShader = null;
        L启动文件.UIText.Padding.X = 0f;
        L启动文件.UIText.Padding.Y = 0f;
        L启动文件.UIText.FontSize = 16f;
        L启动文件.Text = "启动文件";
        this.Add(L启动文件);
        LSVN密码.Name = "LSVN密码";
        EntryEngine.RECT LSVN密码_Clip = new EntryEngine.RECT();
        LSVN密码_Clip.X = 16f;
        LSVN密码_Clip.Y = 176f;
        LSVN密码_Clip.Width = 110f;
        LSVN密码_Clip.Height = 40f;
        LSVN密码.Clip = LSVN密码_Clip;
        
        LSVN密码.UIText = new EntryEngine.UI.UIText();
        LSVN密码.UIText.Text = "SVN密码";
        LSVN密码.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        LSVN密码.UIText.TextAlignment = (EPivot)17;
        LSVN密码.UIText.TextShader = null;
        LSVN密码.UIText.Padding.X = 0f;
        LSVN密码.UIText.Padding.Y = 0f;
        LSVN密码.UIText.FontSize = 16f;
        LSVN密码.Text = "SVN密码";
        this.Add(LSVN密码);
        LSVN账号.Name = "LSVN账号";
        EntryEngine.RECT LSVN账号_Clip = new EntryEngine.RECT();
        LSVN账号_Clip.X = 15f;
        LSVN账号_Clip.Y = 121.8748f;
        LSVN账号_Clip.Width = 110f;
        LSVN账号_Clip.Height = 40f;
        LSVN账号.Clip = LSVN账号_Clip;
        
        LSVN账号.UIText = new EntryEngine.UI.UIText();
        LSVN账号.UIText.Text = "SVN账号";
        LSVN账号.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        LSVN账号.UIText.TextAlignment = (EPivot)17;
        LSVN账号.UIText.TextShader = null;
        LSVN账号.UIText.Padding.X = 0f;
        LSVN账号.UIText.Padding.Y = 0f;
        LSVN账号.UIText.FontSize = 16f;
        LSVN账号.Text = "SVN账号";
        this.Add(LSVN账号);
        LSVN目录.Name = "LSVN目录";
        EntryEngine.RECT LSVN目录_Clip = new EntryEngine.RECT();
        LSVN目录_Clip.X = 15f;
        LSVN目录_Clip.Y = 66.87482f;
        LSVN目录_Clip.Width = 110f;
        LSVN目录_Clip.Height = 40f;
        LSVN目录.Clip = LSVN目录_Clip;
        
        LSVN目录.UIText = new EntryEngine.UI.UIText();
        LSVN目录.UIText.Text = "SVN目录";
        LSVN目录.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        LSVN目录.UIText.TextAlignment = (EPivot)17;
        LSVN目录.UIText.TextShader = null;
        LSVN目录.UIText.Padding.X = 0f;
        LSVN目录.UIText.Padding.Y = 0f;
        LSVN目录.UIText.FontSize = 16f;
        LSVN目录.Text = "SVN目录";
        this.Add(LSVN目录);
        L名字.Name = "L名字";
        EntryEngine.RECT L名字_Clip = new EntryEngine.RECT();
        L名字_Clip.X = 16f;
        L名字_Clip.Y = 12.87482f;
        L名字_Clip.Width = 110f;
        L名字_Clip.Height = 40f;
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
        B确定.Name = "B确定";
        EntryEngine.RECT B确定_Clip = new EntryEngine.RECT();
        B确定_Clip.X = 16f;
        B确定_Clip.Y = 339.8748f;
        B确定_Clip.Width = 110f;
        B确定_Clip.Height = 40f;
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
        B取消_Clip.X = 16f;
        B取消_Clip.Y = 398f;
        B取消_Clip.Width = 110f;
        B取消_Clip.Height = 40f;
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
        TB名字.Name = "TB名字";
        EntryEngine.RECT TB名字_Clip = new EntryEngine.RECT();
        TB名字_Clip.X = 144f;
        TB名字_Clip.Y = 12.87482f;
        TB名字_Clip.Width = 435f;
        TB名字_Clip.Height = 40f;
        TB名字.Clip = TB名字_Clip;
        
        TB名字.Color = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
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
        TBSVN目录.Name = "TBSVN目录";
        EntryEngine.RECT TBSVN目录_Clip = new EntryEngine.RECT();
        TBSVN目录_Clip.X = 144f;
        TBSVN目录_Clip.Y = 66.87482f;
        TBSVN目录_Clip.Width = 435f;
        TBSVN目录_Clip.Height = 40f;
        TBSVN目录.Clip = TBSVN目录_Clip;
        
        TBSVN目录.Color = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TBSVN目录.CursorAreaColor = new COLOR()
        {
            B = 51,
            G = 153,
            R = 255,
            A = 255,
        };
        TBSVN目录.DefaultText = new EntryEngine.UI.UIText();
        TBSVN目录.DefaultText.Text = "";
        TBSVN目录.DefaultText.FontColor = new COLOR()
        {
            B = 211,
            G = 211,
            R = 211,
            A = 255,
        };
        TBSVN目录.DefaultText.TextAlignment = (EPivot)0;
        TBSVN目录.DefaultText.TextShader = null;
        TBSVN目录.DefaultText.Padding.X = 0f;
        TBSVN目录.DefaultText.Padding.Y = 0f;
        TBSVN目录.DefaultText.FontSize = 16f;
        TBSVN目录.UIText = new EntryEngine.UI.UIText();
        TBSVN目录.UIText.Text = "";
        TBSVN目录.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TBSVN目录.UIText.TextAlignment = (EPivot)17;
        TBSVN目录.UIText.TextShader = null;
        TBSVN目录.UIText.Padding.X = 0f;
        TBSVN目录.UIText.Padding.Y = 0f;
        TBSVN目录.UIText.FontSize = 16f;
        TBSVN目录.BreakLine = true;
        this.Add(TBSVN目录);
        TBSVN账号.Name = "TBSVN账号";
        EntryEngine.RECT TBSVN账号_Clip = new EntryEngine.RECT();
        TBSVN账号_Clip.X = 144f;
        TBSVN账号_Clip.Y = 121.8748f;
        TBSVN账号_Clip.Width = 435f;
        TBSVN账号_Clip.Height = 40f;
        TBSVN账号.Clip = TBSVN账号_Clip;
        
        TBSVN账号.Color = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TBSVN账号.CursorAreaColor = new COLOR()
        {
            B = 51,
            G = 153,
            R = 255,
            A = 255,
        };
        TBSVN账号.DefaultText = new EntryEngine.UI.UIText();
        TBSVN账号.DefaultText.Text = "";
        TBSVN账号.DefaultText.FontColor = new COLOR()
        {
            B = 211,
            G = 211,
            R = 211,
            A = 255,
        };
        TBSVN账号.DefaultText.TextAlignment = (EPivot)0;
        TBSVN账号.DefaultText.TextShader = null;
        TBSVN账号.DefaultText.Padding.X = 0f;
        TBSVN账号.DefaultText.Padding.Y = 0f;
        TBSVN账号.DefaultText.FontSize = 16f;
        TBSVN账号.UIText = new EntryEngine.UI.UIText();
        TBSVN账号.UIText.Text = "";
        TBSVN账号.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TBSVN账号.UIText.TextAlignment = (EPivot)17;
        TBSVN账号.UIText.TextShader = null;
        TBSVN账号.UIText.Padding.X = 0f;
        TBSVN账号.UIText.Padding.Y = 0f;
        TBSVN账号.UIText.FontSize = 16f;
        this.Add(TBSVN账号);
        TBSVN密码.Name = "TBSVN密码";
        EntryEngine.RECT TBSVN密码_Clip = new EntryEngine.RECT();
        TBSVN密码_Clip.X = 144f;
        TBSVN密码_Clip.Y = 176f;
        TBSVN密码_Clip.Width = 435f;
        TBSVN密码_Clip.Height = 40f;
        TBSVN密码.Clip = TBSVN密码_Clip;
        
        TBSVN密码.Color = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TBSVN密码.CursorAreaColor = new COLOR()
        {
            B = 51,
            G = 153,
            R = 255,
            A = 255,
        };
        TBSVN密码.DefaultText = new EntryEngine.UI.UIText();
        TBSVN密码.DefaultText.Text = "";
        TBSVN密码.DefaultText.FontColor = new COLOR()
        {
            B = 211,
            G = 211,
            R = 211,
            A = 255,
        };
        TBSVN密码.DefaultText.TextAlignment = (EPivot)0;
        TBSVN密码.DefaultText.TextShader = null;
        TBSVN密码.DefaultText.Padding.X = 0f;
        TBSVN密码.DefaultText.Padding.Y = 0f;
        TBSVN密码.DefaultText.FontSize = 16f;
        TBSVN密码.UIText = new EntryEngine.UI.UIText();
        TBSVN密码.UIText.Text = "";
        TBSVN密码.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TBSVN密码.UIText.TextAlignment = (EPivot)17;
        TBSVN密码.UIText.TextShader = null;
        TBSVN密码.UIText.Padding.X = 0f;
        TBSVN密码.UIText.Padding.Y = 0f;
        TBSVN密码.UIText.FontSize = 16f;
        TBSVN密码.IsPasswordMode = true;
        this.Add(TBSVN密码);
        TB启动文件.Name = "TB启动文件";
        EntryEngine.RECT TB启动文件_Clip = new EntryEngine.RECT();
        TB启动文件_Clip.X = 144f;
        TB启动文件_Clip.Y = 229.8748f;
        TB启动文件_Clip.Width = 435f;
        TB启动文件_Clip.Height = 40f;
        TB启动文件.Clip = TB启动文件_Clip;
        
        TB启动文件.Color = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TB启动文件.CursorAreaColor = new COLOR()
        {
            B = 51,
            G = 153,
            R = 255,
            A = 255,
        };
        TB启动文件.DefaultText = new EntryEngine.UI.UIText();
        TB启动文件.DefaultText.Text = "";
        TB启动文件.DefaultText.FontColor = new COLOR()
        {
            B = 211,
            G = 211,
            R = 211,
            A = 255,
        };
        TB启动文件.DefaultText.TextAlignment = (EPivot)0;
        TB启动文件.DefaultText.TextShader = null;
        TB启动文件.DefaultText.Padding.X = 0f;
        TB启动文件.DefaultText.Padding.Y = 0f;
        TB启动文件.DefaultText.FontSize = 16f;
        TB启动文件.UIText = new EntryEngine.UI.UIText();
        TB启动文件.UIText.Text = "";
        TB启动文件.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TB启动文件.UIText.TextAlignment = (EPivot)17;
        TB启动文件.UIText.TextShader = null;
        TB启动文件.UIText.Padding.X = 0f;
        TB启动文件.UIText.Padding.Y = 0f;
        TB启动文件.UIText.FontSize = 16f;
        this.Add(TB启动文件);
        TB启动命令.Name = "TB启动命令";
        EntryEngine.RECT TB启动命令_Clip = new EntryEngine.RECT();
        TB启动命令_Clip.X = 144f;
        TB启动命令_Clip.Y = 284.8748f;
        TB启动命令_Clip.Width = 435f;
        TB启动命令_Clip.Height = 150f;
        TB启动命令.Clip = TB启动命令_Clip;
        
        TB启动命令.Color = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TB启动命令.CursorAreaColor = new COLOR()
        {
            B = 51,
            G = 153,
            R = 255,
            A = 255,
        };
        TB启动命令.DefaultText = new EntryEngine.UI.UIText();
        TB启动命令.DefaultText.Text = "";
        TB启动命令.DefaultText.FontColor = new COLOR()
        {
            B = 211,
            G = 211,
            R = 211,
            A = 255,
        };
        TB启动命令.DefaultText.TextAlignment = (EPivot)0;
        TB启动命令.DefaultText.TextShader = null;
        TB启动命令.DefaultText.Padding.X = 0f;
        TB启动命令.DefaultText.Padding.Y = 0f;
        TB启动命令.DefaultText.FontSize = 16f;
        TB启动命令.UIText = new EntryEngine.UI.UIText();
        TB启动命令.UIText.Text = "";
        TB启动命令.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TB启动命令.UIText.TextAlignment = (EPivot)17;
        TB启动命令.UIText.TextShader = null;
        TB启动命令.UIText.Padding.X = 0f;
        TB启动命令.UIText.Padding.Y = 0f;
        TB启动命令.UIText.FontSize = 16f;
        TB启动命令.BreakLine = true;
        TB启动命令.Multiple = true;
        this.Add(TB启动命令);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine async;
        ICoroutine ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L启动命令.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L启动文件.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => LSVN密码.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => LSVN账号.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => LSVN目录.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => L名字.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B确定.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0005_删除-1.png", ___c => B取消.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TB名字.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TBSVN目录.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TBSVN账号.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TBSVN密码.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TB启动文件.SourceNormal = ___c));
        if (___async != null && !___async.IsEnd) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TB启动命令.SourceNormal = ___c));
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
