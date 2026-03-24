using UnityEngine;
using UnityEngine.SceneManagement;

#region ReSharper

// ReSharper disable CheckNamespace

#endregion

namespace SayKitInternal
{
    internal class UIManager: MonoBehaviour
    {
        private static bool _isInitializationRequired;

        public static UIManager Instance { get; private set; }

        internal static void Init()
        {
            if(Instance == null)
            {
                var sayKitObject = new GameObject("[SayKit]");
                DontDestroyOnLoad(sayKitObject);

                Instance = sayKitObject.AddComponent<UIManager>();
            }

            // checks whether the active scene exists, is already loaded and available to work with
            if (SceneManager.sceneCount == 0 || !SceneManager.GetActiveScene().isLoaded)
            {
                _isInitializationRequired = true; // defer initialization
                return;
            }

            SayKitUI.getInstance();
            
            SayKitInterstitialCirclePopup.GetInstance();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoad()
        {
            // do deferred initialization if required
            if (_isInitializationRequired) Init();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            FpsCounter.OnApplicationPause(pauseStatus);
        }
        
        private void Update()
        {
            if (SKManager.Instance.IsInitialized)
            {
                FpsCounter.Update();
                PerfomanceManager.Update();
                SKPerformanceTrackerService.Update();                
                DebugService.Instance.Update();
            }
        }

        private void OnEnable()
        {
            Application.logMessageReceived += DebugService.Instance.LogExceptionCallback;
            
#if UNITY_EDITOR
            if (UnityEditor.EditorSettings.enterPlayModeOptionsEnabled)
            {
                if ((UnityEditor.EditorSettings.enterPlayModeOptions & UnityEditor.EnterPlayModeOptions.DisableDomainReload) != 0)
                {
                    UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                }
            }
#endif
        }

        private void OnDisable()
        {
           Application.logMessageReceived -= DebugService.Instance.LogExceptionCallback;
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }
        
#if UNITY_EDITOR
        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode)
            {
                SKManager.Instance.InitCount = 0;
            }
        }
#endif

    }
}
