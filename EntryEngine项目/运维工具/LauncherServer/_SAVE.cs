using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Serialize;
using LauncherProtocolStructure;
using LauncherManagerProtocol;

namespace LauncherServer
{
    static class _SAVE
    {
        private const string SAVE_DATA = "data.sav";

        public static List<ServiceType> ServiceTypes = new List<ServiceType>();
        public static List<Manager> Managers = new List<Manager>();

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
                foreach (var item in Managers)
                {
                    _LOG.Info("Load Manager [{0}] Susscess!", item.Name);
                }
                foreach (var item in ServiceTypes)
                {
                    _LOG.Info("Load ServiceType [{0}] Susscess!", item.Name);
                }
            }
            catch
            {
            }
        }
    }
}
