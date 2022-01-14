using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EntryEngine.Serialize;
using EntryEngine.Network;
using System.Collections;
using System.Threading;
using System.Diagnostics;

namespace EntryEngine.Network
{
#if SERVER
    public static partial class _DATABASE
    {
        public abstract class Database : IDisposable
        {
            private string connectionString;
            public Action<IDbConnection, Database> OnTestConnection;
            public Action<IDbConnection, Database> OnCreateConnection;
            public ushort Timeout = 60;

            public bool Available { get; private set; }
            public virtual string ConnectionString
            {
                get { return connectionString; }
                set
                {
                    if (Available)
                        throw new InvalidOperationException("Can't change tested connection string.");
                    if (string.IsNullOrEmpty(value))
                        throw new ArgumentNullException("connectionString");
                    connectionString = value;
                }
            }
            public virtual string DatabaseName { get; internal set; }
            public virtual string DataSource { get; internal set; }
            public virtual string ServerVersion { get; internal set; }

            protected Database()
            {
            }
            public Database(string connectionString)
            {
                this.ConnectionString = connectionString;
            }

            public void TestConnection()
            {
                if (Available)
                    throw new InvalidOperationException("Connection has been tested.");

                if (string.IsNullOrEmpty(ConnectionString))
                    throw new ArgumentNullException("ConnectionString");

                IDbConnection connection = null;
                try
                {
                    connection = CreateAvailableConnection();
                    var conn = connection as System.Data.Common.DbConnection;
                    if (conn != null)
                    {
                        DatabaseName = conn.Database;
                        DataSource = conn.DataSource;
                        ServerVersion = conn.ServerVersion;
                    }
                    // 允许在TestConnection时使用DB操作函数
                    Available = true;
                    if (OnTestConnection != null)
                        OnTestConnection(connection, this);
                }
                catch (Exception ex)
                {
                    Available = false;
                    _LOG.Error(ex, "TestConnection Error!");
                    throw;
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                        Executed(connection);
                    }
                }

