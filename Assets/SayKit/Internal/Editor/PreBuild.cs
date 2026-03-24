#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using SayKitInternal;

#region ReSharper

// ReSharper disable AccessToStaticMemberViaDerivedType
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantIfElseBlock
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable RedundantAssignment
// ReSharper disable RedundantAnonymousTypePropertyName
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable RedundantNameQualifier
// ReSharper disable RedundantUsingDirective
// ReSharper disable RedundantDefaultMemberInitializer

#endregion

public class ApplicationSettings
{
    public static string facebook_app_id;
    public static string facebook_app_name;
    public static string facebook_client_token;
    public static string admob_app_id;
    public static string maxsdk_key;
    public static string google_license_key;
}

public class SayKitPreBuild : IPreprocessBuildWithReport
{
    private interface IPreBuildTask
    {
        string Title();
        string Run();
    }

    const string DIALOG_TITLE = "SayKit Build";
    const string SettingsPath = "Assets/SayKit/Internal/Plugins/Settings";
    const string PluginsPath = "Assets/Plugins";
    const string AndroidPluginsPath = PluginsPath + "/Android";
    const string IosPluginsPath = PluginsPath + "/iOS";
    internal const string SayKitLibPath =
#if UNITY_2020_1_OR_NEWER
        AndroidPluginsPath + "/saykit.androidlib";
#endif

