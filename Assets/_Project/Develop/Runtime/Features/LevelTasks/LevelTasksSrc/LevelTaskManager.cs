using System.Collections.Generic;
using System.Linq;
using Features.Collectables;
using Features.Events;
using Features.Level;
using Features.LevelConfiguration;
using Features.LevelSessionStateControl;
using Features.LevelTime;
using Features.Tutorial;
using Infrastructure.AssetManagement;
#if PR_CHEAT
using Infrastructure.Cheat;
#endif
using Infrastructure.Configs;
using Infrastructure.MainUICanvasControl;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using Newtonsoft.Json;
using R3;

namespace Features.LevelTasks
{
    public class LevelTaskManager : ILevelSessionSavable
    {
        private readonly AllTaskCompleteEvent allCompleteEvent;
        private readonly Instantiator instantiator;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly ItemCollectManager itemCollectManager;
        private readonly LevelService levelService;
        private readonly LevelTimeManager levelTimeManager;
        private readonly TaskCompleteTutorialEvent taskCompleteTutorialEvent;
        private readonly CollectablesConfig collectablesConfig;
        private readonly LevelTasksHud taskHud;
#if PR_CHEAT
        private readonly CheatService cheatService;
#endif

        private Dictionary<CollectableType, int> currentTasks;

        private Dictionary<CollectableType, int> prevSessionTasks;
        private bool isRestoreSession;

        private bool isInit;

        private bool allTaskCompletedSent = false;

        public LevelTaskManager(Instantiator instantiator,
                                MainUIProvider uiProvider,
                                ConfigProvider configProvider,
                                SpriteAtlasService spriteAtlasService,
                                AllTaskCompleteEvent allCompleteEvent,
                                ItemCollectManager itemCollectManager,
                                LevelService levelService,
                                LevelTimeManager levelTimeManager,
                                TaskCompleteTutorialEvent taskCompleteTutorialEvent
#if PR_CHEAT
                              , CheatService cheatService
#endif
        )
        {
            this.instantiator = instantiator;
            this.spriteAtlasService = spriteAtlasService;
            this.allCompleteEvent = allCompleteEvent;
            this.itemCollectManager = itemCollectManager;
            this.levelService = levelService;
            this.levelTimeManager = levelTimeManager;
            this.taskCompleteTutorialEvent = taskCompleteTutorialEvent;
#if PR_CHEAT
            this.cheatService = cheatService;
#endif
            taskHud = uiProvider.HudProvider.LevelTasksHud;
            collectablesConfig = configProvider.CollectablesConfig;
        }


        void ILevelSessionSavable.RestoreSessionState(LevelSessionData data)
        {
            prevSessionTasks = data.tasks;
            isRestoreSession = true;
        }


        void ILevelSessionSavable.SaveSessionState(LevelSessionData data)
        {
            data.tasks = new Dictionary<CollectableType, int>(currentTasks);
        }


        public void Initialize()
        {
            if (isInit)
                return;

            if (CanRestorePrevSession())
                RestoreTasksFromPrevSession();
            else
                InitializeTasks();

            taskHud.Construct(instantiator, spriteAtlasService, collectablesConfig, currentTasks);
            taskHud.Initialize();
#if PR_CHEAT
            cheatService.OnWinLevelRequest += CheatService_OnWinLevelRequest;
#endif
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

#if PR_CHEAT
            cheatService.OnWinLevelRequest -= CheatService_OnWinLevelRequest;
#endif

            taskHud.Deinitialize();

            currentTasks.Clear();
            isInit = false;
        }


        public void ExecuteScheduled()
        {
            if (currentTasks.Count == 0) 
            {
                allTaskCompletedSent = true;
                allCompleteEvent.Execute(Unit.Default);
            }                
        }


        public bool IsTaskItem(CollectableType itemType)
        {
            return currentTasks.ContainsKey(itemType);
        }


        public void CollectTaskItem(CollectableType itemType, int collectAmount = 1)
        {
            if (!currentTasks.TryGetValue(itemType, out int currentCnt))
                return;

            currentCnt -= collectAmount;

            if (currentCnt < 0)
                currentCnt = 0;

            currentTasks[itemType] = currentCnt;

            if (currentCnt == 0)
                currentTasks.Remove(itemType);
        }


        public int GetCurrentState(CollectableType itemType)
        {
            return currentTasks[itemType];
        }


        public List<CollectableType> GetCurrentTasks()
        {
            return currentTasks.Select(x => x.Key).ToList();
        }


        public void TryCompleteTask(CollectableType itemType, int currentCnt)
        {
            if (currentCnt > 0)
                return;

            taskCompleteTutorialEvent.Execute(itemType);

            if (!allTaskCompletedSent && currentTasks.Count == 0 && !levelTimeManager.TimeIsOff)
            {
                allTaskCompletedSent = true;
                allCompleteEvent.Execute(Unit.Default);
            }                
        }


        private void RestoreTasksFromPrevSession()
        {
            currentTasks = new Dictionary<CollectableType, int>(prevSessionTasks);
            allTaskCompletedSent = false;
        }


        private void InitializeTasks()
        {
            LevelData levelData = levelService.GetCurrentLevelData();

            if (!LevelUtil.TryGetTasks(levelData.id, levelData.taskJson, out List<CollectableType> taskConfig))
            {
                taskConfig = new List<CollectableType>();
            }

            currentTasks = new Dictionary<CollectableType, int>();

            Dictionary<CollectableType, int> lvlGrouped = itemCollectManager.GetGroupedCount();

            for (var i = 0; i < taskConfig.Count; i++)
            {
                var taskType = taskConfig[i];
                currentTasks.TryAdd(taskType, lvlGrouped[taskType]);
            }
            allTaskCompletedSent = false;
        }


        private bool CanRestorePrevSession()
        {
            return isRestoreSession && prevSessionTasks != null;
        }


        private void CheatService_OnWinLevelRequest()
        {
            LevelData levelData = levelService.GetCurrentLevelData();
            List<CollectableType> taskConfig =
                JsonConvert.DeserializeObject<List<CollectableType>>(levelData.taskJson, JsonUtils.SerializerSettings);

            for (var i = 0; i < taskConfig.Count; i++)
            {
                var itemType = taskConfig[i];

                if (currentTasks.TryGetValue(itemType, out int currentCnt))
                    CollectTaskItem(itemType, currentCnt);
                TryCompleteTask(itemType, 0);
            }
        }


        public void PrepareLastItemCollection(CollectableType itemType)
        {
            if (currentTasks.Count > 1)
                return;

            if (!currentTasks.TryGetValue(itemType, out int currentCnt))
                return;

            if (currentCnt >= 1)
                return;

            levelTimeManager.StopTimer();
        }
    }
}