#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable DuplicatedStatements

#endregion

namespace SayKitInternal
{
    public enum ButtonState
    {
        Install,
        Installed,
        Update,
        Downgrade
    }

    public class SGSVersionManager : EditorWindow
    {
        #region Const

        private const string SGS_PACKAGE_NAME = "io.saygames.services";
        private const string SGS_URL = "https://gitlab.saygames.io/api/v4/projects/257/repository/tags?per_page=50";
        private const string SGS_TOKEN_NAME = "PRIVATE-TOKEN";
        private const string SGS_TOKEN = "glpat-nqb1-zb3jCD4j5Qxirgb";
        private const string SAYGAMES_SERVICES = "SayGames Services";
        private static string[] TABS = { "Release versions", "Experimental versions" };

        #endregion

        #region Variables

        private static List<SayGamesServicesTag> _releaseVersions = new List<SayGamesServicesTag>();
        
        private static List<SayGamesServicesTag> _experimentalVersions = new List<SayGamesServicesTag>();
        private static string _currentSGSVersion;
        private static bool _isLoading;
        private static bool _isInitialized;
        private static SGSVersionManager _currentWindow;
        private static bool _isSubscribed;
        private static bool _isExperimental;
        private int _selectedTab;

        #endregion

        private void OnEnable()
        {
            minSize = new Vector2(280, 270); 
            maxSize = new Vector2(280, 270);
            
            SubscribeToPackageEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromPackageEvents();
        }

        private static void ResetStates()
        {
            _isLoading = false;
            _isInitialized = false;
        }

        public static void ShowGSGVersionManager()
        {
            _currentWindow = GetWindow<SGSVersionManager>("SGS Version Manager");
            _currentWindow?.Show();

            ResetStates();
            GetContent();
        }

        private static void GetContent()
        {
            if (_isLoading || _isInitialized)
            {
                return;
            }

            _isInitialized = true;
            _isLoading = true;

            _currentSGSVersion = GetCurrentVersionFromManifest();

            GetSGSTags();
        }

        private void OnGUI()
        {
            if (!_isInitialized)
            {
                GetContent();
            }

            if (_isLoading)
            {
                GUILayout.Label("Loading versions...");
                return;
            }

            _selectedTab = GUILayout.Toolbar(_selectedTab, TABS);
            var versions = _selectedTab == 0 ? _releaseVersions : _experimentalVersions;

            if (versions.Count == 0)
            {
                GUILayout.Label("No versions available.");
                return;
            }

            GUILayout.Space(10);

            for (var i = 0; i < versions.Count; i++)
            {
                var version = versions[i].Name.Substring(1);

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                GUILayout.Space(15);
                GUILayout.Label(version, GUILayout.Width(150));

                var buttonLabel = GetButtonState(version);
                if (buttonLabel == ButtonState.Installed)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button(buttonLabel.ToString(), GUILayout.Width(100)))
                {
                    HandleButtonClick(version, buttonLabel);
                }

                GUI.enabled = true;

                GUILayout.Space(15);
                GUILayout.EndHorizontal();

                if (_selectedTab == 0 && i == 4)
                {
                    var whiteLineStyle = new GUIStyle
                    {
                        normal =
                        {
                            background = Texture2D.grayTexture 
                        },
                        fixedHeight = 2
                    };

                    GUILayout.Space(5);
                    GUILayout.Box(GUIContent.none, whiteLineStyle, GUILayout.ExpandWidth(true));
                    GUILayout.Space(5);
                }
            }

            GUILayout.Space(10);
        }

        private ButtonState GetButtonState(string version)
        {
            if (_currentSGSVersion == null)
            {
                return ButtonState.Install;
            }

            if (_currentSGSVersion == version)
            {
                return ButtonState.Installed;
            }

            if (!_currentSGSVersion.Contains("-"))
            {
                if (!version.Contains("-"))
                {
                    var comparison = CompareVersions(version, _currentSGSVersion);
                    return comparison == 0
                        ? ButtonState.Installed
                        : comparison > 0
                            ? ButtonState.Update
                            : ButtonState.Downgrade;
                }

                return ButtonState.Install;
            }

            return ButtonState.Install;
        }

