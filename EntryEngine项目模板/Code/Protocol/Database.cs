﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;
using EntryEngine;


/// <summary>验证码，单服务应用采用内存模式即可，需要采用数据库模式加上[MemoryTable]标记</summary>
public class T_SMSCode
{
    /// <summary>手机号</summary>
    [Index(EIndex.Primary)]public long Mobile;
    /// <summary>验证码创建时间</summary>
    public DateTime CreatedTime;
    /// <summary>验证码</summary>
    [Index]public int Code;

    /// <summary>短信ID</summary>
    [NonSerialized][Ignore]public string SID;

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

    [Index(EIndex.Identity)]public int ID = 10000;
    /// <summary>注册日期</summary>
    [Index]public DateTime RegisterTime;
    /// <summary>账号登录后的凭证</summary>
    [Index]public string Token;
    /// <summary>账号，可用于登录</summary>
    [Index]public string Account;
    /// <summary>账号也为敏感信息，会被Mask掉</summary>
    public string __Account { get; set; }
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
        __Account = Account;
        Account = null;
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
/// <summary>一个用户</summary>
[MemoryTable]public class T_USER : T_UserBase
{
    /// <summary>玩家进入的平台</summary>
    [Index(EIndex.Group)]public string Platform;
}
/// <summary>后台管理账号</summary>
[MemoryTable]public class T_CENTER_USER : T_UserBase { }

/// <summary>操作日志</summary>
[MemoryTable]public class T_OPLog
{
    [Index(EIndex.Identity)]public int ID;
    [Index(EIndex.Group)]public int PID;
    [Index(EIndex.Group)]public string Operation;
    [Index]public DateTime Time;
    /// <summary>例如获得道具是从哪里获得的</summary>
    [Index(EIndex.Group)]public string Way;
    /// <summary>例如获得了什么道具</summary>
    [Index(EIndex.Group)]public int Sign;
    /// <summary>例如获得了多少个道具</summary>
    public int Statistic;
    public string Detail;
}
