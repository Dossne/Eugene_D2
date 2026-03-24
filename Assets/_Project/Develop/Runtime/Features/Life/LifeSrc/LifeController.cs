using Features.Level;
using Infrastructure.Ads;
using Infrastructure.ApplicationInterrupt;
using Infrastructure.Configs;
using Infrastructure.DateTimeControl;
using Infrastructure.PersistentProgress;
using Infrastructure.SystemsLifeCycle;
using Infrastructure.TimeCycles;
using Infrastructure.WalletSystem;
using System;
using UnityEditor.Rendering;
using UnityEngine;
using VContainer;

namespace Features.Life
{
    public class LifeController : ISavable, ISystemTickable
    {
        public Action<(bool isLifeFull, int timeRestSec)> OnTimerUpdate;
        
        private Wallet wallet;
        private AppInterruptObserver appInterruptObserver;
        private LifeConfiguration lifeConfiguration;
        private readonly AdsConfig adsConfig;
        private LevelService levelService;
        private readonly DateTimeService dateTimeService;
        private readonly TimeCyclesService timeCyclesService;
        private DateTime lastLifeRestoreDate        = DateTime.UnixEpoch;
        private DateTime nextLifeRestoreDate        = DateTime.UnixEpoch;
        private DateTime lastStartDate              = DateTime.UnixEpoch;
        private DateTime infiniteLifeExpireDate     = DateTime.UnixEpoch;
        private int rewardedResurrectCount = 0;
        private int rewardedLifeCount = 0;
        private int bombResurrectCount = 0;

        private bool isLoaded = false;
        private bool isInit = false;
        private bool firstLaunch = true;
        public int LifeAmount => wallet.GetCount(CurrencyType.Life);
        public int LifePrice => lifeConfiguration.lifePriceCoin;

        public float lastUpdateSeconds = 0;

        [Inject]
        public LifeController(Wallet wallet, 
                              AppInterruptObserver appInterruptObserver,
                              ConfigProvider configProvider,
                              LevelService levelService,
                              DateTimeService dateTimeService,
                              TimeCyclesService timeCyclesService)
        {
            this.wallet = wallet;
            this.appInterruptObserver = appInterruptObserver;
            this.levelService = levelService;
            this.lifeConfiguration = configProvider.LifeConfiguration;
            this.adsConfig = configProvider.AdsConfig;
            this.dateTimeService = dateTimeService;
            this.timeCyclesService = timeCyclesService;
        }

        public void Initialize()
        {
            if (isInit)
                return;

            appInterruptObserver.Resume += AppInterruptObserver_Resume;
            if (firstLaunch && LifeAmount == 0)
                wallet.Set(CurrencyType.Life, GetMaxLifeAmount(), string.Empty, true);

            if (lastLifeRestoreDate == DateTime.UnixEpoch)
                lastLifeRestoreDate = GetCurrentDateTime();

            nextLifeRestoreDate = lastLifeRestoreDate.AddSeconds(lifeConfiguration.lifeCooldownSec);

            timeCyclesService.OnCycleReset += TimeCyclesService_OnCycleReset;

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            timeCyclesService.OnCycleReset -= TimeCyclesService_OnCycleReset;

            appInterruptObserver.Resume -= AppInterruptObserver_Resume;

            isInit = false;
        }

        void ISavable.Load(Progress progress)
        {
            infiniteLifeExpireDate = progress.lifeControllerState.infiniteLifeExpireDate;
            lastLifeRestoreDate = progress.lifeControllerState.lastLifeRestoreDate;
            firstLaunch = progress.lifeControllerState.firstLaunch;
            rewardedResurrectCount = progress.lifeControllerState.rewardedResurrectCount;
            rewardedLifeCount = progress.lifeControllerState.rewardedLifeCount;
            bombResurrectCount = progress.lifeControllerState.bombResurrectCount;
            isLoaded = true;
        }

        void ISavable.Save(Progress progress)
        {
            progress.lifeControllerState.firstLaunch = false;
            progress.lifeControllerState.lastLifeRestoreDate = lastLifeRestoreDate;
            progress.lifeControllerState.infiniteLifeExpireDate = infiniteLifeExpireDate;
            progress.lifeControllerState.rewardedResurrectCount = rewardedResurrectCount;
            progress.lifeControllerState.rewardedLifeCount = rewardedLifeCount;
            progress.lifeControllerState.bombResurrectCount = bombResurrectCount;
        }

        void ISystemTickable.Tick()
        {
            if (!isLoaded)
                return;

            ExecuteUpdate(Time.unscaledDeltaTime);
        }

        public void LoseLife() 
        {
            if (levelService.CurrentLevelNumber <= lifeConfiguration.unspendableLastLifeLevel && LifeAmount < 2)
                return;

            if (infiniteLifeExpireDate >= GetCurrentDateTime())
                return;

            if (LifeAmount == lifeConfiguration.defaultMaxCount)
            {
                if (lastStartDate == DateTime.UnixEpoch)
                    SetLastStartTimeStamp();
                lastLifeRestoreDate = lastStartDate;
                nextLifeRestoreDate = lastLifeRestoreDate.AddSeconds(lifeConfiguration.lifeCooldownSec);
            }

            wallet.Decrease(CurrencyType.Life, 1, string.Empty);
            LifeAnalytics.SendOutcomeEvent(1, wallet.GetCount(CurrencyType.Life), Reason.Out.LifeOut);
            ExecuteUpdate(0);
        }

