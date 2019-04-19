using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EntryEngine
{
    // 入口
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
                    try
                    {
                        coroutine.Update(gameTime);
                    }
                    catch (Exception ex)
                    {
                        _LOG.Error(ex, "\r\ncoroutine error!");
                        coroutine.Dispose();
                    }
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
				newCoroutine.PoolIndex = coroutines.Add(newCoroutine);
				return newCoroutine;
			}
		}
		public COROUTINE SetCoroutine(IEnumerable<ICoroutine> coroutine)
		{
			lock (coroutines)
			{
				COROUTINE newCoroutine = new COROUTINE(coroutine);
				newCoroutine.PoolIndex = coroutines.Add(newCoroutine);
				return newCoroutine;
			}
		}
		public COROUTINE SetCoroutine(ICoroutine coroutine)
		{
			lock (coroutines)
			{
				COROUTINE newCoroutine = new COROUTINE(coroutine);
				newCoroutine.PoolIndex = coroutines.Add(newCoroutine);
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
		public float Elapsed
		{
			get;
			internal set;
		}
        /// <summary>经过的秒数</summary>
        public float ElapsedSecond
        {
            get;
            internal set;
        }
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

        public void Elapse()
		{
            FrameID++;

            PreviousFrame = CurrentFrame;
			DateTime now = DateTime.Now;
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
	public class PoolItem
	{
		internal int PoolIndex;
	}
    public delegate void ActionRef<T>(ref T action);
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
				item.PoolIndex = index;
			return index;
		}
		public T Allot()
		{
			T item;
			Allot(out item);
			return item;
		}
        public T AllotOrCreate()
        {
            int index;
            return AllotOrCreate(out index);
        }
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
        public void RemoveAt<U>(U target) where U : PoolItem, T
        {
            RemoveAt(target.PoolIndex);
        }
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
    public abstract class Tree<T> : IEnumerable<T> where T : Tree<T>
    {
        protected List<T> Childs
        {
            get;
            private set;
        }
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
        public T Parent
        {
            get;
            private set;
        }
        public T First
        {
            get { return Childs.First(); }
        }
        public T Last
        {
            get { return Childs.Last(); }
        }
        public T this[int index]
        {
            get { return Childs[index]; }
        }
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
        public bool Add(T node)
        {
            return Insert(node, Childs.Count);
        }
        public void AddRange(IEnumerable<T> nodes)
        {
            foreach (var node in nodes)
                Add(node);
        }
        public void ForeachParentPriority(Func<T, bool> skip, Action<T> func)
        {
            ForParentPriority((T)this, skip, func);
        }
        public void ForeachChildPriority(Func<T, bool> skip, Action<T> func)
        {
            ForChildPriority((T)this, skip, func);
        }
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
        public void Clear()
        {
            T[] item = this.ToArray();
            for (int i = Childs.Count - 1; i >= 0; i--)
                Remove(i);
        }
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
            return Childs.GetEnumerator();
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
        /// <summary>
        /// For：Childs里的所有元素
        /// </summary>
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
        /// <summary>
        /// Foreach：可以重写GetEnumerator影响此结果
        /// </summary>
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


    // 协程
    public enum EAsyncState
    {
        Created,
        Running,
        Success,
        Canceled,
        Faulted,
    }
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
        public EAsyncState State
        {
            get;
            private set;
        }
        public bool IsEnd
        {
            get { return State > EAsyncState.Running; }
        }
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
        public float ProgressFloat
        {
            get { return 1.0f * progress * _MATH.DIVIDE_BY_1[COMPLETED]; }
            set { Progress = (byte)(value * COMPLETED); }
        }
        public Exception FaultedReason
        {
            get;
            private set;
        }

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
        public void Error(Exception ex)
        {
            CheckCompleted();
            FaultedReason = ex;
            Complete(EAsyncState.Faulted);
        }
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
        public virtual void Update(GameTime time)
        {
        }
    }
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

    public interface ICoroutine
    {
        bool IsEnd { get; }
        void Update(GameTime time);
    }
    /// <summary>自定义委托完成协程</summary>
    public class CorDelegate : ICoroutine
    {
        private Func<GameTime, bool> coroutine;
        private bool completed;

        public bool IsEnd
        {
            get { return completed; }
        }
        public CorDelegate(Func<GameTime, bool> coroutine)
        {
            if (coroutine == null)
                throw new ArgumentNullException();
            this.coroutine = coroutine;
        }
        public void Update(GameTime time)
        {
            completed = coroutine(time);
        }
    }
    public class COROUTINE : PoolItem, ICoroutine, IDisposable
    {
        private struct CorSingleEnumerator : IEnumerator<ICoroutine>
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

        public COROUTINE(ICoroutine coroutine)
        {
            if (coroutine == null)
                throw new ArgumentNullException("coroutine");
            this.coroutine = new CorSingleEnumerator(coroutine);
        }
        public COROUTINE(IEnumerator<ICoroutine> coroutine)
        {
            if (coroutine == null)
                throw new ArgumentNullException("coroutine");
            this.coroutine = coroutine;
        }
        public COROUTINE(IEnumerable<ICoroutine> coroutine)
        {
            if (coroutine == null)
                throw new ArgumentNullException("coroutine");
            this.coroutine = coroutine.GetEnumerator();
            if (this.coroutine == null)
                throw new ArgumentNullException("coroutine");
        }

        void ICoroutine.Update(GameTime time)
        {
            Update(time);
        }
        internal void Update(GameTime time)
        {
            if (current == null)
            {
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
            AddQueue(current);
        }
        public CorQueue(IEnumerator<ICoroutine> current)
        {
            AddQueue(current);
        }
        public CorQueue(params IEnumerable<ICoroutine>[] coroutines)
        {
            for (int i = 0; i < coroutines.Length; i++)
                AddQueue(coroutines[i]);
        }
        public CorQueue(params IEnumerator<ICoroutine>[] coroutines)
        {
            for (int i = 0; i < coroutines.Length; i++)
                AddQueue(coroutines[i]);
        }

        public void AddQueue(IEnumerator<ICoroutine> coroutine)
        {
            coroutines.Enqueue(new COROUTINE(coroutine));
        }
        public void AddQueue(IEnumerable<ICoroutine> coroutine)
        {
            coroutines.Enqueue(new COROUTINE(coroutine));
        }
        public void Update(GameTime time)
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
        void ICoroutine.Update(GameTime time)
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
    public struct TIME : ICoroutine, IUpdatable
    {
        public static TIME Second
        {
            get { return new TIME(1000); }
        }
        public float Current;
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
            Current += elapsed;
        }
        public void Update(GameTime time)
        {
            Current += time.Elapsed;
        }
        public void Reset()
        {
            Current = 0;
        }
        public void NextTurn()
        {
            Current -= Interval;
        }
        public void TimeOut()
        {
            Current = Interval;
        }
    }
    public struct CLOCK : ICoroutine
    {
        public float Elapsed;
        /// <summary>起止时间</summary>
        public Range<float> Duration;
        /// <summary>时钟嘀嗒（取值-n仅一次:0每次:n间隔)</summary>
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
        void ICoroutine.Update(GameTime time)
        {
            Tick(time.Elapsed);
        }
    }

    
    // 时间线
    public interface IUpdatable
    {
        void Update(float elapsed);
    }
    public class Timeline<T> : ICoroutine, IEnumerable<KeyValuePair<float, T>>
    {
        private class TimeKeyFrame
        {
            public float Time;
            public KeyFrame<T> KeyFrame;
        }

        public bool Loop;
        private float _elapsed;
        public float StartTime;
        public float EndTime;
        private LinkedList<TimeKeyFrame> keyFrames = new LinkedList<TimeKeyFrame>();
        private LinkedListNode<TimeKeyFrame> current;
        private Action<T> set;

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

        public Timeline(Action<T> set)
        {
            if (set == null)
                throw new ArgumentNullException("Setter");
            this.set = set;
        }

        public void AddFixedFrame(float time, T value)
        {
            AddKeyFrame(time, new KFFixed<T>() { Value = value });
        }
        public void AddComplementFrame(float time, T value)
        {
            //Type type = Type.GetType("EntryEngine.KF" + typeof(T).Name);
            //if (type == null)
            //{
            //    AddFixedFrame(time, value);
            //}
            //else
            //{
            //    KeyFrame<T> key = (KeyFrame<T>)Activator.CreateInstance(type);
            //    key.Value = value;
            //    AddKeyFrame(time, key);
            //}
            KeyFrame<T> key;
            if (value is byte) key = new KFByte() as KeyFrame<T>;
            else if (value is sbyte) key = new KFSByte() as KeyFrame<T>;
            else if (value is ushort) key = new KFUInt16() as KeyFrame<T>;
            else if (value is short) key = new KFInt16() as KeyFrame<T>;
            else if (value is uint) key = new KFUInt32() as KeyFrame<T>;
            else if (value is int) key = new KFInt32() as KeyFrame<T>;
            else if (value is float) key = new KFSingle() as KeyFrame<T>;
            else if (value is double) key = new KFDouble() as KeyFrame<T>;
            else if (value is ulong) key = new KFUInt64() as KeyFrame<T>;
            else if (value is long) key = new KFInt64() as KeyFrame<T>;
            else if (value is TimeSpan) key = new KFTimeSpan() as KeyFrame<T>;
            else if (value is DateTime) key = new KFDateTime() as KeyFrame<T>;
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
        public void Clear()
        {
            keyFrames.Clear();
            _elapsed = 0;
            current = null;
        }
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
    public struct WeekDuration
    {
        public int Week;
        public Duration[] Duration;

        public bool IsDay(DayOfWeek day)
        {
            return Week < 0 || (int)day == Week;
        }
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
    public struct Duration
    {
        public TimeSpan Start;
        public TimeSpan End;

        public TimeSpan Time
        {
            get { return End - Start; }
        }
    }

    public static class _SARRAY<T>
    {
        public static readonly T[] Empty = new T[0];
        public static T[] Single = new T[1];
        public static T[] GetSingleArray(T value)
        {
            Single[0] = value;
            return Single;
        }
    }
}
