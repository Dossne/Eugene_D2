using System;
using Infrastructure.Collections;

namespace Infrastructure.BroTweens
{
    [Serializable]
    internal class BroPool<T> : IBroPool where T : IBroPoolable, new()
    {
        [TriInspector.ShowInInspector] private readonly FastList<T> inactiveElements;
        [TriInspector.ShowInInspector] private int preWarmCount;


        public BroPool(int startCapacity = 4)
        {
            inactiveElements = new FastList<T>(startCapacity);
        }


        public void Prewarm(int targetCount)
        {
            int diff = targetCount - preWarmCount;

            if (diff <= 0)
                return;

            for (int i = 0; i < diff; i++)
            {
                Release(CreateNew());
            }

            preWarmCount += diff;

#if UNITY_EDITOR
            capacity_editor = inactiveElements.capacity;
#endif
        }


        public void Deinitialize()
        {
            inactiveElements.Clear();
        }


        public T Get()
        {
            bool canGet = inactiveElements.length > 0;
            var element = canGet ? GetInactive() : CreateNew();

            element.OnGet();

#if UNITY_EDITOR
            //BroTweenId.SetReserved(element.UniqueId);

            if (isDebug && !canGet)
                DebugCreateNewOnGet();
#endif
            return element;
        }


        private T CreateNew()
        {
            var element = new T();
            element.Construct(this);
            return element;
        }


        public void Release(IBroPoolable element)
        {
            inactiveElements.Add((T)element);
            element.OnReturnToPool();

#if UNITY_EDITOR
            //BroTweenId.SetReleased(element.UniqueId);
            if (isDebug)
            {
                DebugCapacity();
                DebugRelease((T)element);
            }

#endif
        }


        private T GetInactive()
        {
            int last = inactiveElements.length - 1;
            T result = inactiveElements[last];
            inactiveElements.RemoveAtSwapBackFast(last);
            return result;
        }


#if UNITY_EDITOR
        private bool isDebug;
        private int capacity_editor;


        private void DebugCreateNewOnGet()
        {
            UnityEngine.Debug.Log($"[BroTween] Create new item when Get. ItemType: {typeof(T).Name}");
        }


        private void DebugCapacity()
        {
            if (capacity_editor < inactiveElements.length)
            {
                capacity_editor = inactiveElements.length;
                UnityEngine.Debug.Log($"[BroTween] Capacity exceeded to {capacity_editor}. ItemType: {typeof(T).Name}");
            }
        }


        private void DebugRelease(T element)
        {
            UnityEngine.Debug.Log($"[BroTween] Released {element.GetType().Name}");
        }


#endif
    }
}