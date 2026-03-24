using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Effects;
using Features.Events;
using Features.Level;
using Features.LevelConfiguration;
using Features.LevelSessionStateControl;
using Infrastructure.CameraControl;
#if PR_CHEAT
using Infrastructure.Cheat;
#endif
using Infrastructure.Configs;
using Infrastructure.MainUICanvasControl;
using Infrastructure.SystemsLifeCycle;
using Infrastructure.Utilities;
using UnityEngine;

namespace Features.LevelTime
{
    public class LevelTimeManager : ISystemTickable, ILevelSessionSavable
    {
        private readonly LevelTimerHud timerView;
        private readonly EffectsConfig fxConfig;
        private readonly List<Func<bool>> fxTargets;
        private readonly DeathEvent deathEvent;
        private readonly CameraService cameraService;
        private readonly LevelService levelService;
#if PR_CHEAT
        private readonly CheatService cheatService;
#endif
        private readonly CancellationTokenSource cts;
        private bool isInit;

        private float configTime;
        private float totalTime;
        private float currentTime;
        private float secondTime;
        private bool isTimerActive;
        private bool isTimerBoosterActive = false;

        private float currentTimePrevSession;
        private float totalTimePrevSession;
        private bool isRestoreSession;


        public LevelTimeManager(
            MainUIProvider uiProvider,
            ConfigProvider configProvider,
            DeathEvent deathEvent,
            CameraService cameraService,
            LevelService levelService
#if PR_CHEAT
          , CheatService cheatService
#endif
        )
        {
            this.timerView = uiProvider.HudProvider.LevelTimerHud;
            this.fxConfig = configProvider.EffectsConfig;
            this.deathEvent = deathEvent;
            this.cameraService = cameraService;
            this.levelService = levelService;
#if PR_CHEAT
            this.cheatService = cheatService;
#endif
            fxTargets = new List<Func<bool>>();
            cts = new CancellationTokenSource();
        }


        public Vector2 HudIconPos => timerView.IconTransform.position;
        public Vector2 HudFlyTargetPos => timerView.FlyTargetTransform.position;
        public float SecondsLeft => (float)Math.Round(currentTime, 1);
        public float ConfigTime => configTime;
        public float TotalTime => totalTime;
        public bool TimeIsOff => currentTime <= 0;


        void ILevelSessionSavable.RestoreSessionState(LevelSessionData sessionData)
        {
            currentTimePrevSession = sessionData.currentTime;
            totalTimePrevSession = sessionData.totalTime;
            isRestoreSession = true;
        }


        void ILevelSessionSavable.SaveSessionState(LevelSessionData sessionData)
        {
            sessionData.currentTime = currentTime;
            sessionData.totalTime = totalTime;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            isTimerBoosterActive = false;

            LevelData configData = levelService.GetCurrentLevelData();
            configTime = configData.time;

            currentTime = isRestoreSession ? currentTimePrevSession : configTime;
            totalTime = isRestoreSession ? totalTimePrevSession : configTime;

            InitializeTimerHud();
            InitializeGameFxActions();
#if PR_CHEAT
            cheatService.OnReduceLevelTimeRequest += CheatService_OnReduceLevelTimeRequest;
            cheatService.OnFreezeLevelTimeRequest += CheatService_OnFreezeLevelTimeRequest;
#endif
            isInit = true;
        }


        void ISystemTickable.Tick()
        {
            if (!isTimerActive)
                return;

            float dt = Time.deltaTime;
            HandleCurrentTimer(dt);
            HandleSecondTimer(dt);

            HandleTimeOver();
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

#if PR_CHEAT
            cheatService.OnReduceLevelTimeRequest -= CheatService_OnReduceLevelTimeRequest;
            cheatService.OnFreezeLevelTimeRequest -= CheatService_OnFreezeLevelTimeRequest;
#endif

            timerView.Deinitialize();
            fxTargets.Clear();

            cameraService.HideRedVignette(false);
            cts.Cancel();
            cts.Dispose();
            isInit = false;
        }


        public void StopTimer(bool byBooster = false)
        {
            if (byBooster)
                isTimerBoosterActive = true;

            isTimerActive = false;
        }


        public void SetTimerActive(bool byBooster = false)
        {
            if (byBooster)
                isTimerBoosterActive = false;

            if (!isTimerBoosterActive)
                isTimerActive = true;
        }


        public void AddCurrentAndTotalTime(int addedSeconds)
        {
            currentTime += addedSeconds;
            AddTotalTime(addedSeconds);
        }


        public void AddTotalTime(int addedSeconds)
        {
            totalTime += addedSeconds;
            secondTime = 0;
            timerView.SetMaxTime(totalTime);
            timerView.SetSliderProgress(currentTime);
        }


