using Features.Collectables;
using Features.Events;
using Features.LevelConfiguration;
using Features.LevelSessionStateControl;
using Features.LevelUp;
using Infrastructure.Configs;
using Infrastructure.QualityGraphicsControl;
using Infrastructure.Utilities;
using R3;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VContainer.Unity;


namespace Features.Shadows
{
    public class ShadowsService : ILevelSessionSavable
#if UNITY_EDITOR        
        , ITickable
#endif
    {
        private readonly ShadowsController shadowsController;
        private readonly ShadowsConfig shadowsConfig;
        private readonly LevelProgressChangeEvent levelProgressChangeEvent;
        private readonly CompositeDisposable disposable;
        private readonly LevelCreateManager levelCreateManager;
        private readonly CollectablesConfig collectablesConfig;

        private bool isInit = false;
        private int prevSessionLevel;
        private bool isRestoreSession;
        int currentLevel;


        public ShadowsService(ShadowsController shadowsController,
            ConfigProvider configProvider,
            LevelProgressChangeEvent levelProgressChangeEvent,
            LevelCreateManager levelCreateManager)
        {
            this.shadowsController = shadowsController;
            this.shadowsConfig = configProvider.ShadowsConfig;
            this.collectablesConfig = configProvider.CollectablesConfig;
            this.levelProgressChangeEvent = levelProgressChangeEvent;
            this.levelCreateManager = levelCreateManager;
            this.disposable = new CompositeDisposable();
        }


        public void Initialize()
        {
            if (isInit)
                return;
            currentLevel = isRestoreSession ? prevSessionLevel : 1;
            SubscribeOnLevelProgressChange();
            SetShadows();
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            disposable.Dispose();
            isInit = false;
        }


        private void SubscribeOnLevelProgressChange()
        {
            levelProgressChangeEvent.Subscribe(LevelProgressChangeEvent).AddTo(disposable);
        }


        private void LevelProgressChangeEvent(LevelProgressArgs args)
        {
            switch (args.type)
            {
                case ActionType.Add:
                    currentLevel += args.diff;
                break;
                case ActionType.Remove:
                    currentLevel -= args.diff;
                break;
                default:
                    currentLevel = args.diff;
                break;
            }

            SetShadows();
        }


        private void SetShadows()
        {
            SetShadowMaxDistance();
            SetShadowBySize();
        }


        private void SetShadowMaxDistance()
        {
            float shadowsMaxDistance = shadowsConfig.GetShadowsMaxDistanceByLevel(currentLevel);
            shadowsController.SetShadowsMaxDistance(shadowsMaxDistance);
        }


        private void SetShadowBySize()
        {
            float shadowOffMaxY = shadowsConfig.ShadowOffMaxY;
            int shadowOffSize = shadowsConfig.GetShadowsOffByLevel(currentLevel);

            List<CollectableItem> collectableItems = levelCreateManager.GetItems();
            for(int i = 0; i < collectableItems.Count; i++)
            {
                CollectableItem collectableItem = collectableItems[i];
                if(!collectableItem.gameObject.activeSelf
                    || !collectablesConfig.TryGet(collectableItem.CollectableType, out CollectablesData collectablesData))
                {
                    continue;
                }

                UnityEngine.Rendering.ShadowCastingMode shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                if(collectableItem.Center.position.y > shadowOffMaxY
                    || collectablesData.size > shadowOffSize)
                {
                    shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }

                if(collectableItem.MeshRenderer.shadowCastingMode != shadowCastingMode)
                {
                    collectableItem.MeshRenderer.shadowCastingMode = shadowCastingMode;
                }
            }
        }


        void ILevelSessionSavable.RestoreSessionState(LevelSessionData sessionData)
        {
            prevSessionLevel = sessionData.lastReportedLevel;
            isRestoreSession = true;
        }


        void ILevelSessionSavable.SaveSessionState(LevelSessionData sessionData)
        {

        }


#if UNITY_EDITOR
        public void Tick()
        {
            if(isInit
                && shadowsConfig.isDebug)
            {
                SetShadows();
            }
        }
#endif

    }
}

