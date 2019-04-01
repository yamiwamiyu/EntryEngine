using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net;

public class Main : MonoBehaviour
{
    internal static string PersistentDataPath;
    internal static string DataPath;
    internal const string VERSION = "__version.txt";
    internal const string FILE_LIST = "__filelist.txt";
    internal const string RUNTIME = "UnityRuntime.bytes";
    static string[] SPLIT = new string[] { "\r\n" };

    public Material GLMaterial;
    public string ServerUrl;
    string text;

    class Filelist
    {
        public string File;
        public string Time;
        public long Length;
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
                    return www.bytes;
            }
        }
        internal string text
        {
            get { return ReadString(this.bytes); }
        }
        internal WWW www;
        public bool Load(string file)
        {
            _bytes = null;
            www = null;
            if (File.Exists(file))
            {
                _bytes = File.ReadAllBytes(file);
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
                www = new WWW(target);
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
    // Use this for initialization
    void Start()
    {
        if (Application.platform != RuntimePlatform.WindowsEditor)
        {
            PersistentDataPath = Application.persistentDataPath + '/';
            Environment.CurrentDirectory = PersistentDataPath;
        }
        DataPath = Application.streamingAssetsPath + '/';

        // 初始化游戏开始过程
        StartCoroutine(StartCoroutine());
    }
    IEnumerator StartCoroutine()
    {
        Debug.Log(Environment.CurrentDirectory);
        Type entryType = Type.GetType("EntryEngine.Entry");
        if (entryType == null)
        {
            Loader loader = new Loader();
            if (Application.platform != RuntimePlatform.WindowsEditor && !string.IsNullOrEmpty(ServerUrl))
            //if (!string.IsNullOrEmpty(ServerUrl))
            {
                text = "正在检测版本更新";

                // 检查版本更新进行热更
                WaitForEndOfFrame wait = new WaitForEndOfFrame();
                float timeout = 0;
                WWW www = new WWW(ServerUrl + VERSION);
                byte[] versionBytes = null;
                while (true)
                {
                    if (www.isDone)
                    {
                        if (string.IsNullOrEmpty(www.error))
                        {
                            versionBytes = www.bytes;
                        }
                        else
                        {
                            text = "107:" + www.error;
                        }
                        break;
                    }
                    else
                    {
                        timeout += Time.deltaTime;
                        if (timeout >= 2)
                        {
                            // 下载超时
                            text = "下载超时，请检查网络后再试";
                            break;
                        }
                        yield return wait;
                    }
                }
                //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ServerUrl + VERSION);
                //request.Timeout = 5000;
                //request.ContentType = "application/x-www-form-urlencoded";
                //bool isDone = false;
                //request.BeginGetResponse((ar) =>
                //{
                //    try
                //    {
                //        var response = request.EndGetResponse(ar);
                //        versionBytes = new byte[8];
                //        response.GetResponseStream().Read(versionBytes, 0, 8);
                //        response.Close();
                //    }
                //    catch (Exception ex)
                //    {
                //        text = ex.Message;
                //    }
                //    isDone = true;
                //}, request);
                
                if (versionBytes != null)
                {
                    byte[] newVersionBytes = null;
                    if (!loader.Load(VERSION))
                        yield return loader.www;
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            if (loader.bytes[i] != versionBytes[i])
                            {
                                // 需要更新版本
                                newVersionBytes = versionBytes;
                                break;
                            }
                        }
                    }

                    if (newVersionBytes != null)
                    {
                        text = "正在计算更新包大小";

                        www = new WWW(ServerUrl + FILE_LIST);
                        yield return www;

                        if (!string.IsNullOrEmpty(www.error))
                        {
                            text = "169:" + www.error;
                            yield break;
                        }

                        if (!loader.Load(FILE_LIST))
                            yield return loader.www;
                        string[] oldList = ReadString(loader.bytes).Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);
                        byte[] newFilelistBytes = www.bytes;
                        string[] newList = ReadString(newFilelistBytes).Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);

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
                        if (newFilelist.Count > 0)
                        {
                            long needDownload = newFilelist.Sum(f => f.Length) >> 10;
                            long download = 0;
                            text = string.Format("正在更新：{0}kb", needDownload);
                            foreach (var item in newFilelist)
                            {
                                www = new WWW(ServerUrl + item.File);
                                yield return www;

                                if (!string.IsNullOrEmpty(www.error))
                                {
                                    text = string.Format("229: file: {0} error: {1} len: {2} text:{3}", item.File, www.error, item.File.Length, string.Join(",", item.File.Select(c => ((int)c).ToString()).ToArray()));
                                    yield break;
                                }

                                item.File = item.File.Replace('\\', '/');
                                string dir = Path.GetDirectoryName(item.File);
                                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                                    Directory.CreateDirectory(dir);
                                File.WriteAllBytes(item.File, www.bytes);
                                // todo:每完成一个下载都写入旧文件列表，这样中途退出下次也能接着上次中断的文件开始下载
                                download += item.Length >> 10;
                                text = string.Format("正在更新：{0}kb / {1}kb", download, needDownload);
                            }
                            File.WriteAllBytes(FILE_LIST, newFilelistBytes);
                        }

                        File.WriteAllBytes(VERSION, newVersionBytes);
                        Debug.Log("版本更新完成 {0}" + BitConverter.ToInt64(newVersionBytes, 0));
                    }
                }// end of if (www.error)
            }

            text = "正在准备运行环境";

            if (!loader.Load(RUNTIME))
                yield return loader.www;
            {
                // 加载EntryEngine
                Assembly assembly = AppDomain.CurrentDomain.Load(loader.bytes, null);
                // 调用dll自解析
                Type type = assembly.GetType("Program");
                MethodInfo method = type.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
                method.Invoke(null, new object[1] { null });
                Debug.Log("Load runtime completed.");
            }

            if (!loader.Load(FILE_LIST))
                yield return loader.www;
            {
                string[] list = Encoding.UTF8.GetString(loader.bytes).Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);
                // 从1开始，第0个元素是UnityRuntime.bytes
                for (int i = 1; i < list.Length; i++)
                {
                    string dllFile = list[i].Split('\t')[0];
                    if (!dllFile.EndsWith(".bytes"))
                        break;
                    if (!loader.Load(dllFile))
                        yield return loader.www;
                    AppDomain.CurrentDomain.Load(loader.bytes, null);
                    Debug.Log(string.Format("加载{0}完成", dllFile));
                }
            }
        }

        text = "程序即将启动";

        // 创建Unity入口
        Assembly unity = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Unity");
        Type gateType = unity.GetType("EntryEngine.Unity.UnityGate");
        var gate = gameObject.AddComponent(gateType);

        // 设置同名属性
        //Debug.Log(gateType.GetProperty("Gate", BindingFlags.Public | BindingFlags.Static).GetValue(null, new object[0]));  // 测试是否已经执行了Start: 没有执行
        //var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        //for (int i = 0; i < fields.Length; i++)
        //    gateType.GetField(fields[i].Name, BindingFlags.Public | BindingFlags.Static).SetValue(null, fields[i].GetValue(this));
        gateType.GetField("GLMaterial", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).SetValue(gate, GLMaterial);

        // 等待帧结束，让入口Start调用完毕
        yield return new WaitForEndOfFrame();

        // 加载游戏入口场景
        Assembly client = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Client");
        // 使用入口展示场景实例 entry.ShowMainScene<T>();
        PropertyInfo entry = gateType.GetProperty("Entry");
        entry.PropertyType.GetMethod("ShowMainScene", Type.EmptyTypes).
            MakeGenericMethod(client.GetType("MAIN")).
            Invoke(entry.GetValue(gate, new object[0]), new object[0]);

        // 移除加载程序
        DestroyImmediate(this);
    }
    void OnGUI()
    {
        if (Event.current.type == EventType.Repaint)
        {
            GUI.Label(new Rect(30, 50, Screen.width, Screen.height), text);
        }
    }
}