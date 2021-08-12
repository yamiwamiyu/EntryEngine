using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace EntryEngine.Unity
{
	using Input = UnityEngine.Input;
	using Random = UnityEngine.Random;
	using Time = UnityEngine.Time;
    using System.IO;
    using System.Reflection;
    using System.Text;

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
            string result = string.Format("[{0} {1}] {2}", record.Level, record.Time, record.ToString());
            switch (record.Level)
            {
                case 1: UnityEngine.Debug.Log(result); break;
                case 2: UnityEngine.Debug.LogWarning(result); break;
                case 3: UnityEngine.Debug.LogError(result); break;
                default: UnityEngine.Debug.Log(result); break;
            }
		}
	}
    internal enum EIOPathType
    {
        /// <summary>可写目录</summary>
        Write,
        /// <summary>只读目录</summary>
        Read,
        /// <summary>网络目录，包含file://类的本地目录</summary>
        Web,
    }
	public class IOUnity : _IO.iO
	{
        /// <summary>调用了Destroy的资源的对象ToString()的结果为"null"</summary>
        internal const string DESTROIED_RESOURCE = "null";
        /// <summary>可读可写文件夹</summary>
        internal static string PersistentDataPath = Application.persistentDataPath + '/';
        /// <summary>只读内部文件夹</summary>
        internal static string DataPath = Application.streamingAssetsPath + '/';

        /// <returns>返回是否应使用WWW</returns>
        internal EIOPathType GetReadPath(ref string file)
        {
            //file = file.Replace('\\', '/');
            string target = PersistentDataPath + file;
            try
            {
                if (System.IO.File.Exists(target))
                {
                    file = target;
                    return EIOPathType.Write;
                }
            }
            catch (Exception)
            {
            }
            file = DataPath + file;
            try
            {
                if (System.IO.File.Exists(file))
                {
                    return EIOPathType.Read;
                }
            }
            catch (Exception)
            {
            }
            return EIOPathType.Web;
        }
        private string GetWritePath(string file)
        {
            return PersistentDataPath + file.Replace('\\', '/');
        }
		protected override byte[] _ReadByte(string file)
		{
            if (GetReadPath(ref file) == EIOPathType.Web)
            {
                using (UnityWebRequest www = Load(false, file))
                {
                    return www.downloadHandler.data;
                }
            }
            else
                return base._ReadByte(file);
		}
		protected override AsyncReadFile _ReadAsync(string file)
		{
            string origin = file;
            if (GetReadPath(ref file) == EIOPathType.Web)
            {
                AsyncReadFile async = new AsyncReadFile(this, file);
                LoadAsync(false, file, request => 
                    {
                        if (string.IsNullOrEmpty(request.error))
                        {
                            try
                            {
                                async.SetData(request.downloadHandler.data);
                            }
                            catch (Exception ex)
                            {
                                async.Error(ex);
                            }
                        }
                        else
                        {
                            async.Error(new Exception(request.error));
                        }
                    });
                return async;
            }
            else
                return base._ReadAsync(origin);
		}
		protected override void _WriteByte(string file, byte[] content)
		{
            base._WriteByte(GetWritePath(file), content);
		}
        /// <summary>System.Threading.EventWaitHandle估计用不了</summary>
        [Code(ECode.BeNotTest)]
        internal UnityWebRequest Load(bool needGetReadPath, string file)
        {
            UnityWebRequest request = null;
            System.Threading.EventWaitHandle wait = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);
            EntryEngine.Network.AsyncThread.QueueUserWorkItem(() =>
            {
                if (needGetReadPath)
                {
                    GetReadPath(ref file);
                }
                request = UnityWebRequest.Get(file);
                var result = request.SendWebRequest();
                while (!result.isDone)
                {
                    System.Threading.Thread.Sleep(1);
                }
                wait.Set();
            });
            wait.WaitOne();
            return request;
        }
        internal void LoadAsync(bool needGetReadPath, string file, Action<UnityWebRequest> onCallback)
        {
            LoadAsync(needGetReadPath, file, f => UnityWebRequest.Get(f), onCallback);
        }
        internal void LoadAsync(bool needGetReadPath, string file, Func<string, UnityWebRequest> buildWebRequest, Action<UnityWebRequest> onCallback)
        {
            if (needGetReadPath)
            {
                if (GetReadPath(ref file) == EIOPathType.Write)
                {
                    // 本地文件，但又不得不用UnityWebRequest加载时
                    file = "file:///" + file;
                }
            }
            new AsyncUnityCoroutine().Load(Coroutine(buildWebRequest(file), onCallback));
        }
        private System.Collections.IEnumerator Coroutine(UnityWebRequest request, Action<UnityWebRequest> async)
        {
            using (request)
            {
                yield return request.SendWebRequest();
                if (!string.IsNullOrEmpty(request.error))
                {
                    _LOG.Warning("加载资源{0}失败：{1}", request.url, request.error);
                }
                async(request);
            }
        }
	}
	public class MediaPlayerUnity : AUDIO
	{
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
        public override IEnumerable<string> SuffixProcessable { get { return SOUND.FileTypes.Enumerable(); } }

        protected override Content Load(string file)
        {
            using (var request = ((IOUnity)IO).Load(true, file))
                return new SoundUnity(DownloadHandlerAudioClip.GetContent(request));
        }
        protected override void LoadAsync(AsyncLoadContent async)
        {
            DownloadHandlerAudioClip download = null;
            ((IOUnity)this.IO).LoadAsync(true, async.File,
                f =>
                {
                    download = new DownloadHandlerAudioClip(f, AudioType.UNKNOWN);
                    return new UnityWebRequest(f, UnityWebRequest.kHttpVerbGET, download, null);
                },
                request => async.SetData(new SoundUnity(download.audioClip)));
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
        public override IEnumerable<string> SuffixProcessable { get { return TEXTURE.TextureFileType.Enumerable(); } }

        public override Content LoadFromBytes(byte[] bytes)
        {
            Texture2D texture = new Texture2D(1, 1);
            if (!ImageConversion.LoadImage(texture, bytes, false))
                Resources.UnloadAsset(texture);
            return new Texture2DUnity(texture);
        }
	}

    public class CameraTexture : TextureUnity
    {
        static VECTOR2 RotateCenter = new VECTOR2(0.5f, 0.5f);

        WebCamTexture camera;

        public bool IsFront { get; private set; }
        public int DeviceIndex { get; private set; }
        public bool IsPlaying { get { return camera.isPlaying; } }
        public override int Width { get { return camera.requestedWidth; } }
        public override int Height { get { return camera.requestedHeight; } }
        public float FPS
        {
            get { return camera.requestedFPS; }
            set { camera.requestedFPS = value; }
        }
        public bool FrameRefreshed { get { return camera.didUpdateThisFrame; } }

        public CameraTexture(bool front, int index)
        {
            this.IsFront = front;
            this.DeviceIndex = index;

            var cameras = WebCamTexture.devices;
            for (int i = 0; i < cameras.Length; i++)
            {
                if (front == cameras[i].isFrontFacing)
                {
                    if (index == 0)
                    {
                        camera = new WebCamTexture(cameras[i].name, 640, 480, 10);
                        texture = camera;
                        break;
                    }
                    else
                    {
                        index--;
                    }
                }
            }
        }

        public void SetWidth(int width)
        {
            camera.requestedWidth = width;
        }
        public void SetHeight(int height)
        {
            camera.requestedHeight = height;
        }
        public void Play()
        {
            camera.Play();
        }
        public void Pause()
        {
            camera.Pause();
        }
        public void Stop()
        {
            camera.Stop();
        }

        public override COLOR[] GetData(RECT area)
        {
            bool rotate;
            return GetColor((int)area.X, (int)area.Y, (int)area.Width, (int)area.Height, out rotate).GetColor();
        }
        Color[] GetColor(int x, int y, int width, int height, out bool whChange)
        {
            whChange = false;
            Color[] colors = camera.GetPixels(x, Height - y - height, width, height);
            // Unity获得的摄像头图像是横竖颠倒的
            if (camera.videoRotationAngle != 0)
            {
                if (camera.videoRotationAngle == 90)
                {
                    Color[] temp = new Color[width * height];
                    int index1 = 0;
                    int index2;
                    for (int i = 0; i < height; i++)
                    {
                        index2 = (width - 1) * height + i;
                        for (int j = 0; j < width; j++)
                        {
                            temp[index2] = colors[index1];
                            index1++;
                            index2 -= height;
                        }
                    }
                    colors = temp;
                }
                else if (camera.videoRotationAngle == -90)
                {
                    Color[] temp = new Color[width * height];
                    int index1 = 0;
                    int index2;
                    for (int i = 0; i < height; i++)
                    {
                        index2 = width - 1 - i;
                        for (int j = 0; j < width; j++)
                        {
                            temp[index2] = colors[index1];
                            index1++;
                            index2 += height;
                        }
                    }
                    colors = temp;
                }
                else
                {
                    throw new NotImplementedException("videoRotationAngle:" + camera.videoRotationAngle);
                }

                {
                    // 颠倒宽高
                    whChange = true;
                    int temp = width;
                    width = height;
                    height = temp;
                }
            }
            if (camera.videoVerticallyMirrored)
            {
                Color temp;
                for (int i = 0; i < width; i++)
                {
                    int index1 = i;
                    int index2 = (height - 1) * width + i;
                    for (int j = height << 1; j >= 0; j--)
                    {
                        temp = colors[index1];
                        colors[index1] = colors[index2];
                        colors[index2] = temp;
                        index1 += width;
                        index2 -= width;
                    }
                }
            }
            return colors;
        }
        protected override bool Draw(GRAPHICS graphics, ref SpriteVertex vertex)
        {
            if (camera.videoVerticallyMirrored)
            {
                if ((vertex.Flip & EFlip.FlipVertically) == EFlip.None)
                    vertex.Flip |= EFlip.FlipVertically;
                else
                    vertex.Flip &= ~EFlip.FlipVertically;
            }
            if (camera.videoRotationAngle != 0)
            {
                float radian = _MATH.ToRadian(camera.videoRotationAngle);
                vertex.Rotation += radian;
                VECTOR2.Rotate(ref RotateCenter, ref vertex.Origin, -radian, out vertex.Origin);
            }
            return base.Draw(graphics, ref vertex);
        }
        public override void Save(string file)
        {
            bool rotate;
            Color[] colors = GetColor(0, 0, Width, Height, out rotate);
            Texture2D tex1;
            if (rotate)
                tex1 = new Texture2D(Height, Width);
            else
                tex1 = new Texture2D(Width, Height);
            tex1.SetPixels(colors);
            tex1.Apply();
            _IO.WriteByte(file, tex1.EncodeToPNG());
            tex1.Destroy();
        }
    }
    public abstract class TextureUnity : TEXTURE
    {
        protected internal Texture texture;
        public Texture Texture
        {
            get { return texture; }
        }
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
        protected override void InternalDispose()
        {
            if (texture != null)
            {
                texture.Destroy();
                texture = null;
            }
        }
    }
    public class Texture2DUnity : TextureUnity
    {
        public Texture2DUnity(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            this.texture = texture;
        }
        public override COLOR[] GetData(RECT area)
        {
            // 颜色从左下角开始取
            return ((Texture2D)texture).GetPixels((int)area.X, (int)(Height - area.Y - area.Height), (int)area.Width, (int)area.Height).GetColor();
        }
        public override void SetData(COLOR[] buffer, RECT area)
        {
            Texture2D tex1 = (Texture2D)texture;
            // 颜色从左下角开始取
            tex1.SetPixels32((int)area.X, (int)(Height - area.Y - area.Height), (int)area.Width, (int)area.Height, buffer.GetColor());
            tex1.Apply();
        }
        public override void Save(string file)
        {
            _IO.WriteByte(file, ((Texture2D)texture).EncodeToPNG());
        }
    }
    public class FontDynamicUnity : FontDynamic
    {
        // 一帧最多只绘制10个字，多了就异步
        private const int FRAME_DRAW_CHAR_COUNT = 10;
        private static int frameID;
        private static int drawCharCount;

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
            lineHeight = font.lineHeight + 1;
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
            return new Texture2DUnity(texture);
        }
        protected override void DrawChar(AsyncDrawDynamicChar async, char c, Buffer uv)
        {
            //if ('\r' == c)
            //{
            //    async.Cancel();
            //    return;
            //}

            Font font = cache.Font;

            CharacterInfo info;
            if (!font.GetCharacterInfo(c, out info))
            {
                font.RequestCharactersInTexture(c.ToString());
                font.GetCharacterInfo(c, out info);
            }

            uv.Space = (byte)info.advance;
            int width = info.glyphWidth;
            int height = info.glyphHeight;
            if (width > uv.W)
                uv.W = (byte)width;
            if (height > uv.H)
                uv.H = (byte)height;

            if (frameID != GameTime.Time.FrameID)
            {
                //if (drawCharCount != 0)
                //{
                //    _LOG.Debug("draw char count = {0}", drawCharCount);
                //}
                drawCharCount = 0;
                frameID = GameTime.Time.FrameID;
            }

            Texture2D graphics = font.material.mainTexture as Texture2D;
            int twidth = graphics.width;
            int theight = graphics.height;
            //_IO.WriteByte("TEXT.png", graphics.EncodeToPNG());
            // GetPixel矩形为屏幕坐标
            COLOR[] colors = graphics.GetPixels((int)(info.uv.x * twidth), (int)((info.uv.y + info.uv.height) * theight), (int)(info.uv.width * twidth), (int)(-info.uv.height * theight)).GetColor();
            float ascent = font.ascent;

            System.Threading.WaitCallback call = (_) =>
            {
                // 由于w,h不等于uv的宽高，导致文字变为非等宽字体，绘制时文字对不齐

                //int width = info.glyphWidth;
                //int height = info.glyphHeight;

                int uvwidth = _MATH.Ceiling(uv.W);
                int uvheight = _MATH.Ceiling(uv.H);

                int left = -(int)info.vert.x;
                if (left < 0) left = 0;
                if (uvwidth - width < left) left = uvwidth - width;
                int right = uvwidth - width - left;
                if (right < 0) right = 0;

                int count = uvwidth * uvheight;
                COLOR[] result = new COLOR[count];

                // 字体高度28，font.ascent=21，那么字模从下往上数7个像素为基准点绘制
                // 类似jpq这样下面到底的文字info.minY就可能为-7
                // 类似`这样的上面到顶的文字info.minY就可能是13，字模高度为8的话，正好28
                // 从colors上取正确的颜色到result里
                int startY = (int)(lineHeight - (lineHeight - ascent + info.minY + height));
                if (startY < 0)
                    startY = 0;
                int presult = left + startY * uvwidth;
                int tempStart = presult;

                int index = 0;
                if (info.flipped)
                {
                    // 图像顺时针旋转了90°
                    //for (int i = height - 1; i >= 0; i--)
                    //{
                    //    for (int j = 0; j < width; j++)
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = width - 1; j >= 0; j--)
                        {
                            index = j * height + i;
                            result[presult].R = colors[index].R;
                            result[presult].G = colors[index].G;
                            result[presult].B = colors[index].B;
                            result[presult].A = colors[index].A;
                            presult++;
                        }
                        presult += right + left;
                    }
                }
                else
                {
                    // 图像时上下镜像的
                    for (int i = height - 1; i >= 0; i--)
                    {
                        index = i * width;
                        for (int j = 0; j < width; j++)
                        {
                            result[presult].R = colors[index].R;
                            result[presult].G = colors[index].G;
                            result[presult].B = colors[index].B;
                            result[presult].A = colors[index].A;
                            presult++;
                            index++;
                        }
                        presult += right + left;
                    }
                }
                //try
                //{

                //}
                //catch (IndexOutOfRangeException ex)
                //{
                //    _LOG.Error("越界 c:{6} flip:{7} presult:{0} index:{1} u:{2} v:{3} y:{4} h:{5} lineHeight:{8} start:{9} left:{10} right:{11} ascent:{12} uw:{13} uh:{14}", presult, index, uvwidth, uvheight, info.vert.y, info.vert.height, c, info.flipped, lineHeight, tempStart, left, right, font.ascent, info.uv.width * twidth, info.uv.height * theight);
                //}

                //_LOG.Debug(c.ToString());
                if (_ == this)
                    Entry.Instance.Synchronize(() => async.SetData(result));
                else
                    async.SetData(result);
                //Entry.Instance.Synchronize(() => cache.Textures.First().Save(c + ".png"));
            };

            if (drawCharCount < FRAME_DRAW_CHAR_COUNT)
            {
                // 同步绘制
                call(null);
            }
            else
            {
                // 异步处理
                System.Threading.ThreadPool.QueueUserWorkItem(call, this);
            }
            drawCharCount++;
        }
        protected override void InternalDispose()
        {
            base.InternalDispose();
            if (cache.Font != null)
                cache.Font.Destroy();
        }
        public override Content Cache()
        {
            FontDynamicUnity font = new FontDynamicUnity(cache.Font);
            font._Key = this._Key;
            return font;
        }
    }
    public class FontUnity : FONT
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

        Font font;
        float scale;

        public override float FontSize
        {
            get { return font.fontSize * scale; }
            set
            {
                int intSize = (int)value;
                scale = value - intSize;
                if (font.fontSize != intSize)
                    font = Font.CreateDynamicFontFromOSFont(font.fontNames, intSize);
            }
        }
        public override float LineHeight
        {
            get { return font.lineHeight; }
        }
        public override bool IsDisposed
        {
            get { return false; }
        }
        public override bool IsDynamic { get { return true; } }

        public FontUnity(string name, float size)
        {
            int intSize = (int)size;
            if (size != intSize)
            {
                scale = size - intSize;
            }
            font = Font.CreateDynamicFontFromOSFont(name, intSize);
        }

        protected override float CharWidth(char c)
        {
            style.font = font;
            content.text = c.ToString();
            return style.CalcSize(content).x;
        }

        protected override void Draw(GRAPHICS spriteBatch, string text, float x, float y, COLOR color, float scale)
        {
            style.font = font;
            style.normal.textColor = color.GetColor();
            content.text = text;
            //var matrix = GUI.matrix;
            //GUI.matrix = matrix * Matrix4x4.Scale(new Vector3(scale, scale, 1));
            style.Draw(new Rect(x, y, 2048, 2048), content, false, false, false, false);
            //GUI.matrix = matrix;
        }
        protected override void InternalDispose()
        {
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
        public override int PassCount
        {
            get { return material.passCount; }
        }
        public ShaderUnity() { }
        public ShaderUnity(UnityEngine.Shader shader)
        {
            material = new Material(shader);
        }
        public ShaderUnity(Material material)
        {
            this.material = material;
        }
        public override bool IsDisposed
        {
            get { return disposed; }
        }
        protected override void InternalDispose()
        {
            disposed = true;
            if (material != null)
            {
                Resources.UnloadAsset(material);
                material = null;
            }
        }
        public override bool HasProperty(string name)
        {
            return material.HasProperty(name);
        }
        public override void SetValue(string property, bool value)
        {
            material.SetInt(property, value ? 1 : 0);
        }
        public override void SetValue(string property, float value)
        {
            material.SetFloat(property, value);
        }
        public override void SetValue(string property, int value)
        {
            material.SetInt(property, value);
        }
        public override void SetValue(string property, MATRIX value)
        {
            material.SetMatrix(property, value.GetMatrix());
        }
        public override void SetValue(string property, TEXTURE value)
        {
            material.SetTexture(property, value.GetTexture());
        }
        public override void SetValue(string property, VECTOR2 value)
        {
            material.SetVector(property, new Vector4(value.X, value.Y, 0, 0));
        }
        public override void SetValue(string property, VECTOR3 value)
        {
            material.SetVector(property, new Vector4(value.X, value.Y, value.Z, 0));
        }
        public override void SetValue(string property, VECTOR4 value)
        {
            material.SetVector(property, value.GetVector4());
        }
        public SHADER Clone()
        {
            ShaderUnity clone = new ShaderUnity();
            clone.material = new Material(this.material);
            return clone;
        }

        protected override void InternalBegin(GRAPHICS g)
        {
            material.SetPass(CurrentPass);
        }
        protected override void InternalEnd(GRAPHICS g)
        {
        }
        public override void SetValue(string property, bool[] value)
        {
            float[] array = new float[value.Length];
            for (int i = 0; i < array.Length; i++)
                array[i] = value[i] ? 1 : 0;
            material.SetFloatArray(property, array);
        }
        public override void SetValue(string property, float[] value)
        {
            material.SetFloatArray(property, value);
        }
        public override void SetValue(string property, int[] value)
        {
            float[] array = new float[value.Length];
            for (int i = 0; i < array.Length; i++)
                array[i] = value[i];
            material.SetFloatArray(property, array);
        }
        public override void SetValue(string property, MATRIX[] value)
        {
            Matrix4x4[] array = new Matrix4x4[value.Length];
            for (int i = 0; i < array.Length; i++)
                array[i] = value[i].GetMatrix();
            material.SetMatrixArray(property, array);
        }
        public override void SetValue(string property, VECTOR2[] value)
        {
            Vector4[] array = new Vector4[value.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i].x = value[i].X;
                array[i].y = value[i].Y;
            }
            material.SetVectorArray(property, array);
        }
        public override void SetValue(string property, VECTOR3[] value)
        {
            Vector4[] array = new Vector4[value.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i].x = value[i].X;
                array[i].y = value[i].Y;
                array[i].z = value[i].Z;
            }
            material.SetVectorArray(property, array);
        }
        public override void SetValue(string property, VECTOR4[] value)
        {
            Vector4[] array = new Vector4[value.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i].x = value[i].X;
                array[i].y = value[i].Y;
                array[i].z = value[i].Z;
                array[i].w = value[i].W;
            }
            material.SetVectorArray(property, array);
        }
    }
    public class PipelineShaderUnity : PipelineShader
    {
        public override Content LoadFromText(string text)
        {
            return new ShaderUnity(new Material(text));
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
            get { return AudioClip == null || AudioClip.ToString() == IOUnity.DESTROIED_RESOURCE; }
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
        //private MATRIX2x3 modelview;

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

        protected override void SetViewport(ref MATRIX2x3 view, ref RECT graphicsViewport)
        {
            this.View = MATRIX2x3.Identity;
        }
        protected override void InternalBegin(bool threeD, ref MATRIX matrix, ref RECT graphics, SHADER shader)
        {
            RECT gscreen = AreaToScreen(graphics);
            Rect scissor = gscreen.ToCartesian();
            if (shader == null)
            {
                if (threeD)
                {
                    VECTOR2 temp = GraphicsSize;
                    matrix.M31 /= temp.X;
                    matrix.M32 /= temp.Y;
                    var modelview =
                        (MATRIX2x3)matrix * MATRIX2x3.Invert(View)
                        * MATRIX2x3.CreateTranslation(-graphics.X / temp.X, -graphics.Y / temp.Y)
                        * MATRIX2x3.CreateScale(temp.X / graphics.Width, temp.Y / graphics.Height)
                        ;
                    GL.LoadProjectionMatrix(matrix.GetMatrix());
                    //GL.modelview = ((matrix * (MATRIX)(MATRIX2x3.Invert(View)
                    //    * MATRIX2x3.CreateTranslation(-graphics.X / temp.X, -graphics.Y / temp.Y)
                    //    * MATRIX2x3.CreateScale(temp.X / graphics.Width, temp.Y / graphics.Height))).GetMatrix());
                }
                else
                {
                    GL.LoadIdentity();
                    // 将像素坐标从左下角0~1设置为左上角宽0~width,高0~height
                    MATRIX2x3 view =
                        // 1280, 0
                        (MATRIX2x3)matrix *
                        // 画布坐标转换到屏幕坐标
                        graphicsToScreen *
                        // 屏幕坐标转换到视口坐标
                        MATRIX2x3.CreateTranslation(-gscreen.X, -gscreen.Y) * MATRIX2x3.CreateScale(Screen.width / gscreen.Width, Screen.height / gscreen.Height)
                        ;
                    GL.modelview = view.GetMatrix();
                }
            }
            else
            {
                shader.Begin(this);
            }
            // 屏幕左下角0,0
            GL.Viewport(scissor);
        }
        protected override void InternalDrawPrimitivesBegin(TEXTURE texture, EPrimitiveType ptype, int textureIndex)
        {
            if (texture != null)
            {
                UnityGate.Gate.GLMaterial.mainTexture = texture.GetTexture();
                UnityGate.Gate.GLMaterial.SetPass(0);
            }

            if (ptype == EPrimitiveType.Point) throw new NotImplementedException();
            else if (ptype == EPrimitiveType.Line) GL.Begin(GL.LINES);
            else GL.Begin(GL.TRIANGLES);
        }
        protected override void InternalDrawPrimitives(EPrimitiveType ptype, TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount)
        {
            if (ptype == EPrimitiveType.Triangle)
            {
                int idx = indexOffset;
                for (int c = 0; c < primitiveCount; c++)
                {
                    DrawPrimitive(ref vertices[offset + indexes[idx++]]);
                    DrawPrimitive(ref vertices[offset + indexes[idx++]]);
                    DrawPrimitive(ref vertices[offset + indexes[idx++]]);
                }
            }
            else if (ptype == EPrimitiveType.Line)
            {
                int idx = indexOffset;
                for (int c = 0; c < primitiveCount; c++)
                {
                    DrawPrimitive(ref vertices[offset + indexes[idx++]]);
                    DrawPrimitive(ref vertices[offset + indexes[idx++]]);
                }
            }
            else
            {
                throw new NotImplementedException();
                //for (int c = 0; c < primitiveCount; c++)
                //{
                //    DrawPrimitive(ref vertices[offset + indices[indexOffset + c]]);
                //}
            }
        }
        private void DrawPrimitive(ref TextureVertex vertex)
        {
            GL.Color(vertex.Color.GetColor());
            GL.TexCoord2(vertex.UV.X, 1 - vertex.UV.Y);
            GL.Vertex3(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
        }
        public override void DrawPrimitivesEnd()
        {
            GL.End();
        }
        public override TEXTURE Screenshot(RECT graphics)
        {
            Rect rect = AreaToScreen(graphics).ToCartesian();
            Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();
            return new Texture2DUnity(texture);
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
                //if (keyboard.status == TouchScreenKeyboard.Status.Canceled)
                //    return EInput.Canceled;
                //else if (keyboard.status == TouchScreenKeyboard.Status.Done)
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
                case RuntimePlatform.IPhonePlayer:
                    return EPlatform.Mobile;
                case RuntimePlatform.WebGLPlayer:
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
                case RuntimePlatform.PS4:
                case RuntimePlatform.PSP2:
                case RuntimePlatform.TizenPlayer:
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
            _LOG._Logger = new LoggerFile();

            iO = NewiO(null);

            FONT = NewFONT("黑体", 24);

            IPlatform = new PlatformUnity();
            if (Application.isEditor)
                IOUnity.PersistentDataPath = IOUnity.DataPath;
            _LOG.Debug("可读可写文件夹：{0}", IOUnity.PersistentDataPath);
            _LOG.Debug("只读内部文件夹：{0}", IOUnity.DataPath);

            INPUT = ((PlatformUnity)IPlatform).BuildInputDevice();

            PipelinePiece.GetDefaultPipeline();
            ContentManager = NewContentManager();

            AUDIO = new MediaPlayerUnity();
            //AUDIO = new AudioEmpty();

            TEXTURE = null;

            GRAPHICS = new GraphicsUnityGL();

            // 可以让手机在不操作时不进入休眠状态
            //Screen.sleepTimeout = SleepTimeout.NeverSleep;
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
            var result = new IOUnity();
            result.RootDirectory = root;
            return result;
        }
        protected override ContentManager InternalNewContentManager()
        {
            ContentManager manager = new ContentManager(NewiO(null));
            manager.AddPipeline(new PipelinePicture());
            manager.AddPipeline(new PipelineTile());
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
            return new Texture2DUnity(new Texture2D(width, height));
        }

        public override void Exit()
        {
            Application.Quit();
        }
	}
}
