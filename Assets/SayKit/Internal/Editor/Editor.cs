#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable TooWideLocalVariableScope
// ReSharper disable RedundantAssignment
// ReSharper disable MemberCanBePrivate.Global

#endregion

namespace SayKitInternal
{
    public class Editor
    {
        private static readonly string SAYKIT_PURCHASING_SYMBOL = "SAYKIT_PURCHASING";
        private static readonly string SAYKIT_CHINA_VERSION_SYMBOL = "SAYKIT_CHINA_VERSION";
        private const string SGS_PACKAGE_NAME = "io.saygames.services";
        private static ListRequest _request;
        private static string _sayGamesServicesVersion;
        private const string DIALOG_TITLE = "SayKit Message";

        [MenuItem("SayKit/Check Release build")]
        private static void EmbedConfig()
        {
            var pb = new SayKitPreBuild();
            SayKitPreBuild.buildPlace = "check_release_build";

            pb.OnPreprocessBuild(null, true);
            if (!pb.dirtyFlag)
            {
                EditorUtility.DisplayDialog(DIALOG_TITLE, "Finished without errors!\n\nᕦ( ͡° ͜ʖ ͡°)ᕤ", "Awesome!");
            }
        }

        [MenuItem("SayKit/[iOS] Application config/World config")]
        public static void WorldConfig()
        {
            if (!CheckChangeApplicationConfigSetupCorrectly())
            {
                return;
            }

            AddSayKitPurchaseDefineIfNeeded(SAYKIT_PURCHASING_SYMBOL);
            DeleteSayKitPurchaseDefineIfNeeded(SAYKIT_CHINA_VERSION_SYMBOL);

            PlayerSettings.applicationIdentifier = SayKitApp.APP_BUNDLE_IOS;
            PlayerSettings.productName = SayKitApp.APP_NAME_IOS;


            Menu.SetChecked("SayKit/Application config/World config", true);
            Menu.SetChecked("SayKit/Application config/China config", false);

            if (!Application.isBatchMode) EmbedConfig(); // hotfixed issues in batchmode
        }

        [MenuItem("SayKit/[iOS] Application config/China config")]
        public static void ChinaConfig()
        {
            if (!CheckChangeApplicationConfigSetupCorrectly())
            {
                return;
            }

            AddSayKitPurchaseDefineIfNeeded(SAYKIT_CHINA_VERSION_SYMBOL);
            DeleteSayKitPurchaseDefineIfNeeded(SAYKIT_PURCHASING_SYMBOL);

            PlayerSettings.applicationIdentifier = SayKitApp.APP_BUNDLE_CHINA_IOS;
            PlayerSettings.productName = SayKitApp.APP_NAME_CHINA_IOS;


            Menu.SetChecked("SayKit/Application config/World config", false);
            Menu.SetChecked("SayKit/Application config/China config", true);

            if (!Application.isBatchMode) EmbedConfig(); // hotfixed issues in batchmode
        }

        private static AddRequest _nugetPackageRequest;

        [MenuItem("SayKit/Package Manager/Add JSON package")]
        public static void AddNugetPackage()
        {
            _nugetPackageRequest = UnityEditor.PackageManager.Client.Add("com.unity.nuget.newtonsoft-json");
            EditorApplication.update += NugetPackageLoadProgress;
        }

        [MenuItem("SayKit/Package Manager/Add UniTask package")]
        public static void AddUniTaskPackage()
        {
            _nugetPackageRequest = UnityEditor.PackageManager.Client.Add("com.cysharp.unitask");
            EditorApplication.update += NugetPackageLoadProgress;
        }

        [MenuItem("SayKit/SayKit Version")]
        private static void ShowSayKitVersion()
        {
            _request = UnityEditor.PackageManager.Client.List(true);
            EditorApplication.update += HandleVersionCallback;
        }

        [MenuItem("SayKit/SGS Version Manager")]
        private static void ShowSGSVersionManager()
        {
            SGSVersionManager.ShowGSGVersionManager();
        }

        private static void NugetPackageLoadProgress()
        {
            if (_nugetPackageRequest.IsCompleted)
            {
                if (_nugetPackageRequest.Status == StatusCode.Success)
                {
                    Debug.Log("SayKit: Installed: " + _nugetPackageRequest.Result.packageId);
                }
                else if (_nugetPackageRequest.Status >= StatusCode.Failure)
                {
                    Debug.Log(_nugetPackageRequest.Error.message);
                }

                EditorApplication.update -= NugetPackageLoadProgress;
            }
        }

