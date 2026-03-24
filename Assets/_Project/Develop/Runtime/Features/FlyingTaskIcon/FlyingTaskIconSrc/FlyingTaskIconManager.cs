using System.Collections.Generic;
using System.Threading;
using Features.Character;
using Features.Collectables;
using Features.Events;
using Infrastructure.Pool.FloatingIcon;
using Features.LevelTasks;
using Infrastructure.AudioControl;
using Infrastructure.CameraControl;
using Infrastructure.Configs;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Pool;
using Infrastructure.SpriteAtlasControl;
using R3;
using UnityEngine;
using Features.CollectableCurrency;
using Infrastructure.SystemsLifeCycle;

namespace Features.FlyingTaskIcon
{
    public class FlyingTaskIconManager : ISystemTickable
    {
        private const float Cooldown = 0.05f;

        private readonly LevelTasksHud hud;
        private readonly PoolService poolService;
        private readonly CollectItemEvent collectItemEvent;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly CharacterManager characterManager;
        private readonly CameraService cameraService;
        private readonly CollectablesConfig collectablesConfig;
        private readonly LevelTaskManager levelTaskManager;
        private readonly CollectableCurrencyService collectableCurrencyService;
        private readonly CompositeDisposable disposables = new();
        private readonly CancellationTokenSource cts;
        private Queue<CollectableType> refreshQueue = new();
        private Dictionary<CollectableType, Sprite> icons = new();

        private Dictionary<CollectableType, Queue<int>> pendingQueue = new();

        private float poolCd;
        private float soundCd;
        private FloatingIconPool pool;

        private bool isInit;


        public FlyingTaskIconManager(
            MainUIProvider uiProvider,
            PoolService poolService,
            CollectItemEvent collectItemEvent,
            SpriteAtlasService spriteAtlasService,
            ConfigProvider configProvider,
            CharacterManager characterManager,
            CameraService cameraService,
            LevelTaskManager levelTaskManager,
            CollectableCurrencyService collectableCurrencyService)
        {
            this.hud = uiProvider.HudProvider.LevelTasksHud;
            this.poolService = poolService;
            this.collectItemEvent = collectItemEvent;
            this.spriteAtlasService = spriteAtlasService;
            this.characterManager = characterManager;
            this.cameraService = cameraService;
            this.levelTaskManager = levelTaskManager;
            this.collectablesConfig = configProvider.CollectablesConfig;
            this.collectableCurrencyService = collectableCurrencyService;
            cts = new CancellationTokenSource();
        }


        public void Initialize()
        {
            if (isInit)
                return;

            pendingQueue.Clear();
            pool = poolService.Get<FloatingIconPool>();
            collectItemEvent.Subscribe(EnqueueItem).AddTo(disposables);
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            cts.Cancel();
            cts.Dispose();
            disposables.Dispose();
            refreshQueue.Clear();
            pendingQueue.Clear();
            icons.Clear();
            
            isInit = false;
        }


        void ISystemTickable.Tick()
        {
            poolCd -= Time.unscaledDeltaTime;
            soundCd -= Time.unscaledDeltaTime;
        }


        private void PlaySfxForUsualItem(CollectableType itemType)
        {
            if (soundCd > 0 || collectableCurrencyService.IsCollectableCurrency(itemType))
                return;

            AudioService.I.PlaySfx(SfxType.PopBase);
            soundCd = Cooldown;
        }

        private void EnqueueItem(CollectableItem cItem) 
        {
            CollectableType itemType = cItem.CollectableType;
            if (!levelTaskManager.IsTaskItem(itemType))
            {
                PlaySfxForUsualItem(itemType);
                return;
            }
            cItem.MarkAsCollected();

            int currentState = levelTaskManager.GetCurrentState(itemType);
            levelTaskManager.CollectTaskItem(itemType);
            currentState--;

            if (!pendingQueue.TryGetValue(itemType, out var queue)) 
            {
                pendingQueue.Add(itemType, new());
                queue = pendingQueue[itemType];
            }
            
            queue.Enqueue(currentState);
            levelTaskManager.PrepareLastItemCollection(itemType);

            if (poolCd > 0 && queue.Count > 1)
            {
                queue.Dequeue();
                return;
            }


            if (!pool.TryGetItem(FloatingIconType.Task, out PoolableFloatingIcon itemIcon))
            {
                queue.Dequeue();
                return;
            }
            poolCd = Cooldown;

            FlyIcon(itemType, itemIcon);
        }

        private void FlyIcon(CollectableType itemType, PoolableFloatingIcon itemIcon)
        {
            if (!icons.TryGetValue(itemType, out Sprite icon))
            {
                string spriteName = collectablesConfig.Get(itemType).iconName;
                icon = spriteAtlasService.GetFromMain(spriteName);
                icons.Add(itemType, icon);
            }
            
            Vector3 fromPos = cameraService.WorldToScreenPoint(characterManager.GetPosition());
            Vector3 targetPos = hud.GetIconRect(itemType).position;            
            refreshQueue.Enqueue(itemType);
            itemIcon.ShowAndFly(icon, fromPos, targetPos);
            itemIcon.SetOnCompleteCallback(this, target => target.Deque());
            AudioService.I.PlaySfx(SfxType.PopTask);
        }


        private void Deque()
        {
            CollectableType itemType = refreshQueue.Dequeue();

            var currentCount = -1;
            if (pendingQueue.TryGetValue(itemType, out var queue) && queue.Count > 0)
                currentCount = queue.Dequeue();
            else
                currentCount = 0;

            RefreshSlotState(itemType, currentCount, true);
        }
        
        
        private void RefreshSlotState(CollectableType itemType, int currentCount, bool withFx)
        {
             hud.RefreshState(itemType, currentCount, withFx);
             levelTaskManager.TryCompleteTask(itemType, currentCount);
        }
    }
}