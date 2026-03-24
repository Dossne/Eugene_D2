using System;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public sealed class BroSequenceLoop : BroSequence
    {
        [TriInspector.ShowInInspector] private int repeatCount;
        [TriInspector.ShowInInspector] private int currentRepeatCount;
        [TriInspector.ShowInInspector] private float betweenDelay;
        [TriInspector.ShowInInspector] private float currentBetweenDelay;
        [TriInspector.ShowInInspector] private bool isLoopProcess;
        [TriInspector.ShowInInspector] private LoopType loopType;

        public override bool IsTimeComplete => repeatCount >= 0 && currentRepeatCount >= repeatCount;

        ///Must be called before first Play
        public BroSequenceLoop SetParams(in int repeatCount, in LoopType loopType = LoopType.Restart)
        {
            SetParams(0, in repeatCount, in loopType);
            return this;
        }

        ///Must be called before first Play
        /// <param name="betweenDelay">cooldown between loops, not start delay</param>
        /// <param name="repeatCount">count times to repeat. Negative value is infinite times</param>
        public BroSequenceLoop SetParams(in float betweenDelay, in int repeatCount, in LoopType loopType = LoopType.Restart)
        {
            this.betweenDelay = betweenDelay;
            this.repeatCount = repeatCount;
            this.loopType = loopType;

            if (this.repeatCount == 0)
            {
                UnityEngine.Debug.LogWarning("[BroTween]. Set repeatCount > 0");
            }

            return this;
        }

        protected override void OnTick(in float dt)
        {
            if (isLoopProcess)
            {
                currentBetweenDelay += dt;
                ProcessLoop();
                return;
            }

            base.OnTick(dt);

            if (base.IsTimeComplete)
                isLoopProcess = true;
        }

        internal override bool TryReset_Internal()
        {
            currentRepeatCount = repeatCount;
            currentBetweenDelay = 0;
            isLoopProcess = false;
            bool isReset = base.TryReset_Internal();

            if (!IsTimeComplete)
                SetStatePlaying();

            return isReset;
        }

        protected override void OnClearParamsWhenReturnToPool()
        {
            repeatCount = 0;
            currentRepeatCount = 0;
            betweenDelay = 0;
            currentBetweenDelay = 0;
            isLoopProcess = false;
            base.OnClearParamsWhenReturnToPool();
        }

        private void ProcessLoop()
        {
            if (currentBetweenDelay < betweenDelay)
                return;

            currentBetweenDelay = 0;

            if (repeatCount >= 0)
                currentRepeatCount++;

            if (IsTimeComplete)
            {
/*#if UNITY_EDITOR
                UnityEngine.Debug.Log($"[BroTween] Loop end. Break loop: {UniqueId}");
#endif*/
                return;
            }

            if (loopType is LoopType.Yoyo)
                RevertSequence();

            if (TryReset_Internal())
            {
                isLoopProcess = false;
                return;
            }

/*#if UNITY_EDITOR
            UnityEngine.Debug.Log($"[BroTween] Something went wrong. Break loop: {UniqueId}");
#endif*/
            repeatCount = 0;
            currentRepeatCount = 0;
        }

        private void RevertSequence()
        {
            for (int i = 0; i < items.length; i++)
            {
                items[i].tween.SwitchPlayBackwards_Internal();
            }

            SwitchPlayBackwards_Internal();
        }
    }
}