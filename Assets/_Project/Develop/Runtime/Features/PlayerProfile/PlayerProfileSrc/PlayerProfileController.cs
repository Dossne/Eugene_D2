using Cysharp.Threading.Tasks;
using Features.Level;
using Features.Social;
using Features.Warnings;
using Features.Widgets;
using Infrastructure.Ads;
using Infrastructure.Configs;
using Infrastructure.Localization;
using Infrastructure.MainUICanvasControl;
using Infrastructure.PersistentProgress;
using Infrastructure.Pool.FloatingText;
using Infrastructure.Popups;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using VContainer;


namespace Features.PlayerProfile
{
    public class PlayerProfileController : ISavable
    {
        public enum RemoteSaveStatus 
        {
            Success      = 0,
            ExplicitName = 1,
            Fail         = 2,
        }

        public Action<RemoteSaveStatus> OnSaveProfile;

        private readonly PlayerProfileConfiguration featureConfiguration;
        private readonly ConfigProvider configProvider;
        private readonly PopupService popupService;
        private readonly LevelService levelService;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly SocialService socialService;
        private PlayerProfilePopup profilePopup;
        private PlayerProfileEditPopup profileEditPopup;
        private readonly Widget widget;
        private readonly WarningService warningService;
        private readonly AnalyticsContextCreator analyticsContextCreator;
        private PlayerProfileData playerProfileData;

        private CancellationTokenSource cts;

        public PlayerProfileData PlayerProfileData => playerProfileData;
        public bool FeatureEnabled => featureConfiguration.Feature.isEnabled;
        public bool TutorialUnlocked => featureConfiguration.Feature.tutorUnlockLevel <= levelService.CurrentLevelNumber;
        public bool TutorialCompleted => playerProfileData.tutorialCompleted;
        public bool ProfilePopupOpened => profilePopup != null && profilePopup.IsOpened;
        public PlayerProfilePopup ProfilePopup => profilePopup;

        [Inject]
        public PlayerProfileController(ConfigProvider configProvider,
                                       PopupService popupService, 
                                       MainUIProvider mainUIProvider,
                                       SpriteAtlasService spriteAtlasService,
                                       LevelService levelService,
                                       SocialService socialService,
                                       WarningService warningService,
                                       AnalyticsContextCreator analyticsContextCreator)
        {
            this.featureConfiguration = configProvider.PlayerProfileConfiguration;
            this.configProvider = configProvider;
            this.popupService = popupService;
            this.levelService = levelService;
            this.spriteAtlasService = spriteAtlasService;
            this.socialService = socialService;
            this.widget = mainUIProvider.HudProvider.PlayerProfileWidget;
            this.warningService = warningService;
            this.analyticsContextCreator = analyticsContextCreator;
        }

        private bool isInit;


        public void Initialize()
        {
            if (isInit)
                return;

            if (featureConfiguration.Feature.isEnabled)
            {
                widget.Construct("", GetCurrentAvatar());
                widget.SetAction(OpenProfilePopup);
                widget.Initialize();                
            }
            HideWidget();           

            cts = new();

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            if (featureConfiguration.Feature.isEnabled)
                widget.Deinitialize();

            if (profilePopup != null)
                profilePopup.Deinitialize();

            if (profileEditPopup != null)
                profileEditPopup.Deinitialize();

            cts.Cancel();
            cts.Dispose();

            isInit = false;
        }

        public void Load(Infrastructure.PersistentProgress.Progress progress)
        {
            playerProfileData = progress.playerProfileData;
            playerProfileData ??= new();
            playerProfileData.avatarId = (   playerProfileData.avatarId == string.Empty 
                                          || !featureConfiguration.Feature.isAvatarTabEnabled
                                          || !featureConfiguration.Avatars.Exists(x => x.avatarId == playerProfileData.avatarId))
                                       ? featureConfiguration.Feature.defaultAvatar 
                                       : playerProfileData.avatarId;

            playerProfileData.displayName = playerProfileData.displayName == string.Empty
                                          ? GenerateDefaultName()
                                          : playerProfileData.displayName;
        }

        public void Save(Infrastructure.PersistentProgress.Progress progress)
        {
            progress.playerProfileData = playerProfileData;
        }

        public void SetTutorialCompleted() 
        {
            playerProfileData.tutorialCompleted = true;
        }

        public void OpenProfilePopup() 
        {
            OpenCurrentPlayerProfilePopupAsync(cts.Token).Forget();
        }

