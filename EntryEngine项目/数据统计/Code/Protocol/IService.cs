using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;

/// <summary>不需要登录的通用服务接口</summary>
[ProtocolStub(1, null)]
public interface IService
{
    /// <summary>发送短信验证码</summary>
    /// <param name="phone">手机号码</param>
    /// <param name="callback">再次发送冷却时间（秒）</param>
    void SendSMSCode(string phone, Action<int> callback);
    void CenterLoginByPassword(string name, string password, Action<T_CENTER_USER> callback);
    void CenterLoginBySMSCode(string phone, string code, Action<T_CENTER_USER> callback);
}
