using Features.Boosters;
using Features.CameraFollow;
using Features.Character;
using Features.Collectables;
using Features.Effects;
using Features.FlyingText;
using Features.LevelComplete;
using Features.LevelUp;
using Features.LevelLoose;
using Features.LevelConfiguration;
using Features.LevelSequence;
using Infrastructure.WalletSystem;
using Infrastructure.Ads;
using Infrastructure.Localization;
using Infrastructure.Settings;
using Infrastructure.PurchaseSystem;
using Infrastructure.SystemModules;
using UnityEngine;
using Features.FeatureUnlock;
using Features.Life;
using Features.CurrencyView;
using Features.ExtraItemSpawn;
using Features.CollectableCurrency;
using Features.ExtraCollectableUi;
using Features.LavaQuest;
using Features.RewardTrack;
using Features.WinStreak;
using Features.Skin;
using Infrastructure.DateTimeControl;
using Infrastructure.TimeCycles;
using Infrastructure.TooltipControl;
using Features.FreePaidOffer;
using Features.LevelSessionStateControl;
using Features.MainMenuUnlock;
using Features.Tutorial;
using Infrastructure.BroTweens;
using Features.Shadows;
using Features.SuperSpeedMode;
using Infrastructure.HapticControl;
using Infrastructure.Reward.Container;
using Features.PlayerProfile;
using Features.Widgets;
using Features.Social;
using Features.BottomPanel;
using Features.Competition;
using Features.ScoringVisualize;
using Features.LifeUi;

namespace Infrastructure.Configs
{
    [CreateAssetMenu(fileName = "ConfigProvider", menuName = "Config/System/ConfigProvider")]
    public class ConfigProvider : ScriptableObject
    {

        [field: Header("Main")]
        [field: SerializeField] public bool UseRemoteConfigs { get; private set; }

        [field: Header("System configs")]
        [field: SerializeField] public SystemSettingsConfig SystemSettingsConfig { get; private set; }

        [field: SerializeField] public BuildNumberConfig BuildNumberConfig { get; private set; }

        [field: SerializeField] public AssetMappingConfig AssetMappingConfig { get; private set; }
        [field: SerializeField] public LocalizationConfig LocalizationConfig { get; private set; }
        [field: SerializeField] public DateTimeServiceConfig DateTimeServiceConfig { get; private set; }
        [field: SerializeField] public TimeCyclesConfig TimeCyclesConfig { get; private set; }
        [field: SerializeField] public SocialConfig SocialConfig { get; private set; }

        [field: Header("Game")]
        [field: SerializeField] public FeatureUnlockConfiguration FeatureUnlockConfiguration { get; private set; }
        [field: SerializeField] public AdsConfig AdsConfig { get; private set; }

        [field: SerializeField] public LevelUpConfig LevelUpConfig { get; private set; }
        [field: SerializeField] public FlyingTextConfig FlyingTextConfig { get; private set; }
        [field: SerializeField] public LevelSequenceConfig LevelSequenceConfig { get; private set; }
        [field: SerializeField] public LevelCompleteConfig LevelCompleteConfig { get; private set; }
        [field: SerializeField] public CollectablesConfig CollectablesConfig { get; private set; }
        [field: SerializeField] public CollectableCurrencyConfig CollectableCurrencyConfig { get; private set; }
        [field: SerializeField] public CameraConfig CameraConfig { get; private set; }
        [field: SerializeField] public ResurrectConfig ResurrectConfig { get; private set; }
        [field: SerializeField] public CurrencyConfig CurrencyConfig { get; private set; }
        [field: SerializeField] public CurrencyModelAnimConfig CurrencyModelAnimConfig { get; private set; }
        [field: SerializeField] public InAppConfig InAppConfig { get; private set; }
        [field: SerializeField] public ResurrectOfferConfig ResurrectOfferConfig { get; private set; }
        [field: SerializeField] public RewardContainersConfig RewardContainersConfig { get; private set; }
        [field: SerializeField] public LevelConfig LevelConfig { get; private set; }
        [field: SerializeField] public BoosterConfig BoosterConfig { get; private set; }
        [field: SerializeField] public CharacterConfig CharacterConfig { get; private set; }
        [field: SerializeField] public LifeConfiguration LifeConfiguration { get; private set; }
        [field: SerializeField] public LifeUiConfiguration LifeUiConfiguration { get; private set; }
        [field: SerializeField] public ObjectSpawnConfig ObjectSpawnConfig { get; private set; }
        [field: SerializeField] public WinStreakConfig WinStreakConfig { get; private set; }
        [field: SerializeField] public RewardTrackConfig RewardTrackConfig { get; private set; }
        [field: SerializeField] public LavaQuestConfig LavaQuestConfig { get; private set; }
        [field: SerializeField] public FreePaidOfferConfig FreePaidOfferConfig { get; private set; }
        [field: SerializeField] public LevelSessionStateControlConfiguration LevelSessionStateControlConfiguration { get; private set; }
        [field: SerializeField] public TutorialConfiguration TutorialConfiguration { get; private set; }
        [field: SerializeField] public MainMenuUnlockConfig MainMenuUnlockConfig { get; private set; }
        [field: SerializeField] public SuperSpeedConfig SuperSpeedConfig { get; private set; }
        [field: SerializeField] public ExtraItemsUIConfig ExtraItemsUIConfig { get; private set; }
        [field: SerializeField] public PlayerProfileConfiguration PlayerProfileConfiguration { get; private set; }
        [field: SerializeField] public WidgetsConfig WidgetsConfig { get; private set; }
        [field: SerializeField] public BottomPanelConfiguration BottomPanelConfiguration { get; private set; }
        [field: SerializeField] public CompetitionConfig CompetitionConfig { get; private set; }

