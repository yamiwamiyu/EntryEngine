namespace ByteDance.Union
{
#if !UNITY_EDITOR && UNITY_ANDROID
    using UnityEngine;

    public class Pangle : PangleBase
    {
        internal static int NETWORK_STATE_MOBILE = 1;
        internal static int NETWORK_STATE_2G = 2;
        internal static int NETWORK_STATE_3G = 3;
        internal static int NETWORK_STATE_WIFI = 4;
        internal static int NETWORK_STATE_4G = 5;
        private static AndroidJavaObject activity;

        public static void InitializeSDK(PangleInitializeCallBack callback,CustomConfiguration configuration = null)
        {
            Debug.Log("Pangle InitializeSDK start" );
            var activity = GetActivity();
            var runnable = new AndroidJavaRunnable(
                () => initTTSdk(callback, configuration));
            activity.Call("runOnUiThread", runnable);
        }

        private static void initTTSdk(PangleInitializeCallBack callback,CustomConfiguration configuration)
        {
            Debug.Log("Pangle initTTSdk " );
            var sdkInitCallback = new SdkInitCallback(callback);
            AndroidJavaObject adConfigBuilder = new AndroidJavaObject("com.bytedance.sdk.openadsdk.TTAdConfig$Builder");
            Debug.Log("Pangle InitializeSDK 开始设置config");
            adConfigBuilder.Call<AndroidJavaObject>("appId", AD.AD.AppID)
                .Call<AndroidJavaObject>("useTextureView", true) //使用TextureView控件播放视频,默认为SurfaceView,当有SurfaceView冲突的场景，可以使用TextureView
                .Call<AndroidJavaObject>("appName", AD.AD.AppName)
                .Call<AndroidJavaObject>("allowShowNotify", true) //是否允许sdk展示通知栏提示
                //.Call<AndroidJavaObject>("debug", true) //测试阶段打开，可以通过日志排查问题，上线时去除该调用
                .Call<AndroidJavaObject>("directDownloadNetworkType",
                    new int[] {NETWORK_STATE_WIFI, NETWORK_STATE_3G}) //允许直接下载的网络状态集合
                .Call<AndroidJavaObject>("themeStatus", 0)//设置主题类型，0：正常模式；1：夜间模式；默认为0；传非法值，按照0处理
                .Call<AndroidJavaObject>("supportMultiProcess", true) //是否支持多进程
                .Call<AndroidJavaObject>("data",
                    "[{\"name\":\"unity_version\",\"value\":\"" + PangleBase.PangleSdkVersion + "\"}]"); //传递unity版本号
            if (configuration != null)
            {
                adConfigBuilder.Call<AndroidJavaObject>("customController", MakeCustomController(configuration));
            }
            AndroidJavaObject adConfig = adConfigBuilder.Call<AndroidJavaObject>("build");
            var jc = new AndroidJavaClass(
                "com.bytedance.sdk.openadsdk.TTAdSdk");
            jc.CallStatic("init", GetActivity(), adConfig, sdkInitCallback);
        }
        /// <summary>
        /// Gets the unity main activity.
        /// </summary>
        private static AndroidJavaObject GetActivity()
        {
            if (activity == null)
            {
                var unityPlayer = new AndroidJavaClass(
                    "com.unity3d.player.UnityPlayer");
                activity = unityPlayer.GetStatic<AndroidJavaObject>(
                    "currentActivity");
            }
            return activity;
        }
        
        private sealed class SdkInitCallback : AndroidJavaProxy
        {
            private readonly PangleInitializeCallBack listener;

            public SdkInitCallback(
                PangleInitializeCallBack listener)
                : base("com.bytedance.sdk.openadsdk.TTAdSdk$InitCallback")
            {
                this.listener = listener;
            }

            public void fail(int code, string message)
            {
                listener(false, message);
            }

            public void success()
            {
                listener(true, "sdk 初始化成功");
            }
        }


        private static AndroidJavaObject MakeCustomController(CustomConfiguration controller)
        {
            
            var customController = new AndroidJavaObject("com.bytedance.android.CustomController");
            if (controller == null) return customController;
            customController.Call("canUseWifiState", controller.CanUseWifiState);
            customController.Call("setMacAddress", controller.MacAddress);
            customController.Call("canUseLocation", controller.CanUseLocation);
            if (!double.IsNaN(controller.Latitude) || !double.IsNaN(controller.Longitude))
            {
                customController.Call("setLocationInfo", controller.Longitude, controller.Latitude);
            }
            customController.Call("canReadAppList", controller.CanReadAppList);
            customController.Call("canUsePhoneState", controller.CanUsePhoneState);
            customController.Call("setDevImei", controller.DevImei);
            customController.Call("canUseWriteExternal", controller.CanUseWriteExternal);
            customController.Call("setDevOaid", controller.DevOaid);
            return customController;
        }
    }
#endif
}