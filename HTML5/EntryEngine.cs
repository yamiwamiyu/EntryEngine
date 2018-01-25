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
    }
    public class PipelineTextureJSGL : ContentPipeline
    {
        public override IEnumerable<string> SuffixProcessable
        {
            get { return TEXTURE.TextureFileType; }
        }
        protected override Content Load(string file)
        {
            TextureJSGL result = new TextureJSGL();

            Image img = new Image();
            img.onload = () =>
            {
                var texture = GraphicsWebGL.CreateGLTexture(img);
                result.Image = img;
                result.Texture = texture;
            };
            img.src = IOJSWeb.BuildNetUrl(IO.BuildPath(file));
            // todo: 貌似无法实现同步加载完成

            return result;
        }
        protected override void LoadAsync(AsyncLoadContent async)
        {
            Image img = new Image();
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
        protected override Content InternalLoad(byte[] bytes)
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
            return string.Format("{0}px {1}", (int)this.fontSize, fontName);
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
        protected override COLOR[] DrawChar(char c, ref RECT uv)
        {
            return COLOR.Convert(GraphicsCanvas.DrawText(GetFontStyle(), c.ToString(), (int)uv.Width, (int)uv.Height).data.GetBytes());
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
        private static Float32Array arrayBuffer = new Float32Array(8 * 4 * 1);
        private static WebGLBuffer verticesBuffer;
        private static WebGLBuffer indicesBuffer;
        private static int indicesBufferCount = 0;

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

        private VECTOR2 _gs;
        private MATRIX2x3 modelview;
        public override bool IsFullScreen { get { return false; } set { } }
        protected override VECTOR2 InternalScreenSize
        {
            get { return new VECTOR2(gl.canvas.width, gl.canvas.height); }
            set
            {
                gl.canvas.width = (int)value.X;
                gl.canvas.height = (int)value.Y;
            }
        }
        internal GraphicsWebGL(WebGLRenderingContext context)
        {
            //XCornerOffsets = new float[] {  };
            //YCornerOffsets = new float[] {  };

            GraphicsWebGL.gl = context;
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
             * attribute vec2 vpos;
             * attribute vec4 vcolor;
             * attribute vec2 vcoord;
             * varying vec4 color;
             * varying vec2 coord;
             * void main(void) { gl_Position = vec4(vpos, 0.0, 1.0); color = vcolor; coord = vcoord; }
             * 
             * min: attribute vec2 vpos;attribute vec4 vcolor;attribute vec2 vcoord;varying vec4 color;varying vec2 coord;void main(void){gl_Position=vec4(vpos,0.0,1.0);color=vcolor;coord=vcoord;}
             */
            defaultVertexShader = context.createShader(context.VERTEX_SHADER);
            context.shaderSource(defaultVertexShader, "attribute vec2 vpos;attribute vec4 vcolor;attribute vec2 vcoord;varying vec4 color;varying vec2 coord;void main(void){gl_Position=vec4(vpos,0.0,1.0);color=vcolor;coord=vcoord;}");
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

            verticesBuffer = context.createBuffer();
            context.bindBuffer(context.ARRAY_BUFFER, verticesBuffer);
            // 0-2个float是坐标
            context.vertexAttribPointer(0, 2, context.FLOAT, false, 32, 0);
            // 2-6个float是颜色
            context.vertexAttribPointer(1, 4, context.FLOAT, false, 32, 8);
            // 6-8个float是uv
            context.vertexAttribPointer(2, 2, context.FLOAT, false, 32, 24);
        }
        protected override void SetViewport(MATRIX2x3 view, RECT viewport)
        {
            RECT rect = AreaToScreen(viewport);
            this.View = MATRIX2x3.Identity;
            gl.viewport(rect.X, rect.Y, rect.Width, rect.Height);
            gl.enable(gl.SCISSOR_TEST);
        }
        public override void Clear()
        {
            gl.clearColor(0, 0, 0, 1);
            gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
            gl.enable(gl.BLEND);
            gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
        }
        protected override void InternalBegin(ref MATRIX2x3 matrix, ref RECT graphics, SHADER shader)
        {
            RECT rect = AreaToScreen(graphics);
            // 左下角0,0 单位像素
            gl.scissor((int)rect.X, gl.canvas.height - (int)(rect.Y + rect.Height), (int)rect.Width, (int)rect.Height);
            modelview = matrix;
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
            //var tex = _HTML5.GetTextureGL(texture);
            var tex = ((TextureJS)TEXTURE.GetDrawableTexture(texture)).Texture;
            if (tex == null)
            {
                _LOG.Debug("DrawPrimitivesBegin Texture: null");
                return;
            }

            //canvas.activeTexture(canvas.TEXTURE0);
            gl.bindTexture(gl.TEXTURE_2D, tex);

            var graphics = GraphicsSize;
            _gs.X = 1 / graphics.X;
            _gs.Y = 1 / graphics.Y;

            //HTML5Gate.TestTime++;
        }
        //public override void BaseDraw(TEXTURE texture, float x, float y, float w, float h, bool scale, float sx, float sy, float sw, float sh, bool color, byte r, byte g, byte b, byte a, float rotation, float ox, float oy, EFlip flip)
        //{
        //    // BUG: 占用1/3 ~ 2/3时间
        //    var time = new Date().getTime();
        //    base.BaseDraw(texture, x, y, w, h, scale, sx, sy, sw, sh, color, r, g, b, a, rotation, ox, oy, flip);
        //    HTML5Gate.TestTime += new Date().getTime() - time;
        //}
        protected override void OutputVertex(ref TextureVertex output)
        {
            //var time = new Date().getTime();
            // 坐标系：屏幕中央为0,0 左-1 右1 下-1 上1 区间-1~1

            // UV坐标系：左上角0,0 区间0~1
            //output.TextureCoordinate.Y = 1 - output.TextureCoordinate.Y;

            // 使用左上坐标系计算好坐标再转换成左下角坐标
            // WebGL的原点在画布中央
            float x = output.Position.X;
            float y = output.Position.Y;
            output.Position.X = ((x * modelview.M11 + y * modelview.M21 + modelview.M31) * _gs.X - 0.5f) * 2;
            output.Position.Y = (0.5f - (x * modelview.M12 + y * modelview.M22 + modelview.M32) * _gs.Y) * 2;

            //VECTOR2.Transform(ref output.Position.X, ref output.Position.Y, ref modelview);
            //output.Position.X *= _gs.X;
            //output.Position.Y *= _gs.Y;
            // WebGL的原点在画布中央
            //output.Position.X = (output.Position.X - 0.5f) * 2;
            //output.Position.Y = (0.5f - output.Position.Y) * 2;

            //HTML5Gate.TestTime += new Date().getTime() - time;
        }
        protected override void DrawPrimitives(TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount)
        {
            // 核心绘制只占用时间1/10
            if (indicesBufferCount != indices.Length)
            {
                indicesBufferCount = indices.Length;
                if (indicesBuffer == null)
                    indicesBuffer = gl.createBuffer();
                gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indicesBuffer);
                gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, new Uint16Array(indices), gl.STATIC_DRAW);
            }

            int vcount = count * 8;
            if (vcount > arrayBuffer.length)
            {
                int capcity = arrayBuffer.length * 2;
                if (vcount > capcity)
                    capcity = vcount + 32;
                arrayBuffer = new Float32Array(capcity);
            }

            for (int i = offset, n = offset + count; i < n; i++)
            {
                int _offset = i * 8;
                arrayBuffer[_offset + 0] = vertices[i].Position.X;
                arrayBuffer[_offset + 1] = vertices[i].Position.Y;
                arrayBuffer[_offset + 2] = vertices[i].Color.R * COLOR.BYTE_TO_FLOAT;
                arrayBuffer[_offset + 3] = vertices[i].Color.G * COLOR.BYTE_TO_FLOAT;
                arrayBuffer[_offset + 4] = vertices[i].Color.B * COLOR.BYTE_TO_FLOAT;
                arrayBuffer[_offset + 5] = vertices[i].Color.A * COLOR.BYTE_TO_FLOAT;
                arrayBuffer[_offset + 6] = vertices[i].TextureCoordinate.X;
                arrayBuffer[_offset + 7] = vertices[i].TextureCoordinate.Y;
            }
            gl.bufferData(gl.ARRAY_BUFFER, arrayBuffer.subarray(offset, offset + count * 8), gl.STREAM_DRAW);
            gl.drawElements(gl.TRIANGLES, primitiveCount * 3, gl.UNSIGNED_SHORT, 0);
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
            ctx.fillText(text, width / 2, height / 2, width);
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
        protected override void InternalBegin(ref MATRIX2x3 matrix, ref RECT graphics, SHADER shader)
        {
            throw new NotImplementedException();
        }
        protected override void DrawPrimitives(TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount)
        {
            throw new NotImplementedException();
        }
        protected override void DrawPrimitivesBegin(TEXTURE texture)
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
                //INPUT.Keyboard = new KeyboardXna();
                //INPUT.InputDevice = new InputTextXna();
            }
            else
            {
                INPUT = new INPUT(new MouseJS());
            }
            

            AUDIO = null;

            TEXTURE = null;
            var canvas = (HTMLCanvasElement)window.document.getElementById("WEBGL");
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
            canvas = (HTMLCanvasElement)window.document.getElementById("CANVAS");
            graphics = canvas.getContext("2d");
            GraphicsCanvas.TextGraphics = new GraphicsCanvas((CanvasRenderingContext2D)graphics);

            iO = new IOJSWeb();

            ContentManager = NewContentManager();

            window.document.onmousedown = document_onmousedown;
            window.document.onmousemove = document_onmousemove;
            window.document.onmouseup = document_onmouseup;
            window.document.onmousewheel = document_onmousewheel;

            //throw new NotImplementedException();
        }
        public override void Exit()
        {
            HTML5Gate.Exit();
            base.Exit();
        }

        void document_onmousedown(window.MouseEvent obj)
        {
            MouseJS.state.position.X = obj.x;
            MouseJS.state.position.Y = obj.y;
            MouseJS.state.left = true;
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

        protected override _IO.iO InternalNewiO(string root)
        {
            var io = new IOJSWeb();
            io.RootDirectory = root;
            return io;
        }
        protected override ContentManager InternalNewContentManager()
        {
            ContentManager manager = new ContentManager(NewiO(iO.RootDirectory));
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
