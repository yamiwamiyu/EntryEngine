const ICenterProxy = {};
export default ICenterProxy;
ICenterProxy.onSend = null;
ICenterProxy.onSendOnce = null;
ICenterProxy.onCallback = null;
ICenterProxy.onErrorMsg = null;
ICenterProxy.onError = null;
ICenterProxy.url = "";
ICenterProxy.send = function(url, str, callback)
{
    var promise = new Promise(function(resolve, reject)
    {
        var req = new XMLHttpRequest();
        req.onreadystatechange = function()
        {
            if (req.readyState == 4)
            {
                if (ICenterProxy.onCallback) ICenterProxy.onCallback(req);
                if (req.status == 200)
                {
                    var obj = req.responseText ? JSON.parse(req.responseText) : null;
                    if (obj && obj.errCode)
                    {
                        if (ICenterProxy.onErrorMsg) { ICenterProxy.onErrorMsg(obj); }
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
                else if (ICenterProxy.onError) { ICenterProxy.onError(req); }
                else { console.error(req); }
            }
        }
        req.open("POST", ICenterProxy.url + url, true);
        req.responseType = "text";
        if (!ICenterProxy.onSendOnce) { req.setRequestHeader("Content-Type", "application/x-www-form-urlencoded;charset=utf-8"); }
        if (ICenterProxy.onSend) { ICenterProxy.onSend(req); }
        if (ICenterProxy.onSendOnce) { var __send = ICenterProxy.onSendOnce(req); ICenterProxy.onSendOnce = null; if (__send) { return; } }
        req.send(str);
    }
    );
    return promise;
}
ICenterProxy.GetUserInfo = function(callback)
{
    var str = [];
    return ICenterProxy.send("5/GetUserInfo", str.join("&"), callback);
}
ICenterProxy.ChangePassword = function(oldPassword, newPassword, callback)
{
    var str = [];
    if (oldPassword) str.push("oldPassword=" + encodeURIComponent(oldPassword));
    if (newPassword) str.push("newPassword=" + encodeURIComponent(newPassword));
    return ICenterProxy.send("5/ChangePassword", str.join("&"), callback);
}
ICenterProxy.GetGameName = function(callback)
{
    var str = [];
    return ICenterProxy.send("5/GetGameName", str.join("&"), callback);
}
ICenterProxy.GetChannel = function(gameName, callback)
{
    var str = [];
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    return ICenterProxy.send("5/GetChannel", str.join("&"), callback);
}
ICenterProxy.GetAnalysisGame = function(callback)
{
    var str = [];
    return ICenterProxy.send("5/GetAnalysisGame", str.join("&"), callback);
}
ICenterProxy.GetAnalysisLabel = function(gameName, callback)
{
    var str = [];
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    return ICenterProxy.send("5/GetAnalysisLabel", str.join("&"), callback);
}
ICenterProxy.GetAnalysis = function(gameName, channel, label, startTime, endTime, callback)
{
    var str = [];
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(channel));
    if (label) str.push("label=" + encodeURIComponent(label));
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return ICenterProxy.send("5/GetAnalysis", str.join("&"), callback);
}
ICenterProxy.GetRetained = function(page, gameName, channel, startTime, endTime, callback)
{
    var str = [];
    if (page) str.push("page=" + page);
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(channel));
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return ICenterProxy.send("5/GetRetained", str.join("&"), callback);
}
ICenterProxy.GetRetained2 = function(page, gameName, channel, startTime, endTime, callback)
{
    var str = [];
    if (page) str.push("page=" + page);
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(channel));
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return ICenterProxy.send("5/GetRetained2", str.join("&"), callback);
}
ICenterProxy.OnlineCount = function(gameName, channel, unit, startTime, endTime, callback)
{
    var str = [];
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(channel));
    if (unit) str.push("unit=" + unit);
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return ICenterProxy.send("5/OnlineCount", str.join("&"), callback);
}
ICenterProxy.GameTime = function(gameName, channel, startTime, endTime, callback)
{
    var str = [];
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(channel));
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return ICenterProxy.send("5/GameTime", str.join("&"), callback);
}
ICenterProxy.GetGameData = function(page, gameName, channel, startTime, endTime, callback)
{
    var str = [];
    if (page) str.push("page=" + page);
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(channel));
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return ICenterProxy.send("5/GetGameData", str.join("&"), callback);
}
ICenterProxy.GetGameData2 = function(page, gameName, channel, startTime, endTime, callback)
{
    var str = [];
    if (page) str.push("page=" + page);
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(channel));
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return ICenterProxy.send("5/GetGameData2", str.join("&"), callback);
}
ICenterProxy.GetAccountList = function(page, account, callback)
{
    var str = [];
    if (page) str.push("page=" + page);
    if (account) str.push("account=" + encodeURIComponent(account));
    return ICenterProxy.send("5/GetAccountList", str.join("&"), callback);
}
ICenterProxy.BindGame = function(identityID, gameNames, callback)
{
    var str = [];
    if (identityID) str.push("identityID=" + identityID);
    if (gameNames) str.push("gameNames=" + encodeURIComponent(JSON.stringify(gameNames)));
    return ICenterProxy.send("5/BindGame", str.join("&"), callback);
}
ICenterProxy.ModifyAccount = function(identityID, account, password, type, nickName, callback)
{
    var str = [];
    if (identityID) str.push("identityID=" + identityID);
    if (account) str.push("account=" + encodeURIComponent(account));
    if (password) str.push("password=" + encodeURIComponent(password));
    if (type) str.push("type=" + type);
    if (nickName) str.push("nickName=" + encodeURIComponent(nickName));
    return ICenterProxy.send("5/ModifyAccount", str.join("&"), callback);
}
ICenterProxy.DeleteAccount = function(identityID, callback)
{
    var str = [];
    if (identityID) str.push("identityID=" + identityID);
    return ICenterProxy.send("5/DeleteAccount", str.join("&"), callback);
}
ICenterProxy.ChangeAccountState = function(identityID, callback)
{
    var str = [];
    if (identityID) str.push("identityID=" + identityID);
    return ICenterProxy.send("5/ChangeAccountState", str.join("&"), callback);
}
