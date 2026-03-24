using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    /// <summary>
    /// Reach each of input steps
    /// </summary>
    [Serializable]
    public abstract class VectorStepReachTween : BroTweenBase
    {
        [TriInspector.ShowInInspector] protected Transform target;
        [TriInspector.ShowInInspector] protected List<VectorStep> scheduledSteps = new();
        [TriInspector.ShowInInspector] protected Vector3 from;
        [TriInspector.ShowInInspector] protected Vector3 to;

        [TriInspector.ShowInInspector] private int stepIdx;
        [TriInspector.ShowInInspector] private float stepTime;
        [TriInspector.ShowInInspector] private float stepDuration;

        public override bool IsTimeComplete => stepIdx >= scheduledSteps.Count || stepIdx < 0;

        internal override bool TryReset_Internal()
        {
            if (scheduledSteps.Count == 0)
                return false;

            if (!base.TryReset_Internal())
                return false;

            Reset();

            return true;
        }

        /// <summary>
        /// Set precalculated target points for controlled shake
        /// </summary>
        protected void SetParams_Internal(Transform target, in float duration = -1)
        {
            float calcDuration = 0;
            
            if (duration < 0)
            {
                for (int i = 0; i < scheduledSteps.Count; i++)
                    calcDuration += scheduledSteps[i].duration;
            }
            else
            {
                calcDuration = duration;
            }

            SetDefaultParams(calcDuration);
            this.target = target;
            Reset();
        }

        /// <summary>
        /// Set precalculated target points for controlled shake
        /// </summary>
        protected void SetParams_Internal(Transform target, List<VectorStep> steps)
        {
            scheduledSteps.Clear();
            scheduledSteps.AddRange(steps);
            SetParams_Internal(target);
        }

        protected override void OnTick(in float dt)
        {
            stepTime += dt;
            float t = Mathf.Clamp01(stepTime / stepDuration);
            bool isBack = data.isPlayBackwards;

            if (isBack)
                t = 1f - t;

            var progress = data.ease == Ease.Custom ? easeCurve.Evaluate(t) : StandardEasing.Evaluate(t, data.ease);
            ApplyResult(progress);

            if (!isBack && t >= 1)
            {
                stepIdx++;
                InitCurrentData();
            }
            else if (isBack && t <= 0)
            {
                stepIdx--;
                InitCurrentData();
            }
        }

        protected override void OnClearParamsWhenReturnToPool()
        {
            target = null;
            scheduledSteps.Clear();
            from = default;
            to = default;
            stepIdx = 0;
            stepTime = 0;
            stepDuration = 0;
        }

        protected abstract void ApplyResult(float result);

        protected abstract Vector3 GetCurrentState();

        private void Reset()
        {
            if (!data.isPlayBackwards)
                stepIdx = 0;
            else
                stepIdx = scheduledSteps.Count - 1;

            InitCurrentData();
        }

        private void InitCurrentData()
        {
            if (IsTimeComplete)
                return;

            from = GetCurrentState();
            to = scheduledSteps[stepIdx].target;
            stepTime = 0;
            stepDuration = scheduledSteps[stepIdx].duration;
        }
    }
}