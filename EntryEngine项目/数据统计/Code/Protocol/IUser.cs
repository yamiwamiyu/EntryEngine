using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;

/// <summary>需要登录针对某个用户的服务接口</summary>
[ProtocolStub(3, null)]
public interface IUser
{
    void GetUserInfo(Action<T_USER> callback);
}