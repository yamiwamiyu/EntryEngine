//
//  BUToUnityAdManager.m
//  Unity-iPhone
//
//  Created by CHAORS on 2020/7/28.
//

#import "BUToUnityAdManager.h"

#define kBUUnityMaxManagerCount 50


@interface BUToUnityAdManager ()

@property (nonatomic, strong) NSMutableDictionary<NSString *, id> *adManagerMap;

@end


@implementation BUToUnityAdManager

- (void)dealloc {
    
    if (self.adManagerMap.count) {
        [self.adManagerMap removeAllObjects];
        self.adManagerMap = nil;
    }
}

+ (BUToUnityAdManager *)sharedInstance {
    
    static BUToUnityAdManager *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[self alloc] init];
        sharedInstance.adManagerMap = [[NSMutableDictionary alloc] init];
    });
    return sharedInstance;
}

- (void)addAdManager:(id)adManger {
    
    if (!adManger) {
        return;
    }
    
    if (self.adManagerMap.count > kBUUnityMaxManagerCount) {
        [self.adManagerMap removeAllObjects];
    }
    
    [self.adManagerMap setValue:adManger forKey:[NSString stringWithFormat:@"%p", adManger]];
}

- (void)deleteAdManager:(id)adManger {
    
    if (!adManger) {
        return;
    }
    
    [self.adManagerMap removeObjectForKey:[NSString stringWithFormat:@"%p", adManger]];
}



@end
