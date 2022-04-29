namespace ByteDance.Union
{
#if !UNITY_EDITOR && UNITY_IOS
    using System.Runtime.InteropServices;
    /// <summary>
    /// The feed Ad.
    /// </summary>
    public class PangleTools
    {
        /// <summary>
        /// getScreenScale
        /// </summary>
        static public float getScreenScale()
        {
            return UnionPlatform_PangleTools_GetScreenScale();;
        }
        /// <summary>
        /// getScreenWidth
        /// </summary>
        static public float getScreenWidth()
        {
            return UnionPlatform_PangleTools_GetScreenWidth();
        }
        /// <summary>
        /// getScreenWidth
        /// </summary>
        static public float getScreenHeight()
        {
            return UnionPlatform_PangleTools_GetScreenHeight();
        }
        /// <summary>
        /// getWindowSafeAreaInsetsTop
        /// </summary>
        static public float getWindowSafeAreaInsetsTop()
        {
            return UnionPlatform_PangleTools_SafeAreaInsets_Top();
        }
        /// <summary>
        /// getWindowSafeAreaInsetsLeft
        /// </summary>
        static public float getWindowSafeAreaInsetsLeft()
        {
            return UnionPlatform_PangleTools_SafeAreaInsets_Left();
        }
        /// <summary>
        /// getWindowSafeAreaInsetsBottom
        /// </summary>
        static public float getWindowSafeAreaInsetsBottom()
        {
            return UnionPlatform_PangleTools_SafeAreaInsets_Bottom();
        }
        /// <summary>
        /// getWindowSafeAreaInsetsRight
        /// </summary>
        static public float getWindowSafeAreaInsetsRight()
        {
            return UnionPlatform_PangleTools_SafeAreaInsets_Right();
        }

        [DllImport("__Internal")]
        private static extern float UnionPlatform_PangleTools_GetScreenScale();

        [DllImport("__Internal")]
        private static extern float UnionPlatform_PangleTools_GetScreenWidth();

        [DllImport("__Internal")]
        private static extern float UnionPlatform_PangleTools_GetScreenHeight();

        [DllImport("__Internal")]
        private static extern float UnionPlatform_PangleTools_SafeAreaInsets_Top();

        [DllImport("__Internal")]
        private static extern float UnionPlatform_PangleTools_SafeAreaInsets_Left();

        [DllImport("__Internal")]
        private static extern float UnionPlatform_PangleTools_SafeAreaInsets_Bottom();

        [DllImport("__Internal")]
        private static extern float UnionPlatform_PangleTools_SafeAreaInsets_Right();
    }
#endif
}
