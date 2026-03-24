#if UNITY_EDITOR
using System.Collections.Generic;
using Features.Collectables;
using Infrastructure.Utilities;
using Newtonsoft.Json;
using TriInspector;
using System.Linq;
using Features.LevelComplete;
using UnityEditor;
using System.IO;
#endif

using System;
using UnityEngine;

namespace Features.LevelConfiguration
{
    public class GameLevelProvider : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private Transform characterSpawnPoint;
        [SerializeField] private CollectablesContainer collectablesContainer;

        [Title("LevelDesign")]
        [SerializeField, ReadOnly] private string levelId;
        [SerializeField, Min(0)] private float time;
        [SerializeField] public LevelDifficulty difficulty;
        [SerializeField] private LevelTableSkin levelTableSkin;
        [SerializeField] private List<CollectableType> tasks;
        [SerializeField, ReadOnly] private List<CollectableType> levelItems;
        [SerializeField] private bool copyToLevelConfigOnExportToLSO = false;

        [Title("Debug")]
        [SerializeField] private bool showDebug;
        [ShowIf(nameof(showDebug))] [SerializeField, TextArea(1, 10), ReadOnly] private string levelJson;
        [ShowIf(nameof(showDebug))] [SerializeField, TextArea(1, 10), ReadOnly] private string tasksJson;
        [ShowIf(nameof(showDebug))] [SerializeField, ReadOnly] private Level levelData;
        [ShowIf(nameof(showDebug))] [SerializeField, TextArea(1, 10)] private string rawLevelJson;

        public string LevelId => levelId;


        private void OnValidate()
        {
            levelId = gameObject.name;
        }


        [Button]
        public bool InitLevelData()
        {
            if (string.IsNullOrEmpty(levelId))
                levelId = gameObject.name;

            levelData = new Level();
            CollectablesContainer container = GetComponentInChildren<CollectablesContainer>(true);

            container.Initialize();
            IReadOnlyList<CollectableItem> itemList = container.ObjectList;

            levelData.collectables = new List<CollectableData>();

            foreach (var item in itemList)
            {
                levelData.collectables.Add(item.GetData());
            }

            levelData.table = new LevelTableData();

            LevelTableView tableView = GetComponentInChildren<LevelTableView>(true);
            if (tableView == null)
            {
                Debug.LogError($"[LevelDesign] Id: {levelId}. Table prefab not found. {nameof(InitLevelData)} is failed", this);
                return false;
            }

            levelData.table.prefabName = tableView.GetAssetName();
            levelData.table.position = new SerializableVector3(tableView.transform.position);
            levelData.table.rotation = new SerializableVector3(tableView.transform.rotation);
            levelData.table.scale = new SerializableVector3(tableView.transform.localScale);

            levelData.spawnPointPosition = new SerializableVector3(characterSpawnPoint.position);

            InitializeAllItems();
            rawLevelJson = JsonConvert.SerializeObject(levelData, JsonUtils.SerializerSettings);
            levelJson = StringUtils.CompressString(rawLevelJson);
            tasksJson = JsonConvert.SerializeObject(tasks, JsonUtils.SerializerSettings);
            return true;
        }


        public void SetId(string value)
        {
            levelId = value;
        }


        public void SetData(List<CollectableType> tasks, float time, LevelDifficulty difficulty, LevelTableSkin levelTableSkin)
        {
            this.tasks = tasks;
            this.time = time;
            this.difficulty = difficulty;
            this.levelTableSkin = levelTableSkin;
        }


        private LevelData GetLevelData()
        {
            return new LevelData { id = levelId, levelJson = levelJson, taskJson = tasksJson, time = time, difficulty = difficulty, defaultSkin = levelTableSkin };
        }


