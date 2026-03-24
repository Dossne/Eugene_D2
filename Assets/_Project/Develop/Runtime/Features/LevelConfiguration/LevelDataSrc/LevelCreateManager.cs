using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Collectables;
using Features.Level;
using Features.LevelSessionStateControl;
using Infrastructure.AssetManagement;
using Infrastructure.Configs;
using Infrastructure.Utilities;
using Newtonsoft.Json;
using UnityEngine;

namespace Features.LevelConfiguration
{
    /// <summary>
    /// Create scene levels from prefabs
    /// </summary>
    public class LevelCreateManager : ILevelSessionSavable
    {
        private readonly Transform levelRoot;
        private readonly Instantiator instantiator;
        private readonly AssetProvider assetProvider;
        private readonly LevelService levelService;
        private readonly Dictionary<CollectableType, CollectableItem> cachedPfs = new();

        private List<CollectableItem> items;
        private readonly LevelTableSkinConfiguration levelTableSkinConfiguration;
        private readonly CollectablesConfig collectablesConfig;
        private readonly List<LevelTableSkin> levelTableSkins = new();
        private Level currentLevel;
        private LevelTableSkin currentSkin;
        private LevelTableView tableView;

        private Level prevSessionLevel;
        private LevelTableSkin prevSkin;
        private bool isRestoreSession;

        private bool isInit;


        public LevelCreateManager(Transform levelRoot,
                                  Instantiator instantiator,
                                  AssetProvider assetProvider,
                                  LevelService levelService,
                                  ConfigProvider configProvider)
        {
            this.levelRoot = levelRoot;
            this.instantiator = instantiator;
            this.assetProvider = assetProvider;
            this.levelService = levelService;
            this.levelTableSkinConfiguration = configProvider.LevelTableSkinConfiguration;
            this.collectablesConfig = configProvider.CollectablesConfig;
            levelTableSkins = levelTableSkinConfiguration.GetSkins();
            levelTableSkins.Remove(LevelTableSkin.Default);
        }


        void ILevelSessionSavable.RestoreSessionState(LevelSessionData sessionData)
        {
            prevSessionLevel = sessionData.level;
            prevSkin = sessionData.skin;
            isRestoreSession = true;
        }


        void ILevelSessionSavable.SaveSessionState(LevelSessionData sessionData)
        {
            sessionData.level ??= new Level();

            sessionData.level.collectables ??= new List<CollectableData>();
            sessionData.level.collectables.Clear();
            
            foreach (var item in items)
            {
                if (item.IsCollected || !item.IsActive)
                    continue;

                sessionData.level.collectables.Add(item.GetData());
            }

            sessionData.level.table = currentLevel.table;
            sessionData.level.spawnPointPosition = currentLevel.spawnPointPosition;
            sessionData.skin = currentSkin;
        }


        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (isInit)
                return;

            if (CanRestorePrevSession())
                RestoreLevelFromPrevSession();
            else
                CreateLevelByConfig();

            LevelTableData tableData = currentLevel.table;
            Vector3 tablePos = tableData.position.ToVector3();
            
            tableView = await instantiator.InstantiateAsync<LevelTableView>(tableData.prefabName, tablePos,
                                                                            tableData.rotation.ToQuaternion(),
                                                                            levelRoot, 
                                                                            cancellationToken: cancellationToken);
            tableView.Initialize();
            tableView.SetScale(tableData.scale.ToVector3());
            tableView.SetSkin(currentSkin, levelTableSkinConfiguration.GetLevelTableSkinData(currentSkin));

            items = new List<CollectableItem>();
            SerializableVector3 defaultScale = new SerializableVector3(Vector3.one); // default for all collectables

            for (int i = 0; i < currentLevel.collectables.Count; i++)
            {
                CollectableData configItem = currentLevel.collectables[i];
                if (!cachedPfs.TryGetValue(configItem.collectableType, out CollectableItem prefab))
                {
                    GameObject prefabGo =
                        await assetProvider.AddressableLoadAssetAsync<GameObject>(configItem.collectableType.ToString(), cancellationToken);
                    prefab = prefabGo.GetComponent<CollectableItem>();
                    cachedPfs.Add(configItem.collectableType, prefab);
                }

                CollectableItem instance = instantiator.Instantiate(prefab, parent: levelRoot);
                configItem.scale = defaultScale * collectablesConfig.Get(configItem.collectableType).defaultScaleOverride;
                instance.Construct(configItem.collectableType, configItem.position.ToVector3(), configItem.rotation.ToQuaternion(), configItem.scale.ToVector3());
                items.Add(instance);

                if (isRestoreSession && configItem.position.y < tablePos.y)
                {
                    instance.PhysicsWakeUp();
                }
            }

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            items.Clear();
            cachedPfs.Clear();

            isInit = false;
        }


        public (Transform root, Vector3 pos) GetCharacterSpawnPoint()
        {
            return (levelRoot, currentLevel.spawnPointPosition.ToVector3());
        }


        public List<CollectableItem> GetItems()
        {
            return items;
        }


        public LevelTableView GetTable()
        {
            return tableView;
        }


        public Transform GetLevelRoot()
        {
            return levelRoot;
        }


        private bool CanRestorePrevSession()
        {
            return isRestoreSession && prevSessionLevel != null;
        }


        private void CreateLevelByConfig()
        {
            string compressedJson = levelService.GetCurrentLevelData().levelJson;
            string rawJson = StringUtils.DecompressString(compressedJson);
            currentLevel = JsonConvert.DeserializeObject<Level>(rawJson, JsonUtils.SerializerSettings);
            currentSkin = levelTableSkins.Count == 0 || !levelService.IsSequenceOff ? levelService.GetCurrentLevelData().defaultSkin : levelTableSkins[Random.Range(0, levelTableSkins.Count)];
        }


        private void RestoreLevelFromPrevSession()
        {
            currentLevel = prevSessionLevel;
            currentSkin = prevSkin;
        }
    }
}