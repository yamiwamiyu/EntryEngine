using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EntryEngine;
using EntryEngine.Cmdline;
using EntryEngine.Network;
using EntryEngine.Serialize;
using LauncherProtocolStructure;

namespace LauncherClient
{
    partial class ServiceLauncher : _IManagerCall
    {
        public const string DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";

        public static string NowString
        {
            get { return DateTime.Now.ToString(DATE_FORMAT); }
        }

        public static void Copy(Service service)
        {
            Copy(new DirectoryInfo(service.Type), new DirectoryInfo(service.Directory), null);
        }
        public static void Copy(Service service, Func<FileInfo, bool> skip)
        {
            Copy(new DirectoryInfo(service.Type), new DirectoryInfo(service.Directory), skip);
        }
        private static void Copy(DirectoryInfo directory, DirectoryInfo target, Func<FileInfo, bool> skip)
        {
            if (!directory.Exists)
                directory.Create();
            if (!target.Exists)
                target.Create();

            var files = directory.GetFiles();
            for (int i = 0; i < files.Length; i++)
                if ((files[i].Attributes & FileAttributes.Archive) == FileAttributes.Archive
                    && (skip == null || !skip(files[i])))
                    files[i].CopyTo(Path.Combine(target.FullName, Path.GetFileName(files[i].Name)), true);

            var directories = directory.GetDirectories();
            for (int i = 0; i < directories.Length; i++)
            {
                if (directories[i].Attributes != FileAttributes.Directory)
                    continue;
                DirectoryInfo _target = new DirectoryInfo(Path.Combine(target.FullName, directories[i].Name));
                Copy(directories[i], _target, skip);
            }
        }
        private static bool HotUpdateSkip(FileInfo file)
        {
            var extension = file.Extension;
            return extension == ".exe"
                || extension == ".dll"
                || extension == ".pdb";
        }


        void SetStatus(Service service, EServiceStatus status)
        {
            service.Status = status;
            service.LastStatusTime = NowString;
            Proxy.StatusUpdate(service.Name, service.Status, service.LastStatusTime);
        }
        IEnumerable<ICoroutine> WriteLog(string name, Record record, Service service, Launcher launcher)
        {
            if (record.Level <= (byte)ELog.Debug)
            {
                if (service.Status == EServiceStatus.Starting &&
                    (record.Level == 2 || record.Level == 3))
                {
                    _LOG.Info("服务[{0}]启动时发生异常即将自动关闭", service.Name);
                    Proxy.LogServer(name, record);
                    launcher.Dispose();
                    SetStatus(service, EServiceStatus.Stop);
                }
                else
                {
                    Proxy.Log(name, record);
                }
            }
            else if (record.Level == 255)
                SetStatus(service, EServiceStatus.Running);
            else
                Proxy.LogServer(name, record);
            yield break;
        }