        private bool CheckTasks(List<CollectableType> tasksToCheck)
        {
            if (tasksToCheck == null || tasksToCheck.Count == 0)
            {
                Debug.LogError($"[LevelDesign] LevelId: {levelId} not contains tasks");
                return false;
            }

            List<CollectableType> noItems = new List<CollectableType>();
            foreach (var taskItem in tasksToCheck)
            {
                if (!levelItems.Contains(taskItem))
                {
                    noItems.Add(taskItem);
                }
            }

            List<CollectableType> duplicates = tasksToCheck
                                              .GroupBy(x => x)
                                              .Where(g => g.Count() > 1)
                                              .Select(g => g.Key)
                                              .ToList();

            string noItemsStr = noItems.Count > 0 ? $" No items in level: {string.Join(';', noItems)}." : null;
            string duplicatesStr = duplicates.Count > 0 ? $" *** Duplicates: {string.Join(';', duplicates)}." : null;

            if (duplicates.Count > 0 || noItems.Count > 0)
            {
                Debug.LogError($"[LevelDesign] LevelId: {levelId}.{noItemsStr}{duplicatesStr}");
            }

            return duplicates.Count == 0 && noItems.Count == 0;
        }


        private void InitializeAllItems()
        {
            levelItems.Clear();

            for (int i = 0; i < levelData.collectables.Count; i++)
            {
                var type = levelData.collectables[i].collectableType;

                if (!levelItems.Contains(type))
                {
                    levelItems.Add(type);
                }
            }
        }


        [Button]
        private bool CheckTasks()
        {
            return CheckTasks(tasks);
        }


        [Button]
        public void ExportToLSO()
        {
            if (time <= 0)
            {
                Debug.LogError($"[LevelDesign] LevelId {levelId} not inserted. Time must be greater than 0");
                return;
            }

            if (!InitLevelData())
            {
                return;
            }

            if (!CheckTasks())
            {
                Debug.LogError($"[LevelDesign] LevelId {levelId} not inserted. Have errors with tasks");
                return;
            }

            string path = "Assets/_Project/Develop/Runtime/Features";

            LevelConfig config = Utils.SearchItemInAssetdatabase_Editor<LevelConfig>("LevelConfig", path);

            if (config == null)
            {
                Debug.LogError($"[CODE] LevelId {levelId} not inserted. {nameof(LevelConfig)} not found by path {path}. Call devs");
                return;
            }

            var data = GetLevelData();
            if (copyToLevelConfigOnExportToLSO)
                config.Insert_Editor(data);
            SaveAsLevelScriptableObject(data);
        }

        private void SaveAsLevelScriptableObject(LevelData data) 
        {
            var path = "Assets/_Project/Develop/Runtime/Features/LevelConfiguration/LevelDataCfg/Levels";
            var assetPath = $"{path}/{data.id}.asset";
            if (File.Exists(assetPath))
            {
                LevelScriptableObject dataSO = AssetDatabase.LoadAssetAtPath<LevelScriptableObject>(assetPath);
                Debug.LogWarning($"Asset already exists at {assetPath}. LevelScriptableObject will be overwritten.");
                dataSO.FromLevelData(data);
                EditorUtility.SetDirty(dataSO);
            }
            else
            {
                LevelScriptableObject dataSO = ScriptableObject.CreateInstance<LevelScriptableObject>();
                Debug.LogWarning($"New LevelScriptableObject will be created.");
                dataSO.FromLevelData(data);
                AssetDatabase.CreateAsset(dataSO, assetPath);
            }
            AssetDatabase.SaveAssets();
        }

        [Button]
        private void ClearAll()
        {
            rawLevelJson = null;
            levelJson = null;
            levelItems.Clear();
            levelData = null;
            tasksJson = null;
        }


        private void MigrateFromOldConfigs(List<CollectableType> tasks, float time, LevelDifficulty difficulty)
        {
            bool changed = false;

            if (tasks.Count > 0)
            {
                bool tasksEqual = this.tasks.OrderBy(x => x).SequenceEqual(tasks.OrderBy(x => x));

                if (!tasksEqual)
                {
                    this.tasks = new List<CollectableType>(tasks);
                    changed = true;
                }
            }

            if (time > 0)
            {
                this.time = time;
                changed = true;
            }

            if (this.difficulty != difficulty)
            {
                this.difficulty = difficulty;
                changed = true;
            }

            if (changed)
                UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}