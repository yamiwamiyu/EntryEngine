//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
    #if !UNITY_ANDROID
    using UnityEditor;
    using UnityEngine;
    using System.Text;
    using UnityEditor.Callbacks;
    using UnityEditor.iOS.Xcode;

    /// <summary>
    /// The post processor for xcode.
    /// </summary>
    internal static class XCodePostProcess
    {
        [PostProcessBuild(700)]
        public static void OnPostProcessBuild(
            BuildTarget target, string pathToBuiltProject)
        {

            if (target != BuildTarget.iOS)
            {
                return;
            }
    
            var projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            var proj = new PBXProject();
            proj.ReadFromFile(projPath);

            var targetGUID = @"";
#if (UNITY_2019_3_OR_NEWER)
            // 2019.3以上的新接口
            targetGUID = proj.GetUnityFrameworkTargetGuid();
#else
            // 旧有接口
            targetGUID = proj.TargetGuidByName("Unity-iPhone");
#endif

            proj.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-ObjC");
            proj.AddFrameworkToProject(targetGUID, "libresolv.9.tbd", false);
            proj.AddFrameworkToProject(targetGUID, "libc++.tbd", false);
            proj.AddFrameworkToProject(targetGUID, "libz.tbd", false);
            proj.AddFrameworkToProject(targetGUID, "CoreLocation.framework", false);
            proj.AddFrameworkToProject(targetGUID, "AVKit.framework", false);
            proj.AddFrameworkToProject(targetGUID, "ImageIO.framework", false);
            proj.AddFrameworkToProject(targetGUID, "MediaPlayer.framework", false);
            proj.AddFrameworkToProject(targetGUID, "StoreKit.framework", false);
            proj.AddFrameworkToProject(targetGUID, "SystemConfiguration.framework", false);
            proj.AddFrameworkToProject(targetGUID, "AdSupport.framework", false);
            proj.AddFrameworkToProject(targetGUID, "CoreMotion.framework", false);
            proj.AddFrameworkToProject(targetGUID, "Photos.framework", false);
            proj.AddFrameworkToProject(targetGUID, "WebKit.framework", false);
            proj.AddFrameworkToProject(targetGUID, "CoreTelephony.framework", false);
            proj.AddFrameworkToProject(targetGUID, "CoreServices.framework", false);
            proj.AddFrameworkToProject(targetGUID, "Security.framework", false);
            proj.AddFrameworkToProject(targetGUID, "Accelerate.framework", false);
            proj.AddFrameworkToProject(targetGUID, "libsqlite3.tbd", false);
            proj.AddFrameworkToProject(targetGUID, "libbz2.tbd", false);
            proj.AddFrameworkToProject(targetGUID, "libxml2.tbd", false);
            proj.AddFrameworkToProject(targetGUID, "UIKit.framework", false);
            proj.AddFrameworkToProject(targetGUID, "MapKit.framework", false);
            proj.AddFrameworkToProject(targetGUID, "CoreMedia.framework", false);
            proj.AddFrameworkToProject(targetGUID, "AVFoundation.framework", false);
            proj.AddFrameworkToProject(targetGUID, "MobileCoreServices.framework", false);
            proj.AddFrameworkToProject(targetGUID, "CoreMotion.framework", false);
            proj.AddFrameworkToProject(targetGUID, "AudioToolbox.framework", false);
            proj.AddFrameworkToProject(targetGUID, "JavaScriptCore.framework", false);
            proj.AddFrameworkToProject(targetGUID, "CoreImage.framework", false);
            proj.AddFrameworkToProject(targetGUID, "QuartzCore.framework", false);
            proj.AddFrameworkToProject(targetGUID, "CoreGraphics.framework", false);
            proj.AddFrameworkToProject(targetGUID, "CoreText.framework", false);
            proj.AddFrameworkToProject(targetGUID, "libresolv.9.tbd", false);
            proj.AddFrameworkToProject(targetGUID, "libiconv.tbd", false);
            proj.AddFrameworkToProject(targetGUID, "libc++abi.tbd", false);
            proj.AddFrameworkToProject(targetGUID, "AppTrackingTransparency.framework", weak:false);
            proj.WriteToFile(projPath); 
        }
    }
    #endif
}
