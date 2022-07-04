using System;

namespace ByteDance.Union
{
    public class CustomConfiguration
    {
        /// <summary>
        ///是否允许SDK主动使用ACCESS_WIFI_STATE权限
        /// </summary>
        /// <returns> true可以使用，false禁止使用。默认为true</returns>
        public bool CanUseWifiState { get; set; }

        /// <summary>
        ///  是否允许SDK主动使用地理位置信息 true可以获取，false禁止获取。默认为true
        /// </summary>
        public bool CanUseLocation { get; set; }

        /// <summary>
        /// 是否允许sdk上报手机app安装列表 true可以上报、false禁止上报。默认为true
        /// </summary>
        public bool CanReadAppList { get; set; }

        /// <summary>
        /// 是否允许SDK主动使用手机硬件参数，如：imei true可以使用，false禁止使用。默认为true
        /// </summary>
        public bool CanUsePhoneState { get; set; }

        /// <summary>
        /// 是否允许SDK主动使用ACCESS_WIFI_STATE权限  true可以使用，false禁止使用。默认为true
        /// </summary>
        public bool CanUseWriteExternal { get; set; }

        /// <summary>
        /// 当isCanUseWifiState=false时，可传入Mac地址信息，穿山甲sdk使用您传入的Mac地址信息
        /// </summary>
        /// <returns>Mac地址信息</returns>
        public string MacAddress { get; set; }

        //纬度
        public double Latitude { get; set; }

        //经度
        public double Longitude { get; set; }
        
        public string DevImei { get; set; }
        public string DevOaid { get; set; }

        public CustomConfiguration()
        {
            Latitude = double.NaN;
            Longitude = double.NaN;
            CanUseWifiState = true;
            CanUseLocation = true;
            CanReadAppList = true;
            CanUsePhoneState = true;
            CanUseWriteExternal = true;
        }
    }
}