                _LOG.Info("Connection test succuss.");
            }
            private IDbConnection CreateAvailableConnection()
            {
                IDbConnection connection = CreateConnection();
                if (OnCreateConnection != null)
                    OnCreateConnection(connection, this);
                if (connection == null || connection.State != ConnectionState.Open)
                    throw new ArgumentException("Connection must not be null and opened.");
                return connection;
            }
            /// <summary>已打开可使用的数据库连接</summary>
            protected internal abstract IDbConnection CreateConnection();
            private IDbCommand CreateCommand(string sql, params object[] parameters)
            {
                if (!Available)
                    throw new InvalidOperationException("Connection must be tested.");
                IDbConnection conn = CreateAvailableConnection();
                IDbCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;
                cmd.CommandTimeout = Timeout;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                for (int i = 0; i < parameters.Length; ++i)
                {
                    IDbDataParameter param = cmd.CreateParameter();
                    param.ParameterName = string.Format("p{0}", i);
                    param.Value = parameters[i];
                    cmd.Parameters.Add(param);
                }
                return cmd;
            }
            private void ExecuteError(Exception ex, string sql, object[] parameters)
            {
                if (!string.IsNullOrEmpty(sql))
                {
                    if (sql.Length > 500)
                        sql = sql.Substring(0, 500) + "...";
                }
                _LOG.Error(ex, "sql: {0}\r\nparam: {1}", sql, JsonWriter.Serialize(parameters));
            }
            public int ExecuteNonQuery(Action<StringBuilder, List<object>> action, bool transaction)
            {
                StringBuilder sqlBuilder = new StringBuilder();
                List<object> sqlParams = new List<object>();
                action(sqlBuilder, sqlParams);
                if (sqlBuilder.Length > 0)
                {
                    string command;
                    if (transaction)
                        command = string.Concat("begin;", sqlBuilder.ToString(), "commit;");
                    else
                        command = sqlBuilder.ToString();

                    return ExecuteNonQuery(command, sqlParams.ToArray());
                }
                return -1;
            }
            public int ExecuteNonQuery(Action<StringBuilder, List<object>> action)
            {
                return ExecuteNonQuery(action, false);
            }
            public int ExecuteNonQuery(string sql, params object[] parameters)
            {
                var command = CreateCommand(sql, parameters);
                try
                {
                    return command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    ExecuteError(ex, sql, parameters);
                    throw;
                }
                finally
                {
                    if (command.Connection != null)
                    {
                        Executed(command.Connection);
                    }
                }
            }
            public object ExecuteScalar(string sql, params object[] parameters)
            {
                var command = CreateCommand(sql, parameters);
                try
                {
                    return command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    ExecuteError(ex, sql, parameters);
                    throw;
                }
                finally
                {
                    if (command.Connection != null)
                    {
                        Executed(command.Connection);
                    }
                }
            }
            public T ExecuteScalar<T>(string sql, params object[] parameters)
            {
                object value = ExecuteScalar(sql, parameters);
                if (value == null || value == DBNull.Value)
                    return default(T);
                return (T)ChangeType(value, typeof(T));
            }
            public T SelectValue<T>(string sql, params object[] parameters)
            {
                object value = ExecuteScalar(sql, parameters);
                if (value == null || value == DBNull.Value)
                    return default(T);
                else
                    //return (T)value;
                    return (T)ChangeType(value, typeof(T));
            }
            [Obsolete("instead of public void ExecuteReader")]
            protected IDataReader ExecuteReader(string sql, params object[] parameters)
            {
                var command = CreateCommand(sql, parameters);
                try
                {
                    return command.ExecuteReader();
                }
                catch (Exception ex)
                {
                    ExecuteError(ex, sql, parameters);
                    throw;
                }
                finally
                {
                    if (command.Connection != null)
                    {
                        Executed(command.Connection);
                    }
                }
            }
            public void ExecuteReader(Action<IDataReader> read, string sql, params object[] parameters)
            {
                if (read == null)
                    throw new ArgumentNullException("read");

                IDataReader reader = null;
                var command = CreateCommand(sql, parameters);
                try
                {
                    reader = command.ExecuteReader();
                    read(reader);
                }
                catch (Exception ex)
                {
                    ExecuteError(ex, sql, parameters);
                    throw;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                    if (command.Connection != null)
                    {
                        Executed(command.Connection);
                    }
                }
            }
            public List<T> SelectValues<T>(string sql, params object[] parameters)
            {
                List<T> list = new List<T>();
                ExecuteReader(
                    (reader) =>
                    {
                        while (reader.Read())
                        {
                            object value = reader[0];
                            if (value == null || value is DBNull)
                                list.Add(default(T));
                            else
                                list.Add((T)ChangeType(reader[0], typeof(T)));
                        }
                    }, sql, parameters);
                return list;
            }
            public T SelectObject<T>(string sql, params object[] parameters)
            {
                T instance = default(T);
                ExecuteReader(
                    (reader) =>
                    {
                        Type type = typeof(T);
                        int count = reader.FieldCount;

                        if (reader.Read())
                        {
                            instance = Activator.CreateInstance<T>();
                            Read(reader, type, 0, count, instance);
                        }
                    }, sql, parameters);
                return instance;
            }
            public List<T> SelectObjects<T>(string sql, params object[] parameters)
            {
                List<T> list = new List<T>();
                SelectObjects<T>(i => list.Add(i), sql, parameters);
                return list;
            }
            public void SelectObjects<T>(Action<T> newInstance, string sql, params object[] parameters)
            {
                if (newInstance == null)
                    throw new ArgumentNullException("newInstance");
                ExecuteReader(
                    (reader) =>
                    {
                        Type type = typeof(T);
                        int count = reader.FieldCount;
                        List<PropertyInfo> properties;
                        List<FieldInfo> fields;
                        int[] indices = new int[count];
                        MultiReadPrepare(reader, type, 0, count, out properties, out fields, ref indices);

                        while (reader.Read())
                        {
                            T instance = Activator.CreateInstance<T>();
                            MultiRead(reader, 0, count, instance, properties, fields, indices);
                            newInstance(instance);
                        }
                    }, sql, parameters);
            }
            public Dictionary<K, V> SelectObjectsGroup<K, V>(Func<V, K> getKey, string sql, params object[] parameters)
            {
                if (getKey == null)
                    throw new ArgumentNullException("getKey");
                Dictionary<K, V> result = new Dictionary<K, V>();
                SelectObjects<V>(i => result.Add(getKey(i), i), sql, parameters);
                return result;
            }
            public Dictionary<K, List<V>> SelectObjectsGroup2<K, V>(Func<V, K> getKey, string sql, params object[] parameters)
            {
                if (getKey == null)
                    throw new ArgumentNullException("getKey");
                Dictionary<K, List<V>> result = new Dictionary<K, List<V>>();
                List<V> temp;
                K key;
                SelectObjects<V>(i =>
                {
                    key = getKey(i);
                    if (!result.TryGetValue(key, out temp))
                    {
                        temp = new List<V>();
                        result.Add(key, temp);
                    }
                    temp.Add(i);
                }, sql, parameters);
                return result;
            }
            public PagedModel<T> SelectPaged<T>(string sql, int pageIndex, int pageSize, Action<IDataReader, List<T>> read, params object[] _params) where T : new()
            {
                if (_params == null) _params = new object[0];

                PagedModel<T> entry = new PagedModel<T>();
                if (pageIndex < 0)
                {
                    // 查询全部
                    var ret = SelectObjects<T>(sql, _params);
                    entry.Page = -1;
                    entry.PageSize = ret.Count;
                    entry.Count = ret.Count;
                    entry.Models = ret;
                    return entry;
                }

                entry.PageSize = pageSize;
                entry.Page = pageIndex;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("SELECT COUNT(1) AS COUNT FROM ({0}) t;", sql);
                sb.AppendLine(string.Format("{0} limit {1}, {2};", sql, pageIndex * pageSize, pageSize));

                ExecuteReader((reader) =>
                {
                    // 总数
                    reader.Read();
                    entry.Count = (int)(long)reader[0];

                    reader.NextResult();
                    entry.Models = new List<T>(pageSize);
                    read(reader, entry.Models);
                }, sb.ToString(), _params);

                return entry;
            }
            /// <summary>分页查询</summary>
            /// <param name="pageIndex">从0开始的分页，小于0则查询全部</param>
            /// <param name="pageSize">一页的条数</param>
            public PagedModel<T> SelectPaged<T>(string sql, int pageIndex, int pageSize, params object[] _params) where T : new()
            {
                return SelectPaged<T>(sql, pageIndex, pageSize,
                    (reader, list) => list.AddRange(_DATABASE.ReadMultiple<T>(reader)), _params);
            }
            public PagedModel<T> SelectPaged<T>(string selectCountSQL, string __where, string selectSQL, string conditionAfterWhere, int page, int pageSize, params object[] param) where T : new()
            {
                return SelectPaged<T>(selectCountSQL, __where, selectSQL, conditionAfterWhere, page, pageSize, (reader, list) => list.AddRange(ReadMultiple<T>(reader)), param);
            }
            public PagedModel<T> SelectPaged<T>(string selectCountSQL, string __where, string selectSQL, string conditionAfterWhere, int page, int pageSize, Action<IDataReader, List<T>> read, params object[] param)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("{0} {1};", selectCountSQL, __where);
                builder.AppendLine("{0} {1} {2} LIMIT {3}, {4};", selectSQL, __where, conditionAfterWhere, page, pageSize);
                PagedModel<T> result = new PagedModel<T>();
                result.Page = page;
                result.PageSize = pageSize;
                ExecuteReader((reader) =>
                {
                    reader.Read();
                    result.Count = (int)(long)reader[0];
                    result.Models = new List<T>();
                    reader.NextResult();
                    read(reader, result.Models);
                }
                , builder.ToString(), param);
                return result;
            }
            public List<_Tuple<T1, T2>> SelectJoin<T1, T2>(string sql, int count1, params object[] _params)
                where T1 : new()
                where T2 : new()
            {
                List<_Tuple<T1, T2>> list = new List<_Tuple<T1, T2>>();
                ExecuteReader(reader => JoinRead<T1, T2>(reader, list, count1), sql, _params);
                return list;
            }
            public List<_Tuple<T1, T2, T3>> SelectJoin<T1, T2, T3>(string sql, int count1, int count2, params object[] _params)
                where T1 : new()
                where T2 : new()
                where T3 : new()
            {
                List<_Tuple<T1, T2, T3>> list = new List<_Tuple<T1, T2, T3>>();
                ExecuteReader(reader => JoinRead<T1, T2, T3>(reader, list, count1, count2), sql, _params);
                return list;
            }
            public List<_Tuple<T1, T2, T3, T4>> SelectJoin<T1, T2, T3, T4>(string sql, int count1, int count2, int count3, params object[] _params)
                where T1 : new()
                where T2 : new()
                where T3 : new()
                where T4 : new()
            {
                List<_Tuple<T1, T2, T3, T4>> list = new List<_Tuple<T1, T2, T3, T4>>();
                ExecuteReader(reader => JoinRead<T1, T2, T3, T4>(reader, list, count1, count2, count3), sql, _params);
                return list;
            }
            public PagedModel<_Tuple<T1, T2>> SelectJoinPaged<T1, T2>(int page, int pageSize, string sql, int count1, params object[] _params)
                where T1 : new()
                where T2 : new()
            {
                return SelectPaged<_Tuple<T1, T2>>(sql, page, pageSize,
                    (reader, list) => JoinRead<T1, T2>(reader, list, count1), _params);
            }
            public PagedModel<_Tuple<T1, T2, T3>> SelectJoinPaged<T1, T2, T3>(int page, int pageSize, string sql, int count1, int count2, params object[] _params)
                where T1 : new()
                where T2 : new()
                where T3 : new()
            {
                return SelectPaged<_Tuple<T1, T2, T3>>(sql, page, pageSize,
                    (reader, list) => JoinRead<T1, T2, T3>(reader, list, count1, count2), _params);
            }
            public PagedModel<_Tuple<T1, T2, T3, T4>> SelectJoinPaged<T1, T2, T3, T4>(int page, int pageSize, string sql, int count1, int count2, int count3, params object[] _params)
                where T1 : new()
                where T2 : new()
                where T3 : new()
                where T4 : new()
            {
                return SelectPaged<_Tuple<T1, T2, T3, T4>>(sql, page, pageSize,
                    (reader, list) => JoinRead<T1, T2, T3, T4>(reader, list, count1, count2, count3), _params);
            }
            public PagedModel<_Tuple<T1, T2>> SelectJoinPaged<T1, T2>(int page, int pageSize, string selectCountSQL, string __where, string sql, string afterWhere, int count1, params object[] _params)
                where T1 : new()
                where T2 : new()
            {
                return SelectPaged<_Tuple<T1, T2>>(selectCountSQL, __where, sql, afterWhere,
                    page, pageSize,
                    (reader, list) => JoinRead<T1, T2>(reader, list, count1), _params);
            }
            public PagedModel<_Tuple<T1, T2, T3>> SelectJoinPaged<T1, T2, T3>(int page, int pageSize, string selectCountSQL, string __where, string sql, string afterWhere, int count1, int count2, params object[] _params)
                where T1 : new()
                where T2 : new()
                where T3 : new()
            {
                return SelectPaged<_Tuple<T1, T2, T3>>(selectCountSQL, __where, sql, afterWhere,
                    page, pageSize,
                    (reader, list) => JoinRead<T1, T2, T3>(reader, list, count1, count2), _params);
            }
            public PagedModel<_Tuple<T1, T2, T3, T4>> SelectJoinPaged<T1, T2, T3, T4>(int page, int pageSize, string selectCountSQL, string __where, string sql, string afterWhere, int count1, int count2, int count3, params object[] _params)
                where T1 : new()
                where T2 : new()
                where T3 : new()
                where T4 : new()
            {
                return SelectPaged<_Tuple<T1, T2, T3, T4>>(selectCountSQL, __where, sql, afterWhere,
                    page, pageSize,
                    (reader, list) => JoinRead<T1, T2, T3, T4>(reader, list, count1, count2, count3), _params);
            }
            protected virtual void Executed(IDbConnection connection)
            {
                connection.Close();
            }
            public Async ExecuteAsync(Action<Database> action)
            {
                var async = new AsyncThread();
                async.Queue(() =>
                    {
                        action(this);
                        return 1;
                    });
                return async;
            }
            public virtual void Dispose()
            {
            }
        }

        public static void JoinRead<T1, T2>(IDataReader reader, List<_Tuple<T1, T2>> list, int count1)
            where T1 : new()
            where T2 : new()
        {
            int count = reader.FieldCount;
            List<PropertyInfo> properties;
            List<FieldInfo> fields;
            int[] indices = null;
            int offset;
            while (reader.Read())
            {
                offset = 0;
                _DATABASE.MultiReadPrepare(reader, typeof(T1), offset, count1, out properties, out fields, ref indices);
                T1 t1 = _DATABASE.MultiRead<T1>(reader, offset, count1, properties, fields, indices);
                offset += count1;
                _DATABASE.MultiReadPrepare(reader, typeof(T2), offset, count - offset, out properties, out fields, ref indices);
                T2 t2 = _DATABASE.MultiRead<T2>(reader, offset, count - offset, properties, fields, indices);
                list.Add(new _Tuple<T1, T2>(t1, t2));
            }
        }
        public static void JoinRead<T1, T2, T3>(IDataReader reader, List<_Tuple<T1, T2, T3>> list, int count1, int count2)
            where T1 : new()
            where T2 : new()
            where T3 : new()
        {
            int count = reader.FieldCount;
            List<PropertyInfo> properties;
            List<FieldInfo> fields;
            int[] indices = null;
            int offset;
            while (reader.Read())
            {
                offset = 0;
                _DATABASE.MultiReadPrepare(reader, typeof(T1), offset, count1, out properties, out fields, ref indices);
                T1 t1 = _DATABASE.MultiRead<T1>(reader, offset, count1, properties, fields, indices);
                offset += count1;
                _DATABASE.MultiReadPrepare(reader, typeof(T2), offset, count2, out properties, out fields, ref indices);
                T2 t2 = _DATABASE.MultiRead<T2>(reader, offset, count2, properties, fields, indices);
                offset += count2;
                _DATABASE.MultiReadPrepare(reader, typeof(T3), offset, count - offset, out properties, out fields, ref indices);
                T3 t3 = _DATABASE.MultiRead<T3>(reader, offset, count - offset, properties, fields, indices);
                list.Add(new _Tuple<T1, T2, T3>(t1, t2, t3));
            }
        }
        public static void JoinRead<T1, T2, T3, T4>(IDataReader reader, List<_Tuple<T1, T2, T3, T4>> list, int count1, int count2, int count3)
            where T1 : new()
            where T2 : new()
            where T3 : new()
            where T4 : new()
        {
            int count = reader.FieldCount;
            List<PropertyInfo> properties;
            List<FieldInfo> fields;
            int[] indices = null;
            int offset;
            while (reader.Read())
            {
                offset = 0;
                _DATABASE.MultiReadPrepare(reader, typeof(T1), offset, count1, out properties, out fields, ref indices);
                T1 t1 = _DATABASE.MultiRead<T1>(reader, offset, count1, properties, fields, indices);
                offset += count1;
                _DATABASE.MultiReadPrepare(reader, typeof(T2), offset, count2, out properties, out fields, ref indices);
                T2 t2 = _DATABASE.MultiRead<T2>(reader, offset, count2, properties, fields, indices);
                offset += count2;
                _DATABASE.MultiReadPrepare(reader, typeof(T3), offset, count3, out properties, out fields, ref indices);
                T3 t3 = _DATABASE.MultiRead<T3>(reader, offset, count3, properties, fields, indices);
                offset += count3;
                _DATABASE.MultiReadPrepare(reader, typeof(T4), offset, count - offset, out properties, out fields, ref indices);
                T4 t4 = _DATABASE.MultiRead<T4>(reader, offset, count - offset, properties, fields, indices);
                list.Add(new _Tuple<T1, T2, T3, T4>(t1, t2, t3, t4));
            }
        }
        public static IEnumerable<T> ReadMultiple<T>(IDataReader reader) where T : new()
        {
            Type type = typeof(T);
            int count = reader.FieldCount;
            List<PropertyInfo> properties;
            List<FieldInfo> fields;
            int[] indices = new int[count];
            MultiReadPrepare(reader, type, 0, count, out properties, out fields, ref indices);
            while (reader.Read())
            {
                T instance = new T();
                MultiRead(reader, 0, count, instance, properties, fields, indices);
                yield return instance;
            }
        }
        public static IEnumerable<T> ReadMultiple<T>(IDataReader reader, List<PropertyInfo> properties, List<FieldInfo> fields, int[] indices) where T : new()
        {
            Type type = typeof(T);
            int count = reader.FieldCount;
            while (reader.Read())
            {
                T instance = new T();
                MultiRead(reader, 0, count, instance, properties, fields, indices);
                yield return instance;
            }
        }
        public static T Read<T>(IDataReader reader) where T : new()
        {
            if (reader.Read())
            {
                T instance = new T();
                Read(reader, typeof(T), 0, reader.FieldCount, instance);
                return instance;
            }
            return default(T);
        }
        public static T ReadObject<T>(IDataReader reader, int offset, int fieldCount) where T : new()
        {
            T instance = new T();
            Read(reader, typeof(T), offset, fieldCount, instance);
            return instance;
        }
        public static void ReadObject(IDataReader reader, object instance)
        {
            Read(reader, instance.GetType(), 0, reader.FieldCount, instance);
        }
        public static void ReadObject(IDataReader reader, object instance, int offset, int fieldCount)
        {
            Read(reader, instance.GetType(), offset, fieldCount, instance);
        }
        private static void Read(IDataReader reader, Type type, int offset, int count, object instance)
        {
            var properties = type.GetProperties().ToDictionary(v => v.Name);
            var fields = type.GetFields().ToDictionary(v => v.Name);
            PropertyInfo property;
            FieldInfo field;
            for (int i = offset, e = offset + count; i < e; i++)
            {
                object value = reader[i];
                if (value == DBNull.Value)
                    continue;

                string name = reader.GetName(i);
                var _type = reader.GetFieldType(i);

                if (properties.TryGetValue(name, out property))
                {
                    if (_type != property.PropertyType)
                        value = ChangeType(value, property.PropertyType);
                    property.SetValue(instance, value, null);
                    continue;
                }

                if (!fields.TryGetValue(name, out field))
                    continue;
                if (_type != field.FieldType)
                    value = ChangeType(value, field.FieldType);
                field.SetValue(instance, value);
            }
        }
        /// <summary>连续读取多个对象时，一次性把属性和字段都准备好，加速反射效率</summary>
        /// <param name="indices">0:不需要读取 / 负数:-(属性的索引+1) / 正数:字段的索引+1</param>
        public static void MultiReadPrepare(IDataReader reader, Type type, int offset, int count,
            out List<PropertyInfo> properties, out List<FieldInfo> fields, ref int[] indices)
        {
            properties = type.GetProperties().ToList();
            fields = type.GetFields().ToList();
            if (indices == null || indices.Length != reader.FieldCount)
                indices = new int[reader.FieldCount];
            for (int i = offset, e = offset + count; i < e; i++)
            {
                string name = reader.GetName(i);
                int index = properties.IndexOf(p => p.Name == name);
                if (index != -1)
                {
                    indices[i] = -(index + 1);
                    continue;
                }
                index = fields.IndexOf(f => f.Name == name);
                if (index != -1)
                    indices[i] = index + 1;
            }
        }
        /// <summary>使用MultiReadPrepare准备好的参数，快速读取一个对象</summary>
        /// <param name="properties">MultiReadPrepare的输出参数</param>
        /// <param name="fields">MultiReadPrepare的输出参数</param>
        /// <param name="indices">MultiReadPrepare的输出参数</param>
        public static void MultiRead(IDataReader reader, int offset, int count, object instance,
            List<PropertyInfo> properties, List<FieldInfo> fields, int[] indices)
        {
            PropertyInfo property;
            FieldInfo field;
            for (int i = offset, e = offset + count; i < e; i++)
            {
                int index = indices[i];
                if (index == 0) continue;

                object value = reader[i];
                if (value == DBNull.Value)
                    continue;

                var type = reader.GetFieldType(i);

                if (index < 0)
                {
                    property = properties[-index - 1];
                    if (type != property.PropertyType)
                        value = ChangeType(value, property.PropertyType);
                    property.SetValue(instance, value, null);
                    continue;
                }

                field = fields[index - 1];
                if (type != field.FieldType)
                    value = ChangeType(value, field.FieldType);
                field.SetValue(instance, value);
            }
        }
        public static T MultiRead<T>(IDataReader reader, int offset, int count,
            List<PropertyInfo> properties, List<FieldInfo> fields, int[] indices) where T : new()
        {
            T instance = new T();
            PropertyInfo property;
            FieldInfo field;
            for (int i = offset, e = offset + count; i < e; i++)
            {
                int index = indices[i];
                if (index == 0) continue;

                object value = reader[i];
                if (value == DBNull.Value)
                    continue;

                var type = reader.GetFieldType(i);

                if (index < 0)
                {
                    property = properties[-index - 1];
                    if (type != property.PropertyType)
                        value = ChangeType(value, property.PropertyType);
                    property.SetValue(instance, value, null);
                    continue;
                }

                field = fields[index - 1];
                if (type != field.FieldType)
                    value = ChangeType(value, field.FieldType);
                field.SetValue(instance, value);
            }
            return instance;
        }
        public static Dictionary<string, string> ParseConnectionString(string connString, bool keyToLower)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string[] datas = connString.Split(';');
            for (int i = 0; i < datas.Length; i++)
            {
                if (string.IsNullOrEmpty(datas[i])) continue;
                string[] keyvalue = datas[i].Split('=');
                string key = keyToLower ? keyvalue[0].ToLower() : keyvalue[0];
                dic[key] = keyvalue[1];
            }
            return dic;
        }
        public static object ChangeType(object value, Type type)
        {
            if (type.IsEnum)
            {
                return Enum.ToObject(type, value);
            }
            return Convert.ChangeType(value, type);
        }
    }
    public abstract class Database_Link : EntryEngine.Network._DATABASE.Database
    {
        public virtual EntryEngine.Network._DATABASE.Database Base { get; set; }
        public override string ConnectionString
        {
            get
            {
                return Base.ConnectionString;
            }
            set
            {
                Base.ConnectionString = value;
            }
        }
        public override string DatabaseName
        {
            get { return Base.DatabaseName; }
            internal set { Base.DatabaseName = value; }
        }
        public override string DataSource
        {
            get { return Base.DataSource; }
            internal set { Base.DataSource = value; }
        }
        public override string ServerVersion
        {
            get { return Base.ServerVersion; }
            internal set { Base.ServerVersion = value; }
        }

        public Database_Link() { }
        public Database_Link(EntryEngine.Network._DATABASE.Database Base) { this.Base = Base; }

        protected internal override System.Data.IDbConnection CreateConnection()
        {
            return Base.CreateConnection();
        }
        public override void Dispose()
        {
            Base.Dispose();
        }
    }
    public sealed class ConnectionPool : Database_Link
    {
        class CONNECTION : PoolItem
        {
            public IDbConnection Connection;
            public DateTime LastUseTime;
            public bool Idle;
        }

        private Pool<CONNECTION> pools = new Pool<CONNECTION>();
        public TimeSpan ClearTime = TimeSpan.FromSeconds(15);

        public int WorkingCount
        {
            get { lock (pools) { return pools.Count(c => !c.Idle && c.Connection != null && c.Connection.State != ConnectionState.Closed); } }
        }
        public int IdleCount
        {
            get { lock (pools) { return pools.Count(c => c.Idle && (c.Connection == null || c.Connection.State == ConnectionState.Closed)); } }
        }

        public ConnectionPool()
        {
        }
        public ConnectionPool(_DATABASE.Database _base)
        {
            this.Base = _base;
        }

        public void ClearPool()
        {
            var now = DateTime.Now;
            CONNECTION[] array;
            lock (pools)
                array = pools.ToArray();
            foreach (var item in array)
            {
                if (item.Connection == null ||
                    item.Connection.State == ConnectionState.Closed ||
                    (ClearTime.Ticks > 0 && (now - item.LastUseTime) >= ClearTime))
                {
                    ReleaseConnection(item);
                }
            }
        }
        private void ReleaseConnection(CONNECTION connection)
        {
            try
            {
                // 这里的错误导致连接没有变得可用，进而导致连接会越来越多
                if (connection.Connection != null)
                {
                    connection.Connection.Close();
                    connection.Connection.Dispose();
                    connection.Connection = null;
                }
            }
            finally
            {
                lock (pools)
                {
                    pools.RemoveAt(connection);
                    connection.Idle = true;
                }
            }
        }

        protected internal override IDbConnection CreateConnection()
        {
            DateTime now = DateTime.Now;
            lock (pools)
            {
                CONNECTION conn = null;
                foreach (var item in pools)
                {
                    if (item.Connection == null ||
                        item.Connection.State == ConnectionState.Closed ||
                        (ClearTime.Ticks > 0 && (now - item.LastUseTime) >= ClearTime))
                    {
                        ReleaseConnection(item);
                    }
                    else if (item.Idle)
                    {
                        conn = item;
                        break;
                    }
                }
                //var conn = pools.FirstOrDefault(p => p.Idle);
                if (conn == null)
                {
                    conn = pools.Allot();
                    if (conn == null)
                    {
                        conn = new CONNECTION();
                        conn.Idle = false;
                        pools.Add(conn);
                    }
                    try
                    {
                        conn.Connection = base.CreateConnection();
                    }
                    catch
                    {
                        ReleaseConnection(conn);
                        throw;
                    }
                }
                conn.Idle = false;
                conn.LastUseTime = DateTime.Now;
                return conn.Connection;
            }
        }
        protected override void Executed(IDbConnection connection)
        {
            lock (pools)
            {
                CONNECTION conn = pools.FirstOrDefault(p => p.Connection == connection);
                if (conn != null)
                {
                    if (conn.Connection == null || conn.Connection.State == ConnectionState.Closed)
                        ReleaseConnection(conn);
                    conn.Idle = true;
                }
                else
                    base.Executed(connection);
            }
        }
        public override void Dispose()
        {
            lock (pools)
            {
                foreach (var connection in pools)
                {
                    connection.Connection.Close();
                    connection.Connection.Dispose();
                }
                pools.Clear();
            }
        }
    }

    [Code(ECode.MayBeReform)]
    public abstract class MemoryDatabase : IDisposable
    {
        private string name = string.Empty;
        private string dir = string.Empty;
        public string Dir
        {
            get { return dir; }
            set { dir = _IO.DirectoryWithEnding(value); }
        }
        public string Name
        {
            get { return name; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("name");
                name = value;
            }
        }

        public MemoryDatabase(string name)
        {
            this.Name = name;
            this.Dir = name;
        }

        public void BackUp(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                throw new ArgumentNullException("BackUp directory name can't be null.");
            _LOG.Info("BackUp {0} to {1}...", name, dir);
            string temp = this.dir;
            this.Dir = Name + dir;

            Save();

            this.Dir = temp;
            _LOG.Info("BackUp {0} to {1} completed.", name, dir);
        }
        public abstract void Save();
        public abstract void Load();
        private string BuildTableFile(string tableName)
        {
            return string.Format("{0}{1}.csv", dir, tableName);
        }
        protected void InternalSave<T>(T[] table, string tablename)
        {
            tablename = BuildTableFile(tablename);
            var dir = Path.GetDirectoryName(tablename);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            Type type = typeof(T);
            _LOG.Info("Save {0} [{1}]...", type.Name, table.Length);

            TIMER timer = TIMER.StartNew();

            CSVWriter writer = new CSVWriter();
            writer.Setting.Static = false;
            //writer.Setting.AutoType = false;
            writer.Setting.Property = false;
            writer.WriteObject(table);
            File.WriteAllText(tablename, writer.Result);

            _LOG.Info("Save {0} [{1}] completed! Time elapsed: {2}", type.Name, table.Length, timer.Stop());
        }
        protected List<T> InternalLoad<T>(string tablename)
        {
            tablename = BuildTableFile(tablename);
            if (!File.Exists(tablename))
            {
                _LOG.Info("Initialize memory table {0}", tablename);
                return new List<T>();
            }

            Type type = typeof(T);
            _LOG.Info("Load {0}...", type.Name);

            TIMER timer = TIMER.StartNew();

            CSVReader reader = new CSVReader(File.ReadAllText(tablename));
            reader.Setting.Static = false;
            //reader.Setting.AutoType = false;
            reader.Setting.Property = false;
            List<T> load = new List<T>(reader.ReadObject<T[]>());

            _LOG.Info("Load {0} [{1}] completed! Time elapsed: {2}", type.Name, load.Count, timer.Stop());
            return load;
        }
        public abstract void Dispose();
    }

    public class MergeDatabase
    {
        public string ConnectionStringWithDB;
        public MergeTable[] Tables;

        public MergeDatabase() { }
        public MergeDatabase(string connectionString)
        {
            this.ConnectionStringWithDB = connectionString;
        }
    }
    public class MergeTable
    {
        public string TableName;
        /// <summary>
        /// <para>SELECT * FROM TABLE 后的筛选条件，符合条件的数据将被留下，如下</para>
        /// <para>WHERE registTime > '2016/7/20'</para>
        /// <para>JOIN ON playerinfo ON (TABLE.uid = playerinfo.uid) WHERE level > 10</para>
        /// </summary>
        public string Where;
        public bool AutoMergeIdentity = true;
        //public Action<_DATABASE.Database> Update;

        public MergeTable() { }
        public MergeTable(string tableName)
        {
            this.TableName = tableName;
        }
        public MergeTable(string tableName, string where)
        {
            this.TableName = tableName;
            this.Where = where;
        }
    }
    public class AsyncThread : Async
    {
        private Func<float> func;

        public void Queue(Func<float> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            this.func = func;
            Run();
        }
        protected override void InternalRun()
        {
            if (!System.Threading.ThreadPool.QueueUserWorkItem((state) =>
            {
                while (true)
                {
                    lock (this)
                        if (State != EAsyncState.Running)
                            break;
                    try
                    {
                        float progress = func();
                        lock (this)
                            Progress = progress;
                    }
                    catch (Exception ex)
                    {
                        lock (this)
                            Error(ex);
                        break;
                    }
                }
            }))
            {
                throw new OutOfMemoryException();
            }
        }

        public static AsyncThread QueueUserWorkItem(Func<float> func)
        {
            AsyncThread thread = new AsyncThread();
            thread.Queue(func);
            return thread;
        }
        public static AsyncThread QueueUserWorkItem(Func<bool> func)
        {
            return QueueUserWorkItem(() => func() ? 1 : 0);
        }
        public static AsyncThread QueueUserWorkItem(Action action)
        {
            return QueueUserWorkItem(() =>
            {
                action();
                return 1;
            });
        }
        /// <summary>阻塞当前线程，直到异步把指定的操作执行完成</summary>
        /// <param name="action">方法将在异步上执行</param>
        public static void SyncExecute(Action func)
        {
            EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            Exception exception = null;
            if (!System.Threading.ThreadPool.QueueUserWorkItem((state) =>
            {
                try
                {
                    func();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    handle.Set();
                }
            }))
            {
                throw new OutOfMemoryException();
            }
            handle.WaitOne();
            if (exception != null)
                throw exception;
        }
        /// <summary>阻塞当前线程，直到异步把指定的操作执行完成</summary>
        /// <param name="func">方法将在异步上执行，返回true代表执行完毕，请不要阻塞线程，否则等待超时将无效</param>
        /// <param name="timeout">等待超时时间，单位毫秒，负数时则永不超时，超时将抛出TimeoutException</param>
        public static void SyncExecute(Func<bool> func, int timeout)
        {
            EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            Exception exception = null;
            if (!System.Threading.ThreadPool.QueueUserWorkItem((state) =>
            {
                Stopwatch timer = Stopwatch.StartNew();
                while (true)
                {
                    try
                    {
                        if (func())
                        {
                            handle.Set();
                            break;
                        }
                        else
                        {
                            if (timeout >= 0 && timer.ElapsedMilliseconds > timeout)
                            {
                                exception = new TimeoutException();
                                handle.Set();
                                break;
                            }
                            Thread.Sleep(20);
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        handle.Set();
                        break;
                    }
                }
            }))
            {
                throw new OutOfMemoryException();
            }
            handle.WaitOne();
            if (exception != null)
                throw exception;
        }
    }

    /// <summary>内部级联的树状关系，例如分类和二级分类，账号和邀请账号等</summary>
    public abstract class InnerCascade
    {
        public enum EModifiedFlag
        {
            /// <summary>没有修改</summary>
            None,
            /// <summary>修改了Parents</summary>
            Parents,
            /// <summary>修改了ParentID</summary>
            ParentID,
        }
        [Index(EIndex.Identity)]
        public int ID;
        [Index(EIndex.Group)]
        [Foreign(typeof(InnerCascade), "ID")]
        public int ParentID;
        /// <summary>父类的级联数组，格式为0,1,2，越靠前级别越高</summary>
        [Index]
        public string Parents;
        /// <summary>父类的级联数组，越靠前级别越高</summary>
        public int[] ParentsIDs
        {
            get { return ToCascadeParentsArray(Parents); }
        }
        public bool HasParent { get { return ParentID != 0; } }
        /// <summary>顶级ID，若没有上级，则顶级是自己</summary>
        public int Top
        {
            get
            {
                if (ParentID == 0)
                    return ID;

                int index = Parents.IndexOf(',');
                if (index == -1)
                    return ParentID;
                else
                    return int.Parse(Parents.Substring(0, index));
            }
        }
        public EModifiedFlag ModifiedFlag { get; internal set; }
        public void SetParent(InnerCascade parent)
        {
            ParentID = parent.ID;
            if (string.IsNullOrEmpty(parent.Parents))
                Parents = parent.ID.ToString();
            else
                Parents = parent.Parents + "," + parent.ID;
        }
        public static int[] ToCascadeParentsArray(string parents)
        {
            if (string.IsNullOrEmpty(parents))
                return _SARRAY<int>.Empty;

            string[] split = parents.Split(',');
            int length = split.Length;
            int[] array = new int[length];
            for (int i = 0; i < length; i++)
                array[i] = int.Parse(split[i]);
            return array;
        }
        public static string ToCascadeParentsString(int[] parents)
        {
            if (parents == null || parents.Length == 0)
                return null;

            StringBuilder builder = new StringBuilder();
            for (int i = 0, e = parents.Length - 1; i <= e; i++)
            {
                builder.Append(parents[i]);
                if (i != e)
                    builder.Append(',');
            }
            return builder.ToString();
        }
        /// <summary>级联父级更换</summary>
        /// <param name="items">完整级联列表，包含所有级别</param>
        /// <param name="id">要改的目标层级</param>
        /// <param name="newParentID">新的父级</param>
        public static void UpdateCascadeParentID(IEnumerable<InnerCascade> items, int id, int newParentID)
        {
            // 当前项
            InnerCascade current = items.FirstOrDefault(i => i.ID == id);
            if (current == null)
                throw new InvalidOperationException("没有找到目标项");

            // ParentID并没有修改
            if (current.ParentID == newParentID)
                return;

            // 上级项，为null时则是顶级项
            InnerCascade parent = items.FirstOrDefault(i => i.ID == newParentID);
            if (parent == null)
            {
                // 可能是顶级项，那么曾经必须有其它顶级项
                var top = items.FirstOrDefault(i => i.ParentID == newParentID);
                if (top == null)
                    throw new InvalidOperationException("没有找到上级项");
            }

            var dic = items.ToDictionary(t => t.ID, t => t.ParentID);

            // 上下级颠倒
            bool upside_down = false;
            // 如果新的上级是目标原来的下级,那么这个新上级的上级改为目标原来的上级
            if (parent != null)
            {
                int currentID = newParentID;
                int parentID;
                while (true)
                {
                    if (!dic.TryGetValue(currentID, out parentID))
                        break;
                    if (currentID == parentID) break;
                    if (parentID == current.ID)
                    {
                        parent.ParentID = current.ParentID;
                        parent.ModifiedFlag = EModifiedFlag.ParentID;
                        dic[parent.ID] = current.ParentID;
                        upside_down = true;
                        break;
                    }
                    currentID = parentID;
                }
            }

            // 修改自己的上级ID
            current.ParentID = newParentID;
            current.ModifiedFlag = EModifiedFlag.ParentID;
            dic[current.ID] = newParentID;

            // 更新受影响的所有子项的上级数组
            {
                Stack<int> stack = new Stack<int>();
                StringBuilder builder = new StringBuilder();
                foreach (var item in items)
                {
                    stack.Clear();
                    bool flag = item.ID == id || (upside_down && item.ID == newParentID);
                    int currentID = item.ParentID;
                    int parentID;
                    while (true)
                    {
                        if (!dic.TryGetValue(currentID, out parentID))
                            break;
                        if (currentID == parentID) break;
                        if (
                            // 目标更换了上级，所以其所有下级都需要更新级联数组
                            currentID == id
                            // 若新上级是原来自己的下级，则也相当于更换了上级，所以其所有下级都需要更新级联数组
                            || (upside_down && currentID == newParentID)
                            )
                            flag = true;
                        stack.Push(currentID);
                        currentID = parentID;
                    }
                    if (!flag)
                    {
                        item.ModifiedFlag = EModifiedFlag.None;
                        continue;
                    }
                    if (item.ModifiedFlag == EModifiedFlag.None)
                        item.ModifiedFlag = EModifiedFlag.Parents;
                    builder.Remove(0, builder.Length);
                    while (stack.Count > 0)
                    {
                        builder.Append(stack.Pop());
                        if (stack.Count > 0)
                            builder.Append(',');
                    }
                    item.Parents = builder.ToString();
                }
            }
        }
    }
#endif

    /// <summary>翻页数据模型</summary>
    public class PagedModel<T>
    {
        public int Count;
        public int Page;
        public int PageSize;
        public List<T> Models;

        public PagedModel<U> ChangeModel<U>(Func<T, U> select)
        {
            PagedModel<U> ret = new PagedModel<U>();
            ret.Count = this.Count;
            ret.Page = this.Page;
            ret.PageSize = this.PageSize;
            int count = Models.Count;
            ret.Models = new List<U>(count);
            for (int i = 0; i < Models.Count; i++)
                ret.Models.Add(select(Models[i]));
            return ret;
        }
    }

    public enum EIndex : byte
    {
        /// <summary>
        /// 索引：用于查询
        /// </summary>
        Index,
        /// <summary>
        /// 主键：添加时要测重
        /// <para>多主键时，每个主键生成Group</para>
        /// <para>所有主键生成一个复合主键类型，由此类型生成</para>
        /// </summary>
        Primary,
        /// <summary>
        /// 自增：视为主键
        /// </summary>
        Identity,
        /// <summary>
        /// 分组：例如同一个玩家的操作记录
        /// </summary>
        Group,
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class IndexAttribute : Attribute
    {
        public EIndex Index
        {
            get;
            private set;
        }

        public IndexAttribute()
        {
        }
        public IndexAttribute(EIndex index)
        {
            this.Index = index;
        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class ForeignAttribute : Attribute
    {
        public Type ForeignTable
        {
            get;
            private set;
        }
        public string ForeignField
        {
            get;
            private set;
        }

        public ForeignAttribute(Type type, string field)
        {
            this.ForeignTable = type;
            this.ForeignField = field;
        }
        public ForeignAttribute(Type type)
        {
            this.ForeignTable = type;
        }
    }
    /// <summary>数据库不生成标记此特性的字段</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class IgnoreAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class MemoryTableAttribute : Attribute
    {
        public bool TempTable { get; private set; }
        public MemoryTableAttribute()
        {
        }
        public MemoryTableAttribute(bool temp)
        {
            TempTable = temp;
        }
    }
}

#if SERVER
namespace EntryEngine.Database.MDBClient
{
    public class MDB : _DATABASE.Database
    {
        protected internal override IDbConnection CreateConnection()
        {
            MDBConnection connection = new MDBConnection();
            connection.ConnectionString = ConnectionString;
            connection.Open();
            return connection;
        }
    }
    public class MDBConnection : IDbConnection
    {
        static char[] SPLIT = { ';' };

        private LinkTcp link;
        private ConnectionState state = ConnectionState.Closed;
        private int connectionTimeout;
        private string server;
        private ushort port;
        private string user;
        private string password;
        private string database;
        private bool isLocal;

        public ConnectionState State
        {
            get { return state; }
        }
        /// <summary>等待连接打开的时间（以秒为单位）。默认值为 15 秒。</summary>
        public int ConnectionTimeout
        {
            get { return connectionTimeout; }
        }
        public string Database
        {
            get { return database; }
        }
        /// <summary>仅支持server,port,user,password,database
        /// <para>若是本地数据库服务，设置server为127.0.0.1或localhost即可</para>
        /// </summary>
        public string ConnectionString
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                if (!string.IsNullOrEmpty(server))
                {
                    builder.Append("server={0}", server);
                }
                if (port != 0)
                {
                    if (builder.Length > 0)
                        builder.Append(';');
                    builder.Append("port={0}", port);
                }
                if (!string.IsNullOrEmpty(user))
                {
                    if (builder.Length > 0)
                        builder.Append(';');
                    builder.Append("user={0}", user);
                }
                if (!string.IsNullOrEmpty(password))
                {
                    if (builder.Length > 0)
                        builder.Append(';');
                    builder.Append("password={0}", password);
                }
                if (!string.IsNullOrEmpty(database))
                {
                    if (builder.Length > 0)
                        builder.Append(';');
                    builder.Append("database={0}", database);
                }
                return builder.ToString();
            }
            set
            {
                if (state != ConnectionState.Closed)
                    throw new InvalidOperationException("连接已打开，不能更换连接字符串");
                if (string.IsNullOrEmpty(value))
                {
                    ResetConnectionParameter();
                    return;
                }
                string[] items = value.Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < items.Length; i++)
                {
                    string[] keyvalue = items[i].Split('=');
                    if (keyvalue.Length != 2)
                        throw new ArgumentException("错误的键值格式" + items[i]);
                    string key = keyvalue[0].ToLower();
                    switch (key)
                    {
                        case "server": server = keyvalue[1]; break;
                        case "port": port = ushort.Parse(keyvalue[1]); break;
                        case "user": user = keyvalue[1]; break;
                        case "password": password = keyvalue[1]; break;
                        case "database": database = keyvalue[1]; break;
                        default: throw new NotSupportedException("不支持的keyword: " + key);
                    }
                }
                isLocal = (server == "127.0.0.1" || server == "localhost") && port == 0;
            }
        }
        internal bool IsLocal { get { return isLocal; } }

        public MDBConnection()
        {
            ResetConnectionParameter();
        }

        void ResetConnectionParameter()
        {
            connectionTimeout = 15;
            server = null;
            port = 0;
            user = null;
            password = null;
            database = null;
            isLocal = false;
        }
        public void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }
        IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il)
        {
            throw new NotImplementedException();
        }
        IDbTransaction IDbConnection.BeginTransaction()
        {
            throw new NotImplementedException();
        }
        IDbCommand IDbConnection.CreateCommand()
        {
            MDBCommand command = new MDBCommand();
            command.Connection = this;
            if (isLocal)
                command.Tunnle = new DataTunnleLocal();
            else
                command.Tunnle = new DataTunnleRemote();
            return command;
        }
        public void Open()
        {
            if (state != ConnectionState.Closed)
                throw new InvalidOperationException("连接尚未关闭，不能重复打开连接");

            if (isLocal)
            {
                state = ConnectionState.Open;
            }
            else
            {
                state = ConnectionState.Connecting;
                try
                {
                    link = LinkTcp.Connect(server, port, null);
                    ByteWriter writer = new ByteWriter();
                    writer.Write(user);
                    writer.Write(password);
                    link.Write(writer.Buffer, 0, writer.Position);
                    link.Flush();
                }
                catch (Exception ex)
                {
                    Close();
                    _LOG.Error("连接数据库服务器失败：{0}", ex.Message);
                    throw;
                }
            }
        }
        public void Close()
        {
            state = ConnectionState.Closed;
            if (link != null)
            {
                link.Close();
                link = null;
            }
        }
        public void Dispose()
        {
            Close();
        }
    }
    internal class MDBCommand : IDbCommand
    {
        class MDBDataParameter : IDbDataParameter
        {
            internal SQLParameter parameter = new SQLParameter();

            public byte Precision
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }
            public byte Scale
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }
            public int Size
            {
                get { return _MDB.GetPritimiveTypeSize(parameter.Type); }
                set
                {
                    throw new NotImplementedException();
                }
            }
            public DbType DbType
            {
                get
                {
                    switch (parameter.Type)
                    {
                        case EPrimitiveType.STRING: return DbType.String;
                        case EPrimitiveType.DATETIME: return DbType.DateTime;
                        case EPrimitiveType.BOOL: return DbType.Boolean;
                        case EPrimitiveType.SBYTE: return DbType.SByte;
                        case EPrimitiveType.BYTE: return DbType.Byte;
                        case EPrimitiveType.SHORT: return DbType.Int16;
                        case EPrimitiveType.USHORT: return DbType.UInt16;
                        case EPrimitiveType.FLOAT: return DbType.Single;
                        case EPrimitiveType.INT: return DbType.Int32;
                        case EPrimitiveType.UINT: return DbType.UInt32;
                        case EPrimitiveType.LONG: return DbType.Int64;
                        case EPrimitiveType.ULONG: return DbType.UInt64;
                        case EPrimitiveType.DOUBLE: return DbType.Double;
                        default: return DbType.Object;
                    }
                }
                set
                {
                    throw new NotImplementedException();
                }
            }
            public ParameterDirection Direction
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }
            public bool IsNullable
            {
                get { throw new NotImplementedException(); }
            }
            public string ParameterName
            {
                get { return parameter.Name; }
                set
                {
                    if (!string.IsNullOrEmpty(value))
                        throw new ArgumentNullException("参数名不能为空");
                    parameter.Name = value;
                }
            }
            public string SourceColumn
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }
            public DataRowVersion SourceVersion
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }
            public object Value
            {
                get { return parameter.Object; }
                set
                {
                    parameter.Type = _MDB.GetType(value);
                    parameter.Object = value;
                }
            }
        }
        class MDBDataParameterCollection : IDataParameterCollection
        {
            internal List<MDBDataParameter> parameters = new List<MDBDataParameter>();

            public object this[string parameterName]
            {
                get
                {
                    int index = IndexOf(parameterName);
                    if (index == -1)
                        return null;
                    else
                        return parameters[index].Value;
                }
                set
                {
                    int index = IndexOf(parameterName);
                    if (index != -1)
                        parameters[index].Value = value;
                }
            }
            public bool Contains(string parameterName)
            {
                return IndexOf(parameterName) != -1;
            }
            public int IndexOf(string parameterName)
            {
                for (int i = 0, count = parameters.Count; i < count; i++)
                    if (parameters[i].ParameterName == parameterName)
                        return i;
                return -1;
            }
            public void RemoveAt(string parameterName)
            {
                int index = IndexOf(parameterName);
                if (index != -1)
                    parameters.RemoveAt(index);
            }

            object IList.this[int index]
            {
                get { return parameters[index].Value; }
                set { parameters[index].Value = value; }
            }
            public int Add(object value)
            {
                MDBDataParameter parameter = value as MDBDataParameter;
                if (parameter == null)
                    throw new InvalidCastException("参数必须是MDBDataParameter的实例");
                parameters.Add(parameter);
                return parameters.Count - 1;
            }
            public void Clear()
            {
                parameters.Clear();
            }
            bool IList.Contains(object value)
            {
                throw new NotImplementedException();
            }
            int IList.IndexOf(object value)
            {
                throw new NotImplementedException();
            }
            void IList.Insert(int index, object value)
            {
                throw new NotImplementedException();
            }
            bool IList.IsFixedSize
            {
                get { return false; }
            }
            bool IList.IsReadOnly
            {
                get { return false; }
            }
            void IList.Remove(object value)
            {
                throw new NotImplementedException();
            }
            void IList.RemoveAt(int index)
            {
                parameters.RemoveAt(index);
            }

            int ICollection.Count
            {
                get { return parameters.Count; }
            }
            bool ICollection.IsSynchronized
            {
                get { throw new NotImplementedException(); }
            }
            object ICollection.SyncRoot
            {
                get { throw new NotImplementedException(); }
            }
            void ICollection.CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return parameters.GetEnumerator();
            }
        }

        internal MDBConnection Connection;
        internal DataTunnle Tunnle;
        MDBDataParameterCollection parameters = new MDBDataParameterCollection();

        public string CommandText { get; set; }
        public int CommandTimeout { get; set; }
        public CommandType CommandType
        {
            get { return CommandType.Text; }
            set
            {
                if (value != CommandType.Text)
                    throw new NotImplementedException("没有实现CommandType.Text以外的调用方式");
            }
        }
        IDbConnection IDbCommand.Connection
        {
            get { return Connection; }
            set
            {
                MDBConnection connection = value as MDBConnection;
                if (connection == null)
                    throw new InvalidCastException("连接类型必须是MDBConnection");
                Connection = connection;
            }
        }
        IDbTransaction IDbCommand.Transaction
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        UpdateRowSource IDbCommand.UpdatedRowSource
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        IDataParameterCollection IDbCommand.Parameters
        {
            get { return parameters; }
        }


        public MDBCommand()
        {
            CommandTimeout = 30;
        }


        IDataReader InternalExecute(EExecute execute)
        {
            SQLCommand command = new SQLCommand();
            command.ExecuteType = execute;
            command.SQL = CommandText;
            command.Timeout = CommandTimeout;
            command.IsLocal = Connection.IsLocal;
            foreach (var p in parameters.parameters)
            {
                if (!Connection.IsLocal)
                {
                    p.parameter.ObjectToByte();
                }
                command.Parameters.Add(p.parameter);
            }

            var result = Tunnle.Execute(command);

            return new MDBReader(this, result);
        }
        IDbDataParameter IDbCommand.CreateParameter()
        {
            IDbDataParameter parameter = new MDBDataParameter();
            parameters.Add(parameter);
            return parameter;
        }
        int IDbCommand.ExecuteNonQuery()
        {
            using (IDataReader reader = InternalExecute(EExecute.NonQuery))
            {
                return reader.RecordsAffected;
            }
        }
        IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
        {
            if (behavior != CommandBehavior.Default) throw new NotImplementedException("没有实现CommandBehavior.Default以外的调用方式");
            return InternalExecute(EExecute.Reader);
        }
        IDataReader IDbCommand.ExecuteReader()
        {
            return InternalExecute(EExecute.Reader);
        }
        object IDbCommand.ExecuteScalar()
        {
            using (var reader = InternalExecute(EExecute.Scalar))
            {
                if (reader.Read())
                    return reader[0];
                else
                    return null;
            }
        }
        void IDbCommand.Prepare()
        {
            throw new NotImplementedException();
        }
        public void Cancel()
        {
            throw new NotImplementedException();
        }
        void IDisposable.Dispose()
        {
            Cancel();
            Connection = null;
            parameters.Clear();
        }
    }
    internal class MDBReader : IDataReader
    {
        IDbCommand command;
        List<SQLResult> results;
        int resultIndex;
        int rowIndex = -1;


        bool IDataReader.IsClosed
        {
            get { return command == null; }
        }
        int IDataReader.Depth
        {
            get { throw new NotImplementedException(); }
        }
        int IDataReader.RecordsAffected
        {
            get
            {
                int rows = 0;
                int count = results.Count;
                for (int i = 0; i < count; i++)
                {
                    // 单查询语句为-1，读写混合语句则忽略掉读的受影响行数
                    if (count > 1 && results[i].RecordsAffected < 0)
                        continue;
                    rows += results[i].RecordsAffected;
                }
                return rows;
            }
        }
        public int RecordsChanged
        {
            get
            {
                int rows = 0;
                int count = results.Count;
                for (int i = 0; i < count; i++)
                    rows += results[i].RecordsChanged;
                return rows;
            }
        }
        int IDataRecord.FieldCount
        {
            get { return results[resultIndex].Columns.Count; }
        }
        SQLResult Result { get { return results[resultIndex]; } }


        object IDataRecord.this[string name]
        {
            get { throw new NotImplementedException(); }
        }
        object IDataRecord.this[int i]
        {
            get { throw new NotImplementedException(); }
        }


        internal MDBReader(IDbCommand command, List<SQLResult> results)
        {
            this.command = command;
            this.results = results;
        }


        void IDataReader.Close()
        {
            Dispose();
        }
        DataTable IDataReader.GetSchemaTable()
        {
            throw new NotImplementedException();
        }
        bool IDataReader.NextResult()
        {
            int next = resultIndex + 1;
            if (next == results.Count)
                return false;
            rowIndex = -1;
            return true;
        }
        bool IDataReader.Read()
        {
            int next = rowIndex + 1;
            if (next < results[resultIndex].TotalRows)
            {
                rowIndex = next;
                return true;
            }
            return false;
        }
        public void Dispose()
        {
            command = null;
            results = null;
            resultIndex = 0;
            rowIndex = -1;
        }
        string IDataRecord.GetName(int i)
        {
            return Result.Columns[i].Name;
        }
        Type IDataRecord.GetFieldType(int i)
        {
            return _MDB.GetType(Result.Columns[i].Type);
        }
        string IDataRecord.GetDataTypeName(int i)
        {
            return Result.Columns[i].Type.ToString();
        }

        bool IDataRecord.GetBoolean(int i)
        {
            throw new NotImplementedException();
        }
        byte IDataRecord.GetByte(int i)
        {
            throw new NotImplementedException();
        }
        long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }
        char IDataRecord.GetChar(int i)
        {
            throw new NotImplementedException();
        }
        long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }
        IDataReader IDataRecord.GetData(int i)
        {
            throw new NotImplementedException();
        }
        DateTime IDataRecord.GetDateTime(int i)
        {
            throw new NotImplementedException();
        }
        decimal IDataRecord.GetDecimal(int i)
        {
            throw new NotImplementedException();
        }
        double IDataRecord.GetDouble(int i)
        {
            throw new NotImplementedException();
        }
        float IDataRecord.GetFloat(int i)
        {
            throw new NotImplementedException();
        }
        Guid IDataRecord.GetGuid(int i)
        {
            throw new NotImplementedException();
        }
        short IDataRecord.GetInt16(int i)
        {
            throw new NotImplementedException();
        }
        int IDataRecord.GetInt32(int i)
        {
            throw new NotImplementedException();
        }
        long IDataRecord.GetInt64(int i)
        {
            throw new NotImplementedException();
        }
        int IDataRecord.GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }
        string IDataRecord.GetString(int i)
        {
            throw new NotImplementedException();
        }
        object IDataRecord.GetValue(int i)
        {
            throw new NotImplementedException();
        }
        int IDataRecord.GetValues(object[] values)
        {
            throw new NotImplementedException();
        }
        bool IDataRecord.IsDBNull(int i)
        {
            throw new NotImplementedException();
        }
    }
    internal abstract class DataTunnle
    {
        public abstract bool IsLocal { get; }
        public abstract List<SQLResult> Execute(SQLCommand command);
    }
    internal class DataTunnleLocal : DataTunnle
    {
        static object executor;
        static MethodInfo execute;

        public override bool IsLocal
        {
            get { return true; }
        }
        public override List<SQLResult> Execute(SQLCommand command)
        {
            if (executor == null)
            {
                Type executorType = Type.GetType("EntryEngine.Database.MDB.MDBManager");
                if (executorType == null)
                    throw new NotSupportedException("未找到类型MDBManager，无法建立本地数据管道");
                executor = executorType.GetField("LocalBridge", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                if (executor == null)
                    throw new NotSupportedException("MDBManager未实例化LocalBridge，无法建立本地数据管道");
                execute = executorType.GetMethod("Execute", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }
            return (List<SQLResult>)execute.Invoke(executor, new object[] { command });
        }
    }
    internal class DataTunnleRemote : DataTunnle
    {
        public override bool IsLocal
        {
            get { return false; }
        }
        public override List<SQLResult> Execute(SQLCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
namespace EntryEngine.Database
{
    public enum EPrimitiveType : byte
    {
        NULL        = 0,
        STRING      = 1,
        DATETIME    = 2,
        BOOL        = 3,
        SBYTE       = 4,
        BYTE        = 5,
        SHORT       = 6,
        USHORT      = 7,
        FLOAT       = 8,
        INT         = 9,
        UINT        = 10,
        LONG        = 11,
        ULONG       = 12,
        DOUBLE      = 13,
    }
    public enum EExecute : byte
    {
        NonQuery = 0,
        Scalar = 1,
        Reader = 2,
    }
    public class SQLCommand
    {
        public string SQL;
        public int Timeout;
        public EExecute ExecuteType;
        public List<SQLParameter> Parameters = new List<SQLParameter>();
        public bool IsLocal;

        private Dictionary<string, SQLParameter> dic;
        public SQLParameter this[string name]
        {
            get
            {
                if (dic == null)
                {
                    dic = new Dictionary<string, SQLParameter>();
                    foreach (var item in Parameters)
                        dic.Add(item.Name, item);
                }
                SQLParameter result;
                dic.TryGetValue(name, out result);
                return result;
            }
        }
    }
    public class SQLParameter : ByteObjectNamed
    {
    }
    public class ByteObject
    {
        public EPrimitiveType Type;
        /// <summary>字节用于传输</summary>
        public byte[] Byte;

        /// <summary>对象数据用于正常操作</summary>
        public object Object { get; set; }
        public bool IsObject { get { return Object != null; } }
        public bool IsByte { get { return Byte != null; } }
        public bool IsDouble { get { return IsObject && IsDouble; } }
        public bool IsArray { get; set; }

        public void ByteToObject()
        {
            if (IsArray)
            {
            }
            else
            {
                Object = _MDB.ToObject(Type, Byte);
            }
        }
        public void ObjectToByte()
        {
            if (IsArray)
            {
            }
            else
            {
                Byte = _MDB.ToBytes(Type, Object);
            }
        }
    }
    public class ByteObjectNamed : ByteObject
    {
        public string Name;
    }
    public class SQLResult
    {
        /// <summary>已更改、插入或删除的行数；如果没有任何行受到影响或语句失败，则为 0；-1 表示 SELECT 语句</summary>
        public int RecordsAffected;
        /// <summary>更新时实际改变了的行数，插入或删除和RecordsAffected一样</summary>
        public int RecordsChanged;
        /// <summary>查询语句结果集的行数</summary>
        public int TotalRows;
        /// <summary>结果视图的列</summary>
        public List<SQLResultColumn> Columns;
        /// <summary>结果数据</summary>
        //public byte[] Data;

        /// <summary>是否已经是缓存的，若已经缓存则不再重复加入缓存</summary>
        public bool Cached { get; set; }
    }
    public class SQLResultColumn : ByteObjectNamed
    {
    }
    public class MDBException : Exception
    {
        public MDBException(string msg, params object[] param) : base(string.Format(msg, param)) { }
    }
    public static class _MDB
    {
        public static Type[] Types = new Type[]
        {
            typeof(DBNull),
            typeof(string),
            typeof(DateTime),
            typeof(bool),
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(double),
        };
        public static Type GetType(EPrimitiveType type)
        {
            return Types[(int)type];
        }
        public static EPrimitiveType GetType(object value)
        {
            if (value == null)
                return EPrimitiveType.NULL;
            else
                return _MDB.GetType(value.GetType());
        }
        public static EPrimitiveType GetType(Type type)
        {
            for (int i = 1; i < Types.Length; i++)
                if (Types[i] == type)
                    return (EPrimitiveType)i;
            throw new NotSupportedException("不支持的数据类型" + type);
        }
        public static int GetPritimiveTypeSizeShift(EPrimitiveType type)
        {
            switch (type)
            {
                case EPrimitiveType.BOOL:
                case EPrimitiveType.SBYTE:
                case EPrimitiveType.BYTE:
                    return 0;
                case EPrimitiveType.SHORT:
                case EPrimitiveType.USHORT:
                    return 1;
                case EPrimitiveType.FLOAT:
                case EPrimitiveType.INT:
                case EPrimitiveType.UINT:
                    return 2;
                case EPrimitiveType.DATETIME:
                case EPrimitiveType.LONG:
                case EPrimitiveType.ULONG:
                case EPrimitiveType.DOUBLE:
                    return 3;
                default:
                    throw new MDBException("未知长度的类型: {0}", type);
            }
        }
        public static int GetPritimiveTypeSize(EPrimitiveType type)
        {
            return GetPritimiveTypeSize(type, -1);
        }
        /// <summary>获取数据长度</summary>
        /// <param name="type">不包含String类型的基础类型</param>
        /// <param name="arraySize">-1. 非数组 / >=0. 数组长度</param>
        /// <returns></returns>
        public static int GetPritimiveTypeSize(EPrimitiveType type, int arraySize)
        {
            int size;
            switch (type)
            {
                case EPrimitiveType.BOOL:
                case EPrimitiveType.SBYTE:
                case EPrimitiveType.BYTE:
                    size = 1;
                    break;
                case EPrimitiveType.SHORT:
                case EPrimitiveType.USHORT:
                    size = 2;
                    break;
                case EPrimitiveType.FLOAT:
                case EPrimitiveType.INT:
                case EPrimitiveType.UINT:
                    size = 4;
                    break;
                case EPrimitiveType.DATETIME:
                case EPrimitiveType.LONG:
                case EPrimitiveType.ULONG:
                case EPrimitiveType.DOUBLE:
                    size = 8;
                    break;
                default:
                    throw new MDBException("未知长度的类型: {0}", type);
            }
            if (arraySize < 0)
                return size;
            else
                return 4 + arraySize * size;
        }
        public static int GetStringSize(object value)
        {
            if (value == null)
                return 1;
            else
                return 1 + (value.ToString().Length << 1);
        }
        /// <summary>第一位1代表不为空，后面的数据用unicode编码</summary>
        public static int GetStringSize(string value)
        {
            if (value == null)
                return 1;
            else
                return 1 + (value.Length << 1);
        }
        public static int GetStringArraySize(object[] value)
        {
            if (value == null)
                return 4;
            else
            {
                int size = 4;
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] == null)
                        size += 4;
                    else
                        size += 4 + (value[i].ToString().Length << 1);
                }
                return size;
            }
        }
        public static int GetStringArraySize(string[] value)
        {
            if (value == null)
                return 4;
            else
            {
                int size = 4;
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] == null)
                        size += 4;
                    else
                        size += 4 + (value[i].Length << 1);
                }
                return size;
            }
        }
        /* 数据转换
         * 1. 前端 -> 后端，参数传输，基础类型，基于内存(byte[])
         * 2. 后端，查询，基础类型数组，基于文件流(byte[] -> 实际类型[])
         * 3. 后端，批量插入，基础类型数组，基于文件流(相同类型直接复制byte[]，short插入int需要byte[]->short[]->int[]->byte[])
         * 4. 后端，数据类型修改，基础类型数组，基于文件流(short插入int需要byte[]->short[]->int[]->byte[])
         * 5. 后端，表达式，常量，基础类型，基于内存(直接object)
         * 6. 后端 -> 前端，结果集传输，基础类型数组，基于内存(实际类型[] -> byte[])
         */
        public static unsafe byte[] ToBytes(EPrimitiveType type, object value)
        {
            if (type == EPrimitiveType.NULL)
                return null;

            int size;
            if (type == EPrimitiveType.STRING)
                size = GetStringSize(value);
            else
                size = GetPritimiveTypeSize(type);

            byte[] buffer = new byte[size];
            fixed (byte* p = buffer)
            {
                switch (type)
                {
                    case EPrimitiveType.STRING:
                        if (value != null)
                        {
                            *p = 1;
                            string str = (string)value;
                            char* c = (char*)(p + 1);
                            for (int i = 0; i < str.Length; i++, c++)
                                *c = str[i];
                        }
                        // else *p = 0 代表字符串为null
                        break;
                    
                    case EPrimitiveType.BOOL:
                        {
                            var v = (bool)value;
                            if (v) *p = 1;
                            else *p = 0;
                        }
                        break;
                    case EPrimitiveType.SBYTE: *(sbyte*)p = (sbyte)value; break;
                    case EPrimitiveType.BYTE: *p = (byte)value; break;
                    case EPrimitiveType.SHORT: *(short*)p = (short)value; break;
                    case EPrimitiveType.USHORT: *(ushort*)p = (ushort)value; break;
                    case EPrimitiveType.FLOAT: *(float*)p = (float)value; break;
                    case EPrimitiveType.INT: *(int*)p = (int)value; break;
                    case EPrimitiveType.UINT: *(uint*)p = (uint)value; break;
                    case EPrimitiveType.DATETIME: *(long*)p = ((DateTime)value).Ticks; break;
                    case EPrimitiveType.LONG: *(long*)p = (long)value; break;
                    case EPrimitiveType.ULONG: *(ulong*)p = (ulong)value; break;
                    case EPrimitiveType.DOUBLE: *(double*)p = (double)value; break;
                    default: throw new MDBException("不支持的数据类型：{0}", type);
                }
            }
            return buffer;
        }
        public static unsafe byte[] ToBytes(EPrimitiveType type, Array array)
        {
            if (type == EPrimitiveType.NULL)
                return null;

            throw new NotImplementedException();
        }
        public static unsafe object ToObject(EPrimitiveType type, byte[] bytes)
        {
            if (type == EPrimitiveType.NULL)
                return null;

            object value;
            fixed (byte* p = bytes)
            {
                switch (type)
                {
                    case EPrimitiveType.STRING:
                        if (*p == 0)
                            value = null;
                        else
                        {
                            int size = (bytes.Length - 1) >> 1;
                            char[] str = new char[size];
                            char* c = (char*)(p + 1);
                            for (int i = 0; i < size; i++, c++)
                                str[i] = *c;
                            value = new string(str);
                        }
                        break;

                    case EPrimitiveType.BOOL: value = *p != 0; break;
                    case EPrimitiveType.SBYTE: value = *(sbyte*)p; break;
                    case EPrimitiveType.BYTE: value = *p; break;
                    case EPrimitiveType.SHORT: value = *(short*)p; break;
                    case EPrimitiveType.USHORT: value = *(ushort*)p; break;
                    case EPrimitiveType.FLOAT: value = *(float*)p; break;
                    case EPrimitiveType.INT: value = *(int*)p; break;
                    case EPrimitiveType.UINT: value = *(uint*)p; break;
                    case EPrimitiveType.DATETIME: value = new DateTime(*(long*)p); break;
                    case EPrimitiveType.LONG: value = *(long*)p; break;
                    case EPrimitiveType.ULONG: value = *(ulong*)p; break;
                    case EPrimitiveType.DOUBLE: value = *(double*)p; break;
                    default: throw new MDBException("不支持的数据类型：{0}", type);
                }
            }
            return value;
        }
        public static unsafe void ToBytes(bool[] array, int length, byte[] buffer)
        {
            fixed (bool* ptr = array)
            {
                fixed (byte* ptr2 = buffer)
                {
                    var ptrS = ptr;
                    var ptrT = (bool*)ptr2;
                    for (int i = 0; i < length; i++)
                    {
                        *ptrT = *ptrS;
                        ptrS++;
                        ptrT++;
                    }
                }
            }
        }
        public static unsafe void ToBytes(sbyte[] array, int length, byte[] buffer)
        {
            fixed (sbyte* ptr = array)
            {
                fixed (byte* ptr2 = buffer)
                {
                    var ptrS = ptr;
                    var ptrT = (sbyte*)ptr2;
                    for (int i = 0; i < length; i++)
                    {
                        *ptrT = *ptrS;
                        ptrS++;
                        ptrT++;
                    }
                }
            }
        }
        public static unsafe void ToBytes(short[] array, int length, byte[] buffer)
        {
            fixed (short* ptr = array)
            {
                fixed (byte* ptr2 = buffer)
                {
                    var ptrS = ptr;
                    var ptrT = (short*)ptr2;
                    for (int i = 0; i < length; i++)
                    {
                        *ptrT = *ptrS;
                        ptrS++;
                        ptrT++;
                    }
                }
            }
        }
        public static unsafe void ToBytes(ushort[] array, int length, byte[] buffer)
        {
            fixed (ushort* ptr = array)
            {
                fixed (byte* ptr2 = buffer)
                {
                    var ptrS = ptr;
                    var ptrT = (ushort*)ptr2;
                    for (int i = 0; i < length; i++)
                    {
                        *ptrT = *ptrS;
                        ptrS++;
                        ptrT++;
                    }
                }
            }
        }
        public static unsafe void ToBytes(float[] array, int length, byte[] buffer)
        {
            fixed (float* ptr = array)
            {
                fixed (byte* ptr2 = buffer)
                {
                    var ptrS = ptr;
                    var ptrT = (float*)ptr2;
                    for (int i = 0; i < length; i++)
                    {
                        *ptrT = *ptrS;
                        ptrS++;
                        ptrT++;
                    }
                }
            }
        }
        public static unsafe void ToBytes(int[] array, int length, byte[] buffer)
        {
            fixed (int* ptr = array)
            {
                fixed (byte* ptr2 = buffer)
                {
                    var ptrS = ptr;
                    var ptrT = (int*)ptr2;
                    for (int i = 0; i < length; i++)
                    {
                        *ptrT = *ptrS;
                        ptrS++;
                        ptrT++;
                    }
                }
            }
        }
        public static unsafe void ToBytes(uint[] array, int length, byte[] buffer)
        {
            fixed (uint* ptr = array)
            {
                fixed (byte* ptr2 = buffer)
                {
                    var ptrS = ptr;
                    var ptrT = (uint*)ptr2;
                    for (int i = 0; i < length; i++)
                    {
                        *ptrT = *ptrS;
                        ptrS++;
                        ptrT++;
                    }
                }
            }
        }
        public static unsafe void ToBytes(long[] array, int length, byte[] buffer)
        {
            fixed (long* ptr = array)
            {
                fixed (byte* ptr2 = buffer)
                {
                    var ptrS = ptr;
                    var ptrT = (long*)ptr2;
                    for (int i = 0; i < length; i++)
                    {
                        *ptrT = *ptrS;
                        ptrS++;
                        ptrT++;
                    }
                }
            }
        }
        public static unsafe void ToBytes(ulong[] array, int length, byte[] buffer)
        {
            fixed (ulong* ptr = array)
            {
                fixed (byte* ptr2 = buffer)
                {
                    var ptrS = ptr;
                    var ptrT = (ulong*)ptr2;
                    for (int i = 0; i < length; i++)
                    {
                        *ptrT = *ptrS;
                        ptrS++;
                        ptrT++;
                    }
                }
            }
        }
        public static unsafe void ToBytes(double[] array, int length, byte[] buffer)
        {
            fixed (double* ptr = array)
            {
                fixed (byte* ptr2 = buffer)
                {
                    var ptrS = ptr;
                    var ptrT = (double*)ptr2;
                    for (int i = 0; i < length; i++)
                    {
                        *ptrT = *ptrS;
                        ptrS++;
                        ptrT++;
                    }
                }
            }
        }
        public static unsafe void ToBytes(DateTime[] array, int length, byte[] buffer)
        {
            fixed (DateTime* ptr = array)
            {
                fixed (byte* ptr2 = buffer)
                {
                    var ptrS = ptr;
                    var ptrT = (long*)ptr2;
                    for (int i = 0; i < length; i++)
                    {
                        *ptrT = ptrS->Ticks;
                        ptrS++;
                        ptrT++;
                    }
                }
            }
        }
    }
}
namespace EntryEngine.Database.MDB
{
    using EntryEngine.Database.MDB.Syntax;
    using EntryEngine.Network;
    using EntryEngine.Database.Refactoring;
    using EntryEngine.Database.Refactoring.MySQL;
    using EntryEngine.Database.MDB.Semantics;