        void _IManagerCall.New(ServiceType serviceType, string name, CBIManagerCall_New callback)
        {
            _SVN.UserName = serviceType.SVNUser;
            _SVN.Password = serviceType.SVNPassword;

            ServiceTypeRevision srt = _SAVE.ServiceTypes.FirstOrDefault(s => s.Type == serviceType.Name);
            // 新服务类型的首个服务先checkout服务文件
            if (srt == null)
            {
                Directory.CreateDirectory(serviceType.Name);
                srt = new ServiceTypeRevision();
                srt.Type = serviceType.Name;
                srt.Revision = _SVN.Checkout(serviceType.SVNPath, serviceType.Name);
                _SAVE.ServiceTypes.Add(srt);
            }

            Service service = new Service();
            service.Name = name;
            service.Type = serviceType.Name;
            service.Revision = srt.Revision;
            service.Exe = serviceType.Exe;
            service.LaunchCommand = serviceType.LaunchCommand;
            service.RevisionOnServer = srt.Revision;

            Directory.CreateDirectory(service.Directory);
            Copy(service);

            _SAVE.Services.Add(service);
            _SAVE.Save();

            _LOG.Info("新建服务[{0}]完成", name);
            callback.Callback(service);
            Proxy.RevisionUpdate(srt);
        }
        void _IManagerCall.Delete(string name, CBIManagerCall_Delete callback)
        {
            var launcher = Launcher.Find(name);
            if (launcher != null)
                launcher.Dispose();

            var service = _SAVE.Services.FirstOrDefault(s => s.Name == name);
            _SAVE.Services.Remove(service);

            string type = service.Type;
            Directory.Delete(service.Directory, true);
            _LOG.Info("删除服务：{0}", service.Name);
            if (!_SAVE.Services.Any(s => s.Type == type))
            {
                int index = _SAVE.ServiceTypes.IndexOf(s => s.Type == type);
                if (index != -1)
                    _SAVE.ServiceTypes.RemoveAt(index);
                // 删除服务类型
                foreach (var file in Directory.GetFiles(type, "*.svn-base", SearchOption.AllDirectories))
                {
                    // svn的只读文件会导致删除不了文件夹
                    File.SetAttributes(file, FileAttributes.Normal);
                }
                Directory.Delete(type, true);
                Directory.Delete("__" + type, true);
                _LOG.Info("删除服务类型：{0}", type);
            }
            _SAVE.Save();
            callback.Callback();
        }
        void _IManagerCall.Launch(string name)
        {
            if (Launcher.Find(name) != null)
            {
                _LOG.Warning("服务[{0}]已经启动", name);
                return;
            }

            var service = _SAVE.Services.FirstOrDefault(s => s.Name == name);
            
            LauncherCmdline launcher = new LauncherCmdline();

            // 使用顺序协程打印日志队列
            Queue<Record> records = new Queue<Record>();
            CorDelegate logCoroutine = new CorDelegate(
                (time) =>
                {
                    while (records.Count > 0)
                    {
                        var record = records.Dequeue();
                        if (record.Level <= (byte)ELog.Debug)
                        {
                            if (service.Status == EServiceStatus.Starting &&
                                //(record.Level == 2 || record.Level == 3))
                                (record.Level == 3))
                            {
                                _LOG.Info("服务[{0}]启动时发生异常即将自动关闭", service.Name);
                                Proxy.LogServer(name, record);
                                launcher.Dispose();
                                SetStatus(service, EServiceStatus.Stop);
                            }
                            else
                            {
                                Proxy.Log(name, record);
                            }
                        }
                        else if (record.Level == 255)
                            SetStatus(service, EServiceStatus.Running);
                        else
                            Proxy.LogServer(name, record);
                    }
                    return !(service.Status != EServiceStatus.Stop && launcher.Running);
                });
            SetCoroutine(logCoroutine);

            launcher.Name = service.Name;
            launcher.OnLogRecord += (record) =>
            {
                if (Proxy == null)
                    return;
                lock (records)
                    records.Enqueue(record);
            };
            SetStatus(service, EServiceStatus.Starting);
            launcher.Launch(Path.Combine(service.Directory, service.Exe), null, service.Directory, service.LaunchCommand);
            _LOG.Info("启动服务：[{0}]", name);
        }
        void _IManagerCall.Update(string name, CBIManagerCall_Update callback)
        {
            var service = _SAVE.Services.FirstOrDefault(s => s.Name == name);
            int revision = _SAVE.ServiceTypes.FirstOrDefault(s => s.Type == service.Type).Revision;

            var launcher = Launcher.Find(name);
            if (launcher != null && launcher.Running)
            {
                // 热更
                Copy(service, HotUpdateSkip);
                service.Revision = -revision;
                _LOG.Info("热更服务[{0}]完成", name);
            }
            else
            {
                // 冷更
                Copy(service);
                service.Revision = revision;
                _LOG.Info("冷更服务[{0}]完成", name);
            }
            callback.Callback(service.Revision);
            _SAVE.Save();
        }
        void _IManagerCall.Stop(string name)
        {
            var service = _SAVE.Services.FirstOrDefault(s => s.Name == name);

            var launcher = Launcher.Find(name);
            if (launcher != null)
                launcher.Dispose();

            SetStatus(service, EServiceStatus.Stop);
            _LOG.Info("关闭服务：[{0}]", name);
        }
        void _IManagerCall.CallCommand(string name, string command)
        {
            var launcher = Launcher.Find(name);
            if (launcher == null)
                return;
            launcher.ExecuteCommand(command);
            _LOG.Info("服务[{0}]执行命令[{1}]", name, command);
        }
        void _IManagerCall.UpdateSVN()
        {
            bool flag = false;
            foreach (var item in _SAVE.ServiceTypes)
            {
                _LOG.Info("准备开始更新{0}", item.Type);
                int revision = _SVN.Update(item.Type);
                _LOG.Info("版本号: {0} 旧版本号:{1}", revision, item.Revision);
                if (revision != item.Revision)
                {
                    item.Revision = revision;
                    _LOG.Info("更新[{0}]版本至{1}", item.Type, item.Revision);
                    Proxy.RevisionUpdate(item);
                    flag = true;
                }
            }
            if (flag)
                _SAVE.Save();
        }
        void _IManagerCall.ServiceTypeUpdate(ServiceType type)
        {
            bool flag = false;
            foreach (var item in _SAVE.Services)
            {
                // todo: svn信息修改
                if (item.Type == type.Name && item.Exe != type.Exe)
                {
                    _LOG.Debug("服务[{0}]运行程序修改 {1} -> {2}", item.Name, item.Exe, type.Exe);
                    item.Exe = type.Exe;
                    flag = true;
                }
            }
            if (flag)
            {
                _SAVE.Save();
            }
        }
        void _IManagerCall.SetLaunchCommand(string name, string command)
        {
            var service = _SAVE.Services.FirstOrDefault(s => s.Name == name);
            service.LaunchCommand = command;
            _SAVE.Save();
            _LOG.Info("设置服务[{0}]启动命令完成", name);
        }
        void _IManagerCall.UpdateLauncher()
        {
            foreach (var item in Launcher.Launchers)
                item.Dispose();

            string UPDATE = "update.bat";
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("taskkill /PID {0}", System.Diagnostics.Process.GetCurrentProcess().Id);
            builder.AppendLine("svn update {0}", Environment.CurrentDirectory);
            builder.AppendLine("start EntryEngine.exe");
            builder.AppendLine("del {0}", UPDATE);
            File.WriteAllText(UPDATE, builder.ToString());
            System.Diagnostics.Process.Start(UPDATE);
        }
    }
}
