using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LauncherProtocolStructure;
using EntryEngine;
using EntryEngine.Network;

namespace LauncherManager
{
    static class _DATA
    {
        private static S服务器管理 managed;

        internal static IEnumerable<Maintainer> Managed
        {
            get
            {
                if (managed == null)
                    managed = Entry.Instance.GetScene<S服务器管理>();
                return managed.Managed;
            }
        }
        internal static IEnumerable<Server> ManagedServers
        {
            get { return Managed.SelectMany(s => s.Servers); }
        }
        internal static IEnumerable<Service> ManagedServices
        {
            get { return Managed.SelectMany(s => s.Services); }
        }
    }
}
