using System.Collections.Generic;
using Infrastructure.Popups;
using UnityEngine;
using Infrastructure.PurchaseSystem;
using System;
using UnityEngine.UI;
using TMPro;
using Infrastructure.Localization;
using UnityEngine.Splines;
using Febucci.UI;
using Cysharp.Threading.Tasks;
using Infrastructure.BroTweens;
using Infrastructure.Reward;

namespace Features.ShopUi
{
    public class ShopRewardPopup : PopupBase
    {
        [SerializeField] private Button collectRewardButton;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI tapToContinueText;
        [SerializeField] private TextAnimator_TMP headerTextAnimator;
        [SerializeField] private TypewriterByCharacter typewriter;

        [SerializeField] [Range(0, 2)] private float splineIconsScale = 1;
        [SerializeField] [Range(0, 1)] private float partOffset;
        [SerializeField] private SplineContainer boosterSplineContainer;
        [SerializeField] private SplineContainer preBoosterSplineContainer;

        [SerializeField] private PurchaseRewardItemView mainRewardView;
        [SerializeField] private PurchaseRewardItemView noAdsRewardView;
        [SerializeField] private PurchaseRewardItemView purchaseRewardItemViewPf;

        [Header("Reward Animation")]
        [SerializeField] [Range(0, 3)] [Tooltip("System purchase popup on Android have 2 sec animation")] private float startDelay = 2f;
        [SerializeField] [Range(0, 3)] private float betweenAnimationLength = 0.25f;
        [SerializeField] [Range(0, 3)] private float itemAnimationLength = 0.5f;
        private List<PurchaseRewardItemView> instantiatedViews = new();
        private List<(PurchaseRewardItemView view, Vector3 targetScale, float startTime)> animatedViews = new();
        private float timeLineCounter = 0;

        [Header("Debug")]
        [TriInspector.ShowInInspector, TriInspector.ReadOnly] private BroTweenSafe scaleSeq;


        private List<PurchaseOfferUiDatasource> offerDataList;
        private Action onCollectRewardCallback;


        private void OnDestroy()
        {
            scaleSeq.Kill();
        }


        public void Construct(Action onCollectRewardCallback, string headerKey)
        {
            this.onCollectRewardCallback = onCollectRewardCallback;
            headerText.text = LocalizationService.I.Get(headerKey);
            tapToContinueText.text = LocalizationService.I.Get(LocKeys.Purchase.TapToCollect);
        }


        public override void Open()
        {
            for (int i = 0; i < animatedViews.Count; i++)
                animatedViews[i].view.transform.localScale = Vector3.zero;

            base.Open();
            DelayedAnimationStart(startDelay).Forget();
        }


        public void SetRewards(RewardItemVisualData mainRewardData,
                               RewardItemVisualData noAdsRewardData = null,
                               List<RewardItemVisualData> boosterRewards = null,
                               List<RewardItemVisualData> preBoosterRewards = null,
                               List<RewardItemVisualData> infinitePreBoosterRewards = null,
                               RewardItemVisualData infiniteLifeRewardData = null)
        {
            for (int i = 0; i < instantiatedViews.Count; i++)
            {
                if (instantiatedViews[i] != null)
                    Destroy(instantiatedViews[i].gameObject);
            }

            instantiatedViews.Clear();

            mainRewardView.gameObject.SetActive(mainRewardData != null);
            if (mainRewardData != null)
                mainRewardView.Construct(mainRewardData);

            noAdsRewardView.gameObject.SetActive(noAdsRewardData != null);
            if (noAdsRewardData != null)
                noAdsRewardView.Construct(noAdsRewardData);

            animatedViews.Clear();
            SetSplineRewards(boosterSplineContainer, boosterRewards, animatedViews);

            if (preBoosterRewards != null && preBoosterRewards.Count > 0 && infinitePreBoosterRewards != null && infinitePreBoosterRewards.Count > 0)
            {
                Debug.LogWarning($"Prebooster and inf prebooster in reward in the same time!");
            }
            else if (preBoosterRewards != null && preBoosterRewards.Count > 0)
            {
                SetSplineRewards(preBoosterSplineContainer, preBoosterRewards, animatedViews);
            }
            else if (infinitePreBoosterRewards != null && infinitePreBoosterRewards.Count > 0)
            {
                if (infiniteLifeRewardData != null)
                    infinitePreBoosterRewards.Add(infiniteLifeRewardData);

                SetSplineRewards(preBoosterSplineContainer, infinitePreBoosterRewards, animatedViews);
            }

            if (noAdsRewardData != null)
            {
                animatedViews.Add((noAdsRewardView, noAdsRewardView.transform.localScale, timeLineCounter));
                timeLineCounter += betweenAnimationLength;
            }

            if (mainRewardData != null)
            {
                animatedViews.Add((mainRewardView, mainRewardView.transform.localScale, timeLineCounter));
                timeLineCounter += betweenAnimationLength;
            }
        }

        
        protected override void OnInitialize()
        {
            collectRewardButton.onClick.AddListener(CollectRewardHandler);
        }