        public bool TryBuyLife() 
        {
            if (wallet.GetCount(CurrencyType.Coins) < lifeConfiguration.lifePriceCoin)
                return false;

            var buyAmount = GetMaxLifeAmount() - LifeAmount;
            if (buyAmount <= 0)
                return false;

            wallet.Decrease(CurrencyType.Coins, lifeConfiguration.lifePriceCoin, Reason.Out.LifeBuy);            
            wallet.Increase(CurrencyType.Life, buyAmount, string.Empty);
            LifeAnalytics.SendIncomeEvent(buyAmount, wallet.GetCount(CurrencyType.Life), Reason.In.LifeInByCoin);
            return true;
        }

        public void AddRewardedLife()
        {
            wallet.Increase(CurrencyType.Life, 1, string.Empty);
            LifeAnalytics.SendIncomeEvent(1, wallet.GetCount(CurrencyType.Life), Reason.In.LifeInByRewardAd);
            UseRewardedLife();
        }

        public void SetLastStartTimeStamp()
        {
            lastStartDate = GetCurrentDateTime();
        }

        public void AddInfiniteLifeTime(int infiniteLifeTimeMin)
        {
            if (infiniteLifeExpireDate < GetCurrentDateTime())
                infiniteLifeExpireDate = GetCurrentDateTime();

            infiniteLifeExpireDate = infiniteLifeExpireDate.AddMinutes(infiniteLifeTimeMin);
        }


        public bool IsInfiniteLife()
        {
            return infiniteLifeExpireDate >= GetCurrentDateTime();
        }


        public float GetInfiniteExpiresSeconds()
        {
            return Mathf.Max((float)(infiniteLifeExpireDate - GetCurrentDateTime()).TotalSeconds, 0.0f);
        }

        public void UseRewardedResurrect() 
        { 
            rewardedResurrectCount++;
        }

        public void UseRewardedLife()
        {
            rewardedLifeCount++;
        }

        public void UseBombResurrect()
        {
            bombResurrectCount++;
        }

        public bool BombResurrectAvailable()
        {
            return bombResurrectCount < 1;
        }

        public bool RewardedResurrectAvailable()
        {
            return rewardedResurrectCount < adsConfig.AdsData.rewardedResurrectPerDay;
        }

        public bool RewardedLifeAvailable()
        {
            return rewardedLifeCount < adsConfig.AdsData.rewardedLifePerDay;
        }

        public (bool isLifeFull, int timeRestSec) GetUpdateData()
        {
            return (HasMaxLifeAmount(), (int)(nextLifeRestoreDate - GetCurrentDateTime()).TotalSeconds);
        }

        private DateTime GetCurrentDateTime()
        {
            dateTimeService.TryGetServerTime(out DateTime dateTime);
            return dateTime;
        }

        private void RestoreLife()
        {
            var sec = (GetCurrentDateTime() - lastLifeRestoreDate).TotalSeconds;
            var floatAmount = (float)sec / lifeConfiguration.lifeCooldownSec;
            int amount = Mathf.FloorToInt(floatAmount);
            if (amount <= 0 && floatAmount > 0.9)
            {
                Debug.LogWarning($"Restore life float bug {floatAmount}");
                return;
            }

            if (!HasMaxLifeAmount())
            {
                var curAmount = LifeAmount;
                var incAmount = Math.Min(amount, GetMaxLifeAmount() - curAmount);
                wallet.Increase(CurrencyType.Life, incAmount, string.Empty);
                LifeAnalytics.SendIncomeEvent(incAmount, wallet.GetCount(CurrencyType.Life), Reason.In.LifeInByTime);
            }

            lastLifeRestoreDate = GetCurrentDateTime();
            nextLifeRestoreDate = lastLifeRestoreDate.AddSeconds(lifeConfiguration.lifeCooldownSec);
            OnTimerUpdate?.Invoke((HasMaxLifeAmount(), (int)(nextLifeRestoreDate - GetCurrentDateTime()).TotalSeconds));
        }

        private void AppInterruptObserver_Resume()
        {
            ExecuteUpdate(0);
        }


        private void ExecuteUpdate(float dt) 
        {
            lastUpdateSeconds -= dt;
            if (lastUpdateSeconds > 0)
                return;
            lastUpdateSeconds = 1 + lastUpdateSeconds;

            OnTimerUpdate?.Invoke(GetUpdateData());
            if (HasMaxLifeAmount())
                return;
            if (nextLifeRestoreDate <= GetCurrentDateTime())
                RestoreLife();
        }


        public bool HasMaxLifeAmount()
        {
            return LifeAmount >= GetMaxLifeAmount();
        }


        private int GetMaxLifeAmount() 
        {
            return lifeConfiguration.defaultMaxCount;
        }

        private void TimeCyclesService_OnCycleReset(TimeCycleType cycleType)
        {
            if (cycleType != TimeCycleType.Daily)
                return;

            rewardedResurrectCount = 0;
            rewardedLifeCount = 0;
        }

#if PR_CHEAT
        public void CheatReduceLifeCd() 
        {
            nextLifeRestoreDate = GetCurrentDateTime().AddSeconds(5);
            lastLifeRestoreDate = nextLifeRestoreDate - TimeSpan.FromSeconds(lifeConfiguration.lifeCooldownSec);
        }

        public void CheatAddMinutes(int minutes)
        {
            nextLifeRestoreDate += TimeSpan.FromMinutes(minutes);
            lastLifeRestoreDate += TimeSpan.FromMinutes(minutes);
        }
#endif
    }
}