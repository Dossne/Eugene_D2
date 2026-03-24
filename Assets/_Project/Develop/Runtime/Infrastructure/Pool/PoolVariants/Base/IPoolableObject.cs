using System;
using UnityEngine;

namespace Infrastructure.Pool
{
    public interface IPoolableObject<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// Unique id in owner pool
        /// </summary>
        bool IsInPool { get; set; }
        
        /// <summary>
        /// Unique id in owner pool
        /// </summary>
        int PooledId { get; set; }

        /// <summary>
        /// Request place back to pool
        /// </summary>
        event Action<T> RequestReleaseToPool;

        /// <summary>
        /// Observe about destroy (for situations when item destroyed not by pool)
        /// </summary>
        event Action<T> ObserveDestroy;


        /// <summary>
        /// Actions called automatically after instantiate item
        /// </summary>
        void OnCreate();


        /// <summary>
        /// Actions called automatically after take from pool
        /// </summary>
        void OnPoolGet();


        /// <summary>
        /// Actions called automatically each time after invoke ReleaseToPool.
        /// </summary>
        void OnPoolRelease();


        /// <summary>
        /// Set parent of item
        /// </summary>
        void SetParent(Transform parent);


        /// <summary>
        /// Destroy item by pool
        /// </summary>
        void Destroy();
    }
}