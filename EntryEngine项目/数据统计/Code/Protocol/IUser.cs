using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;

/// <summary>需要登录针对某个用户的服务接口</summary>
[ProtocolStub(3, null)]
public interface IUser
{
    /// <summary>记录登录信息</summary>
    /// <param name="gameName">游戏名称</param>
    /// <param name="deviceID">设备号</param>
    /// <param name="channel">渠道号</param>
    void Login(string gameName, string deviceID, string channel, Action<int> callback);

    /// <summary>心跳在线时间</summary>
    /// <param name="loginID">登录ID</param>
    void Online(int loginID, Action<bool> callback);

    /// <summary>记录事件和操作</summary>
    /// <param name="loginID">登录ID</param>
    /// <param name="analysis">事件和操作的复合对象</param>
    void Analysis(int loginID, List<T_Analysis> analysis, Action<bool> callback);
}