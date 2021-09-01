//
//  BUBundleHelper.m
//  BUFoundation
//
//  Created by wangchao on 2020/6/10.
//  Copyright © 2020 Union. All rights reserved.
//

#import "BUToUnityBundleHelper.h"


static NSBundle *sdkResourceBundle;


@implementation NSArray (BUBundle)

- (NSArray *)bu_compactBundleMap:(id (^)(id))transform {
    
    NSMutableArray *array = [[NSMutableArray alloc] initWithCapacity:1];
    
    for (id obj in self) {
        
        id transformObj = transform(obj);
        
        if (transformObj) {
            
            [array addObject:transformObj];
        }
    }
    
    return array;
}

@end


@implementation BUToUnityBundleHelper

+ (NSBundle *)resourceBundle {
    
    if (sdkResourceBundle) {
        
        return sdkResourceBundle;
    }
    
    // 主bundle找不到，尝试从其他framework里找(这里主要为了适配unity2019.3+，bundle会自动导入到unity.framework动态库) 注：这里只对动态库管用，bundle置于静态库framework中无用！！！
    static dispatch_once_t token;
    dispatch_once(&token, ^{
       
        // 默认赋值主bundle
        NSString *bundlePath = [[NSBundle mainBundle] pathForResource:@"BUAdSDK" ofType:@"bundle"];
        
        if (!bundlePath) {
            
            // 借鉴AppLovin处理方式，AppLovin验证当在400+frameworks寻找，耗时约0.5s 故实际可接受
            bundlePath = [[[NSBundle allFrameworks] bu_compactBundleMap:^id(NSBundle *frameworkBundle) {
                
                return [frameworkBundle pathForResource:@"BUAdSDK" ofType:@"bundle"];
                
            }] firstObject];
        }
        
        if (bundlePath) {
            sdkResourceBundle = [NSBundle bundleWithPath:bundlePath];
        }
    });
    
    return sdkResourceBundle;
}

@end


