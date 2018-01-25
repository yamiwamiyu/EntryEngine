using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using FMOD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace EntryEngine.Xna
{
    public class GraphicsXna : GRAPHICS
    {
        private class ScreenshotData
        {
            public MATRIX2x3 View;
            public Viewport Viewport;
            public Action OnBegin;
            public bool Capturing;
            public DepthStencilBuffer Buffer;
        }

        private ScreenshotData screenshot = new ScreenshotData();
        private Matrix spriteTransformMatrix;

        public SpriteBatch XnaBatch
        {
            get;
            private set;
        }
        public GraphicsDeviceManager DeviceManager
        {
            get;
            private set;
        }
        public GraphicsDevice Device
        {
            get { return DeviceManager.GraphicsDevice; }
        }
        public override bool IsFullScreen
        {
            get
            {
                return DeviceManager.IsFullScreen;
            }
            set
            {
                if (DeviceManager.IsFullScreen != value)
                {
                    DeviceManager.IsFullScreen = value;
                    DeviceManager.ApplyChanges();
                }
            }
        }
        protected override VECTOR2 InternalScreenSize
        {
            get
            {
                return new VECTOR2(
                    DeviceManager.PreferredBackBufferWidth,
                    DeviceManager.PreferredBackBufferHeight);
            }
            set
            {
                if (value.X <= 0 || value.Y <= 0)
                    return;
                DeviceManager.PreferredBackBufferWidth = (int)value.X;
                DeviceManager.PreferredBackBufferHeight = (int)value.Y;
                DeviceManager.ApplyChanges();
            }
        }

        public GraphicsXna(GraphicsDeviceManager device)
        {
            DeviceManager = device;
            DefaultColor = COLOR.White;
            XnaBatch = new SpriteBatch(device.GraphicsDevice);
        }

        protected override void SetViewport(MATRIX2x3 view, RECT viewport)
        {
            viewport = AreaToScreen(viewport);
            Viewport v = new Viewport();
            v.MinDepth = 0;
            v.MaxDepth = 1;
            v.X = (int)viewport.X;
            v.Y = (int)viewport.Y;
            v.Width = (int)viewport.Width;
            v.Height = (int)viewport.Height;
            Device.Viewport = v;
            Device.RenderState.ScissorTestEnable = true;
        }
        protected override void InternalBegin(ref MATRIX2x3 matrix, ref RECT graphics, SHADER shader)
        {
            if (RenderTargetCount > 1)
                XnaBatch.End();

            if (screenshot.OnBegin != null)
            {
                screenshot.OnBegin();
                screenshot.OnBegin = null;
            }

            spriteTransformMatrix = matrix.GetMatrix();
            XnaBatch.Begin(SpriteBlendMode.AlphaBlend,
                    SpriteSortMode.Immediate, SaveStateMode.SaveState,
                    spriteTransformMatrix);
            XnaBatch.GraphicsDevice.RenderState.CullMode = CullMode.None;
            // Draw时设置SrouceRectangle超过Texture的宽高可以达到平铺
            SamplerStateCollection samplers = XnaBatch.GraphicsDevice.SamplerStates;
            samplers[0].AddressU = TextureAddressMode.Wrap;
            samplers[0].AddressV = TextureAddressMode.Wrap;

            if (screenshot.Capturing)
                // 截图时，RenderTarget2D在屏幕左上角，Scissor也应该相应调整到左上角
                graphics.Location = VECTOR2.Zero;
            else
                graphics = AreaToScreen(graphics);
            Device.ScissorRectangle = graphics.GetRect();
        }
        protected override void Ending(GRAPHICS.RenderState render)
        {
            if (RenderTargetCount <= 2)
                XnaBatch.End();
        }
        //public override void BeginShader(IShader shader)
        //{
        //    if (shader != null)
        //    {
        //        base.BeginShader(shader);
        //        for (int i = 1; i <= shader.PassCount; i++)
        //        {
        //            shader.SetPass(i);
        //        }
        //    }
        //}
        //public override void EndShader()
        //{
        //    IShader shader = CurrentShader;
        //    if (shader != null)
        //    {
        //        shader.SetPass(0);
        //        base.EndShader();
        //    }
        //}
        public override TEXTURE Screenshot(RECT graphics)
        {
            // Flush the last draw by SpriteSortMode.Immediate 
            Draw(TEXTURE.Pixel, new RECT(0, 0, 0, 0));

            GraphicsDevice device = XnaBatch.GraphicsDevice;

            Rectangle target = AreaToScreen(graphics).GetRect();
            //Rectangle screen = device.Viewport.TitleSafeArea;
            Rectangle screen = new Rectangle(0, 0, device.DepthStencilBuffer.Width, device.DepthStencilBuffer.Height);

            ResolveTexture2D backBuffer = new ResolveTexture2D(device, screen.Width, screen.Height, 1, device.DisplayMode.Format);
            device.ResolveBackBuffer(backBuffer);

            Texture2D screenshot = backBuffer;

            if (target != screen)
            {
                target = Rectangle.Intersect(target, screen);
                if (!target.IsEmpty)
                {
                    Texture2D area = new Texture2D(device, target.Width, target.Height, backBuffer.LevelCount, backBuffer.TextureUsage, backBuffer.Format);
                    byte[] buffer = backBuffer.GetBuffer(target.GetRect());
                    area.SetColor(buffer, new RECT(0, 0, area.Width, area.Height));

                    backBuffer.Dispose();
                    screenshot = area;
                }
            }

            return new TextureXna(screenshot);
        }
        public override void BeginScreenshot(RECT graphics)
        {
            if (screenshot.Capturing)
                throw new InvalidOperationException("Screenshot is capturing.");
            screenshot.View = View;
            screenshot.Viewport = Device.Viewport;
            screenshot.Capturing = true;

            /*
             * position: 屏幕左上角坐标对应的画布坐标；因为Scissor只支持屏幕坐标，在InternalBegin中graphics将会被转换为screen
             */
            RECT screen = AreaToScreen(graphics);
            GraphicsDevice device = XnaBatch.GraphicsDevice;
            //MATRIX2x3 transform =
            //    MATRIX2x3.CreateTranslation(-graphics.X, -graphics.Y) *
            //    // 类似OpenGL修改Viewport一样会改变矩阵
            //    MATRIX2x3.CreateScale(device.Viewport.Width / screen.Width, device.Viewport.Height / screen.Height);
            View = MATRIX2x3.CreateTranslation(-graphics.X, -graphics.Y);
            screenshot.OnBegin = () =>
            {
                RenderTarget2D target = new RenderTarget2D(
                                device, (int)graphics.Width, (int)graphics.Height, 1,
                                device.PresentationParameters.BackBufferFormat);
                if (target.Width > device.DepthStencilBuffer.Width ||
                    target.Height > device.DepthStencilBuffer.Height)
                {
                    screenshot.Buffer = device.DepthStencilBuffer;
                    device.DepthStencilBuffer = new DepthStencilBuffer(device, target.Width, target.Height, screenshot.Buffer.Format);
                }
                device.SetRenderTarget(0, target);
                device.Clear(Microsoft.Xna.Framework.Graphics.Color.TransparentBlack);
            };
            Begin(MATRIX2x3.Identity, graphics);
        }
        public override TEXTURE EndScreenshot()
        {
            GraphicsDevice device = XnaBatch.GraphicsDevice;
            RenderTarget2D target = device.GetRenderTarget(0) as RenderTarget2D;
            if (target != null)
            {
                screenshot.OnBegin = () =>
                {
                    screenshot.Capturing = false;
                    device.SetRenderTarget(0, null);
                    View = screenshot.View;
                    Device.Viewport = screenshot.Viewport;
                };
                End();

                Texture2D texture = target.GetTexture();
                target.Dispose();

                if (screenshot.Buffer != null)
                {
                    device.DepthStencilBuffer.Dispose();
                    device.DepthStencilBuffer = screenshot.Buffer;
                    screenshot.Buffer = null;
                }

                return new TextureXna(texture);
            }

            if (HasRenderTarget)
            {
                return Screenshot(CurrentGraphics);
            }
            return null;
        }
        //protected override void InternalDraw(TEXTURE texture, ref SpriteVertex vertex)
        //{
        //    XnaBatch.Draw(texture.GetTexture(),
        //        vertex.Destination.GetRect(),
        //        vertex.Source.GetRect(),
        //        vertex.Color.GetColor(),
        //        vertex.Rotation,
        //        vertex.Origin.GetVector2(),
        //        GetFlipEffect(vertex.Flip), 0);
        //}
        //private SpriteEffects GetFlipEffect(EFlip flip)
        //{
        //    if (flip == EFlip.FlipVertically)
        //        return SpriteEffects.FlipVertically;
        //    else if (flip == (EFlip.FlipHorizontally | EFlip.FlipVertically))
        //        return SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
        //    else
        //        return (SpriteEffects)flip;
        //}
        protected override void DrawPrimitivesBegin(TEXTURE texture)
        {
            if (texture == null)
                return;
            var t2d = texture.GetTexture();
            Device.Textures[0] = t2d;
            Device.SetVertexShaderConstant(1, new Vector4((float)t2d.Width, (float)t2d.Height, 0f, 0f));
            Device.SetVertexShaderConstant(2, spriteTransformMatrix);
        }
        protected override void DrawPrimitives(TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount)
        {
            Device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, offset, count, indexes, indexOffset, primitiveCount);
        }
        //protected override void OutputVertex(ref TextureVertex output)
        //{
        //    // Xna3.1的颜色是BGRA
        //    byte r = output.Color.R;
        //    byte b = output.Color.B;
        //    output.Color.R = b;
        //    output.Color.B = r;
        //}
    }
	public class TextureXna : TEXTURE
    {
        private Texture2D texture;

        public override int Width
        {
            get { return texture.Width; }
        }
		public override int Height
        {
            get { return texture.Height; }
        }
        public override bool IsDisposed
        {
            get { return texture.IsDisposed; }
        }
        public Texture2D Texture2D
        {
            get { return texture; }
        }
        public TextureXna(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            this.texture = texture;
        }
		public override COLOR[] GetData(RECT area)
		{
			if (texture == null)
				return null;
			COLOR[] buffer = texture.GetColor(area);
			if (texture.Format == SurfaceFormat.Bgr32)
			{
				unsafe
				{
					int count = buffer.Length;
					fixed (COLOR* ptr = buffer)
					{
						COLOR* next = ptr;
						for (int i = 0; i < count; i++)
						{
							next->A = 255;
							next++;
						}
					}
				}
			}
			return buffer;
		}
        public override void SetData(COLOR[] buffer, RECT area)
		{
			if (texture != null)
			{
                try
                {
                    texture.SetData(0, area.GetRect(), buffer, 0, (int)(area.Width * area.Height), SetDataOptions.None);
                }
                catch (InvalidOperationException)
                {
                    int index = 0;
                    while (true)
                    {
                        if (texture.GraphicsDevice.Textures[index] == null)
                            break;
                        if (texture.GraphicsDevice.Textures[index] == texture)
                        {
                            texture.GraphicsDevice.Textures[index] = null;
                            break;
                        }
                        index++;
                    }
                    texture.SetData(0, area.GetRect(), buffer, 0, (int)(area.Width * area.Height), SetDataOptions.None);
                }
			}
		}
		protected override void InternalDispose()
		{
			texture.Dispose();
		}
		public override void Save(string file)
		{
			if (texture != null)
				texture.Save(file, ImageFileFormat.Png);
		}
	}
    public class PipelineTextureXna : ContentPipelineBinary
    {
        public override IEnumerable<string> SuffixProcessable
        {
            get { return TEXTURE.TextureFileType.Enumerable(); }
        }
        protected override Content InternalLoad(byte[] buffer)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer))
            {
                Texture2D t2d = Texture2D.FromFile(XnaGate.Gate.GraphicsDevice, stream);
                if (t2d == null)
                {
                    throw new System.IO.InvalidDataException();
                }
                return new TextureXna(t2d);
            }
        }
    }

	public class ShaderXna : Content, SHADER
    {
        private Effect effect;
        private bool _isInBeginEndPair;

        public ShaderXna()
        {
        }
        public ShaderXna(Effect effect)
        {
            this.effect = effect;
        }

        public Effect Effect
        {
            get { return effect; }
        }
        public int PassCount
        {
            get { return effect.CurrentTechnique.Passes.Count; }
        }
        public override bool IsDisposed
        {
            get { return effect.IsDisposed; }
        }
        public void LoadFromCode(string code)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 开关Shader
        /// </summary>
        /// <param name="pass">开启：1 ~ PassCount/关闭：-pass|0</param>
        /// <returns>是否正常操作Shader</returns>
        public bool SetPass(int pass)
        {
            if (pass == 0)
            {
                if (_isInBeginEndPair)
                {
                    foreach (EffectPass e in effect.CurrentTechnique.Passes)
                        e.End();
                    effect.End();
                    _isInBeginEndPair = false;
                }
            }
            else if (pass < 0)
            {
                pass--;
                effect.CurrentTechnique.Passes[-pass].End();
            }
            else
            {
                if (!_isInBeginEndPair)
                {
                    effect.Begin();
                    _isInBeginEndPair = true;
                }
                pass--;
                effect.CurrentTechnique.Passes[pass].Begin();
            }
            return true;
        }
        public bool HasProperty(string name)
        {
            return effect.Parameters[name] != null;
        }
        public bool GetValueBoolean(string property)
        {
            return effect.Parameters[property].GetValueBoolean();
        }
        public int GetValueInt32(string property)
        {
            return effect.Parameters[property].GetValueInt32();
        }
        public MATRIX GetValueMatrix(string property)
        {
            return effect.Parameters[property].GetValueMatrix().GetMatrix();
        }
        public float GetValueSingle(string property)
        {
            return effect.Parameters[property].GetValueSingle();
        }
        public TEXTURE GetValueTexture(string property)
        {
            return new TextureXna(effect.Parameters[property].GetValueTexture2D());
        }
        public VECTOR2 GetValueVector2(string property)
        {
            return effect.Parameters[property].GetValueVector2().GetVector2();
        }
        public VECTOR3 GetValueVector3(string property)
        {
            return effect.Parameters[property].GetValueVector3().GetVector3();
        }
        public VECTOR4 GetValueVector4(string property)
        {
            return effect.Parameters[property].GetValueVector4().GetVector4();
        }
        public void SetValue(string property, bool value)
        {
            effect.Parameters[property].SetValue(value);
        }
        public void SetValue(string property, float value)
        {
            effect.Parameters[property].SetValue(value);
        }
        public void SetValue(string property, int value)
        {
            effect.Parameters[property].SetValue(value);
        }
        public void SetValue(string property, MATRIX value)
        {
            effect.Parameters[property].SetValue(value.GetMatrix());
        }
        public void SetValue(string property, TEXTURE value)
        {
            effect.Parameters[property].SetValue(value.GetTexture());
        }
        public void SetValue(string property, VECTOR2 value)
        {
            effect.Parameters[property].SetValue(value.GetVector2());
        }
        public void SetValue(string property, VECTOR3 value)
        {
            effect.Parameters[property].SetValue(value.GetVector3());
        }
        public void SetValue(string property, VECTOR4 value)
        {
            effect.Parameters[property].SetValue(value.GetVector4());
        }
		protected override void InternalDispose()
		{
			if (effect != null)
			{
				effect.Dispose();
				effect = null;
			}
		}
		public SHADER Clone()
		{
			return new ShaderXna(effect.Clone(effect.GraphicsDevice));
		}
	}
    public class PipelineShaderXna : ContentPipelineBinary
    {
        private const string SUFFIX = "effect";
        public override IEnumerable<string> SuffixProcessable
        {
            get { yield return SUFFIX; }
        }
        protected override Content InternalLoad(byte[] buffer)
        {
            return new ShaderXna(new Effect(XnaGate.Gate.GraphicsDevice, buffer, CompilerOptions.None, null));
        }
        public byte[] ReadFile(string file)
        {
            CompiledEffect effect = Effect.CompileEffectFromFile(file, null, null, CompilerOptions.None, TargetPlatform.Windows);
            if (!effect.Success)
            {
                throw new Exception(effect.ErrorsAndWarnings);
            }
            return effect.GetEffectCode();
        }
    }

