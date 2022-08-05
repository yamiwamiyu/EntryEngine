using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;

/// <summary>不需要登录的通用服务接口</summary>
[ProtocolStub(1, null)]
public interface IService
{
    #region 用户登录

    /// <summary>发送短信验证码</summary>
    /// <param name="phone">手机号码</param>
    /// <param name="callback">再次发送冷却时间（秒）</param>
    void SendSMSCode(string phone, Action<int> callback);

    /// <summary>手机验证码登录，没有账号则注册账号</summary>
    /// <param name="phone">手机号</param>
    /// <param name="code">验证码</param>
    /// <param name="callback">返回账号信息</param>
    void LoginBySMSCode(string phone, string code, Action<T_USER> callback);
    /// <summary>Token登录</summary>
    void LoginByToken(string token, Action<T_USER> callback);
    /// <summary>密码登录</summary>
    void LoginByPassword(string phone, string password, Action<T_USER> callback);
    /// <summary>忘记密码：手机号和验证码登录，并修改密码</summary>
    void ForgetPassword(string phone, string code, string password, Action<T_USER> callback);
    /// <summary>微信登录</summary>
    void LoginByWX(string code, Action<T_USER> callback);

    /// <summary>清理用户缓存，数据库有修改时，可以用此接口强制清空用户缓存而同步数据库数据</summary>
    void ClearUserCache(int id, Action<bool> callback);

    #endregion

    #region 后台登录

    void CenterLoginByPassword(string name, string password, Action<T_CENTER_USER> callback);
    void CenterLoginBySMSCode(string phone, string code, Action<T_CENTER_USER> callback);

    #endregion

    #region 上传文件

    void UploadImage(FileUpload file, Action<string> callback);
    void UploadFile(FileUpload file, Action<string> callback);

    #endregion

    #region 支付回调

    /// <summary>微信支付回调</summary>
    void WeChatPayCallback();
    /// <summary>微信支付退款回调</summary>
    void WeChatRefundCallback();
    /// <summary>微信JSSDK wx.config 时必要的参数</summary>
    void WXJSSDK(string url, Action<WXJSSDK> callback);


    /// <summary>支付宝支付回调</summary>
    void AlipayCallback(
        string trade_no, string out_trade_no,
        string buyer_id, string buyer_logon_id,
        string trade_status, string total_amount, string gmt_payment,
        Action<string> callback);
    

    #endregion
}
