using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;

/// <summary>后台管理系统需要登录针对某个用户的服务接口</summary>
[ProtocolStub(5, null)]
public interface ICenter
{
    void GetUserInfo(Action<T_CENTER_USER> callback);
    /// <summary>修改个人密码</summary>
    /// <param name="oldPassword">旧密码</param>
    /// <param name="newPassword">新密码</param>
    void ChangePassword(string oldPassword, string newPassword, Action<bool> callback);


    /// <summary>获取游戏名</summary>
    void GetGameName(Action<List<string>> callback);
    /// <summary>获取某一游戏的所有渠道</summary>
    /// <param name="gameName">游戏名</param>
    void GetChannel(string gameName, Action<List<string>> callback);
    /// <summary>获取不包含“全部”的游戏名</summary>
    void GetAnalysisGame(Action<List<string>> callback);


    /// <summary>获取基础分析标签</summary>
    /// <param name="gameName">游戏名</param>
    void GetAnalysisLabel(string gameName, Action<List<string>> callback);
    /// <summary>获取基础分析数据</summary>
    /// <param name="gameName">游戏名</param>
    /// <param name="channel">渠道</param>
    /// <param name="label">分析标签</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    void GetAnalysis(string gameName, string channel, string label, DateTime startTime, DateTime endTime, Action<List<RetAnalysis>> callback);

    /// <summary>第一层留存分析</summary>
    /// <param name="page">分页号</param>
    /// <param name="gameName">游戏名</param>
    /// <param name="channel">渠道</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    void GetRetained(int page, string gameName, string channel, DateTime startTime, DateTime endTime, Action<PagedModel<RetRetained>> callback);
    /// <summary>第二层留存分析</summary>
    /// <param name="page">分页号</param>
    /// <param name="gameName">游戏名</param>
    /// <param name="channel">渠道</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    void GetRetained2(int page, string gameName, string channel, DateTime startTime, DateTime endTime, Action<PagedModel<RetRetained>> callback);

    /// <summary>获取在线人数</summary>
    /// <param name="gameName">游戏名</param>
    /// <param name="channel">渠道</param>
    /// <param name="unit">单位</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    void OnlineCount(string gameName, string channel, RetOnlineUnit unit, DateTime startTime, DateTime endTime, Action<RetOnline> callback);

    /// <summary>获取游戏时长</summary>
    /// <param name="gameName">游戏名</param>
    /// <param name="channel">渠道</param>
    /// <param name="startTime">起始时间</param>
    /// <param name="endTime">结束时间</param>
    void GameTime(string gameName, string channel, DateTime startTime, DateTime endTime, Action<RetGameTime> callback);


    /// <summary>第一层游戏统计</summary>
    /// <param name="page">分页号</param>
    /// <param name="gameName">游戏名</param>
    /// <param name="channel">渠道</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    void GetGameData(int page, string gameName, string channel, DateTime startTime, DateTime endTime, Action<RetGameData> callback);
    /// <summary>第二层游戏统计</summary>
    /// <param name="page">分页号</param>
    /// <param name="gameName">游戏名</param>
    /// <param name="channel">渠道</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    void GetGameData2(int page, string gameName, string channel, DateTime startTime, DateTime endTime, Action<PagedModel<GameDataItem>> callback);


    #region 系统设置
    /// <summary>获取平台账号列表</summary>
    /// <param name="page">分页号</param>
    /// <param name="account">账号筛选</param>
    void GetAccountList(int page, string account, Action<PagedModel<RetAccount>> callback);
    /// <summary>绑定游戏</summary>
    /// <param name="identityID">绑定的账号ID</param>
    /// <param name="gameNames">绑定的游戏名数组</param>
    void BindGame(int identityID, string[] gameNames, Action<bool> callback);
    /// <summary>修改平台账号信息</summary>
    /// <param name="identityID">账号ID，为0添加</param>
    /// <param name="account">账号</param>
    /// <param name="password">密码</param>
    /// <param name="type">账号类型</param>
    /// <param name="nickName">昵称</param>
    void ModifyAccount(int identityID, string account, string password, EAccountType type, string nickName, Action<bool> callback);
    /// <summary>删除平台账号</summary>
    /// <param name="identityID">账号ID</param>
    void DeleteAccount(int identityID, Action<bool> callback);
    /// <summary>改变账号状态</summary>
    /// <param name="identityID">账号ID</param>
    void ChangeAccountState(int identityID, Action<bool> callback);
    #endregion
}

