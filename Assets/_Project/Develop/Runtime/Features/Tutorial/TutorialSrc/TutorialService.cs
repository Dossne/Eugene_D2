using Features.Boosters;
using Features.Collectables;
using Infrastructure.Configs;
using Infrastructure.InputControl;
using Infrastructure.Localization;
using Infrastructure.MainUICanvasControl;
using Infrastructure.PersistentProgress;
using Infrastructure.SceneManagement;
using Infrastructure.TooltipControl;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using VContainer;

namespace Features.Tutorial
{
    public class TutorialService : ISavable
    {
        private readonly Dictionary<string, TutorialBase> tutorials = new();
        private readonly Dictionary<TutorialTrigger, List<string>> tutorialTriggers = new();
        private readonly List<string> tutorialsQueue = new();
        private readonly List<string> tutorialsInProgress = new();
        private List<string> completedTutorials = new();
        private readonly SceneLoadController sceneLoadController;
        private readonly TutorialFactory tutorialFactory;
        private readonly TutorialConfiguration tutorialConfiguration;

        private readonly TutorialTriggerEvent tutorialTriggerEvent;
        private CompositeDisposable disposables;

#if UNITY_EDITOR 
        private bool isDebugLog = true;
#endif
        
        [Inject]
        public TutorialService(SceneLoadController sceneLoadController,
                               TutorialFactory tutorialFactory,
                               ConfigProvider configProvider,
                               MainUIProvider mainUIProvider,
                               TooltipService tooltipService,
                               InputService inputService,
                               TutorialTriggerEvent tutorialTriggerEvent)
        {
            mainUIProvider.MaskTutorialPanel.Construct(tooltipService, inputService);
            this.tutorialConfiguration = configProvider.TutorialConfiguration;  
            this.sceneLoadController = sceneLoadController;  
            this.tutorialFactory = tutorialFactory;
            this.tutorialTriggerEvent = tutorialTriggerEvent;
            this.disposables = new CompositeDisposable();
        }

        public bool IsInitialized { get; private set; } = false;

        public void Initialize()
        {
            if (IsInitialized)
                return;
            disposables = new CompositeDisposable();
            tutorialTriggerEvent.Subscribe(HandleTutorialTrigger).AddTo(disposables);

            tutorials.Clear();

            if (tutorialConfiguration.IsGroupEnabled(TutorialGroup.Move))
            {
                //Move
                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CreateMoveTooltipTutorial(TutorialId.MoveTooltipTutorial));
            }

            if (tutorialConfiguration.IsGroupEnabled(TutorialGroup.Eat))
            {
                //Eat
                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CreateEatTooltipTutorial(TutorialId.EatTooltipTutorial));
            }

