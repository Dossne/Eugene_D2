using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

#region ReSharper

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBeMadeStatic.Local

#endregion

namespace SayKitInternal
{
    public class SKLocalizationService
    {
        public static SKLocalizationService Instance { get; } = new SKLocalizationService();
        
        public static SayKitLanguage currentLanguage = SayKitLanguage.English;
        private static string _eventExtra = string.Empty;

        #region Const

        private const string TAG = "[SKLocalizationService]";
        private const string SAYKIT_UNITY_OVERRIDE_SYSTEM_LANGUAGES = "SAYKIT_UNITY_OVERRIDE_SYSTEM_LANGUAGES";
        private const string SAYKIT_ASSETS_PATH = "SayKit/Localizations";

        #endregion

        public List<KeyValuePair<string, Dictionary<string, string>>> LocalizedMessages { get; set; } =
            new List<KeyValuePair<string, Dictionary<string, string>>>();

        public void SaveOverrideLanguage(string language)
        {
            PlayerPrefs.SetString(SAYKIT_UNITY_OVERRIDE_SYSTEM_LANGUAGES, language);
        }

        public SayKitLanguage GetOverrideLanguage()
        {
            return SayKitLanguageConverter.ConvertToSayKitLanguage(PlayerPrefs.GetString(SAYKIT_UNITY_OVERRIDE_SYSTEM_LANGUAGES, string.Empty));
        }

        public bool HasLocalizedMessage(string code)
        {
            return LocalizedMessages.Any(pair => pair.Value.ContainsKey(code));
        }

