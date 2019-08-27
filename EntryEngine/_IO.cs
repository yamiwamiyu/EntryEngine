using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using System.Linq;

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
                if (chars[i] > byte.MaxValue)
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
    public class SelectFile
    {
        internal _IO.iO io;
        internal object select;
        internal string file;
        private byte[] result;
        public string File { get { return file; } }
        public object SelectObject { get { return select; } }
        public byte[] Read()
        {
            if (result == null)
                result = io.ReadSelectFile(this);
            return result;
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
            public virtual void FileBrowser(string[] suffix, bool multiple, Action<SelectFile[]> onSelect)
            {
                throw new NotImplementedException();
            }
            public virtual void FileBrowserSave(string file, string[] suffix, Action<SelectFile> onSelect)
            {
                throw new NotImplementedException();
            }
            protected SelectFile CreateSelectFile(string file, object obj)
            {
                SelectFile select = new SelectFile();
                select.file = file;
                select.io = this;
                select.select = obj;
                return select;
            }
            protected internal virtual byte[] ReadSelectFile(SelectFile select)
            {
                return _ReadByte(select.file);
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
        public static int ReadStream(ref byte[] buffer, Stream stream)
        {
            int offset = 0;
            while (true)
            {
                int length = buffer.Length - offset;
                int read = stream.Read(buffer, offset, length);
                offset += read;
                if (read == 0)
                {
                    //if (offset < buffer.Length)
                    //{
                    //    Array.Resize(ref buffer, offset);
                    //}
                    break;
                }
                if (offset == buffer.Length)
                {
                    // 扩容继续读取
                    Array.Resize(ref buffer, offset * 2);
                }
            }
            return offset;
        }
        public static byte[] ReadStream(Stream stream, int initCapacity)
        {
            byte[] buffer = new byte[initCapacity];
            int read = ReadStream(ref buffer, stream);
            if (read < buffer.Length)
                Array.Resize(ref buffer, read);
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
    public class IO_NET : _IO.iO
    {
        protected override AsyncReadFile _ReadAsync(string file)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(file);
            AsyncReadFile async = new AsyncReadFile(this, file);
            request.BeginGetResponse((ar) =>
            {
                try
                {
                    var response = request.EndGetResponse(ar);
                    int size = (int)response.ContentLength;
                    if (size == -1) size = 65535;
                    byte[] buffer = _IO.ReadStream(response.GetResponseStream(), size);
                    async.SetData(buffer);
                }
                catch (Exception ex)
                {
                    async.Error(ex);
                }
            }, null);
            return async;
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
    public class EncryptRandom : IEncrypt
    {
        public void Encrypt(ref byte[] bytes)
        {
            int len = bytes.Length;
            byte[] random = new byte[len + 4];
            int seed = _RANDOM.Next();
            random[0] = (byte)seed;
            random[1] = (byte)(seed >> 8);
            random[2] = (byte)(seed >> 16);
            random[3] = (byte)(seed >> 24);
            Random __random = new Random(seed);
            for (int i = 0, index = 4; i < len; i++, index++)
            {
                random[index] = (byte)(bytes[i] + __random.Next(255));
            }
            bytes = random;
        }
        public void Decrypt(ref byte[] bytes)
        {
            int seed = (bytes[3] << 24) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0];
            Random __random = new Random(seed);
            int len = bytes.Length;
            byte[] origin = new byte[len - 4];
            for (int i = 0, index = 4; index < len; i++, index++)
            {
                origin[i] = (byte)(bytes[index] - __random.Next(255));
            }
            bytes = origin;
        }
    }

#if DEBUG
    public static class HotFix
    {
        class Filelist
        {
            public string File;
            public string Time;
            public long Length;
        }

        internal const string FIX_TEMP = "HotFix/";
        internal const string HOT_FIX_BAT = "__hotfix.bat";
        internal const string VERSION = "__version.txt";
        internal const string FILE_LIST = "__filelist.txt";
        internal const int PARALLEL = 10;
        public static readonly string[] SPLIT = new string[] { "\r\n" };
        public static string ServerURL;

        public static string ProcessText { get; private set; }
        public static bool NeedUpdate { get; private set; }
        public static float Progress { get; private set; }

        static string ReadString(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static IEnumerable<ICoroutine> Update()
        {
            if (string.IsNullOrEmpty(ServerURL))
                yield break;

            ProcessText = "正在检测最新版本号";

            // 新版本号
            WebRequest request = HttpWebRequest.Create(ServerURL + VERSION);
            WebResponse response = request.GetResponse();
            byte[] newVersionBuffer = _IO.ReadStream(response.GetResponseStream(), 8);

            // 旧版本号
            byte[] oldVersionBuffer = File.ReadAllBytes(VERSION);

            // 新旧版本比对
            if (oldVersionBuffer != null)
            {
                bool needUpdate = false;
                for (int i = 0; i < newVersionBuffer.Length; i++)
                {
                    if (newVersionBuffer[i] != oldVersionBuffer[i])
                    {
                        needUpdate = true;
                        break;
                    }
                }

                if (!needUpdate)
                    yield break;
            }

            NeedUpdate = true;

            ProcessText = "正在计算更新内容大小";
            yield return null;

            // 新文件列表
            request = WebRequest.Create(ServerURL + FILE_LIST);
            response = request.GetResponse();
            byte[] newFileListBuffer = _IO.ReadStream(response.GetResponseStream(), 65535);

            // 旧文件列表
            byte[] oldFileListBuffer = File.ReadAllBytes(FILE_LIST);
            string[] oldList = ReadString(oldFileListBuffer).Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);
            string[] newList = ReadString(newFileListBuffer).Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);

            // 暂时不删除不需要了的文件，只列出需要下载的新文件或者需要更新的文件
            Dictionary<string, Filelist> oldFilelist = new Dictionary<string, Filelist>();
            foreach (var item in oldList)
            {
                string[] splits = item.Split('\t');
                Filelist file = new Filelist();
                file.File = splits[0];
                file.Time = splits[1];
                file.Length = long.Parse(splits[2]);
                oldFilelist.Add(splits[0], file);
            }

            List<Filelist> newFilelist = new List<Filelist>();
            foreach (var item in newList)
            {
                string[] splits = item.Split('\t');
                Filelist file;
                if (oldFilelist.TryGetValue(splits[0], out file))
                {
                    // 时间不一致需要更新
                    if (file.Time != splits[1])
                    {
                        file.Length = long.Parse(splits[2]);
                        newFilelist.Add(file);
                        _LOG.Debug("Update: {0}", file.File);
                    }
                }
                else
                {
                    // 需要下载的新文件
                    file = new Filelist();
                    file.File = splits[0];
                    file.Time = splits[1];
                    file.Length = long.Parse(splits[2]);
                    newFilelist.Add(file);
                    _LOG.Debug("Download: {0}", file.File);
                }
            }

            // 可能只是删除了文件，导致没有需要更新的文件
            bool withDLL = false;
            if (newFilelist.Count > 0)
            {
                long needDownload = newFilelist.Sum(f => f.Length) >> 10;
                long download = 0;
                int parallel = 0;
                foreach (var item in newFilelist)
                {
                    while (parallel >= PARALLEL)
                    {
                        yield return null;
                    }
                    parallel++;
                    request = WebRequest.Create(ServerURL + item.File);
                    Filelist fileListItem = item;
                    _LOG.Debug("正在下载{0}", fileListItem.File);
                    request.BeginGetResponse(ar =>
                    {
                        try
                        {
                            WebResponse _response = ((WebRequest)ar.AsyncState).EndGetResponse(ar);
                            byte[] result = _IO.ReadStream(_response.GetResponseStream(), (int)fileListItem.Length);

                            string dir = Path.GetDirectoryName(fileListItem.File);
                            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                                Directory.CreateDirectory(dir);

                            if (fileListItem.File.EndsWith(".dll") || fileListItem.File.EndsWith(".exe") || fileListItem.File.EndsWith(".pdb"))
                            {
                                if (!Directory.Exists(FIX_TEMP))
                                    Directory.CreateDirectory(FIX_TEMP);
                                // 写入程序到临时文件夹，重启时通过临时文件夹拷贝程序覆盖原来的程序
                                withDLL = true;
                                //_LOG.Debug("下载DLL:" + FIX_TEMP + fileListItem.File);
                                File.WriteAllBytes(FIX_TEMP + fileListItem.File, result);
                            }
                            else
                            {
                                _IO.WriteByte(fileListItem.File, result);
                            }
                        }
                        catch (Exception ex)
                        {
                            _LOG.Error("下载文件{0}失败 Error:{1}", fileListItem.File, ex.Message);
                        }
                        finally
                        {
                            // todo:每完成一个下载都写入旧文件列表，这样中途退出下次也能接着上次中断的文件开始下载
                            download += fileListItem.Length >> 10;
                            Progress = (float)Math.Min(download * 1.0 / needDownload, 1);
                            ProcessText = string.Format("正在更新：{0}kb / {1}kb", download, needDownload);
                            parallel--;
                        }
                    }, request);
                }
                while (parallel > 0)
                    yield return null;
                // 写入新文件列表
                File.WriteAllBytes(FILE_LIST, newFileListBuffer);
            }

            // 写入新版本列表
            File.WriteAllBytes(VERSION, newVersionBuffer);

            if (withDLL)
            {
                string update_bat =
@"taskkill /PID {0}
cd {1}
xcopy /Y *.* ..\
cd ..\
start {2}
del {3}
";
                // 关闭程序并启动批处理来启动程序
                File.WriteAllText(HOT_FIX_BAT,
                    string.Format(update_bat, Process.GetCurrentProcess().Id, FIX_TEMP, 
                    System.Reflection.Assembly.GetEntryAssembly().Location, HOT_FIX_BAT), Encoding.Default);
                Process.Start(HOT_FIX_BAT);
            }
        }
    }
#endif

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
