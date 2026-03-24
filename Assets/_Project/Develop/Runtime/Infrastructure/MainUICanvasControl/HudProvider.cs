using Features.Boosters;
using Features.ExtraCollectableUi;
using Features.HudLevelButtons;
using Features.LevelTasks;
using Features.LevelTime;
using Features.RewardTrack;
using Features.Widgets;
using Infrastructure.Settings;
using UnityEngine;

namespace Infrastructure.MainUICanvasControl
{
    public class HudProvider : MonoBehaviour
    {
        [SerializeField] private SettingsUIButton settingsUIButton;
        [SerializeField] private Transform currencyUIRoot;
        [SerializeField] private ExtraCollectableHud extraCollectableHud;
        [SerializeField] private LevelTasksHud levelTasksHud;
        [SerializeField] private LevelTimerHud levelTimerHud;
        [SerializeField] private StartLevelButtonView startLevelButtonView;
        [SerializeField] private BoostersPanelHud boostersPanelHud;
        [SerializeField] private RewardTrackHudProvider rewardTrackHud;

        [field: SerializeField] public Widget PlayerProfileWidget;
        [field: SerializeField] public WidgetUIRoots WidgetUIRoots { get; private set; }
        
        public SettingsUIButton SettingsUIButton => settingsUIButton;
        public ExtraCollectableHud ExtraCollectableHud => extraCollectableHud;
        public LevelTasksHud LevelTasksHud => levelTasksHud;
        public LevelTimerHud LevelTimerHud => levelTimerHud;
        public Transform CurrencyUIRoot => currencyUIRoot;
        public StartLevelButtonView StartLevelButtonView => startLevelButtonView;
        public BoostersPanelHud BoostersPanelHud => boostersPanelHud;
        public RewardTrackHudProvider RewardTrackHud => rewardTrackHud;
    }
}