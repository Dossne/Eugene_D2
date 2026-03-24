using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public abstract class BroTweenBase : IBroPoolable
    {

#region Reference fields

        //8-byte reference fields
        protected AnimationCurve easeCurve;
        private Action onCompleteCallbackUniTask; //for UniTask support, should be called before release
        private Action<BroTweenBase> onComplete;
        private IBroPool ownerPool;
        private object onCompleteCallback;
        /*[TriInspector.ShowInInspector] */
        private object callbackRequester; //uncomment only when needed. Can be recursive

#endregion

#region Value fields

        //packed struct. Set value data here
        [TriInspector.ShowInInspector] protected BroTweenData data;

#endregion

#region Properties

        public int UniqueId => data.uniqueId;
        public int Generation => data.generation;
        public int PlayVersion => data.playVersion;

        public virtual bool IsTimeComplete => data.currentTime >= data.duration + data.delay;
        public bool IsCompleted => data.state is BroTweenState.Completed;
        internal float Duration => data.duration;
        internal float CurrentTime => data.currentTime;
        internal float Delay => data.delay;
        internal bool IsLiveBetweenScenes => data.isLiveBetweenScenes;
        internal bool IsUnscaledDt => data.isUnscaledDt;
        internal bool IsProcessing => data.isProcessing;
        internal bool IsInPool => data.isInPool;

#endregion

#region Public Methods

        public virtual void Complete(bool withCallback = false)
        {
            Complete_Internal(withCallback, true);
        }


        public virtual void Stop()
        {
            if (data.isInPool || IsCompleted)
                return;
            
            StopProcessing();
            
            if (data.isKillOnComplete)
                ReleaseToPool_Internal();
        }
        
        
        public BroTweenSafe ToSafe()
        {
            return new BroTweenSafe(this, data.generation);
        }

#endregion

#region Internal Methods

        void IBroPoolable.Construct(IBroPool owner)
        {
            data.isKillOnComplete = true;
            data.uniqueId = BroTweenId.GetNext();
            ownerPool = owner;
        }


        void IBroPoolable.OnGet()
        {
            //Debug.Log($"[{BroTween.Tag}]MOVED FROM POOL: {UniqueId}. Gen: {data.generation}");

            SetStateReadyToPlay();
            data.ease = Ease.Default;
            easeCurve = null;
            data.isInPool = false;
        }


        void IBroPoolable.OnReturnToPool()
        {
            //Debug.Log($"[{BroTween.Tag}] RETURNED TO POOL: {UniqueId}. Gen: {data.generation}");
            easeCurve = null;

            data.duration = 0;
            data.currentTime = 0;
            data.delay = 0;

            data.ease = Ease.Default;
            data.isLiveBetweenScenes = false;
            data.state = BroTweenState.None;
            data.isPlayBackwards = false;
            data.isUnscaledDt = false;
            data.isKillOnComplete = true;
            data.isInPool = true;
            onComplete = null;
            onCompleteCallbackUniTask = null;
            onCompleteCallback = null;
            callbackRequester = null;
            data.generation++;
            data.playVersion = 0;

            OnClearParamsWhenReturnToPool();
        }


        internal void Tick_Internal(in float dt)
        {
            data.currentTime += dt;

            if (data.currentTime < data.delay)
                return;

            OnTick(dt);
        }


        internal virtual bool TryReset_Internal()
        {
            if (data.isInPool)
                return false;

            data.currentTime = 0;
            SetStateReadyToPlay();
            return true;
        }


        internal virtual bool TrySetProcessing_Internal()
        {
            if (data.isProcessing)
                return false;

            SetStatePlaying();
            data.playVersion++;
            data.isProcessing = true;
            return true;
        }


        internal void Kill_Internal(bool withCallback = false)
        {
            if (data.isInPool)
                return;

            Complete_Internal(withCallback, false);
            onCompleteCallbackUniTask?.Invoke(); //fix kill on complete with callback
            ReleaseToPool_Internal();
        }


        public void Complete_Internal(bool withCallback, bool forceLastTick)
        {
            if (data.isInPool || IsCompleted)
                return;

            data.currentTime = data.duration;

            if (forceLastTick)
                OnTick(0);

            SetStateComplete();
            StopProcessing();

            if (withCallback)
                onComplete?.Invoke(this);

            onCompleteCallbackUniTask?.Invoke();

            if (data.isKillOnComplete)
                ReleaseToPool_Internal();
        }


        internal void ReleaseToPool_Internal()
        {
            if (data.isInPool)
                return;

            StopProcessing();
            ownerPool?.Release(this);
/*#if UNITY_EDITOR
            Debug.Log($"{BroTween.Tag} Released: {UniqueId}. Type {GetType().Name}");
#endif*/
        }


        /// <summary>
        /// If TRUE tween will not be released to owner pool on complete.
        /// Reason for TRUE - sometimes SetParam allocates each call, this can kill performance, try to reuse same tween in such cases
        ///</summary>
        internal BroTweenBase SetAutoKill_Internal(in bool value)
        {
            data.isKillOnComplete = value;
            return this;
        }


        internal BroTweenBase SetPlayBackwards_Internal(in bool isPlayBackwards)
        {
            data.isPlayBackwards = isPlayBackwards;
            return this;
        }


        /// <summary>
        /// if true item not affected by BroTweenService.ForceCompleteAll between scenes
        /// </summary>
        internal BroTweenBase SetLiveBetweenScenes_Internal(in bool isLiveBetweenScenes)
        {
            data.isLiveBetweenScenes = isLiveBetweenScenes;
            return this;
        }


        internal void SwitchPlayBackwards_Internal()
        {
            data.isPlayBackwards = !data.isPlayBackwards;
        }


        internal BroTweenBase WithDelay_Internal(in float delay)
        {
            this.data.delay = delay;
            return this;
        }


        internal BroTweenBase SetUpdate_Internal(in bool isUnscaledDeltaTime)
        {
            this.data.isUnscaledDt = isUnscaledDeltaTime;
            return this;
        }


        internal BroTweenBase SetEase_Internal(AnimationCurve curve)
        {
            this.easeCurve = curve;
            data.ease = Ease.Custom;
            return this;
        }


        internal BroTweenBase SetEase_Internal(in Ease ease)
        {
            this.easeCurve = null;
            data.ease = ease;
            return this;
        }


        internal BroTweenBase OnComplete_Internal(in Action onComplete)
        {
            this.onCompleteCallback = onComplete;
            this.onComplete = tween =>
            {
                var callback = tween.onCompleteCallback as Action;
                callback?.Invoke();
            };

            return this;
        }


        /// <summary>
        /// Zero allocation. Usage: "target: this, target => target.YourMethod()"
        /// </summary>
        /// <param name="target">Callback owner, which contains onComplete method </param>
        /// <param name="onComplete">Method</param>
        internal BroTweenBase OnComplete_Internal<T>(T target, in Action<T> onComplete) where T : class
        {
            if (target == null)
            {
                Debug.LogError($"{nameof(target)} is null or has been destroyed");
                return this;
            }

            callbackRequester = target;
            this.onCompleteCallback = onComplete;
            this.onComplete = tween =>
            {
                Action<T> cal = tween.onCompleteCallback as Action<T>;
                T tar = tween.callbackRequester as T;
                cal?.Invoke(tar);
            };

            return this;
        }


        internal void SetToUniTask_Internal(Action onCompleteCallback)
        {
            onCompleteCallbackUniTask = onCompleteCallback;
        }
        
        
        protected void SetDefaultParams(in float duration)
        {
            SetDuration(in duration);
            this.data.currentTime = 0;
            SetStateReadyToPlay();
        }


        protected void SetDuration(in float duration)
        {
            this.data.duration = duration > 0 ? duration : 0.001f;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float GetProgress()
        {
            float t = Mathf.Clamp01(data.currentTime / data.duration);

            if (data.isPlayBackwards)
                t = 1f - t;

            return data.ease == Ease.Custom ? easeCurve.Evaluate(t) : StandardEasing.Evaluate(t, data.ease);
        }


        protected void SetStatePlaying()
        {
            data.state = BroTweenState.Playing;
        }


        protected abstract void OnTick(in float dt);

        protected abstract void OnClearParamsWhenReturnToPool();


        private void StopProcessing()
        {
            data.isProcessing = false;
        }


        private void SetStateReadyToPlay()
        {
            data.state = BroTweenState.ReadyToPlay;
        }


        private void SetStateComplete()
        {
            data.state = BroTweenState.Completed;
        }

#endregion

    }
}