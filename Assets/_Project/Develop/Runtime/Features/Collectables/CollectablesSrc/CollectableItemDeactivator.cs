using Features.Events;
using Infrastructure.Collections;
using Infrastructure.SystemsLifeCycle;
using R3;
using UnityEngine;

namespace Features.Collectables
{
    public class CollectableItemDeactivator : ISystemTickable
    {
        private class TimerItem
        {
            private float time;
            private CollectableItem item;


            public void Construct(float time, CollectableItem item)
            {
                this.time = time;
                this.item = item;
            }


            public void Tick(float dt)
            {
                time -= dt;
            }


            public bool HaveTime()
            {
                return time > 0;
            }


            public void Deactivate()
            {
                item.SetObjectActive(false);
                item = null;
            }
        }

        private const float LifeTime = 1.5f;
        private readonly FastList<TimerItem> active = new(500);
        private readonly FastList<TimerItem> pool = new(500);
        private readonly CollectItemEvent collectItemEvent;
        private CompositeDisposable disposables;
        private bool isInit;


        public CollectableItemDeactivator(CollectItemEvent collectItemEvent)
        {
            this.collectItemEvent = collectItemEvent;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            disposables = new();
            collectItemEvent.Subscribe(ItemCollectManager_OnCollected).AddTo(disposables);

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            disposables.Dispose();

            active.Clear();
            pool.Clear();
            isInit = false;
        }


        void ISystemTickable.Tick()
        {
            float dt = Time.deltaTime;
            float length = active.length;

            for (int i = 0; i < length; i++)
            {
                var item = active[i];
                item.Tick(dt);

                if (item.HaveTime())
                    continue;

                Release(item);
                active.RemoveAtSwapBackFast(i);
                length = active.length;
            }
        }


        private TimerItem GetFromPool()
        {
            bool canGet = pool.length > 0;
            var element = canGet ? GetInactive() : new TimerItem();
            return element;
        }


        private void Release(TimerItem item)
        {
            item.Deactivate();
            pool.Add(item);
        }


        private TimerItem GetInactive()
        {
            int last = pool.length - 1;
            TimerItem result = pool[last];
            pool.RemoveAtSwapBackFast(last);
            return result;
        }


        private void ItemCollectManager_OnCollected(CollectableItem collectable)
        {
            collectable.SetCollidersActive(false);
            var item = GetFromPool();
            item.Construct(LifeTime, collectable);
            active.Add(item);
        }
    }
}