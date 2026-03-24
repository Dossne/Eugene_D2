#if UNITY_EDITOR || UNITY_ANDROID

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

#region ReSharper

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Local
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable UnusedParameter.Local
// ReSharper disable CanSimplifyDictionaryLookupWithTryAdd

#endregion

namespace SayKitInternal
{
    public class SKGradleInjector
    {
        public static SKGradleInjector Instance { get; } = new SKGradleInjector();

        #region Variables

        public const string NativeVersion = "17.2";

        private const string SAYGAMES_URL = "https://gitlab.saygames.io/api/v4/projects/20/packages/maven";
        private const string SAYGAMES_USER_NAME = "Deploy-Token";
        private const string SAYGAMES_TOKEN = "gldt-HcYundSiYfsxvsbQ5Ags";
        private readonly string _nameSpaceArgument = $"namespace = \"{Application.identifier}\"";

        private const string AdReviewKey =
            "Bgm_EpI763LHD-Tzwa-Xvo-0PRaWZCo7n7VyFdu45NXTum_4xBRq8H9embsxqZWsu0-ezTuTbi9ED6KIafrTqK";

        private const string ApplovinQualityRepository = "maven { url 'https://artifacts.applovin.com/android' }";

        private readonly string[] _unityLibraryGradleDependenciesLines =
        {
            $"implementation(platform('saygames:bom:{NativeVersion}'))",
            "implementation('saygames:bridge-unity')",
            "implementation('saygames:saykit-google')",
            "implementation 'com.adjust.signature:adjust-android-signature:3.35.2'",
            
#if !SAYKIT_PLAY_GAMES_SERVICES_OLD
#if SAYKIT_PLAY_GAMES_SERVICES
             "implementation 'com.google.android.gms:play-services-games-v2:+'",
#endif
#endif

#if SAYKIT_ANDROID_LEAKCANARY && DEBUG
            "debugImplementation 'com.squareup.leakcanary:leakcanary-android:2.13'"
#endif
        };

        private readonly string[] _projectGradleBuildScriptDependencies =
        {
            "com.android.tools.build:gradle:8.6.1",
            "com.google.gms:google-services:4.4.2",
            "com.google.firebase:firebase-crashlytics-gradle:3.0.3",
#if SAYKIT_APPLOVIN_QUALITY_ANDROID
            "com.applovin.quality:AppLovinQualityServiceGradlePlugin:5.6.2"
#endif
        };

        private readonly string[] _projectGradleRepositories =
        {
            "google()",
            "mavenCentral()",
            "maven { url 'https://android-sdk.is.com' }",
            "maven { url 'https://dl-maven-android.mintegral.com/repository/mbridge_android_sdk_oversea' }",
            "maven { url 'https://artifact.bytedance.com/repository/pangle' }",
            "maven { url 'https://s3.amazonaws.com/smaato-sdk-releases/' }",
            "maven { url 'https://sdk.tapjoy.com'}",
            "maven { url 'https://maven.ogury.co' }",
            "maven { url 'https://cboost.jfrog.io/artifactory/chartboost-ads' }",
            "maven { url 'https://artifactory.bidmachine.io/bidmachine' }",
            "maven { url 'https://jitpack.io' }",
            "maven { url 'https://ysonetwork.s3.eu-west-3.amazonaws.com/sdk/android' }",
            "maven {",
            "    name 'GitLab'",
            $"    url '{SAYGAMES_URL}'",
            "    credentials(HttpHeaderCredentials) {",
            $"        name = '{SAYGAMES_USER_NAME}'",
            $"        value = '{SAYGAMES_TOKEN}'",
            "    }",
            "    authentication {",
            "        header(HttpHeaderAuthentication)",
            "    }",
            "}"
        };

        private readonly string[] _launcherGradlePlugins =
        {
            "apply plugin: 'com.google.gms.google-services'",
            "apply plugin: 'com.google.firebase.crashlytics'",
#if SAYKIT_APPLOVIN_QUALITY_ANDROID
            "apply plugin: 'applovin-quality-service'",
            "\n",
            "applovin {",
            $"apiKey '{AdReviewKey}'",
            "}"
#endif
        };

