#if SERVER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryEngine.Network
{
    static partial class _DATABASE
    {
        public static Database _Database;
        
        public static bool Available
        {
            get
            {
                return _Database.Available;
            }
        }
        public static string ConnectionString
        {
            get
            {
                return _Database.ConnectionString;
            }
            set
            {
                _Database.ConnectionString = value;
            }
        }
        public static string DatabaseName
        {
            get
            {
                return _Database.DatabaseName;
            }
        }
        public static string DataSource
        {
            get
            {
                return _Database.DataSource;
            }
        }
        public static string ServerVersion
        {
            get
            {
                return _Database.ServerVersion;
            }
        }
        public static void TestConnection()
        {
            _Database.TestConnection();
        }
        public static int ExecuteNonQuery(System.Action<System.Text.StringBuilder, System.Collections.Generic.List<object>> action, bool transaction)
        {
            return _Database.ExecuteNonQuery(action, transaction);
        }
        public static int ExecuteNonQuery(System.Action<System.Text.StringBuilder, System.Collections.Generic.List<object>> action)
        {
            return _Database.ExecuteNonQuery(action);
        }
        public static int ExecuteNonQuery(string sql, params object[] parameters)
        {
            return _Database.ExecuteNonQuery(sql, parameters);
        }
        public static object ExecuteScalar(string sql, params object[] parameters)
        {
            return _Database.ExecuteScalar(sql, parameters);
        }
        public static T ExecuteScalar<T>(string sql, params object[] parameters)
        
        {
            return _Database.ExecuteScalar<T>(sql, parameters);
        }
        public static T SelectValue<T>(string sql, params object[] parameters)
        
        {
            return _Database.SelectValue<T>(sql, parameters);
        }
        public static void ExecuteReader(System.Action<System.Data.IDataReader> read, string sql, params object[] parameters)
        {
            _Database.ExecuteReader(read, sql, parameters);
        }
        public static System.Collections.Generic.List<T> SelectValues<T>(string sql, params object[] parameters)
        
        {
            return _Database.SelectValues<T>(sql, parameters);
        }
        public static T SelectObject<T>(string sql, params object[] parameters)
        
        {
            return _Database.SelectObject<T>(sql, parameters);
        }
        public static System.Collections.Generic.List<T> SelectObjects<T>(string sql, params object[] parameters)
        
        {
            return _Database.SelectObjects<T>(sql, parameters);
        }
        public static void SelectObjects<T>(System.Action<T> newInstance, string sql, params object[] parameters)
        
        {
            _Database.SelectObjects<T>(newInstance, sql, parameters);
        }
        public static System.Collections.Generic.Dictionary<K, V> SelectObjectsGroup<K, V>(System.Func<V, K> getKey, string sql, params object[] parameters)
        
        {
            return _Database.SelectObjectsGroup<K, V>(getKey, sql, parameters);
        }
        public static System.Collections.Generic.Dictionary<K, System.Collections.Generic.List<V>> SelectObjectsGroup2<K, V>(System.Func<V, K> getKey, string sql, params object[] parameters)
        
        {
            return _Database.SelectObjectsGroup2<K, V>(getKey, sql, parameters);
        }
        public static EntryEngine.Network.PagedModel<T> SelectPaged<T>(string sql, int pageIndex, int pageSize, System.Action<System.Data.IDataReader, System.Collections.Generic.List<T>> read, params object[] _params)
        where T : new()
        {
            return _Database.SelectPaged<T>(sql, pageIndex, pageSize, read, _params);
        }
        public static EntryEngine.Network.PagedModel<T> SelectPaged<T>(string sql, int pageIndex, int pageSize, params object[] _params)
        where T : new()
        {
            return _Database.SelectPaged<T>(sql, pageIndex, pageSize, _params);
        }
        public static EntryEngine.Network.PagedModel<T> SelectPaged<T>(string selectCountSQL, string __where, string selectSQL, string conditionAfterWhere, int page, int pageSize, params object[] param)
        where T : new()
        {
            return _Database.SelectPaged<T>(selectCountSQL, __where, selectSQL, conditionAfterWhere, page, pageSize, param);
        }
        public static EntryEngine.Network.PagedModel<T> SelectPaged<T>(string selectCountSQL, string __where, string selectSQL, string conditionAfterWhere, int page, int pageSize, System.Action<System.Data.IDataReader, System.Collections.Generic.List<T>> read, params object[] param)
        
        {
            return _Database.SelectPaged<T>(selectCountSQL, __where, selectSQL, conditionAfterWhere, page, pageSize, read, param);
        }
        public static System.Collections.Generic.List<EntryEngine._Tuple<T1, T2>> SelectJoin<T1, T2>(string sql, int count1, params object[] _params)
        where T1 : new()  where T2 : new()
        {
            return _Database.SelectJoin<T1, T2>(sql, count1, _params);
        }
        public static System.Collections.Generic.List<EntryEngine._Tuple<T1, T2, T3>> SelectJoin<T1, T2, T3>(string sql, int count1, int count2, params object[] _params)
        where T1 : new()  where T2 : new()  where T3 : new()
        {
            return _Database.SelectJoin<T1, T2, T3>(sql, count1, count2, _params);
        }
        public static System.Collections.Generic.List<EntryEngine._Tuple<T1, T2, T3, T4>> SelectJoin<T1, T2, T3, T4>(string sql, int count1, int count2, int count3, params object[] _params)
        where T1 : new()  where T2 : new()  where T3 : new()  where T4 : new()
        {
            return _Database.SelectJoin<T1, T2, T3, T4>(sql, count1, count2, count3, _params);
        }
        public static EntryEngine.Network.PagedModel<EntryEngine._Tuple<T1, T2>> SelectJoinPaged<T1, T2>(int page, int pageSize, string sql, int count1, params object[] _params)
        where T1 : new()  where T2 : new()
        {
            return _Database.SelectJoinPaged<T1, T2>(page, pageSize, sql, count1, _params);
        }
        public static EntryEngine.Network.PagedModel<EntryEngine._Tuple<T1, T2, T3>> SelectJoinPaged<T1, T2, T3>(int page, int pageSize, string sql, int count1, int count2, params object[] _params)
        where T1 : new()  where T2 : new()  where T3 : new()
        {
            return _Database.SelectJoinPaged<T1, T2, T3>(page, pageSize, sql, count1, count2, _params);
        }
        public static EntryEngine.Network.PagedModel<EntryEngine._Tuple<T1, T2, T3, T4>> SelectJoinPaged<T1, T2, T3, T4>(int page, int pageSize, string sql, int count1, int count2, int count3, params object[] _params)
        where T1 : new()  where T2 : new()  where T3 : new()  where T4 : new()
        {
            return _Database.SelectJoinPaged<T1, T2, T3, T4>(page, pageSize, sql, count1, count2, count3, _params);
        }
        public static EntryEngine.Network.PagedModel<EntryEngine._Tuple<T1, T2>> SelectJoinPaged<T1, T2>(int page, int pageSize, string selectCountSQL, string __where, string sql, string afterWhere, int count1, params object[] _params)
        where T1 : new()  where T2 : new()
        {
            return _Database.SelectJoinPaged<T1, T2>(page, pageSize, selectCountSQL, __where, sql, afterWhere, count1, _params);
        }
        public static EntryEngine.Network.PagedModel<EntryEngine._Tuple<T1, T2, T3>> SelectJoinPaged<T1, T2, T3>(int page, int pageSize, string selectCountSQL, string __where, string sql, string afterWhere, int count1, int count2, params object[] _params)
        where T1 : new()  where T2 : new()  where T3 : new()
        {
            return _Database.SelectJoinPaged<T1, T2, T3>(page, pageSize, selectCountSQL, __where, sql, afterWhere, count1, count2, _params);
        }
        public static EntryEngine.Network.PagedModel<EntryEngine._Tuple<T1, T2, T3, T4>> SelectJoinPaged<T1, T2, T3, T4>(int page, int pageSize, string selectCountSQL, string __where, string sql, string afterWhere, int count1, int count2, int count3, params object[] _params)
        where T1 : new()  where T2 : new()  where T3 : new()  where T4 : new()
        {
            return _Database.SelectJoinPaged<T1, T2, T3, T4>(page, pageSize, selectCountSQL, __where, sql, afterWhere, count1, count2, count3, _params);
        }
        public static EntryEngine.Async ExecuteAsync(System.Action<EntryEngine.Network._DATABASE.Database> action)
        {
            return _Database.ExecuteAsync(action);
        }
        public static void Dispose()
        {
            _Database.Dispose();
        }
    }
}

#endif
