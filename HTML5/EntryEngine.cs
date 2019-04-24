/*
 * 对EntryEngine的实现
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;
using System.Net;
using System.IO;

namespace EntryEngine.HTML5
{
    public class Logger : _LOG.Logger
    {
        public override void Log(ref Record record)
        {
            console.log(string.Format("[{0}] {1}", record.Time.ToString("yyyy-MM-dd HH:mm:ss"), record.ToString()));
        }
    }

#if WX
    public class IOJSLocal : _IO.iO
    {
        protected override string _ReadText(string file)
        {
            object data = wx.getStorageSync(file);
            if (data == null) return null;
            else return data.ToString();
        }
        protected override byte[] _ReadByte(string file)
        {
            string text = _ReadText(file);
            if (text == null) return null;
            return SingleEncoding.Single.GetBytes(text);
        }
        protected override void _WriteText(string file, string content, Encoding encoding)
        {
            // 可能由重写IO或File.WriteAllText完成对LocalStorage内写入文件
            wx.setStorageSync(file, content);
        }
        protected override void _WriteByte(string file, byte[] content)
        {
            wx.setStorageSync(file, SingleEncoding.Single.GetString(content));
        }
    }
    public class IOJSWeb : IOJSLocal
    {
        /* 文件请求方式加载图片
         * var req = new XMLHttpRequest();
            req.responseType = "blob";
            req.onreadystatechange = () =>
            {
                if (req.readyState == 4)
                {
                    if (req.status == 200)
                    {
                        console.log("load succuss");

                        var response = req.response;  
                        var data = new Image();

                        data.onload = function()
                        {
                            texture = gl.createTexture();
                            gl.bindTexture(gl.TEXTURE_2D, texture);
                            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
                            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
                            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
                            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
                            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, data);
                            gl.bindTexture(gl.TEXTURE_2D, null);
                        }
                        data.src = window.URL.createObjectURL(response);
                    }
                    else
                    {
                        console.log("error:" + req.status);
                    }
                }
            };
            req.open("GET", "imgfile.png", true);
            req.send();
         */
        internal static string BuildNetUrl(string url)
        {
            if (string.IsNullOrEmpty(HTML5Gate.PATH))
                return url;
            return HTML5Gate.PATH + url;
        }
        protected override AsyncReadFile _ReadAsync(string file)
        {
            AsyncReadFile async = new AsyncReadFile(this, file);

            byte[] data = base._ReadByte(file);
            if (data != null)
            {
                async.SetData(data);
            }
            else
            {
                RequestObject param = new RequestObject();
                param.url = BuildNetUrl(file);
                param.success = (res) =>
                {
                    if (res.statusCode == 200)
                    {
                        async.SetData(SingleEncoding.UTF8.GetBytes(res.data.ToString()));
                    }
                    else
                    {
                        async.Error(new HttpException((HttpStatusCode)res.statusCode));
                    }
                };
                wx.request(param);
            }

            return async;
        }
        protected sealed override string _ReadText(string file)
        {
            throw new NotImplementedException();
        }
        protected sealed override byte[] _ReadByte(string file)
        {
            throw new NotImplementedException();
        }
    }