    #region 数据库执行

    public class MDBManager : IDisposable
    {
        private MDBDatabase database;
        public MDBDatabase Database
        {
            get { return database; }
        }

        /// <summary>抛出给DataTunnleLocal使用的实例</summary>
        static MDBManager LocalBridge;

        public MDBManager()
        {
            LocalBridge = this;
        }

        public void SetDatabase(string dbname)
        {
            if (string.IsNullOrEmpty(dbname))
                throw new ArgumentNullException("数据库名不能为空");

            if (database != null)
                throw new InvalidOperationException("数据库已设置，不能修改");

            database = new MDBDatabase();
            database.Name = dbname;
        }
        void CheckDB()
        {
            if (database == null)
                throw new InvalidOperationException("请先调用SetDatabase设置数据库名");
        }
        public bool DBExists()
        {
            CheckDB();
            return Directory.Exists(database.Name);
        }
        /// <summary>加载数据库</summary>
        public void DBLoad()
        {
            CheckDB();

            if (database.Exists())
            {
                _LOG.Info("加载数据库：{0}", database);
                database.LoadTable();
            }
            else
            {
                Directory.CreateDirectory(database.Name);
                _LOG.Info("创建数据库：{0}", database);
            }

            OnDBInitialize();
        }
        protected virtual void OnDBInitialize() { }
        public void Dispose()
        {
            database = null;
        }

