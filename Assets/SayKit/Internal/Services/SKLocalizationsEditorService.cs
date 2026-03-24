#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

#region ReSharper

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

#endregion

namespace SayKitInternal
{
    public class SKLocalizationsEditorService
    {
        public static SKLocalizationsEditorService Instance { get; } = new SKLocalizationsEditorService();

        private const string SAYKIT_ASSETS_PATH = "SayKit/Localizations";
        private const string TAG = "[SKLocalizationsEditorService]";
        private const string DEFAULT_LANG = "en";

        private List<KeyValuePair<string, Dictionary<string, string>>> LocalizedMessages { get; } =
            new List<KeyValuePair<string, Dictionary<string, string>>>();

        private string GetCurrentLanguage()
        {
            return SayKitLanguageConverter.ConvertFromSayKitLanguage(SKManager.Instance.Config.overrideSystemLanguage);
        }

        public void PreloadLocalizations()
        {
            SetupLocalMessages(GetCurrentLanguage());
            DownloadRemoteLocalizations();
        }

        private void SetupLocalMessages(string language)
        {
            LocalizedMessages.Clear();
            
            if (language.Equals(DEFAULT_LANG, StringComparison.OrdinalIgnoreCase))
            {
                LoadFromResources(DEFAULT_LANG);
                SKLocalizationService.Instance.LocalizedMessages = LocalizedMessages;
                SayKitDebug.Log($"{TAG} Setup local finished: " +
                                $"{string.Join(",", LocalizedMessages.Select(p => p.Key))}");
                return;
            }
            
            if (!LoadFromResources(language))
            {
                SayKitDebug.LogWarning($"{TAG} Resource for '{language}' not found. Falling back to 'en'.");
            }
            
            LoadFromResources(DEFAULT_LANG);

            SKLocalizationService.Instance.LocalizedMessages = LocalizedMessages;
            SayKitDebug.Log($"{TAG} Setup local finished: " +
                            $"{string.Join(",", LocalizedMessages.Select(p => p.Key))}");
        }

        private bool LoadFromResources(string language)
        {
            try
            {
                var path = $"{SAYKIT_ASSETS_PATH}/saykit_localization_{language}";
                var asset = Resources.Load<TextAsset>(path);

                if (asset == null)
                {
                    SayKitDebug.LogWarning($"{TAG} Resource for '{language}' not found at '{path}'");
                    return false;
                }

                var languageLocalization = JsonConvert.DeserializeObject<SKLanguageLocalization>(asset.text);
                if (languageLocalization != null)
                {
                    var key = SKLocalizationService.Instance.GenerateLanguageKey(languageLocalization.Language, languageLocalization.Hash);
                    LocalizedMessages.Add(new KeyValuePair<string, Dictionary<string, string>>(key, languageLocalization.Strings));
                    SayKitDebug.Log($"{TAG} Loaded from resources: {languageLocalization.Language}-{languageLocalization.Hash}");
                    return true;
                }
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"{TAG} [LoadFromResources] Error with language '{language}': {e}");
            }

            return false;
        }

