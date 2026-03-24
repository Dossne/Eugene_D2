using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.GameplayContainers
{
    public class GameplayContainerProvider : MonoBehaviour
    {
        private readonly Dictionary<(string tag, Type), IGameplayContainer> containers = new();
        private bool isInit;


        public void Initialize()
        {
            if (isInit)
            {
                return;
            }

            foreach (Transform child in transform)
            {
                if (child.TryGetComponent(out IGameplayContainer container) && container.IsActive)
                {
                    string keyTag = container.Tag.Trim();
                    keyTag = string.IsNullOrEmpty(keyTag) ? null : keyTag;

                    (string tag, Type) key = (keyTag, container.GetType());

                    if (!containers.TryAdd(key, container))
                    {
                        throw new Exception($"Duplicate key: {key.ToString()}. Try change {nameof(container.Tag)} of {container.GetType().Name}.");
                    }
                }
            }

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
            {
                return;
            }

            containers.Clear();

            isInit = false;
        }


        public T Get<T>(string spawnerTag = null) where T : IGameplayContainer
        {
            (string tag, Type) key = (tag: spawnerTag, typeof(T));

            if (!containers.TryGetValue(key, out IGameplayContainer container))
            {
                throw new Exception($"Item with key {key.ToString()} not registered in service");
            }

            return (T)container;
        }
    }
}