#else
    public class IOJSLocal : _IO.iO
    {
        protected override string _ReadText(string file)
        {
            return window.localStorage.getItem(file);
        }
        protected override byte[] _ReadByte(string file)
        {
            string text = window.localStorage.getItem(file);
            if (text == null) return null;
            return SingleEncoding.Single.GetBytes(text);
        }
        protected override void _WriteText(string file, string content, Encoding encoding)
        {
            // 可能由重写IO或File.WriteAllText完成对LocalStorage内写入文件
            window.localStorage.setItem(file, content);
        }
        protected override void _WriteByte(string file, byte[] content)
        {
            window.localStorage.setItem(file, SingleEncoding.Single.GetString(content));
        }

        private static HTMLFileSelector fileSelector;
        public override void FileBrowser(string[] suffix, bool multiple, Action<SelectFile[]> onSelect)
        {
            // 弹出文件选择框会阻塞导致激活不了鼠标键盘等事件
            MouseJS.state.left = false;
            TouchJS.TouchEvent = null;
            KeyboardJS.Pressed.Clear();
            if (fileSelector == null)
            {
                fileSelector = (HTMLFileSelector)window.document.getElementById("file");
            }
            fileSelector.onchange = () =>
            {
                int len = fileSelector.files.length;
                if (len != 0)
                {
                    //FileReader fileReader = new FileReader();
                    //byte[][] selectes = new byte[len][];
                    //int index = 0;
                    //fileReader.onloadend = () =>
                    //{
                    //    Uint8Array array = (Uint8Array)fileReader.result;
                    //    byte[] bytes = new byte[array.byteLength];
                    //    for (int i = 0; i < array.byteLength; i++)
                    //        bytes[i] = array[i];
                    //    selectes[index] = bytes;
                    //    index++;
                    //    if (index != len)
                    //        fileReader.readAsArrayBuffer(fileSelector.files[index]);
                    //    else
                    //        onSelect(selectes);
                    //};
                    //fileReader.readAsArrayBuffer(fileSelector.files[index]);

                    SelectFile[] files = new SelectFile[len];
                    for (int i = 0; i < files.Length; i++)
                        files[i] = CreateSelectFile(fileSelector.files[i].name, fileSelector.files[i]);
                    onSelect(files);
                }
            };
            fileSelector.click();
        }
        protected override byte[] ReadSelectFile(SelectFile select)
        {
            byte[] result = null;
            FileReader fileReader = new FileReader();
            fileReader.onloadend = () =>
            {
                Uint8Array array = (Uint8Array)fileReader.result;
                result = new byte[array.byteLength];
                for (int i = 0; i < array.byteLength; i++)
                    result[i] = array[i];
            };
            fileReader.readAsArrayBuffer((Blob)select.SelectObject);
            return result;
        }
    }
    public class IOJSWeb : IOJSLocal
    {
        /* 文件请求方式加载图片
         * var req = new XMLHttpRequest();
            req.responseType = "blob";
            req.onreadystatechange = () =>
            {
                if (req.readyState == 4)
                {
                    if (req.status == 200)
                    {
                        console.log("load succuss");

                        var response = req.response;  
                        var data = new Image();

                        data.onload = function()
                        {
                            texture = gl.createTexture();
                            gl.bindTexture(gl.TEXTURE_2D, texture);
                            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
                            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
                            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
                            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
                            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, data);
                            gl.bindTexture(gl.TEXTURE_2D, null);
                        }
                        data.src = window.URL.createObjectURL(response);
                    }
                    else
                    {
                        console.log("error:" + req.status);
                    }
                }
            };
            req.open("GET", "imgfile.png", true);
            req.send();
         */
        internal static string BuildNetUrl(string url)
        {
            if (string.IsNullOrEmpty(HTML5Gate.PATH))
                return url;
            return HTML5Gate.PATH + url;
        }
        protected override AsyncReadFile _ReadAsync(string file)
        {
            AsyncReadFile async = new AsyncReadFile(this, file);

            byte[] data = base._ReadByte(file);
            if (data != null)
            {
                async.SetData(data);
            }
            else
            {
                var req = new XMLHttpRequest();
                req.onreadystatechange = () =>
                {
                    if (req.readyState == 4)
                    {
                        if (req.status == 200)
                        {
                            async.SetData(SingleEncoding.UTF8.GetBytes(req.responseText));
                        }
                        else
                        {
                            async.Error(new HttpException((HttpStatusCode)req.status));
                        }
                    }
                };
                req.open("GET", BuildNetUrl(file), true);
                req.responseType = "text";
                req.send();
            }

            return async;
        }
        protected sealed override string _ReadText(string file)
        {
            string data = base._ReadText(file);
            if (data != null) return data;
            // 网络加载
            var req = new XMLHttpRequest();
            //req.responseType = "text";
            req.open("GET", BuildNetUrl(file), false);
            req.send();
            if (req.readyState == 4 && req.status == 200)
            {
                if (IOEncoding == Encoding.UTF8)
                {
                    return req.responseText;
                }
                else
                {
                    return IOEncoding.GetString(Encoding.UTF8.GetBytes(req.responseText));
                }
            }
            else
            {
                throw new HttpException((HttpStatusCode)req.status);
            }
        }
        protected sealed override byte[] _ReadByte(string file)
        {
            byte[] data = base._ReadByte(file);
            if (data != null) return data;
            // 网络加载
            var req = new XMLHttpRequest();
            //req.responseType = "text";
            req.open("GET", BuildNetUrl(file), false);
            req.send();
            if (req.readyState == 4 && req.status == 200)
            {
                return Encoding.UTF8.GetBytes(req.responseText);
            }
            else
            {
                throw new HttpException((HttpStatusCode)req.status);
            }
        }
    }
#endif
    public class HttpException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }
        public HttpException(HttpStatusCode code)
        {
            this.StatusCode = code;
        }
    }

    public class MouseStateJS : IMouseState
    {
        internal VECTOR2 position;
        internal float scrollWheelValue;
        internal bool left;
        internal bool right;
        internal bool middle;

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
            set { position = value; }
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
        internal void CopyTo(MouseStateJS target)
        {
            target.position.X = this.position.X;
            target.position.Y = this.position.Y;
            target.scrollWheelValue = this.scrollWheelValue;
            target.left = this.left;
            target.right = this.right;
            target.middle = this.middle;
        }
    }
    public class MouseJS : MOUSE
    {
        internal static MouseStateJS state = new MouseStateJS();
        internal static MouseStateJS previous = new MouseStateJS();
        internal static MouseStateJS previous2 = new MouseStateJS();
        private static bool alternate;
        protected override IMouseState GetState()
        {
            alternate = !alternate;
            if (alternate)
            {
                state.CopyTo(previous2);
                return previous2;
            }
            else
            {
                state.CopyTo(previous);
                return previous;
            }
        }
    }
    public class TouchStateJS : ITouchState
    {
        private VECTOR2 position;
        private float radius;
        public float Pressure
        {
            get { return radius; }
        }
        public VECTOR2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public TouchStateJS(window.TouchData touch)
        {
            position.X = touch.clientX;
            position.Y = touch.clientY;
            radius = touch.radiusX * touch.radiusX + touch.radiusY * touch.radiusY;
        }
        public bool IsClick(int key)
        {
            return true;
        }
    }
    public class TouchJS : TOUCH
    {
        internal static window.TouchEvent TouchEvent;
        protected override int GetTouches(ITouchState[] states)
        {
            if (TouchEvent == null) return 0;

            int current = _MATH.Min(TouchEvent.touches.length, states.Length);
            for (int i = 0; i < current; i++)
                states[i] = new TouchStateJS((window.TouchData)TouchEvent.touches[i]);
            return current;
        }
    }
    public class KeyboardStateJS : IKeyboardState
    {
        internal int[] pressedKeys;

        public bool HasPressedAnyKey
        {
            get { return pressedKeys.Length > 0; }
        }
        public int[] GetPressedKey()
        {
            return pressedKeys;
        }
        public bool IsClick(int key)
        {
            for (int i = 0; i < pressedKeys.Length; i++)
                if (pressedKeys[i] == key)
                    return true;
            return false;
        }
    }
    public class KeyboardJS : KEYBOARD
    {
        internal static List<int> Pressed = new List<int>(8);
        protected override IKeyboardState GetState()
        {
            KeyboardStateJS state = new KeyboardStateJS();
            state.pressedKeys = Pressed.ToArray();
            return state;
        }
    }
    public class InputTextJS : InputText
    {
        internal static bool IME;
        internal static window.KeyboardEvent _229;
        static HTMLElement inputElement;
        static void PrepareInputElement()
        {
            if (inputElement == null)
                inputElement = window.document.getElementById("__input");
            //inputElement.oninput = OnInput;
            inputElement.onblur = OnBlur;
        }
        //static void OnInput()
        //{
        //}
        static void OnBlur()
        {
            // 防止鼠标框选文字时打断输入层的焦点
            if (Entry._INPUT.InputDevice.IsActive)
                inputElement.focus();
        }

        string imeText;
        public override bool ImmCapturing
        {
            get { return IME; }
        }

        protected override InputText.EInput InputCapture(out string text)
        {
            PrepareInputElement();
            bool __ime = IME;
            IME = false;
            //if (Entry._IPlatform.Platform == EPlatform.Desktop)
            //{
            //    if (!string.IsNullOrEmpty(inputElement.innerText))
            //    {
            //        if (__ime)
            //        {
            //            imeText = inputElement.innerText;
            //            inputElement.innerText = string.Empty;
            //            text = string.Empty;
            //            return EInput.Input;
            //        }
            //        else
            //        {
            //            // BUG: 最后空格或123确认输入的文字时那次案件还是属于IME状态
            //            imeText = null;
            //            text = inputElement.innerText;
            //            inputElement.innerText = string.Empty;
            //            return EInput.Input;
            //        }
            //    }
            //    else
            //    {
            //        text = string.Empty;
            //        return EInput.Input;
            //    }
            //}
            //else
            {
                text = inputElement.innerText;
                return EInput.Replace;
            }
        }
        protected override void OnActive(ITypist typist)
        {
            OnStop(typist);
            PrepareInputElement();
            //inputElement.contenteditable = true;
            inputElement.innerText = typist.Text;
            inputElement.focus();
        }
        protected override void OnStop(ITypist typist)
        {
            PrepareInputElement();
            //element.contenteditable = false;
            inputElement.blur();
            inputElement.innerText = "";
        }
    }

    public abstract class TextureJS : TEXTURE
    {
        internal WebGLTexture Texture;
        public override bool IsDisposed
        {
            get { return Texture == null; }
        }
        protected override void InternalDispose()
        {
            if (Texture != null)
            {
                GraphicsWebGL.gl.deleteTexture(Texture);
                Texture = null;
            }
        }
    }
    #if WX