        internal List<SQLResult> Execute(SQLCommand command)
        {
            return SyntaxExecutor.Execute(database, command);
        }
    }

    #endregion
    
    /// <summary>数据库服务</summary>
    internal class MDBService : ProxyTcpAsync
    {
        private List<MDBAccount> accounts = new List<MDBAccount>();
        private Dictionary<string, MDBDatabase> databases = new Dictionary<string, MDBDatabase>();
        public void CreateAccount(string name, string password)
        {
            if (accounts.Any(a => a.Name == name))
                throw new ArgumentException("已经存在了同名账号");
            MDBAccount account = new MDBAccount();
            account.Name = name;
            account.Password = password;
            accounts.Add(account);
        }
        public void CreateDatabase(string dbname)
        {
            if (string.IsNullOrEmpty(dbname))
                throw new ArgumentNullException("数据库名字不能为空");
            if (databases.ContainsKey(dbname))
                throw new ArgumentException("已经存在了同名数据库");
            MDBDatabase db = new MDBDatabase();
            db.Name = dbname;
            Directory.CreateDirectory(dbname);
            databases.Add(dbname, db);
        }
        public bool ExistsDatabase(string dbname)
        {
            return databases.ContainsKey(dbname);
        }
        protected override IEnumerator<LoginResult> Login(Link link)
        {
            LoginResult result = new LoginResult();
            while (true)
            {
                byte[] buffer = link.Read();
                if (buffer == null)
                    continue;
                ByteReader reader = new ByteReader(buffer);
                string user, password;
                reader.Read(out user);
                reader.Read(out password);
                // todo: 检查用户的可用性
                break;
            }
            result.Result = EAcceptPermit.Permit;
            yield return result;
        }
        protected override void OnUpdate(GameTime time)
        {
        }
    }
    /// <summary>数据库服务管理权限</summary>
    [Flags]
    public enum EAdministrativeRole
    {
        DBA = 0,
    }
    /// <summary>数据库操作权限</summary>
    [Flags]
    public enum EPrevilege
    {
        ALL = 0,
        //SELECT = 1,
        //INSERT = 2,
        //UPDATE = 4,
        //DELETE = 8,
    }
    /// <summary>数据库管理员账号</summary>
    public class MDBAccount
    {
        public string Name;
        public string Password;
        public EAdministrativeRole Role;
        public EPrevilege Previlege;
    }
    /// <summary>数据库</summary>
    public class MDBDatabase
    {
        /// <summary>数据库名</summary>
        public string Name;
        public Dictionary<string, MDBTable> Tables = new Dictionary<string, MDBTable>();

        public string StorageDirectory
        {
            get { return Name + "/"; }
        }

        void Check()
        {
            if (string.IsNullOrEmpty(Name))
                throw new InvalidOperationException("请先设置数据库名字");
        }
        public bool Exists()
        {
            Check();
            return Directory.Exists(Name);
        }
        public void Create()
        {
            Check();
            Directory.CreateDirectory(Name);
            _LOG.Info("创建数据库：{0}", Name);
        }
        public bool ExistsTable(string name)
        {
            return Tables.ContainsKey(name.ToLower());
        }
        public bool ExistsTable(string name, out MDBTable table)
        {
            return Tables.TryGetValue(name.ToLower(), out table);
        }
        public MDBTable CreateTable(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("数据表名字不能为空");
            if (Tables.ContainsKey(name))
                throw new ArgumentException("已经存在了同名数据表");
            MDBTable create = new MDBTable();
            create.Name = name;
            create.Database = this;
            Directory.CreateDirectory(create.StorageDirectory);
            Tables.Add(name.ToLower(), create);
            _LOG.Info("创建数据表：{0}", name);
            create.Save();
            return create;
        }
        public void LoadTable()
        {
            foreach (var item in Directory.GetFiles(StorageDirectory, "*." + MDBTable.SUFFIX))
            {
                MDBTable table = MDBTable.Load(item);
                table.Database = this;
                Tables.Add(table.Name.ToLower(), table);
            }
            _LOG.Info("加载数据库{0}完成", Name);
        }
    }
    /// <summary>数据表</summary>
    public class MDBTable
    {
        /// <summary>记录了数据表内容的文件后缀</summary>
        public const string SUFFIX = "mdbt";

