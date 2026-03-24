using System;
using System.Collections.Generic;
using Features.LevelConfiguration;
using Features.LevelSequence;
using Infrastructure.Configuration;
using Infrastructure.Utilities;
using UnityEditor;
using UnityEngine;

namespace LevelEditor
{
    [Serializable]
    public class RenameItem
    {
        public string oldId;
        public string newId;
    }

    [Serializable]
    public class RenameItemConfig
    {
        public List<RenameItem> datas;
    }

    [CreateAssetMenu(fileName = "LevelIdRenamer", menuName = "Config/Tools/LevelIdRenamer")]
    public class LevelIdRenamer : JsonConvertableConfig
    {
        [SerializeField] private RenameItemConfig configData;
        [SerializeField] private string configPath = "Assets/_Project/Develop/Runtime/Features";
        [SerializeField] private string levelPrefabPath = "Assets/_Project/Develop/Editor/Level";

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "LevelIdRenamer";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (RenameItemConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(RenameItemConfig);

        private List<RenameItem> Items => configData.datas;

        private bool inProgress = false;
#if UNITY_EDITOR

        public void Rename()
        {
            if (inProgress)
            {
                Debug.LogWarning("[CODE] In progress. Please wait");
                return;
            }

            inProgress = true;
            LevelConfig levelConfig = Utils.SearchItemInAssetdatabase_Editor<LevelConfig>(nameof(LevelConfig), configPath);

            if (levelConfig == null)
            {
                Debug.LogError($"[CODE] {nameof(LevelConfig)} not found by path {configPath}. Call devs");
                return;
            }

            LevelSequenceConfig levelSequenceConfig =
                Utils.SearchItemInAssetdatabase_Editor<LevelSequenceConfig>(nameof(LevelSequenceConfig), configPath);
            if (levelConfig == null)
            {
                Debug.LogError($"[CODE] {nameof(LevelSequenceConfig)} not found by path {configPath}. Call devs");
                return;
            }

            List<LevelSequenceData> sequences = new();

            foreach (var renameItem in Items)
            {
                GameLevelProvider levelPrefabOld = Utils.SearchItemInAssetdatabase_Editor<GameLevelProvider>(renameItem.oldId, levelPrefabPath);

                if (levelPrefabOld == null)
                {
                    continue;
                }
                
                string asstPath = AssetDatabase.GetAssetPath(levelPrefabOld);
                var levelProvGO = PrefabUtility.LoadPrefabContents(asstPath);
                var prov = levelProvGO.GetComponent<GameLevelProvider>();
                prov.SetId(renameItem.newId);
                PrefabUtility.SaveAsPrefabAsset(levelProvGO, asstPath);
                PrefabUtility.UnloadPrefabContents(levelProvGO);
                
                AssetDatabase.RenameAsset(asstPath, renameItem.newId);
                
                string levelConfigResult = "LevelConfig not renamed";

                if (levelConfig.TryGetLevelById(renameItem.oldId, out LevelData levelData))
                {
                    levelData.id = renameItem.newId;
                    levelConfigResult = "LevelConfig renamed";
                }
                sequences.Clear();
                levelSequenceConfig.GetLevelById_Editor(renameItem.oldId, sequences);
                string levelSeqResult = "LevelSequence not renamed";

                for (var i = 0; i < sequences.Count; i++)
                {
                    var sequenceData = sequences[i];
                    sequenceData.levelId = renameItem.newId;

                    if (i == 0)
                    {
                        levelSeqResult = "LevelSequence renamed";
                    }
                }

                Debug.Log($"[CODE] Prefab {renameItem.oldId} renamed successful to {renameItem.newId}. {levelConfigResult}. {levelSeqResult} ");
                
                EditorUtility.SetDirty(levelSequenceConfig);
                EditorUtility.SetDirty(levelConfig);

                AssetDatabase.SaveAssets();
            }

            inProgress = false;
        }

        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(Items), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }

        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<RenameItem>(tableData);
        }
#endif
    }
}