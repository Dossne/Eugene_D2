using System;
using System.Collections.Generic;
using Features.Collectables;
using Infrastructure.Configuration;
using Infrastructure.Utilities;
using UnityEngine;

namespace Features.Boosters
{
    [Serializable]
    public class BoosterCommonData
    {
        public float preBoosterAnimDelaySec;
        public float preBoosterAnimSec;
    }

    [Serializable]
    public class BoosterData
    {
        public BoosterType type;
        public int startCount;
        public string iconName;
        public string shopIconName;
        public string infiniteIconName;
        public float boostedValue;
        public int unlockLevel;
        public bool isFreeOnUnlockLevel;
        public string nameKey;
        public string descriptionKey;
        public string descriptionTutorialKey;
        public int buyCount;
        public int buyPrice;
        public float durationSec;

        public BoosterData Clone()
        {
            return (BoosterData)this.MemberwiseClone();
        }
    }

    [Serializable]
    public class WinStreakBoosterData
    {
        public BoosterType type;
        public CollectableType collectableType;
        public float boostedValue;
    }
    
    
    [Serializable]
    public class StartBoosterDataConfig
    {
        public List<BoosterCommonData> common;
        public List<BoosterData> preBoosters;
        public List<BoosterData> inGameBoosters;
        public List<WinStreakBoosterData> winStreakBoosters;
    }

    [CreateAssetMenu(fileName = "BoosterConfig", menuName = "Config/Game/BoosterConfig")]
    public class BoosterConfig : JsonConvertableConfig
    {
        [SerializeField] private StartBoosterDataConfig configData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "BoosterConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (StartBoosterDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(StartBoosterDataConfig);

        public BoosterCommonData BoosterCommonData => configData.common[0];
        public List<BoosterData> PreBoosters => configData.preBoosters;
        public List<BoosterData> InGameBoosters => configData.inGameBoosters;
        public List<WinStreakBoosterData> WinStreakBoosters => configData.winStreakBoosters;



        public bool IsInGameBooster(BoosterType type)
        {
            foreach (var boosterData in configData.inGameBoosters)
            {
                if (boosterData.type == type)
                    return true;
            }
            
            return false;
        }


        
        public List<BoosterData> PreAndInGameBoosters()
        {
            List <BoosterData> preAndInGameBoosters = new List<BoosterData>(PreBoosters);
            preAndInGameBoosters.AddRange(InGameBoosters);

            return preAndInGameBoosters;
        }



#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.common, nameof(BoosterCommonData), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.preBoosters, nameof(PreBoosters), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.inGameBoosters, nameof(InGameBoosters), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.winStreakBoosters, nameof(WinStreakBoosters), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.common = GoogleDocsUtils.SnatchDataRowsToConfig<BoosterCommonData>(tableData);
            configData.preBoosters = GoogleDocsUtils.SnatchDataRowsToConfig<BoosterData>(tableData);
            configData.inGameBoosters = GoogleDocsUtils.SnatchDataRowsToConfig<BoosterData>(tableData);
            configData.winStreakBoosters = GoogleDocsUtils.SnatchDataRowsToConfig<WinStreakBoosterData>(tableData);
        }
#endif
    }
}