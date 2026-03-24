using UnityEngine;
using UnityEngine.UI;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedMember.Local

#endregion

namespace SayKitInternal
{
    public class InPlayService
    {
        public static InPlayService Instance { get; } = new InPlayService();

        private const string TAG = "[InPlayService]";

        public bool IsInPlay()
        {
            return !string.IsNullOrEmpty(SKManager.Instance.RemoteConfig.ads_settings.inplay_id);
        }

        public void Show(RectTransform promoRect, Image inPlayImage, Canvas canvas)
        {
#if UNITY_EDITOR
            EditorShow(promoRect, inPlayImage, canvas);
#else
            RuntimeShow(promoRect);
#endif
        }

        public void Hide(RectTransform promoRect)
        {
#if UNITY_EDITOR
            promoRect?.gameObject.SetActive(false);
#else
            SKBridgeManager.Instance.HideInPlay();
#endif
        }

        private void EditorShow(RectTransform inPlayBox, Image inPlayImage, Canvas canvas)
        {
            if (inPlayBox == null || inPlayImage == null || canvas == null)
            {
                SayKitDebug.LogError($"{TAG} Missing InPlay references.");
                return;
            }

            var canvasRT = canvas.transform as RectTransform;
            if (canvasRT == null)
            {
                SayKitDebug.LogError($"{TAG} Canvas has no RectTransform.");
                return;
            }

            Camera cam = null;
            switch (canvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    cam = null;
                    break;
                case RenderMode.ScreenSpaceCamera:
                case RenderMode.WorldSpace:
                    cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
                    break;
            }

            var worldCorners = new Vector3[4];
            inPlayBox.GetWorldCorners(worldCorners);

            var fits = true;
            for (var i = 0; i < 4; i++)
            {
                var sp = RectTransformUtility.WorldToScreenPoint(cam, worldCorners[i]);
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, sp, cam, out var local))
                {
                    fits = false;
                    break;
                }

                if (!canvasRT.rect.Contains(local))
                {
                    fits = false;
                    break;
                }
            }

            inPlayImage.color = fits ? Color.green : Color.red;

            if (!fits)
            {
                SKDisplayUtils.GetScreenRectPx(inPlayBox, out var topLeftPx, out var sizePx);
                SayKitDebug.LogWarning($"{TAG}[ShowInPlay] Rect {topLeftPx} size {sizePx} is outside of Canvas bounds {canvasRT.rect.size}");
            }

            inPlayBox.gameObject.SetActive(true);
        }

        private void RuntimeShow(RectTransform inPlayBox)
        {
            if (SKBridgeManager.Instance.IsInPlayAvailable())
            {
                if (inPlayBox == null)
                {
                    SayKitDebug.LogError($"{TAG}[RuntimeShow] inPlayBox is null.");
                    return;
                }

                SKDisplayUtils.GetScreenRectPx(inPlayBox, out var topLeftPx, out var sizePx);
                var centerPx = new Vector2(
                    topLeftPx.x + sizePx.x * 0.5f,
                    topLeftPx.y + sizePx.y * 0.5f
                );

                var vPos = SKDisplayUtils.ConvertPxToPoints(centerPx);
                var vSize = SKDisplayUtils.ConvertPxToPoints(sizePx);

                SKBridgeManager.Instance.ShowInPlay(
                    Mathf.RoundToInt(vPos.x),
                    Mathf.RoundToInt(Mathf.Abs(vPos.y)),
                    Mathf.RoundToInt(vSize.x),
                    Mathf.RoundToInt(vSize.y));
            }
            else
            {
                SayKitDebug.LogWarning($"{TAG}[ShowInPlay] InPlay not available.");
            }
        }
    }
}