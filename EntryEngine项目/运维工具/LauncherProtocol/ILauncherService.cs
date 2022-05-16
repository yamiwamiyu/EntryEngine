using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;
using EntryEngine;
using LauncherProtocolStructure;

namespace LauncherProtocol
{
    /*
     * 游戏服务器 = S
     * LauncherClient = LC
     * LauncherServer = LS
     * LauncherManager = LM
     * 
     * 日志流程
     * 1. S通过Console.WriteLine写日志
     * 2. LC截获S的日志转换成可查询的格式并持久化
     * 3. LM可以通过向LS发出请求，LS再向LC发出请求获取并查看日志
     * 
     * 服务器日志
     * 1. S通过Console.WriteLine写日志
     * 2. LC截获S的日志并发送到LS
     * 3. LS记录日志，若LM在线则像LM主动展示日志
     */
    [ProtocolStub(0, null)]
    public interface ILauncherService
    {
        void PushServices(List<Service> services);
        void RevisionUpdate(ServiceTypeRevision revision);
        void StatusUpdate(string name, EServiceStatus status, string time);
        void ServerStatusStatistic(ServerStatusData data);
        void Log(string name, Record record);
        void LogServer(string name, Record record);
    }
}
