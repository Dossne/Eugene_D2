using UnityEngine;
using UnityEngine.UI;


namespace Infrastructure.Utilities
{
    [RequireComponent(typeof(LayoutElement))]
    public class LayoutElementMaxHeightLimiter : MonoBehaviour, ILayoutSelfController
    {
        public float MaxHeight { get; set; } = 200;


        public void SetLayoutHorizontal() { }


        public void SetLayoutVertical()
        {
            var rt = (RectTransform)transform;
            var size = rt.sizeDelta;
            if (rt.rect.height > MaxHeight)
                size.y = MaxHeight;
            rt.sizeDelta = size;
        }
    }
}

