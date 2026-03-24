using System.Collections.Generic;
using Features.Character;
using Infrastructure.CameraControl;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Pool;
using Infrastructure.SystemsLifeCycle;
using UnityEngine;

namespace Features.TargetMarker
{

    public class TargetMarkerService : ISystemTickable
    {
        private readonly TargetMarkersRoot targetMarkersRoot;
        private readonly CameraService cameraService;
        private readonly CharacterManager characterManager;
        private readonly PoolService poolService;
        private readonly List<TargetMarkerEntry> targets;
        private TargetIconPointerPool pointerPool;
        private TargetMarkerPool markerPool;

        private bool isInit;


        public TargetMarkerService(CameraService cameraService, MainUIProvider mainUIProvider, CharacterManager characterManager,
            PoolService poolService)
        {
            this.cameraService = cameraService;
            this.targetMarkersRoot = mainUIProvider.TargetMarkersRoot;
            this.characterManager = characterManager;

            this.poolService = poolService;
            this.targets = new List<TargetMarkerEntry>();
        }


        public void Initialize()
        {
            if (isInit)
                return;

            pointerPool = poolService.Get<TargetIconPointerPool>();
            markerPool = poolService.Get<TargetMarkerPool>();

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            ClearTargets();
            isInit = false;
        }


        void ISystemTickable.Tick()
        {
            for (var i = targets.Count - 1; i >= 0; i--)
            {
                var target = targets[i];
                if (target.NeedRemove)
                {
                    target.ReleaseViews();
                    targets.RemoveAt(i);
                    continue;
                }

                UpdatePosition(target);
            }
        }


        public void AddTarget(Transform target, Sprite icon)
        {
            if (!markerPool.TryGetItem(out PoolableTargetMarker marker))
                return;

            if (!pointerPool.TryGetItem(out PoolableTargetIconPointer pointer))
                return;

            pointer.Construct(icon);

            targets.Add(new TargetMarkerEntry { markerView = marker, pointerView = pointer, target = target });
        }


        public void MarkForRemove(Transform target)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].target == target)
                {
                    targets[i].MarkForRemove();
                    break;
                }
            }
        }


        private void ClearTargets()
        {
            foreach (var targetEntry in targets)
            {
                targetEntry.ReleaseViews();
            }

            targets.Clear();
        }


        private void UpdatePosition(TargetMarkerEntry targetMarkerEntry)
        {
            Vector3 targetPos = targetMarkerEntry.target.position;
            if (IsTargetVisible(targetPos))
            {
                UpdateMarker(targetMarkerEntry.markerView, targetPos);
                targetMarkerEntry.SwitchToMarker();
            }
            else
            {
                UpdatePointer(targetMarkerEntry.pointerView, targetPos);
                targetMarkerEntry.SwitchToPointer();
            }
        }


        private void UpdatePointer(PoolableTargetIconPointer pointerView, Vector3 targetPos)
        {
            Vector2 newPos = GetClampedScreenPosition(targetPos);
            pointerView.SetLocalPosition(newPos);

            Vector3 toTarget = targetPos - characterManager.GetPosition();
            float angle = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
            pointerView.SetRotation(Quaternion.Euler(0, 0, -angle));
        }

        private void UpdateMarker(PoolableTargetMarker markerView, Vector3 targetPos)
        {
            Vector2 screenPos = cameraService.WorldToScreenPoint(targetPos);
            markerView.SetPosition(screenPos);
        }
        
        

        private bool IsTargetVisible(Vector3 worldPos)
        {
            Vector3 viewport = cameraService.WorldToViewportPoint(worldPos);
            return viewport.z > 0 && viewport.x > 0 && viewport.x < 1 && viewport.y > 0 && viewport.y < 1;
        }


        private Vector2 GetClampedScreenPosition(Vector3 worldPos)
        {
            Vector3 screenPos = cameraService.WorldToScreenPoint(worldPos);

            if (screenPos.z < 0f)
                screenPos *= -1f;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetMarkersRoot.Clamped, screenPos, null, out Vector2 localPoint);
            Rect rect = targetMarkersRoot.Clamped.rect;
            Vector2 clampedLocalPoint = new Vector2(Mathf.Clamp(localPoint.x, rect.xMin, rect.xMax), Mathf.Clamp(localPoint.y, rect.yMin, rect.yMax));
            return clampedLocalPoint;
        }
    }
}