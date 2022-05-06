using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Security.Cryptography;
using EntryEngine;
using System.Threading;
#if !DEBUG
using UnityEngine;
#endif

/// <summary>分析统计SDK A(Analysis) S(Statistic)</summary>
public static class _ASSDK
{
    /// <summary>接口协议</summary>
    private static IUserProxy proxy = new IUserProxy();

    static _ASSDK()
    {
        proxy.IsAsync = false;
#if DEBUG
        proxy.Host = "http://127.0.0.1:888/Action/";
#else
        proxy.Host = "http://47.99.145.87:8865/Action/";
#endif
    }

#if !DEBUG
    private static AndroidJavaObject context;
    private static AndroidJavaObject GetContext()
    {
        if (context == null)
        {
            AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            context = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
            _LOG.Debug("构建Context{0}", context == null ? "失败" : "成功");
        }
        return context;
    }
    private class ApkFile
    {
        public const int CHANNEL_ID = 0x012FCF81;
        static readonly byte[] MAGIC_DIR_END = { 0x50, 0x4b, 0x05, 0x06 };

        public byte[] Bytes;
        public Encoding Encoding = Encoding.UTF8;

        public string Comment
        {
            get
            {
                int offset, length;
                if (CheckComment(out offset, out length))
                    return Encoding.GetString(Bytes, offset, length);
                else
                    return null;
            }
        }
        public bool HasV2
        {
            get
            {
                int offset;
                return CheckHasV2(out offset);
            }
        }

        public ApkFile(byte[] bytes)
        {
            this.Bytes = bytes;
        }
        public ApkFile(byte[] bytes, Encoding encoding)
        {
            this.Bytes = bytes;
            this.Encoding = encoding;
        }

        public byte[] ReadData(int id)
        {
            int offset;
            if (!CheckHasV2(out offset))
                return null;

            int blockEnd = offset - 24;
            int blockSize = (int)BitConverter.ToInt64(Bytes, blockEnd);
            int index = offset - blockSize;
            while (index < blockEnd)
            {
                long size = BitConverter.ToInt64(Bytes, index);
                int readID = BitConverter.ToInt32(Bytes, index + 8);
                if (readID == id)
                {
                    byte[] data = new byte[size - 4];
                    Array.Copy(Bytes, index + 12, data, 0, size - 4);
                    return data;
                }
                else
                    index += 8 + (int)size;
            }
            return null;
        }
        private bool CheckComment(out int commentOffset, out int commentLength)
        {
            for (int i = Bytes.Length - 22; i >= 0; i--)
            {
                if (Bytes[i] == MAGIC_DIR_END[0] &&
                    Bytes[i + 1] == MAGIC_DIR_END[1] &&
                    Bytes[i + 2] == MAGIC_DIR_END[2] &&
                    Bytes[i + 3] == MAGIC_DIR_END[3])
                {
                    commentOffset = i + 22;
                    commentLength = Bytes[i + 20] + (Bytes[i + 21] << 8);
                    return true;
                }
            }
            commentOffset = -1;
            commentLength = 0;
            return false;
        }
        public bool CheckHasV2(out int offset)
        {
            offset = BitConverter.ToInt32(Bytes, Bytes.Length - 6);
            if (offset < 0 || offset >= Bytes.Length)
                return false;
            return Encoding.ASCII.GetString(Bytes, offset - 16, 16) == "APK Sig Block 42";
        }
    }
#endif
    private static string deviceID;
    /// <summary>获取设备号，PC端为CPU序列号</summary>
    private static string GetDeviceID()
    {
        if (deviceID == null)
        {
#if !DEBUG
            // JAVA源码 内部类使用'$'调用
            // DeviceID = android.provider.Settings.Secure.getString(Context.getContentResolver(), Settings.Secure.ANDROID_ID);
            var secure = new AndroidJavaClass("android.provider.Settings$Secure");
            deviceID = secure
                .CallStatic<string>("getString",
                    GetContext().Call<AndroidJavaObject>("getContentResolver"),
                    secure.GetStatic<string>("ANDROID_ID"));
#else
            // CPUID，同一品牌的同一批CPUID相同
            ManagementObjectCollection.ManagementObjectEnumerator moe = new ManagementClass("Win32_Processor").GetInstances().GetEnumerator();
            if (moe.MoveNext())
                deviceID = moe.Current.Properties["ProcessorId"].Value.ToString();

            // 硬盘序列号，可能读不到
            moe = new ManagementClass("Win32_PhysicalMedia").GetInstances().GetEnumerator();
            if (moe.MoveNext())
            {
                var pro = moe.Current.Properties["SerialNumber"];
                if (pro != null && pro.Value != null)
                    deviceID += pro.Value.ToString();
            }

            // 网卡MAC地址
            moe = new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances().GetEnumerator();
            while (moe.MoveNext())
                if ((bool)moe.Current.Properties["IPEnabled"].Value)
                    deviceID += moe.Current.Properties["MacAddress"].Value.ToString();

            // 把各设备号一起进行sha-256加密
            byte[] bytes = Encoding.UTF8.GetBytes(deviceID);
            byte[] hash = SHA256.Create().ComputeHash(bytes);

            StringBuilder sha256 = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sha256.Append(hash[i].ToString("x2"));
            deviceID = sha256.ToString().ToUpper();
#endif
            if (deviceID == null)
                deviceID = string.Empty;
            _LOG.Debug("构建设备号ID:{0}", deviceID);
        }
        return deviceID;
    }
    private static string channel;
    /// <summary>获取注释（渠道）</summary>
    private static string GetComment()
    {
        if (channel == null)
        {
#if !DEBUG
            byte[] buffer = System.IO.File.ReadAllBytes(GetContext().Call<string>("getPackageCodePath"));
            ApkFile apk = new ApkFile(buffer);
            int offset;
            if (apk.CheckHasV2(out offset))
            {
                byte[] data = apk.ReadData(ApkFile.CHANNEL_ID);
                if (data != null)
                    channel = apk.Encoding.GetString(data);
                else
                    _LOG.Debug("未找到渠道号");
            }
            else
            {
                channel = apk.Comment;
            }
#else
            channel = "PC";
#endif
            if (channel == null)
                channel = string.Empty;
            _LOG.Debug("构建渠道号：{0}", channel);
        }
        return channel;
    }

