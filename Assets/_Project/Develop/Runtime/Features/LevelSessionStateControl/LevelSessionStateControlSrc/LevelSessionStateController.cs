using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Events;
using Features.Level;
using Features.LevelComplete;
using Features.LevelLoose;
using Infrastructure.Configs;
using Infrastructure.PersistentProgress;
using Infrastructure.Settings;
using Infrastructure.Utilities;
using Newtonsoft.Json;
using R3;
using Progress = Infrastructure.PersistentProgress.Progress;

namespace Features.LevelSessionStateControl
{
    //Game scope
    public class LevelSessionStateController : ISavable
    {
        private readonly List<ILevelSessionSavable> sessionSavables;
        private readonly LevelSessionStateService stateService;
        private readonly LevelService levelService;
        private readonly AppSettingsController appSettingsController;
        private readonly LevelLooseManager levelLooseManager;
        private readonly ConfigProvider configProvider;
        private readonly LevelSessionStateControlConfiguration levelSessionStateControlConfiguration;
        private readonly LevelStartedEvent levelStartedEvent;
        private readonly LevelFinishEvent levelFinishEvent;
        private readonly CompositeDisposable disposable;

        private LevelSessionState saveState;
        private LevelSessionData sessionData;

        private bool isInit;


        public LevelSessionStateController(IEnumerable<ILevelSessionSavable> injectectedSavables,
                                           LevelSessionStateService stateService,
                                           LevelService levelService,
                                           AppSettingsController appSettingsController,
                                           LevelLooseManager levelLooseManager,
                                           ConfigProvider configProvider,
                                           LevelStartedEvent levelStartedEvent,
                                           LevelFinishEvent levelFinishEvent)
        {
            this.sessionSavables = new List<ILevelSessionSavable>(injectectedSavables);
            this.stateService = stateService;
            this.levelService = levelService;
            this.appSettingsController = appSettingsController;
            this.levelLooseManager = levelLooseManager;
            this.levelSessionStateControlConfiguration = configProvider.LevelSessionStateControlConfiguration;
            this.levelStartedEvent = levelStartedEvent;
            this.levelFinishEvent = levelFinishEvent;

            disposable = new CompositeDisposable();
        }


        void ISavable.Load(Progress progress)
        {
            saveState = progress.levelSessionState;
        }


        void ISavable.Save(Progress progress)
        {
            PrepareSave();
            progress.levelSessionState = saveState;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            ResetIfNextLevel();
            SetLevelSessionData();

            levelStartedEvent.Subscribe(_ => LevelStartedEventHandle()).AddTo(disposable);
            levelFinishEvent.Subscribe(_ => LevelFinishEventHandle(levelFinishEvent.isWin)).AddTo(disposable);
            levelLooseManager.OnResurrectPending += LevelLooseManager_OnResurrectPending;
            levelLooseManager.OnResurrectComplete += LevelLooseManager_OnResurrectComplete;

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            disposable.Dispose();

            levelLooseManager.OnResurrectPending -= LevelLooseManager_OnResurrectPending;
            levelLooseManager.OnResurrectComplete -= LevelLooseManager_OnResurrectComplete;

            sessionSavables.Clear();
            isInit = false;
        }


        public UniTask ExecuteScheduledAsync(CancellationToken cancellationToken)
        {
            if (stateService.CurrentState is SessionStateType.PendingResurrect)
            {
                levelLooseManager.HandleDeath();
            }
            else if (stateService.IsGameLevelInProcess())
            {
                return appSettingsController.OpenPopupAsync(cancellationToken);
            }

            return UniTask.CompletedTask;
        }


        private void SetLevelSessionData()
        {
            if (stateService.IsGameLevelInProcess() && saveState.sessionDataCompressedJson != null)
            {
                string rawJson = StringUtils.DecompressString(saveState.sessionDataCompressedJson);
                sessionData = JsonConvert.DeserializeObject<LevelSessionData>(rawJson, JsonUtils.SerializerSettings);

                foreach (var sessionSavable in sessionSavables)
                {
                    sessionSavable.RestoreSessionState(sessionData);
                }
            }
            else
            {
                sessionData = new LevelSessionData();
            }
        }


        private void ResetIfNextLevel()
        {
            if (saveState.lastLevelNumber != levelService.CurrentLevelNumber || !stateService.LevelSessionStateEnabled)
            {
                SetState(SessionStateType.None);
                saveState.ClearSessionData();
            }
        }


        private void SetState(SessionStateType state)
        {
            if (saveState.state == state)
                return;

            saveState.state = state;

            //UnityEngine.Debug.Log("[LevelSessionStateController]. Set state: " + state);
        }


        private void LevelStartedEventHandle()
        {
            if (!stateService.IsGameLevelInProcess())
            {
                saveState.lastLevelNumber = levelService.CurrentLevelNumber;
                SetState(SessionStateType.InProcess);
            }
        }


        private void LevelFinishEventHandle(bool isWin)
        {
            if (!isWin)
                levelService.RegisterLose();

            var newState = isWin ? SessionStateType.Win : SessionStateType.Loose;
            SetState(newState);
            saveState.ClearSessionData();
        }


        private void PrepareSave()
        {
            if (!stateService.IsGameLevelInProcess())
                return;

            foreach (var sessionSavable in sessionSavables)
            {
                sessionSavable.SaveSessionState(sessionData);
            }

            string rawJson = JsonConvert.SerializeObject(sessionData, JsonUtils.SerializerSettings);
            saveState.sessionDataCompressedJson = StringUtils.CompressString(rawJson);
        }


        private void LevelLooseManager_OnResurrectPending()
        {
            SetState(SessionStateType.PendingResurrect);
        }


        private void LevelLooseManager_OnResurrectComplete()
        {
            SetState(SessionStateType.InProcess);
        }
    }
}