using Cysharp.Threading.Tasks;
using Features.Level;
using Features.PlayerProfile;
using Infrastructure.Configs;
using Infrastructure.Utilities;
using Nakama;
using Newtonsoft.Json;
using R3;
using SayGames.Services;
using SayGames.Services.Logging;
using System;
using System.Threading;
using UnityEngine;


namespace Features.Social
{
    public class SocialService
    {
        private static readonly string ServerScheme = "https";
        private static readonly string ServerHostDev = "dev.cloud.saygames.io";
        //private static readonly string ServerHostStage = "stg.cloud.saygames.io";
        private static readonly string ServerHostProd = "cloud.saygames.io";
        private static readonly int ServerPort = 443;
        private static readonly string ServerKey = "x4qQTmMzy31xCtO9BqoRojmdvhWBz3XP";
        private readonly LevelService levelService;
        private readonly SocialConfig socialConfig;
        private readonly PlayerProfileConfiguration playerProfileConfiguration;
        private readonly UserAuthenticateEvent userAuthenticateEvent;
        private readonly UserDataUpdateEvent userDataUpdateEvent;

        private CancellationTokenSource authCts = null;
        private readonly string deviceId;
        bool isInit = false;



        public bool IsAuthenticated => Session != null;
        public Client Client { get; private set; } = null;
        public ISession Session { get; private set; } = null;



        public SocialService(LevelService levelService,
            ConfigProvider configProvider,
            UserAuthenticateEvent userAuthenticateEvent,
            UserDataUpdateEvent userDataUpdateEvent) 
        {
            this.levelService = levelService;
            this.socialConfig = configProvider.SocialConfig;
            this.playerProfileConfiguration = configProvider.PlayerProfileConfiguration;
            this.userAuthenticateEvent = userAuthenticateEvent;
            this.userDataUpdateEvent = userDataUpdateEvent;

            deviceId = SystemInfo.deviceUniqueIdentifier;
        }


        public void Initialize()
        {
            if(isInit)
            {
                return;
            }

            SayGamesServices.Instance.Initialize(new SayGamesServicesConfig
            {
                AppKey = SayKitApp.GetAppKey(),
                DeviceId = deviceId,
                Idfa = GetIdfa(),
                GetLevel = GetLevel,
                GetPlayingTime = GetPlayingTime,
                Logger = GetLogger()
            });


            Client = new Client(ServerScheme, 
                GetServerHost(), 
                ServerPort, 
                ServerKey, 
                HttpRequestAdapter.WithGzip(decompression: true, compression: true),
                autoRefreshSession: false);

            AuthenticateAsync().Forget();

            levelService.OnLevelIncremented += LevelService_OnLevelIncremented;

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
            {
                return;
            }

            if(authCts != null)
            {
                authCts.Cancel();
                authCts.Dispose();
                authCts = null;
            }

            isInit = false;
            Client = null;
            Session = null;

            levelService.OnLevelIncremented -= LevelService_OnLevelIncremented;
        }


        public bool IsUnlocked()
        {
            return socialConfig.Feature.isFeatureEnabled
                && levelService.CurrentLevelNumber >= socialConfig.Feature.unlockLevel;
        }


        public string GetPlayerId()
        {
            if (IsAuthenticated)
            {
                return Session.UserId;
            }

            return "Not Connected";
        }


        public string GetPlayerName()
        {
            if(IsAuthenticated)
            {
                return Session.Username;
            }

            return "Not Connected";
        }


        public async UniTask<bool> TryAuthenticateAsync()
        {
            if(IsAuthenticated)
            {
                return true;
            }

            return await AuthenticateAsync();
        }


        public async UniTask<IApiUser> UpdateUserDisplayNameAsync(string displayName, CancellationToken cancellationToken = default)
        {
            if(!IsAuthenticated)
            {
                return null;
            }

            try
            {
                IApiUser apiUser = await Client.UpdateAccountExAsync(Session, 
                    username: Session.Username, 
                    displayName: displayName,
                    canceller: cancellationToken).AsUniTask<IApiUser>();
                OnPlayerUpdated(apiUser);
                return apiUser;
            }
            catch (Exception)
            {
                Debug.LogWarning("Update User Display Name failed!");
            }

            return null;
        }


