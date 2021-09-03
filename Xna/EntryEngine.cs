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
using System.IO;
using System.Windows.Forms;
using System.Text;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;

namespace EntryEngine.Xna
{
    /// <summary>矩阵平移有小数时，渲染的图像会有一条线</summary>
    [Code(ECode.BUG)]
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

        protected override void SetViewport(ref MATRIX2x3 view, ref RECT graphicsViewport)
        {
            var screenViewport = AreaToScreen(graphicsViewport);
            Viewport v = new Viewport();
            v.MinDepth = 0;
            v.MaxDepth = 1;
            v.X = (int)screenViewport.X;
            v.Y = (int)screenViewport.Y;
            v.Width = (int)screenViewport.Width;
            v.Height = (int)screenViewport.Height;
            Device.Viewport = v;
            //Device.RenderState.ScissorTestEnable = true;

            // 设置视口后不需要再偏移
            view.M31 = 0;
            view.M32 = 0;
            graphicsViewport.X = 0;
            graphicsViewport.Y = 0;
        }
        protected override void InternalBegin(bool threeD, ref MATRIX matrix, ref RECT graphics, SHADER shader)
        {
            if (screenshot.OnBegin != null)
            {
                screenshot.OnBegin();
                screenshot.OnBegin = null;
            }

            spriteTransformMatrix = matrix.GetMatrix();

            if (shader != null)
            {
                shader.Begin(this);
            }
            else
            {
                XnaBatch.Begin(SpriteBlendMode.AlphaBlend,
                        SpriteSortMode.Immediate, SaveStateMode.None,
                        spriteTransformMatrix);
                Device.SetVertexShaderConstant(2, spriteTransformMatrix);
            }

            var renderState = XnaBatch.GraphicsDevice.RenderState;

            //renderState.CullMode = CullMode.CullCounterClockwiseFace;
            renderState.CullMode = CullMode.None;
            //renderState.DepthBufferEnable = true;
            
            // Draw时设置SrouceRectangle超过Texture的宽高可以达到平铺
            // 设置平铺后，有半像素绘制时上面和左边有时会多出一个像素很难看
            //SamplerStateCollection samplers = XnaBatch.GraphicsDevice.SamplerStates;
            //samplers[0].AddressU = TextureAddressMode.Wrap;
            //samplers[0].AddressV = TextureAddressMode.Wrap;

            if (screenshot.Capturing)
                // 截图时，RenderTarget2D在屏幕左上角，Scissor也应该相应调整到左上角
                graphics.Location = VECTOR2.Zero;
            else
                graphics = AreaToScreen(graphics);
            // 窗体缩小后，这个值会变为false
            Device.RenderState.ScissorTestEnable = true;
            Device.ScissorRectangle = graphics.GetRect();
        }
        protected override void Ending(GRAPHICS.RenderState render)
        {
            if (render.Shader != null)
                render.Shader.End(this);
            else
                XnaBatch.End();
        }
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
        protected override void InternalDrawPrimitivesBegin(TEXTURE texture, EPrimitiveType ptype, int textureIndex)
        {
            if (texture == null)
            {
                Device.Textures[textureIndex] = null;
                return;
            }
            var t2d = texture.GetTexture();
            if (CurrentRenderState.Shader == null)
            {
                // UV设置0~1
                if (UVNormalize)
                    Device.SetVertexShaderConstant(1, new Vector4(1, 1, 0f, 0f));
                else
                    Device.SetVertexShaderConstant(1, new Vector4(texture.Width, texture.Height, 0f, 0f));
            }
            Device.Textures[textureIndex] = t2d;
        }
        protected override void InternalDrawPrimitives(EPrimitiveType ptype, TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount)
        {
            PrimitiveType resultType = ptype == EPrimitiveType.Point ? PrimitiveType.PointList :
                (ptype == EPrimitiveType.Line) ? PrimitiveType.LineList : PrimitiveType.TriangleList;
            if (primitiveCount <= 0)
                primitiveCount = GetPrimitiveCount(ptype, count);
            Device.DrawUserIndexedPrimitives(resultType, vertices, offset, count, indexes, indexOffset, primitiveCount);
        }
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
		public override COLOR[] GetData(int x, int y, int width, int height)
		{
			if (texture == null)
				return null;
			COLOR[] buffer = texture.GetColor(new RECT(x, y, width, height));
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
        public override void SetData(COLOR[] buffer, int x, int y, int width, int height)
		{
			if (texture != null)
			{
                try
                {
                    texture.SetData(0, new Rectangle(x, y, width, height), buffer, 0, width * height, SetDataOptions.None);
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
                    texture.SetData(0, new Rectangle(x, y, width, height), buffer, 0, width * height, SetDataOptions.None);
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
        public override Content LoadFromBytes(byte[] buffer)
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

	public class ShaderXna : SHADER
    {
        private Effect effect;

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
        public override int PassCount
        {
            get { return effect.CurrentTechnique.Passes.Count; }
        }
        public override bool IsDisposed
        {
            get { return effect.IsDisposed; }
        }
        public override bool HasProperty(string name)
        {
            return effect.Parameters[name] != null;
        }

		protected override void InternalDispose()
		{
			if (effect != null)
			{
				effect.Dispose();
				effect = null;
			}
		}
        protected override void InternalBegin(GRAPHICS g)
        {
            effect.Begin();
            effect.CurrentTechnique.Passes[CurrentPass].Begin();
        }
        protected override void InternalEnd(GRAPHICS g)
        {
            effect.CurrentTechnique.Passes[CurrentPass].End();
            effect.End();
        }

        public override void SetValue(string property, bool value)
        {
            effect.Parameters[property].SetValue(value);
        }
        public override void SetValue(string property, bool[] value)
        {
            effect.Parameters[property].SetValue(value);
        }
        public override void SetValue(string property, float value)
        {
            effect.Parameters[property].SetValue(value);
        }
        public override void SetValue(string property, float[] value)
        {
            effect.Parameters[property].SetValue(value);
        }
        public override void SetValue(string property, int value)
        {
            effect.Parameters[property].SetValue(value);
        }
        public override void SetValue(string property, int[] value)
        {
            effect.Parameters[property].SetValue(value);
        }
        public override void SetValue(string property, MATRIX value)
        {
            effect.Parameters[property].SetValue(value.GetMatrix());
        }
        public override void SetValue(string property, MATRIX[] value)
        {
            Matrix[] result = new Matrix[value.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = value[i].GetMatrix();
            effect.Parameters[property].SetValue(result);
        }
        public override void SetValue(string property, TEXTURE value)
        {
            effect.Parameters[property].SetValue(value.GetTexture());
        }
        public override void SetValue(string property, VECTOR2 value)
        {
            effect.Parameters[property].SetValue(value.GetVector2());
        }
        public override void SetValue(string property, VECTOR2[] value)
        {
            Vector2[] result = new Vector2[value.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = value[i].GetVector2();
            effect.Parameters[property].SetValue(result);
        }
        public override void SetValue(string property, VECTOR3 value)
        {
            effect.Parameters[property].SetValue(value.GetVector3());
        }
        public override void SetValue(string property, VECTOR3[] value)
        {
            Vector3[] result = new Vector3[value.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = value[i].GetVector3();
            effect.Parameters[property].SetValue(result);
        }
        public override void SetValue(string property, VECTOR4 value)
        {
            effect.Parameters[property].SetValue(value.GetVector4());
        }
        public override void SetValue(string property, VECTOR4[] value)
        {
            Vector4[] result = new Vector4[value.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = value[i].GetVector4();
            effect.Parameters[property].SetValue(result);
        }
    }
    public class PipelineShaderXna : PipelineShader
    {
        public override Content LoadFromText(string text)
        {
            var effect = Effect.CompileEffectFromSource(text, new CompilerMacro[0], null, CompilerOptions.None, TargetPlatform.Windows);
            if (!effect.Success)
            {
                throw new Exception(effect.ErrorsAndWarnings);
            }
            return new ShaderXna(new Effect(XnaGate.Gate.GraphicsDevice, effect.GetEffectCode(), CompilerOptions.None, null));
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
            if (result != RESULT.OK) throw new Exception(result.ToString());
            result = System.init(128, INITFLAGS.NORMAL, (IntPtr)null);
            if (result != RESULT.OK) throw new Exception(result.ToString());
            result = System.getMasterChannelGroup(ref master);
            if (result != RESULT.OK) throw new Exception(result.ToString());
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

        public override Content LoadFromBytes(byte[] bytes)
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
        internal static KeyboardStateXna previous;
        private static byte[] sorted = new byte[MAX_KEY];
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

            // 前一帧按下的按键按顺序先加入
            fixed (KeyboardStateXna* __ptr = &previous)
            {
                KeyboardStateXna* __ptr__ = __ptr;
                for (int i = 0; i < previous.count; i++, __ptr__++)
                    for (int j = 0; j < keys.Length; j++)
                        if ((byte)keys[j] == *((byte*)__ptr__))
                        {
                            *((byte*)ptr + ptr->count++) = (byte)keys[j];
                            keys[j] = Keys.None;
                            break;
                        }
            }
            // 当前帧新按下的按键后加入
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] != Keys.None)
                {
                    *((byte*)ptr + ptr->count++) = (byte)keys[i];
                    if (ptr->count == MAX_KEY)
                        break;
                }
            }

            previous = state;

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
		public override void Copy(string copy)
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

        private enum EMonospaced
        {
            None,
            Monospaced,
            Proportional,
        }
        private class CacheInfo3 : FontDynamic.CacheInfo2
        {
            public Bitmap image;
            public Graphics graphics;
            public Font font;
            public EMonospaced IsMonospaced;
            //public float 
        }
        
        private CacheInfo3 cacheP2
        {
            get { return (CacheInfo3)base.cacheP; }
        }
        public Font Font { get { return cacheP2.font; } }

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

        protected override float CalcBufferWidth(char c)
        {
            // 检验字体是等宽字体还是比例字体
            var cache = this.cacheP2;
            if (cache.IsMonospaced == EMonospaced.None)
            {
                var measure0 = cache.graphics.MeasureString("w", cache.font, new SizeF(0, 0), StringFormat.GenericTypographic);
                var measure1 = cache.graphics.MeasureString("i", cache.font, new SizeF(0, 0), StringFormat.GenericTypographic);
                if (measure0.Width == measure1.Width)
                    cache.IsMonospaced = EMonospaced.Monospaced;
                else
                    cache.IsMonospaced = EMonospaced.Proportional;
            }
            if (cache.IsMonospaced == EMonospaced.Monospaced)
            {
                return base.CalcBufferWidth(c);
            }
            else
            {
                if (char.IsWhiteSpace(c))
                    return cache.graphics.MeasureString(c.ToString(), cache.font, new SizeF(0, 0), StringFormat.GenericDefault).Width;
                else
                    return cache.graphics.MeasureString(c.ToString(), cache.font, new SizeF(0, 0), StringFormat.GenericTypographic).Width;
            }
        }
        protected override FontDynamic.CacheInfo2 BuildGraphicsInfo()
        {
            CacheInfo3 info = new CacheInfo3();
            info.image = new Bitmap(BUFFER_SIZE, BUFFER_SIZE);
            //info.image.SetResolution(320, 320);
            info.graphics = Graphics.FromImage(info.image);
            //info.graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            //info.graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            info.graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            info.graphics.PageUnit = GraphicsUnit.Pixel;
            //info.graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            //info.graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            info.graphics.TextContrast = 0;
            info.graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            return info;
        }
        protected override FontDynamic OnSizeChanged(float fontSize)
        {
            Font font = cacheP2.font;
            Font newFont = new Font(font.FontFamily, GetDynamicSize(fontSize), font.Style, font.Unit, font.GdiCharSet, font.GdiVerticalFont);
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
        protected override void DrawChar(AsyncDrawDynamicChar async, char c, Buffer buffer)
		{
            CacheInfo3 info = this.cacheP2;

            System.Drawing.Rectangle source =
                new System.Drawing.Rectangle(
                    buffer.X,
                    buffer.Y,
                    buffer.W,
                    buffer.H);
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

            async.SetData(colors);
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
        public override Content Cache()
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
    public class IOWinform : _IO.iO
    {
        private static OpenFileDialog openFile = new OpenFileDialog();
        private static SaveFileDialog saveFile = new SaveFileDialog();
        private static string GetFilters(string[] suffix)
        {
            string filter = GetFilter(suffix);      // 全部
            if (suffix != null && suffix.Length > 1)
            {
                filter += "|\r\n" + _GetFilters(suffix);       // 各类型
            }
            return filter;
        }
        private static string _GetFilters(string[] suffix)
        {
            if (suffix == null || suffix.Length == 0)
                return "(*.*)| *.*";

            StringBuilder filter = new StringBuilder();
            for (int i = 0; i < suffix.Length; i++)
            {
                filter.AppendLine(string.Format("(*.{0})|*.{0}|", suffix[i]));
            }
            filter.Remove(filter.Length - 4, 4);        // 移除最后一个 "空格" "|" 和 "\r\n"
            return filter.ToString();
        }
        private static string GetFilter(string[] suffix)
        {
            if (suffix == null || suffix.Length == 0)
                return "(*.*)|*.*";
            else
                for (int i = 0; i < suffix.Length; i++)
                    if (string.IsNullOrEmpty(suffix[i]))
                        suffix[i] = "*";

            StringBuilder filter = new StringBuilder();
            filter.Append("(");
            for (int i = 0; i < suffix.Length; i++)
            {
                filter.Append(string.Format("*.{0};", suffix[i]));
            }
            filter.Remove(filter.Length - 1, 1);    // 移除最后一个";"
            filter.Append(")|");                 // 添加分割线
            for (int i = 0; i < suffix.Length; i++)
            {
                filter.Append(string.Format("*.{0};", suffix[i]));
            }
            filter.Remove(filter.Length - 1, 1);    // 移除最后一个";"
            return filter.ToString();
        }
        public override void FileBrowser(string[] suffix, bool multiple, Action<SelectFile[]> onSelect)
        {
            openFile.Multiselect = multiple;
            openFile.Filter = GetFilter(suffix);
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                if (onSelect != null)
                {
                    string[] files = openFile.FileNames;
                    int len = files.Length;
                    SelectFile[] result = new SelectFile[len];
                    for (int i = 0; i < len; i++)
                        result[i] = CreateSelectFile(files[i], null);
                    onSelect(result);
                }
            }
        }
        public override void FileBrowserSave(string file, string[] suffix, Action<SelectFile> onSelect)
        {
            saveFile.Filter = GetFilter(suffix);
            saveFile.FileName = file;
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                if (onSelect != null)
                {
                    onSelect(CreateSelectFile(saveFile.FileName, null));
                }
                //else
                //{
                //    // 默认读取文件保存到目标位置
                //    if (!string.IsNullOrEmpty(file) && file != saveFile.FileName)
                //    {
                //        byte[] read = ReadByte(file);
                //        WriteByte(saveFile.FileName, read);
                //    }
                //}
            }
        }
    }
    
	public class EntryXna : Entry
	{
        protected override void Initialize(out AUDIO AUDIO, out ContentManager ContentManager, out FONT FONT, out GRAPHICS GRAPHICS, out INPUT INPUT, out _IO.iO iO, out IPlatform IPlatform, out TEXTURE TEXTURE)
        {
            _LOG._Logger = new LoggerConsole();
            //_LOG._Logger = new LoggerFile();

            XnaGate xna = XnaGate.Gate;

            FONT = new FontGUIP("黑体", 24f);

            IPlatform = new PlatformXna(xna);

            INPUT = new INPUT(new MouseXna());
            INPUT.Keyboard = new KeyboardXna();
            INPUT.InputDevice = new InputTextXna();

            GRAPHICS = new GraphicsXna(xna.GraphicsManager);
            GRAPHICS.ScreenSize = new VECTOR2(960, 540);

#if CLIENT
            try
            {
                AUDIO = new AudioFmod();
            }
            catch (Exception ex)
            {
                _LOG.Error(ex, "初始化声音设备异常");
                AUDIO = new AudioEmpty();
            }
#else
            AUDIO = null;
#endif

            TEXTURE = null;

            iO = new IOWinform();

            PipelinePiece.GetDefaultPipeline();
            ContentManager = NewContentManager();
            ContentManager.IODevice = _iO;

            var preShader = new PipelineShaderXna();
//            ShaderStroke.Shader = (SHADER)preShader.LoadFromText(
//@"");
            ShaderLightening.Shader = (SHADER)preShader.LoadFromText(
@"uniform float4x4 View;
struct VS_OUTPUT
{
    float4 Position   : POSITION; 
    float4 Color      : COLOR0;
    float2 UV		  : TEXCOORD0;
};
VS_OUTPUT vs
	(
		float3 Position : POSITION,
		float4 Color : COLOR0,
		float2 Coord : TEXCOORD0
	)
{
	VS_OUTPUT output;
	output.Position = mul(float4(Position, 1), View);
	output.Color = Color;
	output.UV = Coord;
	return output;
};
uniform sampler Texture;
uniform float3 Lightening = float3(0, 0, 0);
float4 ps(float4 Color : COLOR0, float2 UV : TEXCOORD0) : COLOR
{ 
	return float4(Lightening, 0) + Color * tex2D(Texture, UV);
};
technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_1_1 vs();
		PixelShader = compile ps_1_1 ps();
	}
}");
            ShaderStroke.Shader = (SHADER)preShader.LoadFromText(
@"uniform float4x4 View;
struct VS_OUTPUT
{
    float4 Position   : POSITION; 
    float4 Color      : COLOR0;
    float2 UV		  : TEXCOORD0;
};
VS_OUTPUT vs
	(
		float3 Position : POSITION,
		float4 Color : COLOR0,
		float2 Coord : TEXCOORD0
	)
{
	VS_OUTPUT output;
	output.Position = mul(float4(Position, 1), View);
	output.Color = Color;
	output.UV = Coord;
	return output;
};
uniform sampler Texture;
uniform float2 Delta;
uniform float4 BorderColor;
uniform float Stroke;
uniform float Smooth;
float PickSample(float2 uv[9])
{
    float g[9] = {0.03448,  0.13793,  0.03448,
				  0.13793,  0.31034,  0.13793,
				  0.03448,  0.13793,  0.03448};
    float a;
    float edge = 0;
	bool a0 = false;
	bool a1 = false;
	for (int i = 0; i < 9; i++)
	{
		a = tex2D(Texture, uv[i]).a;
		if (a == 0)
			a0 = true;
		else
			a1 = true;
		edge += a * g[i];
	}
	if (a0 && a1)
		return 1 - edge;
	else
		return 0;
}
float4 ps(float4 Color : COLOR0, float2 UV : TEXCOORD0) : COLOR
{
	float4 c = tex2D(Texture, UV);
	if (c.a != 1)
	{
		float2 uv[9] = {float2(UV.x-Delta.x*Stroke,UV.y-Delta.y*Stroke), float2(UV.x,UV.y-Delta.y*Stroke), float2(UV.x+Delta.x*Stroke,UV.y-Delta.y*Stroke),
						float2(UV.x-Delta.x*Stroke,UV.y), UV, float2(UV.x+Delta.x*Stroke,UV.y),
						float2(UV.x-Delta.x*Stroke,UV.y+Delta.y*Stroke), float2(UV.x,UV.y+Delta.y*Stroke), float2(UV.x+Delta.x*Stroke,UV.y+Delta.y*Stroke)};
		float edge = PickSample(uv);
		if (edge != 0)
			c = float4(BorderColor.rgb, BorderColor.a * edge);
	}
	else
		c = Color * c;
	return c;
};
technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 vs();
		PixelShader = compile ps_3_0 ps();
	}
}");
            ShaderGray.Shader = (SHADER)preShader.LoadFromText(
@"uniform float4x4 View;
struct VS_OUTPUT
{
    float4 Position   : POSITION; 
    float4 Color      : COLOR0;
    float2 UV		  : TEXCOORD0;
};
VS_OUTPUT vs
	(
		float3 Position : POSITION,
		float4 Color : COLOR0,
		float2 Coord : TEXCOORD0
	)
{
	VS_OUTPUT output;
	output.Position = mul(float4(Position, 1), View);
	output.Color = Color;
	output.UV = Coord;
	return output;
};
uniform sampler Texture;
float4 ps(float4 Color : COLOR0, float2 UV : TEXCOORD0) : COLOR
{
	float4 c = Color * tex2D(Texture, UV);
	float min = c.r;
	if (c.g < min)
		min = c.g;
	if (c.b < min)
		min = c.b;
	return float4(min, min, min, c.a);
};
technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 vs();
		PixelShader = compile ps_3_0 ps();
	}
}");
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
            content.AddPipeline(new PipelinePicture());
            content.AddPipeline(new PipelineTile());
            content.AddPipeline(new PipelineParticle());
            content.AddPipeline(new PipelineAnimation());
            content.AddPipeline(new PipelinePiece());
            content.AddPipeline(new PipelinePatch());
            content.AddPipeline(new PipelineFontStatic());
            content.AddPipeline(new PipelineTextureXna());
            content.AddPipeline(new PipelineShaderXna());
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
