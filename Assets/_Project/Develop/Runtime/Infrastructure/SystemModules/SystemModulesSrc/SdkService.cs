namespace Infrastructure.SystemModules
{
    /// <summary>
    /// A wrapper over SayKit, another publisher SDK (everything that is not included in Advertisement and AnalyticSender by meaning)
    /// </summary>
    public class SdkService
    {
        public bool IsInitialized()
        {
#if PR_SAYKIT_ENABLED
            return SayKit.isInitialized;
#else
            return true;
#endif
        }

        public bool IsRemoteConfigLoaded()
        {
#if PR_SAYKIT_ENABLED
            return SayKit.GetInitState() > SayKitInternal.EInitState.RemoteConfig;
#else
            return true;
#endif
        }

        public string BuildVersion()
        {
#if PR_SAYKIT_ENABLED
            return SayKit.BuildVersion;
#else
            return string.Empty;
#endif
        }

        public static SayKitGameConfig GetConfig()
        {
#if PR_SAYKIT_ENABLED
            return SayKit.gameConfig;
#else
            return new SayKitGameConfig();
#endif
        }

        public void OpenSupportPage()
        {
#if PR_SAYKIT_ENABLED
            SayKit.OpenSupportPage();
#endif
        }
    }
}