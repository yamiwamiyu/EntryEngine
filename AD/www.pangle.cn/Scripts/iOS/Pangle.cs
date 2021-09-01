namespace ByteDance.Union
{
    #if !UNITY_EDITOR && UNITY_IOS
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class Pangle : PangleBase
    {
        private static PangleInitializeCallBack callbackio;
        
        public static void InitializeSDK(PangleInitializeCallBack callback)
        {
            callbackio = callback;
            UnionPlatform_PangleInitializeSDK(PangleInitializeCallBackCC);
        }

        [DllImport("__Internal")]
        private static extern float UnionPlatform_PangleInitializeSDK(PangleInitializeCallBack callback);

        [AOT.MonoPInvokeCallback(typeof(PangleInitializeCallBack))]
        private static void PangleInitializeCallBackCC(bool success, string message)
        {
            callbackio(success,message);
        }
    }
    #endif
}