using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Widgets;
using Infrastructure.Reward;
using Infrastructure.TooltipControl;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;

namespace Features.Competition
{
    public class CompetitionWidget : Widget
    {
        [Header("LockState")]
        [SerializeField] private GameObject lockObject;
        [SerializeField] private Tooltip tooltip;

        [Header("Other")]
        [SerializeField] private TextMeshProUGUI notifierText;
        [SerializeField] private TextMeshProUGUI stateText;

        [Header("Image state")]
        [SerializeField] private StateImage[] stateImages;

        [Header("Side notifiers")]
        [SerializeField] private WidgetAnimatedPanels panels;

        private WidgetState currentState;

        public void ConstructRewardPanel(RewardItemVisualData reward)
        {
            panels.ConstructRewardPanel(reward);
        }

        public void InitializeRewardPanel()
        {
            panels.InitializeRewardPanel();
        }

        public UniTask PlayRewardAnimationAsync(CancellationToken token)
        {
            return panels.PlayRewardAnimationAsync(token);
        }

        public void ConstructMultiplierPanel(int multiplierCurrent, int multiplierNext)
        {
            panels.ConstructMultiplierPanel(multiplierCurrent, multiplierNext);
        }

        public void InitializeMultiplierPanel()
        {
            panels.InitializeMultiplierPanel();
        }

        public UniTask PlayMultiplierAnimationAsync(CancellationToken token)
        {
            return panels.PlayMultiplierAnimationAsync(token);
        }

        public void InitializePanels()
        {
            panels.Initialize();
        }

        public void DeinitializePanels()
        {
            panels.Deinitialize();
        }
        
        public void SetNotifierText(string value)
        {
            notifierText.text = value;
        }

        public void SetLeaderboardPositionText(string value)
        {
            stateText.text = value;
        }

        public void SeStateImages(WidgetState newState)
        {
            if (newState == currentState)
                return;

            currentState = newState;
            lockObject.SetObjectActive(currentState == WidgetState.Locked);

            foreach (var stateImage in stateImages)
            {
                if (!stateImage.sprites.TryGetValue(currentState, out ColoredSprite coloredSprite))
                    continue;

                stateImage.target.sprite = coloredSprite.value;
                stateImage.target.color = coloredSprite.color;
            }
        }

        public void ShowTooltip(string text)
        {
            tooltip.Show(text);
        }

        protected override void OnInitialize()
        {
            tooltip.Initialize();
        }

        protected override void OnDeinitialize()
        {
            tooltip.Deinitialize();
        }
    }
}