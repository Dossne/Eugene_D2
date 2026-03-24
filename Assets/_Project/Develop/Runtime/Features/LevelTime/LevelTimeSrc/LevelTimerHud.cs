using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Effects;
using Infrastructure.BroTweens;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.LevelTime
{
    public class LevelTimerHud : MonoBehaviour
    {
        [SerializeField] private RectTransform mainRoot;
        [SerializeField] private RectTransform iconTransform;
        [SerializeField] private TextMeshProUGUI timeTxt;
        [SerializeField] private Slider slider;
        [SerializeField] private Image sliderImage;
        [SerializeField] private RectTransform flyTargetTransform;
        [SerializeField] private CanvasGroup freeezeCG;

        private Color currentColor;
        private BroTweenSafe textColorTween;
        private BroTweenSafe sliderColorTween;
        private BroTweenSafe scaleTween;
        private BroTweenSafe textTween;
        private BroTweenSafe freezeTween;
        private BroTweenSafe shakeTween;

        public RectTransform IconTransform => iconTransform;
        public RectTransform FlyTargetTransform => flyTargetTransform;


        public void Construct(float maxTime, string timeText, Color textDefColor, Color sliderDefColor)
        {
            SetMaxTime(maxTime);
            SetTimeText(timeText);
            timeTxt.color = textDefColor;
            sliderImage.color = sliderDefColor;
        }


        public void Initialize()
        {
            SetObjectActive(true);
        }


        public void Deinitialize()
        {
            SetObjectActive(false);
            StopAnimations();
        }


        public void ResetFx(Color textDefColor, Color sliderDefColor)
        {
            StopAnimations();
            sliderImage.color = sliderDefColor;
            timeTxt.color = textDefColor;
        }


        public void SetTimeText(string value)
        {
            timeTxt.text = value;
        }


        public void SetMaxTime(float maxTime)
        {
            slider.maxValue = maxTime;
        }


        public void DoTextColorYoyo(AnimationCurve ease, Color defaultColor, Color lowColor, float duration)
        {
            textColorTween.Kill();
            textColorTween = BroTween.SequenceLoop(0, -1, LoopType.Yoyo)
                                     .Append(BroTween.ColorText(timeTxt, defaultColor, lowColor, duration).SetEase(ease))
                                     .ToSafe();
            textColorTween.Play();
        }


        public void DoSliderColor(AnimationCurve ease, Color to, float duration)
        {
            sliderColorTween.Kill();
            currentColor = sliderImage.color;
            sliderColorTween = BroTween.ColorImage(sliderImage, sliderImage.color, to, duration).SetEase(ease).ToSafe();
            sliderColorTween.Play();
        }


        public void ResetSliderColorToCurrent(AnimationCurve ease, float duration)
        {
            sliderColorTween.Kill();
            sliderColorTween = BroTween.ColorImage(sliderImage, sliderImage.color, currentColor, duration).SetEase(ease).ToSafe();
            sliderColorTween.Play();
        }


        public void SetSliderProgress(float value)
        {
            slider.value = value;
        }


        public async UniTask PlayAddTimeFxAsync(TimerTextBoostFxData fxData, Color defaultColor, int fromTime, int toTime,
            CancellationToken cancellationToken)
        {
            timeTxt.color = fxData.boostColor;
            scaleTween = BroTween.Scale(mainRoot, Vector3.one, Vector3.one * fxData.hudScale, fxData.scaleDurationSec)
                                 .SetEase(fxData.scaleCurve)
                                 .SetUpdate(true)
                                 .ToSafe();
            scaleTween.Play();
            await UniTask.WaitForSeconds(fxData.scaleDurationSec, ignoreTimeScale: true, cancellationToken: cancellationToken);

            textTween = BroTween.Int(SetTimeText, fromTime, toTime, fxData.textDurationSec)
                                .SetUpdate(true)
                                .ToSafe();
            textTween.Play();
            
            await UniTask.WaitForSeconds(fxData.textDurationSec, ignoreTimeScale: true, cancellationToken: cancellationToken);

            scaleTween = BroTween.Scale(mainRoot, Vector3.one * fxData.hudScale, Vector3.one, fxData.scaleDurationSec)
                                 .SetEase(fxData.scaleCurve)
                                 .SetUpdate(true)
                                 .ToSafe();
            scaleTween.Play();
            
            await UniTask.WaitForSeconds(fxData.scaleDurationSec, ignoreTimeScale: true, cancellationToken: cancellationToken);

            timeTxt.color = defaultColor;
        }


        public void PlayFreezeFx(AnimationCurve curve, float fromAlpha, float toAlpha, float duration)
        {
            freezeTween.Kill();

            if (toAlpha > 0)
                freeezeCG.gameObject.SetObjectActive(true);

            freeezeCG.alpha = fromAlpha;
            freezeTween = BroTween.FadeCanvasGroup(freeezeCG, fromAlpha, toAlpha, duration)
                                  .SetEase(curve)
                                  .SetUpdate(false)
                                  .OnComplete(() =>
                                   {
                                       if (toAlpha <= 0)
                                           freeezeCG.gameObject.SetObjectActive(false);
                                   })
                                  .ToSafe();
            freezeTween.Play();
        }


        public void RemoveFreezeFxInstant()
        {
            freezeTween.Kill();
            freeezeCG.gameObject.SetActive(false);
            sliderImage.color = currentColor;
        }


        public void PlayShake(float duration, float strength, int vibrato)
        {
            shakeTween.Kill();
            shakeTween = BroTween.ShakePosition(mainRoot, new Vector3(strength, strength, 0.0f), duration, true, vibrato)
                                 .SetUpdate(false)
                                 .ToSafe();
            shakeTween.Play();
        }


        private void SetTimeText(int time)
        {
            SetTimeText(TimeUtils.GetTimeString(time, trimFirstZero: true));
        }


        private void StopAnimations()
        {
            textColorTween.Kill();
            sliderColorTween.Kill();
            scaleTween.Kill();
            textTween.Kill();
            freezeTween.Kill();
            shakeTween.Kill();
        }


        private void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }
    }
}