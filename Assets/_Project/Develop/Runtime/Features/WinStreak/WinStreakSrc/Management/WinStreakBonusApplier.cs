using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Boosters;
using Features.Collectables;
using Features.Events;
using Features.LevelConfiguration;
using Features.LevelSessionStateControl;
using Infrastructure.Ads;
using Infrastructure.AudioControl;
using Infrastructure.CameraControl;
using Infrastructure.Configs;
using Infrastructure.Pool;
using UnityEngine;
using R3;

namespace Features.WinStreak
{
    public class WinStreakBonusApplier : ILevelSessionSavable
    {
        private readonly WinStreakStateController winStreakStateController;
        private readonly WinStreakConfig winStreakConfig;
        private readonly BoosterConfig boosterConfig;
        private readonly PoolService poolService;
        private readonly CollectItemEvent collectItemEvent;
        private readonly BoosterBehaviourFactory boosterBehaviourFactory;
        private readonly LevelCreateManager levelCreateManager;
        private readonly CameraService cameraService;
        private readonly CompositeDisposable disposable;
        private readonly CancellationTokenSource cts;
        private readonly GameBaseAnalytics analytics;

        private Dictionary<int, List<BoosterByLevelData>> cacheBoostersByLevelData;
        private Dictionary<BoosterType, WinStreakBoosterData> cachedWinStreakData;
        private readonly List<PoolableWinStreakBooster> sceneBoosters = new();

        private List<CollectableData> prevSessionWinStreakObjects;
        private bool isRestoreSession;
        private bool isInit;


        public WinStreakBonusApplier(WinStreakStateController winStreakStateController,
                                     ConfigProvider configProvider,
                                     CollectItemEvent collectItemEvent,
                                     PoolService poolService,
                                     BoosterBehaviourFactory boosterBehaviourFactory,
                                     LevelCreateManager levelCreateManager,
                                     CameraService cameraService,
                                     GameBaseAnalytics analytics)
        {
            winStreakConfig = configProvider.WinStreakConfig;
            boosterConfig = configProvider.BoosterConfig;
            this.poolService = poolService;
            this.collectItemEvent = collectItemEvent;
            this.boosterBehaviourFactory = boosterBehaviourFactory;
            this.levelCreateManager = levelCreateManager;
            this.cameraService = cameraService;
            this.analytics = analytics;
            this.winStreakStateController = winStreakStateController;

            disposable = new CompositeDisposable();
            cts = new CancellationTokenSource();
        }


        public void RestoreSessionState(LevelSessionData sessionData)
        {
            prevSessionWinStreakObjects = sessionData.winStreakObjects;
            isRestoreSession = true;
        }


        public void SaveSessionState(LevelSessionData sessionData)
        {
            sessionData.winStreakObjects ??= new List<CollectableData>();
            sessionData.winStreakObjects.Clear();

            for (int i = 0; i < sceneBoosters.Count; i++)
            {
                sessionData.winStreakObjects.Add(sceneBoosters[i].GetDataExtended());
            }
        }


        public void Initialize()
        {
            if (isInit || !winStreakStateController.IsFeatureEnabled)
                return;

            collectItemEvent.Subscribe(CollectItemEventHandle).AddTo(disposable);

            if (CanRestorePrevSession())
            {
                RestorePrevSession();
            }

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            cacheBoostersByLevelData?.Clear();
            cachedWinStreakData?.Clear();
            sceneBoosters.Clear();

            disposable.Dispose();
            cts.Cancel();
            cts.Dispose();

            isInit = false;
        }


        public UniTask ExecuteScheduledAsync(CancellationToken cancellationToken)
        {
            return DropCollectableBoostersAsync(winStreakStateController.CurrentLevel, cancellationToken);
        }


        public async UniTask DropCollectableBoostersAsync(int level, CancellationToken cancellationToken)
        {
            if (CanRestorePrevSession() || !winStreakStateController.IsFeatureEnabled)
                return;

            if (!TryGetBoostersByLevel(level, out List<BoosterByLevelData> boostersByLevel))
            {
                return;
            }

            await UniTask.NextFrame(); //Wait for camera setup on character

            WinStreakObjectPool winStreakObjPool = poolService.Get<WinStreakObjectPool>();
            if (!winStreakObjPool.TryGetItem(level, out PoolableWinStreakObject wsObject))
                return;

            (Vector3 left, Vector3 center, Vector3 right) wsPos = GetWinStreakObjectPositions();
            wsObject.MoveAsync(wsPos.left, wsPos.center, wsPos.right, cancellationToken).Forget();
            AudioService.I.PlaySfx(SfxType.WinStreakObjectShow);

            float totalTime = Mathf.Max(0, wsObject.GetTotalTime() - wsObject.GetBoostersBeginSpawnTime());
            await UniTask.WaitForSeconds(wsObject.GetBoostersBeginSpawnTime(), cancellationToken: cancellationToken);

            WinStreakBoosterPool pool = poolService.Get<WinStreakBoosterPool>();
            WinStreakSpawnParams spawnParams = winStreakConfig.SpawnParams;
            LevelTableView tableView = levelCreateManager.GetTable();
            Vector3 flyStartPos = wsObject.BoostersFlyPoint;
            Vector3 charPos = levelCreateManager.GetCharacterSpawnPoint().pos;
            AudioService.I.PlaySfx(SfxType.WinStreakThrow);

            foreach (BoosterByLevelData booster in boostersByLevel)
            {
                WinStreakBoosterData winStreakBoosterData = GetWinStreakBoosterData(booster.boosterType);

                for (int i = 0; i < booster.itemCount; i++)
                {
                    if (!pool.TryGetItem(winStreakBoosterData.collectableType, out PoolableWinStreakBooster view))
                        continue;

                    Vector3 targetRandomPos = GetRandomPosition(charPos,
                                                                spawnParams.minX, spawnParams.maxX,
                                                                spawnParams.minY, spawnParams.maxY,
                                                                spawnParams.minZ, spawnParams.maxZ);

                    Vector3 flyTargetPos = tableView.GetClosestPointInPlanes(targetRandomPos);
                    flyTargetPos.y = targetRandomPos.y;
                    view.Construct(winStreakBoosterData.collectableType, flyStartPos, Quaternion.identity, Vector3.one);
                    view.MoveAsync(flyStartPos, flyTargetPos, cancellationToken).Forget();
                    sceneBoosters.Add(view);
                }
            }
            

            await UniTask.WaitForSeconds(totalTime, cancellationToken: cancellationToken);
        }


