using System;
using Infrastructure.BroTweens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Character
{
    public class ProgressSceneView : MonoBehaviour
    {
        [Serializable]
        private class TextAnimationParams
        {
            public float scaleDuration;
            public AnimationCurve scaleCurve;
        }

        [Serializable]
        private class SliderAnimationParams
        {
            public float maxValueToNormalize = 0.88f;
            public float moveDuration = 0.15f;
            public AnimationCurve moveCurve;
        }

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private RectTransform progressTextRect;
        [SerializeField] private TextAnimationParams textParams;

        [Header("Slider")]
        [SerializeField] private Image progressSlider;
        [SerializeField] private SliderAnimationParams sliderParams;

        private SliderImageMoveTween sliderTween;
        private BroTweenSafe textTween;
        

        public void Initialize()
        {
            sliderTween = BroTween.SliderImageMove()                          
                                  .SetEase(sliderParams.moveCurve)
                                  .SetAutoKill(false);
            
            textTween = BroTween.ScaleByCurve(progressTextRect, progressTextRect.localScale, textParams.scaleDuration, textParams.scaleCurve)
                                .SetAutoKill(false)
                                .ToSafe();
            
        }


        public void Deinitialize()
        {
            BroTween.Kill(ref sliderTween);
            textTween.Kill();
        }


        public void SetText(string value)
        {
            progressText.text = value;
        }


        public void AnimateText()
        {
            if(textTween.TryRewind())
                textTween.Play();
        }


        public void SetProgressSlider(float progress01, bool smooth)
        {
            float to = Mathf.Lerp(0, sliderParams.maxValueToNormalize, progress01);

            if (smooth)
            {
                sliderTween.SetParams(progressSlider, progressSlider.fillAmount, to, sliderParams.moveDuration)
                           .Play();
            }
            else
            {
                progressSlider.fillAmount = to;
            }
        }


#if UNITY_EDITOR
        [TriInspector.Button, TriInspector.ShowInPlayMode]
        private void TestBounceScale()
        {
            AnimateText();
        }
#endif
    }
}