using System.Runtime.CompilerServices;
using Features.Collectables;
using Infrastructure.AudioControl;
using Infrastructure.Collections;
using TriInspector;
using UnityEngine;

namespace Features.Character
{
    public class MagnetField : MonoBehaviour
    {
        [Header("Component")]
        [SerializeField] private Collider coll;
        [SerializeField] private Transform magnetPointUpper;
        [SerializeField] private Transform magnetPointLower;
        [SerializeField] private PhysicMaterial minFriction;

        [Header("Magnet parameters")]
        [SerializeField] private float magnetForceUpper;
        [SerializeField] private float magnetForceLower;

        [Header("Item scale parameters")]
        [SerializeField] private float scaleDuration;
        [SerializeField] private AnimationCurve scaleCurve;

        [Header("Fx")]
        [SerializeField] private GameObject particleRoot;
        [SerializeField] private SfxType magnetSfx;

        [Title("Debug")]
#if UNITY_EDITOR
        [SerializeField]
#endif
        private FastList<int /*persistentId*/> itemIds = new(1000);
#if UNITY_EDITOR
        [SerializeField]
#endif
        private IntHashMap<CollectableItem> itemMap = new(1000);

        private readonly int step = 2;
        private float cleanTimer;
        private int frameOffset;
        private int lastProcessedFrame = -1;

#if UNITY_EDITOR
        [SerializeField] private int debugOrderNumber;
        [SerializeField] private bool isDebug;
        
#endif

        private void FixedUpdate()
        {
            int frame = Time.frameCount;

            if (lastProcessedFrame == frame)
                return;
            
            lastProcessedFrame = frame;
            
            float delta = Time.fixedDeltaTime;
            cleanTimer += delta;
            bool needClean = cleanTimer >= 0.25f;

            if (needClean)
                cleanTimer = 0;

            Vector3 upperPos = magnetPointUpper.position;
            Vector3 lowerPos = magnetPointLower.position;
            frameOffset++;
            int totalCount = itemIds.length;

            for (int i = frameOffset % step; i < totalCount; i += step) //event or odd
            {
                var item = itemMap.GetValueByKey(itemIds[i]);
                Vector3 itemPos = item.Center.position;

                if (item.IsCollected || !item.InMagnetField || !item.IsActive || itemPos.y < lowerPos.y)
                {
                    if (needClean)
                    {
                        itemMap.Remove(itemIds[i], out _);
                        itemIds.RemoveAtSwapBackFast(i);
                        totalCount = itemIds.length;
                    }

                    continue;
                }

                bool toCenter = itemPos.y >= upperPos.y;

                if (toCenter)
                    ApplyForce(item, in upperPos, in itemPos, magnetForceUpper);
                else //force down
                    ApplyForce(item, in lowerPos, in itemPos, magnetForceLower);
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null || !other.attachedRigidbody.TryGetComponent(out CollectableItem item))
                return;

            if (item.Group is ItemGroup.Damage)
                return;

            int itemId = item.OrderNumber;
            item.SetInMagnetField(true);
            PlayScale(item);

            if (itemMap.Has(in itemId))
                return;

            item.SetPhysicMaterial(minFriction);
            item.PhysicsWakeUp();
            itemIds.Add(itemId);
            itemMap.Add(itemId, item, out _);
        }


        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody == null || !other.attachedRigidbody.TryGetComponent(out CollectableItem item))
                return;

            int itemId = item.OrderNumber;
            if (!itemMap.Has(in itemId))
                return;

            item.SetInMagnetField(false);
            PlayScale(item, true);
        }


        [Button]
        public void Activate()
        {
            coll.enabled = true;
            enabled = true;
            particleRoot.SetActive(true);
            AudioService.I.PlaySfx(magnetSfx);
        }


        [Button]
        public void Deactivate()
        {
            for (int i = 0; i < itemIds.length; i++)
            {
                var item = itemMap.GetValueByKey(itemIds[i]);
                item.KillScaleAnimation();
                item.ResetScaleToDefault();
                item.SetVelocity(Vector3.zero);
            }

            itemIds.Clear();
            itemMap.Clear();

            coll.enabled = false;
            enabled = false;
            particleRoot.SetActive(false);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyForce(CollectableItem item, in Vector3 from, in Vector3 itemPos, float forceValue)
        {
            Vector3 dir = from - itemPos;
            float len = dir.magnitude;
            if (len < 0.001f)
                return;

            dir /= len;
            item.SetVelocity(dir * forceValue);
        }


        private void PlayScale(CollectableItem item, in bool isPlayBackwards = false)
        {
            item.PlayScaleAnimation(scaleCurve, scaleDuration, isPlayBackwards);

#if UNITY_EDITOR
            if (isDebug && item.OrderNumber == debugOrderNumber)
            {
                Debug.Log($"--[TRIGGER]. Entered: {!isPlayBackwards}. OrderNumber: {item.OrderNumber}. Frame: {Time.frameCount}. RB_SLEEPING = {item.IsSleeping()}");
            }
#endif
        }
    }
}