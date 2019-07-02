using System;
using System.Collections.Generic;
using System.Linq;
using EntryEngine.Serialize;
using System.Text;
using System.IO;

namespace EntryEngine
{
    public enum ELog : byte
    {
        Debug = 3,
        Statistics = 4,
        Running = 255,
    }
    public class Record : IEquatable<Record>
    {
        public byte Level;
        public DateTime Time;
        public string Content;
        public string[] Params;

        public override string ToString()
        {
            if (Params == null || Params.Length == 0)
                return Content;
            else
                return string.Format(Content, Params);
        }
        public bool Equals(Record other)
        {
            return Level == other.Level && Content == other.Content;
        }
    }
    /// <summary>
    /// Debug   : 测试时信息
    /// Info    : 功能关键信息
    /// Warning : 错误的操作信息
    /// Error   : 抛出且截获的异常
    /// --Fatal   : 抛出且未截获的异常
    /// </summary>
	public static partial class _LOG
	{
        private class AppendLog
        {
            public string Content;
            public string[] Params;
        }

		[ADefaultValue(typeof(LoggerEmpty))]
		public abstract class Logger
		{
            private static string[] emptyArray = new string[0];
            public event Func<Record, bool> Filter;
            private List<AppendLog> appends = new List<AppendLog>();
            public void Append(string value, params object[] param)
            {
                if (value == null)
                    return;
                AppendLog append = new AppendLog();
                append.Content = value;
                append.Params = ConvertAll(param);
                appends.Add(append);
            }
            private string[] ConvertAll(object[] param)
            {
                if (param.Length == 0)
                    return emptyArray;
                string[] array = new string[param.Length];
                for (int i = 0; i < array.Length; i++)
                    array[i] = Converter(param[i]);
                return array;
            }
            public void AppendException(Exception ex)
            {
                while (ex != null)
                {
                    Append("Msg: {0} Stack:{1}", ex.Message, ex.StackTrace);
                    ex = ex.InnerException;
                }
            }
            public void Write(byte level, string value, params object[] param)
            {
                //if (_Logger == null)
                //    return;

                if (value == null)
                    return;

                Record record = new Record();
                record.Level = level;
                record.Time = DateTime.Now;
                record.Content = value;
                record.Params = ConvertAll(param);
                if (appends.Count > 0)
                {
                    // Append追加的内容前置，追加的参数后置
                    int count;
                    int paramCount = appends.Sum(a => a.Params.Length);
                    string[] param2;
                    if (param.Length == 0)
                    {
                        count = 0;
                        if (paramCount == 0)
                            param2 = emptyArray;
                        else
                            param2 = new string[paramCount];
                    }
                    else
                    {
                        count = param.Length;
                        if (paramCount == 0)
                            param2 = record.Params;
                        else
                        {
                            param2 = new string[paramCount + count];
                            Array.Copy(record.Params, param2, count);
                        }
                    }

                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < appends.Count; i++)
                    {
                        AppendLog temp = appends[i];
                        if (temp.Params.Length > 0)
                        {
                            if (count > 0)
                            {
                                for (int j = temp.Params.Length - 1; j >= 0; j--)
                                {
                                    temp.Content = temp.Content.Replace("{" + j + "}", "{" + (j + count) + "}");
                                }
                            }
                            Array.Copy(temp.Params, 0, param2, count, temp.Params.Length);
                            count += temp.Params.Length;
                        }
                        builder.Append(temp.Content);
                    }

                    builder.AppendLine();
                    builder.Append(value);

                    record.Content = builder.ToString();
                    record.Params = param2;
                    appends.Clear();
                }

                if (Filter == null || Filter(record))
                    Log(ref record);
            }
			public abstract void Log(ref Record record);
			public void Debug(string value, params object[] param)
			{
				Write(0, value, param);
			}
			public void Info(string value, params object[] param)
			{
				Write(1, value, param);
			}
			public void Warning(string value, params object[] param)
			{
				Write(2, value, param);
			}
			public void Error(string value, params object[] param)
			{
				Write(3, value, param);
			}
            public void Error(Exception ex, string message, params object[] param)
            {
                //lock (this)
                //{
                //    AppendException(ex);
                //    Error(message, param);
                //}

                int exDepth = 0;
                Exception temp = ex;
                while (temp != null)
                {
                    exDepth++;
                    temp = temp.InnerException;
                }

                if (exDepth > 0)
                {
                    int len = param.Length;
                    Array.Resize(ref param, len + exDepth);
                    Exception[] exes = new Exception[exDepth];
                    {
                        int index = 0;
                        temp = ex;
                        while (temp != null)
                        {
                            exes[index++] = temp;
                            temp = temp.InnerException;
                        }
                    }
                    StringBuilder builder = new StringBuilder(message);
                    for (int i = len; i < param.Length; i++)
                    {
                        builder.AppendFormat("\r\n{{{0}}}", i);
                        // 从内往外记录异常
                        temp = exes[exDepth - (i - len) - 1];
                        param[i] = string.Format("type: {2}\r\nmsg: {0}\r\nstack: {1}", temp.Message, temp.StackTrace, temp.GetType().FullName);
                    }
                    message = builder.ToString();
                }

                Error(message, param);
            }
			public void Statistics(string key, int value)
			{
				Write(4, key, value);
			}
			protected virtual string Converter(object param)
			{
				if (param == null)
					return "null";
				else
					return param.ToString();
			}
		}
        private class LoggerEmpty : Logger
        {
            public override void Log(ref Record record)
            {
            }
            protected override string Converter(object param)
            {
                return null;
            }
        }
	}
