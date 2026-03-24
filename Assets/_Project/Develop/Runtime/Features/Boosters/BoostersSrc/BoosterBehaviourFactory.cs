using System;
using Features.Boosters.Behaviour;
using Features.Boosters.Behaviour.InGameBooster;
using Features.Boosters.Behaviour.PreBooster;
using Features.Boosters.Model;
using Infrastructure.AssetManagement;
using VContainer;

namespace Features.Boosters
{
    public class BoosterBehaviourFactory
    {
        private readonly Instantiator instantiator;

        public BoosterBehaviourFactory(Instantiator instantiator)
        {
            this.instantiator = instantiator;
        }

        public InGameBoosterBehaviour CreateInGameBooster(BoosterModel model, InGameBoosterSlotView slotView)
        {
            switch (model.BoosterType)
            {
                case BoosterType.TimeFreeze:
                    return instantiator.InstantiateClass<TimeFreezeBoosterBehaviour>(Lifetime.Scoped, model, slotView);  
                case BoosterType.SizeBooster:
                    return instantiator.InstantiateClass<SizeBoosterBehaviour>(Lifetime.Scoped, model, slotView);
                case BoosterType.ObjectFinder:
                    return instantiator.InstantiateClass<ObjectFinderBehaviour>(Lifetime.Scoped, model, slotView);
                case BoosterType.SuperMagnet:
                    return instantiator.InstantiateClass<SuperMagnetBehaviour>(Lifetime.Scoped, model, slotView);
            }

            throw new NotSupportedException($"Not supported ingame booster type: {model.BoosterType.ToString()}");
        }


        public PreBoosterBehaviour CreatePreBooster(BoosterModel model)
        {
            switch (model.BoosterType)
            {
                case BoosterType.BonusClock:
                    return instantiator.InstantiateClass<BonusClockBehavior>(Lifetime.Scoped, model);  
                case BoosterType.BoostBottle:
                    return instantiator.InstantiateClass<BoostBottleBehaviour>(Lifetime.Scoped, model);
            }

            throw new NotSupportedException($"Not supported pre booster type: {model.BoosterType.ToString()}");
        }
        
        
        
        public WinStreakBehaviour CreateWinStreakBehaviour(WinStreakBoosterData data)
        {
            switch (data.type)
            {
                case BoosterType.BonusClockWinStreak:
                    return instantiator.InstantiateClass<BonusClockWinStreakBehaviour>(Lifetime.Scoped, data);  
                case BoosterType.BoostBottleWinStreak:
                    return instantiator.InstantiateClass<BoostBottleWinStreakBehaviour>(Lifetime.Scoped, data);
            }

            throw new NotSupportedException($"Not supported WinStreak booster type: {data.type.ToString()}");
        }
    }
}