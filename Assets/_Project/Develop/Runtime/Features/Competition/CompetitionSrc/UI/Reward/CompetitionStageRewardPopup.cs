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
using Infrastructure.Utilities;

namespace Features.Competition.Reward
{
    public sealed class CompetitionStageRewardPopup : PopupBase
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
        [SerializeField] private PurchaseRewardItemView purchaseRewardItemViewPf;

        [Header("Reward Animation")]
        [SerializeField] private float startDelay;
        [SerializeField] private float betweenAnimationDelay = 0.1f;
        [SerializeField] private float scaleDuration = 0.25f;
        [SerializeField] private AnimationCurve scaleEase;
        [SerializeField] private float tapToContinueActivateDelay = 0.65f;

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

        [TriInspector.Button]
        public override void Open()
        {
            collectRewardButton.enabled = false;

            for (int i = 0; i < animatedViews.Count; i++)
                animatedViews[i].view.transform.localScale = Vector3.zero;

            base.Open();
            DelayedAnimationStart(startDelay).Forget();
        }

        public void SetRewards(RewardItemVisualData mainRewardData, List<RewardItemVisualData> boosters, List<RewardItemVisualData> preBoosters)
        {
            for (int i = 0; i < instantiatedViews.Count; i++)
            {
                if (instantiatedViews[i] != null)
                    Destroy(instantiatedViews[i].gameObject);
            }

            instantiatedViews.Clear();
            animatedViews.Clear();

            var haveMain = mainRewardData != null;

            mainRewardView.gameObject.SetActive(haveMain);
            if (haveMain)
            {
                mainRewardView.Construct(mainRewardData);
                animatedViews.Add((mainRewardView, mainRewardView.transform.localScale, timeLineCounter));
                timeLineCounter += betweenAnimationDelay;
            }

            var haveBoosters = boosters != null && boosters.Count > 0;
            boosterSplineContainer.gameObject.SetObjectActive(haveBoosters);

            if (haveBoosters)
                SetSplineRewards(boosterSplineContainer, boosters, animatedViews);

            var havePreBoosters = preBoosters != null && preBoosters.Count > 0;
            preBoosterSplineContainer.gameObject.SetObjectActive(havePreBoosters);

            if (havePreBoosters)
                SetSplineRewards(preBoosterSplineContainer, preBoosters, animatedViews);
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
            tapToContinueText.gameObject.SetActive(false);
            
            await UniTask.NextFrame(gameObject.GetCancellationTokenOnDestroy());
            
            headerTextAnimator.ResetState();
            await UniTask.WaitForSeconds(delay, true, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            
            typewriter.onTextShowed.AddListener(ShowRewardAnimation);
            typewriter.StartShowingText();
        }

        private void SetSplineRewards(SplineContainer splineContainer,
                                      List<RewardItemVisualData> rewards,
                                      List<(PurchaseRewardItemView view, Vector3 targetScale, float startTime)> animatedList)
        {
            if (rewards == null)
                return;

            var spline = splineContainer.Spline;
            float spreadRange = partOffset                      * rewards.Count;
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
                splineTimeLineCounter += betweenAnimationDelay;

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
                seq.Insert(animatedViews[i].startTime, BroTween.Scale(animatedViews[i].view.transform, animatedViews[i].targetScale, scaleDuration).SetEase(scaleEase));

            seq.AppendInterval(tapToContinueActivateDelay);

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