            if (tutorialConfiguration.IsGroupEnabled(TutorialGroup.FinishLevel))
            {
                //Finish level
                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CreateFinishLevelTooltipTutorial(TutorialId.FinishLevelTooltipTutorial));
            }

            if (tutorialConfiguration.IsGroupEnabled(TutorialGroup.InGameBoosterUnlock))
            {
                //Ingame Booster unlock
                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CreateBoosterPopupTutorial(TutorialId.SizeBoosterPopupTutorial, BoosterType.SizeBooster));

                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CreateBoosterPopupTutorial(TutorialId.ObjectFinderPopupTutorial, BoosterType.ObjectFinder));

                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CreateBoosterPopupTutorial(TutorialId.TimeFreezePopupTutorial, BoosterType.TimeFreeze));

                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CreateBoosterPopupTutorial(TutorialId.SuperMagnetPopupTutorial, BoosterType.SuperMagnet));
            }

            if (tutorialConfiguration.IsGroupEnabled(TutorialGroup.InGameBoosterUse))
            {
                //Ingame Booster use
                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CreateBoosterInGameTutorial(TutorialId.SizeBoosterInGameTutorial, BoosterType.SizeBooster));

                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CreateBoosterInGameTutorial(TutorialId.ObjectFinderInGameTutorial, BoosterType.ObjectFinder));

                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CreateBoosterInGameTutorial(TutorialId.TimeFreezeInGameTutorial, BoosterType.TimeFreeze));

                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CreateBoosterInGameTutorial(TutorialId.SuperMagnetInGameTutorial, BoosterType.SuperMagnet));
            }

            if (tutorialConfiguration.IsGroupEnabled(TutorialGroup.PreBooster))
            {
                //Pre Booster use
                RegisterTutorial(TutorialTrigger.PreBoosterPopupBeginOpen, tutorialFactory.CreatePreBoosterTutorial(TutorialId.BoostBottleTutorial, BoosterType.BoostBottle));

                RegisterTutorial(TutorialTrigger.PreBoosterPopupBeginOpen, tutorialFactory.CreatePreBoosterTutorial(TutorialId.BonusClockTutorial, BoosterType.BonusClock));
            }

            if (tutorialConfiguration.IsGroupEnabled(TutorialGroup.Collectable))
            {
                //Bomb collect
                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CollectablePopupTutorial(TutorialId.CollectableBombTutorial, 
                                                                                                     CollectableType.Bomb, 
                                                                                                     LocKeys.CollectableTutorialDescription.Bomb));
            }

            if (tutorialConfiguration.IsGroupEnabled(TutorialGroup.WinStreakUnlock))
            {
                //WinStreak unlock
                RegisterTutorial(TutorialTrigger.PreBoosterPopupBeginOpen, tutorialFactory.CreateWinStreakUnlockTutorial(TutorialId.WinStreakUnlockTutorial));
            }

            if (tutorialConfiguration.IsGroupEnabled(TutorialGroup.RewardTrackUnlock))
            {
                //RewardTrack unlock
                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CreateRewardTrackUnlockTutorial(TutorialId.RewardTrackUnlockTutorial));
            }

            
            if (tutorialConfiguration.IsGroupEnabled(TutorialGroup.LavaQuest))
            {
                //Lava Quest
                RegisterTutorial(TutorialTrigger.LavaQuestEventPopupBeginOpen, tutorialFactory.CreateLavaQuestStartTutorial(TutorialId.LavaQuestStartTutorial));
            }

            if (tutorialConfiguration.IsGroupEnabled(TutorialGroup.SuperSpeed))
            {
                //Super Speed
                RegisterTutorial(TutorialTrigger.PreBoosterPopupBeginOpen, tutorialFactory.CreateSuperSpeedWidgetTutorial(TutorialId.SuperSpeedWidgetTutorial));

                //Super Speed Tooltip
                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CreateSuperSpeedTooltipTutorial(TutorialId.SuperSpeedTooltipTutorial));
            }

            if (tutorialConfiguration.IsGroupEnabled(TutorialGroup.PlayerProfile))
            {
                //Player Profile
                RegisterTutorial(TutorialTrigger.SceneLoad, tutorialFactory.CreatePlayerProfileTutorial(TutorialId.PlayerProfileTutorial));
            }

            sceneLoadController.OnProcessBegin += StopTutorials;

            IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!IsInitialized)
                return;           

            sceneLoadController.OnProcessBegin -= StopTutorials;
            disposables.Dispose();
            IsInitialized = false;
        }

        public bool NeedLockLevelStart(int levelNumber) 
        {
            return tutorialConfiguration.NeedLockLevelStart(levelNumber);
        }

        public void EnqueueTutorial(string tutoruialId, bool startNextImmediate) 
        {
            if (!tutorialsQueue.Contains(tutoruialId)
             && !completedTutorials.Contains(tutoruialId)
             && tutorials.ContainsKey(tutoruialId)
             && tutorials[tutoruialId].CanBeEqueued())
            {
                tutorialsQueue.Add(tutoruialId);
#if UNITY_EDITOR
                if(isDebugLog)
                    UnityEngine.Debug.Log($"EnqueueTutorial {tutoruialId}");
#endif
            }

            if (startNextImmediate)
                TryStartNextTutorial();
        }

        public void Load(Progress progress)
        {
            completedTutorials = progress.tutorialState.completedTutorials;
        }

        public void Save(Progress progress)
        {
            progress.tutorialState.completedTutorials = completedTutorials;
        }

        public void ResetTutorials() 
        {
            completedTutorials.Clear();
        }

        public void EnqueueTutorialsByTrigger(TutorialTrigger trigger)
        {
            if (!tutorialTriggers.TryGetValue(trigger, out var tutorialsByTrigger))
                return;

            if (tutorialsQueue.Count > 0 || tutorialsInProgress.Count > 0)
            {
#if UNITY_EDITOR
                if (isDebugLog)
                    UnityEngine.Debug.Log($"There are tutorials in progress. Trigger {trigger} ignored this time.");
#endif
                return;
            }

            foreach (var item in tutorialsByTrigger)
                EnqueueTutorial(item, false);

            TryStartNextTutorial();
        }

        private void HandleTutorialTrigger(TutorialTrigger trigger)
        {
            EnqueueTutorialsByTrigger(trigger);
        }

        private void RegisterTutorial(TutorialTrigger tutorialTrigger, TutorialBase tutorial)
        {
            tutorials.Add(tutorial.TutorialId, tutorial);
            if (!tutorialTriggers.ContainsKey(tutorialTrigger))
                tutorialTriggers.TryAdd(tutorialTrigger, new());
            if (!tutorialTriggers[tutorialTrigger].Contains(tutorial.TutorialId))
                tutorialTriggers[tutorialTrigger].Add(tutorial.TutorialId);
        }

        private void TryStartNextTutorial()
        {
            if (tutorialsQueue.Count <= 0)
                return;

            string nextTutorialId = string.Empty;
            for (int i = 0; i < tutorialsQueue.Count; i++)
            {
                string tutorialId = tutorialsQueue[i];

                if (!tutorials.TryGetValue(tutorialId, out var tutorial))
                    continue;

                if (!tutorial.CanBeStarted()) 
                    continue;

                if (tutorialsInProgress.Count == 0)
                    nextTutorialId = tutorialId;

                break;
            }

            if (nextTutorialId != string.Empty)
            {
#if UNITY_EDITOR
                if (isDebugLog)
                    UnityEngine.Debug.Log($"Start tutor {nextTutorialId}");
#endif
                tutorialsInProgress.Add(nextTutorialId);
                tutorialsQueue.Remove(nextTutorialId);
                tutorials[nextTutorialId].OnComplete += TutorialBase_OnComplete;
                tutorials[nextTutorialId].Start();
            }
            else
            {
                tutorialsQueue.Clear();
#if UNITY_EDITOR
                if (isDebugLog)
                    UnityEngine.Debug.Log($"There is no tutorials ready to start. Tutorial queue cleared!");
#endif
            }
        }

        private void TutorialBase_OnComplete(string tutorialId)
        {
            tutorials[tutorialId].OnComplete -= TutorialBase_OnComplete;
            tutorialsInProgress.Remove(tutorialId);
            tutorialsQueue.Remove(tutorialId);
            completedTutorials.Add(tutorialId);
#if UNITY_EDITOR
            if(isDebugLog)
                UnityEngine.Debug.Log($"Complete tutor {tutorialId}");
#endif

            TryStartNextTutorial();
        }

        private void StopTutorials(string arg1, LoadSceneMode mode)
        {
            for (int i = 0; i < tutorialsInProgress.Count; i++)
            {
                tutorials[tutorialsInProgress[i]].Stop();
#if UNITY_EDITOR
                if(isDebugLog)
                    UnityEngine.Debug.Log($"Stop tutor {tutorialsInProgress[i]}");
#endif
            }
                
            tutorialsInProgress.Clear();
            tutorialsQueue.Clear();
        }
    }
}