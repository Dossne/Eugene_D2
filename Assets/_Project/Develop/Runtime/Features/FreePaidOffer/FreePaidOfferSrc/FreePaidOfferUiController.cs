using Cysharp.Threading.Tasks;
using Features.Level;
using Features.PurchaseUi;
using Features.Widgets;
using Infrastructure.Pool.FloatingIcon;
using Infrastructure.Pool.FloatingText;
using Infrastructure.AssetManagement;
using Infrastructure.Configs;
using Infrastructure.HapticControl;
using Infrastructure.Localization;
using Infrastructure.MainUICanvasControl;
using Infrastructure.PersistentProgress;
using Infrastructure.Pool;
using Infrastructure.Popups;
using Infrastructure.PurchaseSystem;
using Infrastructure.Reward;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using Infrastructure.BroTweens;
using UnityEngine;
using VContainer;
using VContainer.Unity;


namespace Features.FreePaidOffer
{
    public class FreePaidOfferUiController : ITickable, ISavable
    {
        private Instantiator instantiator;
        private PopupService popupService;
        private IPurchaseManager purchaseManager;
        private ConfigProvider configProvider;
        private SpriteAtlasService spriteAtlasService;
        private PurchaseOfferContainer purchaseOfferContainer;
        private RewardApplyController rewardApplyController;
        private FreePaidOfferService freePaidOfferService;
        private LevelService levelService;
        private WidgetManager widgetManager;
        private PoolService poolService;
        FreePaidOfferAnimationConfig animConfig;

        private FreePaidOfferState freePaidOfferState = new();
        private FreePaidOfferPopup freePaidOfferPopup;
        private FreePaidOfferBoughtPopup freePaidOfferBoughtPopup;
        private FloatingTextPool floatingTextPool;
        private FloatingPurchaseRewardItemViewPool floatingPurchaseRewardItemViewPool;
        private FloatingIconPool floatingIconPool;
        private ParticlesPool particlesPool;
        private int lastRestSec = int.MaxValue;
        private CancellationTokenSource cancellationTokenSource;
        private OpeningMethod lastOfferOpeningMethod = OpeningMethod.Yourself;
        private string lastOfferSource = "";
        bool isWidgetRefreshing = false; 
        bool isOffersRefreshing = false;
        List<BroTweenSafe> animSequences = new();
        private HashSet<OfferId> trackedOffers = new HashSet<OfferId>();


        public FreePaidOfferSkinData CurrentSkinData => configProvider.FreePaidOfferSkinConfiguration.GetFreePaidOfferSkin(CurrentSkin);
        public FreePaidOfferSkinType CurrentSkin => freePaidOfferState.freePaidOfferSkinType;
        public bool IsInitialized { get; private set; }

        public bool FreePaidOfferPopupClosed => !freePaidOfferPopup.IsOpened;
        public bool FreePaidOfferBoughtPopupClosed => !freePaidOfferBoughtPopup.IsOpened;

        [Inject]
        public FreePaidOfferUiController(Instantiator instantiator,
            PopupService popupService,
            SpriteAtlasService spriteAtlasService,
            IPurchaseManager purchaseManager,
            PurchaseOfferContainer purchaseOfferContainer,
            ConfigProvider configProvider,
            RewardApplyController rewardApplyController,
            FreePaidOfferService freePaidOfferService,
            LevelService levelService,
            WidgetManager widgetManager,
            PoolService poolService)
        {
            this.instantiator = instantiator;
            this.popupService = popupService;
            this.spriteAtlasService = spriteAtlasService;
            this.configProvider = configProvider;
            this.purchaseManager = purchaseManager;
            this.purchaseOfferContainer = purchaseOfferContainer;
            this.rewardApplyController = rewardApplyController;
            this.freePaidOfferService = freePaidOfferService;
            this.levelService = levelService;
            this.widgetManager = widgetManager;
            this.poolService = poolService;
        }


        void ISavable.Load(Infrastructure.PersistentProgress.Progress progress)
        {
            freePaidOfferState = progress.freePaidOfferState;
        }

        void ISavable.Save(Infrastructure.PersistentProgress.Progress progress)
        {
            progress.freePaidOfferState = freePaidOfferState;
        }


