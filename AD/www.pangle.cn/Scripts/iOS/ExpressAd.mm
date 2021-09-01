//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

#import <BUAdSDK/BUAdSDK.h>
#import "UnityAppController.h"

static const char* AutonomousStringCopy_Express(const char* string);

static const char* AutonomousStringCopy_Express(const char* string)
{
    if (string == NULL) {
        return NULL;
    }
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}


// ISplashAdListener callbacks.
typedef void(*ExpressAd_OnLoad)(void* expressAd, int context);
typedef void(*ExpressAd_OnLoadError)(int errCode,const char* errMsg,int context);
typedef void(*ExpressAd_ViewRenderSuccess)(int index, float width, float height, int context);
typedef void(*ExpressAd_ViewRenderError)(int index, int errCode, const char* errMsg, int context);
typedef void(*ExpressAd_WillShow)(int index, int context);
typedef void(*ExpressAd_DidClick)(int index, int context);
typedef void(*ExpressAd_PlayerDidPlayFinish)(int index, int context);
typedef void(*ExpressAd_DislikeClose)(int index, int dislikeID, const char* dislikeReason, int context);
typedef void(*ExpressAd_DidClose)(int index, int context);
typedef void(*ExpressAd_WillPresentScreen)(int index, int context);
typedef void(*ExpressAd_DidRemove)(int index, int context);


@interface LGExpressAdView : UIView

@property (nonatomic, assign) NSInteger index;

@end

@implementation LGExpressAdView

- (instancetype)initWithView:(BUNativeExpressAdView *)view index:(NSInteger)index{
    if (self = [super init]) {
        self.frame = CGRectMake(0, 0, view.bounds.size.width, view.bounds.size.height);
        self.index = index;
        [self addSubview:view];
        self.backgroundColor = [UIColor whiteColor];
    }
    return self;
}

@end


@interface LGToUnityExpressAdManager : NSObject<BUNativeExpressAdViewDelegate>

@property(nonatomic, strong)BUNativeExpressAdManager *expressAdManager;
@property (strong, nonatomic) NSMutableArray<__kindof BUNativeExpressAdView *> *expressAdViews;
@property (strong, nonatomic) NSMutableArray<__kindof LGExpressAdView *> *lgAdViews;
@property (nonatomic, assign) float adWidth;
@property (nonatomic, assign) float adHeight;

@property (nonatomic, assign) int loadContext;
@property (nonatomic, assign) int listenContext;
@property (nonatomic, assign) int dislikeContext;

@property (nonatomic, assign) ExpressAd_OnLoad onLoad;
@property (nonatomic, assign) ExpressAd_OnLoadError onLoadError;
@property (nonatomic, assign) ExpressAd_ViewRenderSuccess viewRenderSuccess;
@property (nonatomic, assign) ExpressAd_ViewRenderError viewRenderError;
@property (nonatomic, assign) ExpressAd_WillShow willShow;
@property (nonatomic, assign) ExpressAd_DidClick didClick;
@property (nonatomic, assign) ExpressAd_PlayerDidPlayFinish playerDidPlayFinish;
@property (nonatomic, assign) ExpressAd_DislikeClose dislikeClose;
@property (nonatomic, assign) ExpressAd_DidClose didClose;
@property (nonatomic, assign) ExpressAd_WillPresentScreen willPresentScreen;
@property (nonatomic, assign) ExpressAd_DidRemove didRemove;

@end


@implementation LGToUnityExpressAdManager

+ (LGToUnityExpressAdManager *)sharedInstance {
    static LGToUnityExpressAdManager *manager = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        if(!manager) {
            manager = [[self alloc] init];
            manager.expressAdViews = [[NSMutableArray alloc] init];
            manager.lgAdViews = [[NSMutableArray alloc] init];
        }
    });
    return manager;
}

- (void)refreshAds:(NSInteger)loadCount {
    [self.expressAdManager loadAdDataWithCount:loadCount];
}

- (void)removeAllAds {
    for (BUNativeExpressAdView *nativeExpressAdView in self.expressAdViews) {
        LGExpressAdView *adView = (LGExpressAdView *)nativeExpressAdView.superview;
        [adView removeFromSuperview];
    }
    [self.expressAdViews removeAllObjects];
    [self.lgAdViews removeAllObjects];
}

