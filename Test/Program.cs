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
        DRAGONBONES db;
        protected override IEnumerable<ICoroutine> Loading()
        {
            this.Background = TEXTURE.Pixel;
            db = Content.Load<DRAGONBONES>("dragonbones/牧师.dbs");
            return base.Loading();
        }
        protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
        {
            base.InternalDraw(spriteBatch, e);
            spriteBatch.Draw(db, new VECTOR2(300, 300));
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