        public (string, bool) GetLocalizedMessage(string code, string val1 = null, string val2 = null,
            string val3 = null, string val4 = null, string val5 = null)
        {
            var exist = LocalizedMessages.Any(kv => kv.Value.ContainsKey(code));
            
            if (!exist)
            {
                SayKitDebug.Log($"{TAG} Key '{code}' not found. Returning code.");
                return (code, false);
            }
            
            foreach (var pair in LocalizedMessages)
            {
                if (pair.Value.TryGetValue(code, out var text))
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        return (ReplacePlaceholders(text, val1, val2, val3, val4, val5),
                            pair.Key.Split('-')[0].Equals("en", StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            SayKitDebug.LogWarning($"{TAG} GetLocalizedMessage Key '{code}' not found in any localization. Returning original code.");

            return (string.Empty, true);
        }

        private string ReplacePlaceholders(string text, string val1, string val2, string val3, string val4, string val5)
        {
            if (val1 != null) text = text.Replace("{1}", val1);
            if (val2 != null) text = text.Replace("{2}", val2);
            if (val3 != null) text = text.Replace("{3}", val3);
            if (val4 != null) text = text.Replace("{4}", val4);
            if (val5 != null) text = text.Replace("{5}", val5);

            return text;
        }

        public void ChangeLanguage(string language, Action<bool> OnLanguageChanged)
        {
            if (string.IsNullOrEmpty(language))
            {
                SayKitDebug.LogWarning($"{TAG} Empty language code passed to ChangeLanguage method.");
                OnLanguageChanged?.Invoke(false);
                return;
            }

#if UNITY_EDITOR
            SKLocalizationsEditorService.Instance.ChangeLanguageRemote(language, OnLanguageChanged);
#else 
            SKBridgeManager.Instance.OverrideSystemLanguage(language);
            currentLanguage = SayKitLanguageConverter.ConvertToSayKitLanguage(language);

            SetupLocalMessages(language, OnLanguageChanged);
#endif
        }

        public void InitializeLocalGameMessages()
        {
#if UNITY_EDITOR
            SKLocalizationsEditorService.Instance.PreloadLocalizations();
#else
            SetupLocalMessages();
#endif
        }

        public void SetupRemoteMessages(string data)
        {
            try
            {
                SayKitDebug.Log($"{TAG} Remote localization data: {data}");
                
                if (!GetRemoteLocalizationDict(data, out var remoteDict))
                {
                    SayKitBridge.Instance.TrackEvent("sk_localization_unity", extra1: _eventExtra);
                    return;
                }
                
                if (!GetPriorityLanguages(out var priorityLanguages))
                {
                    SayKitBridge.Instance.TrackEvent("sk_localization_unity", extra1: _eventExtra);
                    return;
                }
                
                var localDict = LocalizedMessages
                    .ToDictionary(pair => GetLanguageFromKey(pair.Key), pair => pair);

                var resolvedList = new List<KeyValuePair<string, Dictionary<string, string>>>();

                foreach (var lang in priorityLanguages)
                {
                    var localizationPair = ResolveLocalization(lang, remoteDict, localDict);
                    if (localizationPair.HasValue)
                    {
                        resolvedList.Add(localizationPair.Value);
                    }
                }

                LocalizedMessages = resolvedList;
                var currentLocalizations = string.Join(", ", LocalizedMessages.Select(p => p.Key));
        
                SayKitDebug.Log($"{TAG} Setup remote finished: {currentLocalizations}");
                _eventExtra += $"remote: {currentLocalizations}";
                
                SayKitBridge.Instance.TrackEvent("sk_localization_unity", extra1: _eventExtra);
            }
            catch (Exception e)
            {
                SKUtils.HandleError($"{TAG} SetupRemoteMessages error: {e}");
            }
        }

        private void SetupLocalMessages(string language = "", Action<bool> OnLanguageChanged = null)
        {
            try
            {
                if (string.IsNullOrEmpty(language))
                {
                    var priorityLanguages = JsonConvert.DeserializeObject<string[]>(SKBridgeManager.Instance.PrioritizedLanguages());
                    if (priorityLanguages?.Length > 0)
                    {
                        var priorityLangs = string.Join(", ", priorityLanguages);
                        SayKitDebug.Log($"{TAG} Priority languages: {priorityLangs}");
                        _eventExtra = $"priority: {priorityLangs}" + " / ";

                        foreach (var priorityLanguage in priorityLanguages)
                        {
                            AddLang(priorityLanguage);
                        }
                    }
                    else
                    {
                        SayKitDebug.Log($"{TAG} No prioritized languages provided.");

                        AddLang("en");
                    }
                }
                else
                {
                    LocalizedMessages.Clear();
                    AddLang(language);
                }
                
                OnLanguageChanged?.Invoke(true);

                var currentLocalizations = string.Join(", ", LocalizedMessages.Select(p => p.Key));
                SayKitDebug.Log($"{TAG} Setup local finished: {currentLocalizations}");
                
                _eventExtra += $"local: {currentLocalizations}" + " / ";

            }
            catch (Exception e)
            {
                SKUtils.HandleError($"{TAG} SetupLocalMessages error: {e.Message}, stacktrace: {e.StackTrace}");
            }
        }

        private void AddLang(string language)
        {
            try
            {
                var path = $"{SAYKIT_ASSETS_PATH}/saykit_localization_{language}";
                var asset = Resources.Load<TextAsset>(path);

                if (asset == null)
                {
                    SayKitDebug.LogWarning($"{TAG} Resource for '{language}' not found at '{path}'");
                    return;
                }

                var languageLocalization = JsonConvert.DeserializeObject<SKLanguageLocalization>(asset.text);
                if (languageLocalization != null)
                {
                    var key = GenerateLanguageKey(languageLocalization.Language, languageLocalization.Hash);
                    LocalizedMessages.Add(new KeyValuePair<string, Dictionary<string, string>>(key, languageLocalization.Strings));
                    SayKitDebug.Log($"{TAG} Loaded from resources: {languageLocalization.Language}-{languageLocalization.Hash}");
                }
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"{TAG} [AddLang] Error with language '{language}': {e}");
            }
        }

        public string GenerateLanguageKey(string language, string hash)
        {
            return $"{language}-{hash}";
        }

        public string GetLanguageFromKey(string key)
        {
            return key?.Split('-')[0] ?? string.Empty;
        }

        public string GetHashFromKey(string key)
        {
            return key?.Split('-')[1] ?? string.Empty;
        }

        public void DownloadLocalizationsWithAttempts(string url, string body, int attempts, Action<string, string> responseCallback)
        {
            var localizationAttempts = 0;

            while (localizationAttempts < attempts)
            {
                localizationAttempts++;

                var (data, errorMessage) = NetworkService.PostWithHeaders(url, body, 10);

                if (string.IsNullOrEmpty(errorMessage))
                {
                    responseCallback(data, string.Empty);
                    return;
                }

                if (localizationAttempts == attempts)
                {
                    responseCallback(string.Empty, errorMessage);
                }
            }
        }
        
        private bool GetPriorityLanguages(out string[] priorityLanguages)
        {
            priorityLanguages = JsonConvert.DeserializeObject<string[]>(SKBridgeManager.Instance.PrioritizedLanguages())
                                ?? Array.Empty<string>();

            if (priorityLanguages.Length == 0)
            {
                SayKitDebug.Log($"{TAG} Priority languages is empty.");
                return false;
            }

            SayKitDebug.Log($"{TAG} Priority languages: [{string.Join(",", priorityLanguages)}]");
            return true;
        }
        
        private bool GetLanguageFromPath(string path, out string lang)
        {
            var filename = Path.GetFileNameWithoutExtension(path);

            if (filename.StartsWith("saykit_localization_", StringComparison.Ordinal))
            {
                lang = filename.Substring("saykit_localization_".Length);
                return true;
            }

            if (filename.StartsWith("localization_", StringComparison.Ordinal))
            {
                lang = filename.Substring("localization_".Length);
                return true;
            }

            lang = string.Empty;
            return false;
        }

        private bool GetRemoteLocalizationDict(string data, out Dictionary<string, string> remoteDict)
        {
            remoteDict = new Dictionary<string, string>();
            string[] paths;

            try
            {
                paths = JsonConvert.DeserializeObject<string[]>(data) ?? Array.Empty<string>();
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"{TAG} Failed to parse remote data JSON: {e}");
                return false;
            }

            if (paths.Length == 0)
            {
                SayKitDebug.LogWarning($"{TAG} Remote localization paths are empty.");
                return false;
            }

            foreach (var path in paths)
            {
                if (GetLanguageFromPath(path, out var lang))
                {
                    remoteDict[lang] = path;
                }
                else
                {
                    SayKitDebug.LogError($"{TAG} Invalid filename format: {path}");
                }
            }

            if (remoteDict.Count == 0)
            {
                SayKitDebug.Log($"{TAG} No valid remote localization paths found.");
                return false;
            }

            return true;
        }

        private KeyValuePair<string, Dictionary<string, string>>? ResolveLocalization(
            string language,
            Dictionary<string, string> remoteDictionary,
            Dictionary<string, KeyValuePair<string, Dictionary<string, string>>> localDictionary)
        {
            if (remoteDictionary.TryGetValue(language, out var path))
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    if (!string.IsNullOrEmpty(json))
                    {
                        var languageLocalization = JsonConvert.DeserializeObject<SKLanguageLocalization>(json);
                        if (languageLocalization != null)
                        {
                            var localizationKey = $"{language}-{languageLocalization.Hash}";
                            if (localDictionary.TryGetValue(language, out var currentPair))
                            {
                                var currentHash = GetHashFromKey(currentPair.Key);
                                if (currentHash == languageLocalization.Hash)
                                {
                                    SayKitDebug.Log($"{TAG} Localization '{language}' with hash '{currentHash}' is relevant.");
                                    _eventExtra += $"loc {language}-{currentHash} relevant" + " / ";
                                    return currentPair;
                                }
                            }
                      
                            SayKitDebug.Log($"{TAG} Localization '{language}' updated to hash '{languageLocalization.Hash}'");
                            _eventExtra += $"loc {language}-{languageLocalization.Hash} updated" + " / ";
                            
                            return new KeyValuePair<string,Dictionary<string,string>>(localizationKey, languageLocalization.Strings ?? new Dictionary<string, string>());
                        }
                    }
                }
                else
                {
                    SayKitDebug.LogError($"{TAG} File not exist: {path}");
                }
                
                SayKitDebug.LogError($"{TAG} Can't parse remote '{language}'");
            }
            
            if (localDictionary.TryGetValue(language, out var fallback))
            {
                SayKitDebug.Log($"{TAG} Fallback keep existing '{language}' (no remote).");
                return fallback;
            }
            
            SayKitDebug.Log($"{TAG} No localization for '{language}' in remote or local.");
            _eventExtra += $"no loc '{language}'" + " / ";
            
            return null;
        }
    }
}