        /// <summary>表名</summary>
        public string Name;
        /// <summary>备注</summary>
        public string Comments;

        /// <summary>表中现存数据的行数（不包括被删除的）</summary>
        public int Count;
        /// <summary>表中总共数据的行数（包括被删除的）</summary>
        public int Row;

        private Dictionary<string, int> _columns = new Dictionary<string, int>();
        public List<MDBColumn> Columns = new List<MDBColumn>();
        public List<MDBIndex> Indexes = new List<MDBIndex>();

        public string StorageDirectory
        {
            get { return string.Format("{0}{1}/", Database.StorageDirectory, Name); }
        }
        public string StorageFile
        {
            get { return string.Format("{0}{1}.{2}", Database.StorageDirectory, Name, SUFFIX); }
        }
        public MDBDatabase Database { get; internal set; }
        public MDBColumn this[int columnIndex] { get { return Columns[columnIndex]; } }
        public MDBColumn this[string columnName]
        {
            get
            {
                int index;
                if (!_columns.TryGetValue(columnName.ToLower(), out index))
                    return null;
                return Columns[index];
            }
        }

        public bool TryGetColumn(string key, out MDBColumn column)
        {
            column = null;
            int index;
            if (_columns.TryGetValue(key.ToLower(), out index))
            {
                column = Columns[index];
                return true;
            }
            else
                return false;
        }
        public void Drop()
        {
            File.Delete(StorageFile);
            Directory.Delete(StorageDirectory, true);
            _LOG.Info("删除表：{0}", Name);
        }
        public void DropColumn(MDBColumn column)
        {
            int index = Columns.IndexOf(column);
            if (index == -1)
                throw new MDBException("表[{0}]未能找到要删除的列：{1}", Name, column.Name);
            if (column.Index != null)
            {
                if (column.Index.Type == EIndex.Primary)
                {
                    // todo: 若另有Primary键，删除一列主键列可能会导致另一列主键列出现重复数据，此时需先删除主键索引
                }
                column.Index.Drop();
            }
            //Directory.Delete(StorageDirectory + column.Name, true);
            File.Delete(StorageDirectory + column.Name);
            _LOG.Info("删除列：{0}", column.Name);
            Save();
        }
        public MDBColumn AddColumn(string name, EPrimitiveType type)
        {
            if (type == EPrimitiveType.NULL)
                throw new ArgumentException("列类型不能为空");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("列名不能为空");
            if (_columns.ContainsKey(name))
                throw new ArgumentException("已经存在了相同列");
            MDBColumn create = new MDBColumn();
            create.Name = name;
            create.Type = type;
            _columns.Add(name.ToLower(), Columns.Count);
            Columns.Add(create);
            Save();
            return create;
        }
        public MDBIndex AddIndex(EIndex type, params string[] name)
        {
            throw new NotImplementedException();
        }
        public bool Exists()
        {
            return File.Exists(StorageFile);
        }
        public void Save()
        {
            _IO.WriteText(StorageFile, JsonWriter.Serialize(this));
            _LOG.Info("保存数据表：{0}", Name);
        }
        public static MDBTable Load(string file)
        {
            var table = JsonReader.Deserialize<MDBTable>(_IO.ReadText(file));
            for (int i = 0; i < table.Columns.Count; i++)
            {
                table.Columns[i].OpenFile();
                table._columns.Add(table.Columns[i].Name.ToLower(), i);
            }
            _LOG.Info("加载数据表：{0}", table.Name);
            return table;
        }
    }
    /// <summary>数据表的列</summary>
    public class MDBColumn : IDisposable
    {
        /// <summary>列名</summary>
        public string Name;
        /// <summary>备注</summary>
        public string Comments;
        /// <summary>字段类型</summary>
        public EPrimitiveType Type;
        /// <summary>默认值的表达式</summary>
        public string Default;
        /// <summary>索引，复合索引按照多个单索引处理</summary>
        public MDBIndex Index;

        internal MDBTable Table;
        internal DataWriterFile Writer;
        internal DataReaderFile Reader;

        public string StorageFile
        {
            get { return Table.StorageDirectory + Name; }
        }

        public void SetPrimitiveType(EPrimitiveType type)
        {
            if (type == EPrimitiveType.NULL)
                throw new ArgumentException("数据类型不能为空");

            if (Type == EPrimitiveType.NULL)
            {
                Type = type;
            }
            else
            {
                if (Type != type)
                {
                    // 修改数据类型时，使用旧类型读取旧文件，使用新类型，写入新文件，最后新文件覆盖旧文件
                    // 例如int数据改为long时，将原本1G的文件读取出来，写入2G的文件
                    byte[] readBuffer = new byte[1024 * 1024 * 4];
                    int sizeOrigin = _MDB.GetPritimiveTypeSize(Type);
                    int sizeCurrent = _MDB.GetPritimiveTypeSize(type);
                    byte[] copyBuffer = new byte[readBuffer.Length * sizeOrigin / sizeCurrent];
                    using (FileStream file = new FileStream(StorageFile, FileMode.Open))
                    {
                        unsafe
                        {
                            fixed (byte* ptr = readBuffer)
                            {
                                fixed (byte* ptr2 = copyBuffer)
                                {
                                    int read;
                                    while (file.Position != file.Length)
                                    {
                                        read = file.Read(readBuffer, 0, readBuffer.Length);
                                        // todo: 更换数据类型
                                        throw new NotImplementedException();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            OpenFile();
        }
        internal void OpenFile()
        {
            //Writer = new DataWriterFile(this);
            //Reader = new DataReaderFile(this);
        }
        public void Dispose()
        {
            if (Writer != null)
            {
                Writer.Dispose();
                Writer = null;
            }
            if (Reader != null)
            {
                Reader.Dispose();
                Reader = null;
            }
        }
    }
    /// <summary>数据表的索引</summary>
    public class MDBIndex
    {
        /// <summary>索引类型</summary>
        public EIndex Type;
        /// <summary>自增键当前的值</summary>
        public int Identity = 1;

        internal MDBColumn Column;
        public bool IsIdentity { get { return Type == EIndex.Identity; } }
        public bool IsPrimary { get { return Type == EIndex.Identity || Type == EIndex.Primary; } }

        internal void OpenFile()
        {
            /*
             * 1. Identity
             * 平衡树，元素不重复，顺序存储，携带行号
             * 
             * 2. Primary
             */
            throw new NotImplementedException();
        }
        public void Drop()
        {
            throw new NotImplementedException();
        }
    }

    internal abstract class DataFile : IDisposable
    {
        public MDBColumn Column;
        public FileStream File;
        internal byte[] buffer = new byte[4096];
        internal int index;
        protected int shift;

        /// <summary>文件流当前行号</summary>
        public int RowIndex
        {
            get { return (int)(File.Position >> shift); }
            set
            {
                int p = value << shift;
                if (p != File.Position)
                    File.Seek(p, SeekOrigin.Begin);
            }
        }
        /// <summary>数据总行数</summary>
        public int Count
        {
            get { return (int)(File.Length >> shift); }
        }

        public void SetBufferCapcity(int capcity)
        {
            if (index > 0)
                throw new MDBException("数据流中仍有数据，不能设置缓冲长度");
            buffer = new byte[capcity];
        }
        public void SetColumn(MDBColumn column)
        {
            this.File = new FileStream(column.StorageFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            this.Column = column;
            shift = _MDB.GetPritimiveTypeSizeShift(column.Type);
        }
        public void Dispose()
        {
            if (File != null)
            {
                File.Close();
                File = null;
            }
        }
    }
    /// <summary>
    /// 文件流效率测试(FileStream.Seek)，测试类型int
    /// 1. Seek指定位置，仅读取需要的数据
    ///    (1). 占用内存小
    ///    (2). 文件流顺序读取时速度较快，约为随机位置读取的1.25~6倍(根据)，10000次随机位置读取时间约200ms
    ///         顺序情况下，SeekOrigin.Begin和SeekOrigin.Current效果一样，对需要Seek的位置进行顺序排序，10000次随机位置读取时间约160ms
    /// 2. Seek起始位置，读取直到结束位置中的所有数据（有不需要数据）
    ///    (1). 临时占用内存大
    ///    (2). 相同时间，可以读取的数据量为前者的2000倍，即200ms可以读取2000万条数据
    /// 所以需要根据查询数据的耗时来选择性采用读取方式
    /// </summary>
    internal abstract class DataWriterFile : DataFile
    {
        public void SeekLast()
        {
            if (File.Length != File.Position)
            {
                File.Position = File.Length;
            }
        }
        public void Flush()
        {
            File.Write(buffer, 0, index);
            File.Flush();
            index = 0;
        }
    }
    internal abstract class DataReaderFile : DataFile
    {
    }

    /// <summary>索引的特性</summary>
    [Flags]
    public enum EIndexAttribute
    {
        /// <summary>索引值唯一</summary>
        Unique = 1,
        /// <summary>字符串长度固定，例如手机号</summary>
        Fixed = 2,
    }
    public class Indexable<T> where T : class
    {
        public T Data;
        /// <summary>
        /// 使用索引时，需要做一个比较复杂度排序
        /// <para>例如一个Time > '2020-08-24' OR Name LIKE '李%'的筛选</para>
        /// <para>Time和Name都有索引</para>
        /// <para>此时'>'和'LIKE'，比较复杂度应该是'>'较小，优先选用Time索引筛选数据</para>
        /// <para>Time索引筛选到的数据，Hit置为true</para>
        /// <para>之后筛选Name时，已经Hit的对象就不需要做LIKE判断了，可以加快筛选速度</para>
        /// <para>其次就是AND和OR的区别</para>
        /// <para>本例中如果是AND条件，可以通过!Hit跳过LIKE判断</para>
        /// </summary>
        public bool Hit;
    }
    public abstract class Index<T> where T : class
    {
        internal List<Indexable<T>> Datas;

        public abstract void Insert(T data);
        public abstract void Update(T data);
        public abstract void Delete(T data);

        public abstract void Fixed(int value);
        public void In(IEnumerable<int> values)
        {
            foreach (var item in values)
                Fixed(item);
        }
        public abstract void Range(int start, int end);

        public abstract void Fixed(long value);
        public void In(IEnumerable<long> values)
        {
            foreach (var item in values)
                Fixed(item);
        }
        public abstract void Range(long start, long end);

        public abstract void Fixed(string value);
        public void In(IEnumerable<string> values)
        {
            foreach (var item in values)
                Fixed(item);
        }

        public abstract void Fixed(bool value);

        public abstract void Like(string find);
    }
    /// <summary>基于数字排序的索引</summary>
    //public class IndexNumber<T> : Index<T> where T : class
    //{
    //}
    ///// <summary>基于键的索引</summary>
    //public class IndexDictionary<T> : Index<T> where T : class
    //{
    //}
    ///// <summary>基于字符长度的索引</summary>
    //public class IndexLength<T> : Index<T> where T : class
    //{
    //}
}
namespace EntryEngine.Database.MDB.Syntax
{
    public class InvalidSyntaxException : Exception
    {
        public InvalidSyntaxException(string message) : base(message) { }
    }
    public abstract class SyntaxNode : IEquatable<SyntaxNode>
    {
        /// <summary>检查语法错误</summary>
        /// <param name="__throw">true则抛出异常</param>
        /// <param name="message">异常信息</param>
        public static void CheckSyntax(bool __throw, string message)
        {
            if (__throw)
                throw new InvalidSyntaxException(message);
        }

        /// <summary>用于将语法树找到执行器缓存</summary>
        public virtual bool Equals(SyntaxNode other) { return this == other; }
    }

    #region Insert

    /// <summary>
    /// Insert语句
    /// <para>INSERT [INTO] TABLE[(Field1, Field2... Field)] VALUES</para>
    /// <para>INSERT [INTO] TABLE[(Field1, Field2... Field)] SELECT</para>
    /// </summary>
    public class INSERT : SyntaxNode
    {
        /// <summary>REPLACE INTO</summary>
        public bool IsReplace;
        /// <summary>要插入的数据表</summary>
        public TABLE Table;
        /// <summary>要插入的列名，空集合则全部插入，考虑将字段移入Table中</summary>
        public List<FIELD> Fields = new List<FIELD>();
        // todo: values(1,2,3),(2,3,4),(3,4,5)可以插入多行
        /// <summary>要插入的值</summary>
        public List<SyntaxNode> Values = new List<SyntaxNode>();
        /// <summary>通过查询已有表插入多条数据</summary>
        public bool IsSelect
        {
            get { return Values.Count == 1 && SELECT.IsSelect(Values[0]); }
        }
        public bool IsAllFields
        {
            get { return Fields.Count == 0; }
        }
        public bool TryGetSelect(out SELECT select)
        {
            select = null;
            if (Values.Count != 1)
                return false;
            return SELECT.IsSelect(Values[0], out select);
        }
        public void CheckSyntax()
        {
            CheckSyntax(Table == null, "缺少插入的表名");
            CheckSyntax(string.IsNullOrEmpty(Table.Name), "插入的表名不能为空");
        }

        public override bool Equals(SyntaxNode other)
        {
            if (base.Equals(other))
                return true;
            INSERT insert = other as INSERT;
            if (insert == null)
                return false;
            if (IsReplace != insert.IsReplace)
                return false;
            if (Table.Name != insert.Table.Name)
                return false;
            if (Fields.Count != insert.Fields.Count)
                return false;
            for (int i = 0; i < Fields.Count; i++)
                if (Fields[i].Name != insert.Fields[i].Name)
                    return false;
            // 值可以单独重算执行器
            return true;
        }
    }

    #endregion

    #region Delete

    public class DELETE : SyntaxNode
    {
    }
    /// <summary>TRUNCATE [TABLE] TABLE</summary>
    public class TRUNCATE : SyntaxNode
    {
        public TABLE Table;
    }

    #endregion

    #region Update

    public class UPDATE : SyntaxNode
    {
    }

    #endregion

    #region Select

    public class SELECT : SyntaxNode
    {
        /// <summary>查询的 常量，函数，[表名].[字段名]|*，子查询 等等</summary>
        public List<SyntaxNode> Values = new List<SyntaxNode>();
        /// <summary>查询的表，可能是join，也可能没有(例如select 1,2)</summary>
        public SyntaxNode From;
        /// <summary>where条件，可能没有</summary>
        public SyntaxNode Where;
        /// <summary>group by字段，可能没有</summary>
        public List<FieldWithTable> GroupBy = new List<FieldWithTable>();
        /// <summary>having条件，可能没有（可以不需要group by单独存在）</summary>
        public SyntaxNode Having;
        /// <summary>group by字段，可能没有</summary>
        public List<ORDERBY> OrderBy = new List<ORDERBY>();
        /// <summary>分页，可能没有</summary>
        public LIMIT Limit;
        /// <summary>union查询，可能没有</summary>
        public UNION Union;
        /// <summary>子查询的别名，可能没有</summary>
        //public AS Alias;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("SELECT ");
            for (int i = 0, c = Values.Count - 1; i <= c; i++)
            {
                builder.Append(Values[i]);
                if (i != c)
                    builder.Append(", ");
            }
            if (From != null)
            {
                builder.Append(" FROM ");
                builder.Append(From);
            }
            if (Where != null)
            {
                builder.Append(" WHERE ");
                builder.Append(Where);
            }
            if (GroupBy.Count > 0)
            {
                builder.Append(" Group BY ");
                for (int i = 0, c = GroupBy.Count - 1; i <= c; i++)
                {
                    builder.Append(GroupBy[i]);
                    if (i != c)
                        builder.Append(", ");
                }
            }
            if (Having != null)
            {
                builder.Append(" HAVING ");
                builder.Append(Having);
            }
            if (OrderBy.Count > 0)
            {
                builder.Append(" ORDER BY ");
                for (int i = 0, c = OrderBy.Count - 1; i <= c; i++)
                {
                    builder.Append(OrderBy[i]);
                    if (i != c)
                        builder.Append(", ");
                }
            }
            if (Limit != null)
            {
                builder.AppendLine();
                builder.Append(Limit);
            }
            if (Union != null)
            {
                builder.AppendLine();
                builder.Append(Union);
            }
            return builder.ToString();
        }
        public override bool Equals(SyntaxNode other)
        {
            if (base.Equals(other))
                return true;
            SELECT select = other as SELECT;
            if (select == null)
                return false;
            if (Values.Count != select.Values.Count)
                return false;
            for (int i = 0; i < Values.Count; i++)
                if (!Values[i].Equals(select.Values[i]))
                    return false;
            if ((From == null || select.From == null) && From != select.From)
                return false;
            if (From != null && !From.Equals(select.From))
                return false;
            if ((Where == null || select.Where == null) && Where != select.Where)
                return false;
            if (Where != null && !Where.Equals(select.Where))
                return false;
            if (GroupBy.Count != select.GroupBy.Count)
                return false;
            for (int i = 0; i < GroupBy.Count; i++)
                if (!GroupBy[i].Equals(select.GroupBy[i]))
                    return false;
            if ((Having == null || select.Having == null) & Having != select.Having)
                return false;
            if (Having != null && !Having.Equals(select.Having))
                return false;
            // order by & limit 不参与缓存，能极大加速排序和翻页
            if ((Union == null || select.Union == null) && Union != select.Union)
                return false;
            if (Union != null && !Union.Equals(select.Union))
                return false;
            return true;
        }

        /// <summary>判断语法是不是查询或子查询</summary>
        public static bool IsSelect(SyntaxNode node)
        {
            if (node is SELECT) return true;
            Parenthesized p = node as Parenthesized;
            if (p != null) return IsSelect(p.Expression);
            return false;
        }
        public static bool IsSelect(SyntaxNode node, out SELECT select)
        {
            select = node as SELECT;
            if (select != null) return true;
            Parenthesized p = node as Parenthesized;
            if (p != null) return IsSelect(p.Expression, out select);
            return false;
        }
    }
    public enum EJoin
    {
        LEFT,
        RIGHT,
        INNER,
    }
    public class JOIN : SyntaxNode
    {
        public EJoin Join;
        public SyntaxNode Left;
        public SyntaxNode Right;
        /// <summary>
        /// on条件，inner join时可以为空，为空时左表*右表
        /// <para>左右连都不能为空，可以是on(true)，此时速度极慢</para>
        /// <para>inner join做笛卡尔乘积时，左表先显示全部行，右表显示第一行</para>
        /// </summary>
        public SyntaxNode On;

        public JOIN() { }
        public JOIN(EJoin join) { this.Join = join; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{0} {1} JOIN {2}", Left, Join, Right);
            if (On != null)
                builder.Append(" ON {0}", On);
            return builder.ToString();
        }
    }
    public class ORDERBY : WithExpressionExpression
    {
        /// <summary>排序顺序，true降序，false升序</summary>
        public bool DESC;
        public override string ToString()
        {
            return string.Format("{0} {1}", Expression, DESC ? "DESC" : "ASC");
        }
    }
    public class LIMIT : SyntaxNode
    {
        /// <summary>第几条数据开始</summary>
        public SyntaxNode Start;
        /// <summary>查询几条数据</summary>
        public SyntaxNode Count;
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("LIMIT ");
            if (Start != null)
            {
                builder.Append(Start);
                builder.Append(',');
            }
            builder.Append(Count);
            return builder.ToString();
        }

        /// <summary>检查语法节点是否是合法的limit参数</summary>
        public static void CheckSyntax(SyntaxNode node)
        {
            if (!(node is FieldParameter))
            {
                SyntaxNode.CheckSyntax(!(node is PrimitiveValue), "limit后只能是参数或常量值");
                EPrimitiveType type = ((PrimitiveValue)node).Type;
                SyntaxNode.CheckSyntax(type != EPrimitiveType.INT && type != EPrimitiveType.LONG, "limit后应该是整数");
            }
        }
    }
    public class UNION : SyntaxNode
    {
        /// <summary>
        /// 是否是union all，否的化会去重全部一样的数据
        /// <para>1 union all 1之后会有2行1，但是再union 2之后，两个1任然会被去重</para>
        /// </summary>
        public bool IsUnionAll;
        /// <summary>union的子查询</summary>
        public SyntaxNode Select;
        public override string ToString()
        {
            if (IsUnionAll)
                return string.Format("UNION ALL {0}", Select);
            else
                return string.Format("UNION {0}", Select);
        }

        /// <summary>检查语法节点是否是合法Select语句(带limit的查询语句在union时需要加上'()')</summary>
        public static void CheckSyntax(SyntaxNode node)
        {
            if (!(node is SELECT))
            {
                SyntaxNode.CheckSyntax(!(node is Parenthesized), "union后只能是查询或子查询语句");
                CheckSyntax((Parenthesized)node);
            }
        }
    }

    #endregion

    #region Create Drop Alter

    #endregion

    #region Expression

    public abstract class WithExpressionExpression : SyntaxNode
    {
        public SyntaxNode Expression;
    }

    /// <summary>常量值</summary>
    public class PrimitiveValue : SyntaxNode
    {
        public object Value;
        public EPrimitiveType Type
        {
            get
            {
                if (Value == null)
                    return EPrimitiveType.NULL;
                else if (Value is string)
                    return EPrimitiveType.STRING;
                else if (Value is DateTime)
                    return EPrimitiveType.DATETIME;
                else if (Value is bool)
                    return EPrimitiveType.BOOL;
                else if (Value is sbyte)
                    return EPrimitiveType.SBYTE;
                else if (Value is byte)
                    return EPrimitiveType.BYTE;
                else if (Value is short)
                    return EPrimitiveType.SHORT;
                else if (Value is ushort)
                    return EPrimitiveType.USHORT;
                else if (Value is float)
                    return EPrimitiveType.FLOAT;
                else if (Value is int)
                    return EPrimitiveType.INT;
                else if (Value is uint)
                    return EPrimitiveType.UINT;
                else if (Value is long)
                    return EPrimitiveType.LONG;
                else if (Value is ulong)
                    return EPrimitiveType.ULONG;
                else if (Value is double)
                    return EPrimitiveType.DOUBLE;
                else if (Value is Enum)
                {
                    Type type = Enum.GetUnderlyingType(Value.GetType());
                    if (type == typeof(sbyte))
                        return EPrimitiveType.SBYTE;
                    else if (type == typeof(byte))
                        return EPrimitiveType.BYTE;
                    else if (type == typeof(short))
                        return EPrimitiveType.SHORT;
                    else if (type == typeof(ushort))
                        return EPrimitiveType.USHORT;
                    else if (type == typeof(int))
                        return EPrimitiveType.INT;
                    else if (type == typeof(uint))
                        return EPrimitiveType.UINT;
                    else if (type == typeof(float))
                        return EPrimitiveType.FLOAT;
                    else if (type == typeof(long))
                        return EPrimitiveType.LONG;
                    else if (type == typeof(ulong))
                        return EPrimitiveType.ULONG;
                    else if (type == typeof(double))
                        return EPrimitiveType.DOUBLE;
                }
                throw new InvalidCastException("未知的常量数据类型：" + Value.GetType());
            }
        }
        public PrimitiveValue() { }
        public PrimitiveValue(object value) { this.Value = value; }
        public override string ToString()
        {
            EPrimitiveType type = Type;
            switch (type)
            {
                case EPrimitiveType.NULL: return "NULL";
                case EPrimitiveType.STRING: return string.Format("'{0}'", Value);
                case EPrimitiveType.DATETIME: return string.Format("'{0}'", ((DateTime)Value).ToString("yyyy-MM-dd HH:mm:ss"));
                default: return Value.ToString();
            }
        }
    }

    // 关键字 前运算符
    public enum EUnaryOperator
    {
        /// <summary>!a</summary>
        Not = 1,
        /// <summary>~a</summary>
        BitNot,
        /// <summary>-a</summary>
        Minus,
        /// <summary>+a</summary>
        Plus,
    }
    /// <summary>一元运算符</summary>
    public class UnaryOperator : WithExpressionExpression
    {
        public static Dictionary<EUnaryOperator, string> Symbols = new Dictionary<EUnaryOperator, string>();
        static UnaryOperator()
        {
            Symbols[EUnaryOperator.Not] = "!";
            //UnarySymbol[EUnaryOperator.Not] = "not ";
            Symbols[EUnaryOperator.BitNot] = "~";
            Symbols[EUnaryOperator.Minus] = "-";
            Symbols[EUnaryOperator.Plus] = "+";
        }

        public EUnaryOperator Operator;
        public override string ToString()
        {
            return Symbols[Operator] + Expression;
        }
    }
    /// <summary>()</summary>
    public class Parenthesized : WithExpressionExpression
    {
        public bool IsChildSelect { get { return SELECT.IsSelect(Expression); } }
        public override string ToString()
        {
            return string.Format("({0})", Expression);
        }
    }
    /// <summary>between and</summary>
    public class BETWEEN : WithExpressionExpression
    {
        public SyntaxNode Between;
        public SyntaxNode And;
        public override string ToString()
        {
            return string.Format("{0} BETWEEN {1} AND {2}", Expression, Between, And);
        }
    }
    /// <summary>
    /// 相当于switch的case，用法有以下两种
    /// <para>用法1(switch): case 表达式 when 表达式的值 then 结果值</para>
    /// <para>用法2(if)：case when 表达式 then 结果值</para>
    /// </summary>
    public class CASE : WithExpressionExpression
    {
        public List<WHEN> Cases = new List<WHEN>();
        public bool IsSwitchMode { get { return Expression != null; } }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("CASE ");
            if (Expression != null)
                builder.Append(Expression);
            for (int i = 0, c = Cases.Count - 1; i <= c; i++)
            {
                builder.Append(" {0}", Cases[i]);
                if (i == c && !Cases[i].IsElse)
                    builder.Append(" END");
            }
            return builder.ToString();
        }
    }
    public class WHEN : WithExpressionExpression
    {
        public SyntaxNode Then;
        public bool IsElse { get { return Expression == null; } }
        public override string ToString()
        {
            if (IsElse)
                return string.Format("ELSE {0} END", Then);
            else
                return string.Format("WHEN {0} THEN {1}", Expression, Then);
        }
    }
    //public class FunctionP1 : WithExpressionExpression
    //{
    //    public override string ToString()
    //    {
    //        return string.Format("{0}({1})", GetType().Name, Expression);
    //    }
    //}
    public class EXISTS : WithExpressionExpression
    {
        public void CheckSyntax()
        {
            SyntaxNode.CheckSyntax(!SELECT.IsSelect(Expression), "exists后的表达式必须是查询");
        }
        public override string ToString()
        {
            return string.Format("EXISTS({0})", Expression);
        }
    }
    /// <summary>distinct exp | distinct(exp)</summary>
    public class DISTINCT : WithExpressionExpression
    {
        public override string ToString()
        {
            return string.Format("DISTINCT({0})", Expression);
        }
    }
    public class FunctionPN : SyntaxNode
    {
        /// <summary>数据库内置方法，key为方法名，value是方法的参数个数，例如sum，就是key=sum,value=1</summary>
        public static Dictionary<string, int> Functions = new Dictionary<string, int>();
        static FunctionPN()
        {
            // exists (子查询)
            Functions["exists"] = 1;
            Functions["sum"] = 1;
            Functions["avg"] = 1;
            Functions["count"] = 1;
            Functions["max"] = 1;
            Functions["min"] = 1;

            // IF (表达式, true返回值, false返回值)
            Functions["if"] = 3;
            Functions["substring_index"] = 3;
            Functions["find_in_set"] = 3;
        }

        /// <summary>方法名</summary>
        public string Function;
        public List<SyntaxNode> Parameters = new List<SyntaxNode>();
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Function);
            builder.Append('(');
            for (int i = 0, c = Parameters.Count - 1; i <= c; i++)
            {
                builder.Append(Parameters[i]);
                if (i != c)
                    builder.Append(", ");
            }
            builder.Append(')');
            return builder.ToString();
        }
    }

    // 关键字 后运算符
    public enum EBinaryOperator
    {
        /// <summary>left * right</summary>
        Multiply = 1,
        /// <summary>left / right</summary>
        Division,
        /// <summary>left % right</summary>
        Modulus,

        /// <summary>left + right</summary>
        Addition,
        /// <summary>left - right</summary>
        Subtraction,

        // left << right
        ShiftLeft,
        /// <summary>left >> right</summary>
        ShiftRight,

        /// <summary>left > right</summary>
        GreaterThan,
        /// <summary>left >= right</summary>
        GreaterThanOrEqual,
        // left < right
        LessThan,
        // left <= right
        LessThanOrEqual,
        /// <summary>left == right</summary>
        Equality,
        /// <summary>left != right</summary>
        Inequality,

        // left & right
        BitwiseAnd,
        /// <summary>left ^ right</summary>
        ExclusiveOr,
        /// <summary>left | right</summary>
        BitwiseOr,

        // left && right
        ConditionalAnd,
        /// <summary>left || right</summary>
        ConditionalOr,
    }
    /// <summary>二元运算符</summary>
    public class BinaryOperator : SyntaxNode
    {
        public static Dictionary<EBinaryOperator, string> Symbols = new Dictionary<EBinaryOperator, string>();
        static BinaryOperator()
        {
            Symbols[EBinaryOperator.Multiply] = "*";
            Symbols[EBinaryOperator.Division] = "/";
            Symbols[EBinaryOperator.Modulus] = "%";
            Symbols[EBinaryOperator.Addition] = "+";
            Symbols[EBinaryOperator.Subtraction] = "-";
            Symbols[EBinaryOperator.ShiftLeft] = "<<";
            Symbols[EBinaryOperator.ShiftRight] = ">>";
            Symbols[EBinaryOperator.BitwiseAnd] = "&";
            Symbols[EBinaryOperator.ExclusiveOr] = "^";
            Symbols[EBinaryOperator.BitwiseOr] = "|";
            Symbols[EBinaryOperator.GreaterThan] = ">";
            Symbols[EBinaryOperator.GreaterThanOrEqual] = ">=";
            Symbols[EBinaryOperator.LessThan] = "<";
            Symbols[EBinaryOperator.LessThanOrEqual] = "<=";
            Symbols[EBinaryOperator.Equality] = "=";
            Symbols[EBinaryOperator.Inequality] = "<>";
            Symbols[EBinaryOperator.ConditionalAnd] = "&&";
            Symbols[EBinaryOperator.ConditionalOr] = "||";
        }

        public EBinaryOperator Operator;
        public SyntaxNode Left;
        public SyntaxNode Right;
        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Left, Symbols[Operator], Right);
        }
    }
    public class IN : WithExpressionExpression
    {
        /// <summary>可以是常量，参数，子查询</summary>
        public List<SyntaxNode> Values = new List<SyntaxNode>();
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{0} IN (", Expression);
            for (int i = 0, c = Values.Count - 1; i <= c; i++)
            {
                builder.Append(Values[i]);
                if (i != c)
                    builder.Append(", ");
            }
            builder.Append(')');
            return builder.ToString();
        }
    }
    /// <summary>
    /// 模糊匹配规则
    /// <para>1. 默认不区分大小写，需要区分时，需要在LIKE后增加BINARY，例如LIKE BINARY 'Aa%'</para>
    /// <para>2. %代表任意字符</para>
    /// <para>3. _代表任一字符</para>
    /// <para>4. 特殊字符转义\% \_ \\\\</para>
    /// </summary>
    public class LIKE : WithExpressionExpression
    {
        public SyntaxNode Value;
        public override string ToString()
        {
            return string.Format("{0} LIKE {1}", Expression, Value);
        }
    }
    /// <summary>is仅由于is null或者is not null的情况</summary>
    public class IS : WithExpressionExpression
    {
        public bool Not;
        public override string ToString()
        {
            if (Not)
                return string.Format("{0} IS NOT NULL", Expression);
            else
                return string.Format("{0} IS NULL", Expression);
        }
    }
    public class AS : WithExpressionExpression
    {
        public string Alias;
        public override string ToString()
        {
            return string.Format("{0} AS `{1}`", Expression, Alias);
        }
    }

