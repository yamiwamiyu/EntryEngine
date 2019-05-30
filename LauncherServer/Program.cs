﻿using System;
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

            Logger logger = new Logger();
            //logger.Colors.Remove(0);
            _LOG._Logger = new LoggerFile(logger);

            _LOG.Debug("加载常量表");
            _C.Load(_IO.ReadText("_C.xml"));

            _LOG.Debug("加载配置文件");
            _SAVE.Load();

            LinkTcp.MaxBuffer = ushort.MaxValue;
            // 初始化协议代理服务
            var managerProxy = new ServiceManager();
            managerProxy.Initialize(_C.PortManager);

            var launcherProxy = new ServiceLauncher();
            launcherProxy.Initialize(_C.PortLauncher);

            EntryLinkServer server = new EntryLinkServer();
            server.AddProxy(managerProxy);
            server.AddProxy(launcherProxy);

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
