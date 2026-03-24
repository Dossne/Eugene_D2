using UnityEngine;

namespace ApplicationBuilder
{
    public class CIBuilderDefines
    {
        public static readonly string ProjectId = "dev-project";

        public static readonly string ProjectName = ProjectId +"-v" + Application.unityVersion;
        public static readonly string ProjectNameConfigPath = ProjectId;

        private static readonly string PathToConfigFolder =
            Application.dataPath + "/_Project/Develop/Editor/ApplicationBuilder/Editor/projects/" + ProjectNameConfigPath + "/configs/";

        public static readonly string PathToAndroidKeystore = PathToConfigFolder + "android.keystore";
        
        public static readonly string KeystorePass = "WhalerDev";
        public static readonly string KeyaliasName = "whaler";
        public static readonly string KeyaliasPass = "WhalerDev";        

        // Система сболрки по умолчанию добавляет дефайны ниже к тем которые прописаны в проекте, если вы хотите заменять из, то измените значение переменно
        public static readonly bool ReplaceDevelopmentDefineSymbols = false;

        // Тут нужно описать все дефайны которые нужны вам для дев билда - включение читов и т.п.
        public static readonly string[] DevelopmentDefineSymbols = new string[]
        {
            // keep this define, its allow to check it was store build or not
            //"SAY_BUILDER_DEV",
            // keep this define, this define prevent show dialogs from SayKit, instead SayKit log error and stop build process if something wrong 
            //"SAYKIT_AUTOBUILD",
            "PR_CHEAT"
        };

        public static readonly bool ReplaceReleaseDefineSymbols = false;

        public static readonly string[] ReleaseDefineSymbols = new string[]
        {
            // keep this define, its allow to check it was release build or not 
           // "SAY_BUILDER_RELEASE",
            // keep this define, this define prevent show dialogs from SayKit, instead SayKit log error and stop build process if something wrong 
            //"SAYKIT_AUTOBUILD",
            "PR_CHEAT"
        };

        public static readonly bool ReplaceStoretDefineSymbols = false;

        public static readonly string[] StoreDefineSymbols = new string[]
        {
            // keep this define, its allow to check it was store build or not
           // "SAY_BUILDER_STORE",
            // keep this define, this define prevent show dialogs from SayKit, instead SayKit log error and stop build process if something wrong
            //"SAYKIT_AUTOBUILD"
        };

        public static readonly string[] DevelopmentDefineSymbolsNeedToRemove = new string[]
        {
            //"SAY_BUILDER_RELEASE",
            //"SAY_BUILDER_STORE",
        };


        public static readonly string[] ReleaseDefineSymbolsNeedToRemove = new string[]
        {
            //"SAY_BUILDER_DEV",
            //"SAY_BUILDER_STORE",
        };


        public static readonly string[] StoreDefineSymbolsNeedToRemove = new string[]
        {
            //"SAY_BUILDER_DEV",
            //"SAY_BUILDER_RELEASE",
            "PR_CHEAT"
        };

        public static string PathToOutputFolder()
        {
#if UNITY_IOS
            return "../Builds.ios";
#elif UNITY_ANDROID
            return "../Builds.android";
#else
            return "../Builds.other";
#endif
        }
    }
}