using System;
using EntryEngine.Cmdline;

namespace LauncherClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //LauncherCmdline launcher = new LauncherCmdline();
            //launcher.Name = "S1";
            //launcher.Launch(@"Test\Test.exe", null, @"Test\",
            //    "a\r\nb\r\nc");

            //Console.ReadKey();

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
            if (EntryEngine.EntryService.Instance != null)
                EntryEngine.EntryService.Instance.Dispose();
            Environment.Exit(-1);
        }
    }
}
