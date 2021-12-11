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
        Sprite st1 = new Sprite();
        SpriteText st2 = new SpriteText();
        SHADER shader;
        protected override IEnumerable<ICoroutine> Loading()
        {
            Label label = new Label();
            label.UIText.FontColor = COLOR.Black;
            label.X = 200;
            label.Y = 200;
            label.Pivot = EPivot.MiddleCenter;
            label.Text = "测试";
            label.Effect.DoMoveX(400, 0, 1).DoFadeIn(1).DoScale(5, 1, 1).DoRotate(-360, 0, 1);
            Add(label);

            //Content.Load<DRAGONBONES>("dragonbones/战士.json");

            this.Background = TEXTURE.Pixel;
            //this.Color = COLOR.Black;
            st1.Texture = Content.Load<TEXTURE>("新建图像.png");
            st1.Position.X = 200;
            st1.Position.Y = 200;
            //st2.Font = Content.Load<FONT>("等宽数值.tfont");
            st2.Font = new FontRich(st2.Font);
            st2.Text = "012<34<Color=0;255;0;255; >5\n67</><Font=等宽数值.tfont;>8</>666";
            st2.Area.X = 200;
            st2.Area.Y = 400;
            st2.Color = COLOR.White;
            st2.Font.FontSize = 72;
            st2.Alignment = EPivot.TopLeft;
            //shader = Content.Load<SHADER>("描边.shader");
            //ShaderStroke.Shader = Content.Load<SHADER>("描边.shader");
            shader = new ShaderGray()
                {
                };

            //UIScene scene = new UIScene();
            //scene.Background = TEXTURE.Pixel;
            //scene.Color = COLOR.Lime;
            //scene.Width = 100;
            //scene.Height = 100;
            //scene.DragMode = EDragMode.Move;
            //scene.DragInertia = 1;
            //scene.Rebound = 1f;
            //Entry.ShowDialogScene(scene, EState.Dialog);

            db = Content.Load<DRAGONBONES>("野人.json");
            db.Proxy.AddDBEventListener(EntryEngine.DragonBone.DBCore.EEventType.FRAME_EVENT,
                e =>
                {
                    _LOG.Debug("Type: {0}, Name: {1} Frame:{2}", e.type, e.name, GameTime.Time.FrameID);
                });
            return base.Loading();
        }
        DRAGONBONES db;
        int a = 0;
        protected override void InternalEvent(Entry e)
        {
            base.InternalEvent(e);
            if (e.INPUT.Pointer.IsRelease(0))
            {
                //STextHint.ShowHint("测试fyiwoupbujagiuew[iruwe");
                db.Proxy.Animation.Play(db.Proxy.Animation.animationNames[a], -1);
                a++;
                if (a == db.Proxy.Animation.animationNames.Count)
                    a = 0;
            }
        }
        protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
        {
            
            base.InternalDraw(spriteBatch, e);
            //shader.SetValue("View", (MATRIX)spriteBatch.GraphicsToCartesianMatrix());
            spriteBatch.Begin(new ShaderAlpha() { Alpha = 0.9f });
            st1.Draw();
            st2.Draw();
            spriteBatch.End();

            spriteBatch.Draw(db, new VECTOR2(400, 800));

            //db.Update(e.GameTime.ElapsedSecond);

            //var result = (DRAGONBONES)((EntryEngine.DragonBone.DBCore.Armature)db.Proxy.Armature.GetSlots().FirstOrDefault(s => s.name == "野人大招").displayList[0]).display;
            ////result.Update(e.GameTime.ElapsedSecond);
            ////if (result.Proxy.Animation.lastAnimationState == null)
            ////    result.Proxy.Animation.Play();
            //spriteBatch.Draw(result, new VECTOR2(400, 800));
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
