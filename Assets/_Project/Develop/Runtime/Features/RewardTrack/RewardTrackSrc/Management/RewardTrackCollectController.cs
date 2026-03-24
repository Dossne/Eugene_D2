using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Collectables;
using Features.Events;
using Features.ExtraCollectableUi;
using Features.LevelConfiguration;
using Features.LevelSessionStateControl;
using Infrastructure.Ads;
using Infrastructure.AssetManagement;
using Infrastructure.Configs;
using R3;
using UnityEngine;

namespace Features.RewardTrack
{
    public class RewardTrackCollectController : ILevelSessionSavable
    {
        private readonly RewardTrackStateController stateController;
        private readonly RewardTrackConfig rewardTrackConfig;
        private readonly Instantiator instantiator;
        private readonly LevelCreateManager levelCreateManager;
        private readonly CollectItemEvent collectItemEvent;
        private readonly CompositeDisposable disposable;
        private readonly CancellationTokenSource cts;

        private readonly ExtraCollectableUIService extraCollectableUIService;
        private readonly List<CollectableItem> specialItems;
        private readonly GameBaseAnalytics analytics;

        private CollectableType targetItemType;
        private int collectedCountRaw;

        private RewardTrackLevelSessionState prevSessionData;
        private bool isRestoreSession;

        private bool isInit;


        public RewardTrackCollectController(ConfigProvider configProvider,
                                            RewardTrackStateController stateController,
                                            Instantiator instantiator,
                                            LevelCreateManager levelCreateManager,
                                            CollectItemEvent collectItemEvent,
                                            ExtraCollectableUIService extraCollectableUIService)
        {
            this.rewardTrackConfig = configProvider.RewardTrackConfig;
            this.stateController = stateController;
            this.instantiator = instantiator;
            this.levelCreateManager = levelCreateManager;
            this.collectItemEvent = collectItemEvent;
            this.extraCollectableUIService = extraCollectableUIService;

            disposable = new CompositeDisposable();
            cts = new CancellationTokenSource();
            specialItems = new List<CollectableItem>();
        }


        public int CollectedCount => collectedCountRaw;


        void ILevelSessionSavable.RestoreSessionState(LevelSessionData sessionData)
        {
            prevSessionData = sessionData.rewardTrack;
            isRestoreSession = true;
        }


        void ILevelSessionSavable.SaveSessionState(LevelSessionData sessionData)
        {
            if (!isInit)
                return;

            sessionData.rewardTrack ??= new RewardTrackLevelSessionState();
            sessionData.rewardTrack.collectedCountRaw = collectedCountRaw;
            sessionData.rewardTrack.targetItemType = targetItemType;

            sessionData.rewardTrack.spawnData ??= new List<CollectableData>();
            sessionData.rewardTrack.spawnData.Clear();

            foreach (var item in specialItems)
            {
                if (item.IsCollected || !item.IsActive)
                    continue;

                sessionData.rewardTrack.spawnData.Add(item.GetDataExtended());
            }
        }


        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (isInit || !IsFeatureActive() || stateController.IsCurrentTrackCompleted())
                return;

            if (CanRestorePrevSession())
                await RestoreFromPrevSessionAsync(cancellationToken);
            else
                await CreateItemsFromConfigAsync(cancellationToken);

            if(!IsTargetItemSet())
                return;
            
            int maxCount = collectedCountRaw + specialItems.Count;

            if (maxCount > 0)
                await extraCollectableUIService.AddSlotAsync(targetItemType, collectedCountRaw, maxCount, cancellationToken);

            if (specialItems.Count > 0)
            {
                collectItemEvent.Subscribe(CollectItemEventHandle).AddTo(disposable);
            }

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            disposable.Dispose();
            cts.Cancel();
            cts.Dispose();

            isInit = false;
        }


        public int GetMultipliedCollectedItems(int multiplier = 1)
        {
            if (!IsFeatureActive())
                return 0;

            int collectedCountMultiplied = collectedCountRaw * multiplier;
            stateController.ApplyCollectedCount(targetItemType, collectedCountMultiplied);
            return collectedCountMultiplied;
        }


        private bool IsFeatureActive()
        {
            return stateController.IsEnabledByConfig() && stateController.IsUnlocked();
        }


        private bool CanRestorePrevSession()
        {
            return isRestoreSession && prevSessionData != null;
        }


        private bool IsTargetItemSet()
        {
            return targetItemType != CollectableType.None;
        }


        private async UniTask RestoreFromPrevSessionAsync(CancellationToken token)
        {
            targetItemType = prevSessionData.targetItemType;
            collectedCountRaw = prevSessionData.collectedCountRaw;
            string assetGuid = targetItemType.ToString();
            Transform levelRoot = levelCreateManager.GetLevelRoot();

            for (int i = 0; i < prevSessionData.spawnData.Count; ++i)
            {
                var data = prevSessionData.spawnData[i];
                CollectableItem special = await instantiator.InstantiateAsync<CollectableItem>(assetGuid, parent: levelRoot, isInstantiateAsync: false, cancellationToken: token);
                special.Construct(data.collectableType, data.position.ToVector3(), data.rotation.ToQuaternion(), data.scale.ToVector3());
                specialItems.Add(special);
            }
        }


        private async UniTask CreateItemsFromConfigAsync(CancellationToken token)
        {
            if (!stateController.TryGetCurrentProgress(out var currentProgress))
            {
                return;
            }

            targetItemType = currentProgress.targetItemType;
            string assetGuid = targetItemType.ToString();
            RewardTrackFeatureData featureConfig = rewardTrackConfig.Feature;
            LevelTableView levelTableView = levelCreateManager.GetTable();
            Transform levelRoot = levelCreateManager.GetLevelRoot();

            for (int i = 0; i < featureConfig.spawnCount; ++i)
            {
                Vector3 spawnPoint = levelTableView.GetRandomPoint();
                spawnPoint.y = featureConfig.spawnYPos;
                float scale = Random.Range(featureConfig.minScale, featureConfig.maxScale);

                CollectableItem special = await instantiator.InstantiateAsync<CollectableItem>(assetGuid, parent: levelRoot, isInstantiateAsync: false, cancellationToken: token);
                special.Construct(targetItemType, spawnPoint, Quaternion.identity, new Vector3(scale, scale, scale));
                specialItems.Add(special);
            }
        }


        private void CollectItemEventHandle(CollectableItem item)
        {
            if (item.Group is not ItemGroup.Special)
                return;

            collectedCountRaw++;
            item.MarkAsCollected();
            extraCollectableUIService.FloatToUi(item.CollectableType, collectedCountRaw, rewardTrackConfig.Feature.spawnCount);
        }
    }
}