        private static readonly string[] _projectGradlePlugins =
        {
            "id 'com.android.application' version '8.6.1' apply false",
            "id 'com.android.library' version '8.6.1' apply false",
            "id 'com.google.gms.google-services' version '4.4.2' apply false",
            "id 'com.google.firebase.crashlytics' version '3.0.3' apply false",
#if SAYKIT_APPLOVIN_QUALITY_ANDROID
            "id 'com.applovin.quality' version '5.6.2' apply false"
#endif
        };

        #endregion

        #region Getters Gradle Content

        private string GetSettingContent()
        {
            return $@"
dependencyResolutionManagement {{
    repositoriesMode.set(RepositoriesMode.PREFER_SETTINGS)
    repositories {{
      {string.Join("\n", _projectGradleRepositories.Select(rep => $"        {rep}"))}
    }}
}}
";
        }

        private string GetProjectGradleContent()
        {
            if (IsUnityVersionForNewGradleTemplates())
            {
                return $@"
// SayKit
plugins {{
{string.Join("\n", _projectGradlePlugins.Select(plugin => $"    {plugin}"))}
}}
// SayKit
";
            }

            return $@"
buildscript {{
    dependencies {{
{string.Join("\n", _projectGradleBuildScriptDependencies.Select(dep => $"       classpath '{dep}'"))}
    }}
    repositories {{
        google()
        mavenCentral()
        {(ApplovinQualityServiceEnabled() ? ApplovinQualityRepository : string.Empty)}
    }}
}}

allprojects {{   
    repositories {{
{string.Join("\n", _projectGradleRepositories.Select(rep => $"        {rep}"))}
    }}
}}
";
        }

        private string GetUnityLibraryGradleContent()
        {
            return $@"
android {{
    {_nameSpaceArgument}
    {GetNDKVersion()}
}}

dependencies {{
{string.Join("\n", _unityLibraryGradleDependenciesLines.Select(line => $"    {line}"))}
}}
";
        }

        private string GetLauncherGradleContent()
        {
            return $@"
{string.Join("\n", _launcherGradlePlugins)}

android {{
    {_nameSpaceArgument}
    {GetNDKVersion()}
    compileOptions {{
        sourceCompatibility = '17'
        targetCompatibility = '17'
    }}
    {(CheckAssetPacksCondition() ? "assetPacks = [\":UnityDataAssetPack\"]" : "")}
}}
";
        }

        private string GetDefaultGradleContent()
        {
            return $@"
android {{
    {_nameSpaceArgument}
    {GetNDKVersion()}
}}
";
        }

        #endregion

        public void UpdateProjectGradle(string path)
        {
            var projectGradlePath = Path.Combine(path.Replace("unityLibrary", ""), "build.gradle");

            if (File.Exists(projectGradlePath))
            {
                if (!IsUnityVersionForNewGradleTemplates())
                {
                    AppendContentToGradleFile(projectGradlePath, GetProjectGradleContent());
                    ReplaceJcenterRepository(projectGradlePath);
                }
                else
                {
                    PatchPluginsBlock(projectGradlePath);
                }
            }
            else
            {
                throw new FileNotFoundException(
                    $"[SKGradleInjector][UpdateProjectGradle] File not found - {projectGradlePath}");
            }
        }
        
        public void UpdateUnityLibraryGradle(string path)
        {
            var unityLibraryGradle = Path.Combine(path + "/build.gradle");
            if (File.Exists(unityLibraryGradle))
            {
                File.WriteAllLines(
                    unityLibraryGradle,
                    File.ReadAllLines(unityLibraryGradle)
                        .Select(line => line.Contains("'UnityAds'") ? string.Empty : line)
                        .ToArray()
                );

                RemovedUnusedCompileOption(unityLibraryGradle);
                AppendContentToGradleFile(unityLibraryGradle, GetUnityLibraryGradleContent());
            }
        }

