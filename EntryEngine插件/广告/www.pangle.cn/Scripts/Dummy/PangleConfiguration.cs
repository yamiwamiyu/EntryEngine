 namespace ByteDance.Union
{
    #if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IOS)
    public class PangleConfiguration
    {
        public enum BUAdSDKTerritory {
            BUAdSDKTerritory_CN = 1,
            BUAdSDKTerritory_NO_CN,
        }
        public enum BUAdSDKLogLevel {
            BUAdSDKLogLevelNone,
            BUAdSDKLogLevelError,
            BUAdSDKLogLevelWarning,
            BUAdSDKLogLevelInfo,
            BUAdSDKLogLevelDebug,
            BUAdSDKLogLevelVerbose,
        };
        public enum BUOfflineType {
            BUOfflineTypeNone,  // Do not set offline
            BUOfflineTypeWebview, // Offline dependence WKWebview
        };
        /// This property should be set when integrating non-China and china areas at the same time,
        /// otherwise it need'nt to be set.you‘d better set Territory first,  if you need to set them
        public BUAdSDKTerritory territory;
        ///Register the App key that’s already been applied before requesting an ad from TikTok Audience Network.
        /// the unique identifier of the App
        public string appID;
        /// Configure development mode. default BUAdSDKLogLevelNone
        public BUAdSDKLogLevel logLevel;
        /// the COPPA of the user, COPPA is the short of Children's Online Privacy Protection Rule,
        /// the interface only works in the United States.
        /// Coppa 0 adult, 1 child
        /// You can change its value at any time
        public int coppa;
        /// additional user information.
        public string userExtData;
        /// Solve the problem when your WKWebview post message empty,
        /// default is BUOfflineTypeWebview
        public BUOfflineType webViewOfflineType;
        /// Custom set the GDPR of the user,GDPR is the short of General Data Protection Regulation,the interface only works in The European.
        /// GDPR 0 close privacy protection, 1 open privacy protection
        /// You can change its value at any time
        public int GDPR;
        /// Custom set the CCPA of the user,CCPA is the short of General Data Protection Regulation,the interface only works in USA.
        /// CCPA  0: "sale" of personal information is permitted, 1: user has opted out of "sale" of personal information -1: default
        public int CCPA;
        /// <summary>
        /// 主题类型，0：正常模式；1：夜间模式；默认为0；传非法值，按照0处理
        /// </summary>
        public int themeStatus; 
        /// Custom set the tob ab sdk version of the user.
        public string abSDKVersion;
        /// Custom set idfa value
        /// You can change its value at any time
        public string customIdfa;
        /**
        Whether to allow SDK to modify the category and options of AVAudioSession when playing audio, default is NO.
        The category set by the SDK is AVAudioSessionCategoryAmbient, and the options are AVAudioSessionCategoryOptionDuckOthers
        */
        public bool allowModifyAudioSessionSetting;
        /// Custom set the AB vid of the user. Array element type is NSNumber
        private static int[] abvids;
        public void setAbvids(int[] abv,int length) {
            abvids = abv;
        }
        public int[] getAbvids() {
            return abvids;
        }

        private static PangleConfiguration _Singleton = null;
        private static object Singleton_Lock = new object();
        public static PangleConfiguration CreateInstance()
        {
            if (_Singleton == null) //双if +lock
            {
                lock (Singleton_Lock)
                {
                    if (_Singleton == null)
                    {
                        _Singleton = new PangleConfiguration();
                    }
                }
            }
            return _Singleton;
        }
    }
    #endif
}