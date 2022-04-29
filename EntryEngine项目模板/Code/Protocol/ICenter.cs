using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;

/// <summary>后台管理系统需要登录针对某个用户的服务接口</summary>
[ProtocolStub(2, null)]
public interface ICenter
{
    void GetUserInfo(Action<T_CENTER_USER> callback);
}
