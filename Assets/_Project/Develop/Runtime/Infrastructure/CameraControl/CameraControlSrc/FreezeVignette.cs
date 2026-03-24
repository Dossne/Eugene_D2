using UnityEngine;

namespace Infrastructure.CameraControl
{
    public class FreezeVignette : CameraVignette
    {
        [SerializeField] private GameObject snowflakes;


        public override void Show(AnimationCurve curve, float from, float to, float duration, bool yoyo)
        {
            PlayAnimation(curve, from, to, duration, yoyo, false);
            SetAdditionalObjectActive(true);
        }


        public override void Hide(AnimationCurve curve, float from, float to, float duration)
        {
            base.Hide(curve, from, to, duration);
            SetAdditionalObjectActive(false);
        }


        public override void InstantHide()
        {
            base.InstantHide();
            SetAdditionalObjectActive(false);
        }


        private void SetAdditionalObjectActive(bool value)
        {
            if (snowflakes.activeSelf == value)
                return;

            snowflakes.SetActive(value);
        }
    }
}