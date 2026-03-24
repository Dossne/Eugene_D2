#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable RedundantUsingDirective
// ReSharper disable InconsistentNaming

#endregion

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

using Newtonsoft.Json;
using SayKitInternal;
using UnityEngine.UI;

public class CrossPromoService
{
    public static CrossPromoService Instance { get; } = new CrossPromoService();
    
    private static bool _wasInitialized;
    private static bool _isClickProcessing;
    private static string _skadData = string.Empty;
    private static int _currentLine = -1;

    private static CrossPromoResponseLine[] _lines = Array.Empty<CrossPromoResponseLine>();
    private static bool _isInitialized;
    private static List<string> _cachedExternalIds = new List<string>();

    private const string TAG = "[CrossPromo]";
    
    private GameObject _gameObject;
    private Button _crossPromoButton;
    private VideoPlayer _videoPlayer;

    public void Init()
    {
        if (_wasInitialized)
        {
            SayKitDebug.Log($"{TAG} init was started earlier.");
            return;
        }

        _wasInitialized = true;

        UIManager.Instance.StartCoroutine(InitRoutine());
    }

    private static string GetUrl()
    {
#if UNITY_IOS
        var platform = "ios";
#elif UNITY_ANDROID
        var platform = "android";
#else
        var platform = "unknown";
#endif

        return "https://app.saygames.io/promo/v1?bundle=" + Application.identifier
                                                          + "&os=" + platform
                                                          + "&idfa=" + SKManager.Instance.RuntimeInfo.idfa
                                                          + "&device_id=" + SKManager.Instance.RuntimeInfo.idfv
                                                          + "&lang=" + SKManager.Instance.RuntimeInfo.language
                                                          + "&saykit=" + SKManager.Instance.Version
                                                          + "&_=" + UnityEngine.Random.Range(100000000,
                                                              900000000)
                                                          + "&memory=" + SystemInfo.systemMemorySize
                                                          + "&device_os=" +
                                                          Uri.EscapeDataString(SystemInfo.operatingSystem)
                                                          + "&device_name=" +
                                                          Uri.EscapeDataString(SKManager.Instance.RuntimeInfo
                                                              .deviceModel);
    }

    private static IEnumerator InitRoutine()
    {
        RemoveCachedCreatives(true);

        SayKitDebug.Log("Getting cross promo config from " + GetUrl());

        var responseData = string.Empty;
        var requestError = string.Empty;

        yield return GetCrossPromoData((data, error) =>
        {
            responseData = data;
            requestError = error;
        });

        if (!string.IsNullOrEmpty(requestError) || string.IsNullOrEmpty(responseData))
        {
            yield break;
        }

        yield return ProcessingCrossPromoResponse(responseData);

        _isInitialized = true;
    }

    private static IEnumerator GetCrossPromoData(Action<string, string> onCompleted)
    {
        var task = NetworkService.Post(GetUrl(), 3, GetCachedExternalIds(), onCompleted);
        yield return new WaitUntil(() => task.IsCompleted);
    }

    private static IEnumerator ProcessingCrossPromoResponse(string responseData)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<CrossPromoResponse>(responseData);
            _lines = response?.Lines ?? Array.Empty<CrossPromoResponseLine>();

            var externalIds = response?.Lines
                .Select(line => line.ExternalId)
                .ToArray();
            
            var jsonExternalIds = JsonConvert.SerializeObject(externalIds);

            SKBridgeManager.Instance.TrackEvent(name: "cross_config", extra1: jsonExternalIds);

