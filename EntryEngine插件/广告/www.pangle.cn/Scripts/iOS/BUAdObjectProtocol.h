//
//  NSObject+BUBridgeAdObject.h
//  UnityFramework
//
//  Created by yujie on 2021/10/21.
//

#import <BUAdSDK/BUAdSDK.h>

@protocol BUAdObjectProtocol <NSObject>

- (id<BUAdClientBiddingProtocol>) adObject;
@end
