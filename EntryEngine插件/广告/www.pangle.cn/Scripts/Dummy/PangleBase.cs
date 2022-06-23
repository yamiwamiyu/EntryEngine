using System;
using ByteDance.Union;

public abstract class PangleBase
{
    
    /// <summary>
    /// The version for the Pangle Unity SDK, which includes specific versions of the Pangle Android and iOS SDKs.
    /// <para>
    /// Please see <a href="https://github.com/bytedance/Bytedance-UnionAD">our GitHub repository</a> for details.
    /// </para>
    /// </summary>
    public const string PangleSdkVersion = "4.5.0.0";

    public delegate void PangleInitializeCallBack(bool success, string message);
    public delegate void InitializeSDK(PangleInitializeCallBack callback);

}