        protected override void OnDeinitialize()
        {
            collectRewardButton.onClick.RemoveListener(CollectRewardHandler);
        }

        protected override void OnBeginClose()
        {
            scaleSeq.Kill();
        }

        private async UniTaskVoid DelayedAnimationStart(float delay)
        {
            collectRewardButton.enabled = false;
            tapToContinueText.gameObject.SetActive(false);
            headerTextAnimator.ResetState();
            await UniTask.WaitForSeconds(delay, true);
            typewriter.StartShowingText();
            typewriter.onTextShowed.AddListener(ShowRewardAnimation);
        }


        private void SetSplineRewards(SplineContainer splineContainer,
                                      List<RewardItemVisualData> rewards,
                                      List<(PurchaseRewardItemView view, Vector3 targetScale, float startTime)> animatedList)
        {
            splineContainer.gameObject.SetActive(rewards != null && rewards.Count > 0);
            if (rewards == null)
                return;

            var spline = splineContainer.Spline;
            float spreadRange = partOffset * rewards.Count;
            float startOffset = (1f - spreadRange + partOffset) / 2f;
            var splineTimeLineCounter = 0f;
            for (int i = 0; i < rewards.Count; i++)
            {
                float buttonSplinePosition = startOffset + partOffset * i;
                var buttonPosition = spline.EvaluatePosition(buttonSplinePosition);
                var view = Instantiate(purchaseRewardItemViewPf, splineContainer.transform);
                view.Construct(rewards[i]);
                view.transform.localPosition = buttonPosition;
                view.transform.localScale = Vector3.one * splineIconsScale;
                instantiatedViews.Add(view);

                animatedList.Add((view, view.transform.localScale, splineTimeLineCounter));
                splineTimeLineCounter += betweenAnimationLength;
                if (timeLineCounter < splineTimeLineCounter)
                    timeLineCounter = splineTimeLineCounter;
            }
        }


        private void CollectRewardHandler()
        {
            scaleSeq.Complete();
            onCollectRewardCallback?.Invoke();
        }


        private void ShowRewardAnimation()
        {
            typewriter.onTextShowed.RemoveListener(ShowRewardAnimation);

            BroSequence seq = BroTween.Sequence().SetUpdate(true).OnComplete(ActivateTapToContinue);

            for (int i = 0; i < animatedViews.Count; i++)
                seq.Insert(animatedViews[i].startTime, BroTween.Scale(animatedViews[i].view.transform, animatedViews[i].targetScale, itemAnimationLength).SetEase(Ease.OutBounce));
            
            scaleSeq = seq.ToSafe();
            scaleSeq.Play();
        }


        private void ActivateTapToContinue()
        {
            tapToContinueText.gameObject.SetActive(true);
            collectRewardButton.enabled = true;
        }
    }
}