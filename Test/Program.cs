using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Xna;
using EntryEngine.UI;
using EntryEngine;
using Spine;
using EntryEngine.Network;
using System.IO;
using EntryEngine.Serialize;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Test
{
    class TestScene : UIScene
    {
        protected override IEnumerable<ICoroutine> Loading()
        {
            return base.Loading();
        }
    }

    class Program
    {
        public int A { get; set; }
        public int B
        {
            get { return 0; }
            set
            {
            }
        }
        static void Main(string[] args)
        {
            var pp = typeof(Program).GetProperties();

            SerializeSetting.DefaultSetting = new SerializeSetting()
                {
                    Filter = new SerializeValidatorReadonly(),
                    Property = true,
                };
            string text = JsonWriter.Serialize(new
                {
                    key1 = "value1",
                    key2 = 5,
                    key3 = new VECTOR2(),
                });
            Console.WriteLine(text);
            Console.ReadKey();

            using (XnaGate gate = new XnaGate())
            {
                gate.OnInitialized += (e) =>
                {
                    e.OnNewContentManager += (cm) =>
                    {
                        cm.AddFirstPipeline(new PipelineSpine());
                    };

                    e.GRAPHICS.ScreenSize = new VECTOR2(1600, 900);
                    e.GRAPHICS.GraphicsSize = e.GRAPHICS.ScreenSize;
                    e.ShowMainScene<TestScene>();
                };
                gate.Run();
            }
        }
    }
}
