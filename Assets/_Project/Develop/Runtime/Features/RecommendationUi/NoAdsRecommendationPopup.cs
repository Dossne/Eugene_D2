using Features.NoAdsUi;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.PurchaseSystem;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Features.RecommendationUi
{
    public class NoAdsRecommendationPopup : PopupBase
    {
        [SerializeField] private Button noAdsBtn;
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI descriptionTxt;
        [SerializeField] private TextMeshProUGUI withAdsDescriptionTxt;
        [SerializeField] private TextMeshProUGUI noAdsDescriptionTxt;
        [SerializeField] private TextMeshProUGUI withAdsButtonTxt;
        [SerializeField] private TextMeshProUGUI noAdsButtonTxt;

        private NoAdsUiController noAdsUiController;

        private Action onAnimationStartCallback;


        [Inject]
        public void Construct(NoAdsUiController noAdsUiController) 
        {
            this.noAdsUiController = noAdsUiController;
        }

        protected override void OnInitialize() 
        {
            noAdsBtn.onClick.AddListener(NoAdsButtonHandler);

            headerTxt.text             = LocalizationService.I.Get(LocKeys.NoAdsRecommendation.Header);
            descriptionTxt.text        = LocalizationService.I.Get(LocKeys.NoAdsRecommendation.Description);
            withAdsDescriptionTxt.text = LocalizationService.I.Get(LocKeys.NoAdsRecommendation.WithAdsDescription);
            noAdsDescriptionTxt.text   = LocalizationService.I.Get(LocKeys.NoAdsRecommendation.NoAdsDescription);
            withAdsButtonTxt.text      = LocalizationService.I.Get(LocKeys.NoAdsRecommendation.WithAdsButton);
            noAdsButtonTxt.text        = LocalizationService.I.Get(LocKeys.NoAdsRecommendation.NoAdsButton);
        }
                
        protected override void OnDeinitialize() 
        {
            noAdsBtn.onClick.RemoveListener(NoAdsButtonHandler);
        }


        protected override void AnimationStartHandler()
        {
            base.AnimationStartHandler();
            onAnimationStartCallback?.Invoke();
        }


        public void SetAnimationStartCallback(Action onAnimationStartCallback)
        {
            this.onAnimationStartCallback = onAnimationStartCallback;
        }


        private void NoAdsButtonHandler()
        {
            Close();
            noAdsUiController.ShowPurchasePopup(OpeningMethod.Yourself, PurchaseOfferShowSource.Recommendation);            
        }
    }
}