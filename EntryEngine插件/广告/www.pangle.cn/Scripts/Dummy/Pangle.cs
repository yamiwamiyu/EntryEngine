namespace ByteDance.Union
{
    #if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IOS)
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using UnityEngine;
    public abstract class Pangle : PangleBase
    {
        public static void InitializeSDK(PangleInitializeCallBack callback,CustomConfiguration configuration=null) {

        }
        
        public static void setCoppa(int coppa)
        {
        }

        public static int getCoppa()
        {
            return 0;
        }
    }
    #endif
}