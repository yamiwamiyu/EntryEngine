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
using EntryEngine.DragonBones;

namespace Test
{
    class TestScene : UIScene
    {
        SpriteText st1 = new SpriteText();
        SpriteText st2 = new SpriteText();
        protected override IEnumerable<ICoroutine> Loading()
        {
            this.Background = TEXTURE.Pixel;
            st1.Font = Content.Load<FONT>("数值.tfont");
            st1.Text = "0123456789+-";
            st1.Area.X = 200;
            st1.Area.Y = 200;
            st1.Alignment = EPivot.TopLeft;
            st2.Font = Content.Load<FONT>("等宽数值.tfont");
            st2.Text = "0123456789+-";
            st2.Area.X = 200;
            st2.Area.Y = 400;
            st2.Alignment = EPivot.TopLeft;
            return base.Loading();
        }
        protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
        {
            base.InternalDraw(spriteBatch, e);
            st1.Draw();
            st2.Draw();

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (XnaGate gate = new XnaGate())
            {
                gate.OnInitialized += (e) =>
                {
                    e.OnNewContentManager += (cm) =>
                    {
                        cm.AddFirstPipeline(new PipelineSpine());
                        cm.AddFirstPipeline(new PipelineDragonBones());
                    };

                    e.GRAPHICS.ScreenSize = new VECTOR2(1600, 900);
                    e.GRAPHICS.GraphicsSize = new VECTOR2(900, 1600);
                    e.ShowMainScene<TestScene>();
                };
                gate.Run();
            }
        }
    }
}