#else

#endif
    public class TextureJSData : TextureJS
    {
        internal ImageData Data;
        public override int Width
        {
            get { return Data.width; }
        }
        public override int Height
        {
            get { return Data.height; }
        }
        public TextureJSData(int width, int height)
        {
            Data = new ImageData(width, height);
            Texture = GraphicsWebGL.CreateGLTexture(Data);
        }
        public override COLOR[] GetData(RECT area)
        {
            int x = (int)area.X;
            int y = (int)area.Y;
            int w = (int)area.Width;
            int h = (int)area.Height;
            return COLOR.Convert(Data.data.GetBytes()).GetArray(x, y, w, h, Data.width);
        }
        public override void SetData(COLOR[] buffer, RECT area)
        {
            byte[] bytes = Data.data.GetBytes();
            Utility.SetArray(COLOR.Convert(buffer), bytes, (int)(area.X * 4), (int)area.Y, (int)(area.Width * 4), (int)area.Height, Data.width * 4, (int)(area.Width * 4), 0);
            bytes.ToUint8ClampedArray(Data.data);
            GraphicsWebGL.SetTextureData(Texture, Data);
        }
        protected override void InternalDispose()
        {
            base.InternalDispose();
            Data = null;
        }
    }
    public class TextureJSGL : TextureJS
    {
        internal Image Image;
        public override int Width
        {
            get { return Image.width; }
        }
        public override int Height
        {
            get { return Image.height; }
        }
        internal TextureJSGL() { }
        public TextureJSGL(Image image, WebGLTexture texture)
        {
            this.Image = image;
            this.Texture = texture;
        }
        public override COLOR[] GetData(RECT area)
        {
            var data = GraphicsCanvas.DrawImage(Image, (int)area.X, (int)area.Y, (int)area.Width, (int)area.Height);
            return COLOR.Convert(data.data.GetBytes());
        }
        //public override void SetData(COLOR[] buffer, RECT area)
        //{
        //    int width = Image.width;
        //    int height = Image.height;
        //    // 绘制全图
        //    var data = GraphicsCanvas.DrawImage(Image, 0, 0, width, height);
        //    // 释放之前的图片资源
        //    Image.src = null;
        //    // 替换颜色并构造新图
        //}
        protected override void InternalDispose()
        {
            base.InternalDispose();
            if (Image != null)
            {
                //Image.src = "";
                Image = null;
            }
        }
        public static void FromBase64(string code, Action<TextureJSGL> onLoad)
        {
            Image image = new Image();
            image.onload = () =>
            {
                if (onLoad != null)
                    onLoad(GraphicsWebGL.CreateTexture(image));
            };
            image.src = code;
        }
    }
    public class PipelineTextureJSGL : ContentPipeline
    {
        public override IEnumerable<string> SuffixProcessable
        {
            get { return TEXTURE.TextureFileType; }
        }
        protected override Content Load(string file)
        {
            TEXTURE_DELAY delay = new TEXTURE_DELAY();

            // 延迟完成同步加载
            AsyncData<Content> async = new AsyncData<Content>();
            delay.Async = async;

            TextureJSGL result = new TextureJSGL();

            Image img = new Image();
            img.crossOrigin = "";
            img.onload = () =>
            {
                var texture = GraphicsWebGL.CreateGLTexture(img);
                result.Image = img;
                result.Texture = texture;
                delay.Base = result;
            };
            img.src = IOJSWeb.BuildNetUrl(IO.BuildPath(file));

            return result;
        }
        protected override void LoadAsync(AsyncLoadContent async)
        {
            Image img = new Image();
            img.crossOrigin = "";
            img.onload = () =>
            {
                async.SetData(GraphicsWebGL.CreateTexture(img));
            };
            img.src = IOJSWeb.BuildNetUrl(IO.BuildPath(async.File));
        }
    }
    public class ShaderJSGL : SHADER
    {
        public static ShaderJSGL DefaultVertexShader
        {
            get { throw new NotImplementedException(); }
        }
        public static ShaderJSGL DefaultPixelShader
        {
            get { throw new NotImplementedException(); }
        }

        public int PassCount
        {
            get { throw new NotImplementedException(); }
        }
        public void LoadFromCode(string code)
        {
            throw new NotImplementedException();
        }
        public bool SetPass(int pass)
        {
            throw new NotImplementedException();
        }
        public bool HasProperty(string name)
        {
            throw new NotImplementedException();
        }
        public bool GetValueBoolean(string property)
        {
            throw new NotImplementedException();
        }
        public int GetValueInt32(string property)
        {
            throw new NotImplementedException();
        }
        public MATRIX GetValueMatrix(string property)
        {
            throw new NotImplementedException();
        }
        public float GetValueSingle(string property)
        {
            throw new NotImplementedException();
        }
        public TEXTURE GetValueTexture(string property)
        {
            throw new NotImplementedException();
        }
        public VECTOR2 GetValueVector2(string property)
        {
            throw new NotImplementedException();
        }
        public VECTOR3 GetValueVector3(string property)
        {
            throw new NotImplementedException();
        }
        public VECTOR4 GetValueVector4(string property)
        {
            throw new NotImplementedException();
        }
        public void SetValue(string property, bool value)
        {
            throw new NotImplementedException();
        }
        public void SetValue(string property, float value)
        {
            throw new NotImplementedException();
        }
        public void SetValue(string property, int value)
        {
            throw new NotImplementedException();
        }
        public void SetValue(string property, MATRIX value)
        {
            throw new NotImplementedException();
        }
        public void SetValue(string property, TEXTURE value)
        {
            throw new NotImplementedException();
        }
        public void SetValue(string property, VECTOR2 value)
        {
            throw new NotImplementedException();
        }
        public void SetValue(string property, VECTOR3 value)
        {
            throw new NotImplementedException();
        }
        public void SetValue(string property, VECTOR4 value)
        {
            throw new NotImplementedException();
        }
    }
    public class PipelineShaderJSGL : ContentPipelineBinary
    {
        public override IEnumerable<string> SuffixProcessable
        {
            get { return null; }
        }
        public override Content LoadFromBytes(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
    public class FontDynamicJSGL : FontDynamic
    {
        private string fontName;

        public FontDynamicJSGL(string name, float size) : base(GetCacheID(name, size))
        {
            this.fontName = name;
            this.fontSize = size;
            this.lineHeight = fontSize;
        }

        private void SetFont()
        {
            GraphicsCanvas.TextGraphics.ctx.font = GetFontStyle();
        }
        private string GetFontStyle()
        {
            //return string.Format("{0}px {1}", (int)this.fontSize, fontName);
            // 生成min.js时px后面的空格会被省略掉
            return (int)this.fontSize + "px " + fontName;
        }
        //protected override VECTOR2 MeasureBufferSize(char c)
        //{
        //    SetFont();
        //    return new VECTOR2(GraphicsCanvas.TextGraphics.ctx.measureText(c.ToString()).width, FontSize);
        //}
        protected override FontDynamic OnSizeChanged(float fontSize)
        {
            FontDynamic result = GetCache(GetCacheID(fontName, fontSize));
            if (result == null)
            {
                result = new FontDynamicJSGL(fontName, fontSize);
                result.FontSize = fontSize;
            }
            return result;
        }
        protected override void DrawChar(AsyncDrawDynamicChar result, char c, Buffer uv)
        {
            result.SetData(COLOR.Convert(GraphicsCanvas.DrawText(GetFontStyle(), c.ToString(), uv.W, uv.H).data.GetBytes()));
        }
        protected override void CopyTo(FontTexture target)
        {
            base.CopyTo(target);
            ((FontDynamicJSGL)target).fontName = this.fontName;
        }
        protected override Content Cache()
        {
            return new FontDynamicJSGL(this.fontName, this.FontSize);
        }
    }

    public class GraphicsWebGL : GRAPHICS
    {
        internal static WebGLRenderingContext gl;
        internal static WebGLShader defaultVertexShader;
        internal static WebGLShader defaultPixelShader;
        private static WebGLProgram shaderProgram;
        private static WebGLUniformLocation vertexShaderMatrix;
        private static float[] vertexShaderMatrixArray;
        private static Float32Array arrayBuffer = new Float32Array(9 * 4 * 1);
        private static WebGLBuffer verticesBuffer;
        private static WebGLBuffer indicesBuffer;
        private static short[] indicesBufferArray;
        static int[] PrimitiveTypes;

        public static WebGLTexture CreateGLTexture(Image image)
        {
            if (image == null || !image.complete)
                throw new InvalidOperationException();
            var texture = gl.createTexture();
            gl.bindTexture(gl.TEXTURE_2D, texture);
            gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, 1);				
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
            gl.bindTexture(gl.TEXTURE_2D, null);
            return texture;
        }
        public static WebGLTexture CreateGLTexture(ImageData image)
        {
            if (image == null)
                throw new InvalidOperationException();
            var texture = gl.createTexture();
            gl.bindTexture(gl.TEXTURE_2D, texture);
            gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, 1);	
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
            gl.bindTexture(gl.TEXTURE_2D, null);
            return texture;
        }
        public static WebGLTexture SetTextureData(WebGLTexture texture, ImageData image)
        {
            if (image == null)
                throw new InvalidOperationException();
            gl.bindTexture(gl.TEXTURE_2D, texture);
            gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, 1);	
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
            gl.bindTexture(gl.TEXTURE_2D, null);
            return texture;
        }
        public static TextureJSGL CreateTexture(Image image)
        {
            return new TextureJSGL(image, CreateGLTexture(image));
        }

        public override bool IsFullScreen { get { return false; } set { } }
        protected override VECTOR2 InternalScreenSize
        {
            //get { return new VECTOR2(gl.canvas.width, gl.canvas.height); }
            get { return new VECTOR2(window.document.body.clientWidth, window.document.body.clientHeight); }
            set
            {
                //float scale;
                //VECTOR2 offset;
                //__GRAPHICS.ViewAdapt(GraphicsSize,
                //    new VECTOR2(
                //        window.document.body.clientWidth,
                //        window.document.body.clientHeight),
                //    out scale, out offset);
                //gl.canvas.style = string.Format("left:{0}px;top:{1}px;position:absolute;", (int)offset.X, (int)offset.Y);
                //gl.canvas.width = (int)(GraphicsSize.X * scale);
                //gl.canvas.height = (int)(GraphicsSize.Y * scale);
            }
        }
        internal GraphicsWebGL(WebGLRenderingContext context)
        {
            //XCornerOffsets = new float[] {  };
            //YCornerOffsets = new float[] {  };

            GraphicsWebGL.gl = context;
            PrimitiveTypes = new int[] { context.POINTS, context.LINES, context.TRIANGLES };
            /*
             * [Pixel Shader]
             * precision mediump float;
             * varying vec4 color;
             * varying vec2 coord;
             * uniform sampler2D sampler;
             * void main(void) { gl_FragColor = color * texture2D(sampler, coord); }
             * 
             * min: precision mediump float;varying vec4 color;varying vec2 coord;uniform sampler2D sampler;void main(void){gl_FragColor=color*texture2D(sampler,coord);}
             * 
             * [Vertex Shader]
             * attribute vec3 vpos;
             * attribute vec4 vcolor;
             * attribute vec2 vcoord;
             * varying vec4 color;
             * varying vec2 coord;
             * void main(void) { gl_Position = vec4(vpos, 1.0); color = vcolor; coord = vcoord; }
             * 
             * min: attribute vec3 vpos;attribute vec4 vcolor;attribute vec2 vcoord;varying vec4 color;varying vec2 coord;void main(void){gl_Position=vec4(vpos,1.0);color=vcolor;coord=vcoord;}
             * 
             * 处理3D时需要的矩阵
             * uniform mat4 view;
             * 
             * main中修改
             * gl_Position = view * vec4(vpos, 1.0);
             * 
             * 代码中追加
             * var view = gl.getUniformLocation(program, "view");
             * 在Begin中绑定矩阵即可，matrix要求的是float[]，参数顺序暂时未明确
             * gl.uniformMatrix4fv(view, false, [matrix]);
             * 其中3D需要开启深度测试，暂未清楚开始绑定就好还是每次Begin都需要绑定
             * context.enable(context.DEPTH_TEST);
             * 
             * uniform mat4 view;
             * attribute vec3 vpos;
             * attribute vec4 vcolor;
             * attribute vec2 vcoord;
             * varying vec4 color;
             * varying vec2 coord;
             * void main(void) { gl_Position = view * vec4(vpos, 1.0); color = vcolor; coord = vcoord; }
             * 
             * min: uniform mat4 view;attribute vec3 vpos;attribute vec4 vcolor;attribute vec2 vcoord;varying vec4 color;varying vec2 coord;void main(void){gl_Position=view*vec4(vpos,1.0);color=vcolor;coord=vcoord;}
             */
            defaultVertexShader = context.createShader(context.VERTEX_SHADER);
            context.shaderSource(defaultVertexShader, "uniform mat4 view;attribute vec3 vpos;attribute vec4 vcolor;attribute vec2 vcoord;varying vec4 color;varying vec2 coord;void main(void){gl_Position=view*vec4(vpos,1.0);color=vcolor;coord=vcoord;}");
            context.compileShader(defaultVertexShader);

            defaultPixelShader = context.createShader(context.FRAGMENT_SHADER);
            context.shaderSource(defaultPixelShader, "precision mediump float;varying vec4 color;varying vec2 coord;uniform sampler2D sampler;void main(void){gl_FragColor=color*texture2D(sampler,coord);}");
            context.compileShader(defaultPixelShader);

            shaderProgram = context.createProgram();
            context.attachShader(shaderProgram, defaultVertexShader);
            context.attachShader(shaderProgram, defaultPixelShader);
            context.linkProgram(shaderProgram);
            context.useProgram(shaderProgram);

            context.enableVertexAttribArray(0);
            context.enableVertexAttribArray(1);
            context.enableVertexAttribArray(2);
            
            vertexShaderMatrix = gl.getUniformLocation(shaderProgram, "view");
            vertexShaderMatrixArray = new float[16];

            verticesBuffer = context.createBuffer();
            context.bindBuffer(context.ARRAY_BUFFER, verticesBuffer);
            // 在手机浏览器中，这个索引的顺序是021，所以一定要用getAttribLocation获得的索引
            int vpos = context.getAttribLocation(shaderProgram, "vpos");
            int vcolor = context.getAttribLocation(shaderProgram, "vcolor");
            int vcoord = context.getAttribLocation(shaderProgram, "vcoord");
            // 0-3个float是坐标
            context.vertexAttribPointer(vpos, 3, context.FLOAT, false, 36, 0);
            // 3-7个float是颜色
            context.vertexAttribPointer(vcolor, 4, context.FLOAT, false, 36, 12);
            // 7-9个float是uv
            context.vertexAttribPointer(vcoord, 2, context.FLOAT, false, 36, 28);            
        }
        protected override void SetViewport(MATRIX2x3 view, RECT viewport)
        {
            // 固定画布尺寸撑满屏幕
            VECTOR2 gsize = GraphicsSize;
            float scale;
            VECTOR2 offset;
            __GRAPHICS.ViewAdapt(gsize,
                new VECTOR2(
                    window.document.body.clientWidth,
                    window.document.body.clientHeight),
                out scale, out offset);
            gl.canvas.style = string.Format("left:{0}px;top:{1}px;position:absolute;", (int)offset.X, (int)offset.Y);
            gl.canvas.width = (int)(gsize.X * scale);
            gl.canvas.height = (int)(gsize.Y * scale);

            //RECT rect = AreaToScreen(viewport);
            this.View = MATRIX2x3.Identity;
            //gl.viewport(-rect.X, -rect.Y, rect.Width, rect.Height);
            gl.viewport(0, 0, gl.canvas.width, gl.canvas.height);
            gl.enable(gl.SCISSOR_TEST);
        }
        public override void Clear()
        {
            gl.clearColor(0, 0, 0, 1);
            gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
            gl.enable(gl.BLEND);
            gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
        }
        protected override void InternalBegin(bool threeD, ref MATRIX matrix, ref RECT graphics, SHADER shader)
        {
            RECT rect = AreaToScreen(graphics);
            // 严格来说这里的rect不是屏幕的，而是画布内部的，所以不采用屏幕的偏移值
            rect.X -= graphicsToScreen.M31;
            rect.Y -= graphicsToScreen.M32;
            // 左下角0,0 单位像素
            gl.scissor((int)rect.X, gl.canvas.height - (int)(rect.Y + rect.Height), (int)rect.Width, (int)rect.Height);
            if (threeD)
            {
                vertexShaderMatrixArray[0] = matrix.M11;
                vertexShaderMatrixArray[1] = matrix.M12;
                vertexShaderMatrixArray[2] = matrix.M13;
                vertexShaderMatrixArray[3] = matrix.M14;
                vertexShaderMatrixArray[4] = matrix.M21;
                vertexShaderMatrixArray[5] = matrix.M22;
                vertexShaderMatrixArray[6] = matrix.M23;
                vertexShaderMatrixArray[7] = matrix.M24;
                vertexShaderMatrixArray[8] = matrix.M31;
                vertexShaderMatrixArray[9] = matrix.M32;
                vertexShaderMatrixArray[10] = matrix.M33;
                vertexShaderMatrixArray[11] = matrix.M34;
                vertexShaderMatrixArray[12] = matrix.M41;
                vertexShaderMatrixArray[13] = matrix.M42;
                vertexShaderMatrixArray[14] = matrix.M43;
                vertexShaderMatrixArray[15] = matrix.M44;

                gl.uniformMatrix4fv(vertexShaderMatrix, false, vertexShaderMatrixArray);
                // todo: 大范围使用3D时，不清楚遮挡关系会如何；目前开启深度测试相同深度的对象覆盖关系将发生变化
                //gl.enable(gl.DEPTH_TEST);
            }
            else
            {
                var gsize = GraphicsSize;
                MATRIX2x3 view =
                    // 1280, 720
                    (MATRIX2x3)matrix *
                    // 将左上角坐标转换成屏幕中央坐标
                    // 2, -2
                    MATRIX2x3.CreateScale(2 / gsize.X, -2 / gsize.Y) *
                    // 1, -1
                    MATRIX2x3.CreateTranslation(-1, 1)
                    ;
                vertexShaderMatrixArray[0] = view.M11;
                vertexShaderMatrixArray[1] = view.M12;
                vertexShaderMatrixArray[2] = 0;
                vertexShaderMatrixArray[3] = 0;
                vertexShaderMatrixArray[4] = view.M21;
                vertexShaderMatrixArray[5] = view.M22;
                vertexShaderMatrixArray[6] = 0;
                vertexShaderMatrixArray[7] = 0;
                vertexShaderMatrixArray[8] = 0;
                vertexShaderMatrixArray[9] = 0;
                vertexShaderMatrixArray[10] = 1;
                vertexShaderMatrixArray[11] = 0;
                vertexShaderMatrixArray[12] = view.M31;
                vertexShaderMatrixArray[13] = view.M32;
                vertexShaderMatrixArray[14] = 0;
                vertexShaderMatrixArray[15] = 1;

                gl.uniformMatrix4fv(vertexShaderMatrix, false, vertexShaderMatrixArray);
            }
        }
        protected override void DrawPrimitivesBegin(TEXTURE texture, EPrimitiveType ptype)
        {
            //var tex = _HTML5.GetTextureGL(texture);
            var tex = ((TextureJS)TEXTURE.GetDrawableTexture(texture)).Texture;
            if (tex == null)
            {
                _LOG.Debug("DrawPrimitivesBegin Texture: null");
                return;
            }

            //canvas.activeTexture(canvas.TEXTURE0);
            gl.bindTexture(gl.TEXTURE_2D, tex);

            //HTML5Gate.TestTime++;
        }
        protected override void DrawPrimitives(EPrimitiveType ptype, TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount)
        {
            if (indexOffset != 0)
                throw new NotImplementedException();
            // 核心绘制只占用时间1/10
            if (indicesBufferArray != indexes)
            {
                if (indicesBuffer == null)
                    indicesBuffer = gl.createBuffer();
                gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indicesBuffer);
                gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, new Uint16Array(indexes), gl.STATIC_DRAW);
                indicesBufferArray = indexes;
            }

            int vcount = count * 9;
            if (vcount > arrayBuffer.length)
            {
                int capcity = arrayBuffer.length * 2;
                if (vcount > capcity)
                    capcity = vcount + 36;
                arrayBuffer = new Float32Array(capcity);
            }

            float twidth = Texture == null ? 1 : _MATH.DIVIDE_BY_1[Texture.Width];
            float theight = Texture == null ? 1 : _MATH.DIVIDE_BY_1[Texture.Height];
            for (int i = offset, n = offset + count; i < n; i++)
            {
                // 使用左上坐标系计算好坐标再转换成左下角坐标
                // WebGL的原点在画布中央
                //float x = vertices[i].Position.X;
                //float y = vertices[i].Position.Y;
                //vertices[i].Position.X = ((x * modelview.M11 + y * modelview.M21 + modelview.M31) * _gs.X - 0.5f) * 2;
                //vertices[i].Position.Y = (0.5f - (x * modelview.M12 + y * modelview.M22 + modelview.M32) * _gs.Y) * 2;

                vertices[i].TextureCoordinate.X *= twidth;
                vertices[i].TextureCoordinate.Y *= theight;
            }
            for (int i = offset, n = offset + count; i < n; i++)
            {
                int _offset = i * 9;
                arrayBuffer[_offset + 0] = vertices[i].Position.X;
                arrayBuffer[_offset + 1] = vertices[i].Position.Y;
                arrayBuffer[_offset + 2] = vertices[i].Position.Z;
                arrayBuffer[_offset + 3] = vertices[i].Color.R * COLOR.BYTE_TO_FLOAT;
                arrayBuffer[_offset + 4] = vertices[i].Color.G * COLOR.BYTE_TO_FLOAT;
                arrayBuffer[_offset + 5] = vertices[i].Color.B * COLOR.BYTE_TO_FLOAT;
                arrayBuffer[_offset + 6] = vertices[i].Color.A * COLOR.BYTE_TO_FLOAT;
                arrayBuffer[_offset + 7] = vertices[i].TextureCoordinate.X;
                arrayBuffer[_offset + 8] = vertices[i].TextureCoordinate.Y;
            }
            gl.bufferData(gl.ARRAY_BUFFER, arrayBuffer.subarray(offset, offset + count * 9), gl.STREAM_DRAW);

            int drawVCount;
            switch (ptype)
            {
                case EPrimitiveType.Line: drawVCount = primitiveCount << 1; break;
                case EPrimitiveType.Triangle: drawVCount = primitiveCount * 3; break;
                default: drawVCount = primitiveCount; break;
            }
            gl.drawElements(PrimitiveTypes[(int)ptype], drawVCount, gl.UNSIGNED_SHORT, 0);
        }
        // 占用大量时间貌似还没什么作用
        //protected override void DrawPrimitivesEnd()
        //{
        //    gl.flush();
        //}
    }
    public class GraphicsCanvas : GRAPHICS
    {
        internal static GraphicsCanvas TextGraphics;

        private static CanvasRenderingContext2D SetCanvas(string fontStyle, int width, int height)
        {
            var ctx = TextGraphics.ctx;
            if (ctx.canvas.width != width || ctx.canvas.height != height)
            {
                ctx.canvas.width = width;
                ctx.canvas.height = height;
                //ctx.clearRect(0, 0, width, height);
            }
            if (!string.IsNullOrEmpty(fontStyle))
            {
                ctx.font = fontStyle;
            }
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillStyle = "#FFFFFF";
            return ctx;
        }
        internal static ImageData DrawText(string fontStyle, string text, int width, int height)
        {
            var ctx = SetCanvas(fontStyle, width, height);
            ctx.fillText(text, width >> 1, height >> 1, width);
            var data = ctx.getImageData(0, 0, width, height);
            ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
            return data;
        }
        internal static ImageData DrawImage(Image image, int x, int y, int width, int height)
        {
            var ctx = SetCanvas(null, width, height);
            ctx.drawImage(image, x, y, width, height, 0, 0, image.width, image.height);
            ImageData data = ctx.getImageData(x, y, width, height);
            ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
            return data;
        }

        internal CanvasRenderingContext2D ctx;
        public override bool IsFullScreen { get { return false; } set { } }
        protected override VECTOR2 InternalScreenSize
        {
            get { return new VECTOR2(ctx.canvas.width, ctx.canvas.height); }
            set
            {
                ctx.canvas.width = (int)value.X;
                ctx.canvas.height = (int)value.Y;
            }
        }
        internal GraphicsCanvas(CanvasRenderingContext2D context)
        {
            ctx = context;
        }
        protected override void SetViewport(MATRIX2x3 view, RECT viewport)
        {
        }
        protected override void InternalBegin(bool threeD, ref MATRIX matrix, ref RECT graphics, SHADER shader)
        {
            throw new NotImplementedException();
        }
        protected override void InternalDraw(TEXTURE texture, ref SpriteVertex vertex, ref BoundingBox box)
        {
            base.InternalDraw(texture, ref vertex, ref box);
        }
        protected override void DrawPrimitives(EPrimitiveType ptype, TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount)
        {
            throw new NotImplementedException();
        }
        protected override void DrawPrimitivesBegin(TEXTURE texture, EPrimitiveType ptype)
        {
            throw new NotImplementedException();
        }
    }

    internal class PlatformJS : IPlatform
    {
        private TimeSpan frameRate = TimeSpan.FromMilliseconds(16.666666667);
        private bool isMouseVisible = true;

        public EPlatform Platform
        {
            get
            {
                if (window.navigator.appVersion.Contains("Android")
                    || window.navigator.appVersion.Contains("iP")
                    || window.navigator.appVersion.Contains("Phone"))
                {
                    return EPlatform.Mobile;
                }
                string pname = window.navigator.platform;
                if (pname.StartsWith("Win") ||
                    pname.StartsWith("Mac") ||
                    pname.StartsWith("Linux") ||
                    pname == "X11")
                {
                    return EPlatform.Desktop;
                }
                else
                {
                    return EPlatform.Mobile;
                }
            }
        }
        public TimeSpan FrameRate
        {
            get { return frameRate; }
            set { frameRate = value; }
        }
        public bool IsMouseVisible
        {
            get { return isMouseVisible; }
            set { isMouseVisible = value; }
        }
        public bool IsActive
        {
            get { return true; }
        }
    }
    public class EntryJS : Entry
    {
        protected override void Initialize(out AUDIO AUDIO, out ContentManager ContentManager, out FONT FONT, out GRAPHICS GRAPHICS, out INPUT INPUT, out _IO.iO iO, out IPlatform IPlatform, out TEXTURE TEXTURE)
        {
            _LOG._Logger = new Logger();

            FONT = new FontDynamicJSGL("黑体", 24);

            IPlatform = new PlatformJS();
            // 判断当前是PC还是Mobile，以便后续决定操作类型
            if (IPlatform.Platform == EPlatform.Desktop)
            {
                INPUT = new INPUT(new MouseJS());
                INPUT.Keyboard = new KeyboardJS();

                window.document.onmousedown = document_onmousedown;
                window.document.onmousemove = document_onmousemove;
                window.document.onmouseup = document_onmouseup;
                window.document.onmousewheel = document_onmousewheel;

                window.document.onkeydown = document_onkeydown;
                window.document.onkeyup = document_onkeyup;
            }
            else
            {
                INPUT = new INPUT(new TouchJS());

                window.document.ontouchstart = document_ontouchstart;
                window.document.ontouchmove = document_ontouchmove;
                window.document.ontouchend = document_ontouchend;
            }
            INPUT.InputDevice = new InputTextJS();

            AUDIO = null;

            TEXTURE = null;
            //var canvas = (HTMLCanvasElement)window.document.getElementById("WEBGL");
            var canvas = (HTMLCanvasElement)window.document.createElement("canvas");
            window.document.body.appendChild(canvas);
            if (canvas == null) throw new ArgumentNullException("don't have 'canvas' element");
            var graphics = canvas.getContext("webgl");
            if (graphics == null)
                graphics = canvas.getContext("experimental-webgl");
            if (graphics != null)
                GRAPHICS = new GraphicsWebGL((WebGLRenderingContext)graphics);
            else
            {
                _LOG.Warning("渲染模式为Canvas而非WebGL");
                graphics = canvas.getContext("2d");
                GRAPHICS = new GraphicsCanvas((CanvasRenderingContext2D)graphics);
            }
            // 保留2d-context用于绘制文字，获取图片颜色数据等，此canvas不能和gl的一样，否则getContext会返回null
            //canvas = (HTMLCanvasElement)window.document.getElementById("CANVAS");
            canvas = (HTMLCanvasElement)window.document.createElement("canvas");
            window.document.body.appendChild(canvas);
            graphics = canvas.getContext("2d");
            GraphicsCanvas.TextGraphics = new GraphicsCanvas((CanvasRenderingContext2D)graphics);

            window.onresize = window_onresize;

            iO = new IOJSWeb();

            ContentManager = NewContentManager();

            // 微信小游戏不支持创建图片，只能加载图片来设置PIXEL和PATCH的默认图
            TextureJSGL.FromBase64("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAACXBIWXMAAAsSAAALEgHS3X78AAAADUlEQVQImWP4////fwAJ+wP9CNHoHgAAAABJRU5ErkJggg==", t => SetPIXEL(t));
            TextureJSGL.FromBase64("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAACXBIWXMAAAsSAAALEgHS3X78AAAAFklEQVQYlWP8////fwY8gAmf5PBRAAAbbgQMcSRW5wAAAABJRU5ErkJggg==", t => SetPATCH(t));
        }
        public override void Exit()
        {
            HTML5Gate.Exit();
            base.Exit();
        }

        void window_onresize()
        {
            // 重置屏幕尺寸和居中偏移值
            //GRAPHICS.ScreenSize = new VECTOR2(GraphicsWebGL.gl.canvas.width, GraphicsWebGL.gl.canvas.height);
            //GRAPHICS.ScreenSize = new VECTOR2(window.document.body.clientWidth, window.document.body.clientHeight);
            GRAPHICS.ScreenSize = GRAPHICS.ScreenSize;
        }

        void document_onkeydown(window.KeyboardEvent e)
        {
            var pressed = KeyboardJS.Pressed;
            int count = pressed.Count;
            for (int i = 0; i < count; i++)
                if (pressed[i] == e.keyCode)
                    return;
            // ime输入，每次按键都是229
            if (e.keyCode == 229)
            {
                InputTextJS.IME = true;
                InputTextJS._229 = e;
            }
            else
            {
                InputTextJS.IME = false;
                pressed.Add(e.keyCode);
            }
        }
        void document_onkeyup(window.KeyboardEvent e)
        {
            KeyboardJS.Pressed.Remove(e.keyCode);
        }

        void document_onmousedown(window.MouseEvent obj)
        {
            MouseJS.state.position.X = obj.x;
            MouseJS.state.position.Y = obj.y;
            MouseJS.state.left = true;
            //_LOG.Debug("{0},{1}", obj.x, obj.y);
            //var pos = Entry._GRAPHICS.PointToGraphics(MouseJS.state.position);
            //_LOG.Debug("{0},{1}", (int)pos.X, (int)pos.Y);
            //pos = Entry._GRAPHICS.PointToScreen(pos);
            //_LOG.Debug("{0},{1}", (int)pos.X, (int)pos.Y);
        }
        void document_onmousemove(window.MouseEvent obj)
        {
            MouseJS.state.position.X = obj.x;
            MouseJS.state.position.Y = obj.y;
        }
        void document_onmouseup(window.MouseEvent obj)
        {
            MouseJS.state.position.X = obj.x;
            MouseJS.state.position.Y = obj.y;
            MouseJS.state.left = false;
        }
        void document_onmousewheel(window.WheelEvent obj)
        {
            MouseJS.state.scrollWheelValue = -obj.wheelDelta / 120;
        }

        void document_ontouchstart(window.TouchEvent touch)
        {
            TouchJS.TouchEvent = touch;
        }
        void document_ontouchmove(window.TouchEvent touch)
        {
            TouchJS.TouchEvent = touch;
        }
        void document_ontouchend(window.TouchEvent touch)
        {
            TouchJS.TouchEvent = null;
        }

        protected override _IO.iO InternalNewiO(string root)
        {
            var io = new IOJSWeb();
            io.RootDirectory = root;
            return io;
        }
        protected override ContentManager InternalNewContentManager()
        {
            ContentManager manager = new ContentManager(NewiO(iO.RootDirectory));
            manager.AddPipeline(new PipelineParticle());
            manager.AddPipeline(new PipelineAnimation());
            manager.AddPipeline(new PipelinePiece());
            manager.AddPipeline(new PipelinePatch());
            manager.AddPipeline(new PipelineTextureJSGL());
            return manager;
        }
        protected override FONT InternalNewFONT(string name, float fontSize)
        {
            return new FontDynamicJSGL(name, fontSize);
        }
        protected override TEXTURE InternalNewTEXTURE(int width, int height)
        {
            return new TextureJSData(width, height);
        }
    }
}
