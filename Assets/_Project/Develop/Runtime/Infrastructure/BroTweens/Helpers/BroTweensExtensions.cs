using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    public static class BroTweensExtensions
    {

        /// <summary>
        /// Call after all tween setups.
        /// For Sequence should be called after all Appends and Inserts.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Play(this BroTweenBase tween)
        {
            BroTweenService.I.TryPlay(tween);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRewind(this BroTweenBase tween)
        {
            return tween != null && tween.TryReset_Internal();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPlaying(this BroTweenBase tween)
        {
            return tween != null && tween.IsProcessing;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SetUpdate<T>(this T tween, in bool isIndependentUpdate) where T : BroTweenBase
        {
            tween.SetUpdate_Internal(isIndependentUpdate);
            return tween;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SetAutoKill<T>(this T tween, in bool isAutoKill) where T : BroTweenBase
        {
            tween.SetAutoKill_Internal(in isAutoKill);
            return tween;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SetEase<T>(this T tween, AnimationCurve ease) where T : BroTweenBase
        {
            tween.SetEase_Internal(ease);
            return tween;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SetEase<T>(this T tween, in Ease ease) where T : BroTweenBase
        {
            tween.SetEase_Internal(in ease);
            return tween;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithDelay<T>(this T tween, in float delay) where T : BroTweenBase
        {
            tween.WithDelay_Internal(delay);
            return tween;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SetPlayBackwards<T>(this T tween, in bool isPlayBackwards) where T : BroTweenBase
        {
            tween.SetPlayBackwards_Internal(isPlayBackwards);
            return tween;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SetLiveBetweenScenes<T>(this T tween, in bool isLiveBetweenScenes) where T : BroTweenBase
        {
            tween.SetLiveBetweenScenes_Internal(isLiveBetweenScenes);
            return tween;
        }


        /// <summary>
        /// Zero allocation. Usage: "target: this, target => target.YourMethod()"
        /// </summary>
        /// <param name="target">Callback owner, which contains onComplete method </param>
        /// <param name="onComplete">Method</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T OnComplete<T, T1>(this T tween, T1 target, in Action<T1> onComplete) where T : BroTweenBase where T1 : class
        {
            tween.OnComplete_Internal(target, in onComplete);
            return tween;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T OnComplete<T>(this T tween, in Action onComplete) where T : BroTweenBase
        {
            tween.OnComplete_Internal(in onComplete);
            return tween;
        }
    }
}