//
//  SayUnityStateController.h
//  Unity-iPhone
//
//  Created by Timur Dularidze on 1.06.21.
//

#ifndef SayUnityStateController_h
#define SayUnityStateController_h

@interface SayUnityStateController : NSObject 
+ (void) CheckUnityPause:(BOOL) pause;
+ (void) UnityPause:(BOOL) pause;
+ (void) SetAutoCheckPausedFlag:(BOOL) autoCheckPaused;
@end

#endif /* SayUnityStateController_h */