- (void)removeAdView:(BUNativeExpressAdView *)nativeExpressAdView {
    if (!nativeExpressAdView) return;
    LGExpressAdView *adView = (LGExpressAdView *)nativeExpressAdView.superview;
    [adView removeFromSuperview];
    if ([self.lgAdViews containsObject:adView]) {
        [self.lgAdViews removeObject:adView];
    }
    if ([self.expressAdViews containsObject:nativeExpressAdView]) {
        [self.expressAdViews removeObject:nativeExpressAdView];
    }
    adView = nil;
}
#pragma mark - BUNativeExpressAdViewDelegate
- (void)nativeExpressAdSuccessToLoad:(BUNativeExpressAdManager *)nativeExpressAd views:(NSArray<__kindof BUNativeExpressAdView *> *)views {
    
    [self removeAllAds];
    
    //【重要】不能保存太多view，需要在合适的时机手动释放不用的，否则内存会过大
    if (views.count) {
        [views enumerateObjectsUsingBlock:^(id  _Nonnull obj, NSUInteger idx, BOOL * _Nonnull stop) {
            UIViewController *rootVc = GetAppController().rootViewController;
            BUNativeExpressAdView *expressView = (BUNativeExpressAdView *)obj;
            expressView.rootViewController = rootVc;
            [expressView render];
        }];
    }
    
    if (self.onLoad) {
        self.onLoad((__bridge void*)self, self.loadContext);
    }
}

- (void)nativeExpressAdFailToLoad:(BUNativeExpressAdManager *)nativeExpressAd error:(NSError *)error {
    NSLog(@"nativeExpressAd FailToLoad:%@", error);

    if (self.onLoadError) {
        self.onLoadError((int)error.code,AutonomousStringCopy_Express([[error localizedDescription] UTF8String]),self.loadContext);
    }
}

- (void)nativeExpressAdViewRenderSuccess:(BUNativeExpressAdView *)nativeExpressAdView {
    NSLog(@"nativeExpressAd ViewRenderSuccess");
    
    if (self.viewRenderSuccess) {
        [self.expressAdViews addObject:nativeExpressAdView];
        NSInteger index = [self.expressAdViews indexOfObject:nativeExpressAdView];
        LGExpressAdView *adView = [[LGExpressAdView alloc] initWithView:nativeExpressAdView index:index];
        [self.lgAdViews addObject:adView];
        
        CGSize size =  nativeExpressAdView.bounds.size;
        self.viewRenderSuccess((int)index, (float)size.width*[UIScreen mainScreen].scale, (float)size.height*[UIScreen mainScreen].scale, self.listenContext);
    }
}

- (void)nativeExpressAdViewRenderFail:(BUNativeExpressAdView *)nativeExpressAdView error:(NSError *)error {
    NSLog(@"nativeExpressAd ViewRenderFail:%@", error);
    
    if (self.viewRenderError) {
        int index = (int)[self.expressAdViews indexOfObject:nativeExpressAdView];
        self.viewRenderError(index, (int)error.code,AutonomousStringCopy_Express([error.localizedDescription UTF8String]),self.listenContext);
    }
}

- (void)nativeExpressAdViewWillShow:(BUNativeExpressAdView *)nativeExpressAdView {
    NSLog(@"nativeExpressAd ViewWillShow");
    
    if (self.willShow) {
        int index = (int)[self.expressAdViews indexOfObject:nativeExpressAdView];
        self.willShow(index, self.listenContext);
    }
}

- (void)nativeExpressAdViewDidClick:(BUNativeExpressAdView *)nativeExpressAdView {
    NSLog(@"nativeExpressAd ViewDidClick");
    
    if (self.didClick) {
        int index = (int)[self.expressAdViews indexOfObject:nativeExpressAdView];
        self.didClick(index, self.listenContext);
    }
}

- (void)nativeExpressAdViewPlayerDidPlayFinish:(BUNativeExpressAdView *)nativeExpressAdView error:(NSError *)error {
    NSLog(@"nativeExpressAd ViewPlayerDidPlayFinish:%@", error);
    
    if (self.playerDidPlayFinish) {
        int index = (int)[self.expressAdViews indexOfObject:nativeExpressAdView];
        self.playerDidPlayFinish(index, self.listenContext);
    }
}

- (void)nativeExpressAdView:(BUNativeExpressAdView *)nativeExpressAdView dislikeWithReason:(NSArray<BUDislikeWords *> *)filterWords {
    
    if (filterWords.count) {
        BUDislikeWords *words = [filterWords firstObject];
        if (self.dislikeClose) {
            int index = (int)[self.expressAdViews indexOfObject:nativeExpressAdView];
            self.dislikeClose(index, (int)[words.dislikeID integerValue],  AutonomousStringCopy_Express([words.name UTF8String]), self.dislikeContext);
        }
    }
    if (self.didClose) {
        int index = (int)[self.expressAdViews indexOfObject:nativeExpressAdView];
        self.didClose(index, self.listenContext);
    }
}

- (void)nativeExpressAdViewWillPresentScreen:(BUNativeExpressAdView *)nativeExpressAdView {
    NSLog(@"nativeExpressAd ViewWillPresentScreen");
    
    if (self.willPresentScreen) {
        int index = (int)[self.expressAdViews indexOfObject:nativeExpressAdView];
        self.willPresentScreen(index, self.listenContext);
    }
}

/**
 This method is called when the Ad view container is forced to be removed.
 @param nativeExpressAdView : Ad view container
 */
