const IMBSProxy =
{
    onSend: null,
    onSendOnce: null,
    onCallback: null,
    onErrorMsg: null,
    onError: null,
    url: "http://127.0.0.1/",
    send: function(url, str, callback)
    {
        var promise = new Promise(function(resolve, reject)
        {
            var req = new XMLHttpRequest();
            req.callback = callback;
            req.onreadystatechange = async function()
            {
                if (req.readyState == 4)
                {
                    if (IMBSProxy.onCallback) IMBSProxy.onCallback(req);
                    if (req.status == 200)
                    {
                        var obj = null;
                        switch (req.responseType)
                        {
                            case "text": if (req.response) obj = JSON.parse(req.responseText); break;
                            case "blob":
                            if (req.response.type == "text/plain") await req.response.text().then((value) => obj = JSON.parse(value));
                            else obj = req.response;
                            break;
                        }
                        if (obj && obj.errCode)
                        {
                            if (IMBSProxy.onErrorMsg) { IMBSProxy.onErrorMsg(obj); }
                            else { console.log(obj); }
                            if (reject) { reject(obj); }
                        }
                        else
                        {
                            if (req.callback) { req.callback(obj); }
                            else { console.log(obj); }
                            if (resolve) { resolve(obj); }
                        }
                    }
                    else
                    {
                        if (IMBSProxy.onError) { IMBSProxy.onError(req); }
                        else { console.error(req); }
                        if (reject) { reject({ "errCode": req.status, "errMsg": req.statusText, "req": req }); }
                    }
                }
            }
            req.open("POST", IMBSProxy.url + url, true);
            req.responseType = "text";
            if (!IMBSProxy.onSendOnce) { req.setRequestHeader("Content-Type", "application/x-www-form-urlencoded;charset=utf-8"); }
            if (IMBSProxy.onSend) { IMBSProxy.onSend(req); }
            if (IMBSProxy.onSendOnce) { var __send = IMBSProxy.onSendOnce(req); IMBSProxy.onSendOnce = null; if (__send) { return; } }
            req.send(str);
        }
        );
        return promise;
    },
    // Example: IMBSProxy.download("download.txt", () => IMBSProxy.Download(param1, param2));
    download: function(filename, download)
    {
        if (!download) throw new Error("下载文件必须指定下载函数");
        IMBSProxy.onSendOnce = (req) =>
        {
            req.responseType = "blob";
            var __callback = req.callback;
            req.callback = (ret) =>
            {
                var a = window.document.createElement("a");
                a.href = URL.createObjectURL(ret);
                a.download = filename;
                a.click();
                if (__callback) __callback(ret);
            }
        }
        download();
        if (IMBSProxy.onSendOnce) throw new Error("下载文件必须发送接口下载");
    },
    Connect: function(username, password, callback)
    {
        var str = [];
        if (username) str.push("username=" + encodeURIComponent(username));
        if (password) str.push("password=" + encodeURIComponent(password));
        return IMBSProxy.send("192/Connect", str.join("&"), callback);
    },
    ModifyServiceType: function(type, callback)
    {
        var str = [];
        if (type) str.push("type=" + encodeURIComponent(JSON.stringify(type)));
        return IMBSProxy.send("192/ModifyServiceType", str.join("&"), callback);
    },
    DeleteServiceType: function(name, callback)
    {
        var str = [];
        if (name) str.push("name=" + encodeURIComponent(name));
        return IMBSProxy.send("192/DeleteServiceType", str.join("&"), callback);
    },
    GetServiceType: function(callback)
    {
        var str = [];
        return IMBSProxy.send("192/GetServiceType", str.join("&"), callback);
    },
    GetServers: function(callback)
    {
        var str = [];
        return IMBSProxy.send("192/GetServers", str.join("&"), callback);
    },
    UpdateServer: function(callback)
    {
        var str = [];
        return IMBSProxy.send("192/UpdateServer", str.join("&"), callback);
    },
    NewService: function(serverID, serviceType, name, exe, command, callback)
    {
        var str = [];
        if (serverID) str.push("serverID=" + serverID);
        if (serviceType) str.push("serviceType=" + encodeURIComponent(serviceType));
        if (name) str.push("name=" + encodeURIComponent(name));
        if (exe) str.push("exe=" + encodeURIComponent(exe));
        if (command) str.push("command=" + encodeURIComponent(command));
        return IMBSProxy.send("192/NewService", str.join("&"), callback);
    },
    SetServiceLaunchCommand: function(serviceNames, exe, command, callback)
    {
        var str = [];
        if (serviceNames) str.push("serviceNames=" + encodeURIComponent(JSON.stringify(serviceNames)));
        if (exe) str.push("exe=" + encodeURIComponent(exe));
        if (command) str.push("command=" + encodeURIComponent(command));
        return IMBSProxy.send("192/SetServiceLaunchCommand", str.join("&"), callback);
    },
    CallCommand: function(serviceNames, command, callback)
    {
        var str = [];
        if (serviceNames) str.push("serviceNames=" + encodeURIComponent(JSON.stringify(serviceNames)));
        if (command) str.push("command=" + encodeURIComponent(command));
        return IMBSProxy.send("192/CallCommand", str.join("&"), callback);
    },
    DeleteService: function(serviceNames, callback)
    {
        var str = [];
        if (serviceNames) str.push("serviceNames=" + encodeURIComponent(JSON.stringify(serviceNames)));
        return IMBSProxy.send("192/DeleteService", str.join("&"), callback);
    },
    LaunchService: function(serviceNames, callback)
    {
        var str = [];
        if (serviceNames) str.push("serviceNames=" + encodeURIComponent(JSON.stringify(serviceNames)));
        return IMBSProxy.send("192/LaunchService", str.join("&"), callback);
    },
    UpdateService: function(serviceNames, callback)
    {
        var str = [];
        if (serviceNames) str.push("serviceNames=" + encodeURIComponent(JSON.stringify(serviceNames)));
        return IMBSProxy.send("192/UpdateService", str.join("&"), callback);
    },
    StopService: function(serviceNames, callback)
    {
        var str = [];
        if (serviceNames) str.push("serviceNames=" + encodeURIComponent(JSON.stringify(serviceNames)));
        return IMBSProxy.send("192/StopService", str.join("&"), callback);
    },
    NewManager: function(manager, callback)
    {
        var str = [];
        if (manager) str.push("manager=" + encodeURIComponent(JSON.stringify(manager)));
        return IMBSProxy.send("192/NewManager", str.join("&"), callback);
    },
    DeleteManager: function(name, callback)
    {
        var str = [];
        if (name) str.push("name=" + encodeURIComponent(name));
        return IMBSProxy.send("192/DeleteManager", str.join("&"), callback);
    },
    GetManagers: function(callback)
    {
        var str = [];
        return IMBSProxy.send("192/GetManagers", str.join("&"), callback);
    },
    GetLog: function(name, start, end, pageCount, page, content, param, levels, callback)
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
        return IMBSProxy.send("192/GetLog", str.join("&"), callback);
    },
    GroupLog: function(name, start, end, content, param, levels, callback)
    {
        var str = [];
        if (name) str.push("name=" + encodeURIComponent(name));
        if (start) str.push("start=" + encodeURIComponent(JSON.stringify(start)));
        if (end) str.push("end=" + encodeURIComponent(JSON.stringify(end)));
        if (content) str.push("content=" + encodeURIComponent(content));
        if (param) str.push("param=" + encodeURIComponent(param));
        if (levels) str.push("levels=" + encodeURIComponent(JSON.stringify(levels)));
        return IMBSProxy.send("192/GroupLog", str.join("&"), callback);
    },
};
export default IMBSProxy;