    #endregion

    #region Define

    /// <summary>注释类型</summary>
    public enum ECommentSign
    {
        /// <summary>#</summary>
        Charp,
        /// <summary>-- </summary>
        Rod,
        /// <summary>/*，注释中再包含/*，也只认第一个*/作为结束</summary>
        Star,
    }
    /// <summary>注释</summary>
    [Code(ECode.LessUseful)]
    public class COMMENT : SyntaxNode
    {
        public ECommentSign Sign;
        public string Content;
        public override string ToString()
        {
            switch (Sign)
            {
                case ECommentSign.Charp: return string.Format("# {0}", Content);
                case ECommentSign.Rod: return string.Format("-- {0}");
                case ECommentSign.Star: return string.Format("/* {0} */");
                default: return null;
            }
        }
    }

    public abstract class Named : SyntaxNode
    {
        public string Name;
        public override string ToString()
        {
            if (Name == "*") return Name;
            return string.Format("`{0}`", Name);
        }
    }

    // 表
    public class TABLE : Named { }

    // 字段
    public class FIELD : Named { }
    /// <summary>`TableName`.`FieldName` [AS Alias]</summary>
    public class FieldWithTable : FIELD
    {
        /// <summary>表名，可能为空</summary>
        public string Table;

        /// <summary>*</summary>
        public bool IsAll { get { return Name == "*"; } }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Table))
                return base.ToString();
            else
                return string.Format("`{0}`.{1}", Table, base.ToString());
        }
    }
    /// <summary>@p0</summary>
    public class FieldParameter : FIELD
    {
        public override string ToString()
        {
            return string.Format("@{0}", Name);
        }
    }


    #endregion
}
namespace EntryEngine.Database.MDB.Semantics
{
    using EntryEngine.Database.MDB.Syntax;
    using EntryEngine.Database.Refactoring;
    using EntryEngine.Database.Refactoring.MySQL;

    class SyntaxExecutor
    {
        /// <summary>检查语法错误</summary>
        /// <param name="__throw">true则抛出异常</param>
        /// <param name="message">异常信息</param>
        public static void CheckSyntax(bool __throw, string message, params object[] param)
        {
            if (__throw)
                throw new MDBException(message, param);
        }

        /// <summary>例如select * from t_test</summary>
        public static Dictionary<string, Executor> ExecutorCacheBySQL = new Dictionary<string, Executor>();
        /// <summary>1. 例如select * from t_test = select c1, c2, c3 from t_test
        /// 2. 或者select * from t_test = select \n * \n from \n t_test
        /// </summary>
        public static Dictionary<SyntaxNode, Executor> ExecutorCacheBySyntax = new Dictionary<SyntaxNode, Executor>();
        /// <summary>缓存 = (执行器 + 参数).执行结果</summary>
        public static Dictionary<Executor, SQLResult> ResultCache = new Dictionary<Executor, SQLResult>();

        abstract class ExecutorBase : SyntaxVisitor
        {
            public abstract Executor Executor { get; }
            public MDBDatabase DB;
        }
        class ExecutorRoute : ExecutorBase
        {
            Executor executor;
            public override Executor Executor
            {
                get { return executor; }
            }
            public static Executor Execute(MDBDatabase db, SyntaxNode model)
            {
                ExecutorRoute executor = new ExecutorRoute();
                executor.DB = db;
                executor.Visit(model);
                return executor.executor;
            }
            public override void Visit(SyntaxNode model)
            {
                if (model == null) return;

                if (model is INSERT) executor = ExecutorInsert.Execute(DB, (INSERT)model);

                else if (model is DELETE) Visit((DELETE)model);
                else if (model is TRUNCATE) Visit((TRUNCATE)model);

                else if (model is UPDATE) Visit((UPDATE)model);

                else if (model is SELECT) executor = ExecutorSelect.Execute(DB, (SELECT)model);

                else if (model is Parenthesized) Visit((Parenthesized)model);
                else if (model is COMMENT) ;

                else throw new MDBException("执行器路由无法执行{0}类型的语法节点", model.GetType());
            }
            public override void Visit(Parenthesized model)
            {
                Visit(model.Expression);
            }
        }
        class ExecutorValue : ExecutorBase
        {
            /// <summary>常量</summary>
            class Constant : Executor, IValue
            {
                SQLResult result;
                PrimitiveValue primitive;
                public Constant(PrimitiveValue value, AS alias)
                {
                    this.primitive = value;

                    result = new SQLResult();
                    result.TotalRows = 1;
                    result.RecordsAffected = -1;
                    result.RecordsChanged = -1;
                    // 常量不加入缓存
                    result.Cached = true;
                    result.Columns = new List<SQLResultColumn>(1);

                    SQLResultColumn column = new SQLResultColumn();
                    column.Type = value.Type;
                    column.Object = value.Value;
                    if (alias != null)
                        column.Name = alias.Alias;
                    else
                        if (value.Value == null)
                            column.Name = "null";
                        else
                            column.Name = value.Value.ToString();
                    result.Columns.Add(column);
                }
                public object Value
                {
                    get { return primitive.Value; }
                }
                protected override SQLResult InternalExecute(EExecute execute)
                {
                    return result;
                }
            }
            // 引用（参数）
            // 运算符（比较运算符）
            // 函数（聚合函数）
            // 子查询
            Executor executor;
            public override Executor Executor
            {
                get { return executor; }
            }
            public static Executor Execute(MDBDatabase db, SyntaxNode model)
            {
                ExecutorValue executor = new ExecutorValue();
                executor.DB = db;
                executor.Visit(model);
                return executor.executor;
            }

            AS alias;
            AS GetAlias()
            {
                AS temp = alias;
                alias = null;
                return temp;
            }
            public override void Visit(PrimitiveValue model)
            {
                executor = new Constant(model, GetAlias());
            }
            public override void Visit(AS model)
            {
                alias = model;
            }
        }
        class ExecutorInsert : ExecutorBase
        {
            class Insert : Executor
            {
                public MDBTable Table;
                public List<MDBColumn> Fields;
                /// <summary>插入数据为多行多列，第一个List代表行，第二个List代表列</summary>
                public List<List<IValue>> Values = new List<List<IValue>>();
                protected override SQLResult InternalExecute(EExecute execute)
                {
                    SQLResult result = new SQLResult();
                    int row = Values.Count;
                    result.RecordsAffected = row;
                    result.RecordsChanged = row;
                    result.TotalRows = row;
                    // 若插入的表有Identity键列，且该列的值没有被指定是自动增长的，则结果列中返回插入的所有行的Identity键值
                    //result.Columns
                    for (int i = 0; i < row; i++)
                    {
                    }
                    return result;
                }
            }
            Insert ret = new Insert();
            public override Executor Executor
            {
                get { return ret; }
            }
            public static Executor Execute(MDBDatabase db, INSERT model)
            {
                ExecutorInsert executor = new ExecutorInsert();
                executor.DB = db;
                executor.Visit(model);
                return executor.ret;
            }