#if DEBUG
    public class LoggerFile : _LOG.Logger, IDisposable
    {
        const string NEW_LOG = "#LOG_new.txt";
        const string OLD_LOG = "#LOG_old.txt";
        public _LOG.Logger Base;
        StreamWriter writer;
        public LoggerFile() : this(_LOG._Logger) { }
        public LoggerFile(_LOG.Logger baseLog) : this(baseLog, NEW_LOG, OLD_LOG) { }
        public LoggerFile(_LOG.Logger baseLog, string newFile, string oldFile)
        {
            Base = baseLog;
            if (File.Exists(newFile))
            {
                File.Copy(newFile, oldFile, true);
            }
            writer = new StreamWriter(newFile, false, Encoding.UTF8);
            writer.AutoFlush = true;
        }
        public override void Log(ref Record record)
        {
            string format = string.Format("[{0}] {1}", record.Time.ToString("yyyy-MM-dd HH:mm:ss"), record.ToString());
            writer.WriteLine(format);
            if (Base != null)
                Base.Log(ref record);
        }
        public void Dispose()
        {
            if (writer != null)
            {
                writer.Close();
            }
        }
    }
    public class LoggerConsole : _LOG.Logger
    {
        private byte last;

        public ConsoleColor[] Colors
        {
            get;
            private set;
        }

        public LoggerConsole()
        {
            Colors = new ConsoleColor[]
            {
                ConsoleColor.Gray,
                ConsoleColor.White,
                ConsoleColor.DarkYellow,
                ConsoleColor.Red,
            };
        }

        public override void Log(ref Record record)
        {
            byte level = record.Level;
            if (level >= Colors.Length)
                return;

            if (level != last)
            {
                last = level;
                Console.ForegroundColor = Colors[level];
            }
            Console.WriteLine("[{0}] {1}", record.Time.ToString("yyyy-MM-dd HH:mm:ss"), record.ToString());
        }
    }
#endif

    public interface IOperation
    {
        void Redo();
        void Undo();
    }
    public abstract class Operation : IOperation
    {
        public string OperationContent;
        public string OperationName;

        public abstract void Redo();
        public abstract void Undo();
    }
    public class OperationLog
    {
        private List<IOperation> operations = new List<IOperation>();
        private Stack<IOperation> redos = new Stack<IOperation>();
        private IOperation last;
        public int Capcity = -1;

        public IOperation[] OperationHistory
        {
            get { return operations.ToArray(); }
        }
        public IOperation[] OperationHistoryUndone
        {
            get { return redos.ToArray(); }
        }
        public bool HasModified
        {
            get { return last != LastOperation; }
        }
        private IOperation LastOperation
        {
            get
            {
                if (operations.Count == 0)
                    return null;
                else
                    return operations.Last();
            }
        }

        public OperationLog()
        {
        }
        public OperationLog(int capcity)
        {
            this.Capcity = capcity;
        }

        public void Operate(IOperation operation)
        {
            InternalOperate(operation, true, false);
        }
        public void Operate(IOperation operation, bool operate)
        {
            InternalOperate(operation, true, operate);
        }
        private void InternalOperate(IOperation operation, bool clearRedo, bool operate)
        {
            if (clearRedo)
            {
                redos.Clear();
            }
            if (Capcity > 0 && operations.Count >= Capcity)
            {
                operations.RemoveAt(0);
            }
            if (operate)
            {
                operation.Redo();
            }
            operations.Add(operation);
        }
        public void Undo()
        {
            Undo(1);
        }
        public void Undo(int count)
        {
            count = _MATH.Min(count, operations.Count);
            for (int i = 0; i < count; i++)
            {
                IOperation operation = operations.Last();
                operations.RemoveLast();

                operation.Undo();

                redos.Push(operation);
            }
        }
        public void Redo()
        {
            Redo(1);
        }
        public void Redo(int count)
        {
            count = _MATH.Min(count, redos.Count);
            for (int i = 0; i < count; i++)
            {
                InternalOperate(redos.Pop(), false, true);
            }
        }
        public void Save()
        {
            last = LastOperation;
        }
        public void Clear()
        {
            operations.Clear();
            redos.Clear();
        }
    }

    public class OValueModify<T> : Operation
    {
        private Action<T> setter;
        private T origin;
        private T target;

        public T Origin
        {
            get { return origin; }
        }
        public T Target
        {
            get { return target; }
        }

        public OValueModify(VariableObject variable, T target, bool operate) : this((T)variable.GetValue(), target, v => variable.SetValue(v), operate)
        {
        }
        public OValueModify(T origin, T target, Action<T> setter, bool operate)
        {
            if (setter == null)
                throw new ArgumentNullException("setter");
            this.origin = origin;
            this.target = target;
            this.setter = setter;
            if (operate)
            {
                Redo();
            }
        }

        public override void Redo()
        {
            setter(target);
        }
        public override void Undo()
        {
            setter(origin);
        }
    }
    public abstract class ORecord : Operation
    {
		public Operation Operation;

        public override void Redo()
        {
            Operate(Operation);
        }
        public override void Undo()
        {
            Operate(this);
        }
		protected abstract void Operate(Operation operation);
    }
}
