using Cysharp.Threading.Tasks;
using Features.Collectables;
using Features.LevelConfiguration;
using Infrastructure.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

#endif
using UnityEngine;



namespace LevelEditor
{
    [CreateAssetMenu(fileName = "LevelCollectableReplacerBatched", menuName = "Config/Tools/LevelCollectableReplacerBatched")]
    public class LevelCollectableReplacerBatched : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private List<GameLevelProvider> lvls = new();
        [SerializeField] private string prefabPath;
        [SerializeField] private LevelConfig config;
        [SerializeField] private List<ReplaceData> replaceDataList = new();
        [SerializeField] private List<GameLevelProvider> affectedLevelPrefabs = new();
        private List<string> affectedLevels = new();        

        //private List<LevelData> levelDataList = new();
        CancellationTokenSource cancellationTokenSource = new();
        private List<ObjectData> cachedData = new();

        private void OnEnable()
        {
            cancellationTokenSource = new();
        }

        private void OnDisable() 
        {
            if (cancellationTokenSource.Token.CanBeCanceled)
                cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        [TriInspector.Button]
        private async void ReplaceItems()
        {
            var startTime = System.DateTime.Now;
            var endTime = System.DateTime.Now;
            replaceDataList.RemoveAll(x => x.fromItemObject == null || x.toItemObject == null);
            cancellationTokenSource = new();
            affectedLevels.Clear();
            affectedLevelPrefabs.Clear();
            try
            {
                await UniTask.RunOnThreadPool(ReplaceItemsImpl, cancellationToken: cancellationTokenSource.Token);
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning($"[CODE] Operation was cancelled: {e.Message}");
                endTime = System.DateTime.Now;
                Debug.Log($"[CODE] Time: {endTime - startTime}");
                return;
            }            
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            endTime = System.DateTime.Now;
            Debug.Log($"[CODE] Time: {endTime - startTime}");
        }


        [TriInspector.Button]
        private void CancelTask()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        private void ReplaceItemsImpl()
        {
            if (replaceDataList.Count < 1)
            {
                Debug.LogError("[CODE] Nothing to replace");
                affectedLevels.Clear();
                return;
            }
                                    
            ReplaceByConfig();
            ReplaceByAddressable();

            return;
        }

        private void ReplaceByConfig()
        {
            affectedLevels.Clear();
            List<LevelData> levelDataList = new List<LevelData>();
            if (lvls.Count > 0)
                levelDataList = config.Items.FindAll(x => lvls.Exists(y => y.LevelId == x.id));
            else
                levelDataList = config.Items;
            Replace(levelDataList);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            InsertConfigToPrefabByPath(levelDataList, affectedLevels, prefabPath);
        }

        private void ReplaceByAddressable()
        {
            affectedLevels.Clear();
            List<LevelData> levelDataList = new List<LevelData>();
            
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings not found.");
                return;
            }
            AddressableAssetGroup group = settings.FindGroup(config.LevelAddressableGroup);
            if (group == null)
            {
                Debug.LogError("AddressableAssetGroup not found.");
                return;
            }

            List<LevelScriptableObject> addressableDataList = new();
                 
            foreach (var entry in group.entries)
            {
                if (lvls.Count > 0 && !lvls.Exists(y => y.LevelId == entry.address))
                    continue;

                string path = AssetDatabase.GUIDToAssetPath(entry.guid);
                LevelScriptableObject asset = AssetDatabase.LoadAssetAtPath<LevelScriptableObject>(path);
                if (asset == null)
                {
                    Debug.LogError($"LevelScriptableObject with guid {entry.guid} is null.");
                    continue;
                }

                levelDataList.Add(asset.ToLevelData());
                addressableDataList.Add(asset);
                return;
            }

            Replace(levelDataList);

            for (int i = 0; i < levelDataList.Count; i++)
            {
                addressableDataList[i].FromLevelData(levelDataList[i]);
                EditorUtility.SetDirty(addressableDataList[i]);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            InsertConfigToPrefabByPath(levelDataList, affectedLevels, prefabPath);
        }


        private void Replace(List<LevelData> levelDataList) 
        {
            int levelCount = 0;
            int collectableCount = 0;

            for (int i = 0; i < levelDataList.Count; i++)
            {
                var currentLevelData = levelDataList[i];
                Debug.Log($"[CODE] Level: {currentLevelData.id}. ({i + 1}/{levelDataList.Count})");

                string rawJson = StringUtils.DecompressString(currentLevelData.levelJson);
                var currentLevel = JsonConvert.DeserializeObject<Level>(rawJson, JsonUtils.SerializerSettings);

                var froms = replaceDataList.Select(x => x.fromItemObject.CollectableType).ToList();

                if (!currentLevel.collectables.Exists(x => froms.Contains(x.collectableType)))
                {
                    Debug.Log($"[CODE] Level: {currentLevelData.id}. Collectables from list not found.");
                    continue;
                }

                affectedLevels.Add(currentLevelData.id);
                levelCount++;

                List<CollectableType> tasks =
                            JsonConvert.DeserializeObject<List<CollectableType>>(currentLevelData.taskJson, JsonUtils.SerializerSettings);


                for (int replaceIndex = 0; replaceIndex < replaceDataList.Count; replaceIndex++)
                {
                    if (!tasks.Contains(replaceDataList[replaceIndex].fromItemObject.CollectableType))
                        continue;

                    tasks.Remove(replaceDataList[replaceIndex].fromItemObject.CollectableType);
                    if (!tasks.Contains(replaceDataList[replaceIndex].toItemObject.CollectableType))
                        tasks.Add(replaceDataList[replaceIndex].toItemObject.CollectableType);
                }

                currentLevelData.taskJson = JsonConvert.SerializeObject(tasks, JsonUtils.SerializerSettings);

                for (int j = 0; j < currentLevel.collectables.Count; j++)
                {
                    var collectable = currentLevel.collectables[j];
                    var replaceData = replaceDataList.Find(x => x.fromItemObject.CollectableType == collectable.collectableType);
                    if (replaceData == null) continue;

                    if (collectable.collectableType == replaceData.fromItemObject.CollectableType)
                    {
                        collectable.collectableType = replaceData.toItemObject.CollectableType;
                        collectableCount++;
                    }
                }

                var processedJson = JsonConvert.SerializeObject(currentLevel, JsonUtils.SerializerSettings);
                levelDataList[i].levelJson = StringUtils.CompressString(processedJson);
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }




        private void InsertConfigToPrefabByPath(List<LevelData> levelData, List<string> affectedLevels, string prefabPath)
        {
            Debug.Log($"[CODE] Setting config in prefabs by path: {prefabPath}");

            string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { prefabPath });

            var affectedConfigItems = levelData.Where(x => affectedLevels.Contains(x.id)).ToList();

            for (int i = 0; i < affectedConfigItems.Count; i++)
            {
                LevelData configItem = affectedConfigItems[i];

                bool found = false;

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                    if (prefabRoot == null)
                        continue;

                    GameLevelProvider provider = prefabRoot.GetComponent<GameLevelProvider>();

                    if (provider == null || provider.LevelId != configItem.id)
                        continue;

                    GameObject prefabInstance = PrefabUtility.LoadPrefabContents(assetPath);

                    GameLevelProvider editableProvider = prefabInstance.GetComponent<GameLevelProvider>();
                    if (editableProvider != null)
                    {
                        List<CollectableType> tasks =
                            JsonConvert.DeserializeObject<List<CollectableType>>(configItem.taskJson, JsonUtils.SerializerSettings);

                        affectedLevelPrefabs.Add(provider);

                        var collectableContainer = editableProvider.GetComponentInChildren<CollectablesContainer>(true);

                        cachedData.Clear();
                        var collectables = editableProvider.GetComponentsInChildren<CollectableItem>(true);
                        for (int j = 0; j < collectables.Length; j++)
                        {
                            var item = collectables[j];
                            if (item == null) continue;
                            
                            var replaceData = replaceDataList.Find(x => x.fromItemObject.CollectableType == item.CollectableType);
                            if (replaceData == null) continue;

                            cachedData.Add(new ObjectData(replaceData.toItemObject,
                                                          item.transform.position, 
                                                          item.transform.rotation, 
                                                          item.transform.parent));
                            DestroyImmediate(item.gameObject);
                        }

                        for (var index = 0; index < cachedData.Count; index++)
                        {
                            var objectData = cachedData[index];
                            var collectable = (CollectableItem)PrefabUtility.InstantiatePrefab(cachedData[index].toItemPrefab, cachedData[index].parentTransform);
                            collectable.transform.SetPositionAndRotation(objectData.position, objectData.rotation);
                            collectable.name = cachedData[index].toItemPrefab.name + "_" + index;
                        }

                        editableProvider.SetData(tasks, configItem.time, configItem.difficulty, configItem.defaultSkin);
                        collectableContainer.CollectAndRename();
                        editableProvider.InitLevelData();
                        PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath);
                        PrefabUtility.UnloadPrefabContents(prefabInstance);

                        Debug.Log($"[CODE] Updated level prefab '{configItem.id}' successfully");
                        found = true;
                        break;
                    }

                    PrefabUtility.UnloadPrefabContents(prefabInstance);
                }

                if (!found)
                {
                    Debug.LogError($"[CODE] Not found level prefab: {configItem.id}");
                }
            }
        }

        private class ObjectData
        {
            public CollectableItem toItemPrefab;
            public Vector3 position;
            public Quaternion rotation;
            public Transform parentTransform;

            public ObjectData(CollectableItem toItemPrefab, Vector3 position, Quaternion rotation, Transform parentTransform)
            {
                this.toItemPrefab = toItemPrefab;
                this.position = position;
                this.rotation = rotation;
                this.parentTransform = parentTransform;
            }
        }

        [Serializable]
        private class ReplaceData
        {
            public CollectableItem fromItemObject;
            public CollectableItem toItemObject;
        }
#endif
    }
}