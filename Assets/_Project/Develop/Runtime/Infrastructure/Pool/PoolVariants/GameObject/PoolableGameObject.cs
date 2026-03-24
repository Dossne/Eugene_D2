using System;
using UnityEngine;

namespace Infrastructure.Pool
{
    public class PoolableGameObject : MonoBehaviour, IPoolableObject<PoolableGameObject>
    {
        public event Action<PoolableGameObject> RequestReleaseToPool;
        public event Action<PoolableGameObject> ObserveDestroy;

        private GameObject thisGameObject;

        [TriInspector.ShowInInspector] public bool IsInPool { get; set; }
        [TriInspector.ShowInInspector] public int PooledId { get; set; }

        private void OnDisable()
        {
            ReleaseToPool();
        }

        private void OnDestroy()
        {
            ObserveDestroy?.Invoke(this);
            ObserveDestroy = null;
            RequestReleaseToPool = null;
        }

        void IPoolableObject<PoolableGameObject>.OnCreate()
        {
            thisGameObject = gameObject;
        }

        void IPoolableObject<PoolableGameObject>.OnPoolGet()
        {
            SetObjectActive(true);
        }

        void IPoolableObject<PoolableGameObject>.OnPoolRelease()
        {
            SetObjectActive(false);
        }

        void IPoolableObject<PoolableGameObject>.SetParent(Transform parent)
        {
            if(thisGameObject.transform.parent != parent)
                thisGameObject.transform.SetParent(parent);
        }

        void IPoolableObject<PoolableGameObject>.Destroy()
        {
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


#if UNITY_EDITOR
        
        [TriInspector.Button, TriInspector.ShowInPlayMode]
        public void ReleaseTest()
        {
            ReleaseToPool();
        }


        [TriInspector.Button, TriInspector.ShowInPlayMode]
        public void DestroyTest()
        {
            Destroy(gameObject);
        }
#endif
    }
}