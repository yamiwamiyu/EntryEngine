using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Networking;
#if WEBGL
using EntryEngine.Unity;
#endif

public class Main : MonoBehaviour
{
    internal static string PersistentDataPath;
    internal static string DataPath;
    internal const string VERSION = "__version.txt";
    internal const string FILE_LIST = "__filelist.txt";
    internal const string RUNTIME = "UnityRuntime.bytes";
    static string[] SPLIT = new string[] { "\r\n" };

    public Material GLMaterial;
    //http://47.99.145.87:8863/
    public string ServerUrl;
    string text = string.Empty;

    class Filelist
    {
        public string File;
        public string Time;
        public long Length;

        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}\r\n", File, Time, Length);
        }
    }
    class Loader
    {
        private byte[] _bytes;
        internal byte[] bytes
        {
            get
            {
                if (www == null)
                    return _bytes;
                else
                {
                    if (string.IsNullOrEmpty(www.error))
                        return www.downloadHandler.data;
                    else
                        return null;
                }
            }
        }
        internal string text
        {
            get
            {
                if (www == null)
                    return ReadString(_bytes);
                else
                    return www.downloadHandler.text;
            }
        }
        internal UnityWebRequest www;
        internal UnityWebRequestAsyncOperation Wait { get; private set; }
        public bool Load(string file)
        {
            _bytes = null;
            www = null;
            if (File.Exists(PersistentDataPath + file))
            {
                _bytes = File.ReadAllBytes(PersistentDataPath + file);
                return true;
            }
            else
            {
                string target = DataPath + file;
                if (File.Exists(target))
                {
                    _bytes = File.ReadAllBytes(target);
                    return true;
                }
                www = UnityWebRequest.Get(target);
                Wait = www.SendWebRequest();
                return www.isDone;
            }
        }
    }

    static string ReadString(byte[] bytes)
    {
        using (MemoryStream stream = new MemoryStream(bytes))
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
    static void WriteFile(string file, byte[] data)
    {
        file = PersistentDataPath + file;
        string dir = Path.GetDirectoryName(file);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllBytes(file, data);
    }
    // Use this for initialization
    void Start()
    {
        DataPath = Application.streamingAssetsPath + '/';
        if (Application.platform != RuntimePlatform.WindowsEditor)
        {
            PersistentDataPath = Application.persistentDataPath + '/';
            Environment.CurrentDirectory = PersistentDataPath;
        }
        else
            PersistentDataPath = DataPath;

        // 初始化游戏开始过程
#if WEBGL
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            var gate = gameObject.AddComponent<UnityGate>();
            gate.GLMaterial = this.GLMaterial;
            gate.Entry.ShowMainScene<MAIN>();
        }
        else
#endif
        {
            StartCoroutine(StartCoroutine());
        }
    }
    System.Collections.IEnumerator StartCoroutine()
    {
        Type entryType = Type.GetType("EntryEngine.Entry");
        if (entryType == null)
        {
            #region 热更新

            Loader loader = new Loader();
            //if (Application.platform != RuntimePlatform.WindowsEditor && !string.IsNullOrEmpty(ServerUrl))
            if (!string.IsNullOrEmpty(ServerUrl))
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                text += "\r\n正在检测版本更新";

                // 检查版本更新进行热更
                WaitForEndOfFrame wait = new WaitForEndOfFrame();
                float timeout = 0;
                UnityWebRequest www = UnityWebRequest.Get(ServerUrl + VERSION + "?" + DateTime.Now.Ticks);
                yield return www.SendWebRequest();
                byte[] versionBytes = null;
                while (true)
                {
                    if (www.isDone)
                    {
                        if (string.IsNullOrEmpty(www.error))
                        {
                            versionBytes = www.downloadHandler.data;
                        }
                        else
                        {
                            text += "166:" + www.error;
                        }
                        break;
                    }
                    else
                    {
                        timeout += Time.deltaTime;
                        if (timeout >= 2)
                        {
                            // 下载超时
                            text += "检测新版本号超时，请检查网络后再试";
                            break;
                        }
                        yield return wait;
                    }
                }

                if (versionBytes != null)
                {
                    byte[] newVersionBytes = null;
                    if (!loader.Load(VERSION))
                    {
                        text += "\r\n版本文件网络加载";
                        yield return loader.Wait;
                    }
                    byte[] oldVersionBytes = loader.bytes;
                    // 没有旧版本号，必定更新新版本
                    if (oldVersionBytes == null)
                        newVersionBytes = versionBytes;
                    else
                    {
                        // 新旧版本号不同则需要更新版本
                        for (int i = 0; i < 8; i++)
                        {
                            if (oldVersionBytes[i] != versionBytes[i])
                            {
                                // 需要更新版本
                                newVersionBytes = versionBytes;
                                break;
                            }
                        }
                    }

                    if (newVersionBytes != null)
                    {
                        text += "\r\n正在计算更新包大小";

                        www = UnityWebRequest.Get(ServerUrl + FILE_LIST + "?" + DateTime.Now.Ticks);
                        yield return www.SendWebRequest();

                        if (!string.IsNullOrEmpty(www.error))
                        {
                            text += "215:" + www.error;
                            yield break;
                        }

                        if (!loader.Load(FILE_LIST))
                        {
                            text += "\r\n文件列表网络加载";
                            yield return loader.Wait;
                        }
                        string[] oldList;
                        if (loader.bytes != null)
                            oldList = ReadString(loader.bytes).Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);
                        else
                            oldList = new string[0];
                        byte[] newFilelistBytes = www.downloadHandler.data;
                        string[] newList = ReadString(newFilelistBytes).Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);
                        // 用于记录已经更新的文件，中途关闭更新下次也能接着上次的更新
                        StringBuilder builder = new StringBuilder();

                        // 暂时不删除不需要了的文件，只列出需要下载的新文件或者需要更新的文件
                        Dictionary<string, Filelist> oldFilelist = new Dictionary<string, Filelist>();
                        foreach (var item in oldList)
                        {
                            string[] splits = item.Split('\t');
                            Filelist file = new Filelist();
                            file.File = splits[0];
                            file.Time = splits[1];
                            file.Length = long.Parse(splits[2]);
                            oldFilelist.Add(splits[0], file);
                        }

                        List<Filelist> newFilelist = new List<Filelist>();
                        foreach (var item in newList)
                        {
                            string[] splits = item.Split('\t');
                            Filelist file;
                            if (oldFilelist.TryGetValue(splits[0], out file))
                            {
                                // 时间不一致需要更新
                                if (file.Time != splits[1])
                                {
                                    file.Length = long.Parse(splits[2]);
                                    newFilelist.Add(file);
                                }
                                else
                                {
                                    // 旧文件不需要更新的写入builder
                                    builder.Append(item);
                                    builder.Append("\r\n");
                                }
                            }
                            else
                            {
                                // 需要下载的新文件
                                file = new Filelist();
                                file.File = splits[0];
                                file.Time = splits[1];
                                file.Length = long.Parse(splits[2]);
                                newFilelist.Add(file);
                            }
                        }

                        // 可能只是删除了文件，导致没有需要更新的文件
                        long needDownload = newFilelist.Sum(f => f.Length) >> 10;
                        if (newFilelist.Count > 0)
                        {
                            string file_list = PersistentDataPath + FILE_LIST;
                            long download = 0;
                            text += string.Format("正在更新：{0}kb", needDownload);
                            foreach (var item in newFilelist)
                            {
                                www = UnityWebRequest.Get(ServerUrl + item.File + "?" + item.Time);
                                if (!www.isDone)
                                    yield return www.SendWebRequest();

                                if (!string.IsNullOrEmpty(www.error))
                                {
                                    text += string.Format("289: file: {0} error: {1} len: {2} text:{3}", item.File, www.error, item.File.Length, string.Join(",", item.File.Select(c => ((int)c).ToString()).ToArray()));
                                    yield break;
                                }

                                item.File = item.File.Replace('\\', '/');
                                WriteFile(item.File, www.downloadHandler.data);
                                // todo:每完成一个下载都写入旧文件列表，这样中途退出下次也能接着上次中断的文件开始下载
                                builder.Append(item.ToString());
                                File.WriteAllText(file_list, builder.ToString());
                                download += item.Length >> 10;
                                text = string.Format("正在更新：{0}kb / {1}kb", download, needDownload);
                            }
                            WriteFile(FILE_LIST, newFilelistBytes);
                        }

                        WriteFile(VERSION, newVersionBytes);
                        text += string.Format("\r\n版本更新完成 更新文件：{0}个 更新大小：{1} kb 版本号：{2}", newFilelist.Count, needDownload, BitConverter.ToInt64(newVersionBytes, 0));
                    }// end of 需要更新新版本
                }// end of 新版本号不为null

                watch.Stop();
                text += string.Format("\r\n版本更新耗时：{0}秒", watch.Elapsed.TotalSeconds);
            }// end of 服务器地址不为null
            #endregion

            #region 动态加载运行时

            text += "\r\n正在准备运行环境";

            if (!loader.Load(RUNTIME))
            {
                text += "\r\n运行时网络加载";
                yield return loader.Wait;
            }
            if (loader.bytes == null)
            {
                text += "\r\n加载运行环境失败";
                yield break;
            }

            {
                // 加载EntryEngine
                Assembly assembly = AppDomain.CurrentDomain.Load(loader.bytes, null);
                // 调用dll自解析
                Type type = assembly.GetType("Program");
                MethodInfo method = type.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
                method.Invoke(null, new object[1] { null });
                Debug.Log("Load runtime completed.");
            }

            text += "\r\n正在准备引用动态库";

            if (!loader.Load(FILE_LIST))
            {
                text += "\r\n动态库文件列表网络加载";
                yield return loader.Wait;
            }
            if (loader.bytes == null)
            {
                text += "\r\n加载动态库失败";
                yield break;
            }
            {
                string[] list = ReadString(loader.bytes).Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < list.Length; i++)
                {
                    if (string.IsNullOrEmpty(list[i]))
                        continue;
                    string dllFile = list[i].Split('\t')[0];
                    if (!dllFile.EndsWith(".bytes"))
                        break;
                    // 不重复加载运行时
                    if (dllFile == RUNTIME)
                        continue;
                    if (!loader.Load(dllFile))
                    {
                        text += "\r\n动态库网络加载";
                        yield return loader.Wait;
                    }
                    AppDomain.CurrentDomain.Load(loader.bytes, null);
                    text += string.Format("\r\n加载{0}完成", dllFile);
                }
            }

            #endregion
        }

        #region 启动主程序

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        //foreach (var item in assemblies)
        //{
        //    string name = item.GetName().Name;
        //    if (name.StartsWith("UnityEngine")) continue;
        //    text += "\r\n已加载DLL:" + name;
        //}

        text += "\r\n添加入口脚本";
        // 创建Unity入口
        Assembly unity = assemblies.FirstOrDefault(a => a.GetName().Name == "Unity");
        Type gateType = unity.GetType("EntryEngine.Unity.UnityGate");
        var gate = gameObject.AddComponent(gateType);

        text += "\r\n设置着色器";
        // 设置同名属性
        //Debug.Log(gateType.GetProperty("Gate", BindingFlags.Public | BindingFlags.Static).GetValue(null, new object[0]));  // 测试是否已经执行了Start: 没有执行
        //var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        //for (int i = 0; i < fields.Length; i++)
        //    gateType.GetField(fields[i].Name, BindingFlags.Public | BindingFlags.Static).SetValue(null, fields[i].GetValue(this));
        gateType.GetField("GLMaterial", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).SetValue(gate, GLMaterial);

        text += "\r\n加载游戏入口动态库";
        // 加载游戏入口场景
        Assembly client = assemblies.FirstOrDefault(a => a.GetName().Name == "Client");
        if (client == null)
        {
            text += "\r\n未找到游戏动态库";
            yield break;
        }
        text += "\r\n加载游戏入口场景";
        // 使用入口展示场景实例 entry.ShowMainScene<T>();
        PropertyInfo entry = gateType.GetProperty("Entry");
        object __entry = entry.GetValue(gate, new object[0]);
        while (__entry == null)
        {
            // 等待帧结束，让入口Start调用完毕
            yield return new WaitForFixedUpdate();
            __entry = entry.GetValue(gate, new object[0]);
        }
        text += "\r\n显示主场景";
        entry.PropertyType.GetMethod("ShowMainScene", Type.EmptyTypes).
            MakeGenericMethod(client.GetType("MAIN")).
            Invoke(__entry, new object[0]);

        #endregion

        text += "\r\n游戏开始";
        // 移除加载程序
        DestroyImmediate(this);

        yield break;
    }
    void OnGUI()
    {
        if (Event.current.type == EventType.Repaint)
        {
            GUI.Label(new Rect(100, 200, Screen.width, Screen.height), text);
        }
    }
}