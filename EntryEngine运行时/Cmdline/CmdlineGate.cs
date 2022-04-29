using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using EntryEngine.Serialize;
using System.Text;

namespace EntryEngine.Cmdline
{
    public class CmdlineGate : IDisposable
    {
        class Command
        {
            public object Instance;
            public MethodInfo[] Methods;
        }

        private TimeSpan frameRate = TimeSpan.FromSeconds(1.0 / 60);
        private byte overheatFrameCount = 10;
        /// <summary>程序Update用时超过FrameRate连续超过OverheatFrameCount时触发</summary>
        public event Action<TimeSpan[]> Overheat;
        /// <summary>每帧循环时触发，返回是否继续程序</summary>
        public event Func<bool> Loop;
        /// <summary>循环发生异常时触发，返回是否继续程序</summary>
        public event Func<Exception, bool> Error;
        private List<Command> commands = new List<Command>();

        private byte overheat;
        private TimeSpan[] overheatTimes;
        private TimeSpan surplus;
        private Thread cmdListener;

        public TimeSpan FrameRate
        {
            get { return frameRate; }
            set
            {
                if (value.Ticks < 0)
                    throw new ArgumentOutOfRangeException("FPS must be positive.");
                frameRate = value;
            }
        }
        public byte OverheatFrameCount
        {
            get { return overheatFrameCount; }
            set
            {
                overheatFrameCount = value;
                if (Running)
                    overheatTimes = new TimeSpan[overheatFrameCount];
            }
        }
        public EntryService Entry
        {
            get;
            private set;
        }
        public bool Running
        {
            get { return Entry != null; }
        }
        public IEnumerable<MethodInfo> Commands
        {
            get { return commands.SelectMany(c => c.Methods); }
        }

        public void AppendCommand(object instance, Type cmdInterface)
        {
            if (!cmdInterface.IsInterface)
                throw new ArgumentException("Listen cmd type must be a interface.");

            if (instance == null)
                throw new ArgumentNullException("Listen cmd interface must has a implement instance that is not be null.");

            AppendCommand(instance, cmdInterface.GetMethods().Where(m => !m.IsSpecialName || m.Name.StartsWith("op_")));
        }
        public void AppendCommand(object instance, params MethodInfo[] commands)
        {
            AppendCommand(instance, (IEnumerable<MethodInfo>)commands);
        }
        public void AppendCommand(object instance, IEnumerable<MethodInfo> commands)
        {
            Command cmd = new Command();
            cmd.Instance = instance;
            cmd.Methods = commands.ToArray();

            if (cmd.Methods.Length == 0)
                throw new ArgumentException("Commands must have one command at least.");

            this.commands.ForEach(c =>
            {
                foreach (var item in cmd.Methods)
                {
                    if (c.Methods.Any(m => m.Name == item.Name))
                    {
                        throw new ArgumentException("Duplicate method {0}.", item.Name);
                    }
                }
            });

            this.commands.Add(cmd);
        }
        public void ClearCommand()
        {
            this.commands.Clear();
        }
        private void ListenCommand()
        {
            string cmdline;
            while (Running)
            {
                cmdline = Console.ReadLine();

                if (!Running)
                    break;

                ExecuteCommand(cmdline);
            }
        }
        public void ExecuteImmediately(string cmdline)
        {
            try
            {
                string command;
                string[] arguments;
                _CMDLINE.ParseCommandLine(cmdline, out command, out arguments);

                object instance = null;
                MethodInfo method = null;

                commands.FirstOrDefault(c =>
                {
                    var temp = c.Methods.FirstOrDefault(m => m.Name == command);
                    if (temp != null)
                    {
                        method = temp;
                        instance = c.Instance;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });

                //var method = commands.FirstOrDefault(c => c.Name == command);
                if (method != null)
                {
                    var parameters = method.GetParameters();
                    int paramCount = parameters.Length;
                    if (arguments.Length < paramCount)
                    {
                        _LOG.Warning("Arguments count dismatch of command {0}.", command);
                        return;
                    }

                    object[] args = new object[paramCount];
                    for (int i = 0; i < paramCount; i++)
                        args[i] = Convert.ChangeType(arguments[i], parameters[i].ParameterType);

                    method.Invoke(instance, args);
                }
                else
                {
                    _LOG.Warning("Not find command '{0}'.", command);
                }
            }
            catch (Exception ex)
            {
                _LOG.Error(ex, "Cmd [{0}] invoke error!", cmdline);
            }
        }
        public COROUTINE ExecuteCommand(string cmdline)
        {
            if (string.IsNullOrEmpty(cmdline))
                return null;

            if (Running)
            {
                return Entry.Synchronize(() => ExecuteImmediately(cmdline));
            }
            else
            {
                ExecuteImmediately(cmdline);
                return null;
            }
        }
        public void RunCommandListener()
        {
            if (cmdListener != null)
                throw new InvalidOperationException("Cmdline listener has been launched.");

            if (!Running)
                throw new InvalidOperationException("Console has not been launched.");

            try
            {
                cmdListener = new Thread(ListenCommand);
                cmdListener.Name = "Command Listener";
                cmdListener.IsBackground = true;
                cmdListener.Start();
            }
            catch (Exception ex)
            {
                _LOG.Error(ex, "Command listener launch error!");
                StopCmdlineListener();
                throw ex;
            }
        }
        public void Run(EntryService entry)
        {
            //Console.InputEncoding = Encoding.UTF8;
            //Console.OutputEncoding = Encoding.UTF8;
            //if (!(_LOG._Logger is LoggerToShell))
            //    _LOG._Logger = new LoggerToShell();

            if (entry == null)
                throw new ArgumentNullException("entry");

            if (Running)
                throw new InvalidOperationException("Program has been running.");

            this.Entry = entry;

            RunCommandListener();

            overheat = 0;
            overheatTimes = new TimeSpan[overheatFrameCount];

            while (Loop == null || Loop())
            {
                try
                {
                    entry.Update();
                }
                catch (Exception ex)
                {
                    _LOG.Error(ex, "Entry update error!");
                    if (Error != null && !Error(ex))
                    {
                        _LOG.Info("Fatal.");
                        break;
                    }
                }

                if (!Running)
                    break;

                surplus = frameRate - entry.GameTime.UpdateElapsedTime;
                if (surplus.Ticks > 0)
                {
                    overheat = 0;
                    Thread.Sleep(surplus);
                }
                else
                {
                    if (overheatFrameCount > 0)
                    {
                        overheatTimes[overheat] = -surplus;
                        overheat++;
                    }

                    if (overheat >= overheatFrameCount)
                    {
                        if (Overheat != null)
                            try
                            {
                                Overheat(overheatTimes);
                            }
                            catch (Exception ex)
                            {
                                _LOG.Error(ex, "Overheat Error");
                            }
                        overheat = 0;
                        Thread.Sleep(frameRate);
                    }
                }
            }
        }
        public void StopCmdlineListener()
        {
            if (cmdListener != null)
            {
                cmdListener.Abort();
                cmdListener = null;
            }
        }
        public void Stop()
        {
            overheat = 0;
            overheatTimes = null;
            if (Entry != null)
                Entry.Dispose();
            Entry = null;
            StopCmdlineListener();
        }
        void IDisposable.Dispose()
        {
            Stop();
        }
    }
}