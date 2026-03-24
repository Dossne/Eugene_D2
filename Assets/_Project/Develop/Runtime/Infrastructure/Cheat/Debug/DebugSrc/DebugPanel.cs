#if PR_CHEAT

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Features.LevelConfiguration;
using Features.LevelSequence;
using Infrastructure.Configs;
using Infrastructure.Utilities;
using IngameDebugConsole;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UI;
using VContainer;

namespace Infrastructure.Cheat.Debug
{
    public class DebugPanel : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private Button fpsBtn;
        [SerializeField] private Button debugConsoleBtn;
        [SerializeField] private Button audioBtn;
        [SerializeField] private Button hapticsBtn;
        [SerializeField] private Button checkLevelsBtn;

        [SerializeField] private List<Button> closeBtns;

        [Header("Prefabs")]
        [SerializeField] private GameObject graphyFpsPf;
        [Header("Links")]
        [SerializeField] private CheatAudioPanel audioPanel;
        [SerializeField] private CheatHapticsPanel hapticsPanel;

        private GameObject graphyFps;
        private DebugLogManager debugConsole;
        private LevelConfig levelConfig;
        private LevelSequenceConfig levelSequenceConfig;


        private bool isInit;

        public bool IsInit => isInit;



        [Inject]
        public void Construct(ConfigProvider configProvider)
        {
            this.levelConfig = configProvider.LevelConfig;
            this.levelSequenceConfig = configProvider.LevelSequenceConfig;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            foreach (var btn in closeBtns)
            {
                btn.onClick.AddListener(Close);
            }

            fpsBtn.onClick.AddListener(ShowHideFps);
            audioBtn.onClick.AddListener(ShowAudio);
            hapticsBtn.onClick.AddListener(ShowHaptics);
            debugConsoleBtn.onClick.AddListener(ShowHideDebugConsole);
            checkLevelsBtn.onClick.AddListener(ValidateLevels);

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            foreach (var btn in closeBtns)
            {
                btn.onClick.RemoveListener(Close);
            }

            fpsBtn.onClick.RemoveListener(ShowHideFps);
            audioBtn.onClick.RemoveListener(ShowAudio);
            hapticsBtn.onClick.RemoveListener(ShowHaptics);
            debugConsoleBtn.onClick.RemoveListener(ShowHideDebugConsole);
            checkLevelsBtn.onClick.RemoveListener(ValidateLevels);

            if (graphyFps != null)
            {
                Destroy(graphyFps);
            }

            if (debugConsole != null)
            {
                Destroy(debugConsole);
            }

            audioPanel.Deinitialize();
            hapticsPanel.Deinitialize();

            isInit = false;
        }


        public void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);

            if (value)
            {
                SetFpsButtonColor();
                SetConsoleButtonColor();
            }
        }


        private void ShowHideFps()
        {
            if (graphyFps == null)
            {
                graphyFps = Instantiate(graphyFpsPf);
                graphyFps.SetActive(false);
            }

            graphyFps.SetActive(!graphyFps.activeSelf);
            SetFpsButtonColor();
        }


        private void ShowHideDebugConsole()
        {
            if (debugConsole == null)
            {
                debugConsole = FindObjectOfType<DebugLogManager>(true); //instanced in Service, we need to see all logs from app start
            }

            debugConsole.gameObject.SetActive(!debugConsole.gameObject.activeSelf);

            SetConsoleButtonColor();
        }


        private void ValidateLevels()
        {
            ValidateLevelsAsync().Forget();            
        }

        private async UniTaskVoid ValidateLevelsAsync()
        {
            var addresses = new HashSet<string>();
            var handle = Addressables.LoadResourceLocationsAsync(levelConfig.LevelAddressableLabel);
            var locations = await handle.ToUniTask();

            foreach (IResourceLocation location in locations)
                addresses.Add(location.PrimaryKey);
            Addressables.Release(handle);

            List<string> allLevelsIds = levelSequenceConfig.GetAllLevelsIds();
            for (int i = 0; i < allLevelsIds.Count; i++)
            {
                string levelId = allLevelsIds[i];
                if (addresses.Contains(levelId))
                    continue;

                if (!levelConfig.TryGetLevelById(levelId, out LevelData result))
                {
                    UnityEngine.Debug.LogError($"<b>{levelId}</b> not found in <b>LevelConfig</b> and addressables!");
                }
            }
            UnityEngine.Debug.Log($"Level Check Done.");
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }


        private void SetFpsButtonColor()
        {
            // fpsBtn.SetColorPressed(graphyFps != null && graphyFps.activeSelf);
        }


        private void SetConsoleButtonColor()
        {
            // debugConsoleBtn.SetColorPressed(debugConsole != null && debugConsole.gameObject.activeSelf);
        }


        private void ShowAudio()
        {
            audioPanel.Initialize();
            audioPanel.Open();
            Close();
        }


        private void ShowHaptics()
        {
            hapticsPanel.Initialize();
            hapticsPanel.Open();
            Close();
        }
    }
}
#endif