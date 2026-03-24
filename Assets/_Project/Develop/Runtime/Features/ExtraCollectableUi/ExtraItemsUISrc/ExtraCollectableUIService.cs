using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Character;
using Features.Collectables;
using Features.Events;
using Infrastructure.Pool.FloatingIcon;
using Infrastructure.AssetManagement;
using Infrastructure.AudioControl;
using Infrastructure.CameraControl;
using Infrastructure.Configs;
using Infrastructure.HapticControl;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Pool;
using Infrastructure.SpriteAtlasControl;
using R3;
using UnityEngine;

namespace Features.ExtraCollectableUi
{
    public class ExtraCollectableUIService
    {
        private readonly ExtraCollectableHud hud;
        private readonly Instantiator instantiator;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly CollectablesConfig collectablesConfig;
        private readonly AssetMappingConfig assetMappingConfig;
        private readonly ExtraItemsUIConfig extraItemsUIConfig;
        private readonly CollectItemEvent collectItemEvent;
        private readonly CharacterManager characterManager;
        private readonly CameraService cameraService;
        private readonly PoolService poolService;
        private readonly CompositeDisposable disposable;
        private readonly CancellationTokenSource cts;
        private Queue<(CollectableType itemType, int currentCount, int maxCount)> refreshQueue = new();

        private FloatingIconPool pool;

        private bool isInit;


        public ExtraCollectableUIService(MainUIProvider uiProvider,
                                         ConfigProvider configProvider,
                                         Instantiator instantiator,
                                         SpriteAtlasService spriteAtlasService,
                                         CollectItemEvent collectItemEvent,
                                         CharacterManager characterManager,
                                         CameraService cameraService,
                                         PoolService poolService)
        {
            this.hud = uiProvider.HudProvider.ExtraCollectableHud;
            this.collectablesConfig = configProvider.CollectablesConfig;
            this.assetMappingConfig = configProvider.AssetMappingConfig;
            this.extraItemsUIConfig = configProvider.ExtraItemsUIConfig;
            this.instantiator = instantiator;
            this.spriteAtlasService = spriteAtlasService;
            this.collectItemEvent = collectItemEvent;
            this.characterManager = characterManager;
            this.cameraService = cameraService;
            this.poolService = poolService;
            this.disposable = new CompositeDisposable();
            this.cts = new CancellationTokenSource();
        }


        public void Initialize()
        {
            if (isInit)
                return;

            pool = poolService.Get<FloatingIconPool>();
            refreshQueue.Clear();
            collectItemEvent.Subscribe(SetSlotActive).AddTo(disposable);
            hud.Initialize();

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            hud.Deinitialize();
            disposable.Dispose();
            refreshQueue.Clear();

            isInit = false;
        }


        public async UniTask AddSlotAsync(CollectableType itemType, int currentCount, int maxCount, CancellationToken token)
        {
            if (!extraItemsUIConfig.TryGetData(itemType, out ExtraItemsUIData itemsUIData) || !itemsUIData.isEnabled)
            {
                return;
            }


            if (!hud.TryGetSlot(itemType, out ExtraCollectableSlotUi slot))
            {
                slot = await instantiator.InstantiateAsync<ExtraCollectableSlotUi>(assetReference: assetMappingConfig.ExtraCollectableSlotUIRef,
                                                                                   parent: hud.ContentRoot,
                                                                                   worldSpace: false,
                                                                                   cancellationToken: token);
                hud.Add(itemType, slot);
            }


            string iconName = collectablesConfig.Get(itemType).iconName;
            Sprite icon = spriteAtlasService.GetFromMain(iconName);
            slot.Construct(icon, itemsUIData.textFormatType);
            slot.Initialize();

            slot.SetSiblingIndex(itemsUIData.slotIdx);
            hud.SetCountText(itemType, currentCount, maxCount);

            if (currentCount == 0)
                slot.SetObjectActive(itemsUIData.isActiveOnZero);
        }


        public void FloatToUi(CollectableType itemType, int currentCount, int maxCount)
        {
            if(!hud.TryGetSlot(itemType, out ExtraCollectableSlotUi slot))
                return;
            
            if (!pool.TryGetItem(FloatingIconType.CollectableCurrency, out PoolableFloatingIcon item))
                return;

            string spriteName = collectablesConfig.Get(itemType).iconName;
            Sprite icon = spriteAtlasService.GetFromMain(spriteName);
            Vector3 fromPos = cameraService.WorldToScreenPoint(characterManager.GetPosition());
            Vector3 targetPos = slot.IconRect.position;

            item.ShowAndFly(icon, fromPos, targetPos);
            item.SetOnCompleteCallback(this, target => target.OnEndFlyIcon().Forget());

            refreshQueue.Enqueue((itemType, currentCount, maxCount));
        }


        private async UniTaskVoid OnEndFlyIcon()
        {
            (CollectableType itemType, int currentCount, int maxCount) item = refreshQueue.Dequeue();
            AudioService.I.PlaySfx(SfxType.GetCurrency);
            HapticService.I.HapticSelection();
            await hud.PlayCountFxAsync(item.itemType, cts.Token);
            hud.SetCountText(item.itemType, item.currentCount, item.maxCount);
        }


        private void SetSlotActive(CollectableItem item)
        {
            hud.SetSlotActive(item.CollectableType);
        }
    }
}