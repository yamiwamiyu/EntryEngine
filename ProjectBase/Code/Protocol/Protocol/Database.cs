using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;

[MemoryTable]
public class T_PLAYER
{
    [Index(EIndex.Identity)]
    public int ID = 1000;
    [Index]
    public string Name;
    [Index]
    public string Password;
    [Index]
    public DateTime RegisterDate;
    /// <summary>玩家进入的平台</summary>
    [Index(EIndex.Group)]
    public string Platform;
    [Index]
    public string Token;
    public DateTime LastLoginTime;
}
/// <summary>操作日志</summary>
[MemoryTable]
public class T_OPLog
{
    [Index(EIndex.Identity)]
    public int ID;

    [Foreign(typeof(T_PLAYER), "ID")]
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