        public void AddTimeOnResurrect(int addedSeconds)
        {
            AddCurrentAndTotalTime(addedSeconds);
            cameraService.HideRedVignette(true);
            timerView.ResetFx(fxConfig.textFx.defaultColor, fxConfig.sliderFx.defaultColor);
            InitializeGameFxActions();
            isTimerActive = true;
        }


        public UniTask PlayAddTimeFxAsync(TimerTextBoostFxData fxData, Color textFxDefaultColor, int fromTime, int toTime,
                                          CancellationToken cancellationToken)
        {
            return timerView.PlayAddTimeFxAsync(fxData, textFxDefaultColor, fromTime, toTime, cancellationToken);
        }


        public void PlayHudFreezeOn(AnimationCurve hudCurve, float hudDuration, AnimationCurve sliderCurve, float sliderDuration, Color sliderColor,
                                    float shakeDuration, float shakeStrength, int shakeVibrato)
        {
            timerView.PlayFreezeFx(hudCurve, 0, 1, hudDuration);
            timerView.PlayShake(shakeDuration, shakeStrength, shakeVibrato);
            timerView.DoSliderColor(sliderCurve, sliderColor, sliderDuration);
        }


        public void PlayHudFreezeOff(AnimationCurve curve, float duration, AnimationCurve sliderCurve, float sliderDuration)
        {
            timerView.PlayFreezeFx(curve, 1, 0, duration);
            timerView.ResetSliderColorToCurrent(sliderCurve, sliderDuration);
        }


        public void RemoveHudFreezeFxInstant()
        {
            timerView.RemoveFreezeFxInstant();
        }


        private void HandleCurrentTimer(float dt)
        {
            currentTime -= dt;
            timerView.SetSliderProgress(currentTime);
        }


        private void HandleSecondTimer(float dt)
        {
            secondTime -= dt;

            if (secondTime <= 0)
            {
                secondTime = 1;
                RefreshTimerHud();
                HandleFxActions();
            }
        }


        private void HandleTimeOver()
        {
            if (currentTime > 0)
                return;

            currentTime = 0;
            deathEvent.ExecuteWithTime();
            isTimerActive = false;
        }


        private void InitializeTimerHud()
        {
            timerView.Construct(totalTime, currentTime.ToString(CultureInfo.InvariantCulture), fxConfig.textFx.defaultColor, fxConfig.sliderFx.defaultColor);

            timerView.Initialize();
            RefreshTimerHud();
            timerView.SetSliderProgress(currentTime);
        }


        private void InitializeGameFxActions()
        {
            fxTargets.Add(TryBeginTextColorFx);

            foreach (var pair in fxConfig.sliderFx.colorsByTime)
            {
                fxTargets.Add(() => TrySliderColorFx(pair.Key));
            }

            foreach (var time in fxConfig.lowTimesFx)
            {
                fxTargets.Add(() => TryVignetteFx(time.value, time.isContinuous));
            }
        }


        private void HandleFxActions()
        {
            for (int i = fxTargets.Count - 1; i >= 0; i--)
            {
                if (fxTargets[i].Invoke())
                {
                    fxTargets.RemoveAt(i);
                }
            }
        }


        private void RefreshTimerHud()
        {
            timerView.SetTimeText(TimeUtils.GetTimeString(SecondsLeft, trimFirstZero: true));
        }


        private float GetProgress()
        {
            return currentTime / totalTime * 100f;
        }


        private bool TryBeginTextColorFx()
        {
            if (currentTime > fxConfig.textFx.beginTime)
                return false;

            timerView.DoTextColorYoyo(fxConfig.textFx.curve, fxConfig.textFx.defaultColor, fxConfig.textFx.lowColor, fxConfig.textFx.duration);

            return true;
        }


        private bool TrySliderColorFx(int targetTimeProgress)
        {
            if (GetProgress() > targetTimeProgress)
                return false;

            var targetColor = fxConfig.sliderFx.colorsByTime[targetTimeProgress];
            timerView.DoSliderColor(fxConfig.sliderFx.curve, targetColor, fxConfig.sliderFx.duration);
            return true;
        }


        private bool TryVignetteFx(float targetTime, bool repeat)
        {
            if (currentTime > targetTime)
                return false;

            cameraService.ShowRedVignette(fxConfig.lowTimeVignetteFx, repeat);
            return true;
        }


        private void CheatService_OnReduceLevelTimeRequest()
        {
            currentTime = 6;
        }


        private void CheatService_OnFreezeLevelTimeRequest()
        {
            if (isTimerActive)
                StopTimer();
            else
                SetTimerActive();
        }
    }
}