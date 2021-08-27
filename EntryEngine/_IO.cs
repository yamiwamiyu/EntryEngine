using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using System.Linq;
using EntryEngine.Network;

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
    /// <summary>IO操作</summary>
    public static partial class _IO
    {
        [ADefaultValue(typeof(iO))]
        [ADevice("_IO._iO")]
        public class iO
        {
            private Encoding ioEncoding = Encoding.UTF8;
            //public event ActionRef<byte[]> OnWriteByte;
            public event ActionRef<byte[]> OnReadByte;

            /// <summary>加载文字内容的编码</summary>
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
            /// <summary>根目录</summary>
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

            public void ClearOnReadByte()
            {
                OnReadByte = null;
            }
            public byte[] _OnReadByte(byte[] data)
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
            /// <summary>向IO读取文字内容</summary>
            public string ReadText(string file)
            {
                return ReadPreambleText(ReadByte(file));
            }
            /// <summary>向IO读取二进制内容</summary>
            public byte[] ReadByte(string file)
            {
                return _OnReadByte(_ReadByte(BuildPath(file)));
            }
            /// <summary>向IO异步读取二进制内容</summary>
            public AsyncReadFile ReadAsync(string file)
            {
                return _ReadAsync(BuildPath(file));
            }
            /// <summary>将二进制内容转换成文字内容</summary>
            public string ReadPreambleText(byte[] bytes)
            {
                return _IO.ReadPreambleText(bytes, ioEncoding);
            }
            /// <summary>向IO写入文字内容</summary>
            public void WriteText(string file, string content)
            {
                WriteText(file, content, ioEncoding);
            }
            /// <summary>向IO写入文字内容</summary>
            public void WriteText(string file, string content, Encoding encoding)
            {
                WriteByte(BuildPath(file), encoding.GetBytes(content));
            }
            /// <summary>向IO写入二进制内容</summary>
            public void WriteByte(string file, byte[] content)
            {
                //_WriteByte(BuildPath(file), _OnWriteByte(content));
                _WriteByte(BuildPath(file), content);
            }

            protected virtual AsyncReadFile _ReadAsync(string file)
            {
                AsyncReadFile sync = new AsyncReadFile(this, file);
                sync.SetData(_ReadByte(file));
                return sync;
            }
            protected virtual byte[] _ReadByte(string file)
            {
                return File.ReadAllBytes(file);
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


        /// <summary>文件名去掉后缀名</summary>
        public static string WithoutExtention(this string filepath)
        {
            int index = filepath.LastIndexOf('.');
            if (index == -1) return filepath;
            else return filepath.Substring(0, index);
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
        public static byte[] ReadStream(Stream stream)
        {
            int length = 128;
            try
            {
                if (stream.Length > int.MaxValue)
                    length = int.MaxValue;
                else
                    length = (int)stream.Length;
            }
            catch
            {
            }
            return ReadStream(stream, length);
        }
        public static int ReadStream(ref byte[] buffer, Stream stream)
        {
            int offset = 0;
            while (true)
            {
                int length = buffer.Length - offset;
                //if (length > int.MaxValue)
                //    length = int.MaxValue;
                int read = stream.Read(buffer, offset, length);
                if (read == 0)
                {
                    //if (offset < buffer.Length)
                    //{
                    //    Array.Resize(ref buffer, offset);
                    //}
                    break;
                }
                offset += read;
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
        //public static void SetEncrypt(iO io)
        //{
        //    io.OnWriteByte += new EncryptShuffle().Encrypt;
        //    io.OnWriteByte += new EncryptInvert().Encrypt;
        //}
        //public static void SetEncryptDecrypt(iO io)
        //{
        //    EncryptInvert invert = new EncryptInvert();
        //    EncryptShuffle shuffle = new EncryptShuffle();
        //    io.OnReadByte += invert.Decrypt;
        //    io.OnReadByte += shuffle.Decrypt;
        //    io.OnWriteByte += shuffle.Encrypt;
        //    io.OnWriteByte += invert.Encrypt;
        //}
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
    public enum EHotFix
    {
        /// <summary>热更新流程刚开始</summary>
        正在检测最新版本号,
        /// <summary>已准备好NewVersionBytes</summary>
        正在获取最新版本内容,
        /// <summary>已准备好NewFileListBytes & NewFileList，有不需要更新的文件在此阶段删除</summary>
        开始更新,
        /// <summary>所有更新已经完成</summary>
        更新完成,
    }
    /// <summary>程序热更新：入口 -> 下载比对版本号 -> 更新dll -> 设置ServerURL -> 打开程序 -> 程序自行热更
    /// <para>若IO设置了资源加密，请将解密程序在热更新后设置</para>
    /// <para>可在[EHotFix.开始更新]阶段的迭代中，实时更新Progress</para>
    /// </summary>
    public static class _HOT_FIX
    {
        /// <summary>一个需要新下载或更新的文件</summary>
        public class FileNew
        {
            /// <summary>文件路径</summary>
            public string File;
            /// <summary>文件最后修改时间</summary>
            public string Time;
            /// <summary>文件大小，单位字节</summary>
            public long Length;

            /// <summary>文件下载时的异常</summary>
            public Exception UpdateException { get; set; }

            /// <summary>可以生成文件列表中一个文件的格式</summary>
            public override string ToString()
            {
                return string.Format("{0}\t{1}\t{2}\r\n", File, Time, Length);
            }
        }
        class FileDownload
        {
            public FileNew File;
            public AsyncHttpRequest Web;
        }

        internal const string FIX_TEMP = "HotFix/";
        internal const string HOT_FIX_BAT = "__hotfix.bat";
        /// <summary>版本文件</summary>
        public const string VERSION = "__version.txt";
        public const string FILE_LIST = "__filelist.txt";
        /// <summary>并行下载数</summary>
        public static int PARALLEL = 8;
        static string[] SPLIT = new string[] { "\r\n" };
        /// <summary>热更新服务器地址，例如http://127.0.0.1:88/，Unity若有热更会自动设置此值</summary>
        public static string ServerURL;

        /// <summary>热更新进程发生变化时触发</summary>
        public static event Action<EHotFix> OnProcess;
        /// <summary>热更新发生异常时触发，返回true就中止热更新</summary>
        public static event Func<AsyncHttpRequest, bool> OnError;
        /// <summary>文件下载完毕后，可以修改文件路径或下载的数据内容，之后将写入文件（此事件在同步线程上触发）</summary>
        public static event Func<FileNew, byte[], byte[]> OnDownload;

        /// <summary>新版本号，热更后清空；null则自动下载VERSION文件作为此值；外部传入可以避免重复下载，更新热更新程序时用</summary>
        public static byte[] NewVersionBytes;
        /// <summary>新文件列表，热更后清空；null则自动下载FILE_LIST文件作为此值；外部传入可以避免重复下载，更新热更新程序时用</summary>
        public static byte[] NewFileListBytes;
        public static long VersionOld { get; private set; }
        public static long Version { get; private set; }
        public static EHotFix Process { get; private set; }
        public static bool NeedUpdate { get; private set; }
        public static List<FileNew> NewFileList { get; private set; }
        public static float Progress
        {
            get { return (float)(DownloadBytes * 1.0 / TotalBytes); }
        }
        /// <summary>需要更新的字节数(转换KB >> 10)</summary>
        public static long TotalBytes { get; private set; }
        /// <summary>已经下载更新的字节数(转换KB >> 10)</summary>
        public static long DownloadBytes { get; private set; }

        static void DoProcess(EHotFix phase)
        {
            Process = phase;
            if (OnProcess != null)
                OnProcess(phase);
        }
        /// <summary>返回true就中止热更新</summary>
        static bool DoError(AsyncHttpRequest ex)
        {
            if (OnError != null)
                return OnError(ex);
            return true;
        }

        /// <summary>程序热更新：Unity -> 下载比对版本号 -> 更新dll -> 设置ServerURL -> 打开程序 -> 程序自行热更</summary>
        public static IEnumerable<ICoroutine> Update()
        {
            if (string.IsNullOrEmpty(ServerURL))
                yield break;

            AsyncHttpRequest async;

            #region 版本号
            // 新版本号
            if (NewVersionBytes == null)
            {
                // 自动下载最新版本号
                _LOG.Debug("正在检测版本更新");
                DoProcess(EHotFix.正在检测最新版本号);
                async = new AsyncHttpRequest(ServerURL + VERSION).Timeout(2000).NoCache().Send(null);
                yield return async;
                if (async.IsFaulted)
                {
                    _LOG.Warning("获取版本号异常：{0}", async.FaultedReason.Message);
                    if (DoError(async))
                        yield break;
                }
                else
                {
                    NewVersionBytes = async.Data;
                    Version = BitConverter.ToInt64(NewVersionBytes, 0);
                }
            }

            // 旧版本号
            byte[] oldVersionBuffer = null;
            try
            {
                oldVersionBuffer = _IO.ReadByte(VERSION);
                VersionOld = BitConverter.ToInt64(oldVersionBuffer, 0);
            }
            catch
            {
            }

            // 新旧版本比对
            if (oldVersionBuffer == null)
            {
                // 没有旧版本号，必定更新新版本
                NeedUpdate = true;
            }
            else
            {
                // 新旧版本号不同则需要更新版本
                for (int i = 0; i < NewVersionBytes.Length; i++)
                {
                    if (NewVersionBytes[i] != oldVersionBuffer[i])
                    {
                        NeedUpdate = true;
                        break;
                    }
                }

                if (!NeedUpdate)
                    yield break;
            }
            #endregion

            #region 文件列表

            string[] oldList;
            string[] newList;
            // 新文件列表
            if (NewFileListBytes == null)
            {
                _LOG.Debug("正在获取最新版本内容");
                DoProcess(EHotFix.正在获取最新版本内容);
                async = new AsyncHttpRequest(ServerURL + FILE_LIST).Timeout(2000).NoCache().Send(null);
                yield return async;
                if (async.IsFaulted)
                {
                    _LOG.Warning("获取版本文件列表异常:{0}", async.FaultedReason.Message);
                    if (DoError(async))
                        yield break;
                }
                else
                {
                    NewFileListBytes = async.Data;
                }
            }
            newList = _IO.ReadPreambleText(NewFileListBytes).Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);

            // 旧文件列表
            try
            {
                oldList = _IO.ReadText(FILE_LIST).Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);
            }
            catch
            {
                oldList = _SARRAY<string>.Empty;
            }
            Dictionary<string, FileNew> oldFilelist = new Dictionary<string, FileNew>();
            foreach (var item in oldList)
            {
                string[] splits = item.Split('\t');
                FileNew file = new FileNew();
                file.File = splits[0];
                file.Time = splits[1];
                file.Length = long.Parse(splits[2]);
                oldFilelist.Add(splits[0], file);
            }

            // 用于记录已经更新的文件，中途关闭更新下次也能接着上次的更新
            StringBuilder builder = new StringBuilder();

            // 暂时不删除不需要了的文件，只列出需要下载的新文件或者需要更新的文件
            List<FileNew> newFilelist = new List<FileNew>();
            foreach (var item in newList)
            {
                string[] splits = item.Split('\t');
                FileNew file;
                if (oldFilelist.TryGetValue(splits[0], out file))
                {
                    // 时间不一致需要更新
                    if (file.Time != splits[1])
                    {
                        file.Length = long.Parse(splits[2]);
                        newFilelist.Add(file);
                        _LOG.Debug("Update: {0}", file.File);
                    }
                    else
                    {
                        // 旧文件不需要更新的写入builder
                        builder.Append(item);
                        builder.Append("\r\n");
                    }
                }
                else
                {
                    // 需要下载的新文件
                    file = new FileNew();
                    file.File = splits[0];
                    file.Time = splits[1];
                    file.Length = long.Parse(splits[2]);
                    newFilelist.Add(file);
                    _LOG.Debug("Download: {0}", file.File);
                }
            }

            NewFileList = newFilelist;

            #endregion

            // 可能只是删除了文件，导致没有需要更新的文件
            if (newFilelist.Count > 0)
            {
                if (PARALLEL <= 0)
                    throw new ArgumentException("并行下载数不能小于1");

                TotalBytes = newFilelist.Sum(f => f.Length);
                DoProcess(EHotFix.开始更新);
                _LOG.Debug("开始更新{0}个文件：共 {1} MB", newFilelist.Count, (TotalBytes / 1048576.0).ToString("0.00"));

                DownloadBytes = 0;
                // 同时开启多个下载
                FileDownload[] asyncs = new FileDownload[PARALLEL];
                for (int i = 0; i < asyncs.Length; i++)
                    asyncs[i] = new FileDownload();
                long downloadBytesTemp = 0;
                // 正在下载的字节数，下载完成的字节数才累计进入DownloadBytes
                long downloading;
                // 已经下载到的文件索引
                int fileIndex = 0;
                while (true)
                {
                    // 是否有新下好的文件
                    bool downloadFlag = false;
                    // 是否全部下载完成
                    bool completeFlag = true;
                    downloading = downloadBytesTemp;
                    for (int i = 0; i < asyncs.Length; i++)
                    {
                        async = asyncs[i].Web;
                        if (async != null && !async.IsEnd)
                        {
                            // 正在下载
                            FileNew file = asyncs[i].File;
                            downloading += (long)(async.Progress * file.Length);
                            completeFlag = false;
                        }
                        else
                        {
                            // 未下载或者下载已经完成
                            if (async != null)
                            {
                                FileNew file = asyncs[i].File;
                                if (async.IsSuccess)
                                {
                                    byte[] bytes = async.Data;
                                    if (OnDownload != null)
                                        bytes = OnDownload(file, bytes);

                                    string dir = Path.GetDirectoryName(file.File);
                                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                                        Directory.CreateDirectory(dir);
                                    
                                    _IO.WriteByte(file.File, bytes);

                                    downloadBytesTemp += file.Length;
                                    // 每完成一个下载都写入旧文件列表，这样中途退出下次也能接着上次中断的文件开始下载
                                    builder.Append(file.ToString());
                                    downloadFlag = true;
                                }
                                else
                                {
                                    _LOG.Warning("下载文件{0}失败：{1}", file.File, async.FaultedReason.Message);
                                    if (DoError(async))
                                        yield break;
                                }
                                asyncs[i].Web = null;
                            }

                            if (fileIndex == newFilelist.Count)
                                continue;

                            completeFlag = false;
                            FileNew temp = newFilelist[fileIndex++];
                            asyncs[i].File = temp;
                            asyncs[i].Web = new AsyncHttpRequest(ServerURL + temp.File + "?" + temp.Time).NoCache().Send(null);
                            _LOG.Debug("开始下载{0}", temp.File);
                        }
                    }
                    // 更新下载进度
                    DownloadBytes = downloading;
                    // 结束或等待下载
                    if (completeFlag)
                    {
                        DownloadBytes = TotalBytes;
                        break;
                    }
                    else
                    {
                        if (downloadFlag)
                            // 每完成一个下载都写入旧文件列表，这样中途退出下次也能接着上次中断的文件开始下载
                            _IO.WriteText(FILE_LIST, builder.ToString());
                        yield return null;
                    }
                }

                // 写入新文件列表
                _IO.WriteByte(FILE_LIST, NewFileListBytes);
            }

            // 写入新版本列表
            _IO.WriteByte(VERSION, NewVersionBytes);
            _LOG.Debug(string.Format("版本更新完成 更新文件：{0}个 更新大小：{1} MB 版本号：{2}", newFilelist.Count, (TotalBytes / 1048576.0).ToString("0.00"), Version));

            DoProcess(EHotFix.更新完成);
            _HOT_FIX.NewVersionBytes = null;
            _HOT_FIX.NewFileListBytes = null;
            _HOT_FIX.NewFileList = null;
        }
    }
#endif
}
