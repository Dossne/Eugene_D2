using System;
using System.Collections.Generic;
using Features.Collectables;
using Features.LevelConfiguration;
using Infrastructure.Utilities;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace LevelEditor
{
    [CreateAssetMenu(fileName = "LevelInserterBatched", menuName = "Config/Tools/LevelInserterBatched")]
    public class LevelInserterBatched : ScriptableObject
    {
        [SerializeField] private List<GameLevelProvider> lvls;
        [SerializeField] private string prefabPath;
        [SerializeField] private LevelTableSkinConfiguration levelTableSkinConfiguration;


        [TriInspector.Button]
        private void ExportToLSO()
        {
            foreach (var item in lvls)
            {
                item.ExportToLSO();
            }
        }


        [TriInspector.Button]
        private void ClearList()
        {
            lvls.Clear();
            EditorUtility.SetDirty(this);

        }


        [TriInspector.Button]
        public void InsertLSOAndLevelConfigIntoPrefabs()
        {
            string path = "Assets/_Project/Develop/Runtime/Features";
            LevelConfig levelConfig = Utils.SearchItemInAssetdatabase_Editor<LevelConfig>("LevelConfig", path);
            if (levelConfig == null)
            {
                Debug.LogError($"[CODE] {nameof(LevelConfig)} not found by path {path}. Call devs");
                return;
            }
            InsertByConfig(levelConfig);
            InsertByAddressable(levelConfig);
        }

        private void InsertByAddressable(LevelConfig levelConfig)
        {
            List<LevelData> levelDataList = new List<LevelData>();

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings not found.");
                return;
            }
            AddressableAssetGroup group = settings.FindGroup(levelConfig.LevelAddressableGroup);
            if (group == null)
            {
                Debug.LogError("AddressableAssetGroup not found.");
                return;
            }

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
                return;
            }

            if (lvls.Count > 0)
                InsertConfigToPrefabList(levelDataList);
            else
                InsertConfigToPrefabByPath(levelDataList);
        }

        private void InsertByConfig(LevelConfig levelConfig)
        {
            if (lvls.Count > 0)
                InsertConfigToPrefabList(levelConfig.Items.FindAll(x => lvls.Exists(y => y.LevelId == x.id)));
            else
                InsertConfigToPrefabByPath(levelConfig.Items);
        }

        private void InsertConfigToPrefabList(List<LevelData> levelDataList)
        {
            Debug.Log($"[CODE] List {nameof(lvls)} is not empty. Setting config to {nameof(lvls)}");

            for (int i = 0; i < lvls.Count; i++)
            {
                GameLevelProvider original = lvls[i];
                LevelData configItem = levelDataList.Find(x => x.id == original.LevelId);
                if (configItem == null)
                {
                    Debug.LogError($"[CODE] Level {original.LevelId} not found!");
                    continue;
                }

                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(original.gameObject);
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogError($"[CODE] Could not get prefab path for {original.LevelId}");
                    continue;
                }

                GameObject prefabInstance = PrefabUtility.LoadPrefabContents(path);
                GameLevelProvider provider = prefabInstance.GetComponent<GameLevelProvider>();
                
                if (provider == null)
                {
                    Debug.LogError($"[CODE] Prefab {original.name} does not contain {nameof(GameLevelProvider)}");
                    PrefabUtility.UnloadPrefabContents(prefabInstance);
                    continue;
                }

                List<CollectableType> tasks =
                    JsonConvert.DeserializeObject<List<CollectableType>>(configItem.taskJson, JsonUtils.SerializerSettings);
                
                provider.SetData(tasks, configItem.time, configItem.difficulty, configItem.defaultSkin);

                LevelTableView tableView = provider.GetComponentInChildren<LevelTableView>(true);
                if (tableView == null)
                    Debug.LogError($"[LevelDesign] Id: {configItem.id}. Table prefab not found. {nameof(InsertConfigToPrefabByPath)} is failed", this);
                else
                    tableView.SetSkin(configItem.defaultSkin, levelTableSkinConfiguration.GetLevelTableSkinData(configItem.defaultSkin));

                PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
                PrefabUtility.UnloadPrefabContents(prefabInstance);

                Debug.Log($"[CODE] Updated level prefab '{configItem.id}' successfully");
            }
        }


        private void InsertConfigToPrefabByPath(List<LevelData> levelDataList)
        {
            Debug.Log($"[CODE] Setting config in prefabs by path: {prefabPath}");

            string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { prefabPath });

            for (int i = 0; i < levelDataList.Count; i++)
            {
                LevelData configItem = levelDataList[i];

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
                
                        provider.SetData(tasks, configItem.time, configItem.difficulty, configItem.defaultSkin);

                        LevelTableView tableView = editableProvider.GetComponentInChildren<LevelTableView>(true);
                        if (tableView == null)
                            Debug.LogError($"[LevelDesign] Id: {configItem.id}. Table prefab not found. {nameof(InsertConfigToPrefabByPath)} is failed", this);
                        else
                            tableView.SetSkin(configItem.defaultSkin, levelTableSkinConfiguration.GetLevelTableSkinData(configItem.defaultSkin));

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
    }
}