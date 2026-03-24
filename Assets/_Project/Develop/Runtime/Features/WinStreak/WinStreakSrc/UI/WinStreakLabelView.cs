using System;
using Infrastructure.BroTweens;
using Infrastructure.Utilities;
using UnityEngine;

namespace Features.WinStreak
{
    [Serializable]
    public class WinStreakLabelAnimation
    {
        [Header("Show")]
        public float winStreakShowDelay = 0.3f;
        public float winStreakShowScaleDuration = 0.5f;
        public AnimationCurve winStreakShowScaleCurve = AnimationCurve.Linear(0, 1, 1, 1);

        [Header("Move")]
        public float winStreakMoveDuration;
        public AnimationCurve winStreakMoveEaseCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve winStreakVerticalOffsetCurve = AnimationCurve.Linear(0, 0, 1, 0);
        public float winStreakOffsetVerticalMulti = 150f;

        [Header("Target bounce")]
        public float targetScaleDuration = 0.2f;
        public AnimationCurve targetScaleCurve = AnimationCurve.Linear(0, 1, 1, 1);
        
        [Header("Text")]
        public Color defaultColor;
        public Color winStreakColor;
    }

    public class WinStreakLabelView : MonoBehaviour
    {
        [SerializeField] private GameObject mainRoot;
        [SerializeField] private Transform animRoot;

        private AnimationCurve verticalOffsetCurve;
        private Vector3 fromPos;
        private Vector3 toPos;
        private float offsetVerticalMulti;

        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }

        public BroSequence GetMoveTween(WinStreakLabelAnimation animParams, Vector3 targetPosition)
        {
            animRoot.localPosition = Vector3.zero;

            this.fromPos = animRoot.position;
            this.toPos = targetPosition;
            verticalOffsetCurve = animParams.winStreakVerticalOffsetCurve;
            offsetVerticalMulti =  animParams.winStreakOffsetVerticalMulti;
            
            BroSequence seq = BroTween.Sequence();
            seq.AppendInterval(animParams.winStreakShowDelay);
            seq.AppendCallback(this, target => target.ActivateWinStreakLabel());

            if (animParams.winStreakShowScaleDuration > 0)
            {
                animRoot.localScale = Vector3.zero;
                seq.Append(GetShowScaleTween(animParams.winStreakShowScaleCurve, animParams.winStreakShowScaleDuration));
            }

            seq.Append(GetMoveTween(animParams.winStreakMoveDuration, animParams.winStreakMoveEaseCurve));
            seq.OnComplete(this, target => target.ResetParams());
            return seq;
        }

        private void ActivateWinStreakLabel()
        {
            SetObjectActive(true);
        }

        private BroTweenBase GetShowScaleTween(AnimationCurve curve, float duration)
        {
            return BroTween.ScaleByCurve(animRoot, Vector3.one, in duration, curve);
        }

        private BroTweenBase GetMoveTween(float duration, AnimationCurve easeCurve)
        {
            return BroTween.Float(this, (target, progress) => target.MoveByCurves(progress), duration)
                           .SetEase(easeCurve);
        }

        private void MoveByCurves(float progress)
        {
            Vector2 basePos = Vector2.LerpUnclamped(fromPos, toPos, progress);
            float yOffset = verticalOffsetCurve.Evaluate(progress);
            basePos.y += yOffset * offsetVerticalMulti;
            animRoot.position = basePos;
        }

        private void ResetParams()
        {
            verticalOffsetCurve = null;
            offsetVerticalMulti = 0;
            toPos = default;
        }
    }
}