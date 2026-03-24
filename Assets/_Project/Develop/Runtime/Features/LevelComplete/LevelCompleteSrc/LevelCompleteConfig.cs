using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;

namespace Features.LevelComplete
{
    [Serializable]
    public class CompleteRewardData
    {
        public int rewardCoinCount;
        public int adsMultiplier;
        public int difficultyHardMultiplier;
        public int difficultyVeryHardMultiplier;
        public int difficultyInsaneMultiplier;
        public bool jellyHolePopupEnabled;
    }

    [Serializable]
    public class LevelCompleteDataConfig
    {
        public List<CompleteRewardData> completeReward;
    }

    [CreateAssetMenu(fileName = "LevelCompleteConfig", menuName = "Config/Game/LevelCompleteConfig")]
    public class LevelCompleteConfig : JsonConvertableConfig
    {

        [SerializeField] private LevelCompleteDataConfig configData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "LevelCompleteConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (LevelCompleteDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(LevelCompleteDataConfig);

        public List<CompleteRewardData> RewardItems => configData.completeReward;

        public int RewardCoinCount => configData.completeReward[0].rewardCoinCount;
        public int AdsMultiplier => configData.completeReward[0].adsMultiplier;
        public bool JellyHolePopupEnabled => configData.completeReward[0].jellyHolePopupEnabled;


        public int GetMultiplierByDifficulty(LevelDifficulty diff)
        {
            return diff switch
            {
                LevelDifficulty.Hard     => configData.completeReward[0].difficultyHardMultiplier,
                LevelDifficulty.VeryHard => configData.completeReward[0].difficultyVeryHardMultiplier,
                LevelDifficulty.Insane   => configData.completeReward[0].difficultyInsaneMultiplier,
                _ => 1
            };
        }


#if UNITY_EDITOR


        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.completeReward, nameof(RewardItems), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.completeReward = GoogleDocsUtils.SnatchDataRowsToConfig<CompleteRewardData>(tableData);
        }
#endif
    }
}