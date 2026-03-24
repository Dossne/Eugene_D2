using Cysharp.Threading.Tasks;
using Features.Collectables;
using Features.Level;
using Features.LevelConfiguration;
using Infrastructure.AssetManagement;
using Infrastructure.Configs;
using System.Collections.Generic;
using System.Threading;
using Features.LevelSessionStateControl;
using UnityEngine;

namespace Features.ExtraItemSpawn
{
    public class ExtraItemsSpawner : ILevelSessionSavable
    {
        private readonly Transform levelRoot;
        private readonly Instantiator instantiator;
        private readonly LevelService levelService;
        private readonly AssetProvider assetProvider;
        private readonly ObjectSpawnConfig objectSpawnConfig;
        private readonly LevelCreateManager levelCreateManager;
        private readonly List<CollectableItem> extraItems = new();

        private List<CollectableData> prevSessionExtraItems;
        private bool isRestoreSession;


        public ExtraItemsSpawner(
            Transform levelRoot,
            Instantiator instantiator,
            LevelService levelService,
            AssetProvider assetProvider,
            ConfigProvider configProvider,
            LevelCreateManager levelCreateManager)
        {
            this.levelRoot = levelRoot;
            this.instantiator = instantiator;
            this.levelService = levelService;
            this.assetProvider = assetProvider;
            this.levelCreateManager = levelCreateManager;

            this.objectSpawnConfig = configProvider.ObjectSpawnConfig;
        }


        public void RestoreSessionState(LevelSessionData sessionData)
        {
            prevSessionExtraItems = sessionData.extraItems;
            isRestoreSession = true;
        }


        public void SaveSessionState(LevelSessionData sessionData)
        {
            sessionData.extraItems ??= new List<CollectableData>();
            
            sessionData.extraItems.Clear();

            foreach (var item in extraItems)
            {
                if (item.IsCollected || !item.IsActive)
                    continue;

                sessionData.extraItems.Add(item.GetDataExtended());
            }
        }


        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            extraItems.Clear();

            return CanRestorePrevSession()
                ? CreateFromPrevSessionAsync(cancellationToken)
                : CreateFromConfigAsync(cancellationToken);
        }


        public void Deinitialize()
        {
            
        }


        public List<CollectableItem> GetItems()
        {
            return extraItems;
        }


        private async UniTask CreateFromPrevSessionAsync(CancellationToken cancellationToken)
        {
            for (int i = 0; i < prevSessionExtraItems.Count; ++i)
            {
                CollectableData data = prevSessionExtraItems[i];
                GameObject prefabGo = await assetProvider.AddressableLoadAssetAsync<GameObject>(data.collectableType.ToString(), cancellationToken);
                CollectableItem prefab = prefabGo.GetComponent<CollectableItem>();
                CollectableItem instance = instantiator.Instantiate(prefab, parent: levelRoot);
                instance.Construct(data.collectableType, data.position.ToVector3(), data.rotation.ToQuaternion(), data.scale.ToVector3());
                extraItems.Add(instance);
            }
        }


        private async UniTask CreateFromConfigAsync(CancellationToken cancellationToken)
        {
            LevelTableView levelTableView = levelCreateManager.GetTable();

            for (int i = 0; i < objectSpawnConfig.ObjectSpawnDatas.Count; ++i)
            {
                ObjectSpawnData objectSpawnData = objectSpawnConfig.ObjectSpawnDatas[i];

                if (levelService.CurrentLevelNumber < objectSpawnData.unlockLevel)
                    continue;

                GameObject prefabGo = await assetProvider.AddressableLoadAssetAsync<GameObject>(objectSpawnData.collectableType.ToString(), cancellationToken);
                CollectableItem prefab = prefabGo.GetComponent<CollectableItem>();

                for (int j = 0; j < objectSpawnData.count; ++j)
                {
                    Vector3 spawnPoint = levelTableView.GetRandomPoint();
                    spawnPoint.y = objectSpawnData.yPosition;
                    float scale = Random.Range(objectSpawnData.minScale, objectSpawnData.maxScale);
                    CollectableItem instance = instantiator.Instantiate(prefab, parent: levelRoot);
                    instance.Construct(objectSpawnData.collectableType, spawnPoint, Quaternion.identity, new Vector3(scale, scale, scale));
                    extraItems.Add(instance);
                }
            }
        }


        private bool CanRestorePrevSession()
        {
            return isRestoreSession && prevSessionExtraItems != null;
        }
    }
}