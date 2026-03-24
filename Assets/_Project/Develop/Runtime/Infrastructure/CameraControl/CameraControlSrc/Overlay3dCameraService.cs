using Infrastructure.MainUICanvasControl;
using Infrastructure.QualityGraphicsControl;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


namespace Infrastructure.CameraControl
{
    public class Overlay3dCameraService : MonoBehaviour
    {
        [SerializeField] private Camera overlay3dCamera;
        private RawImage rtImage;

        private GraphicsQualityHandler graphicsQualityHandler;
        private List<Object> users = new List<Object>();

        [Inject]
        public void Construct(GraphicsQualityHandler graphicsQualityHandler, MainUIProvider mainUIProvider)
        {
            this.graphicsQualityHandler = graphicsQualityHandler;
            rtImage = mainUIProvider.Overlay3d;
        }


        public void Initialize()
        {
            int width = graphicsQualityHandler.CurrentResolution.x;
            int height = graphicsQualityHandler.CurrentResolution.y;

#if UNITY_EDITOR
            width = Screen.width;
            height = Screen.height;
#endif
            var rt = overlay3dCamera.targetTexture;
            rt.Release();
            rt.width = width;
            rt.height = height;
            rt.Create();
            overlay3dCamera.enabled = true;
        }


        public void Deinitialize()
        {
            users.Clear();            
        }


        public void Acquire(Object user)
        {
            if (users.Count == 0)
            {                
                overlay3dCamera.gameObject.SetActive(true);
                rtImage.gameObject.SetActive(true);
            }

            users.Add(user);
        }


        public void Release(Object user)
        {
            users.Remove(user);
            if (users.Count == 0)
            {
                rtImage.gameObject.SetActive(false);
                overlay3dCamera.gameObject.SetActive(false);
            }
        }


        public void Overlay3DCameraRefreshRt()
        {
            Deinitialize();
            Initialize();
        }


        public Vector3 RectTransformToWorldPointOverlay3DCamera(RectTransform rectTransform, Vector2 localPoint, float zDistanceFromNearClipPlane)
        {
            Vector3 screenPoint = rectTransform.TransformPoint(localPoint);
            screenPoint.z = overlay3dCamera.transform.position.z + overlay3dCamera.nearClipPlane + zDistanceFromNearClipPlane;
            return overlay3dCamera.ScreenToWorldPoint(screenPoint);
        }
    }
}