using System;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public sealed class IntTween : BroTweenBase
    {
        private Action<IntTween> setterDelegate;
        private object targetCallback;

        //uncomment only when needed. Can be recursive
        //[TriInspector.ShowInInspector]
        private object target;

        [TriInspector.ShowInInspector] private int from;
        [TriInspector.ShowInInspector] private int to;
        [TriInspector.ShowInInspector] private int currentVal = -1;

        /// <summary>
        /// Delegate allocations on each call. Cache instance of FloatTween if multiple usage at place
        /// </summary>
        /// <param name="setter"></param>
        public IntTween SetSetter(in Action<int> setter)
        {
            this.targetCallback = setter;
            this.setterDelegate = tween =>
            {
                var callback = tween.targetCallback as Action<int>;
                callback?.Invoke(currentVal);
            };

            return this;
        }

        /// <summary>
        /// Zero allocation. Usage: "target: this, (target, value) => target.YourMethod(value)"
        /// </summary>
        public IntTween SetSetter<T>(T target, in Action<T, int> onComplete) where T : class
        {
            if (target == null)
            {
                Debug.LogError($"{nameof(target)} is null or has been destroyed");
                return this;
            }

            this.target = target;
            this.targetCallback = onComplete;

            this.setterDelegate = tween =>
            {
                var cal = tween.targetCallback as Action<T, int>;
                T tar = tween.target as T;
                cal?.Invoke(tar, tween.currentVal);
            };

            return this;
        }

        public IntTween SetParams(in int from, in int to, in float duration)
        {
            base.SetDefaultParams(duration);
            this.from = from;
            this.to = to;
            currentVal = -1;
            return this;
        }

        public IntTween SetParams(in Action<int> setter, in int from, in int to, in float duration)
        {
            SetSetter(in setter);
            SetParams(in from, in to, in duration);
            return this;
        }

        public IntTween SetParams<T>(T target, in Action<T, int> setter, in int from, in int to, in float duration) where T : class
        {
            SetSetter(target, in setter);
            SetParams(from, to, duration);
            return this;
        }

        protected override void OnTick(in float dt)
        {
            float t = GetProgress();
            float currentValue = Mathf.Lerp(from, to, t);
            int intVal = Mathf.FloorToInt(currentValue);

            if (intVal == currentVal)
                return;

            currentVal = intVal;
            setterDelegate?.Invoke(this);
        }

        protected override void OnClearParamsWhenReturnToPool()
        {
            setterDelegate = null;
            targetCallback = null;
            target = null;
            from = 0;
            to = 0;
            currentVal = -1;
        }
    }
}