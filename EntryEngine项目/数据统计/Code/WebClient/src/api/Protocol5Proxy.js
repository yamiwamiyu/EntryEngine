const Protocol5Proxy = {};
export default Protocol5Proxy;
Protocol5Proxy.onSend = null;
Protocol5Proxy.onSendOnce = null;
Protocol5Proxy.onCallback = null;
Protocol5Proxy.onErrorMsg = null;
Protocol5Proxy.onError = null;
Protocol5Proxy.url = "";
Protocol5Proxy.send = function(url, str, callback)
{
    var promise = new Promise(function(resolve, reject)
    {
        var req = new XMLHttpRequest();
        req.onreadystatechange = function()
        {
            if (req.readyState == 4)
            {
                if (Protocol5Proxy.onCallback) Protocol5Proxy.onCallback(req);
                if (req.status == 200)
                {
                    var obj = req.responseText ? JSON.parse(req.responseText) : null;
                    if (obj && obj.errCode)
                    {
                        if (Protocol5Proxy.onErrorMsg) { Protocol5Proxy.onErrorMsg(obj); }
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
                else if (Protocol5Proxy.onError) { Protocol5Proxy.onError(req); }
                else { console.error(req); }
            }
        }
        req.open("POST", Protocol5Proxy.url + url, true);
        req.responseType = "text";
        if (!Protocol5Proxy.onSendOnce) { req.setRequestHeader("Content-Type", "application/x-www-form-urlencoded;charset=utf-8"); }
        if (Protocol5Proxy.onSend) { Protocol5Proxy.onSend(req); }
        if (Protocol5Proxy.onSendOnce) { var __send = Protocol5Proxy.onSendOnce(req); Protocol5Proxy.onSendOnce = null; if (__send) { return; } }
        req.send(str);
    }
    );
    return promise;
}
Protocol5Proxy.GetGameName = function(callback)
{
    var str = [];
    return Protocol5Proxy.send("5/GetGameName", str.join("&"), callback);
}
Protocol5Proxy.GetChannel = function(gameName, callback)
{
    var str = [];
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    return Protocol5Proxy.send("5/GetChannel", str.join("&"), callback);
}
Protocol5Proxy.GetAnalysisGame = function(callback)
{
    var str = [];
    return Protocol5Proxy.send("5/GetAnalysisGame", str.join("&"), callback);
}
Protocol5Proxy.UploadFile = function(file, callback)
{
    var str = [];
    throw '尚未实现该方法';
    return Protocol5Proxy.send("5/UploadFile", str.join("&"), callback);
}
Protocol5Proxy.GetAnalysisLabel = function(gameName, callback)
{
    var str = [];
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    return Protocol5Proxy.send("5/GetAnalysisLabel", str.join("&"), callback);
}
Protocol5Proxy.GetAnalysis = function(gameName, channel, label, startTime, endTime, callback)
{
    var str = [];
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(channel));
    if (label) str.push("label=" + encodeURIComponent(label));
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return Protocol5Proxy.send("5/GetAnalysis", str.join("&"), callback);
}
Protocol5Proxy.GetRetained = function(page, gameName, channel, startTime, endTime, callback)
{
    var str = [];
    if (page) str.push("page=" + page);
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(channel));
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return Protocol5Proxy.send("5/GetRetained", str.join("&"), callback);
}
Protocol5Proxy.GetRetained2 = function(page, gameName, channel, startTime, endTime, callback)
{
    var str = [];
    if (page) str.push("page=" + page);
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(channel));
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return Protocol5Proxy.send("5/GetRetained2", str.join("&"), callback);
}
Protocol5Proxy.OnlineCount = function(gameName, channel, unit, startTime, endTime, callback)
{
    var str = [];
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(channel));
    if (unit) str.push("unit=" + unit);
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return Protocol5Proxy.send("5/OnlineCount", str.join("&"), callback);
}
Protocol5Proxy.GameTime = function(gameName, channel, startTime, endTime, callback)
{
    var str = [];
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(channel));
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return Protocol5Proxy.send("5/GameTime", str.join("&"), callback);
}
Protocol5Proxy.GetGameData = function(page, gameName, channel, startTime, endTime, callback)
{
    var str = [];
    if (page) str.push("page=" + page);
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(channel));
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return Protocol5Proxy.send("5/GetGameData", str.join("&"), callback);
}
Protocol5Proxy.GetGameData2 = function(page, gameName, channel, startTime, endTime, callback)
{
    var str = [];
    if (page) str.push("page=" + page);
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(channel));
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return Protocol5Proxy.send("5/GetGameData2", str.join("&"), callback);
}
Protocol5Proxy.GetGameTaskList = function(page, gameName, callback)
{
    var str = [];
    if (page) str.push("page=" + page);
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    return Protocol5Proxy.send("5/GetGameTaskList", str.join("&"), callback);
}
Protocol5Proxy.ModifyGameTask = function(taskID, gameName, channel, taskType, condition1, condition2, rewardName, rewardImage, description, callback)
{
    var str = [];
    if (taskID) str.push("taskID=" + taskID);
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (channel) str.push("channel=" + encodeURIComponent(JSON.stringify(channel)));
    if (taskType) str.push("taskType=" + encodeURIComponent(taskType));
    if (condition1) str.push("condition1=" + encodeURIComponent(condition1));
    if (condition2) str.push("condition2=" + condition2);
    if (rewardName) str.push("rewardName=" + encodeURIComponent(rewardName));
    if (rewardImage) str.push("rewardImage=" + encodeURIComponent(rewardImage));
    if (description) str.push("description=" + encodeURIComponent(description));
    return Protocol5Proxy.send("5/ModifyGameTask", str.join("&"), callback);
}
Protocol5Proxy.DeleteGameTask = function(taskID, callback)
{
    var str = [];
    if (taskID) str.push("taskID=" + taskID);
    return Protocol5Proxy.send("5/DeleteGameTask", str.join("&"), callback);
}
Protocol5Proxy.GetCustomerService = function(gameName, callback)
{
    var str = [];
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    return Protocol5Proxy.send("5/GetCustomerService", str.join("&"), callback);
}
Protocol5Proxy.SetCustomerService = function(gameName, qrCode, description, callback)
{
    var str = [];
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (qrCode) str.push("qrCode=" + encodeURIComponent(qrCode));
    if (description) str.push("description=" + encodeURIComponent(description));
    return Protocol5Proxy.send("5/SetCustomerService", str.join("&"), callback);
}
Protocol5Proxy.GetGameTaskRecordList = function(page, gameName, type, ID, code, startTime, endTime, callback)
{
    var str = [];
    if (page) str.push("page=" + page);
    if (gameName) str.push("gameName=" + encodeURIComponent(gameName));
    if (type) str.push("type=" + encodeURIComponent(type));
    if (ID) str.push("ID=" + ID);
    if (code) str.push("code=" + code);
    if (startTime) str.push("startTime=" + encodeURIComponent(JSON.stringify(startTime)));
    if (endTime) str.push("endTime=" + encodeURIComponent(JSON.stringify(endTime)));
    return Protocol5Proxy.send("5/GetGameTaskRecordList", str.join("&"), callback);
}
Protocol5Proxy.VerifyGameTaskInfo = function(code, callback)
{
    var str = [];
    if (code) str.push("code=" + code);
    return Protocol5Proxy.send("5/VerifyGameTaskInfo", str.join("&"), callback);
}
Protocol5Proxy.VerifyGameTask = function(code, callback)
{
    var str = [];
    if (code) str.push("code=" + code);
    return Protocol5Proxy.send("5/VerifyGameTask", str.join("&"), callback);
}
Protocol5Proxy.GetAccountList = function(page, account, callback)
{
    var str = [];
    if (page) str.push("page=" + page);
    if (account) str.push("account=" + encodeURIComponent(account));
    return Protocol5Proxy.send("5/GetAccountList", str.join("&"), callback);
}
Protocol5Proxy.BindGame = function(identityID, gameNames, callback)
{
    var str = [];
    if (identityID) str.push("identityID=" + identityID);
    if (gameNames) str.push("gameNames=" + encodeURIComponent(JSON.stringify(gameNames)));
    return Protocol5Proxy.send("5/BindGame", str.join("&"), callback);
}
Protocol5Proxy.ModifyAccount = function(identityID, account, password, type, nickName, callback)
{
    var str = [];
    if (identityID) str.push("identityID=" + identityID);
    if (account) str.push("account=" + encodeURIComponent(account));
    if (password) str.push("password=" + encodeURIComponent(password));
    if (type) str.push("type=" + type);
    if (nickName) str.push("nickName=" + encodeURIComponent(nickName));
    return Protocol5Proxy.send("5/ModifyAccount", str.join("&"), callback);
}
Protocol5Proxy.DeleteAccount = function(identityID, callback)
{
    var str = [];
    if (identityID) str.push("identityID=" + identityID);
    return Protocol5Proxy.send("5/DeleteAccount", str.join("&"), callback);
}
Protocol5Proxy.ChangeAccountState = function(identityID, callback)
{
    var str = [];
    if (identityID) str.push("identityID=" + identityID);
    return Protocol5Proxy.send("5/ChangeAccountState", str.join("&"), callback);
}
Protocol5Proxy.GetUserInfo = function(callback)
{
    var str = [];
    return Protocol5Proxy.send("5/GetUserInfo", str.join("&"), callback);
}
Protocol5Proxy.ChangePassword = function(oldPassword, newPassword, callback)
{
    var str = [];
    if (oldPassword) str.push("oldPassword=" + encodeURIComponent(oldPassword));
    if (newPassword) str.push("newPassword=" + encodeURIComponent(newPassword));
    return Protocol5Proxy.send("5/ChangePassword", str.join("&"), callback);
}
