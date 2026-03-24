using Infrastructure.Localization;
using Infrastructure.Popups;
using System;
using Infrastructure.BroTweens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Infrastructure.Ads;

namespace Features.LifeUi
{
    public class BuyLifePopup : PopupBase
    {
        [Header("Components")]
        [SerializeField] private Button buyBtn;
        [SerializeField] private RewardAdsButton rewardBtn;
        [SerializeField] private Image boosterIcon;
        [SerializeField] private RectTransform backRect;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI descriptionTxt;
        [SerializeField] private TextMeshProUGUI countTxt;
        [SerializeField] private TextMeshProUGUI timeTxt;
        [SerializeField] private TextMeshProUGUI refillTxt;
        [SerializeField] private TextMeshProUGUI priceTxt;
        [SerializeField] private TextMeshProUGUI rewardAdsTxt;

        [Header("Components")]
        [SerializeField] private float adsHeight = 1500f;
        [SerializeField] private float noAdsHeight = 1200f;

        private Action onCallback;
        private Action onRewardCallback;


        public void Construct(int price, Action onCallback, bool withReward = false, string rewardPlacement = "", Action rewardCallback = null)
        {
            this.onCallback = onCallback;
            priceTxt.text = price.ToString();
            backRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, withReward ? adsHeight : noAdsHeight);

            rewardBtn.SetObjectActive(withReward);

            if (withReward)
            {
                rewardBtn.Construct(rewardPlacement, this.name, rewardCallback);
                rewardAdsTxt.text = LocalizationService.I.Get(LocKeys.BuyLifePopup.Rewarded);
            }
        }

        public void UpdateData(int lifeCount, string timeString, bool withReward = false) 
        {
            countTxt.text = lifeCount.ToString();
            timeTxt.text = timeString;
            backRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, withReward ? adsHeight : noAdsHeight);
            rewardBtn.SetObjectActive(withReward);
        }

        protected override void OnInitialize()
        {
            this.headerTxt.text = LocalizationService.I.Get(LocKeys.BuyLifePopup.Header);
            this.descriptionTxt.text = LocalizationService.I.Get(LocKeys.BuyLifePopup.TimeToNext);
            this.refillTxt.text = LocalizationService.I.Get(LocKeys.BuyLifePopup.Refill);
            buyBtn.onClick.AddListener(ButtonClick);
            rewardBtn.Initialize();
        }


        protected override void OnDeinitialize()
        {
            buyBtn.onClick.RemoveListener(ButtonClick);
            rewardBtn.Deinitialize();
        }

        private void ButtonClick()
        {
            BroTween.ClickBounceWithCallBack(buyBtn, buyBtn.transform, this, target => target.ClickInvoke())
                    .Play();
        }


        private void ClickInvoke()
        {
            onCallback?.Invoke();
        }
    }
}