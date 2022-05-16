using System;
using System.Collections;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Cmdline;
using EntryEngine.Network;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace LauncherServer
{
    /// <summary>
    /// LauncherServer(启动器服务端，以下简称服务端)是一个命令行程序，开启了和
    /// LauncherClient(启动器客户端，以下简称客户端)的TCP通信，
    /// LauncherManager(启动器管理端，已弃用)的TCP通信，
    /// LauncherManagerWeb(启动器管理端网页版，以下简称管理端)的HTTP通信。
    /// 
    /// 服务端可以
    ///     管理服务类型(SVN的一个URL)
    ///     管理可以接入服务端的用户账号(默认账号和密码可在_C.xml中配置)
    /// 客户端启动时自动链接服务端，
    /// 运维人员通过管理端进行操作，管理端可以
    /// </summary>
    class Program
    {
        //static string TestEncryptText(string text)
        //{
        //    MD5 md5 = new MD5CryptoServiceProvider();
        //    byte[] encrypt = Encoding.UTF8.GetBytes(text);
        //    encrypt = md5.ComputeHash(encrypt);
        //    return Convert.ToBase64String(encrypt);
        //}

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            _LOG.Logger logger = new LoggerConsole();
            //logger.Colors.Remove(0);
            _LOG._Logger = new LoggerFile(logger);
            //_LOG._Logger = logger;

            _LOG.Debug("加载常量表");
            _C.Load(_IO.ReadText("_C.xml"));

            _LOG.Debug("加载配置文件");
            _SAVE.Load();

            LinkTcp.MaxBuffer = ushort.MaxValue;
            // 初始化协议代理服务
            //var managerProxy = new ServiceManager();
            //managerProxy.Initialize(_C.PortManager);

            var managerBS = new ServiceManagerBS();
            managerBS.Initialize(_C.PortManagerBS);

            var launcherProxy = new ServiceLauncher();
            launcherProxy.Initialize(_C.PortLauncher);

            EntryLinkServer server = new EntryLinkServer();
            //server.AddProxy(managerProxy);
            server.AddProxy(launcherProxy);
            server.AddProxy(managerBS);

            using (CmdlineGate main = new CmdlineGate())
            {
                main.AppendCommand(launcherProxy, typeof(ILauncherCmdline));
                main.FrameRate = TimeSpan.FromMilliseconds(200);
                main.Run(server);
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _LOG.Error("Fatal: {0}", Environment.StackTrace);
            if (EntryEngine.EntryService.Instance != null)
                EntryEngine.EntryService.Instance.Dispose();
            Environment.Exit(-1);
        }
    }
}
