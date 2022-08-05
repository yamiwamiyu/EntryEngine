using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Cmdline;
using EntryEngine.Network;
using EntryEngine;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            // 修改服务器标题，可以让你方便找到服务器的控制台程序
            Console.Title = "服务器标题";
            // 初始化你的入口，入口类型有以下几个（EntryService，Entry，EntryLinkServer）
            // EntryEngine.EntryService: 通用入口，可以通过继承此入口重写InternalUpdate来自定义程序逻辑
            // EntryEngine.Entry: 客户端用入口，服务器可能用不上；若需要制作带有操作界面的服务端，或许可以使用，不过需要重新设置特殊的运行环境
            // EntryEngine.Network.EntryLinkServer: 服务通用入口，自动调用了服务的基本逻辑，只需要为其添加服务即可（推荐使用）
            EntryLinkServer entry = new EntryLinkServer();

            // 可以先设置好日志，加载好你的配置
            _LOG._Logger = new LoggerFile(new LoggerConsole());

            // 构建并设置你的服务
            Service service = _S<Service>.Value;
            service.PermitAcceptTimeout = null;
            service.PermitSameIPLinkPerSecord = null;
            service.PermitSameIPHandlePerSecord = null;

            // 注册你的服务以便让你的服务能正常运行
            // 若入口没有使用EntryLinkServer，则服务的运行需要自己调用 Proxy.Update
            entry.AddProxy(service);

            using (CmdlineGate gate = new CmdlineGate())
            {
                // 在此设置程序参数
                //gate.FrameRate = TimeSpan.FromMilliseconds(1000.0 / 60);
                //gate.Loop += ...;
                //gate.Error += ...;

                // 这里还可以侦听控制台输入命令
                // 构建和初始化服务也可以放到控制台命令里来自行控制服务的行为
                gate.AppendCommand(service, typeof(ICmd));
                // 启动程序参数作为命令行输入，方便本地调试直接输入启动命令
                //args = new string[] { "Launch 1 \"\" 1025" };
                if (args.Length > 0)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        gate.ExecuteCommand(args[i]);
                    }
                }

                // 使用你的入口启动应用程序
                gate.Run(entry);
            }
        }
    }
}
