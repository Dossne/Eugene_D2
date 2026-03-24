using Infrastructure.BroTweens;
using UnityEngine;
using UnityEngine.UI;

namespace Features.ProgressBar
{
    public class NormalizedProgressBarImage : MonoBehaviour
    {
        [SerializeField] private Image progressSlider;
        [SerializeField] private NormalizedProgressBarImageParams barParams;

        private BroTweenSafe moveTween;


        private void OnDestroy()
        {
            moveTween.Kill();
        }


        public void SetProgress(float progress01, bool smooth = false)
        {
            progress01 = Mathf.Clamp01(progress01);

            float val = Mathf.Lerp(barParams.minValue, barParams.maxValue, progress01);

            if (smooth)
                SliderAnimatedMove(val);
            else
                progressSlider.fillAmount = val;
        }


        private void SliderAnimatedMove(float to)
        {
            if (progressSlider.fillAmount > to)
            {
                progressSlider.fillAmount = 0;
            }

            float from = progressSlider.fillAmount;
            moveTween.Kill();

            moveTween = BroTween.SliderImageMove(progressSlider, from, to, barParams.moveDuration)
                                .SetEase(barParams.moveCurve)
                                .SetUpdate(true)
                                .ToSafe();
            
            moveTween.Play();
        }


#if UNITY_EDITOR

        [TriInspector.Title("Debug")]
        [SerializeField] private bool isDebug;
        [SerializeField, TriInspector.ShowIf("isDebug")] private bool setOnValidate;
        [SerializeField, Range(0, 1), TriInspector.ShowIf("isDebug")] private float debugProgress;


        private void OnValidate()
        {
            if (!setOnValidate || progressSlider == null)
                return;
            
            SetProgress(debugProgress);
        }


        [TriInspector.Button]
        private void SetProgressSmooth_Editor()
        {
            SetProgress(debugProgress, true);

        }
#endif
    }
}