            public override void Visit(INSERT model)
            {
                Visit(model.Table);

                // 要插入的字段
                if (model.IsAllFields)
                    ret.Fields = ret.Table.Columns;
                else
                {
                    ret.Fields = new List<MDBColumn>();
                    MDBColumn column;
                    foreach (var item in model.Fields)
                    {
                        CheckSyntax(!ret.Table.TryGetColumn(item.Name, out column), "未知的列'{0}'在插入字段列表中", item.Name);
                        ret.Fields.Add(column);
                    }
                    // todo: 若表有自增列，且插入没有指定自增列，则自动补上插入自增列，且在结果集中返回插入的自增列的值
                }

                // 插入字段对应的值
                SELECT select;
                if (model.TryGetSelect(out select))
                {
                    // select
                    Executor e = ExecutorValue.Execute(DB, select);
                    SQLResult result = e.Execute(EExecute.Reader);
                    CheckSyntax(result.Columns.Count != ret.Fields.Count, "插入的列数和值数不匹配");
                    ret.Values = new List<List<IValue>>(result.TotalRows);
                    for (int i = 0; i < result.TotalRows; i++)
                    {
                        var row = new List<IValue>();
                        ret.Values.Add(row);
                        foreach (var item in result.Columns)
                        {
                            //ret.Values.Add(
                        }
                    }
                }
                else
                {
                    CheckSyntax(model.Values.Count != ret.Fields.Count, "插入的列数和值数不匹配");
                    // todo: 若Values改为了可以插入多行，这里的Capcity=1要改为要插入的行数
                    ret.Values = new List<List<IValue>>(1);
                    ret.Values.Add(new List<IValue>(model.Values.Count));
                    // values
                    foreach (var item in model.Values)
                    {
                        Executor e = ExecutorValue.Execute(DB, item);
                        SQLResult result = e.Execute(EExecute.Reader);
                        CheckSyntax(result.TotalRows > 1, "子查询结果超过了1行1列");
                        ret.Values[0].Add(new Constant(result.Columns[0].Object));
                    }
                }
            }
            public override void Visit(TABLE model)
            {
                CheckSyntax(!DB.Tables.TryGetValue(model.Name, out ret.Table), "数据表'{0}'不存在", model.Name);
            }
        }
        class ExecutorDelete : ExecutorBase
        {
            public override Executor Executor
            {
                get { throw new NotImplementedException(); }
            }
        }
        class ExecutorUpdate : ExecutorBase
        {
            public override Executor Executor
            {
                get { throw new NotImplementedException(); }
            }
        }
        class ExecutorSelect : ExecutorBase
        {
            /// <summary>视图，查询语句生成的临时表</summary>
            class View
            {
            }
            class ViewColumn : SQLResultColumn
            {
            }

            class Select : Executor
            {
                protected override SQLResult InternalExecute(EExecute execute)
                {
                    throw new NotImplementedException();
                }
            }
            Select ret = new Select();
            public override Executor Executor
            {
                get { return ret; }
            }
            public static Executor Execute(MDBDatabase db, SELECT model)
            {
                ExecutorSelect executor = new ExecutorSelect();
                executor.DB = db;
                executor.Visit(model);
                return executor.ret;
            }
            public override void Visit(SELECT model)
            {
                // from
                // where 1: join前就可以确定的表的筛选
                // on
                // join
                // where 2: 必须join后才能筛选的表内容
                // select
                // group by
                // having
                // order by
                // limit
                // union

                // 按照mysql的执行流程，执行过程中，有且仅有1张表
                // where和on的条件仅针对合并的那一张表
                // 我这里要先对原表执行where和on的条件，减少原表数量
                // 再做笛卡尔乘积合成一张大表

                if (model.From != null)
                    Visit(model.From);
            }
            public override void Visit(JOIN model)
            {
                base.Visit(model);
            }
            public override void Visit(TABLE model)
            {
                base.Visit(model);
            }
        }

        public static List<SQLResult> Execute(MDBDatabase db, SQLCommand command)
        {
            List<SQLResult> results = new List<SQLResult>();

            Executor executor;
            SQLResult result;
            // 执行器SQL缓存
            if (ExecutorCacheBySQL.TryGetValue(command.SQL, out executor))
            {
                executor = executor.ReplaceParameters(command);
                if (ResultCache.TryGetValue(executor, out result))
                    // 有结果缓存
                    results.Add(result);
                else
                    // 无结果缓存则执行
                    results.Add(executor.Execute(command.ExecuteType));
                return results;
            }

            // 解析SQL语法
            ParserMySQL parser = new ParserMySQL();
            parser.Parse(command);
            foreach (var item in parser.SQLs)
            {
                if (ExecutorCacheBySyntax.TryGetValue(item, out executor))
                {
                    // SQL语法有缓存
                    executor = executor.ReplaceParameters(command);
                    if (ResultCache.TryGetValue(executor, out result))
                        // 有结果缓存
                        results.Add(result);
                    else
                        // 无结果缓存则执行
                        results.Add(executor.Execute(command.ExecuteType));
                }
                else
                {
                    // SQL语法无缓存
                    // SQL语法 -> 执行器
                    executor = ExecutorRoute.Execute(db, item);
                    executor = executor.ReplaceParameters(command);
                    results.Add(executor.Execute(command.ExecuteType));
                }
            }
            return results;
        }
    }

    abstract class Executor
    {
        public List<Parameter> Parameters = new List<Parameter>();
        public SQLResult Execute(EExecute execute)
        {
            SQLResult result = InternalExecute(execute);
            if (result.Cached)
            {
                SyntaxExecutor.ResultCache.Add(this, result);
                result.Cached = true;
            }
            return result;
        }
        protected abstract SQLResult InternalExecute(EExecute execute);
        public Executor ReplaceParameters(SQLCommand command)
        {
            int count = Parameters.Count;
            if (count == 0) return this;
            Executor clone = Clone();
            if (this != clone && count > 0)
            {
                if (count > command.Parameters.Count)
                    throw new MDBException("参数数量不匹配，需要{0}个，实际{1}个", Parameters.Count, command.Parameters.Count);

                for (int i = 0; i < count; i++)
                {
                    Parameter item = Parameters[i];
                    SQLParameter param = command[item.Name];
                    if (param == null)
                        throw new MDBException("未找到参数{0}", item.Name);
                    clone.Parameters.Add(new Parameter(item, param.Object));
                }
            }
            return clone;
        }
        /// <summary>克隆内部其它信息，不需要克隆参数</summary>
        protected virtual Executor Clone()
        {
            return this;
        }
    }
    interface IValue
    {
        object Value { get; }
    }
    class Raw : IValue
    {
        public byte[] RawData;
        public object Value
        {
            get { throw new NotImplementedException(); }
        }
        public Raw(byte[] raw) { RawData = raw; }
    }
    class Constant : IValue
    {
        public object Value { get; set; }
        public Constant(object value) { this.Value = value; }
    }
    class Parameter : IValue
    {
        public EPrimitiveType Type;
        public string Name;
        public object Value { get; set; }
        public Parameter() { }
        public Parameter(Parameter clone, object value)
        {
            this.Type = clone.Type;
            this.Name = clone.Name;
            this.Value = value;
        }
        public Parameter(SQLParameter parameter)
        {
            this.Type = parameter.Type;
            this.Name = parameter.Name;
            this.Value = parameter.Object;
        }
    }
}
namespace EntryEngine.Database.Refactoring
{
    using EntryEngine.Database.MDB.Syntax;

    public abstract class SyntaxVisitor
    {
        public virtual void Visit(SyntaxNode model)
        {
            if (model == null) return;

            if (model is INSERT) Visit((INSERT)model);

            else if (model is DELETE) Visit((DELETE)model);
            else if (model is TRUNCATE) Visit((TRUNCATE)model);

            else if (model is UPDATE) Visit((UPDATE)model);

            else if (model is SELECT) Visit((SELECT)model);
            else if (model is JOIN) Visit((JOIN)model);
            else if (model is ORDERBY) Visit((ORDERBY)model);
            else if (model is LIMIT) Visit((LIMIT)model);
            else if (model is UNION) Visit((UNION)model);

            else if (model is PrimitiveValue) Visit((PrimitiveValue)model);
            else if (model is UnaryOperator) Visit((UnaryOperator)model);
            else if (model is Parenthesized) Visit((Parenthesized)model);
            else if (model is BETWEEN) Visit((BETWEEN)model);
            else if (model is CASE) Visit((CASE)model);
            else if (model is WHEN) Visit((WHEN)model);
            else if (model is DISTINCT) Visit((DISTINCT)model);
            else if (model is EXISTS) Visit((EXISTS)model);
            else if (model is FunctionPN) Visit((FunctionPN)model);

            else if (model is BinaryOperator) Visit((BinaryOperator)model);
            else if (model is LIKE) Visit((LIKE)model);
            else if (model is IN) Visit((IN)model);
            else if (model is IS) Visit((IS)model);
            else if (model is AS) Visit((AS)model);

            else if (model is COMMENT) Visit((COMMENT)model);
            else if (model is TABLE) Visit((TABLE)model);
            else if (model is FIELD) Visit((FIELD)model);
            else if (model is FieldWithTable) Visit((FieldWithTable)model);
            else if (model is FieldParameter) Visit((FieldParameter)model);
        }

        // 增
        public virtual void Visit(INSERT model) { }

        // 删
        public virtual void Visit(DELETE model) { }
        public virtual void Visit(TRUNCATE model) { }

        // 改
        public virtual void Visit(UPDATE model) { }

        // 查
        public virtual void Visit(SELECT model) { }
        public virtual void Visit(JOIN model) { }
        public virtual void Visit(ORDERBY model) { }
        public virtual void Visit(LIMIT model) { }
        public virtual void Visit(UNION model) { }

        // 前表达式
        public virtual void Visit(PrimitiveValue model) { }
        public virtual void Visit(UnaryOperator model) { }
        public virtual void Visit(Parenthesized model) { }
        public virtual void Visit(BETWEEN model) { }
        public virtual void Visit(CASE model) { }
        public virtual void Visit(WHEN model) { }
        public virtual void Visit(DISTINCT model) { }
        public virtual void Visit(EXISTS model) { }
        public virtual void Visit(FunctionPN model) { }

        // 后表达式
        public virtual void Visit(BinaryOperator model) { }
        public virtual void Visit(LIKE model) { }
        public virtual void Visit(IN model) { }
        public virtual void Visit(IS model) { }
        public virtual void Visit(AS model) { }

        // 定义
        public virtual void Visit(COMMENT model) { }
        public virtual void Visit(TABLE model) { }
        public virtual void Visit(FIELD model) { }
        public virtual void Visit(FieldWithTable model) { }
        public virtual void Visit(FieldParameter model) { }
    }

    /// <summary>SQL语句解析器，生成内存中可执行的语法树，带参数的生成缓存，语句执行结果生成缓存</summary>
    public abstract class SQL_PARSER
    {
        internal List<SyntaxNode> SQLs = new List<SyntaxNode>();
        /// <summary>字符串已经转为小写</summary>
        protected StringStreamReader r;
        /// <summary>字符串保留原样，主要用于读取名字等大小写敏感的字段</summary>
        private StringStreamReader r2;
        protected SQLCommand command;

        /// <summary>r2同步r的位置，使用r2进行操作，之后r再同步回r2的位置，适合需要读取大小写敏感的内容时使用</summary>
        protected void SyncR2(Action<StringStreamReader> action)
        {
            r2.Pos = r.Pos;
            action(r2);
            r.Pos = r2.Pos;
        }
        public void Parse(SQLCommand command)
        {
            this.command = command;
            r2 = new StringStreamReader(command.SQL);
            r2.WORD_BREAK = " \r\n\t'\"#+-*^|&!/(),;";
            r = new StringStreamReader(r2.str.ToLower());
            r.WORD_BREAK = r2.WORD_BREAK;
            InternalParse();
        }
        protected abstract void InternalParse();
        /// <summary>检查语法错误</summary>
        /// <param name="__throw">true则抛出异常</param>
        /// <param name="message">异常信息</param>
        public void CheckSyntax(bool __throw, string message)
        {
            if (__throw)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine(message);
                string[] lines = r.str.Split('\n');
                int count = 0;
                int line = lines.Length;
                int pos = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    count += lines[i].Length;
                    if (r.Pos <= count)
                    {
                        line = i + 1;
                        pos = r.Pos - (count - lines[i].Length);
                        break;
                    }
                    count++;
                }
                // 描述错误前面的字符个数
                const int COUNT = 15;
                builder.AppendLine("在{0}行{1}：\"{2}\"附近", line, pos,
                    r.Pos >= COUNT ? r.str.Substring(r.Pos - COUNT, COUNT) : r.str.Substring(0, r.Pos));
                throw new InvalidSyntaxException(builder.ToString());
            }
        }
        protected internal abstract bool IsKeyword(string word);
    }
    public abstract class SQL_WRITER : SyntaxVisitor
    {
        protected StringBuilder builder = new StringBuilder();
        /// <summary>访问整个解析器解析出来的所有语法树</summary>
        public void Visit(SQL_PARSER parser)
        {
            VisitFromParser(parser.SQLs);
        }
        protected virtual void VisitFromParser(List<SyntaxNode> sqls)
        {
            foreach (var item in sqls)
            {
                Visit(item);
                builder.Append(';');
            }
        }
    }
}
namespace EntryEngine.Database.Refactoring.MySQL
{
    using EntryEngine.Database.MDB.Syntax;

    public class ParserMySQL : SQL_PARSER
    {
        #region MYSQL关键字
        static HashSet<string> KEYWORD_MYSQL = new HashSet<string>
        {
"add","all","alter",
"analyze","and","as",
"asc","asensitive","before",
"between","bigint","binary",
"blob","both","by",
"call","cascade","case",
"change","char","character",
"check","collate","column",
"condition","constraint",
"continue","convert","create",
"cross","current_date","current_time",
"current_timestamp","current_user","cursor",
"database","databases","day_hour",
"day_microsecond","day_minute","day_second",
"dec","decimal","declare",
"default","delayed","delete",
"desc","describe","deterministic",
"distinct","distinctrow","div",
"double","drop","dual",
"each","else","elseif",
"enclosed","escaped","exists",
"exit","explain","false",
"fetch","float","float4",
"float8","for","force",
"foreign","from","fulltext",
"goto","grant","group",
"having","high_priority","hour_microsecond",
"hour_minute","hour_second","if",
"ignore","in","index",
"infile","inner","inout",
"insensitive","insert","int",
"int1","int2","int3",
"int4","int8","integer",
"interval","into","is",
"iterate","join","key",
"keys","kill","label",
"leading","leave","left",
"like","limit","linear",
"lines","load","localtime",
"localtimestamp","lock","long",
"longblob","longtext","loop",
"low_priority","match","mediumblob",
"mediumint","mediumtext","middleint",
"minute_microsecond","minute_second","mod",
"modifies","natural","not",
"no_write_to_binlog","null","numeric",
"on","optimize","option",
"optionally","or","order",
"out","outer","outfile",
"precision","primary","procedure",
"purge","raid0","range",
"read","reads","real",
"references","regexp","release",
"rename","repeat","replace",
"require","restrict","return",
"revoke","right","rlike",
"schema","schemas","second_microsecond",
"select","sensitive","separator",
"set","show","smallint",
"spatial","specific","sql",
"sqlexception","sqlstate","sqlwarning",
"sql_big_result","sql_calc_found_rows","sql_small_result",
"ssl","starting","straight_join",
"table","terminated","then",
"tinyblob","tinyint","tinytext",
"to","trailing","trigger",
"true","undo","union",
"unique","unlock","unsigned",
"update","usage","use",
"using","utc_date","utc_time",
"utc_timestamp","values","varbinary",
"varchar","varcharacter","varying",
"when","where","while",
"with","write","x509",
"xor","year_month","zerofil",
        };
        #endregion

        /// <summary>表名.字段名 的'.'专用</summary>
        string WORD_BREAK2;

        protected override void InternalParse()
        {
            WORD_BREAK2 = r.WORD_BREAK + '.';
            while (!r.IsEnd)
            {
                ParseComment();

                SyntaxNode root;
                string word = r.PeekNextWord;
                switch (word)
                {
                    // 增
                    case "insert":
                    case "replace":
                        root = ParseInsert(); break;

                    // 删
                    case "delete": root = ParseDelete(); break;
                    case "truncate": root = ParseTruncate(); break;

                    // 改
                    case "update": root = ParseUpdate(); break;

                    // 查
                    case "select": root = ParseSelect(); break;

                    default:
                        root = null;
                        break;
                }

                if (root != null)
                    SQLs.Add(root);
            }
        }
        protected internal override bool IsKeyword(string word)
        {
            return KEYWORD_MYSQL.Contains(word);
        }

        void ParseComment()
        {
            while (!r.IsEnd)
            {
                if (r.EatAfterSignIfIs("/*"))
                {
                    COMMENT comment = new COMMENT();
                    comment.Sign = ECommentSign.Star;
                    comment.Content = r.NextToSignAfter("*/");
                }
                else if (r.EatAfterSignIfIs("#"))
                {
                    COMMENT comment = new COMMENT();
                    comment.Sign = ECommentSign.Charp;
                    comment.Content = r.EatLine();
                }
                else if (r.EatAfterSignIfIs("-- "))
                {
                    COMMENT comment = new COMMENT();
                    comment.Sign = ECommentSign.Rod;
                    comment.Content = r.EatLine();
                }
                else if (r.EatAfterSignIfIs(";"))
                {
                }
                else
                    break;
            }
            r.EatWhitespace();
        }

        #region 表达式
        #region 常量
        PrimitiveValue ParsePrimitiveValue()
        {
            bool primitive;
            object value = ParsePrimitiveValue(out primitive);
            if (primitive)
                return new PrimitiveValue(value);
            else
                return null;
        }
        object ParsePrimitiveValue(out bool isPrimitive)
        {
            if (r.IsEnd)
            {
                isPrimitive = false;
                return null;
            }
            else
                isPrimitive = true;

            if (r.IsNextSign("null") && r.IsNext(" \r\n\t,;)", 4, false))
            {
                r.EatAfterSign("null");
                return null;
            }

            // bool
            if (r.IsNextSign("true") && r.IsNext(" \r\n\t,;.])", 4, false))
            {
                r.EatAfterSign("true");
                return true;
            }
            if (r.IsNextSign("false") && r.IsNext(" \r\n\t,;.])", 5, false))
            {
                r.EatAfterSign("false");
                return false;
            }

            // 数字
            char c = r.PeekChar;
            if (IsPrimitiveFlag(c))
            {
                string number = r.Next("(),; \t\r\n!*/%^&|=");
                if (number.Contains("0x"))
                    if (c == '-')
                    {
                        // 采用minus一元运算符
                        isPrimitive = false;
                        //return -Convert.ToInt32(number, 16);
                    }
                    else
                    {
                        long value = Convert.ToInt64(number, 16);
                        if (value <= int.MaxValue && value >= int.MinValue)
                            return (int)value;
                        return value;
                    }
                else if (number.Contains("."))
                    return double.Parse(number);
                else
                    return long.Parse(number);
            }

            byte strSign = 0;
            if (r.EatAfterSignIfIs("\""))
                strSign = 1;
            else if (r.EatAfterSignIfIs("\'"))
                strSign = 2;
            if (strSign != 0)
            {
                // 字符串：双引号需要转意\"，左斜杠则需要将两个左斜杠变回一个左斜杠
                int skip = 0;
                while (true)
                {
                    int pos;
                    if (strSign == 1)
                        pos = r.NextPosition("\"", skip);
                    else
                        pos = r.NextPosition("\'", skip);
                    // 转意的 '\' 为双数时，则没有对引号进行转意，而是左斜杠自身的转意
                    bool convert = false;
                    for (int i = pos - 1, p = r.Pos + skip; i >= p; i--)
                    {
                        if (r.GetChar(i) == '\\')
                            convert = !convert;
                        else
                            break;
                    }

                    if (convert)
                    {
                        skip = pos - r.Pos + 1;
                    }
                    else
                    {
                        string str = r.ToPosition(pos);
                        // eat the right '"'
                        r.Read();
                        string result = _SERIALIZE.CodeToString(str);
                        // 可能是日期格式
                        DateTime time;
                        if (DateTime.TryParse(result, out time))
                            return time;
                        // 普通字符串
                        else
                            return result;
                    }
                }
            }

