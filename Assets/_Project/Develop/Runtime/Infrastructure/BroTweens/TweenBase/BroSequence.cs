using System;
using Infrastructure.Collections;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public class BroSequence : BroTweenBase
    {
        [TriInspector.ShowInInspector] private bool allItemsCompleted;
        [TriInspector.ShowInInspector] protected readonly FastList<BroSequenceItem> items = new();

        public override bool IsTimeComplete => CurrentTime >= Duration + Delay && allItemsCompleted;


        public BroSequence Append(BroTweenBase tween)
        {
            AddSequenceItem(tween, Duration);
            SetDuration(this.Duration + tween.Duration + tween.Delay);
            return this;
        }


        public BroSequence Insert(in float at, BroTweenBase tween)
        {
            float addTime = tween.Duration + tween.Delay;
            float endTime = at + addTime;

            if (endTime > Duration)
                SetDuration(endTime);

            AddSequenceItem(tween, at);
            return this;
        }


        public BroSequence AppendCallback(in Action callback)
        {
            BroCallback tween = BroTweenService.I.Pools.BroCallback.Get();
            tween.SetParams(in callback);

            AddSequenceItem(tween, Duration);
            return this;
        }


        public BroSequence AppendCallback<T>(T target, Action<T> onComplete) where T : class
        {
            BroCallback tween = BroTweenService.I.Pools.BroCallback.Get();
            tween.SetParams(target, onComplete);
            AddSequenceItem(tween, Duration);
            return this;
        }


        public BroSequence InsertCallback(in float at, in Action callback)
        {
            BroCallback tween = BroTweenService.I.Pools.BroCallback.Get();
            tween.SetParams(in callback);

            if (at > Duration)
                SetDuration(at);

            AddSequenceItem(tween, at);
            return this;
        }


        public BroSequence InsertCallback<T>(in float at, T target, in Action<T> onComplete) where T : class
        {
            BroCallback tween = BroTweenService.I.Pools.BroCallback.Get();
            tween.SetParams(target, in onComplete);

            if (at > Duration)
                SetDuration(at);

            AddSequenceItem(tween, at);
            return this;
        }


        public BroSequence AppendInterval(in float value)
        {
            SetDuration(this.Duration + value);
            return this;
        }


        public override void Complete(bool withCallback = false)
        {
            if (data.isInPool || IsCompleted)
                return;
            
            for (int i = 0; i < items.length; i++)
            {
                items[i].tween.Complete(withCallback);
            }

            base.Complete(withCallback);
        }

        
        public override void Stop()
        {
            if (data.isInPool || IsCompleted)
                return;
            
            for (int i = 0; i < items.length; i++)
            {
                items[i].tween.Stop();
            }

            base.Stop();
        }
        

        internal override bool TryReset_Internal()
        {
            if (items.length == 0)
                return false;

            if (!base.TryReset_Internal())
                return false;

            for (int i = 0; i < items.length; i++)
            {
                if (!items[i].tween.TryReset_Internal())
                    return false;
            }

            allItemsCompleted = false;
            return true;
        }


        internal override bool TrySetProcessing_Internal()
        {
            for (int i = 0; i < items.length; i++)
            {
                if (!items[i].tween.TrySetProcessing_Internal())
                    return false;
            }

            if (items.length == 0)
            {
                allItemsCompleted = true; //if empty and have OnComplete callback then invoke it on next tick
            }
            
            return base.TrySetProcessing_Internal();
        }


        protected override void OnTick(in float dt)
        {
            if (allItemsCompleted)
                return;

            int completedCount = 0;

            for (int i = 0; i < items.length; i++)
            {
                var item = items[i];

                if (CurrentTime < item.startTime)
                    continue;

                if (!item.tween.IsTimeComplete)
                    items[i].tween.Tick_Internal(dt);

                if (!item.tween.IsCompleted && items[i].tween.IsTimeComplete)
                {
                    item.tween.Complete(true);

                    if (items.length == 0) //seq killed with callback
                        break;
                }

                if (item.tween.IsCompleted)
                    completedCount++;

                if (completedCount >= items.length)
                {
                    allItemsCompleted = true;
                    break;
                }
            }
        }


        protected override void OnClearParamsWhenReturnToPool()
        {
            foreach (var tickable in items)
            {
                tickable.Release();
            }

            items.Clear();
            allItemsCompleted = false;
        }

        
        private void AddSequenceItem(BroTweenBase tween, float at)
        {
            BroSequenceItem item = BroTweenService.I.Pools.BroSequenceItem.Get();
            item.SetParams(tween, at);
            tween.SetAutoKill(false);
            items.Add(item);
        }
    }
}