        public void OpenProfileEditPopup()
        {
            OpenProfileEditPopupAsync(cts.Token).Forget();
        }

        public void ShowWidget() 
        {
            if (featureConfiguration.Feature.isEnabled)
                widget.Show();
        }

        public void HideWidget() 
        {
            widget.Hide();
        }

        public bool HasServerConnection() 
        {
            if (!socialService.IsUnlocked())
                return false;

            if (!socialService.IsAuthenticated)
                return false;

            return true;
        }

        public async UniTask OpenProfilePopupAsync(string avatarId, string displayName, int level, int firstWinCount, int totalWinCount, int maxWinStreak, bool isEditEnabled,
                                                   CancellationToken token)
        {
            if (profilePopup == null)
            {
                profilePopup = await popupService.GetAsync<PlayerProfilePopup>(token);
                profilePopup.Construct(spriteAtlasService, OpenProfileEditPopup);
                profilePopup.Initialize();
            }

            var avatar = spriteAtlasService.GetFromMain(avatarId);
            
            profilePopup.RefreshData(avatar, displayName, level, firstWinCount, totalWinCount, maxWinStreak);
            profilePopup.SetEditEnabled(isEditEnabled);
            profilePopup.Open();
        }

        private async UniTaskVoid OpenCurrentPlayerProfilePopupAsync(CancellationToken token)
        {
            var firstWinCount = levelService.LevelStatistics.firstWinCount;
            var totalWinCount = levelService.LevelStatistics.totalWinCount;
            var maxWinStreak = levelService.LevelStatistics.maxWinStreak;
            await OpenProfilePopupAsync(playerProfileData.avatarId, GetCurrentName(), GetCurrentLevel(), firstWinCount, totalWinCount, maxWinStreak, true, token);

            PlayerProfileAnalytics.SendProfileOpened();
        }

        private async UniTaskVoid OpenProfileEditPopupAsync(CancellationToken token)
        {
            if (!HasServerConnection())
            {
                NotConnectedNotification();
                return;
            }  

            if (profileEditPopup == null)
            {
                profileEditPopup = await popupService.GetAsync<PlayerProfileEditPopup>(token);
                profileEditPopup.Construct(spriteAtlasService, configProvider, TrySaveProfile, WrongSymbolNotification);
            }

            profileEditPopup.Initialize();
            profileEditPopup.RefreshData(playerProfileData);            
            profileEditPopup.Open();
        }

        public void WrongSymbolNotification()
        {
            warningService.ShowAlert(LocalizationService.I.Get(LocKeys.PlayerProfileEditPopup.InvalidSymbolAlert), FloatingTextType.RedAlert);
        }

        public void InvalidNameNotification()
        {
            warningService.ShowAlert(LocalizationService.I.Get(LocKeys.PlayerProfileEditPopup.InvalidNameAlert), FloatingTextType.RedAlert);
        }

        public void ExplicitNameNotification()
        {
            warningService.ShowAlert(LocalizationService.I.Get(LocKeys.PlayerProfileEditPopup.ExplicitNameAlert), FloatingTextType.RedAlert);
        }

        public void SuccessNotification()
        {
            warningService.ShowAlert(LocalizationService.I.Get(LocKeys.PlayerProfileEditPopup.SaveSuccessAlert), FloatingTextType.WhiteAlert);
        }

        public void AvatarNotSelectedNotification()
        {
            warningService.ShowAlert(LocalizationService.I.Get(LocKeys.PlayerProfileEditPopup.AvatarNotSelectedAlert), FloatingTextType.WhiteAlert);
        }

        public void NotConnectedNotification()
        {
            warningService.ShowAlert(LocalizationService.I.Get(LocKeys.PlayerProfilePopup.NoConnectionTooltip), FloatingTextType.RedAlert);
        }

        public void SetPlayerProfileData(string displayName, string avatarId)
        {
            playerProfileData.displayName = displayName;
            playerProfileData.avatarId = avatarId;

            if (profilePopup != null)
            {
                var firstWinCount = levelService.LevelStatistics.firstWinCount;
                var totalWinCount = levelService.LevelStatistics.totalWinCount;
                var maxWinStreak = levelService.LevelStatistics.maxWinStreak;
                
                profilePopup.RefreshData(GetCurrentAvatar(),
                                         GetCurrentName(),
                                         GetCurrentLevel(),
                                         firstWinCount, 
                                         totalWinCount,
                                         maxWinStreak);
            }

            if (widget != null)
                widget.Construct("", GetCurrentAvatar());
        }