        public async UniTask InitializeAsync()
        {
            if (IsInitialized)
            {
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();

            floatingTextPool = poolService.Get<FloatingTextPool>();
            floatingPurchaseRewardItemViewPool = poolService.Get<FloatingPurchaseRewardItemViewPool>();
            floatingIconPool = poolService.Get<FloatingIconPool>();
            particlesPool = poolService.Get<ParticlesPool>();
            animConfig = configProvider.FreePaidOfferAnimationConfig;
            RefreshWidgetAsync().Forget();
            await InitializePopupAsync();
            await InitializeBoughtPopupAsync();

            levelService.OnLevelIncremented += LevelService_OnLevelIncremented;
            freePaidOfferService.OnCycleReset += FreePaidOfferService_OnCycleReset;
            purchaseOfferContainer.OnPurchaseCompleted += PurchaseOfferContainer_OnPurchaseCompleted;
            freePaidOfferPopup.OnChangeState += FreePaidOfferPopup_OnChangeState;
            freePaidOfferBoughtPopup.OnChangeState += FreePaidOfferBoughtPopup_OnChangeState;
            IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!IsInitialized)
                return;

            levelService.OnLevelIncremented -= LevelService_OnLevelIncremented;
            freePaidOfferService.OnCycleReset -= FreePaidOfferService_OnCycleReset;
            purchaseOfferContainer.OnPurchaseCompleted -= PurchaseOfferContainer_OnPurchaseCompleted;
            freePaidOfferBoughtPopup.OnChangeState -= FreePaidOfferBoughtPopup_OnChangeState;
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            if (freePaidOfferPopup != null)
            {
                freePaidOfferPopup.OnChangeState -= FreePaidOfferPopup_OnChangeState;
                freePaidOfferPopup.Deinitialize();
            }

            IsInitialized = false;
        }


        public void ShowFreePaidOfferPopup(OpeningMethod openingMethod, string source)
        {
            lastOfferSource = source;
            lastOfferOpeningMethod = openingMethod;

            freePaidOfferPopup.RefreshSkin(spriteAtlasService.GetFromMain(CurrentSkinData.popupHeaderSpriteId), LocalizationService.I.Get(CurrentSkinData.popupHeaderLocKey));
            RefreshOffersPrice();
            RefreshCurrentSlotNotifier();
            
            freePaidOfferPopup.Open();
            SendIapOfferAnalytics();
        }

        
        public bool IsOfferActive()
        {
            return IsInitialized
                && freePaidOfferService.IsOfferActive();
        }


        void ITickable.Tick()
        {
            if(!freePaidOfferService.IsOfferActive())
            {
                return;
            }

            double timeRest = freePaidOfferService.TimeRest();
            if(timeRest <= 0)
            {
                ClosePopup();
                widgetManager.RemoveWidget(WidgetId.FreePaidOffer);
                return;
            }

            int restSec = (int)Math.Floor(timeRest);
            if (restSec < lastRestSec)
            {
                OnSecondTick(timeRest);
                lastRestSec = restSec;
            }
        }


        private void OnSecondTick(double timeRestSec)
        {
            string timeString = TimeRestString(timeRestSec);
            widgetManager.SetWidgetText(WidgetId.FreePaidOffer, timeString);
            if (freePaidOfferPopup != null)
            {
                freePaidOfferPopup.SetRestTime(timeString);
            }
        }


        private string TimeRestString(double timeRestSec)
        {
            return timeRestSec < 86400 ? TimeUtils.GetTimeString(timeRestSec) : TimeUtils.GetTimeString(timeRestSec, 100f);
        }


        private void ClosePopup()
        {
            if(freePaidOfferPopup != null)
            {
                freePaidOfferPopup.Close();
            }
        }


        private async UniTask InitializePopupAsync()
        {
            if (freePaidOfferPopup == null)
            {
                freePaidOfferPopup = await popupService.GetAsync<FreePaidOfferPopup>(cancellationTokenSource.Token, true);
                await RefreshOffers();
                freePaidOfferPopup.Initialize();
            }
        }


