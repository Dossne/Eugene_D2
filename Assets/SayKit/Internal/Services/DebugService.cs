using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeObjectCreationWhenTypeEvident

#endregion

namespace SayKitInternal
{
    public class DebugService
    {
        public static DebugService Instance { get; } = new DebugService();
        public static string UnityExceptionContext = "";

        private int _lastExceptionEventTimestamp;

        public bool RewardedDisabled = false;
        public bool InterstitialDisabled = false;
        public bool BannerDisabled = false;
        public bool ReloadConfigEnabled = false;

        private Gyroscope _gyroscope;

        private long _lastGyroscopeCheckTimestampMs;
        private Quaternion _prevGyroscopeAttitude;
        private bool _gyroscopeRotateMistake;
        private int _gyroscopeRotateCount;

        public List<SayKitVersionDTO> VersionList = new List<SayKitVersionDTO>();
        private string _selectedDebugVersion;

        private bool _manualRequestCall;
        private readonly int _requestTimeout = 5;

        public long startInitTimeStamp = 0;
        public readonly SayKitAdsInfoBuffer<SayKitAdInfo> SayKitAdsInfoBuffer = new SayKitAdsInfoBuffer<SayKitAdInfo>(100);

        public void InitDebugService()
        {
            if (SKManager.Instance.RemoteConfig.runtime.debug == 1)
            {
                _gyroscope = Input.gyro;
                _gyroscope.enabled = true;
            }
        }

        public void Update()
        {
            if (SKManager.Instance.RemoteConfig.runtime.debug == 1
                && _gyroscope != null)
            {
                if (SKUtils.currentTimestampMs - _lastGyroscopeCheckTimestampMs > 1000)
                {
                    _lastGyroscopeCheckTimestampMs = SKUtils.currentTimestampMs;
                    UpdateGyroData();
                }
            }
        }

        private void UpdateGyroData()
        {
            try
            {
                var diffX = Math.Abs(Math.Abs(_gyroscope.attitude.x) - Math.Abs(_prevGyroscopeAttitude.x));
                var diffY = Math.Abs(Math.Abs(_gyroscope.attitude.y) - Math.Abs(_prevGyroscopeAttitude.y));

                if ((diffX + diffY) > 0.95)
                {
                    _gyroscopeRotateCount++;
                    _gyroscopeRotateMistake = false;
                }
                else
                {
                    if (!_gyroscopeRotateMistake)
                    {

                        _gyroscopeRotateMistake = true;
                    }
                    else
                    {
                        _gyroscopeRotateCount = 0;
                    }
                }

                if (_gyroscopeRotateCount >= 3)
                {
                    SayKitUI.instance.ShowDebugMenu();

                    _gyroscopeRotateCount = 0;
                }

                _prevGyroscopeAttitude = _gyroscope.attitude;
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"[DebugService] UpdateGyroData message: {e.Message}, stacktrace: {e.StackTrace}");
            }
        }

        public void ChangeConfigVersion(string version)
        {
            _selectedDebugVersion = version;
            _manualRequestCall = true;

            UIManager.Instance.StartCoroutine(CheckConfigVersion());
        }

        private string GetVersion()
        {
            if (_selectedDebugVersion?.Length > 0)
            {
                return _selectedDebugVersion;
            }

            return SKManager.Instance.BuildVersion;
        }

        public IEnumerator ReloadRemoteConfig()
        {
            while (SKManager.Instance.RemoteConfig.runtime.debug == 1)
            {
                if (ReloadConfigEnabled)
                {
                    SayKitDebug.Log("SayKit: Reload remote config.");

                    if (!_manualRequestCall)
                    {
                        yield return RemoteConfigInitRoutine();
                    }
                }

                yield return new WaitForSecondsRealtime(15f);

                SayKitDebug.Log("SayKit: Reload remote config. Check");
            }
        }

