using System;
using System.Diagnostics;
using EntryEngine.Serialize;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EntryEngine.Xna
{
	using GameTime = Microsoft.Xna.Framework.GameTime;
    using System.Windows.Forms;

    public class XnaGate : Microsoft.Xna.Framework.Game
    {
        public static XnaGate Gate
        {
            get;
            private set;
        }

		struct FPSTest
		{
			public int InvokeCount;
			public TimeSpan InvokeTime;
		}

        // 帧效率测试
		FPSTest updateTime;
		FPSTest drawTime;
		TimeSpan realTime;

        GraphicsDeviceManager graphics;
        public Color BGColor = Color.TransparentBlack;
        public event Func<Entry> OnCreateEntry;
        public event Action<Entry> OnInitialize;
        public event Action<Entry> OnInitialized;
        private bool isDragFileEnable;
        private Action<string[]> dragFiles;
        private bool testFPS = true;

        public GraphicsDeviceManager GraphicsManager
        {
            get { return graphics; }
        }
        public Entry Entry
        {
            get;
            private set;
        }
        public string Title
        {
            get { return Window.Title; }
            set
            {
                Window.Title = value;
                testFPS = false;
            }
        }
        public Action<string[]> DragFiles
        {
            get { return dragFiles; }
            set
            {
                if (value != null)
                {
                    if (!isDragFileEnable)
                    {
                        var form = (Form)Form.FromHandle(XnaGate.Gate.Window.Handle);
                        form.AllowDrop = true;
                        form.DragEnter += (sender, e) =>
                        {
                            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                                e.Effect = DragDropEffects.Link;
                            else
                                e.Effect = DragDropEffects.None;
                        };
                        form.DragDrop += (sender, e) =>
                        {
                            if (dragFiles != null)
                            {
                                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                                dragFiles(files);
                            }
                        };
                        isDragFileEnable = true;
                    }
                }
                dragFiles = value;
            }
        }

        public XnaGate()
        {
            graphics = new GraphicsDeviceManager(this);
            base.Content.RootDirectory = "Content";
            Gate = this;
            //this.IsFixedTimeStep = false;
        }

        protected sealed override void Initialize()
        {
            base.Initialize();
        }
        protected sealed override void LoadContent()
        {
            if (OnCreateEntry != null)
                Entry = OnCreateEntry();
            else
                Entry = new EntryXna();
            if (OnInitialize != null)
                OnInitialize(Entry);
            Entry.Initialize();
            if (OnInitialized != null)
                OnInitialized(Entry);
        }
        protected override void UnloadContent()
        {
            //Entry.Dispose();
        }
        protected override void Update(GameTime gameTime)
        {
            if (testFPS)
            {
                Stopwatch clock = new Stopwatch();
                clock.Start();
                Entry.Update();
                clock.Stop();
                updateTime.InvokeTime = updateTime.InvokeTime.Add(clock.Elapsed);
                updateTime.InvokeCount++;

                realTime += Entry.GameTime.ElapsedRealTime;
                if (realTime.TotalSeconds >= 1)
                {
                    Window.Title = string.Format("Update: {0} / {1} = {2} Render: {3} / {4} = {5}",
                        updateTime.InvokeTime.TotalMilliseconds.ToString("0.00"), updateTime.InvokeCount, (updateTime.InvokeTime.TotalMilliseconds / updateTime.InvokeCount).ToString("0.00"),
                        drawTime.InvokeTime.TotalMilliseconds.ToString("0.00"), drawTime.InvokeCount, (drawTime.InvokeTime.TotalMilliseconds / drawTime.InvokeCount).ToString("0.00"));
                    realTime -= TimeSpan.FromSeconds(1);
                    updateTime = new FPSTest();
                    drawTime = new FPSTest();
                }
            }
            else
                Entry.Update();
            
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(BGColor);

            if (testFPS)
            {
                Stopwatch clock = new Stopwatch();
                clock.Start();

                Entry.Draw();

                clock.Stop();
                drawTime.InvokeTime = drawTime.InvokeTime.Add(clock.Elapsed);
                drawTime.InvokeCount++;
            }
            else
            {
                Entry.Draw();
            }

            base.Draw(gameTime);
        }
    }
}
