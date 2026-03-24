using System;
using Features.PurchaseUi;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.Reward;
using Infrastructure.TooltipControl;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.RewardTrack
{
    public class RewardTrackStatePopup : PopupBase
    {
        public event Action OnInfoRequested;
        
        [Header("Top")]
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI timeLeftTxt;
        [SerializeField] private Image themeImage;
        [SerializeField] private Button infoPopupBtn;

        [Header("Progress")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image targetIcon;
        [SerializeField] private PurchaseRewardItemView rewardItemView;
        [SerializeField] private TextMeshProUGUI progressTxt;
        [SerializeField] private Tooltip tooltip;

        [Header("Scroll")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RewardTrackStateUiElement prefab;
        [SerializeField] private float customOffset = 100;

        public float ViewportHeight => scrollRect.viewport.rect.height;
        public float ContentHeight => scrollRect.content.rect.height;
        public float Spacing => verticalLayoutGroup.spacing;
        public float CustomOffset => customOffset;
        public Tooltip Tooltip => tooltip;

        private double timeLeft;
        private float second;


        private void Update()
        {
            second += Time.unscaledDeltaTime;
            if (second < 1)
                return;

            second -= 1;
            timeLeft -= 1;

            RefreshTimer();
        }


        protected override void OnInitialize()
        {
            headerTxt.text = LocalizationService.I.Get(LocKeys.RewardTrack.StateHeader);
            tooltip.Initialize();
            infoPopupBtn.onClick.AddListener(InfoPopupRequest);
        }


        protected override void OnDeinitialize()
        {
            tooltip.Deinitialize();
            infoPopupBtn.onClick.RemoveAllListeners();
            OnInfoRequested = null;
        }


        public void Refresh(Sprite targetIcon,
                            Sprite rewardIcon,
                            string rewardCount,
                            bool isDisplayRibbon,
                            bool isDisplayInfinityIcon,
                            int currentCollected,
                            int maxCollected,
                            double timeLeft)
        {
            this.rewardItemView.Construct(rewardIcon, rewardCount, null, isDisplayRibbon, isDisplayInfinityIcon);
            this.targetIcon.sprite = targetIcon;
            this.progressSlider.value = currentCollected * 1f / maxCollected;
            this.progressTxt.text = $"{currentCollected.ToString()}/{maxCollected.ToString()}";
            this.timeLeft = timeLeft;

            RefreshTimer();
        }


        public void SetThemeImage(Sprite theme)
        {
            themeImage.sprite = theme;
        }

        public void InfoPopupRequest()
        {
            OnInfoRequested?.Invoke();
        }

        private void RefreshTimer()
        {
            timeLeftTxt.text = TimeUtils.GetTimeString((float)timeLeft, 100f);
        }


        public RewardTrackStateUiElement CreateSlotView()
        {
            return Instantiate(prefab, contentRoot);
        }


        public void SetContentRootPosition(float targetPositionY)
        {
            scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, targetPositionY);
        }


        public void StopScrollMovement()
        {
            scrollRect.StopMovement();
        }        
    }
}