using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using LauncherProtocolStructure;
using LauncherManager;
using System;
using LauncherManagerProtocol;

public partial class S日志 : UIScene
{
    const int PAGE_COUNT = 30;
    [Code(ECode.LessUseful)]
    class LogPage
    {
        public int Page;
        public int Pages;
        public LogRecord[] Record;
    }

    Pool<S日志信息> scenes = new Pool<S日志信息>();
    Service service;
    Maintainer maintainer;
    int page;
    int pages;
    //List<LogPage> pages = new List<LogPage>();

    Label PageCurrent
    {
        get { return LPage[LPage.Length / 2]; }
    }

    public S日志()
    {
        Initialize();
        B查询.Clicked += new DUpdate<UIElement>(B查询_Clicked);
        B整合.Clicked += new DUpdate<UIElement>(B整合_Clicked);
        B退出.Clicked += new DUpdate<UIElement>(B退出_Clicked);
        B前一页.Clicked += new DUpdate<UIElement>(B前一页_Clicked);
        B下一页.Clicked += new DUpdate<UIElement>(B下一页_Clicked);
        B首页.Clicked += new DUpdate<UIElement>(B首页_Clicked);
        B末页.Clicked += new DUpdate<UIElement>(B末页_Clicked);
        int current = LPage.Length / 2;
        for (int i = 0; i < LPage.Length; i++)
        {
            // 当前页不跳转
            if (i == current)
                continue;
            int index = i - current;
            LPage[i].Clicked += (sender, e) =>
            {
                page += index;
                B查询_Clicked(sender, e);
            };
        }
        PhaseEnding += new Action<UIScene>(S日志_PhaseEnding);
        P日志消息.Scroll += new DUpdate<Panel>(P日志消息_Scroll);
    }