        private void HandleButtonClick(string version, ButtonState buttonState)
        {
            _currentSGSVersion = version;
            UpdateManifestFile(_currentSGSVersion);

            switch (buttonState)
            {
                case ButtonState.Install:
                    DisplaySGSDialog("installed", _currentSGSVersion);
                    break;
                case ButtonState.Update:
                    DisplaySGSDialog("updated", _currentSGSVersion);
                    break;
                case ButtonState.Downgrade:
                    DisplaySGSDialog("downgraded", _currentSGSVersion);
                    break;
            }
        }

        private int CompareVersions(string a, string b)
        {
            try
            {
                var v1 = a.Split('.').Select(x => int.TryParse(x, out var val) ? val : 0).ToArray();
                var v2 = b.Split('.').Select(x => int.TryParse(x, out var val) ? val : 0).ToArray();
                var maxLength = Mathf.Max(v1.Length, v2.Length);

                for (var i = 0; i < maxLength; i++)
                {
                    var v1Part = i < v1.Length ? v1[i] : 0;
                    var v2Part = i < v2.Length ? v2[i] : 0;

                    var compVersion = v1Part.CompareTo(v2Part);
                    if (compVersion != 0)
                    {
                        return compVersion;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SGS Integration] Version compare failed: {ex.Message}");
            }

            return 0;
        }

        private static async void GetSGSTags()
        {
            try
            {
                using (var unityWebRequest = UnityWebRequest.Get(SGS_URL))
                {
                    unityWebRequest.SetRequestHeader(SGS_TOKEN_NAME, SGS_TOKEN);
                    var isCompleted = await SendWebRequestWithTimeout(unityWebRequest);

                    if (!isCompleted)
                    {
                        Debug.LogError("[SGS Integration] Request timed out.");
                        return;
                    }

                    if (unityWebRequest.result == UnityWebRequest.Result.Success)
                    {
                        if (!string.IsNullOrEmpty(unityWebRequest.downloadHandler.text))
                        {
                            var allVersions = JsonConvert.DeserializeObject<List<SayGamesServicesTag>>(unityWebRequest.downloadHandler.text);

                            if (allVersions.Count > 0)
                            {
                                _releaseVersions = allVersions
                                    .Where(tag => tag.Name.StartsWith("v1.1.") && !tag.Name.Contains("-"))
                                    .OrderByDescending(tag => DateTime.Parse(tag.Commit.CommittedDate))
                                    .Take(5)
                                    .Concat(
                                        allVersions
                                            .Where(tag => tag.Name.StartsWith("v1.0.") && !tag.Name.Contains("-"))
                                            .OrderByDescending(tag => DateTime.Parse(tag.Commit.CommittedDate))
                                            .Take(5)
                                    )
                                    .ToList();

                                _experimentalVersions = allVersions
                                    .Where(tag => tag.Name.Contains("-"))
                                    .Take(10)
                                    .ToList();
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"[SGS Integration] Request failed: {unityWebRequest.error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SGS Integration] Error occurred while fetching versions: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
                _currentWindow?.Repaint();
            }
        }

        private static async Task<bool> SendWebRequestWithTimeout(UnityWebRequest request)
        {
            var tcs = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();

            operation.completed += _ => tcs.TrySetResult(true);

            var timeoutTask = Task.Delay(5000);
            var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                request.Abort();
                return false;
            }

            return true;
        }

        private static string GetCurrentVersionFromManifest()
        {
            try
            {
                var _manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
                
                if (File.Exists(_manifestPath))
                {
                    var manifestJson = File.ReadAllText(_manifestPath);
                    var manifest = JsonConvert.DeserializeObject<JObject>(manifestJson);

                    if (manifest != null && manifest["dependencies"] is JObject dependencies &&
                        dependencies.TryGetValue(SGS_PACKAGE_NAME, value: out var dependency))
                    {
                        var versionMatch = Regex.Match(dependency.ToString(), @"#v(.+)$");
                        if (versionMatch.Success)
                        {
                            var version = versionMatch.Groups[1].Value;
                            return version;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SGS Integration] Failed to read manifest.json: {ex.Message}");
            }

            return null;
        }

        private void UpdateManifestFile(string version)
        {
            try
            {
                var _manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");

                if (File.Exists(_manifestPath))
                {
                    var manifestJson = File.ReadAllText(_manifestPath);
                    var manifest = JsonConvert.DeserializeObject<JObject>(manifestJson);
                    if (manifest != null)
                    {
                        manifest["dependencies"]![SGS_PACKAGE_NAME] =
                            $"https://gitlab+deploy-token-12:gldt-qfDuc8cx3srNh7X-EigY@gitlab.saygames.io/saykit/sg-services.git?path=/Packages/SayGamesServices#v{version}";
                        File.WriteAllText(_manifestPath, manifest.ToString(Formatting.Indented));
                    }
                }
            }
            catch (Exception e)
            {
                ShowErrorDialog($"[SGS Integration] Failed to update manifest file: {e.Message}");
            }
        }

        private static void ShowErrorDialog(string errorMessage)
        {
            if (EditorUtility.DisplayDialog(
                    "SGS Integration Manager Error",
                    errorMessage,
                    "Retry",
                    "Close"))
            {
                ResetStates();
                GetContent();
            }
            else
            {
                _currentWindow?.Close();
                _currentWindow = null;
                UnsubscribeFromPackageEvents();
            }
        }

        private void DisplaySGSDialog(string command, string version)
        {
            var pressed = EditorUtility.DisplayDialog("SayGamesServices",
                $"SGS package is successfully {command}. Version: {version}",
                "OK");

            if (pressed)
            {
                GetContent();
                _currentWindow?.Repaint();
                UnityEditor.PackageManager.Client.Resolve();
            }
        }

        private static void RegisteringPackagesEventHandler(UnityEditor.PackageManager.PackageRegistrationEventArgs packageRegistrationEventArgs)
        {
            _isLoading = false;

            var affectedPackages = new List<UnityEditor.PackageManager.PackageInfo>();
            affectedPackages.AddRange(packageRegistrationEventArgs.added);
            affectedPackages.AddRange(packageRegistrationEventArgs.changedFrom);
            affectedPackages.AddRange(packageRegistrationEventArgs.changedTo);

            if (affectedPackages.Count > 0)
            {
                foreach (var package in affectedPackages)
                {
                    if (package.displayName.Equals(SAYGAMES_SERVICES))
                    {
                        AddLinkXmlFile();
                    }
                }
            }
        }

        private static void AddLinkXmlFile()
        {
            try
            {
                var destinationPath = "Assets/SGS";
                var directories = Directory.GetDirectories("Library/PackageCache", "io.saygames.services*", SearchOption.TopDirectoryOnly);

                if (directories.Length > 0)
                {
                    var absolutePathLinkXml = Path.Combine(directories[0], "link.xml");

                    if (File.Exists(absolutePathLinkXml))
                    {
                        if (!Directory.Exists(destinationPath))
                        {
                            Directory.CreateDirectory(destinationPath);
                        }

                        File.Copy(absolutePathLinkXml, $"{destinationPath}/link.xml", true);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SGS Integration] AddLinkXmlFile error: {e.Message}");
            }
        }

        private static void SubscribeToPackageEvents()
        {
            if (!_isSubscribed)
            {
                UnityEditor.PackageManager.Events.registeringPackages += RegisteringPackagesEventHandler;
                _isSubscribed = true;
            }
        }

        private static void UnsubscribeFromPackageEvents()
        {
            if (_isSubscribed)
            {
                UnityEditor.PackageManager.Events.registeringPackages -= RegisteringPackagesEventHandler;
                _isSubscribed = false;
            }
        }
    }
}
#endif