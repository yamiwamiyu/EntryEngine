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
            // 登录账号
            //var player = _DB._T_USER.Select(null, "WHERE Name=@p0", name);
            // 并行时用此接口创建角色会创建多个相同角色
            // 仅方便测试的话，可以不需要这个检测，以自动创建角色或登录角色
            //if (player == null)
            //    throw new InvalidOperationException("账号不存在");
            callback.Callback(new ImplIUser().LoginByPhone(telphone, null, null, null));
        }
        void _IService.LoginByToken(string token, CBIService_LoginByToken callback)
        {
            callback.Callback(new ImplIUser().LoginByToken(token));
        }
        void _IService.LoginByPassword(string telphone, string password, CBIService_LoginByPassword callback)
        {
            callback.Callback(new ImplIUser().LoginByPassword(telphone, password, true));
        }
        void _IService.ForgetPassword(string telphone, string code, string password, CBIService_ForgetPassword callback)
        {
            "密码不能为空".Check(string.IsNullOrEmpty(password));
            // 验证验证码
            T_SMSCode.CheckTelephone(telphone);
            T_SMSCode.CheckSMSCodeFormat(code);
            T_SMSCode.ValidCode(telphone, code);
            callback.Callback(new ImplIUser().ForgetPassword(telphone, password));
        }

        void _IService.CenterLoginBySMSCode(string telphone, string code, CBIService_CenterLoginBySMSCode callback)
        {
            // 验证验证码
            T_SMSCode.CheckTelephone(telphone);
            T_SMSCode.CheckSMSCodeFormat(code);
            T_SMSCode.ValidCode(telphone, code);
        }
        void _IService.CenterLoginByPassword(string name, string password, CBIService_CenterLoginByPassword callback)
        {
            throw new NotImplementedException();
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
