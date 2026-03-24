using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.RewardTrack;
using Infrastructure.BroTweens;
using Infrastructure.Localization;
using Infrastructure.Pool.Particles;
using Infrastructure.Reward;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Competition
{
    public class CompetitionRewardTrackProgress : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private PurchaseRewardItemView rewardItemView;
        [SerializeField] private GameObject checkMark;
        [SerializeField] private TextMeshProUGUI progressTxt;
        [SerializeField] private RectTransform rewardIconRoot;
        [SerializeField] private CompetitionRewardTrackTooltip rewardTrackTooltip;
        [SerializeField] private Button rewardTrackSliderButton;
        [SerializeField] private ReusableParticleSystem splashFx;

        [TriInspector.Title("Animation")]
        [SerializeField] private RewardTrackProgressHudAnimationParams animParams;

        private BroTweenSafe sliderSeq;
        private BroTweenSafe rewardIconSeq;

        private bool isInit;

        public float RewardIconDurationTotal => animParams.rewardIconDurationTotal;

        private void OnDisable()
        {
            sliderSeq.Kill();
            rewardIconSeq.Kill();
        }

        public void Construct(CompetitionRewardController rewardController, SpriteAtlasService spriteAtlasService, List<CompetitionRewardTrackData> configData)
        {
            rewardTrackTooltip.Construct(rewardController, spriteAtlasService, configData);
        }

        public void Initialize()
        {
            if (isInit)
                return;

            rewardTrackSliderButton.onClick.AddListener(ShowRewardTrackTooltip);
            splashFx.Initialize();

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;
            splashFx.Deinitialize();
            rewardTrackTooltip.Deinitialize();
            rewardTrackSliderButton.onClick.RemoveListener(ShowRewardTrackTooltip);
            isInit = false;
        }

        public void SetRewardTarget(Sprite rewardIcon, string rewardCount, bool isDisplayRibbon, bool isDisplayInfinityIcon)
        {
            this.rewardItemView.Construct(rewardIcon, rewardCount, null, isDisplayRibbon, isDisplayInfinityIcon);
        }

        public void RefreshProgress(int current, int max)
        {
            this.progressSlider.value = current * 1f / max;
            RefreshText(current, max);
        }

        public void SetSliderToMax()
        {
            this.progressSlider.value = this.progressSlider.maxValue;
        }

        public void SwitchRewardTrackToCompleteState(bool isComplete)
        {
            rewardItemView.SetObjectActive(!isComplete);
            checkMark.SetObjectActive(isComplete);

            if (isComplete)
            {
                progressTxt.text = LocalizationService.I.Get(LocKeys.Competition.Complete);
            }
        }

        public UniTask RewardTrackSliderAnimatedMoveAsync(int from, int to, int max, CancellationToken token)
        {
            var fromValue = from * 1f / max;
            var toValue = to     * 1f / max;
            var slParams = to < max ? animParams.sliderPartialMove : animParams.sliderFullMove;
            var duration = slParams.moveDuration;

            if (slParams.isDurationProportional)
            {
                duration = (toValue - fromValue) * slParams.moveDuration;

                if (duration < slParams.minDurationProportional)
                    duration = slParams.minDurationProportional;
            }

            progressSlider.value = fromValue;
            sliderSeq.Kill();
            sliderSeq = BroTween.Sequence()
                                .Insert(0, BroTween.Float(SetSliderValue, fromValue, toValue, duration).SetEase(slParams.moveCurve))
                                .Insert(0, BroTween.Int(currentValue => RefreshText(currentValue, max), from, to, duration).SetEase(slParams.moveCurve))
                                .SetUpdate(true)
                                .OnComplete(() => RefreshProgress(to, max))
                                .ToSafe();

            sliderSeq.Play();

            return sliderSeq.ToUniTask(cancellationToken: token);
        }

        public void PlayDownUpScaleRewardIcon(Action callback)
        {
            rewardIconSeq.Kill();
            rewardItemView.transform.localScale = Vector3.one;
            rewardIconSeq = BroTween.Sequence()
                                    .InsertCallback(0, PlaySplashFx)
                                    .Insert(0, GetRewardIconRootScaleTween(animParams.rewardIconScaleDownCurve, animParams.rewardIconScaleDownDuration))
                                    .AppendCallback(callback)
                                    .Append(GetRewardIconRootScaleTween(animParams.rewardIconScaleUpCurve, animParams.rewardIconScaleUpDuration))
                                    .SetUpdate(true)
                                    .ToSafe();
            rewardIconSeq.Play();
        }

        private void PlaySplashFx()
        {
            splashFx.Play();
        }

        private void SetSliderValue(float value)
        {
            progressSlider.value = value;
        }

        private void RefreshText(int current, int max)
        {
            this.progressTxt.text = $"{current.ToString()}/{max.ToString()}";
        }

        private BroTweenBase GetRewardIconRootScaleTween(AnimationCurve curve, float duration)
        {
            return BroTween.ScaleByCurve(rewardIconRoot, duration, curve);
        }

        private void ShowRewardTrackTooltip()
        {
            rewardTrackTooltip.Initialize();
            rewardTrackTooltip.Show();
        }
    }
}