    /// <summary>本设备，本游戏的ID，为0代表尚未成功调用_SDK.Login接口</summary>
    public static int ID
    {
        get;
        private set;
    }

    /// <summary>是否已调用过登录</summary>
    private static bool logged = false;
    /// <summary>检查是否调用过登录</summary>
    private static void CheckInitialize()
    {
        if (!logged) throw new InvalidOperationException("需要先调用登录接口");
    }

    /// <summary>初始化SDK</summary>
    /// <param name="gameName">当前游戏的名字</param>
    /// <param name="channel">游戏渠道号，若传空，则SDK自动读取安装包的渠道号</param>
    /// <param name="onCallback">初始化成功返回的回调，回调将在主线程上触发</param>
    public static void Initialize(string gameName, string channel, Action onCallback)
    {
        if (logged) throw new InvalidOperationException("已调用过登录接口");
        if (string.IsNullOrEmpty(gameName)) throw new ArgumentException("游戏名不能为空");
        logged = true;
        if (string.IsNullOrEmpty(GetDeviceID()))
        {
            _LOG.Warning("设备号不能为空，不再统计此用户行为");
            return;
        }
        if (string.IsNullOrEmpty(channel))
            // 主线程上初始化渠道号
            GetComment();
        else
            _ASSDK.channel = channel;

        // 上报数据给服务器的队列
        Queue<Func<AsyncData<string>>> sendQueue = new Queue<Func<AsyncData<string>>>();

        sendQueue.Enqueue(() => proxy.Login(gameName, GetDeviceID(), GetComment(), (id) =>
        {
            ID = id;
            if (onCallback != null)
                onCallback();
        }));

        // 开启一个异步线程用于将数据上报给服务器
        new Thread(() =>
        {
            try
            {
                DateTime onlineTime = DateTime.Now;
                while (true)
                {
                    if (ID != 0)
                    {
                        // 定时在线心跳
                        if ((DateTime.Now - onlineTime).TotalSeconds >= 60)
                        {
                            sendQueue.Enqueue(() => proxy.Online(ID, null));
                            onlineTime = DateTime.Now;
                        }

                        // 统计数据
                        lock (analysis)
                        {
                            if (analysis.Count > 0)
                            {
                                List<T_Analysis> copy = new List<T_Analysis>(analysis);
                                sendQueue.Enqueue(() => proxy.Analysis(ID, copy, null));
                                analysis.Clear();
                            }
                        }
                    }

                    // 没有要发送数据时等待
                    if (sendQueue.Count == 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    // 队列一个个发送请求
                    Func<AsyncData<string>> func = sendQueue.Dequeue();

                    // 发送要给请求直到该请求成功为止
                    while (true)
                    {
                        // 本次请求发送的时间
                        DateTime requestTime = DateTime.Now;

                        AsyncData<string> invokeResult = func();
                        // 等待请求结束
                        while (!invokeResult.IsEnd)
                        {
                            Thread.Sleep(10);
                        }
                        if (invokeResult.IsSuccess) break;

                        // 若请求发送失败，则3秒后重发直到成功为止
                        int time = (int)(DateTime.Now - requestTime).TotalMilliseconds;
                        if (time < 3000) Thread.Sleep(3000 - time);
                    }
                }
            }
            catch (Exception e)
            {
                _LOG.Error(e, "SDK线程错误");
            }
        })
        {
            IsBackground = true,
        }.Start();
    }

    private static List<T_Analysis> analysis = new List<T_Analysis>();
    /// <summary>添加分析</summary>
    /// <param name="title">分析标题，相同标题不同值会在一起进行分析，例如升级</param>
    /// <param name="value">分析值，相同标题不同值会在一起进行分析，例如1级，2级</param>
    /// <param name="orderID">分析排序，传0则按照value值升序，否则按照orderID升序</param>
    /// <param name="canRepeat">分析能否重复，重复时一般用于统计数量，不重复时一般用于统计进度</param>
    public static void Analysis(string title, string value, int orderID = 0, bool canRepeat = false)
    {
        CheckInitialize();
        // 如果设备号为空，就不再统计行为
        if (string.IsNullOrEmpty(deviceID)) return;
        lock (analysis)
        {
            analysis.Add(new T_Analysis()
            {
                Label = title,
                Name = value,
                OrderID = orderID,
                Count = canRepeat ? 1 : 0,
            });
        }
    }
}
class T_Analysis
{
    /// <summary>事件页签</summary>
    public string Label;
    /// <summary>事件名称</summary>
    public string Name;
    /// <summary>事件排序，页签内排序</summary>
    public int OrderID;
    /// <summary>事件发生的次数，按人数统计时传0，按次数统计时传1</summary>
    public int Count;
}