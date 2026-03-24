using System;
using System.Collections.Generic;
using Infrastructure.BroTweens;
using Infrastructure.Pool;
using Infrastructure.Pool.Particles;
using Infrastructure.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Competition
{
    [Serializable]
    public class CompetitionMultiplierAnimationParams
    {
        [Header("Cursor move")]
        public AnimationCurve moveEase;
        public float moveDurationMin;
        public float moveDurationMax;

        [Header("Text scale")]
        public AnimationCurve scaleCurve;
        public float scaleDuration;
    }

    public class CompetitionMultiplierView : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private MultiplierSegmentsRoot usualMultipliers;
        [SerializeField] private MultiplierSegmentsRoot maskMultipliers;
        [SerializeField] private RectMask2D mask;
        [SerializeField] private RectTransform cursor;
        [SerializeField] private float cursorCorrectiveWidth = 5f;

        [Header("Tooltip")]
        [SerializeField] private Button btn;
        [SerializeField] private MultiplierTooltip tooltip;

        [Header("Animations")]
        [SerializeField] private ReusableParticleSystemUI splashFx;
        [SerializeField] private ReusableParticleSystemUI glowFx;
        
        [SerializeField] private CompetitionMultiplierAnimationParams animParams;

        private BroTweenSafe cursorTween;
        private int currentIdx;
        private float itemWidth;
        private float maxWidth;

        private float fromPaddingL;
        private float toPaddingL;
        private float fromPaddingR;
        private float toPaddingR;

        private void OnDisable()
        {
            cursorTween.Kill();
        }

        public void Construct(List<string> currentTexts)
        {
            usualMultipliers.Construct(currentTexts);
            maskMultipliers.Construct(currentTexts);
        }

        public void Initialize()
        {
            usualMultipliers.Initialize();
            maskMultipliers.Initialize();
            tooltip.Initialize();
            itemWidth = maskMultipliers.SegmentWidth;
            maxWidth = maskMultipliers.MaxWidth;

            cursor.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemWidth + cursorCorrectiveWidth);
            btn.onClick.AddListener(ShowTooltip);
            splashFx.Initialize();
            glowFx.Initialize();
        }

        public void Deinitialize()
        {
            tooltip.Deinitialize();
            btn.onClick.RemoveListener(ShowTooltip);
        }

        public void SetCurrentIdx(int value)
        {
            currentIdx = value;
            SetCursorInstant(value);
        }

        public void SetCursorAnimated(int toIdx)
        {
            var targetPosition = maskMultipliers.GetPosition(toIdx);
            fromPaddingL = mask.padding.x;
            toPaddingL = toIdx * itemWidth;
            fromPaddingR = mask.padding.z;
            toPaddingR = maxWidth - (toIdx + 1) * itemWidth;

            var seq = BroTween.Sequence().SetUpdate(true);
            float moveDuration = GetDurationByMoveDistance(toIdx);

            seq.Append(BroTween.Position(cursor, targetPosition, moveDuration).SetEase(animParams.moveEase));
            seq.Insert(0, BroTween.Float(LerpMaskPaddings, moveDuration));
            seq.AppendCallback(() => PlayFxOnPosition(targetPosition));
            seq.Append(BroTween.ScaleByCurve(maskMultipliers.GetRectTransform(toIdx), animParams.scaleDuration, animParams.scaleCurve));
            cursorTween = seq.ToSafe();
            cursorTween.Play();
        }

        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }

        private void PlayFxOnPosition(Vector2 targetPosition)
        {
            splashFx.transform.position = targetPosition;
            splashFx.Play();
            
            glowFx.transform.position = targetPosition;
            glowFx.Restart();
        }

        private float GetDurationByMoveDistance(int moveIdx)
        {
            var moveCount = Mathf.Max(currentIdx, moveIdx) - Mathf.Min(currentIdx, moveIdx);
            var progress01 = Mathf.Clamp01(moveCount / (1f * (usualMultipliers.MaxCount - 1)));
            return Mathf.Lerp(animParams.moveDurationMin, animParams.moveDurationMax, progress01);
        }

        private void SetCursorInstant(int idx)
        {
            var pos = maskMultipliers.GetPosition(idx);
            cursor.position = pos;
            var leftPadding = idx * itemWidth;
            var rightPadding = maxWidth - (idx + 1) * itemWidth;
            mask.padding = new Vector4(leftPadding, 0, rightPadding, 0);
        }

        private void LerpMaskPaddings(float progress01)
        {
            var t = animParams.moveEase.Evaluate(progress01);
            var nextL = Mathf.LerpUnclamped(fromPaddingL, toPaddingL, t);
            var nextR = Mathf.LerpUnclamped(fromPaddingR, toPaddingR, t);
            mask.padding = new Vector4(nextL, 0, nextR, 0);
        }

        private void ShowTooltip()
        {
            tooltip.Show();
        }

#if UNITY_EDITOR
        [SerializeField] private int from;
        [SerializeField] private int to;

        [TriInspector.Button]
        private void AnimatedMoveCursor_Editor()
        {
            SetCurrentIdx(from);
            SetCursorAnimated(to);
        }
#endif
    }
}