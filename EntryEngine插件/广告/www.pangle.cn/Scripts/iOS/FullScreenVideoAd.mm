//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

#import <BUAdSDK/BUFullscreenVideoAd.h>
#import "UnityAppController.h"

const char* AutonomousStringCopy1(const char* string)
{
    if (string == NULL) {
        return NULL;
    }
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

// Load Status
typedef void(*FullScreenVideoAd_OnError)(int code, const char* message, int context);
typedef void(*FullScreenVideoAd_OnFullScreenVideoAdLoad)(void* fullScreenVideoAd, int context);
typedef void(*FullScreenVideoAd_OnFullScreenVideoCached)(int context);

// InteractionListener callbacks.
typedef void(*FullScreenVideoAd_OnAdShow)(int context);
typedef void(*FullScreenVideoAd_OnAdVideoBarClick)(int context);
typedef void(*FullScreenVideoAd_OnAdVideoClickSkip)(int context);
typedef void(*FullScreenVideoAd_OnAdClose)(int context);
typedef void(*FullScreenVideoAd_OnVideoComplete)(int context);
typedef void(*FullScreenVideoAd_OnVideoError)(int context);

//
@interface FullScreenVideoAd : NSObject
@end

@interface FullScreenVideoAd () <BUFullscreenVideoAdDelegate>
@property (nonatomic, strong) BUFullscreenVideoAd *fullScreenVideoAd;

@property (nonatomic, assign) int loadContext;
@property (nonatomic, assign) FullScreenVideoAd_OnError onError;
@property (nonatomic, assign) FullScreenVideoAd_OnFullScreenVideoAdLoad onFullScreenVideoAdLoad;
@property (nonatomic, assign) FullScreenVideoAd_OnFullScreenVideoCached onFullScreenVideoCached;

@property (nonatomic, assign) int interactionContext;
@property (nonatomic, assign) FullScreenVideoAd_OnAdShow onAdShow;
@property (nonatomic, assign) FullScreenVideoAd_OnAdVideoBarClick onAdVideoBarClick;
@property (nonatomic, assign) FullScreenVideoAd_OnAdVideoClickSkip onAdVideoClickSkip;
@property (nonatomic, assign) FullScreenVideoAd_OnAdClose onAdClose;
@property (nonatomic, assign) FullScreenVideoAd_OnVideoComplete onVideoComplete;
@property (nonatomic, assign) FullScreenVideoAd_OnVideoError onVideoError;
@end

@implementation FullScreenVideoAd

- (void)fullscreenVideoMaterialMetaAdDidLoad:(BUFullscreenVideoAd *)fullscreenVideoAd {
    self.onFullScreenVideoAdLoad((__bridge void*)self, self.loadContext);
}

- (void)fullscreenVideoAdVideoDataDidLoad:(BUFullscreenVideoAd *)fullscreenVideoAd {
    self.onFullScreenVideoCached(self.loadContext);
}

- (void)fullscreenVideoAdWillVisible:(BUFullscreenVideoAd *)fullscreenVideoAd {
    self.onAdShow(self.interactionContext);
}

- (void)fullscreenVideoAdDidVisible:(BUFullscreenVideoAd *)fullscreenVideoAd {

}

- (void)fullscreenVideoAdWillClose:(BUFullscreenVideoAd *)fullscreenVideoAd {

}

- (void)fullscreenVideoAdDidClose:(BUFullscreenVideoAd *)fullscreenVideoAd {
    self.onAdClose(self.interactionContext);
}

- (void)fullscreenVideoAdDidClick:(BUFullscreenVideoAd *)fullscreenVideoAd {
    self.onAdVideoBarClick(self.interactionContext);
}

- (void)fullscreenVideoAd:(BUFullscreenVideoAd *)fullscreenVideoAd didFailWithError:(NSError *)error {
    self.onError((int)error.code, AutonomousStringCopy1([[error localizedDescription] UTF8String]), self.loadContext);
}

- (void)fullscreenVideoAdDidPlayFinish:(BUFullscreenVideoAd *)fullscreenVideoAd didFailWithError:(NSError *)error {
    if (error) {
        self.onVideoError(self.interactionContext);
    } else {
        self.onVideoComplete(self.interactionContext);
    }
}

- (void)fullscreenVideoAdDidClickSkip:(BUFullscreenVideoAd *)fullscreenVideoAd {
    self.onAdVideoClickSkip(self.interactionContext);
}



@end

#if defined (__cplusplus)
extern "C" {
#endif

void UnionPlatform_FullScreenVideoAd_Load(
    const char* slotID,
    const char* userID,
    FullScreenVideoAd_OnError onError,
    FullScreenVideoAd_OnFullScreenVideoAdLoad onFullScreenVideoAdLoad,
    FullScreenVideoAd_OnFullScreenVideoCached onFullScreenVideoCached,
    int context) {

    BUFullscreenVideoAd* fullScreenVideoAd = [[BUFullscreenVideoAd alloc] initWithSlotID:[[NSString alloc] initWithUTF8String:slotID?:""]];
    FullScreenVideoAd* instance = [[FullScreenVideoAd alloc] init];
    instance.fullScreenVideoAd = fullScreenVideoAd;
    instance.onError = onError;
    instance.onFullScreenVideoAdLoad = onFullScreenVideoAdLoad;
    instance.onFullScreenVideoCached = onFullScreenVideoCached;
    instance.loadContext = context;
    fullScreenVideoAd.delegate = instance;
    [fullScreenVideoAd loadAdData];

    (__bridge_retained void*)instance;
}

void UnionPlatform_FullScreenVideoAd_SetInteractionListener(
    void* fullScreenVideoAdPtr,
    FullScreenVideoAd_OnAdShow onAdShow,
    FullScreenVideoAd_OnAdVideoBarClick onAdVideoBarClick,
    FullScreenVideoAd_OnAdVideoClickSkip onAdVideoClickSkip,
    FullScreenVideoAd_OnAdClose onAdClose,
    FullScreenVideoAd_OnVideoComplete onVideoComplete,
    FullScreenVideoAd_OnVideoError onVideoError,
    int context) {
    FullScreenVideoAd* fullScreenVideoAd = (__bridge FullScreenVideoAd*)fullScreenVideoAdPtr;
    fullScreenVideoAd.onAdShow = onAdShow;
    fullScreenVideoAd.onAdVideoBarClick = onAdVideoBarClick;
    fullScreenVideoAd.onAdVideoClickSkip = onAdVideoClickSkip;
    fullScreenVideoAd.onAdClose = onAdClose;
    fullScreenVideoAd.onVideoComplete = onVideoComplete;
    fullScreenVideoAd.onVideoError = onVideoError;
    fullScreenVideoAd.interactionContext = context;
}

void UnionPlatform_FullScreenVideoAd_ShowFullScreenVideoAd(void* fullscreenVideoAdPtr) {
    FullScreenVideoAd* fullscreenVideoAd = (__bridge FullScreenVideoAd*)fullscreenVideoAdPtr;
    [fullscreenVideoAd.fullScreenVideoAd showAdFromRootViewController:GetAppController().rootViewController];}

void UnionPlatform_FullScreenVideoAd_Dispose(void* fullscreenVideoAdPtr) {
    (__bridge_transfer FullScreenVideoAd*)fullscreenVideoAdPtr;
}

#if defined (__cplusplus)
}
#endif
