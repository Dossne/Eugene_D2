using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.PlayerProfile;
using Infrastructure.Configs;
using Infrastructure.InputControl;
using Infrastructure.Localization;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Popups;
using Infrastructure.SceneManagement;
using Infrastructure.SpriteAtlasControl;
using UnityEngine;
using UnityEngine.UI;
using static Features.PlayerProfile.PlayerProfileController;

namespace Features.Tutorial
{
    public class PlayerProfileTutorial : TutorialBase
    {
        private readonly ConfigProvider configProvider;
        private readonly PlayerProfileUseData configData;
        private readonly MainUIProvider mainUIProvider;
        private readonly InputUiService inputUiService;
        private readonly PopupService popupService;
        private readonly SceneLoadController sceneLoadController;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly MaskTutorialPanel maskTutorialPanel;
        private readonly PlayerProfileController playerProfileController;
        private PlayerProfilePopup profilePopup;
        private CancellationTokenSource cts;
        private string tooltipId = string.Empty;
        private string pointerId = string.Empty;
        private List<string> complexMaskIds = new();
        private PlayerProfileNamePopup   profileNamePopup;
        private PlayerProfileAvatarPopup profileAvatarPopup;
        private bool isStarted = false;

        public PlayerProfileTutorial(string tutorialId,
                                     MainUIProvider mainUIProvider,
                                     InputUiService inputUiService,
                                     PopupService popupService,
                                     SceneLoadController sceneLoadController,
                                     ConfigProvider configProvider,
                                     SpriteAtlasService spriteAtlasService,
                                     MaskTutorialPanel maskTutorialPanel,
                                     PlayerProfileController playerProfileController) :
            base(tutorialId)
        {
            this.configProvider = configProvider;
            this.configData = configProvider.TutorialConfiguration.GetPlayerProfileUseData();
            this.maskTutorialPanel = maskTutorialPanel;
            this.playerProfileController = playerProfileController;
            this.mainUIProvider = mainUIProvider;

            this.inputUiService = inputUiService;
            this.popupService = popupService;
            this.sceneLoadController = sceneLoadController;
            this.spriteAtlasService = spriteAtlasService;
        }

        public override bool CanBeEqueued()
        {
            return CanBeStarted();
        }

        public override bool CanBeStarted()
        {
            return playerProfileController.FeatureEnabled
                && playerProfileController.TutorialUnlocked
                && playerProfileController.HasServerConnection()
                && sceneLoadController.IsMetaSceneActive;
        }


        public override void Start()
        {
            if (playerProfileController.TutorialCompleted)                
            {
                Complete();
                return;
            }
            isStarted = true;
            cts = new CancellationTokenSource();
            StartAsync(cts.Token).Forget();
        }

        public override void Stop()
        {
            inputUiService.EnableInput();

            if (profileNamePopup != null)
                profileNamePopup.Close();

            if (profileAvatarPopup != null)
                profileAvatarPopup.Close();

            RemoveDynamicElements();
            StopWaitingForPopup();            
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
            isStarted = false;
        }

        private void StartWaitingForPopup()
        {
            popupService.OnChangeState += PopupService_OnChangeState;
        }

        private void StopWaitingForPopup()
        {
            popupService.OnChangeState -= PopupService_OnChangeState;
        }