        private async UniTask RefreshOffers()
        {
            if (freePaidOfferPopup == null)
            {
                return;
            }

            CancellationTokenRegistration reg = cancellationTokenSource.Token.Register(() => isOffersRefreshing = false);
            await UniTask.WaitUntil(() => isOffersRefreshing == false, cancellationToken: cancellationTokenSource.Token);
            isOffersRefreshing = true;

            freePaidOfferPopup.RemoveOffers();

            int idx = 0;
            int presetId = freePaidOfferService.CurrentPresetId;
            int slotsAmount = configProvider.FreePaidOfferConfig.GetOffersCount(presetId);
            List<FreePaidOfferRewardData> rewardDatas = configProvider.FreePaidOfferConfig.GetRewardsData(presetId);
            for (int i = 0; i < rewardDatas.Count; ++i)
            {
                FreePaidOfferRewardData data = rewardDatas[i];
                GameObject slotGO = await instantiator.InstantiateAsync(CurrentSkinData.offerSlotRef, cancellationToken: cancellationTokenSource.Token);
                FreePaidOfferSlot slot = slotGO.GetComponent<FreePaidOfferSlot>();
                ComplexReward complexReward = RewardUtils.ConvertFromJson<ComplexReward>(data.rewardJson);
                string buttonText = data.InAppOfferId == OfferId.none
                    ? LocalizationService.I.Get(FreePaidOfferLocalization.SlotBtn)
                    : purchaseOfferContainer.GetOfferUiDatasource(data.InAppOfferId)?.priceString;

                slot.Initialize(complexReward, () => OnSlotBuyButtonClick(data.InAppOfferId, complexReward, slot), 
                    spriteAtlasService, 
                    configProvider.BoosterConfig.PreAndInGameBoosters(), 
                    idx,
                    CurrentSkinData, 
                    buttonText);
                freePaidOfferPopup.AddSlot(slot, idx, freePaidOfferService.CurrentOfferIdx, slotsAmount);
                ++idx;
            }

            freePaidOfferPopup.RefreshSlots(freePaidOfferService.CurrentOfferIdx);
            
            isOffersRefreshing = false;
            reg.Dispose();
        }


        private void RefreshOffersPrice()
        {
            int presetId = freePaidOfferService.CurrentPresetId;
            List<FreePaidOfferRewardData> rewardDatas = configProvider.FreePaidOfferConfig.GetRewardsData(presetId);
            for (int i = 0; i < rewardDatas.Count; ++i)
            {
                FreePaidOfferRewardData rewardData = rewardDatas[i];
                if (rewardData.InAppOfferId != OfferId.none
                    && freePaidOfferPopup.Slots.Count > i)
                {
                    string priceText = purchaseOfferContainer.GetOfferUiDatasource(rewardData.InAppOfferId)?.priceString;
                    freePaidOfferPopup.Slots[i].SetPriceText(priceText);
                }
            }

        }


        private async UniTask InitializeBoughtPopupAsync()
        {
            if (freePaidOfferBoughtPopup == null)
            {
                freePaidOfferBoughtPopup = await popupService.GetAsync<FreePaidOfferBoughtPopup>(cancellationTokenSource.Token, true);
                freePaidOfferBoughtPopup.Construct();
                freePaidOfferBoughtPopup.Initialize();
            }
        }


        private void OnSlotBuyButtonClick(OfferId inAppOfferId, ComplexReward complexReward, FreePaidOfferSlot slot)
        {
            if(slot.Idx > freePaidOfferService.CurrentOfferIdx)
            {
                BroTween.Sequence()
                        .Append(BroTween.RotationLocal(slot.LockImage.transform, new Vector3(0, 0, animConfig.lockRotationOnClickMaxZAngle), animConfig.lockRotationOnClickDuration)
                                        .SetEase(animConfig.lockRotationOnClickCurve))
                        .SetUpdate(true)
                        .Play();
                ShowFloatingText(slot.ButtonTransform.position);
                return;
            }

            if (inAppOfferId == OfferId.none)
            {
                rewardApplyController.ApplyComplexReward(complexReward, "rh_free_paid_free");
                OnOfferApplied(inAppOfferId);
                ShowAnim();
            }
            else
            {
                purchaseManager.InitiatePurchase(inAppOfferId, lastOfferOpeningMethod, lastOfferSource);
            }
        }


        private void OnOfferApplied(OfferId inAppOfferId)
        {
            freePaidOfferService.OnOfferApplied(inAppOfferId, CurrentSkin);
        }


        private async UniTaskVoid RefreshWidgetAsync()
        {
            CancellationTokenRegistration reg = cancellationTokenSource.Token.Register(() => isWidgetRefreshing = false);
            await UniTask.WaitUntil(() => !isWidgetRefreshing, cancellationToken: cancellationTokenSource.Token);

            isWidgetRefreshing = true;

            if (!freePaidOfferService.IsOfferActive())
            {
                RemoveWidget();
            }
            else
            {
                await CreateWidgetAsync();
            }

            isWidgetRefreshing = false;
            reg.Dispose();
            freePaidOfferService.OnOfferRefreshed(CurrentSkin);
            RefreshWidgetNotifier();
        }


