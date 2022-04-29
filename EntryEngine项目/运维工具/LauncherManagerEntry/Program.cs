using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Xna;
using EntryEngine;
using LauncherManager;
using System.Threading;
using EntryEngine.UI;

namespace LauncherManagerEntry
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                var wait = new AutoResetEvent(false);
                new Thread((obj) =>
                {
                    try
                    {
                        Run();
                    }
                    finally
                    {
                        wait.Set();
                    }
                })
                {
                    ApartmentState = ApartmentState.STA,
                    IsBackground = true,
                }.Start();
                wait.WaitOne();
            }
            else
            {
                Run();
            }
        }

        static void Run()
        {
            using (XnaGate gate = new XnaGate())
            {
                gate.BGColor = Microsoft.Xna.Framework.Graphics.Color.White;
                gate.OnInitialized += new Action<Entry>(gate_OnInitialized);
                gate.Run();
            }
        }
        static void gate_OnInitialized(Entry obj)
        {
            Environment.CurrentDirectory = "Content\\";
            obj.GRAPHICS.ScreenSize = new VECTOR2(1280, 720);
            obj.GRAPHICS.GraphicsSize = obj.GRAPHICS.ScreenSize;
            FONT.Default = obj.NewFONT("黑体", 16);
            obj.ShowMainScene(new SEntryPoint());
            //obj.ShowMainScene<TestScene>();
        }
    }
}
