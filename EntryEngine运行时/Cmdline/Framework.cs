using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EntryEngine.Serialize;
using EntryEngine.Network;
using System.Linq;

namespace EntryEngine.Cmdline
{
    /// <summary>进程启动器，类似于.bat文件的作用</summary>
    public class Launcher : MarshalByRefObject, IDisposable
    {
        private static string[] SEPERATOR = { "\r\n" };

        private string name;
        private event EventHandler exit;
        /// <summary>事件触发在异步线程上</summary>
        public event Action<string> ConsoleLog;
        public event Action<Process> OnLaunch;
        public event Action<Process> Launched;

        protected Process Process
        {
            get;
            private set;
        }
        public event EventHandler Exit
        {
            add
            {
                EventHandler temp = exit;
                exit += value;
                if (Process != null)
                {
                    Process.Exited -= temp;
                    Process.Exited += exit;
                }
            }
            remove
            {
                EventHandler temp = exit;
                exit -= value;
                if (Process != null)
                {
                    Process.Exited -= temp;
                    if (exit != null)
                    {
                        Process.Exited += exit;
                    }
                }
            }
        }
        public bool Running
        {
            get { return Process != null && !Process.HasExited; }
        }
        public string Name
        {
            get { return name; }
            set
            {
                if (Running)
                    throw new InvalidOperationException("Process has launched.");

                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("Process name can't be null.");

                name = value;
            }
        }

        public Launcher()
        {
        }
        public Launcher(Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");
            this.Process = process;
        }

        public void Launch(string exe)
        {
            Launch(exe, null, null, (string[])null);
        }
        public void Launch(string exe, string args)
        {
            Launch(exe, args, null, (string[])null);
        }
        public void Launch(string exe, string args, string directory)
        {
            Launch(exe, args, directory, (string[])null);
        }
        public void Launch(string exe, string args, string directory, string cmdline)
        {
            Launch(exe, args, directory, 
                cmdline == null ? null : cmdline.Split(SEPERATOR, StringSplitOptions.RemoveEmptyEntries));
        }
        public void Launch(string exe, string args, string directory, string[] cmdlines)
        {
            // check process and name
            this.Name = name;

            Process = new Process();

            Process.StartInfo.FileName = exe;
            if (string.IsNullOrEmpty(directory))
                directory = Path.GetDirectoryName(exe);
            else
                directory = Path.GetFullPath(directory);
            Process.StartInfo.WorkingDirectory = directory;
            Process.StartInfo.Arguments = args;
            Process.StartInfo.RedirectStandardInput = true;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.CreateNoWindow = true;
            Process.StartInfo.ErrorDialog = false;
            //Process.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;

            Process.EnableRaisingEvents = true;
            if (exit != null)
                Process.Exited += exit;

            if (OnLaunch != null)
                OnLaunch(Process);
            Process.OutputDataReceived += new DataReceivedEventHandler(Process_OutputDataReceived);
            Process.Start();

            Process.BeginOutputReadLine();
            //Process.StandardInput.AutoFlush = true;

            if (cmdlines != null)
                for (int i = 0; i < cmdlines.Length; i++)
                    ExecuteCommand(cmdlines[i]);

            this.Process = Process;

            if (Process.HasExited)
            {
                Dispose();
                return;
            }

            Launch(Process);

            if (Launched != null)
                Launched(Process);
        }
        public void ExecuteCommand(string cmdline)
        {
            if (Process == null)
                throw new InvalidOperationException("Cmdline must be executed after process launched.");
            Process.StandardInput.WriteLine(cmdline);
            Process.StandardInput.Flush();
        }

        void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (Process == null)
                return;
            if (Process.HasExited)
            {
                Process.CancelOutputRead();
                return;
            }
            if (ConsoleLog != null)
            {
                ConsoleLog(e.Data);
            }
        }
        public void Dispose()
        {
            if (Process != null)
            {
                Process.CancelOutputRead();
                processes.Remove(this);
                KillProcess(Process);
                Process = null;

                InternalDispose();
            }
        }
        protected virtual void InternalDispose()
        {
        }

        private static List<Launcher> processes = new List<Launcher>();
        private static bool hasLaunched;