        public async UniTask<IApiUser> WriteUserMetaDataAsync(PlayerProfileAvatarDto avatar, CancellationToken cancellationToken = default)
        {
            if (!IsAuthenticated)
            {
                return null;
            }

            PlayerMetaData playerMetaData = new PlayerMetaData()
            {
                playerAvatar = avatar,
                playerStatistics = GetPlayerStatistics()
            };

            string playerMetaDataJson = JsonConvert.SerializeObject(playerMetaData, JsonUtils.SerializerSettings);

            try
            {
                IApiUser apiUser = await Client.UpdateAccountExAsync(Session,
                    username: Session.Username,
                    avatarUrl: playerMetaDataJson,
                    canceller: cancellationToken).AsUniTask<IApiUser>();
                OnPlayerUpdated(apiUser);
                return apiUser;
            }
            catch (Exception)
            {
                Debug.LogWarning("Update User Avatar failed!");
            }

            return null;
        }


        public async UniTask<IApiUser> GetUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (!IsAuthenticated)
            {
                return null;
            }

            try
            {
                IApiUser apiUser = await Client.GetUserAsync(Session, userId, canceller: cancellationToken).AsUniTask<IApiUser>();
                return apiUser;
            }
            catch (Exception)
            {
                Debug.LogWarning("Update User Display Name failed!");
            }

            return null;
        }


        public PlayerMetaData GetPlayerMetaData(string metaData)
        {
            try
            {
                PlayerMetaData playerMetaData = JsonConvert.DeserializeObject<PlayerMetaData>(metaData, JsonUtils.SerializerSettings);
                if (playerMetaData != null
                    && playerMetaData.playerAvatar != null)
                {
                    return playerMetaData;
                }
            }
            catch
            {
                Debug.Log("Old meta-data");
            }

            PlayerProfileAvatarDto playerProfileAvatarDto = null;
            try
            {
                playerProfileAvatarDto = JsonConvert.DeserializeObject<PlayerProfileAvatarDto>(metaData, JsonUtils.SerializerSettings);
            }
            catch
            {
                Debug.Log("Meta-data error");
            }

            if (playerProfileAvatarDto == null
                || playerProfileAvatarDto.avatarId == null)
            {
                playerProfileAvatarDto = new PlayerProfileAvatarDto()
                {
                    avatarId = playerProfileConfiguration.Feature.defaultAvatar
                };
            }

            PlayerMetaData oldPlayerMetaData = new PlayerMetaData()
            {
                playerAvatar = playerProfileAvatarDto,
                playerStatistics = new PlayerStatistics()
            };

            return oldPlayerMetaData;
        }


        private async UniTask<bool> AuthenticateAsync()
        {
            if(!IsUnlocked()
                || IsAuthenticated
                || authCts != null)
            {
                return false;
            }

            authCts = new CancellationTokenSource();
            try
            {
                Session = await Client.AuthenticateDeviceAsync(deviceId, canceller: authCts.Token).AsUniTask();
            }
            catch (Exception)
            {
                Debug.LogWarning("User Authentication failed!");
            }
            finally
            {
                authCts.Dispose();
                authCts = null;
                userAuthenticateEvent.Execute(Unit.Default);
            }

            return IsAuthenticated;
        }


        private void OnPlayerUpdated(IApiUser apiUser)
        {
            userDataUpdateEvent.Execute(apiUser.DisplayName, GetPlayerMetaData(apiUser.AvatarUrl));
        }


        private string GetIdfa()
        {
#if UNITY_EDITOR
            return "unity_editor";
#else
            return SayKit.runtimeInfo.idfa;
#endif
        }


        private string GetServerHost()
        {
#if PR_CHEAT || UNITY_EDITOR
            return ServerHostDev;
#else
            return ServerHostProd;
#endif
        }


        private SayGames.Services.Logging.ILogger GetLogger()
        {
#if PR_CHEAT || UNITY_EDITOR
            return new DebugUnityLogger();
#else
            return null;
#endif
        }


        private int GetLevel()
        {
            return levelService.CurrentLevelNumber;
        }


        private int GetPlayingTime()
        {
            return 0;
        }


        public PlayerStatistics GetPlayerStatistics()
        {
            return new PlayerStatistics()
            {
                firstWinCount = levelService.LevelStatistics.firstWinCount,
                totalWinCount = levelService.LevelStatistics.totalWinCount,
                maxWinStreak = levelService.LevelStatistics.maxWinStreak
            };
        }


        private void LevelService_OnLevelIncremented()
        {
            AuthenticateAsync().Forget();
        }
    }
}