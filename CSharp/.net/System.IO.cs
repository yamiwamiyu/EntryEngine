using __System;
using __System.Collections.Generic;
using __System.Linq;
using __System.Text;

namespace __System.IO
{
    public static partial class Path
    {
        public static readonly char DirectorySeparatorChar = '\\';
        internal const string DirectorySeparatorCharAsString = "\\";
        public static readonly char AltDirectorySeparatorChar = '/';
        public static readonly char VolumeSeparatorChar = ':';
        internal const int MAX_PATH = 260;
        internal static readonly char[] TrimEndChars = new char[]
		{
			'\t',
			'\n',
			'\v',
			'\f',
			'\r',
			' ',
			'\u0085',
			'\u00a0'
		};
        public static string ChangeExtension(string path, string extension)
        {
            if (path != null)
            {
                string text = path;
                int num = path.Length;
                while (--num >= 0)
                {
                    char c = path[num];
                    if (c == '.')
                    {
                        text = path.Substring(0, num);
                        break;
                    }
                    if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar || c == Path.VolumeSeparatorChar)
                    {
                        break;
                    }
                }
                if (extension != null && path.Length != 0)
                {
                    if (extension.Length == 0 || extension[0] != '.')
                    {
                        text += ".";
                    }
                    text += extension;
                }
                return text;
            }
            return null;
        }
        public static string Combine(string path1, string path2)
        {
            if (path1 == null || path2 == null)
            {
                throw new ArgumentNullException((path1 == null) ? "path1" : "path2");
            }
            if (path2.Length == 0)
            {
                return path1;
            }
            if (path1.Length == 0)
            {
                return path2;
            }
            if (Path.IsPathRooted(path2))
            {
                return path2;
            }
            char c = path1[path1.Length - 1];
            if (c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar && c != Path.VolumeSeparatorChar)
            {
                return path1 + "\\" + path2;
            }
            return path1 + path2;
        }
        public static string GetDirectoryName(string path)
        {
            if (path != null)
            {
                int index = path.LastIndexOf(DirectorySeparatorChar);
                if (index == -1) index = path.LastIndexOf(AltDirectorySeparatorChar);
                if (index == -1)
                    return path;
                return path.Substring(0, index);
            }
            return null;
        }
        public static string GetExtension(string path)
        {
            if (path == null)
            {
                return null;
            }
            int length = path.Length;
            int num = length;
            while (--num >= 0)
            {
                char c = path[num];
                if (c == '.')
                {
                    if (num != length - 1)
                    {
                        return path.Substring(num, length - num);
                    }
                    return string.Empty;
                }
                else
                {
                    if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar || c == Path.VolumeSeparatorChar)
                    {
                        break;
                    }
                }
            }
            return string.Empty;
        }
        public static string GetFileName(string path)
        {
            if (path != null)
            {
                int length = path.Length;
                int num = length;
                while (--num >= 0)
                {
                    char c = path[num];
                    if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar || c == Path.VolumeSeparatorChar)
                    {
                        return path.Substring(num + 1, length - num - 1);
                    }
                }
            }
            return path;
        }
        public static string GetFileNameWithoutExtension(string path)
        {
            path = Path.GetFileName(path);
            if (path == null)
            {
                return null;
            }
            int length;
            if ((length = path.LastIndexOf('.')) == -1)
            {
                return path;
            }
            return path.Substring(0, length);
        }
        [ASystemAPI]public static string GetFullPath(string path)
        {
            return path;
        }
        public static bool HasExtension(string path)
        {
            if (path != null)
            {
                int num = path.Length;
                while (--num >= 0)
                {
                    char c = path[num];
                    if (c == '.')
                    {
                        return num != path.Length - 1;
                    }
                    if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar || c == Path.VolumeSeparatorChar)
                    {
                        break;
                    }
                }
            }
            return false;
        }
        public static bool IsPathRooted(string path)
        {
            if (path != null)
            {
                int length = path.Length;
                if ((length >= 1 && (path[0] == Path.DirectorySeparatorChar || path[0] == Path.AltDirectorySeparatorChar)) || (length >= 2 && path[1] == Path.VolumeSeparatorChar))
                {
                    return true;
                }
            }
            return false;
        }
    }
    public static partial class File
    {
        [ASystemAPI]public static bool Exists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            return false;
        }
        [ASystemAPI]public static byte[] ReadAllBytes(string path)
        {
            throw new NotImplementedException();
        }
        public static string ReadAllText(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }
        public static string ReadAllText(string path, Encoding encoding)
        {
            byte[] buffer = ReadAllBytes(path);
            string result;
            StreamReader streamReader = new StreamReader(new MemoryStream(buffer), encoding);
            result = streamReader.ReadToEnd();
            streamReader.Close();
            return result;
        }
        [ASystemAPI]public static void WriteAllBytes(string path, byte[] bytes)
        {
            throw new NotImplementedException();
        }
        public static void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents, Encoding.UTF8);
        }
        public static void WriteAllText(string path, string contents, Encoding encoding)
        {
            byte[] buffer = encoding.GetBytes(contents);
            byte[] preamble = encoding.GetPreamble();
            if (preamble.Length > 0)
            {
                byte[] temp = new byte[buffer.Length + preamble.Length];
                System.Array.Copy(preamble, temp, preamble.Length);
                System.Array.Copy(buffer, 0, temp, preamble.Length, buffer.Length);
                buffer = temp;
            }
            WriteAllBytes(path, buffer);
        }
    }
    public abstract class TextReader : IDisposable
    {
        private sealed class NullTextReader : TextReader
        {
            public override int Read(char[] buffer, int index, int count)
            {
                return 0;
            }
            public override string ReadLine()
            {
                return null;
            }
        }
        public static readonly TextReader Null = new TextReader.NullTextReader();
        public virtual void Close()
        {
            this.Dispose(true);
        }
        public void Dispose()
        {
            this.Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
        }
        public virtual int Peek()
        {
            return -1;
        }
        public virtual int Read()
        {
            return -1;
        }
        public virtual int Read(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (buffer.Length - index < count)
            {
                throw new ArgumentException("Argument_InvalidOffLen");
            }
            int num = 0;
            do
            {
                int num2 = this.Read();
                if (num2 == -1)
                {
                    break;
                }
                buffer[index + num++] = (char)num2;
            }
            while (num < count);
            return num;
        }
        public virtual string ReadToEnd()
        {
            char[] array = new char[4096];
            StringBuilder stringBuilder = new StringBuilder(4096);
            int charCount;
            while ((charCount = this.Read(array, 0, array.Length)) != 0)
            {
                stringBuilder.Append(array, 0, charCount);
            }
            return stringBuilder.ToString();
        }
        public virtual int ReadBlock(char[] buffer, int index, int count)
        {
            int num = 0;
            int num2;
            do
            {
                num += (num2 = this.Read(buffer, index + num, count - num));
            }
            while (num2 > 0 && num < count);
            return num;
        }
        public virtual string ReadLine()
        {
            StringBuilder stringBuilder = new StringBuilder();
            int num;
            while (true)
            {
                num = this.Read();
                if (num == -1)
                {
                    if (stringBuilder.Length > 0)
                    {
                        return stringBuilder.ToString();
                    }
                    return null;
                }
                if (num == 13 || num == 10)
                {
                    break;
                }
                stringBuilder.Append((char)num);
            }
            if (num == 13 && this.Peek() == 10)
            {
                this.Read();
            }
            return stringBuilder.ToString();
        }
    }
    public class StreamReader : TextReader
    {
        private Stream stream;
        private Encoding encoding;
        private byte[] byteBuffer;
        private char[] charBuffer;
        private byte[] _preamble;
        private int charPos;
        private int charLen;
        private int byteLen;
        private int bytePos;
        private int _maxCharsPerBuffer;
        private bool _detectEncoding;
        private bool _checkPreamble;
        private bool _isBlocked;
        public virtual Encoding CurrentEncoding
        {
            get { return this.encoding; }
        }
        public virtual Stream BaseStream
        {
            get { return this.stream; }
        }
        public bool EndOfStream
        {
            get
            {
                if (this.stream == null)
                {
                    throw new ArgumentNullException("stream");
                }
                if (this.charPos < this.charLen)
                {
                    return false;
                }
                int num = this.ReadBuffer();
                return num == 0;
            }
        }
        internal StreamReader()
        {
        }
        public StreamReader(Stream stream)
            : this(stream, true)
        {
        }
        public StreamReader(Stream stream, bool detectEncodingFromByteOrderMarks)
            : this(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks, 1024)
        {
        }
        public StreamReader(Stream stream, Encoding encoding)
            : this(stream, encoding, true, 1024)
        {
        }
        public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
            : this(stream, encoding, detectEncodingFromByteOrderMarks, 1024)
        {
        }
        public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
        {
            if (stream == null || encoding == null)
            {
                throw new ArgumentNullException((stream == null) ? "stream" : "encoding");
            }
            if (!stream.CanRead)
            {
                throw new ArgumentException("Argument_StreamNotReadable");
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }
            this.Init(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize);
        }
        private void Init(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
        {
            this.stream = stream;
            this.encoding = encoding;
            if (bufferSize < 128)
            {
                bufferSize = 128;
            }
            this.byteBuffer = new byte[bufferSize];
            this._maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
            this.charBuffer = new char[this._maxCharsPerBuffer];
            this.byteLen = 0;
            this.bytePos = 0;
            this._detectEncoding = detectEncodingFromByteOrderMarks;
            this._preamble = encoding.GetPreamble();
            this._checkPreamble = (this._preamble.Length > 0);
            this._isBlocked = false;
        }
        public override void Close()
        {
            this.Dispose(true);
        }
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && this.stream != null)
                {
                    this.stream.Close();
                }
            }
            finally
            {
                if (this.stream != null)
                {
                    this.stream = null;
                    this.encoding = null;
                    this.byteBuffer = null;
                    this.charBuffer = null;
                    this.charPos = 0;
                    this.charLen = 0;
                }
            }
        }
        public override int Peek()
        {
            if (this.stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (this.charPos == this.charLen && (this._isBlocked || this.ReadBuffer() == 0))
            {
                return -1;
            }
            return (int)this.charBuffer[this.charPos];
        }
        public override int Read()
        {
            if (this.stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (this.charPos == this.charLen && this.ReadBuffer() == 0)
            {
                return -1;
            }
            int result = (int)this.charBuffer[this.charPos];
            this.charPos++;
            return result;
        }
        public override int Read(char[] buffer, int index, int count)
        {
            if (this.stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count");
            }
            if (buffer.Length - index < count)
            {
                throw new ArgumentException("Argument_InvalidOffLen");
            }
            int num = 0;
            bool flag = false;
            while (count > 0)
            {
                int num2 = this.charLen - this.charPos;
                if (num2 == 0)
                {
                    num2 = this.ReadBuffer(buffer, index + num, count, out flag);
                }
                if (num2 == 0)
                {
                    break;
                }
                if (num2 > count)
                {
                    num2 = count;
                }
                if (!flag)
                {
                    System.Array.Copy(this.charBuffer, this.charPos * 2, buffer, (index + num) * 2, num2 * 2);
                    this.charPos += num2;
                }
                num += num2;
                count -= num2;
                if (this._isBlocked)
                {
                    break;
                }
            }
            return num;
        }
        public override string ReadToEnd()
        {
            if (this.stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            StringBuilder stringBuilder = new StringBuilder(this.charLen - this.charPos);
            do
            {
                stringBuilder.Append(this.charBuffer, this.charPos, this.charLen - this.charPos);
                this.charPos = this.charLen;
                this.ReadBuffer();
            }
            while (this.charLen > 0);
            return stringBuilder.ToString();
        }
        private void CompressBuffer(int n)
        {
            System.Array.Copy(this.byteBuffer, n, this.byteBuffer, 0, this.byteLen - n);
            this.byteLen -= n;
        }
        private void DetectEncoding()
        {
            if (this.byteLen < 2)
            {
                return;
            }
            this._detectEncoding = false;
            bool flag = false;
            if (this.byteBuffer[0] == 254 && this.byteBuffer[1] == 255)
            {
                throw new NotImplementedException();
                //this.encoding = new UnicodeEncoding(true, true);
                this.CompressBuffer(2);
                flag = true;
            }
            else
            {
                if (this.byteBuffer[0] == 255 && this.byteBuffer[1] == 254)
                {
                    if (this.byteLen >= 4 && this.byteBuffer[2] == 0 && this.byteBuffer[3] == 0)
                    {
                        throw new NotImplementedException();
                        //this.encoding = new UTF32Encoding(false, true);
                        this.CompressBuffer(4);
                    }
                    else
                    {
                        throw new NotImplementedException();
                        //this.encoding = new UnicodeEncoding(false, true);
                        this.CompressBuffer(2);
                    }
                    flag = true;
                }
                else
                {
                    if (this.byteLen >= 3 && this.byteBuffer[0] == 239 && this.byteBuffer[1] == 187 && this.byteBuffer[2] == 191)
                    {
                        this.encoding = Encoding.UTF8;
                        this.CompressBuffer(3);
                        flag = true;
                    }
                    else
                    {
                        if (this.byteLen >= 4 && this.byteBuffer[0] == 0 && this.byteBuffer[1] == 0 && this.byteBuffer[2] == 254 && this.byteBuffer[3] == 255)
                        {
                            throw new NotImplementedException();
                            //this.encoding = new UTF32Encoding(true, true);
                            flag = true;
                        }
                        else
                        {
                            if (this.byteLen == 2)
                            {
                                this._detectEncoding = true;
                            }
                        }
                    }
                }
            }
            if (flag)
            {
                //this.decoder = this.encoding.GetDecoder();
                this._maxCharsPerBuffer = this.encoding.GetMaxCharCount(this.byteBuffer.Length);
                this.charBuffer = new char[this._maxCharsPerBuffer];
            }
        }
        private bool IsPreamble()
        {
            if (!this._checkPreamble)
            {
                return this._checkPreamble;
            }
            int num = (this.byteLen >= this._preamble.Length) ? (this._preamble.Length - this.bytePos) : (this.byteLen - this.bytePos);
            int i = 0;
            while (i < num)
            {
                if (this.byteBuffer[this.bytePos] != this._preamble[this.bytePos])
                {
                    this.bytePos = 0;
                    this._checkPreamble = false;
                    break;
                }
                i++;
                this.bytePos++;
            }
            if (this._checkPreamble && this.bytePos == this._preamble.Length)
            {
                this.CompressBuffer(this._preamble.Length);
                this.bytePos = 0;
                this._checkPreamble = false;
                this._detectEncoding = false;
            }
            return this._checkPreamble;
        }
        private int ReadBuffer()
        {
            this.charLen = 0;
            this.charPos = 0;
            if (!this._checkPreamble)
            {
                this.byteLen = 0;
            }
            while (true)
            {
                if (this._checkPreamble)
                {
                    int num = this.stream.Read(this.byteBuffer, this.bytePos, this.byteBuffer.Length - this.bytePos);
                    if (num == 0)
                    {
                        break;
                    }
                    this.byteLen += num;
                }
                else
                {
                    this.byteLen = this.stream.Read(this.byteBuffer, 0, this.byteBuffer.Length);
                    if (this.byteLen == 0)
                    {
                        return this.charLen;
                    }
                }
                this._isBlocked = (this.byteLen < this.byteBuffer.Length);
                if (!this.IsPreamble())
                {
                    if (this._detectEncoding && this.byteLen >= 2)
                    {
                        this.DetectEncoding();
                    }
                    this.charLen += this.encoding.GetChars(this.byteBuffer, 0, this.byteLen, this.charBuffer, this.charLen);
                }
                if (this.charLen != 0)
                {
                    return this.charLen;
                }
            }
            if (this.byteLen > 0)
            {
                this.charLen += this.encoding.GetChars(this.byteBuffer, 0, this.byteLen, this.charBuffer, this.charLen);
            }
            return this.charLen;
        }
        private int ReadBuffer(char[] userBuffer, int userOffset, int desiredChars, out bool readToUserBuffer)
        {
            this.charLen = 0;
            this.charPos = 0;
            if (!this._checkPreamble)
            {
                this.byteLen = 0;
            }
            int num = 0;
            readToUserBuffer = (desiredChars >= this._maxCharsPerBuffer);
            while (true)
            {
                if (this._checkPreamble)
                {
                    int num2 = this.stream.Read(this.byteBuffer, this.bytePos, this.byteBuffer.Length - this.bytePos);
                    if (num2 == 0)
                    {
                        break;
                    }
                    this.byteLen += num2;
                }
                else
                {
                    this.byteLen = this.stream.Read(this.byteBuffer, 0, this.byteBuffer.Length);
                    if (this.byteLen == 0)
                    {
                        return num;
                    }
                }
                this._isBlocked = (this.byteLen < this.byteBuffer.Length);
                if (!this.IsPreamble())
                {
                    if (this._detectEncoding && this.byteLen >= 2)
                    {
                        this.DetectEncoding();
                        readToUserBuffer = (desiredChars >= this._maxCharsPerBuffer);
                    }
                    this.charPos = 0;
                    if (readToUserBuffer)
                    {
                        num += this.encoding.GetChars(this.byteBuffer, 0, this.byteLen, userBuffer, userOffset + num);
                        this.charLen = 0;
                    }
                    else
                    {
                        num = this.encoding.GetChars(this.byteBuffer, 0, this.byteLen, this.charBuffer, num);
                        this.charLen += num;
                    }
                }
                if (num != 0)
                {
                    this._isBlocked &= (num < desiredChars);
                    return num;
                }
            }
            if (this.byteLen > 0)
            {
                if (readToUserBuffer)
                {
                    num += this.encoding.GetChars(this.byteBuffer, 0, this.byteLen, userBuffer, userOffset + num);
                    this.charLen = 0;
                }
                else
                {
                    num = this.encoding.GetChars(this.byteBuffer, 0, this.byteLen, this.charBuffer, num);
                    this.charLen += num;
                }
            }
            return num;
        }
        public override string ReadLine()
        {
            if (this.stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (this.charPos == this.charLen && this.ReadBuffer() == 0)
            {
                return null;
            }
            int num = this.charPos;
            char c;
            while (num < this.charLen)
            {
                c = this.charBuffer[num];
                num++;
                if (c == '\r' || c == '\n')
                    break;
            }
            if (num == this.charPos) return string.Empty;
            return new string(charBuffer, this.charPos, num - charPos);
        }
    }
    public abstract class Stream : IDisposable
    {
        protected static readonly byte[] _readByte = new byte[1];
        public abstract bool CanRead { get; }
        public abstract bool CanSeek { get; }
        public virtual bool CanTimeout { get { return false; } }
        public abstract bool CanWrite { get; }
        public abstract long Length { get; }
        public abstract long Position { get; set; }
        public virtual int ReadTimeout
        {
            get
            {
                throw new InvalidOperationException("InvalidOperation_TimeoutsNotSupported");
            }
            set
            {
                throw new InvalidOperationException("InvalidOperation_TimeoutsNotSupported");
            }
        }
        public virtual int WriteTimeout
        {
            get
            {
                throw new InvalidOperationException("InvalidOperation_TimeoutsNotSupported");
            }
            set
            {
                throw new InvalidOperationException("InvalidOperation_TimeoutsNotSupported");
            }
        }
        public virtual void Close()
        {
        }
        public void Dispose()
        {
            this.Close();
        }
        public abstract void Flush();
        public abstract int Read(byte[] buffer, int offset, int count);
        public abstract long Seek(long offset, SeekOrigin origin);
        public virtual int ReadByte()
        {
            if (this.Read(_readByte, 0, 1) == 0)
            {
                return -1;
            }
            return (int)_readByte[0];
        }
        public abstract void Write(byte[] buffer, int offset, int count);
        public virtual void WriteByte(byte value)
        {
            _readByte[0] = value;
            this.Write(_readByte, 0, 1);
        }
    }
    public enum SeekOrigin
    {
        Begin,
        Current,
        End
    }
    //public class FileStream : Stream
    //{
    //    private byte[] _buffer;
    //    private string _path;
    //    private string _fileName;
    //    private bool _canRead;
    //    private bool _canWrite;
    //    private bool _canSeek;
    //    private bool _isPipe;
    //    private int _readPos;
    //    private int _readLen;
    //    private int _writePos;
    //    private int _bufferSize;
    //    private long _pos;
    //    private long _appendStart;
    //    public override bool CanRead { get { return this._canRead; } }
    //    public override bool CanWrite { get { return this._canWrite; } }
    //    public override bool CanSeek { get { return this._canSeek; } }
    //    public override long Length { get { return _SAPI.GetFileLength(_path); } }
    //    public string Name { get { return this._fileName; } }
    //    public override long Position
    //    {
    //        get
    //        {
    //            return this._pos + (long)(this._readPos - this._readLen + this._writePos);
    //        }
    //        set
    //        {
    //            if (value < 0L)
    //            {
    //                throw new ArgumentOutOfRangeException("value");
    //            }
    //            if (this._writePos > 0)
    //            {
    //                this.FlushWrite();
    //            }
    //            this._readPos = 0;
    //            this._readLen = 0;
    //            this.Seek(value, SeekOrigin.Begin);
    //        }
    //    }
    //    public FileStream(string path, FileMode mode)
    //        : this(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite)
    //    {
    //    }
    //    public FileStream(string path, FileMode mode, FileAccess access)
    //    {
    //        if (path == null)
    //        {
    //            throw new ArgumentNullException("path");
    //        }
    //        if (path.Length == 0)
    //        {
    //            throw new ArgumentException("Argument_EmptyPath");
    //        }
    //        this._fileName = Path.GetFileName(path);
    //        int bufferSize = 4096;
    //        this._canRead = ((access & FileAccess.Read) != (FileAccess)0);
    //        this._canWrite = ((access & FileAccess.Write) != (FileAccess)0);
    //        this._canSeek = true;
    //        this._isPipe = false;
    //        this._pos = 0L;
    //        this._bufferSize = bufferSize;
    //        this._readPos = 0;
    //        this._readLen = 0;
    //        this._writePos = 0;
    //        if (flag)
    //        {
    //            this._appendStart = this.SeekCore(0L, SeekOrigin.End);
    //            return;
    //        }
    //        this._appendStart = -1L;
    //    }
    //    public override void Close()
    //    {
    //        try
    //        {
    //            if (this._writePos > 0)
    //            {
    //                this.FlushWrite();
    //            }
    //        }
    //        finally
    //        {
    //            if (this._handle != null && !this._handle.IsClosed)
    //            {
    //                this._handle.Dispose();
    //            }
    //            this._canRead = false;
    //            this._canWrite = false;
    //            this._canSeek = false;
    //        }
    //    }
    //    public override void Flush()
    //    {
    //        if (this._writePos > 0)
    //        {
    //            this.FlushWrite();
    //        }
    //        else
    //        {
    //            if (this._readPos < this._readLen && this.CanSeek)
    //            {
    //                this.FlushRead();
    //            }
    //        }
    //        this._readPos = 0;
    //        this._readLen = 0;
    //    }
    //    private void FlushRead()
    //    {
    //        if (this._readPos - this._readLen != 0)
    //        {
    //            this.SeekCore((long)(this._readPos - this._readLen), SeekOrigin.Current);
    //        }
    //        this._readPos = 0;
    //        this._readLen = 0;
    //    }
    //    private void FlushWrite()
    //    {
    //        this.WriteCore(this._buffer, 0, this._writePos);
    //        this._writePos = 0;
    //    }
    //    public override int Read(byte[] array, int offset, int count)
    //    {
    //        if (array == null)
    //        {
    //            throw new ArgumentNullException("array");
    //        }
    //        if (offset < 0)
    //        {
    //            throw new ArgumentOutOfRangeException("offset");
    //        }
    //        if (count < 0)
    //        {
    //            throw new ArgumentOutOfRangeException("count");
    //        }
    //        if (array.Length - offset < count)
    //        {
    //            throw new ArgumentException();
    //        }
    //        bool flag = false;
    //        int num = this._readLen - this._readPos;
    //        if (num == 0)
    //        {
    //            if (this._writePos > 0)
    //            {
    //                this.FlushWrite();
    //            }
    //            if (!this.CanSeek || count >= this._bufferSize)
    //            {
    //                num = this.ReadCore(array, offset, count);
    //                this._readPos = 0;
    //                this._readLen = 0;
    //                return num;
    //            }
    //            if (this._buffer == null)
    //            {
    //                this._buffer = new byte[this._bufferSize];
    //            }
    //            num = this.ReadCore(this._buffer, 0, this._bufferSize);
    //            if (num == 0)
    //            {
    //                return 0;
    //            }
    //            flag = (num < this._bufferSize);
    //            this._readPos = 0;
    //            this._readLen = num;
    //        }
    //        if (num > count)
    //        {
    //            num = count;
    //        }
    //        System.Array.Copy(this._buffer, this._readPos, array, offset, num);
    //        this._readPos += num;
    //        if (!this._isPipe && num < count && !flag)
    //        {
    //            int num2 = this.ReadCore(array, offset + num, count - num);
    //            num += num2;
    //            this._readPos = 0;
    //            this._readLen = 0;
    //        }
    //        return num;
    //    }
    //    public override int ReadByte()
    //    {
    //        if (this._readPos == this._readLen)
    //        {
    //            if (this._writePos > 0)
    //            {
    //                this.FlushWrite();
    //            }
    //            if (this._buffer == null)
    //            {
    //                this._buffer = new byte[this._bufferSize];
    //            }
    //            this._readLen = this.ReadCore(this._buffer, 0, this._bufferSize);
    //            this._readPos = 0;
    //        }
    //        if (this._readPos == this._readLen)
    //        {
    //            return -1;
    //        }
    //        int result = (int)this._buffer[this._readPos];
    //        this._readPos++;
    //        return result;
    //    }
    //    private int ReadCore(byte[] buffer, int offset, int count)
    //    {
    //        int num = 0;
    //        int num2 = this.ReadFileNative(this._handle, buffer, offset, count, null, out num);
    //        if (num2 == -1)
    //        {
    //            if (num == 109)
    //            {
    //                num2 = 0;
    //            }
    //            else
    //            {
    //                if (num == 87)
    //                {
    //                    throw new ArgumentException("Arg_HandleNotSync");
    //                }
    //                __Error.WinIOError(num, string.Empty);
    //            }
    //        }
    //        this._pos += (long)num2;
    //        return num2;
    //    }
    //    public override long Seek(long offset, SeekOrigin origin)
    //    {
    //        if (origin < SeekOrigin.Begin || origin > SeekOrigin.End)
    //        {
    //            throw new ArgumentException("Argument_InvalidSeekOrigin");
    //        }
    //        if (this._writePos > 0)
    //        {
    //            this.FlushWrite();
    //        }
    //        else
    //        {
    //            if (origin == SeekOrigin.Current)
    //            {
    //                offset -= (long)(this._readLen - this._readPos);
    //            }
    //        }
    //        long num = this._pos + (long)(this._readPos - this._readLen);
    //        long num2 = this.SeekCore(offset, origin);
    //        if (this._appendStart != -1L && num2 < this._appendStart)
    //        {
    //            this.SeekCore(num, SeekOrigin.Begin);
    //            throw new IOException(Environment.GetResourceString("IO.IO_SeekAppendOverwrite"));
    //        }
    //        if (this._readLen > 0)
    //        {
    //            if (num == num2)
    //            {
    //                if (this._readPos > 0)
    //                {
    //                    System.Array.Copy(this._buffer, this._readPos, this._buffer, 0, this._readLen - this._readPos);
    //                    this._readLen -= this._readPos;
    //                    this._readPos = 0;
    //                }
    //                if (this._readLen > 0)
    //                {
    //                    this.SeekCore((long)this._readLen, SeekOrigin.Current);
    //                }
    //            }
    //            else
    //            {
    //                if (num - (long)this._readPos < num2 && num2 < num + (long)this._readLen - (long)this._readPos)
    //                {
    //                    int num3 = (int)(num2 - num);
    //                    System.Array.Copy(this._buffer, this._readPos + num3, this._buffer, 0, this._readLen - (this._readPos + num3));
    //                    this._readLen -= this._readPos + num3;
    //                    this._readPos = 0;
    //                    if (this._readLen > 0)
    //                    {
    //                        this.SeekCore((long)this._readLen, SeekOrigin.Current);
    //                    }
    //                }
    //                else
    //                {
    //                    this._readPos = 0;
    //                    this._readLen = 0;
    //                }
    //            }
    //        }
    //        return num2;
    //    }
    //    private long SeekCore(long offset, SeekOrigin origin)
    //    {
    //        int num = 0;
    //        long num2 = Win32Native.SetFilePointer(this._handle, offset, origin, out num);
    //        if (num2 == -1L)
    //        {
    //            if (num == 6 && !this._handle.IsInvalid)
    //            {
    //                this._handle.Dispose();
    //            }
    //            __Error.WinIOError(num, string.Empty);
    //        }
    //        this._pos = num2;
    //        return num2;
    //    }
    //    public override void Write(byte[] array, int offset, int count)
    //    {
    //        if (array == null)
    //        {
    //            throw new ArgumentNullException("array");
    //        }
    //        if (offset < 0)
    //        {
    //            throw new ArgumentOutOfRangeException("offset");
    //        }
    //        if (count < 0)
    //        {
    //            throw new ArgumentOutOfRangeException("count");
    //        }
    //        if (array.Length - offset < count)
    //        {
    //            throw new ArgumentException("Argument_InvalidOffLen");
    //        }
    //        if (this._writePos == 0)
    //        {
    //            if (this._readPos < this._readLen)
    //            {
    //                this.FlushRead();
    //            }
    //            this._readPos = 0;
    //            this._readLen = 0;
    //        }
    //        if (this._writePos > 0)
    //        {
    //            int num = this._bufferSize - this._writePos;
    //            if (num > 0)
    //            {
    //                if (num > count)
    //                {
    //                    num = count;
    //                }
    //                System.Array.Copy(array, offset, this._buffer, this._writePos, num);
    //                this._writePos += num;
    //                if (count == num)
    //                {
    //                    return;
    //                }
    //                offset += num;
    //                count -= num;
    //            }
    //            this.WriteCore(this._buffer, 0, this._writePos);
    //            this._writePos = 0;
    //        }
    //        if (count >= this._bufferSize)
    //        {
    //            this.WriteCore(array, offset, count);
    //            return;
    //        }
    //        if (count == 0)
    //        {
    //            return;
    //        }
    //        if (this._buffer == null)
    //        {
    //            this._buffer = new byte[this._bufferSize];
    //        }
    //        System.Array.Copy(array, offset, this._buffer, this._writePos, count);
    //        this._writePos = count;
    //    }
    //    public override void WriteByte(byte value)
    //    {
    //        if (this._writePos == 0)
    //        {
    //            if (this._readPos < this._readLen)
    //            {
    //                this.FlushRead();
    //            }
    //            this._readPos = 0;
    //            this._readLen = 0;
    //            if (this._buffer == null)
    //            {
    //                this._buffer = new byte[this._bufferSize];
    //            }
    //        }
    //        if (this._writePos == this._bufferSize)
    //        {
    //            this.FlushWrite();
    //        }
    //        this._buffer[this._writePos] = value;
    //        this._writePos++;
    //    }
    //    private void WriteCore(byte[] buffer, int offset, int count)
    //    {
    //        int num = 0;
    //        int num2 = this.WriteFileNative(this._handle, buffer, offset, count, null, out num);
    //        if (num2 == -1)
    //        {
    //            if (num == 232)
    //            {
    //                num2 = 0;
    //            }
    //            else
    //            {
    //                if (num == 87)
    //                {
    //                    throw new IOException(Environment.GetResourceString("IO.IO_FileTooLongOrHandleNotSync"));
    //                }
    //                __Error.WinIOError(num, string.Empty);
    //            }
    //        }
    //        this._pos += (long)num2;
    //    }
    //}
    //public enum FileMode
    //{
    //    CreateNew = 1,
    //    Create,
    //    Open,
    //    OpenOrCreate,
    //    Truncate,
    //    Append
    //}
    //public enum FileAccess
    //{
    //    Read = 1,
    //    Write = 2,
    //    ReadWrite = 3
    //}
    public class MemoryStream : Stream
    {
        private byte[] _buffer;
        private int _origin;
        private int _position;
        private int _length;
        private int _capacity;
        private bool _writable;
        private bool _isOpen;
        public override bool CanRead { get { return this._isOpen; } }
        public override bool CanSeek { get { return this._isOpen; } }
        public override bool CanWrite { get { return this._writable; } }
        public virtual int Capacity
        {
            get
            {
                return this._capacity - this._origin;
            }
            set
            {
                if (value != this._capacity)
                {
                    if (value < this._length)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    if (value > 0)
                    {
                        byte[] array = new byte[value];
                        if (this._length > 0)
                        {
                            System.Array.Copy(this._buffer, 0, array, 0, this._length);
                        }
                        this._buffer = array;
                    }
                    else
                    {
                        this._buffer = null;
                    }
                    this._capacity = value;
                }
            }
        }
        public override long Length { get { return (long)(this._length - this._origin); } }
        public override long Position
        {
            get { return (long)(this._position - this._origin); }
            set { this._position = this._origin + (int)value; }
        }
        public MemoryStream() : this(0) { }
        public MemoryStream(int capacity)
        {
            this._buffer = new byte[capacity];
            this._capacity = capacity;
            this._writable = true;
            this._origin = 0;
            this._isOpen = true;
        }
        public MemoryStream(byte[] buffer)
            : this(buffer, true)
        {
        }
        public MemoryStream(byte[] buffer, bool writable)
        {
            this._buffer = buffer;
            this._length = (this._capacity = buffer.Length);
            this._writable = writable;
            this._origin = 0;
            this._isOpen = true;
        }
        public MemoryStream(byte[] buffer, int index, int count)
            : this(buffer, index, count, true)
        {
        }
        public MemoryStream(byte[] buffer, int index, int count, bool writable)
        {
            this._buffer = buffer;
            this._position = index;
            this._origin = index;
            this._length = (this._capacity = index + count);
            this._writable = writable;
            this._isOpen = true;
        }
        public override void Close()
        {
            this._isOpen = false;
            this._writable = false;
        }
        private bool EnsureCapacity(int value)
        {
            if (value > this._capacity)
            {
                int num = value;
                if (num < 256)
                {
                    num = 256;
                }
                if (num < this._capacity * 2)
                {
                    num = this._capacity * 2;
                }
                this.Capacity = num;
                return true;
            }
            return false;
        }
        public override void Flush()
        {
        }
        public virtual byte[] GetBuffer()
        {
            return this._buffer;
        }
        internal byte[] InternalGetBuffer()
        {
            return this._buffer;
        }
        internal void InternalGetOriginAndLength(out int origin, out int length)
        {
            origin = this._origin;
            length = this._length;
        }
        internal int InternalGetPosition()
        {
            return this._position;
        }
        internal int InternalReadInt32()
        {
            int num = this._position += 4;
            return (int)this._buffer[num - 4] | (int)this._buffer[num - 3] << 8 | (int)this._buffer[num - 2] << 16 | (int)this._buffer[num - 1] << 24;
        }
        internal int InternalEmulateRead(int count)
        {
            int num = this._length - this._position;
            if (num > count)
            {
                num = count;
            }
            if (num < 0)
            {
                num = 0;
            }
            this._position += num;
            return num;
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            int num = this._length - this._position;
            if (num > count)
            {
                num = count;
            }
            if (num <= 0)
            {
                return 0;
            }
            if (num <= 8)
            {
                int num2 = num;
                while (--num2 >= 0)
                {
                    buffer[offset + num2] = this._buffer[this._position + num2];
                }
            }
            else
            {
                System.Array.Copy(this._buffer, this._position, buffer, offset, num);
            }
            this._position += num;
            return num;
        }
        public override int ReadByte()
        {
            if (this._position >= this._length)
            {
                return -1;
            }
            return (int)this._buffer[this._position++];
        }
        public override long Seek(long offset, SeekOrigin loc)
        {
            switch (loc)
            {
                case SeekOrigin.Begin:
                    this._position = this._origin + (int)offset;
                    break;
                case SeekOrigin.Current:
                    this._position += (int)offset;
                    break;
                case SeekOrigin.End:
                    this._position = this._length + (int)offset;
                    break;
                default:
                    throw new ArgumentException("Argument_InvalidSeekOrigin");
            }
            return (long)this._position;
        }
        public virtual byte[] ToArray()
        {
            byte[] array = new byte[this._length - this._origin];
            System.Array.Copy(this._buffer, this._origin, array, 0, this._length - this._origin);
            return array;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            int num = this._position + count;
            if (num > this._length)
            {
                bool flag = this._position > this._length;
                if (num > this._capacity)
                {
                    bool flag2 = this.EnsureCapacity(num);
                    if (flag2)
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    System.Array.Clear(this._buffer, this._length, num - this._length);
                }
                this._length = num;
            }
            if (count <= 8)
            {
                int num2 = count;
                while (--num2 >= 0)
                {
                    this._buffer[this._position + num2] = buffer[offset + num2];
                }
            }
            else
            {
                System.Array.Copy(buffer, offset, this._buffer, this._position, count);
            }
            this._position = num;
        }
        public override void WriteByte(byte value)
        {
            if (this._position >= this._length)
            {
                int num = this._position + 1;
                bool flag = this._position > this._length;
                if (num >= this._capacity)
                {
                    bool flag2 = this.EnsureCapacity(num);
                    if (flag2)
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    System.Array.Clear(this._buffer, this._length, this._position - this._length);
                }
                this._length = num;
            }
            this._buffer[this._position++] = value;
        }
        public virtual void WriteTo(Stream stream)
        {
            stream.Write(this._buffer, this._origin, this._length - this._origin);
        }
    }
}
