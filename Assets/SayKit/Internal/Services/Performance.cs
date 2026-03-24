using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable InconsistentNaming
// ReSharper disable IteratorNeverReturns
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression

#endregion

namespace SayKitInternal
{
    public class FpsCounter
    {
        public static int FreeMemory;
        private static int lastFrames;
        private static float lastTime;
        private static int memoryCounter;
        private static bool _appWasPaused;
        private static int _appFrameRate = Application.targetFrameRate;
        
        public static void Update()
        {
            if (SKManager.Instance.RemoteConfig.runtime.fps_optimisation_enable == 1)
            {
                if (Application.targetFrameRate != 30)
                {
                    Application.targetFrameRate = 30;
                }
            }

            lastFrames++;
        }

        private static void ReportFps()
        {
            var timeSinceUpdate = Time.unscaledTime - lastTime;

            if (timeSinceUpdate > 1.0f)
            {
                var fps = lastFrames / timeSinceUpdate;

                // because unscaledTime counts time in pause.
                if (_appWasPaused)
                {
                    _appWasPaused = false;

                    if (_appFrameRate > 0)
                    {
                        fps = _appFrameRate;
                    }
                    else
                    {
                        fps = 60;
                    }
                }

                lastTime = Time.unscaledTime;
                lastFrames = 0;

                if (fps > 0)
                {
                    SKBridgeManager.Instance.TrackEvent(name: "fps", param1: (int)fps, extra1: PerfomanceManager.GetLastFPS(),
                        extra2: PerfomanceManager.GameContext);
                }
            }
        }

        public static IEnumerator ReportFpsRoutine()
        {
            lastTime = Time.unscaledTime;
            lastFrames = 0;

            while (true)
            {
                memoryCounter++;

                yield return new WaitForSecondsRealtime(20f);
                ReportFps();

                if (memoryCounter >= 3)
                {
                    memoryCounter = 0;

                    SKBridgeManager.Instance.TrackAvailableMemory();
                }

#if UNITY_ANDROID
                FreeMemory = SKBridgeManager.Instance.GetFreeMemory();
#endif
            }
        }

