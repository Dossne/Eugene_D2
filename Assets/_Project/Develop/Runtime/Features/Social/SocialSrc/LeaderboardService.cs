using Cysharp.Threading.Tasks;
using Features.PlayerProfile;
using Infrastructure.Configs;
using Infrastructure.DateTimeControl;
using Infrastructure.PersistentProgress;
using Infrastructure.Reward;
using Infrastructure.Utilities;
using Nakama;
using R3;
using SayGames.Services.Leaderboards;
using SayGames.Services.Mail;
using SayGames.Services.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace Features.Social
{
    public class LeaderboardService: ISavable
    {
        private readonly SocialService socialService;
        private readonly LeaderboardNakamaClientWrapper leaderboardNakamaClientWrapper;
        private readonly MailService mailService;
        private readonly SocialConfig socialConfig;
        private readonly MailReceivedEvent mailReceivedEvent;
        private readonly PlayerProfileController playerProfileController;
        private readonly DateTimeService dateTimeService;
        private readonly LeaderboardCachedEvent leaderboardCachedEvent;
        private readonly UserDataUpdateEvent userDataUpdateEvent;
        private readonly LeaderboardOnGetStateStartEvent leaderboardOnGetStateStartEvent;
        private readonly LeaderboardRequestEvent leaderboardRequestEvent;
        private readonly CompositeDisposable disposables;

        private readonly LeaderboardRecordsActualizer  recordsActualizer;
        
        bool isInit = false;
        bool isNeedRewardUpdate = true;
        Dictionary<string, LeaderboardState> leaderboardsStates = null;
        Dictionary<string, LeaderboardReceivedReward> receivedRewards = new Dictionary<string, LeaderboardReceivedReward>();
        Dictionary<string, LeaderboardRecords> leaderboardsRecords = new Dictionary<string, LeaderboardRecords>();
        


        public Dictionary<string, LeaderboardReceivedReward> ReceivedRewards => receivedRewards;



        public LeaderboardService(SocialService socialService,
            LeaderboardNakamaClientWrapper leaderboardNakamaClientWrapper,
            MailService mailService,
            ConfigProvider configProvider,
            PlayerProfileController playerProfileController,
            DateTimeService dateTimeService,
            MailReceivedEvent mailReceivedEvent,
            LeaderboardCachedEvent leaderboardCachedEvent,
            LeaderboardOnGetStateStartEvent leaderboardGetStateEvent,
            LeaderboardRequestEvent leaderboardRequestEvent,
            UserDataUpdateEvent userDataUpdateEvent)
        {
            this.socialService = socialService;
            this.leaderboardNakamaClientWrapper = leaderboardNakamaClientWrapper;
            this.mailService = mailService;
            this.mailReceivedEvent = mailReceivedEvent;
            this.playerProfileController = playerProfileController;
            this.dateTimeService = dateTimeService;
            this.leaderboardCachedEvent = leaderboardCachedEvent;
            this.userDataUpdateEvent = userDataUpdateEvent;
            this.leaderboardOnGetStateStartEvent = leaderboardGetStateEvent;
            this.leaderboardRequestEvent = leaderboardRequestEvent;
            this.socialConfig = configProvider.SocialConfig;
            this.recordsActualizer = new LeaderboardRecordsActualizer(configProvider.PlayerProfileConfiguration);
            this.disposables = new CompositeDisposable();
        }


        public void Initialize()
        {
            if (isInit)
            {
                return;
            }
            isInit = true;

            mailReceivedEvent.Subscribe(CacheReceivedRewards).AddTo(disposables);
            userDataUpdateEvent.Subscribe(_ =>
            {
                UpdatePlayerData(userDataUpdateEvent.DisplayName, userDataUpdateEvent.PlayerMetaData);
            }).AddTo(disposables);
        }


        public void Deinitialize()
        {
            if (!isInit)
            {
                return;
            }
            isInit = false;

            disposables.Dispose();

            leaderboardsStates = null;
            receivedRewards.Clear();
        }


        public async UniTask<LeaderboardState> GetLeaderboardStateAsync(string leaderboardName, CancellationToken cancellationToken = default)
        {
            if(leaderboardName.IsNullOrEmpty())
            {
                return null;
            }

            leaderboardOnGetStateStartEvent.Execute(leaderboardName);

            if(IsNeedUpdateState(leaderboardName))
            {
                ILeaderboardState leaderboardServerState = await leaderboardNakamaClientWrapper.GetLeaderboardStateAsync(leaderboardName, cancellationToken);
                if (leaderboardServerState != null)
                {
                    CacheLeaderboardState(leaderboardName, leaderboardServerState);
                }
            }

            if(leaderboardsStates.ContainsKey(leaderboardName))
            {
                return leaderboardsStates[leaderboardName];
            }

            return null;
        }


        public async UniTask<bool> TryWriteRecordAsync(string leaderboardName, long score, CancellationToken cancellationToken = default)
        {
            if (leaderboardName.IsNullOrEmpty())
            {
                return false;
            }

            LeaderboardState state = await GetLeaderboardStateAsync(leaderboardName);
            if (!IsLeaderboardRunning(state))
            {
                return false;
            }

            if (!IsUserJoinedToLeaderboard(state))
            {
                if (!await leaderboardNakamaClientWrapper.JoinLeaderboardAsync(leaderboardName))
                {
                    return false;
                }
                leaderboardsRecords.Remove(leaderboardName);
                state.joinedToGeneration = state.generation;
            }

            if (leaderboardsRecords.ContainsKey(leaderboardName))
            {
                leaderboardsRecords[leaderboardName].AddScore(score);
            }

            long resultScore = leaderboardsStates[leaderboardName].unsentScore + score;
            long subscore = GetSubscore(state);
            if (await leaderboardNakamaClientWrapper.WriteRecordAsync(leaderboardName, resultScore, subscore, cancellationToken))
            {
                state.isSubscoreSent = 1;
                if (leaderboardsRecords.ContainsKey(leaderboardName))
                {
                    leaderboardsStates[leaderboardName].unsentScore = 0;
                    leaderboardsRecords[leaderboardName].ScoreSent();
                }

                socialService.WriteUserMetaDataAsync(playerProfileController.GetPlayerAvatarDto(), cancellationToken).Forget();
            }
            else
            {
                leaderboardsStates[leaderboardName].unsentScore += score;
            }

            return true;
        }


        public async UniTask<LeaderboardRecords> ListLeaderboardRecordsAroundOwnerAsync(string leaderboardName, int limit, CancellationToken cancellationToken = default)
        {
            if (leaderboardName.IsNullOrEmpty())
            {
                return null;
            }

            LeaderboardState state = await GetLeaderboardStateAsync(leaderboardName);
            if (!IsLeaderboardRunning(state)
                || !IsUserJoinedToLeaderboard(state))
            {
                return GetCachedLeaderboardRecords(leaderboardName);
            }

            if(IsNeedUpdateRecords(leaderboardName))
            {
                IApiLeaderboardRecordList recordList = await leaderboardNakamaClientWrapper.ListLeaderboardRecordsAroundOwnerAsync(leaderboardName, limit, cancellationToken);
                if(recordList == null)
                {
                    return GetCachedLeaderboardRecords(leaderboardName);
                }

                dateTimeService.TryGetServerTime(out DateTime serverTime);
                DateTime nextUpdateDate = serverTime.AddSeconds(socialConfig.Leaderboard.topUpdateCooldownSec);
                leaderboardsRecords[leaderboardName] = new LeaderboardRecords(GetLeaderboardRecords(recordList), state.joinedToGeneration, state.unsentScore, socialService.GetPlayerId(), nextUpdateDate);
            }

            return GetCachedLeaderboardRecords(leaderboardName);
        }


        public async UniTask<bool> ClaimReceivedRewardAsync(string letterId, CancellationToken cancellationToken = default)
        {
            if (await mailService.ClaimLetterAsync(letterId, cancellationToken))
            {
                ClaimReceivedReward(letterId);
                return true;
            }

            return false;
        }


        public async UniTask<(bool isServerAvailable, LeaderboardReceivedReward reward)> GetRewardAsync(string leaderboardName, CancellationToken cancellationToken = default)
        {
            if (leaderboardName.IsNullOrEmpty())
            {
                return (false, null);
            }

            LeaderboardReceivedReward reward = GetCachedReward(leaderboardName);

            if (reward == null
                || isNeedRewardUpdate)
            {
                if(await mailService.FetchLettersAsync(cancellationToken))
                {
                    isNeedRewardUpdate = false;
                    return (true, GetCachedReward(leaderboardName));
                }

                return (false, null);
            }

            return (true, reward);
        }


        public bool IsLeaderboardRunning(LeaderboardState state)
        {
            dateTimeService.TryGetServerTime(out DateTime serverTime);

            return state != null
                && state.runningState == LeaderboardRunningState.Run
                && state.nextTick > serverTime;
        }

        
        public (int newPlayerPosIdx, List<LeaderboardRecord> records) GetActualizedRecords(List<LeaderboardRecord> records,
                                                                                                int maxLeaderboardCount, 
                                                                                                int serverRewardMaxCount, 
                                                                                                int serverRewardIdx = -1)
        {
            return recordsActualizer.GetActualizedRecords(records, maxLeaderboardCount, serverRewardMaxCount, serverRewardIdx);
        }
        

        public bool IsCacheState(string leaderboardName)
        {
            return leaderboardsStates.ContainsKey(leaderboardName);
        }


        public void RemoveCacheState(string leaderboardName)
        {
            leaderboardsStates.Remove(leaderboardName);
        }


        public bool IsNeedUpdateState(string leaderboardName)
        {
            dateTimeService.TryGetServerTime(out DateTime serverTime);
            return !leaderboardsStates.ContainsKey(leaderboardName)
                || leaderboardsStates[leaderboardName].nextTick <= serverTime;
        }


        public bool IsCacheRecords(string leaderboardName)
        {
            return leaderboardsRecords.ContainsKey(leaderboardName);
        }


        public void RemoveCacheRecords(string leaderboardName)
        {
            leaderboardsRecords.Remove(leaderboardName);
        }


        public bool IsNeedUpdateRecords(string leaderboardName)
        {
            dateTimeService.TryGetServerTime(out DateTime serverTime);
            return !leaderboardsRecords.ContainsKey(leaderboardName)
                || leaderboardsRecords[leaderboardName].IsScoreSent
                || leaderboardsRecords[leaderboardName].NextUpdateDate < serverTime;
        }


        void ISavable.Load(Infrastructure.PersistentProgress.Progress progress)
        {
            leaderboardsStates = new Dictionary<string, LeaderboardState>();
            for (int i = 0; i < progress.leaderboards.Count; ++i)
            {
                LeaderboardSaveState saveState = progress.leaderboards[i];
                CacheLeaderboardState(saveState.name, saveState.leaderboardState);

                if(saveState.leaderboardRecords != null)
                {
                    leaderboardsRecords[saveState.name] = saveState.leaderboardRecords;
                }
            }
        }


        void ISavable.Save(Infrastructure.PersistentProgress.Progress progress)
        {
            progress.leaderboards.Clear();
            foreach (var leaderboard in leaderboardsStates)
            {
                string name = leaderboard.Key;
                LeaderboardSaveState leaderboardSaveState = new LeaderboardSaveState()
                {
                    name = name,
                    leaderboardState = leaderboard.Value,
                };

                if (leaderboardsRecords.ContainsKey(name))
                {
                    leaderboardSaveState.leaderboardRecords = leaderboardsRecords[name];
                }

                progress.leaderboards.Add(leaderboardSaveState);
            }

            foreach (var leaderboardRecord in leaderboardsRecords)
            {
                string name = leaderboardRecord.Key;
                if (!leaderboardsStates.ContainsKey(name))
                {
                    LeaderboardSaveState leaderboardSaveState = new LeaderboardSaveState()
                    {
                        name = name,
                        leaderboardRecords = leaderboardRecord.Value
                    };

                    progress.leaderboards.Add(leaderboardSaveState);
                }
            }
        }


        private void CacheLeaderboardState(string name, ILeaderboardState iLeaderboardState)
        {
            LeaderboardState leaderboardDataState = new LeaderboardState()
            {
                generation = iLeaderboardState.Generation,
                runningState = iLeaderboardState.RunningState,
                nextTick = iLeaderboardState.NextTick,
                league = iLeaderboardState.OwnerLeague,
                division = iLeaderboardState.OwnerBucket,
                joinedToGeneration = -1,
                isSubscoreSent = 0
            };

            CacheLeaderboardState(name, leaderboardDataState);
        }


        private void CacheLeaderboardState(string name, LeaderboardState leaderboardState)
        {
            if(leaderboardState == null)
            {
                return;
            }

            leaderboardsStates[name] = leaderboardState;
            leaderboardCachedEvent.Execute(leaderboardState);
            isNeedRewardUpdate = true;
        }


        private LeaderboardRecords GetCachedLeaderboardRecords(string leaderboardName)
        {
            if (leaderboardsRecords.ContainsKey(leaderboardName))
            {
                return leaderboardsRecords[leaderboardName];
            }
            return null;
        }


        private void CacheReceivedRewards(IReadOnlyCollection<ILetter> letters)
        {
            receivedRewards.Clear();
            foreach (ILetter letter in letters)
            {
                if (letter.Code != MailCode.LeaderboardPlacement
                    && letter.Code != MailCode.LeaderboardScore)
                {
                    continue;
                }

                if (letter.Read
                   || letter.Claimed
                   || letter.Attachment == null)
                {
                    continue;
                }

                Dictionary<string, object> attachments = letter.GetDeserializedAttachment<Dictionary<string, object>>();
                if (!attachments.ContainsKey(MailService.LeaderBoardNameKey)
                    || !attachments.ContainsKey(MailService.AttachmentKey))
                {
                    continue;
                }

                try
                {
                    string leaderboardName = attachments[MailService.LeaderBoardNameKey].ToString();
                    LeaderboardReceivedReward leaderboardReward = RewardUtils.ConvertFromJson<LeaderboardReceivedReward>(attachments[MailService.AttachmentKey].ToString());
                    leaderboardReward.letterId = letter.Id;
                    leaderboardReward.mailCode = letter.Code;
                    leaderboardReward.CreatedAt = letter.CreatedAt;

                    if (receivedRewards.ContainsKey(leaderboardName))
                    {
                        if(leaderboardReward.CreatedAt > receivedRewards[leaderboardName].CreatedAt)
                        {
                            ClaimReceivedRewardAsync(receivedRewards[leaderboardName].letterId).Forget();
                            receivedRewards[leaderboardName] = leaderboardReward;
                        }
                        else
                        {
                            ClaimReceivedRewardAsync(leaderboardReward.letterId).Forget();
                        }
                    }
                    else
                    {
                        receivedRewards[leaderboardName] = leaderboardReward;
                    }

                }
                catch (Exception ex)
                {
                    Debug.LogError("Failed Leaderboard reward parse!\n" + ex);
                }
            }
        }


        private void UpdatePlayerData(string displayName, PlayerMetaData playerMetaData)
        {
            foreach(var record in leaderboardsRecords)
            {
                record.Value.UpdatePlayerData(displayName, playerMetaData);
            }
        }


        private LeaderboardReceivedReward GetCachedReward(string leaderboardName)
        {
            if (!receivedRewards.ContainsKey(leaderboardName))
            {
                return null;
            }

            LeaderboardReceivedReward receivedReward = receivedRewards[leaderboardName];
            if(!receivedReward.IsClaimed)
            {
                return receivedReward;
            }

            return null;
        }


        private void ClaimReceivedReward(string letterId)
        {
            foreach (var item in receivedRewards)
            {
                LeaderboardReceivedReward reward = item.Value;
                if (reward.letterId == letterId)
                {
                    reward.SetClaimed();
                    return;
                }
            }
        }


        private bool IsUserJoinedToLeaderboard(LeaderboardState state)
        {
            return state != null
                && state.IsUserJoinedToLeaderboard;
        }


        private long GetSubscore(LeaderboardState leaderboardState)
        {
            if (leaderboardState == null
                || leaderboardState.IsSubscoreSent)
            {
                return 0;
            }

            dateTimeService.TryGetServerTime(out DateTime serverTime);
            return (leaderboardState.nextTick - serverTime).Ticks;
        }


        private List<LeaderboardRecord> GetLeaderboardRecords(IApiLeaderboardRecordList records)
        {
            List<LeaderboardRecord> leaderboardRecords = new List<LeaderboardRecord>();
            foreach (IApiLeaderboardRecord record in records.Records)
            {
                LeaderboardRecord leaderboardRecord = new LeaderboardRecord()
                {
                    displayName = record.DisplayName,
                    metaData = socialService.GetPlayerMetaData(record.AvatarUrl),
                    score = record.ScoreNum,
                    subScore = record.SubscoreNum,
                    isPlayer = socialService.GetPlayerId() == record.OwnerId ? 1 : 0
                };
                leaderboardRecords.Add(leaderboardRecord);
            }

            return leaderboardRecords;
        }
    }
}
