using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Character.CharacterSrc;
using Features.Collectables;
using Features.Events;
using Features.LevelConfiguration;
using Features.LevelSessionStateControl;
using Features.LevelUp;
using Features.SuperSpeedMode;
using Infrastructure.AssetManagement;
using Infrastructure.Configs;
using Infrastructure.Localization;
using Infrastructure.Settings;
using Infrastructure.SystemsLifeCycle;
using Infrastructure.Utilities;
using R3;
using UnityEngine;
using VContainer;

namespace Features.Character
{
    public class CharacterManager : ISystemTickable, ISystemFixedTickable, ILevelSessionSavable
    {
        private readonly AssetMappingConfig assetMapping;
        private readonly LevelCreateManager levelCreateManager;
        private readonly LevelProgressChangeEvent levelProgressChangeEvent;
        private readonly Instantiator instantiator;
        private readonly CharacterConfig characterConfig;
        private readonly SystemSettingsConfig systemSettingsConfig;
        private readonly CompositeDisposable disposable;
        private readonly CollectItemEvent collectItemEvent;
        private readonly DeathEvent deathEvent;
        private readonly ExperienceAddRequest experienceAddRequest;
        private readonly SuperSpeedController superSpeedController;
        private CharacterData currentConfigData;
        private CharacterData maxLevelConfigData;
        private CharacterMoveController moveController;
        private CharacterView characterView;
        private float currentScale;
        private string sizeLocalizedText;
        private int size;
        private float currentProgress01;

        private int prevSessionSize;
        private float prevSessionProgress01;
        private SerializableVector3 prevSessionPos;
        private bool isRestoreSession;
        private CharacterViewType characterViewType = CharacterViewType.Base;


        public CharacterManager(Instantiator instantiator,
                                ConfigProvider configProvider,
                                LevelCreateManager levelCreateManager,
                                LevelProgressChangeEvent levelProgressChangeEvent,
                                CollectItemEvent collectItemEvent,
                                DeathEvent deathEvent,
                                ExperienceAddRequest experienceAddRequest,
                                SuperSpeedController superSpeedController)
        {
            this.instantiator = instantiator;
            this.assetMapping = configProvider.AssetMappingConfig;
            this.characterConfig = configProvider.CharacterConfig;
            this.systemSettingsConfig = configProvider.SystemSettingsConfig;
            this.levelCreateManager = levelCreateManager;
            this.levelProgressChangeEvent = levelProgressChangeEvent;
            this.collectItemEvent = collectItemEvent;
            this.deathEvent = deathEvent;
            this.experienceAddRequest = experienceAddRequest;
            this.superSpeedController = superSpeedController;
            this.disposable = new CompositeDisposable();
        }


        void ILevelSessionSavable.RestoreSessionState(LevelSessionData sessionData)
        {
            prevSessionSize = sessionData.lastReportedLevel;
            prevSessionPos = sessionData.pos;
            prevSessionProgress01 = sessionData.progress01;
            isRestoreSession = true;
        }


        void ILevelSessionSavable.SaveSessionState(LevelSessionData sessionData)
        {
            sessionData.pos = new SerializableVector3(characterView.GetPosition());
            sessionData.progress01 = currentProgress01;
        }


        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            sizeLocalizedText = LocalizationService.I.Get(LocKeys.Character.Size);
            maxLevelConfigData = characterConfig.GetMaxLevelData();
            await CreateCharacter(cancellationToken);
            SubscribeOnChangeLevel();
            SubscribeOnCollectBomb();
            SubscribeOnExperienceAddRequest();
        }


        public void Deinitialize()
        {
            disposable.Dispose();
            characterView.Deinitialize();
        }


        void ISystemTickable.Tick()
        {
            moveController.Tick();
        }


        void ISystemFixedTickable.FixedTick()
        {
            moveController.FixedTick();
        }


