#if PR_CHEAT

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Events;
using Infrastructure.AssetManagement;
using Infrastructure.HapticControl;
using Infrastructure.Popups;
using Infrastructure.SceneManagement;
using Infrastructure.SystemsLifeCycle;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using VContainer;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Infrastructure.Cheat
{
    public class CheatService : ISystemTickable
    {
        public Action OnReduceLevelTimeRequest;
        public Action OnFreezeLevelTimeRequest;

        public Action OnZoomIn;
        public Action OnZoomOut;
        public Action OnSwitchBombDamage;
        public Action OnSetMaxHoleSize;

        public Action OnWinLevelRequest;

        private PopupService popupService;
        private Instantiator instantiator;
        private InputAction cheatAction;
        private CancellationTokenSource cts;
        private CheatMainMenuPopup cheatMainMenu;
        private GameObject ingameDebugConsole;
        private LevelStartedEvent levelStartedEvent;
        private SceneLoadController sceneLoadController;
        private readonly CompositeDisposable disposables;
        private string currentLevelId;

        public bool IsBombDamage { get; private set; } = true;

        private bool openProcess;

        [Inject]
        public CheatService(PopupService popupService, 
                            Instantiator instantiator, 
                            LevelStartedEvent levelStartedEvent,
                            SceneLoadController sceneLoadController)
        {
            this.popupService = popupService;
            this.instantiator = instantiator;
            this.levelStartedEvent = levelStartedEvent;
            this.sceneLoadController = sceneLoadController;
            cts = new CancellationTokenSource();
            disposables = new CompositeDisposable();
        }

        public async UniTask InitializeAsync()
        {
            EnhancedTouchSupport.Enable();
            cheatAction = InputSystem.actions.FindAction("Cheat");
            cheatAction.performed += CheatAction_Performed;
            this.levelStartedEvent.Subscribe(_ => { currentLevelId = levelStartedEvent.LevelId; }).AddTo(disposables);

            ingameDebugConsole = await instantiator.InstantiateAsync("IngameDebugConsole", cancellationToken: cts.Token);
            ingameDebugConsole.SetActive(false);
        }

        void ISystemTickable.Tick()
        {
            if (Touch.activeTouches.Count >= 3 && !openProcess)
            {
                ShowMenuAsync().Forget();
            }
        }

        public void Deinitialize()
        {
            cheatAction.performed -= CheatAction_Performed;

            if (cheatMainMenu != null)
            {
                cheatMainMenu.Deinitialize();
                cheatMainMenu = null;
            }

            if (ingameDebugConsole != null)
            {
                UnityEngine.Object.Destroy(ingameDebugConsole);
                ingameDebugConsole = null;
            }

            cts?.Cancel();
            cts?.Dispose();
            disposables.Dispose();
        }

        public void ReduceLevelTime()
        {
            OnReduceLevelTimeRequest?.Invoke();
        }

        public void FreezeLevelTime()
        {
            OnFreezeLevelTimeRequest?.Invoke();
        }

        public void WinLevel()
        {
            OnWinLevelRequest?.Invoke();
        }


        private async UniTaskVoid ShowMenuAsync()
        {
            openProcess = true;

            if (cheatMainMenu == null)
            {
                cheatMainMenu = await popupService.GetAsync<CheatMainMenuPopup>(cts.Token, true);
                cheatMainMenu.Initialize();
            }

            var levelId = sceneLoadController.IsMetaSceneActive ? "Level: main" : $"Level: {currentLevelId}";
            cheatMainMenu.SetCurrentLevelId(levelId);
            
            cheatMainMenu.Open();
            HapticService.I.HapticLight();

            openProcess = false;
        }

        private void CheatAction_Performed(InputAction.CallbackContext obj)
        {
            ShowMenuAsync().Forget();
        }

        public void SwitchBombDamage()
        {
            IsBombDamage = !IsBombDamage;
        }

        public void ZoomIn()
        {
            OnZoomIn?.Invoke();
        }

        public void ZoomOut()
        {
            OnZoomOut?.Invoke();
        }
    }
}
#endif