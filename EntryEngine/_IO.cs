using System;
using System.IO;
using System.Text;

namespace EntryEngine
{
    /// <summary>char[]与byte[]直接转换，char>255则抛出异常</summary>
    public class SingleEncoding : Encoding
    {
        private static SingleEncoding single;
        public static SingleEncoding Single
        {
            get
            {
                if (single == null)
                    single = new SingleEncoding();
                return single;
            }
        }
        public override bool IsSingleByte
        {
            get { return true; }
        }
        public override int GetByteCount(char[] chars, int index, int count)
        {
            int bc = 0;
            for (int i = index, n = index + count; i < n; i++)
            {
                if (chars[i] > char.MaxValue)
                {
                    throw new ArgumentException();
                }
                else
                {
                    bc++;
                }
            }
            return bc;
        }
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            for (int i = 0; i < charCount; i++)
                bytes[byteIndex + i] = (byte)chars[charIndex + i];
            return charCount;
        }
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            for (int i = 0; i < byteCount; i++)
                chars[charIndex + i] = (char)bytes[byteIndex + i];
            return byteCount;
        }
        public override int GetMaxByteCount(int charCount)
        {
            return 1;
        }
        public override int GetMaxCharCount(int byteCount)
        {
            return 1;
        }
    }
    public static partial class _IO
    {
        [ADefaultValue(typeof(iO))]
        [ADevice("_IO._iO")]
        public class iO
        {
            private Encoding ioEncoding = Encoding.UTF8;
            public event ActionRef<byte[]> OnReadByte;

            public Encoding IOEncoding
            {
                get { return ioEncoding; }
                set
                {
                    if (value == null)
                        throw new ArgumentNullException("encoding");
                    ioEncoding = value;
                }
            }
            public virtual string RootDirectory
            {
                get;
                set;
            }

            protected internal iO() { this.RootDirectory = string.Empty; }
            [ADeviceNew]
            public iO(string root)
            {
                this.RootDirectory = root;
            }

            internal byte[] _OnReadByte(byte[] data)
            {
                if (OnReadByte != null)
                    OnReadByte(ref data);
                return data;
            }
            public string BuildPath(string file)
            {
                //if (!string.IsNullOrEmpty(RootDirectory))
                //{
                //    // Path.Combine在mono，带'\'的路径会出现"\/"的错误路径
                //    file = Path.Combine(RootDirectory, file);
                //}
                file = PathCombine(RootDirectory, file);
                file = file.Replace(SPLIT, '/');
                return file;
            }
            public string ReadText(string file)
            {
                return _ReadText(BuildPath(file));
            }
            public byte[] ReadByte(string file)
            {
                return _OnReadByte(_ReadByte(BuildPath(file)));
            }
            public AsyncReadFile ReadAsync(string file)
            {
                return _ReadAsync(BuildPath(file));
            }
            public string ReadPreambleText(byte[] bytes)
            {
                return ReadPreambleText(bytes, ioEncoding);
            }
            public void WriteText(string file, string content)
            {
                WriteText(file, content, ioEncoding);
            }
            public void WriteText(string file, string content, Encoding encoding)
            {
                _WriteText(BuildPath(file), content, encoding);
            }
            public void WriteByte(string file, byte[] content)
            {
                _WriteByte(BuildPath(file), content);
            }

            protected virtual AsyncReadFile _ReadAsync(string file)
            {
                AsyncReadFile sync = new AsyncReadFile(this, file);
                sync.SetData(_ReadByte(file));
                return sync;
            }
            protected virtual string _ReadText(string file)
            {
                return File.ReadAllText(file, ioEncoding);
            }
            protected virtual byte[] _ReadByte(string file)
            {
                return File.ReadAllBytes(file);
            }
            protected virtual void _WriteText(string file, string content, Encoding encoding)
            {
                File.WriteAllText(file, content, encoding);
            }
            protected virtual void _WriteByte(string file, byte[] content)
            {
                File.WriteAllBytes(file, content);
            }

            public static string ReadPreambleText(string text, Encoding encoding)
            {
                return ReadPreambleText(encoding.GetBytes(text), encoding);
            }
            public static string ReadPreambleText(byte[] bytes, Encoding encoding)
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    return ReadPreambleText(stream, encoding);
                }
            }
            public static string ReadPreambleText(Stream stream, Encoding encoding)
            {
                StreamReader reader = new StreamReader(stream, encoding);
                return reader.ReadToEnd();
            }
        }


        public const char SPLIT = '\\';
        private const string BACK_DIRECTORY = "..\\";
        private readonly static char[] SPLIT_1 = { '\\' };
        private readonly static char[] SPLIT_2 = { '/' };
        private readonly static byte[] STREAM_BUFFER = new byte[1];
        private static uint[] crc32;


        static _IO()
        {
            crc32 = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint r = i;
                for (int j = 0; j < 8; j++)
                    if ((r & 1) != 0)
                        r = (r >> 1) ^ 0xEDB88320;
                    else
                        r >>= 1;
                crc32[i] = r;
            }
        }


        public static string PathCombine(string p1, string p2)
        {
            if (!string.IsNullOrEmpty(p1))
            {
                char c = p1[p1.Length - 1];
                if (c == SPLIT_2[0] || c == SPLIT_1[0])
                    p2 = p1 + p2;
                else
                    p2 = p1 + "\\" + p2;
                // Path.Combine在mono，带'\'的路径会出现"\/"的错误路径
                //file = Path.Combine(RootDirectory, file);
            }
            return p2;
        }
        private static string RelativeTree(string[] target, string[] path, ref int depth)
        {
            bool flagBack = false;
            bool flagForward = false;
            if (depth >= target.Length)
                flagBack = true;
            else if (depth >= path.Length)
                flagForward = true;
            else if (target[depth] != path[depth])
            {
                flagBack = true;
                flagForward = true;
            }

            StringBuilder relative = null;
            bool flag = flagBack || flagForward;
            if (flag)
                relative = new StringBuilder();

            if (flagBack)
            {
                for (int i = depth; i < path.Length; i++)
                {
                    relative.Append(BACK_DIRECTORY);
                }
            }

            if (flagForward)
            {
                for (int i = depth; i < target.Length; i++)
                {
                    relative.Append(target[i]);
                    if (i != target.Length - 1)
                        relative.Append(SPLIT);
                }
            }

            if (flag)
            {
                return relative.ToString();
            }
            else
            {
                depth++;
                return RelativeTree(target, path, ref depth);
            }
        }
        private static string[] PathTree(string path, bool directory)
        {
            if (!Path.IsPathRooted(path))
                path = PathCombine(Environment.CurrentDirectory, path);
            path = Path.GetFullPath(path);
            if (directory)
                path = Path.GetDirectoryName(path);
            if (path.IndexOf(SPLIT) != -1)
                return path.Split(SPLIT_1, StringSplitOptions.RemoveEmptyEntries);
            else
                return path.Split(SPLIT_2, StringSplitOptions.RemoveEmptyEntries);
        }
        public static string RelativePath(string target, string path)
        {
            int depth = 0;
            return RelativeTree(PathTree(target, false), PathTree(path, true), ref depth);
        }
        public static string RelativePathForward(string target, string path)
        {
            int depth = 0;
            var treePath = PathTree(path, true);
            string result = RelativeTree(PathTree(target, false), treePath, ref depth);
            if (depth >= treePath.Length)
                return result;
            else
                return null;
        }
        public static string RelativeDirectory(string target, string path)
        {
            string relative = RelativePath(target, path);
            if (relative != null && relative.IndexOf('.') != -1)
                relative = relative.Substring(0, relative.LastIndexOf(SPLIT) + 1);
            return relative;
        }
        public static string DirectoryWithEnding(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return string.Empty;
            else if (dir.EndsWith("/") || dir.EndsWith("\\"))
                return dir;
            else
                return dir + SPLIT;
        }
        public static byte[] ReadStream(Stream stream)
        {
            return ReadStream(stream, 128);
        }
        public static byte[] ReadStream(Stream stream, int initCapacity)
        {
            byte[] buffer = new byte[initCapacity];
            int offset = 0;
            while (true)
            {
                int length = buffer.Length - offset;
                int read = stream.Read(buffer, offset, length);
                offset += read;
                if (read <= length)
                {
                    byte[] result = new byte[offset];
                    Array.Copy(buffer, result, offset);
                    buffer = result;
                    break;
                }
                else
                {
                    // 尝试是否已读取完毕
                    read = stream.Read(STREAM_BUFFER, offset, 1);
                    if (read == 0)
                        break;
                    // 扩容继续读取
                    Array.Resize(ref buffer, offset * 2);
                    // 将尝试读取的那个字节数据复制到扩容后的缓冲
                    buffer[offset++] = STREAM_BUFFER[0];
                }
            }
            return buffer;
        }
        public static void SetDecrypt(iO io)
        {
            io.OnReadByte += new EncryptInvert().Decrypt;
            io.OnReadByte += new EncryptShuffle().Decrypt;
        }
        public static uint Crc32(byte[] data, int start, int length)
        {
            uint crc = uint.MaxValue;
            //int end = _MATH.Min(start + length, data.Length);
            int end = start + length;
            for (int i = start; i < end; i++)
            {
                crc = (crc >> 8 & 4294967295u) ^ crc32[(crc ^ data[i]) & 255];
            }
            return crc;
        }
    }
    public class AsyncReadFile : AsyncData<byte[]>
    {
        public _IO.iO IO
        {
            get;
            private set;
        }
        public string File
        {
            get;
            private set;
        }
        public AsyncReadFile(_IO.iO io, string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException("file");
            this.IO = io;
            this.File = file;
        }
        protected sealed override void OnSetData(ref byte[] data)
        {
            data = IO._OnReadByte(data);
            base.OnSetData(ref data);
        }
    }

    public interface IEncrypt
    {
        void Encrypt(ref byte[] bytes);
        void Decrypt(ref byte[] bytes);
    }
    public class EncryptInvert : IEncrypt
    {
        public void Encrypt(ref byte[] bytes)
        {
            for (int i = 0, n = bytes.Length; i < n; i++)
                bytes[i] = (byte)(byte.MaxValue - bytes[i]);
        }
        public void Decrypt(ref byte[] bytes)
        {
            Encrypt(ref bytes);
        }
    }
    public class EncryptShuffle : IEncrypt
    {
        private _RANDOM.Random BuildRandom(byte[] bytes)
        {
            int seed = 0;
            for (int i = 0, len = bytes.Length; i < len; i++)
                seed += bytes[i];
            return new RandomDotNet(seed);
        }
        public void Encrypt(ref byte[] bytes)
        {
            _RANDOM.Random random = BuildRandom(bytes);

            int len = bytes.Length;
            int[] array = new int[len];
            for (int i = 0; i < len; i++)
                array[i] = random.Next(len);

            for (int i = len - 1; i >= 0; i--)
                Utility.Swap(ref bytes[i], ref bytes[array[i]]);
        }
        public void Decrypt(ref byte[] bytes)
        {
            _RANDOM.Random random = BuildRandom(bytes);

            for (int i = 0, len = bytes.Length; i < len; i++)
                Utility.Swap(ref bytes[i], ref bytes[random.Next(len)]);
        }
    }

    #region old implements

    //public class AsyncLoadFileHttp : AsyncLoadFile
    //{
    //    public static uint LocalSyncSize = uint.MaxValue;

    //    private HttpWebRequest web;
    //    private FileStream local;

    //    protected override void LoadLocal()
    //    {
    //        CheckClient();
    //        local = new FileStream(File.File, FileMode.Open, FileAccess.Read);
    //        long length = File.Length;
    //        if (length == 0)
    //            length = local.Length - File.Offset;
    //        if (length <= LocalSyncSize)
    //        {
    //            Bytes = IODotNet.Read(local, File.Offset, length);
    //        }
    //        else
    //        {
    //            byte[] buffer = new byte[length];
    //            local.BeginRead(buffer, 0, buffer.Length, (result) =>
    //            {
    //                if (local == null)
    //                    return;
    //                int read = local.EndRead(result);
    //                if (result.IsCompleted)
    //                    Bytes = buffer;
    //            }, null);
    //        }
    //    }
    //    protected override void LoadRemote()
    //    {
    //        CheckClient();

    //        string protocol = File.Protocol;
    //        if (protocol == "http" || protocol == "https")
    //        {
    //            web = (HttpWebRequest)WebRequest.Create(File.File);
    //            web.KeepAlive = false;
    //            // .Net 2.0 System.dll
    //            // private bool AddRange(string, string, string)
    //            if (File.Offset > 0 || File.Length > 0)
    //            {
    //                var method = typeof(HttpWebRequest).GetMethod("AddRange", BindingFlags.NonPublic | BindingFlags.Instance);
    //                method.Invoke(web, new object[] { "Range", 
    //                    File.Offset.ToString(System.Globalization.NumberFormatInfo.InvariantInfo), 
    //                    File.Length > 0 ? File.Length.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : null });
    //            }

    //            web.BeginGetResponse(WebRequestCallback, null);
    //        }
    //        else
    //        {
    //            throw new NotImplementedException("Not supported protocol " + protocol);
    //        }
    //    }
    //    private void WebRequestCallback(IAsyncResult ar)
    //    {
    //        if (web == null)
    //            return;

    //        var response = (HttpWebResponse)web.EndGetResponse(ar);
    //        try
    //        {
    //            byte[] buffer = new byte[response.ContentLength];
    //            using (Stream stream = response.GetResponseStream())
    //            {
    //                foreach (float progress in IODotNet.Read(stream, ref buffer, 0))
    //                    ProgressFloat = 1.0f * progress / buffer.Length;
    //            }
    //            Bytes = buffer;
    //        }
    //        catch (Exception ex)
    //        {
    //            Error(ex);
    //        }
    //        finally
    //        {
    //            response.Close();
    //        }
    //    }
    //    private void CheckClient()
    //    {
    //        if (web != null || local != null)
    //            throw new InvalidOperationException("WebClient download had been started.");
    //    }
    //    protected override void InternalComplete()
    //    {
    //        if (web != null)
    //        {
    //            if (State == EAsyncState.Canceled)
    //            {
    //                web.Abort();
    //            }
    //            web = null;
    //        }

    //        if (local != null)
    //        {
    //            local.Close();
    //            local.Dispose();
    //            local = null;
    //        }
    //    }
    //}

    #endregion
}
