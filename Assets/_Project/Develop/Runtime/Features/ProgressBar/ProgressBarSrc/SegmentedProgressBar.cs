using System.Collections.Generic;
using Infrastructure.BroTweens;
using Infrastructure.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Features.ProgressBar
{
    public class SegmentedProgressBar : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SegmentBorder segmentBorderPf;
        [SerializeField] private Slider slider;
        [SerializeField] private RectTransform sliderRect;
        [SerializeField] private List<SegmentBorder> borders = new();

        [Header("Animation")]
        [SerializeField] private AnimationCurve sliderMoveCurve;
        [SerializeField] private float sliderMoveDuration;

        private int segmentMaxCount;
        private BroTweenSafe moveTween;

        public float SliderMoveDuration => sliderMoveDuration;


        public void Construct(int segmentMaxCount)
        {
            this.segmentMaxCount = segmentMaxCount;
        }


        public void Initialize()
        {
            RefreshBorders(segmentMaxCount);
        }


        public void SetSliderToSegment(int segmentIndex)
        {
            float betweenDistNorm = 1f / segmentMaxCount;
            float toValue = segmentIndex * betweenDistNorm;
            slider.value = toValue;
        }


        public void SliderAnimatedMove(int fromSegment, int toSegment)
        {
            float betweenDistNorm = 1f / segmentMaxCount;
            float fromValue = fromSegment * betweenDistNorm;
            float toValue = toSegment * betweenDistNorm;
            slider.value = fromValue;
            moveTween.Kill();
                
            moveTween = BroTween.SliderMove(slider, fromValue, toValue, sliderMoveDuration)
                                .SetEase(sliderMoveCurve)
                                .SetUpdate(true)
                                .ToSafe();
            moveTween.Play();
        }


        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }


        private void RefreshBorders(int segmentMaxCount)
        {
            int targetBordersCount = segmentMaxCount - 1;
            int instantiateCount = targetBordersCount - borders.Count;

            for (int i = 0; i < instantiateCount; i++)
            {
                var border = Instantiate(segmentBorderPf, sliderRect);
                border.SetObjectActive(false);
                borders.Add(border);
            }

            float width = sliderRect.rect.size.x;
            float betweenDistNorm = 1f / segmentMaxCount;

            for (var i = 0; i < borders.Count; i++)
            {
                if (i < targetBordersCount)
                {
                    float xPosNormalized = (i + 1) * betweenDistNorm;
                    Vector2 anchoredPos = new Vector2(width * (xPosNormalized - 0.5f), 0);
                    borders[i].SetAnchoredPosition(anchoredPos);
                    borders[i].SetObjectActive(true);
                }
                else
                {
                    borders[i].SetObjectActive(false);
                }
            }
        }


#if UNITY_EDITOR
        [TriInspector.Title("EDITOR ONLY")]
        [SerializeField] private int segmentsMaxCountEditor;
        [SerializeField] private int from_editor;
        [SerializeField] private int to_editor;

        [TriInspector.Button]
        public void SetSegments_Editor()
        {
            RefreshBorders(segmentsMaxCountEditor);
        }
        
        [TriInspector.Button]
        public void SliderAnimatedMove_Editor()
        {
            SliderAnimatedMove(from_editor, to_editor);
        }
#endif
    }
}