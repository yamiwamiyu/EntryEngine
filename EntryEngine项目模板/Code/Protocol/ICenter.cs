using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;

/// <summary>后台管理系统需要登录针对某个用户的服务接口</summary>
[ProtocolStub(2, null)]
public interface ICenter
{
    /// <summary>用户信息</summary>
    void UserInfo(Action<T_CENTER_USER> callback);
    /// <summary>用户修改密码</summary>
    /// <param name="opass">原密码</param>
    /// <param name="npass">新密码</param>
    void UserModifyPassword(string opass, string npass, Action<bool> callback);
    /// <summary>用户修改手机号</summary>
    /// <param name="phone">新手机号</param>
    /// <param name="code">验证码</param>
    void UserModifyPhone(string phone, string code, Action<bool> callback);
    /// <summary>用户退出登录</summary>
    void UserExitLogin(Action<bool> callback);
}
