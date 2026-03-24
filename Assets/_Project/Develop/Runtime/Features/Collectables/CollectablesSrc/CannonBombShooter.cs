using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Character;
using Features.LevelConfiguration;
using Infrastructure.AssetManagement;
using Infrastructure.Configs;
using Infrastructure.SystemsLifeCycle;
using UnityEngine;

namespace Features.Collectables
{
    public class CannonBombShooter : ISystemTickable
    {
        private const string TargetStartPointName = "TargetStartPoint";
        private const float MinFirstShotDelay = 5f;
        private const float MaxFirstShotDelay = 10f;
        private const float ShotInterval = 5f;
        private const float ArcHeight = 1.5f;

        private readonly Instantiator instantiator;
        private readonly AssetProvider assetProvider;
        private readonly LevelCreateManager levelCreateManager;
        private readonly CharacterManager characterManager;
        private readonly CollectablesConfig collectablesConfig;
        private readonly List<CannonData> cannons = new();

        private CollectableItem bombPrefab;
        private bool isInitialized;


        public CannonBombShooter(Instantiator instantiator,
                                 AssetProvider assetProvider,
                                 LevelCreateManager levelCreateManager,
                                 CharacterManager characterManager,
                                 ConfigProvider configProvider)
        {
            this.instantiator = instantiator;
            this.assetProvider = assetProvider;
            this.levelCreateManager = levelCreateManager;
            this.characterManager = characterManager;
            this.collectablesConfig = configProvider.CollectablesConfig;
        }


        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (isInitialized)
                return;

            BuildCannonList();

            if (cannons.Count > 0)
            {
                GameObject prefabGo =
                    await assetProvider.AddressableLoadAssetAsync<GameObject>(CollectableType.Bomb.ToString(), cancellationToken);
                bombPrefab = prefabGo.GetComponent<CollectableItem>();
            }

            float now = Time.time;

            for (int i = 0; i < cannons.Count; i++)
            {
                CannonData cannonData = cannons[i];
                cannonData.nextShotTime = now + Random.Range(MinFirstShotDelay, MaxFirstShotDelay);
                cannons[i] = cannonData;
            }

            isInitialized = true;
        }


        public void Deinitialize()
        {
            cannons.Clear();
            bombPrefab = null;
            isInitialized = false;
        }


        public void Tick()
        {
            if (!isInitialized || bombPrefab == null || cannons.Count == 0)
                return;

            Transform target = characterManager.GetMovementRoot();

            if (target == null)
                return;

            float now = Time.time;

            for (int i = 0; i < cannons.Count; i++)
            {
                CannonData cannonData = cannons[i];

                if (cannonData.isDisabled)
                    continue;

                if (cannonData.cannon == null || !cannonData.cannon.IsActive || cannonData.cannon.IsCollected)
                {
                    cannonData.isDisabled = true;
                    cannons[i] = cannonData;
                    continue;
                }

                if (cannonData.startPoint == null || !cannonData.startPoint.gameObject.activeInHierarchy || now < cannonData.nextShotTime)
                    continue;

                FireBomb(cannonData.startPoint.position, target.position);
                cannonData.nextShotTime = now + ShotInterval;
                cannons[i] = cannonData;
            }
        }


        private void BuildCannonList()
        {
            cannons.Clear();
            IReadOnlyList<CollectableItem> items = levelCreateManager.GetItems();

            for (int i = 0; i < items.Count; i++)
            {
                CollectableItem item = items[i];

                if (item.CollectableType != CollectableType.Cannon)
                    continue;

                if (!TryFindChildByName(item.transform, TargetStartPointName, out Transform targetStartPoint))
                    continue;

                cannons.Add(new CannonData
                {
                    cannon = item,
                    startPoint = targetStartPoint,
                    nextShotTime = 0f,
                    isDisabled = false
                });
            }
        }


        private void FireBomb(Vector3 startPos, Vector3 targetPos)
        {
            CollectableItem bombInstance = instantiator.Instantiate(bombPrefab, parent: levelCreateManager.GetLevelRoot());
            float scale = collectablesConfig.Get(CollectableType.Bomb).defaultScaleOverride;
            Vector3 scaleVector = Vector3.one * scale;
            Vector3 velocity = CalculateBallisticVelocity(startPos, targetPos, ArcHeight);

            bombInstance.Construct(CollectableType.Bomb, startPos, Quaternion.identity, scaleVector);
            bombInstance.SetItemGroup(ItemGroup.Damage);
            bombInstance.PhysicsWakeUp();
            bombInstance.SetVelocity(velocity);
        }


        private static Vector3 CalculateBallisticVelocity(Vector3 startPos, Vector3 targetPos, float arcHeight)
        {
            float gravity = Mathf.Abs(Physics.gravity.y);

            if (gravity < 0.01f)
                gravity = 9.81f;

            float peakY = Mathf.Max(startPos.y, targetPos.y) + arcHeight;
            float riseHeight = Mathf.Max(peakY - startPos.y, 0.01f);
            float fallHeight = Mathf.Max(peakY - targetPos.y, 0.01f);
            float timeUp = Mathf.Sqrt(2f * riseHeight / gravity);
            float timeDown = Mathf.Sqrt(2f * fallHeight / gravity);
            float totalTime = Mathf.Max(timeUp + timeDown, 0.1f);

            Vector3 deltaXZ = new Vector3(targetPos.x - startPos.x, 0f, targetPos.z - startPos.z);
            Vector3 velocityXZ = deltaXZ / totalTime;
            float velocityY = gravity * timeUp;
            return new Vector3(velocityXZ.x, velocityY, velocityXZ.z);
        }


        private static bool TryFindChildByName(Transform root, string childName, out Transform result)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < transforms.Length; i++)
            {
                Transform tr = transforms[i];

                if (tr.name == childName)
                {
                    result = tr;
                    return true;
                }
            }

            result = null;
            return false;
        }


        private struct CannonData
        {
            public CollectableItem cannon;
            public Transform startPoint;
            public float nextShotTime;
            public bool isDisabled;
        }
    }
}