        private void RefreshWidgetNotifier()
        {
            bool isCurrentFree = freePaidOfferService.IsCurrentFreeReward();
            widgetManager.RefreshWidgetNotifier(WidgetId.FreePaidOffer, isCurrentFree);
        }
        
        
        private void RefreshCurrentSlotNotifier()
        {
            bool isCurrentFree = freePaidOfferService.IsCurrentFreeReward();
            if(freePaidOfferPopup.GetSlot(freePaidOfferService.CurrentOfferIdx, out FreePaidOfferSlot nextSlot))
            {
                nextSlot.SetNotifierActive(isCurrentFree);
            }
        }
        
        
        private void RemoveWidget()
        {
            widgetManager.RemoveWidget(WidgetId.FreePaidOffer);
        }


        private UniTask CreateWidgetAsync()
        {
            string timeString = TimeRestString(freePaidOfferService.TimeRest());
            return widgetManager.RecreateWidgetAsync(WidgetId.FreePaidOffer,  
                                                     CurrentSkinData.widgetRef.AssetGUID, 
                                                     timeString, 
                                                     OnWidgetClick, 
                                                     freePaidOfferService.IsUnlocked(), 
                                                     cancellationTokenSource.Token);
        }


        private void OnWidgetClick()
        {
            ShowFreePaidOfferPopup(OpeningMethod.Yourself, PurchaseOfferShowSource.FreePaidOfferWidget);
        }


        private void ShowFloatingText(Vector2 position)
        {
            if (floatingTextPool.TryGetItem(FloatingTextType.FreePaidOfferSlot, out PoolableFloatingText flyText))
            {
                flyText.Show(LocalizationService.I.Get(FreePaidOfferLocalization.SlotLocked), position, Vector2.one, isAnchorPos: false);
            }
        }


        private void ShowFloatingPurchaseRewardItemView(PurchaseRewardItemView purchaseRewardItemView)
        {
            if(floatingPurchaseRewardItemViewPool.TryGetItem(FloatingPurchaseRewardItemViewType.FreePaidOfferSlot, out PoolableFloatingPurchaseRewardItemView item))
            {
                item.Show(purchaseRewardItemView.IconSprite, 
                    purchaseRewardItemView.LabelText, 
                    purchaseRewardItemView.IsDisplayRibbon,
                    purchaseRewardItemView.IsDisplayInfinityIcon,
                    purchaseRewardItemView.transform.position,
                    Vector3.one,
                    isAnchorPos: false);
            }
        }


        private void PlayParticles(Vector3 position)
        {
            if (particlesPool.TryGetItem(PoolableParticleType.FreePaidOfferSparks, out PoolableParticleSystem fx))
            {
                fx.transform.position = position;
            }
        }


        private void ShowFloatingLockIcon(Transform startPosition, Vector3 finishPosShiftedFromStart)
        {
            if (floatingIconPool.TryGetItem(FloatingIconType.FreePaidOfferSlotLock, out PoolableFloatingIcon lockIcon))
            {
                lockIcon.ShowAndFly(icon: null, startPosition, finishPosShiftedFromStart);
            }
        }


