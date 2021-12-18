using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if SERVER
using EntryEngine.Network;
#endif

namespace EntryEngine.Game
{
    #region 背包系统

    /*
     * 服务器背包系统
     * 1. 将所有道具视为IBagItem
     * 2. 内存中对背包内的道具进行增删改
     * 3. 调用Save得出道具的增删改的结果SQL语句和参数
     * 4. 执行SQL语句将内存中的道具批量同步到数据库
     */
    public interface IBagItem
    {
        int ItemID { get; }
        int Count { get; set; }
    }
    public class BAG<T> : IEnumerable<T> where T : IBagItem
    {
        const int CAPCITY = 128;

        enum EArrange
        {
            Insert,
            Delete,
            Update,
        }
        class ArrangeRecord
        {
            public EArrange Start;
            public EArrange End;
            //public int ItemID;
        }

        private Dictionary<int, T> bag = new Dictionary<int, T>();
        private Dictionary<int, ArrangeRecord> records = new Dictionary<int, ArrangeRecord>();

        public Action<T, List<object>, StringBuilder> BuildInsert;
        public Action<T, List<object>, StringBuilder> BuildUpdate;
        public Action<int, List<object>, StringBuilder> BuildDelete;

        public Dictionary<int, T> Bag
        {
            get { return bag; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("bag");
                this.bag = value;
            }
        }
        public ICollection<T> Items { get { return bag.Values; } }
        public T this[int id] { get { return bag[id]; } }

        public BAG() { }
        public BAG(Action<T, List<object>, StringBuilder> insert,
            Action<T, List<object>, StringBuilder> update,
            Action<int, List<object>, StringBuilder> delete)
        {
            this.BuildInsert = insert;
            this.BuildUpdate = update;
            this.BuildDelete = delete;
        }

