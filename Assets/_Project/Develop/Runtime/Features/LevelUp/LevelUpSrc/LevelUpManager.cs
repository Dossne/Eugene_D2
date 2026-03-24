using Features.Character;
using Features.Collectables;
using Features.Events;
using Features.LevelSessionStateControl;
using Infrastructure.Configs;
using Infrastructure.Events;
using Infrastructure.HapticControl;
using Infrastructure.Utilities;
using R3;

namespace Features.LevelUp
{
    public class LevelUpManager : ILevelSessionSavable
    {
        private readonly ExperienceBase state;
        private readonly CharacterManager characterManager;
        private readonly CollectablesConfig collectablesConfig;
        private readonly ItemCollectManager itemCollectManager;
        private readonly CollectItemEvent collectItemEvent;
        private readonly LevelProgressChangeEvent levelProgressChangeEvent;
        private readonly LevelProgressChangeRequestEvent levelProgressRequestEvent;
        private readonly ExperienceAddRequest experienceAddRequest;
        private readonly CompositeDisposable disposables;

        private int lastReportedLevel;

        private int prevSessionExperience;
        private int prevSessionLastReportedLevel;
        private bool isRestoreSession;
        private bool isInit;


        public LevelUpManager(
            CharacterManager characterManager,
            ConfigProvider configProvider,
            CollectItemEvent collectItemEvent,
            LevelProgressChangeEvent levelProgressChangeEvent,
            LevelProgressChangeRequestEvent levelProgressRequestEvent,
            ExperienceAddRequest experienceAddRequest)
        {
            this.characterManager = characterManager;
            this.collectablesConfig = configProvider.CollectablesConfig;
            this.collectItemEvent = collectItemEvent;
            this.levelProgressChangeEvent = levelProgressChangeEvent;
            this.levelProgressRequestEvent = levelProgressRequestEvent;
            this.experienceAddRequest = experienceAddRequest;
            state = new ExperienceBase(configProvider.LevelUpConfig.GetPoints());
            disposables = new CompositeDisposable();
        }


        void ILevelSessionSavable.RestoreSessionState(LevelSessionData sessionData)
        {
            prevSessionExperience = sessionData.experience;
            prevSessionLastReportedLevel = sessionData.lastReportedLevel;
            isRestoreSession = true;
        }


        void ILevelSessionSavable.SaveSessionState(LevelSessionData sessionData)
        {
            sessionData.experience = state.CurrentExp;
            sessionData.lastReportedLevel = lastReportedLevel;
        }


        public void Initialize()
        {
            if (isInit)
                return;
            
            state.Initialize();
            
            if (isRestoreSession) 
                state.Add(prevSessionExperience);

            lastReportedLevel = isRestoreSession ? prevSessionLastReportedLevel : 1;

            state.OnLevelUp += ExperienceBase_OnLevelUp;
            levelProgressRequestEvent.Subscribe(LevelProgressChangeRequest).AddTo(disposables);
            collectItemEvent.Subscribe(CollectItemEvent).AddTo(disposables);
            experienceAddRequest.Subscribe(ExperienceAddRequestHandle).AddTo(disposables);

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            state.OnLevelUp -= ExperienceBase_OnLevelUp;

            isInit = false;
            disposables.Dispose();
        }


        private void AddPoints(int value)
        {
            state.Add(value);
            characterManager.SetProgressSlider(state.CurrenProgress);
            HapticService.I.HapticSelection();
            //UnityEngine.Debug.Log($"[POINTS] Added {value} points.To next level left {state.GetNextLevelExperience()} points");
        }


        private void ExperienceBase_OnLevelUp()
        {
            levelProgressChangeEvent.Execute(new LevelProgressArgs(Source.ExpLevelUp, ActionType.Add, state.Level, state.Level - lastReportedLevel));
            lastReportedLevel = state.Level;
        }


        private void CollectItemEvent(CollectableItem item)
        {
            if (item.Group != ItemGroup.Collectable || !collectablesConfig.TryGet(item.CollectableType, out CollectablesData data) || data.cost <= 0)
                return;

            AddPoints(data.cost);
        }


        private void ExperienceAddRequestHandle(ExperienceArgs args)
        {
            AddPoints(args.addValue);
        }


#region Cheats

        private void LevelProgressChangeRequest(LevelProgressArgs args)
        {
            switch (args.type)
            {
                case ActionType.Add:
                    state.Add(state.GetNextLevelExperience());
                    break;
                case ActionType.Set:
                    state.Reset();
                    lastReportedLevel = state.Level;
                    characterManager.SetProgressSlider(state.CurrenProgress);
                    levelProgressChangeEvent.Execute(new LevelProgressArgs(Source.ExpLevelUp, ActionType.Set, state.Level, lastReportedLevel));
                    break;
            }
        }

#endregion


    }
}