        public void UpdateLauncherGradle(string path)
        {
            var launcherGradleFilePath = path.Replace("unityLibrary", "launcher") + "/build.gradle";

            if (File.Exists(launcherGradleFilePath))
            {
                RemovedUnusedCompileOption(launcherGradleFilePath);
                AppendContentToGradleFile(launcherGradleFilePath, GetLauncherGradleContent());
            }
            else
            {
                throw new FileNotFoundException(
                    $"[SKGradleInjector][UpdateLauncherGradle] File not found - {launcherGradleFilePath}");
            }
        }

        public void UpdateGradleWrapperFile(string path)
        {
            var projectPath = path.Replace("unityLibrary", "");
            var wrapperPath = Path.Combine(projectPath, "gradle", "wrapper");
            if (!Directory.Exists(wrapperPath))
            {
                Directory.CreateDirectory(wrapperPath);
            }

            var gradleWrapperSourcePath = Path.Combine(Application.dataPath,
                "SayKit/Internal/Plugins/Settings/gradle-wrapper.properties");
            var gradleWrapperDestinationPath = Path.Combine(wrapperPath, "gradle-wrapper.properties");

            try
            {
                File.Copy(gradleWrapperSourcePath, gradleWrapperDestinationPath, true);
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"[SKGradleInjector][UpdateGradleWrapperFile] {e.Message}");
            }
        }

        public void UpdateGradleProperties(string path)
        {
            var gradlePropertiesFilePath = Path.Combine(path.Replace("unityLibrary", ""), "gradle.properties");

            if (File.Exists(gradlePropertiesFilePath))
            {
                RemoveGradlePropertiesFlag("android.enableR8=false");
                RemoveGradlePropertiesFlag("android.bundle.enableUncompressedNativeLibs=false");
                RemoveGradlePropertiesFlag("android.enableJetifier=true");
                
                var useAndroidXExist = File.ReadAllText(gradlePropertiesFilePath).Contains("android.useAndroidX=true");
                var jdkPath = GetSystemJDKPath();

                using (var writer = File.AppendText(gradlePropertiesFilePath))
                {
                    writer.WriteLine();
                    writer.WriteLine("# SayKit properties");
                    if (!useAndroidXExist)
                    {
                        writer.WriteLine("android.useAndroidX=true");
                    }

                    if (!string.IsNullOrEmpty(jdkPath))
                    {
                        writer.WriteLine($"org.gradle.java.home={jdkPath}");
                    }

                    writer.WriteLine("# SayKit properties");
                    writer.WriteLine();
                }
            }
            else
            {
                throw new FileNotFoundException(
                    $"[SKGradleInjector][UpdateGradleProperties] File not found - {gradlePropertiesFilePath}");
            }

            string GetSystemJDKPath()
            {
#if UNITY_EDITOR_WIN && UNITY_CLOUD_BUILD
                if (Directory.Exists(@"C:/Program Files/Java/jdk1.17"))
                {
                    return @"C:/Program Files/Java/jdk1.17";
                }
#elif UNITY_EDITOR_WIN
                if (Directory.Exists(@"C:/Program Files/Java/jdk-17"))
                {
                    return @"C:/Program Files/Java/jdk-17";
                }
#elif UNITY_EDITOR_OSX
                if (Directory.Exists("/Library/Java/JavaVirtualMachines/jdk-17.jdk/Contents/Home"))
                {
                    return "/Library/Java/JavaVirtualMachines/jdk-17.jdk/Contents/Home";
                }

                if (Directory.Exists("/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home"))
                {
                    return "/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home";
                }
#elif UNITY_EDITOR_LINUX
                if (Directory.Exists("/usr/lib/jvm/java-17-openjdk-amd64"))
                {
                    return "/usr/lib/jvm/java-17-openjdk-amd64";
                }

                if (Directory.Exists("/usr/java/jdk-17"))
                {
                    return "/usr/java/jdk-17";
                }
#endif

                return string.Empty;
            }

            void RemoveGradlePropertiesFlag(string flag)
            {
                var filteredLines = Array.FindAll(File.ReadAllLines(gradlePropertiesFilePath),
                    line => !line.Trim().Equals(flag));
                File.WriteAllLines(gradlePropertiesFilePath, filteredLines);
            }
        }