        private static bool CheckChangeApplicationConfigSetupCorrectly()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                EditorUtility.DisplayDialog(DIALOG_TITLE, "You have to choose iOS platform to change the config.\n \n" +
                                                          "Note: It needs to switch configuration between iOS and iOS China builds. " +
                                                          "\nPlease, read the README file first.",
                    "OK");
                return false;
            }

            if (string.IsNullOrEmpty(SayKitApp.APP_NAME_IOS)
                || string.IsNullOrEmpty(SayKitApp.APP_NAME_CHINA_IOS)
                || SayKitApp.APP_NAME_IOS.Contains("APP_NAME_IOS")
                || SayKitApp.APP_NAME_CHINA_IOS.Contains("APP_NAME_CHINA_IOS"))
            {
                EditorUtility.DisplayDialog(DIALOG_TITLE,
                    "You have to setup APP_NAME_IOS and APP_NAME_CHINA_IOS in the SayKitApp class.\n \n" +
                    "Note: It needs to switch configuration between iOS and iOS China builds. " +
                    "\nPlease, read the README file first.",
                    "OK");
                return false;
            }

            return true;
        }

        private static void AddSayKitPurchaseDefineIfNeeded(string symbol)
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            if (defines.Contains(symbol))
            {
                Debug.LogWarning("Selected build target (" + EditorUserBuildSettings.activeBuildTarget +
                                 ") already contains <b>" + symbol + "</b> <i>Scripting Define Symbol</i>.");
                return;
            }

            SetScriptingDefineSymbolsForGroup(buildTargetGroup, (defines + ";" + symbol));
            Debug.LogWarning("<b>" + symbol +
                             "</b> added to <i>Scripting Define Symbols</i> for selected build target (" +
                             EditorUserBuildSettings.activeBuildTarget + ").");
        }

        private static void DeleteSayKitPurchaseDefineIfNeeded(string symbol)
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            var newDefines = "";

            if (defines.Contains(symbol))
            {
                int startSymbol = defines.IndexOf(";" + symbol, StringComparison.Ordinal);
                if (startSymbol < 0)
                {
                    startSymbol = defines.IndexOf(symbol, StringComparison.Ordinal);
                }

                int endSymbol = startSymbol + symbol.Length + 1;

                if (endSymbol < defines.Length)
                {
                    newDefines = defines.Substring(0, startSymbol) +
                                 defines.Substring(endSymbol, defines.Length - endSymbol);
                }
                else
                {
                    newDefines = defines.Substring(0, startSymbol);
                }


                SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
                Debug.LogWarning("<b>" + symbol +
                                 "</b> deleted from <i>Scripting Define Symbols</i> for selected build target (" +
                                 EditorUserBuildSettings.activeBuildTarget.ToString() + ").");
            }
            else
            {
                Debug.LogWarning("Selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() +
                                 ") does not contain <b>" + symbol + "</b> <i>Scripting Define Symbol</i>.");
            }
        }

        private static void HandleVersionCallback()
        {
            if (_request is { IsCompleted: true })
            {
                EditorApplication.update -= HandleVersionCallback;

                if (_request.Status == StatusCode.Success)
                {
                    foreach (var package in _request.Result)
                    {
                        if (package.name.Equals(SGS_PACKAGE_NAME))
                        {
                            _sayGamesServicesVersion = package.version;
                        }
                    }

                    if (string.IsNullOrEmpty(_sayGamesServicesVersion))
                    {
                        EditorUtility.DisplayDialog("SayKit",
                            "" + SKManager.Instance.Version,
                            "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("SayKit",
                            "SayKit version: " + SKManager.Instance.Version + "\n" +
                            "SayGamesServices version: " + _sayGamesServicesVersion,
                            "OK");
                    }
                }
            }
        }

        internal static string GetScriptingDefineSymbolsForGroup(BuildTargetGroup targetGroup) =>
#if UNITY_6000_0_OR_NEWER
            PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup));
#else
            PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
#endif

        internal static void SetScriptingDefineSymbolsForGroup(BuildTargetGroup targetGroup, string defines) =>
#if UNITY_6000_0_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup), defines);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
#endif

        internal static ScriptingImplementation GetScriptingBackend(BuildTargetGroup targetGroup) =>
#if UNITY_6000_0_OR_NEWER
            PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup));
#else
            PlayerSettings.GetScriptingBackend(targetGroup);
#endif
    }
}
#endif