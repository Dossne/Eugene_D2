using UnityEngine;

namespace Features.ProgressBar
{
    public class SegmentBorder : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        
        public void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }


        public void SetAnchoredPosition(Vector2 anchoredPosition)
        {
            rectTransform.anchoredPosition = anchoredPosition;
        }
    }
}