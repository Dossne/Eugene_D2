#import <UIKit/UIKit.h>
#import "UnityAppController.h"
#import <SayKit/SayKit-Swift.h>
#import "SayUnityStateController.h"
#import <FBSDKCoreKit/FBSDKCoreKit.h>


@interface SayAppDelegate : UnityAppController
-(void) onScheduledTimer:(NSTimer *)timer;
-(void) checkControllerName:(NSString *) controllerName;
@end

IMPL_APP_CONTROLLER_SUBCLASS(SayAppDelegate)

@implementation SayAppDelegate

-(BOOL)application:(UIApplication*) application didFinishLaunchingWithOptions:(NSDictionary*) options
{
    [UnityPauseObject setupUnityPausesWithUnityPause:^(BOOL shouldPause) {
        [SayUnityStateController UnityPause:shouldPause];
    } setAutoCheckPausedFlag:^(BOOL setFlag) {
        [SayUnityStateController SetAutoCheckPausedFlag:setFlag];
    }];
    [NSTimer scheduledTimerWithTimeInterval:1 target:self selector:@selector(onScheduledTimer:) userInfo:nil repeats:YES];
    [SayDelegate application:application didFinishLaunchingWithOptions:options];
 
    return [super application:application didFinishLaunchingWithOptions:options];
}
    
- (void)applicationWillTerminate:(UIApplication *)application {
    [SayDelegate applicationWillTerminate:application];
}

- (void)application:(UIApplication *)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken
{
    [SayDelegate application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
}

-(void) onScheduledTimer:(NSTimer *)timer {
    
    UIViewController *controller = [[[UIApplication sharedApplication] delegate] window].rootViewController.presentedViewController;
    
    if(controller){
        NSString* controllerName = NSStringFromClass([controller class]);
        [self checkControllerName:controllerName];
    }
    else
    {
        [SayUnityStateController CheckUnityPause:NO];
        [self checkControllerName:@"UnityAppController"];
    }
}

NSString* _presentedControllerName;
-(void) checkControllerName:(NSString *) controllerName
{
    if(![_presentedControllerName isEqualToString:controllerName])
    {
        _presentedControllerName = controllerName;
        
        [[SayKitEvent shared] trackWithEvent:@"controller" param1:0 param2:0 extra:controllerName param3:0 param4:0 extra2:@"" tag:@"" fileID:@"SayAppDelegate" function:@"-(void) checkControllerName:(NSString *) controllerName" line:68];
    }
}

- (void)application:(UIApplication *)application performActionForShortcutItem:(UIApplicationShortcutItem *)shortcutItem completionHandler:(void (^)(BOOL))completionHandler
{
    [SayDelegate application:application performActionFor:shortcutItem completionHandler:completionHandler];
}

- (BOOL)application:(UIApplication *)application
          openURL:(NSURL *)url
          options:(NSDictionary<UIApplicationOpenURLOptionsKey, id> *)options {
    return [SayDelegate application:application open:url options:options];
}

- (BOOL)application:(UIApplication *)application
continueUserActivity:(NSUserActivity *)userActivity
restorationHandler:(void (^)(NSArray<id<UIUserActivityRestoring>> * _Nullable))restorationHandler {
    return [SayDelegate application:application continue:userActivity  restorationHandler:restorationHandler];
}


@end
