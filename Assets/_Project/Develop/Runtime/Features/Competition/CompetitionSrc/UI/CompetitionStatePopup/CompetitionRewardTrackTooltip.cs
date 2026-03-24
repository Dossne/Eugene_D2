using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Features.RewardTrack;
using Infrastructure.Localization;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.TooltipControl;
using UnityEngine;

namespace Features.Competition
{
    public class CompetitionRewardTrackTooltip : Tooltip
    {
        [Header("RewardTrack")]
        [SerializeField] private CompetitionRewardTrackTooltipScroller scroller;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RewardTrackStateUiElement prefab;
        [SerializeField] private float offsetFromBottomY;

        [SerializeField] private Tooltip tooltip;

        private List<RewardTrackStateUiElement> views = new();
        private List<CompetitionRewardTrackData> configData = new();

        private CompetitionRewardController rewardController;
        private SpriteAtlasService spriteAtlasService;
        private int currentIdx = -1;

        public void Construct(CompetitionRewardController stateController, SpriteAtlasService spriteAtlasService, List<CompetitionRewardTrackData> configData)
        {
            this.rewardController = stateController;
            this.spriteAtlasService = spriteAtlasService;
            this.configData = configData;
        }

        public void Show()
        {
            rewardController.TryGetCurrentRewardTrackState(out RewardTrackProgressDto currentProgress);

            bool isComplete = rewardController.IsRewardTrackComplete();
            
            if (currentIdx != currentProgress.configIdx)
            {
                RefreshStateOnProgressChange(currentProgress.configIdx, isComplete);
            }

            Open();
            scroller.InstantScrollAsync(currentIdx, offsetFromBottomY).Forget();
        }

        protected override void OnInitialize()
        {
            rewardController.TryGetCurrentRewardTrackState(out RewardTrackProgressDto currentProgress);
            currentIdx = currentProgress.configIdx;
            CreateViewItems();
            
            bool isComplete = rewardController.IsRewardTrackComplete();
            ConstructViewItems(isComplete);
            tooltip.Initialize();
            scroller.RefreshVerticalLayoutReversed();
        }

        protected override void OnDeinitialize()
        {
            foreach (var slotView in views)
            {
                slotView.OnClick -= RewardTrackStateUiElement_OnClick;
            }

            views.Clear();
            tooltip.Deinitialize();
        }

        private void CreateViewItems()
        {
            int createDiff = configData.Count - views.Count;
            for (int i = 0; i < createDiff; i++)
            {
                RewardTrackStateUiElement element = CreateSlotView();
                views.Add(element);
            }
        }

        private void ConstructViewItems(bool isRewardTrackComplete)
        {
            for (int i = 0; i < views.Count; i++)
            {
                RewardTrackStateUiElement view = views[i];

                if (i >= configData.Count)
                {
                    view.SetObjectActive(false);
                    continue;
                }

                var progress = rewardController.ConvertToProgressDto(configData[i], i, -1, -1, i == configData.Count - 1);
                Sprite bgSprite = spriteAtlasService.GetFromMain(progress.isLast ? view.LastSlotBgName : view.DefaultSlotBgName);

                view.Construct(progress.rewardVisual.icon,
                               bgSprite,
                               progress.rewardVisual.amountText,
                               progress.stepNumber,
                               progress.rewardVisual.isDisplayRibbon,
                               progress.rewardVisual.isDisplayInfinityIcon,
                               progress.isLast,
                               i);
                
                view.OnClick += RewardTrackStateUiElement_OnClick;
                RefreshItemState(view, currentIdx, isRewardTrackComplete);
                view.SetObjectActive(true);
                scroller.AddElement(view);
            }
        }

        private void RefreshStateOnProgressChange(int idx, bool isRewardTrackComplete)
        {
            for (int i = 0; i < views.Count; i++)
            {
                RefreshItemState(views[i], idx, isRewardTrackComplete);
            }

            currentIdx = idx;
        }

        private void RefreshItemState(RewardTrackStateUiElement view, int idx,  bool isRewardTrackComplete)
        {
            bool isCurrent = idx                       == view.Index;
            bool isComplete = isRewardTrackComplete || idx > view.Index;
            view.RefreshState(isCurrent, isComplete);
        }

        private RewardTrackStateUiElement CreateSlotView()
        {
            return Instantiate(prefab, contentRoot);
        }

        private void RewardTrackStateUiElement_OnClick(int idx)
        {
            string text = LocalizationService.I.Get(LocKeys.Competition.RewardTrackTooltip);
            tooltip.Show(text, views[idx].TooltipTarget.position);
        }
    }
}