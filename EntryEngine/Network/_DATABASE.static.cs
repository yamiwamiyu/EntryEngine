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
