using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EntryEngine;
using EntryEngine.Serialize;
using EntryEngine.Network;

namespace Server
{
    public enum ET_USER
    {
        Platform,
        ID,
        RegisterTime,
        Token,
        Account,
        Password,
        Phone,
        Name,
        LastLoginTime,
    }
    public enum ET_CENTER_USER
    {
        ID,
        RegisterTime,
        Token,
        Account,
        Password,
        Phone,
        Name,
        LastLoginTime,
    }
    public enum ET_OPLog
    {
        ID,
        PID,
        Operation,
        Time,
        Way,
        Sign,
        Statistic,
        Detail,
    }
    public class MYSQL_DATABASE : _DATABASE.Database
    {
        protected override System.Data.IDbConnection CreateConnection()
        {
            var conn = new MySql.Data.MySqlClient.MySqlConnection();
            conn.ConnectionString = ConnectionString;
            conn.Open();
            return conn;
        }
    }
    public class MYSQL_TABLE_COLUMN
    {
        public string COLUMN_NAME;
        public string COLUMN_KEY;
        public string EXTRA;
        public string DATA_TYPE;
        public bool IsPrimary { get { return COLUMN_KEY == "PRI"; } }
        public bool IsIndex { get { return COLUMN_KEY == "MUL"; } }
        public bool IsUnique { get { return COLUMN_KEY == "UNI"; } }
        public bool IsIdentity { get { return EXTRA == "auto_increment"; } }
    }
    public class MASTER_STATUS
    {
        public string File;
        public int Position;
        public string Binlog_Do_DB;
    }
    public class SLAVE_STATUS
    {
        public string Master_Host;
        public string Master_User;
        public int Master_Port;
        public string Master_Log_File;
        public int Read_Master_Log_Pos;
        public string Slave_IO_Running;
        public string Slave_SQL_Running;
        public string Replicate_Do_DB;
        public string Last_Error;
        public int Exec_Master_Log_Pos;
        public int Master_Server_Id;
        public bool IsRunning { get { return Slave_IO_Running == "Yes" && Slave_SQL_Running == "Yes"; } }
        public bool IsSynchronous { get { return Read_Master_Log_Pos == Exec_Master_Log_Pos; } }
    }
    public static partial class _DB
    {
        public static bool IsDropColumn;
        public static string DatabaseName;
        public static Action<_DATABASE.Database> OnConstructDatabase;
        public static List<MergeTable> AllMergeTable = new List<MergeTable>()
        {
            new MergeTable("T_USER"),
            new MergeTable("T_CENTER_USER"),
            new MergeTable("T_OPLog"),
        };
        private static _DATABASE.Database _dao;
        /// <summary>Set this will set the event 'OnCreateConnection' and 'OnTestConnection'</summary>
        public static _DATABASE.Database _DAO
        {
            get { if (_dao == null) return _DATABASE._Database; else return _dao; }
            set
            {
                if (_dao == value) return;
                _dao = value;
                if (value != null)
                {
                    value.OnCreateConnection = CREATE_CONNECTION;
                    value.OnTestConnection = UPDATE_DATABASE_STRUCTURE;
                }
            }
        }
        /// <summary>Set this to the _DATABASE.Database.OnCreateConnection event</summary>
        public static void CREATE_CONNECTION(System.Data.IDbConnection conn, _DATABASE.Database database)
        {
            if (string.IsNullOrEmpty(conn.Database) && !string.IsNullOrEmpty(DatabaseName) && !_DAO.Available)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("CREATE DATABASE IF NOT EXISTS `{0}`;", DatabaseName);
                int create = cmd.ExecuteNonQuery();
                conn.ChangeDatabase(DatabaseName);
                _DAO.ConnectionString += string.Format("Database={0};", DatabaseName);
                _DAO.OnCreateConnection -= CREATE_CONNECTION;
                if (create > 0)
                {
                    _LOG.Info("Create database[`{0}`].", DatabaseName);
                }
                _LOG.Info("Set database[`{0}`].", DatabaseName);
            }
        }
        /// <summary>Set this to the _DATABASE.Database.OnTestConnection event</summary>
        public static void UPDATE_DATABASE_STRUCTURE(System.Data.IDbConnection conn, _DATABASE.Database database)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandTimeout = database.Timeout;
            cmd.CommandText = string.Format("SELECT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = '{0}');", conn.Database);
            bool __exists = Convert.ToBoolean(cmd.ExecuteScalar());
            #region Create table
            cmd.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS `T_USER`
            (
            `Platform` TEXT,
            `ID` INT PRIMARY KEY AUTO_INCREMENT,
            `RegisterTime` DATETIME,
            `Token` TEXT,
            `Account` TEXT,
            `Password` TEXT,
            `Phone` BIGINT,
            `Name` TEXT,
            `LastLoginTime` DATETIME
            );
            CREATE TABLE IF NOT EXISTS `T_CENTER_USER`
            (
            `ID` INT PRIMARY KEY AUTO_INCREMENT,
            `RegisterTime` DATETIME,
            `Token` TEXT,
            `Account` TEXT,
            `Password` TEXT,
            `Phone` BIGINT,
            `Name` TEXT,
            `LastLoginTime` DATETIME
            );
            CREATE TABLE IF NOT EXISTS `T_OPLog`
            (
            `ID` INT PRIMARY KEY AUTO_INCREMENT,
            `PID` INT,
            `Operation` TEXT,
            `Time` DATETIME,
            `Way` TEXT,
            `Sign` INT,
            `Statistic` INT,
            `Detail` TEXT
            );
            ";
            _LOG.Info("Begin create table.");
            cmd.ExecuteNonQuery();
            _LOG.Info("Create table completed.");
            #endregion
            
            Dictionary<string, MYSQL_TABLE_COLUMN> __columns = new Dictionary<string, MYSQL_TABLE_COLUMN>();
            MYSQL_TABLE_COLUMN __value;
            bool __noneChangePrimary;
            bool __hasPrimary;
            IDataReader reader;
            StringBuilder builder = new StringBuilder();
            
