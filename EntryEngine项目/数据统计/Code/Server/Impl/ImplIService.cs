using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using EntryEngine.Network;
using System.Net;

namespace Server.Impl
{
    class ImplIService : _IService
    {
        void _IService.SendSMSCode(string phone, CBIService_SendSMSCode callback)
        {
            var send = T_SMSCode.Send(phone);
            _DB.SendSMSCode(send);
            callback.Callback(send.ResendCountdown);
        }
        void _IService.CenterLoginBySMSCode(string phone, string code, CBIService_CenterLoginBySMSCode callback)
        {
            // 验证验证码
            T_SMSCode.CheckTelephone(phone);
            T_SMSCode.CheckSMSCodeFormat(code);
            T_SMSCode.ValidCode(phone, code);
            var impl = new ImplICenter();
            long _phone = long.Parse(phone);
            impl.InitializeByPhone(_phone);
            "此用户不存在".Check(impl.User == null);
            impl.Login(impl.User, ELoginWay.手机号);
            callback.Callback(impl.User);
        }
        void _IService.Login(string name, string password, CBIService_Login callback)
        {
            "账户名不能为空".Check(string.IsNullOrEmpty(name));
            "密码不能为空".Check(string.IsNullOrEmpty(password));
            var impl = new ImplICenter();
            impl.InitializeByAccount(name);
            "账号密码不正确".Check(impl.User == null || !impl.User.IsMatchPassword(password));
            impl.Login(impl.User, ELoginWay.密码);
            callback.Callback(impl.User.Token);
        }
    }
}