/// <summary>行为分析</summary>
public class RetAnalysis
{
    /// <summary>事件名称</summary>
    public string Name;
    /// <summary>数量</summary>
    public int Count;
    /// <summary>排序</summary>
    [NonSerialized]
    public int OrderID;
    /// <summary>是否是可重复的</summary>
    [NonSerialized]
    public bool CanRepeat = false;
}
/// <summary>留存分析</summary>
public class RetRetained
{
    /// <summary>留存率需要计算的天数</summary>
    public static int[] Retained = { 1, 2, 3, 4, 5, 6, 7, 14, 30 };

    /// <summary>日期</summary>
    public DateTime Time;
    /// <summary>游戏名</summary>
    public string GameName;
    /// <summary>活跃数</summary>
    public int ActiveCount;
    /// <summary>新增用户数</summary>
    public int RegistCount;
    /// <summary>登录的用户数</summary>
    public int[] LoginCount { get; set; }
    /// <summary>留存率</summary>
    public double[] RetainedRate = new double[Retained.Length];

    public HashSet<int> loginAccountID { set; get; }

    public void Calc()
    {
        ActiveCount = loginAccountID != null ? loginAccountID.Count : 0;
        double count = RegistCount * 0.01;
        if (LoginCount == null)
            LoginCount = new int[Retained.Length];
        for (int i = 0; i < Retained.Length; i++)
            if (LoginCount[i] != -1)
                RetainedRate[i] = RegistCount != 0 ? Math.Round(LoginCount[i] / count, 2) : 0;
            else
                RetainedRate[i] = -1;
    }
}
/// <summary>在线单位</summary>
public enum RetOnlineUnit
{
    十五分钟 = 1,
    三十分钟 = 2,
    一小时 = 3,
    六小时 = 4,
    一天 = 5,
    一周 = 6,
    一月 = 7
}
/// <summary>在线人数</summary>
public class RetOnlineItem
{
    /// <summary>在线的时间</summary>
    public DateTime Time;
    /// <summary>在线人数</summary>
    public int Count;
}
/// <summary>在线分析</summary>
public class RetOnline
{
    /// <summary>当前时间</summary>
    public DateTime NowTime = DateTime.Now;
    /// <summary>当前在线人数</summary>
    public int NowCount;
    /// <summary>平均在线人数</summary>
    public double AvgCount;
    /// <summary>在线图标数据</summary>
    public List<RetOnlineItem> Info = new List<RetOnlineItem>();
    /// <summary>在线图表数据单位</summary>
    public RetOnlineUnit unit;

