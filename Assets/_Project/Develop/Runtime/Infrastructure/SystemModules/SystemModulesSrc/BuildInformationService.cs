using Infrastructure.Configs;
using UnityEditor;

namespace Infrastructure.SystemModules
{

    public class BuildInformationService
    {

        private BuildNumberConfig buildNumberConfig;
        private SdkService sdkService;
        
        public BuildInformationService(SdkService sdkService, ConfigProvider configProvider)
        {
            this.sdkService = sdkService;
            this.buildNumberConfig = configProvider.BuildNumberConfig;
        }


        public string GetBuildInfo()
        {
            return $"{sdkService.BuildVersion()} ({GetBuildNumber()})";
        }
        
        public string GetBuildNumber()
        {
            string result = buildNumberConfig.buildNumber;
#if UNITY_EDITOR
            result = GetBuildNumberFromPlayerSettings();
#endif
            return result;
        }


#if UNITY_EDITOR
        public static void Actualize()
        {
            string[] result = AssetDatabase.FindAssets("BuildNumberConfig", new string[] { "Assets/_Project/Develop/Runtime/Infrastructure/SystemModules/" });
            string path = AssetDatabase.GUIDToAssetPath(result[0]);

            BuildNumberConfig buildNumberConfig = (BuildNumberConfig)AssetDatabase.LoadAssetAtPath(path, typeof(BuildNumberConfig));
            buildNumberConfig.buildNumber = GetBuildNumberFromPlayerSettings();

            EditorUtility.SetDirty(buildNumberConfig);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        private static string GetBuildNumberFromPlayerSettings()
        {
            var result = "wrong build target";
#if UNITY_ANDROID
            result = PlayerSettings.Android.bundleVersionCode.ToString();
#endif

#if UNITY_IPHONE || UNITY_IOS
        result = PlayerSettings.iOS.buildNumber;
#endif

            return result;
        }
#endif

    }
}