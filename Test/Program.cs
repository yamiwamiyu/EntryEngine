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
using EntryEngine.DragonBone;

namespace Test
{
    class TestScene : UIScene
    {
        Sprite sprite = new Sprite();

        SHADER shader;
        protected override IEnumerable<ICoroutine> Loading()
        {
            sprite.Texture = Content.Load<TEXTURE>("Test.ps");
            sprite.Position.X = 500;
            sprite.Position.Y = 300;

            shader = Content.Load<SHADER>("Test.shader");
            //ShaderStroke.Shader = Content.Load<SHADER>("描边.shader");
            
            return base.Loading();
        }
        protected override void InternalEvent(Entry e)
        {
            base.InternalEvent(e);
        }
        protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
        {
            base.InternalDraw(spriteBatch, e);
            //shader.SetValue("View", (MATRIX)spriteBatch.GraphicsToCartesianMatrix());
            spriteBatch.Begin(shader);

            sprite.Draw();

            spriteBatch.End();
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
