using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.Pool
{
    [Serializable]
    public sealed class BasePool<T> where T : class
    {
        [Header("Debug")]
        [SerializeField] private List<T> activeElements;
        [SerializeField] private List<T> inactiveElements;

        private string poolKey;
        private Func<T> create;
        private Func<CancellationToken, UniTask<T>> createAsync;

        private readonly Action<T> get;
        private readonly Action<T> release;
        private readonly Action<T> destroy;

        private readonly int maxSize;
        private readonly int preWarmCount;


        public BasePool(
            string poolKey,
            Func<T> create,
            Func<CancellationToken, UniTask<T>> createAsync,
            Action<T> get = null,
            Action<T> release = null,
            Action<T> destroy = null,
            int preWarmCount = 10,
            int maxSize = 10000)
        {
            this.poolKey = poolKey;
            if (preWarmCount < 0)
            {
                throw new ArgumentException("Default capacity must be greater than 0", nameof(maxSize));
            }

            if (maxSize < 1)
            {
                throw new ArgumentException("Max size must be greater than 1", nameof(maxSize));
            }

            this.create = create ?? throw new ArgumentNullException(nameof(create));
            this.createAsync = createAsync ?? throw new ArgumentNullException(nameof(createAsync));
            this.get = get;
            this.release = release;
            this.destroy = destroy;
            this.preWarmCount = preWarmCount;
            this.maxSize = maxSize;

            inactiveElements = new List<T>(preWarmCount);
            activeElements = new List<T>(preWarmCount);
        }


        /// <summary>
        /// Prepare elements by default capacity
        /// </summary>
        public void PreWarm()
        {
            for (int i = 0; i < preWarmCount; i++)
            {
                T element = create();
                inactiveElements.Add(element);
            }
        }


        /// <summary>
        /// Prepare elements by default capacity
        /// </summary>
        public async UniTask PreWarmAsync(CancellationToken cancellationToken)
        {
            for (int i = 0; i < preWarmCount; i++)
            {
                T element = await createAsync(cancellationToken);
                inactiveElements.Add(element);
            }
        }


        /// <summary>
        /// Release all active elements back to pool
        /// </summary>
        public void ReleaseAll()
        {
            for (var i = activeElements.Count - 1; i >= 0; i--)
            {
                Release(activeElements[i]);
            }
        }


        /// <summary>
        /// Destroy all active and inactive elements, and clear pools
        /// </summary>
        public void Destroy()
        {
            if (destroy != null)
            {
                foreach (var element in inactiveElements)
                {
                    destroy?.Invoke(element);
                }

                foreach (var element in activeElements)
                {
                    destroy?.Invoke(element);
                }
            }

            inactiveElements.Clear();
            activeElements.Clear();
        }


        /// <summary>
        /// Get inactive element from pool
        /// </summary>
        public T Get()
        {
            T element;
            if (inactiveElements.Count == 0)
            {
                element = create();
                activeElements.Add(element);
            }
            else
            {
                element = PopItem();
            }

            CheckMaxCount();
            get?.Invoke(element);

            return element;
        }


        /// <summary>
        /// Async get inactive element from pool
        /// </summary>
        public async UniTask<T> GetAsync(CancellationToken cancellationToken)
        {
            T element;
            if (inactiveElements.Count == 0)
            {
                element = await createAsync(cancellationToken);
                activeElements.Add(element);
            }
            else
            {
                element = PopItem();
            }

            CheckMaxCount();
            get?.Invoke(element);

            return element;
        }


        /// <summary>
        /// Release active element back to pool
        /// </summary>
        public void Release(T element)
        {
            if (!activeElements.Remove(element))
                return;

            release?.Invoke(element);
            inactiveElements.Add(element);
        }


        /// <summary>
        /// Unregister element from pool
        /// </summary>
        public void Unregister(T element)
        {
            inactiveElements.Remove(element);
            activeElements.Remove(element);
        }


        private T PopItem()
        {
            T element = PopInactive();
            activeElements.Add(element);
            return element;
        }


        private T PopInactive()
        {
            int last = inactiveElements.Count - 1;
            T result = inactiveElements[last];

            inactiveElements.RemoveAt(last);
            return result;
        }


        private void CheckMaxCount()
        {
            if (activeElements.Count > maxSize)
            {
                Debug.LogError($"WARNING! {poolKey} pool limit is reached! Count = {activeElements.Count}. Limit = {maxSize}");
            }
        }
    }
}