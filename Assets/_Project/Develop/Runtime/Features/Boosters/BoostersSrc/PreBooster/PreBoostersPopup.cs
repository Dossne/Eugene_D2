using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Features.LevelComplete;
using Features.SuperSpeedMode;
using Features.Tutorial;
using Features.WinStreak;
using Infrastructure.Ads;
using Infrastructure.BroTweens;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.SpriteAtlasControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Features.Boosters
{
    public class PreBoostersPopup : PopupBase
    {
        public event Action OnStartClick;
        public event Action OnSuperSpeedWidgetClick;
        public event Action<BoosterType, bool /*isSelected*/> OnSlotClick;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI levelTxt;
        [SerializeField] private TextMeshProUGUI selectBoosterTxt;
        [SerializeField] private TextMeshProUGUI buttonTxt;

        [Header("Components")]
        [SerializeField] private Transform boostersRoot;
        [SerializeField] private PreBoosterSlotView preBoosterViewPf;
        [SerializeField] private Button startLevelButton;
        [SerializeField] private RewardMultiplierView rewardMultiplierView;
        [SerializeField] private SerializedDictionary<LevelDifficulty, GameObject> mainBg;
        [SerializeField] private SerializedDictionary<LevelDifficulty, Sprite> boostersBg;
        [SerializeField] private Image boosterBgImage;
        [SerializeField] private WinStreakUIPanel winStreakUIPanel;
        [SerializeField] private SuperSpeedWidget superSpeedWidget;

        private readonly Dictionary<BoosterType, PreBoosterSlotView> boosterViews = new();
        private SpriteAtlasService spriteAtlasService;
        private TutorialTriggerEvent tutorialTriggerEvent;
        private List<BoosterData> startBoosterConfig;

        public WinStreakUIPanel WinStreakUIPanel => winStreakUIPanel;


        [Inject]
        public void Construct(SpriteAtlasService spriteAtlasService,
                              TutorialTriggerEvent tutorialTriggerEvent)
        {
            this.spriteAtlasService = spriteAtlasService;
            this.tutorialTriggerEvent = tutorialTriggerEvent;
            superSpeedWidget.Construct(spriteAtlasService.GetFromMain("super_speed_icon"), SuperSpeedWidget_OnClick);
        }


        public void Construct(List<BoosterData> startBoosterConfig)
        {
            this.startBoosterConfig = startBoosterConfig;
        }


        protected override void OnInitialize()
        {
            startLevelButton.onClick.AddListener(StartButtonClick);
            superSpeedWidget.Initialize();

            selectBoosterTxt.text = LocalizationService.I.Get(LocKeys.Boosters.SelectBoosters);
            for (int i = 0; i < startBoosterConfig.Count; i++)
            {
                BoosterData configData = startBoosterConfig[i];
                PreBoosterSlotView view = Instantiate(preBoosterViewPf, boostersRoot);

                view.Construct(configData.type, spriteAtlasService.GetFromMain(configData.iconName));
                view.Initialize();
                view.OnClick += BoosterView_OnClick;
                boosterViews.Add(configData.type, view);
            }
        }


        protected override void OnDeinitialize()
        {
            startLevelButton.onClick.RemoveListener(StartButtonClick);
            superSpeedWidget.Deinitialize();

            foreach (var view in boosterViews.Values)
            {
                view.OnClick -= BoosterView_OnClick;
                view.Deinitialize();
                view.Destroy();
            }

            boosterViews.Clear();
            OnStartClick = null;
            OnSlotClick = null;
        }

        protected override void OnBeginOpen() 
        { 
            tutorialTriggerEvent.Execute(TutorialTrigger.PreBoosterPopupBeginOpen);
        }

        public void RefreshPopup(LevelDifficulty difficulty, int level, int rewardMultiplier)
        {
            foreach (var entryPair in mainBg)
            {
                bool active = entryPair.Key == difficulty;

                if (entryPair.Value.gameObject.activeSelf != active)
                    entryPair.Value.gameObject.SetActive(active);
            }

            this.levelTxt.text = LocalizationService.I.Get(LocKeys.Boosters.Level, level.ToString());
            rewardMultiplierView.Construct(rewardMultiplier, difficulty is LevelDifficulty.Hard or LevelDifficulty.VeryHard or LevelDifficulty.Insane);
            boosterBgImage.sprite = boostersBg[difficulty];

            buttonTxt.text = LocalizationService.I.Get(LocKeys.PreBoosterPopup.PlayButton);
        }

        public void RefreshSuperSpeedWidget(SuperSpeedController superSpeedController)
        {
            superSpeedWidget.SetWidgetState(superSpeedController.IsFeatureEnabled,
                                            superSpeedController.IsAnnounced,
                                            superSpeedController.IsUnlocked,
                                            superSpeedController.WinCount,
                                            superSpeedController.MaxWinCount);
        }


        public void RefreshSlotState(BoosterType type,
                                     BoosterStateType stateType,
                                     string count,
                                     string lockText,
                                     string freeText,
                                     bool isSelectable,
                                     float infiniteExpiresTime)
        {
            if (boosterViews.TryGetValue(type, out var view))
                view.RefreshState(stateType,
                                  count,
                                  lockText,
                                  freeText,
                                  isSelectable,
                                  infiniteExpiresTime);
        }


        public RectTransform GetBoosterViewRectTransform(BoosterType type)
        {
            if (boosterViews.TryGetValue(type, out var view))
            {
                return view.transform as RectTransform;
            }

            return null;
        }

        public RectTransform GetSuperSpeedWidgetRectTransform()
        {
            return superSpeedWidget.GetComponent<RectTransform>();
        }

        public void ShowSuperSpeedWidgetTooltip(string text)
        {
            superSpeedWidget.ShowTooltip(text);
        }

        private void StartButtonClick()
        {
            AnalyticSender.TrackClick(this.name, startLevelButton.name);

            BroTween.ClickBounceWithCallBack(startLevelButton, startLevelButton.transform, this, target => target.StartClickInvoke(), callbackOnEnd: true)
                    .Play();
        }


        private void StartClickInvoke()
        {
            OnStartClick?.Invoke();
        }


        private void BoosterView_OnClick(BoosterType type, bool isSelected)
        {
            OnSlotClick?.Invoke(type, isSelected);
        }

        private void SuperSpeedWidget_OnClick()
        {
            OnSuperSpeedWidgetClick?.Invoke();
        }
    }
}