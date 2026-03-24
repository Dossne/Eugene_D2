using Infrastructure.Localization;
using System;

namespace Features.Boosters.Model
{
    //Holds state and config. Lifetime = MainScope
    public class BoosterModel
    {
        protected readonly BoosterData configData;
        private readonly BoosterItemState saveState;


        public BoosterModel(BoosterData configData, BoosterItemState saveState)
        {
            this.configData = configData;
            this.saveState = saveState;
        }

        //config
        public virtual string Description => LocalizationService.I.Get(configData.descriptionKey);
        public BoosterType BoosterType => configData.type;
        public string IconName => configData.iconName;
        public string InfiniteIconName => configData.infiniteIconName;
        public string Name => LocalizationService.I.Get(configData.nameKey);
        public int BuyCount => configData.buyCount;
        public int BuyPrice => configData.buyPrice;
        public int UnlockLevel => configData.unlockLevel;
        public float DurationSec => configData.durationSec;
        public float BoostedValue => configData.boostedValue;
        public string LockLevelText => LocalizationService.I.Get(LocKeys.Boosters.Lock, UnlockLevel.ToString());
        public string FreeText => LocalizationService.I.Get(LocKeys.Boosters.Free);
        public bool IsFreeOnUnlockLevel => configData.isFreeOnUnlockLevel;

        //state
        public BoosterStateType CurrentState { get; private set; }
        public bool IsSelectable => CurrentState is BoosterStateType.Ready or BoosterStateType.Free;
        public int CurrentCount => saveState.count;
        public DateTime InfiniteExpiresTime => saveState.infiniteExpiresTime;


        public void Decrease()
        {
            if (CurrentState == BoosterStateType.Infinite || CurrentState == BoosterStateType.Free)
            {
                return;
            }

            this.saveState.count--;
        }


        public void Increase(int addedValue)
        {
            this.saveState.count += addedValue;
        }


        public void SetState(BoosterStateType state)
        {
            CurrentState = state;
        }


        public void SetInfiniteExpiresTime(DateTime infiniteExpiresTime)
        {
            this.saveState.infiniteExpiresTime = infiniteExpiresTime;
        }
    }
}