using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.WinStreak;
using Febucci.UI;
using Infrastructure.Ads;
using Infrastructure.AudioControl;
using Infrastructure.BroTweens;
using Infrastructure.Localization;
using Infrastructure.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Features.LevelComplete
{
    public class LevelCompletePopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI header;
        [SerializeField] private List<string> headerKeys = new();
        [SerializeField] private TextAnimator_TMP headerTextAnimator;
        [SerializeField] private TypewriterByCharacter typewriter;

        [SerializeField] private RectTransform imageRect;
        [SerializeField] private float imageAppearDelay;
        [SerializeField] private float imageAppearDuration;
        [SerializeField] private AnimationCurve imageAppearCurve;

        [SerializeField] private RectTransform agreeButtonRect;
        [SerializeField] private float agreeButtonAppearDelay;
        [SerializeField] private RectTransform rewardAdsButtonRect;
        [SerializeField] private float rewardAdsButtonAppearDelay;

        [SerializeField] private float buttonAppearDuration;
        [SerializeField] private AnimationCurve buttonAppearCurve;

        [SerializeField] private Button agreeBtn;
        [SerializeField] private TextMeshProUGUI agreeBtnTxt;
        [SerializeField] private RewardAdsButton rewardAdsBtn;
        [SerializeField] private TextMeshProUGUI rewardAdsBtnTxt;
        [SerializeField] private TextMeshProUGUI timeTxt;
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI rewardedCoinsText;
        [SerializeField] private WinStreakUIBanner winStreakUIBanner;

        private AnalyticsContextCreator analyticsContextCreator;
        private Action nextCallback;
        private float startDelay = 0f;
        private BroSequence bounceSeq;
        private CancellationTokenSource cts;

        public WinStreakUIBanner WinStreakUIBanner => winStreakUIBanner;



        [Inject]
        public void Construct(AnalyticsContextCreator analyticsContextCreator)
        {
            this.analyticsContextCreator = analyticsContextCreator;
        }


        public void Construct(string coins, string coinsRewarded, string timeText, string rewardPlacement, string rewardAdsBtnTxt, Action rewardCallback, Action nextCallback)
        {
            this.timeTxt.text = timeText;
            this.rewardAdsBtnTxt.text = rewardAdsBtnTxt;

            rewardAdsBtn.Construct(rewardPlacement, this.name, rewardCallback);
            rewardAdsBtn.SetOnClickDelegate(SetButtonsNotInteractable);
            rewardAdsBtn.SetSkipDelegate(SetButtonsInteractable);

            coinsText.text = coins;
            rewardedCoinsText.text = coinsRewarded;
            this.nextCallback = nextCallback;
        }


        protected override void OnInitialize()
        {
            header.text = LocalizationService.I.Get(LocKeys.CompletePopup.Header);
            agreeBtnTxt.text = LocalizationService.I.Get(LocKeys.CompletePopup.Btn);
            agreeBtn.onClick.AddListener(OnClickButton);
            rewardAdsBtn.Initialize();
        }


        protected override void OnDeinitialize()
        {
            agreeBtn.onClick.RemoveListener(OnClickButton);
            rewardAdsBtn.Deinitialize();
        }


        public override void Open()
        {
            if (headerKeys.Count > 0)
                header.text = LocalizationService.I.Get(headerKeys[UnityEngine.Random.Range(0, headerKeys.Count)]);

            imageRect.localScale = Vector3.zero;
            agreeButtonRect.localScale = Vector3.zero;
            rewardAdsButtonRect.localScale = Vector3.zero;

            if (!bounceSeq.TryRewind())
            {
                bounceSeq = BroTween.Sequence().SetAutoKill(false).SetUpdate(true);
                bounceSeq.Insert(imageAppearDelay, BroTween.ScaleByCurve(imageRect, Vector3.one, imageAppearDuration, imageAppearCurve));
                bounceSeq.Insert(agreeButtonAppearDelay, BroTween.ScaleByCurve(agreeButtonRect, Vector3.one, buttonAppearDuration, buttonAppearCurve));
                bounceSeq.Insert(rewardAdsButtonAppearDelay, BroTween.ScaleByCurve(rewardAdsButtonRect, Vector3.one, buttonAppearDuration, buttonAppearCurve));
                bounceSeq.OnComplete(() =>
                {
                    imageRect.localScale = Vector3.one;
                    agreeButtonRect.localScale = Vector3.one;
                    rewardAdsButtonRect.localScale = Vector3.one;
                });
            }

            SetButtonsInteractable();

            base.Open();
            cts = new CancellationTokenSource();
            DelayedAnimationStart(startDelay, cts.Token).Forget();
        }


        public void InstantAnimationComplete()
        {
            if (typewriter.isShowingText)
                typewriter.SkipTypewriter();
            bounceSeq.Complete(true);
        }


        protected override void OnBeginClose()
        {
            cts?.Cancel();
            cts?.Dispose();
        }


        private async UniTaskVoid DelayedAnimationStart(float delay, CancellationToken cancellationToken)
        {
            try
            {
                headerTextAnimator.ResetState();
                await UniTask.WaitForSeconds(delay, true, cancellationToken: cancellationToken);
                await UniTask.WaitForEndOfFrame(this, cancellationToken: cancellationToken);
                typewriter.StartShowingText();                
                ShowBerryAnimation();
                AudioService.I.PlaySfx(SfxType.LevelCompletePopupOpen);
            }
            catch (Exception)
            {
                typewriter.StartShowingText();
                typewriter.SkipTypewriter();
                bounceSeq.Complete(true);
            }
        }


        private void ShowBerryAnimation()
        {
            bounceSeq.Play();
        }


        private void NextCallback()
        {
            nextCallback?.Invoke();
        }


        private void SetButtonsNotInteractable()
        {
            rewardAdsBtn.SetButtonsInteractable(false);
            agreeBtn.interactable = false;
        }


        private void SetButtonsInteractable()
        {
            rewardAdsBtn.SetButtonsInteractable(true);
            agreeBtn.interactable = true;
        }


        private void OnClickButton()
        {
            AnalyticSender.TrackClick(this.name, agreeBtn.name);

            SetButtonsNotInteractable();

            BroTween.ClickBounceWithCallBack(agreeBtn, agreeBtn.GetComponent<RectTransform>(), this, target => target.NextCallback(), callbackOnEnd: true)
                    .Play();
        }
    }
}