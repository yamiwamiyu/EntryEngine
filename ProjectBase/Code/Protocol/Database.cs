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
    public static void CheckTelephone(string phone)
    {
        "无效的手机号码".Check(string.IsNullOrEmpty(phone) || phone.Length != 11 || !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^(13|14|15|16|18|19|17)\d{9}$"));
    }
    public static void CheckSMSCodeFormat(string smscode)
    {
        "验证码格式错误".Check((string.IsNullOrEmpty(smscode) || smscode.Length != 4) && smscode != 万能验证码);
    }
    public static T_SMSCode Send(string telphone)
    {
        CheckTelephone(telphone);

        T_SMSCode data;
        lock (smsCodes)
            if (smsCodes.TryGetValue(telphone, out data) && data.Code != 0 && data.ResendCountdown > 0)
                return data;

        bool add = data == null;
        if (add) data = new T_SMSCode();
        data.Mobile = long.Parse(telphone);
        data.CreatedTime = DateTime.Now;
        data.Code = _RANDOM.Next(1000, 10000);

        lock (smsCodes)
            smsCodes[telphone] = data;

        _LOG.Info("{0}的验证码:{1}", telphone, data.Code);

        return data;
    }
    public static void ValidCode(string telphone, string code)
    {
        if (!IsValid)
            return;
        if (code == 万能验证码)
            return;
        T_SMSCode sms;
        lock (smsCodes)
            "验证码错误".Check(!smsCodes.TryGetValue(telphone, out sms) || code != sms.Code.ToString());
        "验证码已过期".Check(DateTime.Now >= sms.ExpireTime);
        sms.Code = 0;
    }
    public static bool IsValid = true;
    #endregion
}

/// <summary>用户基类</summary>
public class T_UserBase
{
    public const int TOKEN_EXPIRE_MINUTES = 60 * 24 * 7;

    [Index(EIndex.Identity)]
    public int ID = 10000;
    [Index]
    public DateTime RegisterTime;
    [Index]
    public string Token;
    [Index]
    public string Account;
    [Index]
    public string Password;
    /// <summary>修改密码时不用再去翻数据库</summary>
    public string __Password { get; set; }
    /// <summary>手机号</summary>
    [Index]
    public long Phone;
    public long __Phone { get; set; }
    public DateTime LastLoginTime;
    /// <summary>用户Session缓存，每次请求不需要频繁刷新LastLoginTime，优化接口访问速度</summary>
    [Ignore]
    public DateTime LastRefreshLoginTime;

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
        __Phone = Phone;
        if (Phone != 0)
            Phone -= ((Phone / 10000) % 10000) * 10000;
        __Password = Password;
        Password = null;
        OnMaskData();
    }
    public bool IsMatchPassword(string password)
    {
        return Masked ? __Password == password : this.Password == password;
    }
    protected virtual void OnMaskData()
    {
    }
}
/// <summary>一个用户</summary>
[MemoryTable]
public class T_USER : T_UserBase
{
    [Index]
    public string Name;
    /// <summary>玩家进入的平台</summary>
    [Index(EIndex.Group)]
    public string Platform;
}
/// <summary>后台管理账号</summary>
[MemoryTable]
public class T_CENTER_USER : T_UserBase
{
    [Index]
    public string Name;
}

/// <summary>操作日志</summary>
[MemoryTable]
public class T_OPLog
{
    [Index(EIndex.Identity)]
    public int ID;

    [Foreign(typeof(T_USER), "ID")]
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