#if CLIENT
    public class AudioFmod : AUDIO
    {
        internal static FMOD.System System;
        private static FMOD.ChannelGroup master;

        public override float Volume
        {
            get
            {
                float volume = 0;
                master.getVolume(ref volume);
                return volume;
            }
            set
            {
                master.setVolume(value);
            }
        }
        public override bool Mute
        {
            get
            {
                bool mute = false;
                master.getMute(ref mute);
                return mute;
            }
            set
            {
                master.setMute(value);
            }
        }

        public AudioFmod()
        {
            RESULT result;
            result = FMOD.Factory.System_Create(ref System);
            result = System.init(128, INITFLAGS.NORMAL, (IntPtr)null);
            result = System.getMasterChannelGroup(ref master);
        }

        protected override void Play(ref SoundSource source, SOUND wave)
        {
            SoundSourceFmod fmod_source;
            if (source == null)
            {
                fmod_source = new SoundSourceFmod();
                source = fmod_source;
            }
            else
                fmod_source = (SoundSourceFmod)source;
            System.playSound(CHANNELINDEX.REUSE, ((SoundFmod)wave).Sound, false, ref fmod_source.channel);
        }
        protected override void Stop(SoundSource source)
        {
            ((SoundSourceFmod)source).channel.stop();
        }
        protected override void Pause(SoundSource source)
        {
            ((SoundSourceFmod)source).channel.setPaused(true);
        }
        protected override void Resume(SoundSource source)
        {
            ((SoundSourceFmod)source).channel.setPaused(false);
        }
    }
    public class SoundFmod : SOUND
    {
        public Sound Sound { get; private set; }
        public override bool IsDisposed
        {
            get { return Sound == null; }
        }
        public SoundFmod(Sound sound)
        {
            this.Sound = sound;
        }
        protected override void InternalDispose()
        {
            if (Sound != null)
            {
                Sound.release();
                Sound = null;
            }
        }
    }
    public class SoundSourceFmod : SoundSource
    {
        internal Channel channel;
        public Channel FModChannel
        {
            get { return channel; }
        }
        protected override ESoundState State
        {
            get
            {
                bool result = false;
                channel.isPlaying(ref result);
                if (result)
                    return ESoundState.Playing;

                channel.getPaused(ref result);
                if (result)
                    return ESoundState.Paused;

                return ESoundState.Stopped;
            }
        }
        protected override float Volume
        {
            get
            {
                float volume = 0;
                channel.getVolume(ref volume);
                return volume;
            }
            set
            {
                channel.setVolume(value);
            }
        }
        protected override float Channel
        {
            get
            {
                float pan = 0;
                channel.getPan(ref pan);
                return pan;
            }
            set
            {
                channel.setPan(value);
            }
        }
        protected override void SetLoop(bool loop)
        {
            if (loop)
            {
                channel.setMode(MODE.LOOP_NORMAL);
                channel.setLoopCount(-1);
            }
            else
                channel.setLoopCount(0);
        }
    }
    public class PipelineSoundFmod : ContentPipelineBinary
    {
        public override IEnumerable<string> SuffixProcessable
        {
            get { return SOUND.FileTypes.Enumerable(); }
        }

        protected override Content InternalLoad(byte[] bytes)
        {
            Sound sound = null;
            CREATESOUNDEXINFO info = new CREATESOUNDEXINFO();
            info.cbsize = Marshal.SizeOf(info);
            info.length = (uint)bytes.Length;

            RESULT result;
            result = AudioFmod.System.createSound(bytes, MODE.HARDWARE | MODE.OPENMEMORY, ref info, ref sound);
            return new SoundFmod(sound);
        }
    }
