using System.Collections.Generic;
using System.Threading;
using Infrastructure.Popups;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using Infrastructure.Localization;
using Features.PurchaseUi;
using Cysharp.Threading.Tasks;
using Infrastructure.BroTweens;
using Infrastructure.Reward;

namespace Features.Tutorial
{
    public class ItemTutorialPopup : PopupBase
    {
        [SerializeField] private Button collectRewardButton;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI subHeaderText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI tapToContinueText;

        //[SerializeField][Range(0, 2)] private float splineIconsScale = 1;
        [SerializeField][Range(0, 1)] private float partOffset;
        
        [SerializeField] private PurchaseRewardItemView itemView;

        [Header("Reward Animation")]
        [SerializeField][Range(0, 3)] private float startDelay = 2f;
        [SerializeField][Range(0, 3)] private float betweenAnimationLength = 0.25f;
        [SerializeField][Range(0, 3)] private float itemAnimationLength = 0.5f;
        [SerializeField] private AnimationCurve appearAnimationCurve = new();
        private List<(Transform viewTransform, Vector3 targetScale, float startTime)> animatedViews 
            = new List<(Transform viewTransform, Vector3 targetScale, float startTime)>();

        private Vector3 headerTargetScale      = Vector3.zero;
        private Vector3 subHeaderTargetScale   = Vector3.zero;
        private Vector3 descriptionTargetScale = Vector3.zero;
        private Vector3 itemTargetScale        = Vector3.zero;

        private float timeLineCounter = 0;

        private CancellationTokenSource cts;
        private Action onTapCallback;
        private BroTweenSafe animatedViewTween;

        public void Construct(Action onTapCallback)
        {
            this.cts = new CancellationTokenSource();
            this.onTapCallback = onTapCallback;            
        }

        public override void Open()
        {
            for (int i = 0; i < animatedViews.Count; i++)
                animatedViews[i].viewTransform.localScale = Vector3.zero;

            base.Open();       
            DelayedAnimationStart(startDelay).Forget();
        }

        private async UniTaskVoid DelayedAnimationStart(float delay)
        {
            collectRewardButton.enabled = false;
            tapToContinueText.gameObject.SetActive(false);            
            await UniTask.WaitForSeconds(delay, true);
            ShowAnimation();
        }

        public void SetData(string header, string subHeader, string description, RewardItemVisualData itemData)
        {
            headerText.text = header;
            subHeaderText.text = subHeader;
            descriptionText.text = description;
            tapToContinueText.text = LocalizationService.I.Get(LocKeys.ItemTutorialPopup.TapButton);

            itemView.gameObject.SetActive(itemData != null);
            if (itemData != null)
                itemView.Construct(itemData);

            animatedViews.Clear();
            //timeLineCounter += betweenAnimationLength;

            animatedViews.Add((headerText.transform, headerTargetScale, timeLineCounter));
            timeLineCounter += betweenAnimationLength;

            animatedViews.Add((subHeaderText.transform, subHeaderTargetScale, timeLineCounter));
            timeLineCounter += betweenAnimationLength;

            animatedViews.Add((descriptionText.transform, descriptionTargetScale, timeLineCounter));
            timeLineCounter += betweenAnimationLength;

            if (itemData != null)
            {
                animatedViews.Add((itemView.transform, itemTargetScale, timeLineCounter));
                timeLineCounter += betweenAnimationLength;
            }               
        }

        protected override void OnInitialize()
        {
            headerTargetScale      = headerTargetScale      == Vector3.zero ? headerText.transform.localScale      : headerTargetScale;
            subHeaderTargetScale   = subHeaderTargetScale   == Vector3.zero ? subHeaderText.transform.localScale   : subHeaderTargetScale;
            descriptionTargetScale = descriptionTargetScale == Vector3.zero ? descriptionText.transform.localScale : descriptionTargetScale;
            itemTargetScale        = itemTargetScale        == Vector3.zero ? itemView.transform.localScale        : itemTargetScale;
            collectRewardButton.onClick.AddListener(TapHandler);
        }

        protected override void OnDeinitialize() 
        {
            collectRewardButton.onClick.RemoveListener(TapHandler);
        }

        private void TapHandler()
        {
            if (animatedViewTween.IsPlaying())
                animatedViewTween.Complete(true);
            animatedViewTween.Kill();

            headerText.transform.localScale      = headerTargetScale;
            subHeaderText.transform.localScale   = subHeaderTargetScale;
            descriptionText.transform.localScale = descriptionTargetScale;
            itemView.transform.localScale        = itemTargetScale;

            onTapCallback?.Invoke(); 
        }

        private void ShowAnimation()
        {
            animatedViewTween.Kill();
            BroSequence seq = BroTween.Sequence()
                                      .SetUpdate(true);
            for (int i = 0; i < animatedViews.Count; i++)
                seq.Insert(animatedViews[i].startTime, BroTween.Scale(animatedViews[i].viewTransform, animatedViews[i].targetScale, itemAnimationLength).SetEase(appearAnimationCurve));
            seq.OnComplete(() => {
                tapToContinueText.gameObject.SetActive(true);
                collectRewardButton.enabled = true;
            });                     
            animatedViewTween = seq.ToSafe();
            animatedViewTween.Play();
        }
    }
}
