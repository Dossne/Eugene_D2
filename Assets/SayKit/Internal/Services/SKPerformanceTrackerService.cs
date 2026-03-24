using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable CanSimplifyDictionaryLookupWithTryGetValue

#endregion

namespace SayKitInternal
{
    public static class SKPerformanceTrackerService
    {
        private static readonly float _histogramStep = 120f / HistogramColumns;
        private const int HistogramColumns = 12;

        private static Dictionary<string, SegmentSampler> _dictSamplers = new Dictionary<string, SegmentSampler>();

        public static void Start(string name, string tag)
        {
            if (SKManager.Instance.RemoteConfig.runtime.sk_performance_service_enabled == 0)
            {
                return;
            }

            if (_dictSamplers.ContainsKey(name))
            {
                SayKitDebug.LogError($"[FPSTracker]: Segment '{name}' with tag '{tag}' isn't closed");
                return;
            }

            var segmentSampler = new SegmentSampler();
            segmentSampler.Start();
            segmentSampler.Tag = tag;
            _dictSamplers[name] = segmentSampler;
        }

        public static void Update()
        {
            if (SKManager.Instance.RemoteConfig.runtime.sk_performance_service_enabled == 0)
            {
                return;
            }

            if (_dictSamplers.Count == 0)
            {
                return;
            }

            foreach (var segmentSampler in _dictSamplers.Values)
            {
                var currentFps = 1f / Time.unscaledDeltaTime;
                segmentSampler.Update(currentFps);
            }
        }

        public static void End(string name, string endTag)
        {
            if (SKManager.Instance.RemoteConfig.runtime.sk_performance_service_enabled == 0)
            {
                return;
            }

            if (!_dictSamplers.ContainsKey(name))
            {
                SayKitDebug.LogError($"[FPSTracker]: Segment '{name}' isn't started");
                return;
            }

            var segmentSampler = _dictSamplers[name];
            var extra = "[" + segmentSampler.Tag + " - " + endTag + "]";
            _dictSamplers.Remove(name);

            SendEvent(new FpsTrackerData(segmentSampler.GetFpsHistogram()), extra);
        }

        private static void SendEvent(FpsTrackerData data, string extra)
        {
            SKBridgeManager.Instance.TrackEvent(name: "sk_fps_performance", param1: Application.targetFrameRate,
                extra1: JsonConvert.SerializeObject(data), extra2: extra);
        }

        private class SegmentSampler
        {
            public string Tag;
            private int[] fpsBins = new int[HistogramColumns];

            public void Start()
            {
                Reset();
            }

            private void Reset()
            {
                fpsBins = new int[HistogramColumns];
            }

            public void Update(float currentFps)
            {
                try
                {
                    var sampleIndex = Mathf.FloorToInt(Mathf.Clamp(currentFps, 0.0f, 119.9f) / _histogramStep);
                    fpsBins[sampleIndex]++;
                }
                catch (Exception e)
                {
                    SKUtils.HandleError($"[FPSTracker]: Update {e.Message}, {e.StackTrace}");
                }
            }

            public int[] GetFpsHistogram()
            {
                return fpsBins;
            }
        }
    }
}