        public bool CheckCount(int itemID, int count)
        {
            T item;
            if (bag.TryGetValue(itemID, out item))
                return item.Count >= Math.Abs(count);
            else
                return false;
        }
        public bool CheckCount(int itemID, int count, out T item)
        {
            if (bag.TryGetValue(itemID, out item))
                return item.Count >= Math.Abs(count);
            else
                return false;
        }
        public T GetItem(int itemID)
        {
            T item;
            bag.TryGetValue(itemID, out item);
            return item;
        }
        public int GetItemCount(int itemID)
        {
            T item;
            if (bag.TryGetValue(itemID, out item))
                return item.Count;
            else
                return 0;
        }
        public bool Add(int itemID, int count)
        {
            T item;
            return Add(itemID, count, out item);
        }
        /// <summary>数量变化，正数直接加，负数检查数量后减</summary>
        public bool Add(int itemID, int count, out T item)
        {
            if (count == 0)
            {
                item = default(T);
                return false;
            }

            if (bag.TryGetValue(itemID, out item))
            {
                if (count < 0 && item.Count < -count)
                {
                    return false;
                }
                InternalUpdate(item, item.Count + count);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>数量添加，没有则加入背包</summary>
        /// <returns>是否新增或删除</returns>
        public bool Insert(T item)
        {
            T old;
            if (bag.TryGetValue(item.ItemID, out old))
            {
                // 修改数量
                return InternalUpdate(old, old.Count + item.Count);
            }
            else
            {
                // 添加道具
                bag.Add(item.ItemID, item);
                Build(EArrange.Insert, item);
                return true;
            }
        }
        /// <summary>数量变成</summary>
        public bool Update(int itemID, int count)
        {
            T item;
            if (bag.TryGetValue(itemID, out item))
            {
                InternalUpdate(item, count);
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool Update(T item)
        {
            if (item == null) return false;
            T temp;
            if (!bag.TryGetValue(item.ItemID, out temp)) return false;
            if (item.Count <= 0)
            {
                bag.Remove(item.ItemID);
                Build(EArrange.Delete, item);
            }
            else
            {
                Build(EArrange.Update, item);
            }
            return true;
        }
        private bool InternalUpdate(T item, int count)
        {
            int temp = item.Count;
            item.Count = count;
            // 可能堆叠数上限之类的让道具没有添加成功
            if (temp == item.Count)
            {
                return false;
            }
            if (item.Count <= 0)
            {
                bag.Remove(item.ItemID);
                Build(EArrange.Delete, item);
                return true;
            }
            else
            {
                Build(EArrange.Update, item);
                return false;
            }
        }
        public bool Delete(int itemID)
        {
            T item;
            if (bag.TryGetValue(itemID, out item))
            {
                Build(EArrange.Delete, item);
                return true;
            }
            return false;
        }
        private void Build(EArrange mode, T item)
        {
            ArrangeRecord record;
            if (!records.TryGetValue(item.ItemID, out record))
            {
                record = new ArrangeRecord();
                record.Start = mode;
                records.Add(item.ItemID, record);
            }
            //record.ItemID = item.ItemID;
            record.End = mode;
        }
        protected virtual void OnBuildInsert(T item, List<object> args, StringBuilder builder)
        {
            if (BuildInsert != null) BuildInsert(item, args, builder);
        }
        protected virtual void OnBuildUpdate(T item, List<object> args, StringBuilder builder)
        {
            if (BuildInsert != null) BuildUpdate(item, args, builder);
        }
        protected virtual void OnBuildDelete(int itemID, List<object> args, StringBuilder builder)
        {
            if (BuildInsert != null) BuildDelete(itemID, args, builder);
        }
        public BAG_PACKAGE Save()
        {
            List<BAG_PACKAGE> list = new List<BAG_PACKAGE>();
            Save(list, -1, new StringBuilder(), new List<object>(CAPCITY));
            return list[0];
        }
        /// <summary>保存数据变动</summary>
        /// <param name="output">SQL数据包，包括语句和参数</param>
        /// <param name="batchLength">分批打包的sql语句长度，超过长度则分批，-1则不分批</param>
        /// <returns>是否有保存数据</returns>
        public bool Save(List<BAG_PACKAGE> output, int batchLength, StringBuilder builder, List<object> args)
        {
            //sql = null;
            //values = null;
            if (records.Count == 0) return false;
            // 整合数据
            // 增 -> ... -> 删除：不做任何操作
            // 增加 -> ... -> !删除：增加
            // 修改 -> ... -> 删除：删除
            // 修改 -> ... -> !删除：修改
            // 删除 -> ... -> 删除：删除
            // 删除 -> ... -> !删除：修改
            foreach (var record in records)
            {
                var arrange = record.Value;
                if (arrange.Start == EArrange.Insert)
                {
                    if (arrange.End == EArrange.Delete)
                    {
                        // 添加了又用完了
                    }
                    else
                    {
                        // 添加
                        BuildInsert(bag[record.Key], args, builder);
                    }
                }
                else
                {
                    if (arrange.End == EArrange.Delete)
                    {
                        // 删除
                        BuildDelete(record.Key, args, builder);
                    }
                    else
                    {
                        // 修改
                        BuildUpdate(bag[record.Key], args, builder);
                    }
                }

                // 字数过多打个包
                if (batchLength > 0 && builder.Length >= batchLength)
                {
                    output.Add(new BAG_PACKAGE(builder.ToString(), args.ToArray()));
                    builder.Remove(0, builder.Length);
                    args.Clear();
                }
            }

            this.records.Clear();

            if (builder.Length > 0)
            {
                output.Add(new BAG_PACKAGE(builder.ToString(), args.ToArray()));
            }
            //sql = builder.ToString();
            //values = args.ToArray();
            return true;
        }
        public bool Save(List<BAG_PACKAGE> output)
        {
            return Save(output, BAG_PACKAGE.BATCH, new StringBuilder(), new List<object>());
        }
        public bool Save(StringBuilder builder, List<object> args)
        {
            List<BAG_PACKAGE> output = new List<BAG_PACKAGE>();
            return Save(output, -1, builder, args);
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in bag)
                yield return item.Value;
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
    public struct BAG_PACKAGE
    {
        public const int BATCH = 1024 * 1024;
        public string SQL;
        public object[] Values;
        public BAG_PACKAGE(string sql, object[] values)
        {
            this.SQL = sql;
            this.Values = values;
        }
    }

    #endregion

    #region 掉落系统

    public interface IDrop
    {
        int ItemID { get; }
        int Count { get; }
        int Weight { get; }
    }
    public struct DROP
    {
        public int ItemID;
        public float Weight;

        public DROP(int itemID, float varyWeight)
        {
            this.ItemID = itemID;
            this.Weight = varyWeight;
        }
    }
    /// <summary>掉落权重变化在_MATH.WeightVary</summary>
    public struct ITEM : IBagItem
    {
        public int ID;
        public int Count;
        public ITEM(int id, int count)
        {
            this.ID = id;
            this.Count = count;
        }
        public override string ToString()
        {
            return string.Format("ID:{0} Count:{1}", ID, Count);
        }
        public override int GetHashCode()
        {
            return ID;
        }
        /// <summary>整合道具到字典中；新增道具或者添加数量</summary>
        public static void Integration(Dictionary<int, int> dic, ITEM item)
        {
            int count;
            if (dic.TryGetValue(item.ID, out count))
            {
                count += item.Count;
                dic[item.ID] = count;
            }
            else
                dic.Add(item.ID, item.Count);
        }
        /// <summary>整合道具到字典中；新增道具或者添加数量</summary>
        public static void Integration(Dictionary<int, int> dic, ITEM[] items)
        {
            if (items == null) return;
            int count;
            for (int i = 0; i < items.Length; i++)
            {
                if (dic.TryGetValue(items[i].ID, out count))
                {
                    count += items[i].Count;
                    dic[items[i].ID] = count;
                }
                else
                    dic.Add(items[i].ID, items[i].Count);
            }
        }
        public static ITEM[] ToArray(Dictionary<int, int> dic)
        {
            ITEM[] array = new ITEM[dic.Count];
            int index = 0;
            foreach (var item in dic)
            {
                array[index].ID = item.Key;
                array[index].Count = item.Value;
                index++;
            }
            return array;
        }

        /// <summary>掉落列表中掉落道具</summary>
        /// <param name="list">掉落列表</param>
        /// <param name="random">随机对象</param>
        /// <param name="weightVary">权重变化量，可通过_MATH.WeightVary计算，权重变化的ID在掉落列表中不存在时，权重变化不起作用</param>
        /// <returns>掉落的道具，ID为0则代表空道具</returns>
        public static IDrop Drop(IEnumerable<IDrop> list, _RANDOM.Random random, DROP[] weightVary)
        {
            double weight = 0;
            foreach (var item in list)
            {
                int id = item.ItemID;
                if (weightVary != null)
                {
                    for (int i = 0; i < weightVary.Length; i++)
                    {
                        if (weightVary[i].ItemID == id)
                        {
                            weight += weightVary[i].Weight;
                            break;
                        }
                    }
                }
                weight += item.Weight;
            }

            if (weight == 0)
                return null;

            double dropPoint = random.Next(weight);
            foreach (var item in list)
            {
                int id = item.ItemID;
                float varyWeight = 0;
                if (weightVary != null)
                {
                    for (int i = 0; i < weightVary.Length; i++)
                    {
                        if (weightVary[i].ItemID == id)
                        {
                            varyWeight = weightVary[i].Weight;
                            break;
                        }
                    }
                }

                if (dropPoint < item.Weight + varyWeight)
                    return item;
                else
                    dropPoint -= item.Weight + varyWeight;
            }
            throw new InvalidOperationException("掉落不能没掉出东西");
        }

        int IBagItem.ItemID
        {
            get { return ID; }
        }
        int IBagItem.Count { get { return this.Count; } set { this.Count = value; } }
    }

    #endregion

#if SERVER


    #region 统计在线人数

    /// <summary>统计时间点的在线人数</summary>
    [MemoryTable]
    public class T_S_ONLINE
    {
        [Index(EIndex.Primary)]
        public DateTime Time;
        public ushort Quarter0;
        public ushort Quarter1;
        public ushort Quarter2;
        public ushort Quarter3;
        public ushort Quarter4;
        public ushort Quarter5;
        public ushort Quarter6;
        public ushort Quarter7;
        public ushort Quarter8;
        public ushort Quarter9;
        public ushort Quarter10;
        public ushort Quarter11;
        public ushort Quarter12;
        public ushort Quarter13;
        public ushort Quarter14;
        public ushort Quarter15;
        public ushort Quarter16;
        public ushort Quarter17;
        public ushort Quarter18;
        public ushort Quarter19;
        public ushort Quarter20;
        public ushort Quarter21;
        public ushort Quarter22;
        public ushort Quarter23;
        public ushort Quarter24;
        public ushort Quarter25;
        public ushort Quarter26;
        public ushort Quarter27;
        public ushort Quarter28;
        public ushort Quarter29;
        public ushort Quarter30;
        public ushort Quarter31;
        public ushort Quarter32;
        public ushort Quarter33;
        public ushort Quarter34;
        public ushort Quarter35;
        public ushort Quarter36;
        public ushort Quarter37;
        public ushort Quarter38;
        public ushort Quarter39;
        public ushort Quarter40;
        public ushort Quarter41;
        public ushort Quarter42;
        public ushort Quarter43;
        public ushort Quarter44;
        public ushort Quarter45;
        public ushort Quarter46;
        public ushort Quarter47;
        public ushort Quarter48;
        public ushort Quarter49;
        public ushort Quarter50;
        public ushort Quarter51;
        public ushort Quarter52;
        public ushort Quarter53;
        public ushort Quarter54;
        public ushort Quarter55;
        public ushort Quarter56;
        public ushort Quarter57;
        public ushort Quarter58;
        public ushort Quarter59;
        public ushort Quarter60;
        public ushort Quarter61;
        public ushort Quarter62;
        public ushort Quarter63;
        public ushort Quarter64;
        public ushort Quarter65;
        public ushort Quarter66;
        public ushort Quarter67;
        public ushort Quarter68;
        public ushort Quarter69;
        public ushort Quarter70;
        public ushort Quarter71;
        public ushort Quarter72;
        public ushort Quarter73;
        public ushort Quarter74;
        public ushort Quarter75;
        public ushort Quarter76;
        public ushort Quarter77;
        public ushort Quarter78;
        public ushort Quarter79;
        public ushort Quarter80;
        public ushort Quarter81;
        public ushort Quarter82;
        public ushort Quarter83;
        public ushort Quarter84;
        public ushort Quarter85;
        public ushort Quarter86;
        public ushort Quarter87;
        public ushort Quarter88;
        public ushort Quarter89;
        public ushort Quarter90;
        public ushort Quarter91;
        public ushort Quarter92;
        public ushort Quarter93;
        public ushort Quarter94;
        public ushort Quarter95;

        /// <summary>
        /// <para>int quarter = T_S_ONLINE.GetQuarter(ref time);</para>
        /// <para>if (quarter == 0) // 新的一天第一次插入数据库</para>
        /// <para>else db.ExecuteNonQuery(string.Format("UPDATE T_S_ONLINE SET Quarter{0} = {1} WHERE Time = @p0", quarter, online), time);</para>
        /// </summary>
        /// <returns>0~95</returns>
        public static int GetQuarter(DateTime time)
        {
            return (int)(time.TimeOfDay.TotalMinutes / 15);
        }
        public static int GetQuarter(ref DateTime time)
        {
            int quarter = (int)(time.TimeOfDay.TotalMinutes / 15);
            time = time.Date + TimeSpan.FromMinutes(quarter * 15);
            return quarter;
        }
    }

    #endregion


#endif
}