        public void UpdateSettingsGradleFile(string path)
        {
            var gradleSettingsFilePath = Path.Combine(path.Replace("unityLibrary", ""), "settings.gradle");

            if (File.Exists(gradleSettingsFilePath))
            {
                if (IsUnityVersionForNewGradleTemplates())
                {
                    if (ApplovinQualityServiceEnabled())
                    {
                        InjectPluginRepository(gradleSettingsFilePath, ApplovinQualityRepository);
                    }

                    AppendContentToGradleFile(gradleSettingsFilePath, GetSettingContent());
                }
            }
            else
            {
                throw new FileNotFoundException(
                    $"[SKGradleInjector][UpdateSettingsGradleFile] File not found - {gradleSettingsFilePath}");
            }
        }

        private void InjectPluginRepository(string gradleFilePath, string contentBlock)
        {
            var content = File.ReadAllText(gradleFilePath);
            const string pattern = @"(pluginManagement\s*{\s*repositories\s*{\s*)([^}]*)(\s*})";
            var regex = new Regex(pattern, RegexOptions.Singleline);

            if (!content.Contains(contentBlock))
            {
                var modifiedContent = regex.Replace(content, "$1$2" +
                                                             "    // SayKit\n" + $"        {contentBlock}\n" +
                                                             "        // SayKit" + "\n$3");
                File.WriteAllText(gradleFilePath, modifiedContent);
            }
        }

        public void ExcludePlayCoreGroupForSplitBinary(string path)
        {
#if UNITY_2020
#if UNITY_EDITOR
            if (!UnityEditor.PlayerSettings.Android.useAPKExpansionFiles || !UnityEditor.EditorUserBuildSettings.buildAppBundle)
            {
                return;
            }
#endif
           
            var unityGradleFile = path + "/build.gradle";

            if (File.Exists(unityGradleFile))
            {
                var lines = File.ReadAllLines(unityGradleFile);
                var filteredLines = lines.Where(line => !line.Contains("com.google.android.play:core")).ToArray();
                File.WriteAllLines(unityGradleFile, filteredLines);
            }

            var baseGradleFile = Path.Combine(path.Replace("unityLibrary", ""), "build.gradle");

            if (File.Exists(baseGradleFile))
            {
                const string content = "// SayKit " +
                                       "\n configurations.all { \n" +
                                       "    exclude group: 'com.google.android.play', module: 'core' \n" +
                                       "}\n" +
                                       "// SayKit";

                File.AppendAllText(baseGradleFile, content);
            }
#endif
        }

        public void UpdateGPGSGradleFile(string path)
        {
#if SAYKIT_PLAY_GAMES_SERVICES
            var googlePlayGamesGradleFile = Path.Combine(path + "/GooglePlayGamesManifest.androidlib/build.gradle");

            if (File.Exists(googlePlayGamesGradleFile))
            {
                AppendContentToGradleFile(googlePlayGamesGradleFile, GetDefaultGradleContent());
            }
            
#if SAYKIT_PLAY_GAMES_SERVICES_OLD
            var oldGooglePlayGamesGradleFile = Path.Combine(path + "/GooglePlayGamesManifest.plugin/build.gradle");
            
            if (File.Exists(oldGooglePlayGamesGradleFile))
            {
                AppendContentToGradleFile(oldGooglePlayGamesGradleFile, GetDefaultGradleContent());
            }
            
            var unityAdnroidResourcesGradleFile = Path.Combine(path + "/unity-android-resources/build.gradle");
            
            if (File.Exists(unityAdnroidResourcesGradleFile))
            {
                AppendContentToGradleFile(unityAdnroidResourcesGradleFile, GetDefaultGradleContent());
            }
#endif            
            
#endif
        }