            isPrimitive = false;
            return null;
        }
        bool IsPrimitiveFlag(char c)
        {
            if (c == '.')
                return true;
            if (c >= '0' && c <= '9')
                return true;
            if (c == '-')
            {
                c = r.GetChar(r.Pos + 1);
                if (c == '.')
                    return true;
                if (c >= '0' && c <= '9')
                    return true;
            }
            return false;
        }
        #endregion
        SyntaxNode ParseExpression()
        {
            SyntaxNode expression = ParseExpressionFront();
            if (expression == null)
                return null;

            while (true)
            {
                var expression2 = ParseExpressionBack(expression);
                if (expression2 == null)
                    return expression;
                // 整理二元运算符顺序(先*/再+-)
                if (expression2 is BinaryOperator)
                {
                    BinaryOperator o1 = (BinaryOperator)expression2;
                    BinaryOperator o2 = o1.Right as BinaryOperator;
                    if (o2 != null && o1.Operator < o2.Operator)
                    {
                        o1.Right = o2.Left;
                        o2.Left = o1;
                        expression2 = o2;
                    }
                }
                expression = expression2;
            }
        }
        /// <summary>一元表达式，即只有一个参数，例如-5,sum(a)</summary>
        SyntaxNode ParseExpressionFront()
        {
            ParseComment();

            // 原始值
            var s2 = ParsePrimitiveValue();
            if (s2 != null) return s2;

            #region unary
            EUnaryOperator o = 0;
            if (r.EatAfterSignIfIs("!") || r.EatAfterSignIfIs("not"))
                o = EUnaryOperator.Not;
            else if (r.EatAfterSignIfIs("~"))
                o = EUnaryOperator.BitNot;
            else if (r.EatAfterSignIfIs("-"))
                o = EUnaryOperator.Minus;
            else if (r.EatAfterSignIfIs("+"))
                o = EUnaryOperator.Plus;
            if (o != 0)
            {
                UnaryOperator unary = new UnaryOperator();
                unary.Operator = o;
                unary.Expression = ParseExpressionFront();
                return unary;
            }
            #endregion

            #region 关键字
            if (r.EatAfterSignIfIs("case"))
            {
                ParseComment();
                CASE node = new CASE();
                string next = r.PeekNextWord;
                if (next != "when")
                    node.Expression = ParseExpression();
                CheckSyntax(!r.EatAfterSignIfIs("when"), "case后缺少when");
                while (true)
                {
                    WHEN when = new WHEN();
                    when.Expression = ParseExpression();
                    CheckSyntax(!r.EatAfterSignIfIs("then"), "when后缺少then");
                    when.Then = ParseExpression();
                    node.Cases.Add(when);
                    ParseComment();
                    if (r.EatAfterSignIfIs("when"))
                        continue;
                    else if (r.EatAfterSignIfIs("else"))
                    {
                        WHEN __else = new WHEN();
                        __else.Then = ParseExpression();
                        node.Cases.Add(__else);
                        CheckSyntax(!r.EatAfterSignIfIs("end"), "case else结束缺少end");
                        break;
                    }
                    else if (r.EatAfterSignIfIs("end"))
                        break;
                    else
                        throw new SyntaxErrorException("case结束缺少end");
                }
                return node;
            }

            if (r.EatAfterSignIfIs("distinct"))
            {
                DISTINCT node = new DISTINCT();
                // 可以是'('，也可以是' '
                bool flag = r.EatAfterSignIfIs("(");
                node.Expression = ParseExpression();
                CheckSyntax(flag && !r.EatAfterSignIfIs(")"), "distinct结束缺少')'");
                return node;
            }

            if (r.EatAfterSignIfIs("exists"))
            {
                var exists = ParseFunctionP1<EXISTS>();
                CheckSyntax(!SELECT.IsSelect(exists.Expression), "exists后的表达式必须是查询");
                return exists;
            }

            foreach (var item in FunctionPN.Functions)
                if (r.EatAfterSignIfIs(item.Key))
                    return ParseFunctionPN(item.Key, item.Value);
            #endregion

            #region 括号
            if (r.IsNextSign('('))
            {
                r.EatAfterSign("(");
                ParseComment();
                var expression = ParseExpression();
                ParseComment();
                r.EatAfterSign(")");
                // 子查询
                Parenthesized parenthesized = new Parenthesized();
                parenthesized.Expression = expression;
                if (parenthesized.IsChildSelect)
                    return ParseAS(parenthesized);
                else
                    return parenthesized;
            }
            #endregion

            #region select
            if (r.IsNextSign("select"))
                return ParseSelect();
            #endregion

            #region 字段
            // 参数
            if (r.EatAfterSignIfIs("@"))
            {
                FieldParameter field = new FieldParameter();
                field.Name = r.NextWord();
                return field;
            }

            // 引用的字段名
            return ParseSelectField();
            #endregion
        }
        /// <summary>二元表达式，即需要两个参数，例如1+2,str like '%a%'</summary>
        SyntaxNode ParseExpressionBack(SyntaxNode front)
        {
            ParseComment();

            #region operator

            EBinaryOperator o = 0;
            if (r.EatAfterSignIfIs("&&") || r.EatAfterWordIfIs("and"))
                o = EBinaryOperator.ConditionalAnd;
            else if (r.EatAfterSignIfIs("||") || r.EatAfterWordIfIs("or"))
                o = EBinaryOperator.ConditionalOr;
            else if (r.EatAfterSignIfIs("&"))
                o = EBinaryOperator.BitwiseAnd;
            else if (r.EatAfterSignIfIs("|"))
                o = EBinaryOperator.BitwiseOr;
            else if (r.EatAfterSignIfIs("^"))
                o = EBinaryOperator.ExclusiveOr;
            else if (r.EatAfterSignIfIs("<<"))
                o = EBinaryOperator.ShiftLeft;
            else if (r.EatAfterSignIfIs(">>"))
                o = EBinaryOperator.ShiftRight;
            else if (r.EatAfterSignIfIs("+"))
                o = EBinaryOperator.Addition;
            else if (r.EatAfterSignIfIs("-"))
                o = EBinaryOperator.Subtraction;
            else if (r.EatAfterSignIfIs("*"))
                o = EBinaryOperator.Multiply;
            else if (r.EatAfterSignIfIs("/"))
                o = EBinaryOperator.Division;
            else if (r.EatAfterSignIfIs("%"))
                o = EBinaryOperator.Modulus;
            else if (r.EatAfterSignIfIs("!=") || r.EatAfterSignIfIs("<>"))
                o = EBinaryOperator.Inequality;
            else if (r.EatAfterSignIfIs("="))
                o = EBinaryOperator.Equality;
            else if (r.EatAfterSignIfIs("<="))
                o = EBinaryOperator.LessThanOrEqual;
            else if (r.EatAfterSignIfIs("<"))
                o = EBinaryOperator.LessThan;
            else if (r.EatAfterSignIfIs(">="))
                o = EBinaryOperator.GreaterThanOrEqual;
            else if (r.EatAfterSignIfIs(">"))
                o = EBinaryOperator.GreaterThan;
            if (o != 0)
            {
                BinaryOperator binary = new BinaryOperator();
                binary.Left = front;
                binary.Operator = o;
                binary.Right = ParseExpression();
                return binary;
            }

            #endregion

            #region between and

            // between and MYSQL相当于 >= and <=
            if (r.EatAfterWordIfIs("between"))
            {
                // 将a between b and c转换成(a >= b and a <= c)
                // HACK: a表达式可能是个计算值，转换之后可能会计算2次导致效率下降

                //BinaryOperator greater = new BinaryOperator();
                //greater.Operator = EBinaryOperator.GreaterThanOrEqual;
                //greater.Left = front;
                //greater.Right = ParseExpression();

                //ParseComment();
                //CheckSyntax(!r.EatAfterSignIfIs("and"), "between的表达式后缺少and");

                //BinaryOperator less = new BinaryOperator();
                //less.Operator = EBinaryOperator.LessThanOrEqual;
                //less.Left = front;
                //less.Right = ParseExpression();

                //BinaryOperator and = new BinaryOperator();
                //and.Operator = EBinaryOperator.ConditionalAnd;
                //and.Left = greater;
                //and.Right = less;

                //return new Parenthesized(and);

                BETWEEN ba = new BETWEEN();
                ba.Expression = front;
                ba.Between = ParseExpression();
                ParseComment();
                CheckSyntax(!r.EatAfterWordIfIs("and"), "between的表达式后缺少and");
                ba.And = ParseExpression();
                return ba;
            }

            #endregion

            #region 关键字

            if (r.EatAfterWordIfIs("in"))
            {
                ParseComment();
                CheckSyntax(!r.EatAfterSignIfIs("("), "in后面缺少'('");
                IN node = new IN();
                node.Expression = front;
                while (true)
                {
                    var expression = ParseExpression();
                    CheckSyntax(expression == null, "in括号里必须有值");
                    node.Values.Add(expression);
                    if (!r.EatAfterSignIfIs(","))
                        break;
                }
                ParseComment();
                CheckSyntax(!r.EatAfterSignIfIs(")"), "in后面缺少')'");
                return node;
            }

            if (r.EatAfterWordIfIs("like"))
            {
                LIKE node = new LIKE();
                node.Expression = front;
                node.Value = ParseExpression();
                return node;
            }

            if (r.EatAfterSignIfIs("is"))
            {
                ParseComment();
                string word = r.NextWord();
                IS node = new IS();
                node.Expression = front;
                if (word == "not")
                {
                    node.Not = true;
                    word = r.NextWord();
                }
                CheckSyntax(word != "null", "is后面必须是[not] null");
                return node;
            }

            #endregion

            return null;
        }
        /// <summary>查询的字段，[`Table`.]`Field` [AS Alias]</summary>
        /// <param name="canWithAlias">基本只有from|join|select后面的"表"|"字段"可以有别名</param>
        FieldWithTable ParseSelectField()
        {
            string name = ParseName(false);
            if (string.IsNullOrEmpty(name)) return null;

            FieldWithTable field = new FieldWithTable();
            field.Name = name;
            if (r.IsNextSign('.'))
            {
                field.Table = field.Name.ToLower();
                ParseComment();
                // 去掉'.'
                r.Read();
                field.Name = ParseName(false);
            }
            return field;
        }
        /// <summary>一个参数的方法，例如sum,count等聚合函数</summary>
        T ParseFunctionP1<T>() where T : WithExpressionExpression, new()
        {
            string tname = typeof(T).Name;
            T node = new T();
            CheckSyntax(!r.EatAfterSignIfIs("("), tname + "后缺少'('");
            node.Expression = ParseExpression();
            CheckSyntax(!r.EatAfterSignIfIs(")"), tname + "结束缺少')'");
            return node;
        }
        /// <summary>多个参数的方法，例如if等</summary>
        /// <param name="count">具体参数的个数，-1代表可以无数个</param>
        FunctionPN ParseFunctionPN(string key, int count)
        {
            FunctionPN node = new FunctionPN();
            node.Function = key;
            CheckSyntax(!r.EatAfterSignIfIs("("), key + "后缺少'('");
            if (count > 0)
            {
                do
                {
                    node.Parameters.Add(ParseExpression());
                } while (!r.IsEnd && r.EatAfterSignIfIs(",") && --count > 0);
            }
            CheckSyntax(!r.EatAfterSignIfIs(")"), key + "结束缺少')'");
            return node;
        }
        #endregion

        INSERT ParseInsert()
        {
            INSERT root = new INSERT();
            root.IsReplace = r.EatAfterSignIfIs("replace");
            if (!root.IsReplace)
                CheckSyntax(!r.EatAfterSignIfIs("insert"), "插入语句缺少insert或replace");
            ParseComment();
            if (r.EatAfterWordIfIs("into"))
                ParseComment();
            root.Table = new TABLE() { Name = ParseName(true) };
            CheckSyntax(string.IsNullOrEmpty(root.Table.Name), "insert缺少表名");
            ParseComment();
            if (r.EatAfterSignIfIs("("))
            {
                ParseComment();
                // 解析要插入的字段名
                while (!r.EatAfterSignIfIs(")"))
                {
                    FIELD field = new FIELD();
                    field.Name = ParseName(true);
                    CheckSyntax(string.IsNullOrEmpty(field.Name), "缺少插入的字段名");
                    root.Fields.Add(field);
                    ParseComment();
                    // (field1, field2, )的情况
                    if (r.EatAfterSignIfIs(","))
                        CheckSyntax(r.IsNextSign(')'), "缺少插入的字段名");
                }
            }
            ParseComment();
            if (r.EatAfterSignIfIs("values"))
            {
                CheckSyntax(!r.EatAfterSignIfIs("("), "values后面缺少括号");
                while (!r.EatAfterSignIfIs(")"))
                {
                    root.Values.Add(ParseExpression());
                    // (field1, field2, )的情况
                    if (r.EatAfterSignIfIs(","))
                        CheckSyntax(r.IsNextSign(')'), "缺少插入的值");
                }
            }
            else
            {
                // 查询语句
                root.Values.Add(ParseExpression());
                CheckSyntax(!root.IsSelect, "插入不是values的情况下，必须是查询语句");
            }
            return root;
        }
        DELETE ParseDelete()
        {
            DELETE root = new DELETE();

            return root;
        }
        TRUNCATE ParseTruncate()
        {
            CheckSyntax(!r.EatAfterSignIfIs("truncate"), "缺少truncate");
            TRUNCATE root = new TRUNCATE();
            r.EatAfterSignIfIs("table");
            root.Table = new TABLE() { Name = ParseName(true) };
            CheckSyntax(string.IsNullOrEmpty(root.Table.Name), "truncate缺少表名");
            return root;
        }
        UPDATE ParseUpdate()
        {
            UPDATE root = new UPDATE();

            return root;
        }

        SELECT ParseSelect()
        {
            CheckSyntax(!r.EatAfterWordIfIs("select"), "缺少select");
            SELECT root = new SELECT();
            #region fields
            // 常量，函数，[表名].[字段名]|*，子查询
            while (!r.IsEnd)
            {
                var node = ParseExpression();
                if (!(node is AS))
                    node = ParseAS(node);
                root.Values.Add(node);
                if (!r.EatAfterSignIfIs(",")) break;
            }
            #endregion
            #region from table join on
            // 全部查询常量或子查询时(例如select 1,2,3)，将没有from
            if (r.EatAfterSignIfIs("from"))
            {
                ParseComment();
                SyntaxNode table = null;
                do
                {
                    // 表，子查询
                    var current = ParseFromTable();
                    // ','相当于inner join
                    if (table != null)
                    {
                        JOIN join = new JOIN();
                        join.Join = EJoin.INNER;
                        join.Left = table;
                        join.Right = current;
                        root.From = join;
                    }
                    table = current;
                } while (!r.IsEnd && r.EatAfterSignIfIs(","));

                // join
                while (table != null)
                {
                    ParseComment();
                    JOIN join = null;
                    string next = r.PeekNextWord;
                    if (next == "join")
                    {
                        // 单join关键字相当于inner join
                        join = new JOIN(EJoin.INNER);
                        r.NextWord();
                    }
                    else
                    {
                        if (next == "left") join = new JOIN(EJoin.LEFT);
                        else if (next == "right") join = new JOIN(EJoin.RIGHT);
                        else if (next == "inner") join = new JOIN(EJoin.INNER);

                        if (join != null)
                        {
                            r.NextWord();
                            ParseComment();
                            CheckSyntax(!r.EatAfterSignIfIs("join"), "缺少join关键字");
                        }
                    }

                    if (join != null)
                    {
                        join.Left = table;
                        join.Right = ParseFromTable();
                        // on
                        if (r.EatAfterSignIfIs("on"))
                        {
                            ParseComment();
                            join.On = ParseExpression();
                        }
                        else
                        {
                            // 只有inner join允许没有on
                            CheckSyntax(join.Join != EJoin.INNER, "join缺少on关键字");
                        }
                        table = join;
                    }
                    // 没有join
                    else
                        break;
                }
                root.From = table;
            }
            #endregion
            #region where
            if (r.EatAfterSignIfIs("where"))
            {
                root.Where = ParseExpression();
                ParseComment();
            }
            #endregion
            #region group by
            if (r.EatAfterSignIfIs("group"))
            {
                ParseComment();
                CheckSyntax(!r.EatAfterSignIfIs("by"), "group关键字后缺少by，完整语法应为group by");
                while (!r.IsEnd)
                {
                    ParseComment();
                    root.GroupBy.Add(ParseSelectField());
                    if (!r.EatAfterSignIfIs(",")) break;
                }
                ParseComment();
            }
            #endregion
            #region having
            if (r.EatAfterSignIfIs("having"))
            {
                root.Having = ParseExpression();
                ParseComment();
            }
            #endregion
            #region order by
            if (r.EatAfterSignIfIs("order"))
            {
                ParseComment();
                CheckSyntax(!r.EatAfterSignIfIs("by"), "order关键字后缺少by，完整语法应为order by");
                while (!r.IsEnd)
                {
                    ParseComment();
                    ORDERBY orderby = new ORDERBY();
                    orderby.Expression = ParseExpression();
                    root.OrderBy.Add(orderby);
                    ParseComment();
                    if (!r.EatAfterSignIfIs(","))
                    {
                        // 是否有升序或降序符号
                        bool flag = false;
                        string word = r.PeekNextWord;
                        if (word == "asc")
                        {
                            orderby.DESC = false;
                            flag = true;
                        }
                        else if (word == "desc")
                        {
                            orderby.DESC = true;
                            flag = true;
                        }

                        if (flag)
                        {
                            r.NextWord();
                            ParseComment();
                            if (!r.EatAfterSignIfIs(","))
                                break;
                        }
                        else
                            break;
                    }
                }
                ParseComment();
            }
            #endregion
            #region limit
            if (r.EatAfterSignIfIs("limit"))
            {
                ParseComment();
                root.Limit = new LIMIT();
                root.Limit.Start = ParseExpression();
                LIMIT.CheckSyntax(root.Limit.Start);
                ParseComment();
                if (r.EatAfterSignIfIs(","))
                {
                    ParseComment();
                    // limit s, c
                    root.Limit.Count = ParseExpression();
                    LIMIT.CheckSyntax(root.Limit.Count);
                }
                else
                {
                    // limit c = limit 0, c
                    root.Limit.Count = root.Limit.Start;
                    root.Limit.Start = null;
                }
            }
            #endregion
            #region union
            if (r.EatAfterSignIfIs("union"))
            {
                root.Union = new UNION();
                root.Union.IsUnionAll = r.EatAfterSignIfIs("all");
                root.Union.Select = ParseExpression();
                UNION.CheckSyntax(root.Union.Select);
            }
            #endregion
            //string test = root.ToString();
            //Console.WriteLine(test);
            return root;
        }
        /// <summary>解析from或join后面的表，可以是表名或子查询</summary>
        SyntaxNode ParseFromTable()
        {
            var node = ParseExpression();
            FieldWithTable namedTable = null;
            Parenthesized childSelect = null;
            var temp = node;
            while (true)
            {
                if (temp is Parenthesized && ((Parenthesized)temp).IsChildSelect)
                {
                    childSelect = (Parenthesized)temp;
                    break;
                }
                else if (temp is FieldWithTable)
                {
                    namedTable = (FieldWithTable)temp;
                    break;
                }
                else
                {
                    CheckSyntax(!(temp is WithExpressionExpression), "from和join后必须是表名或者子查询的临时表");
                    temp = ((WithExpressionExpression)temp).Expression;
                }
            }
            if (namedTable != null)
            {
                // from或join后面，ParseExpression出来的字段名其实是表，这里进行转换
                TABLE table = new TABLE();
                table.Name = namedTable.Name;
                return ParseAS(table);
            }
            else
            {
                // 子查询
                CheckSyntax(!(node is AS), "from和join后的子查询表名必须带有别名");
            }
            return node;
        }
        string ParseName(bool ignoreCase)
        {
            ParseComment();
            bool flag = r.PeekChar == '`';
            string result = null;
            if (flag)
            {
                r.Read();
                if (ignoreCase)
                    result = r.NextToSignAfter("`");
                else
                    SyncR2(r2 => result = r2.NextToSignAfter("`"));
            }
            else
                if (r.PeekChar == '*')
                    return r.Read().ToString();
                else
                    if (ignoreCase)
                        result = r.Next(WORD_BREAK2);
                    else
                        SyncR2(r2 => result = r2.Next(WORD_BREAK2));
            return result;
        }
        /// <summary>只有子查询，select，from|join后面会有as别名</summary>
        SyntaxNode ParseAS(SyntaxNode node)
        {
            ParseComment();
            AS __as = null;
            // as
            if (r.EatAfterSignIfIs("as"))
            {
                __as = new AS();
                ParseComment();
            }

            bool flag = r.IsNextSign('`');
            if (flag && __as == null)
            {
                __as = new AS();
                r.Read();
            }

            string alias;
            if (__as == null)
            {
                ParseComment();
                // 直接空格接别名的方式
                alias = r.PeekNextWord;
                // 关键字不能作为别名，所以认为其不是别名
                if (string.IsNullOrEmpty(alias) || IsKeyword(alias))
                    return node;
                __as = new AS();
            }

            __as.Expression = node;
            SyncR2(r2 => __as.Alias = r2.NextWord());
            return __as;
        }
    }
    public class SQLWriterMySQL : SQL_WRITER
    {
        public override void Visit(SyntaxNode model)
        {
            if (model == null) return;
            builder.Append(model);
        }
    }
}
#endif