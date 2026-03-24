using UnityEngine;
using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace

#endregion

namespace SayKitInternal
{
    internal class RemoteConfigManager
    {
        public static RemoteConfigManager Instance { get; } = new RemoteConfigManager();

        public bool Initialized;
        private bool _shouldInitLocal = true;
        
        public RemoteConfig Config = new RemoteConfig();
        
        private string GetUrl()
        {
            var embedded = Application.version.Split('.').Length == 3 ? "&embedded=1" : string.Empty;
            return "https://app.saygames.io/config/" + SKManager.Instance.Config.appKey
                                                     + "?version=" + Application.version
                                                     + "&saykit=" + SKManager.Instance.Version
                                                     + embedded
                                                     + "&v=3" 
                                                     + "&pretty=1" 
                                                     + "&_=" + UnityEngine.Random.Range(100000000, 900000000);
        }

        private static string GetCachePath()
        {
            var key = "saykit_" + SKManager.Instance.Config.appKey + "_" + Application.version;
            return Path.Combine(Application.persistentDataPath, key);
        }

        public void InitializeLocalConfig(string appKey, string version)
        {
            _shouldInitLocal = false;

            var key = "saykit_" + appKey + "_" + version;
            var cachePath = Path.Combine(Application.persistentDataPath, key);
            var data = string.Empty;

            try
            {
                if (File.Exists(cachePath))
                {
                    data = File.ReadAllText(cachePath);
                }

                if (data.Length == 0 || data[0] != '{')
                {
                    var targetFile = Resources.Load<TextAsset>($"SayKit/{key}");
                    if (targetFile != null)
                    {
                        data = targetFile.text;
                    }
                }

                if (data.Length > 0 && data[0] == '{')
                {
                    Config = JsonConvert.DeserializeObject<RemoteConfig>(data);

#if SAYKIT_DEBUG
                    SayKitDebug.InitDebugLogs(true);
#endif
                }
                else
                {
                    SayKitDebug.LogError("Remote Config is not initialized: no data");
                }
            }
            catch (Exception e)
            {
                _shouldInitLocal = true;
                SKUtils.HandleError( $"[RCM] InitializeLocalConfig data size: {data?.Length}, " +
                                     $" json: {data}, message: {e.Message}, stacktrace: {e.StackTrace}");
            }
        }

        public IEnumerator RequestRemoteConfigUpdate()
        {
            if (_shouldInitLocal)
            {
                InitializeLocalConfig(SKManager.Instance.Config.appKey, Application.version);
            }

            yield return DownloadConfigV3();

            Initialized = true;
        }

        private IEnumerator DownloadConfigV3()
        {
            var url = GetUrl();
            
            var networkError = string.Empty;
            var backupListJson = string.Empty;

            var startTimestamp = SKUtils.currentTimestampMs;

            var task = NetworkService.GetAsString(url, 5, (data, error) =>
            {
                networkError = error;
                backupListJson = data;
            });

            yield return new WaitUntil(() => task.IsCompleted);
            
            if (!string.IsNullOrEmpty(networkError))
            {
                SKUtils.HandleError($"[RemoteConfigManager] NetworkError: {networkError}");
            }

            if (backupListJson?.Length > 0)
            {
                NetworkService.InternetReachability = NetworkReachability.ReachableViaLocalAreaNetwork;
            }

            try
            {
                if (backupListJson?.Length > 0 && backupListJson[0] == '{')
                {
                    var data = backupListJson;

                    Config = JsonConvert.DeserializeObject<RemoteConfig>(data);

                    SKBridgeManager.Instance.TrackEvent(name: "config_loaded"
                        , param1:Config.runtime.disable_rc_v2
                        , param2:(int)(SKUtils.currentTimestampMs - startTimestamp)
                        , extra1: "network"
                        , extra2: "v3");

                    var cachePath = GetCachePath();
                    SayKitDebug.Log("Saving config to " + cachePath);

                    File.WriteAllText(cachePath, data);
                }
                else
                {
                    if (string.IsNullOrEmpty(networkError))
                    {
                        SKUtils.HandleError($"[RemoteConfigManager] Wrong data: {backupListJson}");
                    }
                }
            }
            catch (Exception e)
            {
                SKUtils.HandleError($"[RCM] DownloadConfigV3 data size: {backupListJson?.Length}, " +
                                    $" json: {backupListJson}, message: {e.Message}, stacktrace: {e.StackTrace}");
            }
        }

        public void ConfigInitialized(string data)
        {
            if (data.Length > 0 && data[0] == '{')
            {
                Config = JsonConvert.DeserializeObject<RemoteConfig>(data);

                if (Initialized)
                {
                    SKManager.Instance.Config.remoteConfigUpdated?.Invoke();
                }

                Initialized = true;

                if (Config.runtime.debug == 1)
                {
                    DebugService.Instance.InitDebugService();
                    SayKitDebug.InitDebugLogs(true);
                }

#if SAYKIT_DEBUG
                SayKitDebug.InitDebugLogs(true);
#endif
            }
            else
            {
                SKUtils.HandleError($"[RCM.ConfigInitialized] No data: {data}");
            }
        }

    }
}