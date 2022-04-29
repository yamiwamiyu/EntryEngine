var IMBSProxy = {};
IMBSProxy.onSend = null;
IMBSProxy.onSendOnce = null;
IMBSProxy.onCallback = null;
IMBSProxy.onErrorMsg = null;
IMBSProxy.onError = null;
IMBSProxy.url = "";
IMBSProxy.send = function(url, str, callback)
{
    var req = new XMLHttpRequest();
    req.onreadystatechange = function()
    {
        if (req.readyState == 4)
        {
            if (IMBSProxy.onCallback) IMBSProxy.onCallback(req);
            if (req.status == 200)
            {
                var obj = req.responseText ? JSON.parse(req.responseText) : null;
                if (obj && obj.errCode)
                {
                    if (IMBSProxy.onErrorMsg) { IMBSProxy.onErrorMsg(obj); }
                    else { console.log(obj); }
                }
                else
                {
                    if (callback) { callback(obj); }
                    else { console.log(obj); }
                }
            }
            else if (IMBSProxy.onError) { IMBSProxy.onError(req); }
            else { console.error(req); }
        }
    }
    req.open("POST", IMBSProxy.url + url, true);
    req.responseType = "text";
    if (!IMBSProxy.onSendOnce) { req.setRequestHeader("Content-Type", "application/x-www-form-urlencoded;charset=utf-8"); }
    if (IMBSProxy.onSend) { IMBSProxy.onSend(req); }
    if (IMBSProxy.onSendOnce) { var __send = IMBSProxy.onSendOnce(req); IMBSProxy.onSendOnce = null; if (__send) { return; } }
    req.send(str);
}
IMBSProxy.Connect = function(username, password, callback)
{
    var str = [];
    if (username) str.push("username=" + encodeURIComponent(username));
    if (password) str.push("password=" + encodeURIComponent(password));
    IMBSProxy.send("192/Connect", str.join("&"), callback);
}
IMBSProxy.ModifyServiceType = function(type, callback)
{
    var str = [];
    if (type) str.push("type=" + encodeURIComponent(JSON.stringify(type)));
    IMBSProxy.send("192/ModifyServiceType", str.join("&"), callback);
}
IMBSProxy.DeleteServiceType = function(name, callback)
{
    var str = [];
    if (name) str.push("name=" + encodeURIComponent(name));
    IMBSProxy.send("192/DeleteServiceType", str.join("&"), callback);
}
IMBSProxy.GetServiceType = function(callback)
{
    var str = [];
    IMBSProxy.send("192/GetServiceType", str.join("&"), callback);
}
IMBSProxy.GetServers = function(callback)
{
    var str = [];
    IMBSProxy.send("192/GetServers", str.join("&"), callback);
}
IMBSProxy.UpdateServer = function(callback)
{
    var str = [];
    IMBSProxy.send("192/UpdateServer", str.join("&"), callback);
}
IMBSProxy.NewService = function(serverID, serviceType, name, command, callback)
{
    var str = [];
    if (serverID) str.push("serverID=" + serverID);
    if (serviceType) str.push("serviceType=" + encodeURIComponent(serviceType));
    if (name) str.push("name=" + encodeURIComponent(name));
    if (command) str.push("command=" + encodeURIComponent(command));
    IMBSProxy.send("192/NewService", str.join("&"), callback);
}
IMBSProxy.SetServiceLaunchCommand = function(serviceNames, command, callback)
{
    var str = [];
    if (serviceNames) str.push("serviceNames=" + encodeURIComponent(JSON.stringify(serviceNames)));
    if (command) str.push("command=" + encodeURIComponent(command));
    IMBSProxy.send("192/SetServiceLaunchCommand", str.join("&"), callback);
}
IMBSProxy.CallCommand = function(serviceNames, command, callback)
{
    var str = [];
    if (serviceNames) str.push("serviceNames=" + encodeURIComponent(JSON.stringify(serviceNames)));
    if (command) str.push("command=" + encodeURIComponent(command));
    IMBSProxy.send("192/CallCommand", str.join("&"), callback);
}
IMBSProxy.DeleteService = function(serviceNames, callback)
{
    var str = [];
    if (serviceNames) str.push("serviceNames=" + encodeURIComponent(JSON.stringify(serviceNames)));
    IMBSProxy.send("192/DeleteService", str.join("&"), callback);
}
IMBSProxy.LaunchService = function(serviceNames, callback)
{
    var str = [];
    if (serviceNames) str.push("serviceNames=" + encodeURIComponent(JSON.stringify(serviceNames)));
    IMBSProxy.send("192/LaunchService", str.join("&"), callback);
}
IMBSProxy.UpdateService = function(serviceNames, callback)
{
    var str = [];
    if (serviceNames) str.push("serviceNames=" + encodeURIComponent(JSON.stringify(serviceNames)));
    IMBSProxy.send("192/UpdateService", str.join("&"), callback);
}
IMBSProxy.StopService = function(serviceNames, callback)
{
    var str = [];
    if (serviceNames) str.push("serviceNames=" + encodeURIComponent(JSON.stringify(serviceNames)));
    IMBSProxy.send("192/StopService", str.join("&"), callback);
}
IMBSProxy.NewManager = function(manager, callback)
{
    var str = [];
    if (manager) str.push("manager=" + encodeURIComponent(JSON.stringify(manager)));
    IMBSProxy.send("192/NewManager", str.join("&"), callback);
}
IMBSProxy.DeleteManager = function(name, callback)
{
    var str = [];
    if (name) str.push("name=" + encodeURIComponent(name));
    IMBSProxy.send("192/DeleteManager", str.join("&"), callback);
}
IMBSProxy.GetManagers = function(callback)
{  
    var str = [];
    IMBSProxy.send("192/GetManagers", str.join("&"), callback);
}
IMBSProxy.GetLog = function(name, start, end, pageCount, page, content, param, levels, callback)
{
    var str = [];
    if (name) str.push("name=" + encodeURIComponent(name));
    if (start) str.push("start=" + encodeURIComponent(JSON.stringify(start)));
    if (end) str.push("end=" + encodeURIComponent(JSON.stringify(end)));
    if (pageCount) str.push("pageCount=" + pageCount);
    if (page) str.push("page=" + page);
    if (content) str.push("content=" + encodeURIComponent(content));
    if (param) str.push("param=" + encodeURIComponent(param));
    if (levels) str.push("levels=" + encodeURIComponent(JSON.stringify(levels)));
    IMBSProxy.send("192/GetLog", str.join("&"), callback);
}
IMBSProxy.GroupLog = function(name, start, end, content, param, levels, callback)
{
    var str = [];
    if (name) str.push("name=" + encodeURIComponent(name));
    if (start) str.push("start=" + encodeURIComponent(JSON.stringify(start)));
    if (end) str.push("end=" + encodeURIComponent(JSON.stringify(end)));
    if (content) str.push("content=" + encodeURIComponent(content));
    if (param) str.push("param=" + encodeURIComponent(param));
    if (levels) str.push("levels=" + encodeURIComponent(JSON.stringify(levels)));
    IMBSProxy.send("192/GroupLog", str.join("&"), callback);
}
export default IMBSProxy;
