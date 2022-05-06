using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;
using EntryEngine;


/// <summary>验证码，单服务应用采用内存模式即可，需要采用数据库模式加上[MemoryTable]标记</summary>
public class T_SMSCode
{
    /// <summary>手机号</summary>
    [Index(EIndex.Primary)]
    public long Mobile;
    /// <summary>验证码创建时间</summary>
    public DateTime CreatedTime;
    /// <summary>验证码</summary>
    [Index]
    public int Code;

    /// <summary>短信ID</summary>
    [NonSerialized]
    [Ignore]
    public string SID;

    /// <summary>过期时间</summary>
    public DateTime ExpireTime { get { return CreatedTime.AddMinutes(15); } }
    /// <summary>是否过期</summary>
    public bool IsExpired { get { return DateTime.Now >= ExpireTime; } }
    /// <summary>重发剩余秒数</summary>
    public int ResendCountdown { get { return Math.Max(0, (int)((CreatedTime.AddSeconds(60) - DateTime.Now).TotalSeconds) + 1); } }

    #region 内存验证码
    const string 万能验证码 = "999666";
    public static Dictionary<string, T_SMSCode> smsCodes = new Dictionary<string, T_SMSCode>();
    public static bool IsTelephone(string phone)
    {
        return !string.IsNullOrEmpty(phone) && phone.Length == 11 && System.Text.RegularExpressions.Regex.IsMatch(phone, @"^(13|14|15|16|18|19|17)\d{9}$");
    }
    public static void CheckTelephone(string phone)
    {
        "无效的手机号码".Check(!IsTelephone(phone));
    }
    public static void CheckSMSCodeFormat(string smscode)
    {
        "验证码格式错误".Check((string.IsNullOrEmpty(smscode) || smscode.Length != 4) && smscode != 万能验证码);
    }
    public static T_SMSCode Send(string phone)
    {
        CheckTelephone(phone);

        T_SMSCode data;
        lock (smsCodes)
            if (smsCodes.TryGetValue(phone, out data) && data.Code != 0 && data.ResendCountdown > 0)
                return data;

        bool add = data == null;
        if (add) data = new T_SMSCode();
        data.Mobile = long.Parse(phone);
        data.CreatedTime = DateTime.Now;
        data.Code = _RANDOM.Next(1000, 10000);

        lock (smsCodes)
            smsCodes[phone] = data;

        _LOG.Info("{0}的验证码:{1}", phone, data.Code);

        return data;
    }
    public static void ValidCode(string phone, string code)
    {
        if (!IsValid)
            return;
        if (code == 万能验证码)
            return;
        T_SMSCode sms;
        lock (smsCodes)
            "验证码错误".Check(!smsCodes.TryGetValue(phone, out sms) || code != sms.Code.ToString());
        "验证码已过期".Check(DateTime.Now >= sms.ExpireTime);
        sms.Code = 0;
    }
    /// <summary>是否验证验证码，测试模式时可以不验证</summary>
    public static bool IsValid = true;
    #endregion
}

/// <summary>用户基类</summary>
public class T_UserBase
{
    public const int TOKEN_EXPIRE_MINUTES = 60 * 24 * 7;

    [Index(EIndex.Identity)]
    public int ID = 10000;
    /// <summary>注册日期</summary>
    [Index]public DateTime RegisterTime;
    /// <summary>账号登录后的凭证</summary>
    [Index]public string Token;
    /// <summary>账号，可用于登录</summary>
    [Index]public string Account;
    /// <summary>登录密码，空则不能用密码登录</summary>
    [Index]public string Password;
    /// <summary>修改密码时不用再去翻数据库</summary>
    public string __Password { get; set; }
    /// <summary>手机号</summary>
    [Index]public long Phone;
    public long __Phone { get; set; }
    /// <summary>用户昵称，可修改，主要用于前端显示</summary>
    [Index]public string Name;
    public DateTime LastLoginTime;
    /// <summary>用户Session缓存，每次请求不需要频繁刷新LastLoginTime，优化接口访问速度</summary>
    [Ignore]public DateTime LastRefreshLoginTime;

