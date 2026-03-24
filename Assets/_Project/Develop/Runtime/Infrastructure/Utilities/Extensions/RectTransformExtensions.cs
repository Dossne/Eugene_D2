using UnityEngine;

namespace Infrastructure.Utilities
{
    public static class RectTransformExtensions
    {
        public static void AnchorToCenter(this RectTransform rect)
        {
            Vector2 center = new Vector2(0.5f, 0.5f);
            rect.pivot = center;
            rect.anchorMin = center;
            rect.anchorMax = center;
        }

        public static void StretchFull(this RectTransform rect)
        {
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void StretchHorizontalTop(this RectTransform rect)
        {
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(0, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0, rect.offsetMax.y);
        }

        public static void StretchHorizontalCenter(this RectTransform rect)
        {
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.offsetMin = new Vector2(0, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0, rect.offsetMax.y);
        }
        
        public static void StretchHorizontalBottom(this RectTransform rect)
        {
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.offsetMin = new Vector2(0, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0, rect.offsetMax.y);
        }
        
        
        public static bool Contains(this RectTransform a, RectTransform b)
        {
            var awr = a.WorldRect();
            var bwr = b.WorldRect();
            return awr.Contains(bwr.min) && awr.Contains(bwr.max);
        }

        public static bool Overlaps(this RectTransform a, RectTransform b)
        {
            return a.WorldRect().Overlaps(b.WorldRect());
        }

        public static bool Overlaps(this RectTransform a, RectTransform b, bool allowInverse)
        {
            return a.WorldRect().Overlaps(b.WorldRect(), allowInverse);
        }

        public static Rect WorldRect(this RectTransform rectTransform)
        {
            var localRect = rectTransform.rect;
            return new Rect()
            {
                min = rectTransform.TransformPoint(localRect.min),
                max = rectTransform.TransformPoint(localRect.max)
            };
        }
    }
}