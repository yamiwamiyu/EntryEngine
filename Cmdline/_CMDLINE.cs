using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using EntryEngine.Serialize;
using System.Collections.Generic;

namespace EntryEngine.Cmdline
{
    public static class _CMDLINE
    {
        private delegate bool ControlCtrlDelegate(int type);
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate handle, bool Add);

        public static void ApplicationExit(Action<int> onExit)
        {
            SetConsoleCtrlHandler((type) =>
            {
                onExit(type);
                return false;
            }, true);
        }
        /// <summary>
        /// 运行命令行命令
        /// </summary>
        /// <param name="argument">命令</param>
        /// <returns>命令输出结果</returns>
        public static string RunCmd(string argument)
        {
            if (string.IsNullOrEmpty(argument))
                throw new ArgumentNullException("Argument can't be null.");

            string result;
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                //process.StartInfo.Arguments = arguments[0];
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                process.StandardInput.WriteLine(argument);
                process.StandardInput.WriteLine("exit");

                result = process.StandardOutput.ReadToEnd();
                int index = result.IndexOf(argument);
                if (index != -1)
                    result = result.Substring(index + argument.Length).Trim();
                index = result.LastIndexOf("\n");
                if (index != -1)
                    result = result.Substring(0, index).Trim();

                string error = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                    throw new Exception(error);
            }
            return result;
        }
        public static string RunCmds(params string[] arguments)
        {
            if (arguments.IsEmpty())
                throw new ArgumentNullException("Arguments can't be null.");

            string result;
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                for (int i = 0; i < arguments.Length; i++)
                    process.StandardInput.WriteLine(arguments[i]);
                process.StandardInput.WriteLine("exit");
                result = process.StandardOutput.ReadToEnd();

                string error = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                    throw new Exception(error);
            }
            return result;
        }
        /// <summary>
        /// 需要制作异步等待进程结束
        /// </summary>
        [Code(ECode.ToBeContinue)]
        public static string RunExe(string exe, string arguments)
        {
            string result;
            using (Process process = new Process())
            {
                process.StartInfo.FileName = exe;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                bool started = process.Start();

                Stopwatch watch = Stopwatch.StartNew();
                while (!process.HasExited)
                    process.WaitForExit(50);
                watch.Stop();
                _LOG.Warning("Wait check out time: {0}ms", watch.ElapsedMilliseconds);
                result = process.StandardOutput.ReadToEnd();

                string error = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                    throw new Exception(error);
            }
            return result;
        }
        public static void ParseCommandLine(string cmdline, out string command, out string[] arguments)
        {
            StringStreamReader reader = new StringStreamReader(cmdline);

            command = reader.Next(" ", true);
            if (string.IsNullOrEmpty(command))
            {
                command = cmdline;
                arguments = new string[0];
            }
            else
            {
                List<string> args = new List<string>();
                while (!reader.IsEnd)
                {
                    reader.EatWhitespace();
                    if (reader.PeekChar == '\"')
                    {
                        reader.Read();
                        args.Add(reader.Next("\""));
                        reader.Read();
                    }
                    else
                    {
                        args.Add(reader.Next(" "));
                    }
                }
                arguments = args.ToArray();
            }
        }
    }
    /// <summary>
    /// <para>SVN操作都可能抛出异常</para>
    /// </summary>
    public static class _SVN
    {
        private const string EXE = "SVN.exe";
        public static string UserName;
        public static string Password;

        public class InfoData
        {
            public string Path;
            public string URL;
            public string RelativeURL;
            public string RepositoryRoot;
            public string RepositoryUUID;
            public int Revision;
            public string NodeKind;
            public string LastChangedAuthor;
            public int LastChangedRev;
            public string LastChangedDate;
        }
        public class LogData
        {
            public int Revision;
            public string ChangedAuthor;
            public string ChangedDate;
        }

        private static string Run(string arg)
        {
            //return _CMDLINE.RunExe(EXE, arg + GetValidate());
            return _CMDLINE.RunCmd("svn " + arg + GetValidate());
        }
        private static string GetValidate()
        {
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
                return null;
            else
                return string.Format(" --username {0} --password {1}", UserName, Password);
        }
        private static string ReadInfo(ref string text, string key)
        {
            int index = text.IndexOf(key);
            if (index == -1)
                throw new ArgumentOutOfRangeException(key);
            text = text.Substring(index + key.Length - 1);
            index = text.IndexOf(FONT.LINE_BREAK);
            if (index != -1)
                return text.Substring(0, index).Trim();
            else
                return text;
        }
        public static InfoData Info(string url)
        {
            // svn info [url]

            /* Path: EntryEngine
             * URL: https://127.0.0.1/svn/EntryEngine
             * Relative URL: ^/
             * Repository Root: https://127.0.0.1/svn/EntryEngine
             * Repository UUID: 85c15c05-c1d8-c84a-be71-faae794dbbb9
             * Revision: 31
             * Node Kind: directory
             * Last Changed Author: yamiwamiyu
             * Last Changed Rev: 31
             * Last Changed Date: 2016-07-14 18:06:33 +0800 (周四, 14 七月 2016)
             * string result = RunCmd("svn info " + url);
             */
            string result = Run("info " + url);

            InfoData info = new InfoData();

            info.Path = ReadInfo(ref result, "Path: ");
            info.URL = ReadInfo(ref result, "URL: ");
            info.RelativeURL = ReadInfo(ref result, "Relative URL: ");
            info.RepositoryRoot = ReadInfo(ref result, "Repository Root: ");
            info.RepositoryUUID = ReadInfo(ref result, "Repository UUID: ");
            info.Revision = int.Parse(ReadInfo(ref result, "Revision: "));
            info.NodeKind = ReadInfo(ref result, "Node Kind: ");
            info.LastChangedAuthor = ReadInfo(ref result, "Last Changed Author: ");
            info.LastChangedRev = int.Parse(ReadInfo(ref result, "Last Changed Rev: "));
            info.LastChangedDate = ReadInfo(ref result, "Last Changed Date: ");

            return info;
        }
        public static LogData Log(string url)
        {
            // svn log [url] -r HEAD -q
            // -r HEAD: 最新版本
            // -q: 不显示更改的文件

            /* ------------------------------------------------------------------------
             * r2 | yamiwamiyu | 2016-07-15 12:01:49 +0800 (周五, 15 七月 2016)
             * ------------------------------------------------------------------------
             */
            string result = Run(string.Format("log {0} -r HEAD -q", url));

            LogData data = new LogData();

            int index = result.IndexOf('r');
            if (index == -1)
                return null;
            result = result.Substring(index + 1);
            index = result.IndexOf(' ');
            data.Revision = int.Parse(result.Substring(0, index));

            result = result.Substring(index + 3);
            index = result.IndexOf(' ');
            data.ChangedAuthor = result.Substring(0, index);

            result = result.Substring(index + 3);
            index = result.IndexOf('\n');
            data.ChangedDate = result.Substring(0, index).Trim();

            return data;
        }
        public static int Checkout(string url, string path)
        {
            // svn checkout [url] -q

            /* Checked out revision 0. */

            string result = Run(string.Format("checkout {0} {1}", url, path));
            int index = result.LastIndexOf("Checked out revision");
            string revision = result.Substring(index + 21);
            return int.Parse(revision.Substring(0, revision.IndexOf('.')));
        }
        public static int Update(string path)
        {
            // svn update [path]
            // Status: Added Deleted Updated Conflict Merged Existed Replaced

            /* Updating 'path':
             * Status    FileOrDirectory
             * At revision 3.           //已经在当前版本
             * Updated to revision 3.   //更新至当前版本
             */

            string result = Run(string.Format("update {0}", path));
            int index = result.LastIndexOf("revision");
            string revision = result.Substring(index + 9);
            return int.Parse(revision.Substring(0, revision.IndexOf('.')));
        }
    }
}
