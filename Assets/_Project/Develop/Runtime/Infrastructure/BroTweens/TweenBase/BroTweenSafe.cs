using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    /// <summary>
    /// A Tween who isn't afraid of being killed!
    /// A safe variant of BroTweenBase/Sequence.
    /// Checks generation of the tween to avoid setting references to it in multiple places.
    /// </summary>
    [Serializable]
    public struct BroTweenSafe : IEquatable<BroTweenSafe>
    {
        [TriInspector.ShowInInspector] private BroTweenBase tween;
        [TriInspector.ShowInInspector] private int generation;


        public BroTweenSafe(BroTweenBase tween, int generation)
        {
            this.tween = tween;
            this.generation = generation;
        }


#region IEquatable

        public bool Equals(BroTweenBase other)
        {
            return tween != null
                && other != null
                && tween.UniqueId == other.UniqueId
                && generation == other.Generation;
        }


        public bool Equals(BroTweenSafe other)
        {
            return tween != null
                && other.tween != null
                && tween.UniqueId == other.tween.UniqueId
                && generation == other.generation;
        }


        public override bool Equals(object obj)
        {
            return obj is BroTweenSafe other && Equals(other);
        }


        public override int GetHashCode()
        {
            var tweenId = tween == null ? 0 : tween.UniqueId;
            return HashCode.Combine(tweenId, generation);
        }

#endregion

        /// <summary>
        /// Call after all tween setups.
        /// For Sequence should be called after all Appends and Inserts.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Play()
        {
            if (!IsValid())
            {
                Debug.LogWarning($"{BroTween.Tag}. Can't play invalid tween");
                return;
            }
            
            BroTweenService.I.TryPlay(tween);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPlay()
        {
            return TryRewind() && BroTweenService.I.TryPlay(tween);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRewind()
        {
            return IsValid() && tween.TryReset_Internal();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPlaying()
        {
            return IsValid() && tween.IsProcessing;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Kill(bool withCallback = false)
        {
            if (IsValid())
            {
/*#if UNITY_EDITOR
                Debug.Log($"{BroTween.Tag} SafeKill: {tween.UniqueId}. Type {tween.GetType().Name}. Gen: {generation}");
#endif*/
                tween.Kill_Internal(withCallback);
            }

            tween = null;
            generation = -1;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Complete(bool withCallback = false)
        {
            if (IsValid() && tween.IsProcessing)
            {
                tween.Complete(withCallback);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop()
        {
            if (IsValid() && tween.IsProcessing)
            {
                tween.Stop();
            }
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetUpdate(in bool isIndependentUpdate)
        {
            tween.SetUpdate_Internal(isIndependentUpdate);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAutoKill(in bool isAutoKill)
        {
            tween.SetAutoKill_Internal(in isAutoKill);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetEase(AnimationCurve ease)
        {
            tween.SetEase_Internal(ease);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetEase(in Ease ease)
        {
            tween.SetEase_Internal(in ease);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WithDelay(in float delay)
        {
            tween.WithDelay_Internal(delay);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPlayBackwards(in bool isPlayBackwards)
        {
            tween.SetPlayBackwards_Internal(isPlayBackwards);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLiveBetweenScenes(in bool isLiveBetweenScenes)
        {
            tween.SetLiveBetweenScenes_Internal(isLiveBetweenScenes);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValid()
        {
            return tween != null && generation == tween.Generation;
        }


        internal BroTweenBase GetReference_Internal()
        {
            return tween;
        }
    }
}