using System;
using Infrastructure.AudioControl;
using Infrastructure.HapticControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Infrastructure.UiElementFx
{
    public class PointButtonScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float dragThreshold = 5f;
        [SerializeField] private Transform root;
        [SerializeField] private Vector2 defaultScale = Vector2.one;
        [SerializeField] private Vector2 clickedScale = new(0.95f, 0.95f);
        [SerializeField] private HapticType hapticOnClick = HapticType.Selection;
        [SerializeField] private SfxType sfx = SfxType.ClickUI;

        private Vector2 startPos;
        private Action pointUpCallback;
        private bool isEnabled;
        private bool isScaleEnabled;

        public void Construct(Action pointUpCallback)
        {
            this.pointUpCallback = pointUpCallback;
            isEnabled = true;
            isScaleEnabled = true;
        }

        public void SetEnabled(bool enabled)
        {
            this.isEnabled = enabled;
        }

        public void SetScaleEnabled(bool enabled)
        {
            this.isScaleEnabled = enabled;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (!isEnabled)
                return;

            startPos = eventData.position;

            if (isScaleEnabled)
                root.transform.localScale = clickedScale;
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            float delta = Vector2.Distance(startPos, eventData.position);
            
            if (isScaleEnabled)
                root.transform.localScale = defaultScale;

            if (delta > dragThreshold)
                return;

            if (!isEnabled)
                return;

            pointUpCallback?.Invoke();
            PlayOnClick();
        }

        private void PlayOnClick()
        {
            HapticService.I.Haptic(hapticOnClick);
            AudioService.I.PlaySfx(sfx);
        }
    }
}