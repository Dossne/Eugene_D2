using Features.Level;
using Infrastructure.Ads;
using Infrastructure.Configs;
using Infrastructure.PersistentProgress;
using Infrastructure.PurchaseSystem;
using Infrastructure.TimeCycles;
using Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using Unity.Mathematics;


namespace Features.FreePaidOffer
{
    public class FreePaidOfferService : ISavable
    {
        public Action<double> OnSecondTick;
        public Action OnCycleReset;

        private FreePaidOfferConfig freePaidOfferConfig;
        private TimeCyclesService timeCyclesService;
        private PurchaseOfferContainer purchaseOfferContainer;
        private LevelService levelService;
        private AnalyticsContextCreator analyticsContextCreator;

        private ITimeCycleReadable timeCycle = null;
        double delay = 0;

        public bool IsInitialized { get; private set; }
        public int OfferCounter { get; private set; } = 1;
        public int CurrentPresetId { get; private set; } = 0;
        public int CurrentOfferIdx { get; private set; } = 0;



        public FreePaidOfferService(ConfigProvider configProvider,
            TimeCyclesService timeCyclesService,
            PurchaseOfferContainer purchaseOfferContainer,
            LevelService levelService,
            AnalyticsContextCreator analyticsContextCreator)
        {
            this.freePaidOfferConfig = configProvider.FreePaidOfferConfig;
            this.timeCyclesService = timeCyclesService;
            this.purchaseOfferContainer = purchaseOfferContainer;
            this.levelService = levelService;
            this.analyticsContextCreator = analyticsContextCreator;
        }


        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            InitCurrentPresetId();
            InitTimeCycle();

            timeCyclesService.OnCycleReset += TimeCyclesService_OnCycleReset;

            IsInitialized = true;
        }


        public void Deinitialize()
        {
            if (!IsInitialized)
            {
                return;
            }


            timeCyclesService.OnCycleReset -= TimeCyclesService_OnCycleReset;

            IsInitialized = false;
        }


        public double TimeRest()
        {
            if (timeCycle == null
                && !InitTimeCycle())
            {
                return 0;
            }

            
            return timeCycle.TimeRest - delay;
        }


        public void OnOfferRefreshed(FreePaidOfferSkinType currentSkin)
        {
            FreePaidOfferAnalytics.SendEventOfferAvailable(OfferCounter, CurrentPresetId, currentSkin, analyticsContextCreator.MainContext);
        }


        public void OnOfferApplied(OfferId inAppOfferId, FreePaidOfferSkinType currentSkin)
        {
            FreePaidOfferAnalytics.SendEventRewardReceived(
                OfferCounter,
                CurrentPresetId,
                CurrentOfferIdx,
                currentSkin,
                inAppOfferId,
                IsLastFreeReward(),
                IsLastIapReward(),
                analyticsContextCreator.MainContext);

            ++CurrentOfferIdx;
        }


        public bool IsOfferActive()
        {
            return IsUnlocked()
                && !IsOffersCompleted();
        }


        public bool IsOffersCompleted()
        {
            return CurrentOfferIdx >= freePaidOfferConfig.GetOffersCount(CurrentPresetId);
        }


        public bool IsCurrentFreeReward()
        {
            List<FreePaidOfferRewardData> rewardDatas = freePaidOfferConfig.GetRewardsData(CurrentPresetId);
            
            if (CurrentOfferIdx >= rewardDatas.Count)
                return false;
            
            return rewardDatas[CurrentOfferIdx].InAppOfferId == OfferId.none;
        }
        
        
        public bool IsUnlocked()
        {
            return freePaidOfferConfig.Feature.isFeatureEnabled
                && levelService.CurrentLevelNumber >= freePaidOfferConfig.Feature.unlockLevel;
        }


        void ISavable.Load(Progress progress)
        {
            OfferCounter = progress.freePaidOfferState.offerCounter;
            CurrentPresetId = progress.freePaidOfferState.presetId;
            CurrentOfferIdx = progress.freePaidOfferState.offerIdx;
        }


        void ISavable.Save(Progress progress)
        {
            progress.freePaidOfferState.offerCounter = OfferCounter;
            progress.freePaidOfferState.presetId = CurrentPresetId;
            progress.freePaidOfferState.offerIdx = CurrentOfferIdx;
        }


        private bool IsLastFreeReward()
        {
            List<FreePaidOfferRewardData> rewardDatas = freePaidOfferConfig.GetRewardsData(CurrentPresetId);
            if (rewardDatas[CurrentOfferIdx].InAppOfferId != OfferId.none)
            {
                return false;
            }

            for (int i = CurrentOfferIdx + 1; i < rewardDatas.Count; ++i)
            {
                if(rewardDatas[i].InAppOfferId == OfferId.none)
                {
                    return false;
                }
            }

            return true;
        }


        private bool IsLastIapReward()
        {
            List<FreePaidOfferRewardData> rewardDatas = freePaidOfferConfig.GetRewardsData(CurrentPresetId);
            if (rewardDatas[CurrentOfferIdx].InAppOfferId == OfferId.none)
            {
                return false;
            }

            for (int i = CurrentOfferIdx + 1; i < rewardDatas.Count; ++i)
            {
                if (rewardDatas[i].InAppOfferId != OfferId.none)
                {
                    return false;
                }
            }

            return true;
        }


        private void InitCurrentPresetId()
        {
            if(CurrentPresetId == 0)
            {
                CurrentPresetId = GetPresetId();
            }
        }


        private bool InitTimeCycle()
        {
            timeCyclesService.TryGetCycleItemReadable(TimeCycleType.FreePaidOfferWeekly, out timeCycle);
            if (timeCycle == null)
            {
                return false;
            }
            delay = math.max(0, TimeUtils.WEEK_SECONDS - freePaidOfferConfig.Feature.durationSec);
            return true;
        }


        private int GetPresetId()
        {
            FreePaidOfferPresetData resPreset = freePaidOfferConfig.Presets[0];
            for (int i = 1; i < freePaidOfferConfig.Presets.Count; ++i)
            {
                FreePaidOfferPresetData preset = freePaidOfferConfig.Presets[i];
                if (preset.cumulativeSpentMin <= purchaseOfferContainer.CumulativeSpentUsd
                    && preset.cumulativeSpentMin > resPreset.cumulativeSpentMin)
                {
                    resPreset = preset;
                }
            }

            return resPreset.presetId;
        }


        private void TimeCyclesService_OnCycleReset(TimeCycleType timeCycleType)
        {
            if (timeCycleType != TimeCycleType.FreePaidOfferWeekly
                || !IsUnlocked())
            {
                return;
            }

            ++OfferCounter;
            CurrentPresetId = GetPresetId();
            CurrentOfferIdx = 0;
            OnCycleReset?.Invoke();
        }
    }
}