        public void UpdateEasyMobileGradleFile(string path)
        {
            var easyMobileGradleFile = Path.Combine(path + "/EasyMobile.androidlib/build.gradle");

            if (File.Exists(easyMobileGradleFile))
            {
                AppendContentToGradleFile(easyMobileGradleFile, GetDefaultGradleContent());
            }
        }

        public void UpdateMobileNotificationGradleFile(string path)
        {
            var mobileNotificationsGradleFile = Path.Combine(path + "/mobilenotifications.androidlib/build.gradle");

            if (File.Exists(mobileNotificationsGradleFile))
            {
                AppendContentToGradleFile(mobileNotificationsGradleFile, GetDefaultGradleContent());
            }
        }

        public void UpdateSayKitGradleFile(string path)
        {
            var skGradleFile = Path.Combine(path + "/saykit.androidlib/build.gradle");

            if (File.Exists(skGradleFile))
            {
                AppendContentToGradleFile(skGradleFile, GetDefaultGradleContent());
            }
        }

        private void AppendContentToGradleFile(string filePath, string gradleContent)
        {
            using (var writer = File.AppendText(filePath))
            {
                writer.WriteLine();
                writer.WriteLine("// SayKit");
                if (!string.IsNullOrEmpty(gradleContent))
                    writer.WriteLine(gradleContent);
                writer.WriteLine("// SayKit");
                writer.WriteLine();
            }
        }

        private void RemovedUnusedCompileOption(string gradleFilePath)
        {
            var content = File.ReadAllText(gradleFilePath);

            const string compileOptionsPattern8 =
                @"compileOptions\s*{\s*sourceCompatibility\s*JavaVersion\.VERSION_1_8\s*targetCompatibility\s*JavaVersion\.VERSION_1_8\s*}";
            const string compileOptionsPattern11 =
                @"compileOptions\s*{\s*sourceCompatibility\s*JavaVersion\.VERSION_11\s*targetCompatibility\s*JavaVersion\.VERSION_11\s*}";

            content = Regex.Replace(content,
                IsUnityVersionForNewGradleTemplates() ? compileOptionsPattern11 : compileOptionsPattern8,
                string.Empty, RegexOptions.Singleline);

            File.WriteAllText(gradleFilePath, content);
        }

        private bool ApplovinQualityServiceEnabled()
        {
#if SAYKIT_APPLOVIN_QUALITY_ANDROID
            return true;
#else
            return false;
#endif
        }

        private bool CheckAssetPacksCondition()
        {
#if SAYKIT_ASSETPACKS && !SAYKIT_AUTOBUILD && !UNITY_CLOUD_BUILD
            return UnityEditor.PlayerSettings.Android.useAPKExpansionFiles;
#endif
            return false;
        }

        private string GetNDKVersion()
        {
#if UNITY_6000_0_OR_NEWER
            return "ndkVersion = \"27.2.12479018\"";
#elif UNITY_2022 || UNITY_2023
            return "ndkVersion = \"23.1.7779620\"";
#elif UNITY_2021
            return "ndkVersion = \"21.3.6528147\"";
#elif UNITY_2020
            return "ndkVersion = \"19.0.5232133\"";
#endif
        }

        private void ReplaceJcenterRepository(string gradlePath)
        {
#if UNITY_2020 || UNITY_2021
            var updatedLines = File.ReadAllLines(gradlePath)
                .Select(line => line.Replace("jcenter()", "mavenCentral()"))
                .ToArray();

            File.WriteAllLines(gradlePath, updatedLines);
#endif
        }

        private static bool IsUnityVersionForNewGradleTemplates()
        {
#if UNITY_2022_1_OR_NEWER
            return true;
#elif UNITY_2021
            return IsUnity2021Patch41();
#else
            return false;
#endif
        }

