const IServiceProxy = {};
export default IServiceProxy;
IServiceProxy.onSend = null;
IServiceProxy.onSendOnce = null;
IServiceProxy.onCallback = null;
IServiceProxy.onErrorMsg = null;
IServiceProxy.onError = null;
IServiceProxy.url = "";
IServiceProxy.send = function(url, str, callback)
{
    var promise = new Promise(function(resolve, reject)
    {
        var req = new XMLHttpRequest();
        req.onreadystatechange = function()
        {
            if (req.readyState == 4)
            {
                if (IServiceProxy.onCallback) IServiceProxy.onCallback(req);
                if (req.status == 200)
                {
                    var obj = req.responseText ? JSON.parse(req.responseText) : null;
                    if (obj && obj.errCode)
                    {
                        if (IServiceProxy.onErrorMsg) { IServiceProxy.onErrorMsg(obj); }
                        else { console.log(obj); }
                        if (reject) { reject(obj); }
                    }
                    else
                    {
                        if (callback) { callback(obj); }
                        else { console.log(obj); }
                        if (resolve) { resolve(obj); }
                    }
                }
                else if (IServiceProxy.onError) { IServiceProxy.onError(req); }
                else { console.error(req); }
            }
        }
        req.open("POST", IServiceProxy.url + url, true);
        req.responseType = "text";
        if (!IServiceProxy.onSendOnce) { req.setRequestHeader("Content-Type", "application/x-www-form-urlencoded;charset=utf-8"); }
        if (IServiceProxy.onSend) { IServiceProxy.onSend(req); }
        if (IServiceProxy.onSendOnce) { var __send = IServiceProxy.onSendOnce(req); IServiceProxy.onSendOnce = null; if (__send) { return; } }
        req.send(str);
    }
    );
    return promise;
}
IServiceProxy.SendSMSCode = function(phone, callback)
{
    var str = [];
    if (phone) str.push("phone=" + encodeURIComponent(phone));
    return IServiceProxy.send("4/SendSMSCode", str.join("&"), callback);
}
IServiceProxy.Login = function(account, password, callback)
{
    var str = [];
    if (account) str.push("account=" + encodeURIComponent(account));
    if (password) str.push("password=" + encodeURIComponent(password));
    return IServiceProxy.send("4/Login", str.join("&"), callback);
}
IServiceProxy.CenterLoginBySMSCode = function(phone, code, callback)
{
    var str = [];
    if (phone) str.push("phone=" + encodeURIComponent(phone));
    if (code) str.push("code=" + encodeURIComponent(code));
    return IServiceProxy.send("4/CenterLoginBySMSCode", str.join("&"), callback);
}
