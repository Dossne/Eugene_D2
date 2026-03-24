using Features.LevelConfiguration;
using Features.PhysicsLogic;
using Infrastructure.BroTweens;
using Infrastructure.GameplayContainers;
using Infrastructure.Utilities;
using UnityEngine;

namespace Features.Collectables
{
    public class CollectableItem : MonoBehaviour, IContainerItem
    {
        [SerializeField] private CollectableType collectableType;
        [SerializeField] private ItemGroup group = ItemGroup.Collectable;
        [SerializeField] private Transform thisTransform;
        [SerializeField] private Transform center;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Collider[] colliders;

        private Vector3 defaultScale = Vector3.one;
        private BroTweenSafe scaleTween;
        private GameObject[] colliderObjects;

        public string ObjectId { get; }
        public string ObjectNameDetails { get; }

        public CollectableType CollectableType => collectableType;
        public ItemGroup Group => group;
        public Transform Center => center;
        public MeshRenderer MeshRenderer { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsCollected { get; private set; }
        public bool InMagnetField { get; private set; }

        [TriInspector.ShowInInspector] public int OrderNumber { get; private set; }


        private void OnEnable()
        {
            IsActive = true;
        }


        private void OnDisable()
        {
            IsActive = false;
            KillScaleAnimation();
        }


        public void Construct(CollectableType collectableType, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            OrderNumber = CollectableId.GetNext();
            colliderObjects = new GameObject[colliders.Length];
            MeshRenderer = GetComponentInChildren<MeshRenderer>();

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == null)
                {
                    Debug.LogError($"Colliders not assigned {collectableType}");
                    continue;
                }                    
                colliderObjects[i] = colliders[i].gameObject;
            }

            this.collectableType = collectableType;
            thisTransform.SetPositionAndRotation(position, rotation);
            defaultScale = scale;
            ResetScaleToDefault();
        }


        public void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }


        public void SetCollidersActive(bool value)
        {
            foreach (Collider col in colliders)
            {
                col.enabled = value;
            }
        }


        public void SetPhysicMaterial(PhysicMaterial mat)
        {
            foreach (Collider col in colliders)
            {
                col.material = mat;
            }
        }


        public void SetCollidersLayerToDefault()
        {
            SetCollidersLayer(LayerConstants.DefaultLayer);
        }


        public void SetCollidersLayerToNonCollab()
        {
            SetCollidersLayer(LayerConstants.NoCollabPhysLayer);
        }


        private void SetCollidersLayer(int layer)
        {
            foreach (GameObject go in colliderObjects)
            {
                go.layer = layer;
            }
        }


        public void PhysicsWakeUp()
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.WakeUp();
        }


        public void RigidbodyWakeUpIfSleeping()
        {
            if (rb.IsSleeping())
                rb.WakeUp();
        }


        public void SetVelocity(Vector3 value)
        {
            rb.velocity = value;
        }


        public void PhysicsSetSleep()
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.Sleep();
        }


        public void SetPositionAnRotation(Vector3 position, Quaternion rotation)
        {
            rb.position = position;
            thisTransform.SetPositionAndRotation(position, rotation);
        }


        public void SetInMagnetField(bool value)
        {
            InMagnetField = value;
        }


        public void ResetScaleToDefault()
        {
            thisTransform.localScale = defaultScale;
        }


        public void PlayScaleAnimation(AnimationCurve curve, in float duration, in bool isPlayBackwards)
        {
            if (!scaleTween.TryRewind())
            {
                KillScaleAnimation();
                scaleTween = BroTween.ScaleByCurve(thisTransform, in defaultScale, in duration, curve).ToSafe();

            }

            scaleTween.SetPlayBackwards(isPlayBackwards);
            scaleTween.Play();
        }


        public void KillScaleAnimation()
        {
            scaleTween.Kill();
        }


        public CollectableData GetData()
        {
            return new CollectableData
            {
                collectableType = collectableType,
                position = new SerializableVector3(transform.position),
                rotation = new SerializableVector3(transform.rotation),
            };
        }


        public CollectableData GetDataExtended()
        {
            return new CollectableData
            {
                collectableType = collectableType,
                position = new SerializableVector3(transform.position),
                rotation = new SerializableVector3(transform.rotation),
                scale = new SerializableVector3(transform.localScale),
            };
        }


        public void SetItemGroup(ItemGroup itemGroup)
        {
            group = itemGroup;
        }


        public void MarkAsCollected()
        {
            IsCollected = true;
            KillScaleAnimation();
        }


#if UNITY_EDITOR

        [TriInspector.Button]
        private void CollectColliders_Editor()
        {
            colliders = GetComponentsInChildren<Collider>(true);
        }
#endif
        public bool IsSleeping()
        {
            return rb.IsSleeping();
        }
    }
}