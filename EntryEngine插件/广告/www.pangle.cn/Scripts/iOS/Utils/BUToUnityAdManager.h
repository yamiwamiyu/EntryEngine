//
//  BUToUnityAdManager.h
//  Unity-iPhone
//
//  Created by CHAORS on 2020/7/28.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface BUToUnityAdManager : NSObject

+ (BUToUnityAdManager *)sharedInstance;

- (void)addAdManager:(id)adManger;
- (void)deleteAdManager:(id)adManger;

@end

NS_ASSUME_NONNULL_END
