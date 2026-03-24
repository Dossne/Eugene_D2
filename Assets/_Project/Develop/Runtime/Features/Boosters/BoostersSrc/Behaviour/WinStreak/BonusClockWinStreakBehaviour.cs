using System.Threading;
using Features.Boosters.Behaviour.Shared;
using Features.LevelTime;
using Infrastructure.Configs;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Pool;

namespace Features.Boosters.Behaviour
{
    public class BonusClockWinStreakBehaviour : WinStreakBehaviour
    {
        private readonly BonusClockLogic bonusClockLogic;


        public BonusClockWinStreakBehaviour(WinStreakBoosterData data,
                                   MainUIProvider mainUIProvider,
                                   PoolService poolService,
                                   ConfigProvider configProvider,
                                   LevelTimeManager levelTimeManager) : base(data)
        {
            bonusClockLogic = new BonusClockLogic(poolService, mainUIProvider.FlyingCurrencyRoot, configProvider.EffectsConfig, levelTimeManager);

        }


        public override void ApplyAsync(CancellationToken cancellationToken)
        {
            bonusClockLogic.ApplyAsync((int)data.boostedValue, cancellationToken).Forget();
        }
    }
}