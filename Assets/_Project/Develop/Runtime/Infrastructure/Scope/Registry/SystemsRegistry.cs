using System.Collections.Generic;
using Infrastructure.PersistentProgress;
using VContainer.Unity;

namespace Infrastructure.SystemsLifeCycle
{
    public class SystemsRegistry
    {
        private readonly SaveStorage saveStorage;
        private readonly SavablesRegistry savables = new();
        private readonly TickablesRegistry tickables = new();
        private readonly FixedTickablesRegistry fixedTickables = new();
        private readonly LateTickablesRegistry lateTickables = new();

        public SystemsRegistry(SaveStorage saveStorage)
        {
            this.saveStorage = saveStorage;
        }


        public void Tick()
        {
            if (!tickables.IsAnyActive)
                return;

            for (int i = 0; i < tickables.Active.Count; i++)
            {
                tickables.Active[i].Tick();
            }
        }


        public void FixedTick()
        {
            if (!fixedTickables.IsAnyActive)
                return;

            for (int i = 0; i < fixedTickables.Active.Count; i++)
            {
                fixedTickables.Active[i].FixedTick();
            }
        }


        public void LateTick()
        {
            if (!lateTickables.IsAnyActive)
                return;

            for (int i = 0; i < lateTickables.Active.Count; i++)
            {
                lateTickables.Active[i].LateTick();
            }
        }


        public void PrepareSave()
        {
            if (!savables.IsAnyActive)
                return;

            foreach (var item in savables.Active)
            {
                item.Save(saveStorage.Progress);
            }
        }


        public void Load(LifetimeScope owner)
        {
            if (!savables.TryGet(owner, out List<ISavable> cachedSavables))
                return;

            foreach (var savable in cachedSavables)
            {
                savable.Load(saveStorage.Progress);
            }
        }


        public void Register(LifetimeScope owner, object item)
        {
            if (item is ISavable savable)
                savables.Register(owner, savable);

            if (item is ISystemTickable tickable)
                tickables.Register(owner, tickable);

            if (item is ISystemFixedTickable fixedTickable)
                fixedTickables.Register(owner, fixedTickable);

            if (item is ISystemLateTickable lateTickable)
                lateTickables.Register(owner, lateTickable);
        }


        public void Register(LifetimeScope owner, IEnumerable<ISavable> items)
        {
            savables.Register(owner, items);
        }


        public void Register(LifetimeScope owner, IEnumerable<ISystemTickable> items)
        {
            tickables.Register(owner, items);
        }


        public void Register(LifetimeScope owner, IEnumerable<ISystemFixedTickable> items)
        {
            fixedTickables.Register(owner, items);
        }


        public void Register(LifetimeScope owner, IEnumerable<ISystemLateTickable> items)
        {
            lateTickables.Register(owner, items);
        }


        // Move to active list in the end of all initialize
        public void SetActive(LifetimeScope owner)
        {
            savables.SetActive(owner);
            tickables.SetActive(owner);
            fixedTickables.SetActive(owner);
            lateTickables.SetActive(owner);
        }


        public void Unregister(LifetimeScope owner)
        {
            savables.Unregister(owner);
            tickables.Unregister(owner);
            fixedTickables.Unregister(owner);
            lateTickables.Unregister(owner);
        }


        public void Dispose()
        {
            savables.Dispose();
            tickables.Dispose();
            fixedTickables.Dispose();
            lateTickables.Dispose();
        }
    }
}