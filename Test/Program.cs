using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Xna;
using EntryEngine.UI;
using EntryEngine;
using Spine;

namespace Test
{
    class TestScene : UIScene
    {
        SPINE spine;

        protected override IEnumerable<ICoroutine> Loading()
        {
            spine = Content.Load<SPINE>(@"C:\Yamiwamiyu\Project\2DActionGame\trunk\Graphics\pgy（二进制）无非必要数据\pugongying.spine");
            spine.Animation.SetAnimation(0, "2.run", true);

            return base.Loading();
        }

        protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
        {
            base.InternalDraw(spriteBatch, e);

            spine.Update(e.GameTime);
            //spriteBatch.Draw(PATCH._PATCH, new RECT(0, 0, 500, 500));
            spriteBatch.Draw(spine, new VECTOR2(800, 450));
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
