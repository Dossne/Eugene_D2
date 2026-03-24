using Features.ScrollList;
using Infrastructure.Ads;
using Infrastructure.Localization;
using Infrastructure.PurchaseSystem;
using Infrastructure.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.ShopUi
{
    public class OneCurrencyRewardOfferScrollElement : ShopOfferScrollElement
    {
        [SerializeField] private Button buyButton;
        [SerializeField] private Image currencyIcon;
        [SerializeField] private TextMeshProUGUI costLabel;
        [SerializeField] private TextMeshProUGUI offerLabel;
        [SerializeField] private RewardAdsButton rewardAdsBtn;

        [SerializeField] private TextMeshProUGUI freeTxt;
        [SerializeField] private TextMeshProUGUI countTxt;
        [SerializeField] private TextMeshProUGUI timerTxt;
        [SerializeField] private Image adIcon;

        [SerializeField] private GameObject notifier;

        private PurchaseOfferContainer purchaseOfferContainer;

        protected override void InitializeImpl()
        {
            var oneCurrencyReward = datasource.complexReward.currencyOfferRewards[0];

            currencyIcon.sprite = oneCurrencyReward.overrideIconId.IsNullOrEmpty() ? spriteAtlasService.GetCurrencyIcon(oneCurrencyReward.currencyType) 
                                                                                   : spriteAtlasService.GetFromMain(oneCurrencyReward.overrideIconId);
            offerLabel.text = Utils.GetSpaceSeparatedNumberString(oneCurrencyReward.amount);

            rewardAdsBtn.Construct("shop", this.name, HandleClick);
            rewardAdsBtn.SetOnClickDelegate(SetButtonsNotInteractable);
            rewardAdsBtn.SetSkipDelegate(SetButtonsInteractable);

            rewardAdsBtn.Initialize();

            purchaseOfferContainer.OnOfferSecondTick += PurchaseOfferContainer_OnOfferSecondTick;
            purchaseOfferContainer.OnOfferCycleReset += PurchaseOfferContainer_OnOfferCycleReset;
            UpdateButtonState();
        }

        protected override void DeinitializeImpl()
        {
            purchaseOfferContainer.OnOfferCycleReset -= PurchaseOfferContainer_OnOfferCycleReset;
            purchaseOfferContainer.OnOfferSecondTick -= PurchaseOfferContainer_OnOfferSecondTick;
            rewardAdsBtn.Deinitialize();
        }

        public void SetPurchaseOfferContainer(PurchaseOfferContainer purchaseOfferContainer)
        {
            this.purchaseOfferContainer = purchaseOfferContainer;
        }

        private void SetButtonsNotInteractable()
        {
            rewardAdsBtn.SetButtonsInteractable(false);
        }


        private void SetButtonsInteractable()
        {
            rewardAdsBtn.SetButtonsInteractable(true);
        }

        private void PurchaseOfferContainer_OnOfferSecondTick((OfferId offerId, double timeRestSec) offerSecondTick)
        {
            if (datasource == null)
                return;

            if (offerSecondTick.offerId != datasource.id)
                return;

            UpdateButtonState();

            freeTxt.text = LocalizationService.I.Get(LocKeys.Common.Free);
            countTxt.text = $"{datasource.maxLimit - datasource.curLimit}/{datasource.maxLimit}";
            if (timerTxt != null)
                timerTxt.text = TimeUtils.GetTimeString(offerSecondTick.timeRestSec);
        }

        private void PurchaseOfferContainer_OnOfferCycleReset((OfferId offerId, PurchaseOfferUiDatasource offerUiDatasource, double timeRestSec) offerResetTick)
        {
            if (datasource == null)
                return;

            if (offerResetTick.offerId != datasource.id)
                return;

            datasource = offerResetTick.offerUiDatasource;
            PurchaseOfferContainer_OnOfferSecondTick((offerResetTick.offerId, offerResetTick.timeRestSec));
        }

        private void UpdateButtonState() 
        {
            notifier.SetObjectActive(datasource.curLimit < datasource.maxLimit);

            freeTxt.gameObject.SetObjectActive(datasource.curLimit < datasource.maxLimit);
            countTxt.gameObject.SetObjectActive(datasource.curLimit < datasource.maxLimit);
            timerTxt.gameObject.SetObjectActive(datasource.curLimit >= datasource.maxLimit);

            if (datasource.curLimit >= datasource.maxLimit)
                SetButtonsNotInteractable();
            else 
                SetButtonsInteractable();

            adIcon.gameObject.SetObjectActive(datasource.curLimit < datasource.maxLimit);
        }
    }
}