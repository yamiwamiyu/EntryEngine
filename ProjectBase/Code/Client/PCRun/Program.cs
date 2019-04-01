using System;
using System.Threading;
using EntryEngine;
using EntryEngine.Xna;

namespace PCRun
{
    class Program
    {
#if HTML5
        [AInvariant]
        static void Main(string[] args)
        {
            // 可修改PATH更改项目资源的根目录
            EntryEngine.HTML5.HTML5Gate.PATH = "http://127.0.0.1:88/Content/";
            new EntryEngine.HTML5.HTML5Gate().Run(gate_OnInitialized);
        }
#else
        [STAThread]
        static void Main(string[] args)
        {
            // 可修改DEBUG自定义需要使用STA线程的程序
#if DEBUG
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                var wait = new AutoResetEvent(false);
                Thread sta = new Thread((obj) =>
                    {
                        try
                        {
                            Run();
                        }
                        finally
                        {
                            wait.Set();
                        }
                    });
                sta.SetApartmentState(ApartmentState.STA);
                sta.IsBackground = true;
                sta.Start();
                wait.WaitOne();
            }
            else
            {
                Run();
            }
#else
            Run();
#endif
        }
        static void Run()
        {
            using (XnaGate gate = new XnaGate())
            {
                gate.OnInitialized += new Action<EntryEngine.Entry>(gate_OnInitialized);
                try
                {
                    gate.Run();
                }
                catch (Exception ex)
                {
                    #if DEBUG
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.ReadKey();
                    #endif
                }
            }
        }
#endif
        static void gate_OnInitialized(Entry entry)
        {
            // 在此设置测试环境参数
            //entry.GRAPHICS.ScreenSize = new VECTOR2(960, 540);
            //entry.GRAPHICS.GraphicsSize = new VECTOR2(1280, 720);
            //entry.GRAPHICS.ViewportMode = EViewport.Strength;
#if HTML5
#else
            //XnaGate.Gate.Window.AllowUserResizing = true;
            //XnaGate.Gate.Window.ClientSizeChanged += (sender, e) =>
            //{
            //    var rect = XnaGate.Gate.Window.ClientBounds;
            //    entry.GRAPHICS.ScreenSize = new VECTOR2(rect.Width, rect.Height);
            //};
            // 注意：这里不适合更换环境设备，因为此程序不跨平台
            // 若需要更换的环境设备支持跨平台，应在程序显示的第一个菜单里更换
            Environment.CurrentDirectory = "Content\\";
#endif
            // entry.OnNewContentManager += ...;

            // 在此启动程序的第一个菜单
            // entry.ShowMainScene()
            entry.ShowMainScene<MAIN>();
        }
    }
}
