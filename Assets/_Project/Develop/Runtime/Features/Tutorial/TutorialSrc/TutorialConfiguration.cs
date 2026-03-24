using Features.Boosters;
using Infrastructure.Configuration;
using Infrastructure.MainUICanvasControl;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Features.Tutorial
{
    [Serializable]
    public class TutorialGroupData
    {
        public TutorialGroup tutorialGroup;
        public bool enabled;
    }

    [Serializable]
    public class PreBoosterUseData
    {
        public BoosterType boosterType;
        public UiOrientationType tooltipOrientation;
        public float tooltipSizeHorizontal;
        public float tooltipSizeVertical;
        public float tooltipOffsetX;
        public float tooltipOffsetY;
        public UiOrientationType pointerOrientation;
        public float pointerSizeHorizontal;
        public float pointerSizeVertical;
        public float pointerOffsetX;
        public float pointerOffsetY;
    }


    [Serializable]
    public class InGameBoosterUseData
    {
        public BoosterType boosterType;
        public UiOrientationType tooltipOrientation;
        public float tooltipSizeHorizontal;
        public float tooltipSizeVertical;
        public float tooltipOffsetX;
        public float tooltipOffsetY;
        public UiOrientationType pointerOrientation;
        public float pointerSizeHorizontal;
        public float pointerSizeVertical;
        public float pointerOffsetX;
        public float pointerOffsetY;
    }

    [Serializable]
    public class WinStreakUnlockData
    {
        public UiOrientationType pointerOrientation;
        public float pointerSizeHorizontal;
        public float pointerSizeVertical;
        public float pointerOffsetX;
        public float pointerOffsetY;
    }

    [Serializable]
    public class RewardTrackUnlockData
    {
        public UiOrientationType tooltipOrientation;
        public float tooltipSizeHorizontal;
        public float tooltipSizeVertical;
        public float tooltipOffsetX;
        public float tooltipOffsetY;
        public UiOrientationType pointerOrientation;
        public float pointerSizeHorizontal;
        public float pointerSizeVertical;
        public float pointerOffsetX;
        public float pointerOffsetY;
    }

    [Serializable]
    public class LavaQuestStartData
    {
        public float stepDelay;
        public UiOrientationType tooltipOrientation;
        public float tooltipSizeHorizontal;
        public float tooltipSizeVertical;
        public float tooltipOffsetX;
        public float tooltipOffsetY;
    }

    [Serializable]
    public class SuperSpeedWidgetData
    {
        public UiOrientationType pointerOrientation;
        public float pointerSizeHorizontal;
        public float pointerSizeVertical;
        public float pointerOffsetX;
        public float pointerOffsetY;
    }

    [Serializable]
    public class PlayerProfileUseData
    {
        public UiOrientationType tooltipOrientation;
        public float tooltipSizeHorizontal;
        public float tooltipSizeVertical;
        public float tooltipOffsetX;
        public float tooltipOffsetY;
        public UiOrientationType pointerOrientation;
        public float pointerSizeHorizontal;
        public float pointerSizeVertical;
        public float pointerOffsetX;
        public float pointerOffsetY;
    }


    [Serializable]
    public class TutotialConfigurationData
    {
        public List<TutorialGroupData>     groupData;
        public List<InGameBoosterUseData>  inGameBoosterUseData;
        public List<PreBoosterUseData>     preBoosterUseData;
        public List<WinStreakUnlockData>   winStreakUnlockData;
        public List<RewardTrackUnlockData> rewardTrackUnlockData;
        public List<LavaQuestStartData>    lavaQuestStartData;
        public List<SuperSpeedWidgetData>  superSpeedWidgetData;
        public List<PlayerProfileUseData>  playerProfileUseData;
        public List<int> lockLevelStartUntilTutorialData;
    }

    [CreateAssetMenu(fileName = "TutorialConfiguration", menuName = "Config/Game/TutorialConfiguration")]
    public class TutorialConfiguration : JsonConvertableConfig
    {
        [SerializeField] private TutotialConfigurationData configData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "TutorialConfiguration";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (TutotialConfigurationData)value;
        }

        protected override Type ConfigurationDataType => typeof(TutotialConfigurationData);

        public bool IsGroupEnabled(TutorialGroup tutorialGroup)
        {
            var data = configData.groupData.Find(x => x.tutorialGroup == tutorialGroup);
            return data != null && data.enabled;
        }

        public bool NeedLockLevelStart(int levelNumber)
        {
            return configData.lockLevelStartUntilTutorialData.Contains(levelNumber);            
        }

        public InGameBoosterUseData GetInGameBoosterUseData(BoosterType boosterType)
        {
            return configData.inGameBoosterUseData.Find(x => x.boosterType == boosterType);
        }

        public PreBoosterUseData GetPreBoosterUseData(BoosterType boosterType)
        {
            return configData.preBoosterUseData.Find(x => x.boosterType == boosterType);
        }

        public WinStreakUnlockData GetWinStreakUnlockData()
        {
            return configData.winStreakUnlockData.Count == 0 ? null : configData.winStreakUnlockData[0];
        }

        public RewardTrackUnlockData GetRewardTrackUnlockData()
        {
            return configData.rewardTrackUnlockData.Count == 0 ? null : configData.rewardTrackUnlockData[0];
        }

        public List<LavaQuestStartData> GetLavaQuestStartData()
        {
            return configData.lavaQuestStartData;
        }

        public SuperSpeedWidgetData GetSuperSpeedWidgetData()
        {
            return configData.superSpeedWidgetData.Count == 0 ? null : configData.superSpeedWidgetData[0];
        }

        public PlayerProfileUseData GetPlayerProfileUseData()
        {
            return configData.playerProfileUseData.Count == 0 ? null : configData.playerProfileUseData[0];
        }

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.groupData, nameof(TutorialGroupData), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.inGameBoosterUseData, nameof(InGameBoosterUseData), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.preBoosterUseData, nameof(PreBoosterUseData), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.winStreakUnlockData, nameof(WinStreakUnlockData), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.rewardTrackUnlockData, nameof(RewardTrackUnlockData), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.lavaQuestStartData, nameof(LavaQuestStartData), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.superSpeedWidgetData, nameof(SuperSpeedWidgetData), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.playerProfileUseData, nameof(PlayerProfileUseData), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.groupData = GoogleDocsUtils.SnatchDataRowsToConfig<TutorialGroupData>(tableData);
            configData.inGameBoosterUseData = GoogleDocsUtils.SnatchDataRowsToConfig<InGameBoosterUseData>(tableData);
            configData.preBoosterUseData = GoogleDocsUtils.SnatchDataRowsToConfig<PreBoosterUseData>(tableData);
            configData.winStreakUnlockData = GoogleDocsUtils.SnatchDataRowsToConfig<WinStreakUnlockData>(tableData);
            configData.rewardTrackUnlockData = GoogleDocsUtils.SnatchDataRowsToConfig<RewardTrackUnlockData>(tableData);
            configData.lavaQuestStartData = GoogleDocsUtils.SnatchDataRowsToConfig<LavaQuestStartData>(tableData);
            configData.superSpeedWidgetData = GoogleDocsUtils.SnatchDataRowsToConfig<SuperSpeedWidgetData>(tableData);
            configData.playerProfileUseData = GoogleDocsUtils.SnatchDataRowsToConfig<PlayerProfileUseData>(tableData);
        }
#endif
    }
}