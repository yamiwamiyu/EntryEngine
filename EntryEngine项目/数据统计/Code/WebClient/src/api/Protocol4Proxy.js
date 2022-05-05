const Protocol4Proxy = {};
export default Protocol4Proxy;
Protocol4Proxy.onSend = null;
Protocol4Proxy.onSendOnce = null;
Protocol4Proxy.onCallback = null;
Protocol4Proxy.onErrorMsg = null;
Protocol4Proxy.onError = null;
Protocol4Proxy.url = "";
Protocol4Proxy.send = function(url, str, callback)
{
    var promise = new Promise(function(resolve, reject)
    {
        var req = new XMLHttpRequest();
        req.onreadystatechange = function()
        {
            if (req.readyState == 4)
            {
                if (Protocol4Proxy.onCallback) Protocol4Proxy.onCallback(req);
                if (req.status == 200)
                {
                    var obj = req.responseText ? JSON.parse(req.responseText) : null;
                    if (obj && obj.errCode)
                    {
                        if (Protocol4Proxy.onErrorMsg) { Protocol4Proxy.onErrorMsg(obj); }
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
                else if (Protocol4Proxy.onError) { Protocol4Proxy.onError(req); }
                else { console.error(req); }
            }
        }
        req.open("POST", Protocol4Proxy.url + url, true);
        req.responseType = "text";
        if (!Protocol4Proxy.onSendOnce) { req.setRequestHeader("Content-Type", "application/x-www-form-urlencoded;charset=utf-8"); }
        if (Protocol4Proxy.onSend) { Protocol4Proxy.onSend(req); }
        if (Protocol4Proxy.onSendOnce) { var __send = Protocol4Proxy.onSendOnce(req); Protocol4Proxy.onSendOnce = null; if (__send) { return; } }
        req.send(str);
    }
    );
    return promise;
}
Protocol4Proxy.Login = function(account, password, callback)
{
    var str = [];
    if (account) str.push("account=" + encodeURIComponent(account));
    if (password) str.push("password=" + encodeURIComponent(password));
    return Protocol4Proxy.send("4/Login", str.join("&"), callback);
}