        private void TrySaveProfile(PlayerProfileData data)
        {
            TrySaveProfileAsync(data).Forget();
        }

        private async UniTaskVoid TrySaveProfileAsync(PlayerProfileData playerProfileData)
        {
            if (!IsValidName(playerProfileData.displayName))
            {
                InvalidNameNotification();
                return;
            }

            if (this.playerProfileData.displayName != playerProfileData.displayName) 
            {
                var saveStatus = await SaveProfileNameToRemote(playerProfileData.displayName);

                if (saveStatus == RemoteSaveStatus.Fail)
                {
                    profileEditPopup.Close();
                    Debug.LogWarning("Failed to save profile name!");
                    NotConnectedNotification();
                    saveStatus = await SaveProfileNameToRemote(this.playerProfileData.displayName);
                    return;
                }

                if (saveStatus == RemoteSaveStatus.ExplicitName)
                {
                    ExplicitNameNotification();
                    saveStatus = await SaveProfileNameToRemote(this.playerProfileData.displayName);
                    return;
                }
                PlayerProfileAnalytics.SendNameChanged(levelService.CurrentLevelNumber, analyticsContextCreator.MainContext);
            }

            if (this.playerProfileData.avatarId != playerProfileData.avatarId)
            {
                var saveStatus = await SaveProfileAvatarToRemote(playerProfileData.avatarId);
                if (saveStatus == RemoteSaveStatus.Fail)
                {
                    profileEditPopup.Close();
                    Debug.LogWarning("Failed to save profile avatar!");
                    NotConnectedNotification();
                    saveStatus = await SaveProfileAvatarToRemote(this.playerProfileData.avatarId);
                    return;
                }
                PlayerProfileAnalytics.SendCosmeticChanged(levelService.CurrentLevelNumber, playerProfileData.avatarId, analyticsContextCreator.MainContext);
            }

            SetTutorialCompleted();
            SuccessNotification();

            SetPlayerProfileData(playerProfileData.displayName, playerProfileData.avatarId);            
            profileEditPopup.RefreshData(playerProfileData);
            profileEditPopup.Close();            
        }

        private Sprite GetCurrentAvatar()
        {
            return spriteAtlasService.GetFromMain(playerProfileData.avatarId);
        }

        private string GetCurrentName()
        {
            return playerProfileData.displayName;
        }

        private int GetCurrentLevel()
        {
            return levelService.CurrentLevelNumber;
        }

        public async UniTask<RemoteSaveStatus> SaveProfileNameToRemote(string name)
        {
            if (!HasServerConnection())
                return RemoteSaveStatus.Fail;

            Nakama.IApiUser userState = null;
            try
            {                 
                userState = await socialService.UpdateUserDisplayNameAsync(name, cts.Token);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Update User name failed! {ex.Message}");
            }

            if (Utils.IsNull(userState))
                return RemoteSaveStatus.Fail;

            if (IsExplicitName(userState.DisplayName))
                return RemoteSaveStatus.ExplicitName;

            return RemoteSaveStatus.Success;
        }


        public async UniTask<RemoteSaveStatus> SaveProfileAvatarToRemote(string avatarId) 
        {
            if (!HasServerConnection())
                return RemoteSaveStatus.Fail;

            Nakama.IApiUser userState = null;

            try
            {
                PlayerProfileAvatarDto playerAvatar = new PlayerProfileAvatarDto()
                {
                    avatarId = avatarId
                };
                userState = await socialService.WriteUserMetaDataAsync(playerAvatar, cts.Token);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Update User Avatar failed! {ex.Message}");
            }

            if (Utils.IsNull(userState))
                return RemoteSaveStatus.Fail;

            return RemoteSaveStatus.Success;
        }

        public bool IsValidName(string input)
        {
            return Regex.IsMatch(input, "^[A-Za-z0-9 ]{3,15}$");
        }


        public PlayerProfileAvatarDto GetPlayerAvatarDto()
        {
            return new PlayerProfileAvatarDto()
            {
                avatarId = PlayerProfileData.avatarId
            };
        }


        private bool IsExplicitName(string input)
        {
            return input.Contains("***");
        }

        private string GenerateDefaultName() 
        {
            return $"{featureConfiguration.Feature.defaultName}{UnityEngine.Random.Range(0, 1000000)}"; 
        }
    }
}