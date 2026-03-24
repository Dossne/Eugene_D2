using Infrastructure.Settings;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#pragma warning disable 0162

namespace Infrastructure.QualityGraphicsControl
{
    public class GraphicsQualityHandler
    {
        private static int nativeScreenWidth;
        private static int nativeScreenHeight;


        private int currentQualityLevel;

        private float lowResolutionScale;
        private float highResolutionScale;
        
        
        public int CurrentQualityLevel => currentQualityLevel;
        public UniversalRenderPipelineAsset CurrentRenderPipeline { get; private set; }
        public Vector2Int CurrentResolution { get; private set; }
       

        
        public void Initialize(int level, int fps, bool isFirstLaunch, SettingsData settingsData)
        {
            currentQualityLevel = level;
            lowResolutionScale = settingsData.lowResolutionScale;
            highResolutionScale = settingsData.highResolutionScale;
            
            if(nativeScreenWidth == 0) //soft reset fix
                nativeScreenWidth = Screen.width;
    
            if(nativeScreenHeight == 0) //soft reset fix
                nativeScreenHeight = Screen.height;
            
            QualitySettings.vSyncCount = 0;

            if (isFirstLaunch)
            {
                bool isHighQuality = true;

#if UNITY_IOS || UNITY_IPHONE
            var generation = UnityEngine.iOS.Device.generation;
            isHighQuality = generation is >= UnityEngine.iOS.DeviceGeneration.iPhone7 or UnityEngine.iOS.DeviceGeneration.Unknown;
#endif

#if UNITY_ANDROID
                isHighQuality = SystemInfo.graphicsMemorySize > settingsData.lowGraphicsMemorySize && SystemInfo.systemMemorySize > settingsData.lowSystemMemorySize;
#endif

                currentQualityLevel = isHighQuality ? QualityLevel.High : QualityLevel.Low;
            }

            SetGraphics(currentQualityLevel);
            SetFps(fps);
        }


        public float GetCurrentResolutionScale()
        {
            if (currentQualityLevel == QualityLevel.Low)
                return lowResolutionScale;

            return highResolutionScale;
        }


        public void SetGraphics(int level)
        {
            currentQualityLevel = level;
            QualitySettings.SetQualityLevel(level, true);
            CurrentRenderPipeline = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            SetScreenResolution();
        }

        public void SetFps(int value)
        {
#if UNITY_EDITOR
            return;
#endif
            Application.targetFrameRate = value;
        }


        private void SetScreenResolution()
        {
            float resolutionScale = GetCurrentResolutionScale();

            CurrentResolution = new Vector2Int((int)(nativeScreenWidth * resolutionScale), (int)(nativeScreenHeight * resolutionScale));

            Screen.SetResolution(CurrentResolution.x, CurrentResolution.y, FullScreenMode.FullScreenWindow);
        }
    }
}