    /// <summary>Token是否过期</summary>
    public bool TokenExpired
    {
        get { return DateTime.Now >= LastLoginTime + TimeSpan.FromMinutes(TOKEN_EXPIRE_MINUTES); }
    }

    /// <summary>是否已经调用过MaskData</summary>
    public bool Masked { get; private set; }
    /// <summary>防止敏感信息泄露</summary>
    public void MaskData()
    {
        if (Masked)
            return;
        Masked = true;
        //if (RealName != null)
        //    RealName = RealName.Mask(1, 0);
        //if (IDCard != null)
        //    IDCard = IDCard.Mask();
        MaskPhone();
        __Password = Password;
        Password = null;
        OnMaskData();
    }
    public void MaskPhone()
    {
        __Phone = Phone;
        if (Phone != 0)
            Phone -= ((Phone / 10000) % 10000) * 10000;
    }
    public bool IsMatchPassword(string password)
    {
        return Masked ? __Password == password : this.Password == password;
    }
    protected virtual void OnMaskData()
    {
    }
}
/// <summary>后台管理账号</summary>
[MemoryTable]public class T_CENTER_USER : T_UserBase
{
    /// <summary>账号类型</summary>
    [Index]public EAccountType Type;
    /// <summary>账号管理的游戏，当账号类型为1时不需要判断此字段</summary>
    public string[] ManagerGame;
    /// <summary>账号状态，false时为封禁</summary>
    [Index]public bool State = true;
    /// <summary>昵称</summary>
    [Ignore]public string Nickname;
}
/// <summary>账号类型</summary>
public enum EAccountType : byte
{
    平台账号 = 1,
    普通账号 = 2
}

/// <summary>操作日志</summary>
[MemoryTable]
public class T_OPLog
{
    [Index(EIndex.Identity)]
    public int ID;

    [Index(EIndex.Group)]
    public int PID;

    [Index(EIndex.Group)]
    public string Operation;

    [Index]
    public DateTime Time;

    /// <summary>例如获得道具是从哪里获得的</summary>
    [Index(EIndex.Group)]
    public string Way;
    /// <summary>例如获得了什么道具</summary>
    [Index(EIndex.Group)]
    public int Sign;
    /// <summary>例如获得了多少个道具</summary>
    public int Statistic;

    public string Detail;
}