        public void SetProgressSlider(float progress01, bool smooth = true)
        {
            currentProgress01 = progress01;
            characterView.SetProgressSlider(progress01, smooth);
        }


        public Vector3 GetPosition()
        {
            return characterView.GetPosition();
        }


        public Transform GetViewRoot()
        {
            return characterView.GetViewRoot();
        }


        public Transform GetMovementRoot()
        {
            return characterView.GetMovementRoot();
        }


        private void SubscribeOnExperienceAddRequest()
        {
            experienceAddRequest.Subscribe(ExperienceAddRequestHandle).AddTo(disposable);
        }


        private async UniTask CreateCharacter(CancellationToken cancellationToken)
        {
            (Transform root, Vector3 pos) spawner = levelCreateManager.GetCharacterSpawnPoint();

            characterView = await instantiator.InstantiateAsync<CharacterView>(assetMapping.GetCharacterView(),
                                                                               parent: spawner.root, inject: true,
                                                                               cancellationToken: cancellationToken);

            size = isRestoreSession ? prevSessionSize : 1;
            Vector3 spawnPos = isRestoreSession ? prevSessionPos.ToVector3() : spawner.pos;
            currentProgress01 = isRestoreSession ? prevSessionProgress01 : 0;
            InitializeCharacterData(size);
            characterView.Construct(currentConfigData.scale, spawnPos, Quaternion.identity);
            characterView.Initialize();
            this.moveController = instantiator.InstantiateClass<CharacterMoveController>(Lifetime.Singleton, characterView);
            characterViewType = superSpeedController.IsSuperSpeedActive ? CharacterViewType.SuperSpeed : CharacterViewType.Base;
            UpdateCharacterView();
            characterView.SetProgressSlider(currentProgress01, false);
        }


        private void SubscribeOnCollectBomb()
        {
            collectItemEvent.Subscribe(item =>
            {
                if (item.Group is ItemGroup.Damage)
                {
                    characterView.PlayDeathVFX(deathEvent.ExecuteWithBomb);
                }
            }).AddTo(disposable);
        }


        private void SubscribeOnChangeLevel()
        {
            levelProgressChangeEvent.Subscribe(LevelProgressChangeEventHandle).AddTo(disposable);
        }


        private void InitializeCharacterData(int level)
        {
            if (!characterConfig.TryGetDataByLevel(level, out var newData))
            {
                newData = maxLevelConfigData;
            }

            currentConfigData = newData;
        }


        private void UpdateCharacterView()
        {
            var speed = superSpeedController.IsSuperSpeedActive ? currentConfigData.superSpeed : currentConfigData.moveSpeed;
            characterView.SetScale(currentConfigData.scale);
            moveController.SetMoveSpeed(speed * systemSettingsConfig.SettingsData.holeSpeedMultiplier);
            characterView.SetSizeText($"{sizeLocalizedText} {size.ToString()}");
            characterView.SetActiveSuperSpeedFX(superSpeedController.IsSuperSpeedActive);
            characterView.SetCharacterViewType(characterViewType);
        }


        private void LevelProgressChangeEventHandle(LevelProgressArgs args)
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

            if (args.source is Source.ExpLevelUp)
            {
                size = args.levelByCurrentXp;
                characterView.PlayLevelUpFx();
            }

            else if (args.source is Source.SizeBooster)
            {
                if (args.type is ActionType.Add)
                {
                    characterView.PlaySizeBoosterFx();
                    characterViewType = CharacterViewType.Boosted;
                }

                else if (args.type is ActionType.Remove)
                {
                    characterViewType = superSpeedController.IsSuperSpeedActive ? CharacterViewType.SuperSpeed : CharacterViewType.Base;
                }
            }

            InitializeCharacterData(newLevel);
            UpdateCharacterView();
        }


        private void ExperienceAddRequestHandle(ExperienceArgs args)
        {
            if (args.source is Source.BoostBottle)
            {
                characterView.PlayBoostBottleFx();
            }
        }
    }
}
