using UnityEngine;
using System;

/* Remote Config */

[Serializable]
public class SayKitGameConfig
{

    /* Place remote configs parameters here as class properties with default values. */

    public int some_param = 0;
    public float other_param = 2;
    public string string_param = "";

    /*
    You can access them using:

    SayKit.gameConfig.some_param;
    SayKit.gameConfig.other_param;
    SayKit.gameConfig.string_param;
    */
}

public class SayKitApp
{
    /* Features */
    public const bool notificationsEnabled = false;

    /* App settings */
    public const string APP_NAME_CHINA_IOS = "";
    public const string APP_NAME_IOS = "Roll the Hole";

    public const string APP_BUNDLE_CHINA_IOS = "";
    public const string APP_BUNDLE_IOS = "com.ultimatelavashgames.rollthehole";

#if SAYKIT_CHINA_VERSION
    public static bool purchasesEnabled = false;

    public const string APP_KEY_IOS = "<APP_KEY_CHINA_IOS or empty>";
    public const string APP_SECRET_IOS = "<APP_SECRET_CHINA_IOS or empty>";

#else
    public static bool purchasesEnabled = true;

    public const string APP_KEY_IOS = "rllthli";
    public const string APP_SECRET_IOS = "jmqqKFii2gDyEY7zpXFUDAtdOyE1T3jP";

#endif

    public const string APP_KEY_ANDROID = "rllthla";
    public const string APP_SECRET_ANDROID = "DDNf5YafM5E1OLOgPYs7XH0AMhlID3ZI";

    public const string PROMO_KEY_IOS = "";

    /* App constants */
    public const string AD_INTERSTITIAL = "ad_interstitial";
    public const string AD_REWARDED = "ad_rewarded";


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void init()
    {
#if PR_SAYKIT_ENABLED
        SayKitConfig config = new SayKitConfig
        {
            disableAutoBanner = true
        };

#if UNITY_IOS
		config.appKey = APP_KEY_IOS;
#elif UNITY_ANDROID
        config.appKey = APP_KEY_ANDROID;
#endif

        //For delayed remote config update for certain players group
        //It takes up to 30 sec
        config.attributionConfigUpdate = true;
        config.remoteConfigUpdated = RemoteConfigUpdated;
        config.nonConfirmedPurchaseReceived += NonConfirmedPurchaseReceived;
        SayKit.init(config);
#endif
    }

    #region Custom

    //For delayed remote config update for certain players group
    public static Action OnRemoteConfigUpdated;
    public static Action OnNonConfirmedPurchaseReceived;

    private static void RemoteConfigUpdated()
    {
        OnRemoteConfigUpdated?.Invoke();
    }

    private static void NonConfirmedPurchaseReceived()
    {
        OnNonConfirmedPurchaseReceived?.Invoke();
    }

    public static string GetAppKey()
    {
#if UNITY_IOS
        return APP_KEY_IOS;
#elif UNITY_ANDROID
        return APP_KEY_ANDROID;
#endif
    }

    #endregion

}