        public static IEnumerable<Launcher> Launchers
        {
            get { return processes.Enumerable(); }
        }
        public static IEnumerable<Process> LaunchedProcesses
        {
            get { return processes.Select(p => p.Process); }
        }

        public static Launcher Find(string name)
        {
            return processes.FirstOrDefault(p => p.name == name);
        }
        private void Launch(Process process)
        {
            processes.Add(this);
            if (!hasLaunched)
            {
                hasLaunched = true;
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
                _CMDLINE.ApplicationExit(OnClose);
            }
        }
        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            OnClose(1);
        }
        private static void OnClose(int type)
        {
            foreach (var process in processes)
                KillProcess(process.Process);
            processes.Clear();
        }
        public static void KillProcess(Process process)
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
            process.Close();
            process.Dispose();
        }
    }
    /// <summary>命令行进程启动器，可捕获控制台日志</summary>
    public class LauncherCmdline : Launcher
    {
        private JsonReader reader = new JsonReader();
        /// <summary>捕获到控制台日志对象，事件触发在异步线程上</summary>
        public Action<Record> OnLogRecord;

        protected StreamWriter Writer
        {
            get;
            private set;
        }

        public LauncherCmdline()
        {
            ConsoleLog += this.OnLog;
            Launched += this.OnLaunch;
            Exit += OnExit;
        }

        private void OnExit(object sender, EventArgs e)
        {
            Process process = sender as Process;
            if (process != null)
                _LOG.Info("进程[{0}]关闭！ExitCode:{1}", Name, process.ExitCode);
            else
                _LOG.Info("进程[{0}]关闭！{1}", sender);
        }
        private void OnLaunch(Process process)
        {
            Stream stream = new FileStream(Path.Combine(Process.StartInfo.WorkingDirectory, Name + ".txt"), FileMode.Create, FileAccess.Write);
            this.Writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
            this.Writer.AutoFlush = true;
        }
        private void OnLog(string log)
        {
            if (Writer == null)
                return;

            if (string.IsNullOrEmpty(log))
                return;

            try
            {
                if (!log.StartsWith("{\"Level\":"))
                {
                    Writer.WriteLine("非常规Json日志：{0}", log);
                    return;
                }

                reader.Input(log);
                Record record = reader.ReadObject<Record>();
                if (record == null || record.Content == null)
                {
                    Writer.WriteLine("解析错误的日志：{0}", log);
                    return;
                }

                if (OnLogRecord != null)
                    OnLogRecord(record);

                if (record.Level >= 2 && record.Level <= 3)
                    Writer.WriteLine("[{0}] {1}", record.Time.ToString("yyyy-MM-dd HH:mm:ss"), record.ToString());
            }
            catch (Exception)
            {
                Writer.WriteLine("解析错误的日志：{0}", log);
            }
        }
        protected override void InternalDispose()
        {
            if (Writer != null)
            {
                Writer.Flush();
                Writer.Close();
                Writer.Dispose();
                Writer = null;
            }
        }
    }
    /// <summary>写日志到控制台</summary>
    public class Logger : _LOG.Logger
    {
        private ConsoleColor last;
        public Dictionary<byte, ConsoleColor> Colors
        {
            get;
            private set;
        }
        public Logger()
        {
            this.Colors = new Dictionary<byte, ConsoleColor>();
            this.Colors.Add(0, ConsoleColor.Gray);
            this.Colors.Add(1, ConsoleColor.White);
            this.Colors.Add(2, ConsoleColor.DarkYellow);
            this.Colors.Add(3, ConsoleColor.Red);
        }
        public sealed override void Log(ref Record record)
        {
            byte level = record.Level;
            ConsoleColor color;
            if (this.Colors.TryGetValue(level, out color))
            {
                if (color != last)
                {
                    last = color;
                    Console.ForegroundColor = color;
                }
                InternalLog(record);
            }
        }
        protected virtual void InternalLog(Record record)
        {
            Console.WriteLine("[{0}] {1}", record.Time.ToString("yyyy-MM-dd HH:mm:ss"), record.ToString());
        }
    }
    /// <summary>写Json日志到控制台，控制台可解析日志内容变回Record对象</summary>
    public class LoggerToShell : Logger
    {
        public LoggerToShell()
        {
            //Colors.Remove(0);
            Colors.Add(4, ConsoleColor.Gray);
            Colors.Add(5, ConsoleColor.White);
            Colors.Add(6, ConsoleColor.DarkYellow);
            Colors.Add(7, ConsoleColor.Red);
            Colors.Add(255, ConsoleColor.Green);
        }

        public void Debug2M(string content, params object[] param)
        {
            Write(4, content, param);
        }
        public void Info2M(string content, params object[] param)
        {
            Write(5, content, param);
        }
        public void Warning2M(string content, params object[] param)
        {
            Write(6, content, param);
        }
        public void Error2M(string content, params object[] param)
        {
            Write(7, content, param);
        }
        /// <summary>写入等级255到运维工具，代表服务已经正常启动</summary>
        public void StatusRunning()
        {
            Write(255, string.Empty);
        }
        protected override void InternalLog(Record record)
        {
            Console.WriteLine(JsonWriter.Serialize(record));
        }
    }
    /// <summary>LogSearch在修改时间段，查询条件微调，等级筛选时可以在原缓存上稍作修改</summary>
    [Code(ECode.Optimize)]
    public class LogStorage : _LOG.Logger, IDisposable
    {
        public class Storage : IEquatable<Storage>, IComparable<Storage>
        {
            public Record Record;
            //internal long Index;
            //public long IndexContent;
            //public long IndexParam;
            public Dictionary<long, Storage> SameContent = new Dictionary<long, Storage>();

            //bool IEquatable<Storage>.Equals(Storage other)
            //{
            //    return Index == other.Index;
            //    //return Record.Equals(other.Record);
            //}
            //int IComparable<Storage>.CompareTo(Storage other)
            //{
            //    return Index.CompareTo(other.Index);
            //}
            bool IEquatable<Storage>.Equals(Storage other)
            {
                return Record.Equals(other.Record);
            }
            int IComparable<Storage>.CompareTo(Storage other)
            {
                return Record.Time.CompareTo(other.Record.Time);
            }
            public override int GetHashCode()
            {
                return Record.Level.GetHashCode() + Record.Content.GetHashCode();
            }
        }
        class LogSearch
        {
            public DateTime? From;
            public DateTime? To;
            public string Content;
            public string Param;
            public byte[] Levels;
            public long EndTime;
        }
        class StorageComparer : IEqualityComparer<LogSearch>
        {
            public bool Equals(LogSearch x, LogSearch y)
            {
                return x.From == y.From
                    && x.To == y.To
                    && x.Content == y.Content
                    && x.Param == y.Param
                    && Utility.Equals(x.Levels, y.Levels);
            }
            int IEqualityComparer<LogSearch>.GetHashCode(LogSearch obj)
            {
                return 0;
            }
        }
        private static Storage[] EMPTY = new Storage[0];
        private static StorageComparer COMPARER = new StorageComparer();
        public static byte[] LogLevels = { 0, 1, 2, 3 };

        private static byte[] buf;             // 解析时间，长度，等级
        private static byte[] buf2;            // 解析content的byte[]内容
        private static char[] buf3;            // content的byte[]所转换的char[]
        private static ByteWriter writer;
        FileStream _time;       // 每条日志的时间及其索引
        FileStream _index;      // 每条日志的内容索引，参数索引，同样内容的时间索引
        FileStream _content;    // 日志内容（不重复）
        FileStream _param;      // 每条日志的参数
        // index相同的缓存
        Dictionary<long, Storage> cache1 = new Dictionary<long, Storage>();
        // 查询条件相同的缓存
        Dictionary<LogSearch, List<Storage>> cache2 = new Dictionary<LogSearch, List<Storage>>(COMPARER);
        // 内容index相同的缓存
        Dictionary<long, Dictionary<long, Storage>> cache3 = new Dictionary<long, Dictionary<long, Storage>>();
        public byte CacheCount = 10;

        public long LogDataByteCount
        {
            get { return _time.Length + _index.Length + _content.Length + _param.Length; }
        }
        public DateTime LastReadTime
        {
            get;
            private set;
        }
        public string Name
        {
            get;
            private set;
        }
        public bool IsDisposed
        {
            get
            {
                return _time == null || !_time.CanRead;
            }
        }

        public LogStorage(string directory, string name)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            _time = new FileStream(Path.Combine(directory, name + ".tidx"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _index = new FileStream(Path.Combine(directory, name + ".idx"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _content = new FileStream(Path.Combine(directory, name + ".cnt"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _param = new FileStream(Path.Combine(directory, name + ".prm"), FileMode.OpenOrCreate, FileAccess.ReadWrite);

            buf = new byte[8];
            buf2 = new byte[8192];
            buf3 = new char[4096];
            writer = new ByteWriter(8192);

            this.Name = name;
        }

        private void Append(FileStream stream, Action<ByteWriter> write)
        {
            writer.Reset();
            write(writer);
            stream.Seek(stream.Length, SeekOrigin.Begin);
            stream.Write(writer.Buffer, 0, writer.Position);
        }
        private long BinarySearchTime(long time)
        {
            long result = -1;
            long s = 0;
            long last = _time.Length / 16 - 1;
            long e = last;
            while (s <= e)
            {
                long median = s + ((e - s) >> 1);
                _time.Seek(median * 16, SeekOrigin.Begin);
                _time.Read(buf, 0, 8);
                long t = BitConverter.ToInt64(buf, 0);
                if (t == time
                    || (t < time && median == last)
                    || (t > time && median == 0))
                {
                    result = median;
                    break;
                }
                if (t > time)
                    e = median - 1;
                else
                    s = median + 1;

                if (s >= e)
                {
                    result = s;
                    break;
                }
            }
            return result * 16;
        }
        public override void Log(ref Record origin)
        {
            Record record = origin;
            bool flag = false;
            long seek = 0;
            while (seek < _content.Length)
            {
                //_content.Seek(seek + 1, SeekOrigin.Begin);
                _content.Seek(seek, SeekOrigin.Begin);
                _content.Read(buf, 0, 1);
                byte level = buf[0];
                _content.Read(buf, 0, 4);
                // 字符串长度 *2为字符串byte[]长度
                int length = BitConverter.ToInt32(buf, 0);
                // 字符串长度相等时检测是否为同样内容的日志
                if (level == record.Level && length == record.Content.Length)
                {
                    string text = ReadString(_content, length);
                    if (text == record.Content)
                    {
                        // 旧内容
                        flag = true;
                        break;
                    }
                }
                seek += 5 + length * 2;
            }

            if (!flag)
                seek = _content.Length;

            // 写入日志时间
            Append(_time, writer =>
            {
                writer.Write(record.Time.Ticks);
                writer.Write(_index.Length);
            });

            // 写入日志索引
            Append(_index, writer =>
            {
                //writer.Write(_index.Length);
                writer.Write(seek);
                writer.Write(_param.Length);
            });

            // 写入参数
            Append(_param, writer =>
            {
                writer.Write(record.Params);
            });

            // 新内容
            if (!flag)
            {
                // 写入日志内容
                Append(_content, writer =>
                {
                    writer.Write(record.Level);
                    writer.Write(record.Content);
                });
            }
        }
        public Storage[] ReadAllLog(DateTime? start, DateTime? end, string content, string param)
        {
            int count;
            return ReadLog(start, end, 0, 0, content, param, out count, LogLevels);
        }
        public Storage[] ReadLog(DateTime? start, DateTime? end, byte pageCount, int page,
            string content, string param, out int count, params byte[] levels)
        {
            count = 0;
            if (page < 0)
                throw new ArgumentOutOfRangeException();

            if (_time.Length == 0)
                return EMPTY;

            long _st = start.HasValue ? start.Value.Ticks : 0;
            long _et = end.HasValue ? end.Value.Ticks : long.MaxValue;
            if (_st >= _et)
                throw new ArgumentOutOfRangeException();

            LogSearch search = new LogSearch();
            search.From = start;
            search.To = end;
            search.Content = content;
            search.Param = param;
            search.Levels = levels;

            List<Storage> storages;
            bool cacheFlag = cache2.TryGetValue(search, out storages);
            if (cacheFlag)
                search = cache2.Keys.FirstOrDefault(k => COMPARER.Equals(k, search));
            if (!cacheFlag || (end == null && _time.Length > search.EndTime))
            {
                long _si = BinarySearchTime(_st);
                long _ei = BinarySearchTime(_et);
                //count = (int)(_ei - _si) / 16 + 1;
                if (cacheFlag)
                {
                    // 补足新增时间的日志进入缓存
                    _si = search.EndTime;
                }
                else
                    storages = new List<Storage>(256);
                //_si = Math.Min(_si + pageCount * page * 16, _ei - pageCount * 16);
                _time.Seek(_si, SeekOrigin.Begin);
                while (_time.Position <= _ei)
                {
                    // time & index
                    _time.Read(buf, 0, 8);
                    DateTime time = new DateTime(BitConverter.ToInt64(buf, 0));
                    _time.Read(buf, 0, 8);
                    long index = BitConverter.ToInt64(buf, 0);

                    _index.Seek(index, SeekOrigin.Begin);
                    // index for content
                    _index.Read(buf, 0, 8);
                    long cindex = BitConverter.ToInt64(buf, 0);
                    // get from cache
                    Storage storage;
                    if (!cache1.TryGetValue(index, out storage))
                    {
                        _content.Seek(cindex, SeekOrigin.Begin);
                        _content.Read(buf, 0, 1);
                        byte level = buf[0];
                        _content.Read(buf, 0, 4);
                        int length = BitConverter.ToInt32(buf, 0);
                        string text = ReadString(_content, length);

                        // index for parameter
                        _index.Read(buf, 0, 8);
                        long pindex = BitConverter.ToInt64(buf, 0);

                        _param.Seek(pindex, SeekOrigin.Begin);
                        _param.Read(buf, 0, 4);
                        int pcount = BitConverter.ToInt32(buf, 0);
                        string[] p;
                        if (pcount <= 0)
                            p = null;
                        else
                        {
                            p = new string[pcount];
                            for (int i = 0; i < pcount; i++)
                                p[i] = ReadString(_param);
                        }

                        storage = new Storage();
                        storage.Record = new Record();
                        storage.Record.Time = time;
                        storage.Record.Level = level;
                        storage.Record.Content = text;
                        storage.Record.Params = p;

                        //storage.Index = index;
                        //storage.IndexContent = cindex;
                        //storage.IndexParam = pindex;

                        cache1.Add(index, storage);
                    } // end of read

                    Dictionary<long, Storage> cache;
                    if (cache3.TryGetValue(cindex, out cache))
                    {
                        if (!cache.ContainsKey(index))
                            cache.Add(index, storage);
                    }
                    else
                    {
                        cache = new Dictionary<long, Storage>();
                        cache.Add(index, storage);
                        cache3.Add(cindex, cache);
                    }
                    storage.SameContent = cache;

                    if (levels.Length > 0 && !levels.Contains(storage.Record.Level))
                        continue;
                    if (!string.IsNullOrEmpty(content) && !storage.Record.Content.Contains(content))
                        continue;
                    if (!string.IsNullOrEmpty(param) && (storage.Record.Params == null || !storage.Record.Params.Any(p => p.Contains(param))))
                        continue;

                    storages.Add(storage);
                } // end of time loop

                if (cache2.Count > CacheCount)
                {
                    var first = cache2.First().Key;
                    cache2.Remove(first);
                }
                if (!cacheFlag)
                    cache2.Add(search, storages);
            } // end of get cache

            if (end == null)
                search.EndTime = _time.Length;
            LastReadTime = DateTime.Now;
            count = storages.Count;
            if (pageCount == 0)
                return storages.ToArray();
            else if (storages.Count == 0 || pageCount * page >= storages.Count)
                return EMPTY;
            else
            {
                int from = pageCount * page;
                return storages.GetRange(from, _MATH.Min(from + pageCount, storages.Count) - from).ToArray();
            }
        }
        public Storage[] ReadLogGroup(DateTime? start, DateTime? end, string content, string param, params byte[] levels)
        {
            int count;
            Storage[] storages = ReadLog(start, end, 0, 0, content, param, out count, levels);
            return storages.Distinct().ToArray();
        }
        public Storage[] FindContext(Storage log, DateTime? start, DateTime? end, byte pageCount,
            string content, string param, out int page, params byte[] levels)
        {
            return FindContext(cache => cache.BinarySearch(log), ref start, ref end, pageCount, content, param, out page, levels);
        }
        public Storage[] FindContext(DateTime time, DateTime? start, DateTime? end, byte pageCount,
            string content, string param, out int page, params byte[] levels)
        {
            return FindContext(cache => cache.BinarySearch(null, (s1, s2) => s1.Record.Time.CompareTo(time)),
                ref start, ref end, pageCount, content, param, out page, levels);
        }
        private Storage[] FindContext(Func<List<Storage>, int> searchIndex, ref DateTime? start, ref DateTime? end, byte pageCount,
            string content, string param, out int page, byte[] levels)
        {
            LogSearch search = new LogSearch();
            search.From = start;
            search.To = end;
            search.Content = content;
            search.Param = param;
            search.Levels = levels;

            page = 0;
            List<Storage> cache;
            if (!cache2.TryGetValue(search, out cache))
                return EMPTY;

            int index = searchIndex(cache);
            if (index == -1)
                return EMPTY;
            int contextCount = pageCount / 2;
            int min = index - contextCount + 1;
            int max = index + contextCount + 1;
            if (min < 0)
            {
                max -= min;
                min = 0;
            }
            if (max > cache.Count)
            {
                if (min > 0)
                    min = _MATH.Abs(min - (max - cache.Count));
                max = cache.Count;
            }
            page = index / pageCount;
            LastReadTime = DateTime.Now;
            if (pageCount == 0)
                return cache.ToArray();
            else if (cache.Count == 0 || min >= cache.Count)
                return EMPTY;
            else
                return cache.GetRange(min, max - min).ToArray();
        }
        public void ClearCache()
        {
            cache1.Clear();
            cache2.Clear();
            cache3.Clear();
        }
        public void Delete()
        {
            Dispose();
            File.Delete(_time.Name);
            File.Delete(_index.Name);
            File.Delete(_content.Name);
            File.Delete(_param.Name);
        }
        public void Renew()
        {
            Delete();
            _time = new FileStream(_time.Name, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _index = new FileStream(_index.Name, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _content = new FileStream(_content.Name, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _param = new FileStream(_param.Name, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }
        public void Dispose()
        {
            ClearCache();
            _time.Close();
            _index.Close();
            _content.Close();
            _param.Close();
        }

        private static string ReadString(FileStream stream, int length)
        {
            // 字符串长度 *2为字符串byte[]长度
            int charLength = length * 2;
            if (charLength > buf2.Length)
            {
                buf2 = new byte[charLength];
                buf3 = new char[length];
            }
            stream.Read(buf2, 0, charLength);
            for (int i = 0; i < length; i++)
                buf3[i] = (char)BitConverter.ToUInt16(buf2, i * 2);
            return new string(buf3, 0, length);
        }
        private static string ReadString(FileStream stream)
        {
            stream.Read(buf, 0, 4);
            return ReadString(stream, BitConverter.ToInt32(buf, 0));
        }
        private static int ReadInt()
        {
            return BitConverter.ToInt32(buf, 0);
        }
        private static long ReadLong()
        {
            return BitConverter.ToInt64(buf, 0);
        }
    }

    public enum ECounter
    {
        Network,
        CPU,
        Disk,
        Memory,
    }
    /// <summary>计算机性能统计</summary>
    public class StatisticCounter
    {
        private static Dictionary<ECounter, string> COUNTER_TYPES = new Dictionary<ECounter, string>();
        static StatisticCounter()
        {
            COUNTER_TYPES.Add(ECounter.Network, "Network Interface");
            COUNTER_TYPES.Add(ECounter.CPU, "Processor");
            COUNTER_TYPES.Add(ECounter.Disk, "PhysicalDisk");
            COUNTER_TYPES.Add(ECounter.Memory, "Memory");
        }

        private PerformanceCounterCategory pcc;
        private string[] instances;
        private PerformanceCounter[] counters;

        public PerformanceCounter this[int index]
        {
            get { return counters[index]; }
        }
        public PerformanceCounter this[string name]
        {
            get { return counters.FirstOrDefault(c => c.CounterName == name); }
        }

        public StatisticCounter(ECounter counter)
        {
            pcc = new PerformanceCounterCategory(COUNTER_TYPES[counter]);
            instances = pcc.GetInstanceNames();
            if (instances.Length > 0)
                counters = pcc.GetCounters(instances[0]);
            else
                counters = pcc.GetCounters();
        }
    }
}