    /// <summary>计算在线人数</summary>
    /// <param name="items">登录信息，Count是登录的账号ID</param>
    public void Calc(List<RetOnlineItem> items, DateTime startTime, DateTime endTime)
    {
        if (items.Count == 0) return;
        // 需要统计多少时间的数据
        switch (unit)
        {
            case RetOnlineUnit.十五分钟:
                long minutes = (long)Math.Ceiling((endTime - startTime).TotalMinutes / 15);
                for (long i = 0; i < minutes; i++)
                    Info.Add(new RetOnlineItem()
                    {
                        Time = startTime.AddMinutes(i * 15),
                        Count = 0
                    });
                break;
            case RetOnlineUnit.三十分钟:
                long minutes2 = (long)Math.Ceiling((endTime - startTime).TotalMinutes / 30);
                for (long i = 0; i < minutes2; i++)
                    Info.Add(new RetOnlineItem()
                    {
                        Time = startTime.AddMinutes(i * 30),
                        Count = 0
                    });
                break;
            case RetOnlineUnit.一小时:
                long hours = (long)Math.Ceiling((endTime - startTime).TotalHours);
                for (long i = 0; i < hours; i++)
                    Info.Add(new RetOnlineItem()
                    {
                        Time = startTime.AddHours(i),
                        Count = 0
                    });
                break;
            case RetOnlineUnit.六小时:
                long hours2 = (long)Math.Ceiling((endTime - startTime).TotalHours / 6);
                for (long i = 0; i < hours2; i++)
                    Info.Add(new RetOnlineItem()
                    {
                        Time = startTime.AddHours(i * 6),
                        Count = 0
                    });
                break;
            case RetOnlineUnit.一天:
                long days = (long)Math.Ceiling((endTime - startTime).TotalDays);
                for (long i = 0; i < days; i++)
                    Info.Add(new RetOnlineItem()
                    {
                        Time = startTime.AddDays(i),
                        Count = 0
                    });
                break;
            case RetOnlineUnit.一周:
                long days2 = (long)Math.Ceiling((endTime - startTime).TotalDays / 7);
                for (long i = 0; i < days2; i++)
                    Info.Add(new RetOnlineItem()
                    {
                        Time = startTime.AddDays(i * 7),
                        Count = 0
                    });
                break;
            case RetOnlineUnit.一月:
                long days3 = (long)Math.Ceiling((endTime - startTime).TotalDays / 30);
                for (long i = 0; i < days3; i++)
                    Info.Add(new RetOnlineItem()
                    {
                        Time = startTime.AddDays(i * 30),
                        Count = 0
                    });
                break;
        }

        if (Info.Count == 0) return;

        HashSet<int> ids;
        Dictionary<DateTime, HashSet<int>> loginByTime = new Dictionary<DateTime, HashSet<int>>();
        int lastIndex = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Time < startTime || items[i].Time >= endTime) continue;
            // 获取此item是属于哪个时间段数据
            ids = null;
            for (; lastIndex < Info.Count; lastIndex++)
            {
                if (items[i].Time < Info[lastIndex].Time)
                {
                    if (!loginByTime.TryGetValue(Info[lastIndex - 1].Time, out ids))
                    {
                        ids = new HashSet<int>();
                        loginByTime.Add(Info[lastIndex - 1].Time, ids);
                    }
                    break;
                }
            }
            // 用Count存的注册ID
            if (ids == null && !loginByTime.TryGetValue(Info[lastIndex - 1].Time, out ids))
            {
                ids = new HashSet<int>();
                loginByTime.Add(Info[lastIndex - 1].Time, ids);
            }
            ids.Add(items[i].Count);
        }

        for (int i = 0; i < Info.Count; i++)
            if (loginByTime.TryGetValue(Info[i].Time, out ids))
                Info[i].Count = ids.Count;
    }
}
/// <summary>游戏时长</summary>
public class RetGameTimeItem
{
    /// <summary>时长(分钟)</summary>
    public int Time;
    /// <summary>数量</summary>
    public int Count;
}
/// <summary>游戏时长分析</summary>
public class RetGameTime
{
    /// <summary>所有游戏时长</summary>
    public List<RetGameTimeItem> GameTimes = new List<RetGameTimeItem>();
    /// <summary>总游戏时长</summary>
    public long TotalTime = 0;
    /// <summary>总登录次数</summary>
    public long TotalCount = 0;
    /// <summary>时长均值</summary>
    public double TimeAvg;

    /// <summary>计算时长均值</summary>
    public void Calc()
    {
        if (GameTimes.Count == 0) return;
        foreach (RetGameTimeItem item in GameTimes)
        {
            TotalTime += (item.Time * item.Count);
            TotalCount += item.Count;
        }
        TimeAvg = Math.Round((double)TotalTime / TotalCount, 2);
    }
}
/// <summary>游戏数据</summary>
public class GameDataItem
{
    /// <summary>游戏名</summary>
    public string GameName;
    /// <summary>渠道</summary>
    public string Channel;
    /// <summary>活跃数</summary>
    public int ActivityCount;
    /// <summary>新增用户数</summary>
    public int RegistCount;
    /// <summary>平均在线时长</summary>
    public double AvgOnlineTime;

    /// <summary>登录的ID</summary>
    public HashSet<int> LoginID { set; get; }

    public void Calc()
    {
        AvgOnlineTime = LoginID.Count != 0 ? Math.Round((AvgOnlineTime / LoginID.Count / 60), 2) : 0;
    }
}
/// <summary>游戏数据分析</summary>
public class RetGameData
{
    /// <summary>游戏数据条目</summary>
    public PagedModel<GameDataItem> Items = new PagedModel<GameDataItem>();
    /// <summary>游戏数据总计</summary>
    public GameDataItem Total = new GameDataItem();
}
/// <summary>系统用户</summary>
public class RetAccount
{
    /// <summary>账号ID</summary>
    public int ID;
    /// <summary>账号</summary>
    public string Account;
    /// <summary>昵称</summary>
    public string Nickname;
    /// <summary>账号类型</summary>
    public EAccountType Type;
    /// <summary>账号管理的游戏，当账号类型为1时不需要判断此字段</summary>
    public string[] ManagerGames;
    public string ManagerGame { get; set; }
    /// <summary>账号状态，false时为封禁</summary>
    public bool State;

    public void Calc()
    {
        if (!string.IsNullOrEmpty(ManagerGame))
        {
            var temp = ManagerGame.Replace("[", "").Replace("]", "").Split(',');
            ManagerGames = new string[temp.Length];
            for (int i = 0; i < temp.Length; i++)
                ManagerGames[i] = temp[i].Trim().Replace("\"", "");
        }
        else
            ManagerGames = new string[0];
    }
}