#endif
	
	public struct MouseStateXna : IMouseState
    {
        private VECTOR2 position;
        private float scrollWheelValue;
        private bool left;
        private bool right;
        private bool middle;

		public bool Focused
		{
			get { return true; }
		}
        public float ScrollWheelValue
        {
            get { return scrollWheelValue; }
        }
        public VECTOR2 Position
        {
            get { return position; }
            set
            {
				position = value;
                Mouse.SetPosition((int)value.X, (int)value.Y);
            }
        }

        public bool IsClick(int key)
        {
            switch (key)
            {
                case 0: return left;
                case 1: return right;
                case 2: return middle;
                default:
                    throw new ArgumentException("key");
            }
        }

        public static MouseStateXna GetState()
        {
            MouseStateXna state;

            MouseState mouse = Mouse.GetState();
            state.position = new VECTOR2(mouse.X, mouse.Y);
            state.scrollWheelValue = -mouse.ScrollWheelValue * _MATH.DIVIDE_BY_1[120];
            state.left = mouse.LeftButton == ButtonState.Pressed;
            state.right = mouse.RightButton == ButtonState.Pressed;
            state.middle = mouse.MiddleButton == ButtonState.Pressed;

            return state;
        }
	}
	public struct KeyboardStateXna : IKeyboardState
    {
        private const byte MAX_KEY = 8;
        private byte key1;
        private byte key2;
        private byte key3;
        private byte key4;
        private byte key5;
        private byte key6;
        private byte key7;
        private byte key8;
        private byte count;

        public bool HasPressedAnyKey
        {
            get { return count > 0; }
        }

        public unsafe int[] GetPressedKey()
        {
            int[] keys = new int[count];
            fixed (KeyboardStateXna* ptr = &this)
                for (int i = 0; i < count; i++)
                    keys[i] = *((byte*)ptr + i);
            return keys;
        }
        public unsafe bool IsClick(int key)
        {
            fixed (KeyboardStateXna* ptr = &this)
                for (int i = 0; i < count; i++)
                    if (*((byte*)ptr + i) == key)
                        return true;
            return false;
        }

        public static unsafe KeyboardStateXna GetState()
        {
            Keys[] keys = Keyboard.GetState().GetPressedKeys();

            KeyboardStateXna state;
            KeyboardStateXna* ptr = &state;
            for (int i = 0; i < keys.Length; i++)
            {
                *((byte*)ptr + i) = (byte)keys[i];
                if (++(ptr->count) == MAX_KEY)
                    break;
            }
            return state;
        }
    }
	public class MouseXna : MOUSE
    {
        protected override IMouseState GetState()
        {
            return MouseStateXna.GetState();
        }
    }
    public class TouchStateSim : ITouchState
    {
        public MouseStateXna MouseState;
        public TouchStateSim() : this(MouseStateXna.GetState()) { }
        public TouchStateSim(MouseStateXna mouse)
        {
            this.MouseState = mouse;
        }
        public float Pressure
        {
            get { return 0; }
        }
        public VECTOR2 Position
        {
            get
            {
                return MouseState.Position;
            }
            set
            {
                MouseState.Position = value;
            }
        }
        public bool IsClick(int key)
        {
            return true;
        }
    }
	public class TouchSimXna : TOUCH
	{
        protected override int GetTouches(ITouchState[] states)
        {
            MouseStateXna state = MouseStateXna.GetState();
            if (state.IsClick(DefaultKey))
            {
                states[0] = new TouchStateSim(state);
                return 1;
            }
            return 0;
        }
    }
    public class Touch2SimXna : TOUCH
    {
        class TouchState : ITouchState
        {
            internal MouseStateXna state;
            public float Pressure
            {
                get { return 0; }
            }
            public VECTOR2 Position
            {
                get
                {
                    return state.Position;
                }
                set
                {
                    state.Position = value;
                }
            }
            public bool IsClick(int key)
            {
                return state.IsClick(key);
            }
        }
        int size = 0;
        bool release1;
        TouchState state1 = new TouchState();
        TouchState state2 = new TouchState();
        protected override int GetTouches(ITouchState[] states)
        {
            MouseStateXna state = MouseStateXna.GetState();
            // 右键取消第一个测试点
            if (state.IsClick(1))
            {
                size = 0;
            }
            if (state.IsClick(0))
            {
                if (size == 0)
                {
                    release1 = false;
                    // 测试第一点并按住
                    size = 1;
                    states[0] = state1;
                    state1.state = state;
                }
                else
                {
                    if (release1)
                    {
                        // 测试第二点
                        state2.state = state;
                        size = 2;
                        states[1] = state2;
                    }
                }
            }
            else
            {
                if (size == 1)
                {
                    release1 = true;
                }
                // 放开第二点
                if (size == 2)
                {
                    size = 1;
                }
            }
            return size;
        }
    }
	public class KeyboardXna : KEYBOARD
    {
		protected override IKeyboardState GetState()
		{
			return KeyboardStateXna.GetState();
		}
	}
	public class InputTextXna : InputText
	{
        delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("imm32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr ImmGetContext(IntPtr hWnd);
        [DllImport("imm32.dll", SetLastError = true)]
        static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        const int ImeSetContext = 0x0281;
        const int InputLanguageChange = 0x0051;
        const int WM_GETDLGCODE = 0x0087;
        const int WM_CHAR = 0x0102;
        const int DLGC_WANTCHARS = 0x0080;
        const int DLGC_WANTALLKEYS = 0x0004;
        IntPtr Handle;
        IntPtr WindowWndProc;
        WndProc InputWndProc;
        IntPtr context = IntPtr.Zero;
        List<char> builder = new List<char>();
        bool capturing = false;
        bool captureEnd = false;

        bool Is32Bit
        {
            get { return IntPtr.Size == 4; }
        }
        public override bool ImmCapturing
        {
            get { return capturing; }
        }

		public InputTextXna()
		{
            const int GWL_WNDPROC = -4;

            Handle = XnaGate.Gate.Window.Handle;
            InputWndProc = HookProc;
            WindowWndProc = (IntPtr)SetWindowLong(Handle, GWL_WNDPROC, (int)Marshal.GetFunctionPointerForDelegate(InputWndProc));
		}

		private IntPtr HookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			if (msg == InputLanguageChange)
			{
				//Don't pass this message to base class
				return IntPtr.Zero;
			}
			if (msg == ImeSetContext)
			{
				if (wParam.ToInt32() == 1)
				{
					IntPtr imeContext = ImmGetContext(Handle);
					if (context == IntPtr.Zero)
						context = imeContext;
					ImmAssociateContext(Handle, context);
				}
			}
			hWnd = CallWindowProc(WindowWndProc, hWnd, msg, wParam, lParam);
			if (!IsActive)
			{
				return hWnd;
			}
			switch (msg)
			{
				case WM_GETDLGCODE:
					if (Is32Bit)
					{
						int returnCode = hWnd.ToInt32();
						returnCode |= (DLGC_WANTALLKEYS | DLGC_WANTCHARS);
						hWnd = new IntPtr(returnCode);
					}
					else
					{
						long returnCode = hWnd.ToInt64();
						returnCode |= (DLGC_WANTALLKEYS | DLGC_WANTCHARS);
						hWnd = new IntPtr(returnCode);
					}
					break;

				case WM_CHAR:
					// ctrl + alphabet = 1 ~ 26
					char c = (char)wParam.ToInt32();
					if (c > 27)
					{
						builder.Add(c);
					}
					break;

				case 0x010D:
				case 0x010F:
					capturing = true;
					break;

				case 0x010E:
					captureEnd = true;
					break;
			}
			return hWnd;
		}
		protected override EInput InputCapture(out string text)
		{
            if (builder.Count == 0)
            {
                text = null;
            }
            else
            {
                text = new string(builder.ToArray());
            }
            if (capturing && captureEnd)
                OnStop(null);
            builder.Clear();
            return EInput.Input;
		}
        protected override void OnActive(ITypist typist)
        {
            OnStop(typist);
        }
        protected override void OnStop(ITypist typist)
        {
            capturing = false;
            captureEnd = false;
            builder.Clear();
        }
		protected override void Copy(string copy)
		{
			System.Windows.Forms.Clipboard.SetText(copy);
		}
		protected override void Paste(ref string paste)
		{
			string clipboard = System.Windows.Forms.Clipboard.GetText();
			if (!string.IsNullOrEmpty(clipboard))
				paste = clipboard;
		}
	}
	public class PlatformXna : IPlatform
    {
        public XnaGate Entry
        {
            get;
            private set;
        }
        public EPlatform Platform
        {
            get { return EPlatform.Desktop; }
        }
        public TimeSpan FrameRate
        {
            get { return Entry.TargetElapsedTime; }
            set { Entry.TargetElapsedTime = value; }
        }
        public bool IsMouseVisible
        {
            get { return Entry.IsMouseVisible; }
            set { Entry.IsMouseVisible = value; }
        }
        public bool IsActive
        {
            get { return Entry.IsActive; }
        }

        internal PlatformXna(XnaGate entry)
        {
            this.Entry = entry;
            IsMouseVisible = true;
        }
    }
	public class FontGUIP : FontDynamic
	{
        private static Brush brush = Brushes.White;

        private class CacheInfo3 : FontDynamic.CacheInfo2
        {
            public Bitmap image;
            public Graphics graphics;
            public Font font;
        }
        
        private CacheInfo3 cacheP2
        {
            get { return (CacheInfo3)base.cacheP; }
        }

        public FontGUIP(string fontName, float fontSize)
            : this(new Font(fontName, GetDynamicSize(fontSize), FontStyle.Regular, GraphicsUnit.Pixel))
        {
            this.FontSize = fontSize;
        }
		public FontGUIP(Font font) : base(GetID(font))
		{
            if (font == null)
                throw new ArgumentNullException("font");
            if (cacheP2.font == null)
                cacheP2.font = font;
            this.fontSize = font.Size;
            this.lineHeight = font.GetHeight();
		}

        protected override FontDynamic.CacheInfo2 BuildGraphicsInfo()
        {
            CacheInfo3 info = new CacheInfo3();
            info.image = new Bitmap(BUFFER_SIZE, BUFFER_SIZE);
            info.graphics = Graphics.FromImage(info.image);
            info.graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            return info;
        }
        protected override FontDynamic OnSizeChanged(float fontSize)
        {
            Font font = cacheP2.font;
            Font newFont = new Font(font.FontFamily, GetDynamicSize(fontSize), font.Style, GraphicsUnit.Pixel);
            FontDynamic result = GetCache(GetID(newFont));
            if (result == null)
            {
                result = new FontGUIP(newFont);
                result.FontSize = fontSize;
            }
            return result;
        }
		protected override TEXTURE CreateTextureBuffer()
		{
            cacheP2.graphics.Clear(System.Drawing.Color.Transparent);
			return new TextureXna(new Texture2D(XnaGate.Gate.GraphicsDevice, cacheP2.image.Width, cacheP2.image.Height));
		}
		protected override COLOR[] DrawChar(char c, ref RECT uv)
		{
            CacheInfo3 info = this.cacheP2;

			System.Drawing.Rectangle source =
				new System.Drawing.Rectangle(
					(int)uv.X,
					(int)uv.Y,
					(int)uv.Width,
					(int)uv.Height);
            info.graphics.DrawString(c.ToString(), info.font, brush, source, StringFormat.GenericTypographic);

            BitmapData data = info.image.LockBits(source, ImageLockMode.ReadOnly, info.image.PixelFormat);
			byte[] bytes = new byte[source.Width * source.Height * 4];
			int start = 0;
			int stride = source.Width * 4;
			int line = source.Height;
			for (int i = 0; i < line; i++)
			{
				System.Runtime.InteropServices.Marshal.Copy(new IntPtr(data.Scan0.ToInt32() + data.Stride * i), bytes, start, stride);
				start += stride;
			}
            info.image.UnlockBits(data);

            COLOR[] colors = new COLOR[source.Width * source.Height];
            for (int i = 0; i < colors.Length; i++)
            {
                int index = i * 4;
                colors[i] = new COLOR(bytes[index], bytes[index + 1], bytes[index + 2], bytes[index + 3]);
            }
            return colors;
		}
		protected override void InternalDispose()
		{
			base.InternalDispose();
            if (cacheP2.image != null)
			{
                cacheP2.graphics.Dispose();
                cacheP2.image.Dispose();
			}
		}
        protected override Content Cache()
        {
            FontGUIP font = new FontGUIP(cacheP2.font);
            font._Key = this._Key;
            return font;
        }

        public static int GetID(Font font)
        {
            return font.Name.GetHashCode() + ((int)(GetDynamicSize(font.Size) * 100) / 100) + (int)font.Style;
        }
	}

    public class Logger : _LOG.Logger
    {
        private const byte LOG = (byte)ELog.Debug;
        private byte last;

        public ConsoleColor[] Colors
        {
            get;
            private set;
        }

        public Logger()
        {
            Colors = new ConsoleColor[]
            {
                ConsoleColor.Gray,
                ConsoleColor.White,
                ConsoleColor.DarkYellow,
                ConsoleColor.Red,
            };
        }

        public override void Log(ref Record record)
        {
            byte level = record.Level;
            if (level > LOG)
                return;

            if (level != last)
            {
                last = level;
                Console.ForegroundColor = Colors[level];
            }
            Console.WriteLine("[{0}] {1}", record.Time.ToString("yyyy-MM-dd HH:mm:ss"), record.ToString());
        }
    }

	public class EntryXna : Entry
	{
        protected override void Initialize(out AUDIO AUDIO, out ContentManager ContentManager, out FONT FONT, out GRAPHICS GRAPHICS, out INPUT INPUT, out _IO.iO iO, out IPlatform IPlatform, out TEXTURE TEXTURE)
        {
            _LOG._Logger = new Logger();

            XnaGate xna = XnaGate.Gate;

            FONT = new FontGUIP(new Font("黑体", 24f, FontStyle.Bold, GraphicsUnit.Pixel));

            IPlatform = new PlatformXna(xna);

            INPUT = new INPUT(new MouseXna());
            INPUT.Keyboard = new KeyboardXna();
            INPUT.InputDevice = new InputTextXna();

            GRAPHICS = new GraphicsXna(xna.GraphicsManager);
            GRAPHICS.ScreenSize = new VECTOR2(960, 540);

#if CLIENT
            AUDIO = new AudioFmod();
#else
            AUDIO = null;
#endif

            TEXTURE = null;

            iO = null;

            PipelinePiece.GetDefaultPipeline();
            ContentManager = NewContentManager();
            ContentManager.IODevice = _iO;
        }
        
        protected override TEXTURE InternalNewTEXTURE(int width, int height)
		{
			Texture2D texture = new Texture2D(XnaGate.Gate.GraphicsDevice, width, height);
			return new TextureXna(texture);
		}
        protected override ContentManager InternalNewContentManager()
		{
			ContentManager content = new ContentManager(NewiO(null));
            //content.RootDirectory = "Content\\";
            content.AddPipeline(new PipelineParticle());
            content.AddPipeline(new PipelineAnimation());
            content.AddPipeline(new PipelinePiece());
            content.AddPipeline(new PipelinePatch());
			content.AddPipeline(new PipelineTextureXna());
            content.AddPipeline(new PipelineFontStatic());
#if CLIENT
            content.AddPipeline(new PipelineSoundFmod());
#endif
			return content;
		}
        protected override FONT InternalNewFONT(string name, float fontSize)
		{
            return new FontGUIP(name, fontSize);
		}
	}
}