        private async UniTaskVoid StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.WaitUntil(() => !popupService.IsAnyOpened, cancellationToken: cancellationToken);
                inputUiService.DisableInput();
                await UniTask.WaitForSeconds(0.5f, true, cancellationToken: cancellationToken);
                maskTutorialPanel.Acquire(this);
                AddDynamicElements(mainUIProvider.HudProvider.PlayerProfileWidget.RectTransform, configData);
                if (playerProfileController.ProfilePopupOpened)
                    playerProfileController.ProfilePopup.Close();
                StartWaitingForPopup();
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning($"[CODE] Operation was cancelled: {e.Message}");
            }
            inputUiService.EnableInput();
        }

        private void PopupService_OnChangeState(PopupBase popup, PopupState state)
        {
            if (state is not PopupState.BeginOpen)
                return;

            if (popup is PlayerProfilePopup)
            {
                profilePopup = popup as PlayerProfilePopup;
                RemoveDynamicElements();
                maskTutorialPanel.Release(this);
                StopWaitingForPopup();
                StartNameStep();
            }
        }

        private void StartNameStep()
        {
            OpenProfileNamePopupAsync(cts.Token).Forget();
        }

        private void Complete()
        {
            playerProfileController.SetTutorialCompleted();
            OnComplete?.Invoke(TutorialId);
        }

        private void AddDynamicElements(RectTransform mainRectTransform, PlayerProfileUseData playerProfileUseData)
        {
            complexMaskIds = maskTutorialPanel.AddNewComplexMask(mainRectTransform, false);
            tooltipId = maskTutorialPanel.AddNewTooltip(LocalizationService.I.Get(LocKeys.PlayerProfileTutorial.WidgetTooltip),
                                                        mainRectTransform,
                                                        playerProfileUseData.tooltipOrientation,
                                                        new(playerProfileUseData.tooltipOffsetX,        playerProfileUseData.tooltipOffsetY),
                                                        new(playerProfileUseData.tooltipSizeHorizontal, playerProfileUseData.tooltipSizeVertical));
            
            pointerId = maskTutorialPanel.AddNewPointer(mainRectTransform,
                                                        playerProfileUseData.pointerOrientation,
                                                        new(playerProfileUseData.pointerOffsetX,        playerProfileUseData.pointerOffsetY),
                                                        new(playerProfileUseData.pointerSizeHorizontal, playerProfileUseData.pointerSizeVertical));
        }

        private void RemoveDynamicElements()
        {
            maskTutorialPanel.RemoveComplexMask(complexMaskIds);
            maskTutorialPanel.RemovePointer(pointerId);
            maskTutorialPanel.RemoveTooltip(tooltipId);
        }

        private async UniTaskVoid OpenProfileNamePopupAsync(CancellationToken token)
        {
            if (!(playerProfileController.FeatureEnabled && playerProfileController.HasServerConnection()))
            {
                StopOnBadConnection();
                return;
            }

            if (profileNamePopup == null)
            {
                profileNamePopup = await popupService.GetAsync<PlayerProfileNamePopup>(token);
                profileNamePopup.Construct(configProvider, TrySaveProfileName, playerProfileController.WrongSymbolNotification);
            }

            profileNamePopup.Initialize();
            profileNamePopup.RefreshData(playerProfileController.PlayerProfileData.displayName);
            profileNamePopup.Open();
        }

        private void TrySaveProfileName(string name)
        {
            if (isStarted)
                TrySaveProfileNameAsync(name, cts.Token).Forget();
        }

        private async UniTaskVoid TrySaveProfileNameAsync(string name, CancellationToken token)
        {
            if (!playerProfileController.IsValidName(name))
            {
                playerProfileController.InvalidNameNotification();
                return;
            }

            try
            {
                if (playerProfileController.PlayerProfileData.displayName != name)
                {
                    var saveStatus = await playerProfileController.SaveProfileNameToRemote(name);

                    if (saveStatus == RemoteSaveStatus.Fail || saveStatus == RemoteSaveStatus.ExplicitName)
                    {
                        if (saveStatus == RemoteSaveStatus.ExplicitName)
                        {
                            playerProfileController.ExplicitNameNotification();
                            saveStatus = await playerProfileController.SaveProfileNameToRemote(playerProfileController.PlayerProfileData.displayName);
                        }
                        else
                        {
                            StopOnBadConnection();
                            Debug.LogWarning("Failed to save profile name!");
                        }    
                        return;
                    }
                }

                playerProfileController.SetPlayerProfileData(name, playerProfileController.PlayerProfileData.avatarId);
                OpenProfileAvatarPopupAsync(token).Forget();
                if (profileNamePopup != null)
                    profileNamePopup.Close();
            }
            catch (OperationCanceledException e)
            {
                StopOnBadConnection();
                Debug.LogWarning($"[CODE] Operation was cancelled: {e.Message}");
            }
        }

        private async UniTaskVoid OpenProfileAvatarPopupAsync(CancellationToken token)
        {
            if (!(playerProfileController.FeatureEnabled && playerProfileController.HasServerConnection()))
            {
                StopOnBadConnection();
                return;
            }

            if (profileAvatarPopup == null)
            {          
                profileAvatarPopup = await popupService.GetAsync<PlayerProfileAvatarPopup>(token);
                profileAvatarPopup.Construct(spriteAtlasService, configProvider, TrySaveProfileAvatar);
            }

            profileAvatarPopup.Initialize();
            profileAvatarPopup.RefreshData(playerProfileController.PlayerProfileData.avatarId);
            profileAvatarPopup.Open();
        }


        private void TrySaveProfileAvatar(string avatarId)
        {
            if (isStarted)
                TrySaveProfileAvatarAsync(avatarId).Forget();            
        }


        private async UniTaskVoid TrySaveProfileAvatarAsync(string avatarId)
        {
            if (!(playerProfileController.FeatureEnabled && playerProfileController.HasServerConnection()))
            {
                StopOnBadConnection();
                return;
            }

            try
            {
                if (playerProfileController.PlayerProfileData.avatarId != avatarId)
                {
                    var saveStatus = await playerProfileController.SaveProfileAvatarToRemote(avatarId);
                    if (saveStatus == RemoteSaveStatus.Fail)
                    {                        
                        StopOnBadConnection();
                        return;
                    }
                }
                else if (configProvider.PlayerProfileConfiguration.Feature.defaultAvatar == avatarId)
                {
                    playerProfileController.AvatarNotSelectedNotification();
                }

                if (profileAvatarPopup != null)
                    profileAvatarPopup.Close();

                playerProfileController.SetPlayerProfileData(playerProfileController.PlayerProfileData.displayName, avatarId);
                Complete();
            }
            catch (OperationCanceledException e)
            {
                StopOnBadConnection();
                Debug.LogWarning($"[CODE] Operation was cancelled: {e.Message}");
            }
        }

        private void StopOnBadConnection() 
        {
            playerProfileController.NotConnectedNotification();
            Stop();
        }
    }
}