        [field: Header("Non json convertable")]
        [field: SerializeField] public EffectsConfig EffectsConfig { get; private set; }
        [field: SerializeField] public LostItemSettings LostItemSettings { get; private set; }
        [field: SerializeField] public SkinConfiguration SkinConfiguration { get; private set; }
        [field: SerializeField] public CustomTooltipConfiguration CustomTooltipConfiguration { get; private set; }
        [field: SerializeField] public FreePaidOfferSkinConfiguration FreePaidOfferSkinConfiguration { get; private set; }
        [field: SerializeField] public FreePaidOfferAnimationConfig FreePaidOfferAnimationConfig { get; private set; }
        [field: SerializeField] public LevelTableSkinConfiguration LevelTableSkinConfiguration { get; private set; }
        [field: SerializeField] public BroTweenConfig BroTweenConfig { get; private set; }
        [field: SerializeField] public ShadowsConfig ShadowsConfig { get; private set; }
        [field: SerializeField] public HapticsCustomPresetsConfig HapticsCustomPresetsConfig { get; private set; }
        [field: SerializeField] public LevelDifficultyCache LevelDifficultyCache { get; private set; }
        [field: SerializeField] public ScoringVisualizeConfig ScoringVisualizeConfig { get; private set; }



        public void Initialize()
        {
            CollectablesConfig.Initialize();
            CurrencyConfig.Initialize();
            CurrencyModelAnimConfig.Initialize();
            LocalizationConfig.Initialize();
            FreePaidOfferConfig.Initialize();
        }


        public void Deinitialize()
        {

        }


        public void SetConfigsFromRemoteSource()
        {
            SystemSettingsConfig.SetConfigsFromRemoteSource();
            AdsConfig.SetConfigsFromRemoteSource();
            LocalizationConfig.SetConfigsFromRemoteSource();
            LevelUpConfig.SetConfigsFromRemoteSource();
            FlyingTextConfig.SetConfigsFromRemoteSource();
            LevelSequenceConfig.SetConfigsFromRemoteSource();
            CollectablesConfig.SetConfigsFromRemoteSource();
            ResurrectConfig.SetConfigsFromRemoteSource();
            InAppConfig.SetConfigsFromRemoteSource();
            ResurrectOfferConfig.SetConfigsFromRemoteSource();
            RewardContainersConfig.SetConfigsFromRemoteSource();
            LevelConfig.SetConfigsFromRemoteSource();
            BoosterConfig.SetConfigsFromRemoteSource();
            CharacterConfig.SetConfigsFromRemoteSource();
            CameraConfig.SetConfigsFromRemoteSource();
            WinStreakConfig.SetConfigsFromRemoteSource();
            DateTimeServiceConfig.SetConfigsFromRemoteSource();
            TimeCyclesConfig.SetConfigsFromRemoteSource();
            RewardTrackConfig.SetConfigsFromRemoteSource();
            LavaQuestConfig.SetConfigsFromRemoteSource();
            FreePaidOfferConfig.SetConfigsFromRemoteSource();
            LevelSessionStateControlConfiguration.SetConfigsFromRemoteSource();
            TutorialConfiguration.SetConfigsFromRemoteSource();
            MainMenuUnlockConfig.SetConfigsFromRemoteSource();
            SuperSpeedConfig.SetConfigsFromRemoteSource();
            LevelCompleteConfig.SetConfigsFromRemoteSource();
            ExtraItemsUIConfig.SetConfigsFromRemoteSource();
            PlayerProfileConfiguration.SetConfigsFromRemoteSource();
            WidgetsConfig.SetConfigsFromRemoteSource();
            SocialConfig.SetConfigsFromRemoteSource();
            CompetitionConfig.SetConfigsFromRemoteSource();
            CurrencyConfig.SetConfigsFromRemoteSource();
            BottomPanelConfiguration.SetConfigsFromRemoteSource();
            LifeUiConfiguration.SetConfigsFromRemoteSource();
        }
    }
}