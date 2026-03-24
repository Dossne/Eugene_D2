using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

#pragma warning disable CS0162 // Unreachable code detected

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable Unity.UnknownResource
// ReSharper disable HeuristicUnreachableCode

#endregion

namespace SayKitInternal
{
    public class SKUtils
    {
        public static T FindComponentOnRootObjects<T>() where T : Component
        {
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < rootObjects.Length; i++)
            {
                var obj = rootObjects[i].GetComponent<T>();
                if (obj) return obj;
            }

            return null;
        }

        public static int currentTimestamp => Convert.ToInt32(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

        public static long currentTimestampMs => Convert.ToInt64(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);

        public static string GetScriptingDefineSymbols()
        {
            var targetFile = Resources.Load<TextAsset>($"SayKit/saykit_platform_defines");
            return targetFile != null ? targetFile.text : string.Empty;
        }
        
        public static T DeepCopy<T>(T obj)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
    
            var json = JsonConvert.SerializeObject(obj, settings);
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public static string CheckNullString(string value)
        {
            return value ?? string.Empty;
        }

        public static string GetIAPVersion()
        {
            var version = string.Empty;

            try
            {
                var textAsset = Resources.Load<TextAsset>("SayKit/saykit_iap");
                if (textAsset != null)
                {
                    var iapVersion = JsonConvert.DeserializeObject<SayKitIAPVersion>(textAsset.text);
                    if (iapVersion != null)
                    {
                        version = iapVersion.Version;
                    }
                }
            }
            catch (Exception e)
            {
                HandleError($"[SKUtils][GetIAPVersion] error {e}");
            }

            return version;
        }

        public static void SaveFile(string path, string fileName, string content)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var filePath = Path.Combine(path, fileName);
                File.WriteAllText(filePath, content);
            }
            catch (Exception e)
            {
               HandleError($"[SKUtils][SaveFile] error: {e.Message}");
            }

        }

        public static void HandleError(string message)
        {
            SayKitDebug.LogError(message);
            SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception", extra1: message);
        }
        
        public static string GetAppKey()
        {
#if UNITY_IOS
            return SayKitApp.APP_KEY_IOS;
#elif UNITY_ANDROID
            return SayKitApp.APP_KEY_ANDROID;
#else
            return string.Empty;
#endif
        }

        internal static void SetupCanvas(Canvas canvas)
        {
            if (canvas == null) return;

            canvas.sortingOrder = SayKit.config.canvasSortingOrder;
        }

        public static bool CheckXcodeVersion16()
        {
#if UNITY_EDITOR_OSX
            var process = new Process();
            process.StartInfo.FileName = "xcodebuild";
            process.StartInfo.Arguments = "-version";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var lines = output.Split('\n');
            var versionRegex = new Regex(@"Xcode (\d+)\.(\d+)");
            var match = versionRegex.Match(lines.Length > 0 ? lines[0] : "Unknown version");
            if (match.Success)
            {
                if (int.Parse(match.Groups[1].Value) >= 16)
                {
                    return true;
                }
            }

            return false;
#endif
            return false;
        }
        
        public static void CheckInit(Action action)
        {
            if (SKManager.Instance.IsInitialized)
            {
                action?.Invoke();
            }
            else
            {
                SayKitDebug.LogError("Wait for SayKit initialization.");
            }
        }
        
        public static IEnumerator FetchServerTimestamp(Action<bool, long> onTimestampReceived)
        {
            if (onTimestampReceived == null)
            {
                Debug.LogError("[GetServerTimestamp] Callback onTimestampReceived is null.");
                yield break;
            }
            
            string result = null;
            string error = null;

            var task = NetworkService.GetAsString("https://live.saygames.io/runtime", 5, (response, e) =>
            {
                result = response;
                error  = e;
            });

            yield return new WaitUntil(() => task.IsCompleted);

            try
            {
                if (!string.IsNullOrEmpty(result) && string.IsNullOrEmpty(error))
                {
                    var data = JsonConvert.DeserializeObject<SayKitLiveRuntimeResponse>(result);
                    if (data != null && data.Timestamp > 0)
                    {
                        onTimestampReceived(true, data.Timestamp);
                        yield break;
                    }
                }
            }
            catch (Exception e)
            {
                HandleError($"[GetServerTimestamp] {e}");
            }

            onTimestampReceived(false, 0);
        }

    }
}