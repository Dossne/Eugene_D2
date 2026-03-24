using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.PlayerProfile;
using Infrastructure.BroTweens;
using Infrastructure.Configs;
using Infrastructure.HapticControl;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.Reward;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Features.LavaQuest
{
    public class LavaQuestEventPopupController
    {
        public event Action OnClosePopup;

        private readonly LavaQuestStateController stateController;
        private readonly PopupService popupService;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly LavaQuestConfig lavaQuestConfig;
        private readonly PlayerProfileController playerProfileController;
        private readonly List<(float distance, LavaQuestFallZone fallZone)> distancesTemp = new();
        private readonly List<CharacterIcon> activeIconsTemp = new();
        private readonly List<Vector3> stayPositionsTemp = new();
        private readonly List<Vector3> fallPositionsTemp = new();

        private CancellationTokenSource cts;
        private LavaQuestEventPopup eventPopup;
        private LavaQuestCongratulationPopup congratulationPopup;

        private BroTweenSafe popupAnimSeq;
        private BroTweenSafe playerAnimSeq;
        private BroTweenSafe hapticActionsSeq;

        //cached
        private RewardItemVisualData rewardVisualInfo;
        private bool isComplete;
        private int currentStepIndex = -1;
        private int playersFakeCountNextStep;

        private bool isOpenProcess;
        private bool isInit;


        public LavaQuestEventPopupController(LavaQuestStateController stateController,
                                             PopupService popupService,
                                             ConfigProvider configProvider,
                                             SpriteAtlasService spriteAtlasService,
                                             PlayerProfileController playerProfileController)
        {
            this.stateController = stateController;
            this.popupService = popupService;
            this.spriteAtlasService = spriteAtlasService;
            this.lavaQuestConfig = configProvider.LavaQuestConfig;
            this.playerProfileController = playerProfileController;
        }


        public void Initialize()
        {
            if (isInit || !stateController.IsFeatureEnabled())
                return;

            this.cts = new CancellationTokenSource();

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            cts.Cancel();
            cts.Dispose();
            isOpenProcess = false;
            
            if (eventPopup != null)
            {
                eventPopup.OnInfoRequested -= LavaQuestEventPopup_OnInfoRequested;
                eventPopup.OnChangeState -= LavaQuestEventPopup_OnChangeState;
                eventPopup.Deinitialize();
            }

            DisposeCongratulationsPopup();

            popupAnimSeq.Kill();
            playerAnimSeq.Kill();
            hapticActionsSeq.Kill();

            distancesTemp.Clear();
            stayPositionsTemp.Clear();
            fallPositionsTemp.Clear();
            activeIconsTemp.Clear();
            OnClosePopup = null;

            isInit = false;
        }

        public async UniTask OpenPopupOnState(LQEventPopupState popupState)
        {
            if (!stateController.IsFeatureEnabled())
                return;

            if (popupState == LQEventPopupState.ReadyToStart)
                currentStepIndex = -1;

            if (popupState == LQEventPopupState.FinishedInRuntime)
                stateController.SetReadyToStart();

            string description = popupState == LQEventPopupState.FinishedInRuntime
                               ? LocalizationService.I.Get(LocKeys.LavaQuest.EventFinished)
                               : LocalizationService.I.Get(LocKeys.LavaQuest.EventPopupDescr, stateController.MaxStepIdx.ToString());

            bool isLose            = popupState == LQEventPopupState.FinishedInRuntime;
            bool isMoveTop         = popupState == LQEventPopupState.FinishedInRuntime;
            bool needTapToContinue = popupState == LQEventPopupState.FinishedInRuntime;

            isOpenProcess = true;
            
            await PreparePopupAsync(stateController.LavaQuestStepData,
                                    stateController.CurrentChainId,
                                    stateController.CurrentStepIdx,
                                    stateController.MaxStepIdx,
                                    description,
                                    isLose,
                                    isMoveTop,
                                    needTapToContinue,
                                    stateController.GetCurrentPlayersFakeCount());
            
            eventPopup.Open();
            isOpenProcess = false;
        }

        public bool TryOpenPopupOnScheduledAction()
        {
            if (!stateController.IsFeatureEnabled())
                return false;

            if(isOpenProcess)
                return false;
            
            if (stateController.IsFinishedInRuntimeState())
            {
                OpenPopupOnState(LQEventPopupState.FinishedInRuntime).Forget();
                return true;
            }

            if (stateController.IsStartedState())
            {
                OpenPopupWithIconAnimationsAsync(stateController.IsScheduledLoose()).Forget();
                return true;
            }

            return false;
        }


        private async UniTask OpenPopupWithIconAnimationsAsync(bool isLoose)
        {
            isOpenProcess = true;

            //cache current state
            int currentStepIdx = stateController.CurrentStepIdx;
            int nextStepIdx = currentStepIdx + 1;
            int maxStepIdx = stateController.MaxStepIdx;
            int chainId = stateController.CurrentChainId;
            isComplete = !isLoose && currentStepIdx + 1 >= maxStepIdx;
            LavaQuestStepData currentStepConfig = stateController.LavaQuestStepData;
            string description = isLoose ? GetPopupLooseDescription() : GetPopupWinDescription(isComplete);
            int playersFakeCountCurrentStep = stateController.GetCurrentPlayersFakeCount();
            playersFakeCountNextStep = stateController.GetPlayersFakeCount(nextStepIdx);
            rewardVisualInfo = stateController.CurrentRewardVisualInfo;

            //change state, step idx etc by scheduled action
            stateController.ApplyScheduledAction();

            await PreparePopupAsync(currentStepConfig, chainId, currentStepIdx, maxStepIdx, description, isLoose, true, false, playersFakeCountCurrentStep);

            PrepareFallPositions(currentStepIdx, currentStepConfig.iconCountFall);

            foreach (var icon in activeIconsTemp)
            {
                icon.SetAlphaZero();
            }

            eventPopup.Open();

            await UniTask.WaitForSeconds(eventPopup.AnimParams.iconAnimationsDelay, true, cancellationToken: cts.Token);

            PlayIconsFadeIn();

            await UniTask.WaitForSeconds(eventPopup.AnimParams.iconFadeInDuration, true, cancellationToken: cts.Token);

            PlayPopupTopAnimations(nextStepIdx, maxStepIdx, playersFakeCountNextStep);
            PlayJumpAnimations(currentStepConfig, currentStepIdx, nextStepIdx, isLoose);

            eventPopup.DelayedActivateTapToContinue();
            
            isOpenProcess = false;
        }


        private void PlayPopupTopAnimations(int currentStepIdx, int maxStepIdx, int playersFakeCount)
        {
            popupAnimSeq.Kill();
            BroSequence seq = BroTween.Sequence().SetUpdate(true);

            //levels txt
            seq.Append(eventPopup.GetScaleByCurveTweenLevels(true));
            seq.AppendCallback(() => eventPopup.SetLevelsTxt(GetLevelsText(currentStepIdx, maxStepIdx)));
            seq.AppendInterval(eventPopup.AnimParams.levelChangeTextDuration);
            seq.Append(eventPopup.GetScaleByCurveTweenLevels(false));
            seq.AppendCallback(() => eventPopup.PlaySplashFxOnLevels());

            //players txt
            seq.Append(eventPopup.GetScaleByCurveTweenPlayers(true));
            seq.Append(eventPopup.GetChangeTextByCurveTween(isComplete ? playersFakeCount + 1 : playersFakeCount));
            seq.Append(eventPopup.GetScaleByCurveTweenPlayers(false));
            seq.AppendCallback(() => eventPopup.PlaySplashFxOnPlayers());
            
            popupAnimSeq = seq.ToSafe();
            popupAnimSeq.Play();
        }


        private void PlayJumpAnimations(LavaQuestStepData currentStepConfig, int currentStepIdx, int nextStepIdx, bool isPlayerFallOnLava)
        {
            if (!eventPopup.TryGetActiveEventZone(out LavaQuestEventZone eventZone))
                return;

            if (!eventZone.TryGetPlatform(nextStepIdx, out LavaQuestPlatform nextPlatform))
                return;

            playerAnimSeq.Kill();
            BroSequence playerSeq = BroTween.Sequence().SetUpdate(true);
                
            hapticActionsSeq.Kill();
            BroSequence hapticSeq = BroTween.Sequence().SetUpdate(true);

            
            int jumpCount = Math.Min(activeIconsTemp.Count - currentStepConfig.iconCountFall, activeIconsTemp.Count);
            PrepareStayPositions(nextPlatform.ItemPlaceRect, jumpCount);

            float jumpDelay = 0;

            if (isPlayerFallOnLava && activeIconsTemp.Count > jumpCount)
            {
                var playerIcon = activeIconsTemp[0];
                activeIconsTemp.RemoveAt(0);
                activeIconsTemp.Insert(jumpCount, playerIcon);
            }

            for (int i = 0; i < jumpCount; i++)
            {
                CharacterIcon icon = activeIconsTemp[i];
                Vector3 from = icon.CurrentPos;
                Vector3 to = stayPositionsTemp[i];

                float playerJumpDelay = i == 1 && !isPlayerFallOnLava ? eventPopup.AnimParams.enemiesStartMoveDelay : 0;

                float enemyDelay = i > 1 ? Random.Range(eventPopup.AnimParams.enemiesBetweenDelayMin, eventPopup.AnimParams.enemiesBetweenDelayMax) : 0;
                jumpDelay += playerJumpDelay + enemyDelay;

                playerSeq.Insert(jumpDelay, icon.GetJumpSequence(eventPopup.AnimParams.iconMoveCurve, eventPopup.AnimParams.iconMoveDuration, from, to));

                if (i + 1 <= eventPopup.AnimParams.platformLandHapticCount)
                {
                    hapticSeq.InsertCallback(jumpDelay + eventPopup.AnimParams.iconMoveDuration, () => PlayHaptic(eventPopup.AnimParams.platformLandHaptic));
                }
            }

            jumpDelay += eventPopup.AnimParams.iconMoveDuration;

            int fallPosIdx = 0;

            if (eventZone.TryGetPlatform(currentStepIdx, out LavaQuestPlatform currentPlatform) && currentPlatform.IsMoveInLava)
            {
                playerSeq.AppendCallback(() => currentPlatform.AnimateMoveDown());
            }

            float lavaJumpDuration = eventPopup.AnimParams.iconJumpLavaDuration;

            for (int i = jumpCount; i < activeIconsTemp.Count; i++)
            {
                CharacterIcon icon = activeIconsTemp[i];
                Vector3 from = icon.CurrentPos;
                Vector3 toJump = fallPositionsTemp[fallPosIdx];

                //jump
                playerSeq.Insert(jumpDelay, icon.GetJumpFallSequence(eventPopup.AnimParams.iconMoveCurve, lavaJumpDuration, from, toJump));

                //move down to lava. rotate
                float rotationZ = Random.Range(eventPopup.AnimParams.iconRotationZMin, eventPopup.AnimParams.iconRotationZMax);
                playerSeq.Insert(jumpDelay + lavaJumpDuration,
                                     icon.GetLocalRotateByZTween(eventPopup.AnimParams.iconFallRotationCurve, eventPopup.AnimParams.iconRotationDuration, rotationZ));

                //move down to lava. move
                Vector3 toFall = toJump + eventPopup.AnimParams.iconFallTargetOffset;
                playerSeq.Insert(jumpDelay + lavaJumpDuration,
                                     icon.GetMoveDownSequence(eventPopup.AnimParams.iconFallCurve, eventPopup.AnimParams.iconFallDuration, toJump, toFall));
                if (fallPosIdx == 0)
                {
                    AddLavaFallHaptics(hapticSeq, jumpDelay + lavaJumpDuration);
                }

                fallPosIdx++;
            }
            
            playerAnimSeq = playerSeq.ToSafe();
            playerAnimSeq.Play();

            hapticActionsSeq = hapticSeq.ToSafe();
            hapticActionsSeq.Play();
        }


        private async UniTask PreparePopupAsync(LavaQuestStepData stepConfig, 
                                             int chainId, 
                                             int currentStepIdx, 
                                             int maxStepIdx, 
                                             string description, 
                                             bool isLose, 
                                             bool isMoveTop,
                                             bool needTapToContinue,
                                             int playersFakeCount)
        {
            if (eventPopup == null)
            {
                eventPopup = await popupService.GetAsync<LavaQuestEventPopup>(cts.Token, true, false);
                eventPopup.OnInfoRequested += LavaQuestEventPopup_OnInfoRequested;
                eventPopup.OnChangeState += LavaQuestEventPopup_OnChangeState;
                eventPopup.Initialize();
            }

            Sprite billboardRewardIcon = GetBillboardRewardIcon();
            eventPopup.Refresh(billboardRewardIcon,
                               stateController.CurrentRewardVisualInfo.amountText,
                               description,
                               GetLevelsText(currentStepIdx, maxStepIdx),
                               playersFakeCount,
                               stateController.TimeRest,
                               isLose,
                               isMoveTop);

            if (playerProfileController.FeatureEnabled)
                eventPopup.UpdatePlayerIcon(spriteAtlasService.GetFromMain(playerProfileController.PlayerProfileData.avatarId));

            eventPopup.ActivateAsInvisible();

            if (currentStepIndex != currentStepIdx)
            {
                PlaceCharacterIcons(stepConfig, chainId, currentStepIdx);
                currentStepIndex = currentStepIdx;
            }

            if (needTapToContinue)
                eventPopup.DelayedActivateTapToContinue();
        }


        private string GetLevelsText(int current, int max)
        {
            return $"{current.ToString()}/{max.ToString()}";
        }


        private void PlaceCharacterIcons(LavaQuestStepData stepConfig, int chainId, int stepIdx)
        {
            if (!eventPopup.TrySetEventZoneActive(chainId, out LavaQuestEventZone eventZone))
                return;

            if (!eventZone.TryGetPlatform(stepIdx, out LavaQuestPlatform platform))
                return;

            for (int i = 0; i < eventZone.Platform.Count; i++)
            {
                LavaQuestPlatform evPlatform = eventZone.Platform[i];
                evPlatform.SetObjectActive(!evPlatform.IsMoveInLava || i >= stepIdx);

                if (evPlatform.IsMoveInLava && i >= stepIdx)
                    evPlatform.ResetState();
            }

            activeIconsTemp.Clear();

            int viewCreateCount = stepConfig.iconCountTotal - eventPopup.CharacterIcons.Count;

            for (int i = 0; i < viewCreateCount; i++)
            {
                eventPopup.CreateNewIconInstance();
            }

            PrepareStayPositions(platform.ItemPlaceRect, stepConfig.iconCountTotal);
            List<(Sprite icon, Sprite back)> sprites = GetIconSprites();

            for (int i = 0; i < eventPopup.CharacterIcons.Count; i++)
            {
                CharacterIcon icon = eventPopup.CharacterIcons[i];

                if (i <= stepConfig.iconCountTotal - 1)
                {
                    int spriteIndex = i;

                    if (i >= sprites.Count - 1)
                    {
                        spriteIndex = Random.Range(1, sprites.Count); //0 is player
                    }

                    icon.SetIcon(sprites[spriteIndex].icon, sprites[spriteIndex].back);

                    icon.SetObjectActive(true);
                    Vector2 pos = stayPositionsTemp[i];
                    icon.SetPosition(pos);
                    icon.ResetToDefaultState();
                    activeIconsTemp.Add(icon);

                }
                else
                {
                    icon.SetObjectActive(false);
                }
            }
        }


        private void PlayIconsFadeIn()
        {
            this.playerAnimSeq.Kill();
            var seq = BroTween.Sequence().SetUpdate(true);

            foreach (var icon in activeIconsTemp)
            {
                seq.Insert(0, icon.GetFadeInIconTween(eventPopup.AnimParams.iconFadeInCurve, eventPopup.AnimParams.iconFadeInDuration));
                seq.Insert(0, icon.GetFadeInBackTween(eventPopup.AnimParams.iconFadeInCurve, eventPopup.AnimParams.iconFadeInDuration));
            }

            this.playerAnimSeq = seq.ToSafe();
            this.playerAnimSeq.Play();
        }


        private List<(Sprite icon, Sprite back)> GetIconSprites()
        {
            Sprite playerIcon = null;
            Sprite playerBack = null;
            List<(Sprite icon, Sprite back)> iconSprites = new List<(Sprite icon, Sprite back)>();

            foreach (var iconData in lavaQuestConfig.Icons)
            {
                var spriteIcon = spriteAtlasService.GetFromMain(iconData.name);
                var backIcon   = spriteAtlasService.GetFromMain(iconData.back);

                if (iconData.enemy)
                {
                    iconSprites.Add((spriteIcon, backIcon));
                }
                else
                {
                    if (playerProfileController.FeatureEnabled)
                        playerIcon = spriteAtlasService.GetFromMain(playerProfileController.PlayerProfileData.avatarId);
                    else
                        playerIcon = spriteIcon;
                    playerBack = backIcon;
                }                    
            }

            GameplayUtils.ShuffleElements(iconSprites);

            iconSprites.Insert(0, (playerIcon, playerBack));
            return iconSprites;
        }


        private string GetPopupWinDescription(bool isComplete)
        {
            return isComplete
                ? LocalizationService.I.Get(LocKeys.LavaQuest.EventComplete)
                : LocalizationService.I.Get(LocKeys.LavaQuest.EventPopupDescr, stateController.MaxStepIdx.ToString());
        }


        private string GetPopupLooseDescription()
        {
            return LocalizationService.I.Get(LocKeys.LavaQuest.EventLoose);
        }


        private void PrepareStayPositions(RectTransform platform, int count)
        {
            float minYSpawn = eventPopup.MinSpawnYDistance;

            Vector2 size = platform.rect.size;
            float top = size.y * 0.5f;
            float bottom = -size.y * 0.5f;
            float currentY = bottom;

            int maxCountInRow = 1;
            int currentCountInRow = 0;

            stayPositionsTemp.Clear();

            for (int i = 0; i < count; i++)
            {
                Vector2 localPos = i == 0
                    ? new Vector2(0f, currentY)
                    : GetRandomLocalPointInRect(platform, currentY);

                Vector3 worldPos = platform.TransformPoint(localPos);
                stayPositionsTemp.Add(worldPos);

                currentCountInRow++;

                if (currentCountInRow >= maxCountInRow)
                {
                    maxCountInRow += currentCountInRow;
                    currentCountInRow = 0;

                    currentY += minYSpawn;
                    currentY = Mathf.Min(currentY, top);
                }
            }
        }


        private void PrepareFallPositions(int platformId, int fallPosCount)
        {
            if (!eventPopup.TryGetActiveEventZone(out LavaQuestEventZone eventZone))
                return;

            if (!eventZone.TryGetPlatform(platformId, out LavaQuestPlatform platform))
                return;

            Vector3 from = platform.ItemPlaceRect.position;

            SortDistances(eventZone.FallZones, distancesTemp, from);
            fallPositionsTemp.Clear();

            if (distancesTemp.Count == 0)
            {
                Debug.LogError("[LavaQuest] No fall zones found");
                return;
            }

            for (int i = 0; i < fallPosCount; i++)
            {
                RectTransform rect = i <= distancesTemp.Count - 1 ? distancesTemp[i].fallZone.Root : distancesTemp[^1].fallZone.Root;
                Vector3 localPos = GetRandomLocalPointInRect(rect);
                Vector3 worldPos = rect.TransformPoint(localPos);
                fallPositionsTemp.Add(worldPos);
            }
        }


        private void SortDistances(List<LavaQuestFallZone> input, List<(float distance, LavaQuestFallZone fallZone)> result, Vector3 fromPos)
        {
            if (input == null || input.Count == 0)
                return;

            result.Clear();

            foreach (var item in input)
            {
                if (item == null)
                    continue;

                result.Add((Vector3.Distance(item.Root.position, fromPos), item));
            }

            result.Sort((left, right) => left.distance.CompareTo(right.distance));
        }


        private Vector3 GetRandomLocalPointInRect(RectTransform tr)
        {
            float x = tr.rect.width * 0.5f;
            float y = tr.rect.height * 0.5f;
            return new Vector3(Random.Range(-x, x), Random.Range(-y, y), 0f);
        }


        private Vector2 GetRandomLocalPointInRect(RectTransform tr, float y)
        {
            float x = Random.Range(-tr.rect.width * 0.5f, tr.rect.width * 0.5f);
            return new Vector2(x, y);
        }


        private async UniTaskVoid OpenInfoPopupAsync()
        {
            LavaQuestInfoPopup infoPopup = await popupService.OpenAsync<LavaQuestInfoPopup>(cts.Token);

            int maxPlayersCount = stateController.GetFirstLavaQuestStepData().maxPlayers;
            int levelsCount = stateController.MaxStepIdx;
            string rewardCount = stateController.CurrentRewardVisualInfo.amountText;

            string playersTxt = LocalizationService.I.Get(LocKeys.LavaQuest.InfoPopupPlayers, maxPlayersCount.ToString());
            string levelsTxt = LocalizationService.I.Get(LocKeys.LavaQuest.InfoPopupLevels, levelsCount.ToString());
            string rewardCountTxt = rewardCount;
            string shareCoinsTxt = LocalizationService.I.Get(LocKeys.LavaQuest.InfoPopupShare, rewardCount);

            Sprite rewardIcon = GetBillboardRewardIcon();
            infoPopup.SetTexts(playersTxt, levelsTxt, rewardIcon, rewardCountTxt, shareCoinsTxt);

        }


        private async UniTaskVoid OpenCongratulationPopupAsync(RewardItemVisualData displayReward, int playersFakeCount)
        {
            if (congratulationPopup == null)
            {
                congratulationPopup = await popupService.GetAsync<LavaQuestCongratulationPopup>(cts.Token, false, false);
                congratulationPopup.OnChangeState += LavaQuestCongratulationPopup_OnChangeState;
                congratulationPopup.Initialize();
            }

            displayReward.amount = stateController.GetDividedByPlayersAmount(displayReward.amount, playersFakeCount);
            string sharingText = LocalizationService.I.Get(LocKeys.LavaQuest.CongratPopupSharing, playersFakeCount.ToString());

            List<(Sprite, Sprite)> characterSprites = activeIconsTemp is { Count: > 0 } 
                                                    ? activeIconsTemp.Select(x => (x.CurrentSprite, x.CurrentBack)).ToList() 
                                                    : GetIconSprites();
            Sprite billboardRewardIcon = GetBillboardRewardIcon();

            congratulationPopup.Construct(characterSprites,
                                          displayReward.icon,
                                          billboardRewardIcon,
                                          displayReward.amount,
                                          displayReward.amountText,
                                          displayReward.isDisplayRibbon,
                                          displayReward.isDisplayInfinityIcon,
                                          sharingText);

            congratulationPopup.Open();
        }


        private void DisposeCongratulationsPopup()
        {
            if (congratulationPopup == null)
                return;

            congratulationPopup.OnChangeState -= LavaQuestCongratulationPopup_OnChangeState;
            popupService.Dispose(congratulationPopup);
            congratulationPopup = null;
        }


        private void AddLavaFallHaptics(BroSequence hapticActionsSeq, float lavaFallDelay)
        {
            float lavaHapticDelay = lavaFallDelay;

            for (int i = 0; i < eventPopup.AnimParams.lavaFallHaptics.Length; i++)
            {
                var hAction = eventPopup.AnimParams.lavaFallHaptics[i];
                lavaHapticDelay += hAction.activateDelaysSec;

                hapticActionsSeq.InsertCallback(lavaHapticDelay, () => PlayHaptic(hAction.hapticType));
            }
        }


        private void PlayHaptic(HapticType hapticType)
        {
            HapticService.I.Haptic(hapticType);
        }


        private Sprite GetBillboardRewardIcon()
        {
            return spriteAtlasService.GetFromMain(stateController.BillboardRewardIcon);
        }


        private void LavaQuestEventPopup_OnInfoRequested()
        {
            OpenInfoPopupAsync().Forget();
        }


        private void LavaQuestEventPopup_OnChangeState(PopupBase eventPopup, PopupState state)
        {
            if (state is not PopupState.BeginClose)
            {
                return;
            }
            
            playerAnimSeq.Complete(true);
            popupAnimSeq.Complete(true);
            hapticActionsSeq.Kill();
            
            if (isComplete)
            {
                OpenCongratulationPopupAsync(rewardVisualInfo, playersFakeCountNextStep).Forget();
                isComplete = false;
            }
            else
            {
                OnClosePopup?.Invoke();
            }
        }


        private void LavaQuestCongratulationPopup_OnChangeState(PopupBase obj, PopupState state)
        {
            if(state is not PopupState.BeginClose)
                return;
            
            OnClosePopup?.Invoke();
        }



#if UNITY_EDITOR
        public void OpenCongratulationsPopup_Editor()
        {
            OpenCongratulationPopupAsync(stateController.CurrentRewardVisualInfo, stateController.GetPlayersFakeCount(stateController.MaxStepIdx)).Forget();
        }


        public (Vector3 from, List<Vector3> to) GetFallPoints_Editor()
        {
            if (eventPopup == null
             || !eventPopup.IsOpened
             || !eventPopup.TryGetActiveEventZone(out LavaQuestEventZone eventZone)
             || !eventZone.TryGetPlatform(stateController.CurrentStepIdx, out LavaQuestPlatform platform))
            {
                return (Vector3.zero, new List<Vector3>());
            }

            LavaQuestStepData currentStepConfig = stateController.LavaQuestStepData;
            int currentStepIdx = stateController.CurrentStepIdx;

            PrepareFallPositions(currentStepIdx, currentStepConfig.iconCountFall);
            Vector3 from = platform.ItemPlaceRect.position;
            return (from, fallPositionsTemp);
        }
#endif

#if PR_CHEAT
        public void CheatReset()
        {
            currentStepIndex = -1;
        }


        public void CheatClosePopup()
        {
            if(eventPopup != null)
                eventPopup.Close();
        }
#endif
    }
}