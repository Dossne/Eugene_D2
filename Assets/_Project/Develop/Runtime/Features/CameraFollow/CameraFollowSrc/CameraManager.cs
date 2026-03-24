using System;
using Features.Character;
using Features.Collectables;
using Features.Effects;
using Features.Events;
using Features.LevelSessionStateControl;
using Features.LevelUp;
using Infrastructure.CameraControl;
using Infrastructure.Configs;
using Infrastructure.HapticControl;
using Infrastructure.SystemsLifeCycle;
using Infrastructure.Utilities;
using R3;

namespace Features.CameraFollow
{
    public class CameraManager : ISystemLateTickable, ILevelSessionSavable
    {
        private readonly CameraService cameraService;
        private readonly CameraConfig cameraConfig;
        private readonly CharacterManager characterManager;
        private readonly LevelProgressChangeEvent levelProgressChangeEvent;
        private readonly LevelStartedEvent startedEvent;
        private readonly CollectItemEvent collectItemEvent;
        private readonly CompositeDisposable disposable;
        private readonly EffectsConfig effectsConfig;
        private PrewiewData previewConfigData;
        private CameraData currentConfigData;
        private CameraData maxLevelConfigData;
        private int currentLevel;

        private int prevSessionLevel;
        private bool isRestoreSession;
        private bool isInit;


        public CameraManager(CameraService cameraService,
                             ConfigProvider configProvider,
                             CharacterManager characterManager,
                             LevelProgressChangeEvent levelProgressChangeEvent,
                             LevelStartedEvent startedEvent,
                             CollectItemEvent collectItemEvent)
        {
            this.cameraService = cameraService;
            this.cameraConfig = configProvider.CameraConfig;
            this.effectsConfig = configProvider.EffectsConfig;
            this.characterManager = characterManager;
            this.levelProgressChangeEvent = levelProgressChangeEvent;
            this.startedEvent = startedEvent;
            this.collectItemEvent = collectItemEvent;
            this.disposable = new CompositeDisposable();
        }


        void ILevelSessionSavable.RestoreSessionState(LevelSessionData sessionData)
        {
            prevSessionLevel = sessionData.lastReportedLevel;
            isRestoreSession = true;
        }


        void ILevelSessionSavable.SaveSessionState(LevelSessionData sessionData)
        {
        }


        public void Initialize()
        {
            if (isInit)
                return;

            currentLevel = isRestoreSession ? prevSessionLevel : 1;
            InitializeCameraData(currentLevel);

            if (previewConfigData != null && !isRestoreSession)
                SetPreviewCameraPosition();
            else
                SetCameraPosition(currentConfigData.posY, currentConfigData.posZ);
            cameraService.SetPosition(characterManager.GetPosition());

            maxLevelConfigData = cameraConfig.GetMaxLevelData();
            SubscribeOnLevelProgressChange();
            SubscribeOnCollectBomb();
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            disposable.Dispose();
            isInit = false;
        }


        void ISystemLateTickable.LateTick()
        {
            FollowCharacter();
        }


        private void FollowCharacter()
        {
            cameraService.SmoothFollow(characterManager.GetPosition());
        }


        private void InitializeCameraData(int level)
        {
            if (!cameraConfig.TryGetDataByLevel(level, out var newData))
            {
                newData = maxLevelConfigData;
            }

            currentConfigData = newData;
            currentLevel = level;
            previewConfigData = cameraConfig.PreviewData;
        }


        private void SetPreviewCameraPosition()
        {
            if (previewConfigData == null)
                return;
            SetCameraPosition(previewConfigData.posY, previewConfigData.posZ);
        }

        private void SetCameraPosition(float y, float z, float smoothTime = 0)
        {
            cameraService.SetCameraPosition(y, z, smoothTime);
        }


        private void SubscribeOnLevelProgressChange()
        {
            levelProgressChangeEvent.Subscribe(LevelProgressChangeEvent).AddTo(disposable);
            startedEvent.Subscribe(StartEvent).AddTo(disposable);
        }

        private void SubscribeOnCollectBomb()
        {
            collectItemEvent.Subscribe(item =>
            {
                if (item.Group is ItemGroup.Damage)
                {
                    cameraService.ShakeCamera(effectsConfig.deathCamShake);
                    cameraService.ShowRedVignette(effectsConfig.deathVignetteFx, false);
                    HapticService.I.Haptic(effectsConfig.deathHaptics);
                }
            }).AddTo(disposable);
        }


        private void LevelProgressChangeEvent(LevelProgressArgs args)
        {
            int newLevel = currentConfigData.level;

            switch (args.type)
            {
                case ActionType.Add:
                    newLevel += args.diff;
                    break;
                case ActionType.Remove:
                    newLevel -= args.diff;
                    break;
                default:
                    newLevel = args.diff;
                    break;
            }

            newLevel = Math.Clamp(newLevel, 1, maxLevelConfigData.level);

            InitializeCameraData(newLevel);
            SetCameraPosition(currentConfigData.posY, currentConfigData.posZ, currentConfigData.smoothTimeSec);
        }

        private void StartEvent(Unit unit)
        {
            if (previewConfigData == null)
                return;
            SetCameraPosition(currentConfigData.posY, currentConfigData.posZ, previewConfigData.smoothTimeSec);
        }
    }
}