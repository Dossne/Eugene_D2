using System;
using System.Collections.Generic;
using Features.LevelComplete;
using Infrastructure.Configuration;
using Infrastructure.Utilities;
using UnityEngine;
using VContainer;

namespace Features.LevelSequence
{
    [Serializable]
    public class LevelSequenceData
    {
        public string levelId;
    }

    [Serializable]
    public class LevelRulesDataConfig
    {
        public List<LevelSequenceData> rules;
        public List<LevelSequenceData> randomLevelsRules;
        public List<LevelDifficulty> randomLevelsDifficultySequence;
    }

    [CreateAssetMenu(fileName = "LevelSequenceConfig", menuName = "Config/Game/LevelSequenceConfig")]
    public class LevelSequenceConfig : JsonConvertableConfig
    {
        [SerializeField] private LevelRulesDataConfig configData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "LevelSequenceConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (LevelRulesDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(LevelRulesDataConfig);

        public List<LevelSequenceData> LevelSequence => configData.rules;
        public List<LevelSequenceData> RandomLevelRules => configData.randomLevelsRules;
        public List<LevelDifficulty> RandomLevelsDifficultySequence => configData.randomLevelsDifficultySequence;



        public List<string> GetAllLevelsIds()
        {
            List<string> levels = new List<string>();
            GetAllLevelsIds(LevelSequence, ref levels);
            GetAllLevelsIds(RandomLevelRules, ref levels);
            return levels;
        }


        private void GetAllLevelsIds(List<LevelSequenceData> levelSequences, ref List<string> levels)
        {
            for(int i = 0; i < LevelSequence.Count; ++i)
            {
                string levelId = LevelSequence[i].levelId;
                if(!levels.Contains(levelId))
                {
                    levels.Add(levelId);
                }
            }
        }


#if UNITY_EDITOR

        private void OnValidate()
        {
            foreach (LevelSequenceData item in LevelSequence)
            {
                if (string.IsNullOrEmpty(item.levelId))
                {
                    item.levelId = Utils.GenerateUniqueId();
                }
            }
        }

        public void GetLevelById_Editor(string searchId, List<LevelSequenceData> input)
        {
            for (var i = 0; i < configData.rules.Count; i++)
            {
                var data = configData.rules[i];
                if (data.levelId == searchId)
                {
                    input.Add(data);
                }
            }

            for (var i = 0; i < configData.randomLevelsRules.Count; i++)
            {
                var data = configData.randomLevelsRules[i];
                if (data.levelId == searchId)
                {
                    input.Add(data);
                }
            }
        }

        public override GoogleDocsUtils.MajorDimension SpreadSheetDimension => GoogleDocsUtils.MajorDimension.COLUMNS;

        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataColumns(configData.rules, nameof(LevelSequence), ref data);
            GoogleDocsUtils.AddConfigToDataColumns(configData.randomLevelsRules, "RandomLevels", ref data);
            GoogleDocsUtils.AddConfigToDataColumns(configData.randomLevelsDifficultySequence, "Random Levels Difficulty Sequence", ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }

        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.rules = GoogleDocsUtils.SnatchDataColumnsToConfig<LevelSequenceData>(tableData);
            configData.randomLevelsRules = GoogleDocsUtils.SnatchDataColumnsToConfig<LevelSequenceData>(tableData);
            configData.randomLevelsDifficultySequence = GoogleDocsUtils.SnatchDataColumnsToConfig<LevelDifficulty>(tableData);

        }
#endif
    }

}