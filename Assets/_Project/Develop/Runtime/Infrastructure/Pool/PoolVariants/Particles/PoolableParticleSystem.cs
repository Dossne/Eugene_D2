using System;
using Infrastructure.Pool.Particles;
using UnityEngine;

namespace Infrastructure.Pool
{
    public sealed class PoolableParticleSystem : MonoBehaviour, IPoolableObject<PoolableParticleSystem>
    {
        public event Action<PoolableParticleSystem> RequestReleaseToPool;
        public event Action<PoolableParticleSystem> ObserveDestroy;

        [SerializeField] private ReusableParticleSystem reusableParticle;
        private GameObject thisGameObject;

        [TriInspector.ShowInInspector] public bool IsInPool { get; set; }
        [TriInspector.ShowInInspector] public int PooledId { get; set; }

        private void OnDestroy()
        {
            ObserveDestroy?.Invoke(this);
            ObserveDestroy = null;
            RequestReleaseToPool = null;
        }


        void IPoolableObject<PoolableParticleSystem>.OnCreate()
        {
            reusableParticle.Construct(ReleaseToPool);
            reusableParticle.Initialize();
            thisGameObject = gameObject;
        }


        void IPoolableObject<PoolableParticleSystem>.OnPoolRelease()
        {
            reusableParticle.Stop();
            SetObjectActive(false);
        }


        void IPoolableObject<PoolableParticleSystem>.OnPoolGet()
        {
            SetObjectActive(true);
            reusableParticle.Play();
        }


        void IPoolableObject<PoolableParticleSystem>.SetParent(Transform parent)
        {
            if (thisGameObject.transform.parent != parent)
                thisGameObject.transform.SetParent(parent);
        }


        void IPoolableObject<PoolableParticleSystem>.Destroy()
        {
            reusableParticle.Deinitialize();
            Destroy(gameObject);
        }


        private void ReleaseToPool()
        {
            RequestReleaseToPool?.Invoke(this);
        }


        private void SetObjectActive(bool isActive)
        {
            if (thisGameObject.activeSelf != isActive)
                thisGameObject.SetActive(isActive);
        }
    }
}