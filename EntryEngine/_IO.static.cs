using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryEngine
{
    static partial class _IO
    {
        public static iO _iO = new EntryEngine._IO.iO();
        
        public static System.Text.Encoding IOEncoding
        {
            get
            {
                #if EntryBuilder
                throw new System.NotImplementedException();
                #else
                return _iO.IOEncoding;
                #endif
            }
            set
            {
                #if EntryBuilder
                throw new System.NotImplementedException();
                #else
                _iO.IOEncoding = value;
                #endif
            }
        }
        public static string RootDirectory
        {
            get
            {
                #if EntryBuilder
                throw new System.NotImplementedException();
                #else
                return _iO.RootDirectory;
                #endif
            }
            set
            {
                #if EntryBuilder
                throw new System.NotImplementedException();
                #else
                _iO.RootDirectory = value;
                #endif
            }
        }
        public static string BuildPath(string file)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _iO.BuildPath(file);
            #endif
        }
        public static string ReadText(string file)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _iO.ReadText(file);
            #endif
        }
        public static byte[] ReadByte(string file)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _iO.ReadByte(file);
            #endif
        }
        public static EntryEngine.AsyncReadFile ReadAsync(string file)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _iO.ReadAsync(file);
            #endif
        }
        public static string ReadPreambleText(byte[] bytes)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _iO.ReadPreambleText(bytes);
            #endif
        }
        public static void WriteText(string file, string content)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            _iO.WriteText(file, content);
            #endif
        }
        public static void WriteText(string file, string content, System.Text.Encoding encoding)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            _iO.WriteText(file, content, encoding);
            #endif
        }
        public static void WriteByte(string file, byte[] content)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            _iO.WriteByte(file, content);
            #endif
        }
    }
}
