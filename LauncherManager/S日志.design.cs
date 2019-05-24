using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class S日志
{
    public EntryEngine.UI.CheckBox CBDebug = new EntryEngine.UI.CheckBox();
    public EntryEngine.UI.CheckBox CBInfo = new EntryEngine.UI.CheckBox();
    public EntryEngine.UI.CheckBox CBWarning = new EntryEngine.UI.CheckBox();
    public EntryEngine.UI.CheckBox CBError = new EntryEngine.UI.CheckBox();
    public EntryEngine.UI.Button B查询 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B整合 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B退出 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Panel P日志 = new EntryEngine.UI.Panel();
    public EntryEngine.UI.Button B末页 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B下一页 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button B前一页 = new EntryEngine.UI.Button();
    public EntryEngine.UI.Label[] LPage = new EntryEngine.UI.Label[7]
    {
        new EntryEngine.UI.Label(),
        new EntryEngine.UI.Label(),
        new EntryEngine.UI.Label(),
        new EntryEngine.UI.Label(),
        new EntryEngine.UI.Label(),
        new EntryEngine.UI.Label(),
        new EntryEngine.UI.Label(),
    };
    public EntryEngine.UI.Label[] L160810124416 = new EntryEngine.UI.Label[3]
    {
        new EntryEngine.UI.Label(),
        new EntryEngine.UI.Label(),
        new EntryEngine.UI.Label(),
    };
    public EntryEngine.UI.Panel P日志消息 = new EntryEngine.UI.Panel();
    public EntryEngine.UI.Button B首页 = new EntryEngine.UI.Button();
    public EntryEngine.UI.TextBox TB起始时间 = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.TextBox TB结束时间 = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.TextBox TB内容筛选 = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.TextBox TB参数筛选 = new EntryEngine.UI.TextBox();
    
    
    private void Initialize()
    {
        this.Name = "S日志";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 1280f;
        this_Clip.Height = 720f;
        this.Clip = this_Clip;
        
        CBDebug.Name = "CBDebug";
        EntryEngine.RECT CBDebug_Clip = new EntryEngine.RECT();
        CBDebug_Clip.X = 623f;
        CBDebug_Clip.Y = 19.875f;
        CBDebug_Clip.Width = 25f;
        CBDebug_Clip.Height = 26.31579f;
        CBDebug.Clip = CBDebug_Clip;
        
        CBDebug.UIText = new EntryEngine.UI.UIText();
        CBDebug.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        CBDebug.UIText.TextAlignment = (EPivot)18;
        CBDebug.UIText.TextShader = null;
        CBDebug.UIText.Padding.X = 0f;
        CBDebug.UIText.Padding.Y = 0f;
        CBDebug.UIText.FontSize = 16f;
        this.Add(CBDebug);
        CBInfo.Name = "CBInfo";
        EntryEngine.RECT CBInfo_Clip = new EntryEngine.RECT();
        CBInfo_Clip.X = 703f;
        CBInfo_Clip.Y = 19.875f;
        CBInfo_Clip.Width = 25f;
        CBInfo_Clip.Height = 26.31579f;
        CBInfo.Clip = CBInfo_Clip;
        
        CBInfo.Checked = true;
        CBInfo.UIText = new EntryEngine.UI.UIText();
        CBInfo.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        CBInfo.UIText.TextAlignment = (EPivot)18;
        CBInfo.UIText.TextShader = null;
        CBInfo.UIText.Padding.X = 0f;
        CBInfo.UIText.Padding.Y = 0f;
        CBInfo.UIText.FontSize = 16f;
        this.Add(CBInfo);
        CBWarning.Name = "CBWarning";
        EntryEngine.RECT CBWarning_Clip = new EntryEngine.RECT();
        CBWarning_Clip.X = 782f;
        CBWarning_Clip.Y = 19.875f;
        CBWarning_Clip.Width = 25f;
        CBWarning_Clip.Height = 26.31579f;
        CBWarning.Clip = CBWarning_Clip;
        
        CBWarning.Checked = true;
        CBWarning.UIText = new EntryEngine.UI.UIText();
        CBWarning.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        CBWarning.UIText.TextAlignment = (EPivot)18;
        CBWarning.UIText.TextShader = null;
        CBWarning.UIText.Padding.X = 0f;
        CBWarning.UIText.Padding.Y = 0f;
        CBWarning.UIText.FontSize = 16f;
        this.Add(CBWarning);
        CBError.Name = "CBError";
        EntryEngine.RECT CBError_Clip = new EntryEngine.RECT();
        CBError_Clip.X = 860.5f;
        CBError_Clip.Y = 19.875f;
        CBError_Clip.Width = 25f;
        CBError_Clip.Height = 26.31579f;
        CBError.Clip = CBError_Clip;
        
        CBError.Checked = true;
        CBError.UIText = new EntryEngine.UI.UIText();
        CBError.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        CBError.UIText.TextAlignment = (EPivot)18;
        CBError.UIText.TextShader = null;
        CBError.UIText.Padding.X = 0f;
        CBError.UIText.Padding.Y = 0f;
        CBError.UIText.FontSize = 16f;
        this.Add(CBError);
        B查询.Name = "B查询";
        EntryEngine.RECT B查询_Clip = new EntryEngine.RECT();
        B查询_Clip.X = 980f;
        B查询_Clip.Y = 14.875f;
        B查询_Clip.Width = 129f;
        B查询_Clip.Height = 35f;
        B查询.Clip = B查询_Clip;
        
        B查询.UIText = new EntryEngine.UI.UIText();
        B查询.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B查询.UIText.TextAlignment = (EPivot)17;
        B查询.UIText.TextShader = null;
        B查询.UIText.Padding.X = 0f;
        B查询.UIText.Padding.Y = 0f;
        B查询.UIText.FontSize = 16f;
        this.Add(B查询);
        B整合.Name = "B整合";
        EntryEngine.RECT B整合_Clip = new EntryEngine.RECT();
        B整合_Clip.X = 980f;
        B整合_Clip.Y = 65.875f;
        B整合_Clip.Width = 129f;
        B整合_Clip.Height = 35f;
        B整合.Clip = B整合_Clip;
        
        B整合.UIText = new EntryEngine.UI.UIText();
        B整合.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B整合.UIText.TextAlignment = (EPivot)17;
        B整合.UIText.TextShader = null;
        B整合.UIText.Padding.X = 0f;
        B整合.UIText.Padding.Y = 0f;
        B整合.UIText.FontSize = 16f;
        this.Add(B整合);
        B退出.Name = "B退出";
        EntryEngine.RECT B退出_Clip = new EntryEngine.RECT();
        B退出_Clip.X = 1110.5f;
        B退出_Clip.Y = 14.875f;
        B退出_Clip.Width = 129f;
        B退出_Clip.Height = 35f;
        B退出.Clip = B退出_Clip;
        
        B退出.UIText = new EntryEngine.UI.UIText();
        B退出.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B退出.UIText.TextAlignment = (EPivot)17;
        B退出.UIText.TextShader = null;
        B退出.UIText.Padding.X = 0f;
        B退出.UIText.Padding.Y = 0f;
        B退出.UIText.FontSize = 16f;
        this.Add(B退出);
        P日志.Name = "P日志";
        EntryEngine.RECT P日志_Clip = new EntryEngine.RECT();
        P日志_Clip.X = 24f;
        P日志_Clip.Y = 121.875f;
        P日志_Clip.Width = 1214f;
        P日志_Clip.Height = 573f;
        P日志.Clip = P日志_Clip;
        
        this.Add(P日志);
        B末页.Name = "B末页";
        EntryEngine.RECT B末页_Clip = new EntryEngine.RECT();
        B末页_Clip.X = 1151f;
        B末页_Clip.Y = 2f;
        B末页_Clip.Width = 60f;
        B末页_Clip.Height = 37f;
        B末页.Clip = B末页_Clip;
        
        B末页.UIText = new EntryEngine.UI.UIText();
        B末页.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B末页.UIText.TextAlignment = (EPivot)17;
        B末页.UIText.TextShader = null;
        B末页.UIText.Padding.X = 0f;
        B末页.UIText.Padding.Y = 0f;
        B末页.UIText.FontSize = 16f;
        P日志.Add(B末页);
        B下一页.Name = "B下一页";
        EntryEngine.RECT B下一页_Clip = new EntryEngine.RECT();
        B下一页_Clip.X = 958f;
        B下一页_Clip.Y = 1f;
        B下一页_Clip.Width = 60f;
        B下一页_Clip.Height = 37f;
        B下一页.Clip = B下一页_Clip;
        
        B下一页.UIText = new EntryEngine.UI.UIText();
        B下一页.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B下一页.UIText.TextAlignment = (EPivot)17;
        B下一页.UIText.TextShader = null;
        B下一页.UIText.Padding.X = 0f;
        B下一页.UIText.Padding.Y = 0f;
        B下一页.UIText.FontSize = 16f;
        P日志.Add(B下一页);
        B前一页.Name = "B前一页";
        EntryEngine.RECT B前一页_Clip = new EntryEngine.RECT();
        B前一页_Clip.X = 194f;
        B前一页_Clip.Y = 2f;
        B前一页_Clip.Width = 60f;
        B前一页_Clip.Height = 37f;
        B前一页.Clip = B前一页_Clip;
        
        B前一页.UIText = new EntryEngine.UI.UIText();
        B前一页.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B前一页.UIText.TextAlignment = (EPivot)17;
        B前一页.UIText.TextShader = null;
        B前一页.UIText.Padding.X = 0f;
        B前一页.UIText.Padding.Y = 0f;
        B前一页.UIText.FontSize = 16f;
        P日志.Add(B前一页);
        LPage[0].Name = "LPage";
        EntryEngine.RECT LPage_0__Clip = new EntryEngine.RECT();
        LPage_0__Clip.X = 276f;
        LPage_0__Clip.Y = 1f;
        LPage_0__Clip.Width = 71f;
        LPage_0__Clip.Height = 37f;
        LPage[0].Clip = LPage_0__Clip;
        
        LPage[0].UIText = new EntryEngine.UI.UIText();
        LPage[0].UIText.Text = "#99999";
        LPage[0].UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        LPage[0].UIText.TextAlignment = (EPivot)17;
        LPage[0].UIText.TextShader = null;
        LPage[0].UIText.Padding.X = 0f;
        LPage[0].UIText.Padding.Y = 0f;
        LPage[0].UIText.FontSize = 16f;
        LPage[0].Text = "#99999";
        P日志.Add(LPage[0]);
        LPage[1].Name = "LPage";
        EntryEngine.RECT LPage_1__Clip = new EntryEngine.RECT();
        LPage_1__Clip.X = 373f;
        LPage_1__Clip.Y = 1f;
        LPage_1__Clip.Width = 71f;
        LPage_1__Clip.Height = 37f;
        LPage[1].Clip = LPage_1__Clip;
        
        LPage[1].UIText = new EntryEngine.UI.UIText();
        LPage[1].UIText.Text = "#99999";
        LPage[1].UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        LPage[1].UIText.TextAlignment = (EPivot)17;
        LPage[1].UIText.TextShader = null;
        LPage[1].UIText.Padding.X = 0f;
        LPage[1].UIText.Padding.Y = 0f;
        LPage[1].UIText.FontSize = 16f;
        LPage[1].Text = "#99999";
        P日志.Add(LPage[1]);
        LPage[2].Name = "LPage";
        EntryEngine.RECT LPage_2__Clip = new EntryEngine.RECT();
        LPage_2__Clip.X = 472f;
        LPage_2__Clip.Y = 1f;
        LPage_2__Clip.Width = 71f;
        LPage_2__Clip.Height = 37f;
        LPage[2].Clip = LPage_2__Clip;
        
        LPage[2].UIText = new EntryEngine.UI.UIText();
        LPage[2].UIText.Text = "#99999";
        LPage[2].UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        LPage[2].UIText.TextAlignment = (EPivot)17;
        LPage[2].UIText.TextShader = null;
        LPage[2].UIText.Padding.X = 0f;
        LPage[2].UIText.Padding.Y = 0f;
        LPage[2].UIText.FontSize = 16f;
        LPage[2].Text = "#99999";
        P日志.Add(LPage[2]);
        LPage[3].Name = "LPage";
        EntryEngine.RECT LPage_3__Clip = new EntryEngine.RECT();
        LPage_3__Clip.X = 569f;
        LPage_3__Clip.Y = 1f;
        LPage_3__Clip.Width = 71f;
        LPage_3__Clip.Height = 37f;
        LPage[3].Clip = LPage_3__Clip;
        
        LPage[3].UIText = new EntryEngine.UI.UIText();
        LPage[3].UIText.Text = "#99999";
        LPage[3].UIText.FontColor = new COLOR()
        {
            R = 255,
            G = 26,
            B = 26,
            A = 255,
        };
        LPage[3].UIText.TextAlignment = (EPivot)17;
        LPage[3].UIText.TextShader = null;
        LPage[3].UIText.Padding.X = 0f;
        LPage[3].UIText.Padding.Y = 0f;
        LPage[3].UIText.FontSize = 16f;
        LPage[3].Text = "#99999";
        P日志.Add(LPage[3]);
        LPage[4].Name = "LPage";
        EntryEngine.RECT LPage_4__Clip = new EntryEngine.RECT();
        LPage_4__Clip.X = 663.5f;
        LPage_4__Clip.Y = 1f;
        LPage_4__Clip.Width = 71f;
        LPage_4__Clip.Height = 37f;
        LPage[4].Clip = LPage_4__Clip;
        
        LPage[4].UIText = new EntryEngine.UI.UIText();
        LPage[4].UIText.Text = "#99999";
        LPage[4].UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        LPage[4].UIText.TextAlignment = (EPivot)17;
        LPage[4].UIText.TextShader = null;
        LPage[4].UIText.Padding.X = 0f;
        LPage[4].UIText.Padding.Y = 0f;
        LPage[4].UIText.FontSize = 16f;
        LPage[4].Text = "#99999";
        P日志.Add(LPage[4]);
        LPage[5].Name = "LPage";
        EntryEngine.RECT LPage_5__Clip = new EntryEngine.RECT();
        LPage_5__Clip.X = 760f;
        LPage_5__Clip.Y = 1f;
        LPage_5__Clip.Width = 71f;
        LPage_5__Clip.Height = 37f;
        LPage[5].Clip = LPage_5__Clip;
        
        LPage[5].UIText = new EntryEngine.UI.UIText();
        LPage[5].UIText.Text = "#99999";
        LPage[5].UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        LPage[5].UIText.TextAlignment = (EPivot)17;
        LPage[5].UIText.TextShader = null;
        LPage[5].UIText.Padding.X = 0f;
        LPage[5].UIText.Padding.Y = 0f;
        LPage[5].UIText.FontSize = 16f;
        LPage[5].Text = "#99999";
        P日志.Add(LPage[5]);
        LPage[6].Name = "LPage";
        EntryEngine.RECT LPage_6__Clip = new EntryEngine.RECT();
        LPage_6__Clip.X = 859f;
        LPage_6__Clip.Y = 1f;
        LPage_6__Clip.Width = 71f;
        LPage_6__Clip.Height = 37f;
        LPage[6].Clip = LPage_6__Clip;
        
        LPage[6].UIText = new EntryEngine.UI.UIText();
        LPage[6].UIText.Text = "#99999";
        LPage[6].UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        LPage[6].UIText.TextAlignment = (EPivot)17;
        LPage[6].UIText.TextShader = null;
        LPage[6].UIText.Padding.X = 0f;
        LPage[6].UIText.Padding.Y = 0f;
        LPage[6].UIText.FontSize = 16f;
        LPage[6].Text = "#99999";
        P日志.Add(LPage[6]);
        L160810124416[0].Name = "L160810124416";
        EntryEngine.RECT L160810124416_0__Clip = new EntryEngine.RECT();
        L160810124416_0__Clip.X = 37f;
        L160810124416_0__Clip.Y = 46f;
        L160810124416_0__Clip.Width = 48f;
        L160810124416_0__Clip.Height = 27.375f;
        L160810124416[0].Clip = L160810124416_0__Clip;
        
        L160810124416[0].UIText = new EntryEngine.UI.UIText();
        L160810124416[0].UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L160810124416[0].UIText.TextAlignment = (EPivot)0;
        L160810124416[0].UIText.TextShader = null;
        L160810124416[0].UIText.Padding.X = 0f;
        L160810124416[0].UIText.Padding.Y = 0f;
        L160810124416[0].UIText.FontSize = 24f;
        P日志.Add(L160810124416[0]);
        L160810124416[1].Name = "L160810124416";
        EntryEngine.RECT L160810124416_1__Clip = new EntryEngine.RECT();
        L160810124416_1__Clip.X = 279f;
        L160810124416_1__Clip.Y = 46f;
        L160810124416_1__Clip.Width = 48f;
        L160810124416_1__Clip.Height = 27.375f;
        L160810124416[1].Clip = L160810124416_1__Clip;
        
        L160810124416[1].UIText = new EntryEngine.UI.UIText();
        L160810124416[1].UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L160810124416[1].UIText.TextAlignment = (EPivot)0;
        L160810124416[1].UIText.TextShader = null;
        L160810124416[1].UIText.Padding.X = 0f;
        L160810124416[1].UIText.Padding.Y = 0f;
        L160810124416[1].UIText.FontSize = 24f;
        P日志.Add(L160810124416[1]);
        L160810124416[2].Name = "L160810124416";
        EntryEngine.RECT L160810124416_2__Clip = new EntryEngine.RECT();
        L160810124416_2__Clip.X = 1037f;
        L160810124416_2__Clip.Y = 46f;
        L160810124416_2__Clip.Width = 144f;
        L160810124416_2__Clip.Height = 27.375f;
        L160810124416[2].Clip = L160810124416_2__Clip;
        
        L160810124416[2].UIText = new EntryEngine.UI.UIText();
        L160810124416[2].UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        L160810124416[2].UIText.TextAlignment = (EPivot)0;
        L160810124416[2].UIText.TextShader = null;
        L160810124416[2].UIText.Padding.X = 0f;
        L160810124416[2].UIText.Padding.Y = 0f;
        L160810124416[2].UIText.FontSize = 24f;
        P日志.Add(L160810124416[2]);
        P日志消息.Name = "P日志消息";
        EntryEngine.RECT P日志消息_Clip = new EntryEngine.RECT();
        P日志消息_Clip.X = 2f;
        P日志消息_Clip.Y = 73.375f;
        P日志消息_Clip.Width = 1209f;
        P日志消息_Clip.Height = 499.625f;
        P日志消息.Clip = P日志消息_Clip;
        
        P日志消息.DragMode = (EDragMode)1;
        P日志.Add(P日志消息);
        B首页.Name = "B首页";
        EntryEngine.RECT B首页_Clip = new EntryEngine.RECT();
        B首页_Clip.X = 7f;
        B首页_Clip.Y = 1f;
        B首页_Clip.Width = 60f;
        B首页_Clip.Height = 37f;
        B首页.Clip = B首页_Clip;
        
        B首页.UIText = new EntryEngine.UI.UIText();
        B首页.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        B首页.UIText.TextAlignment = (EPivot)17;
        B首页.UIText.TextShader = null;
        B首页.UIText.Padding.X = 0f;
        B首页.UIText.Padding.Y = 0f;
        B首页.UIText.FontSize = 16f;
        P日志.Add(B首页);
        TB起始时间.Name = "TB起始时间";
        EntryEngine.RECT TB起始时间_Clip = new EntryEngine.RECT();
        TB起始时间_Clip.X = 24f;
        TB起始时间_Clip.Y = 14.875f;
        TB起始时间_Clip.Width = 250f;
        TB起始时间_Clip.Height = 35f;
        TB起始时间.Clip = TB起始时间_Clip;
        
        TB起始时间.Color = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TB起始时间.DefaultText = new EntryEngine.UI.UIText();
        TB起始时间.DefaultText.Text = "起始时间";
        TB起始时间.DefaultText.FontColor = new COLOR()
        {
            R = 130,
            G = 130,
            B = 130,
            A = 255,
        };
        TB起始时间.DefaultText.TextAlignment = (EPivot)17;
        TB起始时间.DefaultText.TextShader = null;
        TB起始时间.DefaultText.Padding.X = 0f;
        TB起始时间.DefaultText.Padding.Y = 0f;
        TB起始时间.DefaultText.FontSize = 16f;
        TB起始时间.UIText = new EntryEngine.UI.UIText();
        TB起始时间.UIText.Text = "";
        TB起始时间.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TB起始时间.UIText.TextAlignment = (EPivot)17;
        TB起始时间.UIText.TextShader = null;
        TB起始时间.UIText.Padding.X = 0f;
        TB起始时间.UIText.Padding.Y = 0f;
        TB起始时间.UIText.FontSize = 16f;
        this.Add(TB起始时间);
        TB结束时间.Name = "TB结束时间";
        EntryEngine.RECT TB结束时间_Clip = new EntryEngine.RECT();
        TB结束时间_Clip.X = 327f;
        TB结束时间_Clip.Y = 14.875f;
        TB结束时间_Clip.Width = 250f;
        TB结束时间_Clip.Height = 35f;
        TB结束时间.Clip = TB结束时间_Clip;
        
        TB结束时间.Color = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TB结束时间.DefaultText = new EntryEngine.UI.UIText();
        TB结束时间.DefaultText.Text = "结束时间";
        TB结束时间.DefaultText.FontColor = new COLOR()
        {
            R = 130,
            G = 130,
            B = 130,
            A = 255,
        };
        TB结束时间.DefaultText.TextAlignment = (EPivot)17;
        TB结束时间.DefaultText.TextShader = null;
        TB结束时间.DefaultText.Padding.X = 0f;
        TB结束时间.DefaultText.Padding.Y = 0f;
        TB结束时间.DefaultText.FontSize = 16f;
        TB结束时间.UIText = new EntryEngine.UI.UIText();
        TB结束时间.UIText.Text = "";
        TB结束时间.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TB结束时间.UIText.TextAlignment = (EPivot)17;
        TB结束时间.UIText.TextShader = null;
        TB结束时间.UIText.Padding.X = 0f;
        TB结束时间.UIText.Padding.Y = 0f;
        TB结束时间.UIText.FontSize = 16f;
        this.Add(TB结束时间);
        TB内容筛选.Name = "TB内容筛选";
        EntryEngine.RECT TB内容筛选_Clip = new EntryEngine.RECT();
        TB内容筛选_Clip.X = 23f;
        TB内容筛选_Clip.Y = 65.875f;
        TB内容筛选_Clip.Width = 554f;
        TB内容筛选_Clip.Height = 35f;
        TB内容筛选.Clip = TB内容筛选_Clip;
        
        TB内容筛选.Color = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TB内容筛选.DefaultText = new EntryEngine.UI.UIText();
        TB内容筛选.DefaultText.Text = "内容筛选";
        TB内容筛选.DefaultText.FontColor = new COLOR()
        {
            R = 130,
            G = 130,
            B = 130,
            A = 255,
        };
        TB内容筛选.DefaultText.TextAlignment = (EPivot)17;
        TB内容筛选.DefaultText.TextShader = null;
        TB内容筛选.DefaultText.Padding.X = 0f;
        TB内容筛选.DefaultText.Padding.Y = 0f;
        TB内容筛选.DefaultText.FontSize = 16f;
        TB内容筛选.UIText = new EntryEngine.UI.UIText();
        TB内容筛选.UIText.Text = "";
        TB内容筛选.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TB内容筛选.UIText.TextAlignment = (EPivot)17;
        TB内容筛选.UIText.TextShader = null;
        TB内容筛选.UIText.Padding.X = 0f;
        TB内容筛选.UIText.Padding.Y = 0f;
        TB内容筛选.UIText.FontSize = 16f;
        this.Add(TB内容筛选);
        TB参数筛选.Name = "TB参数筛选";
        EntryEngine.RECT TB参数筛选_Clip = new EntryEngine.RECT();
        TB参数筛选_Clip.X = 623f;
        TB参数筛选_Clip.Y = 65.875f;
        TB参数筛选_Clip.Width = 160f;
        TB参数筛选_Clip.Height = 35f;
        TB参数筛选.Clip = TB参数筛选_Clip;
        
        TB参数筛选.Color = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TB参数筛选.DefaultText = new EntryEngine.UI.UIText();
        TB参数筛选.DefaultText.Text = "参数筛选";
        TB参数筛选.DefaultText.FontColor = new COLOR()
        {
            R = 130,
            G = 130,
            B = 130,
            A = 255,
        };
        TB参数筛选.DefaultText.TextAlignment = (EPivot)17;
        TB参数筛选.DefaultText.TextShader = null;
        TB参数筛选.DefaultText.Padding.X = 0f;
        TB参数筛选.DefaultText.Padding.Y = 0f;
        TB参数筛选.DefaultText.FontSize = 16f;
        TB参数筛选.UIText = new EntryEngine.UI.UIText();
        TB参数筛选.UIText.Text = "";
        TB参数筛选.UIText.FontColor = new COLOR()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255,
        };
        TB参数筛选.UIText.TextAlignment = (EPivot)17;
        TB参数筛选.UIText.TextShader = null;
        TB参数筛选.UIText.Padding.X = 0f;
        TB参数筛选.UIText.Padding.Y = 0f;
        TB参数筛选.UIText.FontSize = 16f;
        this.Add(TB参数筛选);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_单选未选中.png", ___c => CBDebug.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_登陆单选.png", ___c => CBDebug.SourceClicked = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_单选未选中.png", ___c => CBInfo.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_登陆单选.png", ___c => CBInfo.SourceClicked = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_单选未选中.png", ___c => CBWarning.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_登陆单选.png", ___c => CBWarning.SourceClicked = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_单选未选中.png", ___c => CBError.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0001_登陆单选.png", ___c => CBError.SourceClicked = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0000_登陆全选-6.png", ___c => B查询.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0000_登陆全选-6.png", ___c => B整合.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0000_登陆全选-6.png", ___c => B退出.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => P日志.Background = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0000_登陆全选-6.png", ___c => B末页.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0000_登陆全选-6.png", ___c => B下一页.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0000_登陆全选-6.png", ___c => B前一页.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_页签.png", ___c => LPage[0].SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_页签.png", ___c => LPage[1].SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_页签.png", ___c => LPage[2].SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_页签.png", ___c => LPage[3].SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_页签.png", ___c => LPage[4].SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_页签.png", ___c => LPage[5].SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_页签.png", ___c => LPage[6].SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        
        
        
        
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"LM_0000_登陆全选-6.png", ___c => B首页.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TB起始时间.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TB结束时间.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TB内容筛选.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Frame1.mtpatch", ___c => TB参数筛选.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    public void Show(UIScene __scene)
    {
        CBDebug.UIText.Text = _LANGUAGE.GetString("89");
        CBDebug.Text = _LANGUAGE.GetString("89");
        CBInfo.UIText.Text = _LANGUAGE.GetString("90");
        CBInfo.Text = _LANGUAGE.GetString("90");
        CBWarning.UIText.Text = _LANGUAGE.GetString("91");
        CBWarning.Text = _LANGUAGE.GetString("91");
        CBError.UIText.Text = _LANGUAGE.GetString("92");
        CBError.Text = _LANGUAGE.GetString("92");
        B查询.UIText.Text = _LANGUAGE.GetString("93");
        B查询.Text = _LANGUAGE.GetString("93");
        B整合.UIText.Text = _LANGUAGE.GetString("94");
        B整合.Text = _LANGUAGE.GetString("94");
        B退出.UIText.Text = _LANGUAGE.GetString("99");
        B退出.Text = _LANGUAGE.GetString("99");
        B末页.UIText.Text = _LANGUAGE.GetString("97");
        B末页.Text = _LANGUAGE.GetString("97");
        B下一页.UIText.Text = _LANGUAGE.GetString("98");
        B下一页.Text = _LANGUAGE.GetString("98");
        B前一页.UIText.Text = _LANGUAGE.GetString("96");
        B前一页.Text = _LANGUAGE.GetString("96");
        L160810124416[0].UIText.Text = _LANGUAGE.GetString("78");
        L160810124416[0].Text = _LANGUAGE.GetString("78");
        L160810124416[1].UIText.Text = _LANGUAGE.GetString("103");
        L160810124416[1].Text = _LANGUAGE.GetString("103");
        L160810124416[2].UIText.Text = _LANGUAGE.GetString("104");
        L160810124416[2].Text = _LANGUAGE.GetString("104");
        B首页.UIText.Text = _LANGUAGE.GetString("95");
        B首页.Text = _LANGUAGE.GetString("95");
        
    }
}
