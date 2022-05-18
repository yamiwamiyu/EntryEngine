using System;
using EntryEngine.Network;
using LauncherProtocolStructure;
using System.Collections.Generic;

namespace LauncherProtocol
{
    [ProtocolStub(1, null)]
    public interface IManagerCall
    {
        void New(ServiceType serviceType, string name, Action<Service> callback);
        void Delete(string name, Action callback);
        void Launch(string name, Action callback);
        void Update(string name, Action<int> callback);
        void Stop(string name, Action callback);
        void GetCommands(string name, Action<List<string>> callback);
        void CallCommand(string name, string command);
        void UpdateSVN();
        void ServiceTypeUpdate(ServiceType type);
        void SetLaunchCommand(string name, string exe, string command);
        void UpdateLauncher();
    }
}