- (void)nativeExpressAdViewDidRemoved:(BUNativeExpressAdView *)nativeExpressAdView {
    if (self.didRemove) {
        int index = (int)[self.expressAdViews indexOfObject:nativeExpressAdView];
        self.didRemove(index, self.listenContext);
    }
    
    [self removeAdView:nativeExpressAdView];
}

@end

#if defined (__cplusplus)
extern "C" {
#endif
    void UnionPlatform_ExpressAd_Load(
                                      int context,
                                      const char* slotID,
                                      float width,
                                      float height,
                                      int adCount,
                                      ExpressAd_OnLoad onLoad,
                                      ExpressAd_OnLoadError onLoadError) {
        LGToUnityExpressAdManager *instance = [LGToUnityExpressAdManager sharedInstance];
        instance.loadContext = context;
        instance.onLoad = onLoad;
        instance.onLoadError = onLoadError;
        instance.adWidth = width / [UIScreen mainScreen].scale;
        instance.adHeight = height / [UIScreen mainScreen].scale;
        
        BUAdSlot *slot = [[BUAdSlot alloc] init];
        slot.ID = [NSString stringWithUTF8String:slotID];
        slot.AdType = BUAdSlotAdTypeFeed;
        BUSize *imgSize = [BUSize sizeBy:BUProposalSize_Feed228_150];
        slot.imgSize = imgSize;
        slot.position = BUAdSlotPositionFeed;
        
        CGFloat expressWidth = (CGFloat)instance.adWidth;
        CGFloat expressHeight = (CGFloat)instance.adHeight;
        
        if (expressWidth > [UIScreen mainScreen].bounds.size.width) {
            expressWidth = [UIScreen mainScreen].bounds.size.width;
        }
        
        if (expressHeight > [UIScreen mainScreen].bounds.size.height) {
            expressHeight = [UIScreen mainScreen].bounds.size.height;
        }
        CGSize adSize = CGSizeMake(expressWidth, 0);
        
        instance.expressAdManager = [[BUNativeExpressAdManager alloc] initWithSlot:slot adSize:adSize];
        instance.expressAdManager.delegate = [LGToUnityExpressAdManager sharedInstance];
        [instance.expressAdManager loadAdDataWithCount:adCount];
        
        (__bridge_retained void*)instance;
    }
    
    void UnionPlatform_ExpressAd_SetInteractionListener(
                                                        int context,
                                                        void* expressAdPtr,
                                                        ExpressAd_ViewRenderSuccess viewRenderSuccess,
                                                        ExpressAd_ViewRenderError viewRenderError,
                                                        ExpressAd_WillShow willShow,
                                                        ExpressAd_DidClick didClick,
                                                        ExpressAd_DidRemove didRemove,
                                                        ExpressAd_DidClose didClose
                                                        ) {
        
        [LGToUnityExpressAdManager sharedInstance].listenContext = context;
        [LGToUnityExpressAdManager sharedInstance].viewRenderSuccess = viewRenderSuccess;
        [LGToUnityExpressAdManager sharedInstance].viewRenderError = viewRenderError;
        [LGToUnityExpressAdManager sharedInstance].willShow = willShow;
        [LGToUnityExpressAdManager sharedInstance].didClick = didClick;
        [LGToUnityExpressAdManager sharedInstance].didRemove = didRemove;
        [LGToUnityExpressAdManager sharedInstance].didClose = didClose;
    }
    
    void UnionPlatform_ExpressAd_SetDislikeListener(int context,
                                                    ExpressAd_DislikeClose dislikeClose) {
        [LGToUnityExpressAdManager sharedInstance].dislikeContext = context;
        [LGToUnityExpressAdManager sharedInstance].dislikeClose = dislikeClose;
        
    }
    
    void UnionPlatform_ExpressAd_Show(int index, float originX, float originY) {
        
         if (index < [LGToUnityExpressAdManager sharedInstance].lgAdViews.count) {
            
            LGExpressAdView *adView = [[LGToUnityExpressAdManager sharedInstance].lgAdViews objectAtIndex:index];
            CGFloat x = (CGFloat)(originX / [UIScreen mainScreen].scale);
            CGFloat y = (CGFloat)(originY / [UIScreen mainScreen].scale);
            adView.frame = CGRectMake(x, y, CGRectGetWidth(adView.frame), CGRectGetHeight(adView.frame));
            [GetAppController().rootViewController.view addSubview:adView];
        }
    }
    
    void UnionPlatform_ExpressAd_Dispose(void* expressAdPtr) {
        dispatch_async(dispatch_get_main_queue(), ^{
            [[LGToUnityExpressAdManager sharedInstance] removeAllAds];
            (__bridge_transfer LGToUnityExpressAdManager*)expressAdPtr;
        });
    }
    
    void UnionPlatform_ExpressAdView_Dispose(void* expressAdViewPtr) {
        dispatch_async(dispatch_get_main_queue(), ^{
            [[LGToUnityExpressAdManager sharedInstance] removeAdView:(__bridge_transfer BUNativeExpressAdView*)expressAdViewPtr];
        });
    }
    
#if defined (__cplusplus)
}
#endif
