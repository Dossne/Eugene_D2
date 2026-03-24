using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Effects;
using Features.LevelTime;
using Infrastructure.Pool.FloatingIcon;
using Infrastructure.Pool.FloatingText;
using Infrastructure.AudioControl;
using Infrastructure.Pool;
using UnityEngine;

namespace Features.Boosters.Behaviour.Shared
{
    public class BonusClockLogic
    {
        private readonly PoolService poolService;
        private readonly RectTransform flyingItemRoot;
        private readonly EffectsConfig fxConfig;
        private readonly LevelTimeManager levelTimeManager;


        public BonusClockLogic(
            PoolService poolService,
            RectTransform flyingItemRoot,
            EffectsConfig fxConfig,
            LevelTimeManager levelTimeManager
        )
        {
            this.poolService = poolService;
            this.flyingItemRoot = flyingItemRoot;
            this.fxConfig = fxConfig;
            this.levelTimeManager = levelTimeManager;
        }


        public async UniTaskVoid ApplyAsync(int addTime, CancellationToken cancellationToken)
        {
            int fromTime = (int)levelTimeManager.SecondsLeft;

            var iconPool = poolService.Get<FloatingIconPool>();
            if (!iconPool.TryGetItem(FloatingIconType.BonusClockBooster, out PoolableFloatingIcon flyIcon))
                return;

            flyIcon.ShowAndFly(null, flyingItemRoot.position, levelTimeManager.HudIconPos);
            AudioService.I.PlaySfx(SfxType.BonusClock);
            await UniTask.WaitForSeconds(flyIcon.MoveDuration, ignoreTimeScale: true, cancellationToken: cancellationToken);

            var textPool = poolService.Get<FloatingTextPool>();
            if (!textPool.TryGetItem(FloatingTextType.BonusClockBooster, out PoolableFloatingText flyText))
                return;
            
            flyText.Show($"+{addTime.ToString()}", levelTimeManager.HudFlyTargetPos, Vector3.one, false);

            await levelTimeManager.PlayAddTimeFxAsync(fxConfig.preBoosterBonusClockFx, fxConfig.textFx.defaultColor, fromTime, fromTime + addTime,
                                                      cancellationToken);

            levelTimeManager.AddCurrentAndTotalTime(addTime);
        }
    }
}