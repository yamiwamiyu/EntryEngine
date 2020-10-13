using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;
using EntryEngine;
using EntryEngine.Serialize;
using System.Net;
using LauncherProtocolStructure;
using EntryEngine.Cmdline;

namespace LauncherServer
{
    interface ILauncherCmdline
    {
        void LauncherCall(string methodNameAndParam);
    }
    partial class ServiceLauncher :
        ProxyTcp
        //ProxyHttpAsync, _ILauncherService
        , ILauncherCmdline
    {
        public ServiceLauncher()
        {
            this.Heartbeat = TimeSpan.FromMinutes(-35);
            this.ClientDisconnect += new Action<EntryEngine.Network.Link>(ServiceLauncher_ClientDisconnect);
        }

        protected override IEnumerator<LoginResult> Login(Link link)
        {
            LoginResult result = new LoginResult();

            while (true)
            {
                byte[] data = link.Read();
                if (data == null)
                {
                    yield return result;
                    continue;
                }

                ByteReader reader = new ByteReader(data);
                string ip, sign, name;
                reader.Read(out ip);
                reader.Read(out sign);
                reader.Read(out name);

                string remote = link.EndPoint.Address.ToString();
                //if (ip != remote
                //    && !(remote.StartsWith("192.168") && ip == "127.0.0.1")
                //    && !(remote == "127.0.0.1" && ip.StartsWith("192.168")))
                //{
                //    _LOG.Error("异常的终端:{0}尝试连接管理器 IP:{1}", link.EndPoint, ip);
                //    result.Result = EAcceptPermit.Block;
                //    break;
                //}

                if (_NETWORK.ValidMD5toBase64(ip + _C.PublicKey) != sign)
                {
                    _LOG.Error("IP:{0}连接管理器PublicKey:{1}异常", link.EndPoint, sign);
                    result.Result = EAcceptPermit.Block;
                    break;
                }

                SERVER server = SERVER.CreateServer(link, link.EndPoint.ToString(), ServiceManager.Instance);
                server.ServerData.NickName = name;

                ByteWriter writer = new ByteWriter();
                // 写入分配给服务器的唯一ID
                writer.Write(server.ID);
                link.Write(writer.GetBuffer());
                link.Flush();

                AgentProtocolStub agent = new AgentProtocolStub(link, 
                    new ILauncherServiceStub(() => server), server.Proxy);
                result.Agent = agent;
                result.Result = EAcceptPermit.Permit;
            }

            yield return result;
        }
        protected override void OnUpdate(EntryEngine.GameTime time)
        {
        }
        void ServiceLauncher_ClientDisconnect(Link link)
        {
            var server = SERVER.Servers.FirstOrDefault(s => s.Link != null && s.Link.EndPoint != null && s.Link.EndPoint.Equals(link.EndPoint));
            if (server != null)
                server.Dispose();
        }

        void ILauncherCmdline.LauncherCall(string methodNameAndParam)
        {
            string command;
            string[] args;
            _CMDLINE.ParseCommandLine(methodNameAndParam, out command, out args);

            var method = typeof(IManagerCallProxy).GetMethod(command);
            if (method == null)
            {
                _LOG.Warning("Don't have method named '{0}' in IManagerCallProxy.", command);
                return;
            }

            var parameters = method.GetParameters();
            if (parameters.Length != args.Length)
            {
                _LOG.Warning("Command[{0}] parameter count dismatch.", command);
                return;
            }

            int i = 0;
            var result = args.Select(c => Convert.ChangeType(c, parameters[i++].ParameterType)).ToArray();
            foreach (var server in SERVER.Servers)
                method.Invoke(server.Proxy, result);
        }
    }
}
