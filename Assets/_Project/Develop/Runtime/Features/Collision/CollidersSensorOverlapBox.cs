using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Features.Collision
{
    public interface ICollidersSensorListener
    {
        void OnCollidersUpdated(Collider[] buffer, int size);
    }

    public class CollidersSensorOverlapBox : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private Transform centerPoint;
        [SerializeField] private int bufferCapacity = 64;
        [SerializeField] private bool scanEveryFrame;
        [SerializeField] private float scanPeriod = 0.35f;
        [SerializeField] private Vector3 size;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal;

        [Header("Debug")]
        [SerializeField] private bool isDebugDraw;
        [SerializeField] private Color debugColor = Color.blue;
        [SerializeField] private Collider[] buffer;

        private readonly List<ICollidersSensorListener> listeners = new();
        private float currentScanPeriod;
        private bool isScanning;


        public void Tick(float dt)
        {
            if (!isScanning)
                return;

            if (scanEveryFrame)
            {
                UpdateColliders();
                return;
            }

            currentScanPeriod += dt;

            if (currentScanPeriod >= scanPeriod)
            {
                currentScanPeriod = 0;
                UpdateColliders();
            }
        }


        public void StartDetect()
        {
            buffer = new Collider[bufferCapacity];
            currentScanPeriod = 0;
            isScanning = true;
        }


        public void StopDetect()
        {
            buffer = null;
            isScanning = false;
        }


        public void AddListener(ICollidersSensorListener listener)
        {
            if (listeners.Contains(listener))
                return;

            listeners.Add(listener);
        }


        public void RemoveListener(ICollidersSensorListener listener)
        {
            listeners.Remove(listener);
        }


        public int DetectInRadius(Collider[] buffer, Vector3 size)
        {
            return Physics.OverlapBoxNonAlloc(
                center: this.centerPoint.position,
                halfExtents: size * 0.5f,
                results: buffer,
                Quaternion.identity,
                mask: this.layerMask,
                queryTriggerInteraction: this.triggerInteraction
            );
        }


        public void SetScanPeriod(float value)
        {
            scanPeriod = value;
            currentScanPeriod = 0;
        }


        public void SetSize(Vector3 size)
        {
            this.size = size;
        }


        private void UpdateColliders()
        {
            Array.Clear(this.buffer, 0, this.buffer.Length);
            int bufferSize = DetectInRadius(buffer, this.size);
            ObserveOnColliderUpdate(this.buffer, bufferSize);
        }


        private void ObserveOnColliderUpdate(Collider[] buffer, int size)
        {
            for (int i = 0; i < this.listeners.Count; i++)
            {
                listeners[i].OnCollidersUpdated(buffer, size);
            }
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!isDebugDraw || centerPoint == null)
                return;

            var prevColor = Handles.color;
            Handles.color = debugColor;
            Handles.DrawWireCube(centerPoint.position, this.size);
            Handles.color = prevColor;
        }
#endif
    }
}