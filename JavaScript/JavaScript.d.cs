using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Object_
{
    public extern object this[object obj] { get; set; }
    public extern string toString();
}
public class Date
{
    public extern Date();
    public extern Date(string str);
    public extern Date(int year, int month, int day);
    public extern Date(int year, int month, int day, int hour, int minute, int second);
    public extern Date(int time);
    public extern int getDate();
    public extern void setDate(int time);
    public extern int getDay();
    public extern void setDay(int time);
    public extern int getMonth();
    public extern void setMonth(int time);
    public extern int getFullYear();
    public extern void setFullYear(int time);
    public extern int getHours();
    public extern void setHours(int time);
    public extern int getMinutes();
    public extern void setMinutes(int time);
    public extern int getSeconds();
    public extern void setSeconds(int time);
    public extern int getMilliseconds();
    public extern void setMilliseconds(int time);
    /// <suextern mmary>返回 1970 年 1 月 1 日至今的毫秒数</summary>
    public extern int getTime();
    public extern void setTime(int time);
    public extern static long now();
    public extern static long UTC(int year, int month, int date, int hours, int minutes, int seconds, int ms);
}
public class String
{
    public int length;
    public extern string charAt(int index);
    public extern ushort charCodeAt(int index);
    public extern int indexOf(string str);
    public extern int indexOf(string str, int index);
    public extern string slice(int start);
    /// <suextern mmary>SubString，后面的参数是结束的索引</summary>
    public extern string slice(int start, int end);
    public extern string[] split(string str);
    public extern string replace(string regexp, string str2);
    public extern string replace(RegExp searchValue, string replaceValue);
    public extern string substring(int start);
    public extern string substring(int start, int end);
    public extern string valueOf();
    public extern static string fromCharCode(params char[] unicodes);
    public extern string toUpperCase();
    public extern string toLowerCase();
}
public class RegExp
{
    public extern RegExp(string pattern, string attributes);
}
public class Number
{
    public const double MAX_VALUE = 0;
    public const double MIN_VALUE = 0;
    public const double NaN = 0.0 / 0.0;
    /// <summary>负无穷大，溢出时返回该值</summary>
    public static double NEGATIVE_INFINITY;
    /// <summary>正无穷大，溢出时返回该值</summary>
    public static double POSITIVE_INFINITY;
    public extern Number(object obj);
}
/// <summary>用于给Array继承</summary>
public class Array_
{
    public int length;
    //public extern object this[int index] { get; set; }
    public extern Array_();
    public extern Array_(int size);
    public extern Array_(params object[] args);
    public extern string join(string split);
    public extern void push(object obj);
    public extern object pop();
    public extern Array slice(int start, int end);
}
public class Error
{
    public string message;
    public string stack;
}
public static class console
{
    public extern static void log(object obj);
}
public static class window
{
    public class TouchList
    {
        public int length;
        public extern object this[int index] { get; set; }
    }
    public class TouchEvent
    {
        public TouchList touches;
    }
    public class TouchData
    {
        public float clientX;
        public float clientY;
        public int identifier;
        public float pageX;
        public float pageY;
        public float radiusX;
        public float radiusY;
        public float rotationAngle;
        public int screenX;
        public int screenY;
    }
    public class MouseEvent
    {
        public int x;
        public int y;
    }
    public class KeyboardEvent
    {
        public ushort keyCode;
    }
    public class WheelEvent
    {
        /// <summary>120为单位，向前滚为120，相反为-120</summary>
        public sbyte wheelDelta;
    }
    public class Document
    {
        /* 事件处理
            * 收到down事件后，直到未收到up事件期间都认为down事件有效
            */
        public Action<MouseEvent> onmousedown;
        public Action<MouseEvent> onmousemove;
        public Action<MouseEvent> onmouseup;
        public Action<TouchEvent> ontouchstart;
        public Action<TouchEvent> ontouchmove;
        public Action<TouchEvent> ontouchend;
        public Action<KeyboardEvent> onkeydown;
        public Action<KeyboardEvent> onkeypress;
        public Action<KeyboardEvent> onkeyup;
        public Action onload;
        public Action<WheelEvent> onmousewheel;
        public HTMLElement documentElement;
        public HTMLElement body;
        public extern HTMLElement getElementById(string elementId);
        public extern HTMLElement createElement(string label);
        public extern void write(string content);
        public extern void writeln(string content);
    }
    public class Math
    {
        public extern static double sqrt(double d);
        public extern static double acos(double value);
        public extern static double asin(double value);
        /// <summary>以介于 -PI/2 与 PI/2 弧度之间的数值来返回 x 的反正切值</summary>
        public extern static double atan(double value);
        /// <summary>返回从 x 轴到点 (x,y) 的角度（介于 -PI/2 与 PI/2 弧度之间）</summary>
        public extern static double atan2(double y, double x);
        public extern static double tan(double value);
        public extern static double sin(double value);
        public extern static double cos(double value);
        public extern static double pow(double x, double y);
        /// <summary>返回 0 ~ 1 之间的随机数</summary>
        public extern static double random();
    }