        private void DownloadRemoteLocalizations()
        {
            var url = $"https://app.saygames.io/localization/{SKUtils.GetAppKey()}";
            var payload = JsonConvert.SerializeObject(new
            {
                Application.version,
                saykit = SKManager.Instance.Version,
                v = 1,
                embedded = true
            });

            SKLocalizationService.Instance.DownloadLocalizationsWithAttempts(
                url,
                payload,
                3,
                (data, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        SayKitDebug.LogWarning($"{TAG} Remote load error: {error}");
                        return;
                    }

                    if (string.IsNullOrEmpty(data))
                    {
                        SayKitDebug.LogWarning($"{TAG} Remote localization data empty");
                        return;
                    }

                    if (!ParseRemoteLocalizations(data, out var remoteLocalizations))
                    {
                        return;
                    }

                    SetupRemoteMessages(remoteLocalizations);
                });
        }

        private void SetupRemoteMessages(SKLanguageLocalization[] remoteLocalizations)
        {
            var resolvedList = new List<KeyValuePair<string, Dictionary<string, string>>>();
            var remoteDict = remoteLocalizations
                .ToDictionary(localization => localization.Language, localization => localization, StringComparer.OrdinalIgnoreCase);

            if (LocalizedMessages.Count == 0)
            {
                var currentLang = GetCurrentLanguage();
                remoteDict.TryGetValue(currentLang, out var remote);
                if (remote == null)
                {
                    remoteDict.TryGetValue("en", out remote);
                }

                if (remote != null)
                {
                    var key = SKLocalizationService.Instance.GenerateLanguageKey(remote.Language, remote.Hash);
                    resolvedList.Add(new KeyValuePair<string, Dictionary<string, string>>(key, remote.Strings));
                    SayKitDebug.Log($"{TAG} Localization '{remote.Language}' updated to hash '{remote.Hash}'");
                }
                else
                {
                    SayKitDebug.LogError($"{TAG} No remote localization found for '{currentLang}' or 'en'");
                }
            }
            else
            {
                resolvedList = ResolveLocalizations(remoteDict);
            }

            SKLocalizationService.Instance.LocalizedMessages = resolvedList;

            SayKitDebug.Log($"{TAG} Setup remote finished:" +
                            $" {string.Join(",", SKLocalizationService.Instance.LocalizedMessages.Select(p => p.Key))}");
        }

        private List<KeyValuePair<string, Dictionary<string, string>>> ResolveLocalizations(
            Dictionary<string, SKLanguageLocalization> remoteDict)
        {
            var resolved = new List<KeyValuePair<string, Dictionary<string, string>>>();

            foreach (var local in LocalizedMessages)
            {
                var lang = SKLocalizationService.Instance.GetLanguageFromKey(local.Key);
                var hash = SKLocalizationService.Instance.GetHashFromKey(local.Key);

                if (remoteDict.TryGetValue(lang, out var remote))
                {
                    if (remote.Hash.Equals(hash))
                    {
                        resolved.Add(local);
                        SayKitDebug.Log($"{TAG} Localization '{lang}' with hash '{hash}' is relevant.");
                    }
                    else
                    {
                        var key = SKLocalizationService.Instance.GenerateLanguageKey(remote.Language, remote.Hash);
                        resolved.Add(new KeyValuePair<string, Dictionary<string, string>>(key, remote.Strings));
                        SayKitDebug.Log($"{TAG} Localization '{lang}' updated to hash '{remote.Hash}'");
                    }
                }
                else
                {
                    resolved.Add(local);
                    SayKitDebug.LogWarning($"{TAG} No remote for '{lang}', using local resource");
                }
            }

            return resolved;
        }

        public void ChangeLanguageRemote(string lang, Action<bool> onLanguageChanged)
        {
            var callback = onLanguageChanged;

            try
            {
                SKLocalizationService.Instance.DownloadLocalizationsWithAttempts(
                    $"https://app.saygames.io/localization/{SKUtils.GetAppKey()}",
                    JsonConvert.SerializeObject(new
                    {
                        Application.version,
                        lang,
                        v = 1,
                        pretty = true
                    }),
                    1,
                    (data, error) =>
                    {
                        if (!string.IsNullOrEmpty(error))
                        {
                            SayKitDebug.LogWarning($"{TAG} Can't load localization for {lang}. {error}\n" +
                                                   $"Language '{lang}' didn't apply. Keeping '{GetCurrentLanguage()}'");
                            callback?.Invoke(false);
                            return;
                        }

                        if (string.IsNullOrEmpty(data))
                        {
                            SayKitDebug.Log($"{TAG} Empty localizations data\n" +
                                            $"Language '{lang}' didn't apply. Keeping '{GetCurrentLanguage()}'");
                            callback?.Invoke(false);
                            return;
                        }

                        if (!ParseRemoteLocalization(data, out var remoteDict))
                        {
                            callback?.Invoke(false);
                            return;
                        }

                        var resolvedList = DetermineFallbackLanguages(lang, remoteDict);

                        LocalizedMessages.Clear();

                        foreach (var lng in resolvedList)
                        {
                            LocalizedMessages.Add(new KeyValuePair<string,
                                Dictionary<string, string>>(SKLocalizationService.Instance.GenerateLanguageKey(
                                remoteDict[lng].Language, remoteDict[lng].Hash), remoteDict[lng].Strings));
                        }

                        var success = resolvedList.Contains(lang);
                        if (success)
                        {
                            SKLocalizationService.currentLanguage = SayKitLanguageConverter.ConvertToSayKitLanguage(lang);
                            SKBridgeManager.Instance.OverrideSystemLanguage(lang);
                            SKLocalizationService.Instance.LocalizedMessages = LocalizedMessages;
                            SayKitDebug.Log($"{TAG} Remote localization applied: {lang}");
                        }

                        callback?.Invoke(success);
                        SayKitDebug.Log($"{TAG} Setup language after changing finished: " +
                                        $" {string.Join(",", SKLocalizationService.Instance.LocalizedMessages.Select(p => p.Key))}");
                    });
            }
            catch (Exception e)
            {
                callback?.Invoke(false);
                SayKitDebug.LogError($"{TAG} [ChangeLanguageRemote] {e}");
            }
        }

        private bool ParseRemoteLocalizations(string data, out SKLanguageLocalization[] remoteLocs)
        {
            try
            {
                remoteLocs = JsonConvert.DeserializeObject<SKLanguageLocalization[]>(data);
                return true;
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"{TAG} Error parsing remote data: {e}");
                remoteLocs = null;
                return false;
            }
        }

        private bool ParseRemoteLocalization(string data, out Dictionary<string, SKLanguageLocalization> remoteLocs)
        {
            remoteLocs = new Dictionary<string, SKLanguageLocalization>(StringComparer.OrdinalIgnoreCase);

            JObject root;
            try
            {
                root = JObject.Parse(data);
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"{TAG} Error parsing JSON: {e}");
                return false;
            }

            var hashes = root["hash"]?.ToObject<Dictionary<string, string>>();
            if (hashes == null || !(root["game_message"] is JArray messages))
            {
                SayKitDebug.LogError($"{TAG} Remote localization missing 'hash' or 'game_message'");
                return false;
            }

            foreach (var kv in hashes)
            {
                var codeLang = kv.Key;
                var hash = kv.Value;
                var messagesDict = messages
                    .OfType<JObject>()
                    .Where(m => m["code"] != null)
                    .ToDictionary(
                        m => m["code"]!.ToString(),
                        m => m[codeLang]?.ToString() ?? string.Empty
                    );

                remoteLocs[codeLang] = new SKLanguageLocalization
                {
                    Language = codeLang,
                    Hash = hash,
                    Strings = messagesDict
                };
            }

            return true;
        }

        private List<string> DetermineFallbackLanguages(string lang, IReadOnlyDictionary<string, SKLanguageLocalization> available)
        {
            var langs = new List<string>();
            
            if (available.ContainsKey(lang)) langs.Add(lang);
            if (!lang.Equals("en") && available.ContainsKey("en")) langs.Add("en");
            if (!langs.Any() && available.Any()) langs.Add(available.Keys.First());
            
            return langs;
        }
    }
}
#endif