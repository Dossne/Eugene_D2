//
//  SayUnityStateController.m
//  Unity-iPhone
//
//  Created by Timur Dularidze on 1.06.21.
//

#import <Foundation/Foundation.h>
#import "SayUnityStateController.h"
#import <SayKit/SayKit-Swift.h>


#ifdef __cplusplus
extern "C" {
#endif
    void UnityPause(int pause);
#ifdef __cplusplus
}
#endif


@implementation SayUnityStateController

static BOOL Paused = NO;
static BOOL AutoCheckPaused = NO;

+ (void) CheckUnityPause:(BOOL) pause
{   
    if(AutoCheckPaused)
    {
        return;
    }

    if(Paused != pause)
    {
        [SayUnityStateController UnityPause:pause];
        [[SayKitEvent shared] trackWithEvent:@"max_unity_state" param1:0 param2:5 extra:@"resumed" param3:0 param4:0 extra2:@"bug" tag:@"" fileID: @"SayUnityStateController" function:@"+ (void) CheckUnityPause:(BOOL) pause" line:37];
    }
}

+ (void) UnityPause:(BOOL) pause
{
    if(Paused == pause)
    {
        return;
    }
    
    Paused = pause;
    UnityPause(pause);
    
    if(!Paused)
    {
        AutoCheckPaused = NO;
    }
}

+ (void) SetAutoCheckPausedFlag:(BOOL) autoCheckPaused
{
    AutoCheckPaused = autoCheckPaused;
}

@end