        private bool TryGetBoostersByLevel(int level, out List<BoosterByLevelData> result)
        {
            result = null;

            if (cacheBoostersByLevelData == null)
            {
                InitializeCachedBoostersByLevel();
            }

            if (cacheBoostersByLevelData!.TryGetValue(level, out result))
            {
                return result.Count > 0;
            }

            return false;
        }


        private WinStreakBoosterData GetWinStreakBoosterData(BoosterType boosterType)
        {
            if (cachedWinStreakData == null)
            {
                InitializeCachedWinStreakData();
            }

            return cachedWinStreakData![boosterType];
        }


        private void InitializeCachedBoostersByLevel()
        {
            cacheBoostersByLevelData = new Dictionary<int, List<BoosterByLevelData>>();

            foreach (var boosterByLevelData in winStreakConfig.BoosterByLevel)
            {
                if (!cacheBoostersByLevelData.ContainsKey(boosterByLevelData.lvl))
                {
                    cacheBoostersByLevelData.Add(boosterByLevelData.lvl, new List<BoosterByLevelData>());
                }

                cacheBoostersByLevelData[boosterByLevelData.lvl].Add(boosterByLevelData);
            }
        }


        private void InitializeCachedWinStreakData()
        {
            cachedWinStreakData = new Dictionary<BoosterType, WinStreakBoosterData>();

            foreach (var winStreakBoosterData in boosterConfig.WinStreakBoosters)
            {
                cachedWinStreakData.TryAdd(winStreakBoosterData.type, winStreakBoosterData);
            }
        }


        private Vector3 GetRandomPosition(Vector3 center, float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            float posX = Random.Range(minX, maxX);
            float posY = Random.Range(minY, maxY);
            float posZ = Random.Range(minZ, maxZ);
            return new Vector3(center.x + posX, center.y + posY, center.z + posZ);
        }


        private (Vector3 left, Vector3 center, Vector3 right) GetWinStreakObjectPositions()
        {
            var spawn = winStreakConfig.SpawnParams;
            Vector3 left = ViewportToWorldPoint(spawn.startXPosNorm, spawn.yPosNorm, spawn.depthFromCamera);
            Vector3 center = ViewportToWorldPoint(0.5f, spawn.yPosNorm, spawn.depthFromCamera);
            Vector3 right = ViewportToWorldPoint(spawn.endXPosNorm, spawn.yPosNorm, spawn.depthFromCamera);
            return (left, center, right);
        }


        private Vector3 ViewportToWorldPoint(float xNormalized, float yNormalized, float depthFromCamera)
        {
            Vector3 viewportPoint = new Vector3(xNormalized, yNormalized, depthFromCamera);
            return cameraService.ViewportToWorldPoint(viewportPoint);
        }


        private bool CanRestorePrevSession()
        {
            return isRestoreSession && prevSessionWinStreakObjects != null;
        }


        private void RestorePrevSession()
        {
            sceneBoosters.Clear();
            WinStreakBoosterPool pool = poolService.Get<WinStreakBoosterPool>();

            for (int i = 0; i < prevSessionWinStreakObjects.Count; i++)
            {
                var data = prevSessionWinStreakObjects[i];

                if (!pool.TryGetItem(data.collectableType, out PoolableWinStreakBooster view))
                    continue;

                view.Construct(data.collectableType, data.position.ToVector3(), data.rotation.ToQuaternion(), data.scale.ToVector3());
                view.PhysicsWakeUp();
                sceneBoosters.Add(view);
            }
        }


        private void CollectItemEventHandle(CollectableItem item)
        {
            if (item.Group is not ItemGroup.Booster)
            {
                return;
            }

            foreach (var winStreakBoosterData in boosterConfig.WinStreakBoosters)
            {
                if (winStreakBoosterData.collectableType == item.CollectableType)
                {
                    var behaviour = boosterBehaviourFactory.CreateWinStreakBehaviour(winStreakBoosterData);
                    behaviour.ApplyAsync(cts.Token);
                    analytics.AddUsedWinStreakBooster(winStreakBoosterData.type);
                    break;
                }
            }

            for (var i = sceneBoosters.Count - 1; i >= 0; i--)
            {
                if (sceneBoosters[i].IsTargetItem(item))
                {
                    sceneBoosters.RemoveAt(i);
                    break;
                }
            }
        }

    }
}