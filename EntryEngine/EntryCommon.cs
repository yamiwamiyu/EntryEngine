using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EntryEngine
{
    /// <summary>入口</summary>
	public class EntryService : IDisposable
	{
		public static EntryService Instance
		{
			get;
			private set;
		}
		private GameTime gameTime = new GameTime();
		private Pool<COROUTINE> coroutines = new Pool<COROUTINE>();
        private COROUTINE[] _coroutines;
		public event Action<EntryService> AfterUpdate;

		public GameTime GameTime
		{
			get { return gameTime; }
		}

		public EntryService()
		{
			Instance = this;
		}

		public void Update()
		{
			gameTime.Elapse();
			Elapsed(gameTime);

            int count;
			lock (coroutines)
			{
                count = coroutines.ToArray(ref _coroutines);
			}
            for (int i = 0; i < count; i++)
            {
                var coroutine = _coroutines[i];
                if (coroutine.IsEnd)
                {
                    continue;
                }
                else
                {
#if DEBUG
                    try
                    {
                        coroutine.Update(gameTime.ElapsedSecond);
                    }
                    catch (Exception ex)
                    {
                        _LOG.Error(ex, "\r\ncoroutine error!");
                        coroutine.Dispose();
                    }
#else
                    coroutine.Update(gameTime);
#endif
                }
            }
			lock (coroutines)
			{
                for (int i = 0; i < count; i++)
				{
					if (_coroutines[i].IsEnd)
					{
                        coroutines.RemoveAt(_coroutines[i].PoolIndex);
					}
				}
			}

			InternalUpdate();

			gameTime.UpdateElapsedTime = gameTime.CurrentElapsed;

			// 用于统计帧更新时间
			if (AfterUpdate != null)
				AfterUpdate(this);
		}
		protected virtual void InternalUpdate()
		{
		}
		protected virtual void Elapsed(GameTime gameTime)
		{
		}
		public virtual void Dispose()
		{
		}
		public virtual void Exit()
		{
			Environment.Exit(-1);
		}
		public COROUTINE SetCoroutine(IEnumerator<ICoroutine> coroutine)
		{
			lock (coroutines)
			{
				COROUTINE newCoroutine = new COROUTINE(coroutine);
				coroutines.Add(newCoroutine);
				return newCoroutine;
			}
		}
		public COROUTINE SetCoroutine(IEnumerable<ICoroutine> coroutine)
		{
			lock (coroutines)
			{
				COROUTINE newCoroutine = new COROUTINE(coroutine);
				coroutines.Add(newCoroutine);
				return newCoroutine;
			}
		}
		public COROUTINE SetCoroutine(ICoroutine coroutine)
		{
			lock (coroutines)
			{
				COROUTINE newCoroutine = new COROUTINE(coroutine);
				coroutines.Add(newCoroutine);
				return newCoroutine;
			}
		}
		public COROUTINE Synchronize(Action action)
		{
			CorDelegate coroutine = new CorDelegate(
				(gameTime) =>
				{
					action();
					return true;
				});
			return SetCoroutine(coroutine);
		}
		public COROUTINE Delay(int interval, Action action)
		{
			TIME time = new TIME(interval);
			CorDelegate coroutine = new CorDelegate(
				(gameTime) =>
				{
					time.Update(gameTime);
					if (time.IsEnd)
					{
						action();
						return true;
					}
					else
					{
						return false;
					}
				});
			return SetCoroutine(coroutine);
		}
		public COROUTINE DelayTimer(int interval, Action action)
		{
			TIME time = new TIME(interval);
			CorDelegate coroutine = new CorDelegate(
				(gameTime) =>
				{
					time.Update(gameTime);
					if (time.IsEnd)
					{
						SetTimer(interval, action);
						return true;
					}
					else
					{
						return false;
					}
				});
			return SetCoroutine(coroutine);
		}
		public COROUTINE SetTimer(int interval, Action action)
		{
			TIME time = new TIME(interval);
			CorDelegate timer = new CorDelegate(
				(gameTime) =>
				{
					time.Update(gameTime);
					bool tick = time.IsEnd;
					if (tick)
						time.NextTurn();
					return tick;
				});
			CorDelegate coroutine = new CorDelegate(
				(gameTime) =>
				{
					action();
					return true;
				});
			return SetCoroutine(TimerCoroutine(coroutine, timer));
		}
		/// <summary>
		/// 先timer，后tick
		/// </summary>
		public static IEnumerator<ICoroutine> TimerCoroutine(ICoroutine timer, ICoroutine tick)
		{
			while (true)
			{
				yield return timer;
				yield return tick;
			}
		}
	}
    /// <summary>游戏用到的时间相关的内容</summary>
	public class GameTime
	{
        public static GameTime Time;

        /// <summary>不断累加的帧ID</summary>
        public int FrameID { get; private set; }
        /// <summary>入口开放时间</summary>
		public DateTime OpenTime
		{
			get;
			private set;
		}
        /// <summary>入口开放总时间</summary>
		public TimeSpan OpenedTotalTime
		{
			get;
			private set;
		}
        /// <summary>上一帧时间</summary>
        public DateTime PreviousFrame
        {
            get;
            private set;
        }
        /// <summary>当前帧时间</summary>
		public DateTime CurrentFrame
		{
			get;
			private set;
		}
        /// <summary>一帧经过的理论时间</summary>
		public TimeSpan ElapsedTime
		{
			get;
			internal set;
		}
        /// <summary>一帧经过的真实时间</summary>
		public TimeSpan ElapsedRealTime
		{
			get;
			private set;
		}
        /// <summary>一帧经过的理论时间毫秒数</summary>
        public float Elapsed;
        /// <summary>经过的秒数</summary>
        public float ElapsedSecond;
        /// <summary>当前帧经过的实时时间</summary>
		public TimeSpan CurrentElapsed
		{
			get { return DateTime.Now - CurrentFrame; }
		}
        /// <summary>帧更新经过的时间</summary>
		public TimeSpan UpdateElapsedTime
		{
			get;
			internal set;
		}
        /// <summary>是否经过整秒</summary>
		public bool TickSecond
		{
			get;
			private set;
		}
        /// <summary>是否经过整分</summary>
		public bool TickMinute
		{
			get;
			private set;
		}
        /// <summary>是否经过整点</summary>
		public bool TickHour
		{
			get;
			private set;
		}
        /// <summary>是否经过整天</summary>
		public bool TickDay
		{
			get;
			private set;
		}
        /// <summary>是否经过整月</summary>
		public bool TickMonth
		{
			get;
			private set;
		}
        /// <summary>是否经过整年</summary>
		public bool TickYear
		{
			get;
			private set;
		}
        /// <summary>经过到达的秒/summary>
		public int Second
		{
			get;
			private set;
		}
        /// <summary>经过到达的分</summary>
		public int Minute
		{
			get;
			private set;
		}
        /// <summary>经过到达的时</summary>
		public int Hour
		{
			get;
			private set;
		}
        /// <summary>经过到达的天</summary>
		public int Day
		{
			get;
			private set;
		}
        /// <summary>经过到达的月</summary>
		public int Month
		{
			get;
			private set;
		}
        /// <summary>经过到达的年</summary>
		public int Year
		{
			get;
			private set;
		}

        public GameTime()
        {
            OpenTime = DateTime.Now;
            CurrentFrame = OpenTime;
            PreviousFrame = OpenTime;
            if (Time == null)
                Time = this;
        }

        /// <summary>保持上一帧的时间</summary>
        public void Still()
        {
            Elapse(CurrentFrame);
        }
        /// <summary>指定时间经过一帧</summary>
        /// <param name="now">当前帧的时间</param>
        public void Elapse(DateTime now)
        {
            FrameID++;

            PreviousFrame = CurrentFrame;
            TimeSpan elapsed = now - CurrentFrame;

            this.ElapsedTime = elapsed;
            this.Elapsed = (float)elapsed.TotalMilliseconds;
            this.ElapsedSecond = (float)elapsed.TotalSeconds;
            this.ElapsedRealTime = elapsed;
            this.OpenedTotalTime += elapsed;

            TimeSpan previous = CurrentFrame.TimeOfDay;
            TimeSpan current = now.TimeOfDay;
            TickSecond = (int)current.TotalSeconds != (int)previous.TotalSeconds;
            Second = TickSecond ? current.Seconds : -1;
            TickMinute = (int)current.TotalMinutes != (int)previous.TotalMinutes;
            Minute = TickMinute ? current.Minutes : -1;
            TickHour = (int)current.TotalHours != (int)previous.TotalHours;
            Hour = TickHour ? current.Hours : -1;

            DateTime date = now.Date;
            DateTime yesterday = CurrentFrame.Date;
            TickDay = date != yesterday;
            Day = TickDay ? date.Day : -1;
            TickYear = TickDay && date.Year != yesterday.Year;
            Year = TickYear ? date.Year : -1;
            TickMonth = TickDay && (TickYear || date.Month != yesterday.Month);
            Month = TickMonth ? date.Month : -1;

            CurrentFrame = now;
        }
        /// <summary>实时时间经过一帧</summary>
        public void Elapse()
		{
            Elapse(DateTime.Now);
		}
	}

    /// <summary>
    /// <para>静态类里定义的内部类或接口</para>
    /// <para>静态类将生成partial的部分，包含每个静态接口的一个对应的静态实例及调用实例的每个公开方法的静态方法</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ADefaultValue : Attribute
    {
        /// <summary>
        /// 生成的静态实例的默认值的类型(可以在非partial的静态类的静态构造函数手动构造默认值，此时需要增加#if !EntryBuilder)
        /// </summary>
        public Type DefaultValue { get; protected set; }

        protected ADefaultValue()
        {
        }
        public ADefaultValue(Type defaultValue)
        {
            if (defaultValue == null)
                throw new ArgumentNullException("defaultValue");
            this.DefaultValue = defaultValue;
        }
    }
    /// <summary>
    /// 跨平台设备
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ADevice : Attribute
    {
        public string StaticInstance
        {
            get;
            private set;
        }

        public ADevice()
        {
        }
        public ADevice(string staticInstance)
        {
            this.StaticInstance = staticInstance;
        }
    }
    /// <summary>
    /// 构造函数创建跨平台设备
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class ADeviceNew : ADefaultValue
    {
        public ADeviceNew()
        {
        }
        public ADeviceNew(Type defaultValue)
            : base(defaultValue)
        {
        }
    }


	[Flags]
	public enum ECode
	{
		/// <summary>
		/// 还未写完
		/// </summary>
		ToBeContinue = 1,
		/// <summary>
		/// 写完还未测试
		/// </summary>
		BeNotTest = 2,
		/// <summary>
		/// 待扩展
		/// </summary>
		Expand = 4,
		/// <summary>
		/// 或许会重整
		/// </summary>
		MayBeReform = 8,
		/// <summary>
		/// 不常用的
		/// </summary>
		LessUseful = 16,
		/// <summary>
		/// 测试用
		/// </summary>
		Test = 32,
        /// <summary>
        /// 有已知BUG
        /// </summary>
        BUG = 64,
        /// <summary>
        /// 可能有BUG
        /// </summary>
        MayBeBUG = 128,
        /// <summary>
        /// 变量的跨平台通用值未定
        /// </summary>
        Value = 256,
        /// <summary>
        /// 可以优化
        /// </summary>
        Optimize = 512,
        /// <summary>有需要注意的地方</summary>
        Attention = 1024,
	}
	public class CodeAttribute : Attribute
	{
		public CodeAttribute(ECode code)
		{
		}
	}
    /// <summary>标识此特性的类型或程序集在重构代码或转换语言代码时类型及其成员不进行重命名，[不进行优化无引用成员（暂未实现，优化掉也貌似可以）]</summary>
    [AInvariant]public class AInvariant : Attribute { }
    /// <summary>标识此特性的类型在重构代码时生成程序集信息</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum)]
    [AInvariant]public class AReflexible : Attribute { }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum)]
    [AInvariant]public class AReflexibleField : Attribute { }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum)]
    [AInvariant]public class AReflexibleMethod : Attribute { }
    /// <summary>标识此特性则不会被优化掉</summary>
    public class ANonOptimize : Attribute { public static readonly string Name = typeof(ANonOptimize).Name; }

    // 表
	public abstract class SimpleTable<T, K> : IEnumerable<KeyValuePair<T, K>>
	{
		public abstract int ColumnCount { get; }
		public abstract int RowCount { get; }
		public abstract IEnumerable<T> Columns { get; }
		public abstract K this[T column, int row] { get; set; }
		public abstract K this[int column, int row] { get; set; }

		public int AddColumn(T column)
		{
			return AddColumn(column, null);
		}
		public abstract int AddColumn(T column, IEnumerable<K> newColumnValues);
		public void AddRow(params K[] newRowValues)
		{
			AddRow((IEnumerable<K>)newRowValues);
		}
		public abstract void AddRow(IEnumerable<K> newRowValues);
		public abstract bool AddValue(K value);
		public abstract bool RemoveColumn(T column);
		public abstract bool RemoveColumn(int column);
		public abstract bool RemoveRow(int row);
		public abstract int GetColumnIndex(T column);
		public abstract T GetColumn(int column);
		public abstract int GetRowIndex(T column, K row);
		public abstract int GetRowIndex(int column, K row);
		public abstract K[] GetColumns(T column);
		public abstract K[] GetColumns(int column);
		public abstract K[] GetRows(int row);
		public abstract Dictionary<T, K[]> ToDictionary();
		public abstract void Clear(int startRow);
		public void Clear()
		{
			Clear(0);
		}

		public abstract IEnumerator<KeyValuePair<T, K>> GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
        }
    }
	public abstract class TableList<T, K> : SimpleTable<T, K>
	{
		protected List<T> columns;
		protected List<List<K>> rows = new List<List<K>>();

		public sealed override int ColumnCount
		{
			get { return columns.Count; }
		}
		public sealed override IEnumerable<T> Columns
		{
			get { return columns.Enumerable(); }
		}

		public sealed override bool RemoveColumn(T column)
		{
			int index = GetColumnIndex(column);
			if (index == -1)
				return false;
			else
				return RemoveColumn(index);
		}
		public sealed override int GetColumnIndex(T column)
		{
			return columns.IndexOf(column);
		}
		public sealed override T GetColumn(int column)
		{
			if (column < 0 || column >= ColumnCount)
				return default(T);
			else
				return columns[column];
		}
		public sealed override int GetRowIndex(T column, K row)
		{
			return GetRowIndex(GetColumnIndex(column), row);
		}
		public sealed override K[] GetColumns(T column)
		{
			int index = GetColumnIndex(column);
			if (index == -1)
				return null;
			return GetColumns(index);
		}
	}
	public class TableRList<T, K> : TableList<T, K>
	{
		struct Enumerator : IEnumerator<KeyValuePair<T, K>>
		{
			private TableRList<T, K> table;
			private int rowIndex;
			private int columnIndex;
			private KeyValuePair<T, K> current;
			public KeyValuePair<T, K> Current
			{
				get { return current; }
			}
			object IEnumerator.Current
			{
				get { return current; }
			}
			public Enumerator(TableRList<T, K> table)
			{
				this.table = table;
				this.rowIndex = 0;
				this.columnIndex = 0;
				this.current = new KeyValuePair<T, K>();
			}
			public void Dispose()
			{
				this.table = null;
			}
			public bool MoveNext()
			{
				if (table.ColumnCount == 0)
					return false;

				if (rowIndex >= table.rows.Count)
					return false;

				current = new KeyValuePair<T, K>(table.columns[columnIndex], table.rows[rowIndex][columnIndex]);

				columnIndex++;
				if (columnIndex >= table.ColumnCount)
				{
					columnIndex = 0;
					rowIndex++;
				}

				return true;
			}
			public void Reset()
			{
				this.rowIndex = 0;
				this.columnIndex = 0;
				this.current = new KeyValuePair<T, K>();
			}
		}

		public override int RowCount
		{
			get { return rows.Count; }
		}
		public override K this[T column, int row]
		{
			get { return rows[row][GetColumnIndex(column)]; }
			set { rows[row][GetColumnIndex(column)] = value; }
		}
		public override K this[int column, int row]
		{
			get { return rows[row][column]; }
			set { rows[row][column] = value; }
		}

		public TableRList()
		{
			columns = new List<T>();
		}
		public TableRList(IEnumerable<T> keys)
		{
			columns = new List<T>(keys);
		}
		public TableRList(params T[] keys)
		{
			columns = new List<T>(keys);
		}
		public TableRList(IEnumerable<Dictionary<T, K>> table)
		{
			bool flag = true;
			foreach (Dictionary<T, K> dic in table)
			{
				if (flag)
				{
					columns = new List<T>(dic.Count);
					foreach (T key in dic.Keys)
					{
						columns.Add(key);
					}
					flag = false;
				}
				AddRow(dic.Values);
			}
		}

		public override int AddColumn(T column, IEnumerable<K> newColumnValues)
		{
			int index = ColumnCount;
			columns.Add(column);

			if (newColumnValues == null)
			{
				for (int i = 0; i < rows.Count; i++)
				{
					rows[i].Add(default(K));
				}
			}
			else
			{
				int i = 0;
				foreach (var value in newColumnValues)
				{
					if (i >= rows.Count)
						break;
					rows[i++].Add(value);
				}
			}

			return index;
		}
		public override void AddRow(IEnumerable<K> newRowValues)
		{
			int count = ColumnCount;
			int i = 0;
			List<K> newRow = new List<K>(count);
			foreach (var value in newRowValues)
			{
				if (i >= count)
					break;
				newRow.Add(value);
				i++;
			}
			for (; i < count; i++)
			{
				newRow.Add(default(K));
			}
			rows.Add(newRow);
		}
		public override bool AddValue(K value)
		{
			List<K> row = null;
			bool newRow;

			if (rows.Count == 0)
			{
				newRow = true;
			}
			else
			{
				row = rows.Last();
				newRow = row.Count == columns.Count;
			}

			if (newRow)
			{
				row = new List<K>(ColumnCount);
				rows.Add(row);
			}

			row.Add(value);

			return newRow;
		}
		public override bool RemoveColumn(int index)
		{
			if (index < 0 || index >= ColumnCount)
				return false;
			columns.RemoveAt(index);
			foreach (var row in rows)
				row.RemoveAt(index);
			return true;
		}
		public override bool RemoveRow(int row)
		{
			if (row < 0 || row >= rows.Count)
				return false;
			rows.RemoveAt(row);
			return true;
		}
		public override int GetRowIndex(int column, K row)
		{
			for (int i = 0; i < rows.Count; i++)
			{
				if (rows[i][column] == null && row != null)
					continue;
				if (rows[i][column].Equals(row))
					return i;
			}
			return -1;
		}
		public override K[] GetColumns(int column)
		{
			int count = RowCount;
			K[] rows = new K[count];
			for (int i = 0; i < count; i++)
				rows[i] = this.rows[i][column];
			return rows;
		}
		public override K[] GetRows(int index)
		{
			return rows[index].ToArray();
		}
		public override Dictionary<T, K[]> ToDictionary()
		{
			Dictionary<T, K[]> table = new Dictionary<T, K[]>();
			for (int i = 0; i < ColumnCount; i++)
				table.Add(columns[i], GetColumns(i));
			return table;
		}
		public Dictionary<T, K>[] ToDictionaryArray()
		{
			Dictionary<T, K>[] array = new Dictionary<T, K>[RowCount];
			for (int i = 0; i < RowCount; i++)
			{
				Dictionary<T, K> value = new Dictionary<T, K>();
				for (int j = 0; j < ColumnCount; j++)
				{
					value.Add(columns[j], rows[i][j]);
				}
				array[i] = value;
			}
			return array;
		}
		public override void Clear(int startRow)
		{
			rows.RemoveRange(startRow, RowCount - startRow);
		}

		public override IEnumerator<KeyValuePair<T, K>> GetEnumerator()
		{
			return new Enumerator(this);
		}
	}
	public class TableCList<T, K> : TableList<T, K>
	{
		struct Enumerator : IEnumerator<KeyValuePair<T, K>>
		{
			private TableCList<T, K> table;
			private int rowIndex;
			private int columnIndex;
			private KeyValuePair<T, K> current;
			public KeyValuePair<T, K> Current
			{
				get { return current; }
			}
			object IEnumerator.Current
			{
				get { return current; }
			}
			public Enumerator(TableCList<T, K> table)
			{
				this.table = table;
				this.rowIndex = 0;
				this.columnIndex = 0;
				this.current = new KeyValuePair<T, K>();
			}
			public void Dispose()
			{
				this.table = null;
			}
			public bool MoveNext()
			{
				if (table.ColumnCount == 0)
					return false;

				if (rowIndex >= table.RowCount)
					return false;

				current = new KeyValuePair<T, K>(table.columns[columnIndex], table.rows[columnIndex][rowIndex]);

				columnIndex++;
				if (columnIndex >= table.ColumnCount)
				{
					columnIndex = 0;
					rowIndex++;
				}

				return true;
			}
			public void Reset()
			{
				this.rowIndex = 0;
				this.columnIndex = 0;
				this.current = new KeyValuePair<T, K>();
			}
		}

		public override int RowCount
		{
			get
			{
				if (ColumnCount == 0)
					return 0;
				else
					return rows.First().Count;
			}
		}
		public override K this[T column, int row]
		{
			get { return rows[GetColumnIndex(column)][row]; }
			set { rows[GetColumnIndex(column)][row] = value; }
		}
		public override K this[int column, int row]
		{
			get { return rows[column][row]; }
			set { rows[column][row] = value; }
		}

		public TableCList()
		{
			columns = new List<T>();
		}
		public TableCList(IEnumerable<T> keys)
		{
			columns = new List<T>(keys);
			for (int i = 0; i < ColumnCount; i++)
				rows.Add(new List<K>());
		}
		public TableCList(params T[] keys)
			: this((IEnumerable<T>)keys)
		{
		}
		public TableCList(IEnumerable<Dictionary<T, K>> table)
		{
			bool flag = true;
			foreach (Dictionary<T, K> dic in table)
			{
				if (flag)
				{
					columns = new List<T>(dic.Count);
					foreach (T key in dic.Keys)
					{
						columns.Add(key);
					}
					flag = false;
				}
				AddRow(dic.Values);
			}
		}

		public override int AddColumn(T column, IEnumerable<K> newColumnValues)
		{
			int index = ColumnCount;
			int count = RowCount;
			List<K> newColumn = new List<K>(count);
			rows.Add(newColumn);
			columns.Add(column);

			if (newColumnValues == null)
			{
				for (int i = 0; i < count; i++)
				{
					newColumn.Add(default(K));
				}
			}
			else
			{
				newColumn.AddRange(newColumnValues);
			}

			return index;
		}
		public override void AddRow(IEnumerable<K> newRowValues)
		{
			int count = ColumnCount;
			int i = 0;
			foreach (var value in newRowValues)
			{
				if (i >= count)
					break;
				rows[i].Add(value);
				i++;
			}
			for (; i < count; i++)
			{
				rows[i].Add(default(K));
			}
		}
		public override bool AddValue(K value)
		{
			if (ColumnCount == 0)
				return false;

			List<K> row = null;
			bool newRow;

			int rowCount = RowCount;
			if (rowCount == 0)
			{
				newRow = true;
			}
			else
			{
				row = rows.FirstOrDefault(r => r.Count < rowCount);
				newRow = row == null;
			}

			if (newRow)
			{
				row = rows.First();
			}
			row.Add(value);

			return newRow;
		}
		public override bool RemoveColumn(int index)
		{
			if (index < 0 || index >= ColumnCount)
				return false;
			columns.RemoveAt(index);
			rows.RemoveAt(index);
			return true;
		}
		public override bool RemoveRow(int row)
		{
			if (row < 0 || row >= RowCount)
				return false;
			foreach (var column in rows)
				column.RemoveAt(row);
			return true;
		}
		public override int GetRowIndex(int column, K row)
		{
			return rows[column].IndexOf(row);
		}
		public override K[] GetColumns(int column)
		{
			return rows[column].ToArray();
		}
		public override K[] GetRows(int index)
		{
			int count = ColumnCount;
			K[] row = new K[count];
			for (int i = 0; i < count; i++)
				row[i] = rows[i][index];
			return row;
		}
		public override Dictionary<T, K[]> ToDictionary()
		{
			Dictionary<T, K[]> table = new Dictionary<T, K[]>();
			for (int i = 0; i < ColumnCount; i++)
				table.Add(columns[i], GetColumns(i));
			return table;
		}
		public Dictionary<T, K>[] ToDictionaryArray()
		{
			int count = RowCount;
			int count2 = ColumnCount;
			Dictionary<T, K>[] array = new Dictionary<T, K>[count];
			for (int i = 0; i < count; i++)
			{
				Dictionary<T, K> value = new Dictionary<T, K>();
				for (int j = 0; j < count2; j++)
				{
					value.Add(columns[j], rows[j][i]);
				}
				array[i] = value;
			}
			return array;
		}
		public override void Clear(int startRow)
		{
			int count = RowCount - startRow;
			for (int i = 0; i < rows.Count; i++)
				rows[i].RemoveRange(startRow, count);
		}

		public override IEnumerator<KeyValuePair<T, K>> GetEnumerator()
		{
			return new Enumerator(this);
		}
	}
	public class StringTable : TableCList<string, string>
	{
		public StringTable()
		{
		}
		public StringTable(params string[] columns)
			: base(columns)
		{
		}
		public StringTable(IEnumerable<Dictionary<string, string>> table)
			: base(table)
		{
		}

		public static StringTable DefaultLanguageTable()
		{
			StringTable table = new StringTable();
			table.AddColumn("ID");
			table.AddColumn("Source");
			table.AddColumn("CHS");
			return table;
		}
	}

    /// <summary>区间</summary>
	public struct Range<T> where T : IComparable<T>
	{
		public T Min;
		public T Max;
		public T SameValue
		{
			set
			{
				Max = value;
				Min = value;
			}
		}
		public Range(T value)
		{
			Min = value;
			Max = value;
		}
		public Range(T max, T min)
		{
			this.Max = max;
			this.Min = min;
		}
		public bool InRange(T value)
		{
			return value.CompareTo(Min) >= 0 && value.CompareTo(Max) <= 0;
		}
		public override string ToString()
		{
			return string.Format("Min: {0} , Max: {1}", Min, Max);
		}
		public override int GetHashCode()
		{
			return Min.GetHashCode() & Max.GetHashCode();
		}
	}
	public class RandomList<T> : IEnumerable<T>
	{
		private T[] list;
		private _RANDOM.Random random;
		private int[] indexes;
		private int lastIndex = -1;

		public T[] List
		{
			get { return list; }
			set
			{
				if (list == null)
					throw new ArgumentNullException("list");
				list = value;
				Reset();
			}
		}
		public _RANDOM.Random Random
		{
			get { return random; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("random");
				random = value;
				Reset();
			}
		}
		public int Count
		{
			get { return list.Length; }
		}
		public int Last
		{
			get
			{
				if (lastIndex == -1)
					return -1;
				return indexes[lastIndex];
			}
		}
		public bool IsLast
		{
			get { return lastIndex == Count - 1; }
		}

		public RandomList()
            : this(new T[0], new RandomDotNet(_RANDOM.Next()))
		{
		}
		public RandomList(IEnumerable<T> list)
            : this(list.ToArray(), new RandomDotNet(_RANDOM.Next()))
		{
		}
		public RandomList(params T[] list)
            : this(list, new RandomDotNet(_RANDOM.Next()))
		{
		}
		public RandomList(T[] list, _RANDOM.Random random)
		{
			this.list = list;
			this.random = random;
			Reset();
		}

		public void Reset()
		{
			int last = Last;
			int count = Count;
			indexes = new int[Count];
			for (int i = 0; i < count; i++)
				indexes[i] = i;
			do
			{
				Utility.Shuffle(indexes);
			} while (last != -1 && count > 0 && last == indexes[0]);
			lastIndex = -1;
		}
		public T Previous()
		{
			if (lastIndex == -1)
				return default(T);
			if (lastIndex > 0)
				lastIndex--;
			return list[Last];
		}
		public T Next()
		{
			int count = Count;
			if (count == 0)
				return default(T);
			if (lastIndex + 1 == count)
				Reset();
			lastIndex++;
			return list[Last];
		}

		public IEnumerator<T> GetEnumerator()
		{
			int count = Count;
			for (int i = 0; i < count; i++)
				yield return list[indexes[i]];
			lastIndex = count - 1;
			Reset();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}

    // 池
    /// <summary>要用到池的对象应该继承此类型以便快速进行池内的增删改</summary>
	public class PoolItem
	{
        internal int poolIndex;
        /// <summary>池对象在池中的索引</summary>
        public int PoolIndex { get { return poolIndex; } }
	}
    public delegate void ActionRef<T>(ref T action);
    /// <summary>池数据结构，可以循环利用对象，减少new对象的次数</summary>
	public class Pool<T> : IEnumerable<T>
	{
		private T[] items;
		private int size;
		private int[] free;

		public int Count
		{
			get { return size; }
		}
		public int Capacity
		{
			get { return items.Length; }
		}
		public T this[int index]
		{
            get
            {
                if (index < 0 || index >= items.Length)
                    return default(T);
                return items[index];
            }
            //set
            //{
            //    if (index < 0 || index >= size)
            //        throw new ArgumentOutOfRangeException();
            //    items[index] = value;
            //}
		}
		public bool IsFull
		{
			get { return size == items.Length; }
		}
		private int Tail
		{
			get { return items.Length - size; }
		}
		private int Next
		{
			get { return Tail - 1; }
		}

		public Pool() : this(4) { }
		public Pool(int capacity)
		{
			Clear(capacity);
		}

		private int Push(T target)
		{
			int index = free[Next];
			items[index] = target;
			size++;

			PoolItem item = target as PoolItem;
			if (item != null)
				item.poolIndex = index;
			return index;
		}
        /// <summary>从池里取出一个空闲的对象</summary>
		public T Allot()
		{
			T item;
			Allot(out item);
			return item;
		}
        /// <summary>从池里取出一个空闲的对象，没有空闲对象时new一个对象</summary>
        public T AllotOrCreate()
        {
            int index;
            return AllotOrCreate(out index);
        }
        /// <summary>从池里取出一个空闲的对象，没有空闲对象时new一个对象</summary>
        /// <param name="index">对象在池里的索引，索引可用于RemoveAt</param>
        public T AllotOrCreate(out int index)
        {
            T item;
            index = Allot(out item);
            if (index == -1)
            {
                item = Activator.CreateInstance<T>();
                index = Add(item);
            }
            return item;
        }
        /// <summary>从池里取出一个空闲的对象</summary>
        /// <returns>对象在池里的索引，索引可用于RemoveAt</returns>
		public int Allot(out T item)
		{
			item = default(T);
			if (!IsFull)
			{
				int index = free[Next];
				item = items[index];
				if (item == null)
					return -1;
				Push(item);
				return index;
			}
			return -1;
		}
        /// <summary>往池里放入一个对象</summary>
		public int Add(T target)
		{
			if (IsFull)
			{
				int num = size * 2;

				T[] clone = new T[num];
				Utility.Copy(items, 0, clone, 0, size);
				this.items = clone;

				this.free = new int[num];
				int tail = Tail;
				int last = num - 1;
				for (int i = 0; i < tail; i++)
				{
					free[i] = last - i;
				}
			}
			// pop free and push target
			return Push(target);
		}
		public int IndexOf(T target)
		{
			return Array.IndexOf(items, target);
		}
        /// <summary>从池里移除一个对象</summary>
		public bool Remove(T target)
		{
			int index = IndexOf(target);
			if (index >= 0)
			{
				RemoveAt(index);
				return true;
			}
			else
			{
				return false;
			}
		}
        /// <summary>从池里通过索引快速移除一个对象</summary>
        public void RemoveAt<U>(U target) where U : PoolItem, T
        {
            RemoveAt(target.PoolIndex);
        }
        /// <summary>从池里通过索引快速移除一个对象</summary>
		public T RemoveAt(int index)
		{
			T target = items[index];
			free[Tail] = index;
			//items[index] = default(T);
			size--;
			return target;
		}
        public int ToArray(ref T[] array)
        {
            return ToArray(ref array, 0);
        }
        public int ToArray(ref T[] array, int arrayStartIndex)
        {
            int size = this.size;
            if (array == null || array.Length < size)
                array = new T[size];
            int capacity = Capacity;
            bool[] isNull = new bool[capacity];
            int tail = Tail;
            for (int i = 0; i < tail; i++)
                isNull[free[i]] = true;
            for (int i = 0; i < capacity && arrayStartIndex < size; i++)
                if (!isNull[i])
                    array[arrayStartIndex++] = items[i];
            return size;
        }
        public T[] ToArray()
		{
            T[] array = null;
            ToArray(ref array);
            return array;
		}
        public void For(ActionRef<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            int capacity = Capacity;
            bool[] isNull = new bool[capacity];
            int tail = Tail;
            for (int i = 0; i < tail; i++)
                isNull[free[i]] = true;
            for (int i = 0; i < capacity; i++)
                if (!isNull[i])
                    action(ref items[i]);
        }
        public void For(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            int capacity = Capacity;
            bool[] isNull = new bool[capacity];
            int tail = Tail;
            for (int i = 0; i < tail; i++)
                isNull[free[i]] = true;
            for (int i = 0; i < capacity; i++)
                if (!isNull[i])
                    //&& i < items.Length) // 有可能action调用Clear减少Capacity导致数组越界
                    action(items[i]);
        }
        public void Clear()
		{
			Clear(4);
		}
		public void Clear(int capacity)
		{
			this.items = new T[capacity];
			this.free = new int[capacity];
			int last = capacity - 1;
			for (int i = 0; i < capacity; i++)
				free[i] = last - i;
            size = 0;
		}
		public void ClearToFree()
		{
			int last = items.Length - 1;
			for (int i = 0; i <= last; i++)
				free[i] = last - i;
            size = 0;
		}

		public IEnumerator<T> GetEnumerator()
		{
            //int capacity = Capacity;
            //bool[] isNull = new bool[capacity];
            //int tail = Tail;
            //for (int i = 0; i < tail; i++)
            //    isNull[free[i]] = true;
            //for (int i = 0; i < capacity; i++)
            //    if (!isNull[i])
            //        yield return items[i];
            return ((IEnumerable<T>)ToArray()).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
	public class PoolStack<T> : IEnumerable<T>
	{
		protected T[] items;
        protected int size;

		public int Count
		{
			get { return size; }
		}
		public int Capcity
		{
			get { return items.Length; }
		}

		public PoolStack()
			: this(4)
		{
		}
		public PoolStack(int capcity)
		{
			items = new T[capcity];
		}
		public PoolStack(IEnumerable<T> array)
		{
			ICollection<T> collection = array as ICollection<T>;
			if (collection != null)
				items = new T[collection.Count];
			else
				items = new T[4];

			foreach (var item in array)
				Push(item);
		}

        public void For(Action<T> action)
        {
            for (int i = size - 1; i >= 0; i--)
                action(items[i]);
        }
        public T Allot()
		{
			T item;
			Allot(out item);
			return item;
		}
		public bool Allot(out T item)
		{
			if (size < Capcity)
			{
				item = items[size];
				if (item == null)
					return false;
				Push(item);
				return true;
			}
			else
			{
				item = default(T);
			}
			return false;
		}
		public void Push(T item)
		{
			if (size == items.Length)
			{
				T[] array = new T[size * 2];
				Array.Copy(items, 0, array, 0, size);
				items = array;
			}
			items[size++] = item;
		}
		public T Peek()
		{
			return items[size - 1];
		}
		public T Pop()
		{
			if (size == 0)
				throw new InvalidOperationException("Empty stack.");
			return items[--size];
		}
		public T[] ToArray()
		{
			T[] array = new T[size];
			for (int i = 0; i < size; i++)
			{
				array[i] = items[size - i - 1];
			}
			return array;
		}
		public void Clear()
		{
			size = 0;
		}
		public void ClearAndTrim(int capcity)
		{
			if (capcity < 4)
				capcity = 4;
			size = capcity;
			Trim();
			size = 0;
		}
		public void Trim()
		{
			int capcity = Capcity;
			if (capcity > size)
			{
				T[] array = new T[size];
				Array.Copy(items, 0, array, 0, size);
				items = array;
			}
		}
		public IEnumerator<T> GetEnumerator()
		{
			for (int i = size - 1; i >= 0; i--)
			{
				yield return items[i];
			}
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
	/// <summary>
	/// 池堆
	/// 将BufferSize个对象视为一堆
	/// 整个池能放满PoolSize堆时溢出
	/// </summary>
	public class PoolHeap<T> : IEnumerable<T>
	{
		private int poolSize;
		private int bufferSize;
		private Queue<T[]> records;
		private T[] buffers;
		private int index;
		public event Action<T> PoolFlush;

		public int PoolSize
		{
			get { return poolSize; }
			set
			{
				if (poolSize != value)
				{
					Flush();
					poolSize = value;
					InitPool();
				}
			}
		}
		public int BufferSize
		{
			get { return bufferSize; }
			set
			{
				if (bufferSize != value)
				{
					Flush();
					bufferSize = value;
					InitPool();
				}
			}
		}
		public bool PoolFull
		{
			get { return index == bufferSize && records.Count == poolSize; }
		}
		public bool HeapFull
		{
			get { return index == bufferSize; }
		}

		public PoolHeap() : this(32, 4096) { }
		public PoolHeap(int poolSize, int bufferSize)
		{
			this.poolSize = poolSize;
			this.bufferSize = bufferSize;
			InitPool();
		}
		public PoolHeap(int poolSize, int bufferSize, Action<T> recorder)
			: this(poolSize, bufferSize)
		{
			this.PoolFlush = recorder;
		}

		private void InitPool()
		{
			records = new Queue<T[]>(poolSize);
			buffers = new T[bufferSize];
		}
		public void Add(T record)
		{
			if (index == bufferSize)
			{
				if (records.Count == poolSize)
				{
					Flush();
				}
				else
				{
					records.Enqueue(buffers);
				}
				buffers = new T[bufferSize];
				index = 0;
			}
			buffers[index++] = record;
		}
		public void Flush()
		{
			if (PoolFlush != null)
			{
				while (records.Count > 0)
				{
					T[] queue = records.Dequeue();
					for (int i = 0; i < bufferSize; i++)
					{
						PoolFlush(queue[i]);
					}
				}
			}
			records.Clear();

			if (index > 0)
			{
				if (PoolFlush != null)
				{
					for (int i = 0; i < index; i++)
					{
						PoolFlush(buffers[i]);
					}
				}
				buffers = new T[bufferSize];
			}
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			foreach (var record in records)
				for (int i = 0; i < bufferSize; i++)
					yield return record[i];

			if (index > 0)
				for (int i = 0; i < index; i++)
					yield return buffers[i];
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}
	}


    // 树
    /// <summary>树数据结构</summary>
    public abstract class Tree<T> : IEnumerable<T> where T : Tree<T>
    {
        protected internal List<T> Childs
        {
            get;
            private set;
        }
        /// <summary>整棵树的根节点</summary>
        public T Root
        {
            get
            {
                T root = (T)this;
                while (true)
                {
                    if (root.Parent == null)
                        return root;
                    else
                        root = root.Parent;
                }
            }
        }
        /// <summary>当前节点的父节点</summary>
        public T Parent
        {
            get;
            private set;
        }
        /// <summary>当前节点的首个子节点</summary>
        public T First
        {
            get { return Childs[0]; }
        }
        /// <summary>当前节点的末尾子节点</summary>
        public T Last
        {
            get { return Childs[Childs.Count - 1]; }
        }
        public T this[int index]
        {
            get { return Childs[index]; }
        }
        /// <summary>当前节点在数中的深度，根节点从0算起</summary>
        public int Depth
        {
            get
            {
                int depth = 0;
                T parent = this.Parent;
                while (parent != null)
                {
                    depth++;
                    parent = parent.Parent;
                }
                return depth;
            }
        }
        /// <summary>子节点的数量</summary>
        public int ChildCount
        {
            get { return Childs.Count; }
        }

        public Tree()
        {
            Childs = new List<T>();
        }

        protected virtual bool CheckAdd(T node)
        {
            return true;
        }
        /// <summary>给此节点添加一个子节点</summary>
        public bool Add(T node)
        {
            return Insert(node, Childs.Count);
        }
        /// <summary>给此节点添加多个子节点</summary>
        public void AddRange(IEnumerable<T> nodes)
        {
            foreach (var node in nodes)
                Add(node);
        }
        public T[] GetAllChilds()
        {
            return Childs.ToArray();
        }
        /// <summary>遍历此节点及其子节点，先处理自己，再处理子节点</summary>
        /// <param name="skip">返回true则不处理节点及其子节点</param>
        /// <param name="func">需要对遍历的节点进行的操作</param>
        public void ForeachParentPriority(Func<T, bool> skip, Action<T> func)
        {
            ForParentPriority((T)this, skip, func);
        }
        /// <summary>遍历此节点及其子节点，先处理子节点，再处理自己</summary>
        /// <param name="skip">返回true则不处理节点及其子节点</param>
        /// <param name="func">需要对遍历的节点进行的操作</param>
        public void ForeachChildPriority(Func<T, bool> skip, Action<T> func)
        {
            ForChildPriority((T)this, skip, func);
        }
        /// <summary>给此节点在指定位置插入一个子节点</summary>
        public bool Insert(T node, int index)
        {
            if (node == null)
                throw new ArgumentNullException("graph node");

            if (CheckAdd(node))
            {
                if (node.Parent != null)
                    node.Parent.Remove(node);
                OnAdd(node, index);
                Childs.Insert(index, node);
                node.Parent = (T)this;
                OnAdded(node, index);
                node.OnAddedBy((T)this, index);
                return true;
            }
            return false;
        }
        protected virtual void OnAdd(T node, int index)
        {
        }
        protected virtual void OnAdded(T node, int index)
        {
        }
        protected virtual void OnAddedBy(T parent, int index)
        {
        }
        /// <summary>删除指定的子节点</summary>
        public bool Remove(T node)
        {
            bool result = Childs.Remove(node);
            if (result)
            {
                node.Parent = null;
                node.OnRemovedBy((T)this);
                OnRemoved(node);
            }
            return result;
        }
        /// <summary>删除指定位置的子节点</summary>
        public void Remove(int index)
        {
            T node = Childs[index];
            node.Parent = null;
            Childs.RemoveAt(index);
            node.OnRemovedBy((T)this);
            OnRemoved(node);
        }
        protected virtual void OnRemoved(T node)
        {
        }
        protected virtual void OnRemovedBy(T parent)
        {
        }
        /// <summary>清除所有子节点</summary>
        public void Clear()
        {
            T[] item = this.ToArray();
            for (int i = Childs.Count - 1; i >= 0; i--)
                Remove(i);
        }
        /// <summary>遍历此节点及其子节点，找到符合条件的节点</summary>
        public T Find(Func<T, bool> func)
        {
            return FindParentPriority((T)this, null, func);
        }
        public int IndexOf(T node)
        {
            return Childs.IndexOf(node);
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            T[] array = Childs.ToArray();
            int count = array.Length;
            for (int i = 0; i < count; i++)
                yield return array[i];
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static T FindParentPriority(T parent, Func<T, bool> skip, Func<T, bool> match)
        {
            if (skip != null && skip(parent))
                return null;
            if (match(parent))
                return parent;
            //foreach (T child in parent.Childs)
            for (int i = 0; i < parent.Childs.Count; i++)
            {
                T target = FindParentPriority(parent.Childs[i], skip, match);
                if (target != null)
                    return target;
            }
            return null;
        }
        public static T FindChildPriority(T parent, Func<T, bool> skip, Func<T, bool> match)
        {
            if (skip != null && skip(parent))
                return null;
            //foreach (T child in parent.Childs)
            for (int i = 0; i < parent.Childs.Count; i++)
            {
                T target = FindChildPriority(parent.Childs[i], skip, match);
                if (target != null)
                    return target;
            }
            if (match(parent))
                return parent;
            else
                return null;
        }
        /// <summary>For：Childs里的所有元素</summary>
        public static void ForParentPriority(T parent, Func<T, bool> skip, Action<T> func)
        {
            if (skip != null && skip(parent))
                return;
            func(parent);
            //foreach (T child in parent.Childs)
            for (int i = 0; i < parent.Childs.Count; i++)
                ForParentPriority(parent.Childs[i], skip, func);
        }
        public static void ForChildPriority(T parent, Func<T, bool> skip, Action<T> func)
        {
            if (skip != null && skip(parent))
                return;
            //foreach (T child in parent.Childs)
            for (int i = 0; i < parent.Childs.Count; i++)
                ForChildPriority(parent.Childs[i], skip, func);
            func(parent);
        }
        public static List<T> Elements(T parent, Func<T, bool> skip)
        {
            List<T> elements = new List<T>();
            ForParentPriority(parent, skip, e => elements.Add(e));
            return elements;
        }
        public static T FindEachParentPriority(T parent, Func<T, bool> skip, Func<T, bool> match)
        {
            if (skip != null && skip(parent))
                return null;
            if (match(parent))
                return parent;
            foreach (T child in parent)
            {
                T target = FindEachParentPriority(child, skip, match);
                if (target != null)
                    return target;
            }
            return null;
        }
        public static T FindEachChildPriority(T parent, Func<T, bool> skip, Func<T, bool> match)
        {
            if (skip != null && skip(parent))
                return null;
            foreach (T child in parent)
            {
                T target = FindEachChildPriority(child, skip, match);
                if (target != null)
                    return target;
            }
            if (match(parent))
                return parent;
            else
                return null;
        }
        /// <summary>Foreach：可以重写GetEnumerator影响此结果</summary>
        public static void ForeachParentPriority(T parent, Func<T, bool> skip, Action<T> func)
        {
            if (skip != null && skip(parent))
                return;
            func(parent);
            foreach (T child in parent)
                ForeachParentPriority(child, skip, func);
        }
        public static void ForeachChildPriority(T parent, Func<T, bool> skip, Action<T> func)
        {
            if (skip != null && skip(parent))
                return;
            foreach (T child in parent)
                ForeachChildPriority(child, skip, func);
            func(parent);
        }
        public static List<T> EachElements(T parent, Func<T, bool> skip)
        {
            List<T> elements = new List<T>();
            ForeachParentPriority(parent, skip, e => elements.Add(e));
            return elements;
        }
        public static Stack<T> GetParents(T element)
        {
            Stack<T> parents = new Stack<T>();
            for (T i = element.Parent; i != null; i = i.Parent)
                parents.Push(i);
            return parents;
        }
        public static Stack<T> GetParentsWithMe(T element)
        {
            Stack<T> parents = new Stack<T>();
            for (T i = element; i != null; i = i.Parent)
                parents.Push(i);
            return parents;
        }
        public static void ForRootToLeaf(T parent, Action<T> func)
        {
            ForRootToLeaf(parent, func, null);
        }
        /// <summary>父节点全部循环完后，再循环父节点的</summary>
        public static void ForRootToLeaf(T parent, Action<T> func, Action<T> newNode)
        {
            if (newNode != null) newNode(parent);
            if (func != null)
                for (int i = 0; i < parent.Childs.Count; i++)
                    func(parent.Childs[i]);
            for (int i = 0; i < parent.Childs.Count; i++)
                ForRootToLeaf(parent.Childs[i], func, newNode);
        }
    }
    public class BTree<T> : IEnumerable<T>
    {
        class Node
        {
            public T Data;
            public Node Left;
            public Node Right;
            public Node(T data)
            {
                Data = data;
            }
        }
        private Comparison<T> comparer;
        private Node root;
        private int count;

        public int Count
        {
            get { return count; }
        }
        public T this[T key]
        {
            get
            {
                Node node = FindEntry(key);
                if (node == null)
                    throw new KeyNotFoundException();
                return node.Data;
            }
        }
        private static Comparison<T> GetDefaultComparison()
        {
            var comparer = Comparer<T>.Default;
            if (comparer != null)
                return comparer.Compare;
            return null;
        }
        public BTree() : this(GetDefaultComparison()) { }
        public BTree(Comparison<T> comparer)
        {
            this.comparer = comparer;
        }
        public BTree(T root) : this(GetDefaultComparison(), root) { }
        public BTree(Comparison<T> comparer, T root)
        {
            this.comparer = comparer;
            this.root = new Node(root);
        }
        private Node FindEntry(T key)
        {
            if (root == null) return null;
            var node = root;
            while (node != null)
            {
                int value = comparer(node.Data, key);
                if (value == 0)
                    return node;
                else if (value < 0)
                    node = node.Right;
                else
                    node = node.Left;
            }
            return null;
        }
        public bool Contains(T key)
        {
            return FindEntry(key) != null;
        }
        public bool Add(T key)
        {
            if (root == null)
            {
                count = 1;
                root = new Node(key);
                return true;
            }
            var node = root;
            while (true)
            {
                int value = comparer(node.Data, key);
                if (value == 0)
                {
                    return false;
                }
                else if (value < 0)
                {
                    if (node.Right == null)
                    {
                        count++;
                        node.Right = new Node(key);
                        return true;
                    }
                    node = node.Right;
                }
                else
                {
                    if (node.Left == null)
                    {
                        count++;
                        node.Left = new Node(key);
                        return true;
                    }
                    node = node.Left;
                }
            }
            throw new InvalidOperationException();
        }
        public bool Remove(T key)
        {
            var node = root;
            var parent = node;
            while (true)
            {
                int value = comparer(node.Data, key);
                if (value == 0)
                {
                    if (node == parent.Left)
                    {
                        if (node.Left != null)
                        {
                            parent.Left = node.Left;
                        }
                        else if (node.Right != null)
                        {
                            parent.Left = node.Right;
                        }
                        else
                        {
                            parent.Left = null;
                        }
                    }
                    else if (node == parent.Right)
                    {
                        if (node.Left != null)
                        {
                            parent.Right = node.Left;
                        }
                        else if (node.Right != null)
                        {
                            parent.Right = node.Right;
                        }
                        else
                        {
                            parent.Right = null;
                        }
                    }
                    else
                    {
                        // delete root
                        root = null;
                    }
                    count--;
                    return true;
                }
                else if (value < 0)
                {
                    if (node.Right == null)
                    {
                        return false;
                    }
                    parent = node;
                    node = node.Right;
                }
                else
                {
                    if (node.Left == null)
                    {
                        return false;
                    }
                    parent = node;
                    node = node.Left;
                }
            }
        }
        public void Clear()
        {
            count = 0;
            root = null;
        }

        public IEnumerable<T> ForeachLeftToRight()
        {
            return ForeachLeftToRight(root);
        }
        public IEnumerable<T> ForeachRightToLeft()
        {
            return ForeachRightToLeft(root);
        }
        public IEnumerator<T> GetEnumerator()
        {
            return ForeachLeftToRight(root).GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        static IEnumerable<T> ForeachLeftToRight(Node node)
        {
            if (node == null)
                yield break;
            if (node.Left != null)
                foreach (var item in ForeachLeftToRight(node.Left))
                    yield return item;
            yield return node.Data;
            if (node.Right != null)
                foreach (var item in ForeachLeftToRight(node.Right))
                    yield return item;
        }
        static IEnumerable<T> ForeachRightToLeft(Node node)
        {
            if (node == null)
                yield break;
            if (node.Right != null)
                foreach (var item in ForeachRightToLeft(node.Right))
                    yield return item;
            yield return node.Data;
            if (node.Left != null)
                foreach (var item in ForeachRightToLeft(node.Left))
                    yield return item;
        }
    }
#if SERVER
    /// <summary>
    /// 平衡二叉树
    /// 扩展应用: B+Tree
    ///   节点类型又为一个平衡二叉树（一个节点也叫页），树里的数据一次性通过文件读取到内存，内存数据主要是数据的主键
    ///   硬盘读取数据最小单位也是一页，一页大小为16kb，所以B+Tree的页也可以设置为16kb
    /// </summary>
    public abstract class AVLTreeBase<T> : ICollection<T>
    {
        protected class AVLTreeVisitor
        {
            /// <summary>Find时经过的路径，可以节约AVLTreeNode.Parent的内存</summary>
            public AVLTreeNode[] Path = new AVLTreeNode[32];
            /// <summary>路径长度</summary>
            public int Length;
            /// <summary>0. 找到了目标 / -1. 最后往左没找到目标 / 1. 最后往右没找到目标</summary>
            public sbyte Flag;
            public bool Found { get { return Flag == 0 && Length > 0; } }
            public AVLTreeNode this[int index]
            {
                get
                {
                    if (index < 0 || index >= Length)
                        return null;
                    return Path[index];
                }
            }
            /// <summary>寻找到的目标</summary>
            public AVLTreeNode Target
            {
                get
                {
                    if (Length == 0)
                        return null;
                    return Path[Length - 1];
                }
            }
            public bool IsLeft(int index)
            {
                return Path[index - 1].Left == Path[index];
            }
            public AVLTreeVisitor SetResult(sbyte flag)
            {
                this.Flag = flag;
                return this;
            }
        }
        public class AVLTreeNode
        {
            public byte Height;
            public AVLTreeNode Left;
            public AVLTreeNode Right;
            public T Value;
            public int BalanceFactor
            {
                get
                {
                    int l = Left == null ? -1 : Left.Height;
                    int r = Right == null ? -1 : Right.Height;
                    return l - r;
                }
            }
            public void ResetHeight()
            {
                int l = Left == null ? -1 : Left.Height;
                int r = Right == null ? -1 : Right.Height;
                Height = (byte)((l > r ? l : r) + 1);
            }
            public void CopyTo(AVLTreeNode node)
            {
                node.Height = this.Height;
                node.Left = this.Left;
                node.Right = this.Right;
                node.Value = this.Value;
            }
            public override string ToString()
            {
                return Value == null ? "null" : Value.ToString();
            }
        }
        // 可扩展多个相同的值，此时可以是HashSet，List，或其它
        public class AVLTreeNodeList : AVLTreeNode
        {
            /// <summary>相同键的元素</summary>
            public ICollection<T> Same;
        }

        private int count;
        protected AVLTreeNode root;
        private AVLTreeVisitor visitor = new AVLTreeVisitor();

        public AVLTreeNode Root { get { return root; } }
        public int Count { get { return count; } }
        public int Height { get { return root == null ? -1 : root.Height; } }
        public bool IsReadOnly { get { return false; } }

        public abstract int Compare(T x, T y);
        /// <summary>查找元素</summary>
        /// <param name="item">要查找的目标</param>
        /// <returns>查找结果</returns>
        private AVLTreeVisitor Find(T item)
        {
            visitor.Flag = 0;
            visitor.Length = 0;

            if (root == null)
                return visitor;

            AVLTreeNode node = root;
            int c;
            while (true)
            {
                visitor.Path[visitor.Length++] = node;
                c = Compare(item, node.Value);
                if (c == 0)
                    return visitor.SetResult(0);
                else if (c > 0)
                    // 向右没找到
                    if (node.Right == null) return visitor.SetResult(1);
                    else node = node.Right;
                else
                    // 向左没找到
                    if (node.Left == null) return visitor.SetResult(-1);
                    else node = node.Left;
            }
        }
        protected virtual AVLTreeNode CreateNode()
        {
            return new AVLTreeNode();
        }
        public bool Contains(T item)
        {
            return Find(item).Found;
        }
        /// <summary>将已经升序排好序的对象快速插入以构建树</summary>
        public void SetSortedValuesASC(IList<T> list)
        {
            throw new NotImplementedException();
        }
        /// <summary>将已经降序排好序的对象快速插入以构建树</summary>
        public void SetSortedValuesDESC(IList<T> list)
        {
            throw new NotImplementedException();
        }
        public void Add(T item)
        {
            Insert(item);
        }
        /// <summary>可以添加重复键的节点</summary>
        /// <param name="item">item算出来的键一样，值不一样则插入，否则将调用Update</param>
        public bool Insert(T item)
        {
            if (root == null)
            {
                root = CreateNode();
                root.Value = item;
            }
            else
            {
                var find = Find(item);
                var node = find.Target;
                if (find.Found)
                {
                    // 找到了，替换 | 插入
                    var ret = FindInsert(item, node);
                    if (ret != node)
                    {
                        // 更换节点
                        var parent = find[find.Length - 2];
                        if (parent == null)
                            root = ret;
                        else
                            if (parent.Left == node)
                                parent.Left = ret;
                            else
                                parent.Right = ret;
                    }
                }
                else
                {
                    bool noUncleFlag;
                    AVLTreeNode newNode = CreateNode();
                    newNode.Value = item;
                    visitor.Path[visitor.Length++] = newNode;
                    // 插入时一定是往叶子节点插入，所以高度一定是1
                    node.Height = 1;
                    if (find.Flag > 0)
                    {
                        node.Right = newNode;
                        noUncleFlag = node.Left == null;
                    }
                    else
                    {
                        node.Left = newNode;
                        noUncleFlag = node.Right == null;
                    }

                    // 有叔叔节点时一定平衡
                    if (noUncleFlag && find.Length > 2)
                    {
                        AVLTreeNode child, parent, gparent;
                        int index = find.Length - 1;
                        child = find[index--];
                        parent = find[index--];
                        gparent = find[index];
                        // 重算高度 & 找到失衡节点 & 单次旋转平衡
                        byte height = parent.Height;
                        int b;
                        do
                        {
                            gparent.Height = ++height;
                            b = gparent.BalanceFactor;
                            // 找到失衡节点
                            if (b == -2 || b == 2)
                            {
                                // 旋转平衡
                                ToBalance(child, parent, gparent, find[index - 1]);
                                break;
                            }
                            child = parent;
                            parent = gparent;
                            gparent = find[--index];
                        } while (gparent != null);
                    }
                }
            }
            count++;
            return true;
        }
        /// <summary>通过旋转让树保持平衡</summary>
        /// <param name="gparent">失衡节点</param>
        private void ToBalance(AVLTreeNode child, AVLTreeNode parent, AVLTreeNode gparent, AVLTreeNode ggparent)
        {
            // 左右
            if (parent.Right == child && gparent.Left == parent)
            {
                gparent.Left = child;
                parent.Right = child.Left;
                child.Left = parent;

                byte temp = parent.Height;
                parent.Height = child.Height;
                child.Height = temp;

                RotateRight(gparent, ggparent);
            }
            // 右左
            else if (parent.Left == child && gparent.Right == parent)
            {
                gparent.Right = child;
                parent.Left = child.Right;
                child.Right = parent;

                byte temp = parent.Height;
                parent.Height = child.Height;
                child.Height = temp;

                RotateLeft(gparent, ggparent);
            }
            // 左左
            else if (parent.Left == child && gparent.Left == parent)
                RotateRight(gparent, ggparent);
            // 右右
            else
                RotateLeft(gparent, ggparent);
        }
        private void RotateLeft(AVLTreeNode rotation, AVLTreeNode parent)
        {
            //rotation.Height -= 2;
            var right = rotation.Right;
            if (parent != null)
                if (parent.Left == rotation)
                    parent.Left = right;
                else
                    parent.Right = right;
            rotation.Right = right.Left;
            right.Left = rotation;
            rotation.ResetHeight();
            right.ResetHeight();
            if (parent != null)
                parent.ResetHeight();
            if (rotation == root)
                root = right;
        }
        private void RotateRight(AVLTreeNode rotation, AVLTreeNode parent)
        {
            //rotation.Height -= 2;
            var left = rotation.Left;
            if (parent != null)
                if (parent.Left == rotation)
                    parent.Left = left;
                else
                    parent.Right = left;
            rotation.Left = left.Right;
            left.Right = rotation;
            rotation.ResetHeight();
            left.ResetHeight();
            if (parent != null)
                parent.ResetHeight();
            if (rotation == root)
                root = left;
        }
        /// <summary>插入时找到了相应的节点，默认替换节点</summary>
        /// <param name="item">要插入的键</param>
        /// <param name="node">找到的节点</param>
        protected virtual AVLTreeNode FindInsert(T item, AVLTreeNode node)
        {
            node.Value = item;
            return node;
        }
        /// <summary>更新节点的键，请求重新排序</summary>
        public void Update(T item)
        {
            if (Remove(item))
                Add(item);
        }
        public bool Remove(T item)
        {
            if (root == null) return false;
            var find = Find(item);
            if (find.Found)
            {
                if (DeleteNode(find))
                {
                    int index = find.Length;
                    AVLTreeNode node;
                    // 删除使原本就是短边的树枝变得更加短导致不平衡，相当于另一边树枝插入导致不平衡
                    // 对另一边树枝使用插入相同的算法来维持树平衡
                    // 旋转保持平衡后，可能引起树枝高度变短，从而再次引起不平衡，所以需要一致回溯到根节点，进行多次旋转
                    int b;
                    // 变短需要一直回溯向上更新节点高度
                    while (index > 0)
                    {
                        // 向上回溯 & 更新高度
                        index--;
                        node = find.Path[index];
                        node.ResetHeight();
                        b = node.BalanceFactor;
                        if (b == -2)
                        {
                            // 左边矮，相当于右边插入，向右寻找旋转方案
                            var parent = node.Right;
                            var child = parent.BalanceFactor > 0 ? parent.Left : parent.Right;
                            // 旋转
                            ToBalance(child, parent, node, find[index - 1]);
                        }
                        else if (b == 2)
                        {
                            // 右边矮，相当于左边插入，向左寻找旋转方案
                            var parent = node.Left;
                            var child = parent.BalanceFactor < 0 ? parent.Right : parent.Left;
                            // 旋转
                            ToBalance(child, parent, node, find[index - 1]);
                        }
                    }
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 删除节点，有些情况不需要通过旋转来保持树平衡
        /// <para>不需要平衡</para>
        /// <para>1. 对于同值单一节点</para>
        /// <para>2. 删除节点的左右树枝平衡（非叶子节点），即删除完后，无论左还是右代替其原位置，整棵树高度不变</para>
        /// <para>需要平衡</para>
        /// <para>1. 短边删除，导致父级直接失衡</para>
        /// <para>2. 长边删除，父级虽然平衡，但整条边可能变短，引起上层的不平衡</para>
        /// </summary>
        /// <returns>是否需要通过旋转保持平衡</returns>
        protected virtual bool DeleteNode(AVLTreeVisitor visitor)
        {
            count--;
            if (count == 0)
            {
                root = null;
                return false;
            }
            else
            {
                // 获得要删除的节点，并从路径中移除
                int index = visitor.Length - 1;
                var node = visitor.Path[index];
                var parent = visitor[index - 1];

                // 删除非叶子节点且左右平衡的树，替换节点后高度不变，整棵树任然平衡
                bool balance = node.Left != null && node.Right != null && node.Left.Height == node.Right.Height;

                // 删除节点有左节点时，把左节点中最大的节点替换原节点
                // 删除节点只有右节点时，直接把右节点替换原节点
                if (node.Left != null)
                {
                    // 找到左节点中最大的节点
                    var max = node.Left;
                    while (true)
                    {
                        if (max.Right == null)
                        {
                            // 替换到被删除节点额为止
                            visitor.Path[index] = max;
                            break;
                        }
                        else
                            visitor.Path[visitor.Length++] = max;
                        max = max.Right;
                    }
                    // 替换到原节点
                    if (parent == null)
                        root = max;
                    else if (parent.Left == node)
                        parent.Left = max;
                    else
                        parent.Right = max;
                    // 最大节点被移走，将其父节点的右节点指向其左节点
                    var maxParent = visitor[visitor.Length - 1];
                    if (maxParent != null)
                        maxParent.Right = max.Left;
                    max.Left = node.Left;
                    max.Right = node.Right;
                    // 更新父节点高度
                    //int l = visitor.Length - 1;
                    //while (l >= 0 && visitor.Path[l] != parent)
                    //    visitor.Path[l--].ResetHeight();
                }
                else
                {
                    // 替换到原节点
                    if (parent == null)
                        root = node.Right;
                    else if (parent.Left == node)
                        parent.Left = node.Right;
                    else
                        parent.Right = node.Right;
                }

                if (count < 3) return false;
                return !balance;
            }
        }
        public void Clear()
        {
            root = null;
            count = 0;
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (arrayIndex + count >= array.Length)
                throw new IndexOutOfRangeException();

            foreach (var item in LeftToRight())
                array[arrayIndex++] = item;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return LeftToRight().GetEnumerator();
        }
        public IEnumerable<T> GetOrderBy(bool asc)
        {
            if (asc)
                return LeftToRight();
            else
                return RightToLeft();
        }
        public IEnumerable<T> LeftToRight()
        {
            return LeftToRight(root);
        }
        public IEnumerable<T> RightToLeft()
        {
            return RightToLeft(root);
        }
        private IEnumerable<T> Empty()
        {
            yield break;
        }
        public IEnumerable<T> SmallerThan(T item, bool equals, bool asc)
        {
            var find = Find(item);
            var node = find.Target;
            if (node == null) return Empty();
            if (equals)
                return Left(node, asc);
            else
                return Left(node.Left, asc);
        }
        public IEnumerable<T> BiggerThan(T item, bool equals, bool asc)
        {
            var find = Find(item);
            var node = find.Target;
            if (node == null) return Empty();
            if (equals)
                if (asc)
                    return LeftToRight(node);
                else
                    return RightToLeft(node);
            else
                if (asc)
                    return LeftToRight(node.Left);
                else
                    return RightToLeft(node.Left);
        }
        private IEnumerable<T> LeftToRight(AVLTreeNode node)
        {
            if (node == null) yield break;
            if (node.Left != null)
                foreach (var item in LeftToRight(node.Left))
                    yield return item;
            yield return node.Value;
            if (node.Right != null)
                foreach (var item in LeftToRight(node.Right))
                    yield return item;
        }
        private IEnumerable<T> RightToLeft(AVLTreeNode node)
        {
            if (node.Left != null)
                foreach (var item in LeftToRight(node.Left))
                    yield return item;
            yield return node.Value;
            if (node.Right != null)
                foreach (var item in LeftToRight(node.Right))
                    yield return item;
        }
        private IEnumerable<T> Left(AVLTreeNode node, bool asc)
        {
            if (node == null) yield break;
            if (asc)
            {
                int height = node.Height;
                AVLTreeNode[] path = new AVLTreeNode[height + 1];
                path[0] = node;
                for (int i = 1; i <= height; i++)
                {
                    node = node.Left;
                    if (node != null)
                        path[i] = node;
                    else
                        break;
                }
                if (path[height] == null)
                    height--;
                for (int i = height; i >= 0; i--)
                    yield return path[i].Value;
            }
            else
            {
                while (node != null)
                {
                    yield return node.Value;
                    node = node.Left;
                }
            }
        }
        private IEnumerable<T> Right(AVLTreeNode node, bool asc)
        {
            if (node == null) yield break;
            if (asc)
            {
                while (node != null)
                {
                    yield return node.Value;
                    node = node.Right;
                }
            }
            else
            {
                int height = node.Height;
                AVLTreeNode[] path = new AVLTreeNode[height + 1];
                path[0] = node;
                for (int i = 1; i <= height; i++)
                {
                    node = node.Right;
                    if (node != null)
                        path[i] = node;
                    else
                        break;
                }
                if (path[height] == null)
                    height--;
                for (int i = height; i >= 0; i--)
                    yield return path[i].Value;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        //public void Draw(string output)
        //{
        //    int depth = Height + 1;
        //    int w = (int)Math.Pow(2, depth);
        //    int height = 50;
        //    int y = 10;
        //    using (Bitmap bitmap = new Bitmap(w * height, depth * height + y * 2))
        //    {
        //        using (Graphics g = Graphics.FromImage(bitmap))
        //        {
        //            Font font = new Font("黑体", 12);

        //            Action<int, int, int, AVLTreeNode> draw = null;
        //            draw = (_x, _y, _w, node) =>
        //            {
        //                if (node == null) return;
        //                if (node.Left != null)
        //                    g.DrawLine(Pens.Red, _x, _y, _x - (_w >> 1), _y + height);
        //                if (node.Right != null)
        //                    g.DrawLine(Pens.Red, _x, _y, _x + (_w >> 1), _y + height);
        //                int __x = _x - (height >> 1);
        //                int __y = _y - (height >> 1);
        //                g.FillEllipse(Brushes.Black, __x, __y, height, height);
        //                string text = node == null ? "Null" : node.ToString();
        //                var size = g.MeasureString(text, font);
        //                g.DrawString(text, font, Brushes.White,
        //                    _x - ((int)size.Width >> 1),
        //                    _y - ((int)size.Height >> 1));
        //                g.DrawString(node.Height.ToString(), font, Brushes.Red, _x, _y);

        //                draw(_x - (_w >> 1), _y + height, _w >> 1, node.Left);
        //                draw(_x + (_w >> 1), _y + height, _w >> 1, node.Right);
        //            };

        //            draw((bitmap.Width >> 1), y + (height >> 1), (bitmap.Width >> 1), root);
        //        }
        //        bitmap.Save(output);
        //    }
        //}
    }
    public class AVLTreeSimple<T> : AVLTreeBase<T> where T : IComparable<T>
    {
        public override int Compare(T x, T y)
        {
            return x.CompareTo(y);
        }
    }
    public class AVLTree<T> : AVLTreeBase<T> where T : class
    {
        private Comparison<T> comparer;
        private Func<ICollection<T>> sameElementList;

        public AVLTree() : this(Comparer<T>.Default.Compare) { }
        public AVLTree(Func<T, int> comparer) : this((x, y) => comparer(x) - comparer(y)) { }
        public AVLTree(IComparer<T> comparer) : this(comparer.Compare) { }
        public AVLTree(Comparison<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("比较器不能为空");
            this.comparer = comparer;
        }

        public override int Compare(T x, T y)
        {
            if (x == y) return 0;
            return comparer(x, y);
        }
        /// <summary>相同键的元素将用集合来存储，这里指定使用哪种集合类（只能指定一次）</summary>
        /// <param name="func">返回集合的新实例</param>
        public void SetSameElementList(Func<ICollection<T>> func)
        {
            if (sameElementList != null)
                throw new InvalidOperationException("不能重复指定集合类型");
            this.sameElementList = func;
        }
        protected override AVLTreeNode FindInsert(T item, AVLTreeNode node)
        {
            if (sameElementList == null || node.Value == null)
            {
                node.Value = item;
                return node;
            }
            else
            {
                AVLTreeNodeList list = node as AVLTreeNodeList;
                if (node == null)
                {
                    node.CopyTo(list);
                    list.Same = sameElementList();
                }
                list.Same.Add(item);
                return list;
            }
        }
        protected override bool DeleteNode(AVLTreeBase<T>.AVLTreeVisitor visitor)
        {
            return base.DeleteNode(visitor);
        }
    }
#endif


    // 协程
    /// <summary>协程的状态</summary>
    public enum EAsyncState
    {
        /// <summary>刚创建的协程</summary>
        Created,
        /// <summary>正在执行中</summary>
        Running,
        /// <summary>执行完成</summary>
        Success,
        /// <summary>执行取消</summary>
        Canceled,
        /// <summary>执行过程中出现异常</summary>
        Faulted,
    }
    /// <summary>协程基类</summary>
    public abstract class Async : ICoroutine
    {
        protected const byte COMPLETED = 100;
        private byte progress;

        public bool IsSuccess
        {
            get { return State == EAsyncState.Success; }
        }
        public bool IsCanceled
        {
            get { return State == EAsyncState.Canceled; }
        }
        public bool IsFaulted
        {
            get { return State == EAsyncState.Faulted; }
        }
        /// <summary>当前协程执行的状态</summary>
        public EAsyncState State
        {
            get;
            private set;
        }
        /// <summary>协程是否执行完成</summary>
        public bool IsEnd
        {
            get { return State > EAsyncState.Running; }
        }
        /// <summary>协程执行的进度，0~100</summary>
        public byte Progress
        {
            get { return progress; }
            protected set
            {
                progress = (byte)_MATH.Min(value, COMPLETED);
                if (progress == COMPLETED)
                {
                    if (!IsEnd)
                    {
                        State = EAsyncState.Success;
                        Complete(EAsyncState.Success);
                    }
                }
                else
                {
                    CheckCompleted();
                }
            }
        }
        /// <summary>协程执行的进度，0~1</summary>
        public float ProgressFloat
        {
            get { return progress * 0.01f; }
            set { Progress = (byte)(value * COMPLETED); }
        }
        /// <summary>协程执行异常时的异常信息</summary>
        public Exception FaultedReason
        {
            get;
            private set;
        }

        /// <summary>开始执行协程</summary>
        public void Run()
        {
            if (State >= EAsyncState.Running)
                throw new InvalidOperationException("Async task has been Running.");
            State = EAsyncState.Running;
            try
            {
                InternalRun();
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }
        protected abstract void InternalRun();
        protected void Complete()
        {
            Progress = COMPLETED;
        }
        /// <summary>协程执行异常</summary>
        public void Error(Exception ex)
        {
            CheckCompleted();
            FaultedReason = ex;
            Complete(EAsyncState.Faulted);
        }
        /// <summary>协程执行取消</summary>
        public void Cancel()
        {
            CheckCompleted();
            Complete(EAsyncState.Canceled);
        }
        protected void CheckCompleted()
        {
            if (IsEnd)
                throw new InvalidOperationException("Async task has been completed.");
        }
        private void Complete(EAsyncState state)
        {
            this.State = state;
            InternalComplete();
        }
        protected virtual void InternalComplete()
        {
        }
        public virtual void Update(float time)
        {
        }
    }
    /// <summary>协程加载数据</summary>
    public class AsyncData<T> : Async
    {
        private bool set;

        public T Data
        {
            get;
            private set;
        }

        public void SetData(T data)
        {
            if (set)
                throw new InvalidOperationException("Async data has been set.");
            OnSetData(ref data);
            Data = data;
            set = true;
            Progress = COMPLETED;
        }
        protected virtual void OnSetData(ref T data)
        {
        }
        protected sealed override void InternalRun()
        {
            Data = default(T);
            set = false;
        }
    }

    /// <summary>协程接口</summary>
    public interface ICoroutine : IUpdatable
    {
        bool IsEnd { get; }
    }
    /// <summary>自定义委托完成协程</summary>
    public class CorDelegate : ICoroutine
    {
        private Func<float, bool> coroutine;
        private bool completed;

        public bool IsEnd
        {
            get { return completed; }
        }
        public CorDelegate(Func<float, bool> coroutine)
        {
            if (coroutine == null)
                throw new ArgumentNullException();
            this.coroutine = coroutine;
        }
        public void Update(float time)
        {
            completed = coroutine(time);
        }
    }
    /// <summary>可迭代协程，一般放入EntryService.SetCoroutine</summary>
    public class COROUTINE : PoolItem, ICoroutine, IDisposable
    {
        public struct CorSingleEnumerator : IEnumerator<ICoroutine>
        {
            private ICoroutine coroutine;
            private bool moved;
            public ICoroutine Current
            {
                get { return moved ? coroutine : null; }
            }
            object IEnumerator.Current
            {
                get { return this.Current; }
            }
            public CorSingleEnumerator(ICoroutine coroutine)
            {
                this.coroutine = coroutine;
                this.moved = false;
            }
            public void Dispose()
            {
                this.coroutine = null;
            }
            public bool MoveNext()
            {
                if (moved)
                    return false;
                this.moved = true;
                return true;
            }
            public void Reset()
            {
                this.moved = false;
            }
        }

        private bool last;
        private IEnumerator<ICoroutine> coroutine;
        private ICoroutine current;

        public bool IsEnd
        {
            get { return coroutine == null; }
        }

        public COROUTINE() { }
        public COROUTINE(ICoroutine coroutine)
        {
            Set(coroutine);
        }
        public COROUTINE(IEnumerator<ICoroutine> coroutine)
        {
            Set(coroutine);
        }
        public COROUTINE(IEnumerable<ICoroutine> coroutine)
        {
            Set(coroutine);
        }

        public void Set(ICoroutine coroutine)
        {
            if (coroutine == null)
                throw new ArgumentNullException("coroutine");
            InternalSet(new CorSingleEnumerator(coroutine));
        }
        public void Set(IEnumerator<ICoroutine> coroutine)
        {
            if (coroutine == null)
                throw new ArgumentNullException("coroutine");
            InternalSet(coroutine);
        }
        public void Set(IEnumerable<ICoroutine> coroutine)
        {
            if (coroutine == null)
                throw new ArgumentNullException("coroutine");
            var c = coroutine.GetEnumerator();
            if (c == null)
                throw new ArgumentNullException("coroutine");
            InternalSet(c);
        }
        private void InternalSet(IEnumerator<ICoroutine> coroutine)
        {
            this.coroutine = coroutine;
            this.current = null;
            this.last = false;
        }

        public void Update(float time)
        {
            if (current == null)
            {
                if (coroutine == null)
                    return;
                last = !coroutine.MoveNext();
                // MoveNext may dispose this
                if (coroutine == null)
                    current = null;
                else
                    current = coroutine.Current;
            }

            if (last)
            {
                Dispose();
            }
            else
            {
                if (current != null)
                {
                    current.Update(time);
                    if (current != null && current.IsEnd)
                    {
                        current = null;
                    }
                }
            }
        }
        public void Dispose()
        {
            current = null;
            coroutine = null;
        }
    }
    /// <summary>队列协程，按照队列顺序执行协程，整个队列执行完毕时协程完毕</summary>
    public class CorQueue : ICoroutine
    {
        private Queue<COROUTINE> coroutines = new Queue<COROUTINE>();

        public bool IsEnd
        {
            get { return coroutines.Count == 0; }
        }

        public CorQueue()
        {
        }
        public CorQueue(IEnumerable<ICoroutine> current)
        {
            Add(current);
        }
        public CorQueue(IEnumerator<ICoroutine> current)
        {
            Add(current);
        }
        public CorQueue(params IEnumerable<ICoroutine>[] coroutines)
        {
            for (int i = 0; i < coroutines.Length; i++)
                Add(coroutines[i]);
        }
        public CorQueue(params IEnumerator<ICoroutine>[] coroutines)
        {
            for (int i = 0; i < coroutines.Length; i++)
                Add(coroutines[i]);
        }

        public void Add(COROUTINE coroutine)
        {
            coroutines.Enqueue(coroutine);
        }
        public COROUTINE Add(IEnumerator<ICoroutine> coroutine)
        {
            var add = new COROUTINE(coroutine);
            Add(add);
            return add;
        }
        public COROUTINE Add(IEnumerable<ICoroutine> coroutine)
        {
            var add = new COROUTINE(coroutine);
            Add(add);
            return add;
        }
        public void Update(float time)
        {
            if (coroutines.Count > 0)
            {
                coroutines.Peek().Update(time);
                if (coroutines.Peek().IsEnd)
                    coroutines.Dequeue();
            }
        }
        public void CancelCurrent()
        {
            if (coroutines.Count > 0)
                coroutines.Dequeue().Dispose();
        }
        public void Clear()
        {
            while (coroutines.Count > 0)
                coroutines.Dequeue().Dispose();
        }
    }
    /// <summary>并行协程，并行执行全部协程，全部协程执行完毕时协程完毕</summary>
    public class CorParallel : ICoroutine
    {
        private List<COROUTINE> coroutines = new List<COROUTINE>();

        public bool IsEnd
        {
            get { return coroutines.Count == 0; }
        }

        public CorParallel(){}
        public CorParallel(IEnumerable<ICoroutine> current)
        {
            Add(current);
        }
        public CorParallel(IEnumerator<ICoroutine> current)
        {
            Add(current);
        }
        public CorParallel(params IEnumerable<ICoroutine>[] coroutines)
        {
            for (int i = 0; i < coroutines.Length; i++)
                Add(coroutines[i]);
        }
        public CorParallel(params IEnumerator<ICoroutine>[] coroutines)
        {
            for (int i = 0; i < coroutines.Length; i++)
                Add(coroutines[i]);
        }

        public void Add(COROUTINE coroutine)
        {
            coroutines.Add(coroutine);
        }
        public COROUTINE Add(IEnumerator<ICoroutine> coroutine)
        {
            var add = new COROUTINE(coroutine);
            Add(add);
            return add;
        }
        public COROUTINE Add(IEnumerable<ICoroutine> coroutine)
        {
            var add = new COROUTINE(coroutine);
            Add(add);
            return add;
        }
        public void Update(float time)
        {
            if (coroutines.Count > 0)
            {
                for (int i = coroutines.Count - 1; i >= 0; i--)
                {
                    coroutines[i].Update(time);
                    if (coroutines[i].IsEnd)
                        coroutines.RemoveAt(i);
                }
            }
        }
        public void Clear()
        {
            coroutines.Clear();
        }
    }
    /// <summary>可迭代协程，迭代返回指定类型的数据</summary>
    public class CorEnumerator<T> : PoolItem, ICoroutine, IDisposable
    {
        private IEnumerator<T> coroutine;

        public bool IsEnd
        {
            get { return coroutine == null; }
        }

        public CorEnumerator(T item)
            : this(StartCoroutine(item))
        {
        }
        public CorEnumerator(IEnumerable<T> enumerable)
            : this(enumerable.GetEnumerator())
        {
        }
        public CorEnumerator(IEnumerator<T> enumerator)
        {
            if (enumerator == null)
                throw new ArgumentNullException("enumerator");
            this.coroutine = enumerator;
        }

        /// <summary>执行下一项</summary>
        /// <param name="item">当前项</param>
        /// <returns>是否结束</returns>
        public bool Update(out T item)
        {
            if (IsEnd)
            {
                item = default(T);
                return true;
            }
            bool last = !coroutine.MoveNext();
            item = coroutine.Current;
            if (last)
                Dispose();
            return last;
        }
        public void Update(float time)
        {
            T item;
            Update(out item);
        }
        public void Dispose()
        {
            coroutine = null;
        }
        public static IEnumerator<T> StartCoroutine(T coroutine)
        {
            yield return coroutine;
        }
    }
    /// <summary>等待时间协程</summary>
    public struct TIME : ICoroutine, IUpdatable
    {
        /// <summary>一秒钟</summary>
        public static TIME Second
        {
            get { return new TIME(1000); }
        }
        /// <summary>当前经过的时间，单位毫秒</summary>
        public float Current;
        /// <summary>需要等待的时间，单位毫秒</summary>
        public int Interval;
        public bool IsEnd
        {
            get { return Current >= Interval; }
        }
        public TIME(int interval)
        {
            this.Current = 0;
            this.Interval = _MATH.Nature(interval);
        }
        public void Update(float elapsed)
        {
            Current += elapsed * 1000;
        }
        public void Update(GameTime time)
        {
            Current += time.Elapsed;
        }
        /// <summary>重新开始计时</summary>
        public void Reset()
        {
            Current = 0;
        }
        /// <summary>当前经过时间减去等待时间，开始下一轮的计时</summary>
        public void NextTurn()
        {
            Current -= Interval;
        }
        /// <summary>让当前经过时间等于需要等待的时间以结束协程</summary>
        public void TimeOut()
        {
            Current = Interval;
        }
    }
    /// <summary>等待钟表协程</summary>
    public struct CLOCK : ICoroutine
    {
        public float Elapsed;
        /// <summary>起止时间</summary>
        public Range<float> Duration;
        /// <summary>时钟嘀嗒（取值-n仅一次:0每次:n间隔)，单位毫秒</summary>
        public int TickTime;

        public bool IsEnd
        {
            get
            {
                if (Duration.Max > Duration.Min)
                    return Elapsed >= Duration.Max;
                if (TickTime < 0 && Elapsed - Duration.Min >= -TickTime)
                    return true;
                return false;
            }
        }

        /// <summary>经过时间</summary>
        /// <param name="time">单位毫秒</param>
        /// <returns>是否滴答一次</returns>
        public bool Tick(float time)
        {
            // 持续时间已过
            if (Duration.Max > Duration.Min && Elapsed >= Duration.Max)
                return false;
            float previous = Elapsed;
            Elapsed += time;
            if (TickTime == 0)
                return Elapsed >= Duration.Min;

            if (Elapsed < Duration.Min)
                return false;

            previous -= Duration.Min;
            float current = Elapsed - Duration.Min;
            if (TickTime < 0)
            {
                int tick = -TickTime;
                if (current > tick)
                    return false;
                else
                    return previous < tick && current >= tick;
            }
            else
                return (int)(previous / TickTime) != (int)(current / TickTime);
        }
        void IUpdatable.Update(float time)
        {
            Tick(time);
        }
    }

    
    // 时间线
    /// <summary>每帧可更新的内容</summary>
    public interface IUpdatable
    {
        /// <summary>更新</summary>
        /// <param name="elapsed">一帧经过的时间，单位自定义，一般为秒</param>
        void Update(float elapsed);
    }
    /// <summary>时间线</summary>
    public class Timeline<T> : ICoroutine, IEnumerable<KeyValuePair<float, T>>
    {
        private class TimeKeyFrame
        {
            public float Time;
            public KeyFrame<T> KeyFrame;
        }

        /// <summary>是否循环</summary>
        public bool Loop;
        private float _elapsed;
        /// <summary>开始时间，单位秒</summary>
        public float StartTime;
        /// <summary>结束时间，单位秒</summary>
        public float EndTime;
        private LinkedList<TimeKeyFrame> keyFrames = new LinkedList<TimeKeyFrame>();
        private LinkedListNode<TimeKeyFrame> current;
        private Action<T> set;

        /// <summary>当前经过时间，单位秒</summary>
        public float Elapsed
        {
            get { return _elapsed; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Key frame time must bigger than 0.");
                _elapsed = value;
                if (keyFrames.Count == 0)
                    return;
                // 计算当前的关键帧
                current = keyFrames.First;
                while (current != null)
                {
                    if (value < current.Value.Time)
                        return;
                    current = current.Next;
                }
                Elapse();
            }
        }
        /// <summary>结束时间，单位秒</summary>
        public float OverTime
        {
            get
            {
                if (EndTime > StartTime)
                    return EndTime;

                if (keyFrames.Count == 0)
                    return 0;

                return keyFrames.Last.Value.Time + StartTime;
            }
        }
        public bool IsEnd
        {
            get { return _elapsed >= OverTime; }
        }
        /// <summary>获取时间点上的关键帧</summary>
        public T this[float time]
        {
            get
            {
                var node = keyFrames.FirstOrDefault(k => k.Time == time);
                if (node == null)
                    throw new KeyNotFoundException();
                return node.KeyFrame.Value;
            }
            set
            {
                var node = keyFrames.FirstOrDefault(k => k.Time == time);
                if (node != null)
                    node.KeyFrame.Value = value;
            }
        }
        /// <summary>关键帧</summary>
        public KeyValuePair<float, T>[] Keys
        {
            get
            {
                KeyValuePair<float, T>[] keys = new KeyValuePair<float, T>[keyFrames.Count];
                int index = 0;
                var node = keyFrames.First;
                while (node != null)
                {
                    keys[index] = new KeyValuePair<float, T>(node.Value.Time, node.Value.KeyFrame.Value);
                    node = node.Next;
                }
                return keys;
            }
        }

        /// <summary>一条时间线只关注一个属性值</summary>
        /// <param name="set">属性值的赋值方法</param>
        public Timeline(Action<T> set)
        {
            if (set == null)
                throw new ArgumentNullException("Setter");
            this.set = set;
        }

        /// <summary>添加一个固定帧，瞬间切换到这个帧的值</summary>
        public void AddFixedFrame(float time, T value)
        {
            AddKeyFrame(time, new KFFixed<T>() { Value = value });
        }
        /// <summary>添加一个补帧，数字类型可以补帧，非数字类型会变成一个固定帧</summary>
        public void AddComplementFrame(float time, T value)
        {
            Type type = typeof(T);
            KeyFrame<T> key;
            if (type == typeof(byte)) key = new KFByte() as KeyFrame<T>;
            else if (type == typeof(sbyte)) key = new KFSByte() as KeyFrame<T>;
            else if (type == typeof(ushort)) key = new KFUInt16() as KeyFrame<T>;
            else if (type == typeof(short)) key = new KFInt16() as KeyFrame<T>;
            else if (type == typeof(uint)) key = new KFUInt32() as KeyFrame<T>;
            else if (type == typeof(int)) key = new KFInt32() as KeyFrame<T>;
            else if (type == typeof(float)) key = new KFSingle() as KeyFrame<T>;
            else if (type == typeof(double)) key = new KFDouble() as KeyFrame<T>;
            else if (type == typeof(ulong)) key = new KFUInt64() as KeyFrame<T>;
            else if (type == typeof(long)) key = new KFInt64() as KeyFrame<T>;
            else if (type == typeof(TimeSpan)) key = new KFTimeSpan() as KeyFrame<T>;
            else if (type == typeof(DateTime)) key = new KFDateTime() as KeyFrame<T>;
            else
            {
                AddFixedFrame(time, value);
                return;
            }
            key.Value = value;
            AddKeyFrame(time, key);
        }
        /// <summary>自定义脚本关键帧</summary>
        /// <param name="script">时间，前一帧对象，后一帧对象，Return当前帧的值</param>
        public void AddScriptFrame(float time, T value, Func<float, T, T, T> script)
        {
            if (script == null)
                throw new ArgumentNullException("script");
            AddKeyFrame(time, new KFScript<T>() { Value = value, Script = script });
        }
        private void AddKeyFrame(float time, KeyFrame<T> frame)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException("Key frame time must bigger than 0.");

            TimeKeyFrame key = new TimeKeyFrame();
            key.Time = time;
            key.KeyFrame = frame;
            if (keyFrames.Count == 0)
            {
                keyFrames.AddFirst(key);
            }
            else
            {
                var node = keyFrames.First;
                while (node != null)
                {
                    // 插入
                    if (node.Value.Time > time)
                    {
                        keyFrames.AddBefore(node, key);
                        return;
                    }
                    // 替换
                    if (node.Value.Time == time)
                    {
                        node.Value.KeyFrame = frame;
                        return;
                    }
                    node = node.Next;
                }
                keyFrames.AddLast(key);
            }
        }
        /// <summary>清除所有关键帧，重置时间</summary>
        public void Clear()
        {
            keyFrames.Clear();
            _elapsed = 0;
            current = null;
        }
        /// <summary>移除一个关键帧</summary>
        public bool RemoveKeyFrame(float time)
        {
            var node = keyFrames.FirstOrDefault(k => k.Time == time);
            if (node != null)
                return keyFrames.Remove(node);
            return false;
        }
        /// <summary>整体调整时间线时间</summary>
        public void Expand(float multiple)
        {
            if (multiple <= 0)
                throw new ArgumentOutOfRangeException();
            foreach (var item in keyFrames)
                item.Time *= multiple;
        }
        private void Elapse()
        {
            if (keyFrames.Count == 0)
                return;

            if (current == null)
            {
                current = keyFrames.First;
                //set(current.Value.KeyFrame.Update(0, current.Next == null ? null : current.Next.Value.KeyFrame));
            }

            //if (EndTime > StartTime && _elapsed > EndTime)
            //{
            //    if (Loop)
            //    {
            //        current = keyFrames.First;
            //        _elapsed = 0;
            //        set(current.Value.KeyFrame.Update(0, current.Next == null ? null : current.Next.Value.KeyFrame));
            //    }
            //    else
            //        return;
            //}

            if (_elapsed >= StartTime)
            {
                float time = _elapsed - StartTime;
                // 切换到下一关键帧
                while (current.Next != null && time >= current.Next.Value.Time)
                    current = current.Next;
                // 更新当前帧
                if (time >= current.Value.Time)
                {
                    var next = current.Next;
                    if (next == null)
                    {
                        var temp = current;
                        float et = OverTime;
                        if (et == 0)
                            time = 1;
                        else
                        {
                            if (time > et)
                            {
                                time = 1;
                                if (Loop)
                                {
                                    _elapsed %= et;
                                    // 下一帧起从头开始
                                    current = null;
                                }
                            }
                            else
                            {
                                time = time / et;
                            }
                        }
                        set(temp.Value.KeyFrame.Update(time, null));
                    }
                    else
                    {
                        time = (time - current.Value.Time) / (next.Value.Time - current.Value.Time);
                        // 时间：相对于当前帧开始时间到当前帧结束时间的百分比
                        set(current.Value.KeyFrame.Update(time, next.Value.KeyFrame));
                    }
                }
            }
        }
        public void Update(GameTime time)
        {
            _elapsed += time.ElapsedSecond;
            Elapse();
        }
        /// <summary>时间线经过时间，单位秒</summary>
        public void Update(float elapsed)
        {
            _elapsed += elapsed;
            Elapse();
        }
        public IEnumerator<KeyValuePair<float, T>> GetEnumerator()
        {
            var node = keyFrames.First;
            while (node != null)
            {
                yield return new KeyValuePair<float, T>(node.Value.Time, node.Value.KeyFrame.Value);
                node = node.Next;
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    internal abstract class KeyFrame<T>
    {
        public T Value;

        protected internal abstract T Update(float elapsedPercent, KeyFrame<T> next);
    }
    internal class KFScript<T> : KeyFrame<T>
    {
        public Func<float, T, T, T> Script;

        protected internal override T Update(float elapsedPercent, KeyFrame<T> next)
        {
            return Script(elapsedPercent, Value, next == null ? default(T) : next.Value);
        }
    }
    internal class KFFixed<T> : KeyFrame<T>
    {
        protected internal override T Update(float elapsedPercent, KeyFrame<T> next)
        {
            return Value;
        }
    }
    internal class KFByte : KeyFrame<byte>
    {
        protected internal override byte Update(float elapsedPercent, KeyFrame<byte> next)
        {
            if (next == null)
                return Value;
            return (byte)(Value + (byte)(elapsedPercent * (next.Value - Value)));
        }
    }
    internal class KFSByte : KeyFrame<sbyte>
    {
        protected internal override sbyte Update(float elapsedPercent, KeyFrame<sbyte> next)
        {
            if (next == null)
                return Value;
            return (sbyte)(Value + (sbyte)(elapsedPercent * (next.Value - Value)));
        }
    }
    internal class KFUInt16 : KeyFrame<ushort>
    {
        protected internal override ushort Update(float elapsedPercent, KeyFrame<ushort> next)
        {
            if (next == null)
                return Value;
            return (ushort)(Value + (ushort)(elapsedPercent * (next.Value - Value)));
        }
    }
    internal class KFInt16 : KeyFrame<short>
    {
        protected internal override short Update(float elapsedPercent, KeyFrame<short> next)
        {
            if (next == null)
                return Value;
            return (short)(Value + (short)(elapsedPercent * (next.Value - Value)));
        }
    }
    internal class KFUInt32 : KeyFrame<uint>
    {
        protected internal override uint Update(float elapsedPercent, KeyFrame<uint> next)
        {
            if (next == null)
                return Value;
            return Value + (uint)(elapsedPercent * (next.Value - Value));
        }
    }
    internal class KFInt32 : KeyFrame<int>
    {
        protected internal override int Update(float elapsedPercent, KeyFrame<int> next)
        {
            if (next == null)
                return Value;
            return Value + (int)(elapsedPercent * (next.Value - Value));
        }
    }
    internal class KFSingle : KeyFrame<float>
    {
        protected internal override float Update(float elapsedPercent, KeyFrame<float> next)
        {
            if (next == null)
                return Value;
            return Value + elapsedPercent * (next.Value - Value);
        }
    }
    internal class KFDouble : KeyFrame<double>
    {
        protected internal override double Update(float elapsedPercent, KeyFrame<double> next)
        {
            if (next == null)
                return Value;
            return Value + elapsedPercent * (next.Value - Value);
        }
    }
    internal class KFUInt64 : KeyFrame<ulong>
    {
        protected internal override ulong Update(float elapsedPercent, KeyFrame<ulong> next)
        {
            if (next == null)
                return Value;
            return Value + (ulong)(elapsedPercent * (next.Value - Value));
        }
    }
    internal class KFInt64 : KeyFrame<long>
    {
        protected internal override long Update(float elapsedPercent, KeyFrame<long> next)
        {
            if (next == null)
                return Value;
            return Value + (long)(elapsedPercent * (next.Value - Value));
        }
    }
    internal class KFTimeSpan : KeyFrame<TimeSpan>
    {
        protected internal override TimeSpan Update(float elapsedPercent, KeyFrame<TimeSpan> next)
        {
            if (next == null)
                return Value;
            return new TimeSpan(Value.Ticks + (long)(elapsedPercent * (next.Value.Ticks - Value.Ticks)));
        }
    }
    internal class KFDateTime : KeyFrame<DateTime>
    {
        protected internal override DateTime Update(float elapsedPercent, KeyFrame<DateTime> next)
        {
            if (next == null)
                return Value;
            return new DateTime(Value.Ticks + (long)(elapsedPercent * (next.Value.Ticks - Value.Ticks)));
        }
    }

    // 时间结构
    /// <summary>计时器</summary>
    public struct TIMER
    {
        private static long Tick
        {
            get { return DateTime.UtcNow.Ticks; }
        }
        public static TIMER StartNew()
        {
            TIMER counter = new TIMER();
            counter.Start();
            return counter;
        }
        private long startTick;
        private bool running;
        private TimeSpan elapsed;
        public bool Running
        {
            get { return running; }
        }
        public TimeSpan Elapsed
        {
            get { return elapsed; }
        }
        public TimeSpan ElapsedNow
        {
            get { return new TimeSpan(Tick - startTick); }
        }
        public void Start()
        {
            running = true;
            startTick = Tick;
            elapsed = TimeSpan.Zero;
        }
        public TimeSpan Stop()
        {
            if (!running)
                return elapsed;

            elapsed = ElapsedNow;
            running = false;
            return elapsed;
        }
        public TimeSpan StopAndStart()
        {
            TimeSpan elapsed = Stop();
            Start();
            return elapsed;
        }
    }
    /// <summary>周计时器</summary>
    public struct WeekDuration
    {
        /// <summary>周几，-1可以无论周几</summary>
        public DayOfWeek Week;
        /// <summary>一天内的时间短</summary>
        public Duration[] Duration;

        /// <summary>是否满足天</summary>
        public bool IsDay(DayOfWeek day)
        {
            return Week < 0 || day == Week;
        }
        /// <summary>是否满足天和时间</summary>
        public bool IsTime(DateTime time)
        {
            if (Duration == null)
                return true;
            if (IsDay(time.DayOfWeek))
            {
                var now = time.TimeOfDay;
                foreach (var during in Duration)
                {
                    if (now > during.Start && now < during.End)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>没到时间前，距离开始时间的倒计时</summary>
        public TimeSpan? BeginCountdown(DateTime time)
        {
            if (Duration != null && IsDay(time.DayOfWeek))
            {
                var now = time.TimeOfDay;
                foreach (var during in Duration)
                {
                    if (now > during.Start)
                    {
                        if (now < during.End)
                            break;
                        else
                            continue;
                    }
                    return during.Start - now;
                }
            }
            return null;
        }
        /// <summary>已到时间后，距离结束时间的倒计时</summary>
        public TimeSpan? EndCountdown(DateTime time)
        {
            if (Duration != null && IsDay(time.DayOfWeek))
            {
                var now = time.TimeOfDay;
                for (int i = Duration.Length - 1; i >= 0; i--)
                {
                    var during = Duration[i];
                    if (now > during.Start)
                    {
                        if (now < during.End)
                            return during.End - now;
                        else
                            continue;
                    }
                }
            }
            return null;
        }
    }
    /// <summary>日计时器</summary>
    public struct Duration
    {
        public TimeSpan Start;
        public TimeSpan End;

        public TimeSpan Time
        {
            get { return End - Start; }
        }
    }

    /// <summary>空数组和单个元素的数组单例</summary>
    public static class _SARRAY<T>
    {
        public static readonly T[] Empty = new T[0];
        public static readonly T[] Single = new T[1];
        public static T[] GetSingleArray(T value)
        {
            Single[0] = value;
            return Single;
        }
    }
    /// <summary>对象单例</summary>
    public static class _S<T> where T : class, new()
    {
        private static T value;
        public static T Value
        {
            get
            {
                if (value == null)
                    value = new T();
                return value;
            }
        }
    }

    public class _Tuple<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;
        public _Tuple() { }
        public _Tuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }
    }
    public class _Tuple<T1, T2, T3> : _Tuple<T1, T2>
    {
        public T3 Item3;
        public _Tuple() { }
        public _Tuple(T1 item1, T2 item2, T3 item3)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
        }
    }
    public class _Tuple<T1, T2, T3, T4> : _Tuple<T1, T2, T3>
    {
        public T4 Item4;
        public _Tuple() { }
        public _Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
        }
    }
}
