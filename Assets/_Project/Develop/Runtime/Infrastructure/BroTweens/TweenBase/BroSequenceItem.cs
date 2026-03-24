using System;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public sealed class BroSequenceItem : IBroPoolable
    {
        public BroTweenBase tween;
        private IBroPool ownerPool;

        [TriInspector.ShowInInspector, TriInspector.ReadOnly] public int UniqueId { get; private set; }

        public float startTime;

        [TriInspector.ShowInInspector] private bool isInPool;


        public void SetParams(BroTweenBase tween, in float startTime)
        {
            this.tween = tween;
            this.startTime = startTime;
        }


        void IBroPoolable.Construct(IBroPool owner)
        {
            UniqueId = BroTweenId.GetNext();
            this.ownerPool = owner;
        }

        void IBroPoolable.OnReturnToPool()
        {
            tween = null;
            startTime = 0;
            isInPool = true;
        }


        void IBroPoolable.OnGet()
        {
            isInPool = false;
        }


        internal void Release()
        {
            if (isInPool)
                return;

            tween.ReleaseToPool_Internal();
            ownerPool?.Release(this);

/*#if UNITY_EDITOR
            UnityEngine.Debug.Log($"[BroTween] Released: {UniqueId}. Type {GetType().Name}");
#endif*/
        }
    }
}