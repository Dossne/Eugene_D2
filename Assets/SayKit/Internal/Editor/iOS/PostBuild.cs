#if UNITY_EDITOR && UNITY_IOS
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Debug = UnityEngine.Debug;

// ReSharper disable once CheckNamespace
namespace SayKitInternal
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class SayKitPostBuild
    {
        private static readonly Dictionary<string, string> _userTrackingDescriptions = new Dictionary<string, string>
        {
            { "en", "This will only be used to serve more relevant ads" },
            { "zh-Hans", "这只会用于服务更相关的广告" },
            { "zh-Hant", "这只会用于服务更相关的广告" },
            { "zh-HK", "这只会用于服务更相关的广告" },

            { "ru", "Эта информация будет использоваться только для отображения интересующей вас рекламы" },
            { "de", "Wird nur verwendet, um Werbung relevanter zu gestalten" },
            { "fr", "Ne faremo uso solo per mostrarti annunci più pertinenti" },
            { "es", "Solo se utilizará para mostrarte anuncios más relevantes." },
            { "es-419", "Solo se utilizará para mostrarte anuncios más relevantes." },

            { "it", "Ne faremo uso solo per mostrarti annunci più pertinenti" },
            { "pt-BR", "Isto será usado somente para apresentar anúncios mais relevantes" },
            { "pt-PT", "Isto será usado somente para apresentar anúncios mais relevantes" },
            { "ja", "この情報は、お客様に最適な広告を 配信する目的で使われます。" },
            { "ko", "보다 관련성 높은 광고를 제공하는 데만 사용됩니다" },
        };

        private static readonly string[] _localizations =
        {
            "en",
            "zh-Hans",
            "zh-Hant",
            "zh-HK",
            "ru",
            "de",
            "fr",
            "es",
            "es-419",
            "it",
            "pt-BR",
            "pt-PT",
            "ja",
            "ko",
            "ar",
            "hi", 
            "uk", 
            "pl",
            "tr",
            "id", 
            "th",
            "vi" 
        };

        private static readonly string[] _chinaLocalizations =
        {
            "zh-Hans",
            "zh-Hant",
            "zh-HK"
        };

#if SAYKIT_SHIFT_IOS_POSTPROCESS_ORDER
        [PostProcessBuild(int.MaxValue - 1)]
#else
        [PostProcessBuild(int.MaxValue)]
#endif
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuildProject)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }
            
            SayKitDebug.Log("SayKit: Updating Info.plist");

            var plistPath = Path.Combine(pathToBuildProject, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            //Get Root
            var rootDict = plist.root;

            AddQueriesSchemes(rootDict);
            RegisterURLSchemes(rootDict);
            SetPlistKeys(rootDict);
            InsertSkAdNetworkIds(rootDict);

            File.WriteAllText(plistPath, plist.WriteToString());

            // XCode project
            SayKitDebug.Log("SayKit: Updating Xcode project");

            var projPath = Path.Combine(pathToBuildProject, "Unity-iPhone.xcodeproj/project.pbxproj");
            var project = new PBXProject();
            var projectFile = File.ReadAllText(projPath);

            project.ReadFromString(projectFile);

            var projTarget = project.GetUnityFrameworkTargetGuid();
            var mainTarget = project.GetUnityMainTargetGuid();

            SetBuildProperty(project, projTarget, mainTarget);

            AddRequiredFrameworks(project, projTarget);
            AddRequiredAdFrameworks(project, projTarget);

#if SAYKIT_CLOUD_BUILD
            project.RemoveFrameworkFromProject(projTarget, "StoreKit.framework");
#endif

#if UNITY_IOS && SAYKIT_IOS_GAME_CENTER && !SAYKIT_CLOUD_BUILD
            AddGameCenter(pathToBuildProject, project);
#endif

#if UNITY_IOS && SAYKIT_NOTIFICATIONS && !SAYKIT_CLOUD_BUILD
            AddPushNotificationCapability(pathToBuildProject, project);
#endif

#if UNITY_IOS && SAYKIT_DEEP_LINK && !SAYKIT_CLOUD_BUILD
            AddAssociatedDomains(pathToBuildProject, project);
#endif

            CommentRowsInUnityCleanupTrampoline(pathToBuildProject);
            AddRequiredLibs(project, projTarget);

            UploadFirebaseSymbols(project, projTarget, mainTarget, pathToBuildProject);
            
#if !UNITY_EDITOR_WIN
            AddXcodeVersionScript(project, projTarget, pathToBuildProject);
#endif

            SwiftCheck(project, mainTarget);

            AddFileToUnityMainTarget(project, pathToBuildProject, "saykit_attribution_settings.json");
            AddFileToUnityMainTarget(project, pathToBuildProject, "saykit_" + SKUtils.GetAppKey() + "_" + Application.version + ".json");
            AddFileToUnityMainTarget(project, pathToBuildProject, "saykit_versions.json");
            AddFileToUnityMainTarget(project, pathToBuildProject, "saykit_iap.json");
            AddFileToUnityMainTarget(project, pathToBuildProject, "saykit_platform_defines.txt");

            AddLocalizationsToUnityMainTarget(project, pathToBuildProject);


            File.WriteAllText(projPath, project.WriteToString());

            CopyGooglePlistFile(pathToBuildProject);
            CheckXCodeProjectSettings(pathToBuildProject);
            RemoveMetaFiles(pathToBuildProject);
            RenameMRAIDSource(pathToBuildProject);
            AddPrivacyManifest(pathToBuildProject);

#if !SAYKIT_PLIST_LOCALIZATION_DISABLE
            LocalizeInfoPlist(pathToBuildProject);
#endif
            RemoveMnoThumbFlag(pathToBuildProject);
        }

        private static void AddQueriesSchemes(PlistElementDict rootDict)
        {
            var lsApplicationQueriesSchemes = rootDict.CreateArray("LSApplicationQueriesSchemes");
            lsApplicationQueriesSchemes.AddString("fb");
            lsApplicationQueriesSchemes.AddString("instagram");

            var queriesSchemes = new List<string>();
            queriesSchemes.Add("fb412266819304521");
            queriesSchemes.Add("fb2326383180965488");

            for (var i = 17; i <= 60; i++)
            {
                queriesSchemes.Add("saygames" + i);
            }

            foreach (var scheme in queriesSchemes.Where(scheme => SayKitApp.PROMO_KEY_IOS != scheme))
            {
                lsApplicationQueriesSchemes.AddString(scheme);
            }
        }

        private static void RegisterURLSchemes(PlistElementDict rootDict)
        {
            var urlSchemes = new List<string>
            {
                "fb" + SayKitRemoteSettings.Instance.FacebookAppID
            };
            
            urlSchemes.Add($"{SKUtils.GetAppKey()}");

            if (!string.IsNullOrEmpty(SayKitApp.PROMO_KEY_IOS))
            {
                urlSchemes.Add(SayKitApp.PROMO_KEY_IOS);
            }

            PlistElementArray urlTypesArray;

            if (rootDict.values.TryGetValue("CFBundleURLTypes", out var urlTypesElement) &&
                urlTypesElement is PlistElementArray existingArray)
            {
                urlTypesArray = existingArray;
            }
            else
            {
                urlTypesArray = rootDict.CreateArray("CFBundleURLTypes");
            }

            foreach (var scheme in urlSchemes)
            {
                var dict = urlTypesArray.AddDict();
                var schemesArray = new PlistElementArray();
                schemesArray.AddString(scheme);
                dict["CFBundleURLSchemes"] = schemesArray;
            }
        }

        private static void SetPlistKeys(PlistElementDict rootDict)
        {
            rootDict.SetString("FacebookAppID", SayKitRemoteSettings.Instance.FacebookAppID);
            rootDict.SetString("FacebookDisplayName", SayKitRemoteSettings.Instance.FacebookAppName);
            rootDict.SetString("FacebookClientToken", SayKitRemoteSettings.Instance.FacebookClientToken);
            rootDict.SetBoolean("FacebookAdvertiserIDCollectionEnabled", true);
            
#if SAYKIT_DISABLE_FACEBOOK_AUTOLOG
            rootDict.SetBoolean("FacebookAutoLogAppEventsEnabled", false);   
            rootDict.SetBoolean("SKIncludeConsumableInAppPurchaseHistory", false); 
#else
            rootDict.SetBoolean("FacebookAutoLogAppEventsEnabled", true);   
            rootDict.SetBoolean("SKIncludeConsumableInAppPurchaseHistory", true);   
#endif
           
            if (!string.IsNullOrEmpty(SayKitRemoteSettings.Instance.StoreId))
            {
                rootDict.SetString("sk_appstore_id", SayKitRemoteSettings.Instance.StoreId);
            }

            var NSAppTransportSecurity = rootDict.CreateDict("NSAppTransportSecurity");
            NSAppTransportSecurity.SetBoolean("NSAllowsArbitraryLoads", true);

            rootDict.SetString("NSCameraUsageDescription", "This app does not use the camera.");
            rootDict.SetString("NSCalendarsUsageDescription", "This app does not use the calendar.");
            rootDict.SetString("NSPhotoLibraryUsageDescription", "This app does not use the photo library.");
            rootDict.SetString("NSMotionUsageDescription", "This app does not use the accelerometer.");
            rootDict.SetString("NSLocationAlwaysUsageDescription", "This app does not use the location.");
            rootDict.SetString("NSLocationWhenInUseUsageDescription", "This app does not use the location.");
            rootDict.SetString("NSLocationAlwaysAndWhenInUseUsageDescription", "This app does not use the location.");
            rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://tracksaygames.io");

#if SAYKIT_CHINA_VERSION
        rootDict.SetString("NSUserTrackingUsageDescription", "这只会用于服务更相关的广告");
#else
            rootDict.SetString("NSUserTrackingUsageDescription", "This will only be used to serve more relevant ads.");
#endif

            rootDict.SetBoolean("GADIsAdManagerApp", true);
            rootDict.SetString("GADApplicationIdentifier", SayKitRemoteSettings.Instance.AdmobAppId);
            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            // Remote Notifications
            if (SayKitApp.notificationsEnabled)
            {
#pragma warning disable CS0162 // Unreachable code detected
                var UIBackgroundModes = rootDict.CreateArray("UIBackgroundModes");
                UIBackgroundModes.AddString("remote-notification");
#pragma warning restore CS0162 // Unreachable code detected
            }

            // Remove "UIApplicationExitsOnSuspend" flag.
            var exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
            if (rootDict.values.ContainsKey(exitsOnSuspendKey))
            {
                rootDict.values.Remove(exitsOnSuspendKey);
            }
            
            rootDict.SetString("SayKitVersion", SKManager.Instance.Version.ToString());
            rootDict.SetString("SayKitAppKey", SKUtils.GetAppKey());
            rootDict.SetString("LSMinimumSystemVersion", "15.0");
        }

        private static void SetBuildProperty(PBXProject project, string projTarget, string mainTarget)
        {
            project.SetBuildProperty(projTarget, "EMBEDDED_CONTENT_CONTAINS_SWIFT", "NO");
            project.SetBuildProperty(mainTarget, "EMBEDDED_CONTENT_CONTAINS_SWIFT", "YES");
            project.SetBuildProperty(projTarget, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");
            project.AddBuildProperty(projTarget, "OTHER_LDFLAGS", "-ObjC");
            project.SetBuildProperty(projTarget, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
            project.SetBuildProperty(projTarget, "CLANG_ENABLE_MODULES", "YES");

            var swiftVersion = project.GetBuildPropertyForAnyConfig(projTarget, "SWIFT_VERSION");
            if (string.IsNullOrEmpty(swiftVersion) || !swiftVersion.Equals("5.1"))
            {
                project.SetBuildProperty(projTarget, "SWIFT_VERSION", "5.1");
            }

            project.AddFrameworkToProject(project.GetUnityMainTargetGuid(), "UnityFramework.framework", false);
            project.SetBuildProperty(mainTarget, "ENABLE_BITCODE", "NO");
            project.SetBuildProperty(projTarget, "ENABLE_BITCODE", "NO");
        }

        private static void RemoveMnoThumbFlag(string pathToBuiltProject)
        {
#if UNITY_2020
            var pbxProjectPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";

            if (File.Exists(pbxProjectPath))
            {
                if (SKUtils.CheckXcodeVersion16())
                {
                    var pbxProjectContents = File.ReadAllText(pbxProjectPath);
                    var updatedContents = pbxProjectContents.Replace("-mno-thumb", "");
                    File.WriteAllText(pbxProjectPath, updatedContents);
                }
            }
#endif
        }

        private static void AddRequiredFrameworks(PBXProject project, string projTarget)
        {
            // Required Frameworks
            var frameworks = new[]
            {
                "Accelerate.framework",
                "AVFoundation.framework",
                "CoreGraphics.framework",
                "CoreLocation.framework",
                "CoreMedia.framework",
                "CoreTelephony.framework",
                "Foundation.framework",
                "MediaPlayer.framework",
                "MessageUI.framework",
                "QuartzCore.framework",
                "SafariServices.framework",
                "SystemConfiguration.framework",
                "UIKit.framework",
                "WebKit.framework",
                "MobileCoreServices.framework",
                "Photos.framework",
                "VideoToolbox.framework"
            };

            foreach (var framework in frameworks)
            {
                if (!project.ContainsFramework(projTarget, framework))
                {
                    project.AddFrameworkToProject(projTarget, framework, false);
                }
            }
        }

        private static void AddRequiredAdFrameworks(PBXProject project, string projTarget)
        {
            // Required Ad Frameworks
            var adFrameworks = new[]
            {
                "AdSupport.framework",
                "StoreKit.framework",
                "AdServices.framework",
                "AppTrackingTransparency.framework"
            };

            foreach (var framework in adFrameworks)
            {
                if (!project.ContainsFramework(projTarget, framework))
                {
                    project.AddFrameworkToProject(projTarget, framework, true);
                    project.AddFrameworkToProject(project.GetUnityMainTargetGuid(), framework, true);
                }
            }
        }

        private static void AddRequiredLibs(PBXProject project, string projTarget)
        {
            var libs = new[]
            {
                "libresolv.9.tbd",
                "libc++.tbd",
                "libz.tbd",
                "libbz2.tbd",
                "libz.dylib",
                "libsqlite3.dylib",
                "libxml2.dylib"
            };

            foreach (var lib in libs)
            {
                SayKitDebug.Log("SayKit: Adding " + lib + " to Xcode project");

                var libGuid = project.AddFile("usr/lib/" + lib, "Libraries/" + lib, PBXSourceTree.Sdk);
                project.AddFileToBuild(projTarget, libGuid);
            }
        }

        private static void AddXcodeVersionScript(PBXProject project, string mainTarget, string pathToBuildProject)
        {
            if (project.GetShellScriptBuildPhaseForTarget(mainTarget, "Xcode version", "/bin/sh", "\"$PROJECT_DIR/xcode_version.sh\"") == null)
            {
                project.AddShellScriptBuildPhase(mainTarget, "Xcode version", "/bin/sh", "\"$PROJECT_DIR/xcode_version.sh\"");
            }

            var xcodeVersionPath = Path.Combine(Application.dataPath, "SayKit/Internal/Plugins/iOS/SayKit/xcode_version.sh");
            var destinationPath = Path.Combine(pathToBuildProject, "xcode_version.sh");

            if (File.Exists(xcodeVersionPath))
            {
                File.Copy(xcodeVersionPath, destinationPath, true);

                if (File.Exists(destinationPath))
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "/bin/bash",
                            Arguments = $"-c \"chmod +x '{destinationPath}'\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();
                    process.WaitForExit();
                }
            }
        }

        private static void UploadFirebaseSymbols(PBXProject project, string projTarget, string mainTarget,
            string pathToBuildProject)
        {
#if !SAYKIT_UPLOAD_SYMB_DISABLE
            if (project.GetShellScriptBuildPhaseForTarget(projTarget, "Firebase Crashlytics", "/bin/sh",
                    "\"${PROJECT_DIR}/firebase-run\" --debug") == null)
            {
                project.AddShellScriptBuildPhase(projTarget, "Firebase Crashlytics", "/bin/sh",
                    "\"${PROJECT_DIR}/firebase-run\"  --debug");
            }

            if (project.GetShellScriptBuildPhaseForTarget(projTarget, "Firebase Crashlytics dSYMs", "/bin/sh",
                    "\"$PROJECT_DIR/firebase_symbols.sh\"") == null)
            {
                project.AddShellScriptBuildPhase(projTarget, "Firebase Crashlytics dSYMs", "/bin/sh",
                    "\"$PROJECT_DIR/firebase_symbols.sh\"");
            }

            if (project.GetShellScriptBuildPhaseForTarget(mainTarget, "Firebase Crashlytics dSYMs", "/bin/sh",
                    "\"$PROJECT_DIR/firebase_symbols.sh\"") == null)
            {
                project.AddShellScriptBuildPhase(mainTarget, "Firebase Crashlytics dSYMs", "/bin/sh",
                    "\"$PROJECT_DIR/firebase_symbols.sh\"");
            }

            var firebaseSymbolsPath =
                Application.dataPath + "/SayKit/Internal/Plugins/iOS/SayKit/Firebase/firebase_symbols.sh";
            var projectFirebaseSymbolsPath = Path.Combine(pathToBuildProject, "firebase_symbols.sh");
            File.Copy(firebaseSymbolsPath, projectFirebaseSymbolsPath, true);

            var firebaseRunPath = Application.dataPath + "/SayKit/Internal/Plugins/iOS/SayKit/Firebase/firebase-run";
            var projectFirebaseRunPath = Path.Combine(pathToBuildProject, "firebase-run");
            File.Copy(firebaseRunPath, projectFirebaseRunPath, true);

            var firebaseUploadSymbolsPath =
                Application.dataPath + "/SayKit/Internal/Plugins/iOS/SayKit/Firebase/firebase-upload-symbols";
            var projectFirebaseUploadSymbolsPath = Path.Combine(pathToBuildProject, "firebase-upload-symbols");
            File.Copy(firebaseUploadSymbolsPath, projectFirebaseUploadSymbolsPath, true);
#endif
        }

        private static void SwiftCheck(PBXProject project, string mainTarget)
        {
            var swiftCheck = "echo \"Start Unity Swift Bug script.\"  \n" +
                             "if [ \"${CONFIGURATION}\" = \"Release\" ]; then \n" +
                             "cd \"${CONFIGURATION_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/Frameworks/UnityFramework.framework/\" \n" +
                             "if [[ -d \"Frameworks\" ]]; then \n" +
                             "rm -fr Frameworks \n" +
                             "echo \"Remove Frameworks folder from UnityFramework.framework.\" \n" +
                             "fi \n" +
                             "fi \n";

            if (project.GetShellScriptBuildPhaseForTarget(mainTarget, "Unity Swift Bug", "/bin/sh", swiftCheck) == null)
            {
                project.AddShellScriptBuildPhase(mainTarget, "Unity Swift Bug", "/bin/sh", swiftCheck);
            }
        }

        private static void InsertSkAdNetworkIds(PlistElementDict rootDict)
        {
            try
            {
                var idsPath = Path.Combine(Application.dataPath, "SayKit", "SKAdNetworkItems.json");

                if (File.Exists(idsPath))
                {
                    var jsonContent = File.ReadAllText(idsPath);
                    var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);

                    if (dictionary != null && dictionary.TryGetValue("networks", out var networksObj) && networksObj is JArray networksArray)
                    {
                        var allSkIds = new HashSet<string>();

                        foreach (var network in networksArray)
                        {
                            if (network["items"] is JArray idsArray)
                            {
                                foreach (var id in idsArray)
                                {
                                    if (id.Type == JTokenType.String)
                                    {
                                        allSkIds.Add(id.ToString());
                                    }
                                }
                            }
                        }

                        if (allSkIds.Count > 0)
                        {
                            var array = rootDict.CreateArray("SKAdNetworkItems");
                            foreach (var id in allSkIds)
                            {
                                var pair = array.AddDict();
                                pair.SetString("SKAdNetworkIdentifier", id);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SayKit][PostBuild] InsertSkAdNetworkIds error: {e}");
            }
        }

        private static void CheckXCodeProjectSettings(string pathToBuiltProject)
        {
            var xcodeProjectPath = Path.Combine(pathToBuiltProject, "Unity-iPhone.xcodeproj");
            var pbxPath = Path.Combine(xcodeProjectPath, "project.pbxproj");

            var xcodeProjectLines = File.ReadAllLines(pbxPath);
            var sb = new StringBuilder();
            var isNeedToAddValidArchs = false;

            foreach (var line in xcodeProjectLines)
            {
                if (line.Contains("GCC_ENABLE_OBJC_EXCEPTIONS") ||
                    line.Contains("GCC_ENABLE_CPP_EXCEPTIONS") ||
                    line.Contains("CLANG_ENABLE_MODULES"))
                {
                    var newLine = line.Replace("NO", "YES");
                    sb.AppendLine(newLine);

                    isNeedToAddValidArchs = true;
                }
                else
                {
                    sb.AppendLine(line);
                }

                if (isNeedToAddValidArchs && line.Contains("USYM_UPLOAD_URL_SOURCE"))
                {
                    isNeedToAddValidArchs = false;
                    sb.AppendLine("VALID_ARCHS = \"arm64 armv7 armv7s\";");
                }
            }

            File.WriteAllText(pbxPath, sb.ToString());
        }
        
        private static void AddLocalizationsToUnityMainTarget(PBXProject proj, string pathToBuildProject)
        {
            var sourceDirectoryPath = Path.Combine(Application.dataPath, "Resources/SayKit/Localizations");
            var destinationDirectoryPath = Path.Combine(pathToBuildProject, "Resources");

            if (Directory.Exists(sourceDirectoryPath))
            {
                if (!Directory.Exists(destinationDirectoryPath))
                {
                    Directory.CreateDirectory(destinationDirectoryPath);
                }

                var localizationFiles = Directory.GetFiles(sourceDirectoryPath, "*.json", SearchOption.TopDirectoryOnly);

                foreach (var sourceFilePath in localizationFiles)
                {
                    var fileName = Path.GetFileName(sourceFilePath);
                    var destinationFilePath = Path.Combine(destinationDirectoryPath, fileName);
                    File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
                    proj.AddFileToBuild(proj.GetUnityMainTargetGuid(), proj.AddFile(destinationFilePath, destinationFilePath));
                }
            }
        }
        
        private static void AddFileToUnityMainTarget(PBXProject proj, string pathToBuildProject, string filePath)
        {
            var sourceFilePath = Path.Combine(Application.dataPath, "Resources/SayKit", filePath);
            var destinationDirectoryPath = Path.Combine(pathToBuildProject, "Resources");

            if (File.Exists(sourceFilePath))
            {
                if (!Directory.Exists(destinationDirectoryPath))
                {
                    Directory.CreateDirectory(destinationDirectoryPath);
                }

                var destinationFilePath = Path.Combine(destinationDirectoryPath, filePath);
                File.Copy(sourceFilePath, destinationFilePath, overwrite: true);

                proj.AddFileToBuild(proj.GetUnityMainTargetGuid(), proj.AddFile(destinationFilePath, destinationFilePath));
            }
        }
        
        private static void CopyGooglePlistFile(string pathToBuildProject)
        {
            var projPath = PBXProject.GetPBXProjectPath(pathToBuildProject);
            var proj = new PBXProject();

            proj.ReadFromString(File.ReadAllText(projPath));

            var projTarget = proj.GetUnityFrameworkTargetGuid();
            var mainTarget = proj.GetUnityMainTargetGuid();

            var googlePlistPath = Application.dataPath + "/Plugins/iOS/GoogleService-Info.plist";
            var googlePlistProjectPath = Path.Combine(pathToBuildProject, "GoogleService-Info.plist");

            File.Copy(googlePlistPath, googlePlistProjectPath, true);

            proj.AddFileToBuild(projTarget,
                proj.AddFolderReference("GoogleService-Info.plist", "GoogleService-Info.plist"));
            proj.AddFileToBuild(mainTarget,
                proj.AddFolderReference("GoogleService-Info.plist", "GoogleService-Info.plist"));

            File.WriteAllText(projPath, proj.WriteToString());
        }

        private static void RemoveMetaFiles(string buildPath)
        {
            // Remove all the .meta files that Unity copies into the Xcode subdirectories.
            foreach (var subdir in new[]
                         { "Frameworks/SayKit/Internal/Plugins/iOS", "Libraries/SayKit/Internal/Plugins/iOS" })
            {
                var path = Path.Combine(buildPath, subdir);
                if (!Directory.Exists(path))
                {
                    continue;
                }

                var metaFiles = Directory.GetFiles(path, "*.meta", SearchOption.AllDirectories);
                foreach (var metaFile in metaFiles)
                {
                    File.Delete(metaFile);
                }
            }
        }

        private static void RenameMRAIDSource(string buildPath)
        {
            // Unity will try to compile anything with the ".js" extension. Since mraid.js is not intended
            // for Unity, it'd break the build. So we store the file with a masked extension and after the
            // build rename it to the correct one.

            var maskedFiles = Directory.GetFiles(
                buildPath, "*.prevent_unity_compilation", SearchOption.AllDirectories);
            foreach (var maskedFile in maskedFiles)
            {
                var unmaskedFile = maskedFile.Replace(".prevent_unity_compilation", "");
                File.Move(maskedFile, unmaskedFile);
            }
        }

        private static void CommentRowsInUnityCleanupTrampoline(string pathToBuildProject)
        {
            var filePath = Path.Combine(pathToBuildProject, "Classes/UnityAppController.mm");
            var data = File.ReadAllText(filePath);

            data = data.Replace("[_UnityAppController window].rootViewController = nil;",
                "//[_UnityAppController window].rootViewController = nil;");
            data = data.Replace("[[_UnityAppController unityView] removeFromSuperview];",
                "//[[_UnityAppController unityView] removeFromSuperview];");

            File.WriteAllText(filePath, data);
        }

        private static void LocalizeInfoPlist(string pathToBuildProject)
        {
            var projPath = Path.Combine(pathToBuildProject, "Unity-iPhone.xcodeproj/project.pbxproj");
            var projectFileData = File.ReadAllText(projPath);

            var project = new PBXProject();
            project.ReadFromString(projectFileData);

            var projTarget = project.GetUnityMainTargetGuid();


            if (!Directory.Exists(pathToBuildProject + "/Resources"))
            {
                Directory.CreateDirectory(pathToBuildProject + "/Resources");
            }

            var localizeBundle = true;
            if (string.IsNullOrEmpty(SayKitApp.APP_NAME_IOS)
                || string.IsNullOrEmpty(SayKitApp.APP_NAME_CHINA_IOS)
                || SayKitApp.APP_NAME_IOS.Contains("APP_NAME_IOS")
                || SayKitApp.APP_NAME_CHINA_IOS.Contains("APP_NAME_CHINA_IOS"))
            {
                Debug.Log(
                    "Bundle name localization is disabled because APP_NAME_IOS or APP_NAME_CHINA_IOS is null or empty.");
                localizeBundle = false;
            }

            foreach (var loc in _localizations)
            {
                var infoPlistDirectoryPath = pathToBuildProject + "/Resources/" + loc + ".lproj";

                if (!Directory.Exists(infoPlistDirectoryPath))
                {
                    Directory.CreateDirectory(infoPlistDirectoryPath);
                }

                var infoPlistStringsPath = infoPlistDirectoryPath + "/InfoPlist.strings";
                var infoPlistData = "";

                foreach (var userTrackingDescription
                         in _userTrackingDescriptions.Where(userTrackingDescription =>
                             userTrackingDescription.Key.Equals(loc)))
                {
                    infoPlistData += "\"NSUserTrackingUsageDescription\" = \"" + userTrackingDescription.Value +
                                     "\";\n";
                    break;
                }

                if (localizeBundle)
                {
                    var chinaLocale = _chinaLocalizations.Any(chinaLocalization => chinaLocalization.Equals(loc));

                    if (chinaLocale)
                    {
                        infoPlistData += "\"CFBundleDisplayName\" = \"" + SayKitApp.APP_NAME_CHINA_IOS + "\";\n";
                        infoPlistData += "\"CFBundleName\" = \"" + SayKitApp.APP_NAME_CHINA_IOS + "\";\n";
                    }
                    else
                    {
                        infoPlistData += "\"CFBundleDisplayName\" = \"" + SayKitApp.APP_NAME_IOS + "\";\n";
                        infoPlistData += "\"CFBundleName\" = \"" + SayKitApp.APP_NAME_IOS + "\";\n";
                    }
                }

                File.WriteAllText(infoPlistStringsPath, infoPlistData);
                var infoPlistStringsProjectPath = "Resources/" + loc + ".lproj/InfoPlist.strings";
                project.AddFileToBuild(projTarget,
                    project.AddFolderReference(infoPlistStringsProjectPath, infoPlistStringsProjectPath));
            }

            File.WriteAllText(projPath, project.WriteToString());
        }

        private static void AddGameCenter(string buildPath, PBXProject project)
        {
            var entitlements = GetOrCreateEntitlements(buildPath, project, out var entitlementsPath);
            entitlements.root.SetBoolean("com.apple.developer.game-center", true);
            entitlements.WriteToFile(entitlementsPath);

            project.AddCapability(project.GetUnityMainTargetGuid(), PBXCapabilityType.GameCenter, entitlementsPath);
        }

        private static void AddPushNotificationCapability(string buildPath, PBXProject project, bool development = true)
        {
            var entitlements = GetOrCreateEntitlements(buildPath, project, out var entitlementsPath);
            entitlements.root.SetString("aps-environment", development ? "development" : "production");
            entitlements.WriteToFile(entitlementsPath);

            project.AddCapability(project.GetUnityMainTargetGuid(), PBXCapabilityType.PushNotifications,
                entitlementsPath);
        }
        
#if SAYKIT_DEEP_LINK
        private static void AddAssociatedDomains(string buildPath, PBXProject project)
        {
            var entitlements = GetOrCreateEntitlements(buildPath, project, out var entitlementsPath);

            var array = entitlements.root.CreateArray("com.apple.developer.associated-domains");
            array.AddString($"applinks:{SKUtils.GetAppKey()}.go.link");

            entitlements.WriteToFile(entitlementsPath);
            project.AddCapability(project.GetUnityMainTargetGuid(), PBXCapabilityType.AssociatedDomains, entitlementsPath);
        }
#endif

        private static PlistDocument GetOrCreateEntitlements(string buildPath, PBXProject project,
            out string entitlementsPath)
        {
            var targetGuid = project.GetUnityMainTargetGuid();
            var relativePath = project.GetEntitlementFilePathForTarget(targetGuid);

            if (string.IsNullOrEmpty(relativePath))
            {
#if SAYKIT_CHINA_VERSION
            relativePath = "app.entitlements";
#else
                relativePath = Application.productName.Replace(" ", "") + ".entitlements";
#endif
                project.SetBuildProperty(targetGuid, "CODE_SIGN_ENTITLEMENTS", relativePath);
            }

            entitlementsPath = Path.Combine(buildPath, relativePath).Replace('\\', '/');
            var entitlements = new PlistDocument();
            if (File.Exists(entitlementsPath))
            {
                entitlements.ReadFromFile(entitlementsPath);
            }
            else
            {
                entitlements.Create();
                project.AddFile(relativePath, relativePath);
            }

            return entitlements;
        }

        private static void AddPrivacyManifest(string pathToBuildProject)
        {
            const string privacyInfoFileName = "PrivacyInfo.xcprivacy";

            var projPath = PBXProject.GetPBXProjectPath(pathToBuildProject);
            var proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));

            var privacyPolicyPath = Path.Combine(Application.dataPath,
                $"SayKit/Internal/Plugins/iOS/SayKit/{privacyInfoFileName}");
            var privacyPolicyUnityFrameworkPath =
                Path.Combine(pathToBuildProject, $"UnityFramework/{privacyInfoFileName}");
            var privacyPolicyMainTargetPath = Path.Combine(pathToBuildProject, privacyInfoFileName);

            File.Copy(privacyPolicyPath, privacyPolicyMainTargetPath, true);
            File.Copy(privacyPolicyPath, privacyPolicyUnityFrameworkPath, true);

            proj.AddFileToBuild(proj.GetUnityFrameworkTargetGuid(),
                proj.AddFolderReference($"UnityFramework/{privacyInfoFileName}",
                    $"UnityFramework/{privacyInfoFileName}"));
            proj.AddFileToBuild(proj.GetUnityMainTargetGuid(),
                proj.AddFolderReference(privacyInfoFileName, privacyInfoFileName));

            File.WriteAllText(projPath, proj.WriteToString());
        }
    }
}
#endif