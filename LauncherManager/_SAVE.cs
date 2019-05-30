using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Serialize;
using EntryEngine;

namespace LauncherManager
{
    class User
    {
        public string Name;
        public List<Platform> Managed = new List<Platform>();
    }
    class Platform
    {
        public string Name;
        public string IP;
        public ushort Port;

        public string FullName
        {
            get { return string.Format("{0} - {1}:{2}", Name, IP, Port); }
        }
    }
    static class _SAVE
    {
        private const string SAVE_DATA = "data.sav";


        public static List<User> Users = new List<User>();


        public static void Save()
        {
            ByteRefWriter writer = new ByteRefWriter(SerializeSetting.DefaultSerializeStatic);
            writer.WriteObject(null, typeof(_SAVE));
            _IO.WriteByte(SAVE_DATA, writer.GetBuffer());
        }
        public static void Load()
        {
            try
            {
                ByteRefReader reader = new ByteRefReader(_IO.ReadByte(SAVE_DATA), SerializeSetting.DefaultSerializeStatic);
                reader.ReadObject(typeof(_SAVE));
            }
            catch (Exception ex)
            {
                Save();
            }
        }
    }
}