    public bool dirtyFlag = false;
    private static bool _isBlackList = false;
    public static string buildPlace = "default";

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        OnPreprocessBuild(report, false);
    }

    public void OnPreprocessBuild(BuildReport report, bool releaseCheck)
    {
        
        SayKitRemoteSettings.Instance.Validate();

        if (SayKitRemoteSettings.Instance.HasError)
        {
            throw new BuildFailedException(SayKitRemoteSettings.Instance.ErrorMessage);
        }

        var tasksNumber = 3;
        dirtyFlag = false;

        var tasks = new List<IPreBuildTask>
        {
            new RemoveObsoleteSayKitFiles(),
            new CheckUnityLicense(),
            new CheckPlatformDefines(),
            new CheckUnityPurchasingVersion(),
            new CreateDependentFolders(),
            new CheckSayKitDependencyVersions()
        };
        
        var app_key = SKUtils.GetAppKey();

        // Embed Config
#if SAYKIT_AUTOBUILD
        tasks.Add(new EmbedConfig(app_key, 5));
        tasks.Add(new CheckRemoteConfigs(5));
        tasks.Add(new GameLocalizations(app_key,5));
#else
        tasks.Add(new EmbedConfig(app_key, 1));
        tasks.Add(new CheckRemoteConfigs(1));
        tasks.Add(new GameLocalizations(app_key, 1));
#endif

#if UNITY_IOS
        tasks.Add(new ConfigureGenerateFiles());
        tasks.Add(new CheckGoogleServicesSettings());
        
        tasksNumber += 3;

#elif UNITY_ANDROID

#if !SAYKIT_AUTOBUILD_KEYSTORE
        CheckKyestoreFile();
#endif

        tasks.Add(new ConfigureGenerateFiles());
        tasks.Add(new CheckGoogleServicesSettings());
        tasks.Add(new CheckAndroidSettings());
        tasks.Add(new CheckMultiDexFabricApplicationSettings());
        tasks.Add(new CheckMinificationSettings());


        tasksNumber += 6;
#endif

#if SAYKIT_DEBUG
        tasks.Add(new SetDebugNetworkConfigSettings());
#endif

#if SAYKIT_PURCHASING
        if (SayKitApp.purchasesEnabled)
        {
            CheckPurchaseDefine(app_key);
        }
#endif

        // Check Symbols
        tasks.Add(new CheckScriptingSymbols());

        // Check platform settings
        if (releaseCheck)
        {
            tasks.Add(new CheckBuildSettings());
        }

        tasks.Add(new RemoveConfigs(app_key));

        for (int i = 0; i < tasks.Count; i++)
        {
            EditorUtility.DisplayProgressBar(DIALOG_TITLE, tasks[i].Title(), (float)i / tasksNumber);
            Assert(tasks[i].Run());

            if (_isBlackList)
            {
                break;
            }
        }

        if (Application.isBatchMode)
        {
            if (dirtyFlag)
            {
                EditorApplication.Exit(1);
            }
        }

        // We are done.
        EditorUtility.ClearProgressBar();
    }

    private void Assert(string result)
    {
        if (result.Length > 0)
        {
            dirtyFlag = true;
            Debug.LogError("SayKit: " + result);

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(DIALOG_TITLE, result, "OK");
            }
        }
    }

    #region PreBuildTasks

    private class RemoveObsoleteSayKitFiles : IPreBuildTask
    {
        public string Title()
        {
            return "Removing obsolete files";
        }

        public string Run()
        {
            var resourcePath = Path.Combine(Application.dataPath, "Resources");

            if (Directory.Exists(resourcePath))
            {
                var files = Directory.GetFiles(resourcePath, "saykit_*", SearchOption.AllDirectories);

                if (files.Length > 0)
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception e)
                        {
                            SayKitDebug.LogError($"[PreBuild] RemoveObsoleteSayKitFiles Failed to delete file {file}: {e.Message}");
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
    
    private class CheckSayKitFolder : IPreBuildTask
    {
        public string Title()
        {
            return "Checking existing SayKit folder";
        }

        public string Run()
        {
            var sayKitDirectoryName = Application.dataPath + "/" + "SayKit";

            return Directory.GetDirectories(Application.dataPath).Any(directoryName =>
                sayKitDirectoryName.Equals(directoryName, StringComparison.Ordinal))
                ? string.Empty
                : $"Missing directory {sayKitDirectoryName}. Please check the directory name Assets/SayKit, letter case matters!";
        }
    }

    private class RemoveConfigs : IPreBuildTask
    {
        private string _appKey;

        public RemoveConfigs(string appKey)
        {
            _appKey = appKey;
        }

        public string Title()
        {
            return "Removing old configs";
        }

        public string Run()
        {
            FindAndDeleteObsoleteConfigs();

            return string.Empty;
        }

        private void FindAndDeleteObsoleteConfigs()
        {
            RemoveFoundConfigs(Directory.GetFiles("Assets/Resources", $"saykit_{_appKey}_" + "*.json"));
            RemoveFoundConfigs(Directory.GetFiles("Assets/Resources/SayKit", $"saykit_{_appKey}_" + "*.json"));

            void RemoveFoundConfigs(string[] configFiles)
            {
                if (configFiles.Length > 1)
                {
                    foreach (var configFile in configFiles)
                    {
                        var version = ExtractVersionFromPath(configFile);
                        if (!string.IsNullOrEmpty(version) && version != Application.version)
                        {
                            var metaPath = configFile + ".meta";

                            if (File.Exists(metaPath))
                            {
                                File.Delete(metaPath);
                            }

                            File.Delete(configFile);
                        }
                    }

                    AssetDatabase.Refresh();
                }
            }
        }

        private string ExtractVersionFromPath(string filePath)
        {
            try
            {
                var startIndex = filePath.LastIndexOf('_') + 1;
                var endIndex = filePath.LastIndexOf(".json", StringComparison.Ordinal);

                if (startIndex < 0 || endIndex < 0)
                {
                    return null;
                }

                var version = filePath.Substring(startIndex, endIndex - startIndex);
                return version;
            }
            catch (Exception e)
            {
                SayKitDebug.LogError("Cannot extract a version from the file: " + filePath + $", exception: {e.Message}");
            }

            return null;
        }
    }

    private class EmbedConfig : IPreBuildTask
    {
        private string _appKey;
        private int _attempts;

        public EmbedConfig(string appKey, int attempts)
        {
            _appKey = appKey;
            _attempts = attempts;
        }

        public string Title()
        {
            return "Embedding config";
        }

        public string Run()
        {
            var result = string.Empty;

            if (_appKey != string.Empty)
            {
                var path = "Assets/Resources/SayKit";
                var key = path + "/saykit_" + _appKey + "_" + Application.version + ".json";

                var url = "https://app.saygames.io/config/" + _appKey + "?version=" + Application.version +
                          "&embedded=1" + "&v=3" + "&pretty=1" + "&saykit=" + SKManager.Instance.Version + "&_=" +
                          UnityEngine.Random.Range(100000000, 900000000);

                var errorTemplate = "Can't embed config for " + _appKey + " version " + Application.version +
                                    "\n\nUrl: " + url + "\n\nLocal path: " + key + "\n\n";
                
                DownloadConfigFileWithAttempts(url, _attempts, out var data, out var errorMessage);

                try
                {
                    if (data?.Length > 0 && data[0] == '{')
                    {
                        SKUtils.SaveFile(
                            Path.Combine(Application.dataPath, "Resources/SayKit"),
                            "saykit_" + _appKey + "_" + Application.version + ".json",
                            data);

                        var config = JsonConvert.DeserializeObject<RemoteConfig>(data);

                        result = CheckCopyConfig(config);

                        if (config.ads_settings.maxsdk_enabled != 1
                            || string.IsNullOrEmpty(config.ads_settings.maxsdk_interstitial_id)
                            || string.IsNullOrEmpty(config.ads_settings.maxsdk_rewarded_id))
                        {
                            result =
                                "MaxMediation is not configured, please connect with SayGames support team.";
                        }
                        else
                        {
                            ApplicationSettings.maxsdk_key = config.ads_settings.maxsdk_key;
                        }
                    }
                    else
                    {
                        result = errorTemplate + "Error in downloading. " + errorMessage;
                    }
                }
                catch (Exception e)
                {
                    result = errorTemplate + "Error in saving. " + e;
                }
            }

            return result;
        }

        private string CheckCopyConfig(RemoteConfig config)
        {
            var gameSettingsJson = JsonConvert.SerializeObject(config.game_settings);
            var gs2 = SKUtils.DeepCopy(config.game_settings);
            var gameSettingsJson2 = JsonConvert.SerializeObject(gs2);

            if (gameSettingsJson != gameSettingsJson2)
            {
                return "SayKit: copy of game_settings doesn't equal the original data. ";
            }

            var configJson = JsonConvert.SerializeObject(config);
            var config2 = config.DeepCopy();
            var configJson2 = JsonConvert.SerializeObject(config2);

            if (configJson != configJson2)
            {
                return "SayKit: copy of RemoteConfig doesn't equal the original data.";
            }

            return string.Empty;
        }
    }

    private class GameLocalizations : IPreBuildTask
    {
        private string _appKey;
        private int _attempts;

        public GameLocalizations(string appKey, int attempts)
        {
            _appKey = appKey;
            _attempts = attempts;
        }

        public string Title()
        {
            return "Game localizations";
        }

        public string Run()
        {
            var result = string.Empty;

            var url = $"https://app.saygames.io/localization/{_appKey}";
            var errorTemplate = "Can't load localization for " + _appKey + " version " + Application.version;

            try
            {
                SKLocalizationService.Instance.DownloadLocalizationsWithAttempts(url, JsonConvert.SerializeObject(new
                    {
                        version = Application.version,
                        saykit = SKManager.Instance.Version,
                        v = 1,
                        embedded = true
                    }),
                    _attempts,
                    (data, error) =>
                    {
                        if (string.IsNullOrEmpty(error))
                        {
                            if (!string.IsNullOrEmpty(data))
                            {
                                var localizations = JsonConvert.DeserializeObject<SKLanguageLocalization[]>(data);

                                if (localizations.Length > 0)
                                {
                                    foreach (var localization in localizations)
                                    {
                                        var jsonLoc = JsonConvert.SerializeObject(localization);
                                        SKUtils.SaveFile(
                                            Path.Combine(Application.dataPath, "Resources/SayKit/Localizations"),
                                            $"saykit_localization_{localization.Language}.json",
                                            jsonLoc);
                                    }
                                }
                            }
                            else
                            {
                                result = "Empty localizations data";
                            }
                        }
                        else
                        {
                            result = $"{errorTemplate} {error}";
                        }
                    });
            }
            catch (Exception e)
            {
                result = $"{errorTemplate} {e}";
            }

            return result;
        }
    }

    private class CheckRemoteConfigs : IPreBuildTask
    {
        private int _attempts;

        public CheckRemoteConfigs(int attempts)
        {
            _attempts = attempts;
        }

        public class SayKitRemoteConfiguration
        {
            [JsonProperty("Name")]
            public string Name { get; set; }

            [JsonProperty("Data")]
            public string Data { get; set; }
        }

        public class SayKitRemoteData
        {
            [JsonProperty("Version")]
            public int Version { get; set; }

            [JsonProperty("Error")]
            public string Error { get; set; }

            [JsonProperty("Platform")]
            public string Platform { get; set; }

            [JsonProperty("Configuration")]
            public List<SayKitRemoteConfiguration> Configuration { get; set; }

            [JsonProperty("Attribution")]
            public string Attribution { get; set; }

            [JsonProperty("AttributionToken")]
            public string AttributionToken { get; set; }

            [JsonProperty("Alerts")]
            public string[] Alerts { get; set; }

            [JsonProperty("Messages")]
            public string[] Messages { get; set; }
        }

        public string Title()
        {
            return "Checking remote configurations";
        }

        public string Run()
        {
            return DownloadConfig();
        }

        private string GetRemoteURL()
        {
            var url = "https://api.launcher.saygames.io/saykit/configure?";

#if UNITY_IOS
            url += "app_key=" + SKUtils.GetAppKey();
            url += "&app_secret=" + SayKitApp.APP_SECRET_IOS;
            url += "&saykit_platform=" + "ios";
#elif UNITY_ANDROID
            url += "app_key=" + SKUtils.GetAppKey();
            url += "&app_secret=" + SayKitApp.APP_SECRET_ANDROID;
            url += "&saykit_platform=" + "android";
#endif

            url += "&app_version=" + Application.version;
            url += "&saykit=" + SKManager.Instance.Version;

            url += "&place=" + buildPlace;
            url += "&device_id=" + SystemInfo.deviceUniqueIdentifier;

            return url;
        }

        private string DownloadConfig()
        {
            var url = GetRemoteURL();

            DownloadConfigFileWithAttempts(url, _attempts, out var data, out var errorMessage);

            try
            {
                if (data?.Length > 0 && data[0] == '{')
                {
                    var attributionData = new AttributionData();
                    var config = JsonConvert.DeserializeObject<SayKitRemoteData>(data);
                    
                    if (config.Error.Length == 0)
                    {
#if !SAYKIT_DEBUG
                        if (config.Alerts?.Length > 0)
                        {
                            foreach (var alert in config.Alerts)
                            {
                                if (!string.IsNullOrEmpty(alert))
                                {
                                    _isBlackList = true;
                                }

                                return alert;
                            }
                        }
#endif

                        var isFirebaseSettingsFinded = false;
                        for (var i = 0; i < config.Configuration.Count; i++)
                        {
                            if (config.Configuration[i].Name == "firebase")
                            {
                                isFirebaseSettingsFinded = true;
#if UNITY_IOS
                                var googlePlistPath = Application.dataPath + "/Plugins/iOS/GoogleService-Info.plist";
                                var dirInfo = new DirectoryInfo(googlePlistPath);
                                var destinationPath = dirInfo.FullName;

                                File.WriteAllText(destinationPath, config.Configuration[i].Data);
#elif UNITY_ANDROID
                                var googleJsonPath = Application.dataPath + "/Plugins/Android/google-services.json";
                                var dirInfo = new DirectoryInfo(googleJsonPath);
                                var destinationPath = dirInfo.FullName;

                                File.WriteAllText(destinationPath, config.Configuration[i].Data);
#endif
                            }
                            else if (config.Configuration[i].Name == "facebook_app_id")
                            {
                                ApplicationSettings.facebook_app_id = config.Configuration[i].Data;
                            }
                            else if (config.Configuration[i].Name == "facebook_app_name")
                            {
                                ApplicationSettings.facebook_app_name =
                                    System.Security.SecurityElement.Escape(config.Configuration[i].Data);
                            }
                            else if (config.Configuration[i].Name == "facebook_client_token")
                            {
                                ApplicationSettings.facebook_client_token = config.Configuration[i].Data;
                            }
                            else if (config.Configuration[i].Name == "admob_app_id")
                            {
                                ApplicationSettings.admob_app_id = config.Configuration[i].Data;
                            }
                            else if (config.Configuration[i].Name == "google_license_key")
                            {
                                ApplicationSettings.google_license_key = config.Configuration[i].Data;
                            }
                        }

                        attributionData.Attribution = config.Attribution;
                        attributionData.AttributionToken = config.AttributionToken;

                        if (string.IsNullOrEmpty(attributionData.AttributionToken))
                        {
                            return
                                "Attribution is not configurated. Please, check your internet connection or connect to SayGames support team.";
                        }
                        else
                        {
                            var attributionJson = JsonConvert.SerializeObject(attributionData);

                            SKUtils.SaveFile(
                                Path.Combine(Application.dataPath, "Resources/SayKit"),
                                "saykit_attribution_settings.json",
                                attributionJson);
                        }

                        if (isFirebaseSettingsFinded)
                        {
                            return string.Empty;
                        }
                        else
                        {
                            Debug.LogError(
                                "Error when loading google settings. Server configuration doesn't contain google settings.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Error when loading google settings: " + config.Error);
                    }
                }
                else
                {
                    Debug.LogError("Error when loading google settings, content data is not correct: " +
                                   errorMessage);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception when loading google settings: " + ex.Message);
            }

            return "Google settings wasn't loaded from server. Please, check the logs for more information.";
        }
    }

    private class CreateDependentFolders : IPreBuildTask
    {
        public string Title()
        {
            return "Create dependent folders.";
        }

        private readonly string[] _dependentDirectories =
        {
            "Assets/Plugins",
            "Assets/Plugins/iOS",
            "Assets/Plugins/Android",
            SayKitLibPath,
            SayKitLibPath + "/res",
            SayKitLibPath + "/res/values",
            SayKitLibPath + "/res/xml",
            SayKitLibPath + "/res/drawable"
        };

        public string Run()
        {
            foreach (var directory in _dependentDirectories)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            CheckSayKitGeneratedFiles();

            return string.Empty;
        }

        private void CheckSayKitGeneratedFiles()
        {
#if UNITY_ANDROID
            var path = "Assets/Plugins/Android/";
            var d = new DirectoryInfo(path);
            var Files = d.GetFiles("*.gradle");

            var needToCleanPlugins = false;
            foreach (var file in Files)
            {
                if (file.Name.Equals("launcherTemplate.gradle"))
                {
                    var text = File.ReadAllText(path + "launcherTemplate.gradle");
                    if (text.Contains("com.google.gms:google-services"))
                    {
                        needToCleanPlugins = true;
                        break;
                    }
                }

                if (file.Name.Equals("mainTemplate.gradle"))
                {
                    var text = File.ReadAllText(path + "mainTemplate.gradle");
                    if (text.Contains("com.google.gms:google-services"))
                    {
                        needToCleanPlugins = true;
                        break;
                    }
                }

                if (file.Name.Equals("baseProjectTemplate.gradle"))
                {
                    needToCleanPlugins = true;
                    break;
                }
            }

            if (needToCleanPlugins)
            {
#if SAYKIT_AUTOFIX_DISABLE
                EditorUtility.DisplayDialog(DIALOG_TITLE, "Please, remove all files from the Assets/Plugins/Android folder.", "OK");
#else
                DeleteAllFiles(path);
#endif
            }
#endif
        }

        private void DeleteAllFiles(string path)
        {
            var files = Directory.GetFiles(path);

            var exclusions = new[] { ".cs", ".java", ".c", ".cpp", ".h" };
            foreach (var file in files)
            {
                if (exclusions.Any(file.Contains))
                {
                    continue;
                }

                File.Delete(file);
            }

            Directory.Delete(path + "saykit", true);
        }
    }

    private class CheckScriptingSymbols : IPreBuildTask
    {
        public string Title()
        {
            return "Checking Scripting Symbols";
        }

        public string Run()
        {
            var errors = new List<string>();

            var purchasesSymbol = false;
            var notificationsSymbol = false;

#if SAYKIT_PURCHASING || SAYKIT_BILLING
            purchasesSymbol = true;
#endif

#if SAYKIT_NOTIFICATIONS
            notificationsSymbol = true;
#endif

            if (SayKitApp.purchasesEnabled && !purchasesSymbol)
            {
                errors.Add(
                    "Purchases is enabled, but SAYKIT_PURCHASING or SAYKIT_BILLING symbol is not defined.\nRefer SayKit README for details.");
            }

            if (SayKitApp.notificationsEnabled && !notificationsSymbol)
            {
                errors.Add(
                    "Notifications is enabled, but SAYKIT_NOTIFICATIONS symbol is not defined.\nRefer SayKit README for details.");
            }


            return string.Join("\n\n", errors);
        }
    }

    private class CheckBuildSettings : IPreBuildTask
    {
        public string Title()
        {
            return "Checking build settings";
        }

        public string Run()
        {
#if UNITY_IOS
#elif UNITY_ANDROID
            if (SayKitInternal.Editor.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
            {
                return "Set IL2CPP as Scripting Backend\n\nGo to Player Settings -> Other Settings -> Configuration";
            }
#endif

            return string.Empty;
        }
    }

    private class CheckMinificationSettings : IPreBuildTask
    {
        public string Title()
        {
            return "Checking minification settings";
        }

        public string Run()
        {
#if !SAYKIT_MINIFICATION_CHECK_DISABLE && UNITY_2020_1_OR_NEWER

            if (PlayerSettings.Android.minifyDebug
                || PlayerSettings.Android.minifyRelease)
            {
                return
                    "Please, remove all minification settings flags:\n\n Go to Player Settings -> Publishing Settings -> Minify";
            }
#endif
            return string.Empty;
        }
    }

    private class CheckMultiDexFabricApplicationSettings : IPreBuildTask
    {
        public string Title()
        {
            return "Checking MultiDexFabricApplication settings";
        }

        public string Run()
        {
#if UNITY_ANDROID
            var path = "Assets/Plugins/Android/";
            var manifestPath = Path.Combine(path, "AndroidManifest.xml");

            if (File.Exists(manifestPath))
            {
                var str = File.ReadAllText(manifestPath);
                if (str.Contains("MultiDexFabricApplication"))
                {
                    return
                        "AndroidManifest.xml contains MultiDexFabricApplication settings. Please, delete android:name=\"io.fabric.unity.android.MultiDexFabricApplication\" line from Assets/Plugins/Android/AndroidManifest.xml file.";
                }
            }
#endif

            return string.Empty;
        }
    }
    
    private class ConfigureGenerateFiles : IPreBuildTask
    {
        private string _settingsPath = "Assets/SayKit/Internal/Plugins/Settings/";

        public string Title()
        {
            return "Configure gradle and manifest files.";
        }

        public string Run()
        {
#if UNITY_ANDROID

            var sayKitResult = CheckSayKitInitialize();
            if (sayKitResult.Length > 0)
            {
                return sayKitResult;
            }

            var networkSecurityResult = CheckNetworkSecurityConfigFile();
            if (networkSecurityResult.Length > 0)
            {
                return networkSecurityResult;
            }

            var valuesSettingsResult = CheckValuesSettings();
            if (valuesSettingsResult.Length > 0)
            {
                return valuesSettingsResult;
            }

#endif

            AssetDatabase.Refresh();

            return string.Empty;
        }

        private string CheckSayKitInitialize()
        {
            CheckDirectory(SayKitLibPath);

            var projectPropertiesPath = Path.Combine(_settingsPath + "saykit/", "project.properties");
            var projectPropertiesDestinationPath = Path.Combine(SayKitLibPath + "/", "project.properties");

            if (CheckFile(projectPropertiesPath, projectPropertiesDestinationPath, "project.properties").Length > 0)
            {
                var lines = new List<string>
                {
                    "android.library=true"
                };

                File.AppendAllLines(projectPropertiesDestinationPath, lines);
            }


            var manifestPath = Path.Combine(_settingsPath + "saykit/", "AndroidManifest.xml");
            var manifestDestinationPath = Path.Combine(SayKitLibPath + "/", "AndroidManifest.xml");

            return CheckFile(manifestPath, manifestDestinationPath, "AndroidManifest.xml");
        }

        private string CheckNetworkSecurityConfigFile()
        {
            var resDirectoryPath = Path.Combine(SayKitLibPath, "res");

            CheckDirectory(resDirectoryPath);

            var xmlDirectoryPath = Path.Combine(SayKitLibPath, "res/xml");

            CheckDirectory(xmlDirectoryPath);


            var networkSecurityConfigFilePath =
                Path.Combine(_settingsPath + "saykit/res/xml/", "network_security_config.xml");

            var networkSecurityConfigDestinationPath =
                Path.Combine(xmlDirectoryPath + "/", "network_security_config.xml");

            return CheckFile(networkSecurityConfigFilePath, networkSecurityConfigDestinationPath,
                "network_security_config.xml");
        }

        private string CheckValuesSettings()
        {
            if (string.IsNullOrEmpty(ApplicationSettings.facebook_app_name)
                || string.IsNullOrEmpty(ApplicationSettings.facebook_app_id)
                || string.IsNullOrEmpty(ApplicationSettings.facebook_client_token)
                || string.IsNullOrEmpty(ApplicationSettings.admob_app_id))
            {
                return
                    "facebook_app_name, facebook_app_id or admob_app_id didn't download from server. Please, check your internet connection or connect to SayGames support team.";
            }
            else
            {
                var stringsPath = Path.Combine(SayKitLibPath, "res/values/strings.xml");
                var valuesPath = Path.Combine(SayKitLibPath, "res/values");
                if (!Directory.Exists(valuesPath))
                {
                    Directory.CreateDirectory(valuesPath);
                }

                var lines = new List<string>
                {
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
                    "<resources>",
                    "    <string name=\"sk_admob_app_id\">" + ApplicationSettings.admob_app_id + "</string>",
                    "    <string name=\"sk_facebook_app_id\">" + ApplicationSettings.facebook_app_id + "</string>",
                    "    <string name=\"sk_facebook_app_name\">" + ApplicationSettings.facebook_app_name + "</string>",
                    "    <string name=\"sk_facebook_client_token\">" + ApplicationSettings.facebook_client_token + "</string>",
                    "    <string name=\"fb_login_protocol_scheme\">fb" + ApplicationSettings.facebook_app_id + "</string>",
                    "    <string name=\"sk_maxsdk_key\">" + ApplicationSettings.maxsdk_key + "</string>",
                    "    <string name=\"sk_google_license_key\">" + ApplicationSettings.google_license_key + "</string>",
#if UNITY_6000
                    "    <string name=\"game_view_content_description\">" + "Game view" + "</string>",
#endif
                    "</resources>"
                };

                if (!File.Exists(stringsPath))
                {
                    File.WriteAllLines(stringsPath, lines);
                }
                else
                {
#if !SAYKIT_AUTOFIX_DISABLE
                    File.WriteAllLines(stringsPath, lines);
#endif

                    var baseFileLines = File.ReadAllLines(stringsPath);

                    foreach (var item in baseFileLines)
                    {
                        if (item.Length > 0 && lines.All(t => t.Replace(" ", "") != item.Replace(" ", "")))
                        {
                            return
                                "facebook_app_name, facebook_app_id, facebook_client_token or admob_app_id aren't configurated correctrly. Please, check a Assets/Plugins/Android/saykit/res/values/strings.xml file.";
                        }
                    }
                }
            }

            return string.Empty;
        }

        private void CheckDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private string CheckFile(string baseFile, string targetFile, string fileName)
        {
            if (!File.Exists(targetFile))
            {
                File.Copy(baseFile, targetFile, true);
            }
            else
            {
#if !SAYKIT_AUTOFIX_DISABLE
                File.Copy(baseFile, targetFile, true);
#endif

                if (!CompareFiles(baseFile, targetFile))
                {
                    return fileName + " file doesn't contain depended lines. " +
                           "Please, check a " + targetFile + " file." +
                           " It needs to contain all data from a " + baseFile + " file."
                           + "\nYou can delete " + targetFile + " and it will be generated correctly.";
                }
            }

            return string.Empty;
        }
    }
    
    private class SetDebugNetworkConfigSettings : IPreBuildTask
    {
        public string Title()
        {
            return "Update network config settings";
        }

        public string Run()
        {
            var networkConfigPath = Path.Combine(SayKitLibPath, "res/xml/network_security_config.xml");
            if (File.Exists(networkConfigPath))
            {
                string contentString;
                using (var reader = new StreamReader(networkConfigPath))
                {
                    contentString = reader.ReadToEnd();
                    reader.Close();
                }

                contentString = Regex.Replace(contentString, "<certificates src=\"system\"/>",
                    "<certificates src=\"system\"/>" + "\n" + "            <certificates src=\"user\"/>");

                using (var writer = new StreamWriter(networkConfigPath))
                {
                    writer.Write(contentString);
                    writer.Close();
                }
            }

            return string.Empty;
        }
    }
    
    private class CheckGoogleServicesSettings : IPreBuildTask
    {
        public string Title()
        {
            return "Checking google services settings";
        }

        public string Run()
        {
#if UNITY_IOS
            var path = "Assets/Plugins/iOS/";
            var d = new DirectoryInfo(path);
            var Files = d.GetFiles("*.plist");
            var isFileExist = false;

            foreach (var file in Files)
            {
                if(file.Name == "GoogleService-Info.plist")
                {
                    isFileExist = true;
                }
            }

            if(!isFileExist)
            {
                return "File GoogleService-Info.plist is not found. You need to download it from Firebase console.";
            }
            
            float.TryParse(PlayerSettings.iOS.targetOSVersionString, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out float targetOSVersion);
            
            if (targetOSVersion < 15)
            {
                return "Minimum supported iOS version is 15.0. Please update target minimum iOS version in player settings.";
            }

#elif UNITY_ANDROID
            var path = "Assets/Plugins/Android/";
            var d = new DirectoryInfo(path);
            var Files = d.GetFiles("*.json");
            var isFileExist = false;

            foreach (FileInfo file in Files)
            {
                if (file.Name == "google-services.json")
                {
                    isFileExist = true;
                }
            }

            if (!isFileExist)
            {
                return "File google-services.json is not found. You need to download it from Firebase console";
            }

#endif

            return string.Empty;
        }
    }

    private class CheckAndroidSettings : IPreBuildTask
    {
        public string Title()
        {
            return "Checking gradle settings";
        }

        public string CutGradleLibVersion(string str)
        {
            if (str.Contains("@aar"))
            {
                return CutGradleLibVersion(str, 5);
            }
            else
            {
                return CutGradleLibVersion(str, 1);
            }
        }

        private string CutGradleLibVersion(string str, int tailLength)
        {
            var splits = str.Split(':');
            var split = splits[splits.Length - 1];

            return split.Substring(0, split.Length - tailLength);
        }

        public string CutLibVersion(string str)
        {
            return CutLibVersion(str, 4);
        }

        public string CutGradleJarVersion(string str)
        {
            return CutLibVersion(str, 6);
        }

        public string CutGradleAarVersion(string str)
        {
            if (str.Contains("@aar"))
            {
                return CutGradleLibVersion(str, 5);
            }
            else
            {
                var splits = str.Split(',');
                var split = splits[0];
                return CutLibVersion(split, 1);
            }
        }

        private string CutLibVersion(string str, int tailLength)
        {
            var splits = str.Split('-');
            var split = splits[splits.Length - 1];

            return split.Substring(0, split.Length - tailLength);
        }

        private string CheckMinAPILevel()
        {
            var errorMessage = string.Empty;

            if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel26)
            {
                errorMessage =
                    "You have to update minimum API level to 26 \n(Build Settings -> Player Settings -> Other settings -> Identification). \nPlease see the Readme file for more information.";
            }

            if ((int)PlayerSettings.Android.targetSdkVersion < 35)
            {
                errorMessage =
                    "You have to update minimum target API level to 35 \n(Build Settings -> Player Settings -> Other settings -> Identification). \nPlease see the Readme file for more information.";
            }

            return errorMessage;
        }

        public string Run()
        {
            return CheckMinAPILevel();
        }
    }

    private class CheckUnityLicense : IPreBuildTask
    {
        public string Title()
        {
            return "Checking Unity license";
        }

        public string Run()
        {
            if (Application.isBatchMode)
            {
                if (!Application.HasProLicense())
                {
                    return "Building the project is only available with a Unity Pro license.";
                }
            }

            return string.Empty;
        }
    }

    private class CheckSayKitDependencyVersions : IPreBuildTask
    {
        public string Title()
        {
            return "Checking SayKit libraries versions";
        }

        public string Run()
        {
            var androidVersion = SKGradleInjector.NativeVersion;
            var iOSVersion = SKPodVersions.NativeVersion;

            if (string.IsNullOrEmpty(androidVersion))
            {
                return "Failed to get Android native version";
            }

            if (string.IsNullOrEmpty(iOSVersion))
            {
                return "Failed to get iOS native version";
            }

            SKUtils.SaveFile(
                Path.Combine(Application.dataPath, "Resources/SayKit"),
                "saykit_versions.json",
                JsonConvert.SerializeObject(new SayKitLibrariesVersions(androidVersion, iOSVersion, SKManager.Instance.Version.ToString())));

            return string.Empty;
        }
    }

    private class CheckUnityPurchasingVersion : IPreBuildTask
    {
        public string Title()
        {
            return "Checking Inn App Purchasing package version";
        }

        public string Run()
        {
            var request = UnityEditor.PackageManager.Client.List(true);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (!request.IsCompleted)
            {
                if (stopwatch.ElapsedMilliseconds > 3000)
                {
                    break;
                }
            }

            stopwatch.Stop();

            if (request is { IsCompleted: true })
            {
                if (request.Status == UnityEditor.PackageManager.StatusCode.Success)
                {
                    var package = request.Result.FirstOrDefault(p =>
                        p.name.Equals("com.unity.purchasing", StringComparison.OrdinalIgnoreCase));

                    if (package != null)
                    {
                        SKUtils.SaveFile(
                            Path.Combine(Application.dataPath, "Resources/SayKit"),
                            "saykit_iap.json",
                            JsonConvert.SerializeObject(new SayKitIAPVersion(package.version)));
                    }
                }
            }
            else
            {
                SayKitDebug.Log("[PreBuild] CheckUnityPurchasingVersion: Timeout error when loading project packages");
            }

            return string.Empty;
        }
    }

    private class CheckPlatformDefines : IPreBuildTask
    {
        public string Title()
        {
            return "Checking platform defines";
        }

        public string Run()
        {
            var defines = string.Empty;

#if UNITY_EDITOR && UNITY_ANDROID
            defines = SayKitInternal.Editor.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
#endif

#if UNITY_EDITOR && UNITY_IOS
            defines = SayKitInternal.Editor.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
#endif

            if (string.IsNullOrEmpty(defines))
            {
                return string.Empty;
            }

            SKUtils.SaveFile(
                Path.Combine(Application.dataPath, "Resources/SayKit"),
                "saykit_platform_defines.txt",
                defines);

            return string.Empty;
        }
    }

    #endregion

    #region Utils

    private void CheckKyestoreFile()
    {
        var applicationPath = Application.dataPath.Replace("/Assets", "");
        var dirInfo = new DirectoryInfo(applicationPath);

        var applicationFiles = dirInfo.GetFiles();


        foreach (var fileInfo in applicationFiles)
        {
            var fileName = fileInfo.Name;
            if (fileName.Contains("keystore"))
            {
                PlayerSettings.Android.keystoreName = Path.GetFullPath(fileName);
                return;
            }
        }
    }

    private void CheckPurchaseDefine(string appKey)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            NetworkService.PatchAsync($"https://manager.hydra.saygames.io/api/saykit/apps/{appKey}/uses_inapp",
                "saykit", "jADS2eYfa9fa0isdeYfa9fjADS").Wait();
        });
    }

    public string CheckSayKitUiPrefab()
    {
        var result = string.Empty;

        // Check if SayKitUI prefab exists
        if (EditorSceneManager.sceneCountInBuildSettings > 0)
        {
            var scene = EditorSceneManager.GetSceneByBuildIndex(0);
            if (!scene.isLoaded)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(0);
                scene = EditorSceneManager.OpenScene(path);
            }

            var rootObjects = scene.GetRootGameObjects();
            var sayKitUIFound = rootObjects.Any(gameObject => gameObject.GetComponent<SayKitUI>() != null);

            if (!sayKitUIFound)
            {
                result = "Add SayKitUI prefab on the top of first loading scene (build index = 0)";
            }
        }

        return result;
    }

    private static bool CompareFiles(string baseFile, string targetFile)
    {
        var i = 0;
        var baseFileLines = File.ReadAllLines(baseFile);
        var targetFileLines = File.ReadAllLines(targetFile);

        foreach (var item in baseFileLines)
        {
            i++;

            var line = item.Replace(" ", "").Replace("\t", "");

            if (line.Length > 0 && targetFileLines.All(t => t.Replace(" ", "").Replace("\t", "") != line))
            {
                Debug.Log("Cannot find |" + line + "| line number " + i + " in a " + targetFile +
                          " file. Please, compare data from it with a " + baseFile + " file.");
                return false;
            }
        }

        return true;
    }

    private static void DownloadConfigFileWithAttempts(string url, int attempts, out string data,
        out string errorMessage)
    {
        data = string.Empty;
        errorMessage = string.Empty;

        var embedAttempts = 0;
        while (embedAttempts < attempts)
        {
            embedAttempts++;

            var sayKitWebRequest = new SayKitWebRequest(url);
            sayKitWebRequest.SendAndWait(10);

            data = sayKitWebRequest.Text;

            if (sayKitWebRequest.IsDone && string.IsNullOrEmpty(sayKitWebRequest.ErrorMessage))
            {
                if (data.Length > 0 && data[0] == '{')
                {
                    break;
                }
            }
            else
            {
                errorMessage = sayKitWebRequest.ErrorMessage;
            }
        }
    }

    #endregion
}
#endif