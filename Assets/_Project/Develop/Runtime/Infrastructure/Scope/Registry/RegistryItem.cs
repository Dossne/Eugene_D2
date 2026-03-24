using System.Collections.Generic;
using Infrastructure.Utilities;
using VContainer.Unity;

namespace Infrastructure.SystemsLifeCycle
{
    public class RegistryItem<T>
    {
        private readonly Dictionary<LifetimeScope, List<T>> registry = new();
        private readonly List<T> active = new();
        private bool isAnyActive;

        public List<T> Active => active;
        public bool IsAnyActive => isAnyActive;


        public void Register(LifetimeScope owner, T item)
        {
            if (!registry.TryGetValue(owner, out List<T> registryItems))
            {
                registryItems = new List<T>();
                registry.Add(owner, registryItems);
            }

            if (active.Contains(item))
                return;

            if (!registryItems.Contains(item))
                registryItems.Add(item);
        }


        public void Register(LifetimeScope owner, IEnumerable<T> input)
        {
            if(input.IsNullOrEmpty())
                return;
            
            if (!registry.TryGetValue(owner, out List<T> registryItems))
            {
                registryItems = new List<T>();
                registry.Add(owner, registryItems);
            }

            foreach (var item in input)
            {
                if (active.Contains(item))
                    continue;

                registryItems.Add(item);
            }
        }


        public void Unregister(LifetimeScope owner)
        {
            if (!this.registry.TryGetValue(owner, out List<T> registeredItems))
                return;

            foreach (var item in registeredItems)
            {
                active.Remove(item);
            }

            this.registry.Remove(owner);
            isAnyActive = active.Count > 0;
        }


        public bool TryGet(LifetimeScope owner, out List<T> resultByKey)
        {
            return registry.TryGetValue(owner, out resultByKey);
        }


        public void SetActive(LifetimeScope owner)
        {
            if (!this.registry.TryGetValue(owner, out List<T> registeredItems))
                return;

            foreach (var item in registeredItems)
            {
                active.Add(item);
            }

            isAnyActive = active.Count > 0;
        }


        public void Dispose()
        {
            registry.Clear();
            active.Clear();
            isAnyActive = false;
        }
    }
}