        private void ShowAnim()
        {
            int claimedOfferIdx = freePaidOfferService.CurrentOfferIdx - 1;
            freePaidOfferPopup.GetSlot(claimedOfferIdx, out FreePaidOfferSlot claimedSlot);
            claimedSlot.SetButtonInteractable(false);
            List<PurchaseRewardItemView> rewardItemViewList = claimedSlot.PurchaseRewardGridView.PurchaseRewardItemViewList;
            float timeToFloatingAllRewards = animConfig.rewardFirstFloatDelay;
            bool isNeedMoveSlots = !freePaidOfferPopup.IsSlotDisplayed(claimedOfferIdx, claimedOfferIdx + 1);
            if(freePaidOfferPopup.GetSlot(freePaidOfferService.CurrentOfferIdx, out FreePaidOfferSlot nextSlot))
            {
                nextSlot.SetButtonInteractable(false);
            }

            BroSequence animSequence = BroTween.Sequence().SetUpdate(true);
            for (int i = 0; i < rewardItemViewList.Count; ++i)
            {
                PurchaseRewardItemView purchaseRewardItemView = rewardItemViewList[i];
                if (purchaseRewardItemView.gameObject.activeInHierarchy)
                {
                    animSequence.InsertCallback(timeToFloatingAllRewards, () =>
                    {
                        ShowFloatingPurchaseRewardItemView(purchaseRewardItemView);
                        HapticService.I.Haptic(animConfig.rewardFloatHaptic);
                    });
                    timeToFloatingAllRewards += animConfig.rewardNextFloatDelay;
                }
                if(i == rewardItemViewList.Count - 1)
                {
                    timeToFloatingAllRewards -= animConfig.rewardNextFloatDelay;
                }
            }

            animSequence.Insert(animConfig.buttonScaleDelayFromStart, BroTween.ScaleByCurve(claimedSlot.ButtonTransform,  animConfig.buttonScaleDuration, animConfig.buttonScaleCurve));
            animSequence.Insert(animConfig.buttonScaleDelayFromStart, BroTween.ScaleByCurve(claimedSlot.OpenedBkgButtonTransform, animConfig.buttonScaleDuration, animConfig.buttonScaleCurve));
            animSequence.Insert(animConfig.buttonScaleDelayFromStart, BroTween.ScaleByCurve(claimedSlot.ClosedButtonBkgTransform, animConfig.buttonScaleDuration, animConfig.buttonScaleCurve));
            animSequence.InsertCallback(animConfig.tickScaleDelayFromStart, () =>
            {
                claimedSlot.TickGO.SetActive(true);
                PlayParticles(claimedSlot.TickGO.transform.position);
            });
            animSequence.Insert(animConfig.tickScaleDelayFromStart, BroTween.ScaleByCurve(claimedSlot.TickGO.transform as RectTransform, animConfig.tickScaleDuration, animConfig.tickScaleCurve));
            
            if(isNeedMoveSlots)
            {
                animSequence.Insert(timeToFloatingAllRewards + animConfig.currentSlotScaleDelayAfterAllRewards, BroTween.ScaleByCurve(claimedSlot.transform as RectTransform, animConfig.currentSlotScaleDuration, animConfig.currentSlotScaleCurve));
            }

            int lastSlotIdx = claimedOfferIdx + freePaidOfferPopup.SlotsAnchors.Count - 1;
            int nextSlotIdx = claimedOfferIdx + 1;
            for (int i = 0; i < freePaidOfferPopup.Slots.Count; ++i)
            {
                FreePaidOfferSlot slot = freePaidOfferPopup.Slots[i];
                if (i > claimedOfferIdx && i <= lastSlotIdx)
                {
                    int prevAnchorIdx = i - claimedOfferIdx - 1;
                    int currentAnchorIdx = i - claimedOfferIdx;
                    float slotMoveStartTime = timeToFloatingAllRewards + animConfig.otherSlotsMovementDelayAfterAllRewards[i - claimedOfferIdx - 1];
                    float slotMoveFinishTime = 0.0f;

                    if (isNeedMoveSlots)
                    {
                        slotMoveFinishTime = slotMoveStartTime + animConfig.otherSlotsMovementDuration;

                        Vector3 startPos = freePaidOfferPopup.SlotsAnchors[currentAnchorIdx].position;
                        Vector3 finishPos = freePaidOfferPopup.SlotsAnchors[prevAnchorIdx].position;
                        animSequence.Insert(slotMoveStartTime, GetMoveTween(slot.transform, startPos, finishPos, animConfig.otherSlotsMovementEaseCurve, animConfig.otherSlotsMovementDuration));
                    }

                    if(i == nextSlotIdx)
                    {
                        animSequence.InsertCallback(slotMoveFinishTime, () => slot.SetButtonInteractable(true));

                        slot.OpenedBackground.gameObject.SetActive(true);                       
                        float slotFadeDelay = timeToFloatingAllRewards + animConfig.nextSlotFadeBkgDelayAfterAllRewards;
                        animSequence.Insert(slotFadeDelay, BroTween.FadeCanvasGroup(slot.OpenedBackground, 0, 1, animConfig.nextSlotFadeBkgDuration)
                            .SetEase(animConfig.nextSlotFadeBkgEaseCurve));

                        float lockDelay = isNeedMoveSlots ? timeToFloatingAllRewards + animConfig.lockDelayAfterAllRewards : slotFadeDelay;
                        animSequence.InsertCallback(lockDelay, () =>
                        {
                            slot.SetActiveLockImage(false);
                            PlayParticles(slot.LockImage.transform.position);
                            ShowFloatingLockIcon(slot.LockImage.transform, animConfig.lockFinishPosShiftedFromStart);
                        });
                    }

                    if (i == lastSlotIdx)
                    {
                        slot.transform.localScale = Vector3.zero;
                        slot.transform.position = freePaidOfferPopup.SlotsAnchors[currentAnchorIdx].position;
                        animSequence.InsertCallback(slotMoveStartTime, () => { slot.gameObject.SetActive(true); });
                        animSequence.Insert(slotMoveStartTime, BroTween.ScaleByCurve(slot.transform, animConfig.otherSlotsMovementDuration, animConfig.lastSlotScaleScaleCurve));
                    }
                }
            }

            var safeSeq = animSequence.ToSafe();
            
            animSequence.AppendCallback(() =>
            {
                if(animSequences.Count == 1)
                {
                    freePaidOfferPopup.RefreshSlots(freePaidOfferService.CurrentOfferIdx);
                    SendIapOfferAnalytics();
                }

                if (freePaidOfferService.IsOffersCompleted())
                {
                    widgetManager.RemoveWidget(WidgetId.FreePaidOffer);
                }
                else
                { 
                    RefreshCurrentSlotNotifier();
                }

                animSequences.Remove(safeSeq);
            });

            safeSeq.Play();
            animSequences.Add(safeSeq);
        }


