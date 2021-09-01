//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

#import <BUAdSDK/BUAdSDK.h>

#if defined (__cplusplus)
extern "C" {
#endif

    void UnionPlatform_setTerritory(int territory) {
        [BUAdSDKConfiguration configuration].territory = (BUAdSDKTerritory)territory;
    }
    
    BUAdSDKTerritory UnionPlatform_territory() {
        return [BUAdSDKConfiguration configuration].territory;
    }
    
    void UnionPlatform_setAppID(const char* appID) {
        NSString *ocString = [[NSString alloc] initWithUTF8String:appID?:""];
        if (ocString.length) {
            [BUAdSDKConfiguration configuration].appID = ocString;
        }
    }
    
    const char* UnionPlatform_appID() {
        return (char*)[[BUAdSDKConfiguration configuration].appID?:@"" cStringUsingEncoding:NSUTF8StringEncoding];
    }
    
    void UnionPlatform_setLogLevel(int logLevel) {
        [BUAdSDKConfiguration configuration].logLevel = (BUAdSDKLogLevel)logLevel;
    }
    
    BUAdSDKLogLevel UnionPlatform_logLevel() {
        return [BUAdSDKConfiguration configuration].logLevel;
    }
    
    void UnionPlatform_setCoppa(int coppa) {
        [BUAdSDKConfiguration configuration].coppa = @(coppa);
    }
    
    int UnionPlatform_coppa() {
        return [BUAdSDKConfiguration configuration].coppa.intValue;
    }
    
    void UnionPlatform_setUserExtData(const char* userExtData) {
        NSString *ocString = [[NSString alloc] initWithUTF8String:userExtData?:""];
        if (ocString.length) {
            [BUAdSDKConfiguration configuration].userExtData = ocString;
        }
    }
    
    const char* UnionPlatform_userExtData() {
        return (char*)[[BUAdSDKConfiguration configuration].userExtData?:@"" cStringUsingEncoding:NSUTF8StringEncoding];
    }
    
    void UnionPlatform_setWebViewOfflineType(int webViewOfflineType) {
        [BUAdSDKConfiguration configuration].webViewOfflineType = (BUOfflineType)webViewOfflineType;
    }
    
    BUOfflineType UnionPlatform_webViewOfflineType() {
        return [BUAdSDKConfiguration configuration].webViewOfflineType;
    }
    
    void UnionPlatform_setGDPR(int GDPR) {
        [BUAdSDKConfiguration configuration].GDPR = @(GDPR);
    }
    
    int UnionPlatform_GDPR() {
        return [BUAdSDKConfiguration configuration].GDPR.intValue;
    }

    void UnionPlatform_setCCPA(int CCPA) {
        [BUAdSDKConfiguration configuration].CCPA = @(CCPA);
    }
    
    int UnionPlatform_CCPA() {
        return [BUAdSDKConfiguration configuration].CCPA.intValue;
    }

    void UnionPlatform_setThemeStatus(int themeStatus) {
        [BUAdSDKConfiguration configuration].themeStatus = @(themeStatus);
    }
    
    int UnionPlatform_themeStatus() {
        return [BUAdSDKConfiguration configuration].themeStatus.intValue;
    }
    
    void UnionPlatform_setAbvids(int abvids[],int length) {
        NSMutableArray *mutableArr = [[NSMutableArray alloc]initWithCapacity:length];
        for (int i = 0; i < length; i++) {
            [mutableArr addObject:@(abvids[i])];
        }
        [BUAdSDKConfiguration configuration].abvids = mutableArr.copy;
    }

    void UnionPlatform_setAbSDKVersion(const char* abSDKVersion) {
        NSString *ocString = [[NSString alloc] initWithUTF8String:abSDKVersion?:""];
        if (ocString.length) {
            [BUAdSDKConfiguration configuration].abSDKVersion = ocString;
        }
    }
    
    const char* UnionPlatform_abSDKVersion() {
        return (char*)[[BUAdSDKConfiguration configuration].abSDKVersion?:@"" cStringUsingEncoding:NSUTF8StringEncoding];
    }

    void UnionPlatform_setCustomIdfa(const char* customIdfa) {
        NSString *ocString = [[NSString alloc] initWithUTF8String:customIdfa?:""];
        if (ocString.length) {
            [BUAdSDKConfiguration configuration].customIdfa = ocString;
        }
    }
    
    const char* UnionPlatform_customIdfa() {
        return (char*)[[BUAdSDKConfiguration configuration].customIdfa?:@"" cStringUsingEncoding:NSUTF8StringEncoding];
    }
    
    void UnionPlatform_setAllowModifyAudioSessionSetting(bool allowModifyAudioSessionSetting) {
        [BUAdSDKConfiguration configuration].allowModifyAudioSessionSetting = allowModifyAudioSessionSetting;
    }
    
    bool UnionPlatform_allowModifyAudioSessionSetting() {
        return [BUAdSDKConfiguration configuration].allowModifyAudioSessionSetting;
    }

#if defined (__cplusplus)
}
#endif
