using System.Runtime.CompilerServices;
using Infrastructure.Collections;
using Infrastructure.SystemsLifeCycle;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    /// <summary>
    /// Uses poolable tweens, less allocations.
    /// Your Bro in hot code.
    /// </summary>
    public class BroTweenService : SingletonComponent<BroTweenService>, ISystemTickable
    {
        [TriInspector.Title("Playing tweens")]
        [TriInspector.ShowInInspector, TriInspector.ReadOnly] private IntHashSet activeUids;
        [TriInspector.ShowInInspector, TriInspector.ReadOnly] private FastList<BroTweenBase> activeScaled;
        [TriInspector.ShowInInspector, TriInspector.ReadOnly] private FastList<BroTweenBase> activeUnscaled;

        [TriInspector.Title("Pools")]
        [TriInspector.ShowInInspector, TriInspector.ReadOnly] internal BroPoolRegistry Pools;

        private BroTweenConfig config;
        private bool isCompleteBetweenScenes;
        private bool isInit;

        public BroTweenConfig Config => config;

        public void Construct(BroTweenConfig config)
        {
            this.config = config;
            
            activeUids = new IntHashSet(config.activeUidsStartCapacity);
            activeScaled = new FastList<BroTweenBase>(config.activeScaledStartCapacity);
            activeUnscaled = new FastList<BroTweenBase>(config.activeUnscaledStartCapacity);
        }

        void ISystemTickable.Tick()
        {
            if (isCompleteBetweenScenes)
            {
                CompleteItemsBetweenScenes(activeScaled);
                CompleteItemsBetweenScenes(activeUnscaled);
                isCompleteBetweenScenes = false;
            }

            TickItems(activeScaled, Time.deltaTime);
            TickItems(activeUnscaled, Time.unscaledDeltaTime);
        }

        public void Initialize()
        {
            if (isInit)
                return;

            Pools = new BroPoolRegistry(config);
            Pools.Initialize();
            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            BroTweenId.Reset();
            activeScaled.Clear();
            activeUnscaled.Clear();
            activeUids.Clear();
            isCompleteBetweenScenes = false;

            Pools.Deinitialize();
            Pools = null;
            isInit = false;
        }

        public void CompleteBetweenScenes()
        {
            isCompleteBetweenScenes = true;
        }

        internal bool TryPlay(BroTweenBase item)
        {
            if (!item.TrySetProcessing_Internal())
                return false;

            if (!activeUids.Add(item.UniqueId))
                return false;

            if (item.IsUnscaledDt)
                activeUnscaled.Add(item);
            else
                activeScaled.Add(item);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TickItems(FastList<BroTweenBase> items, in float dt)
        {
            if (dt <= 0)
                return;

            int length = items.length;

            for (int i = 0; i < length; i++)
            {
                var item = items[i];

                if (!item.IsProcessing || item.IsInPool)
                {
                    length = RemoveOneItem(items, item, i);
                    continue;
                }

                item.Tick_Internal(in dt);

                if (!item.IsTimeComplete)
                {
                    continue;
                }

                item.Complete_Internal(true, false);
                length = RemoveOneItem(items, item, i);
            }
        }

        private void CompleteItemsBetweenScenes(FastList<BroTweenBase> items)
        {
            int length = items.length;

            for (int i = 0; i < length; i++)
            {
                var item = items[i];

                if (item.IsLiveBetweenScenes)
                {
                    continue;
                }

                item.Complete_Internal(true, true);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int RemoveOneItem(FastList<BroTweenBase> processedItems, BroTweenBase item, int i)
        {
            if (processedItems.length > 0)
                processedItems.RemoveAtSwapBackFast(i);

            activeUids.Remove(item.UniqueId);
            return processedItems.length;
        }
    }
}