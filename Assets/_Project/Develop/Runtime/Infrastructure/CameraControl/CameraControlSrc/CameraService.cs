using Features.CameraFollow;
using Features.Effects;
using Infrastructure.BroTweens;
#if PR_CHEAT
using Infrastructure.Cheat;
#endif
using Infrastructure.MainUICanvasControl;
using UnityEngine;
using VContainer;

namespace Infrastructure.CameraControl
{
    public class CameraService : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private AnimationCurve zoomCurve;

        [SerializeField, Tooltip("Used for SmoothFollow target")]
        private float smoothTime = 0.2f;

        [Header("Debug only")]
        [SerializeField] private float posX;

        [SerializeField] private float posY;
        [SerializeField] private float posZ;

        private FloatTween posYTween;
        private FloatTween posZTween;

        private Vector3 velocity = Vector3.zero;
        private BroTweenSafe shakeTween;
        private CameraVFX cameraVFX;

        private int cheatZoomStep = 0;

#if PR_CHEAT
        private CheatService cheatService;
#endif


        [Inject]
        public void Construct(MainUIProvider mainUIProvider
#if PR_CHEAT
                            , CheatService cheatService
#endif
        )
        {
            this.cameraVFX = mainUIProvider.CameraVFX;
#if PR_CHEAT
            this.cheatService = cheatService;
#endif
        }


        public void Initialize()
        {
#if PR_CHEAT
            cheatService.OnZoomIn += CheatService_OnZoomIn;
            cheatService.OnZoomOut += CheatService_OnZoomOut;
#endif

            posYTween = BroTween.Float();
            posYTween.SetSetter(SetY).SetEase(zoomCurve).SetAutoKill(false);

            posZTween = BroTween.Float();
            posZTween.SetSetter(SetZ).SetEase(zoomCurve).SetAutoKill(false);
        }


        public void Deinitialize()
        {
#if PR_CHEAT
            cheatService.OnZoomIn -= CheatService_OnZoomIn;
            cheatService.OnZoomOut -= CheatService_OnZoomOut;
#endif
            BroTween.Kill(ref posYTween);
            BroTween.Kill(ref posZTween);
            shakeTween.Kill();
        }


        public void SetCameraPosition(float posY, float posZ, float duration = 0)
        {
            if (duration > 0)
                SmoothZoom(posY, posZ, duration);
            else
                InstantZoom(posY, posZ);
        }


        public void SetPosition(Vector3 targetPos)
        {
            mainCamera.transform.localPosition = new Vector3(targetPos.x + posX, posY, targetPos.z + posZ);
            velocity = Vector3.zero;
        }


        public void SmoothFollow(Vector3 targetPos)
        {
            Transform cam = mainCamera.transform;
            Vector3 targetPosOffset = new Vector3(targetPos.x + posX, posY, targetPos.z + posZ);
            cam.localPosition = Vector3.SmoothDamp(cam.localPosition, targetPosOffset, ref velocity, smoothTime);
        }


        public Vector3 WorldToScreenPoint(Vector3 worldPos)
        {
            return mainCamera.WorldToScreenPoint(worldPos);
        }


        public Vector3 WorldToViewportPoint(Vector3 worldPos)
        {
            return mainCamera.WorldToViewportPoint(worldPos);
        }


        public Vector3 ViewportToWorldPoint(Vector3 viewportPos)
        {
            return mainCamera.ViewportToWorldPoint(viewportPos);
        }


        public void ShakeCamera(CameraShakeFxData fxData)
        {
            shakeTween.Kill();
            shakeTween = BroTween.ShakePosition(transform,
                                                Vector3.one * fxData.strength,
                                                in fxData.duration,
                                                false,
                                                in fxData.vibrato,
                                                in fxData.randomness,
                                                in fxData.fadeout,
                                                in fxData.isFullRandomness)
                                 .ToSafe();
            shakeTween.Play();
        }


        public void ShowRedVignette(VignetteAnimFxData config, bool repeat)
        {
            cameraVFX.ShowRedVignette(config, repeat);
        }


        public void HideRedVignette(bool isAnimated)
        {
            cameraVFX.HideRedVignette(isAnimated);
        }


        public void ShowFreezeVignette(VignetteAnimFxData config)
        {
            cameraVFX.ShowFreezeVignette(config);
        }


        public void HideFreezeVignette(VignetteAnimFxData config, bool isAnimated)
        {
            cameraVFX.HideFreezeVignette(config, isAnimated);
        }


        private void SmoothZoom(float toPosY, float toPosZ, float duration)
        {
            if (cheatZoomStep != 0)
                return;

            posYTween.SetParams(posY, toPosY, duration).Play();
            posZTween.SetParams(posZ, toPosZ, duration).Play();
        }


        private void SetY(float val)
        {
            posY = val;
        }


        private void SetZ(float val)
        {
            posZ = val;
        }


        private void InstantZoom(float posY, float posZ)
        {
            if (cheatZoomStep != 0)
                return;

            this.posY = posY;
            this.posZ = posZ;
            Transform tr = mainCamera.transform;
            tr.position = new Vector3(tr.position.x, posY, posZ);
        }


        private void CheatService_OnZoomOut()
        {
            cheatZoomStep++;
            posY++;
            posZ -= 0.3f;
        }


        private void CheatService_OnZoomIn()
        {
            cheatZoomStep--;
            posY--;
            posZ += 0.3f;
        }
    }
}