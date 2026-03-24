using System;
using System.Collections.Generic;

namespace Infrastructure.Collections
{
    public class PooledList<T> : List<T>, IDisposable
    {
        private static readonly Stack<PooledList<T>> pool = new();
        private bool active;

        private PooledList() { }

        public static PooledList<T> Get(int capacity = 2)
        {
            if (pool.Count == 0)
            {
                return new PooledList<T> { active = true, Capacity = capacity };
            }

            var list = pool.Pop();
            list.active = true;
            return list;
        }

        public void Dispose()
        {
            Clear();
            
            if (!active)
            {
                UnityEngine.Debug.LogWarning($"[PooledList]. List of type \"{typeof(T).Name}\" has already been disposed!");
                CheckedPush();
                return;
            }
            
            active = false;
            pool.Push(this);
            //UnityEngine.Debug.Log($"[PooledList]. List of type \"{typeof(T).Name}\" disposed");
        }


        private void CheckedPush()
        {
            if(!pool.Contains(this))
                pool.Push(this);
        }
    }
}