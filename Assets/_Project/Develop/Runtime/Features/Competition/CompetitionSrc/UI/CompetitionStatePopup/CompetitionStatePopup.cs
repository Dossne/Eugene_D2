using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Features.ScrollList;
using Features.Social;
using Infrastructure.BroTweens;
using Infrastructure.Localization;
using Infrastructure.Pool.Particles;
using Infrastructure.Popups;
using Infrastructure.Reward;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Competition
{
    [Serializable]
    public class CompetitionStatePopupAnimations
    {
        [Header("Move scroll")]
        public AnimationCurve scrollEaseCurve;
        public float scrollDurationMin = 1f;
        public float scrollDurationMax = 3f;

        [Header("Scale player slot")]
        public AnimationCurve scaleCurve;
        public float scaleDuration = 0.5f;

        [Header("Slot other move")]
        public AnimationCurve slotMoveEaseCurve;
        public float slotMinDistanceCheck = 15f;
    }

    public class CompetitionStatePopup : PopupBase
    {
        public event Action OnInfoRequested;
        public event Action<int> OnSlotClicked;

        [TriInspector.Title("Top")]
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI timeLeftTxt;
        [SerializeField] private Button infoPopupBtn;
        [SerializeField] private CompetitionMultiplierView multipliers;
        [SerializeField] private string multiplierPrefix = "x";
        [SerializeField] private CompetitionRewardTrackProgress rewardTrackProgress;

        [TriInspector.Title("Scroll")]
        [SerializeField] private CompetitionSlotView slotViewPf;
        [SerializeField] private RectTransform slotsContentRoot;
        [SerializeField] private StickySlotController stickySlotController;
        [SerializeField] private CompetitionStatePopupScroller scroller;
        [SerializeField] private RectTransform pinnedRoot;
        [SerializeField] private ReusableParticleSystemUI slotSplashFx;
        [SerializeField] private Sprite playerBgImage;
        [SerializeField] private Sprite otherBgImage;
        [SerializeField] private Sprite playerStateImage;
        [SerializeField] private Sprite otherStateImage;
        [SerializeField] private List<Sprite> positionIcons;
        [SerializeField] private RewardTooltip rewardTooltip;

        [TriInspector.Title("Scroll animation")]
        [SerializeField] private CompetitionStatePopupAnimations scrollAnimParams;

        private List<RewardContainerVisualData> rewardContainers;

        private BroTweenSafe scrollSeq;
        private int moveCount;
        private int playerPosIdx;
        private int slotCount;
        private float perSlotMoveDuration;
        private double timeLeft;
        private float second;

        public CompetitionRewardTrackProgress RewardTrackProgress => rewardTrackProgress;
        public string MultiplierPrefix => multiplierPrefix;

        public int PlayerPosIdx => playerPosIdx;

        public void Construct(List<RewardContainerVisualData> rewardContainers,
                              List<string> multiplierTexts,
                              CompetitionRewardController rewardController,
                              SpriteAtlasService spriteAtlasService,
                              List<CompetitionRewardTrackData> configData,
                              int slotCount)
        {
            this.rewardContainers = rewardContainers;
            multipliers.Construct(multiplierTexts);
            rewardTrackProgress.Construct(rewardController, spriteAtlasService, configData);
            this.slotCount = slotCount;
        }

        private void Update()
        {
            second += Time.unscaledDeltaTime;
            if (second < 1)
                return;

            second -= 1;
            timeLeft -= 1;

            if (timeLeft < 0)
                timeLeft = 0;

            RefreshTimeTxt();
        }

        private void OnDisable()
        {
            scrollSeq.Kill();
        }

        public void RefreshTimer(double timeLeft)
        {
            this.timeLeft = timeLeft;
            RefreshTimeTxt();
        }

        public void SetRewardTarget(Sprite rewardIcon, string rewardCount, bool isDisplayRibbon, bool isDisplayInfinityIcon)
        {
            rewardTrackProgress.SetRewardTarget(rewardIcon, rewardCount, isDisplayRibbon, isDisplayInfinityIcon);
        }

        public void RefreshProgress(int current, int max)
        {
            rewardTrackProgress.RefreshProgress(current, max);
        }

        public void SetSliderToMax()
        {
            rewardTrackProgress.SetSliderToMax();
        }

        public void SwitchRewardTrackToCompleteState(bool isComplete)
        {
            rewardTrackProgress.SwitchRewardTrackToCompleteState(isComplete);
        }

        public void SetPlayerIdx(int value)
        {
            this.playerPosIdx = value;
        }

        public void PlayScrollAnimation(int scrollIdx)
        {
            if (playerPosIdx > scrollIdx)
            {
                PlayScrollAnimation(playerPosIdx, scrollIdx);
            }
        }

        public void InstantScroll(int scrollIdx)
        {
            InstantScroll(scrollIdx, playerPosIdx);
        }

        public void SetMultiplierIdx(int value)
        {
            multipliers.SetCurrentIdx(value);
        }

        public void SetCursorAnimated(int index)
        {
            multipliers.SetCursorAnimated(index);
        }

        public void SetMultiplierObjectActive(bool value)
        {
            multipliers.SetObjectActive(value);
        }

        public void RefreshSlotsViews(List<LeaderboardRecord> leaderboardRecords, SpriteAtlasService spriteAtlasService)
        {
            stickySlotController.UnpinTarget();

            var elements = scroller.Elements;

            for (int i = 0; i < elements.Count; i++)
            {
                var slot = elements[i].ElementDatasource;
                slot.SetViewIndex(i);

                if (i >= leaderboardRecords.Count)
                {
                    slot.SetObjectActive(false);
                    continue;
                }

                var lRecord = leaderboardRecords[i];

                PlayerMetaData playerMetaData = lRecord.metaData;
                var avatarSprite = spriteAtlasService.GetFromMain(playerMetaData.playerAvatar.avatarId);
                slot.SetCharacterData(avatarSprite, lRecord.displayName);
                slot.SetScoreText(lRecord.score.ToString());

                slot.SetPositionText((i + 1).ToString());

                SetPositionIcon(slot);

                var rootImg = lRecord.IsPlayer() ? playerBgImage : otherBgImage;
                slot.SetRootImage(rootImg);

                var stateImg = lRecord.IsPlayer() ? playerStateImage : otherStateImage;
                slot.SetStateImage(stateImg);

                SetRewardData(slot);
                slot.SetObjectActive(true);
            }

            scroller.RefreshVerticalLayout();
        }

        public void RefreshPlayerData(Sprite icon, string playerName)
        {
            var elements = scroller.Elements;

            if (playerPosIdx < 0 || playerPosIdx >= elements.Count)
                return;

            scroller.Elements[playerPosIdx].ElementDatasource.SetCharacterData(icon, playerName);
        }
        
        protected override void OnInitialize()
        {
            CreateSlotViews();
            headerTxt.text = LocalizationService.I.Get(LocKeys.Competition.StatePopupHeader);
            infoPopupBtn.onClick.AddListener(InfoPopupRequest);
            stickySlotController.Initialize();
            slotSplashFx.Initialize();
            multipliers.Initialize();
            rewardTooltip.Initialize();
            rewardTrackProgress.Initialize();
        }

        protected override void OnDeinitialize()
        {
            foreach (var element in scroller.Elements)
            {
                element.OnClicked -= CompetitionSlotView_OnClicked;
            }

            infoPopupBtn.onClick.RemoveAllListeners();
            OnInfoRequested = null;
            OnSlotClicked = null;
            scroller.Clear();
            stickySlotController.Deinitialize();
            rewardTooltip.Deinitialize();
            multipliers.Deinitialize();
            rewardTrackProgress.Deinitialize();
        }

        private void InfoPopupRequest()
        {
            OnInfoRequested?.Invoke();
        }

        private void RefreshTimeTxt()
        {
            timeLeftTxt.text = TimeUtils.GetTimeString((float)timeLeft, 100f);
        }

        private void CreateSlotViews()
        {
            for (int i = 0; i < slotCount; i++)
            {
                CompetitionSlotView slot = Instantiate(slotViewPf, slotsContentRoot);
                slot.Construct(slot, null, null);
                slot.SetParentForRoot(slotsContentRoot, false);
                slot.OnClicked += CompetitionSlotView_OnClicked;
                scroller.AddElement(slot);
            }
        }

        private void InstantScroll(int moveToIdx, int stickToIdx)
        {
            var slot = scroller.Elements[stickToIdx].ElementDatasource;
            scroller.InstantScrollAsync(moveToIdx, slot.RectHeight).Forget();
            stickySlotController.SetTarget(slot.ElementRect, slot.AnchoredPosDefault, slot.ViewIdx, slot.RectHeight);
        }

        private void PlayScrollAnimation(int fromIdx, int toIdx)
        {
            SetInteractiveElementsEnabled(false);

            moveCount = fromIdx - toIdx;
            var progress01 = Mathf.Clamp01(moveCount / (1f * (scroller.Elements.Count - 1)));
            var scrollDuration = Mathf.Lerp(scrollAnimParams.scrollDurationMin, scrollAnimParams.scrollDurationMax, progress01);
            perSlotMoveDuration = scrollDuration / moveCount;

            var movingSlot = scroller.Elements[fromIdx];
            movingSlot.ElementDatasource.SetParentForRoot(pinnedRoot, true);
            var swappedSlot = scroller.Elements[toIdx];

            var fromPosLocal = movingSlot.ElementRect.localPosition;
            var toPosLocal = GetFutureLocalPositionInViewport(pinnedRoot, swappedSlot.ElementRect, swappedSlot.RectHeight, toIdx);
            var sequence = BroTween.Sequence().SetUpdate(true);

            sequence.Append(BroTween.ScaleByCurve(movingSlot.ElementRect, scrollAnimParams.scaleDuration, scrollAnimParams.scaleCurve));

            sequence.Insert(scrollAnimParams.scaleDuration, BroTween.PositionLocal(movingSlot.ElementRect, fromPosLocal, toPosLocal, scrollDuration));
            sequence.Insert(scrollAnimParams.scaleDuration, BroTween.Float(SwapSlots, scrollDuration));
            sequence.Insert(scrollAnimParams.scaleDuration,
                            scroller.GetScrollTween(toIdx, movingSlot.ElementDatasource.RectHeight, scrollDuration, scrollAnimParams.scrollEaseCurve));

            sequence.Append(GetScaleDown(movingSlot));

            sequence.InsertCallback(sequence.Duration + 0.5f, () => { SetInteractiveElementsEnabled(true); });

            scrollSeq = sequence.ToSafe();
            scrollSeq.Play();
        }

        private BroTweenBase GetScaleDown(ScrollElement<CompetitionSlotView> playerSlot)
        {
            return BroTween.ScaleByCurve(playerSlot.ElementRect, scrollAnimParams.scaleDuration, scrollAnimParams.scaleCurve)
                           .SetPlayBackwards(true)
                           .OnComplete(() =>
                            {
                                var slot = playerSlot.ElementDatasource;
                                slot.SetParentForRoot(slotsContentRoot, true);
                                slot.SetSiblingIndex(slot.ViewIdx);
                                slot.SetAnchoredPositionDefault(slot.AnchoredPosCurrent);
                                stickySlotController.SetTarget(slot.ElementRect, slot.AnchoredPosDefault, slot.ViewIdx, slot.RectHeight);
                                PlaySplashFx(slot);
                            });
        }

        private void SwapSlots(float progress)
        {
            if (moveCount <= 0)
                return;

            var player = scroller.Elements[playerPosIdx];
            var swapSlot = scroller.Elements[playerPosIdx - 1];

            if (player.ElementRect.position.y <= swapSlot.ElementRect.position.y - scrollAnimParams.slotMinDistanceCheck)
                return;

            var idxDecr = player.ElementDatasource.ViewIdx - 1;
            player.ElementDatasource.SetViewIndex(idxDecr);
            player.ElementDatasource.SetPositionText((idxDecr + 1).ToString());
            SetPositionIcon(player.ElementDatasource);
            SetRewardData(player.ElementDatasource);

            var idxIncr = swapSlot.ElementDatasource.ViewIdx + 1;
            swapSlot.ElementDatasource.SetViewIndex(idxIncr);
            swapSlot.ElementDatasource.SetPositionText((idxIncr + 1).ToString());
            SetPositionIcon(swapSlot.ElementDatasource);
            SetRewardData(swapSlot.ElementDatasource);

            scroller.Elements[playerPosIdx] = swapSlot;
            scroller.Elements[playerPosIdx - 1] = player;

            var movePos = swapSlot.ElementDatasource.AnchoredPosDefault - new Vector2(0, swapSlot.ElementDatasource.RectHeight + scroller.VerticalSpacing);
            swapSlot.ElementDatasource.SetAnchoredPositionDefault(movePos);
            var slotMoveTween = BroTween.AnchoredPosition(swapSlot.ElementRect, movePos, perSlotMoveDuration)
                                        .SetUpdate(true)
                                        .SetEase(scrollAnimParams.slotMoveEaseCurve)
                                        .ToSafe();
            slotMoveTween.Play();

            moveCount--;
            playerPosIdx--;
        }

        /// <summary>
        /// Get the future position in the viewport after the scrolling process has completed for the targetElement if its ViewIdx == targetIdx
        /// </summary>
        private Vector2 GetFutureLocalPositionInViewport(RectTransform viewportRoot, RectTransform targetElement, float elementHeight, int targetIdx)
        {
            var currentContentPos = scroller.GetContentRootPosition();
            var futureContentPosY = scroller.GetContentRootPositionY(targetIdx, elementHeight);
            scroller.SetContentRootPosition(new Vector2(currentContentPos.x, futureContentPosY));
            var posInViewport = GetLocalPositionInViewPort(viewportRoot, targetElement.position);
            scroller.SetContentRootPosition(currentContentPos);
            return posInViewport;
        }

        private Vector2 GetLocalPositionInViewPort(RectTransform viewportRoot, Vector3 worldPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewportRoot,
                                                                    RectTransformUtility.WorldToScreenPoint(null, worldPos), null, out Vector2 localPos);
            return localPos;
        }

        private void SetPositionIcon(CompetitionSlotView slot)
        {
            var idx = slot.ViewIdx;

            if (idx >= positionIcons.Count)
            {
                slot.SetPositionIconActive(false);
                return;
            }

            slot.SetPositionIcon(positionIcons[idx]);
            slot.SetPositionIconActive(true);
        }

        private void SetRewardData(CompetitionSlotView slot)
        {
            var idx = slot.ViewIdx;

            if (idx >= rewardContainers.Count)
            {
                slot.SetRewardItemActive(false);
                return;
            }

            slot.SetRewardData(rewardContainers[idx], ShowRewardContainerToolTip);
            slot.SetRewardItemActive(true);
        }

        private void PlaySplashFx(CompetitionSlotView slot)
        {
            var localPos = GetLocalPositionInViewPort(pinnedRoot, slot.ElementRect.position);
            slotSplashFx.transform.localPosition = localPos - new Vector2(0, slot.RectHeight * 0.5f);
            slotSplashFx.Play();
        }

        private void SetInteractiveElementsEnabled(bool value)
        {
            scroller.SetScrollEnabled(value);

            foreach (var item in scroller.Elements)
            {
                item.ElementDatasource.SetButtonEnabled(value);
            }
        }

        protected void ShowRewardContainerToolTip(PurchaseRewardItemView clickedView)
        {
            rewardTooltip.Show(clickedView.RewardData, clickedView.TooltipTarget.position);
        }

        private void CompetitionSlotView_OnClicked(CompetitionSlotView view, RectTransform transform)
        {
            OnSlotClicked?.Invoke(view.ViewIdx);
        }
#if UNITY_EDITOR
        [TriInspector.Title("Debug")]
        [SerializeField] private int scrollFromNum;
        [SerializeField] private int scrollToNum;
        [SerializeField] private int splashPosNum;

        [TriInspector.Button]
        private void SplashFx_Editor()
        {
            var idx = splashPosNum - 1;

            idx = Mathf.Clamp(idx, 0, scroller.Elements.Count - 1);
            var player = scroller.Elements[idx].ElementDatasource;
            PlaySplashFx(player);
        }

        [TriInspector.Button]
        private void PlayScrollAnimation_Editor()
        {
            var idxFrom = scrollFromNum - 1;
            playerPosIdx = idxFrom;
            idxFrom = Mathf.Clamp(idxFrom, 0, scroller.Elements.Count - 1);

            var idxTo = scrollToNum - 1;
            idxTo = Mathf.Clamp(idxTo, 0, scroller.Elements.Count - 1);
            InstantScroll(idxFrom, idxFrom);

            PlayScrollAnimation(idxFrom, idxTo);
        }
#endif
    }
}