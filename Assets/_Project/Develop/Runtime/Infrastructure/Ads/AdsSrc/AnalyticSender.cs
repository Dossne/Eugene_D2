using System;

namespace Infrastructure.Ads
{
    public static class AnalyticSender
    {
#if PR_CHEAT || UNITY_EDITOR
        private static bool isDebugLog = false;
#endif


#if PR_SAYKIT_ENABLED

#endif
        public static void SendTagEvent(string eventName, string tag, int eventParam1, int eventParam2, string eventParam3, string eventParam4)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackTagEvent(eventName, tag, eventParam1, eventParam2, eventParam3, eventParam4);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tag tracked: {eventName} {tag} {eventParam1} {eventParam2} {eventParam3} {eventParam4}");
            }
#endif
        }


        public static void SendTagEvent(string eventName, string tag, int eventParam1, int eventParam2, int eventParam3, string eventParam4, string eventParam5)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackTagEvent(eventName, tag, eventParam1, eventParam2, eventParam3, eventParam4, eventParam5);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tag tracked: {eventName} {tag} {eventParam1} {eventParam2} {eventParam3} {eventParam4} {eventParam5}");
            }
#endif
        }


        public static void SendEvent(string name, int param1, int param2, string param3, string param4)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackEvent(name, param1, param2, param3, param4);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked: {name} {param1} {param2} {param3} {param4}");
            }
#endif
        }


        public static void SendEvent(string name, int param1, int param2, int param3, string param4)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackEvent(name, param1, param2, param3, param4);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked: {name} {param1} {param2} {param3} {param4}");
            }
#endif
        }


        public static void SendEvent(string name, int param1, int param2, int param3, string param4, string param5)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackEvent(name, param1, param2, param3, param4, param5);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked: {name} {param1} {param2} {param3} {param4} {param5}");
            }
#endif
        }


        public static void SendEvent(string name, int param1, string param2, string param3)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackEvent(name, param1, param2, param3);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked: {name} {param1} {param2} {param3}");
            }
#endif
        }

        public static void SendEvent(string name, int param1, string eventExtra2)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackEvent(name, param1, string.Empty, eventExtra2);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked: {name} {param1} {eventExtra2}");
            }
#endif
        }
        
        

        public static void SendEvent(string name, string eventExtra1, string eventExtra2)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackEvent(name, eventExtra1, eventExtra2);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked: {name} {eventExtra1} {eventExtra2}");
            }
#endif
        }


        public static void SendEvent(string name, string param1)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackEvent(name, param1);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked: {name} {param1}");
            }
#endif
        }


        public static void SendEvent(string name, int param1, int param2, string extra1)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackEvent(name, param1, param2, extra1);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked: {name}, {param1}, {param2}, {extra1}");
            }
#endif
        }


        public static void SendRewardedOfferEvent(string ads)
        {
#if PR_SAYKIT_ENABLED
            if (!SayKit.isRewardedAvailable(ads))
            {
                return;
            }

            SayKit.trackRewardedOffer(ads);
#endif


#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event rewarded offer tracked: {ads}");
            }
#endif
        }


        public static void SendSoftIncome(int amount, int total, string place, string extra)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackSoftIncome(amount, total, place, extra);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked soft income: {amount} {total} {place}");
            }
#endif
        }


        public static void SendSoftOutcome(int amount, int total, string place, string extra)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackSoftOutcome(amount, total, place, extra);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked soft outcome: {amount} {total} {place}");
            }
#endif
        }


        public static void SendHardIncome(int amount, int total, string place, string extra)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackHardIncome(amount, total, place, extra);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked hard income: {amount} {total} {place}");
            }
#endif
        }


        public static void SendHardOutcome(int amount, int total, string place, string extra)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackHardOutcome(amount, total, place, extra);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked hard outcome: {amount} {total} {place}");
            }
#endif
        }


        public static void TrackLevelStarted(string tag, int level, long score, string extra1)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackLevelStarted(tag, level, score, extra1);
#endif


#if PR_CHEAT || UNITY_EDITOR

            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked level started. tag: {tag}. level: {level}. score {score}. extra1 {extra1}");
            }
#endif
        }


        public static void TrackLevelCompleted(string tag, int level, int score, string extra1)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackLevelCompleted(tag, level, score, extra1);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked level completed. tag: {tag}. level: {level}. score : {score}. extra1: {extra1}");
            }
#endif
        }


        public static void TrackLevelFailed(string tag, int level, int score, string extra1)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackLevelFailed(tag, level, score, extra1);
#endif

#if PR_CHEAT || UNITY_EDITOR

            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked level failed. tag: {tag}. level: {level}. score : {score}. extra1: {extra1}");
            }
#endif
        }


        public static void TrackLevelExtraStarted(string tag, int number, string extra1, string extra2)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackLevelExtraStarted(tag, number, extra1, extra2);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked level extra started: {tag} {number} {extra1}");
            }
#endif
        }


        public static void TrackLevelExtraCompleted(string tag, int score, int number, string extra1, string extra2)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackLevelExtraCompleted(tag, score, number, extra1, extra2);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked level extra completed: {tag} {score} {number} {extra1}");
            }
#endif
        }


        public static void TrackLevelExtraFailed(string tag, int score, int number, string extra1, string extra2)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackLevelExtraFailed(tag, score, number, extra1, extra2);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Event tracked level extra failed: {tag} {score} {number} {extra1}");
            }
#endif
        }


        public static void TrackTutorialStep(string tutorialName, string stepName)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackTutorialStep(tutorialName, stepName);
#endif
            

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Tutorial step tracked: {tutorialName} {stepName}");
            }
#endif
        }


        public static void TrackTagEvent(string eventName, string tag, long eventParam1, long eventParam2, string eventExtra1, string eventExtra2)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackTagEvent(eventName, tag, eventParam1, eventParam2, eventExtra1, eventExtra2);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Track tag event: {eventName}, {tag}, {eventParam1}, {eventParam2}, {eventExtra1}, {eventExtra2}");
            }
#endif
        }


        public static void TrackAppLoaded()
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackApplicationLoaded();
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log("Track ApplicationLoaded");
            }
#endif
        }


        public static void TrackClick(string screen, string element)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackClick(screen, element);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Track Click: {screen},  {element}");
            }
#endif
        }

        public static void TrackScreen(string screen)
        {
#if PR_SAYKIT_ENABLED
            SayKit.trackScreen(screen);
#endif

#if PR_CHEAT || UNITY_EDITOR
            if (isDebugLog)
            {
                UnityEngine.Debug.Log($"Track Screen: {screen}");
            }
#endif
        }
    }
}