            #region Table structure "T_USER"
            _LOG.Info("Begin update table[`T_USER`] structure.");
            __columns.Clear();
            builder.Remove(0, builder.Length);
            cmd.CommandText = "SELECT COLUMN_NAME, COLUMN_KEY, EXTRA, DATA_TYPE FROM information_schema.COLUMNS WHERE TABLE_NAME = 'T_USER' AND TABLE_SCHEMA = '" + conn.Database + "';";
            reader = cmd.ExecuteReader();
            __hasPrimary = false;
            foreach (var __column in _DATABASE.ReadMultiple<MYSQL_TABLE_COLUMN>(reader))
            {
                if (__column.IsPrimary) __hasPrimary = true;
                __columns.Add(__column.COLUMN_NAME, __column);
            }
            reader.Close();
            __noneChangePrimary = true;
            __noneChangePrimary &= (__columns.TryGetValue("ID", out __value) && __value.IsPrimary);
            if (!__noneChangePrimary && __hasPrimary)
            {
                var pk = __columns.Values.FirstOrDefault(f => f.IsPrimary);
                if (pk.IsIdentity) builder.AppendLine("ALTER TABLE `T_USER` CHANGE COLUMN `{0}` `{0}` {1};", pk.COLUMN_NAME, pk.DATA_TYPE);
                builder.AppendLine("ALTER TABLE `T_USER` DROP PRIMARY KEY;");
                _LOG.Debug("Drop primary key.");
            }
            if (__columns.TryGetValue("Platform", out __value))
            {
                if (__value.DATA_TYPE != "text" && (__value.IsIndex || __value.IsUnique))
                {
                    __value.COLUMN_KEY = null;
                    builder.AppendLine("ALTER TABLE T_USER DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                builder.AppendLine("ALTER TABLE `T_USER` CHANGE COLUMN `Platform` `Platform` TEXT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_USER` ADD INDEX(`Platform`(10));");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_USER` ADD COLUMN `Platform` TEXT;");
                builder.AppendLine("ALTER TABLE `T_USER` ADD INDEX(`Platform`(10));");
                _LOG.Debug("Add index[`{0}`].", "Platform");
                _LOG.Debug("Add column[`{0}`].", "Platform");
            }
            if (__columns.TryGetValue("ID", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_USER` CHANGE COLUMN `ID` `ID` INT" + (__value.IsPrimary ? "" : " PRIMARY KEY") + " AUTO_INCREMENT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_USER DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_USER` ADD COLUMN `ID` INT PRIMARY KEY AUTO_INCREMENT;");
                _LOG.Debug("Add column[`{0}`].", "ID");
            }
            if (__columns.TryGetValue("RegisterTime", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_USER` CHANGE COLUMN `RegisterTime` `RegisterTime` DATETIME;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_USER` ADD INDEX(`RegisterTime`);");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_USER` ADD COLUMN `RegisterTime` DATETIME;");
                builder.AppendLine("ALTER TABLE `T_USER` ADD INDEX(`RegisterTime`);");
                _LOG.Debug("Add index[`{0}`].", "RegisterTime");
                _LOG.Debug("Add column[`{0}`].", "RegisterTime");
            }
            if (__columns.TryGetValue("Token", out __value))
            {
                if (__value.DATA_TYPE != "text" && (__value.IsIndex || __value.IsUnique))
                {
                    __value.COLUMN_KEY = null;
                    builder.AppendLine("ALTER TABLE T_USER DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                builder.AppendLine("ALTER TABLE `T_USER` CHANGE COLUMN `Token` `Token` TEXT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_USER` ADD INDEX(`Token`(10));");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_USER` ADD COLUMN `Token` TEXT;");
                builder.AppendLine("ALTER TABLE `T_USER` ADD INDEX(`Token`(10));");
                _LOG.Debug("Add index[`{0}`].", "Token");
                _LOG.Debug("Add column[`{0}`].", "Token");
            }
            if (__columns.TryGetValue("Account", out __value))
            {
                if (__value.DATA_TYPE != "text" && (__value.IsIndex || __value.IsUnique))
                {
                    __value.COLUMN_KEY = null;
                    builder.AppendLine("ALTER TABLE T_USER DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                builder.AppendLine("ALTER TABLE `T_USER` CHANGE COLUMN `Account` `Account` TEXT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_USER` ADD INDEX(`Account`(10));");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_USER` ADD COLUMN `Account` TEXT;");
                builder.AppendLine("ALTER TABLE `T_USER` ADD INDEX(`Account`(10));");
                _LOG.Debug("Add index[`{0}`].", "Account");
                _LOG.Debug("Add column[`{0}`].", "Account");
            }
            if (__columns.TryGetValue("Password", out __value))
            {
                if (__value.DATA_TYPE != "text" && (__value.IsIndex || __value.IsUnique))
                {
                    __value.COLUMN_KEY = null;
                    builder.AppendLine("ALTER TABLE T_USER DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                builder.AppendLine("ALTER TABLE `T_USER` CHANGE COLUMN `Password` `Password` TEXT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_USER` ADD INDEX(`Password`(10));");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_USER` ADD COLUMN `Password` TEXT;");
                builder.AppendLine("ALTER TABLE `T_USER` ADD INDEX(`Password`(10));");
                _LOG.Debug("Add index[`{0}`].", "Password");
                _LOG.Debug("Add column[`{0}`].", "Password");
            }
            if (__columns.TryGetValue("Phone", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_USER` CHANGE COLUMN `Phone` `Phone` BIGINT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_USER` ADD INDEX(`Phone`);");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_USER` ADD COLUMN `Phone` BIGINT;");
                builder.AppendLine("ALTER TABLE `T_USER` ADD INDEX(`Phone`);");
                _LOG.Debug("Add index[`{0}`].", "Phone");
                _LOG.Debug("Add column[`{0}`].", "Phone");
            }
            if (__columns.TryGetValue("Name", out __value))
            {
                if (__value.DATA_TYPE != "text" && (__value.IsIndex || __value.IsUnique))
                {
                    __value.COLUMN_KEY = null;
                    builder.AppendLine("ALTER TABLE T_USER DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                builder.AppendLine("ALTER TABLE `T_USER` CHANGE COLUMN `Name` `Name` TEXT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_USER` ADD INDEX(`Name`(10));");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_USER` ADD COLUMN `Name` TEXT;");
                builder.AppendLine("ALTER TABLE `T_USER` ADD INDEX(`Name`(10));");
                _LOG.Debug("Add index[`{0}`].", "Name");
                _LOG.Debug("Add column[`{0}`].", "Name");
            }
            if (__columns.TryGetValue("LastLoginTime", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_USER` CHANGE COLUMN `LastLoginTime` `LastLoginTime` DATETIME;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_USER DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_USER` ADD COLUMN `LastLoginTime` DATETIME;");
                _LOG.Debug("Add column[`{0}`].", "LastLoginTime");
            }
            if (IsDropColumn)
            {
                foreach (var __column in __columns.Keys)
                {
                    builder.AppendLine("ALTER TABLE `T_USER` DROP COLUMN `{0}`;", __column);
                    _LOG.Debug("Drop column[`{0}`].", __column);
                }
            }
            
            cmd.CommandText = builder.ToString();
            _LOG.Info("Building table[`T_USER`] structure.");
            cmd.ExecuteNonQuery();
            #endregion
            
            #region Table structure "T_CENTER_USER"
            _LOG.Info("Begin update table[`T_CENTER_USER`] structure.");
            __columns.Clear();
            builder.Remove(0, builder.Length);
            cmd.CommandText = "SELECT COLUMN_NAME, COLUMN_KEY, EXTRA, DATA_TYPE FROM information_schema.COLUMNS WHERE TABLE_NAME = 'T_CENTER_USER' AND TABLE_SCHEMA = '" + conn.Database + "';";
            reader = cmd.ExecuteReader();
            __hasPrimary = false;
            foreach (var __column in _DATABASE.ReadMultiple<MYSQL_TABLE_COLUMN>(reader))
            {
                if (__column.IsPrimary) __hasPrimary = true;
                __columns.Add(__column.COLUMN_NAME, __column);
            }
            reader.Close();
            __noneChangePrimary = true;
            __noneChangePrimary &= (__columns.TryGetValue("ID", out __value) && __value.IsPrimary);
            if (!__noneChangePrimary && __hasPrimary)
            {
                var pk = __columns.Values.FirstOrDefault(f => f.IsPrimary);
                if (pk.IsIdentity) builder.AppendLine("ALTER TABLE `T_CENTER_USER` CHANGE COLUMN `{0}` `{0}` {1};", pk.COLUMN_NAME, pk.DATA_TYPE);
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` DROP PRIMARY KEY;");
                _LOG.Debug("Drop primary key.");
            }
            if (__columns.TryGetValue("ID", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` CHANGE COLUMN `ID` `ID` INT" + (__value.IsPrimary ? "" : " PRIMARY KEY") + " AUTO_INCREMENT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_CENTER_USER DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD COLUMN `ID` INT PRIMARY KEY AUTO_INCREMENT;");
                _LOG.Debug("Add column[`{0}`].", "ID");
            }
            if (__columns.TryGetValue("RegisterTime", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` CHANGE COLUMN `RegisterTime` `RegisterTime` DATETIME;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD INDEX(`RegisterTime`);");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD COLUMN `RegisterTime` DATETIME;");
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD INDEX(`RegisterTime`);");
                _LOG.Debug("Add index[`{0}`].", "RegisterTime");
                _LOG.Debug("Add column[`{0}`].", "RegisterTime");
            }
            if (__columns.TryGetValue("Token", out __value))
            {
                if (__value.DATA_TYPE != "text" && (__value.IsIndex || __value.IsUnique))
                {
                    __value.COLUMN_KEY = null;
                    builder.AppendLine("ALTER TABLE T_CENTER_USER DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` CHANGE COLUMN `Token` `Token` TEXT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD INDEX(`Token`(10));");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD COLUMN `Token` TEXT;");
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD INDEX(`Token`(10));");
                _LOG.Debug("Add index[`{0}`].", "Token");
                _LOG.Debug("Add column[`{0}`].", "Token");
            }
            if (__columns.TryGetValue("Account", out __value))
            {
                if (__value.DATA_TYPE != "text" && (__value.IsIndex || __value.IsUnique))
                {
                    __value.COLUMN_KEY = null;
                    builder.AppendLine("ALTER TABLE T_CENTER_USER DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` CHANGE COLUMN `Account` `Account` TEXT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD INDEX(`Account`(10));");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD COLUMN `Account` TEXT;");
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD INDEX(`Account`(10));");
                _LOG.Debug("Add index[`{0}`].", "Account");
                _LOG.Debug("Add column[`{0}`].", "Account");
            }
            if (__columns.TryGetValue("Password", out __value))
            {
                if (__value.DATA_TYPE != "text" && (__value.IsIndex || __value.IsUnique))
                {
                    __value.COLUMN_KEY = null;
                    builder.AppendLine("ALTER TABLE T_CENTER_USER DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` CHANGE COLUMN `Password` `Password` TEXT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD INDEX(`Password`(10));");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD COLUMN `Password` TEXT;");
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD INDEX(`Password`(10));");
                _LOG.Debug("Add index[`{0}`].", "Password");
                _LOG.Debug("Add column[`{0}`].", "Password");
            }
            if (__columns.TryGetValue("Phone", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` CHANGE COLUMN `Phone` `Phone` BIGINT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD INDEX(`Phone`);");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD COLUMN `Phone` BIGINT;");
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD INDEX(`Phone`);");
                _LOG.Debug("Add index[`{0}`].", "Phone");
                _LOG.Debug("Add column[`{0}`].", "Phone");
            }
            if (__columns.TryGetValue("Name", out __value))
            {
                if (__value.DATA_TYPE != "text" && (__value.IsIndex || __value.IsUnique))
                {
                    __value.COLUMN_KEY = null;
                    builder.AppendLine("ALTER TABLE T_CENTER_USER DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` CHANGE COLUMN `Name` `Name` TEXT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD INDEX(`Name`(10));");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD COLUMN `Name` TEXT;");
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD INDEX(`Name`(10));");
                _LOG.Debug("Add index[`{0}`].", "Name");
                _LOG.Debug("Add column[`{0}`].", "Name");
            }
            if (__columns.TryGetValue("LastLoginTime", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` CHANGE COLUMN `LastLoginTime` `LastLoginTime` DATETIME;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_CENTER_USER DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_CENTER_USER` ADD COLUMN `LastLoginTime` DATETIME;");
                _LOG.Debug("Add column[`{0}`].", "LastLoginTime");
            }
            if (IsDropColumn)
            {
                foreach (var __column in __columns.Keys)
                {
                    builder.AppendLine("ALTER TABLE `T_CENTER_USER` DROP COLUMN `{0}`;", __column);
                    _LOG.Debug("Drop column[`{0}`].", __column);
                }
            }
            
            cmd.CommandText = builder.ToString();
            _LOG.Info("Building table[`T_CENTER_USER`] structure.");
            cmd.ExecuteNonQuery();
            #endregion
            
            #region Table structure "T_OPLog"
            _LOG.Info("Begin update table[`T_OPLog`] structure.");
            __columns.Clear();
            builder.Remove(0, builder.Length);
            cmd.CommandText = "SELECT COLUMN_NAME, COLUMN_KEY, EXTRA, DATA_TYPE FROM information_schema.COLUMNS WHERE TABLE_NAME = 'T_OPLog' AND TABLE_SCHEMA = '" + conn.Database + "';";
            reader = cmd.ExecuteReader();
            __hasPrimary = false;
            foreach (var __column in _DATABASE.ReadMultiple<MYSQL_TABLE_COLUMN>(reader))
            {
                if (__column.IsPrimary) __hasPrimary = true;
                __columns.Add(__column.COLUMN_NAME, __column);
            }
            reader.Close();
            __noneChangePrimary = true;
            __noneChangePrimary &= (__columns.TryGetValue("ID", out __value) && __value.IsPrimary);
            if (!__noneChangePrimary && __hasPrimary)
            {
                var pk = __columns.Values.FirstOrDefault(f => f.IsPrimary);
                if (pk.IsIdentity) builder.AppendLine("ALTER TABLE `T_OPLog` CHANGE COLUMN `{0}` `{0}` {1};", pk.COLUMN_NAME, pk.DATA_TYPE);
                builder.AppendLine("ALTER TABLE `T_OPLog` DROP PRIMARY KEY;");
                _LOG.Debug("Drop primary key.");
            }
            if (__columns.TryGetValue("ID", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_OPLog` CHANGE COLUMN `ID` `ID` INT" + (__value.IsPrimary ? "" : " PRIMARY KEY") + " AUTO_INCREMENT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_OPLog DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_OPLog` ADD COLUMN `ID` INT PRIMARY KEY AUTO_INCREMENT;");
                _LOG.Debug("Add column[`{0}`].", "ID");
            }
            if (__columns.TryGetValue("PID", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_OPLog` CHANGE COLUMN `PID` `PID` INT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_OPLog` ADD INDEX(`PID`);");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_OPLog` ADD COLUMN `PID` INT;");
                builder.AppendLine("ALTER TABLE `T_OPLog` ADD INDEX(`PID`);");
                _LOG.Debug("Add index[`{0}`].", "PID");
                _LOG.Debug("Add column[`{0}`].", "PID");
            }
            if (__columns.TryGetValue("Operation", out __value))
            {
                if (__value.DATA_TYPE != "text" && (__value.IsIndex || __value.IsUnique))
                {
                    __value.COLUMN_KEY = null;
                    builder.AppendLine("ALTER TABLE T_OPLog DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                builder.AppendLine("ALTER TABLE `T_OPLog` CHANGE COLUMN `Operation` `Operation` TEXT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_OPLog` ADD INDEX(`Operation`(10));");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_OPLog` ADD COLUMN `Operation` TEXT;");
                builder.AppendLine("ALTER TABLE `T_OPLog` ADD INDEX(`Operation`(10));");
                _LOG.Debug("Add index[`{0}`].", "Operation");
                _LOG.Debug("Add column[`{0}`].", "Operation");
            }
            if (__columns.TryGetValue("Time", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_OPLog` CHANGE COLUMN `Time` `Time` DATETIME;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_OPLog` ADD INDEX(`Time`);");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_OPLog` ADD COLUMN `Time` DATETIME;");
                builder.AppendLine("ALTER TABLE `T_OPLog` ADD INDEX(`Time`);");
                _LOG.Debug("Add index[`{0}`].", "Time");
                _LOG.Debug("Add column[`{0}`].", "Time");
            }
            if (__columns.TryGetValue("Way", out __value))
            {
                if (__value.DATA_TYPE != "text" && (__value.IsIndex || __value.IsUnique))
                {
                    __value.COLUMN_KEY = null;
                    builder.AppendLine("ALTER TABLE T_OPLog DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                builder.AppendLine("ALTER TABLE `T_OPLog` CHANGE COLUMN `Way` `Way` TEXT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_OPLog` ADD INDEX(`Way`(10));");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_OPLog` ADD COLUMN `Way` TEXT;");
                builder.AppendLine("ALTER TABLE `T_OPLog` ADD INDEX(`Way`(10));");
                _LOG.Debug("Add index[`{0}`].", "Way");
                _LOG.Debug("Add column[`{0}`].", "Way");
            }
            if (__columns.TryGetValue("Sign", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_OPLog` CHANGE COLUMN `Sign` `Sign` INT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_OPLog` ADD INDEX(`Sign`);");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_OPLog` ADD COLUMN `Sign` INT;");
                builder.AppendLine("ALTER TABLE `T_OPLog` ADD INDEX(`Sign`);");
                _LOG.Debug("Add index[`{0}`].", "Sign");
                _LOG.Debug("Add column[`{0}`].", "Sign");
            }
            if (__columns.TryGetValue("Statistic", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_OPLog` CHANGE COLUMN `Statistic` `Statistic` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_OPLog DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_OPLog` ADD COLUMN `Statistic` INT;");
                _LOG.Debug("Add column[`{0}`].", "Statistic");
            }
            if (__columns.TryGetValue("Detail", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_OPLog` CHANGE COLUMN `Detail` `Detail` TEXT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_OPLog DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_OPLog` ADD COLUMN `Detail` TEXT;");
                _LOG.Debug("Add column[`{0}`].", "Detail");
            }
            if (IsDropColumn)
            {
                foreach (var __column in __columns.Keys)
                {
                    builder.AppendLine("ALTER TABLE `T_OPLog` DROP COLUMN `{0}`;", __column);
                    _LOG.Debug("Drop column[`{0}`].", __column);
                }
            }
            
            cmd.CommandText = builder.ToString();
            _LOG.Info("Building table[`T_OPLog`] structure.");
            cmd.ExecuteNonQuery();
            #endregion
            if (!__exists)
            {
                _LOG.Info("The first time to construct database.");
                if (OnConstructDatabase != null) OnConstructDatabase(database);
            }
        }
        /// <summary>/* Phase说明 */ BuildTemp: 原库，可用于延长Timeout，_DB操作原库 / ChangeTemp: 临时库，可用于修改主键可能重复的数据，_DB操作临时库 / Merge: 临时库，可用于修改需要参考其它合服数据的数据，_DB操作目标库</summary>
        public static void MERGE(MergeDatabase[] dbs, Action<_DATABASE.Database> phaseBuildTemp, Action<_DATABASE.Database> phaseChangeTemp, Action<_DATABASE.Database[]> phaseMerge)
        {
            _DATABASE.Database __target = _DAO;
            if (__target == null) throw new ArgumentNullException("_DAO");
            if (__target.Available) throw new InvalidOperationException("_DAO can't be available.");
            _DATABASE.Database[] sources = new _DATABASE.Database[dbs.Length];
            int __T_USER_ID = 0;
            int __T_CENTER_USER_ID = 0;
            int __T_OPLog_ID = 0;
            
            #region create temp database
            for (int i = 0; i < sources.Length; i++)
            {
                _DATABASE.Database db = new ConnectionPool() { Base = new MYSQL_DATABASE() };
                db.ConnectionString = dbs[i].ConnectionStringWithDB;
                db.OnTestConnection = (__conn, __db) =>
                {
                    string __temp = "TEMP_" + db.DatabaseName;
                    db.ExecuteNonQuery(string.Format("DROP DATABASE IF EXISTS `{0}`; CREATE DATABASE `{0}`;", __temp));
                    __conn.ChangeDatabase(__temp);
                };
                db.OnTestConnection += UPDATE_DATABASE_STRUCTURE;
                db.OnTestConnection += (__conn, __db) => __conn.ChangeDatabase(db.DatabaseName);
                db.TestConnection();
                _DAO = db;
                if (phaseBuildTemp != null) phaseBuildTemp(db);
                sources[i] = db;
                string dbName = db.DatabaseName;
                string tempName = "TEMP_" + dbName;
                _LOG.Info("Begin build temp database[{0}].", dbName);
                
                if (dbs[i].Tables == null) dbs[i].Tables = AllMergeTable.ToArray();
                
                StringBuilder builder = new StringBuilder();
                string result;
                MergeTable table;
                table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_USER");
                if (table != null)
                {
                    builder.Append("INSERT INTO {0}.{1} SELECT {1}.`Platform`,{1}.`ID`,{1}.`RegisterTime`,{1}.`Token`,{1}.`Account`,{1}.`Password`,{1}.`Phone`,{1}.`Name`,{1}.`LastLoginTime` FROM {2}.{1}", tempName, table.TableName, dbName);
                    if (!string.IsNullOrEmpty(table.Where)) builder.Append(" " + table.Where);
                    builder.AppendLine(";");
                    result = builder.ToString();
                    builder.Remove(0, builder.Length);
                    _LOG.Info("Merge table[`{0}`] data.", table.TableName);
                    _LOG.Debug("SQL: {0}", result);
                    db.ExecuteNonQuery(result);
                }
                table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_CENTER_USER");
                if (table != null)
                {
                    builder.Append("INSERT INTO {0}.{1} SELECT {1}.`ID`,{1}.`RegisterTime`,{1}.`Token`,{1}.`Account`,{1}.`Password`,{1}.`Phone`,{1}.`Name`,{1}.`LastLoginTime` FROM {2}.{1}", tempName, table.TableName, dbName);
                    if (!string.IsNullOrEmpty(table.Where)) builder.Append(" " + table.Where);
                    builder.AppendLine(";");
                    result = builder.ToString();
                    builder.Remove(0, builder.Length);
                    _LOG.Info("Merge table[`{0}`] data.", table.TableName);
                    _LOG.Debug("SQL: {0}", result);
                    db.ExecuteNonQuery(result);
                }
                table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_OPLog");
                if (table != null)
                {
                    builder.Append("INSERT INTO {0}.{1} SELECT {1}.`ID`,{1}.`PID`,{1}.`Operation`,{1}.`Time`,{1}.`Way`,{1}.`Sign`,{1}.`Statistic`,{1}.`Detail` FROM {2}.{1}", tempName, table.TableName, dbName);
                    if (!string.IsNullOrEmpty(table.Where)) builder.Append(" " + table.Where);
                    builder.AppendLine(";");
                    result = builder.ToString();
                    builder.Remove(0, builder.Length);
                    _LOG.Info("Merge table[`{0}`] data.", table.TableName);
                    _LOG.Debug("SQL: {0}", result);
                    db.ExecuteNonQuery(result);
                }
                _LOG.Info("Build temp database[{0}] completed.", dbName);
                db.OnCreateConnection = (conn, __db) => conn.ChangeDatabase(tempName);
                // 对临时数据库的表自动修改自增列
                table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_USER");
                if (table.AutoMergeIdentity)
                {
                    UpdateIdentityKey_T_USER_ID(ref __T_USER_ID);
                    _LOG.Info("自动修改自增列`T_USER`.ID");
                }
                table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_CENTER_USER");
                if (table.AutoMergeIdentity)
                {
                    UpdateIdentityKey_T_CENTER_USER_ID(ref __T_CENTER_USER_ID);
                    _LOG.Info("自动修改自增列`T_CENTER_USER`.ID");
                }
                table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_OPLog");
                if (table.AutoMergeIdentity)
                {
                    UpdateIdentityKey_T_OPLog_ID(ref __T_OPLog_ID);
                    _LOG.Info("自动修改自增列`T_OPLog`.ID");
                }
                if (phaseChangeTemp != null) phaseChangeTemp(db);
            }
            #endregion
            
            _LOG.Info("Build all temp database completed.");
            #region import data in temp database to merge target
            
            if (phaseMerge != null) phaseMerge(sources);
            _DAO = __target;
            _DAO.OnCreateConnection = (conn, __db) =>
            {
                if (string.IsNullOrEmpty(conn.Database) && !string.IsNullOrEmpty(DatabaseName))
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = string.Format("DROP DATABASE IF EXISTS `{0}`; CREATE DATABASE `{0}`;", DatabaseName);
                    cmd.ExecuteNonQuery();
                    conn.ChangeDatabase(DatabaseName);
                    _DAO.ConnectionString += string.Format("Database={0};", DatabaseName);
                }
            };
            _DAO.TestConnection();
            for (int i = 0; i < sources.Length; i++)
            {
                var db = sources[i];
                var tables = dbs[i].Tables;
                string tempName = "TEMP_" + db.DatabaseName;
                _LOG.Info("Begin merge from temp database[`{0}`].", db.DatabaseName);
                if (db.DataSource == _DAO.DataSource)
                {
                    for (int j = 0; j < tables.Length; j++)
                    {
                        _LOG.Debug("Merge table[`{0}`].", tables[j].TableName);
                        _DAO.ExecuteNonQuery(string.Format("INSERT INTO {1} SELECT * FROM {0}.{1};", tempName, tables[j].TableName));
                    }
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    MergeTable table;
                    table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_USER");
                    if (table != null)
                    {
                        _LOG.Debug("Merge table[`{0}`].", table.TableName);
                        var list = db.SelectObjects<T_USER>("SELECT * FROM T_USER;");
                        if (list.Count > 0)
                        {
                            foreach (var item in list)
                            {
                                _T_USER.Insert(item);
                            }
                        }
                    }
                    table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_CENTER_USER");
                    if (table != null)
                    {
                        _LOG.Debug("Merge table[`{0}`].", table.TableName);
                        var list = db.SelectObjects<T_CENTER_USER>("SELECT * FROM T_CENTER_USER;");
                        if (list.Count > 0)
                        {
                            foreach (var item in list)
                            {
                                _T_CENTER_USER.Insert(item);
                            }
                        }
                    }
                    table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_OPLog");
                    if (table != null)
                    {
                        _LOG.Debug("Merge table[`{0}`].", table.TableName);
                        var list = db.SelectObjects<T_OPLog>("SELECT * FROM T_OPLog;");
                        if (list.Count > 0)
                        {
                            foreach (var item in list)
                            {
                                _T_OPLog.Insert(item);
                            }
                        }
                    }
                }
                _LOG.Info("Merge database[`{0}`] completed!", db.DatabaseName);
                db.ExecuteNonQuery("DROP DATABASE " + tempName);
                db.Dispose();
            }
            #endregion
        }
        public static void UpdateIdentityKey_T_USER_ID(ref int start)
        {
            int min = _DAO.ExecuteScalar<int>("SELECT MIN(`ID`) FROM `T_USER`;");
            int max = _DAO.ExecuteScalar<int>("SELECT MAX(`ID`) FROM `T_USER`;");
            if (start > 0)
            {
                if (min > start) min = start - min;
                else min = Math.Max(start, max + 1) - min;
                start = max + min + 1;
            }
            else
            {
                start = max + 1;
                return;
            }
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("UPDATE `T_USER` SET `ID` = `ID` + @p0;");
            _DAO.ExecuteNonQuery(builder.ToString(), min);
        }
        public static void UpdateIdentityKey_T_CENTER_USER_ID(ref int start)
        {
            int min = _DAO.ExecuteScalar<int>("SELECT MIN(`ID`) FROM `T_CENTER_USER`;");
            int max = _DAO.ExecuteScalar<int>("SELECT MAX(`ID`) FROM `T_CENTER_USER`;");
            if (start > 0)
            {
                if (min > start) min = start - min;
                else min = Math.Max(start, max + 1) - min;
                start = max + min + 1;
            }
            else
            {
                start = max + 1;
                return;
            }
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("UPDATE `T_CENTER_USER` SET `ID` = `ID` + @p0;");
            _DAO.ExecuteNonQuery(builder.ToString(), min);
        }
        public static void UpdateIdentityKey_T_OPLog_ID(ref int start)
        {
            int min = _DAO.ExecuteScalar<int>("SELECT MIN(`ID`) FROM `T_OPLog`;");
            int max = _DAO.ExecuteScalar<int>("SELECT MAX(`ID`) FROM `T_OPLog`;");
            if (start > 0)
            {
                if (min > start) min = start - min;
                else min = Math.Max(start, max + 1) - min;
                start = max + min + 1;
            }
            else
            {
                start = max + 1;
                return;
            }
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("UPDATE `T_OPLog` SET `ID` = `ID` + @p0;");
            _DAO.ExecuteNonQuery(builder.ToString(), min);
        }
        public partial class _T_USER : T_USER
        {
            public static ET_USER[] FIELD_ALL = { ET_USER.Platform, ET_USER.ID, ET_USER.RegisterTime, ET_USER.Token, ET_USER.Account, ET_USER.Password, ET_USER.Phone, ET_USER.Name, ET_USER.LastLoginTime };
            public static ET_USER[] FIELD_UPDATE = { ET_USER.Platform, ET_USER.RegisterTime, ET_USER.Token, ET_USER.Account, ET_USER.Password, ET_USER.Phone, ET_USER.Name, ET_USER.LastLoginTime };
            public static ET_USER[] NoNeedField(params ET_USER[] noNeed)
            {
                if (noNeed.Length == 0) return FIELD_ALL;
                List<ET_USER> list = new List<ET_USER>(FIELD_ALL.Length);
                for (int i = 0; i < FIELD_ALL.Length; i++)
                {
                    if (!noNeed.Contains(FIELD_ALL[i])) list.Add(FIELD_ALL[i]);
                }
                return list.ToArray();
            }
            public static int FieldCount { get { return FIELD_ALL.Length; } }
            
            public static T_USER Read(IDataReader reader)
            {
                return Read(reader, 0, FieldCount);
            }
            public static T_USER Read(IDataReader reader, int offset)
            {
                return Read(reader, offset, FieldCount);
            }
            public static T_USER Read(IDataReader reader, int offset, int fieldCount)
            {
                return _DATABASE.ReadObject<T_USER>(reader, offset, fieldCount);
            }
            public static void MultiReadPrepare(IDataReader reader, int offset, int fieldCount, out List<PropertyInfo> properties, out List<FieldInfo> fields, ref int[] indices)
            {
                _DATABASE.MultiReadPrepare(reader, typeof(T_USER), offset, fieldCount, out properties, out fields, ref indices);
            }
            public static T_USER MultiRead(IDataReader reader, int offset, int fieldCount, List<PropertyInfo> properties, List<FieldInfo> fields, int[] indices)
            {
                return _DATABASE.MultiRead<T_USER>(reader, offset, fieldCount, properties, fields, indices);
            }
            public static void GetInsertSQL(T_USER target, StringBuilder builder, List<object> values)
            {
                int index = values.Count;
                builder.AppendFormat("INSERT `T_USER`(`Platform`, `RegisterTime`, `Token`, `Account`, `Password`, `Phone`, `Name`, `LastLoginTime`) VALUES(");
                for (int i = 0, n = 7; i <= n; i++)
                {
                    builder.AppendFormat("@p{0}", index++);
                    if (i != n) builder.Append(", ");
                }
                builder.AppendLine(");");
                values.Add(target.Platform);
                values.Add(target.RegisterTime);
                values.Add(target.Token);
                values.Add(target.Account);
                values.Add(target.Password);
                values.Add(target.Phone);
                values.Add(target.Name);
                values.Add(target.LastLoginTime);
            }
            public static int Insert(T_USER target)
            {
                StringBuilder builder = new StringBuilder();
                List<object> values = new List<object>(9);
                GetInsertSQL(target, builder, values);
                builder.Append("SELECT LAST_INSERT_ID();");
                target.ID = _DAO.SelectValue<int>(builder.ToString(), values.ToArray());
                return target.ID;
            }
            public static void GetDeleteSQL(int ID, StringBuilder builder, List<object> values)
            {
                int index = values.Count;
                builder.AppendFormat("DELETE FROM `T_USER` WHERE `ID` = @p{0};", index++);
                values.Add(ID);
            }
            public static int Delete(int ID)
            {
                return _DAO.ExecuteNonQuery("DELETE FROM `T_USER` WHERE `ID` = @p0", ID);
            }
            public static int DeleteByPlatform(string Platform)
            {
                return _DAO.ExecuteNonQuery("DELETE FROM `T_USER` WHERE `Platform` = @p0;", Platform);
            }
            public static void GetUpdateSQL(T_USER target, string condition, StringBuilder builder, List<object> values, params ET_USER[] fields)
            {
                int index = values.Count;
                bool all = fields.Length == 0 || fields == FIELD_UPDATE;
                builder.Append("UPDATE `T_USER` SET");
                if (all || fields.Contains(ET_USER.Platform))
                {
                    builder.AppendFormat(" `Platform` = @p{0},", index++);
                    values.Add(target.Platform);
                }
                if (all || fields.Contains(ET_USER.RegisterTime))
                {
                    builder.AppendFormat(" `RegisterTime` = @p{0},", index++);
                    values.Add(target.RegisterTime);
                }
                if (all || fields.Contains(ET_USER.Token))
                {
                    builder.AppendFormat(" `Token` = @p{0},", index++);
                    values.Add(target.Token);
                }
                if (all || fields.Contains(ET_USER.Account))
                {
                    builder.AppendFormat(" `Account` = @p{0},", index++);
                    values.Add(target.Account);
                }
                if (all || fields.Contains(ET_USER.Password))
                {
                    builder.AppendFormat(" `Password` = @p{0},", index++);
                    values.Add(target.Password);
                }
                if (all || fields.Contains(ET_USER.Phone))
                {
                    builder.AppendFormat(" `Phone` = @p{0},", index++);
                    values.Add(target.Phone);
                }
                if (all || fields.Contains(ET_USER.Name))
                {
                    builder.AppendFormat(" `Name` = @p{0},", index++);
                    values.Add(target.Name);
                }
                if (all || fields.Contains(ET_USER.LastLoginTime))
                {
                    builder.AppendFormat(" `LastLoginTime` = @p{0},", index++);
                    values.Add(target.LastLoginTime);
                }
                if (index == 0) return;
                builder[builder.Length - 1] = ' ';
                if (!string.IsNullOrEmpty(condition)) builder.Append(condition);
                else
                {
                    builder.AppendFormat("WHERE `ID` = @p{0}", index++);
                    values.Add(target.ID);
                }
                builder.AppendLine(";");
            }
            /// <summary>condition that 'where' or 'join' without ';'</summary>
            public static int Update(T_USER target, string condition, params ET_USER[] fields)
            {
                StringBuilder builder = new StringBuilder();
                List<object> values = new List<object>(fields.Length + 1);
                GetUpdateSQL(target, condition, builder, values, fields);
                return _DAO.ExecuteNonQuery(builder.ToString(), values.ToArray());
            }
            public static void GetSelectField(string tableName, StringBuilder builder, params ET_USER[] fields)
            {
                if (string.IsNullOrEmpty(tableName)) tableName = "`T_USER`";
                int count = fields == null ? 0 : fields.Length;
                if (count == 0)
                {
                    builder.Append("{0}.*", tableName);
                    return;
                }
                count--;
                for (int i = 0; i <= count; i++)
                {
                    builder.Append("{0}.{1}", tableName, fields[i].ToString());
                    if (i != count) builder.Append(",");
                }
            }
            public static StringBuilder GetSelectSQL(params ET_USER[] fields)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("SELECT ");
                GetSelectField(null, builder, fields);
                builder.AppendLine(" FROM `T_USER`");
                return builder;
            }
            public static T_USER Select(int __ID, params ET_USER[] fields)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.Append(" WHERE `ID` = @p0;");
                var ret = _DAO.SelectObject<T_USER>(builder.ToString(), __ID);
                if (ret != default(T_USER))
                {
                    ret.ID = __ID;
                }
                return ret;
            }
            public static T_USER Select(ET_USER[] fields, string condition, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                return _DAO.SelectObject<T_USER>(builder.ToString(), param);
            }
            public static bool Exists(int __ID)
            {
                return _DAO.ExecuteScalar<bool>("SELECT EXISTS(SELECT 1 FROM `T_USER` WHERE `ID` = @p0)", __ID);
            }
            public static bool Exists2(string condition, params object[] param)
            {
                return _DAO.ExecuteScalar<bool>(string.Format("SELECT EXISTS(SELECT 1 FROM `T_USER` {0})", condition), param);
            }
            public static List<T_USER> SelectMultiple(ET_USER[] fields, string condition, params object[] param)
            {
                StringBuilder builder;
                if (fields == null || fields.Length == 0) builder = new StringBuilder("SELECT * FROM T_USER");
                else builder = GetSelectSQL(fields);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                return _DAO.SelectObjects<T_USER>(builder.ToString(), param);
            }
            public static List<T_USER> SelectMultipleByPlatform(ET_USER[] fields, string Platform, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `Platform` = @p{0}", param.Length + 0);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_USER>(builder.ToString(), Platform);
                else return _DAO.SelectObjects<T_USER>(builder.ToString(), param.Add(Platform));
            }
            public static PagedModel<T_USER> SelectPages(string __where, ET_USER[] fields, string conditionAfterWhere, int page, int pageSize, params object[] param)
            {
                var ret = SelectPages<T_USER>(__where, GetSelectSQL(fields).ToString(), conditionAfterWhere, page, pageSize, param);
                return ret;
            }
            public static PagedModel<T> SelectPages<T>(string __where, string selectSQL, string conditionAfterWhere, int page, int pageSize, params object[] param) where T : new()
            {
                return _DB.SelectPages<T>(_DAO, "SELECT count(`T_USER`.`Platform`) FROM `T_USER`", __where, selectSQL, conditionAfterWhere, page, pageSize, param);
            }
        }
        public partial class _T_CENTER_USER : T_CENTER_USER
        {
            public static ET_CENTER_USER[] FIELD_ALL = { ET_CENTER_USER.ID, ET_CENTER_USER.RegisterTime, ET_CENTER_USER.Token, ET_CENTER_USER.Account, ET_CENTER_USER.Password, ET_CENTER_USER.Phone, ET_CENTER_USER.Name, ET_CENTER_USER.LastLoginTime };
            public static ET_CENTER_USER[] FIELD_UPDATE = { ET_CENTER_USER.RegisterTime, ET_CENTER_USER.Token, ET_CENTER_USER.Account, ET_CENTER_USER.Password, ET_CENTER_USER.Phone, ET_CENTER_USER.Name, ET_CENTER_USER.LastLoginTime };
            public static ET_CENTER_USER[] NoNeedField(params ET_CENTER_USER[] noNeed)
            {
                if (noNeed.Length == 0) return FIELD_ALL;
                List<ET_CENTER_USER> list = new List<ET_CENTER_USER>(FIELD_ALL.Length);
                for (int i = 0; i < FIELD_ALL.Length; i++)
                {
                    if (!noNeed.Contains(FIELD_ALL[i])) list.Add(FIELD_ALL[i]);
                }
                return list.ToArray();
            }
            public static int FieldCount { get { return FIELD_ALL.Length; } }
            
            public static T_CENTER_USER Read(IDataReader reader)
            {
                return Read(reader, 0, FieldCount);
            }
            public static T_CENTER_USER Read(IDataReader reader, int offset)
            {
                return Read(reader, offset, FieldCount);
            }
            public static T_CENTER_USER Read(IDataReader reader, int offset, int fieldCount)
            {
                return _DATABASE.ReadObject<T_CENTER_USER>(reader, offset, fieldCount);
            }
            public static void MultiReadPrepare(IDataReader reader, int offset, int fieldCount, out List<PropertyInfo> properties, out List<FieldInfo> fields, ref int[] indices)
            {
                _DATABASE.MultiReadPrepare(reader, typeof(T_CENTER_USER), offset, fieldCount, out properties, out fields, ref indices);
            }
            public static T_CENTER_USER MultiRead(IDataReader reader, int offset, int fieldCount, List<PropertyInfo> properties, List<FieldInfo> fields, int[] indices)
            {
                return _DATABASE.MultiRead<T_CENTER_USER>(reader, offset, fieldCount, properties, fields, indices);
            }
            public static void GetInsertSQL(T_CENTER_USER target, StringBuilder builder, List<object> values)
            {
                int index = values.Count;
                builder.AppendFormat("INSERT `T_CENTER_USER`(`RegisterTime`, `Token`, `Account`, `Password`, `Phone`, `Name`, `LastLoginTime`) VALUES(");
                for (int i = 0, n = 6; i <= n; i++)
                {
                    builder.AppendFormat("@p{0}", index++);
                    if (i != n) builder.Append(", ");
                }
                builder.AppendLine(");");
                values.Add(target.RegisterTime);
                values.Add(target.Token);
                values.Add(target.Account);
                values.Add(target.Password);
                values.Add(target.Phone);
                values.Add(target.Name);
                values.Add(target.LastLoginTime);
            }
            public static int Insert(T_CENTER_USER target)
            {
                StringBuilder builder = new StringBuilder();
                List<object> values = new List<object>(8);
                GetInsertSQL(target, builder, values);
                builder.Append("SELECT LAST_INSERT_ID();");
                target.ID = _DAO.SelectValue<int>(builder.ToString(), values.ToArray());
                return target.ID;
            }
            public static void GetDeleteSQL(int ID, StringBuilder builder, List<object> values)
            {
                int index = values.Count;
                builder.AppendFormat("DELETE FROM `T_CENTER_USER` WHERE `ID` = @p{0};", index++);
                values.Add(ID);
            }
            public static int Delete(int ID)
            {
                return _DAO.ExecuteNonQuery("DELETE FROM `T_CENTER_USER` WHERE `ID` = @p0", ID);
            }
            public static void GetUpdateSQL(T_CENTER_USER target, string condition, StringBuilder builder, List<object> values, params ET_CENTER_USER[] fields)
            {
                int index = values.Count;
                bool all = fields.Length == 0 || fields == FIELD_UPDATE;
                builder.Append("UPDATE `T_CENTER_USER` SET");
                if (all || fields.Contains(ET_CENTER_USER.RegisterTime))
                {
                    builder.AppendFormat(" `RegisterTime` = @p{0},", index++);
                    values.Add(target.RegisterTime);
                }
                if (all || fields.Contains(ET_CENTER_USER.Token))
                {
                    builder.AppendFormat(" `Token` = @p{0},", index++);
                    values.Add(target.Token);
                }
                if (all || fields.Contains(ET_CENTER_USER.Account))
                {
                    builder.AppendFormat(" `Account` = @p{0},", index++);
                    values.Add(target.Account);
                }
                if (all || fields.Contains(ET_CENTER_USER.Password))
                {
                    builder.AppendFormat(" `Password` = @p{0},", index++);
                    values.Add(target.Password);
                }
                if (all || fields.Contains(ET_CENTER_USER.Phone))
                {
                    builder.AppendFormat(" `Phone` = @p{0},", index++);
                    values.Add(target.Phone);
                }
                if (all || fields.Contains(ET_CENTER_USER.Name))
                {
                    builder.AppendFormat(" `Name` = @p{0},", index++);
                    values.Add(target.Name);
                }
                if (all || fields.Contains(ET_CENTER_USER.LastLoginTime))
                {
                    builder.AppendFormat(" `LastLoginTime` = @p{0},", index++);
                    values.Add(target.LastLoginTime);
                }
                if (index == 0) return;
                builder[builder.Length - 1] = ' ';
                if (!string.IsNullOrEmpty(condition)) builder.Append(condition);
                else
                {
                    builder.AppendFormat("WHERE `ID` = @p{0}", index++);
                    values.Add(target.ID);
                }
                builder.AppendLine(";");
            }
            /// <summary>condition that 'where' or 'join' without ';'</summary>
            public static int Update(T_CENTER_USER target, string condition, params ET_CENTER_USER[] fields)
            {
                StringBuilder builder = new StringBuilder();
                List<object> values = new List<object>(fields.Length + 1);
                GetUpdateSQL(target, condition, builder, values, fields);
                return _DAO.ExecuteNonQuery(builder.ToString(), values.ToArray());
            }
            public static void GetSelectField(string tableName, StringBuilder builder, params ET_CENTER_USER[] fields)
            {
                if (string.IsNullOrEmpty(tableName)) tableName = "`T_CENTER_USER`";
                int count = fields == null ? 0 : fields.Length;
                if (count == 0)
                {
                    builder.Append("{0}.*", tableName);
                    return;
                }
                count--;
                for (int i = 0; i <= count; i++)
                {
                    builder.Append("{0}.{1}", tableName, fields[i].ToString());
                    if (i != count) builder.Append(",");
                }
            }
            public static StringBuilder GetSelectSQL(params ET_CENTER_USER[] fields)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("SELECT ");
                GetSelectField(null, builder, fields);
                builder.AppendLine(" FROM `T_CENTER_USER`");
                return builder;
            }
            public static T_CENTER_USER Select(int __ID, params ET_CENTER_USER[] fields)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.Append(" WHERE `ID` = @p0;");
                var ret = _DAO.SelectObject<T_CENTER_USER>(builder.ToString(), __ID);
                if (ret != default(T_CENTER_USER))
                {
                    ret.ID = __ID;
                }
                return ret;
            }
            public static T_CENTER_USER Select(ET_CENTER_USER[] fields, string condition, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                return _DAO.SelectObject<T_CENTER_USER>(builder.ToString(), param);
            }
            public static bool Exists(int __ID)
            {
                return _DAO.ExecuteScalar<bool>("SELECT EXISTS(SELECT 1 FROM `T_CENTER_USER` WHERE `ID` = @p0)", __ID);
            }
            public static bool Exists2(string condition, params object[] param)
            {
                return _DAO.ExecuteScalar<bool>(string.Format("SELECT EXISTS(SELECT 1 FROM `T_CENTER_USER` {0})", condition), param);
            }
            public static List<T_CENTER_USER> SelectMultiple(ET_CENTER_USER[] fields, string condition, params object[] param)
            {
                StringBuilder builder;
                if (fields == null || fields.Length == 0) builder = new StringBuilder("SELECT * FROM T_CENTER_USER");
                else builder = GetSelectSQL(fields);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                return _DAO.SelectObjects<T_CENTER_USER>(builder.ToString(), param);
            }
            public static PagedModel<T_CENTER_USER> SelectPages(string __where, ET_CENTER_USER[] fields, string conditionAfterWhere, int page, int pageSize, params object[] param)
            {
                var ret = SelectPages<T_CENTER_USER>(__where, GetSelectSQL(fields).ToString(), conditionAfterWhere, page, pageSize, param);
                return ret;
            }
            public static PagedModel<T> SelectPages<T>(string __where, string selectSQL, string conditionAfterWhere, int page, int pageSize, params object[] param) where T : new()
            {
                return _DB.SelectPages<T>(_DAO, "SELECT count(`T_CENTER_USER`.`ID`) FROM `T_CENTER_USER`", __where, selectSQL, conditionAfterWhere, page, pageSize, param);
            }
        }
        public partial class _T_OPLog : T_OPLog
        {
            public static ET_OPLog[] FIELD_ALL = { ET_OPLog.ID, ET_OPLog.PID, ET_OPLog.Operation, ET_OPLog.Time, ET_OPLog.Way, ET_OPLog.Sign, ET_OPLog.Statistic, ET_OPLog.Detail };
            public static ET_OPLog[] FIELD_UPDATE = { ET_OPLog.PID, ET_OPLog.Operation, ET_OPLog.Time, ET_OPLog.Way, ET_OPLog.Sign, ET_OPLog.Statistic, ET_OPLog.Detail };
            public static ET_OPLog[] NoNeedField(params ET_OPLog[] noNeed)
            {
                if (noNeed.Length == 0) return FIELD_ALL;
                List<ET_OPLog> list = new List<ET_OPLog>(FIELD_ALL.Length);
                for (int i = 0; i < FIELD_ALL.Length; i++)
                {
                    if (!noNeed.Contains(FIELD_ALL[i])) list.Add(FIELD_ALL[i]);
                }
                return list.ToArray();
            }
            public static int FieldCount { get { return FIELD_ALL.Length; } }
            
            public static T_OPLog Read(IDataReader reader)
            {
                return Read(reader, 0, FieldCount);
            }
            public static T_OPLog Read(IDataReader reader, int offset)
            {
                return Read(reader, offset, FieldCount);
            }
            public static T_OPLog Read(IDataReader reader, int offset, int fieldCount)
            {
                return _DATABASE.ReadObject<T_OPLog>(reader, offset, fieldCount);
            }
            public static void MultiReadPrepare(IDataReader reader, int offset, int fieldCount, out List<PropertyInfo> properties, out List<FieldInfo> fields, ref int[] indices)
            {
                _DATABASE.MultiReadPrepare(reader, typeof(T_OPLog), offset, fieldCount, out properties, out fields, ref indices);
            }
            public static T_OPLog MultiRead(IDataReader reader, int offset, int fieldCount, List<PropertyInfo> properties, List<FieldInfo> fields, int[] indices)
            {
                return _DATABASE.MultiRead<T_OPLog>(reader, offset, fieldCount, properties, fields, indices);
            }
            public static void GetInsertSQL(T_OPLog target, StringBuilder builder, List<object> values)
            {
                int index = values.Count;
                builder.AppendFormat("INSERT `T_OPLog`(`PID`, `Operation`, `Time`, `Way`, `Sign`, `Statistic`, `Detail`) VALUES(");
                for (int i = 0, n = 6; i <= n; i++)
                {
                    builder.AppendFormat("@p{0}", index++);
                    if (i != n) builder.Append(", ");
                }
                builder.AppendLine(");");
                values.Add(target.PID);
                values.Add(target.Operation);
                values.Add(target.Time);
                values.Add(target.Way);
                values.Add(target.Sign);
                values.Add(target.Statistic);
                values.Add(target.Detail);
            }
            public static int Insert(T_OPLog target)
            {
                StringBuilder builder = new StringBuilder();
                List<object> values = new List<object>(8);
                GetInsertSQL(target, builder, values);
                builder.Append("SELECT LAST_INSERT_ID();");
                target.ID = _DAO.SelectValue<int>(builder.ToString(), values.ToArray());
                return target.ID;
            }
            public static void GetDeleteSQL(int ID, StringBuilder builder, List<object> values)
            {
                int index = values.Count;
                builder.AppendFormat("DELETE FROM `T_OPLog` WHERE `ID` = @p{0};", index++);
                values.Add(ID);
            }
            public static int Delete(int ID)
            {
                return _DAO.ExecuteNonQuery("DELETE FROM `T_OPLog` WHERE `ID` = @p0", ID);
            }
            public static int DeleteByPID(int PID)
            {
                return _DAO.ExecuteNonQuery("DELETE FROM `T_OPLog` WHERE `PID` = @p0;", PID);
            }
            public static int DeleteByOperation(string Operation)
            {
                return _DAO.ExecuteNonQuery("DELETE FROM `T_OPLog` WHERE `Operation` = @p0;", Operation);
            }
            public static int DeleteByWay(string Way)
            {
                return _DAO.ExecuteNonQuery("DELETE FROM `T_OPLog` WHERE `Way` = @p0;", Way);
            }
            public static int DeleteBySign(int Sign)
            {
                return _DAO.ExecuteNonQuery("DELETE FROM `T_OPLog` WHERE `Sign` = @p0;", Sign);
            }
            public static void GetUpdateSQL(T_OPLog target, string condition, StringBuilder builder, List<object> values, params ET_OPLog[] fields)
            {
                int index = values.Count;
                bool all = fields.Length == 0 || fields == FIELD_UPDATE;
                builder.Append("UPDATE `T_OPLog` SET");
                if (all || fields.Contains(ET_OPLog.PID))
                {
                    builder.AppendFormat(" `PID` = @p{0},", index++);
                    values.Add(target.PID);
                }
                if (all || fields.Contains(ET_OPLog.Operation))
                {
                    builder.AppendFormat(" `Operation` = @p{0},", index++);
                    values.Add(target.Operation);
                }
                if (all || fields.Contains(ET_OPLog.Time))
                {
                    builder.AppendFormat(" `Time` = @p{0},", index++);
                    values.Add(target.Time);
                }
                if (all || fields.Contains(ET_OPLog.Way))
                {
                    builder.AppendFormat(" `Way` = @p{0},", index++);
                    values.Add(target.Way);
                }
                if (all || fields.Contains(ET_OPLog.Sign))
                {
                    builder.AppendFormat(" `Sign` = @p{0},", index++);
                    values.Add(target.Sign);
                }
                if (all || fields.Contains(ET_OPLog.Statistic))
                {
                    builder.AppendFormat(" `Statistic` = @p{0},", index++);
                    values.Add(target.Statistic);
                }
                if (all || fields.Contains(ET_OPLog.Detail))
                {
                    builder.AppendFormat(" `Detail` = @p{0},", index++);
                    values.Add(target.Detail);
                }
                if (index == 0) return;
                builder[builder.Length - 1] = ' ';
                if (!string.IsNullOrEmpty(condition)) builder.Append(condition);
                else
                {
                    builder.AppendFormat("WHERE `ID` = @p{0}", index++);
                    values.Add(target.ID);
                }
                builder.AppendLine(";");
            }
            /// <summary>condition that 'where' or 'join' without ';'</summary>
            public static int Update(T_OPLog target, string condition, params ET_OPLog[] fields)
            {
                StringBuilder builder = new StringBuilder();
                List<object> values = new List<object>(fields.Length + 1);
                GetUpdateSQL(target, condition, builder, values, fields);
                return _DAO.ExecuteNonQuery(builder.ToString(), values.ToArray());
            }
            public static void GetSelectField(string tableName, StringBuilder builder, params ET_OPLog[] fields)
            {
                if (string.IsNullOrEmpty(tableName)) tableName = "`T_OPLog`";
                int count = fields == null ? 0 : fields.Length;
                if (count == 0)
                {
                    builder.Append("{0}.*", tableName);
                    return;
                }
                count--;
                for (int i = 0; i <= count; i++)
                {
                    builder.Append("{0}.{1}", tableName, fields[i].ToString());
                    if (i != count) builder.Append(",");
                }
            }
            public static StringBuilder GetSelectSQL(params ET_OPLog[] fields)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("SELECT ");
                GetSelectField(null, builder, fields);
                builder.AppendLine(" FROM `T_OPLog`");
                return builder;
            }
            public static T_OPLog Select(int __ID, params ET_OPLog[] fields)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.Append(" WHERE `ID` = @p0;");
                var ret = _DAO.SelectObject<T_OPLog>(builder.ToString(), __ID);
                if (ret != default(T_OPLog))
                {
                    ret.ID = __ID;
                }
                return ret;
            }
            public static T_OPLog Select(ET_OPLog[] fields, string condition, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                return _DAO.SelectObject<T_OPLog>(builder.ToString(), param);
            }
            public static bool Exists(int __ID)
            {
                return _DAO.ExecuteScalar<bool>("SELECT EXISTS(SELECT 1 FROM `T_OPLog` WHERE `ID` = @p0)", __ID);
            }
            public static bool Exists2(string condition, params object[] param)
            {
                return _DAO.ExecuteScalar<bool>(string.Format("SELECT EXISTS(SELECT 1 FROM `T_OPLog` {0})", condition), param);
            }
            public static List<T_OPLog> SelectMultiple(ET_OPLog[] fields, string condition, params object[] param)
            {
                StringBuilder builder;
                if (fields == null || fields.Length == 0) builder = new StringBuilder("SELECT * FROM T_OPLog");
                else builder = GetSelectSQL(fields);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param);
            }
            public static List<T_OPLog> SelectMultipleByPID(ET_OPLog[] fields, int PID, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `PID` = @p{0}", param.Length + 0);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), PID);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(PID));
            }
            public static List<T_OPLog> SelectMultipleByOperation(ET_OPLog[] fields, string Operation, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `Operation` = @p{0}", param.Length + 0);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), Operation);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(Operation));
            }
            public static List<T_OPLog> SelectMultipleByWay(ET_OPLog[] fields, string Way, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `Way` = @p{0}", param.Length + 0);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), Way);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(Way));
            }
            public static List<T_OPLog> SelectMultipleBySign(ET_OPLog[] fields, int Sign, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `Sign` = @p{0}", param.Length + 0);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), Sign);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(Sign));
            }
            public static List<T_OPLog> SelectMultipleByPID_Operation(ET_OPLog[] fields, int PID, string Operation, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `PID` = @p{0} AND `Operation` = @p{1}", param.Length + 0, param.Length + 1);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), PID, Operation);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(PID, Operation));
            }
            public static List<T_OPLog> SelectMultipleByPID_Way(ET_OPLog[] fields, int PID, string Way, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `PID` = @p{0} AND `Way` = @p{1}", param.Length + 0, param.Length + 1);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), PID, Way);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(PID, Way));
            }
            public static List<T_OPLog> SelectMultipleByPID_Sign(ET_OPLog[] fields, int PID, int Sign, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `PID` = @p{0} AND `Sign` = @p{1}", param.Length + 0, param.Length + 1);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), PID, Sign);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(PID, Sign));
            }
            public static List<T_OPLog> SelectMultipleByOperation_Way(ET_OPLog[] fields, string Operation, string Way, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `Operation` = @p{0} AND `Way` = @p{1}", param.Length + 0, param.Length + 1);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), Operation, Way);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(Operation, Way));
            }
            public static List<T_OPLog> SelectMultipleByOperation_Sign(ET_OPLog[] fields, string Operation, int Sign, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `Operation` = @p{0} AND `Sign` = @p{1}", param.Length + 0, param.Length + 1);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), Operation, Sign);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(Operation, Sign));
            }
            public static List<T_OPLog> SelectMultipleByWay_Sign(ET_OPLog[] fields, string Way, int Sign, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `Way` = @p{0} AND `Sign` = @p{1}", param.Length + 0, param.Length + 1);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), Way, Sign);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(Way, Sign));
            }
            public static List<T_OPLog> SelectMultipleByPID_Operation_Way(ET_OPLog[] fields, int PID, string Operation, string Way, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `PID` = @p{0} AND `Operation` = @p{1} AND `Way` = @p{2}", param.Length + 0, param.Length + 1, param.Length + 2);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), PID, Operation, Way);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(PID, Operation, Way));
            }
            public static List<T_OPLog> SelectMultipleByPID_Operation_Sign(ET_OPLog[] fields, int PID, string Operation, int Sign, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `PID` = @p{0} AND `Operation` = @p{1} AND `Sign` = @p{2}", param.Length + 0, param.Length + 1, param.Length + 2);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), PID, Operation, Sign);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(PID, Operation, Sign));
            }
            public static List<T_OPLog> SelectMultipleByPID_Way_Sign(ET_OPLog[] fields, int PID, string Way, int Sign, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `PID` = @p{0} AND `Way` = @p{1} AND `Sign` = @p{2}", param.Length + 0, param.Length + 1, param.Length + 2);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), PID, Way, Sign);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(PID, Way, Sign));
            }
            public static List<T_OPLog> SelectMultipleByOperation_Way_Sign(ET_OPLog[] fields, string Operation, string Way, int Sign, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `Operation` = @p{0} AND `Way` = @p{1} AND `Sign` = @p{2}", param.Length + 0, param.Length + 1, param.Length + 2);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), Operation, Way, Sign);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(Operation, Way, Sign));
            }
            public static List<T_OPLog> SelectMultipleByPID_Operation_Way_Sign(ET_OPLog[] fields, int PID, string Operation, string Way, int Sign, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `PID` = @p{0} AND `Operation` = @p{1} AND `Way` = @p{2} AND `Sign` = @p{3}", param.Length + 0, param.Length + 1, param.Length + 2, param.Length + 3);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_OPLog>(builder.ToString(), PID, Operation, Way, Sign);
                else return _DAO.SelectObjects<T_OPLog>(builder.ToString(), param.Add(PID, Operation, Way, Sign));
            }
            public static PagedModel<T_OPLog> SelectPages(string __where, ET_OPLog[] fields, string conditionAfterWhere, int page, int pageSize, params object[] param)
            {
                var ret = SelectPages<T_OPLog>(__where, GetSelectSQL(fields).ToString(), conditionAfterWhere, page, pageSize, param);
                return ret;
            }
            public static PagedModel<T> SelectPages<T>(string __where, string selectSQL, string conditionAfterWhere, int page, int pageSize, params object[] param) where T : new()
            {
                return _DB.SelectPages<T>(_DAO, "SELECT count(`T_OPLog`.`ID`) FROM `T_OPLog`", __where, selectSQL, conditionAfterWhere, page, pageSize, param);
            }
        }
        public static PagedModel<T> SelectPages<T>(_DATABASE.Database db, string selectCountSQL, string __where, string selectSQL, string conditionAfterWhere, int page, int pageSize, params object[] param) where T : new()
        {
            return SelectPages(db, selectCountSQL, __where, selectSQL, conditionAfterWhere, page, pageSize, new Action<IDataReader, List<T>>((reader, list) => { while (reader.Read()) list.Add(_DATABASE.ReadObject<T>(reader, 0, reader.FieldCount)); }), param);
        }
        public static PagedModel<T> SelectPages<T>(_DATABASE.Database db, string selectCountSQL, string __where, string selectSQL, string conditionAfterWhere, int page, int pageSize, Action<IDataReader, List<T>> read, params object[] param)
        {
            StringBuilder builder = new StringBuilder();
            PagedModel<T> result = new PagedModel<T>();
            if (page < 0)
            {
                builder.AppendLine("{0} {1} {2}", selectSQL, __where, conditionAfterWhere);
                db.ExecuteReader((reader) =>
                {
                    result.Models = new List<T>();
                    read(reader, result.Models);
                    result.Count = result.Models.Count;
                }
                , builder.ToString(), param);
                return result;
            }
            builder.AppendLine("{0} {1};", selectCountSQL, __where);
            builder.AppendLine("{0} {1} {2} LIMIT @p{3},@p{4};", selectSQL, __where, conditionAfterWhere, param.Length, param.Length + 1);
            object[] __param = new object[param.Length + 2];
            Array.Copy(param, __param, param.Length);
            __param[param.Length] = page * pageSize;
            __param[param.Length + 1] = pageSize;
            result.Page = page;
            result.PageSize = pageSize;
            db.ExecuteReader((reader) =>
            {
                if (reader.Read()) result.Count = (int)(long)reader[0];
                result.Models = new List<T>();
                reader.NextResult();
                read(reader, result.Models);
            }
            , builder.ToString(), __param);
            result.CalcTotalPage();
            return result;
        }
        public static void MasterSlave(string masterConnString, string slaveConnStrings)
        {
            Dictionary<string, string> dic = _DATABASE.ParseConnectionString(masterConnString, true);
            string host = dic["server"];
            string port = dic["port"];
            string user = dic["user"];
            string password = dic["password"];
            MASTER_STATUS masterStatus;
            using (MYSQL_DATABASE master = new MYSQL_DATABASE())
            {
                master.ConnectionString = masterConnString;
                master.TestConnection();
                masterStatus = master.SelectObject<MASTER_STATUS>("SHOW MASTER STATUS");
            }
            string user2 = null;
            string password2 = null;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("stream {");
            builder.AppendFormat("    upstream {0} {{", masterStatus.Binlog_Do_DB);
            builder.AppendLine();
            string[] slaves = slaveConnStrings.Split(',');
            for (int i = 0; i < slaves.Length; i++)
            {
                using (MYSQL_DATABASE slave = new MYSQL_DATABASE())
                {
                    var dic2 = _DATABASE.ParseConnectionString(slaves[i], true);
                    if (user2 == null) user2 = dic2["user"];
                    else if (user2 != dic2["user"]) throw new InvalidOperationException("从库作为分布式读库时登录用户名必须一致");
                    if (password2 == null) password2 = dic2["password"];
                    else if (password2 != dic2["password"]) throw new InvalidOperationException("从库作为分布式读库时登录密码必须一致");
                    builder.AppendLine("        server {0}:{1};", dic2["server"], dic2["port"]);
                    slave.ConnectionString = slaves[i];
                    slave.TestConnection();
                    var slaveStatus = slave.SelectObject<SLAVE_STATUS>("SHOW SLAVE STATUS");
                    if (slaveStatus == null || slaveStatus.IsRunning) continue;
                    slave.ExecuteNonQuery("CHANGE MASTER TO MASTER_HOST=@p0,MASTER_PORT=@p1,MASTER_USER=@p2,MASTER_PASSWORD=@p3,MASTER_LOG_FILE=@p4,MASTER_LOG_POS=@p5;", host, port, user, password, masterStatus.File, masterStatus.Position);
                }
            }
            builder.AppendLine("    }");
            builder.AppendLine("    server {");
            builder.AppendLine("        listen nginxport;");
            builder.AppendLine("        proxy_pass {0};", masterStatus.Binlog_Do_DB);
            builder.AppendLine("    }");
            builder.AppendLine("}");
            _LOG.Debug("Nginx配置文件代码\r\n{0}", builder.ToString());
            _LOG.Debug("服务器启动的主从数据库连接字符串配置命令");
            _LOG.Info("\"Server={0};Port={1};User={2};Password={3}; {4} Server=nginxip;Port=nginxport;User={5};Password={6};\"", host, port, user, password, masterStatus.Binlog_Do_DB, user2, password2);
        }
    }
}
