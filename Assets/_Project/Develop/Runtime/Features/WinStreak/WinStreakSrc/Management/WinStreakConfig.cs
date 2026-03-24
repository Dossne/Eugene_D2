using System;
using System.Collections.Generic;
using Features.Boosters;
using Infrastructure.Configuration;
using UnityEngine;

namespace Features.WinStreak
{
    [Serializable]
    public class WinStreakFeatureData
    {
        public bool isFeatureEnabled;
        public int unlockLevel;
        public bool isMultiplierEnabled;
        public int rewardMultiplier;
    }
    
    
    [Serializable]
    public class WinStreakData
    {
        public int lvl;
        public string iconName;
    }

    [Serializable]
    public class BoosterByLevelData
    {
        public int lvl;
        public BoosterType boosterType;
        public int itemCount;
    }

    [Serializable]
    public class WinStreakDataConfig
    {
        public List<WinStreakFeatureData> winStreakFeatureData;
        public List<WinStreakData> winStreakData;
        public List<BoosterByLevelData> boosterByLevelData;
    }

    [CreateAssetMenu(fileName = "WinStreakConfig", menuName = "Config/Game/WinStreakConfig")]
    public class WinStreakConfig : JsonConvertableConfig
    {
        [Header("Google sheets")]
        [SerializeField] private WinStreakDataConfig configData;

        [Header("Non google sheets")]
        [SerializeField] private WinStreakSpawnParams spawnParams;

        public WinStreakFeatureData WinStreakFeature => configData.winStreakFeatureData[0];
        public List<WinStreakData> WinStreak => configData.winStreakData;
        public List<BoosterByLevelData> BoosterByLevel => configData.boosterByLevelData;
        public WinStreakSpawnParams SpawnParams => spawnParams;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "WinStreakConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (WinStreakDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(WinStreakDataConfig);


        public int GetMaxWinStreakLevel()
        {
            int result = 0;
            foreach (var winStreakData in WinStreak)
            {
                if (winStreakData.lvl > result)
                    result = winStreakData.lvl;
            }

            return result;
        }


        public WinStreakData GetWinStreakData(int level)
        {
            foreach (var winStreakData in WinStreak)
            {
                if (winStreakData.lvl == level)
                    return winStreakData;
            }

            return WinStreak[0];
        }


#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.winStreakFeatureData, nameof(WinStreakFeature), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.winStreakData, nameof(WinStreak), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.boosterByLevelData, nameof(BoosterByLevel), ref data);

            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.winStreakFeatureData = GoogleDocsUtils.SnatchDataRowsToConfig<WinStreakFeatureData>(tableData);
            configData.winStreakData = GoogleDocsUtils.SnatchDataRowsToConfig<WinStreakData>(tableData);
            configData.boosterByLevelData = GoogleDocsUtils.SnatchDataRowsToConfig<BoosterByLevelData>(tableData);
        }
#endif
    }
}