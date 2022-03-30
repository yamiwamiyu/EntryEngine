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
        void _IService.SendSMSCode(string telphone, CBIService_SendSMSCode callback)
        {
            var send = T_SMSCode.Send(telphone);
            _DB.SendSMSCode(send);
            callback.Callback(send.ResendCountdown);
        }

        void _IService.LoginBySMSCode(string telphone, string code, CBIService_LoginBySMSCode callback)
        {
            // 验证验证码
            T_SMSCode.CheckTelephone(telphone);
            T_SMSCode.CheckSMSCodeFormat(code);
            T_SMSCode.ValidCode(telphone, code);

            // 并行时用此接口创建角色会创建多个相同角色
            long phone = long.Parse(telphone);
            var impl = new ImplIUser();
            impl.InitializeByPhone(phone);
            if (impl.User == null)
            {
                T_USER newUser = new T_USER();
                newUser.Phone = phone;
                newUser.Account = telphone;
                newUser.Name = telphone.Mask();
                impl.Register(newUser, ELoginWay.手机号);
            }
            else
            {
                impl.Login(impl.User, ELoginWay.手机号);
            }
            callback.Callback(impl.User);
        }
        void _IService.LoginByToken(string token, CBIService_LoginByToken callback)
        {
            if (string.IsNullOrEmpty(token))
            {
                callback.Callback(null);
                return;
            }
            var impl = new ImplIUser();
            impl.InitializeByToken(token);
            if (impl.User == null || impl.User.TokenExpired)
            {
                callback.Callback(null);
                return;
            }
            impl.Login(impl.User, ELoginWay.Token);
            callback.Callback(impl.User);
        }
        void _IService.LoginByPassword(string telphone, string password, CBIService_LoginByPassword callback)
        {
            "账户名不能为空".Check(string.IsNullOrEmpty(telphone));
            "密码不能为空".Check(string.IsNullOrEmpty(password));
            var impl = new ImplIUser();
            impl.InitializeByAccount(telphone);
            "账号密码不正确".Check(impl.User == null || !impl.User.IsMatchPassword(password));
            impl.Login(impl.User, ELoginWay.密码);
            callback.Callback(impl.User);
        }
        void _IService.ForgetPassword(string telphone, string code, string password, CBIService_ForgetPassword callback)
        {
            "密码不能为空".Check(string.IsNullOrEmpty(password));
            // 验证验证码
            T_SMSCode.CheckTelephone(telphone);
            T_SMSCode.CheckSMSCodeFormat(code);
            T_SMSCode.ValidCode(telphone, code);

            long phone = long.Parse(telphone);
            var impl = new ImplIUser();
            impl.InitializeByPhone(phone);
            "此用户不存在".Check(impl.User == null);
            // 修改密码
            impl.User.Password = password;
            _DB._T_USER.GetUpdateSQL(impl.User, null, impl.SaveBuilder, impl.SaveValues, ET_USER.Password);
            if (impl.User.Masked)
            {
                impl.User.__Password = password;
                impl.User.Password = null;
            }
            impl.Login(impl.User, ELoginWay.忘记密码);
            callback.Callback(impl.User);
        }
        void _IService.ClearUserCache(int id, CBIService_ClearUserCache callback)
        {
            callback.Callback(ImplIUser.ClearUserCache(id));
        }

        void _IService.CenterLoginBySMSCode(string telphone, string code, CBIService_CenterLoginBySMSCode callback)
        {
            // 验证验证码
            T_SMSCode.CheckTelephone(telphone);
            T_SMSCode.CheckSMSCodeFormat(code);
            T_SMSCode.ValidCode(telphone, code);
            var impl = new ImplICenter();
            long phone = long.Parse(telphone);
            impl.InitializeByPhone(phone);
            "此用户不存在".Check(impl.User == null);
            impl.Login(impl.User, ELoginWay.手机号);
            callback.Callback(impl.User);
        }
        void _IService.CenterLoginByPassword(string name, string password, CBIService_CenterLoginByPassword callback)
        {
            "账户名不能为空".Check(string.IsNullOrEmpty(name));
            "密码不能为空".Check(string.IsNullOrEmpty(password));
            var impl = new ImplICenter();
            impl.InitializeByAccount(name);
            "账号密码不正确".Check(impl.User == null || !impl.User.IsMatchPassword(password));
            impl.Login(impl.User, ELoginWay.密码);
            callback.Callback(impl.User);
        }

        void _IService.UploadImage(FileUpload file, CBIService_UploadImage callback)
        {
            "请上传正确的文件".Check(file == null);
            file.Filename.CheckFileType(".jpe", ".gif", ".jpg", ".jpeg", ".png");
            callback.Callback(_FILE.WriteUploadFile(file, false));
        }
        void _IService.UploadFile(FileUpload file, CBIService_UploadFile callback)
        {
            "请上传正确的文件".Check(file == null);
            callback.Callback(_FILE.WriteUploadFile(file, false));
        }

        void _IService.WeChatPayCallback(HttpListenerContext __context)
        {
//            __context.Response.ContentEncoding = __context.Request.ContentEncoding;

//            using (__context.Request.InputStream)
//            {
//                __context.Response.ContentEncoding = __context.Request.ContentEncoding;

//                using (__context.Request.InputStream)
//                {
//                    try
//                    {
//                        byte[] bytes = _IO.ReadStream(__context.Request.InputStream);
//                        string text = Encoding.UTF8.GetString(bytes);
//                        _LOG.Info("微信支付回调: {0}", text);
//                        _DB._WX.WxPayData data = new _DB._WX.WxPayData();
//                        data.FromXml(text);

//                        // 完成支付处理
//                        {
//                            var orderID = data.GetValue("out_trade_no").ToString();

//                            // 处理成功
//                            bytes = __context.Response.ContentEncoding.GetBytes(
//    @"<xml>
//  <return_code><![CDATA[SUCCESS]]></return_code>
//  <return_msg><![CDATA[OK]]></return_msg>
//</xml>");
//                            __context.Response.OutputStream.Write(bytes, 0, bytes.Length);
//                            __context.Response.Close();

//                            _LOG.Info("微信支付回调成功: {0}", orderID);

//                            // 首次收到付款成功的回调
//                            var wxresult = _DB._WX.RetQueryOrder.FromWxPayData(data);
//                            // todo: 支付成功
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        _DB._WX.WxPayData res = new _DB._WX.WxPayData();
//                        res.SetValue("return_code", "FAIL");
//                        res.SetValue("return_msg", ex.Message);
//                        _LOG.Error(ex, "微信支付回调异常");
//                        byte[] bytes = __context.Response.ContentEncoding.GetBytes(res.ToXml());
//                        __context.Response.OutputStream.Write(bytes, 0, bytes.Length);
//                        __context.Response.Close();
//                    }
//                }
//            }
        }
        void _IService.AlipayCallback(string trade_no, string out_trade_no, string buyer_id, string buyer_logon_id, string trade_status, string total_amount, string gmt_payment, CBIService_AlipayCallback callback)
        {
            //_LOG.Info("支付宝支付回调：trace_no: {0} out_trade_no: {1} total_amount: {2}", trade_no, out_trade_no, total_amount);

            //try
            //{
            //    _DB._ZFB.RetOrderQueryData data = new _DB._ZFB.RetOrderQueryData();
            //    data.buyer_logon_id = buyer_logon_id;
            //    data.buyer_user_id = buyer_id;
            //    data.out_trade_no = out_trade_no;
            //    data.trade_no = trade_no;
            //    data.total_amount = total_amount;
            //    data.trade_status = trade_status;
            //    data.send_pay_date = gmt_payment;

            //    // todo: 支付成功

            //    callback.__link.Response("success");
            //    _LOG.Info("支付宝支付回调成功：{0}", out_trade_no);
            //}
            //catch (Exception ex)
            //{
            //    _LOG.Error(ex, "支付宝支付回调异常");
            //    callback.__link.Response("failed");
            //}
        }
    }
}
