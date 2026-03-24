using UnityEngine;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment

#endregion

namespace SayKitInternal
{
    public static class SKDisplayUtils
    {
        public static void LogCurrentResolutionInfo()
        {
            var wPx = Screen.width;
            var hPx = Screen.height;

            var scale = SKBridgeManager.Instance.GetScreenScale();
            var wLogical = ConvertPxToPoints(new Vector2(wPx, 0f), scale).x;
            var hLogical = ConvertPxToPoints(new Vector2(0f, hPx), scale).y;

            var unit = Application.platform switch
            {
                RuntimePlatform.IPhonePlayer => "pt",
                RuntimePlatform.Android => "dp",
                _ => "px"
            };

            var dpi = Screen.dpi > 0f ? Screen.dpi : 0f;

            Debug.Log(
                $"[Display]\n" +
                $"Pixels: {wPx} × {hPx}\n" +
                $"Logical: {wLogical:F1} × {hLogical:F1} ({unit})\n" +
                $"Scale: {scale:F3}" + (dpi > 0f ? $", DPI: {dpi:F1}" : string.Empty)
            );
        }
        
        public static void GetScreenRectPx(RectTransform rectTransform, out Vector2 topLeftPx, out Vector2 sizePx)
        {
            var canvas = rectTransform.GetComponentInParent<Canvas>();
            Camera cam = null;
            if (canvas != null)
            {
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
            }

            rectTransform.GetWorldCorners(_corners);

            var tl = RectTransformUtility.WorldToScreenPoint(cam, _corners[1]);
            var tr = RectTransformUtility.WorldToScreenPoint(cam, _corners[2]);
            var bl = RectTransformUtility.WorldToScreenPoint(cam, _corners[0]);

            topLeftPx = new Vector2(tl.x, Screen.height - tl.y);
            sizePx = new Vector2(tr.x - tl.x, tl.y - bl.y);
        }
        
        public static Vector2 ConvertPxToPoints(Vector2 px)
        {
            var scale = SKBridgeManager.Instance.GetScreenScale();
            return ConvertPxToPoints(px, scale);
        }
        
        public static Vector2 ConvertPointsToPx(Vector2 points)
        {
            var scale = SKBridgeManager.Instance.GetScreenScale();
            return points * Mathf.Max(0.0001f, scale);
        }

        private static readonly Vector3[] _corners = new Vector3[4];

        private static Vector2 ConvertPxToPoints(Vector2 px, float screenScale)
        {
            var scale = Mathf.Max(0.0001f, screenScale);
            return px / scale;
        }
    }
}