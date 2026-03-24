using System.Threading;
using Features.Boosters.Behaviour.Shared;
using Features.Boosters.Model;
using Features.LevelTime;
using Infrastructure.Configs;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Pool;

namespace Features.Boosters.Behaviour.PreBooster
{
    public class BonusClockBehavior : PreBoosterBehaviour
    {
        private readonly BonusClockLogic bonusClockLogic;
        
        public BonusClockBehavior(BonusClockBoosterModel model,
                                  MainUIProvider mainUIProvider,
                                  PoolService poolService,
                                  ConfigProvider configProvider,
                                  LevelTimeManager levelTimeManager
        ) : base(model)
        {
            bonusClockLogic = new BonusClockLogic(poolService, mainUIProvider.FlyingCurrencyRoot, configProvider.EffectsConfig, levelTimeManager);
        }


        public override void ApplyAsync(CancellationToken cancellationToken)
        {
            bonusClockLogic.ApplyAsync((int)model.BoostedValue, cancellationToken).Forget();
        }
    }
}