        private static bool IsUnity2021Patch41()
        {
            var currentVersion = Application.unityVersion;
            var currentParts = ParseVersion(currentVersion);
            var targetParts = ParseVersion("2021.3.41");
            for (var i = 0; i < targetParts.Length; i++)
            {
                if (currentParts[i] > targetParts[i])
                    return true;
                if (currentParts[i] < targetParts[i])
                    return false;
            }

            return true;
        }

        private static int[] ParseVersion(string version)
        {
            var parts = version.Split('.');
            var major = int.Parse(parts[0]);
            var minor = int.Parse(parts[1]);
            var patchString = parts[2].Split('f', 'a')[0];
            var patch = int.Parse(patchString);

            return new[] { major, minor, patch };
        }

        private void PatchPluginsBlock(string projectGradlePath)
        {
            var projectGradleContent = File.ReadAllText(projectGradlePath);
            var pluginsMatches = Regex.Matches(projectGradleContent, @"plugins\s*\{([\s\S]*?)\}", RegexOptions.Singleline);

            var pluginsDict = new Dictionary<string, string>();

            if (pluginsMatches.Count > 0)
            {
                foreach (Match match in pluginsMatches)
                {
                    var pluginLines =
                        Regex.Matches(match.Groups[1].Value, @"id\s+'([^']+)'\s*(version\s+'([^']+)')?\s*(apply\s+\w+)?")
                            .Cast<Match>();

                    foreach (var pluginMatch in pluginLines)
                    {
                        var pluginId = pluginMatch.Groups[1].Value;
                        var version = pluginMatch.Groups[3].Success ? pluginMatch.Groups[3].Value : null;
                        var apply = pluginMatch.Groups[4].Success ? pluginMatch.Groups[4].Value : "";

                        var fullPluginString = $"id '{pluginId}'" + (version != null ? $" version '{version}'" : "") + $" {apply}".Trim();

                        if (pluginsDict.ContainsKey(pluginId))
                        {
                            var existingVersion = ExtractVersion(pluginsDict[pluginId]);
                            if (IsNewerVersion(version, existingVersion))
                            {
                                pluginsDict[pluginId] = fullPluginString;
                            }
                        }
                        else
                        {
                            pluginsDict[pluginId] = fullPluginString;
                        }
                    }
                }
            }

            foreach (var plugin in _projectGradlePlugins)
            {
                var pluginId = Regex.Match(plugin, @"id\s+'([^']+)'").Groups[1].Value;
                pluginsDict[pluginId] = plugin;
            }

            var updatedPluginsBlock = $"plugins {{\n    {string.Join("\n    ", pluginsDict.Values)}\n}}";

            projectGradleContent = Regex.Replace(projectGradleContent, @"plugins\s*\{([\s\S]*?)\}", "").Trim();
            projectGradleContent = updatedPluginsBlock + "\n\n" + projectGradleContent;

            File.WriteAllText(projectGradlePath, projectGradleContent);
        }

        private string ExtractVersion(string pluginString)
        {
            var match = Regex.Match(pluginString, @"version\s+'([^']+)'");
            return match.Success ? match.Groups[1].Value : null;
        }

        private bool IsNewerVersion(string newVersion, string oldVersion)
        {
            if (string.IsNullOrEmpty(newVersion)) return false;
            if (string.IsNullOrEmpty(oldVersion)) return true;

            var newParts = newVersion.Split('.').Select(p => int.TryParse(p, out var num) ? num : 0).ToArray();
            var oldParts = oldVersion.Split('.').Select(p => int.TryParse(p, out var num) ? num : 0).ToArray();

            for (var i = 0; i < Math.Max(newParts.Length, oldParts.Length); i++)
            {
                var newPart = i < newParts.Length ? newParts[i] : 0;
                var oldPart = i < oldParts.Length ? oldParts[i] : 0;

                if (newPart > oldPart) return true;
                if (newPart < oldPart) return false;
            }

            return false;
        }
    }
}
#endif