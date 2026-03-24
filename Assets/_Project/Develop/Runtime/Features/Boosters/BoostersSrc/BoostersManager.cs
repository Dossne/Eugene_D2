using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Boosters.Model;
using Features.Level;
#if PR_CHEAT
using Infrastructure.Cheat;
#endif
using Infrastructure.Configs;
using Infrastructure.DateTimeControl;
using Infrastructure.PersistentProgress;
using Infrastructure.Popups;
using Infrastructure.PurchaseSystem;
using Infrastructure.WalletSystem;
using R3;
using UnityEngine;
using VContainer.Unity;
using Progress = Infrastructure.PersistentProgress.Progress;

namespace Features.Boosters
{
    public class BoostersManager : ISavable, ITickable
    {
        public Action<string, PurchaseOfferUiType> OnRedirectToShop;

        public readonly Subject<BoosterType> OnBoosterUpdated = new();

        private readonly PopupService popupService;
        private readonly BoosterConfig boosterConfig;
        private readonly Dictionary<BoosterType, BoosterModel> registry = new();
        private readonly CancellationTokenSource cts;
        private readonly Wallet wallet;
        private readonly LevelService levelService;
        private readonly DateTimeService dateTimeService;

        private List<BoosterType> selectedPreBoosters = new();
        private BoosterBuyPopup boosterBuyPopup;
        private BoostersState boosterState;

        private bool isInit;


        public BoostersManager(
            PopupService popupService,
            ConfigProvider configProvider,
            Wallet wallet,
            LevelService levelService,
            DateTimeService dateTimeService)
        {
            this.popupService = popupService;
            this.boosterConfig = configProvider.BoosterConfig;
            this.wallet = wallet;
            this.levelService = levelService;
            this.dateTimeService = dateTimeService;

            cts = new CancellationTokenSource();
        }


        void ISavable.Load(Progress progress)
        {
            boosterState = progress.gameState.booster;
        }


        void ISavable.Save(Progress progress)
        {
            progress.gameState.booster = boosterState;
        }


        void ITickable.Tick()
        {
            UpdateInfiniteBoosters();
        }


        public void Initialize()
        {
            if (isInit)
                return;

            CreateBoosterModels();

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            if (boosterBuyPopup != null)
            {
                boosterBuyPopup.Deinitialize();
                boosterBuyPopup.OnBuyClick -= BoostersBuyPopup_OnBuyClick;
            }

            registry.Clear();

            isInit = false;
        }


        public void OpenBoosterBuyPopup(BoosterModel boosterModel)
        {
            OpenBoosterBuyPopupAsync(boosterModel).Forget();
        }


        public void SetSelectedPreBoosters(HashSet<BoosterType> value)
        {
            selectedPreBoosters = value.ToList();
        }


        public List<BoosterType> GetSelectedPreBoosters()
        {
            return new List<BoosterType>(selectedPreBoosters);
        }


        public void ClearSelectedPreBoosters()
        {
            selectedPreBoosters.Clear();
        }


        public bool TryGetBoosterModel(BoosterType type, out BoosterModel boosterModel)
        {
            return registry.TryGetValue(type, out boosterModel);
        }


        public bool TryGetBoosterIconName(BoosterType type, out string result)
        {
            if (registry.TryGetValue(type, out var boosterModel))
            {
                result = boosterModel.IconName;
                return true;
            }

            result = null;
            return false;
        }


        public bool TryGetBoosterInfiniteIconName(BoosterType type, out string result)
        {
            if (registry.TryGetValue(type, out var boosterModel))
            {
                result = boosterModel.InfiniteIconName;
                return true;
            }

            result = null;
            return false;
        }


        public void RefreshBoosterState(BoosterModel boosterModel)
        {
            BoosterStateType newState;

            if (levelService.CurrentLevelNumber < boosterModel.UnlockLevel)
            {
                newState = BoosterStateType.Locked;
            }
            else if (levelService.CurrentLevelNumber == boosterModel.UnlockLevel
                  && !levelService.HasLoseOnCurrentLevel
                  && boosterModel.IsFreeOnUnlockLevel)
            {
                newState = BoosterStateType.Free;
            }
            else if (IsInfiniteBooster(boosterModel))
            {
                newState = BoosterStateType.Infinite;
            }
            else if (boosterModel.CurrentCount > 0)
            {
                newState = BoosterStateType.Ready;
            }
            else
            {
                newState = BoosterStateType.NeedBuy;
            }

            if (boosterModel.CurrentState != newState)
            {
                boosterModel.SetState(newState);
            }
        }


        public void AddBooster(BoosterType type, int amount)
        {
            BoosterModel boosterModel = registry[type];
            boosterModel.Increase(amount);
            RefreshBoosterState(boosterModel);
            OnBoosterUpdated.OnNext(type);
        }


