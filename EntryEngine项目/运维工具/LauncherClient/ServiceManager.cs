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

        public static bool Copy(Service service)
        {
            return Copy(new DirectoryInfo(service.Type), new DirectoryInfo(service.Directory), null);
        }
        public static bool Copy(Service service, Func<FileInfo, bool> skip)
        {
            return Copy(new DirectoryInfo(service.Type), new DirectoryInfo(service.Directory), skip);
        }
        /// <summary>拷贝文件价内的所有文件到目标文件夹内</summary>
        /// <param name="directory">要拷贝的文件夹</param>
        /// <param name="target">目标文件夹</param>
        /// <param name="skip">设置需要忽略的文件，null则不忽略任何文件，委托返回true则忽略文件</param>
        /// <returns>是否全部正常拷贝完成</returns>
        private static bool Copy(DirectoryInfo directory, DirectoryInfo target, Func<FileInfo, bool> skip)
        {
            bool all = true;

            if (!directory.Exists)
                directory.Create();
            if (!target.Exists)
                target.Create();

            var files = directory.GetFiles();
            for (int i = 0; i < files.Length; i++)
                if ((files[i].Attributes & FileAttributes.Archive) == FileAttributes.Archive
                    && (skip == null || !skip(files[i])))
                    try
                    {
                        files[i].CopyTo(Path.Combine(target.FullName, Path.GetFileName(files[i].Name)), true);
                    }
                    catch
                    {
                        all = false;
                        _LOG.Warning("跳过更新{0}", files[i].Name);
                    }

            var directories = directory.GetDirectories();
            for (int i = 0; i < directories.Length; i++)
            {
                if (directories[i].Attributes != FileAttributes.Directory)
                    continue;
                DirectoryInfo _target = new DirectoryInfo(Path.Combine(target.FullName, directories[i].Name));
                all &= Copy(directories[i], _target, skip);
            }

            return all;
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

            Service service = new Service();
            service.Name = name;
            service.Type = serviceType.Name;

            // 新建服务直接从svn checkout下来
            Directory.CreateDirectory(service.Directory);
            service.Revision = _SVN.Checkout(serviceType.SVNPath, service.Directory);
            service.RevisionOnServer = service.Revision;

            service.Exe = serviceType.Exe;
            service.LaunchCommand = serviceType.LaunchCommand;

            _SAVE.Services.Add(service);
            _SAVE.Save();

            _LOG.Info("新建服务[{0}]完成", name);
            callback.Callback(service);
        }
        void _IManagerCall.Delete(string name, CBIManagerCall_Delete callback)
        {
            _LOG.Info("删除服务：{0}", name);

            var launcher = Launcher.Find(name);
            if (launcher != null)
                launcher.Dispose();

            var service = _SAVE.Services.FirstOrDefault(s => s.Name == name);
            _SAVE.Services.Remove(service);
            _SAVE.Save();

            // 删除服务类型
            foreach (var file in Directory.GetFiles(service.Directory, "*.svn-base", SearchOption.AllDirectories))
                // svn的只读文件会导致删除不了文件夹
                File.SetAttributes(file, FileAttributes.Normal);
            Directory.Delete(service.Directory, true);
            // 该服务类型的所有服务都被删除了，服务类型的文件夹也一并删除
            if (!_SAVE.Services.Any(s => s.Type == service.Type))
            {
                Directory.Delete("__" + service.Type, true);
                _LOG.Info("删除服务类型：{0}", service.Type);
            }
            
            callback.Callback();
        }
        void _IManagerCall.Launch(string name, CBIManagerCall_Launch callback)
        {
            "服务已启动".Check(Launcher.Find(name) != null);
            var service = _SAVE.Services.FirstOrDefault(s => s.Name == name);
            "启动服务不存在".Check(service == null);
            "启动服务没有可执行文件".Check(string.IsNullOrEmpty(service.Exe));
            Launch(service);

            callback.Callback();
        }
        private void Launch(Service service)
        {
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
                                Proxy.LogServer(service.Name, record);
                                launcher.Dispose();
                                SetStatus(service, EServiceStatus.Stop);
                            }
                            else
                            {
                                Proxy.Log(service.Name, record);
                            }
                        }
                        else if (record.Level == 255)
                        {
                            SetStatus(service, EServiceStatus.Running);
                        }
                        else if (record.Level == 254)
                        {
                            if (!string.IsNullOrEmpty(record.Content))
                                service.Commands = JsonReader.Deserialize<List<string>>(record.Content);
                        }
                        else
                            Proxy.LogServer(service.Name, record);
                    }
                    try
                    {
                        return !(service.Status != EServiceStatus.Stop && launcher.Running);
                    }
                    catch (Exception ex)
                    {
                        _LOG.Error(ex, "日志打印协程异常");
                        return true;
                    }
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
            _LOG.Info("启动服务：[{0}]", service.Name);
        }
        void _IManagerCall.Update(string name, CBIManagerCall_Update callback)
        {
            var service = _SAVE.Services.FirstOrDefault(s => s.Name == name);

            bool restart = false;
            // 若是热更且更新到运行程序会导致需要cleanup，否则全部更新失败
            // 所以先看看有没有更新到运行程序，更新到的话就自动重启程序
            if (!string.IsNullOrEmpty(service.Exe))
            {
                var launcher = Launcher.Find(name);
                if (launcher != null && launcher.Running)
                {
                    var status = _SVN.Status(Path.Combine(service.Directory, service.Exe));
                    // 运行程序有更新，关掉后更新再重启
                    if (status.Updates.Count > 0)
                    {
                        launcher.Dispose();
                        SetStatus(service, EServiceStatus.Stop);
                        restart = true;
                    }
                }
            }
            
            service.Revision = _SVN.Update(service.Directory);
            service.RevisionOnServer = service.Revision;
            _SAVE.Save();

            if (restart)
                Launch(service);
            
            callback.Callback(service.Revision);
        }
        void _IManagerCall.Stop(string name, CBIManagerCall_Stop callback)
        {
            var service = _SAVE.Services.FirstOrDefault(s => s.Name == name);

            var launcher = Launcher.Find(name);
            if (launcher != null)
                launcher.Dispose();

            SetStatus(service, EServiceStatus.Stop);
            _LOG.Info("关闭服务：[{0}]", name);
            callback.Callback();
        }
        void _IManagerCall.GetCommands(string name, CBIManagerCall_GetCommands callback)
        {
            var service = _SAVE.Services.FirstOrDefault(s => s.Name == name);

            if (string.IsNullOrEmpty(service.Exe) || service.Commands == null)
                callback.Callback(new List<string>());
            else
                callback.Callback(service.Commands);
            //{
            //    List<string> results = new List<string>();
            //    try
            //    {
            //        var assembly = System.Reflection.Assembly.Load(service.Directory + service.Exe);
            //        _LOG.Info("Load Assembly: {0}", assembly.FullName);
            //        foreach (var method in
            //            assembly.GetExportedTypes()
            //            .Where(i => i.GetCustomAttributes(typeof(CMDAttribute), true).Length > 0)
            //            .SelectMany(i => i.GetMethods()))
            //        {
            //            StringBuilder builder = new StringBuilder();
            //            builder.Append(method.Name);
            //            foreach (var param in method.GetParameters())
            //            {
            //                builder.Append(' ');
            //                builder.Append(param.Name);
            //            }
            //            results.Add(builder.ToString());
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        _LOG.Error(ex, "读取[CMD]命令错误");
            //    }
            //    callback.Callback(results);
            //}
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
            //bool flag = false;
            //foreach (var item in _SAVE.ServiceTypes)
            //{
            //    _LOG.Info("准备开始更新{0}", item.Type);
            //    int revision = _SVN.Update(item.Type);
            //    _LOG.Info("版本号: {0} 旧版本号:{1}", revision, item.Revision);
            //    if (revision != item.Revision)
            //    {
            //        item.Revision = revision;
            //        _LOG.Info("更新[{0}]版本至{1}", item.Type, item.Revision);
            //        Proxy.RevisionUpdate(item);
            //        flag = true;
            //    }
            //}
            //if (flag)
            //    _SAVE.Save();
            throw new NotImplementedException();
        }
        void _IManagerCall.ServiceTypeUpdate(ServiceType type)
        {
            //bool flag = false;
            //foreach (var item in _SAVE.Services)
            //{
            //    // todo: svn信息修改
            //    if (item.Type == type.Name && item.Exe != type.Exe)
            //    {
            //        _LOG.Debug("服务[{0}]运行程序修改 {1} -> {2}", item.Name, item.Exe, type.Exe);
            //        item.Exe = type.Exe;
            //        flag = true;
            //    }
            //}
            //if (flag)
            //{
            //    _SAVE.Save();
            //}
            throw new NotImplementedException();
        }
        void _IManagerCall.SetLaunchCommand(string name, string exe, string command)
        {
            var service = _SAVE.Services.FirstOrDefault(s => s.Name == name);
            service.Exe = exe;
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
            var process = System.Diagnostics.Process.GetCurrentProcess();
            builder.AppendLine("taskkill /PID {0}", process.Id);
            builder.AppendLine("svn update {0}", Environment.CurrentDirectory);
            builder.AppendLine("start {0}.exe", process.ProcessName);
            builder.AppendLine("del {0}", UPDATE);
            File.WriteAllText(UPDATE, builder.ToString());
            System.Diagnostics.Process.Start(UPDATE);
        }
    }
}