    public static Action onresize;
    public static Document document;
    public static Storage localStorage;
    public static URL URL;
    public static Navigator navigator;
    public extern static int parseInt(object obj);
    public extern static float parseFloat(object obj);
    public extern static bool isNaN(object obj);
    public extern static bool isFinite(object obj);
    public extern static int setTimeout(Action action, double ms);
    public extern static int setInterval(Action action, double ms);
    public extern static void clearTimeout(int timer);
    public extern static void clearInterval(int timer);
    /// <summary>转换成base64编码</summary>
    public extern static string btoa(string str);
    /// <summary>base64编码转换为原先的字符串</summary>
    public extern static string atob(string str);
    public extern static void open(string url);
}
public class Storage
{
    public int length;
    public extern void clear();
    public extern string getItem(string key);
    public extern string key(int index);
    public extern void removeItem(string key);
    public extern void setItem(string key, string data);
}
public class URL
{
    public string hash;
    public string host;
    public string hostname;
    public string href;
    public string origin;
    public string password;
    public string pathname;
    public string port;
    public string protocol;
    public string search;
    public string username;
    public extern string createObjectURL(object obj);
    public extern void revokeObjectURL(string url);
}
public class Image
{
    public string src;
    public string crossOrigin;
    public Action onload;
    public int width;
    public int height;
    public bool complete;
    public extern Image();
    public extern Image(int width, int height);
}
public interface EventTarget
{
}
public class NavigatorID
{
    public string appName;
    public string appVersion;
    public string platform;
    public string product;
    public string productSub;
    public string userAgent;
    public string vendor;
    public string vendorSub;
}
public class Navigator : NavigatorID
{
    public string appCodeName;
    public bool cookieEnabled;
    public string language;
    public int maxTouchPoints;
    //readonly mimeTypes: MimeTypeArray;
    public bool msManipulationViewsEnabled;
    public int msMaxTouchPoints;
    public bool msPointerEnabled;
    //readonly plugins: PluginArray;
    public bool pointerEnabled;
    public bool webdriver;
    public int hardwareConcurrency;
    //getGamepads(): Gamepad[];
    public extern bool javaEnabled();
}
public class XMLHttpRequestEventTarget
{
    public Action onabort;
    public Action onerror;
    public Action onload;
    public Action onloadend;
    public Action onloadstart;
    public Action onprogress;
    public Action ontimeout;
}
public class XMLHttpRequest : XMLHttpRequestEventTarget, EventTarget
{
    public Action onreadystatechange;
    public int readyState;
    public object response;
    public string responseText;
    public string responseType;
    public object responseXML;
    public int status;
    public string statusText;
    public int timeout;
    //readonly upload: XMLHttpRequestUpload;
    public bool withCredentials;
    public string responseURL;
    public extern void abort();
    public extern string getAllResponseHeaders();
    public extern string getResponseHeader(string header);
    public extern bool msCachingEnabled();
    public extern void open(string method, string url, bool async);
    public extern void overrideMimeType(string mime);
    public extern void send();
    public extern void send(string data);
    public extern void send(object data);
    public extern void setRequestHeader(string header, string value);
    public int DONE;
    public int HEADERS_RECEIVED;
    public int LOADING;
    public int OPENED;
    public int UNSENT;
}
public interface WebGLObject { }
public interface WebGLProgram : WebGLObject { }
public interface WebGLShader : WebGLObject { }
public interface WebGLBuffer : WebGLObject { }
public interface WebGLFramebuffer : WebGLObject { }
public interface WebGLRenderbuffer : WebGLObject { }
public interface WebGLTexture : WebGLObject { }
public interface WebGLUniformLocation { }
public struct WebGLContextAttributes
{
    public bool failIfMajorPerformanceCaveat;
    public bool alpha;
    public bool depth;
    public bool stencil;
    public bool antialias;
    public bool premultipliedAlpha;
    public bool preserveDrawingBuffer;
}
public struct WebGLShaderPrecisionFormat
{
    public int precision;
    public float rangeMax;
    public float rangeMin;
}
public class Blob
{
    public int size;
    public string type;
    public extern void msClose();
    public extern object msDetachStream();
    public extern Blob slice(int start, int end, string contentType);
}
public class WebGLActiveInfo
{
    public string name;
    public int size;
    public int type;
}
public abstract class Element
{
    public int clientHeight;
    public int clientLeft;
    public int clientTop;
    public int clientWidth;
    public string id;
}
public abstract class HTMLElement : Element
{
    /// <summary>是否允许编辑文字，允许则可以输入文字</summary>
    public bool contenteditable;
    public string outerHTML;
    public string outerText;
    public string innerText;
    public string style;
    public Action oninput;
    public Action onblur;
    public Action onchange;
    public extern void appendChild(HTMLElement element);
    public extern void click();
    public extern void focus();
    public extern void blur();
}
public abstract class HTMLFileSelector : HTMLElement
{
    public string multiple;
    public FileList files;
}
public abstract class AElement : HTMLElement
{
    public string href;
    public string download;
}
public abstract class FileList
{
    public abstract class File : Blob
    {
        public string name;
    }
    public int length;
    public extern File this[int index] { get; }
}
public class FileReader
{
    public Action onloadend;
    public ArrayBuffer result;
    public extern void readAsArrayBuffer(Blob blob);
}
public abstract class HTMLCanvasElement : HTMLElement
{
    /**
      * Gets or sets the height of a canvas element on a document.
      */
    public int height;
    /**
      * Gets or sets the width of a canvas element on a document.
      */
    public int width;
    /**
      * Returns an object that provides methods and properties for drawing and manipulating images and graphics on a canvas element in a document. A context object includes information about colors, line widths, fonts, and other graphic parameters that can be drawn on a canvas.
      * @param contextId The identifier (ID) of the type of canvas to create. Internet Explorer 9 and Internet Explorer 10 support only a 2-D context using canvas.getContext("2d"); IE11 Preview also supports 3-D or WebGL context using canvas.getContext("experimental-webgl");
     * WebGLRenderingContext: "webgl" | "experimental-webgl"
     * CanvasRenderingContext2D: "2d"
      */
    public extern object getContext(string contextId);
    /**
      * Returns a blob object encoded as a Portable Network Graphics (PNG) format from a canvas image or drawing.
      */
    public extern Blob msToBlob();
    /**
      * Returns the content of the current canvas as an image that you can use as a source for another canvas or an HTML element.
      * @param type The standard MIME type for the image format to return. If you do not specify this parameter, the default value is a PNG format image.
      */
    public extern string toDataURL(string type);
}
/**
  * Represents a raw buffer of binary data, which is used to store data for the
  * different typed arrays. ArrayBuffers cannot be read from or written to directly,
  * but can be passed to a typed array or DataView Object to interpret the raw
  * buffer as needed.
  */
