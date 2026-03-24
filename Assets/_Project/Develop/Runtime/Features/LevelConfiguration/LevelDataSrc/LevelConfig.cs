using System;
using System.Collections.Generic;
using Features.LevelComplete;
using Infrastructure.Ads;
using Infrastructure.Configuration;
using Newtonsoft.Json;
using UnityEngine;

namespace Features.LevelConfiguration
{
    [Serializable]
    public class LevelData
    {
        [JsonProperty("i")]  public string id;
        [JsonProperty("l")]  public string levelJson;
        [JsonProperty("t")]  public string taskJson;
        [JsonProperty("s")]  public float time;
        [JsonProperty("d")]  public LevelDifficulty difficulty;
        [JsonProperty("ts")] public LevelTableSkin defaultSkin;
    }

    [Serializable]
    public class LevelDataConfig
    {
        public List<LevelData> datas;
    }

    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Config/Game/LevelConfig")]
    public class LevelConfig : JsonConvertableConfig
    {
        [SerializeField] private string levelAddressableLabel = "Levels";
        [SerializeField] private string levelAddressableGroup = "Levels";
        [SerializeField] private LevelDataConfig configData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "LevelConfig";
        
        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (LevelDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(LevelDataConfig);

        public List<LevelData> Items => configData.datas;
        public string LevelAddressableLabel => levelAddressableLabel;
        public string LevelAddressableGroup => levelAddressableGroup;



        public (LevelData levelData, Level level) GetLevelById(string id)
        {
            LevelData levelData = null;
            Level level = null;
            
            for (var i = 0; i < configData.datas.Count; i++)
            {
                var data = configData.datas[i];

                if (data.id != id) 
                    continue;
                
                if (LevelUtil.TryGetLevel(id, data.levelJson, out level))
                {
                    levelData = data;
                    break;
                }
            }

            if(level == null
                || levelData == null)
            {
                levelData = configData.datas[0];
                LevelUtil.TryGetLevel(levelData.id, levelData.levelJson, out level);
                string errorMessage = $"Level <b>{id}</b> is not found. Level set to fallback. Everything went ok. Please, check <b>LevelConfig</b>";
                Debug.LogError(errorMessage);
                AnalyticSender.SendEvent("vortex_unity_exception", errorMessage);
            }

            return (levelData, level);
        }

        public bool HasLevel(string id)
        {
            return configData.datas.Exists(x => x.id == id);
        }

        public bool TryGetLevelById(string id, out LevelData result)
        {
            result = null;
            for (var i = 0; i < configData.datas.Count; i++)
            {
                var data = configData.datas[i];
                if (data.id == id)
                {
                    result = data;
                    return true;
                }
            }

            return false;
        }
        
#if UNITY_EDITOR

        public void Insert_Editor(LevelData item)
        {
            bool inserted = false;
            for (var i = 0; i < Items.Count; i++)
            {
                if (Items[i].id == item.id)
                {
                    Items[i] = item;
                    inserted = true;
                    Debug.Log($"[LevelDesign] {item.id} updated successfully", this);
                    break;
                }
            }

            if (!inserted)
            {
                Items.Add(item);
                Debug.Log($"[LevelDesign] {item.id} inserted successfully as new", this);
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }

        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(Items), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }

        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<LevelData>(tableData);
        }


        public void ValidateData_Editor()
        {
            Debug.Log("[LevelConfig] Begin validate");

            foreach (var data in Items)
            {
                LevelUtil.TryGetLevel(data.id, data.levelJson, out _);
                LevelUtil.TryGetTasks(data.id, data.taskJson, out _);
            }
            
            Debug.Log("[LevelConfig] Finished validate");
        }        
#endif
    }
}