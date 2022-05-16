using System;
using EntryEngine.Cmdline;
using EntryEngine;

namespace LauncherClient
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            using (CmdlineGate main = new CmdlineGate())
            {
                var service = new ServiceLauncher();
                main.AppendCommand(service, typeof(ICmdline));
                main.FrameRate = TimeSpan.FromMilliseconds(200);
                main.Run(service);
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