    void P日志消息_Scroll(Panel sender, Entry e)
    {
        float offset = P日志消息.OffsetY;
        float y = 0;
        int i;
        for (i = 0; i < P日志消息.ChildCount; i++)
        {
            y += P日志消息[i].Height;
            if (y > offset)
            {
                y -= P日志消息[i].Height;
                break;
            }
            P日志消息[i].Visible = false;
        }
        offset += P日志消息.Height;
        for (; i < P日志消息.ChildCount; i++)
        {
            P日志消息[i].Visible = y < offset;
            y += P日志消息[i].Height;
        }
    }
    void B末页_Clicked(UIElement sender, Entry e)
    {
        if (page + 1 < pages)
        {
            page = pages - 1;
            B查询_Clicked(sender, e);
        }
    }
    void B首页_Clicked(UIElement sender, Entry e)
    {
        if (page > 0)
        {
            page = 0;
            B查询_Clicked(sender, e);
        }
    }
    void B下一页_Clicked(UIElement sender, Entry e)
    {
        if (page + 1 < pages)
        {
            page++;
            B查询_Clicked(sender, e);
        }
    }
    void B前一页_Clicked(UIElement sender, Entry e)
    {
        if (page > 0)
        {
            page--;
            B查询_Clicked(sender, e);
        }
    }
    void S日志_PhaseEnding(UIScene obj)
    {
        Close();
    }
    void B退出_Clicked(UIElement sender, Entry e)
    {
        Entry.ShowMainScene<S服务器管理>().NoRefresh = true;
    }
    void B整合_Clicked(UIElement sender, Entry e)
    {
        DateTime? start = null;
        DateTime? end = null;
        string content;
        string param;
        byte[] level;
        if (GetParameter(out start, out end, out content, out param, out level))
            maintainer.Proxy.GroupLog(service.Name, start, end, content, param, level);
    }
    void B查询_Clicked(UIElement sender, Entry e)
    {
        DateTime? start = null;
        DateTime? end = null;
        string content;
        string param;
        byte[] level;
        if (GetParameter(out start, out end, out content, out param, out level))
            maintainer.Proxy.GetLog(service.Name, start, end, PAGE_COUNT, page, content, param, level);
    }
    bool GetParameter(out DateTime? start, out DateTime? end, out string content, out string param, out byte[] level)
    {
        start = null;
        end = null;

        content = TB内容筛选.Text;
        param = TB参数筛选.Text;

        List<byte> levels = new List<byte>();
        if (CBDebug.Checked)
            levels.Add(0);
        if (CBInfo.Checked)
            levels.Add(1);
        if (CBWarning.Checked)
            levels.Add(2);
        if (CBError.Checked)
            levels.Add(3);
        if (levels.Count == 0)
            for (byte i = 0; i < 4; i++)
                levels.Add(i);

        level = levels.ToArray();

        try
        {
            if (!string.IsNullOrEmpty(TB起始时间.Text))
                start = DateTime.Parse(TB起始时间.Text);

            if (!string.IsNullOrEmpty(TB结束时间.Text))
                end = DateTime.Parse(TB结束时间.Text);
        }
        catch (Exception ex)
        {
            S确认对话框.Hint("日期格式错误，正确格式[yyyy-MM-dd HH:mm:ss]");
            return false;
        }

        return true;
    }
    void GetLog()
    {
        Close();

        int page = Maintainer._OnGetLog.page;
        int pages = Maintainer._OnGetLog.pages;
        var log = Maintainer._OnGetLog.logs;

        if (log.IsEmpty())
        {
            S确认对话框.Hint("没有日志");
            return;
        }

        this.page = page;
        if (pages != 0)
            this.pages = pages;
        for (int i = 0; i < LPage.Length; i++)
        {
            int p = page + i - LPage.Length / 2;
            if (p >= 0 && p < pages)
            {
                LPage[i].Text = (p + 1).ToString();
                LPage[i].Visible = true;
            }
        }

        float y = 0;
        int index = 0;
        foreach (var item in log)
        {
            var scene = scenes.Allot();
            if (scene == null)
            {
                scene = new S日志信息();
                scenes.Add(scene);

                scene.L整合条数.DoubleClick += new DUpdate<UIElement>(Group_DoubleClick);
                scene.DoubleClick += new DUpdate<UIElement>(scene_DoubleClick);
            }

            scene.Y = y;
            scene.Record = item;
            scene.Index = index;

            P日志消息.Add(scene);
            Entry.ShowDialogScene(scene, EState.Break);

            scene.L时间.Text = scene.Record.Record.Time.ToString("yyyy-MM-dd HH:mm:ss");
            scene.L日志内容.Text = scene.Record.Record.ToString();
            if (scene.Record.Count > 1)
                scene.L整合条数.Text = scene.Record.Count.ToString();
            else
                scene.L整合条数.Text = null;

            if (scene.Record.Record.Level > 3)
                scene.L日志内容.UIText.FontColor = S日志信息.LevelColor[4];
            else
                scene.L日志内容.UIText.FontColor = S日志信息.LevelColor[scene.Record.Record.Level];

            index++;
            float height = scene.L日志内容.ContentSize.Y;
            scene.Height = height;
            y += height;

            //if (scenes.Count > 56)
            //    return;
        }
        P日志消息.ContentScope = new VECTOR2(0, y);
        P日志消息.OffsetY = 1;
        P日志消息.OffsetY = 0;
    }
    void GetLogRepeat()
    {
        var data = IManagerAgent._OnGetLogRepeat.data;
        LogRecord[] records = new LogRecord[data.Records.Length];
        for (int i = 0; i < records.Length; i++)
        {
            records[i] = new LogRecord();
            records[i].Count = records.Length;
            records[i].Record = new Record()
            {
                Level = data.Level,
                Content = data.Content,
                Time = data.Records[i].Time,
                Params = data.Records[i].Param,
            };
        }
        IManagerAgent._OnGetLog.page = 0;
        IManagerAgent._OnGetLog.pages = 1;
        IManagerAgent._OnGetLog.logs = records;
        GetLog();
    }
    void Group_DoubleClick(UIElement sender, Entry e)
    {
        S日志信息 scene = (S日志信息)sender.Parent;
        if (scene.Record.Count > 1)
        {
            maintainer.Proxy.GetLogRepeat(scene.Index);
            Handle();
        }
    }
    void scene_DoubleClick(UIElement sender, Entry e)
    {
        S日志信息 scene = (S日志信息)sender;
        DateTime? start = null;
        DateTime? end = null;
        string content;
        string param;
        byte[] level;
        if (GetParameter(out start, out end, out content, out param, out level))
            maintainer.Proxy.FindContext(scene.Index, start, end, PAGE_COUNT, content, param, level);
    }
    void Close()
    {
        foreach (var scene in scenes)
            scene.Close(true);
        scenes.ClearToFree();
        P日志消息.Clear();
        for (int i = 0; i < LPage.Length; i++)
            LPage[i].Visible = false;
    }
    public void Show(Service service)
    {
        this.service = service;
        this.maintainer = Maintainer.Find(service);

        IManagerAgent._OnGetLog.Callback = GetLog;
        IManagerAgent._OnGetLogRepeat.Callback = GetLogRepeat;
    }

    private IEnumerable<ICoroutine> MyLoading()
    {
        page = 0;
        pages = 0;
        if (string.IsNullOrEmpty(TB起始时间.Text))
        {
            TB起始时间.Text = (DateTime.Now - TimeSpan.FromDays(3)).ToString("yyyy-MM-dd HH:mm:ss");
        }
        Close();
        return null;
    }

    protected override void InternalEvent(Entry e)
    {
        base.InternalEvent(e);

        if (e.INPUT.Keyboard != null)
        {
            if (__INPUT.KeyboardIsClick(PCKeys.Enter))
                B查询_Clicked(this, e);
            if (__INPUT.KeyboardIsClick(PCKeys.Left))
                B前一页_Clicked(this, e);
            if (__INPUT.KeyboardIsClick(PCKeys.Right))
                B下一页_Clicked(this, e);
        }
        if (e.INPUT.Mouse != null && P日志消息.IsHover)
        {
            float scroll = e.INPUT.Mouse.ScrollWheelValue;
            if (scroll != 0)
            {
                P日志消息.OffsetY += scroll * 100;
            }
        }
    }
}
