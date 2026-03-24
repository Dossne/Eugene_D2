using Infrastructure.Utilities;
using TMPro;
using UnityEngine;

namespace Features.Competition
{
    public class LabeledSegment : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private TextMeshProUGUI labelTxt;
        [SerializeField] private RectTransform border;

        public float Width => rectTransform.rect.width;
        public Vector2 Position => rectTransform.position;
        public RectTransform RectTransform => rectTransform;
        
        public void SetLabelText(string text)
        {
            this.labelTxt.text = text;
        }

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

        
        public void SetBorderActive(bool isLast)
        {
            if(border != null)
                border.gameObject.SetObjectActive(!isLast);
        }
        
        public void SetWidth(float value)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
        }
    }
}