public abstract class ArrayBuffer
{
    public int byteLength;
    public extern ArrayBuffer slice(int begin, int end);
}
public class ArrayBufferView
{
    public ArrayBuffer buffer;
    public int byteLength;
    public int byteOffset;
}
public abstract class DataView
{
    public ArrayBuffer buffer;
    public int byteLength;
    public int byteOffset;
    public extern int getFloat32(int byteOffset, bool littleEndian);
    public extern int getFloat64(int byteOffset, bool littleEndian);
    public extern int getInt8(int byteOffset);
    public extern int getInt16(int byteOffset, bool littleEndian);
    public extern int getInt32(int byteOffset, bool littleEndian);
    public extern int getUint8(int byteOffset);
    public extern int getUint16(int byteOffset, bool littleEndian);
    public extern int getUint32(int byteOffset, bool littleEndian);
    public extern void setFloat32(int byteOffset, float value, bool littleEndian);
    public extern void setFloat64(int byteOffset, double value, bool littleEndian);
    public extern void setInt8(int byteOffset, sbyte value);
    public extern void setInt16(int byteOffset, short value, bool littleEndian);
    public extern void setInt32(int byteOffset, int value, bool littleEndian);
    public extern void setUint8(int byteOffset, byte value);
    public extern void setUint16(int byteOffset, ushort value, bool littleEndian);
    public extern void setUint32(int byteOffset, uint value, bool littleEndian);
}
public abstract class WebGLRenderingContext
{
    public HTMLCanvasElement canvas;
    public int drawingBufferHeight;
    public int drawingBufferWidth;
    public extern void activeTexture(int texture);
    public extern void attachShader(WebGLProgram program, WebGLShader shader);
    public extern void bindAttribLocation(WebGLProgram program, int index, string name);
    public extern void bindBuffer(int target, WebGLBuffer buffer);
    public extern void bindFramebuffer(int target, WebGLFramebuffer framebuffer);
    public extern void bindRenderbuffer(int target, WebGLRenderbuffer renderbuffer);
    public extern void bindTexture(int target, WebGLTexture texture);
    public extern void blendColor(float red, float green, float blue, float alpha);
    public extern void blendFunc(int sfactor, int dfactor);
    public extern void blendEquation(int mode);
    public extern void blendEquationSeparate(int modeRGB, int modeAlpha);
    public extern void bufferData(int target, ArrayBuffer size, int usage);
    public extern int checkFramebufferStatus(int target);
    public extern void clear(int mask);
    public extern void clearColor(float red, float green, float blue, float alpha);
    public extern void clearDepth(float depth);
    public extern void clearStencil(int s);
    public extern void colorMask(bool red, bool green, bool blue, bool alpha);
    public extern void compileShader(WebGLShader shader);
    public extern void compressedTexImage2D(int target, int level, int internalformat, int width, int height, int border, ArrayBufferView data);
    public extern void compressedTexSubImage2D(int target, int level, int xoffset, int yoffset, int width, int height, int format, ArrayBufferView data);
    public extern void copyTexImage2D(int target, int level, int internalformat, int x, int y, int width, int height, int border);
    public extern void copyTexSubImage2D(int target, int level, int xoffset, int yoffset, int x, int y, int width, int height);
    public extern WebGLBuffer createBuffer();
    public extern WebGLFramebuffer createFramebuffer();
    public extern WebGLProgram createProgram();
    public extern WebGLRenderbuffer createRenderbuffer();
    public extern WebGLShader createShader(int type);
    public extern WebGLTexture createTexture();
    public extern void cullFace(int mode);
    public extern void deleteBuffer(WebGLBuffer buffer);
    public extern void deleteFramebuffer(WebGLFramebuffer framebuffer);
    public extern void deleteProgram(WebGLProgram program);
    public extern void deleteRenderbuffer(WebGLRenderbuffer renderbuffer);
    public extern void deleteShader(WebGLShader shader);
    public extern void deleteTexture(WebGLTexture texture);
    public extern void depthFunc(int func);
    public extern void depthMask(bool flag);
    public extern void depthRange(float zNear, float zFar);
    public extern void detachShader(WebGLProgram program, WebGLShader shader);
    public extern void disable(int cap);
    public extern void disableVertexAttribArray(int index);
    public extern void drawArrays(int mode, int first, int count);
    public extern void drawElements(int mode, int count, int type, int offset);
    public extern void enable(int cap);
    public extern void enableVertexAttribArray(int index);
    public extern void finish();
    public extern void flush();
    public extern void framebufferRenderbuffer(int target, int attachment, int renderbuffertarget, WebGLRenderbuffer renderbuffer);
    public extern void framebufferTexture2D(int target, int attachment, int textarget, WebGLTexture texture, int level);
    public extern void frontFace(int mode);
    public extern void generateMipmap(int target);
    public extern WebGLActiveInfo getActiveAttrib(WebGLProgram program, int index);
    public extern WebGLActiveInfo getActiveUniform(WebGLProgram program, int index);
    public extern WebGLShader[] getAttachedShaders(WebGLProgram program);
    public extern int getAttribLocation(WebGLProgram program, string name);
    public extern object getBufferParameter(int target, int pname);
    public extern WebGLContextAttributes getContextAttributes();
    public extern int getError();
    public extern object getExtension(string name);
    public extern object getFramebufferAttachmentParameter(int target, int attachment, int pname);
    public extern object getParameter(int pname);
    public extern string getProgramInfoLog(WebGLProgram program);
    public extern object getProgramParameter(WebGLProgram program, int pname);
    public extern object getRenderbufferParameter(int target, int pname);
    public extern string getShaderInfoLog(WebGLShader shader);
    public extern object getShaderParameter(WebGLShader shader, int pname);
    public extern WebGLShaderPrecisionFormat getShaderPrecisionFormat(int shadertype, int precisiontype);
    public extern string getShaderSource(WebGLShader shader);
    public extern string[] getSupportedExtensions();
    public extern object getTexParameter(int target, int pname);
    public extern object getUniform(WebGLProgram program, WebGLUniformLocation location);
    public extern WebGLUniformLocation getUniformLocation(WebGLProgram program, string name);
    public extern object getVertexAttrib(int index, int pname);
    public extern int getVertexAttribOffset(int index, int pname);
    public extern object hint(int target, int mode);
    public extern bool isBuffer(WebGLBuffer buffer);
    public extern bool isContextLost();
    public extern bool isEnabled(int cap);
    public extern bool isFramebuffer(WebGLFramebuffer framebuffer);
    public extern bool isProgram(WebGLProgram program);
    public extern bool isRenderbuffer(WebGLRenderbuffer renderbuffer);
    public extern bool isShader(WebGLShader shader);
    public extern bool isTexture(WebGLTexture texture);
    public extern void lineWidth(int width);
    public extern void linkProgram(WebGLProgram program);
    public extern void pixelStorei(int pname, int param);
    public extern void polygonOffset(float factor, int units);
    public extern void readPixels(int x, int y, int width, int height, int format, int type, ArrayBufferView pixels);
    public extern void renderbufferStorage(int target, int internalformat, int width, int height);
    public extern void sampleCoverage(int value, bool invert);
    /// <summary>左下角0,0，宽高单位为像素</summary>
    public extern void scissor(int x, int y, int width, int height);
    public extern void shaderSource(WebGLShader shader, string source);
    public extern void stencilFunc(int func, int _ref, int mask);
    public extern void stencilFuncSeparate(int face, int func, int _ref, int mask);
    public extern void stencilMask(int mask);
    public extern void stencilMaskSeparate(int face, int mask);
    public extern void stencilOp(int fail, int zfail, int zpass);
    public extern void stencilOpSeparate(int face, int fail, int zfail, int zpass);
    public extern void texImage2D(int target, int level, int internalformat, int width, int height, int border, int format, int type, ArrayBufferView pixels);
    public extern void texImage2D(int target, int level, int internalformat, int format, int type, Image pixels);
    public extern void texImage2D(int target, int level, int internalformat, int format, int type, ImageData pixels);
    public extern void texParameterf(int target, int pname, int param);
    public extern void texParameteri(int target, int pname, int param);
    public extern void texSubImage2D(int target, int level, int xoffset, int yoffset, int width, int height, int format, int type, ArrayBufferView pixels);
    public extern void texSubImage2D(int target, int level, int xoffset, int yoffset, int format, int type, Image pixels);
    public extern void uniform1f(WebGLUniformLocation location, float x);
    public extern void uniform1fv(WebGLUniformLocation location, float[] v);
    public extern void uniform1i(WebGLUniformLocation location, int x);
    public extern void uniform1iv(WebGLUniformLocation location, int[] v);
    public extern void uniform2f(WebGLUniformLocation location, float x, float y);
    public extern void uniform2fv(WebGLUniformLocation location, float[] v);
    public extern void uniform2i(WebGLUniformLocation location, int x, int y);
    public extern void uniform2iv(WebGLUniformLocation location, int[] v);
    public extern void uniform3f(WebGLUniformLocation location, float x, float y, float z);
    public extern void uniform3fv(WebGLUniformLocation location, float[] v);
    public extern void uniform3i(WebGLUniformLocation location, int x, int y, int z);
    public extern void uniform3iv(WebGLUniformLocation location, int[] v);
    public extern void uniform4f(WebGLUniformLocation location, float x, float y, float z, float w);
    public extern void uniform4fv(WebGLUniformLocation location, float[] v);
    public extern void uniform4i(WebGLUniformLocation location, int x, int y, int z, int w);
    public extern void uniform4iv(WebGLUniformLocation location, int[] v);
    public extern void uniformMatrix2fv(WebGLUniformLocation location, bool transpose, float[] value);
    public extern void uniformMatrix3fv(WebGLUniformLocation location, bool transpose, float[] value);
    public extern void uniformMatrix4fv(WebGLUniformLocation location, bool transpose, float[] value);
    public extern void useProgram(WebGLProgram program);
    public extern void validateProgram(WebGLProgram program);
    public extern void vertexAttrib1f(int indx, float x);
    public extern void vertexAttrib1fv(int indx, float[] values);
    public extern void vertexAttrib2f(int indx, float x, float y);
    public extern void vertexAttrib2fv(int indx, float[] values);
    public extern void vertexAttrib3f(int indx, float x, float y, float z);
    public extern void vertexAttrib3fv(int indx, float[] values);
    public extern void vertexAttrib4f(int indx, float x, float y, float z, float w);
    public extern void vertexAttrib4fv(int indx, float[] values);
    public extern void vertexAttribPointer(int indx, int size, int type, bool normalized, int stride, int offset);
    /// <summary>左下角0,0，宽高单位为像素</summary>
    public extern void viewport(float x, float y, float width, float height);
    public int ACTIVE_ATTRIBUTES;
    public int ACTIVE_TEXTURE;
    public int ACTIVE_UNIFORMS;
    public int ALIASED_LINE_WIDTH_RANGE;
    public int ALIASED_POINT_SIZE_RANGE;
    public int ALPHA;
    public int ALPHA_BITS;
    public int ALWAYS;
    public int ARRAY_BUFFER;
    public int ARRAY_BUFFER_BINDING;
    public int ATTACHED_SHADERS;
    public int BACK;
    public int BLEND;
    public int BLEND_COLOR;
    public int BLEND_DST_ALPHA;
    public int BLEND_DST_RGB;
    public int BLEND_EQUATION;
    public int BLEND_EQUATION_ALPHA;
    public int BLEND_EQUATION_RGB;
    public int BLEND_SRC_ALPHA;
    public int BLEND_SRC_RGB;
    public int BLUE_BITS;
    public int BOOL;
    public int BOOL_VEC2;
    public int BOOL_VEC3;
    public int BOOL_VEC4;
    public int BROWSER_DEFAULT_WEBGL;
    public int BUFFER_SIZE;
    public int BUFFER_USAGE;
    public int BYTE;
    public int CCW;
    public int CLAMP_TO_EDGE;
    public int COLOR_ATTACHMENT0;
    public int COLOR_BUFFER_BIT;
    public int COLOR_CLEAR_VALUE;
    public int COLOR_WRITEMASK;
    public int COMPILE_STATUS;
    public int COMPRESSED_TEXTURE_FORMATS;
    public int CONSTANT_ALPHA;
    public int CONSTANT_COLOR;
    public int CONTEXT_LOST_WEBGL;
    public int CULL_FACE;
    public int CULL_FACE_MODE;
    public int CURRENT_PROGRAM;
    public int CURRENT_VERTEX_ATTRIB;
    public int CW;
    public int DECR;
    public int DECR_WRAP;
    public int DELETE_STATUS;
    public int DEPTH_ATTACHMENT;
    public int DEPTH_BITS;
    public int DEPTH_BUFFER_BIT;
    public int DEPTH_CLEAR_VALUE;
    public int DEPTH_COMPONENT;
    public int DEPTH_COMPONENT16;
    public int DEPTH_FUNC;
    public int DEPTH_RANGE;
    public int DEPTH_STENCIL;
    public int DEPTH_STENCIL_ATTACHMENT;
    public int DEPTH_TEST;
    public int DEPTH_WRITEMASK;
    public int DITHER;
    public int DONT_CARE;
    public int DST_ALPHA;
    public int DST_COLOR;
    public int DYNAMIC_DRAW;
    public int ELEMENT_ARRAY_BUFFER;
    public int ELEMENT_ARRAY_BUFFER_BINDING;
    public int EQUAL;
    public int FASTEST;
    public int FLOAT;
    public int FLOAT_MAT2;
    public int FLOAT_MAT3;
    public int FLOAT_MAT4;
    public int FLOAT_VEC2;
    public int FLOAT_VEC3;
    public int FLOAT_VEC4;
    public int FRAGMENT_SHADER;
    public int FRAMEBUFFER;
    public int FRAMEBUFFER_ATTACHMENT_OBJECT_NAME;
    public int FRAMEBUFFER_ATTACHMENT_OBJECT_TYPE;
    public int FRAMEBUFFER_ATTACHMENT_TEXTURE_CUBE_MAP_FACE;
    public int FRAMEBUFFER_ATTACHMENT_TEXTURE_LEVEL;
    public int FRAMEBUFFER_BINDING;
    public int FRAMEBUFFER_COMPLETE;
    public int FRAMEBUFFER_INCOMPLETE_ATTACHMENT;
    public int FRAMEBUFFER_INCOMPLETE_DIMENSIONS;
    public int FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT;
    public int FRAMEBUFFER_UNSUPPORTED;
    public int FRONT;
    public int FRONT_AND_BACK;
    public int FRONT_FACE;
    public int FUNC_ADD;
    public int FUNC_REVERSE_SUBTRACT;
    public int FUNC_SUBTRACT;
    public int GENERATE_MIPMAP_HINT;
    public int GEQUAL;
    public int GREATER;
    public int GREEN_BITS;
    public int HIGH_FLOAT;
    public int HIGH_INT;
    public int IMPLEMENTATION_COLOR_READ_FORMAT;
    public int IMPLEMENTATION_COLOR_READ_TYPE;
    public int INCR;
    public int INCR_WRAP;
    public int INT;
    public int INT_VEC2;
    public int INT_VEC3;
    public int INT_VEC4;
    public int INVALID_ENUM;
    public int INVALID_FRAMEBUFFER_OPERATION;
    public int INVALID_OPERATION;
    public int INVALID_VALUE;
    public int INVERT;
    public int KEEP;
    public int LEQUAL;
    public int LESS;
    public int LINEAR;
    public int LINEAR_MIPMAP_LINEAR;
    public int LINEAR_MIPMAP_NEAREST;
    public int LINES;
    public int LINE_LOOP;
    public int LINE_STRIP;
    public int LINE_WIDTH;
    public int LINK_STATUS;
    public int LOW_FLOAT;
    public int LOW_INT;
    public int LUMINANCE;
    public int LUMINANCE_ALPHA;
    public int MAX_COMBINED_TEXTURE_IMAGE_UNITS;
    public int MAX_CUBE_MAP_TEXTURE_SIZE;
    public int MAX_FRAGMENT_UNIFORM_VECTORS;
    public int MAX_RENDERBUFFER_SIZE;
    public int MAX_TEXTURE_IMAGE_UNITS;
    public int MAX_TEXTURE_SIZE;
    public int MAX_VARYING_VECTORS;
    public int MAX_VERTEX_ATTRIBS;
    public int MAX_VERTEX_TEXTURE_IMAGE_UNITS;
    public int MAX_VERTEX_UNIFORM_VECTORS;
    public int MAX_VIEWPORT_DIMS;
    public int MEDIUM_FLOAT;
    public int MEDIUM_INT;
    public int MIRRORED_REPEAT;
    public int NEAREST;
    public int NEAREST_MIPMAP_LINEAR;
    public int NEAREST_MIPMAP_NEAREST;
    public int NEVER;
    public int NICEST;
    public int NONE;
    public int NOTEQUAL;
    public int NO_ERROR;
    public int ONE;
    public int ONE_MINUS_CONSTANT_ALPHA;
    public int ONE_MINUS_CONSTANT_COLOR;
    public int ONE_MINUS_DST_ALPHA;
    public int ONE_MINUS_DST_COLOR;
    public int ONE_MINUS_SRC_ALPHA;
    public int ONE_MINUS_SRC_COLOR;
    public int OUT_OF_MEMORY;
    public int PACK_ALIGNMENT;
    public int POINTS;
    public int POLYGON_OFFSET_FACTOR;
    public int POLYGON_OFFSET_FILL;
    public int POLYGON_OFFSET_UNITS;
    public int RED_BITS;
    public int RENDERBUFFER;
    public int RENDERBUFFER_ALPHA_SIZE;
    public int RENDERBUFFER_BINDING;
    public int RENDERBUFFER_BLUE_SIZE;
    public int RENDERBUFFER_DEPTH_SIZE;
    public int RENDERBUFFER_GREEN_SIZE;
    public int RENDERBUFFER_HEIGHT;
    public int RENDERBUFFER_INTERNAL_FORMAT;
    public int RENDERBUFFER_RED_SIZE;
    public int RENDERBUFFER_STENCIL_SIZE;
    public int RENDERBUFFER_WIDTH;
    public int RENDERER;
    public int REPEAT;
    public int REPLACE;
    public int RGB;
    public int RGB565;
    public int RGB5_A1;
    public int RGBA;
    public int RGBA4;
    public int SAMPLER_2D;
    public int SAMPLER_CUBE;
    public int SAMPLES;
    public int SAMPLE_ALPHA_TO_COVERAGE;
    public int SAMPLE_BUFFERS;
    public int SAMPLE_COVERAGE;
    public int SAMPLE_COVERAGE_INVERT;
    public int SAMPLE_COVERAGE_VALUE;
    public int SCISSOR_BOX;
    public int SCISSOR_TEST;
    public int SHADER_TYPE;
    public int SHADING_LANGUAGE_VERSION;
    public int SHORT;
    public int SRC_ALPHA;
    public int SRC_ALPHA_SATURATE;
    public int SRC_COLOR;
    public int STATIC_DRAW;
    public int STENCIL_ATTACHMENT;
    public int STENCIL_BACK_FAIL;
    public int STENCIL_BACK_FUNC;
    public int STENCIL_BACK_PASS_DEPTH_FAIL;
    public int STENCIL_BACK_PASS_DEPTH_PASS;
    public int STENCIL_BACK_REF;
    public int STENCIL_BACK_VALUE_MASK;
    public int STENCIL_BACK_WRITEMASK;
    public int STENCIL_BITS;
    public int STENCIL_BUFFER_BIT;
    public int STENCIL_CLEAR_VALUE;
    public int STENCIL_FAIL;
    public int STENCIL_FUNC;
    public int STENCIL_INDEX;
    public int STENCIL_INDEX8;
    public int STENCIL_PASS_DEPTH_FAIL;
    public int STENCIL_PASS_DEPTH_PASS;
    public int STENCIL_REF;
    public int STENCIL_TEST;
    public int STENCIL_VALUE_MASK;
    public int STENCIL_WRITEMASK;
    public int STREAM_DRAW;
    public int SUBPIXEL_BITS;
    public int TEXTURE;
    public int TEXTURE0;
    public int TEXTURE1;
    public int TEXTURE10;
    public int TEXTURE11;
    public int TEXTURE12;
    public int TEXTURE13;
    public int TEXTURE14;
    public int TEXTURE15;
    public int TEXTURE16;
    public int TEXTURE17;
    public int TEXTURE18;
    public int TEXTURE19;
    public int TEXTURE2;
    public int TEXTURE20;
    public int TEXTURE21;
    public int TEXTURE22;
    public int TEXTURE23;
    public int TEXTURE24;
    public int TEXTURE25;
    public int TEXTURE26;
    public int TEXTURE27;
    public int TEXTURE28;
    public int TEXTURE29;
    public int TEXTURE3;
    public int TEXTURE30;
    public int TEXTURE31;
    public int TEXTURE4;
    public int TEXTURE5;
    public int TEXTURE6;
    public int TEXTURE7;
    public int TEXTURE8;
    public int TEXTURE9;
    public int TEXTURE_2D;
    public int TEXTURE_BINDING_2D;
    public int TEXTURE_BINDING_CUBE_MAP;
    public int TEXTURE_CUBE_MAP;
    public int TEXTURE_CUBE_MAP_NEGATIVE_X;
    public int TEXTURE_CUBE_MAP_NEGATIVE_Y;
    public int TEXTURE_CUBE_MAP_NEGATIVE_Z;
    public int TEXTURE_CUBE_MAP_POSITIVE_X;
    public int TEXTURE_CUBE_MAP_POSITIVE_Y;
    public int TEXTURE_CUBE_MAP_POSITIVE_Z;
    public int TEXTURE_MAG_FILTER;
    public int TEXTURE_MIN_FILTER;
    public int TEXTURE_WRAP_S;
    public int TEXTURE_WRAP_T;
    public int TRIANGLES;
    public int TRIANGLE_FAN;
    public int TRIANGLE_STRIP;
    public int UNPACK_ALIGNMENT;
    public int UNPACK_COLORSPACE_CONVERSION_WEBGL;
    public int UNPACK_FLIP_Y_WEBGL;
    public int UNPACK_PREMULTIPLY_ALPHA_WEBGL;
    public int UNSIGNED_BYTE;
    public int UNSIGNED_INT;
    public int UNSIGNED_SHORT;
    public int UNSIGNED_SHORT_4_4_4_4;
    public int UNSIGNED_SHORT_5_5_5_1;
    public int UNSIGNED_SHORT_5_6_5;
    public int VALIDATE_STATUS;
    public int VENDOR;
    public int VERSION;
    public int VERTEX_ATTRIB_ARRAY_BUFFER_BINDING;
    public int VERTEX_ATTRIB_ARRAY_ENABLED;
    public int VERTEX_ATTRIB_ARRAY_NORMALIZED;
    public int VERTEX_ATTRIB_ARRAY_POINTER;
    public int VERTEX_ATTRIB_ARRAY_SIZE;
    public int VERTEX_ATTRIB_ARRAY_STRIDE;
    public int VERTEX_ATTRIB_ARRAY_TYPE;
    public int VERTEX_SHADER;
    public int VIEWPORT;
    public int ZERO;
}
public struct TextMetrics
{
    public float width;
}
public class ImageData
{
    public Uint8ClampedArray data;
    //public byte[] data;
    public int height;
    public int width;
    public extern ImageData(int width, int height);
}
public class Uint8ClampedArray
{
    public int BYTES_PER_ELEMENT;
    public ArrayBuffer buffer;
    public int byteLength;
    public int byteOffset;
    public extern byte this[int index] { get; set; }
    public extern Uint8ClampedArray(ArrayBuffer buffer);
    public extern Uint8ClampedArray(ArrayBuffer buffer, int start, int length);
    public extern Uint8ClampedArray copyWithin(int target, int start, int end);
    public extern Uint8ClampedArray fill(int value, int start, int end);
    public int length;
    public extern Uint8ClampedArray reverse();
    public extern void set(int index, byte value);
    public extern void set(Uint8ClampedArray array, int offset);
    public extern Uint8ClampedArray slice(int start, int end);
}
public class Uint8Array : ArrayBuffer
{
    public int length;
    public int byteLength;
    public extern byte this[int index] { get; set; }
    public extern Uint8Array();
    public extern Uint8Array(ArrayBuffer buffer);
}
public class Float32Array : ArrayBuffer
{
    public int length;
    public int byteLength;
    public extern float this[int index] { get; set; }
    public extern Float32Array(int length);
    public extern Float32Array(float[] array);
    public extern Float32Array(ArrayBuffer buffer, int byteOffset, int length);
    public extern Float32Array subarray(int begin, int end);
}
public class Uint16Array : ArrayBuffer
{
    public extern ushort this[int index] { get; set; }
    public extern Uint16Array(int length);
    public extern Uint16Array(short[] array);
    public extern Uint16Array(ushort[] array);
    public extern Uint16Array(ArrayBuffer buffer, int byteOffset, int length);
}
public abstract class CanvasRenderingContext2D
{
    public HTMLCanvasElement canvas;
    public string fillStyle;
    public string font;
    public float globalAlpha;
    public string globalCompositeOperation;
    public string lineCap;
    public int lineDashOffset;
    public string lineJoin;
    public int lineWidth;
    public int miterLimit;
    public string msFillRule;
    public bool msImageSmoothingEnabled;
    public int shadowBlur;
    public string shadowColor;
    public int shadowOffsetX;
    public int shadowOffsetY;
    public string strokeStyle;
    public string textAlign;
    public string textBaseline;
    public bool mozImageSmoothingEnabled;
    public bool webkitImageSmoothingEnabled;
    public bool oImageSmoothingEnabled;
    public extern void beginPath();
    public extern void clearRect(int x, int y, int w, int h);
    public extern void clip(string fillRule);
    public extern ImageData createImageData(int imageDataOrSw, int sh);
    public extern void drawImage(Image image, int offsetX, int offsetY, int width, int height, int canvasOffsetX, int canvasOffsetY, int canvasImageWidth, int canvasImageHeight);
    public extern void fillRect(int x, int y, int w, int h);
    public extern void fillText(string text, int x, int y, int maxWidth);
    public extern ImageData getImageData(int sx, int sy, int sw, int sh);
    public extern TextMetrics measureText(string text);
    public extern void putImageData(ImageData imagedata, int dx, int dy, int dirtyX, int dirtyY, int dirtyWidth, int dirtyHeight);
    public extern void restore();
    public extern void rotate(float angle);
    public extern void save();
    public extern void scale(float x, float y);
    public extern void setTransform(float m11, float m12, float m21, float m22, float dx, float dy);
    public extern void stroke();
    public extern void strokeRect(int x, int y, int w, int h);
    public extern void strokeText(string text, int x, int y, int maxWidth);
    public extern void transform(float m11, float m12, float m21, float m22, float dx, float dy);
    public extern void translate(float x, float y);
}