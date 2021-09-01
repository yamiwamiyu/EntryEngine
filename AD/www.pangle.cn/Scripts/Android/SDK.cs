//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
    using System;
    using UnityEngine;

#if !UNITY_EDITOR && UNITY_ANDROID
    /// <summary>
    /// The android bridge of the union SDK.
    /// </summary>
    public static class SDK
    {
        private static AndroidJavaObject activity;
        private static AndroidJavaObject adManager;

        /// <summary>
        /// Gets the version of this SDK.
        /// </summary>
        public static string Version
        {
            get
            {
                var adManager = GetAdManager();
                return adManager.Call<string>("getSDKVersion");
            }
        }

        /// <summary>
        /// Create the advertisement native object.
        /// </summary>
        public static AdNative CreateAdNative()
        {
            var adManager = GetAdManager();
            var context = GetActivity();
            var adNative = adManager.Call<AndroidJavaObject>(
                "createAdNative", context);
            return new AdNative(adNative);
        }

        /// <summary>
        /// Request permission if necessary on some device, for example ask
        /// for READ_PHONE_STATE.
        /// </summary>
        public static void RequestPermissionIfNecessary()
        {
            var adManager = GetAdManager();
            var context = GetActivity();
            adManager.Call("requestPermissionIfNecessary", context);
        }

        /// <summary>
        /// Try to show install dialog when exit the app.
        /// </summary>
        /// <returns>True means show dialog.</returns>
        public static bool TryShowInstallDialogWhenExit(Action onExitInstall)
        {
            var adManager = GetAdManager();
            var context = GetActivity();
            var listener = new ExitInstallListener(onExitInstall);
            return adManager.Call<bool>(
                "tryShowInstallDialogWhenExit", context, listener);
        }

        /// <summary>
        /// Gets the unity main activity.
        /// </summary>
        internal static AndroidJavaObject GetActivity()
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

        private static AndroidJavaObject GetAdManager()
        {
            if (adManager == null)
            {
                var jc = new AndroidJavaClass(
                    "com.bytedance.sdk.openadsdk.TTAdSdk");
                adManager = jc.CallStatic<AndroidJavaObject>("getAdManager");
            }

            return adManager;
        }

#pragma warning disable SA1300
#pragma warning disable IDE1006

        private sealed class ExitInstallListener : AndroidJavaProxy
        {
            private readonly Action callback;

            public ExitInstallListener(Action callback)
                : base("com.bytedance.sdk.openadsdk.downloadnew.core.ExitInstallListener")
            {
                this.callback = callback;
            }

            public void onExitInstall()
            {
                this.callback();
            }
        }

#pragma warning restore SA1300
#pragma warning restore IDE1006
    }
#endif
}