        public IEnumerator DownloadVersionList()
        {
            string url = "https://app.saygames.io/debug/configs/"
                         + SKManager.Instance.Config.appKey
                         + "?version=" + SKManager.Instance.RuntimeInfo.version
                         + "&device_id=" + SKManager.Instance.RuntimeInfo.idfv
                         + "&idfa=" + SKManager.Instance.RuntimeInfo.idfa
                         + "&saykit=" + SKManager.Instance.Version
                         + "&lng=" + SKBridgeManager.Instance.GetFullCurrentLanguage();

            var versionListJson = string.Empty;
            var requestError = string.Empty;

            var task = NetworkService.GetAsString(url, _requestTimeout, (data, error) =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.Log("SayKit: GetVersionList NetworkError: " + error);
                    
                    requestError = error;
                }
                else
                {
                    versionListJson = data;
                }
            });

            yield return new WaitUntil(() => task.IsCompleted);
            
            if (!string.IsNullOrEmpty(requestError))
            {
                SayKitDebug.Log("DebugService.DownloadVersionList: " + requestError);
                yield break;
            }

            if (string.IsNullOrEmpty(versionListJson))
            {
                SayKitDebug.Log("DebugService.DownloadVersionList: empty data");
                yield break;
            }

            try
            {
                if (versionListJson?.Length > 0 && versionListJson[0] == '{')
                {
                    var versionsArray = JsonConvert.DeserializeObject<SayKitVersionsDTO>(versionListJson);
                    VersionList = versionsArray.Versions.ToList();
                }
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"[DebugService] GetVersionList message: {e.Message}, stacktrace: {e.StackTrace}");
            }
        }

        private IEnumerator CheckConfigVersion()
        {
            var needToUpdate = false;

            var url = "https://app.saygames.io/debug/bind-config/" + SKManager.Instance.Config.appKey;

            var formFields = new Dictionary<string, string>
            {
                { "idfa", SKManager.Instance.RuntimeInfo.idfa },
                { "device_id", SKManager.Instance.RuntimeInfo.idfv },
                { "version", GetVersion() },
                { "saykit", SKManager.Instance.Version.ToString() },
                { "hash", SKManager.Instance.RemoteConfig.runtime.hash }
            };

            var task = NetworkService.Post(url, _requestTimeout, formFields,
                (httpStatusCode, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        SayKitDebug.Log("SayKit: CheckConfigVersion: " + error);
                    }
                    else
                    {
                        if (httpStatusCode == (int)HttpStatusCode.OK)
                        {
                            needToUpdate = true;
                        }
                    }
                });

            yield return new WaitUntil((() => task.IsCompleted));

            if (needToUpdate)
            {
                SKBridgeManager.Instance.RequestRemoteConfigUpdate();
            }

            if (_manualRequestCall)
            {
                UISayKitDebugMenu.GetInstance().ConfigUpdated(needToUpdate);
                _manualRequestCall = false;
            }
        }

        private IEnumerator RemoteConfigInitRoutine()
        {
            var backupListJson = string.Empty;
            var requestError = string.Empty;
            
            var key = "saykit_" + SayKit.config.appKey + "_" + SayKit.runtimeInfo.version;
            var cachePath = Application.persistentDataPath + "/" + key;

            var url = "https://app.saygames.io/config/" + SayKit.config.appKey
                                                        + "?version=" + SayKit.runtimeInfo.version
                                                        + "&device_id=" + SayKit.runtimeInfo.idfv
                                                        + "&idfa=" + SayKit.runtimeInfo.idfa
                                                        + "&saykit=" + SayKit.GetVersion
                                                        + "&lng=" + SKBridgeManager.Instance.GetFullCurrentLanguage()
                                                        + "&_=" + UnityEngine.Random.Range(100000000, 900000000)
                                                        + "&hash=" + SayKit.remoteConfig.runtime.hash
                                                        + "&device_name=" +
                                                        Uri.EscapeDataString(SayKit.runtimeInfo.deviceModel);

            var task = NetworkService.GetAsString(url, 5, (data, error) =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    SayKitDebug.Log("DebugService: DownloadAvailableDatabaseList NetworkError: " + error);
                    requestError = error;
                }
                else
                {
                    backupListJson = data;
                }
            });

            yield return new WaitUntil(() => task.IsCompleted);
            
            if (!string.IsNullOrEmpty(requestError))
            {
                SayKitDebug.Log("DebugService.RemoteConfigInitRoutine: " + requestError);
                yield break;
            }

            if (string.IsNullOrEmpty(backupListJson))
            {
                SayKitDebug.Log("DebugService.RemoteConfigInitRoutine: empty data");
                yield break;
            }

            try
            {
                if (backupListJson?.Length > 0 && backupListJson[0] == '{')
                {
                    RemoteConfigManager.Instance.Config = JsonConvert.DeserializeObject<RemoteConfig>(backupListJson);

                    SayKitDebug.Log($"DebugService: Saving config to {cachePath} ");
                    File.WriteAllText(cachePath, backupListJson);
                }
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"[RCM] RemoteConfigInitRoutine data size: {backupListJson?.Length},  json: {backupListJson}");
                SayKitDebug.LogError($"[RCM] RemoteConfigInitRoutine message: {e.Message}, stacktrace: {e.StackTrace}");
            }
        }

        public void LogExceptionCallback(string condition, string stackTrace, LogType type)
        {
            LogExceptionCallbackBase(condition, stackTrace, type);
        }

        private void LogExceptionCallbackBase(string condition, string stackTrace, LogType type, string extra = "")
        {
            if (type != LogType.Exception)
            {
                return;
            }

            if (SKUtils.currentTimestamp - _lastExceptionEventTimestamp <= 5)
            {
                return;
            }

            _lastExceptionEventTimestamp = SKUtils.currentTimestamp;

            try
            {
                var unityException = new SayKitUnityException
                {
                    Scene = SceneManager.GetActiveScene().name,
                    Exception = condition
                };

                var extraParam = "";
                if (!string.IsNullOrEmpty(UnityExceptionContext))
                {
                    extraParam += $"UnityExceptionContext: {UnityExceptionContext} \n";
                }

                if (string.IsNullOrEmpty(extra))
                {
                    SKBridgeManager.Instance.TrackEvent(
                        name: "sk_unity_exception",
                        extra1: extraParam + $"[DebugService]: exception: {JsonConvert.SerializeObject(unityException)}",
                        extra2: stackTrace);
                }
                else
                {
                    var exceptionInfo = extraParam + $"[DebugService]: exception: {JsonConvert.SerializeObject(unityException)}, " +
                                        $"stackTrace: {stackTrace}";
                    SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception", extra1: exceptionInfo,
                        extra2: TrimExtraString(extra));
                }
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"[DebugService] LogExceptionCallbackBase message: {e.Message}, stacktrace: {e.StackTrace}");
            }
        }

        private string TrimExtraString(string extra)
        {
            if (string.IsNullOrEmpty(extra))
            {
                return string.Empty;
            }

            var count = Math.Min(extra.Length, 3000);
            return extra.Substring(0, count);
        }
        
        public void AddAdInfo(string adTime, string adType, string adNetwork, string creativeId)
        {
            SayKitAdsInfoBuffer.Add(new SayKitAdInfo(adTime, adType, adNetwork, creativeId));
        }
    }
}