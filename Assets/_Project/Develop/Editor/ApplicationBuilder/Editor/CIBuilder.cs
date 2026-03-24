/*using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ApplicationBuilder
{
    public class CIBuilder
    {
        static string globalBuildPostfix = "";

        #region Android

        [MenuItem("Tools/Build Server/1.Build Android RELEASE")]
        public static void PerformBuild_android_release()
        {
            Debug.Log("CI: Build with following PerformBuild_android_release");
            Debug.Log("CI: CIBuilderDefines.ReleaseDefineSymbols=" + CIBuilderDefines.ReleaseDefineSymbols.ToString());
            Debug.Log("CI: CIBuilderDefines.ReplaceReleaseDefineSymbols=" + CIBuilderDefines.ReplaceReleaseDefineSymbols.ToString());
            Debug.Log("CI: CIBuilderDefines.ReleaseDefineSymbolsNeedToRemove=" + CIBuilderDefines.ReleaseDefineSymbolsNeedToRemove.ToString());
            ProjectPreBuilder.PrepareBuild_release();
            
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.allowDebugging = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
#if UNITY_2020_3_OR_NEWER
#pragma warning disable CS0618 // Type or member is obsolete
            EditorUserBuildSettings.androidCreateSymbolsZip = true;
#pragma warning restore CS0618 // Type or member is obsolete
#endif            
            PlayerSettings.Android.useAPKExpansionFiles = false;
            DefineDevelopmentSymbols(BuildTargetGroup.Android, CIBuilderDefines.ReleaseDefineSymbols,
                CIBuilderDefines.ReplaceReleaseDefineSymbols,
                CIBuilderDefines.ReleaseDefineSymbolsNeedToRemove);

#if SAY_BUILDER_FORCE_AAB
            EditorUserBuildSettings.buildAppBundle = true;
            PerformBuild_android(BuildOptions.None, "release", ".aab");
#else 
            EditorUserBuildSettings.buildAppBundle = false;
            PerformBuild_android(BuildOptions.None, "release", ".apk");
#endif

        }



        [MenuItem("Tools/Build Server/2.Build Android DEV")]
        public static void PerformBuild_android_dev()
        {
            Debug.Log("CI: Build with following PerformBuild_android_dev");
            Debug.Log("CI: CIBuilderDefines.DevelopmentDefineSymbols=" + CIBuilderDefines.DevelopmentDefineSymbols.ToString());
            Debug.Log("CI: CIBuilderDefines.ReplaceDevelopmentDefineSymbols=" + CIBuilderDefines.ReplaceDevelopmentDefineSymbols.ToString());
            Debug.Log("CI: CIBuilderDefines.DevelopmentDefineSymbolsNeedToRemove=" + CIBuilderDefines.DevelopmentDefineSymbolsNeedToRemove.ToString());

            ProjectPreBuilder.PrepareBuild_dev();
            EditorUserBuildSettings.allowDebugging = true;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
#if UNITY_2020_3_OR_NEWER
#pragma warning disable CS0618 // Type or member is obsolete
            EditorUserBuildSettings.androidCreateSymbolsZip = true;
#pragma warning restore CS0618 // Type or member is obsolete
#endif            
            PlayerSettings.Android.useAPKExpansionFiles = false;

            EditorUserBuildSettings.development = true;

            DefineDevelopmentSymbols(BuildTargetGroup.Android, CIBuilderDefines.DevelopmentDefineSymbols,
                CIBuilderDefines.ReplaceDevelopmentDefineSymbols,
                CIBuilderDefines.DevelopmentDefineSymbolsNeedToRemove);
                
            EditorUserBuildSettings.allowDebugging = true;
            
            var buildOptions = BuildOptions.Development | BuildOptions.AllowDebugging;

#if SAY_BUILDER_FORCE_AAB
            EditorUserBuildSettings.buildAppBundle = true;
            PerformBuild_android(buildOptions, "dev", ".aab");
#else 
            EditorUserBuildSettings.buildAppBundle = false;
            PerformBuild_android(buildOptions, "dev", ".apk");
#endif                

        }

        [MenuItem("Tools/Build Server/3.Build Android DEBUG")]
        public static void PerformBuild_android_debug()
        {
            Debug.Log("CI: Build with following PerformBuild_android_debug");
            Debug.Log("CI: CIBuilderDefines.DevelopmentDefineSymbols=" + CIBuilderDefines.DevelopmentDefineSymbols.ToString());
            Debug.Log("CI: CIBuilderDefines.ReplaceDevelopmentDefineSymbols=" + CIBuilderDefines.ReplaceDevelopmentDefineSymbols.ToString());
            Debug.Log("CI: CIBuilderDefines.DevelopmentDefineSymbolsNeedToRemove=" + CIBuilderDefines.DevelopmentDefineSymbolsNeedToRemove.ToString());
            
            ProjectPreBuilder.PrepareBuild_debug();
            
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.allowDebugging = true;
            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.buildWithDeepProfilingSupport = true;
            EditorUserBuildSettings.connectProfiler = true;
            EditorUserBuildSettings.waitForManagedDebugger = true;
            
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
#if UNITY_2020_3_OR_NEWER
#pragma warning disable CS0618 // Type or member is obsolete
            EditorUserBuildSettings.androidCreateSymbolsZip = true;
#pragma warning restore CS0618 // Type or member is obsolete
#endif            
            PlayerSettings.Android.useAPKExpansionFiles = false;
            

            DefineDevelopmentSymbols(BuildTargetGroup.Android, CIBuilderDefines.DevelopmentDefineSymbols,
                CIBuilderDefines.ReplaceDevelopmentDefineSymbols,
                CIBuilderDefines.DevelopmentDefineSymbolsNeedToRemove);
            var buildOptions = BuildOptions.Development
                               | BuildOptions.AllowDebugging
                               | BuildOptions.ConnectWithProfiler
                               | BuildOptions.EnableDeepProfilingSupport;
            
            PerformBuild_android(buildOptions, "debug", ".apk");
        }

        public static void OpenFileFolder()
        {
            string path = CIBuilderDefines.PathToOutputFolder();
            EditorUtility.RevealInFinder(path);
        }
        
        [MenuItem("Tools/Build Server/4.Build Android STORE")]
        public static void PerformBuild_android_store()
        {
            Debug.Log("CI: Build with following PerformBuild_android_store");
            Debug.Log("CI: CIBuilderDefines.StoreDefineSymbols=" + CIBuilderDefines.StoreDefineSymbols.ToString());
            Debug.Log("CI: CIBuilderDefines.ReplaceStoretDefineSymbols=" + CIBuilderDefines.ReplaceStoretDefineSymbols.ToString());
            Debug.Log("CI: CIBuilderDefines.StoreDefineSymbolsNeedToRemove=" + CIBuilderDefines.StoreDefineSymbolsNeedToRemove.ToString());

            // TODO: if need something for GP build - do it here
            ProjectPreBuilder.PrepareBuild_store();
            
            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            EditorUserBuildSettings.allowDebugging = false;
            EditorUserBuildSettings.development = false;

#if UNITY_2020_3_OR_NEWER
#pragma warning disable CS0618 // Type or member is obsolete
            EditorUserBuildSettings.androidCreateSymbolsZip = true;
#pragma warning restore CS0618 // Type or member is obsolete
#endif            
            
            PlayerSettings.Android.useAPKExpansionFiles = false;
            DefineDevelopmentSymbols(BuildTargetGroup.Android, CIBuilderDefines.StoreDefineSymbols,
                CIBuilderDefines.ReplaceStoretDefineSymbols, CIBuilderDefines.StoreDefineSymbolsNeedToRemove);
            PerformBuild_android(BuildOptions.None, "store", ".aab");
        }
        
        static void PerformBuild_android(BuildOptions buildOptions, string configuration, string extention)
        {

            AndroidExternalToolsSettings.stopGradleDaemonsOnExit = false;

#if SAY_BUILDER_REIMPORT_RESOURCE    
            string folderPath = Path.Combine("Assets", "Resources"); 
            Debug.Log("CI: begin reimport: " + folderPath);
            AssetDatabase.ImportAsset(folderPath , ImportAssetOptions.ImportRecursive | ImportAssetOptions.DontDownloadFromCacheServer);
            Debug.Log("CI: end reimport: " + folderPath);
#endif 

#if SAY_DISABLE_MTR
            PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, false);
#endif 
            
            PlayerSettings.SplashScreen.showUnityLogo = false;

            var switchResult =
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            if (!switchResult)
            {
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }

            Debug.Log("CI: Build with following keystore: " + CIBuilderDefines.PathToAndroidKeystore);
            if (CIBuilderDefines.PathToAndroidKeystore == null) 
            {
                
                Debug.Log("CI: setup debug keystore");
#if UNITY_2019_1_OR_NEWER
                PlayerSettings.Android.useCustomKeystore = false;
#endif

            } else {
                if (File.Exists(CIBuilderDefines.PathToAndroidKeystore))
                {

                    Debug.Log("CI: setup following keystore: " + CIBuilderDefines.PathToAndroidKeystore);
#if UNITY_2019_1_OR_NEWER
                    PlayerSettings.Android.useCustomKeystore = true;
#endif
                    PlayerSettings.Android.keystoreName = CIBuilderDefines.PathToAndroidKeystore;
                    PlayerSettings.Android.keystorePass = CIBuilderDefines.KeystorePass;
                    PlayerSettings.Android.keyaliasName = CIBuilderDefines.KeyaliasName;
                    PlayerSettings.Android.keyaliasPass = CIBuilderDefines.KeyaliasPass;
                }
                else
                {
                    Debug.LogError($"CI: Can`t find keystore at path {CIBuilderDefines.PathToAndroidKeystore}. Exit.");
                    if (Application.isBatchMode)
                    {
                        EditorApplication.Exit(1);
                    }
                }
            }

            ProcessCommandLineParams(BuildTargetGroup.Android);

#if SAYBUILDER_BUILD_APK_OOB || SAYBUILDER_BUILD_ANDROID_APP_BUNDLE_FORCE
            EditorUserBuildSettings.buildAppBundle = true;
            PlayerSettings.Android.useAPKExpansionFiles = true;
#endif
            

            var outputProject = CIBuilderDefines.ProjectName +
                "-" +
                configuration +
                "-v" +
                PlayerSettings.bundleVersion.Replace('.', '_') +
                "-c" +
                PlayerSettings.Android.bundleVersionCode;

            if (!string.IsNullOrEmpty(globalBuildPostfix)) {
                outputProject += "-" + globalBuildPostfix;
            }
            
            var buildPath = Path.Combine("./", CIBuilderDefines.PathToOutputFolder(),  outputProject + extention);
            Debug.Log("CI: Build with following defines: " +
                      PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android));
            Debug.Log("CI: Build to: " + buildPath);


            BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(GetScenes(), buildPath, BuildTarget.Android, buildOptions | BuildOptions.CompressWithLz4HC);
            if (report.summary.result == BuildResult.Failed)
            {
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
            
            OpenFileFolder();

        }

        #endregion

  
        static void DefineDevelopmentSymbols(BuildTargetGroup btg, string[] defineSymbols, bool replaceCurrent,
            string[] defineSymbolsNeedToRemove)
        {
            Debug.Log("CI: DefineDevelopmentSymbols, replaceCurrent="+replaceCurrent);
            if (defineSymbols != null)
                Debug.Log("CI: defineSymbols=" + string.Join(",", defineSymbols));
            if (defineSymbolsNeedToRemove != null)
                Debug.Log("CI: defineSymbolsNeedToRemove=" + string.Join(",", defineSymbolsNeedToRemove));
            
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
            List<string> allDefines = definesString.Split(';').ToList();
            Debug.Log("CI: DefineDevelopmentSymbols, before add, allDefines=" + string.Join(",", allDefines));
            if (!replaceCurrent)
            {
                allDefines.AddRange(defineSymbols.Except(allDefines));
            }
            else
            {
                allDefines.Clear();
                allDefines.AddRange(defineSymbols);
            }

            Debug.Log("CI: DefineDevelopmentSymbols, after add, allDefines=" + string.Join(",", allDefines));

            if (defineSymbolsNeedToRemove != null)
            {
                foreach (var define in defineSymbolsNeedToRemove)
                {
                    allDefines.Remove(define);
                }
            }

            Debug.Log("CI: DefineDevelopmentSymbols, after remove, allDefines=" + string.Join(",", allDefines));
            // if android platfrom so add upload dsym define
            if (btg == BuildTargetGroup.Android)
            {
                //allDefines.Add("SAYKIT_UPLOAD_ANDROID_SYMB");
                Debug.Log("CI: DefineDevelopmentSymbols, add SAYKIT_UPLOAD_ANDROID_SYMB define, allDefines=" + string.Join(",", allDefines));
            }

            //allDefines.Add("SAY_BUILDER");
            Debug.Log("CI: DefineDevelopmentSymbols, add SAY_BUILDER define, allDefines=" + string.Join(",", allDefines));


            Debug.Log("CI: defines before update: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(btg));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, string.Join(";", allDefines.ToArray()));
            Debug.Log("CI: defines after update: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(btg));
        }


        static string[] GetScenes()
        {
            var projectScenes = EditorBuildSettings.scenes;
            List<string> scenesToBuild = new List<string>();
            for (int i = 0; i < projectScenes.Length; i++)
            {
                if (projectScenes[i].enabled)
                {
                    scenesToBuild.Add(projectScenes[i].path);
                }
            }

            return scenesToBuild.ToArray();
        }

        

        // TODO: Rewrite me, a highly specialized method, think about how to generalize 
        static string ExtractTagAfter(string input, string tag)
        {
            tag = "<key>" + tag + "</key>";
            string pattern = "<(.*)>(.*)</\\1>";
            var matches = Regex.Matches(input, pattern);
            Debug.Log(matches);
            Debug.Log(matches.Count);
            bool takeNext = false;
            string result = null;
            foreach (var match in matches)
            {
                if (takeNext)
                {
                    result = match.ToString();
                    break;
                }
                if (match.ToString() == tag)
                {
                    takeNext = true;
                }
            }

            result = result.Replace("<string>", "");
            result = result.Replace("</string>", "");

            return result;
        }

        static void SetupInfoFromProvisionProfile(string pathToProvisionProfile)
        {
            if (!File.Exists(pathToProvisionProfile))
            {
                Debug.LogError($"CI: Can not find provision profile file at {pathToProvisionProfile}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }

            }

            var profileText = File.ReadAllText(pathToProvisionProfile);
            var UUID = ExtractTagAfter(profileText, "UUID");
            Debug.Log("CI: setup follow UUID: " + UUID);
            var teamID = ExtractTagAfter(profileText, "TeamIdentifier");
            Debug.Log("CI: setup follow Team ID: " + teamID);
            
            PlayerSettings.iOS.appleDeveloperTeamID = teamID;
            PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Development;
            PlayerSettings.iOS.iOSManualProvisioningProfileID = UUID;

            // string pattern = "<key>TeamIdentifier</key>*<array>*<string>[0-9]+<</string>*</array>";
        }
        
        //[MenuItem("Tools/Build Server/Fix Project Settings/Android")]
        static void FixProjectSettings_android()
        {
            DefineDevelopmentSymbols(BuildTargetGroup.Android, CIBuilderDefines.DevelopmentDefineSymbols,
                CIBuilderDefines.ReplaceDevelopmentDefineSymbols,
                CIBuilderDefines.DevelopmentDefineSymbolsNeedToRemove);            
            ProcessCommandLineParams(BuildTargetGroup.Android);
        }        



        private static string CMD_LINE_ARG_VERSION = "version=";
        private static string CMD_LINE_ARG_BUNDLE = "bundle-id=";
        private static string CMD_LINE_ARG_VERSION_CODE = "version-code=";
        private static string CMD_LINE_ARG_DEFINE_ADD = "defines-add=";
        private static string CMD_LINE_ARG_DEFINE_REPLACE = "defines-replace=";

        private static string CMD_LINE_ARG_BUILD_POSTFIX = "build-postfix=";
        private static string CMD_LINE_ARG_IPHONEOS_DEPLOYMENT_TARGET = "ios-deployment-target=";
        
        private static string CMD_LINE_ARG_PROVISION_PROFILE = "mobileprovision=";
        private static string CMD_TARGET_SDK_VERSION = "target-sdk-version=";
        private static string CMD_MIN_SDK_VERSION = "min-sdk-version=";
        private static string CMD_PATH_TO_GRADLE = "path-to-gradle=";
        private static string CMD_PATH_TO_JDK = "path-to-jdk=";

        

        static void ProcessCommandLineParams(BuildTargetGroup buildTargetGroup)
        {
            string[] args = System.Environment.GetCommandLineArgs();

            Debug.Log(args.Length);
            Debug.Log(args);
            

            foreach (var arg in args)
            {
                Debug.Log("|" + arg + "|");
            }

            foreach (var arg in args)
            {
                if (arg.StartsWith(CMD_LINE_ARG_PROVISION_PROFILE))
                {
                    var pathToProvisionProfile = arg.Split('=')[1];

                    if (!File.Exists(pathToProvisionProfile))
                    {
                        Debug.LogError($"CI: Can not find provision profile file at {pathToProvisionProfile}");
                        if (Application.isBatchMode)
                        {
                            EditorApplication.Exit(1);
                        }
                    } 
                    
                    Debug.Log($"CI: Apply following provision: {pathToProvisionProfile}");                    
                    SetupInfoFromProvisionProfile(pathToProvisionProfile);

                }                  

                if (arg.StartsWith(CMD_LINE_ARG_IPHONEOS_DEPLOYMENT_TARGET))
                {
                    var target = arg.Split('=')[1];

                    if (!string.IsNullOrEmpty(target))
                    {
                        Debug.Log($"CI: current targetOSVersionString = {PlayerSettings.iOS.targetOSVersionString}");
                        Debug.Log($"CI: need targetOSVersionString = {target}");

                        double current = -1.0;
                        double replace = -1.0;

                        double.TryParse(PlayerSettings.iOS.targetOSVersionString.Trim(), NumberStyles.Any,
                            CultureInfo.InvariantCulture, out current);
                        double.TryParse(target.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out replace);

                        Debug.Log($"CI: current target as float = {current}");
                        Debug.Log($"CI: need target as float {replace}");

                        target = replace.ToString("0.0");
                        Debug.Log($"CI: target as float {replace}");

                        if (current < replace)
                        {
                            Debug.Log(
                                $"CI: replace targetOSVersionString from {PlayerSettings.iOS.targetOSVersionString} to {target}");
                            PlayerSettings.iOS.targetOSVersionString = target;
                        } else {
                            Debug.Log($"CI: replace targetOSVersionString - NOT need, current version > passed version");
                        }
                    }
                }  

                if (arg.StartsWith(CMD_LINE_ARG_DEFINE_ADD))
                {
                    var defines = arg.Split('=')[1];
                    if (!string.IsNullOrEmpty(defines))
                    {
                        Debug.Log($"CI: add following defines {defines}");
                        var arrDefines = defines.Split(';');
                        DefineDevelopmentSymbols(buildTargetGroup, arrDefines, false, null);
                    }
                }  

                if (arg.StartsWith(CMD_LINE_ARG_BUILD_POSTFIX))
                {
                    var postfix = arg.Split('=')[1];
                    if (!string.IsNullOrEmpty(postfix))
                    {
                        Debug.Log($"CI: add following build postfix {postfix}");

                        globalBuildPostfix = postfix;
                    }
                }                              
                
                if (arg.StartsWith(CMD_LINE_ARG_DEFINE_REPLACE))
                {
                    var defines = arg.Split('=')[1];
                    if (!string.IsNullOrEmpty(defines))
                    {
                        Debug.Log($"CI: add following defines {defines}");
                        var arrDefines = defines.Split(';');
                        DefineDevelopmentSymbols(buildTargetGroup, arrDefines, true, null);
                    }
                }                  
                
                if (arg.StartsWith(CMD_LINE_ARG_VERSION))
                {
                    var version = arg.Split('=')[1];
                    if (!string.IsNullOrEmpty(version))
                    {
                        Debug.Log($"CI: replace version from {PlayerSettings.bundleVersion} to {version}");
                        PlayerSettings.bundleVersion = version;
                    }
                }

                if (arg.StartsWith(CMD_LINE_ARG_BUNDLE))
                {
                    var bundle = arg.Split('=')[1];
                    if (!string.IsNullOrEmpty(bundle))
                    {
                        Debug.Log($"CI: replace bundle id from {PlayerSettings.GetApplicationIdentifier(buildTargetGroup)} to {bundle}");
                        PlayerSettings.SetApplicationIdentifier(buildTargetGroup, bundle);
                    }
                }
                
                if (arg.StartsWith(CMD_TARGET_SDK_VERSION))
                {
                    var version = arg.Split('=')[1];
                    if (!string.IsNullOrEmpty(version))
                    {
                        var newTargetSDKVersion = (AndroidSdkVersions)int.Parse(version);
                        Debug.Log($"CI: replace android target sdk version from {PlayerSettings.Android.targetSdkVersion} to {version}");
                        PlayerSettings.Android.targetSdkVersion = newTargetSDKVersion;
                    }
                }

                if (arg.StartsWith(CMD_MIN_SDK_VERSION))
                {
                    var version = arg.Split('=')[1];
                    if (!string.IsNullOrEmpty(version))
                    {
                        var newMinSDKVersion = (AndroidSdkVersions)int.Parse(version);
                        Debug.Log($"CI: replace android min sdk version from {PlayerSettings.Android.minSdkVersion} to {version}");
                        PlayerSettings.Android.minSdkVersion = newMinSDKVersion;
                    }
                }
                
#if UNITY_2019_4_OR_NEWER
                if (arg.StartsWith(CMD_PATH_TO_GRADLE))
                {
                    var path = arg.Split('=')[1];
                    if (!string.IsNullOrEmpty(path))
                    {
                        AndroidExternalToolsSettings.gradlePath = path;
                        AndroidExternalToolsSettings.stopGradleDaemonsOnExit = false;
                    } else {
                        AndroidExternalToolsSettings.gradlePath = null;
                        AndroidExternalToolsSettings.stopGradleDaemonsOnExit = false;
                    }
                }
                if (arg.StartsWith(CMD_PATH_TO_JDK))
                {
                    var path = arg.Split('=')[1];
                    if (!string.IsNullOrEmpty(path))
                    {
                        AndroidExternalToolsSettings.jdkRootPath = path;
                    } else {
                        AndroidExternalToolsSettings.jdkRootPath = null;
                    }
                }                
#endif                

                if (arg.StartsWith(CMD_LINE_ARG_VERSION_CODE))
                {
                    var version_code = arg.Split('=')[1];

                    if (!string.IsNullOrEmpty(version_code))
                    {
                        if (buildTargetGroup == BuildTargetGroup.Android)
                        {
                            Debug.Log(
                                $"CI: replace bundle version code from {PlayerSettings.Android.bundleVersionCode} to {version_code}");
                            PlayerSettings.Android.bundleVersionCode = Int32.Parse(version_code);
                        }

                        if (buildTargetGroup == BuildTargetGroup.iOS)
                        {
                            Debug.Log(
                                $"CI: replace build number from {PlayerSettings.iOS.buildNumber} to {version_code}");
                            PlayerSettings.iOS.buildNumber = version_code;
                        }
                    }
                }
            }
        }
    }
}*/