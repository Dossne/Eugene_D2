using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Character;
using Features.LevelConfiguration;
using Infrastructure.AudioControl;
using Infrastructure.AssetManagement;
using Infrastructure.Configs;
using Infrastructure.SystemsLifeCycle;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Features.Collectables
{
    public class CannonBombShooter : ISystemTickable
    {
        private const string TargetStartPointName = "TargetStartPoint";
        private const float MinFirstShotDelay = 5f;
        private const float MaxFirstShotDelay = 10f;
        private const float MinShotInterval = 4f;
        private const float MaxShotInterval = 8f;
        private const float ArcHeight = 1.5f;
        private const float MinBombScale = 0.5f;
        private const float MaxBombScale = 1.9f;
        private const float ShotFxScaleMultiplier = 0.33333334f;
        private const float BombTargetMarkerOffsetY = 0.02f;
        private const float BombTargetMarkerScaleMultiplier = 1.6f;
        private const float BombTargetMarkerMinScale = 0.6f;
        private const float BombTargetMarkerMaxScale = 2.5f;
        private const float BombTargetMarkerMaxLifetime = 15f;
        private const string SmokePoofResourcePath = "CannonFx/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR Smoke Poof";
        private const string SmokePoofDenseResourcePath = "CannonFx/Cartoon FX Remaster/CFXR Prefabs/Misc/Variants/CFXR Smoke Poof (Dense)";
        private const string TargetMarkerTextureResourcePath = "CannonFx/Targeting/vfx_target";
        private const string TargetMarkerShaderName = "Project/CannonTargetOverlay";
        private const string TargetMarkerShaderResourcePath = "CannonFx/Shaders/CannonTargetOverlay";

        private readonly Instantiator instantiator;
        private readonly AssetProvider assetProvider;
        private readonly LevelCreateManager levelCreateManager;
        private readonly CharacterManager characterManager;
        private readonly CollectablesConfig collectablesConfig;
        private readonly List<CannonData> cannons = new();
        private readonly List<GameObject> shotFxPrefabs = new();

        private CollectableItem bombPrefab;
        private Texture2D targetMarkerTexture;
        private Material targetMarkerMaterial;
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
            CacheShotFxPrefabs();
            CacheTargetMarkerResources();

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
            shotFxPrefabs.Clear();
            bombPrefab = null;
            targetMarkerTexture = null;

            if (targetMarkerMaterial != null)
                Object.Destroy(targetMarkerMaterial);

            targetMarkerMaterial = null;
            isInitialized = false;
        }


        public void Tick()
        {
            if (!isInitialized || bombPrefab == null || cannons.Count == 0)
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

                if (now < cannonData.nextShotTime)
                    continue;

                if (!TryGetShootStartPosition(cannonData, out Vector3 startPos))
                {
                    cannonData.isDisabled = true;
                    cannons[i] = cannonData;
                    continue;
                }

                if (!TryGetTargetPosition(out Vector3 targetPos))
                    continue;

                FireBomb(startPos, targetPos);
                cannonData.nextShotTime = now + Random.Range(MinShotInterval, MaxShotInterval);
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
                    localStartPoint = item.transform.InverseTransformPoint(targetStartPoint.position),
                    nextShotTime = 0f,
                    isDisabled = false
                });
            }
        }


        private void FireBomb(Vector3 startPos, Vector3 targetPos)
        {
            SpawnShotFx(startPos);

            if (AudioService.I != null)
            {
                AudioService.I.PlaySfx(SfxType.CannonShoot, isRandom: true);
            }

            float baseScale = collectablesConfig.Get(CollectableType.Bomb).defaultScaleOverride;
            float randomScale = Random.Range(MinBombScale, MaxBombScale);
            float bombScale = baseScale * randomScale;
            Vector3 scaleVector = Vector3.one * bombScale;
            Vector3 velocity = CalculateBallisticVelocity(startPos, targetPos, ArcHeight);
            GameObject targetMarker = SpawnTargetMarker(targetPos, bombScale);

            CollectableItem bombInstance = instantiator.Instantiate(bombPrefab, startPos, parent: levelCreateManager.GetLevelRoot());

            bombInstance.Construct(CollectableType.Bomb, startPos, Quaternion.identity, scaleVector);
            bombInstance.SetItemGroup(ItemGroup.Damage);
            bombInstance.PhysicsWakeUp();
            bombInstance.SetVelocity(velocity);
            AttachTargetMarkerTracker(bombInstance, targetMarker, targetPos.y);
        }


        private void CacheShotFxPrefabs()
        {
            shotFxPrefabs.Clear();
            TryAddShotFxPrefab(SmokePoofResourcePath);
            TryAddShotFxPrefab(SmokePoofDenseResourcePath);
        }


        private void CacheTargetMarkerResources()
        {
            targetMarkerTexture = Resources.Load<Texture2D>(TargetMarkerTextureResourcePath);

            if (targetMarkerTexture == null)
            {
                Debug.LogWarning($"[CannonBombShooter] Target marker texture not found in Resources at '{TargetMarkerTextureResourcePath}'");
                return;
            }

            Shader markerShader = Resources.Load<Shader>(TargetMarkerShaderResourcePath);

            if (markerShader == null)
                markerShader = Shader.Find(TargetMarkerShaderName);

            if (markerShader == null)
            {
                Debug.LogWarning($"[CannonBombShooter] Shader '{TargetMarkerShaderName}' not found. Fallback to Unlit/Transparent.");
                markerShader = Shader.Find("Unlit/Transparent");
            }

            if (markerShader == null)
            {
                Debug.LogWarning("[CannonBombShooter] Target marker shader unavailable.");
                return;
            }

            targetMarkerMaterial = new Material(markerShader);
            targetMarkerMaterial.color = Color.red;
            targetMarkerMaterial.mainTexture = targetMarkerTexture;
            targetMarkerMaterial.renderQueue = 5000;
        }


        private void TryAddShotFxPrefab(string resourcePath)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);

            if (prefab != null)
            {
                shotFxPrefabs.Add(prefab);
            }
            else
            {
                Debug.LogWarning($"[CannonBombShooter] Shot FX not found in Resources at '{resourcePath}'");
            }
        }


        private void SpawnShotFx(Vector3 position)
        {
            if (shotFxPrefabs.Count == 0)
                return;

            GameObject fxPrefab = shotFxPrefabs[Random.Range(0, shotFxPrefabs.Count)];
            Transform parent = levelCreateManager.GetLevelRoot();
            GameObject fxInstance = Object.Instantiate(fxPrefab, position, Quaternion.identity, parent);
            fxInstance.transform.localScale *= ShotFxScaleMultiplier;
            float lifetime = GetFxLifetime(fxInstance);
            Object.Destroy(fxInstance, lifetime + 0.25f);
        }


        private GameObject SpawnTargetMarker(Vector3 targetPos, float bombScale)
        {
            if (targetMarkerMaterial == null || targetMarkerTexture == null)
                return null;

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.name = "BombTargetMarker";
            marker.transform.SetParent(levelCreateManager.GetLevelRoot(), false);
            marker.transform.position = targetPos + Vector3.up * BombTargetMarkerOffsetY;
            marker.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            float markerScale =
                Mathf.Clamp(bombScale * BombTargetMarkerScaleMultiplier, BombTargetMarkerMinScale, BombTargetMarkerMaxScale);
            marker.transform.localScale = new Vector3(markerScale, markerScale, 1f);

            Collider markerCollider = marker.GetComponent<Collider>();

            if (markerCollider != null)
                Object.Destroy(markerCollider);

            MeshRenderer renderer = marker.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = targetMarkerMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            Object.Destroy(marker, BombTargetMarkerMaxLifetime);
            return marker;
        }


        private static void AttachTargetMarkerTracker(CollectableItem bombInstance, GameObject targetMarker, float targetY)
        {
            if (bombInstance == null || targetMarker == null)
                return;

            BombTargetMarkerTracker tracker = bombInstance.gameObject.GetComponent<BombTargetMarkerTracker>();

            if (tracker == null)
                tracker = bombInstance.gameObject.AddComponent<BombTargetMarkerTracker>();

            tracker.Construct(targetMarker, targetY);
        }


        private static float GetFxLifetime(GameObject fxInstance)
        {
            ParticleSystem[] systems = fxInstance.GetComponentsInChildren<ParticleSystem>(true);
            float maxLifetime = 0.5f;

            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem ps = systems[i];
                var main = ps.main;
                float duration = main.duration;
                float startLifetime = main.startLifetime.constantMax;
                float totalLifetime = duration + startLifetime;

                if (totalLifetime > maxLifetime)
                    maxLifetime = totalLifetime;
            }

            return maxLifetime;
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


        private bool TryGetShootStartPosition(CannonData cannonData, out Vector3 result)
        {
            result = default;

            if (cannonData.cannon == null)
                return false;

            if (cannonData.startPoint != null && cannonData.startPoint.gameObject.activeInHierarchy)
            {
                result = cannonData.startPoint.position;
                return true;
            }

            // Fallback to local offset in case Unity temporarily loses child linkage during runtime changes.
            result = cannonData.cannon.transform.TransformPoint(cannonData.localStartPoint);
            return true;
        }


        private bool TryGetTargetPosition(out Vector3 result)
        {
            result = characterManager.GetPosition();
            Transform movementRoot = characterManager.GetMovementRoot();

            if (movementRoot != null && movementRoot.gameObject.activeInHierarchy)
            {
                result = movementRoot.position;
                return true;
            }

            return true;
        }


        private struct CannonData
        {
            public CollectableItem cannon;
            public Transform startPoint;
            public Vector3 localStartPoint;
            public float nextShotTime;
            public bool isDisabled;
        }


        private sealed class BombTargetMarkerTracker : MonoBehaviour
        {
            private const float ArmDelay = 0.12f;
            private const float ContactTargetYThreshold = 0.08f;

            private GameObject marker;
            private float targetY;
            private float armedAt;
            private bool markerDestroyed;


            public void Construct(GameObject marker, float targetY)
            {
                this.marker = marker;
                this.targetY = targetY;
                armedAt = Time.time + ArmDelay;
                markerDestroyed = marker == null;
            }


            private void OnCollisionEnter(UnityEngine.Collision collision)
            {
                float hitY = collision.contactCount > 0 ? collision.GetContact(0).point.y : transform.position.y;
                TryDestroyMarker(collision.collider, hitY);
            }


            private void OnTriggerEnter(Collider other)
            {
                TryDestroyMarker(other, transform.position.y);
            }


            private void OnDisable()
            {
                TryDestroyMarker(null, transform.position.y, force: true);
            }


            private void OnDestroy()
            {
                TryDestroyMarker(null, transform.position.y, force: true);
            }


            private void TryDestroyMarker(Collider other, float contactY, bool force = false)
            {
                if (markerDestroyed)
                    return;

                if (force)
                {
                    DestroyMarker();
                    return;
                }

                bool tableHit = IsTableHit(other);

                if (!tableHit)
                {
                    if (Time.time < armedAt)
                        return;

                    if (contactY > targetY + ContactTargetYThreshold)
                        return;
                }

                DestroyMarker();
            }


            private static bool IsTableHit(Collider other)
            {
                if (other == null)
                    return false;

                LevelTableElement tableElement = other.GetComponentInParent<LevelTableElement>(true);

                if (tableElement != null)
                {
                    return tableElement.ElementType == LevelTableElementType.TopPlane
                        || tableElement.ElementType == LevelTableElementType.TableBase
                        || tableElement.ElementType == LevelTableElementType.SidePlane;
                }

                return other.name.IndexOf("table", global::System.StringComparison.OrdinalIgnoreCase) >= 0;
            }


            private void DestroyMarker()
            {
                markerDestroyed = true;

                if (marker != null)
                    Object.Destroy(marker);
            }
        }
    }
}
