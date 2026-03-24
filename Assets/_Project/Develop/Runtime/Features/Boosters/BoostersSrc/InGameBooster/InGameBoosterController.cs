using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Boosters.Behaviour.InGameBooster;
using Features.Boosters.Behaviour.PreBooster;
using Features.Boosters.Model;
using Features.Events;
using Features.LevelSessionStateControl;
using Infrastructure.Ads;
using Infrastructure.Configs;
using Infrastructure.MainUICanvasControl;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace Features.Boosters
{
    public class InGameBoosterController : ITickable, ILevelSessionSavable
    {
        private readonly BoostersManager boostersManager;
        private readonly BoostersPanelHud panelHud;
        private readonly BoosterConfig boosterConfig;
        private readonly BoosterBehaviourFactory behaviourFactory;
        private readonly GameBaseAnalytics analytics;
        private readonly LevelFinishEvent levelFinishEvent;
        private readonly CompositeDisposable disposable;

        private readonly Dictionary<BoosterType, InGameBoosterBehaviour> inGameRegistry = new();
        private readonly List<InGameBoosterBehaviour> activeBoosters = new();
        private List<BoosterType> selectedPreBoosters = new();
        private List<PreBoosterBehaviour> selectedPreBoosterBhvrs = new();

        private Dictionary<BoosterType, int> prevSessionActiveBoosters;
        private bool isRestoreSession;
        private bool isInit;

        public bool HasActiveBoosters => activeBoosters.Count > 0;


        public InGameBoosterController(BoostersManager boostersManager,
                                       MainUIProvider uiProvider,
                                       ConfigProvider configProvider,
                                       BoosterBehaviourFactory behaviourFactory,
                                       GameBaseAnalytics analytics,
                                       LevelFinishEvent levelFinishEvent)
        {
            this.boostersManager = boostersManager;
            this.behaviourFactory = behaviourFactory;
            this.analytics = analytics;
            this.levelFinishEvent = levelFinishEvent;
            this.panelHud = uiProvider.HudProvider.BoostersPanelHud;
            this.boosterConfig = configProvider.BoosterConfig;
            this.disposable = new CompositeDisposable();
        }


        public void RestoreSessionState(LevelSessionData sessionData)
        {
            prevSessionActiveBoosters = sessionData.activeBoosters;
            isRestoreSession = true;
        }


        public void SaveSessionState(LevelSessionData sessionData)
        {
            if (activeBoosters.Count == 0)
                return;

            sessionData.activeBoosters ??= new Dictionary<BoosterType, int>();
            sessionData.activeBoosters.Clear();

            foreach (var activeBooster in activeBoosters)
            {
                int timeLeft = Mathf.RoundToInt(activeBooster.CurrentTime);

                if (timeLeft <= 0)
                    continue;

                sessionData.activeBoosters.Add(activeBooster.Model.BoosterType, timeLeft);
            }
        }


        public void Initialize()
        {
            if (isInit)
                return;

            levelFinishEvent.Subscribe(_ => StopActiveBoosters()).AddTo(disposable);

            panelHud.Construct(boosterConfig.InGameBoosters);
            panelHud.Initialize();
            panelHud.OnSlotClick += BoostersPanelHud_OnSlotClick;

            InitializePreBoosters();
            InitializeInGameBoosters();
            SubscribeOnBuy();

            if (CanRestorePrevSession())
            {
                ActivateBoostersFromPrevSession();
            }

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            StopActiveBoosters();
            inGameRegistry.Clear();
            panelHud.Deinitialize();
            panelHud.OnSlotClick -= BoostersPanelHud_OnSlotClick;
            disposable.Dispose();
            isInit = false;
        }


        void ITickable.Tick()
        {
            for (int i = activeBoosters.Count - 1; i >= 0; i--)
            {
                InGameBoosterBehaviour entry = activeBoosters[i];

                entry.Tick(Time.deltaTime);

                if (entry.IsOff)
                {
                    entry.Unapply();
                    activeBoosters.RemoveAt(i);
                }
            }
        }


        public async UniTask ExecuteScheduledAsync(CancellationToken token)
        {
            if (selectedPreBoosters.Count > 0)
                await UniTask.WaitForSeconds(boosterConfig.BoosterCommonData.preBoosterAnimDelaySec, ignoreTimeScale: true, cancellationToken: token);

            foreach (var boosterType in selectedPreBoosters)
            {
                if (!boostersManager.TryGetBoosterModel(boosterType, out BoosterModel boosterModel))
                    continue;

                PreBoosterBehaviour behaviour = behaviourFactory.CreatePreBooster(boosterModel);
                selectedPreBoosterBhvrs.Add(behaviour);
                behaviour.ApplyAsync(token);
            }

            if (selectedPreBoosters.Count > 0)
                await UniTask.WaitForSeconds(boosterConfig.BoosterCommonData.preBoosterAnimSec, ignoreTimeScale: true, cancellationToken: token);

            selectedPreBoosters.Clear();
        }


        public void DecreaseSelectedPreBoosters()
        {
            foreach (var preBoosterBehaviour in selectedPreBoosterBhvrs)
            {
                preBoosterBehaviour.DecreaseCount();
            }

            selectedPreBoosterBhvrs.Clear();
        }


        private void InitializePreBoosters()
        {
            selectedPreBoosters = boostersManager.GetSelectedPreBoosters();
            boostersManager.ClearSelectedPreBoosters();
            selectedPreBoosters.AddRange(boostersManager.GetInfinitePreBoosters());
            analytics.AddSelectedPreBoosters(selectedPreBoosters);
        }


        private void InitializeInGameBoosters()
        {
            foreach (BoosterData configData in boosterConfig.InGameBoosters)
            {
                if (!boostersManager.TryGetBoosterModel(configData.type, out BoosterModel booster))
                    continue;

                var inGameBooster = behaviourFactory.CreateInGameBooster(booster, panelHud.GetSlotView(configData.type));

                inGameRegistry.Add(configData.type, inGameBooster);

                RefreshBoosterState(booster);
            }
        }


        private void SubscribeOnBuy()
        {
            boostersManager.OnBoosterUpdated.Subscribe(boosterType =>
            {
                if (!inGameRegistry.TryGetValue(boosterType, out InGameBoosterBehaviour behaviour))
                    return;

                RefreshSlotView(behaviour.Model);

            }).AddTo(disposable);
        }


        private void RefreshBoosterState(BoosterModel boosterModel)
        {
            boostersManager.RefreshBoosterState(boosterModel);
            RefreshSlotView(boosterModel);
        }


        private void RefreshSlotView(BoosterModel boosterModel)
        {
            panelHud.RefreshSlotState(boosterModel.BoosterType, boosterModel.CurrentState, boosterModel.CurrentCount.ToString(), boosterModel.LockLevelText, boosterModel.FreeText);
        }


        private void StopActiveBoosters()
        {
            foreach (InGameBoosterBehaviour inGameBooster in activeBoosters)
            {
                inGameBooster.Deinitialize();
            }

            activeBoosters.Clear();
        }


        private void ApplyBoosterOnHudClick(BoosterType boosterType)
        {

            if (!inGameRegistry.TryGetValue(boosterType, out InGameBoosterBehaviour behaviour))
                return;

            if (behaviour.Model.CurrentState == BoosterStateType.NeedBuy)
            {
                boostersManager.OpenBoosterBuyPopup(behaviour.Model);
                return;
            }

            behaviour.Apply();
            RefreshBoosterState(behaviour.Model);
            activeBoosters.Add(behaviour);
            analytics.AddUsedInGameBooster(boosterType);
        }


        private void ApplyBoosterOnRestoreSession(BoosterType boosterType, int timeLeft)
        {
            if (!inGameRegistry.TryGetValue(boosterType, out InGameBoosterBehaviour behaviour))
                return;

            if (timeLeft <= 0)
                return;

            behaviour.ApplyRestore(timeLeft);
            RefreshBoosterState(behaviour.Model);
            activeBoosters.Add(behaviour);
        }


        private bool CanRestorePrevSession()
        {
            return isRestoreSession && prevSessionActiveBoosters != null;
        }


        private void ActivateBoostersFromPrevSession()
        {
            foreach (var entryPair in prevSessionActiveBoosters)
            {
                ApplyBoosterOnRestoreSession(entryPair.Key, entryPair.Value);
            }

            prevSessionActiveBoosters.Clear();
        }


        private void BoostersPanelHud_OnSlotClick(BoosterType boosterType)
        {
            ApplyBoosterOnHudClick(boosterType);
        }

    }
}