        private BroTweenBase GetMoveTween(Transform transform, Vector2 startPos, Vector2 finishPos, AnimationCurve easeCurve, float duration)
        {
            return BroTween.Position(transform, startPos,  finishPos, duration).SetEase(easeCurve);
        }


        private void LevelService_OnLevelIncremented()
        {
            if(!widgetManager.IsWidgetExist(WidgetId.FreePaidOffer))
            {
                lastRestSec = int.MaxValue;
                RefreshWidgetAsync().Forget();
            }
        }


        private void FreePaidOfferService_OnCycleReset()
        {
            if (!freePaidOfferService.IsUnlocked())
            {
                return;
            }

            lastRestSec = int.MaxValue;
            freePaidOfferState.freePaidOfferSkinType = freePaidOfferState.freePaidOfferSkinType.Next();
            ClosePopup();
            RefreshWidgetAsync().Forget();
            RefreshOffers().Forget();
        }


        private void PurchaseOfferContainer_OnPurchaseCompleted(OfferId id, bool arg2)
        {
            var rewardDs = purchaseOfferContainer.GetOfferUiDatasource(id);
            if (!freePaidOfferService.IsUnlocked()
                || rewardDs == null
                || rewardDs.offerGroup != OfferGroup.FreePaid)
            {
                return;
            }

            OnOfferApplied(id);
            freePaidOfferBoughtPopup.Open();
        }


        private void FreePaidOfferPopup_OnChangeState(PopupBase obj, PopupState state)
        {
            if (state is not PopupState.BeginClose)
                return;

            trackedOffers.Clear();

            while (animSequences.Count != 0)
            {
                animSequences[0].Complete(withCallback: true);
            }

            particlesPool.ReleaseAllToPool(PoolableParticleType.FreePaidOfferSparks);
            floatingPurchaseRewardItemViewPool.ReleaseAllToPool(FloatingPurchaseRewardItemViewType.FreePaidOfferSlot);
            floatingIconPool.ReleaseAllToPool(FloatingIconType.FreePaidOfferSlotLock);
            floatingTextPool.ReleaseAllToPool(FloatingTextType.FreePaidOfferSlot);

            RefreshWidgetNotifier();
        }


        private void FreePaidOfferBoughtPopup_OnChangeState(PopupBase obj, PopupState state)
        {
            if (state is not PopupState.BeginClose)
                return;

            ShowAnim();
        }

        private void SendIapOfferAnalytics()
        {
            int presetId = freePaidOfferService.CurrentPresetId;
            List<FreePaidOfferRewardData> rewardDatas = configProvider.FreePaidOfferConfig.GetRewardsData(presetId);
            for (int i = 0; i < freePaidOfferPopup.Slots.Count; ++i)
            {
                if (!freePaidOfferPopup.IsSlotDisplayed(i, freePaidOfferService.CurrentOfferIdx))
                    continue;
                if (rewardDatas[i].InAppOfferId == OfferId.none)
                    continue;

                if (trackedOffers.Add(rewardDatas[i].InAppOfferId))
                    PurchaseAnalytics.TrackInAppOffer(rewardDatas[i].InAppOfferId, lastOfferOpeningMethod, lastOfferSource, string.Empty);
            }
        }
    }
}