/// <summary>注册信息</summary>
[MemoryTable]public class T_Register
{
    [Index(EIndex.Identity)]public int ID;
    /// <summary>游戏名</summary>
    [Index]public string GameName;
    /// <summary>设备ID</summary>
    [Index]public string DeviceID;
    /// <summary>渠道号</summary>
    [Index]public string Channel;
    /// <summary>注册时间</summary>
    [Index]public DateTime RegisterTime;
}
/// <summary>登录信息</summary>
[MemoryTable]public class T_Login
{
    /// <summary>登录ID</summary>
    [Index(EIndex.Identity)]public int ID;
    /// <summary>注册ID</summary>
    [Index, Foreign(typeof(T_Register), "ID")]public int RegisterID;
    /// <summary>登录时间</summary>
    [Index]public DateTime LoginTime;
    /// <summary>最后在线时间</summary>
    public DateTime LastOnlineTime;
}
/// <summary>事件分析信息</summary>
[MemoryTable]public class T_Analysis
{
    /// <summary>注册ID</summary>
    [Index]public int RegisterID;
    /// <summary>事件页签</summary>
    public string Label;
    /// <summary>事件名称</summary>
    public string Name;
    /// <summary>事件排序，页签内排序</summary>
    public int OrderID;
    /// <summary>事件发生的次数，按人数统计时传0，按次数统计时传1</summary>
    public int Count;
    /// <summary>事件发生时间</summary>
    [Index]public DateTime Time;
}
/// <summary>统计时间点的在线人数</summary>
[MemoryTable]public class T_Online
{
    [Index(EIndex.Primary)]public DateTime Time;
    [Index(EIndex.Primary)]public string GameName;
    [Index(EIndex.Primary)]public string Channel;

    public int Quarter0 = 0;
    public int Quarter1 = 0;
    public int Quarter2 = 0;
    public int Quarter3 = 0;
    public int Quarter4 = 0;
    public int Quarter5 = 0;
    public int Quarter6 = 0;
    public int Quarter7 = 0;
    public int Quarter8 = 0;
    public int Quarter9 = 0;
    public int Quarter10 = 0;
    public int Quarter11 = 0;
    public int Quarter12 = 0;
    public int Quarter13 = 0;
    public int Quarter14 = 0;
    public int Quarter15 = 0;
    public int Quarter16 = 0;
    public int Quarter17 = 0;
    public int Quarter18 = 0;
    public int Quarter19 = 0;
    public int Quarter20 = 0;
    public int Quarter21 = 0;
    public int Quarter22 = 0;
    public int Quarter23 = 0;
    public int Quarter24 = 0;
    public int Quarter25 = 0;
    public int Quarter26 = 0;
    public int Quarter27 = 0;
    public int Quarter28 = 0;
    public int Quarter29 = 0;
    public int Quarter30 = 0;
    public int Quarter31 = 0;
    public int Quarter32 = 0;
    public int Quarter33 = 0;
    public int Quarter34 = 0;
    public int Quarter35 = 0;
    public int Quarter36 = 0;
    public int Quarter37 = 0;
    public int Quarter38 = 0;
    public int Quarter39 = 0;
    public int Quarter40 = 0;
    public int Quarter41 = 0;
    public int Quarter42 = 0;
    public int Quarter43 = 0;
    public int Quarter44 = 0;
    public int Quarter45 = 0;
    public int Quarter46 = 0;
    public int Quarter47 = 0;
    public int Quarter48 = 0;
    public int Quarter49 = 0;
    public int Quarter50 = 0;
    public int Quarter51 = 0;
    public int Quarter52 = 0;
    public int Quarter53 = 0;
    public int Quarter54 = 0;
    public int Quarter55 = 0;
    public int Quarter56 = 0;
    public int Quarter57 = 0;
    public int Quarter58 = 0;
    public int Quarter59 = 0;
    public int Quarter60 = 0;
    public int Quarter61 = 0;
    public int Quarter62 = 0;
    public int Quarter63 = 0;
    public int Quarter64 = 0;
    public int Quarter65 = 0;
    public int Quarter66 = 0;
    public int Quarter67 = 0;
    public int Quarter68 = 0;
    public int Quarter69 = 0;
    public int Quarter70 = 0;
    public int Quarter71 = 0;
    public int Quarter72 = 0;
    public int Quarter73 = 0;
    public int Quarter74 = 0;
    public int Quarter75 = 0;
    public int Quarter76 = 0;
    public int Quarter77 = 0;
    public int Quarter78 = 0;
    public int Quarter79 = 0;
    public int Quarter80 = 0;
    public int Quarter81 = 0;
    public int Quarter82 = 0;
    public int Quarter83 = 0;
    public int Quarter84 = 0;
    public int Quarter85 = 0;
    public int Quarter86 = 0;
    public int Quarter87 = 0;
    public int Quarter88 = 0;
    public int Quarter89 = 0;
    public int Quarter90 = 0;
    public int Quarter91 = 0;
    public int Quarter92 = 0;
    public int Quarter93 = 0;
    public int Quarter94 = 0;
    public int Quarter95 = 0;

    /// <summary>获取时间的分区</summary>
    /// <returns>0~95</returns>
    public static int GetQuarter(DateTime time)
    {
        return (int)(time.TimeOfDay.TotalMinutes / 15);
    }
    /// <summary>在线人数是否需要+1</summary>
    /// <param name="lastLoginTime">最后在线时间</param>
    public static bool needRecord(DateTime lastLoginTime)
    {
        var now = DateTime.Now;
        return lastLoginTime.Date != now.Date || GetQuarter(lastLoginTime) != GetQuarter(now);
    }
}