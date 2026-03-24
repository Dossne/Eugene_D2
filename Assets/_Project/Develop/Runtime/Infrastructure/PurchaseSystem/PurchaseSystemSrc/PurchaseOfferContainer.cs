using Infrastructure.Configs;
using Infrastructure.DateTimeControl;
using Infrastructure.PersistentProgress;
using Infrastructure.TimeCycles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Infrastructure.PurchaseSystem
{
    public class PurchaseOfferContainer : ITickable, ISavable
    {
        public Action<(OfferId offerId, double timeRestSec)> OnOfferSecondTick;
        public Action<(OfferId offerId, PurchaseOfferUiDatasource offerUiDatasource, double timeRestSec)> OnOfferCycleReset;
        public Action<OfferId, bool/*isFirst*/> OnPurchaseCompleted;

        private IPurchaseManager purchaseManager;
        private readonly DateTimeService dateTimeService;
        private readonly PurchaseOfferFactory purchaseOfferFactory;
        private readonly TimeCyclesService timeCyclesService;
        private Dictionary<OfferId, PurchaseOfferBase> purchaseOffers = new();
        private List<PurchaseOfferTimeCycle> cycleList = new();
        private Dictionary<OfferId, PurchaseOfferTimeCycle> offerCycles = new();
        private PurchaseState purchaseSaveState = new();
        private List<PurchaseOfferBase> defaultOrderOfferList = new();

        [Inject]
        public PurchaseOfferContainer(IPurchaseManager     purchaseManager,
                                      PurchaseOfferFactory purchaseOfferFactory,
                                      TimeCyclesService    timeCyclesService,
                                      DateTimeService      dateTimeService)
        {
            this.purchaseManager = purchaseManager;
            this.dateTimeService = dateTimeService; 
            this.purchaseOfferFactory = purchaseOfferFactory;
            this.timeCyclesService = timeCyclesService;
        }

        public bool IsInitialized { get; private set; } = false;
        public double CumulativeSpentUsd => purchaseSaveState.cumulativeSpendedUsd;

        public void Initialize()
        {
            if (IsInitialized)
                return;

            InitializePurchaseOffers();
            InitializeOfferCycles();           

            purchaseManager.OnPurchaseCompleted += PurchaseManager_OnPurchaseCompleted;            

            IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!IsInitialized)
                return;

            purchaseManager.OnPurchaseCompleted -= PurchaseManager_OnPurchaseCompleted;

            IsInitialized = false;
        }

        public bool IsOfferLimitReached(OfferId inAppOfferId)
        {
            if (purchaseOffers.TryGetValue(inAppOfferId, out var purchaseOffer) && purchaseOffer != null)
                return purchaseOffer.LimitReached;

            return false;
        }


        public bool OfferExists(OfferId inAppOfferId)
        {
            return purchaseOffers.TryGetValue(inAppOfferId, out var purchaseOffer) && purchaseOffer != null;
        }

        void ITickable.Tick()
        {
            for (int i = 0; i < cycleList.Count; i++)
                cycleList[i].Tick();
        }

        public float GetOfferUsdPrice(OfferId inAppOfferId)
        {
            return purchaseOffers[inAppOfferId].Price;
        }

        public PurchaseOfferUiDatasource GetOfferUiDatasource(OfferId inAppOfferId)
        {
            if (purchaseOffers.TryGetValue(inAppOfferId, out var purchaseOffer))
                return GetUiDatasource(purchaseOffer);

            Debug.Log($"{inAppOfferId} is not active!");
            return null; 
        }

        public OfferId GetFirstActiveOfferOfType(OfferGroup offerGroup, int filterLevel)
        {
            for (int i = 0; i < defaultOrderOfferList.Count; i++)
            {
                if (!offerCycles.TryGetValue(defaultOrderOfferList[i].Id, out var cycle))
                    cycle = null;

                if (defaultOrderOfferList[i].OfferGroup == offerGroup && !defaultOrderOfferList[i].IsExcluded(filterLevel, 
                                                                                                              purchaseSaveState.cumulativeSpendedUsd, 
                                                                                                              cycle, 
                                                                                                              purchaseSaveState.purchasedInAppDataList))
                    return defaultOrderOfferList[i].Id;
            }

            return OfferId.none;
        }

        public PurchaseOfferTimeCycle GetPurchaseOfferTimeCycle(OfferId inAppOfferId)
        {
            if (offerCycles.TryGetValue(inAppOfferId, out var purchaseOfferTimeCycle))
                return purchaseOfferTimeCycle;
        
            return null;
        }

        public List<PurchaseOfferUiDatasource> GetOfferUiDatasources(int filterLevel)
        {
            List<PurchaseOfferUiDatasource> result = new();
            List<List<PurchaseOfferBase>> groupOrder = new();
            if (defaultOrderOfferList.Count == 0)
                return result;

            OfferGroup currOfferGroup = defaultOrderOfferList[0].OfferGroup;
            OfferGroup prevOfferGroup = defaultOrderOfferList[0].OfferGroup;

            for (int i = 0; i < defaultOrderOfferList.Count; i++)
            {
                prevOfferGroup = currOfferGroup;
                currOfferGroup = defaultOrderOfferList[i].OfferGroup;

                if (!offerCycles.TryGetValue(defaultOrderOfferList[i].Id, out var cycle))
                    cycle = null;

                if (defaultOrderOfferList[i].IsExcluded(filterLevel, 
                                                        purchaseSaveState.cumulativeSpendedUsd, 
                                                        cycle,
                                                        purchaseSaveState.purchasedInAppDataList))
                    continue;

                if (prevOfferGroup != currOfferGroup || groupOrder.Count == 0)
                    groupOrder.Add(new());

                groupOrder[^1].Add(defaultOrderOfferList[i]);
            }

            for (int i = 0; i < groupOrder.Count; i++)
            {
                if (groupOrder[i][0].OfferGroup == OfferGroup.BestDeal && purchaseSaveState.purchasedInAppDataList.Count > 0)
                    groupOrder[i] = groupOrder[i].OrderBy(x => (x.Id == OfferId.rh_starter_pack ? 0 : 1))
                                                 .ThenByDescending(x => x.Price).ToList();
                result.AddRange(groupOrder[i].Select(x => GetUiDatasource(x)));
            }

            if (!result.Exists(x => x.offerGroup == OfferGroup.SuperDiscount) && !result.Exists(x => x.id == OfferId.rh_starter_pack))
                result.RemoveAll(x => x.id == OfferId.rh_special_offer_header);

            return result;
        }        

        public void Load(Progress progress)
        {
            purchaseSaveState = progress.purchaseState;           
        }

        public void Save(Progress progress)
        {
            if (purchaseSaveState.limitedOffers == null)
                purchaseSaveState.limitedOffers = new();
            else
                purchaseSaveState.limitedOffers.Clear();

            foreach (var item in purchaseOffers)
                if (item.Value.IsLimitedPerCycle && item.Value.BoughtThisCycleCount > 0)
                    purchaseSaveState.limitedOffers.Add(new() { offerId = item.Value.Id, count = item.Value.BoughtThisCycleCount } );

            progress.purchaseState = purchaseSaveState;
        }

        public bool HasAvailableRewarded()
        {
            foreach (var item in purchaseOffers)
                if (item.Value.Type == OfferType.RewardedAd && item.Value.BoughtThisCycleCount < item.Value.MaxLimitPerCycle)
                    return true;
            return false;
        }

        private void InitializePurchaseOffers()
        {
            defaultOrderOfferList.Clear();
            purchaseOffers.Clear();
            var activeOfferDataList = purchaseManager.ActiveOfferDataList;
            for (int i = 0; i < activeOfferDataList.Count; i++)
                purchaseOffers.Add(activeOfferDataList[i].id, purchaseOfferFactory.CreatePurchaseOffer(activeOfferDataList[i]));
            defaultOrderOfferList = purchaseOffers.Values.OrderBy(x => x.DefaultOrder).ToList();

            if (purchaseSaveState.limitedOffers == null)
                return;

            for (int i = 0; i < purchaseSaveState.limitedOffers.Count; i++)
                if (purchaseOffers.TryGetValue(purchaseSaveState.limitedOffers[i].offerId, out var offerBase))
                    offerBase.IncreaseBoughtThisCycle(purchaseSaveState.limitedOffers[i].count);
        }

        private void InitializeOfferCycles()
        {
            cycleList.Clear();
            cycleList.Add(new SuperDiscountTimeCycle(timeCyclesService, dateTimeService));
            cycleList.Add(new CurrencyRewardTimeCycle(timeCyclesService, dateTimeService));

            offerCycles.Clear();
            for (int i = 0; i < cycleList.Count; i++)
            {
                cycleList[i].OnCycleReset += Cycle_OnCycleReset;
                cycleList[i].OnSecondTick += Cycle_OnSecondTick;
                for (int j = 0; j < cycleList[i].Offers.Count; j++)
                    offerCycles.Add(cycleList[i].Offers[j], cycleList[i]);
            }
        }

        private void CompleteOfferPurchase(OfferId inAppOfferId, out bool isFirst)
        {
            isFirst = !purchaseSaveState.purchasedInAppDataList.Exists(x => x.inAppOfferId == inAppOfferId);
            if (!purchaseOffers.TryGetValue(inAppOfferId, out var offer))
                return;

            offer.CompletePurchase(isFirst);
            if (offerCycles.TryGetValue(inAppOfferId, out var cycle))
            {
                var cycleOffers = cycle.Offers;
                for (int i = 0; i < cycleOffers.Count; i++)
                    if (purchaseOffers.TryGetValue(cycleOffers[i], out var cycleOffer))
                        cycleOffer.IncreaseBoughtThisCycle();
            }

            purchaseSaveState.cumulativeSpendedUsd += GetOfferUsdPrice(inAppOfferId);
            purchaseSaveState.purchasedInAppDataList.Add(new() { inAppOfferId = inAppOfferId, dateTime = GetCurrentDateTime() });
        }

        private DateTime GetCurrentDateTime()
        {
            var utcNow = DateTime.UtcNow;
            if (dateTimeService.TryGetServerTime(out var dateTime))
                utcNow = dateTime;
            return utcNow;
        }

        private PurchaseOfferUiDatasource GetUiDatasource(PurchaseOfferBase purchaseOffer)
        {
            if (purchaseOffer == null)
                return null;

            string priceString = String.Empty;
            if(purchaseOffer.OfferGroup != OfferGroup.None && purchaseOffer.Type == OfferType.InApp)
            {
                priceString = purchaseManager.GetLocalizedPrice(purchaseOffer.Id);
            }

            return new()
            {
                id = purchaseOffer.Id,
                offerType = purchaseOffer.Type,
                purchaseOfferUiType = purchaseOffer.OfferUiType,
                offerGroup = purchaseOffer.OfferGroup,
                complexReward = purchaseOffer.ComplexReward,
                priceString = priceString,
                isBestPrice = purchaseOffer.IsBestPrice,
                isPopular = purchaseOffer.IsPopular,
                isDecorative = purchaseOffer.IsDecorative,
                isEnabled = purchaseOffer.IsEnabled,
                maxLimit = purchaseOffer.MaxLimitPerCycle,
                curLimit = purchaseOffer.BoughtThisCycleCount
            };
        }

        private void Cycle_OnCycleReset(PurchaseOfferTimeCycle purchaseOfferTimeCycle)
        {
            var offers = purchaseOfferTimeCycle.Offers;
            for (int i = 0; i < offers.Count; i++) 
            {
                if (!purchaseOffers.ContainsKey(offers[i]))
                    continue;
                purchaseOffers[offers[i]].ResetBoughtThisCycle();
                OnOfferCycleReset?.Invoke((offers[i], GetUiDatasource(purchaseOffers[offers[i]]), purchaseOfferTimeCycle.OfferActiveTimeRest()));
            }
        }

        private void Cycle_OnSecondTick(PurchaseOfferTimeCycle purchaseOfferTimeCycle)
        {
            var offers = purchaseOfferTimeCycle.Offers;
            for (int i = 0; i < offers.Count; i++)
                OnOfferSecondTick?.Invoke((offers[i], purchaseOfferTimeCycle.OfferActiveTimeRest()));
        }

        private void PurchaseManager_OnPurchaseCompleted(OfferId id)
        {
            CompleteOfferPurchase(id, out bool isFirst);
            OnPurchaseCompleted?.Invoke(id, isFirst);
        }
    }
}