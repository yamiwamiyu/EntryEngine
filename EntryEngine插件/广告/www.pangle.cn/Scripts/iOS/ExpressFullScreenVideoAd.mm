//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

#import <BUAdSDK/BUAdSDK.h>
#import "UnityAppController.h"

static const char* AutonomousStringCopy1(const char* string)
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
@interface ExpressFullScreenVideoAd : NSObject
@end

@interface ExpressFullScreenVideoAd () <BUNativeExpressFullscreenVideoAdDelegate>
@property (nonatomic, strong) BUNativeExpressFullscreenVideoAd *fullScreenVideoAd;

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

@implementation ExpressFullScreenVideoAd

#pragma mark - BUFullscreenVideoAdDelegate
- (void)nativeExpressFullscreenVideoAdDidLoad:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd {
    self.onFullScreenVideoAdLoad((__bridge void*)self, self.loadContext);
}

- (void)nativeExpressFullscreenVideoAd:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd didFailWithError:(NSError *_Nullable)error {
    self.onError((int)error.code, AutonomousStringCopy1([[error localizedDescription] UTF8String]), self.loadContext);
}

- (void)nativeExpressFullscreenVideoAdViewRenderSuccess:(BUNativeExpressFullscreenVideoAd *)rewardedVideoAd {
}

- (void)nativeExpressFullscreenVideoAdViewRenderFail:(BUNativeExpressFullscreenVideoAd *)rewardedVideoAd error:(NSError *_Nullable)error {
    self.onError((int)error.code, AutonomousStringCopy1([[error localizedDescription] UTF8String]), self.loadContext);
}

- (void)nativeExpressFullscreenVideoAdDidDownLoadVideo:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd {
    self.onFullScreenVideoCached(self.loadContext);
}

- (void)nativeExpressFullscreenVideoAdWillVisible:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd {
    self.onAdShow(self.interactionContext);
}

- (void)nativeExpressFullscreenVideoAdDidVisible:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd {
}

- (void)nativeExpressFullscreenVideoAdDidClick:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd {
    self.onAdVideoBarClick(self.interactionContext);
}

- (void)nativeExpressFullscreenVideoAdDidClickSkip:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd {
    self.onAdVideoClickSkip(self.interactionContext);
}

- (void)nativeExpressFullscreenVideoAdWillClose:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd {
}

- (void)nativeExpressFullscreenVideoAdDidClose:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd {
    self.onAdClose(self.interactionContext);
}

- (void)nativeExpressFullscreenVideoAdDidPlayFinish:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd didFailWithError:(NSError *_Nullable)error {
    if (error) {
        self.onVideoError(self.interactionContext);
    } else {
        self.onVideoComplete(self.interactionContext);
    }
}

@end

#if defined (__cplusplus)
extern "C" {
#endif

void UnionPlatform_ExpressFullScreenVideoAd_Load(
    const char* slotID,
    const char* userID,
    FullScreenVideoAd_OnError onError,
    FullScreenVideoAd_OnFullScreenVideoAdLoad onFullScreenVideoAdLoad,
    FullScreenVideoAd_OnFullScreenVideoCached onFullScreenVideoCached,
    int context) {

    BUNativeExpressFullscreenVideoAd* fullScreenVideoAd = [[BUNativeExpressFullscreenVideoAd alloc] initWithSlotID:[[NSString alloc] initWithUTF8String:slotID?:""]];
    ExpressFullScreenVideoAd* instance = [[ExpressFullScreenVideoAd alloc] init];
    instance.fullScreenVideoAd = fullScreenVideoAd;
    instance.onError = onError;
    instance.onFullScreenVideoAdLoad = onFullScreenVideoAdLoad;
    instance.onFullScreenVideoCached = onFullScreenVideoCached;
    instance.loadContext = context;
    fullScreenVideoAd.delegate = instance;
    [fullScreenVideoAd loadAdData];

    (__bridge_retained void*)instance;
}

void UnionPlatform_ExpressFullScreenVideoAd_SetInteractionListener(
    void* fullScreenVideoAdPtr,
    FullScreenVideoAd_OnAdShow onAdShow,
    FullScreenVideoAd_OnAdVideoBarClick onAdVideoBarClick,
    FullScreenVideoAd_OnAdVideoClickSkip onAdVideoClickSkip,
    FullScreenVideoAd_OnAdClose onAdClose,
    FullScreenVideoAd_OnVideoComplete onVideoComplete,
    FullScreenVideoAd_OnVideoError onVideoError,
    int context) {
    ExpressFullScreenVideoAd* fullScreenVideoAd = (__bridge ExpressFullScreenVideoAd*)fullScreenVideoAdPtr;
    fullScreenVideoAd.onAdShow = onAdShow;
    fullScreenVideoAd.onAdVideoBarClick = onAdVideoBarClick;
    fullScreenVideoAd.onAdVideoClickSkip = onAdVideoClickSkip;
    fullScreenVideoAd.onAdClose = onAdClose;
    fullScreenVideoAd.onVideoComplete = onVideoComplete;
    fullScreenVideoAd.onVideoError = onVideoError;
    fullScreenVideoAd.interactionContext = context;
}

void UnionPlatform_ExpressFullScreenVideoAd_ShowFullScreenVideoAd(void* fullscreenVideoAdPtr) {
    ExpressFullScreenVideoAd* fullscreenVideoAd = (__bridge ExpressFullScreenVideoAd*)fullscreenVideoAdPtr;
    [fullscreenVideoAd.fullScreenVideoAd showAdFromRootViewController:GetAppController().rootViewController];}

void UnionPlatform_ExpressFullScreenVideoAd_Dispose(void* fullscreenVideoAdPtr) {
    (__bridge_transfer ExpressFullScreenVideoAd*)fullscreenVideoAdPtr;
}

#if defined (__cplusplus)
}
#endif
