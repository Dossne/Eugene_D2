using VContainer;

using Infrastructure.PersistentProgress;
using Infrastructure.Configs;

using Features.Level;
using Features.Events;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Popups;
using System;
using Features.Tutorial;
using Infrastructure.Localization;
using Infrastructure.InputControl;
using Infrastructure.SpriteAtlasControl;


namespace Features.SuperSpeedMode
{
    public class SuperSpeedController : ISavable
    {
        private readonly SuperSpeedConfig superSpeedConfig;
        private readonly LevelService levelService;
        private readonly PopupService popupService;
        private readonly InputUiService inputUiService;
        private readonly SpriteAtlasService spriteAtlasService;

        private int winCounter = 0;
        private bool isPopupShown = false;
        private ItemTutorialPopup activationPopup;

        public int UnlockLevel => superSpeedConfig.UnlockLevel();
        public int AnnounceLevel => superSpeedConfig.AnnounceLevel();
        public int WinCount => winCounter;
        public int MaxWinCount => superSpeedConfig.MaxWinCount();
        public bool IsFeatureEnabled => superSpeedConfig.IsFeatureEnabled();
        public bool IsAnnounced => levelService.CurrentLevelNumber >= AnnounceLevel;
        public bool IsUnlocked => levelService.CurrentLevelNumber >= UnlockLevel;
        public bool IsSuperSpeedActive => IsFeatureEnabled
                                       && IsUnlocked
                                       && winCounter >= superSpeedConfig.MaxWinCount();

        [Inject]
        public SuperSpeedController(ConfigProvider configProvider, 
                                    LevelService levelService, 
                                    PopupService popupService,
                                    LevelFinishEvent levelFinishEvent,
                                    InputUiService inputUiService,
                                    SpriteAtlasService spriteAtlasService)
        { 
            superSpeedConfig = configProvider.SuperSpeedConfig;
            this.levelService = levelService;
            this.popupService = popupService;
            this.inputUiService = inputUiService;
            this.spriteAtlasService = spriteAtlasService;
        }

        public void Load(Infrastructure.PersistentProgress.Progress progress)
        {
            winCounter = progress.superSpeedState.winCounter;
            isPopupShown = progress.superSpeedState.isPopupShown;
        }

        public void Save(Infrastructure.PersistentProgress.Progress progress)
        {
            progress.superSpeedState.winCounter = winCounter;
            progress.superSpeedState.isPopupShown = isPopupShown;
        }

        public void HandleLevelFinish(bool isWin)
        {
            if (!IsFeatureEnabled || levelService.CurrentLevelNumber < UnlockLevel)
                return;

            if (isWin)
                CountWin();
            else
                CountLose();
        }

        public void CountWin() 
        { 
            if (winCounter < MaxWinCount) 
                winCounter++;
        }
        
        public void CountLose()
        {
            isPopupShown = false;
            if (!IsSuperSpeedActive)
                return;
            winCounter = 0;            
        }

        public async UniTask ExecuteScheduledAsync(CancellationToken token)
        {
            if (!IsSuperSpeedActive || isPopupShown)
                return;

            try
            {
                inputUiService.DisableInput();
                await UniTask.WaitUntil(() => !popupService.IsAnyOpened, cancellationToken: token);                
                await OpenPopupAsync(token);
                inputUiService.EnableInput();
                await UniTask.WaitUntil(() => activationPopup.IsOpened, cancellationToken: token);                
                await UniTask.WaitUntil(() => !activationPopup.IsOpened, cancellationToken: token);

            }
            catch (OperationCanceledException e)
            {
                UnityEngine.Debug.LogWarning(e);
                inputUiService.EnableInput();
            }            
        }

        public async UniTask OpenPopupAsync(CancellationToken token)
        {
            if (activationPopup == null)
            {
                activationPopup = await popupService.GetAsync<ItemTutorialPopup>(token, isInstantiateAsync: true);
                activationPopup.Construct(ClosePopup);
                activationPopup.Initialize();
            }
            activationPopup.SetData(LocalizationService.I.Get(LocKeys.SuperSpeedActivationPopup.Header),
                                    string.Empty,
                                    LocalizationService.I.Get(LocKeys.SuperSpeedActivationPopup.Description),
                                    new()
                                    {
                                        icon = spriteAtlasService.GetFromMain("super_speed_icon"),
                                        amountText = string.Empty,
                                        backgroundSprite = null,
                                        isDisplayInfinityIcon = false,
                                        isDisplayRibbon = false
                                    });
            activationPopup.Open();
            isPopupShown = true;
        }

        private void ClosePopup()
        {
            if (activationPopup == null)
                return;

            activationPopup.Close();
        }

        public void CheatAdd()
        {
            if (winCounter < MaxWinCount)
                winCounter++;
        }

        public void CheatMax()
        {
            winCounter = MaxWinCount;
        }

        public void CheatReset()
        {
            winCounter = 0;
        }
    }
}