using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EntryEngine.Unity
{
	using Input = UnityEngine.Input;
	using Random = UnityEngine.Random;
	using Time = UnityEngine.Time;

	/*
	 * Sound
	 * 1. Read document refference AudioClip.Create
	 * 2. Read document refference AudioClip.SetData
	 * 
	 * Shader
	 * 1. new Material(shader code)
	 */

    public class LoggerUnity : _LOG.Logger
    {
		public override void Log(ref Record record)
		{
			UnityEngine.Debug.Log(string.Format("[{0} {1}] {2}", record.Level, record.Time, record.ToString()));
		}
	}
	public class IOWWW : _IO.iO
	{
        /// <summary>调用了Destroy的资源的对象ToString()的结果为"null"</summary>
        internal const string DESTROIED_RESOURCE = "null";
        internal static string PersistentDataPath = Application.persistentDataPath + '/';
        internal static string DataPath = Application.streamingAssetsPath + '/';

        //public override string RootDirectory
        //{
        //    get
        //    {
        //        return string.Empty;
        //    }
        //    set
        //    {
        //    }
        //}

        /// <returns>是否应使用WWW</returns>
        internal bool GetReadPath(ref string file)
        {
            //file = file.Replace('\\', '/');
            string target = PersistentDataPath + file;
            try
            {
                if (System.IO.File.Exists(target))
                {
                    file = target;
                    return false;
                }
            }
            catch (Exception)
            {
            }
            file = DataPath + file;
            return file.Contains("://");
        }
        private string GetWritePath(string file)
        {
            return PersistentDataPath + file.Replace('\\', '/');
        }
		protected override byte[] _ReadByte(string file)
		{
            if (GetReadPath(ref file))
            {
                using (WWW www = Load(file))
                {
                    return www.bytes;
                }
            }
            else
                return base._ReadByte(file);
		}
        protected override string _ReadText(string file)
        {
            if (GetReadPath(ref file))
            {
                using (WWW www = Load(file))
                {
                    return ReadPreambleText(www.bytes, IOEncoding);
                    //return ReadPreambleText(www.text, Encoding);
                    //return www.text;
                }
            }
            else
                return base._ReadText(file);
        }
		protected override AsyncReadFile _ReadAsync(string file)
		{
            string origin = file;
            if (GetReadPath(ref file))
            {
                AsyncReadFile async = new AsyncReadFile(this, file);
                AsyncUnityCoroutine coroutine = new AsyncUnityCoroutine();
                coroutine.Load(Coroutine(async));
                return async;
            }
            else
                return base._ReadAsync(origin);
		}
		protected override void _WriteByte(string file, byte[] content)
		{
            base._WriteByte(GetWritePath(file), content);
		}
        protected override void _WriteText(string file, string content, System.Text.Encoding encoding)
        {
            base._WriteText(GetWritePath(file), content, encoding);
        }
        /// <summary>
        /// Web将死循环
        /// </summary>
        [Code(ECode.BUG)]
        internal WWW Load(string file)
        {
            WWW www = new WWW(file);
            while (!www.isDone) { }
            return www;
        }
		internal System.Collections.IEnumerator Coroutine(AsyncReadFile async)
		{
			using (WWW www = new WWW(async.File))
			{
                if (!www.isDone)
				    yield return www;

				if (string.IsNullOrEmpty(www.error))
				{
					try
					{
						async.SetData(www.bytes);
					}
					catch (Exception ex)
					{
						async.Error(ex);
					}
				}
				else
				{
					async.Error(new Exception(www.error));
				}
			}
		}
	}
	public class MediaPlayerUnity : AUDIO
	{
        public override float Volume
        {
            get;
            set;
        }
        public override bool Mute
        {
            get;
            set;
        }

        public MediaPlayerUnity()
        {
            Volume = 1;
        }

        protected override void Play(ref SoundSource source, SOUND wave)
        {
            SoundSourceUnity _source;
            if (source == null)
            {
                _source = new SoundSourceUnity();
                _source.AudioSource = UnityGate.Gate.gameObject.AddComponent<AudioSource>();
                source = _source;
            }
            else
                _source = (SoundSourceUnity)source;
            _source.AudioSource.clip = ((SoundUnity)wave).AudioClip;
            _source.AudioSource.Play();
        }
		protected override void Stop(SoundSource source)
		{
            ((SoundSourceUnity)source).AudioSource.Stop();
		}
        protected override void Pause(SoundSource source)
		{
            ((SoundSourceUnity)source).AudioSource.Pause();
		}
        protected override void Resume(SoundSource source)
        {
            ((SoundSourceUnity)source).AudioSource.UnPause();
        }
	}
    public class PipelineSound : ContentPipeline
    {
        public override IEnumerable<string> SuffixProcessable
        {
            get
            {
                return SOUND.FileTypes.Enumerable();
            }
        }

        private string BuildResourcePath(string file)
        {
            file = file.Replace('\\', '/');
            int index = file.LastIndexOf(".");
            if (index != -1)
                file = file.Substring(0, index);
            return file;
        }
        //protected override Content Load(string file)
        //{
        //    return new SoundUnity(Resources.Load<AudioClip>(BuildResourcePath(file)));
        //}
        //protected override void LoadAsync(AsyncLoadContent async)
        //{
        //    AsyncData<AudioClip> load = new AsyncData<AudioClip>();
        //    var request = Resources.LoadAsync(BuildResourcePath(async.File));
        //    Entry.Instance.SetCoroutine(
        //        new CorDelegate((time) =>
        //        {
        //            if (request.isDone)
        //            {
        //                load.SetData((AudioClip)request.asset);
        //                return true;
        //            }
        //            else
        //            {
        //                async.ProgressFloat = request.progress;
        //                return false;
        //            }
        //        }));
        //    Wait(async, load, clip => new SoundUnity(clip.Data));
        //}
        protected override Content Load(string file)
        {
            IOWWW io = (IOWWW)this.IO;
            string localFile = file;
            io.GetReadPath(ref localFile);
            using (WWW www = io.Load(localFile))
                return new SoundUnity(www.audioClip);
            //else
            //{
            //    return new SoundUnity(Resources.Load<AudioClip>(BuildResourcePath(file)));
            //}
        }
        protected override void LoadAsync(AsyncLoadContent async)
        {
            IOWWW io = (IOWWW)this.IO;
            string file = async.File;
            io.GetReadPath(ref file);
            //if (System.IO.File.Exists(file))
            {
                AsyncReadFile asyncReadFile = new AsyncReadFile(io, file);
                AsyncUnityCoroutine coroutine = new AsyncUnityCoroutine();
                coroutine.Load(io.Coroutine(asyncReadFile));
            }
            //else
            //{
            //    var request = Resources.LoadAsync(BuildResourcePath(async.File));
            //    if (request.isDone)
            //    {
            //        async.SetData(new SoundUnity((AudioClip)request.asset));
            //    }
            //    else
            //    {
            //        AsyncData<AudioClip> load = new AsyncData<AudioClip>();
            //        Entry.Instance.SetCoroutine(
            //            new CorDelegate((time) =>
            //            {
            //                if (request.isDone)
            //                {
            //                    load.SetData((AudioClip)request.asset);
            //                    return true;
            //                }
            //                else
            //                {
            //                    async.ProgressFloat = request.progress;
            //                    return false;
            //                }
            //            }));
            //        Wait(async, load, clip => new SoundUnity(clip.Data));
            //    }
            //}
        }
    }
    public class AsyncUnityCoroutine : Async
    {
		private System.Collections.IEnumerator enumerator;
        private UnityEngine.Coroutine coroutine;

		public void Load(System.Collections.IEnumerator enumerator)
        {
			this.enumerator = enumerator;
			Run();
        }
		protected override void InternalRun()
		{
			coroutine = UnityGate.Gate.StartCoroutine(enumerator);
		}
		protected override void InternalComplete()
		{
			UnityGate.Gate.StopCoroutine(coroutine);
			coroutine = null;
			enumerator = null;
		}
		private System.Collections.IEnumerator Coroutine()
		{
			yield return enumerator;
			Complete();
		}
	}
    public class PipelineTextureUnity : ContentPipelineBinary
    {
        //public override IEnumerable<string> SuffixProcessable
        //{
        //    get { yield return ""; }
        //}
        public override IEnumerable<string> SuffixProcessable
        {
            get { return TEXTURE.TextureFileType.Enumerable(); }
        }

		protected override Content InternalLoad(byte[] bytes)
		{
			Texture2D texture = new Texture2D(1, 1);
			if (!texture.LoadImage(bytes))
				Resources.UnloadAsset(texture);
			return new TextureUnity(texture);
		}
	}

    public class TextureUnity : TEXTURE
    {
        private Texture2D texture;
        public override bool IsDisposed
        {
            get { return texture == null; }
        }
        public override int Width
        {
            get { return texture.width; }
        }
		public override int Height
        {
            get { return texture.height; }
        }
        public Texture2D Texture2D
        {
            get { return texture; }
            internal set { texture = value; }
        }
        public TextureUnity(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            this.texture = texture;
        }
        public override COLOR[] GetData(RECT area)
        {
            // 颜色从左下角开始取
            return texture.GetPixels((int)area.X, (int)(Height - area.Y - area.Height), (int)area.Width, (int)area.Height).GetColor();
        }
        public override void SetData(COLOR[] buffer, RECT area)
        {
            texture.SetPixels32((int)area.X, (int)(Height - area.Y - area.Height), (int)area.Width, (int)area.Height, buffer.GetColor());
            texture.Apply();
        }
		protected override void InternalDispose()
		{
            if (texture != null)
            {
                texture.Destroy();
                texture = null;
            }
		}
        public override void Save(string file)
        {
            _IO.WriteByte(file, texture.EncodeToPNG());
        }
    }
    public class FontDynamicUnity : FontDynamic
    {
        private static GUIStyle style = GUIStyle.none;
        private static GUIContent content = new GUIContent();

        public static GUIStyle FontStyle
        {
            get { return style; }
            set
            {
                if (value == null)
                    style = GUIStyle.none;
                else
                    style = value;
            }
        }

        private new class CacheInfo : FontDynamic.CacheInfo2
        {
            public Font Font;
        }

        private new CacheInfo cache
        {
            get { return (CacheInfo)base.cacheP; }
        }

        public FontDynamicUnity(string name, int size)
            : this(Font.CreateDynamicFontFromOSFont(name, (int)GetDynamicSize(size)))
        {
            this.FontSize = size;
        }
        public FontDynamicUnity(Font font)
            : base(GetCacheID(font.name, font.fontSize))
        {
            if (font == null)
                throw new ArgumentNullException("font");
            cache.Font = font;
            fontSize = font.fontSize;
            lineHeight = font.lineHeight;
        }

        protected override FontDynamic.CacheInfo2 BuildGraphicsInfo()
        {
            return new CacheInfo();
        }
        protected override FontDynamic OnSizeChanged(float fontSize)
        {
            Font font = cache.Font;
            FontDynamic result = GetCache(GetCacheID(font.name, GetDynamicSize(fontSize)));
            if (result == null)
            {
                result = new FontDynamicUnity(cache.Font.name, (int)fontSize);
                result.FontSize = fontSize;
            }
            return result;
        }
        protected override TEXTURE CreateTextureBuffer()
        {
            Texture2D texture = new Texture2D(BUFFER_SIZE, BUFFER_SIZE);
            texture.SetPixels32(new Color32[BUFFER_SIZE * BUFFER_SIZE]);
            //Entry.Instance.Delay(2000, () =>
            //    {
            //        _LOG.Debug("生成动态字体图");
            //        _IO.WriteByte("TEST.png", (cache.Font.material.mainTexture as Texture2D).EncodeToPNG());
            //        cache.Textures[0].Save("TestFont.png");
            //    });
            return new TextureUnity(texture);
        }
        /// <summary>
        /// 1. 文字有黑色边缘
        /// 2. 数组越界
        /// </summary>
        [Code(ECode.BUG)]
        protected override COLOR[] DrawChar(char c, ref RECT uv)
        {
            if ('\r' == c)
                return null;

            Font font = cache.Font;

            //if (!font.HasCharacter(c))
                font.RequestCharactersInTexture(c.ToString());
            CharacterInfo info;
            font.GetCharacterInfo(c, out info);
            Texture2D graphics = font.material.mainTexture as Texture2D;
            //_IO.WriteByte("TEXT.png", graphics.EncodeToPNG());

            Vector2 tl = info.uvTopLeft;
            Vector2 tr = info.uvTopRight;
            Vector2 bl = info.uvBottomLeft;
            Vector2 br = info.uvBottomRight;
            int width = font.material.mainTexture.width;
            int height = font.material.mainTexture.height;
            POINT ptl = new POINT((int)(tl.x * width + 0.5f), (int)(tl.y * height + 0.5f));
            POINT ptr = new POINT((int)(tr.x * width + 0.5f), (int)(tr.y * height + 0.5f));
            POINT pbl = new POINT((int)(bl.x * width + 0.5f), (int)(bl.y * height + 0.5f));
            POINT pbr = new POINT((int)(br.x * width + 0.5f), (int)(br.y * height + 0.5f));
            int x, y, w, h;
            bool flipV;// 循环宽度i
            bool flipH;// 循环高度j
            bool flipped = ptl.X != pbl.X;
            // 旋转90度
            if (flipped)
            {
                // 左右颠倒
                if (ptl.Y < ptr.Y)
                {
                    flipV = true;
                    y = ptl.Y;
                    h = ptr.Y - ptl.Y;
                }
                else
                {
                    flipV = false;
                    y = ptr.Y;
                    h = ptl.Y - ptr.Y;
                }
                // 左右颠倒
                if (ptl.X > pbl.X)
                {
                    flipH = true;
                    x = pbl.X;
                    w = ptl.X - pbl.X;
                }
                else
                {
                    flipH = false;
                    x = ptl.X;
                    w = pbl.X - ptl.X;
                }
            }
            else
            {
                // 左右颠倒
                if (ptl.X > ptr.X)
                {
                    flipH = true;
                    x = ptr.X;
                    w = ptl.X - ptr.X;
                }
                else
                {
                    flipH = false;
                    x = ptl.X;
                    w = ptr.X - ptl.X;
                }
                // 上下颠倒
                if (ptl.Y < pbl.Y)
                {
                    flipV = true;
                    y = ptl.Y;
                    h = pbl.Y - ptl.Y;
                }
                else
                {
                    flipV = false;
                    y = pbl.Y;
                    h = ptl.Y - pbl.Y;
                }
            }

            // GetPixel矩形为屏幕坐标
            COLOR[] colors = graphics.GetPixels(x, y, w, h).GetColor();

            if (flipped)
                Utility.Swap(ref w, ref h);
            // 由于w,h不等于uv的宽高，导致文字变为等宽字体，绘制时文字对不齐
            //uv.Width = w;
            //uv.Height = h;
            //width = (int)uv.Width;
            //height = (int)uv.Height;
            width = _MATH.Ceiling(uv.Width);
            height = _MATH.Ceiling(uv.Height);
            //if (w > width || h > height)
            //    throw new ArgumentOutOfRangeException("Buffer size must bigger than source size.");
            int nextline = width - w;
            int start = ((height - h - info.minY - (font.lineHeight - font.ascent)) * width) + _MATH.Ceiling(nextline * 0.5f);
            //_LOG.Debug("height={0} h={1} info.minY={2} lineheight={3} ascent={4} width={5} nextline={6} start={7}", height, h, info.minY, font.lineHeight, font.ascent, width, nextline, start);

            COLOR[] result = new COLOR[width * height];
            //_IO.WriteText(c + ".txt", string.Join("\r\n", graphics.GetPixels(x, y, w, h).Where(cl => cl.a != 0).Select(cl => string.Format("r:{0} g:{1} b:{2} a:{3}", cl.r, cl.g, cl.b, cl.a)).ToArray()));

            // 从colors上取正确的颜色到result里
            int presult = start;
            int count = result.Length;

            int i1, i2, i3;
            int j1, j2, j3;
            if (flipV)
            {
                i1 = h - 1;
                i2 = -1;
                i3 = -1;
            }
            else
            {
                i1 = 0;
                i2 = h;
                i3 = 1;
            }
            if (flipH)
            {
                j1 = w - 1;
                j2 = -1;
                j3 = -1;
            }
            else
            {
                j1 = 0;
                j2 = w;
                j3 = 1;
            }

            if (flipped)
            {
                int index = 0;
                for (int i = i1; i != i2; i += i3)
                {
                    for (int j = j1; j != j2; j += j3)
                    {
                        index = j * h + i;
                        result[presult].R = colors[index].R;
                        result[presult].G = colors[index].G;
                        result[presult].B = colors[index].B;
                        result[presult].A = colors[index].A;
                        presult++;
                    }
                    presult += nextline;
                }
            }
            else
            {
                int index = 0;
                for (int i = i1; i != i2; i += i3)
                {
                    index = i * w;
                    for (int j = j1; j != j2; j += j3)
                    {
                        result[presult].R = colors[index].R;
                        result[presult].G = colors[index].G;
                        result[presult].B = colors[index].B;
                        result[presult].A = colors[index].A;
                        presult++;
                        index++;
                    }
                    presult += nextline;
                }
            }

            return result;
        }
        /// <summary>
        /// // 5.4以后CalcSize算出来的高度变低，导致start的计算变为负数以至于数组越界
        /// </summary>
        [Code(ECode.BUG)]
        protected override VECTOR2 MeasureBufferSize(char c)
        {
            content.text = c.ToString();
            style.font = cache.Font;
            if (Application.unityVersion.StartsWith("5.3"))
                return style.CalcSize(content).GetVector2();
            VECTOR2 size1 = FontStyle.CalcSize(content).GetVector2();
            VECTOR2 size2 = base.MeasureBufferSize(c);
            return new VECTOR2(_MATH.Max(size1.X, size2.X), _MATH.Max(size1.Y, size2.Y));
        }
        protected override void InternalDispose()
        {
            base.InternalDispose();
            if (cache.Font != null)
                cache.Font.Destroy();
        }
        protected override Content Cache()
        {
            FontDynamicUnity font = new FontDynamicUnity(cache.Font);
            font._Key = this._Key;
            return font;
        }
    }
    public class ShaderUnity : SHADER
    {
        private Material material;
        private bool disposed;
        public Material Material
        {
            get { return material; }
        }
        public int PassCount
        {
            get { return material.passCount; }
        }
        public ShaderUnity() { }
        public ShaderUnity(UnityEngine.Shader shader)
        {
            material = new Material(shader);
        }
        public bool IsDisposed
        {
            get { return disposed; }
        }
        public void Dispose()
        {
			disposed = true;
			if (material != null)
			{
				Resources.UnloadAsset(material);
				material = null;
			}
        }
        public void LoadFromCode(string code)
        {
            material = new Material(code);
        }
        public bool SetPass(int pass)
        {
            return material.SetPass(pass);
        }
        public bool HasProperty(string name)
        {
            return material.HasProperty(name);
        }
        public bool GetValueBoolean(string property)
        {
            return material.GetInt(property) != 0;
        }
        public int GetValueInt32(string property)
        {
            return material.GetInt(property);
        }
        public MATRIX GetValueMatrix(string property)
        {
            return material.GetMatrix(property).GetMatrix();
        }
        public float GetValueSingle(string property)
        {
            return material.GetFloat(property);
        }
        public TEXTURE GetValueTexture(string property)
        {
            return new TextureUnity((Texture2D)material.GetTexture(property));
        }
        public VECTOR2 GetValueVector2(string property)
        {
            Vector4 vector = material.GetVector(property);
            return new VECTOR2(vector.x, vector.y);
        }
        public VECTOR3 GetValueVector3(string property)
        {
            Vector4 vector = material.GetVector(property);
            return new VECTOR3(vector.x, vector.y, vector.z);
        }
        public VECTOR4 GetValueVector4(string property)
        {
            return material.GetVector(property).GetVector4();
        }
        public void SetValue(string property, bool value)
        {
            material.SetInt(property, value ? 1 : 0);
        }
        public void SetValue(string property, float value)
        {
            material.SetFloat(property, value);
        }
        public void SetValue(string property, int value)
        {
            material.SetInt(property, value);
        }
        public void SetValue(string property, MATRIX value)
        {
            material.SetMatrix(property, value.GetMatrix());
        }
        public void SetValue(string property, TEXTURE value)
        {
            material.SetTexture(property, value.GetTexture());
        }
        public void SetValue(string property, VECTOR2 value)
        {
            material.SetVector(property, new Vector4(value.X, value.Y, 0, 0));
        }
        public void SetValue(string property, VECTOR3 value)
        {
            material.SetVector(property, new Vector4(value.X, value.Y, value.Z, 0));
        }
        public void SetValue(string property, VECTOR4 value)
        {
            material.SetVector(property, value.GetVector4());
        }
        public SHADER Clone()
        {
            ShaderUnity clone = new ShaderUnity();
            clone.material = new Material(this.material);
            return clone;
        }
    }
    public class SoundUnity : SOUND
    {
        public AudioClip AudioClip
        {
            get;
            private set;
        }
        public override bool IsDisposed
        {
            get { return AudioClip == null || AudioClip.ToString() == IOWWW.DESTROIED_RESOURCE; }
        }
        public SoundUnity(AudioClip clip)
        {
            this.AudioClip = clip;
        }
        protected override void InternalDispose()
        {
            if (AudioClip != null)
            {
                AudioClip.Destroy();
                //Resources.UnloadAsset(AudioClip);
                AudioClip = null;
            }
        }
    }
    public class SoundSourceUnity : SoundSource
    {
        public AudioSource AudioSource
        {
            get;
            internal set;
        }
        protected override ESoundState State
        {
            get { return AudioSource.isPlaying ? ESoundState.Playing : ESoundState.Stopped; }
        }
        protected override float Volume
        {
            get { return AudioSource.volume; }
            set { AudioSource.volume = value; }
        }
        protected override float Channel
        {
            get { return AudioSource.panStereo; }
            set { AudioSource.panStereo = value; }
        }
        protected override void SetLoop(bool loop)
        {
            AudioSource.loop = loop;
        }
    }
    public class ObjectUnity : Content
    {
        public override bool IsDisposed
        {
            get { return UnityObject == null; }
        }
        public UnityEngine.Object UnityObject
        {
            get;
            private set;
        }
        public ObjectUnity(UnityEngine.Object obj)
        {
            this.UnityObject = obj;
        }
        protected override void InternalDispose()
        {
            if (UnityObject != null)
            {
                UnityObject.Destroy();
                UnityObject = null;
            }
        }
    }
    public class GraphicsUnityGL : GRAPHICS
    {
        private RECT screenShotArea;
        private Coroutine screenShotCoroutine;
        private MATRIX2x3 modelview;
        private VECTOR2 _gs;

        public override bool IsFullScreen
        {
            get { return Screen.fullScreen; }
            set { Screen.fullScreen = value; }
        }
        protected override VECTOR2 InternalScreenSize
        {
            get { return new VECTOR2(Screen.width, Screen.height); }
            set { Screen.SetResolution((int)value.X, (int)value.Y, IsFullScreen); }
        }

        public GraphicsUnityGL()
        {
            //XCornerOffsets = new float[] { 0, 0, 1, 1 };
            XCornerOffsets[1] = 0;
            XCornerOffsets[3] = 1;
            //YCornerOffsets = new float[] { 1, 0, 0, 1 };
            YCornerOffsets[0] = 1;
            YCornerOffsets[2] = 0;
        }

        protected override void SetViewport(MATRIX2x3 view, RECT viewport)
        {
        }
        protected override void InternalBegin(ref MATRIX2x3 matrix, ref RECT graphics, SHADER shader)
        {
            if (RenderTargetCount > 1)
                // Flush
                base.Ending(null);

            GL.LoadOrtho();
            Rect screen = AreaToScreen(graphics).ToCartesian();
            GL.Viewport(screen);

            VECTOR2 temp = GraphicsSize;
            matrix.M31 /= temp.X;
            matrix.M32 /= temp.Y;
            modelview = 
                matrix * MATRIX2x3.Invert(View)
                * MATRIX2x3.CreateTranslation(-graphics.X / temp.X, -graphics.Y / temp.Y)
                * MATRIX2x3.CreateScale(temp.X / graphics.Width, temp.Y / graphics.Height)
                ;
        }
        protected override void InternalDraw(TEXTURE texture, ref SpriteVertex vertex)
        {
            float x = _MATH.DIVIDE_BY_1[texture.Width];
            float y = _MATH.DIVIDE_BY_1[texture.Height];

            vertex.Origin.X *= x;
            vertex.Origin.Y *= y;

            vertex.Source.X *= x;
            vertex.Source.Y *= y;
            vertex.Source.Width *= x;
            vertex.Source.Height *= y;
            
            base.InternalDraw(texture, ref vertex);
        }
        protected override void DrawPrimitivesBegin(TEXTURE texture)
        {
            if (texture == null)
            {
                _LOG.Debug("DrawPrimitivesBegin Texture: null");
                return;
            }
            UnityGate.GLMaterial.mainTexture = texture.GetTexture();
            UnityGate.GLMaterial.SetPass(0);
            GL.Begin(GL.TRIANGLES);

            var graphics = GraphicsSize;
            _gs.X = 1 / graphics.X;
            _gs.Y = 1 / graphics.Y;
        }
        protected override void OutputVertex(ref TextureVertex output)
        {
            output.TextureCoordinate.Y = 1 - output.TextureCoordinate.Y;

            // 使用左上坐标系计算好坐标再转换成左下角坐标
            output.Position.X *= _gs.X;
            output.Position.Y *= _gs.Y;
            VECTOR2.Transform(ref output.Position.X, ref output.Position.Y, ref modelview);
            output.Position.Y = 1 - output.Position.Y;
        }
        protected override void DrawPrimitives(TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount)
        {
            int idx;
            for (int c = 0; c < primitiveCount; c++)
            {
                idx = indexOffset + c * 3;
                DrawPrimitive(ref vertices[offset + indices[idx]]);
                DrawPrimitive(ref vertices[offset + indices[idx + 1]]);
                DrawPrimitive(ref vertices[offset + indices[idx + 2]]);
            }
        }
        private void DrawPrimitive(ref TextureVertex vertex)
        {
            GL.Color(vertex.Color.GetColor());
            GL.TexCoord2(vertex.TextureCoordinate.X, vertex.TextureCoordinate.Y);
            GL.Vertex3(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
        }
        protected override void DrawPrimitivesEnd()
        {
            GL.End();
        }
        public override TEXTURE Screenshot(RECT graphics)
        {
            Rect rect = AreaToScreen(graphics).ToCartesian();
            Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();
            return new TextureUnity(texture);
        }
        public override void Screenshot(RECT graphics, Action<TEXTURE> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            if (screenShotCoroutine != null)
                throw new InvalidOperationException("Screenshot coroutine is running.");
            if (!screenShotArea.IsEmpty)
                throw new InvalidOperationException("Screenshot began in BeginScreenshot.");
            this.screenShotArea = graphics;
            UnityGate.Gate.StartCoroutine(ScreenshotCoroutine(callback));
        }
        private System.Collections.IEnumerator ScreenshotCoroutine(Action<TEXTURE> callback)
        {
            yield return new WaitForEndOfFrame();
            callback(Screenshot(screenShotArea));
            screenShotArea = new RECT();
            screenShotCoroutine = null;
        }
        public override void BeginScreenshot(RECT graphics)
        {
            if (!screenShotArea.IsEmpty)
                throw new InvalidOperationException("Screenshot coroutine is running.");
            GL.Clear(true, true, Color.clear, 0);
            screenShotArea = graphics;
        }
        public override TEXTURE EndScreenshot()
        {
            if (screenShotArea.IsEmpty)
                throw new InvalidOperationException("Screenshot coroutine has not began.");
            if (screenShotCoroutine != null)
                throw new InvalidOperationException("Screenshot began in Screenshot.");
            var screenshot = Screenshot(screenShotArea);
            screenShotArea = new RECT();
            return screenshot;
        }
    }

    public struct MouseStateUnity : IMouseState
    {
        private VECTOR2 position;
        private float scrollWheelValue;
        private bool left;
        private bool right;
        private bool middle;

        public float ScrollWheelValue
        {
            get { return scrollWheelValue; }
        }
        public VECTOR2 Position
        {
            get { return position; }
            set
            {
                value = value.ToCartesian();
                this.position = value;
                Input.mousePosition.Set(value.X, value.Y, 0);
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

        public static MouseStateUnity GetState()
        {
            MouseStateUnity state;
            // 转换屏幕左上角坐标
            // 转换矩阵变换屏幕坐标
            // 限制屏幕内坐标
            state.position = Input.mousePosition.GetVector2().ToCartesian();
            state.scrollWheelValue = Input.GetAxis("Mouse ScrollWheel");
            state.left = Input.GetMouseButton(0);
            state.right = Input.GetMouseButton(0);
            state.middle = Input.GetMouseButton(0);
            return state;
        }
	}
    public struct TouchStateUnity : ITouchState
    {
        private float pressure;
        private VECTOR2 position;
        private bool click;

        public VECTOR2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public float Pressure
        {
            get { return pressure; }
        }

        internal TouchStateUnity(Touch touch)
        {
            // 坐标转换
            this.position = touch.position.GetVector2().ToCartesian();
            this.pressure = touch.pressure;
            this.click = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
        }

        public bool IsClick(int key)
        {
            return click;
        }
    }
    /// <summary>
    /// 网页不能使用指针
    /// </summary>
    public struct KeyboardStateUnity : IKeyboardState
    {
        private const byte MAX_KEY = 8;
        private static int[] Empty = new int[0];
        public static KeyCode[] Keys = (KeyCode[])Enum.GetValues(typeof(KeyCode));
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
        public byte this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return key1;
                    case 1: return key2;
                    case 2: return key3;
                    case 3: return key4;
                    case 4: return key5;
                    case 5: return key6;
                    case 6: return key7;
                    case 7: return key8;
                    default:
                        throw new ArgumentException("index");
                }
            }
            set
            {
                switch (index)
                {
                    case 0: key1 = value; break;
                    case 1: key2 = value; break;
                    case 2: key3 = value; break;
                    case 3: key4 = value; break;
                    case 4: key5 = value; break;
                    case 5: key6 = value; break;
                    case 6: key7 = value; break;
                    case 7: key8 = value; break;
                    default:
                        throw new ArgumentException("index");
                }
            }
        }

        public int[] GetPressedKey()
        {
            if (count == 0)
                return Empty;
            int[] keys = new int[count];
            //unsafe
            //{
            //    fixed (KeyboardStateUnity* ptr = &this)
            //        for (int i = 0; i < count; i++)
            //            keys[i] = *((byte*)ptr + i);
            //}
            for (int i = 0; i < count; i++)
                keys[i] = this[i];
            return keys;
        }
        public bool IsClick(int key)
        {
            //unsafe
            //{
            //    fixed (KeyboardStateUnity* ptr = &this)
            //        for (int i = 0; i < count; i++)
            //            if (*((byte*)ptr + i) == key)
            //                return true;
            //}
            for (int i = 0; i < count; i++)
                if (this[i] == key)
                    return true;
            return false;
        }

        public static KeyboardStateUnity GetState()
        {
            KeyboardStateUnity state;
            //unsafe
            //{
            //    KeyboardStateUnity* ptr = &state;
            //    // 135: KeyCode.Mouse0
            //    byte* ptr2 = &ptr->key1;
            //    for (int i = 0; i < 135; i++)
            //    {
            //        if (Input.GetKey(Keys[i]))
            //        {
            //            byte key = (byte)KeyboardUnity.GetKey(Keys[i]);
            //            if (key == 0)
            //                continue;
            //            //*((byte*)ptr + i) = key;
            //            *ptr2 = key;
            //            ptr2++;
            //            if (++(ptr->count) == MAX_KEY)
            //                break;
            //        }
            //    }
            //}
            state = new KeyboardStateUnity();
            for (int i = 0; i < 135; i++)
            {
                if (Input.GetKey(Keys[i]))
                {
                    byte key = (byte)KeyboardUnity.GetKey(Keys[i]);
                    if (key == 0)
                        continue;
                    state[state.count] = key;
                    if (++state.count == MAX_KEY)
                        break;
                }
            }
            return state;
        }
    }
    public class TouchSimUnity : SingleTouch<IMouseState>
    {
        protected override IMouseState GetState()
        {
            MouseStateUnity state = new MouseStateUnity();
            if (state.IsClick(DefaultKey))
            {
                return state;
            }
            return null;
        }
    }
    public class MouseUnity : MOUSE
    {
        protected override IMouseState GetState()
        {
            return MouseStateUnity.GetState();
        }
    }
    public class TouchUnity : TOUCH
    {
        protected override int GetTouches(ITouchState[] states)
        {
            // TouchState.ID
            // 根据按下顺序标识为01234(最大5个)
            // 放开中间的，后面的ID不会往前，类似于Pool
            Touch[] touches = Input.touches;

            int current = _MATH.Min(touches.Length, states.Length);
            for (int i = 0; i < current; i++)
                states[i] = new TouchStateUnity(touches[i]);

            return current;
        }
    }
	public class KeyboardUnity : KEYBOARD
    {
        public static Dictionary<KeyCode, PCKeys> ToPCKeys;

        static KeyboardUnity()
        {
            ToPCKeys = new Dictionary<KeyCode, PCKeys>();
            //ToPCKeys.Add((KeyCode)301, (PCKeys)20); // CapsLock
            //ToPCKeys.Add((KeyCode)280, (PCKeys)33); // PageUp
            //ToPCKeys.Add((KeyCode)281, (PCKeys)34); // PageDown
            //ToPCKeys.Add((KeyCode)279, (PCKeys)35); // End
            //ToPCKeys.Add((KeyCode)278, (PCKeys)36); // Home
            //ToPCKeys.Add((KeyCode)316, (PCKeys)42); // Print
            //ToPCKeys.Add((KeyCode)277, (PCKeys)45); // Insert
            //ToPCKeys.Add((KeyCode)127, (PCKeys)46); // Delete
            //ToPCKeys.Add((KeyCode)315, (PCKeys)47); // Help
            //ToPCKeys.Add((KeyCode)97, (PCKeys)65); // A
            //ToPCKeys.Add((KeyCode)98, (PCKeys)66); // B
            //ToPCKeys.Add((KeyCode)99, (PCKeys)67); // C
            //ToPCKeys.Add((KeyCode)100, (PCKeys)68); // D
            //ToPCKeys.Add((KeyCode)101, (PCKeys)69); // E
            //ToPCKeys.Add((KeyCode)102, (PCKeys)70); // F
            //ToPCKeys.Add((KeyCode)103, (PCKeys)71); // G
            //ToPCKeys.Add((KeyCode)104, (PCKeys)72); // H
            //ToPCKeys.Add((KeyCode)105, (PCKeys)73); // I
            //ToPCKeys.Add((KeyCode)106, (PCKeys)74); // J
            //ToPCKeys.Add((KeyCode)107, (PCKeys)75); // K
            //ToPCKeys.Add((KeyCode)108, (PCKeys)76); // L
            //ToPCKeys.Add((KeyCode)109, (PCKeys)77); // M
            //ToPCKeys.Add((KeyCode)110, (PCKeys)78); // N
            //ToPCKeys.Add((KeyCode)111, (PCKeys)79); // O
            //ToPCKeys.Add((KeyCode)112, (PCKeys)80); // P
            //ToPCKeys.Add((KeyCode)113, (PCKeys)81); // Q
            //ToPCKeys.Add((KeyCode)114, (PCKeys)82); // R
            //ToPCKeys.Add((KeyCode)115, (PCKeys)83); // S
            //ToPCKeys.Add((KeyCode)116, (PCKeys)84); // T
            //ToPCKeys.Add((KeyCode)117, (PCKeys)85); // U
            //ToPCKeys.Add((KeyCode)118, (PCKeys)86); // V
            //ToPCKeys.Add((KeyCode)119, (PCKeys)87); // W
            //ToPCKeys.Add((KeyCode)120, (PCKeys)88); // X
            //ToPCKeys.Add((KeyCode)121, (PCKeys)89); // Y
            //ToPCKeys.Add((KeyCode)122, (PCKeys)90); // Z
            //ToPCKeys.Add((KeyCode)311, (PCKeys)91); // LeftWindows
            //ToPCKeys.Add((KeyCode)312, (PCKeys)92); // RightWindows
            //ToPCKeys.Add((KeyCode)282, (PCKeys)112); // F1
            //ToPCKeys.Add((KeyCode)283, (PCKeys)113); // F2
            //ToPCKeys.Add((KeyCode)284, (PCKeys)114); // F3
            //ToPCKeys.Add((KeyCode)285, (PCKeys)115); // F4
            //ToPCKeys.Add((KeyCode)286, (PCKeys)116); // F5
            //ToPCKeys.Add((KeyCode)287, (PCKeys)117); // F6
            //ToPCKeys.Add((KeyCode)288, (PCKeys)118); // F7
            //ToPCKeys.Add((KeyCode)289, (PCKeys)119); // F8
            //ToPCKeys.Add((KeyCode)290, (PCKeys)120); // F9
            //ToPCKeys.Add((KeyCode)291, (PCKeys)121); // F10
            //ToPCKeys.Add((KeyCode)292, (PCKeys)122); // F11
            //ToPCKeys.Add((KeyCode)293, (PCKeys)123); // F12
            //ToPCKeys.Add((KeyCode)294, (PCKeys)124); // F13
            //ToPCKeys.Add((KeyCode)295, (PCKeys)125); // F14
            //ToPCKeys.Add((KeyCode)296, (PCKeys)126); // F15
            //ToPCKeys.Add((KeyCode)304, (PCKeys)160); // LeftShift
            //ToPCKeys.Add((KeyCode)303, (PCKeys)161); // RightShift
            //ToPCKeys.Add((KeyCode)306, (PCKeys)162); // LeftControl
            //ToPCKeys.Add((KeyCode)305, (PCKeys)163); // RightControl
            //ToPCKeys.Add((KeyCode)308, (PCKeys)164); // LeftAlt
            //ToPCKeys.Add((KeyCode)307, (PCKeys)165); // RightAlt

            ToPCKeys.Add(KeyCode.None, PCKeys.None);
            ToPCKeys.Add(KeyCode.Tab, PCKeys.Tab);
            ToPCKeys.Add(KeyCode.Pause, PCKeys.Pause);
            ToPCKeys.Add(KeyCode.Escape, PCKeys.Escape);
            ToPCKeys.Add(KeyCode.Space, PCKeys.Space);
            ToPCKeys.Add(KeyCode.A, PCKeys.A);
            ToPCKeys.Add(KeyCode.B, PCKeys.B);
            ToPCKeys.Add(KeyCode.C, PCKeys.C);
            ToPCKeys.Add(KeyCode.D, PCKeys.D);
            ToPCKeys.Add(KeyCode.E, PCKeys.E);
            ToPCKeys.Add(KeyCode.F, PCKeys.F);
            ToPCKeys.Add(KeyCode.G, PCKeys.G);
            ToPCKeys.Add(KeyCode.H, PCKeys.H);
            ToPCKeys.Add(KeyCode.I, PCKeys.I);
            ToPCKeys.Add(KeyCode.J, PCKeys.J);
            ToPCKeys.Add(KeyCode.K, PCKeys.K);
            ToPCKeys.Add(KeyCode.L, PCKeys.L);
            ToPCKeys.Add(KeyCode.M, PCKeys.M);
            ToPCKeys.Add(KeyCode.N, PCKeys.N);
            ToPCKeys.Add(KeyCode.O, PCKeys.O);
            ToPCKeys.Add(KeyCode.P, PCKeys.P);
            ToPCKeys.Add(KeyCode.Q, PCKeys.Q);
            ToPCKeys.Add(KeyCode.R, PCKeys.R);
            ToPCKeys.Add(KeyCode.S, PCKeys.S);
            ToPCKeys.Add(KeyCode.T, PCKeys.T);
            ToPCKeys.Add(KeyCode.U, PCKeys.U);
            ToPCKeys.Add(KeyCode.V, PCKeys.V);
            ToPCKeys.Add(KeyCode.W, PCKeys.W);
            ToPCKeys.Add(KeyCode.X, PCKeys.X);
            ToPCKeys.Add(KeyCode.Y, PCKeys.Y);
            ToPCKeys.Add(KeyCode.Z, PCKeys.Z);
            ToPCKeys.Add(KeyCode.Delete, PCKeys.Delete);
            ToPCKeys.Add(KeyCode.Insert, PCKeys.Insert);
            ToPCKeys.Add(KeyCode.Home, PCKeys.Home);
            ToPCKeys.Add(KeyCode.End, PCKeys.End);
            ToPCKeys.Add(KeyCode.PageUp, PCKeys.PageUp);
            ToPCKeys.Add(KeyCode.PageDown, PCKeys.PageDown);
            ToPCKeys.Add(KeyCode.F1, PCKeys.F1);
            ToPCKeys.Add(KeyCode.F2, PCKeys.F2);
            ToPCKeys.Add(KeyCode.F3, PCKeys.F3);
            ToPCKeys.Add(KeyCode.F4, PCKeys.F4);
            ToPCKeys.Add(KeyCode.F5, PCKeys.F5);
            ToPCKeys.Add(KeyCode.F6, PCKeys.F6);
            ToPCKeys.Add(KeyCode.F7, PCKeys.F7);
            ToPCKeys.Add(KeyCode.F8, PCKeys.F8);
            ToPCKeys.Add(KeyCode.F9, PCKeys.F9);
            ToPCKeys.Add(KeyCode.F10, PCKeys.F10);
            ToPCKeys.Add(KeyCode.F11, PCKeys.F11);
            ToPCKeys.Add(KeyCode.F12, PCKeys.F12);
            ToPCKeys.Add(KeyCode.F13, PCKeys.F13);
            ToPCKeys.Add(KeyCode.F14, PCKeys.F14);
            ToPCKeys.Add(KeyCode.F15, PCKeys.F15);
            ToPCKeys.Add(KeyCode.CapsLock, PCKeys.CapsLock);
            ToPCKeys.Add(KeyCode.RightShift, PCKeys.RightShift);
            ToPCKeys.Add(KeyCode.LeftShift, PCKeys.LeftShift);
            ToPCKeys.Add(KeyCode.RightControl, PCKeys.RightControl);
            ToPCKeys.Add(KeyCode.LeftControl, PCKeys.LeftControl);
            ToPCKeys.Add(KeyCode.RightAlt, PCKeys.RightAlt);
            ToPCKeys.Add(KeyCode.LeftAlt, PCKeys.LeftAlt);
            ToPCKeys.Add(KeyCode.LeftWindows, PCKeys.LeftWindows);
            ToPCKeys.Add(KeyCode.RightWindows, PCKeys.RightWindows);
            ToPCKeys.Add(KeyCode.Help, PCKeys.Help);
            ToPCKeys.Add(KeyCode.Print, PCKeys.Print);

            ToPCKeys.Add(KeyCode.LeftArrow, PCKeys.Left);
            ToPCKeys.Add(KeyCode.UpArrow, PCKeys.Up);
            ToPCKeys.Add(KeyCode.RightArrow, PCKeys.Right);
            ToPCKeys.Add(KeyCode.DownArrow, PCKeys.Down);
            ToPCKeys.Add(KeyCode.BackQuote, PCKeys.OemTilde);
            ToPCKeys.Add(KeyCode.LeftParen, PCKeys.D0);
            ToPCKeys.Add(KeyCode.Exclaim, PCKeys.D1);
            ToPCKeys.Add(KeyCode.At, PCKeys.D2);
            ToPCKeys.Add(KeyCode.Hash, PCKeys.D3);
            ToPCKeys.Add(KeyCode.Dollar, PCKeys.D4);
            //ToPCKeys.Add(KeyCode.BackQuote, PCKeys.D5);
            ToPCKeys.Add(KeyCode.Caret, PCKeys.D6);
            ToPCKeys.Add(KeyCode.Ampersand, PCKeys.D7);
            ToPCKeys.Add(KeyCode.Asterisk, PCKeys.D8);
            ToPCKeys.Add(KeyCode.RightParen, PCKeys.D9);
            ToPCKeys.Add(KeyCode.Minus, PCKeys.OemMinus);
            ToPCKeys.Add(KeyCode.Underscore, PCKeys.OemMinus);
            ToPCKeys.Add(KeyCode.Plus, PCKeys.OemPlus);
            ToPCKeys.Add(KeyCode.Equals, PCKeys.OemPlus);
            ToPCKeys.Add(KeyCode.LeftBracket, PCKeys.OemOpenBrackets);
            //ToPCKeys.Add(KeyCode.LeftBracket, PCKeys.OemOpenBrackets);
            ToPCKeys.Add(KeyCode.RightBracket, PCKeys.OemCloseBrackets);
            //ToPCKeys.Add(KeyCode.RightBracket, PCKeys.OemCloseBrackets);
            ToPCKeys.Add(KeyCode.Backslash, PCKeys.OemPipe);
            //ToPCKeys.Add(KeyCode.Backslash, PCKeys.OemPipe);
            ToPCKeys.Add(KeyCode.Semicolon, PCKeys.OemSemicolon);
            ToPCKeys.Add(KeyCode.Colon, PCKeys.OemSemicolon);
            ToPCKeys.Add(KeyCode.Quote, PCKeys.OemQuotes);
            ToPCKeys.Add(KeyCode.DoubleQuote, PCKeys.OemQuotes);
            ToPCKeys.Add(KeyCode.Comma, PCKeys.OemComma);
            ToPCKeys.Add(KeyCode.Less, PCKeys.OemComma);
            ToPCKeys.Add(KeyCode.Period, PCKeys.OemPeriod);
            ToPCKeys.Add(KeyCode.Greater, PCKeys.OemPeriod);
            ToPCKeys.Add(KeyCode.Slash, PCKeys.OemQuestion);
            ToPCKeys.Add(KeyCode.Question, PCKeys.OemQuestion);
            ToPCKeys.Add(KeyCode.Keypad0, PCKeys.NumPad0);
            ToPCKeys.Add(KeyCode.Keypad1, PCKeys.NumPad1);
            ToPCKeys.Add(KeyCode.Keypad2, PCKeys.NumPad2);
            ToPCKeys.Add(KeyCode.Keypad3, PCKeys.NumPad3);
            ToPCKeys.Add(KeyCode.Keypad4, PCKeys.NumPad4);
            ToPCKeys.Add(KeyCode.Keypad5, PCKeys.NumPad5);
            ToPCKeys.Add(KeyCode.Keypad6, PCKeys.NumPad6);
            ToPCKeys.Add(KeyCode.Keypad7, PCKeys.NumPad7);
            ToPCKeys.Add(KeyCode.Keypad8, PCKeys.NumPad8);
            ToPCKeys.Add(KeyCode.Keypad9, PCKeys.NumPad9);
            ToPCKeys.Add(KeyCode.Break, PCKeys.Sleep);
            ToPCKeys.Add(KeyCode.KeypadMultiply, PCKeys.Multiply);
            ToPCKeys.Add(KeyCode.KeypadPlus, PCKeys.Add);
            ToPCKeys.Add(KeyCode.KeypadMinus, PCKeys.Subtract);
            ToPCKeys.Add(KeyCode.KeypadPeriod, PCKeys.Decimal);
            ToPCKeys.Add(KeyCode.KeypadEnter, PCKeys.Enter);
            ToPCKeys.Add(KeyCode.KeypadDivide, PCKeys.Divide);
            ToPCKeys.Add(KeyCode.Numlock, PCKeys.NumLock);
            ToPCKeys.Add(KeyCode.ScrollLock, PCKeys.Scroll);
        }

		protected override IKeyboardState GetState()
        {
            return KeyboardStateUnity.GetState();
        }

        public static PCKeys GetKey(KeyCode key)
        {
            PCKeys result;
            if (ToPCKeys.TryGetValue(key, out result))
                return result;
            if ((int)key > 255)
                return PCKeys.None;
            return (PCKeys)key;
        }
    }
    public class InputTextUnity : InputText
    {
        private TouchScreenKeyboard keyboard;
        //public override bool ImmCapturing
        //{
        //    get
        //    {
        //        return UnityEngine.Input.imeIsSelected;
        //    }
        //}
        protected override EInput InputCapture(out string text)
        {
            if (keyboard == null)
            {
                text = UnityEngine.Input.inputString;
                return EInput.Input;
            }
            else
            {
                text = keyboard.text;
                if (keyboard.wasCanceled)
                    return EInput.Canceled;
                else if (keyboard.done)
                    return EInput.Done;
                else
                    return EInput.Replace;
            }
        }
        protected override void OnActive(ITypist typist)
        {
            OnStop(typist);
            if (TouchScreenKeyboard.isSupported)
            {
                keyboard = TouchScreenKeyboard.Open(typist.Text, TouchScreenKeyboardType.Default, true, typist.Multiple, false, true, null);
            }
        }
        protected override void OnStop(ITypist typist)
        {
            keyboard = null;
        }
    }

    public class PlatformUnity : IPlatform
    {
        public EPlatform Platform
        {
            get;
            private set;
        }
        public bool IsMouseVisible
        {
            get { return Cursor.visible; }
            set { Cursor.visible = value; }
        }
        public TimeSpan FrameRate
        {
            get { return TimeSpan.FromSeconds(Time.fixedDeltaTime); }
            set { Time.fixedDeltaTime = (float)value.TotalSeconds; }
        }
        public bool IsActive
        {
            get { return Application.isPlaying; }
        }

        internal EntryEngine.INPUT BuildInputDevice()
        {
            Platform = GetPlatform();
            EntryEngine.INPUT input = null;
            switch (Platform)
            {
                case EPlatform.Desktop:
                    input = new INPUT(new MouseUnity());
                    break;
                case EPlatform.Mobile:
                    input = new EntryEngine.INPUT(new TouchUnity());
                    break;
                case EPlatform.Console:
                    break;
                case EPlatform.VR:
                    break;
                case EPlatform.Unique:
                    break;
            }
            input.Keyboard = new KeyboardUnity();
            input.InputDevice = new InputTextUnity();
            return input;
        }

        public static EPlatform GetPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                case RuntimePlatform.BlackBerryPlayer:
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXDashboardPlayer:
                case RuntimePlatform.WP8Player:
                    return EPlatform.Mobile;
                case RuntimePlatform.OSXWebPlayer:
                case RuntimePlatform.WebGLPlayer:
                case RuntimePlatform.WindowsWebPlayer:
                    //return EPlatform.Web;
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.WSAPlayerARM:
                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerX86:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return EPlatform.Desktop;
                case RuntimePlatform.PS3:
                case RuntimePlatform.PS4:
                case RuntimePlatform.PSM:
                case RuntimePlatform.PSP2:
                case RuntimePlatform.SamsungTVPlayer:
                case RuntimePlatform.TizenPlayer:
                case RuntimePlatform.XBOX360:
                case RuntimePlatform.XboxOne:
                    return EPlatform.Console;
                default:
                    return EPlatform.Unique;
            }
        }
    }
	public class EntryUnity : Entry
	{
        protected override void Initialize(out AUDIO AUDIO, out ContentManager ContentManager, out FONT FONT, out GRAPHICS GRAPHICS, out INPUT INPUT, out _IO.iO iO, out IPlatform IPlatform, out TEXTURE TEXTURE)
        {
            _LOG._Logger = new LoggerUnity();

            iO = NewiO(null);

            FONT = NewFONT("黑体", 24);

            IPlatform = new PlatformUnity();
            if (Application.isEditor)
            {
                const string WritePath = "Content/";
                if (!System.IO.Directory.Exists(WritePath))
                    System.IO.Directory.CreateDirectory(WritePath);
                IOWWW.PersistentDataPath = WritePath;
                IOWWW.DataPath = "file://" + IOWWW.DataPath;
            }

            INPUT = ((PlatformUnity)IPlatform).BuildInputDevice();

            PipelinePiece.GetDefaultPipeline();
            ContentManager = NewContentManager();

            AUDIO = new MediaPlayerUnity();
            //AUDIO = new AudioEmpty();

            TEXTURE = null;

            GRAPHICS = new GraphicsUnityGL();
        }
        protected override _IO.iO InternalNewiO(string root)
        {
            //_IO.iO result = null;
            //switch (PlatformUnity.GetPlatform())
            //{
            //    case EPlatform.PC:
            //        result = base.NewiO();
            //        result.RootDirectory = Application.streamingAssetsPath + "/";
            //        break;
            //    case EPlatform.Phone:
            //        result = new IOWWW();
            //        result.RootDirectory = Application.streamingAssetsPath + "/";
            //        break;
            //    case EPlatform.掌机:
            //    case EPlatform.主机:
            //    case EPlatform.Unique:
            //        throw new NotSupportedException();
            //}
            //return result;
            var result = new IOWWW();
            result.RootDirectory = root;
            return result;
        }
        protected override ContentManager InternalNewContentManager()
        {
            ContentManager manager = new ContentManager(NewiO(null));
            manager.AddPipeline(new PipelineParticle());
            manager.AddPipeline(new PipelineAnimation());
            manager.AddPipeline(new PipelinePiece());
            manager.AddPipeline(new PipelinePatch());
            manager.AddPipeline(new PipelineFontStatic());
            manager.AddPipeline(new PipelineTextureUnity());
            manager.AddPipeline(new PipelineSound());
            return manager;
        }
        protected override FONT InternalNewFONT(string name, float fontSize)
        {
            return new FontDynamicUnity(name, (int)fontSize);
        }
        protected override TEXTURE InternalNewTEXTURE(int width, int height)
        {
            return new TextureUnity(new Texture2D(width, height));
        }

        public override void Exit()
        {
            Application.Quit();
        }
	}
}
