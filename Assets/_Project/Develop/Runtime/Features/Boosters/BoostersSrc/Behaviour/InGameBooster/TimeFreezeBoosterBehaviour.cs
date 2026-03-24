using Features.Boosters.Model;
using Features.Effects;
using Features.LevelTime;
using Infrastructure.AudioControl;
using Infrastructure.CameraControl;
using Infrastructure.Configs;

namespace Features.Boosters.Behaviour.InGameBooster
{
    public sealed class TimeFreezeBoosterBehaviour : InGameBoosterBehaviour
    {
        private readonly LevelTimeManager levelTimeManager;
        private readonly CameraService cameraService;
        private readonly EffectsConfig fxConfig;


        public TimeFreezeBoosterBehaviour(
            InGameBoosterModelBase model,
            InGameBoosterSlotView view,
            LevelTimeManager levelTimeManager,
            CameraService cameraService,
            ConfigProvider configProvider
        ) : base(model, view)
        {
            this.levelTimeManager = levelTimeManager;
            this.cameraService = cameraService;
            this.fxConfig = configProvider.EffectsConfig;
        }


        public override void Deinitialize()
        {
            cameraService.HideFreezeVignette(null, false);
            levelTimeManager.RemoveHudFreezeFxInstant();
        }


        protected override void ActionsOnApply()
        {
            levelTimeManager.StopTimer(true);
            levelTimeManager.AddTotalTime((int)model.DurationSec);
            
            cameraService.ShowFreezeVignette(fxConfig.freezeTimeVignetteFx);

            levelTimeManager.PlayHudFreezeOn(
                fxConfig.hudFreezeCurve,
                fxConfig.hudFreezeDuration,
                fxConfig.sliderFreezeCurve,
                fxConfig.sliderFreezeDuration,
                fxConfig.sliderFreezeColor,
                fxConfig.hudShakeDuration,
                fxConfig.hudShakeStrength,
                fxConfig.hudShakeVibrato);
            
            AudioService.I.PlaySfx(SfxType.FreezeTimeBooster);
        }


        protected override void ActionsOnUnapply()
        {
            levelTimeManager.SetTimerActive(true);
            cameraService.HideFreezeVignette(fxConfig.freezeTimeVignetteFx, true);
            
            levelTimeManager.PlayHudFreezeOff(
                fxConfig.hudFreezeCurve, 
                fxConfig.hudFreezeDuration, 
                fxConfig.sliderFreezeCurve,
                fxConfig.sliderFreezeDuration);
        }
    }
}