        public void AddInfiniteBooster(BoosterType boosterType, int timeLengthMin)
        {
            if (!TryGetBoosterModel(boosterType, out BoosterModel boosterModel))
            {
                return;
            }

            if (IsInfiniteBooster(boosterModel))
            {
                boosterModel.SetInfiniteExpiresTime(boosterModel.InfiniteExpiresTime.AddMinutes(timeLengthMin));
            }
            else
            {
                boosterModel.SetInfiniteExpiresTime(GetCurrentDateTime().AddMinutes(timeLengthMin));
                RefreshBoosterState(boosterModel);
            }

            OnBoosterUpdated.OnNext(boosterType);
        }


        public bool IsInfiniteBooster(BoosterModel boosterModel)
        {
            return boosterModel.InfiniteExpiresTime >= GetCurrentDateTime();
        }


        public List<BoosterType> GetInfinitePreBoosters()
        {
            List<BoosterType> infiniteBoostersType = new List<BoosterType>();
            for (int i = 0; i < boosterConfig.PreBoosters.Count; ++i)
            {
                BoosterType boosterType = boosterConfig.PreBoosters[i].type;
                if (TryGetBoosterModel(boosterType, out BoosterModel boosterModel)
                 && boosterModel.CurrentState == BoosterStateType.Infinite)
                {
                    infiniteBoostersType.Add(boosterType);
                }
            }

            return infiniteBoostersType;
        }


        public float GetInfinitePreboostersExpiresTime(BoosterType boosterType)
        {
            if (TryGetBoosterModel(boosterType, out BoosterModel boosterModel))
            {
                return Mathf.Max((float)(boosterModel.InfiniteExpiresTime - GetCurrentDateTime()).TotalSeconds, 0.0f);
            }

            return 0;
        }


        private DateTime GetCurrentDateTime()
        {
            dateTimeService.TryGetServerTime(out DateTime dateTime);
            return dateTime;
        }
        
        
        private void CreateBoosterModels()
        {
            var boosterData = new List<BoosterData>(boosterConfig.PreBoosters);
            boosterData.AddRange(boosterConfig.InGameBoosters);

            foreach (var configData in boosterData)
            {
                if (!TryGetSaveState(configData.type, out BoosterItemState saveState))
                {
                    saveState = new BoosterItemState
                    {
                        type = configData.type,
                        count = configData.startCount,
                        infiniteExpiresTime = DateTime.UnixEpoch,
                    };
                    boosterState.states.Add(saveState);
                }

                BoosterModel boosterModel = CreateModel(configData.Clone(), saveState);
                registry.Add(configData.type, boosterModel);
            }
        }


        private BoosterModel CreateModel(BoosterData configData, BoosterItemState saveState)
        {
            if (configData.type == BoosterType.BonusClock)
                return new BonusClockBoosterModel(configData, saveState);

            if (boosterConfig.IsInGameBooster(configData.type))
                return new InGameBoosterModelBase(configData, saveState);

            return new BoosterModel(configData, saveState);
        }


        private bool TryGetSaveState(BoosterType type, out BoosterItemState result)
        {
            result = null;

            foreach (var itemState in boosterState.states)
            {
                if (itemState.type == type)
                {
                    result = itemState;
                    return true;
                }
            }

            return false;
        }


        private void UpdateInfiniteBoosters()
        {
            for (int i = 0; i < boosterConfig.PreBoosters.Count; ++i)
            {
                BoosterType boosterType = boosterConfig.PreBoosters[i].type;
                
                if (TryGetBoosterModel(boosterType, out BoosterModel boosterModel)
                 && boosterModel.CurrentState == BoosterStateType.Infinite
                 && !IsInfiniteBooster(boosterModel))
                {
                    RefreshBoosterState(boosterModel);
                    OnBoosterUpdated.OnNext(boosterModel.BoosterType);
                }
            }
        }


        private async UniTask OpenBoosterBuyPopupAsync(BoosterModel boosterModel)
        {
            if (boosterBuyPopup == null)
            {
                boosterBuyPopup = await popupService.GetAsync<BoosterBuyPopup>(cts.Token, true);
                boosterBuyPopup.Initialize();
                boosterBuyPopup.OnBuyClick += BoostersBuyPopup_OnBuyClick;
            }

            boosterBuyPopup.Construct(boosterModel.BoosterType,
                                      boosterModel.IconName,
                                      boosterModel.Name,
                                      boosterModel.Description,
                                      boosterModel.BuyCount,
                                      boosterModel.BuyPrice);

            boosterBuyPopup.Open();
        }


        private void BoostersBuyPopup_OnBuyClick(BoosterType type)
        {
            BoosterModel boosterModel = registry[type];
            var currency = CurrencyType.Coins;

            if (wallet.IsEnough(currency, boosterModel.BuyPrice))
            {
                wallet.Decrease(currency, boosterModel.BuyPrice, $"{Reason.Out.BoosterBuy}_{type}");
                boosterModel.Increase(boosterModel.BuyCount);
                RefreshBoosterState(boosterModel);
                boosterBuyPopup.Close();
                OnBoosterUpdated.OnNext(type);
            }
            else
            {
                OnRedirectToShop?.Invoke(ShopOpenSource.BoosterBuyPopup, PurchaseOfferUiType.BestDealHeader);
            }
        }
    }
}