        public static void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                if (!_appWasPaused)
                {
                    _appWasPaused = true;
                }
            }
        }
    }

    public class TagItem
    {
        public TagItem(string name, int timestamp, string extra, int seconds = 0, bool isTimetag = false)
        {
            Name = name;
            Timestapm = timestamp;
            Seconds = seconds;
            IsTimeTag = isTimetag;
            Extra = extra;
        }

        public string Name;
        public int Timestapm;
        public int Spikes;

        public int Seconds;
        public bool IsTimeTag;

        public string Extra;
    }

    public class PerfomanceManager
    {
        private static int _enabled;

        public static float MinFPSRate = 20f;
        public static int MinSpikes = 4;

        private static float _deltaTime;

        private static int _screenSpikes;
        private static int _screenTimestamp;
        private static string _screenName = string.Empty;

        private static int _appFrameRate = Application.targetFrameRate;

        private static List<TagItem> _tagList = new List<TagItem>();
        public static List<string> LastTrackedTags = new List<string>();

        private static string _lastTrackedFPS = string.Empty;
        public static string GameContext = string.Empty;

        private static void UpdateParameters(int minFPSRate, int minSpikes)
        {
            if (minFPSRate > 0)
            {
                MinFPSRate = minFPSRate;
            }

            if (minSpikes > 0)
            {
                MinSpikes = minSpikes;
            }
        }
        
        public static void Update()
        {
            if (_enabled != 1)
            {
                return;
            }

            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        }

        public static IEnumerator StartFPSRoutine(int enabled, int minFPSRate, int minSpikes)
        {
            _enabled = enabled;
            if (_enabled != 1)
            {
                yield break;
            }


            UpdateParameters(minFPSRate, minSpikes);

            if (_appFrameRate <= 0)
            {
                _appFrameRate = 60;
            }

            if (MinFPSRate >= _appFrameRate)
            {
                SKBridgeManager.Instance.TrackEvent(name: "fps_error", param1: _appFrameRate, param2: (int)MinFPSRate);

                _enabled = 0;
                yield break;
            }


            SKBridgeManager.Instance.TrackEvent(name: "fps_routine", param1: _appFrameRate, param2: (int)MinFPSRate, param3: MinSpikes);

            while (true)
            {
                yield return new WaitForSecondsRealtime(1);

                var fps = (int)Mathf.Round(1.0f / _deltaTime);

                SaveFPS(fps);


                if (fps <= MinFPSRate)
                {
                    UpdateSpikes();

                    if (_screenSpikes < int.MaxValue)
                    {
                        _screenSpikes++;
                    }
                }

                CheckTimeTags();
            }
        }
        
        public static string GetLastFPS()
        {
            var lastTrackedFPS = String.Copy(_lastTrackedFPS);
            _lastTrackedFPS = string.Empty;

            return lastTrackedFPS;
        }

        private static void SaveFPS(int fps)
        {
            if (_lastTrackedFPS.Length == 0)
            {
                _lastTrackedFPS += fps;
            }
            else
            {
                _lastTrackedFPS += "," + fps;
            }

            if (_lastTrackedFPS.Length > 1000)
            {
                _lastTrackedFPS = string.Empty;
            }
        }

        public static void UpdateScreen(string screenName)
        {
            if (_enabled != 1)
            {
                return;
            }


            if (_screenName.Length > 0 && _screenSpikes > MinSpikes)
            {
                var timestamp = SKUtils.currentTimestamp - _screenTimestamp;
                SKBridgeManager.Instance.TrackEvent(name: "fps_screen", param1: _screenSpikes, param2: timestamp, extra1: _screenName);
            }

            _screenTimestamp = SKUtils.currentTimestamp;
            _screenName = screenName;
            _screenSpikes = 0;
        }
        
        public static void StartTag(string tagName, string extra)
        {
            if (_enabled != 1)
            {
                return;
            }


            if (_tagList.Exists(t => t.Name == tagName))
            {
                EndTag(tagName);
            }

            _tagList.Add(new TagItem(tagName, SKUtils.currentTimestamp, extra));
        }

        public static void EndTag(string tagName)
        {
            if (_enabled != 1)
            {
                return;
            }
            
            if (_tagList.Exists(t => t.Name == tagName))
            {
                var tag = _tagList.Find(t => t.Name == tagName);
                if (tag.Spikes > MinSpikes)
                {
                    var timestamp = SKUtils.currentTimestamp - tag.Timestapm;
                    SKBridgeManager.Instance.TrackEvent(name: "fps_tag", param1: tag.Spikes, param2: timestamp, extra1: tagName);
                    SaveNewTag(tagName, tag, timestamp);
                }

                _tagList.Remove(tag);
            }
        }

        private static void SaveNewTag(string tagName, TagItem tag, int duration)
        {
            if (SKManager.Instance.RemoteConfig.runtime.debug == 1)
            {
                LastTrackedTags.Add("Tag: " + tagName
                                            + "; extra: " + tag.Extra
                                            + "; spikes: " + tag.Spikes
                                            + "; duration: " + duration
                                            + "; time: " + DateTime.Now.ToString("yyyy-mm-dd hh:mm:ss"));
            }
        }

        public static void StartTimeTag(string tagName, int seconds, string extra)
        {
            if (_enabled != 1)
            {
                return;
            }

            _tagList.Add(new TagItem(tagName, SKUtils.currentTimestamp, extra, seconds, true));
        }

        private static void CheckTimeTags()
        {
            if (_enabled != 1)
            {
                return;
            }

            foreach (var tagItem in _tagList)
            {
                if (tagItem.IsTimeTag)
                {
                    tagItem.Seconds -= 1;

                    if (tagItem.Seconds <= 0 && tagItem.Spikes > 0)
                    {
                        var timestamp = SKUtils.currentTimestamp - tagItem.Timestapm;
                        SKBridgeManager.Instance.TrackEvent(name: "fps_tag", param1: tagItem.Spikes, param2: timestamp, extra1: tagItem.Name);
                    }
                }
            }

            _tagList.RemoveAll(t => t.IsTimeTag && t.Seconds <= 0);
        }

        private static void UpdateSpikes()
        {
            if (_enabled != 1)
            {
                return;
            }

            foreach (var tagItem in _tagList)
            {
                tagItem.Spikes += 1;
            }
        }
    }
}