using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryEngine
{
    static partial class _LOG
    {
        public static Logger _Logger = new EntryEngine._LOG.LoggerEmpty();
        
        public static void Append(string value, params object[] param)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            _Logger.Append(value, param);
            #endif
        }
        public static void AppendException(System.Exception ex)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            _Logger.AppendException(ex);
            #endif
        }
        public static void Write(byte level, string value, params object[] param)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            _Logger.Write(level, value, param);
            #endif
        }
        public static void Log(ref EntryEngine.Record record)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            _Logger.Log(ref record);
            #endif
        }
        public static void Debug(string value, params object[] param)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            _Logger.Debug(value, param);
            #endif
        }
        public static void Info(string value, params object[] param)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            _Logger.Info(value, param);
            #endif
        }
        public static void Warning(string value, params object[] param)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            _Logger.Warning(value, param);
            #endif
        }
        public static void Error(string value, params object[] param)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            _Logger.Error(value, param);
            #endif
        }
        public static void Error(System.Exception ex, string message, params object[] param)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            _Logger.Error(ex, message, param);
            #endif
        }
        public static void Statistics(string key, int value)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            _Logger.Statistics(key, value);
            #endif
        }
    }
}
