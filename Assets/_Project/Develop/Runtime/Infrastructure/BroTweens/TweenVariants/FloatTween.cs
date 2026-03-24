using System;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    /// <summary>
    /// Similar to DOTween.To()
    /// </summary>
    [Serializable]
    public sealed class FloatTween : BroTweenBase
    {
        private Action<FloatTween> setterDelegate;
        private object targetCallback;

        //uncomment only when needed. Can be recursive
        //[TriInspector.ShowInInspector]
        private object target;

        [TriInspector.ShowInInspector] private float from;
        [TriInspector.ShowInInspector] private float to;
        [TriInspector.ShowInInspector] private float currentVal;

        /// <summary>
        /// Delegate allocations on each call. Cache instance of FloatTween if multiple usage at place
        /// </summary>
        /// <param name="setter"></param>
        public FloatTween SetSetter(in Action<float> setter)
        {
            this.targetCallback = setter;
            this.setterDelegate = tween =>
            {
                var callback = tween.targetCallback as Action<float>;
                callback?.Invoke(currentVal);
            };

            return this;
        }

        /// <summary>
        /// Zero allocation. Usage: "target: this, (target, value) => target.YourMethod(value)"
        /// </summary>
        public FloatTween SetSetter<T>(T target, in Action<T, float> setter) where T : class
        {
            if (target == null)
            {
                Debug.LogError($"{nameof(target)} is null or has been destroyed");
                return this;
            }

            this.target = target;
            this.targetCallback = setter;

            this.setterDelegate = tween =>
            {
                Action<T, float> cal = tween.targetCallback as Action<T, float>;
                T tar = tween.target as T;
                cal?.Invoke(tar, tween.currentVal);
            };
            return this;
        }

        public FloatTween SetParams(in Action<float> setter, in float duration)
        {
            return SetParams(in setter, 0, 1, in duration);
        }

        public FloatTween SetParams(in Action<float> setter, in float from, in float to, in float duration)
        {
            SetSetter(in setter);
            SetParams(in from, in to, in duration);
            
            return this;
        }

        public FloatTween SetParams<T>(T target, in Action<T, float> setter, in float duration) where T : class
        {
            return SetParams(target, in setter, 0, 1, in duration);
        }

        public FloatTween SetParams<T>(T target, in Action<T, float> setter, in float from, in float to, in float duration) where T : class
        {
            SetSetter(target, in setter);
            SetParams(in from, in to, in duration);
            return this;
        }

        public FloatTween SetParams(in float from, in float to, in float duration)
        {
            base.SetDefaultParams(duration);
            this.from = from;
            this.to = to;
            return this;
        }

        private FloatTween SetParams(in float duration)
        {
            base.SetDefaultParams(duration);
            this.from = 0;
            this.to = 1;
            return this;
        }

        protected override void OnTick(in float dt)
        {
            float t = GetProgress();
            currentVal = Mathf.LerpUnclamped(from, to, t);
            setterDelegate?.Invoke(this);
        }

        protected override void OnClearParamsWhenReturnToPool()
        {
            setterDelegate = null;
            targetCallback = null;
            target = null;
            from = 0;
            to = 0;
            currentVal = 0;
        }
    }
}