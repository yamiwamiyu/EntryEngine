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
    public enum ET_Register
    {
        ID,
        GameName,
        DeviceID,
        Channel,
        RegisterTime,
    }
    public enum ET_Login
    {
        ID,
        RegisterID,
        LoginTime,
        LastOnlineTime,
    }
    public enum ET_Analysis
    {
        RegisterID,
        Label,
        Name,
        OrderID,
        Count,
        Time,
    }
    public enum ET_Online
    {
        Time,
        GameName,
        Channel,
        Quarter0,
        Quarter1,
        Quarter2,
        Quarter3,
        Quarter4,
        Quarter5,
        Quarter6,
        Quarter7,
        Quarter8,
        Quarter9,
        Quarter10,
        Quarter11,
        Quarter12,
        Quarter13,
        Quarter14,
        Quarter15,
        Quarter16,
        Quarter17,
        Quarter18,
        Quarter19,
        Quarter20,
        Quarter21,
        Quarter22,
        Quarter23,
        Quarter24,
        Quarter25,
        Quarter26,
        Quarter27,
        Quarter28,
        Quarter29,
        Quarter30,
        Quarter31,
        Quarter32,
        Quarter33,
        Quarter34,
        Quarter35,
        Quarter36,
        Quarter37,
        Quarter38,
        Quarter39,
        Quarter40,
        Quarter41,
        Quarter42,
        Quarter43,
        Quarter44,
        Quarter45,
        Quarter46,
        Quarter47,
        Quarter48,
        Quarter49,
        Quarter50,
        Quarter51,
        Quarter52,
        Quarter53,
        Quarter54,
        Quarter55,
        Quarter56,
        Quarter57,
        Quarter58,
        Quarter59,
        Quarter60,
        Quarter61,
        Quarter62,
        Quarter63,
        Quarter64,
        Quarter65,
        Quarter66,
        Quarter67,
        Quarter68,
        Quarter69,
        Quarter70,
        Quarter71,
        Quarter72,
        Quarter73,
        Quarter74,
        Quarter75,
        Quarter76,
        Quarter77,
        Quarter78,
        Quarter79,
        Quarter80,
        Quarter81,
        Quarter82,
        Quarter83,
        Quarter84,
        Quarter85,
        Quarter86,
        Quarter87,
        Quarter88,
        Quarter89,
        Quarter90,
        Quarter91,
        Quarter92,
        Quarter93,
        Quarter94,
        Quarter95,
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
            new MergeTable("T_CENTER_USER"),
            new MergeTable("T_OPLog"),
            new MergeTable("T_Register"),
            new MergeTable("T_Login"),
            new MergeTable("T_Analysis"),
            new MergeTable("T_Online"),
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
            CREATE TABLE IF NOT EXISTS `T_Register`
            (
            `ID` INT PRIMARY KEY AUTO_INCREMENT,
            `GameName` TEXT,
            `DeviceID` TEXT,
            `Channel` TEXT,
            `RegisterTime` DATETIME
            );
            CREATE TABLE IF NOT EXISTS `T_Login`
            (
            `ID` INT PRIMARY KEY AUTO_INCREMENT,
            `RegisterID` INT,
            `LoginTime` DATETIME,
            `LastOnlineTime` DATETIME
            );
            CREATE TABLE IF NOT EXISTS `T_Analysis`
            (
            `RegisterID` INT,
            `Label` TEXT,
            `Name` TEXT,
            `OrderID` INT,
            `Count` INT,
            `Time` DATETIME
            );
            CREATE TABLE IF NOT EXISTS `T_Online`
            (
            `Time` DATETIME,
            `GameName` TEXT,
            `Channel` TEXT,
            `Quarter0` INT,
            `Quarter1` INT,
            `Quarter2` INT,
            `Quarter3` INT,
            `Quarter4` INT,
            `Quarter5` INT,
            `Quarter6` INT,
            `Quarter7` INT,
            `Quarter8` INT,
            `Quarter9` INT,
            `Quarter10` INT,
            `Quarter11` INT,
            `Quarter12` INT,
            `Quarter13` INT,
            `Quarter14` INT,
            `Quarter15` INT,
            `Quarter16` INT,
            `Quarter17` INT,
            `Quarter18` INT,
            `Quarter19` INT,
            `Quarter20` INT,
            `Quarter21` INT,
            `Quarter22` INT,
            `Quarter23` INT,
            `Quarter24` INT,
            `Quarter25` INT,
            `Quarter26` INT,
            `Quarter27` INT,
            `Quarter28` INT,
            `Quarter29` INT,
            `Quarter30` INT,
            `Quarter31` INT,
            `Quarter32` INT,
            `Quarter33` INT,
            `Quarter34` INT,
            `Quarter35` INT,
            `Quarter36` INT,
            `Quarter37` INT,
            `Quarter38` INT,
            `Quarter39` INT,
            `Quarter40` INT,
            `Quarter41` INT,
            `Quarter42` INT,
            `Quarter43` INT,
            `Quarter44` INT,
            `Quarter45` INT,
            `Quarter46` INT,
            `Quarter47` INT,
            `Quarter48` INT,
            `Quarter49` INT,
            `Quarter50` INT,
            `Quarter51` INT,
            `Quarter52` INT,
            `Quarter53` INT,
            `Quarter54` INT,
            `Quarter55` INT,
            `Quarter56` INT,
            `Quarter57` INT,
            `Quarter58` INT,
            `Quarter59` INT,
            `Quarter60` INT,
            `Quarter61` INT,
            `Quarter62` INT,
            `Quarter63` INT,
            `Quarter64` INT,
            `Quarter65` INT,
            `Quarter66` INT,
            `Quarter67` INT,
            `Quarter68` INT,
            `Quarter69` INT,
            `Quarter70` INT,
            `Quarter71` INT,
            `Quarter72` INT,
            `Quarter73` INT,
            `Quarter74` INT,
            `Quarter75` INT,
            `Quarter76` INT,
            `Quarter77` INT,
            `Quarter78` INT,
            `Quarter79` INT,
            `Quarter80` INT,
            `Quarter81` INT,
            `Quarter82` INT,
            `Quarter83` INT,
            `Quarter84` INT,
            `Quarter85` INT,
            `Quarter86` INT,
            `Quarter87` INT,
            `Quarter88` INT,
            `Quarter89` INT,
            `Quarter90` INT,
            `Quarter91` INT,
            `Quarter92` INT,
            `Quarter93` INT,
            `Quarter94` INT,
            `Quarter95` INT
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
            
            #region Table structure "T_Register"
            _LOG.Info("Begin update table[`T_Register`] structure.");
            __columns.Clear();
            builder.Remove(0, builder.Length);
            cmd.CommandText = "SELECT COLUMN_NAME, COLUMN_KEY, EXTRA, DATA_TYPE FROM information_schema.COLUMNS WHERE TABLE_NAME = 'T_Register' AND TABLE_SCHEMA = '" + conn.Database + "';";
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
                if (pk.IsIdentity) builder.AppendLine("ALTER TABLE `T_Register` CHANGE COLUMN `{0}` `{0}` {1};", pk.COLUMN_NAME, pk.DATA_TYPE);
                builder.AppendLine("ALTER TABLE `T_Register` DROP PRIMARY KEY;");
                _LOG.Debug("Drop primary key.");
            }
            if (__columns.TryGetValue("ID", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Register` CHANGE COLUMN `ID` `ID` INT" + (__value.IsPrimary ? "" : " PRIMARY KEY") + " AUTO_INCREMENT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Register DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Register` ADD COLUMN `ID` INT PRIMARY KEY AUTO_INCREMENT;");
                _LOG.Debug("Add column[`{0}`].", "ID");
            }
            if (__columns.TryGetValue("GameName", out __value))
            {
                if (__value.DATA_TYPE != "text" && (__value.IsIndex || __value.IsUnique))
                {
                    __value.COLUMN_KEY = null;
                    builder.AppendLine("ALTER TABLE T_Register DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                builder.AppendLine("ALTER TABLE `T_Register` CHANGE COLUMN `GameName` `GameName` TEXT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_Register` ADD INDEX(`GameName`(10));");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Register` ADD COLUMN `GameName` TEXT;");
                builder.AppendLine("ALTER TABLE `T_Register` ADD INDEX(`GameName`(10));");
                _LOG.Debug("Add index[`{0}`].", "GameName");
                _LOG.Debug("Add column[`{0}`].", "GameName");
            }
            if (__columns.TryGetValue("DeviceID", out __value))
            {
                if (__value.DATA_TYPE != "text" && (__value.IsIndex || __value.IsUnique))
                {
                    __value.COLUMN_KEY = null;
                    builder.AppendLine("ALTER TABLE T_Register DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                builder.AppendLine("ALTER TABLE `T_Register` CHANGE COLUMN `DeviceID` `DeviceID` TEXT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_Register` ADD INDEX(`DeviceID`(10));");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Register` ADD COLUMN `DeviceID` TEXT;");
                builder.AppendLine("ALTER TABLE `T_Register` ADD INDEX(`DeviceID`(10));");
                _LOG.Debug("Add index[`{0}`].", "DeviceID");
                _LOG.Debug("Add column[`{0}`].", "DeviceID");
            }
            if (__columns.TryGetValue("Channel", out __value))
            {
                if (__value.DATA_TYPE != "text" && (__value.IsIndex || __value.IsUnique))
                {
                    __value.COLUMN_KEY = null;
                    builder.AppendLine("ALTER TABLE T_Register DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                builder.AppendLine("ALTER TABLE `T_Register` CHANGE COLUMN `Channel` `Channel` TEXT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_Register` ADD INDEX(`Channel`(10));");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Register` ADD COLUMN `Channel` TEXT;");
                builder.AppendLine("ALTER TABLE `T_Register` ADD INDEX(`Channel`(10));");
                _LOG.Debug("Add index[`{0}`].", "Channel");
                _LOG.Debug("Add column[`{0}`].", "Channel");
            }
            if (__columns.TryGetValue("RegisterTime", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Register` CHANGE COLUMN `RegisterTime` `RegisterTime` DATETIME;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_Register` ADD INDEX(`RegisterTime`);");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Register` ADD COLUMN `RegisterTime` DATETIME;");
                builder.AppendLine("ALTER TABLE `T_Register` ADD INDEX(`RegisterTime`);");
                _LOG.Debug("Add index[`{0}`].", "RegisterTime");
                _LOG.Debug("Add column[`{0}`].", "RegisterTime");
            }
            if (IsDropColumn)
            {
                foreach (var __column in __columns.Keys)
                {
                    builder.AppendLine("ALTER TABLE `T_Register` DROP COLUMN `{0}`;", __column);
                    _LOG.Debug("Drop column[`{0}`].", __column);
                }
            }
            
            cmd.CommandText = builder.ToString();
            _LOG.Info("Building table[`T_Register`] structure.");
            cmd.ExecuteNonQuery();
            #endregion
            
            #region Table structure "T_Login"
            _LOG.Info("Begin update table[`T_Login`] structure.");
            __columns.Clear();
            builder.Remove(0, builder.Length);
            cmd.CommandText = "SELECT COLUMN_NAME, COLUMN_KEY, EXTRA, DATA_TYPE FROM information_schema.COLUMNS WHERE TABLE_NAME = 'T_Login' AND TABLE_SCHEMA = '" + conn.Database + "';";
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
                if (pk.IsIdentity) builder.AppendLine("ALTER TABLE `T_Login` CHANGE COLUMN `{0}` `{0}` {1};", pk.COLUMN_NAME, pk.DATA_TYPE);
                builder.AppendLine("ALTER TABLE `T_Login` DROP PRIMARY KEY;");
                _LOG.Debug("Drop primary key.");
            }
            if (__columns.TryGetValue("ID", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Login` CHANGE COLUMN `ID` `ID` INT" + (__value.IsPrimary ? "" : " PRIMARY KEY") + " AUTO_INCREMENT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Login DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Login` ADD COLUMN `ID` INT PRIMARY KEY AUTO_INCREMENT;");
                _LOG.Debug("Add column[`{0}`].", "ID");
            }
            if (__columns.TryGetValue("RegisterID", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Login` CHANGE COLUMN `RegisterID` `RegisterID` INT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_Login` ADD INDEX(`RegisterID`);");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Login` ADD COLUMN `RegisterID` INT;");
                builder.AppendLine("ALTER TABLE `T_Login` ADD INDEX(`RegisterID`);");
                _LOG.Debug("Add index[`{0}`].", "RegisterID");
                _LOG.Debug("Add column[`{0}`].", "RegisterID");
            }
            if (__columns.TryGetValue("LoginTime", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Login` CHANGE COLUMN `LoginTime` `LoginTime` DATETIME;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_Login` ADD INDEX(`LoginTime`);");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Login` ADD COLUMN `LoginTime` DATETIME;");
                builder.AppendLine("ALTER TABLE `T_Login` ADD INDEX(`LoginTime`);");
                _LOG.Debug("Add index[`{0}`].", "LoginTime");
                _LOG.Debug("Add column[`{0}`].", "LoginTime");
            }
            if (__columns.TryGetValue("LastOnlineTime", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Login` CHANGE COLUMN `LastOnlineTime` `LastOnlineTime` DATETIME;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Login DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Login` ADD COLUMN `LastOnlineTime` DATETIME;");
                _LOG.Debug("Add column[`{0}`].", "LastOnlineTime");
            }
            if (IsDropColumn)
            {
                foreach (var __column in __columns.Keys)
                {
                    builder.AppendLine("ALTER TABLE `T_Login` DROP COLUMN `{0}`;", __column);
                    _LOG.Debug("Drop column[`{0}`].", __column);
                }
            }
            
            cmd.CommandText = builder.ToString();
            _LOG.Info("Building table[`T_Login`] structure.");
            cmd.ExecuteNonQuery();
            #endregion
            
            #region Table structure "T_Analysis"
            _LOG.Info("Begin update table[`T_Analysis`] structure.");
            __columns.Clear();
            builder.Remove(0, builder.Length);
            cmd.CommandText = "SELECT COLUMN_NAME, COLUMN_KEY, EXTRA, DATA_TYPE FROM information_schema.COLUMNS WHERE TABLE_NAME = 'T_Analysis' AND TABLE_SCHEMA = '" + conn.Database + "';";
            reader = cmd.ExecuteReader();
            __hasPrimary = false;
            foreach (var __column in _DATABASE.ReadMultiple<MYSQL_TABLE_COLUMN>(reader))
            {
                if (__column.IsPrimary) __hasPrimary = true;
                __columns.Add(__column.COLUMN_NAME, __column);
            }
            reader.Close();
            __noneChangePrimary = true;
            if (!__noneChangePrimary && __hasPrimary)
            {
                var pk = __columns.Values.FirstOrDefault(f => f.IsPrimary);
                if (pk.IsIdentity) builder.AppendLine("ALTER TABLE `T_Analysis` CHANGE COLUMN `{0}` `{0}` {1};", pk.COLUMN_NAME, pk.DATA_TYPE);
                builder.AppendLine("ALTER TABLE `T_Analysis` DROP PRIMARY KEY;");
                _LOG.Debug("Drop primary key.");
            }
            if (__columns.TryGetValue("RegisterID", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Analysis` CHANGE COLUMN `RegisterID` `RegisterID` INT;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_Analysis` ADD INDEX(`RegisterID`);");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Analysis` ADD COLUMN `RegisterID` INT;");
                builder.AppendLine("ALTER TABLE `T_Analysis` ADD INDEX(`RegisterID`);");
                _LOG.Debug("Add index[`{0}`].", "RegisterID");
                _LOG.Debug("Add column[`{0}`].", "RegisterID");
            }
            if (__columns.TryGetValue("Label", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Analysis` CHANGE COLUMN `Label` `Label` TEXT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Analysis DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Analysis` ADD COLUMN `Label` TEXT;");
                _LOG.Debug("Add column[`{0}`].", "Label");
            }
            if (__columns.TryGetValue("Name", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Analysis` CHANGE COLUMN `Name` `Name` TEXT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Analysis DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Analysis` ADD COLUMN `Name` TEXT;");
                _LOG.Debug("Add column[`{0}`].", "Name");
            }
            if (__columns.TryGetValue("OrderID", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Analysis` CHANGE COLUMN `OrderID` `OrderID` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Analysis DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Analysis` ADD COLUMN `OrderID` INT;");
                _LOG.Debug("Add column[`{0}`].", "OrderID");
            }
            if (__columns.TryGetValue("Count", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Analysis` CHANGE COLUMN `Count` `Count` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Analysis DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Analysis` ADD COLUMN `Count` INT;");
                _LOG.Debug("Add column[`{0}`].", "Count");
            }
            if (__columns.TryGetValue("Time", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Analysis` CHANGE COLUMN `Time` `Time` DATETIME;");
                if (!__value.IsIndex && !__value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE `T_Analysis` ADD INDEX(`Time`);");
                    _LOG.Debug("Add index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Analysis` ADD COLUMN `Time` DATETIME;");
                builder.AppendLine("ALTER TABLE `T_Analysis` ADD INDEX(`Time`);");
                _LOG.Debug("Add index[`{0}`].", "Time");
                _LOG.Debug("Add column[`{0}`].", "Time");
            }
            if (IsDropColumn)
            {
                foreach (var __column in __columns.Keys)
                {
                    builder.AppendLine("ALTER TABLE `T_Analysis` DROP COLUMN `{0}`;", __column);
                    _LOG.Debug("Drop column[`{0}`].", __column);
                }
            }
            
            cmd.CommandText = builder.ToString();
            _LOG.Info("Building table[`T_Analysis`] structure.");
            cmd.ExecuteNonQuery();
            #endregion
            
            #region Table structure "T_Online"
            _LOG.Info("Begin update table[`T_Online`] structure.");
            __columns.Clear();
            builder.Remove(0, builder.Length);
            cmd.CommandText = "SELECT COLUMN_NAME, COLUMN_KEY, EXTRA, DATA_TYPE FROM information_schema.COLUMNS WHERE TABLE_NAME = 'T_Online' AND TABLE_SCHEMA = '" + conn.Database + "';";
            reader = cmd.ExecuteReader();
            __hasPrimary = false;
            foreach (var __column in _DATABASE.ReadMultiple<MYSQL_TABLE_COLUMN>(reader))
            {
                if (__column.IsPrimary) __hasPrimary = true;
                __columns.Add(__column.COLUMN_NAME, __column);
            }
            reader.Close();
            __noneChangePrimary = true;
            __noneChangePrimary &= (__columns.TryGetValue("Time", out __value) && __value.IsPrimary);
            __noneChangePrimary &= (__columns.TryGetValue("GameName", out __value) && __value.IsPrimary);
            __noneChangePrimary &= (__columns.TryGetValue("Channel", out __value) && __value.IsPrimary);
            if (!__noneChangePrimary && __hasPrimary)
            {
                var pk = __columns.Values.FirstOrDefault(f => f.IsPrimary);
                if (pk.IsIdentity) builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `{0}` `{0}` {1};", pk.COLUMN_NAME, pk.DATA_TYPE);
                builder.AppendLine("ALTER TABLE `T_Online` DROP PRIMARY KEY;");
                _LOG.Debug("Drop primary key.");
            }
            if (__columns.TryGetValue("Time", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Time` `Time` DATETIME;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Time` DATETIME;");
                _LOG.Debug("Add column[`{0}`].", "Time");
            }
            if (__columns.TryGetValue("GameName", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `GameName` `GameName` TEXT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `GameName` TEXT;");
                _LOG.Debug("Add column[`{0}`].", "GameName");
            }
            if (__columns.TryGetValue("Channel", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Channel` `Channel` TEXT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Channel` TEXT;");
                _LOG.Debug("Add column[`{0}`].", "Channel");
            }
            if (__columns.TryGetValue("Quarter0", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter0` `Quarter0` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter0` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter0");
            }
            if (__columns.TryGetValue("Quarter1", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter1` `Quarter1` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter1` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter1");
            }
            if (__columns.TryGetValue("Quarter2", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter2` `Quarter2` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter2` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter2");
            }
            if (__columns.TryGetValue("Quarter3", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter3` `Quarter3` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter3` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter3");
            }
            if (__columns.TryGetValue("Quarter4", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter4` `Quarter4` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter4` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter4");
            }
            if (__columns.TryGetValue("Quarter5", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter5` `Quarter5` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter5` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter5");
            }
            if (__columns.TryGetValue("Quarter6", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter6` `Quarter6` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter6` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter6");
            }
            if (__columns.TryGetValue("Quarter7", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter7` `Quarter7` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter7` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter7");
            }
            if (__columns.TryGetValue("Quarter8", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter8` `Quarter8` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter8` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter8");
            }
            if (__columns.TryGetValue("Quarter9", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter9` `Quarter9` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter9` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter9");
            }
            if (__columns.TryGetValue("Quarter10", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter10` `Quarter10` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter10` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter10");
            }
            if (__columns.TryGetValue("Quarter11", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter11` `Quarter11` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter11` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter11");
            }
            if (__columns.TryGetValue("Quarter12", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter12` `Quarter12` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter12` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter12");
            }
            if (__columns.TryGetValue("Quarter13", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter13` `Quarter13` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter13` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter13");
            }
            if (__columns.TryGetValue("Quarter14", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter14` `Quarter14` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter14` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter14");
            }
            if (__columns.TryGetValue("Quarter15", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter15` `Quarter15` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter15` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter15");
            }
            if (__columns.TryGetValue("Quarter16", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter16` `Quarter16` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter16` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter16");
            }
            if (__columns.TryGetValue("Quarter17", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter17` `Quarter17` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter17` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter17");
            }
            if (__columns.TryGetValue("Quarter18", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter18` `Quarter18` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter18` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter18");
            }
            if (__columns.TryGetValue("Quarter19", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter19` `Quarter19` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter19` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter19");
            }
            if (__columns.TryGetValue("Quarter20", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter20` `Quarter20` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter20` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter20");
            }
            if (__columns.TryGetValue("Quarter21", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter21` `Quarter21` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter21` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter21");
            }
            if (__columns.TryGetValue("Quarter22", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter22` `Quarter22` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter22` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter22");
            }
            if (__columns.TryGetValue("Quarter23", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter23` `Quarter23` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter23` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter23");
            }
            if (__columns.TryGetValue("Quarter24", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter24` `Quarter24` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter24` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter24");
            }
            if (__columns.TryGetValue("Quarter25", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter25` `Quarter25` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter25` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter25");
            }
            if (__columns.TryGetValue("Quarter26", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter26` `Quarter26` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter26` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter26");
            }
            if (__columns.TryGetValue("Quarter27", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter27` `Quarter27` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter27` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter27");
            }
            if (__columns.TryGetValue("Quarter28", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter28` `Quarter28` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter28` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter28");
            }
            if (__columns.TryGetValue("Quarter29", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter29` `Quarter29` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter29` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter29");
            }
            if (__columns.TryGetValue("Quarter30", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter30` `Quarter30` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter30` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter30");
            }
            if (__columns.TryGetValue("Quarter31", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter31` `Quarter31` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter31` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter31");
            }
            if (__columns.TryGetValue("Quarter32", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter32` `Quarter32` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter32` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter32");
            }
            if (__columns.TryGetValue("Quarter33", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter33` `Quarter33` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter33` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter33");
            }
            if (__columns.TryGetValue("Quarter34", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter34` `Quarter34` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter34` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter34");
            }
            if (__columns.TryGetValue("Quarter35", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter35` `Quarter35` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter35` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter35");
            }
            if (__columns.TryGetValue("Quarter36", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter36` `Quarter36` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter36` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter36");
            }
            if (__columns.TryGetValue("Quarter37", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter37` `Quarter37` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter37` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter37");
            }
            if (__columns.TryGetValue("Quarter38", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter38` `Quarter38` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter38` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter38");
            }
            if (__columns.TryGetValue("Quarter39", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter39` `Quarter39` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter39` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter39");
            }
            if (__columns.TryGetValue("Quarter40", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter40` `Quarter40` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter40` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter40");
            }
            if (__columns.TryGetValue("Quarter41", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter41` `Quarter41` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter41` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter41");
            }
            if (__columns.TryGetValue("Quarter42", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter42` `Quarter42` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter42` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter42");
            }
            if (__columns.TryGetValue("Quarter43", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter43` `Quarter43` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter43` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter43");
            }
            if (__columns.TryGetValue("Quarter44", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter44` `Quarter44` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter44` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter44");
            }
            if (__columns.TryGetValue("Quarter45", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter45` `Quarter45` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter45` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter45");
            }
            if (__columns.TryGetValue("Quarter46", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter46` `Quarter46` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter46` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter46");
            }
            if (__columns.TryGetValue("Quarter47", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter47` `Quarter47` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter47` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter47");
            }
            if (__columns.TryGetValue("Quarter48", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter48` `Quarter48` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter48` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter48");
            }
            if (__columns.TryGetValue("Quarter49", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter49` `Quarter49` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter49` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter49");
            }
            if (__columns.TryGetValue("Quarter50", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter50` `Quarter50` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter50` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter50");
            }
            if (__columns.TryGetValue("Quarter51", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter51` `Quarter51` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter51` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter51");
            }
            if (__columns.TryGetValue("Quarter52", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter52` `Quarter52` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter52` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter52");
            }
            if (__columns.TryGetValue("Quarter53", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter53` `Quarter53` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter53` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter53");
            }
            if (__columns.TryGetValue("Quarter54", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter54` `Quarter54` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter54` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter54");
            }
            if (__columns.TryGetValue("Quarter55", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter55` `Quarter55` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter55` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter55");
            }
            if (__columns.TryGetValue("Quarter56", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter56` `Quarter56` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter56` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter56");
            }
            if (__columns.TryGetValue("Quarter57", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter57` `Quarter57` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter57` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter57");
            }
            if (__columns.TryGetValue("Quarter58", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter58` `Quarter58` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter58` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter58");
            }
            if (__columns.TryGetValue("Quarter59", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter59` `Quarter59` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter59` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter59");
            }
            if (__columns.TryGetValue("Quarter60", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter60` `Quarter60` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter60` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter60");
            }
            if (__columns.TryGetValue("Quarter61", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter61` `Quarter61` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter61` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter61");
            }
            if (__columns.TryGetValue("Quarter62", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter62` `Quarter62` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter62` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter62");
            }
            if (__columns.TryGetValue("Quarter63", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter63` `Quarter63` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter63` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter63");
            }
            if (__columns.TryGetValue("Quarter64", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter64` `Quarter64` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter64` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter64");
            }
            if (__columns.TryGetValue("Quarter65", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter65` `Quarter65` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter65` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter65");
            }
            if (__columns.TryGetValue("Quarter66", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter66` `Quarter66` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter66` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter66");
            }
            if (__columns.TryGetValue("Quarter67", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter67` `Quarter67` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter67` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter67");
            }
            if (__columns.TryGetValue("Quarter68", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter68` `Quarter68` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter68` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter68");
            }
            if (__columns.TryGetValue("Quarter69", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter69` `Quarter69` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter69` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter69");
            }
            if (__columns.TryGetValue("Quarter70", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter70` `Quarter70` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter70` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter70");
            }
            if (__columns.TryGetValue("Quarter71", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter71` `Quarter71` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter71` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter71");
            }
            if (__columns.TryGetValue("Quarter72", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter72` `Quarter72` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter72` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter72");
            }
            if (__columns.TryGetValue("Quarter73", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter73` `Quarter73` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter73` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter73");
            }
            if (__columns.TryGetValue("Quarter74", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter74` `Quarter74` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter74` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter74");
            }
            if (__columns.TryGetValue("Quarter75", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter75` `Quarter75` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter75` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter75");
            }
            if (__columns.TryGetValue("Quarter76", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter76` `Quarter76` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter76` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter76");
            }
            if (__columns.TryGetValue("Quarter77", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter77` `Quarter77` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter77` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter77");
            }
            if (__columns.TryGetValue("Quarter78", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter78` `Quarter78` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter78` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter78");
            }
            if (__columns.TryGetValue("Quarter79", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter79` `Quarter79` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter79` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter79");
            }
            if (__columns.TryGetValue("Quarter80", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter80` `Quarter80` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter80` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter80");
            }
            if (__columns.TryGetValue("Quarter81", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter81` `Quarter81` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter81` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter81");
            }
            if (__columns.TryGetValue("Quarter82", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter82` `Quarter82` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter82` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter82");
            }
            if (__columns.TryGetValue("Quarter83", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter83` `Quarter83` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter83` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter83");
            }
            if (__columns.TryGetValue("Quarter84", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter84` `Quarter84` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter84` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter84");
            }
            if (__columns.TryGetValue("Quarter85", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter85` `Quarter85` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter85` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter85");
            }
            if (__columns.TryGetValue("Quarter86", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter86` `Quarter86` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter86` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter86");
            }
            if (__columns.TryGetValue("Quarter87", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter87` `Quarter87` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter87` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter87");
            }
            if (__columns.TryGetValue("Quarter88", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter88` `Quarter88` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter88` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter88");
            }
            if (__columns.TryGetValue("Quarter89", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter89` `Quarter89` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter89` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter89");
            }
            if (__columns.TryGetValue("Quarter90", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter90` `Quarter90` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter90` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter90");
            }
            if (__columns.TryGetValue("Quarter91", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter91` `Quarter91` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter91` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter91");
            }
            if (__columns.TryGetValue("Quarter92", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter92` `Quarter92` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter92` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter92");
            }
            if (__columns.TryGetValue("Quarter93", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter93` `Quarter93` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter93` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter93");
            }
            if (__columns.TryGetValue("Quarter94", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter94` `Quarter94` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter94` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter94");
            }
            if (__columns.TryGetValue("Quarter95", out __value))
            {
                builder.AppendLine("ALTER TABLE `T_Online` CHANGE COLUMN `Quarter95` `Quarter95` INT;");
                if (__value.IsIndex || __value.IsUnique)
                {
                    builder.AppendLine("ALTER TABLE T_Online DROP INDEX `{0}`;", __value.COLUMN_NAME);
                    _LOG.Debug("Drop index[`{0}`].", __value.COLUMN_NAME);
                }
                __columns.Remove(__value.COLUMN_NAME);
            }
            else
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD COLUMN `Quarter95` INT;");
                _LOG.Debug("Add column[`{0}`].", "Quarter95");
            }
            if (IsDropColumn)
            {
                foreach (var __column in __columns.Keys)
                {
                    builder.AppendLine("ALTER TABLE `T_Online` DROP COLUMN `{0}`;", __column);
                    _LOG.Debug("Drop column[`{0}`].", __column);
                }
            }
            if (!__noneChangePrimary)
            {
                builder.AppendLine("ALTER TABLE `T_Online` ADD PRIMARY KEY (`Time`,`GameName`(10),`Channel`(10));");
                _LOG.Debug("Add primary key[{0}].", "Time,GameName,Channel");
            }
            
            cmd.CommandText = builder.ToString();
            _LOG.Info("Building table[`T_Online`] structure.");
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
            int __T_CENTER_USER_ID = 0;
            int __T_OPLog_ID = 0;
            int __T_Register_ID = 0;
            int __T_Login_ID = 0;
            
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
                table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_Register");
                if (table != null)
                {
                    builder.Append("INSERT INTO {0}.{1} SELECT {1}.`ID`,{1}.`GameName`,{1}.`DeviceID`,{1}.`Channel`,{1}.`RegisterTime` FROM {2}.{1}", tempName, table.TableName, dbName);
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
                table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_Login");
                if (table != null)
                {
                    bool __flag = false;
                    builder.Append("INSERT INTO {0}.{1} SELECT {1}.`ID`,{1}.`RegisterID`,{1}.`LoginTime`,{1}.`LastOnlineTime` FROM {2}.{1}", tempName, table.TableName, dbName);
                    if (!string.IsNullOrEmpty(table.Where)) builder.Append(" " + table.Where);
                    if (dbs[i].Tables.Any(t => t.TableName == "T_Register"))
                    {
                        if (!__flag)
                        {
                            __flag = true;
                            builder.Append(" WHERE EXISTS ");
                            builder.Append("(SELECT {0}.T_Register.ID FROM {0}.T_Register WHERE T_Login.RegisterID = {0}.T_Register.ID)", tempName);
                        }
                    }
                    builder.AppendLine(";");
                    result = builder.ToString();
                    builder.Remove(0, builder.Length);
                    _LOG.Info("Merge table[`{0}`] data.", table.TableName);
                    _LOG.Debug("SQL: {0}", result);
                    db.ExecuteNonQuery(result);
                }
                table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_Analysis");
                if (table != null)
                {
                    builder.Append("INSERT INTO {0}.{1} SELECT {1}.`RegisterID`,{1}.`Label`,{1}.`Name`,{1}.`OrderID`,{1}.`Count`,{1}.`Time` FROM {2}.{1}", tempName, table.TableName, dbName);
                    if (!string.IsNullOrEmpty(table.Where)) builder.Append(" " + table.Where);
                    builder.AppendLine(";");
                    result = builder.ToString();
                    builder.Remove(0, builder.Length);
                    _LOG.Info("Merge table[`{0}`] data.", table.TableName);
                    _LOG.Debug("SQL: {0}", result);
                    db.ExecuteNonQuery(result);
                }
                table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_Online");
                if (table != null)
                {
                    builder.Append("INSERT INTO {0}.{1} SELECT {1}.`Time`,{1}.`GameName`,{1}.`Channel`,{1}.`Quarter0`,{1}.`Quarter1`,{1}.`Quarter2`,{1}.`Quarter3`,{1}.`Quarter4`,{1}.`Quarter5`,{1}.`Quarter6`,{1}.`Quarter7`,{1}.`Quarter8`,{1}.`Quarter9`,{1}.`Quarter10`,{1}.`Quarter11`,{1}.`Quarter12`,{1}.`Quarter13`,{1}.`Quarter14`,{1}.`Quarter15`,{1}.`Quarter16`,{1}.`Quarter17`,{1}.`Quarter18`,{1}.`Quarter19`,{1}.`Quarter20`,{1}.`Quarter21`,{1}.`Quarter22`,{1}.`Quarter23`,{1}.`Quarter24`,{1}.`Quarter25`,{1}.`Quarter26`,{1}.`Quarter27`,{1}.`Quarter28`,{1}.`Quarter29`,{1}.`Quarter30`,{1}.`Quarter31`,{1}.`Quarter32`,{1}.`Quarter33`,{1}.`Quarter34`,{1}.`Quarter35`,{1}.`Quarter36`,{1}.`Quarter37`,{1}.`Quarter38`,{1}.`Quarter39`,{1}.`Quarter40`,{1}.`Quarter41`,{1}.`Quarter42`,{1}.`Quarter43`,{1}.`Quarter44`,{1}.`Quarter45`,{1}.`Quarter46`,{1}.`Quarter47`,{1}.`Quarter48`,{1}.`Quarter49`,{1}.`Quarter50`,{1}.`Quarter51`,{1}.`Quarter52`,{1}.`Quarter53`,{1}.`Quarter54`,{1}.`Quarter55`,{1}.`Quarter56`,{1}.`Quarter57`,{1}.`Quarter58`,{1}.`Quarter59`,{1}.`Quarter60`,{1}.`Quarter61`,{1}.`Quarter62`,{1}.`Quarter63`,{1}.`Quarter64`,{1}.`Quarter65`,{1}.`Quarter66`,{1}.`Quarter67`,{1}.`Quarter68`,{1}.`Quarter69`,{1}.`Quarter70`,{1}.`Quarter71`,{1}.`Quarter72`,{1}.`Quarter73`,{1}.`Quarter74`,{1}.`Quarter75`,{1}.`Quarter76`,{1}.`Quarter77`,{1}.`Quarter78`,{1}.`Quarter79`,{1}.`Quarter80`,{1}.`Quarter81`,{1}.`Quarter82`,{1}.`Quarter83`,{1}.`Quarter84`,{1}.`Quarter85`,{1}.`Quarter86`,{1}.`Quarter87`,{1}.`Quarter88`,{1}.`Quarter89`,{1}.`Quarter90`,{1}.`Quarter91`,{1}.`Quarter92`,{1}.`Quarter93`,{1}.`Quarter94`,{1}.`Quarter95` FROM {2}.{1}", tempName, table.TableName, dbName);
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
                table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_Register");
                if (table.AutoMergeIdentity)
                {
                    UpdateIdentityKey_T_Register_ID(ref __T_Register_ID);
                    _LOG.Info("自动修改自增列`T_Register`.ID");
                }
                table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_Login");
                if (table.AutoMergeIdentity)
                {
                    UpdateIdentityKey_T_Login_ID(ref __T_Login_ID);
                    _LOG.Info("自动修改自增列`T_Login`.ID");
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
                    table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_Register");
                    if (table != null)
                    {
                        _LOG.Debug("Merge table[`{0}`].", table.TableName);
                        var list = db.SelectObjects<T_Register>("SELECT * FROM T_Register;");
                        if (list.Count > 0)
                        {
                            foreach (var item in list)
                            {
                                _T_Register.Insert(item);
                            }
                        }
                    }
                    table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_Login");
                    if (table != null)
                    {
                        _LOG.Debug("Merge table[`{0}`].", table.TableName);
                        var list = db.SelectObjects<T_Login>("SELECT * FROM T_Login;");
                        if (list.Count > 0)
                        {
                            foreach (var item in list)
                            {
                                _T_Login.Insert(item);
                            }
                        }
                    }
                    table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_Analysis");
                    if (table != null)
                    {
                        _LOG.Debug("Merge table[`{0}`].", table.TableName);
                        var list = db.SelectObjects<T_Analysis>("SELECT * FROM T_Analysis;");
                        if (list.Count > 0)
                        {
                            foreach (var item in list)
                            {
                                _T_Analysis.Insert(item);
                            }
                        }
                    }
                    table = dbs[i].Tables.FirstOrDefault(t => t.TableName == "T_Online");
                    if (table != null)
                    {
                        _LOG.Debug("Merge table[`{0}`].", table.TableName);
                        var list = db.SelectObjects<T_Online>("SELECT * FROM T_Online;");
                        if (list.Count > 0)
                        {
                            foreach (var item in list)
                            {
                                _T_Online.Insert(item);
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
        public static int DeleteForeignKey_T_Register_ID(int target)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("DELETE FROM `T_Register` WHERE `ID` = @p0;");
            builder.AppendLine("DELETE FROM `T_Login` WHERE `RegisterID` = @p0;");
            return _DAO.ExecuteNonQuery(builder.ToString(), target);
        }
        public static int UpdateForeignKey_T_Register_ID(int origin, int target)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("UPDATE `T_Register` SET `ID` = @p0 WHERE `ID` = @p1;");
            builder.AppendLine("UPDATE `T_Login` SET `RegisterID` = @p0 WHERE `RegisterID` = @p1;");
            return _DAO.ExecuteNonQuery(builder.ToString(), target, origin);
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
        public static void UpdateIdentityKey_T_Register_ID(ref int start)
        {
            int min = _DAO.ExecuteScalar<int>("SELECT MIN(`ID`) FROM `T_Register`;");
            int max = _DAO.ExecuteScalar<int>("SELECT MAX(`ID`) FROM `T_Register`;");
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
            builder.AppendLine("UPDATE `T_Register` SET `ID` = `ID` + @p0;");
            builder.AppendLine("UPDATE `T_Login` SET `RegisterID` = `RegisterID` + @p0;");
            _DAO.ExecuteNonQuery(builder.ToString(), min);
        }
        public static void UpdateIdentityKey_T_Login_ID(ref int start)
        {
            int min = _DAO.ExecuteScalar<int>("SELECT MIN(`ID`) FROM `T_Login`;");
            int max = _DAO.ExecuteScalar<int>("SELECT MAX(`ID`) FROM `T_Login`;");
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
            builder.AppendLine("UPDATE `T_Login` SET `ID` = `ID` + @p0;");
            _DAO.ExecuteNonQuery(builder.ToString(), min);
        }
        public class JoinT_Login
        {
            public T_Login T_Login;
            public T_Register RegisterID;
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
                return _DATABASE.MultiRead<T_CENTER_USER>(reader, offset, fieldCount, properties, fields, indices)
                ;
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
                return _DATABASE.MultiRead<T_OPLog>(reader, offset, fieldCount, properties, fields, indices)
                ;
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
        public partial class _T_Register : T_Register
        {
            public static ET_Register[] FIELD_ALL = { ET_Register.ID, ET_Register.GameName, ET_Register.DeviceID, ET_Register.Channel, ET_Register.RegisterTime };
            public static ET_Register[] FIELD_UPDATE = { ET_Register.GameName, ET_Register.DeviceID, ET_Register.Channel, ET_Register.RegisterTime };
            public static ET_Register[] NoNeedField(params ET_Register[] noNeed)
            {
                if (noNeed.Length == 0) return FIELD_ALL;
                List<ET_Register> list = new List<ET_Register>(FIELD_ALL.Length);
                for (int i = 0; i < FIELD_ALL.Length; i++)
                {
                    if (!noNeed.Contains(FIELD_ALL[i])) list.Add(FIELD_ALL[i]);
                }
                return list.ToArray();
            }
            public static int FieldCount { get { return FIELD_ALL.Length; } }
            
            public static T_Register Read(IDataReader reader)
            {
                return Read(reader, 0, FieldCount);
            }
            public static T_Register Read(IDataReader reader, int offset)
            {
                return Read(reader, offset, FieldCount);
            }
            public static T_Register Read(IDataReader reader, int offset, int fieldCount)
            {
                return _DATABASE.ReadObject<T_Register>(reader, offset, fieldCount);
            }
            public static void MultiReadPrepare(IDataReader reader, int offset, int fieldCount, out List<PropertyInfo> properties, out List<FieldInfo> fields, ref int[] indices)
            {
                _DATABASE.MultiReadPrepare(reader, typeof(T_Register), offset, fieldCount, out properties, out fields, ref indices);
            }
            public static T_Register MultiRead(IDataReader reader, int offset, int fieldCount, List<PropertyInfo> properties, List<FieldInfo> fields, int[] indices)
            {
                return _DATABASE.MultiRead<T_Register>(reader, offset, fieldCount, properties, fields, indices)
                ;
            }
            public static void GetInsertSQL(T_Register target, StringBuilder builder, List<object> values)
            {
                int index = values.Count;
                builder.AppendFormat("INSERT `T_Register`(`GameName`, `DeviceID`, `Channel`, `RegisterTime`) VALUES(");
                for (int i = 0, n = 3; i <= n; i++)
                {
                    builder.AppendFormat("@p{0}", index++);
                    if (i != n) builder.Append(", ");
                }
                builder.AppendLine(");");
                values.Add(target.GameName);
                values.Add(target.DeviceID);
                values.Add(target.Channel);
                values.Add(target.RegisterTime);
            }
            public static int Insert(T_Register target)
            {
                StringBuilder builder = new StringBuilder();
                List<object> values = new List<object>(5);
                GetInsertSQL(target, builder, values);
                builder.Append("SELECT LAST_INSERT_ID();");
                target.ID = _DAO.SelectValue<int>(builder.ToString(), values.ToArray());
                return target.ID;
            }
            public static void GetDeleteSQL(int ID, StringBuilder builder, List<object> values)
            {
                int index = values.Count;
                builder.AppendFormat("DELETE FROM `T_Register` WHERE `ID` = @p{0};", index++);
                values.Add(ID);
            }
            public static int Delete(int ID)
            {
                return _DAO.ExecuteNonQuery("DELETE FROM `T_Register` WHERE `ID` = @p0", ID);
            }
            public static void GetUpdateSQL(T_Register target, string condition, StringBuilder builder, List<object> values, params ET_Register[] fields)
            {
                int index = values.Count;
                bool all = fields.Length == 0 || fields == FIELD_UPDATE;
                builder.Append("UPDATE `T_Register` SET");
                if (all || fields.Contains(ET_Register.GameName))
                {
                    builder.AppendFormat(" `GameName` = @p{0},", index++);
                    values.Add(target.GameName);
                }
                if (all || fields.Contains(ET_Register.DeviceID))
                {
                    builder.AppendFormat(" `DeviceID` = @p{0},", index++);
                    values.Add(target.DeviceID);
                }
                if (all || fields.Contains(ET_Register.Channel))
                {
                    builder.AppendFormat(" `Channel` = @p{0},", index++);
                    values.Add(target.Channel);
                }
                if (all || fields.Contains(ET_Register.RegisterTime))
                {
                    builder.AppendFormat(" `RegisterTime` = @p{0},", index++);
                    values.Add(target.RegisterTime);
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
            public static int Update(T_Register target, string condition, params ET_Register[] fields)
            {
                StringBuilder builder = new StringBuilder();
                List<object> values = new List<object>(fields.Length + 1);
                GetUpdateSQL(target, condition, builder, values, fields);
                return _DAO.ExecuteNonQuery(builder.ToString(), values.ToArray());
            }
            public static void GetSelectField(string tableName, StringBuilder builder, params ET_Register[] fields)
            {
                if (string.IsNullOrEmpty(tableName)) tableName = "`T_Register`";
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
            public static StringBuilder GetSelectSQL(params ET_Register[] fields)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("SELECT ");
                GetSelectField(null, builder, fields);
                builder.AppendLine(" FROM `T_Register`");
                return builder;
            }
            public static T_Register Select(int __ID, params ET_Register[] fields)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.Append(" WHERE `ID` = @p0;");
                var ret = _DAO.SelectObject<T_Register>(builder.ToString(), __ID);
                if (ret != default(T_Register))
                {
                    ret.ID = __ID;
                }
                return ret;
            }
            public static T_Register Select(ET_Register[] fields, string condition, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                return _DAO.SelectObject<T_Register>(builder.ToString(), param);
            }
            public static bool Exists(int __ID)
            {
                return _DAO.ExecuteScalar<bool>("SELECT EXISTS(SELECT 1 FROM `T_Register` WHERE `ID` = @p0)", __ID);
            }
            public static bool Exists2(string condition, params object[] param)
            {
                return _DAO.ExecuteScalar<bool>(string.Format("SELECT EXISTS(SELECT 1 FROM `T_Register` {0})", condition), param);
            }
            public static List<T_Register> SelectMultiple(ET_Register[] fields, string condition, params object[] param)
            {
                StringBuilder builder;
                if (fields == null || fields.Length == 0) builder = new StringBuilder("SELECT * FROM T_Register");
                else builder = GetSelectSQL(fields);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                return _DAO.SelectObjects<T_Register>(builder.ToString(), param);
            }
            public static PagedModel<T_Register> SelectPages(string __where, ET_Register[] fields, string conditionAfterWhere, int page, int pageSize, params object[] param)
            {
                var ret = SelectPages<T_Register>(__where, GetSelectSQL(fields).ToString(), conditionAfterWhere, page, pageSize, param);
                return ret;
            }
            public static PagedModel<T> SelectPages<T>(string __where, string selectSQL, string conditionAfterWhere, int page, int pageSize, params object[] param) where T : new()
            {
                return _DB.SelectPages<T>(_DAO, "SELECT count(`T_Register`.`ID`) FROM `T_Register`", __where, selectSQL, conditionAfterWhere, page, pageSize, param);
            }
        }
        public partial class _T_Login : T_Login
        {
            public static ET_Login[] FIELD_ALL = { ET_Login.ID, ET_Login.RegisterID, ET_Login.LoginTime, ET_Login.LastOnlineTime };
            public static ET_Login[] FIELD_UPDATE = { ET_Login.RegisterID, ET_Login.LoginTime, ET_Login.LastOnlineTime };
            public static ET_Login[] NoNeedField(params ET_Login[] noNeed)
            {
                if (noNeed.Length == 0) return FIELD_ALL;
                List<ET_Login> list = new List<ET_Login>(FIELD_ALL.Length);
                for (int i = 0; i < FIELD_ALL.Length; i++)
                {
                    if (!noNeed.Contains(FIELD_ALL[i])) list.Add(FIELD_ALL[i]);
                }
                return list.ToArray();
            }
            public static int FieldCount { get { return FIELD_ALL.Length; } }
            
            public static T_Login Read(IDataReader reader)
            {
                return Read(reader, 0, FieldCount);
            }
            public static T_Login Read(IDataReader reader, int offset)
            {
                return Read(reader, offset, FieldCount);
            }
            public static T_Login Read(IDataReader reader, int offset, int fieldCount)
            {
                return _DATABASE.ReadObject<T_Login>(reader, offset, fieldCount);
            }
            public static void MultiReadPrepare(IDataReader reader, int offset, int fieldCount, out List<PropertyInfo> properties, out List<FieldInfo> fields, ref int[] indices)
            {
                _DATABASE.MultiReadPrepare(reader, typeof(T_Login), offset, fieldCount, out properties, out fields, ref indices);
            }
            public static T_Login MultiRead(IDataReader reader, int offset, int fieldCount, List<PropertyInfo> properties, List<FieldInfo> fields, int[] indices)
            {
                return _DATABASE.MultiRead<T_Login>(reader, offset, fieldCount, properties, fields, indices)
                ;
            }
            public static void GetInsertSQL(T_Login target, StringBuilder builder, List<object> values)
            {
                int index = values.Count;
                builder.AppendFormat("INSERT `T_Login`(`RegisterID`, `LoginTime`, `LastOnlineTime`) VALUES(");
                for (int i = 0, n = 2; i <= n; i++)
                {
                    builder.AppendFormat("@p{0}", index++);
                    if (i != n) builder.Append(", ");
                }
                builder.AppendLine(");");
                values.Add(target.RegisterID);
                values.Add(target.LoginTime);
                values.Add(target.LastOnlineTime);
            }
            public static int Insert(T_Login target)
            {
                StringBuilder builder = new StringBuilder();
                List<object> values = new List<object>(4);
                GetInsertSQL(target, builder, values);
                builder.Append("SELECT LAST_INSERT_ID();");
                target.ID = _DAO.SelectValue<int>(builder.ToString(), values.ToArray());
                return target.ID;
            }
            public static void GetDeleteSQL(int ID, StringBuilder builder, List<object> values)
            {
                int index = values.Count;
                builder.AppendFormat("DELETE FROM `T_Login` WHERE `ID` = @p{0};", index++);
                values.Add(ID);
            }
            public static int Delete(int ID)
            {
                return _DAO.ExecuteNonQuery("DELETE FROM `T_Login` WHERE `ID` = @p0", ID);
            }
            public static void GetUpdateSQL(T_Login target, string condition, StringBuilder builder, List<object> values, params ET_Login[] fields)
            {
                int index = values.Count;
                bool all = fields.Length == 0 || fields == FIELD_UPDATE;
                builder.Append("UPDATE `T_Login` SET");
                if (all || fields.Contains(ET_Login.RegisterID))
                {
                    builder.AppendFormat(" `RegisterID` = @p{0},", index++);
                    values.Add(target.RegisterID);
                }
                if (all || fields.Contains(ET_Login.LoginTime))
                {
                    builder.AppendFormat(" `LoginTime` = @p{0},", index++);
                    values.Add(target.LoginTime);
                }
                if (all || fields.Contains(ET_Login.LastOnlineTime))
                {
                    builder.AppendFormat(" `LastOnlineTime` = @p{0},", index++);
                    values.Add(target.LastOnlineTime);
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
            public static int Update(T_Login target, string condition, params ET_Login[] fields)
            {
                StringBuilder builder = new StringBuilder();
                List<object> values = new List<object>(fields.Length + 1);
                GetUpdateSQL(target, condition, builder, values, fields);
                return _DAO.ExecuteNonQuery(builder.ToString(), values.ToArray());
            }
            public static void GetSelectField(string tableName, StringBuilder builder, params ET_Login[] fields)
            {
                if (string.IsNullOrEmpty(tableName)) tableName = "`T_Login`";
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
            public static StringBuilder GetSelectSQL(params ET_Login[] fields)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("SELECT ");
                GetSelectField(null, builder, fields);
                builder.AppendLine(" FROM `T_Login`");
                return builder;
            }
            public static T_Login Select(int __ID, params ET_Login[] fields)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.Append(" WHERE `ID` = @p0;");
                var ret = _DAO.SelectObject<T_Login>(builder.ToString(), __ID);
                if (ret != default(T_Login))
                {
                    ret.ID = __ID;
                }
                return ret;
            }
            public static T_Login Select(ET_Login[] fields, string condition, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                return _DAO.SelectObject<T_Login>(builder.ToString(), param);
            }
            public static bool Exists(int __ID)
            {
                return _DAO.ExecuteScalar<bool>("SELECT EXISTS(SELECT 1 FROM `T_Login` WHERE `ID` = @p0)", __ID);
            }
            public static bool Exists2(string condition, params object[] param)
            {
                return _DAO.ExecuteScalar<bool>(string.Format("SELECT EXISTS(SELECT 1 FROM `T_Login` {0})", condition), param);
            }
            public static List<T_Login> SelectMultiple(ET_Login[] fields, string condition, params object[] param)
            {
                StringBuilder builder;
                if (fields == null || fields.Length == 0) builder = new StringBuilder("SELECT * FROM T_Login");
                else builder = GetSelectSQL(fields);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                return _DAO.SelectObjects<T_Login>(builder.ToString(), param);
            }
            public static StringBuilder GetSelectJoinSQL(ref ET_Login[] fT_Login, ref ET_Register[] fRegisterID)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("SELECT ");
                _T_Login.GetSelectField("t0", builder, fT_Login);
                if (fT_Login == null || fT_Login.Length == 0) fT_Login = FIELD_ALL;
                if (fRegisterID != null)
                {
                    builder.Append(", ");
                    _T_Register.GetSelectField("t1", builder, fRegisterID);
                    if (fRegisterID.Length == 0) fRegisterID = _T_Register.FIELD_ALL;
                }
                builder.Append(" FROM `T_Login` as t0");
                if (fRegisterID != null) builder.Append(" LEFT JOIN `T_Register` as t1 ON (t0.RegisterID = t1.ID)");
                return builder;
            }
            public static void SelectJoinRead(IDataReader reader, List<JoinT_Login> list, ET_Login[] fT_Login, ET_Register[] fRegisterID)
            {
                int offset = 0;
                int[] indices = new int[reader.FieldCount];
                List<PropertyInfo> _pT_Login;
                List<FieldInfo> _fT_Login;
                _T_Login.MultiReadPrepare(reader, 0, fT_Login.Length, out _pT_Login, out _fT_Login, ref indices);
                offset = fT_Login.Length;
                List<PropertyInfo> _pRegisterID = null;
                List<FieldInfo> _fRegisterID = null;
                if (fRegisterID != null)
                {
                    _T_Register.MultiReadPrepare(reader, offset, fRegisterID.Length, out _pRegisterID, out _fRegisterID, ref indices);
                    offset += fRegisterID.Length;
                }
                while (reader.Read())
                {
                    JoinT_Login join = new JoinT_Login();
                    list.Add(join);
                    join.T_Login = _T_Login.MultiRead(reader, 0, fT_Login.Length, _pT_Login, _fT_Login, indices);
                    offset = fT_Login.Length;
                    if (fRegisterID != null)
                    {
                        join.RegisterID = _T_Register.MultiRead(reader, offset, fRegisterID.Length, _pRegisterID, _fRegisterID, indices);
                        offset += fRegisterID.Length;
                    }
                }
            }
            public static List<JoinT_Login> SelectJoin(ET_Login[] fT_Login, ET_Register[] fRegisterID, string condition, params object[] param)
            {
                StringBuilder builder = GetSelectJoinSQL(ref fT_Login, ref fRegisterID);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                List<JoinT_Login> results = new List<JoinT_Login>();
                _DAO.ExecuteReader((reader) => SelectJoinRead(reader, results, fT_Login, fRegisterID), builder.ToString(), param);
                return results;
            }
            public static PagedModel<JoinT_Login> SelectJoinPages(ET_Login[] fT_Login, ET_Register[] fRegisterID, string __where, string conditionAfterWhere, int page, int pageSize, params object[] param)
            {
                StringBuilder builder = GetSelectJoinSQL(ref fT_Login, ref fRegisterID);
                StringBuilder builder2 = new StringBuilder();
                builder2.Append("SELECT count(t0.`ID`) FROM `T_Login` as t0");
                if (fRegisterID != null) builder2.Append(" LEFT JOIN `T_Register` as t1 ON (t0.RegisterID = t1.ID)");
                return _DB.SelectPages<JoinT_Login>(_DAO, builder2.ToString(), __where, builder.ToString(), conditionAfterWhere, page, pageSize, (reader, list) => SelectJoinRead(reader, list, fT_Login, fRegisterID), param);
            }
            public static PagedModel<T_Login> SelectPages(string __where, ET_Login[] fields, string conditionAfterWhere, int page, int pageSize, params object[] param)
            {
                var ret = SelectPages<T_Login>(__where, GetSelectSQL(fields).ToString(), conditionAfterWhere, page, pageSize, param);
                return ret;
            }
            public static PagedModel<T> SelectPages<T>(string __where, string selectSQL, string conditionAfterWhere, int page, int pageSize, params object[] param) where T : new()
            {
                return _DB.SelectPages<T>(_DAO, "SELECT count(`T_Login`.`ID`) FROM `T_Login`", __where, selectSQL, conditionAfterWhere, page, pageSize, param);
            }
        }
        public partial class _T_Analysis : T_Analysis
        {
            public static ET_Analysis[] FIELD_ALL = { ET_Analysis.RegisterID, ET_Analysis.Label, ET_Analysis.Name, ET_Analysis.OrderID, ET_Analysis.Count, ET_Analysis.Time };
            public static ET_Analysis[] FIELD_UPDATE = { ET_Analysis.RegisterID, ET_Analysis.Label, ET_Analysis.Name, ET_Analysis.OrderID, ET_Analysis.Count, ET_Analysis.Time };
            public static ET_Analysis[] NoNeedField(params ET_Analysis[] noNeed)
            {
                if (noNeed.Length == 0) return FIELD_ALL;
                List<ET_Analysis> list = new List<ET_Analysis>(FIELD_ALL.Length);
                for (int i = 0; i < FIELD_ALL.Length; i++)
                {
                    if (!noNeed.Contains(FIELD_ALL[i])) list.Add(FIELD_ALL[i]);
                }
                return list.ToArray();
            }
            public static int FieldCount { get { return FIELD_ALL.Length; } }
            
            public static T_Analysis Read(IDataReader reader)
            {
                return Read(reader, 0, FieldCount);
            }
            public static T_Analysis Read(IDataReader reader, int offset)
            {
                return Read(reader, offset, FieldCount);
            }
            public static T_Analysis Read(IDataReader reader, int offset, int fieldCount)
            {
                return _DATABASE.ReadObject<T_Analysis>(reader, offset, fieldCount);
            }
            public static void MultiReadPrepare(IDataReader reader, int offset, int fieldCount, out List<PropertyInfo> properties, out List<FieldInfo> fields, ref int[] indices)
            {
                _DATABASE.MultiReadPrepare(reader, typeof(T_Analysis), offset, fieldCount, out properties, out fields, ref indices);
            }
            public static T_Analysis MultiRead(IDataReader reader, int offset, int fieldCount, List<PropertyInfo> properties, List<FieldInfo> fields, int[] indices)
            {
                return _DATABASE.MultiRead<T_Analysis>(reader, offset, fieldCount, properties, fields, indices)
                ;
            }
            public static void GetInsertSQL(T_Analysis target, StringBuilder builder, List<object> values)
            {
                int index = values.Count;
                builder.AppendFormat("INSERT `T_Analysis`(`RegisterID`, `Label`, `Name`, `OrderID`, `Count`, `Time`) VALUES(");
                for (int i = 0, n = 5; i <= n; i++)
                {
                    builder.AppendFormat("@p{0}", index++);
                    if (i != n) builder.Append(", ");
                }
                builder.AppendLine(");");
                values.Add(target.RegisterID);
                values.Add(target.Label);
                values.Add(target.Name);
                values.Add(target.OrderID);
                values.Add(target.Count);
                values.Add(target.Time);
            }
            public static void Insert(T_Analysis target)
            {
                StringBuilder builder = new StringBuilder();
                List<object> values = new List<object>(6);
                GetInsertSQL(target, builder, values);
                _DAO.ExecuteNonQuery(builder.ToString(), values.ToArray());
            }
            public static void GetUpdateSQL(T_Analysis target, string condition, StringBuilder builder, List<object> values, params ET_Analysis[] fields)
            {
                int index = values.Count;
                bool all = fields.Length == 0 || fields == FIELD_UPDATE;
                builder.Append("UPDATE `T_Analysis` SET");
                if (all || fields.Contains(ET_Analysis.RegisterID))
                {
                    builder.AppendFormat(" `RegisterID` = @p{0},", index++);
                    values.Add(target.RegisterID);
                }
                if (all || fields.Contains(ET_Analysis.Label))
                {
                    builder.AppendFormat(" `Label` = @p{0},", index++);
                    values.Add(target.Label);
                }
                if (all || fields.Contains(ET_Analysis.Name))
                {
                    builder.AppendFormat(" `Name` = @p{0},", index++);
                    values.Add(target.Name);
                }
                if (all || fields.Contains(ET_Analysis.OrderID))
                {
                    builder.AppendFormat(" `OrderID` = @p{0},", index++);
                    values.Add(target.OrderID);
                }
                if (all || fields.Contains(ET_Analysis.Count))
                {
                    builder.AppendFormat(" `Count` = @p{0},", index++);
                    values.Add(target.Count);
                }
                if (all || fields.Contains(ET_Analysis.Time))
                {
                    builder.AppendFormat(" `Time` = @p{0},", index++);
                    values.Add(target.Time);
                }
                if (index == 0) return;
                builder[builder.Length - 1] = ' ';
                if (!string.IsNullOrEmpty(condition)) builder.Append(condition);
                builder.AppendLine(";");
            }
            /// <summary>condition that 'where' or 'join' without ';'</summary>
            public static int Update(T_Analysis target, string condition, params ET_Analysis[] fields)
            {
                StringBuilder builder = new StringBuilder();
                List<object> values = new List<object>(fields.Length + 0);
                GetUpdateSQL(target, condition, builder, values, fields);
                return _DAO.ExecuteNonQuery(builder.ToString(), values.ToArray());
            }
            public static void GetSelectField(string tableName, StringBuilder builder, params ET_Analysis[] fields)
            {
                if (string.IsNullOrEmpty(tableName)) tableName = "`T_Analysis`";
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
            public static StringBuilder GetSelectSQL(params ET_Analysis[] fields)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("SELECT ");
                GetSelectField(null, builder, fields);
                builder.AppendLine(" FROM `T_Analysis`");
                return builder;
            }
            public static T_Analysis Select(ET_Analysis[] fields, string condition, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                return _DAO.SelectObject<T_Analysis>(builder.ToString(), param);
            }
            public static bool Exists()
            {
                return _DAO.ExecuteScalar<bool>("SELECT EXISTS(SELECT 1 FROM `T_Analysis`)");
            }
            public static bool Exists2(string condition, params object[] param)
            {
                return _DAO.ExecuteScalar<bool>(string.Format("SELECT EXISTS(SELECT 1 FROM `T_Analysis` {0})", condition), param);
            }
            public static List<T_Analysis> SelectMultiple(ET_Analysis[] fields, string condition, params object[] param)
            {
                StringBuilder builder;
                if (fields == null || fields.Length == 0) builder = new StringBuilder("SELECT * FROM T_Analysis");
                else builder = GetSelectSQL(fields);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                return _DAO.SelectObjects<T_Analysis>(builder.ToString(), param);
            }
            public static PagedModel<T_Analysis> SelectPages(string __where, ET_Analysis[] fields, string conditionAfterWhere, int page, int pageSize, params object[] param)
            {
                var ret = SelectPages<T_Analysis>(__where, GetSelectSQL(fields).ToString(), conditionAfterWhere, page, pageSize, param);
                return ret;
            }
            public static PagedModel<T> SelectPages<T>(string __where, string selectSQL, string conditionAfterWhere, int page, int pageSize, params object[] param) where T : new()
            {
                return _DB.SelectPages<T>(_DAO, "SELECT count(`T_Analysis`.`RegisterID`) FROM `T_Analysis`", __where, selectSQL, conditionAfterWhere, page, pageSize, param);
            }
        }
        public partial class _T_Online : T_Online
        {
            public static ET_Online[] FIELD_ALL = { ET_Online.Time, ET_Online.GameName, ET_Online.Channel, ET_Online.Quarter0, ET_Online.Quarter1, ET_Online.Quarter2, ET_Online.Quarter3, ET_Online.Quarter4, ET_Online.Quarter5, ET_Online.Quarter6, ET_Online.Quarter7, ET_Online.Quarter8, ET_Online.Quarter9, ET_Online.Quarter10, ET_Online.Quarter11, ET_Online.Quarter12, ET_Online.Quarter13, ET_Online.Quarter14, ET_Online.Quarter15, ET_Online.Quarter16, ET_Online.Quarter17, ET_Online.Quarter18, ET_Online.Quarter19, ET_Online.Quarter20, ET_Online.Quarter21, ET_Online.Quarter22, ET_Online.Quarter23, ET_Online.Quarter24, ET_Online.Quarter25, ET_Online.Quarter26, ET_Online.Quarter27, ET_Online.Quarter28, ET_Online.Quarter29, ET_Online.Quarter30, ET_Online.Quarter31, ET_Online.Quarter32, ET_Online.Quarter33, ET_Online.Quarter34, ET_Online.Quarter35, ET_Online.Quarter36, ET_Online.Quarter37, ET_Online.Quarter38, ET_Online.Quarter39, ET_Online.Quarter40, ET_Online.Quarter41, ET_Online.Quarter42, ET_Online.Quarter43, ET_Online.Quarter44, ET_Online.Quarter45, ET_Online.Quarter46, ET_Online.Quarter47, ET_Online.Quarter48, ET_Online.Quarter49, ET_Online.Quarter50, ET_Online.Quarter51, ET_Online.Quarter52, ET_Online.Quarter53, ET_Online.Quarter54, ET_Online.Quarter55, ET_Online.Quarter56, ET_Online.Quarter57, ET_Online.Quarter58, ET_Online.Quarter59, ET_Online.Quarter60, ET_Online.Quarter61, ET_Online.Quarter62, ET_Online.Quarter63, ET_Online.Quarter64, ET_Online.Quarter65, ET_Online.Quarter66, ET_Online.Quarter67, ET_Online.Quarter68, ET_Online.Quarter69, ET_Online.Quarter70, ET_Online.Quarter71, ET_Online.Quarter72, ET_Online.Quarter73, ET_Online.Quarter74, ET_Online.Quarter75, ET_Online.Quarter76, ET_Online.Quarter77, ET_Online.Quarter78, ET_Online.Quarter79, ET_Online.Quarter80, ET_Online.Quarter81, ET_Online.Quarter82, ET_Online.Quarter83, ET_Online.Quarter84, ET_Online.Quarter85, ET_Online.Quarter86, ET_Online.Quarter87, ET_Online.Quarter88, ET_Online.Quarter89, ET_Online.Quarter90, ET_Online.Quarter91, ET_Online.Quarter92, ET_Online.Quarter93, ET_Online.Quarter94, ET_Online.Quarter95 };
            public static ET_Online[] FIELD_UPDATE = { ET_Online.Quarter0, ET_Online.Quarter1, ET_Online.Quarter2, ET_Online.Quarter3, ET_Online.Quarter4, ET_Online.Quarter5, ET_Online.Quarter6, ET_Online.Quarter7, ET_Online.Quarter8, ET_Online.Quarter9, ET_Online.Quarter10, ET_Online.Quarter11, ET_Online.Quarter12, ET_Online.Quarter13, ET_Online.Quarter14, ET_Online.Quarter15, ET_Online.Quarter16, ET_Online.Quarter17, ET_Online.Quarter18, ET_Online.Quarter19, ET_Online.Quarter20, ET_Online.Quarter21, ET_Online.Quarter22, ET_Online.Quarter23, ET_Online.Quarter24, ET_Online.Quarter25, ET_Online.Quarter26, ET_Online.Quarter27, ET_Online.Quarter28, ET_Online.Quarter29, ET_Online.Quarter30, ET_Online.Quarter31, ET_Online.Quarter32, ET_Online.Quarter33, ET_Online.Quarter34, ET_Online.Quarter35, ET_Online.Quarter36, ET_Online.Quarter37, ET_Online.Quarter38, ET_Online.Quarter39, ET_Online.Quarter40, ET_Online.Quarter41, ET_Online.Quarter42, ET_Online.Quarter43, ET_Online.Quarter44, ET_Online.Quarter45, ET_Online.Quarter46, ET_Online.Quarter47, ET_Online.Quarter48, ET_Online.Quarter49, ET_Online.Quarter50, ET_Online.Quarter51, ET_Online.Quarter52, ET_Online.Quarter53, ET_Online.Quarter54, ET_Online.Quarter55, ET_Online.Quarter56, ET_Online.Quarter57, ET_Online.Quarter58, ET_Online.Quarter59, ET_Online.Quarter60, ET_Online.Quarter61, ET_Online.Quarter62, ET_Online.Quarter63, ET_Online.Quarter64, ET_Online.Quarter65, ET_Online.Quarter66, ET_Online.Quarter67, ET_Online.Quarter68, ET_Online.Quarter69, ET_Online.Quarter70, ET_Online.Quarter71, ET_Online.Quarter72, ET_Online.Quarter73, ET_Online.Quarter74, ET_Online.Quarter75, ET_Online.Quarter76, ET_Online.Quarter77, ET_Online.Quarter78, ET_Online.Quarter79, ET_Online.Quarter80, ET_Online.Quarter81, ET_Online.Quarter82, ET_Online.Quarter83, ET_Online.Quarter84, ET_Online.Quarter85, ET_Online.Quarter86, ET_Online.Quarter87, ET_Online.Quarter88, ET_Online.Quarter89, ET_Online.Quarter90, ET_Online.Quarter91, ET_Online.Quarter92, ET_Online.Quarter93, ET_Online.Quarter94, ET_Online.Quarter95 };
            public static ET_Online[] NoNeedField(params ET_Online[] noNeed)
            {
                if (noNeed.Length == 0) return FIELD_ALL;
                List<ET_Online> list = new List<ET_Online>(FIELD_ALL.Length);
                for (int i = 0; i < FIELD_ALL.Length; i++)
                {
                    if (!noNeed.Contains(FIELD_ALL[i])) list.Add(FIELD_ALL[i]);
                }
                return list.ToArray();
            }
            public static int FieldCount { get { return FIELD_ALL.Length; } }
            
            public static T_Online Read(IDataReader reader)
            {
                return Read(reader, 0, FieldCount);
            }
            public static T_Online Read(IDataReader reader, int offset)
            {
                return Read(reader, offset, FieldCount);
            }
            public static T_Online Read(IDataReader reader, int offset, int fieldCount)
            {
                return _DATABASE.ReadObject<T_Online>(reader, offset, fieldCount);
            }
            public static void MultiReadPrepare(IDataReader reader, int offset, int fieldCount, out List<PropertyInfo> properties, out List<FieldInfo> fields, ref int[] indices)
            {
                _DATABASE.MultiReadPrepare(reader, typeof(T_Online), offset, fieldCount, out properties, out fields, ref indices);
            }
            public static T_Online MultiRead(IDataReader reader, int offset, int fieldCount, List<PropertyInfo> properties, List<FieldInfo> fields, int[] indices)
            {
                return _DATABASE.MultiRead<T_Online>(reader, offset, fieldCount, properties, fields, indices)
                ;
            }
            public static void GetInsertSQL(T_Online target, StringBuilder builder, List<object> values)
            {
                int index = values.Count;
                builder.AppendFormat("INSERT `T_Online`(`Time`, `GameName`, `Channel`, `Quarter0`, `Quarter1`, `Quarter2`, `Quarter3`, `Quarter4`, `Quarter5`, `Quarter6`, `Quarter7`, `Quarter8`, `Quarter9`, `Quarter10`, `Quarter11`, `Quarter12`, `Quarter13`, `Quarter14`, `Quarter15`, `Quarter16`, `Quarter17`, `Quarter18`, `Quarter19`, `Quarter20`, `Quarter21`, `Quarter22`, `Quarter23`, `Quarter24`, `Quarter25`, `Quarter26`, `Quarter27`, `Quarter28`, `Quarter29`, `Quarter30`, `Quarter31`, `Quarter32`, `Quarter33`, `Quarter34`, `Quarter35`, `Quarter36`, `Quarter37`, `Quarter38`, `Quarter39`, `Quarter40`, `Quarter41`, `Quarter42`, `Quarter43`, `Quarter44`, `Quarter45`, `Quarter46`, `Quarter47`, `Quarter48`, `Quarter49`, `Quarter50`, `Quarter51`, `Quarter52`, `Quarter53`, `Quarter54`, `Quarter55`, `Quarter56`, `Quarter57`, `Quarter58`, `Quarter59`, `Quarter60`, `Quarter61`, `Quarter62`, `Quarter63`, `Quarter64`, `Quarter65`, `Quarter66`, `Quarter67`, `Quarter68`, `Quarter69`, `Quarter70`, `Quarter71`, `Quarter72`, `Quarter73`, `Quarter74`, `Quarter75`, `Quarter76`, `Quarter77`, `Quarter78`, `Quarter79`, `Quarter80`, `Quarter81`, `Quarter82`, `Quarter83`, `Quarter84`, `Quarter85`, `Quarter86`, `Quarter87`, `Quarter88`, `Quarter89`, `Quarter90`, `Quarter91`, `Quarter92`, `Quarter93`, `Quarter94`, `Quarter95`) VALUES(");
                for (int i = 0, n = 98; i <= n; i++)
                {
                    builder.AppendFormat("@p{0}", index++);
                    if (i != n) builder.Append(", ");
                }
                builder.AppendLine(");");
                values.Add(target.Time);
                values.Add(target.GameName);
                values.Add(target.Channel);
                values.Add(target.Quarter0);
                values.Add(target.Quarter1);
                values.Add(target.Quarter2);
                values.Add(target.Quarter3);
                values.Add(target.Quarter4);
                values.Add(target.Quarter5);
                values.Add(target.Quarter6);
                values.Add(target.Quarter7);
                values.Add(target.Quarter8);
                values.Add(target.Quarter9);
                values.Add(target.Quarter10);
                values.Add(target.Quarter11);
                values.Add(target.Quarter12);
                values.Add(target.Quarter13);
                values.Add(target.Quarter14);
                values.Add(target.Quarter15);
                values.Add(target.Quarter16);
                values.Add(target.Quarter17);
                values.Add(target.Quarter18);
                values.Add(target.Quarter19);
                values.Add(target.Quarter20);
                values.Add(target.Quarter21);
                values.Add(target.Quarter22);
                values.Add(target.Quarter23);
                values.Add(target.Quarter24);
                values.Add(target.Quarter25);
                values.Add(target.Quarter26);
                values.Add(target.Quarter27);
                values.Add(target.Quarter28);
                values.Add(target.Quarter29);
                values.Add(target.Quarter30);
                values.Add(target.Quarter31);
                values.Add(target.Quarter32);
                values.Add(target.Quarter33);
                values.Add(target.Quarter34);
                values.Add(target.Quarter35);
                values.Add(target.Quarter36);
                values.Add(target.Quarter37);
                values.Add(target.Quarter38);
                values.Add(target.Quarter39);
                values.Add(target.Quarter40);
                values.Add(target.Quarter41);
                values.Add(target.Quarter42);
                values.Add(target.Quarter43);
                values.Add(target.Quarter44);
                values.Add(target.Quarter45);
                values.Add(target.Quarter46);
                values.Add(target.Quarter47);
                values.Add(target.Quarter48);
                values.Add(target.Quarter49);
                values.Add(target.Quarter50);
                values.Add(target.Quarter51);
                values.Add(target.Quarter52);
                values.Add(target.Quarter53);
                values.Add(target.Quarter54);
                values.Add(target.Quarter55);
                values.Add(target.Quarter56);
                values.Add(target.Quarter57);
                values.Add(target.Quarter58);
                values.Add(target.Quarter59);
                values.Add(target.Quarter60);
                values.Add(target.Quarter61);
                values.Add(target.Quarter62);
                values.Add(target.Quarter63);
                values.Add(target.Quarter64);
                values.Add(target.Quarter65);
                values.Add(target.Quarter66);
                values.Add(target.Quarter67);
                values.Add(target.Quarter68);
                values.Add(target.Quarter69);
                values.Add(target.Quarter70);
                values.Add(target.Quarter71);
                values.Add(target.Quarter72);
                values.Add(target.Quarter73);
                values.Add(target.Quarter74);
                values.Add(target.Quarter75);
                values.Add(target.Quarter76);
                values.Add(target.Quarter77);
                values.Add(target.Quarter78);
                values.Add(target.Quarter79);
                values.Add(target.Quarter80);
                values.Add(target.Quarter81);
                values.Add(target.Quarter82);
                values.Add(target.Quarter83);
                values.Add(target.Quarter84);
                values.Add(target.Quarter85);
                values.Add(target.Quarter86);
                values.Add(target.Quarter87);
                values.Add(target.Quarter88);
                values.Add(target.Quarter89);
                values.Add(target.Quarter90);
                values.Add(target.Quarter91);
                values.Add(target.Quarter92);
                values.Add(target.Quarter93);
                values.Add(target.Quarter94);
                values.Add(target.Quarter95);
            }
            public static void Insert(T_Online target)
            {
                StringBuilder builder = new StringBuilder();
                List<object> values = new List<object>(99);
                GetInsertSQL(target, builder, values);
                _DAO.ExecuteNonQuery(builder.ToString(), values.ToArray());
            }
            public static void GetDeleteSQL(System.DateTime Time, string GameName, string Channel, StringBuilder builder, List<object> values)
            {
                int index = values.Count;
                builder.AppendFormat("DELETE FROM `T_Online` WHERE `Time` = @p{0} AND `GameName` = @p{1} AND `Channel` = @p{2};", index++, index++, index++);
                values.Add(Time);
                values.Add(GameName);
                values.Add(Channel);
            }
            public static int Delete(System.DateTime Time, string GameName, string Channel)
            {
                return _DAO.ExecuteNonQuery("DELETE FROM `T_Online` WHERE `Time` = @p0 AND `GameName` = @p1 AND `Channel` = @p2", Time, GameName, Channel);
            }
            public static int DeleteByTime(System.DateTime Time)
            {
                return _DAO.ExecuteNonQuery("DELETE FROM `T_Online` WHERE `Time` = @p0;", Time);
            }
            public static int DeleteByGameName(string GameName)
            {
                return _DAO.ExecuteNonQuery("DELETE FROM `T_Online` WHERE `GameName` = @p0;", GameName);
            }
            public static int DeleteByChannel(string Channel)
            {
                return _DAO.ExecuteNonQuery("DELETE FROM `T_Online` WHERE `Channel` = @p0;", Channel);
            }
            public static void GetUpdateSQL(T_Online target, string condition, StringBuilder builder, List<object> values, params ET_Online[] fields)
            {
                int index = values.Count;
                bool all = fields.Length == 0 || fields == FIELD_UPDATE;
                builder.Append("UPDATE `T_Online` SET");
                if (all || fields.Contains(ET_Online.Quarter0))
                {
                    builder.AppendFormat(" `Quarter0` = @p{0},", index++);
                    values.Add(target.Quarter0);
                }
                if (all || fields.Contains(ET_Online.Quarter1))
                {
                    builder.AppendFormat(" `Quarter1` = @p{0},", index++);
                    values.Add(target.Quarter1);
                }
                if (all || fields.Contains(ET_Online.Quarter2))
                {
                    builder.AppendFormat(" `Quarter2` = @p{0},", index++);
                    values.Add(target.Quarter2);
                }
                if (all || fields.Contains(ET_Online.Quarter3))
                {
                    builder.AppendFormat(" `Quarter3` = @p{0},", index++);
                    values.Add(target.Quarter3);
                }
                if (all || fields.Contains(ET_Online.Quarter4))
                {
                    builder.AppendFormat(" `Quarter4` = @p{0},", index++);
                    values.Add(target.Quarter4);
                }
                if (all || fields.Contains(ET_Online.Quarter5))
                {
                    builder.AppendFormat(" `Quarter5` = @p{0},", index++);
                    values.Add(target.Quarter5);
                }
                if (all || fields.Contains(ET_Online.Quarter6))
                {
                    builder.AppendFormat(" `Quarter6` = @p{0},", index++);
                    values.Add(target.Quarter6);
                }
                if (all || fields.Contains(ET_Online.Quarter7))
                {
                    builder.AppendFormat(" `Quarter7` = @p{0},", index++);
                    values.Add(target.Quarter7);
                }
                if (all || fields.Contains(ET_Online.Quarter8))
                {
                    builder.AppendFormat(" `Quarter8` = @p{0},", index++);
                    values.Add(target.Quarter8);
                }
                if (all || fields.Contains(ET_Online.Quarter9))
                {
                    builder.AppendFormat(" `Quarter9` = @p{0},", index++);
                    values.Add(target.Quarter9);
                }
                if (all || fields.Contains(ET_Online.Quarter10))
                {
                    builder.AppendFormat(" `Quarter10` = @p{0},", index++);
                    values.Add(target.Quarter10);
                }
                if (all || fields.Contains(ET_Online.Quarter11))
                {
                    builder.AppendFormat(" `Quarter11` = @p{0},", index++);
                    values.Add(target.Quarter11);
                }
                if (all || fields.Contains(ET_Online.Quarter12))
                {
                    builder.AppendFormat(" `Quarter12` = @p{0},", index++);
                    values.Add(target.Quarter12);
                }
                if (all || fields.Contains(ET_Online.Quarter13))
                {
                    builder.AppendFormat(" `Quarter13` = @p{0},", index++);
                    values.Add(target.Quarter13);
                }
                if (all || fields.Contains(ET_Online.Quarter14))
                {
                    builder.AppendFormat(" `Quarter14` = @p{0},", index++);
                    values.Add(target.Quarter14);
                }
                if (all || fields.Contains(ET_Online.Quarter15))
                {
                    builder.AppendFormat(" `Quarter15` = @p{0},", index++);
                    values.Add(target.Quarter15);
                }
                if (all || fields.Contains(ET_Online.Quarter16))
                {
                    builder.AppendFormat(" `Quarter16` = @p{0},", index++);
                    values.Add(target.Quarter16);
                }
                if (all || fields.Contains(ET_Online.Quarter17))
                {
                    builder.AppendFormat(" `Quarter17` = @p{0},", index++);
                    values.Add(target.Quarter17);
                }
                if (all || fields.Contains(ET_Online.Quarter18))
                {
                    builder.AppendFormat(" `Quarter18` = @p{0},", index++);
                    values.Add(target.Quarter18);
                }
                if (all || fields.Contains(ET_Online.Quarter19))
                {
                    builder.AppendFormat(" `Quarter19` = @p{0},", index++);
                    values.Add(target.Quarter19);
                }
                if (all || fields.Contains(ET_Online.Quarter20))
                {
                    builder.AppendFormat(" `Quarter20` = @p{0},", index++);
                    values.Add(target.Quarter20);
                }
                if (all || fields.Contains(ET_Online.Quarter21))
                {
                    builder.AppendFormat(" `Quarter21` = @p{0},", index++);
                    values.Add(target.Quarter21);
                }
                if (all || fields.Contains(ET_Online.Quarter22))
                {
                    builder.AppendFormat(" `Quarter22` = @p{0},", index++);
                    values.Add(target.Quarter22);
                }
                if (all || fields.Contains(ET_Online.Quarter23))
                {
                    builder.AppendFormat(" `Quarter23` = @p{0},", index++);
                    values.Add(target.Quarter23);
                }
                if (all || fields.Contains(ET_Online.Quarter24))
                {
                    builder.AppendFormat(" `Quarter24` = @p{0},", index++);
                    values.Add(target.Quarter24);
                }
                if (all || fields.Contains(ET_Online.Quarter25))
                {
                    builder.AppendFormat(" `Quarter25` = @p{0},", index++);
                    values.Add(target.Quarter25);
                }
                if (all || fields.Contains(ET_Online.Quarter26))
                {
                    builder.AppendFormat(" `Quarter26` = @p{0},", index++);
                    values.Add(target.Quarter26);
                }
                if (all || fields.Contains(ET_Online.Quarter27))
                {
                    builder.AppendFormat(" `Quarter27` = @p{0},", index++);
                    values.Add(target.Quarter27);
                }
                if (all || fields.Contains(ET_Online.Quarter28))
                {
                    builder.AppendFormat(" `Quarter28` = @p{0},", index++);
                    values.Add(target.Quarter28);
                }
                if (all || fields.Contains(ET_Online.Quarter29))
                {
                    builder.AppendFormat(" `Quarter29` = @p{0},", index++);
                    values.Add(target.Quarter29);
                }
                if (all || fields.Contains(ET_Online.Quarter30))
                {
                    builder.AppendFormat(" `Quarter30` = @p{0},", index++);
                    values.Add(target.Quarter30);
                }
                if (all || fields.Contains(ET_Online.Quarter31))
                {
                    builder.AppendFormat(" `Quarter31` = @p{0},", index++);
                    values.Add(target.Quarter31);
                }
                if (all || fields.Contains(ET_Online.Quarter32))
                {
                    builder.AppendFormat(" `Quarter32` = @p{0},", index++);
                    values.Add(target.Quarter32);
                }
                if (all || fields.Contains(ET_Online.Quarter33))
                {
                    builder.AppendFormat(" `Quarter33` = @p{0},", index++);
                    values.Add(target.Quarter33);
                }
                if (all || fields.Contains(ET_Online.Quarter34))
                {
                    builder.AppendFormat(" `Quarter34` = @p{0},", index++);
                    values.Add(target.Quarter34);
                }
                if (all || fields.Contains(ET_Online.Quarter35))
                {
                    builder.AppendFormat(" `Quarter35` = @p{0},", index++);
                    values.Add(target.Quarter35);
                }
                if (all || fields.Contains(ET_Online.Quarter36))
                {
                    builder.AppendFormat(" `Quarter36` = @p{0},", index++);
                    values.Add(target.Quarter36);
                }
                if (all || fields.Contains(ET_Online.Quarter37))
                {
                    builder.AppendFormat(" `Quarter37` = @p{0},", index++);
                    values.Add(target.Quarter37);
                }
                if (all || fields.Contains(ET_Online.Quarter38))
                {
                    builder.AppendFormat(" `Quarter38` = @p{0},", index++);
                    values.Add(target.Quarter38);
                }
                if (all || fields.Contains(ET_Online.Quarter39))
                {
                    builder.AppendFormat(" `Quarter39` = @p{0},", index++);
                    values.Add(target.Quarter39);
                }
                if (all || fields.Contains(ET_Online.Quarter40))
                {
                    builder.AppendFormat(" `Quarter40` = @p{0},", index++);
                    values.Add(target.Quarter40);
                }
                if (all || fields.Contains(ET_Online.Quarter41))
                {
                    builder.AppendFormat(" `Quarter41` = @p{0},", index++);
                    values.Add(target.Quarter41);
                }
                if (all || fields.Contains(ET_Online.Quarter42))
                {
                    builder.AppendFormat(" `Quarter42` = @p{0},", index++);
                    values.Add(target.Quarter42);
                }
                if (all || fields.Contains(ET_Online.Quarter43))
                {
                    builder.AppendFormat(" `Quarter43` = @p{0},", index++);
                    values.Add(target.Quarter43);
                }
                if (all || fields.Contains(ET_Online.Quarter44))
                {
                    builder.AppendFormat(" `Quarter44` = @p{0},", index++);
                    values.Add(target.Quarter44);
                }
                if (all || fields.Contains(ET_Online.Quarter45))
                {
                    builder.AppendFormat(" `Quarter45` = @p{0},", index++);
                    values.Add(target.Quarter45);
                }
                if (all || fields.Contains(ET_Online.Quarter46))
                {
                    builder.AppendFormat(" `Quarter46` = @p{0},", index++);
                    values.Add(target.Quarter46);
                }
                if (all || fields.Contains(ET_Online.Quarter47))
                {
                    builder.AppendFormat(" `Quarter47` = @p{0},", index++);
                    values.Add(target.Quarter47);
                }
                if (all || fields.Contains(ET_Online.Quarter48))
                {
                    builder.AppendFormat(" `Quarter48` = @p{0},", index++);
                    values.Add(target.Quarter48);
                }
                if (all || fields.Contains(ET_Online.Quarter49))
                {
                    builder.AppendFormat(" `Quarter49` = @p{0},", index++);
                    values.Add(target.Quarter49);
                }
                if (all || fields.Contains(ET_Online.Quarter50))
                {
                    builder.AppendFormat(" `Quarter50` = @p{0},", index++);
                    values.Add(target.Quarter50);
                }
                if (all || fields.Contains(ET_Online.Quarter51))
                {
                    builder.AppendFormat(" `Quarter51` = @p{0},", index++);
                    values.Add(target.Quarter51);
                }
                if (all || fields.Contains(ET_Online.Quarter52))
                {
                    builder.AppendFormat(" `Quarter52` = @p{0},", index++);
                    values.Add(target.Quarter52);
                }
                if (all || fields.Contains(ET_Online.Quarter53))
                {
                    builder.AppendFormat(" `Quarter53` = @p{0},", index++);
                    values.Add(target.Quarter53);
                }
                if (all || fields.Contains(ET_Online.Quarter54))
                {
                    builder.AppendFormat(" `Quarter54` = @p{0},", index++);
                    values.Add(target.Quarter54);
                }
                if (all || fields.Contains(ET_Online.Quarter55))
                {
                    builder.AppendFormat(" `Quarter55` = @p{0},", index++);
                    values.Add(target.Quarter55);
                }
                if (all || fields.Contains(ET_Online.Quarter56))
                {
                    builder.AppendFormat(" `Quarter56` = @p{0},", index++);
                    values.Add(target.Quarter56);
                }
                if (all || fields.Contains(ET_Online.Quarter57))
                {
                    builder.AppendFormat(" `Quarter57` = @p{0},", index++);
                    values.Add(target.Quarter57);
                }
                if (all || fields.Contains(ET_Online.Quarter58))
                {
                    builder.AppendFormat(" `Quarter58` = @p{0},", index++);
                    values.Add(target.Quarter58);
                }
                if (all || fields.Contains(ET_Online.Quarter59))
                {
                    builder.AppendFormat(" `Quarter59` = @p{0},", index++);
                    values.Add(target.Quarter59);
                }
                if (all || fields.Contains(ET_Online.Quarter60))
                {
                    builder.AppendFormat(" `Quarter60` = @p{0},", index++);
                    values.Add(target.Quarter60);
                }
                if (all || fields.Contains(ET_Online.Quarter61))
                {
                    builder.AppendFormat(" `Quarter61` = @p{0},", index++);
                    values.Add(target.Quarter61);
                }
                if (all || fields.Contains(ET_Online.Quarter62))
                {
                    builder.AppendFormat(" `Quarter62` = @p{0},", index++);
                    values.Add(target.Quarter62);
                }
                if (all || fields.Contains(ET_Online.Quarter63))
                {
                    builder.AppendFormat(" `Quarter63` = @p{0},", index++);
                    values.Add(target.Quarter63);
                }
                if (all || fields.Contains(ET_Online.Quarter64))
                {
                    builder.AppendFormat(" `Quarter64` = @p{0},", index++);
                    values.Add(target.Quarter64);
                }
                if (all || fields.Contains(ET_Online.Quarter65))
                {
                    builder.AppendFormat(" `Quarter65` = @p{0},", index++);
                    values.Add(target.Quarter65);
                }
                if (all || fields.Contains(ET_Online.Quarter66))
                {
                    builder.AppendFormat(" `Quarter66` = @p{0},", index++);
                    values.Add(target.Quarter66);
                }
                if (all || fields.Contains(ET_Online.Quarter67))
                {
                    builder.AppendFormat(" `Quarter67` = @p{0},", index++);
                    values.Add(target.Quarter67);
                }
                if (all || fields.Contains(ET_Online.Quarter68))
                {
                    builder.AppendFormat(" `Quarter68` = @p{0},", index++);
                    values.Add(target.Quarter68);
                }
                if (all || fields.Contains(ET_Online.Quarter69))
                {
                    builder.AppendFormat(" `Quarter69` = @p{0},", index++);
                    values.Add(target.Quarter69);
                }
                if (all || fields.Contains(ET_Online.Quarter70))
                {
                    builder.AppendFormat(" `Quarter70` = @p{0},", index++);
                    values.Add(target.Quarter70);
                }
                if (all || fields.Contains(ET_Online.Quarter71))
                {
                    builder.AppendFormat(" `Quarter71` = @p{0},", index++);
                    values.Add(target.Quarter71);
                }
                if (all || fields.Contains(ET_Online.Quarter72))
                {
                    builder.AppendFormat(" `Quarter72` = @p{0},", index++);
                    values.Add(target.Quarter72);
                }
                if (all || fields.Contains(ET_Online.Quarter73))
                {
                    builder.AppendFormat(" `Quarter73` = @p{0},", index++);
                    values.Add(target.Quarter73);
                }
                if (all || fields.Contains(ET_Online.Quarter74))
                {
                    builder.AppendFormat(" `Quarter74` = @p{0},", index++);
                    values.Add(target.Quarter74);
                }
                if (all || fields.Contains(ET_Online.Quarter75))
                {
                    builder.AppendFormat(" `Quarter75` = @p{0},", index++);
                    values.Add(target.Quarter75);
                }
                if (all || fields.Contains(ET_Online.Quarter76))
                {
                    builder.AppendFormat(" `Quarter76` = @p{0},", index++);
                    values.Add(target.Quarter76);
                }
                if (all || fields.Contains(ET_Online.Quarter77))
                {
                    builder.AppendFormat(" `Quarter77` = @p{0},", index++);
                    values.Add(target.Quarter77);
                }
                if (all || fields.Contains(ET_Online.Quarter78))
                {
                    builder.AppendFormat(" `Quarter78` = @p{0},", index++);
                    values.Add(target.Quarter78);
                }
                if (all || fields.Contains(ET_Online.Quarter79))
                {
                    builder.AppendFormat(" `Quarter79` = @p{0},", index++);
                    values.Add(target.Quarter79);
                }
                if (all || fields.Contains(ET_Online.Quarter80))
                {
                    builder.AppendFormat(" `Quarter80` = @p{0},", index++);
                    values.Add(target.Quarter80);
                }
                if (all || fields.Contains(ET_Online.Quarter81))
                {
                    builder.AppendFormat(" `Quarter81` = @p{0},", index++);
                    values.Add(target.Quarter81);
                }
                if (all || fields.Contains(ET_Online.Quarter82))
                {
                    builder.AppendFormat(" `Quarter82` = @p{0},", index++);
                    values.Add(target.Quarter82);
                }
                if (all || fields.Contains(ET_Online.Quarter83))
                {
                    builder.AppendFormat(" `Quarter83` = @p{0},", index++);
                    values.Add(target.Quarter83);
                }
                if (all || fields.Contains(ET_Online.Quarter84))
                {
                    builder.AppendFormat(" `Quarter84` = @p{0},", index++);
                    values.Add(target.Quarter84);
                }
                if (all || fields.Contains(ET_Online.Quarter85))
                {
                    builder.AppendFormat(" `Quarter85` = @p{0},", index++);
                    values.Add(target.Quarter85);
                }
                if (all || fields.Contains(ET_Online.Quarter86))
                {
                    builder.AppendFormat(" `Quarter86` = @p{0},", index++);
                    values.Add(target.Quarter86);
                }
                if (all || fields.Contains(ET_Online.Quarter87))
                {
                    builder.AppendFormat(" `Quarter87` = @p{0},", index++);
                    values.Add(target.Quarter87);
                }
                if (all || fields.Contains(ET_Online.Quarter88))
                {
                    builder.AppendFormat(" `Quarter88` = @p{0},", index++);
                    values.Add(target.Quarter88);
                }
                if (all || fields.Contains(ET_Online.Quarter89))
                {
                    builder.AppendFormat(" `Quarter89` = @p{0},", index++);
                    values.Add(target.Quarter89);
                }
                if (all || fields.Contains(ET_Online.Quarter90))
                {
                    builder.AppendFormat(" `Quarter90` = @p{0},", index++);
                    values.Add(target.Quarter90);
                }
                if (all || fields.Contains(ET_Online.Quarter91))
                {
                    builder.AppendFormat(" `Quarter91` = @p{0},", index++);
                    values.Add(target.Quarter91);
                }
                if (all || fields.Contains(ET_Online.Quarter92))
                {
                    builder.AppendFormat(" `Quarter92` = @p{0},", index++);
                    values.Add(target.Quarter92);
                }
                if (all || fields.Contains(ET_Online.Quarter93))
                {
                    builder.AppendFormat(" `Quarter93` = @p{0},", index++);
                    values.Add(target.Quarter93);
                }
                if (all || fields.Contains(ET_Online.Quarter94))
                {
                    builder.AppendFormat(" `Quarter94` = @p{0},", index++);
                    values.Add(target.Quarter94);
                }
                if (all || fields.Contains(ET_Online.Quarter95))
                {
                    builder.AppendFormat(" `Quarter95` = @p{0},", index++);
                    values.Add(target.Quarter95);
                }
                if (index == 0) return;
                builder[builder.Length - 1] = ' ';
                if (!string.IsNullOrEmpty(condition)) builder.Append(condition);
                else
                {
                    builder.AppendFormat("WHERE `Time` = @p{0} AND `GameName` = @p{1} AND `Channel` = @p{2}", index++, index++, index++);
                    values.Add(target.Time);
                    values.Add(target.GameName);
                    values.Add(target.Channel);
                }
                builder.AppendLine(";");
            }
            /// <summary>condition that 'where' or 'join' without ';'</summary>
            public static int Update(T_Online target, string condition, params ET_Online[] fields)
            {
                StringBuilder builder = new StringBuilder();
                List<object> values = new List<object>(fields.Length + 3);
                GetUpdateSQL(target, condition, builder, values, fields);
                return _DAO.ExecuteNonQuery(builder.ToString(), values.ToArray());
            }
            public static void GetSelectField(string tableName, StringBuilder builder, params ET_Online[] fields)
            {
                if (string.IsNullOrEmpty(tableName)) tableName = "`T_Online`";
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
            public static StringBuilder GetSelectSQL(params ET_Online[] fields)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("SELECT ");
                GetSelectField(null, builder, fields);
                builder.AppendLine(" FROM `T_Online`");
                return builder;
            }
            public static T_Online Select(System.DateTime __Time, string __GameName, string __Channel, params ET_Online[] fields)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.Append(" WHERE `Time` = @p0 AND `GameName` = @p1 AND `Channel` = @p2;");
                var ret = _DAO.SelectObject<T_Online>(builder.ToString(), __Time, __GameName, __Channel);
                if (ret != default(T_Online))
                {
                    ret.Time = __Time;
                    ret.GameName = __GameName;
                    ret.Channel = __Channel;
                }
                return ret;
            }
            public static T_Online Select(ET_Online[] fields, string condition, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                return _DAO.SelectObject<T_Online>(builder.ToString(), param);
            }
            public static bool Exists(System.DateTime __Time, string __GameName, string __Channel)
            {
                return _DAO.ExecuteScalar<bool>("SELECT EXISTS(SELECT 1 FROM `T_Online` WHERE `Time` = @p0 AND `GameName` = @p1 AND `Channel` = @p2)", __Time, __GameName, __Channel);
            }
            public static bool Exists2(string condition, params object[] param)
            {
                return _DAO.ExecuteScalar<bool>(string.Format("SELECT EXISTS(SELECT 1 FROM `T_Online` {0})", condition), param);
            }
            public static List<T_Online> SelectMultiple(ET_Online[] fields, string condition, params object[] param)
            {
                StringBuilder builder;
                if (fields == null || fields.Length == 0) builder = new StringBuilder("SELECT * FROM T_Online");
                else builder = GetSelectSQL(fields);
                if (!string.IsNullOrEmpty(condition)) builder.Append(" {0}", condition);
                builder.Append(';');
                return _DAO.SelectObjects<T_Online>(builder.ToString(), param);
            }
            public static List<T_Online> SelectMultipleByTime(ET_Online[] fields, System.DateTime Time, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `Time` = @p{0}", param.Length + 0);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_Online>(builder.ToString(), Time);
                else return _DAO.SelectObjects<T_Online>(builder.ToString(), param.Add(Time));
            }
            public static List<T_Online> SelectMultipleByGameName(ET_Online[] fields, string GameName, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `GameName` = @p{0}", param.Length + 0);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_Online>(builder.ToString(), GameName);
                else return _DAO.SelectObjects<T_Online>(builder.ToString(), param.Add(GameName));
            }
            public static List<T_Online> SelectMultipleByChannel(ET_Online[] fields, string Channel, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `Channel` = @p{0}", param.Length + 0);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_Online>(builder.ToString(), Channel);
                else return _DAO.SelectObjects<T_Online>(builder.ToString(), param.Add(Channel));
            }
            public static List<T_Online> SelectMultipleByTime_GameName(ET_Online[] fields, System.DateTime Time, string GameName, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `Time` = @p{0} AND `GameName` = @p{1}", param.Length + 0, param.Length + 1);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_Online>(builder.ToString(), Time, GameName);
                else return _DAO.SelectObjects<T_Online>(builder.ToString(), param.Add(Time, GameName));
            }
            public static List<T_Online> SelectMultipleByTime_Channel(ET_Online[] fields, System.DateTime Time, string Channel, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `Time` = @p{0} AND `Channel` = @p{1}", param.Length + 0, param.Length + 1);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_Online>(builder.ToString(), Time, Channel);
                else return _DAO.SelectObjects<T_Online>(builder.ToString(), param.Add(Time, Channel));
            }
            public static List<T_Online> SelectMultipleByGameName_Channel(ET_Online[] fields, string GameName, string Channel, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `GameName` = @p{0} AND `Channel` = @p{1}", param.Length + 0, param.Length + 1);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_Online>(builder.ToString(), GameName, Channel);
                else return _DAO.SelectObjects<T_Online>(builder.ToString(), param.Add(GameName, Channel));
            }
            public static List<T_Online> SelectMultipleByTime_GameName_Channel(ET_Online[] fields, System.DateTime Time, string GameName, string Channel, string conditionAfterWhere, params object[] param)
            {
                StringBuilder builder = GetSelectSQL(fields);
                builder.AppendFormat(" WHERE `Time` = @p{0} AND `GameName` = @p{1} AND `Channel` = @p{2}", param.Length + 0, param.Length + 1, param.Length + 2);
                if (!string.IsNullOrEmpty(conditionAfterWhere)) builder.Append(" {0}", conditionAfterWhere);
                builder.Append(';');
                if (param.Length == 0) return _DAO.SelectObjects<T_Online>(builder.ToString(), Time, GameName, Channel);
                else return _DAO.SelectObjects<T_Online>(builder.ToString(), param.Add(Time, GameName, Channel));
            }
            public static PagedModel<T_Online> SelectPages(string __where, ET_Online[] fields, string conditionAfterWhere, int page, int pageSize, params object[] param)
            {
                var ret = SelectPages<T_Online>(__where, GetSelectSQL(fields).ToString(), conditionAfterWhere, page, pageSize, param);
                return ret;
            }
            public static PagedModel<T> SelectPages<T>(string __where, string selectSQL, string conditionAfterWhere, int page, int pageSize, params object[] param) where T : new()
            {
                return _DB.SelectPages<T>(_DAO, "SELECT count(`T_Online`.`Time`) FROM `T_Online`", __where, selectSQL, conditionAfterWhere, page, pageSize, param);
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
                reader.Read();
                result.Count = (int)(long)reader[0];
                result.Models = new List<T>();
                reader.NextResult();
                read(reader, result.Models);
            }
            , builder.ToString(), __param);
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
