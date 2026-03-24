using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.BroTweens;
using Infrastructure.Pool;
using Infrastructure.Reward;
using Infrastructure.Utilities;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Features.RewardTrack
{
    public class RewardTrackHudProgress : MonoBehaviour
    {
        public event Action OnPopupButtonClick;

        [Title("Common")]
        [SerializeField] private Slider slider;
        [SerializeField] private Button rewardStatePopupBtn;

        [SerializeField] private RectTransform mainRectTransform;
        [SerializeField] private RectTransform itemRectTransform;

        [Title("Target")]
        [SerializeField] private Image targetIcon;
        [SerializeField] private RectTransform startFlyPosition;
        [SerializeField] private RectTransform targetIconPoint;

        [Title("Progress")]
        [SerializeField] private TextMeshProUGUI progressTxt;
        [SerializeField] private Slider progressSlider;

        [Title("Reward")]
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private PurchaseRewardItemView rewardItemView;
        [SerializeField] private PurchaseRewardItemViewFloating rewardItemViewFloating;
        [SerializeField] private RectTransform rewardIconRoot;
        [SerializeField] private RectTransform rewardIconPoint;

        [Title("Timer")]
        [SerializeField] private GameObject timerGo;
        [SerializeField] private TextMeshProUGUI timeLeftTxt;

        [Title("Animation")]
        [SerializeField] private RewardTrackProgressHudAnimationParams animParams;

        private ParticlesPool particlesPool;
        private BroTweenSafe sliderSeq;
        private BroTweenSafe rewardIconSeq;

        public Vector3 StartFlyPosition => startFlyPosition.position;
        public Vector3 TargetIconPosition => targetIconPoint.position;
        public float RewardIconDurationTotal => animParams.rewardIconDurationTotal;

        public RectTransform MainRectTransform => mainRectTransform;
        public RectTransform ItemRectTransform => itemRectTransform;

        private Vector3 RewardIconPosition => rewardIconPoint.position;

        private bool isInit;


        public void Construct(ParticlesPool particlesPool)
        {
            this.particlesPool = particlesPool;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            rewardItemViewFloating.Initialize();
            rewardStatePopupBtn.onClick.AddListener(RewardPopupButtonClick);

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            rewardStatePopupBtn.onClick.RemoveAllListeners();
            rewardItemViewFloating.Deinitialize();
            rewardIconSeq.Kill();
            sliderSeq.Kill();

            isInit = false;
        }


        public void SetLocked(string lockText)
        {
            SwitchState(true);
            this.progressSlider.value = 0;
            progressTxt.text = lockText;
        }


        public void SetTarget(Sprite targetIcon, Sprite rewardIcon, string rewardCount, bool isDisplayRibbon = false, bool isDisplayInfinityIcon = false)
        {
            this.rewardItemView.Construct(rewardIcon, rewardCount, null, isDisplayRibbon, isDisplayInfinityIcon);
            this.targetIcon.sprite = targetIcon;
            SwitchState(false);
        }


        public void RefreshProgress(int current, int max)
        {
            this.progressSlider.value = current * 1f / max;
            RefreshText(current, max);
        }


        public void PlayFxOnTargetIcon()
        {
            if (particlesPool.TryGetItem(PoolableParticleType.RewardTrackHudProgress, out PoolableParticleSystem splashFx))
            {
                splashFx.transform.position = TargetIconPosition;
            }
        }


        public void ConstructFlyingIcon(Sprite icon, string labelText, bool isDisplayRibbon, bool isDisplayInfinityIcon)
        {
            this.rewardItemViewFloating.Construct(icon, labelText, isDisplayRibbon, isDisplayInfinityIcon);
        }


        public void PlayFxOnRewardIcon(Action onScaleDownCallback)
        {
            rewardItemViewFloating.StartPlay();

            if (particlesPool.TryGetItem(PoolableParticleType.RewardTrackHudProgress, out PoolableParticleSystem splashFx))
            {
                splashFx.transform.position = RewardIconPosition;
            }

            PlayDownUpScaleRewardIcon(onScaleDownCallback);
        }


        public UniTask SliderAnimatedMoveAsync(int from, int to, int max, CancellationToken token)
        {
            float fromValue = from * 1f / max;
            float toValue = to * 1f / max;
            ProgressSliderAnimationParams slParams = to < max ? animParams.sliderPartialMove : animParams.sliderFullMove;

            float duration = slParams.moveDuration;

            if (slParams.isDurationProportional)
            {
                duration = (toValue - fromValue) * slParams.moveDuration;

                if (duration < slParams.minDurationProportional)
                    duration = slParams.minDurationProportional;
            }

            slider.value = fromValue;
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


        private void SetSliderValue(float value)
        {
            progressSlider.value = value;
        }
        
        public void RefreshTime(string timeText)
        {
            timeLeftTxt.text = timeText;
        }


        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }


        public void SetButtonEnabled(bool value)
        {
            rewardStatePopupBtn.enabled = value;
        }


        private void PlayDownUpScaleRewardIcon(Action callback)
        {
            rewardIconSeq.Kill();
            rewardItemView.transform.localScale = Vector3.one;
            rewardIconSeq = BroTween.Sequence()
                                    .Insert(0, GeRewardIconRootScaleTween(animParams.rewardIconScaleDownCurve, animParams.rewardIconScaleDownDuration))
                                    .AppendCallback(callback)
                                    .Append(GeRewardIconRootScaleTween(animParams.rewardIconScaleUpCurve, animParams.rewardIconScaleUpDuration))
                                    .SetUpdate(true)
                                    .ToSafe();
            rewardIconSeq.Play();
        }


        private BroTweenBase GeRewardIconRootScaleTween(AnimationCurve curve, float duration)
        {
            return BroTween.ScaleByCurve(rewardIconRoot, duration, curve);
        }


        private void SwitchState(bool isLocked)
        {
            lockIcon.SetObjectActive(isLocked);
            this.rewardItemView.SetObjectActive(!isLocked);
            timerGo.SetObjectActive(!isLocked);
        }


        private void RefreshText(int current, int max)
        {
            this.progressTxt.text = $"{current.ToString()}/{max.ToString()}";
        }


        private void RewardPopupButtonClick()
        {
            OnPopupButtonClick?.Invoke();
        }
    }
}