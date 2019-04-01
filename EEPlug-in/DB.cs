using System;
using System.Collections.Generic;
using System.Linq;
using EntryEngine;
using EntryEngine.Serialize;
using EntryEngine.Database;

namespace DB
{
    public class _Table1 : Table1
    {
        public MemoryTable1 MemoryTable { get; internal set; }
        private static int identity_ID = 0;
        internal static int GetIdentityID()
        {
            return identity_ID++;
        }
        public new int ID
        {
            get { return base.ID; }
            set
            {
                if (value == ID) return;
                if (MemoryTable.FindByID(value) != null) throw new InvalidOperationException(string.Format("Modify duplicate ID = {0}.", value));
                base.ID = value;
                identity_ID = Utility.Max(identity_ID, value);
            }
        }
        public new string Name
        {
            get { return base.Name; }
            set
            {
                if (value == Name) return;
                if (MemoryTable.FindByName(value) != null) throw new InvalidOperationException(string.Format("Modify duplicate Name = {0}.", value));
                base.Name = value;
                ForeignnameTable3.SetForeignname(value);
            }
        }
        public new int SID
        {
            get { return base.SID; }
            set
            {
                if (value == SID) return;
                if (MemoryTable.FindBySID(value) != null) throw new InvalidOperationException(string.Format("Modify duplicate SID = {0}.", value));
                base.SID = value;
            }
        }
        public new int Level
        {
            get { return base.Level; }
            set
            {
                if (value == Level) return;
                base.Level = value;
                MemoryTable.ChangeGroupLevel(this, value);
            }
        }
    }
    public class MemoryTable1 : IEnumerable<_Table1>
    {
        public event Action<Table1> OnAdd;
        public event Action<Table1> OnDelete;
        public event Action<Table1> OnTruncate;
        private List<Table1> Table1 = new List<Table1>();
        public int Count { get { return Table1.Count; } }
        
        private Dictionary<int, Table1> Table1ID = new Dictionary<int, Table1>();
        private Dictionary<string, Table1> Table1Name = new Dictionary<string, Table1>();
        private Dictionary<int, Table1> Table1SID = new Dictionary<int, Table1>();
        private Dictionary<int, List<Table1>> Table1Level = new Dictionary<int, List<Table1>>();
        
        public Table1 FindByID(int ID)
        {
            return Table1ID[ID];
        }
        public Table1 FindByName(string Name)
        {
            return Table1Name[Name];
        }
        public Table1 FindBySID(int SID)
        {
            return Table1SID[SID];
        }
        public IEnumerable<Table1> FindByLevel(int Level)
        {
            return Table1Level[Level];
        }
        
        public bool Add(Table1 table)
        {
            Log.Info("Memory Table1 Add {0}", SerializerJson.Log(table));
            bool duplicate = false;
            duplicate = Table1Name.ContainsKey(table.Name);
            if (duplicate)
            {
                Log.Error("Add duplicate Name = {0}", table.Name);
                return false;
            }
            duplicate = Table1SID.ContainsKey(table.SID);
            if (duplicate)
            {
                Log.Error("Add duplicate SID = {0}", table.SID);
                return false;
            }
            Table1.Add(table);
            ((Table1)table).ID = Table1.GetIdentityID();
            Table1ID.Add(table.ID, table);
            Table1Name.Add(table.Name, table);
            Table1SID.Add(table.SID, table);
            List<Table1> temp;
            if (Table1Level.TryGetValue(table.Level, out temp)) temp.Add(table);
            Table1Level.Add(table.Level, new List<Table1>() { table });
            if (OnAdd != null) OnAdd(table);
            return true;
        }
    }
    public class _Table2 : Table2
    {
        public MemoryTable2 MemoryTable { get; internal set; }
        public new string RechargeID
        {
            get { return base.RechargeID; }
            set
            {
                if (value == RechargeID) return;
                if (MemoryTable.FindByRechargeID(value) != null) throw new InvalidOperationException(string.Format("Modify duplicate RechargeID = {0}.", value));
                base.RechargeID = value;
            }
        }
        public new string Name
        {
            get { return base.Name; }
            set
            {
                if (value == Name) return;
                ForeignnameTable3.name = value;
            }
        }
        internal void SetForeignName(string value)
        {
            base.Name = value;
            MemoryTable.ChangeGroupName(this, value);
        }
    }
    public class MemoryTable2 : IEnumerable<_Table2>
    {
        public event Action<Table2> OnAdd;
        public event Action<Table2> OnDelete;
        public event Action<Table2> OnTruncate;
        private List<Table2> Table2 = new List<Table2>();
        public int Count { get { return Table2.Count; } }
        
        private Dictionary<string, Table2> Table2RechargeID = new Dictionary<string, Table2>();
        private Dictionary<string, List<Table2>> Table2Name = new Dictionary<string, List<Table2>>();
        
        public Table2 FindByRechargeID(string RechargeID)
        {
            return Table2RechargeID[RechargeID];
        }
        public IEnumerable<Table2> FindByName(string Name)
        {
            return Table2Name[Name];
        }
        
        public bool Add(Table2 table)
        {
            Log.Info("Memory Table2 Add {0}", SerializerJson.Log(table));
            bool duplicate = false;
            duplicate = Table2RechargeID.ContainsKey(table.RechargeID);
            if (duplicate)
            {
                Log.Error("Add duplicate RechargeID = {0}", table.RechargeID);
                return false;
            }
            Table2.Add(table);
            Table2RechargeID.Add(table.RechargeID, table);
            List<Table2> temp;
            if (Table2Name.TryGetValue(table.Name, out temp)) temp.Add(table);
            Table2Name.Add(table.Name, new List<Table2>() { table });
            if (OnAdd != null) OnAdd(table);
            return true;
        }
    }
    public class _Table3 : Table3
    {
        public MemoryTable3 MemoryTable { get; internal set; }
        public new int ID
        {
            get { return base.ID; }
            set
            {
                if (value == ID) return;
                if (MemoryTable.FindByID(value) != null) throw new InvalidOperationException(string.Format("Modify duplicate ID = {0}.", value));
                base.ID = value;
            }
        }
    }
    public class MemoryTable3 : IEnumerable<_Table3>
    {
        public event Action<Table3> OnAdd;
        public event Action<Table3> OnDelete;
        public event Action<Table3> OnTruncate;
        private List<Table3> Table3 = new List<Table3>();
        public int Count { get { return Table3.Count; } }
        
        private Dictionary<int, Table3> Table3ID = new Dictionary<int, Table3>();
        
        public Table3 FindByID(int ID)
        {
            return Table3ID[ID];
        }
        
        public bool Add(Table3 table)
        {
            Log.Info("Memory Table3 Add {0}", SerializerJson.Log(table));
            bool duplicate = false;
            duplicate = Table3ID.ContainsKey(table.ID);
            if (duplicate)
            {
                Log.Error("Add duplicate ID = {0}", table.ID);
                return false;
            }
            Table3.Add(table);
            Table3ID.Add(table.ID, table);
            if (OnAdd != null) OnAdd(table);
            return true;
        }
    }
    public class DB : MemoryDatabase
    {
        private bool loaded;
        
    }
}