            SayKitDebug.Log($"Cross config: - {jsonExternalIds}");
        }
        catch (Exception e)
        {
            HandleError($"Error parsing response. Message: {e.Message}, Data Size: {responseData?.Length}");
            yield break;
        }

        UpdateCachedCreatives();

        yield return DownloadCrossPromoCreatives();

        CheckAppInstalledAndCachedCreatives();
    }

    private static void UpdateCachedCreatives()
    {
        if (_lines.Length > 0 && _cachedExternalIds.Count > 0)
        {
            var actualExternalIds = _lines.Select(line => line.ExternalId).ToList();

            foreach (var externalId in _cachedExternalIds.Except(actualExternalIds).ToList())
            {
                RemoveCachedCreatives(false, externalId);
            }
        }
    }

    private static void CheckAppInstalledAndCachedCreatives()
    {
        try
        {
            if (_lines.Length == 0)
            {
                SayKitDebug.LogWarning($"{TAG}[CheckAppInstalledAndCachedCreatives] Cross promo lines are empty.");
                return;
            }

            var installedDict = new Dictionary<string, string>();
            var cachedDict = new Dictionary<string, string>();
            var groups = _lines.GroupBy(line => line.ExternalId).ToList();

            foreach (var group in groups)
            {
                var externalId = group.Key;
                var titles = group.Select(line => line.AppTitle).Where(title => !string.IsNullOrEmpty(title)).Distinct().ToList();
                var titleLabel = titles.Count == 1 ? titles[0] : string.Join(", ", titles);

                var schemes = group.Select(line => line.AppScheme).Where(scheme => !string.IsNullOrEmpty(scheme)).Distinct().ToList();
                var isInstalled = false;

                if (schemes.Count > 0)
                {
                    foreach (var scheme in schemes)
                    {
                        var schemeInstalled = SKBridgeManager.Instance.IsAppInstalled(scheme);

                        if (schemeInstalled)
                        {
                            isInstalled = true;
                            break;
                        }
                    }
                }

                var isCached = false;
                foreach (var item in group)
                {
                    var localName = item.GetLocalName();
                    var localPath = Path.Combine(Application.persistentDataPath, localName);
                    var exists = File.Exists(localPath);

                    if (exists)
                    {
                        isCached = true;
                    }
                }

                foreach (var item in group)
                {
                    if (isInstalled && !item.WasInstalled)
                    {
                        item.WasInstalled = true;
                    }

                    if (isCached && !item.WasLoaded)
                    {
                        item.WasLoaded = true;
                    }
                }

                if (isInstalled && !installedDict.ContainsKey(externalId))
                {
                    installedDict[externalId] = titleLabel;
                }

                if (isCached && !cachedDict.ContainsKey(externalId))
                {
                    cachedDict[externalId] = titleLabel;
                }
            }

            TrackEvent("cross_installed", installedDict, "No installed apps", "Installed apps");
            TrackEvent("cross_cached", cachedDict, "No cached creatives", "Cached creatives");
        }
        catch (Exception e)
        {
            HandleError($"[CheckAppInstalledAndCachedCreatives] Exception: {e}");
        }
    }

    private static IEnumerator DownloadCrossPromoCreatives()
    {
        if (_lines.Length == 0)
        {
            SayKitDebug.LogWarning($"{TAG}[DownloadCrossPromoCreatives] lines is empty");
            yield break;
        }

        var downloadedDict = new Dictionary<string, string>();

        var uniqueLines = _lines
            .GroupBy(line => line.ExternalId)
            .Select(responseLines => responseLines.First())
            .ToList();

        foreach (var line in uniqueLines)
        {
            if (line.WasInstalled)
            {
                SayKitDebug.Log($"app_title: {line.AppTitle} with {line.ExternalId} already installed.");
                continue;
            }

            var localPath = Path.Combine(Application.persistentDataPath, line.GetLocalName());

            if (!File.Exists(localPath))
            {
                bool downloadingError = false;
                byte[] downloadingData = null;

                var task = NetworkService.GetAsByteArray(line.CreativeUrl, 3,
                    (data, error) =>
                    {
                        if (!string.IsNullOrEmpty(error))
                        {
                            downloadingError = true;
                        }
                        else
                        {
                            downloadingData = data;
                        }
                    }
                );

                yield return new WaitUntil(() => task.IsCompleted);

                if (downloadingError || downloadingData == null)
                {
                    SayKitDebug.LogError($"Download error: {downloadingError}." +
                                         $" (external_id: {line.ExternalId}, creative_url: {line.CreativeUrl}).");
                    continue;
                }

                try
                {
                    new FileSystemService(line.GetLocalName()).SaveFile(downloadingData);

                    downloadedDict[line.ExternalId] = line.AppTitle;
                }
                catch (Exception e)
                {
                    HandleError($"[DownloadCrossPromoCreatives] Error saving file. Line: {line.CreativeUrl}, Exception: {e}");
                }
            }
        }

        TrackEvent("cross_downloaded", downloadedDict, "No downloaded cross promo creatives", "Download creatives");
    }

    private static bool IsAvailable()
    {
        if (!_isInitialized)
        {
            return false;
        }

        if (SKManager.Instance.IsPremium)
        {
            return false;
        }

        if (_lines.Length > 0)
        {
            return _lines.Any(sayPromoResponseLine => sayPromoResponseLine.IsReady());
        }

        return false;
    }

    private static void NextLine()
    {
        var limit = 100;
        while (limit > 0)
        {
            _currentLine++;
            if (_currentLine >= _lines.Length)
            {
                _currentLine = 0;
            }

            if (_lines[_currentLine].IsReady())
            {
                break;
            }

            limit--;
        }
    }

    public void Show(GameObject gameObject, Button crossPromoButton, VideoPlayer videoPlayer)
    {
        try
        {
            if (!_isInitialized) {
                SayKitDebug.LogWarning($"{TAG} service not initialized.");
                return;
            }
            
            if (gameObject == null || crossPromoButton == null || videoPlayer == null)
            {
                SayKitDebug.LogError($"{TAG}[Show] Invalid arguments.");
                return;
            }

            _gameObject = gameObject;
            _crossPromoButton = crossPromoButton;
            _videoPlayer = videoPlayer;

            if (!IsAvailable())
            {
                return;
            }
            
            _skadData = string.Empty;

            NextLine();

            if (_videoPlayer.isPlaying)
            {
                _videoPlayer.Stop();
            }

            _videoPlayer.url = Path.Combine(Application.persistentDataPath, _lines[_currentLine].GetLocalName());

            _isClickProcessing = false;

            SKBridgeManager.Instance.TrackEvent(name: "cross_show", extra1: _lines[_currentLine].AppTitle,
                extra2: _lines[_currentLine].ExternalId);

            SayKitDebug.Log($"{TAG} Show: {_lines[_currentLine].AppTitle} - {_lines[_currentLine].ExternalId}");

            if (_lines[_currentLine].HasCatalog)
            {
                SKBridgeManager.Instance.TrackSayCatalogueOffer("crosspromo");
            }

            UIManager.Instance.StartCoroutine(TrackImpression());

            _crossPromoButton.onClick.RemoveListener(Click);
            _crossPromoButton?.onClick.AddListener(Click);

            _gameObject.SetActive(true);
            _videoPlayer.Play();
        }
        catch (Exception e)
        {
            HandleError($"[Show] Message: {e.Message}, stacktrace: {e.StackTrace}",
                _lines[_currentLine].ExternalId);
        }
    }

    private static IEnumerator TrackImpression()
    {
        var task = NetworkService.GetAsString(_lines[_currentLine].ImpressionUrl, 5,
            (data, error) =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    SayKitDebug.LogError($"{TAG} TrackImpression NetworkError: " + error);
                }
                else
                {
                    _skadData = data;
                }
            });

        yield return new WaitUntil(() => task.IsCompleted);

        if (!string.IsNullOrEmpty(_skadData))
        {
            SayKitDebug.Log($"{TAG}: " + _skadData);
        }
    }

    private void Click()
    {
        if (_gameObject)
        {
            if (_gameObject.activeSelf)
            {
                if (_isClickProcessing)
                {
                    return;
                }

                if (_lines.Length > 0)
                {
                    _isClickProcessing = true;

                    SKBridgeManager.Instance.TrackEvent(name: "cross_click", extra1: _lines[_currentLine].AppTitle,
                        extra2: _lines[_currentLine].ExternalId);

                    if (_lines[_currentLine].HasCatalog)
                    {
                        SKBridgeManager.Instance.ShowSayCatalogue("crosspromo", _lines[_currentLine].CatalogParams);

                        _isClickProcessing = false;
                    }
                    else
                    {
                        var clickId = GenerateClickId();
                        var clickUrl = _lines[_currentLine].ClickUrl + "&click_id=" + clickId;
                        SKBridgeManager.Instance.TrackEvent(name: "cross_click_id", extra1: clickId,
                            extra2: _lines[_currentLine].ExternalId);

                        UIManager.Instance.StartCoroutine(ClickRoutine(clickUrl));
                    }
                }
            }
        }
    }

    private static IEnumerator ClickRoutine(string clickUrl)
    {
        var request = UnityWebRequest.Get(clickUrl);
        request.redirectLimit = 0;
        yield return request.SendWebRequest();

        if (_lines.Length > 0)
        {
            if (string.IsNullOrEmpty(_lines[_currentLine].ResultUrl))
            {
#if UNITY_IOS
                var isParsed = int.TryParse(_lines[_currentLine].AppStoreId, out int storeId);
                if (isParsed)
                {
                    SKBridgeManager.Instance.OpenStoreProductView(storeId, _skadData,
                        "https://itunes.apple.com/app/apple-store/id" + _lines[_currentLine].AppStoreId);
                }

#elif UNITY_ANDROID
                Application.OpenURL("https://play.google.com/store/apps/details?id=" + _lines[_currentLine].AppStoreId);
#endif
            }
            else
            {
                Application.OpenURL(_lines[_currentLine].ResultUrl);
            }

            _isClickProcessing = false;
        }
    }

    private static string GenerateClickId()
    {
        const string chars = "0123456789qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
        var result = new System.Text.StringBuilder();
        for (var i = 0; i < 10; i++)
        {
            result.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
        }

        return result.ToString();
    }

    private static void RemoveCachedCreatives(bool checkPreviousSaving, string externalId = "")
    {
        try
        {
            if (checkPreviousSaving)
            {
                foreach (var creative in Directory.GetFiles(Application.persistentDataPath, "saypromo_*"))
                {
                    if (File.Exists(creative))
                    {
                        File.Delete(creative);
                    }
                }
            }
            else
            {
                var creativePath = Path.Combine(Application.persistentDataPath, "crosspromo_" + externalId + ".mp4");

                if (File.Exists(creativePath))
                {
                    File.Delete(creativePath);
                }
            }
        }
        catch (Exception e)
        {
            HandleError($"Removing creatives from cache exception {e.Message}, {e.StackTrace}");
        }
    }

    private static string GetCachedExternalIds()
    {
        _cachedExternalIds = Directory.GetFiles(Application.persistentDataPath, "crosspromo_*.mp4")
            .Select(filePath => Path.GetFileNameWithoutExtension(filePath).Replace("crosspromo_", ""))
            .ToList();

        return _cachedExternalIds.Count > 0 ? JsonConvert.SerializeObject(_cachedExternalIds) : string.Empty;
    }

    private static void HandleError(string errorMessage, string extra = "")
    {
        SayKitDebug.LogError($"{TAG}{errorMessage}");
        SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception", extra1: errorMessage, extra2: extra);
    }

    private static void TrackEvent(string eventName, Dictionary<string, string> dictionary, string extraMessage, string log)
    {
        try
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                SKBridgeManager.Instance.TrackEvent(
                    name: eventName,
                    extra1: extraMessage
                );

                SayKitDebug.Log($"{TAG} {log}: - {extraMessage}");
            }
            else
            {
                var json = JsonConvert.SerializeObject(dictionary);

                SKBridgeManager.Instance.TrackEvent(
                    name: eventName,
                    extra1: json
                );

                SayKitDebug.Log($"{TAG} {log}: - {json}");
            }
        }
        catch (Exception e)
        {
            HandleError($"[TrackEvent]:{eventName}] exception: {e}");
        }
    }
    
    public void OnVideoPlayerError(VideoPlayer source, string message)
    {
        SayKitDebug.LogError($"{TAG}[OnVideoPlayerError] VideoPlayer error: " + message);
        _gameObject?.SetActive(false);
        SKBridgeManager.Instance.TrackEvent("cross_error", extra1: message);
    }

    public void Hide()
    {
        _crossPromoButton?.onClick.RemoveListener(Click);
        _videoPlayer?.Stop();
        _gameObject?.SetActive(false);
        
        _crossPromoButton = null;
        _videoPlayer = null;
        _gameObject = null;
    }

}