using System.Collections.Generic;
using UnityEngine;

namespace Features.Competition
{
    public class MultiplierSegmentsRoot : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private RectTransform bordersRoot;
        [SerializeField] private LabeledSegment segmentPf;
        [SerializeField] private List<LabeledSegment> items = new();

        [Header("Debug")]
        [SerializeField] private List<string> currentTexts;

        public float SegmentWidth => items[0].Width;
        public float MaxWidth => bordersRoot.rect.width;
        public float MaxCount => currentTexts.Count;

        public void Construct(List<string> currentTexts)
        {
            this.currentTexts = currentTexts;
        }

        public void Initialize()
        {
            RefreshSegments();
        }

        public Vector2 GetPosition(int i)
        {
            return items[i].Position;
        }
        
        public RectTransform GetRectTransform(int i)
        {
            return items[i].RectTransform;
        }

        [TriInspector.Button]
        private void RefreshSegments()
        {
            int segmentCount = currentTexts.Count;

            int targetBordersCount = segmentCount     - 1;
            int instantiateCount = targetBordersCount - items.Count;

            for (int i = 0; i < instantiateCount; i++)
            {
                var border = Instantiate(segmentPf, bordersRoot);
                items.Add(border);
            }

            float segmentWidth = bordersRoot.rect.width / segmentCount;
            float posX = -bordersRoot.rect.width        * bordersRoot.pivot.x;

            for (var i = 0; i < items.Count; i++)
            {
                if (i <= targetBordersCount)
                {
                    float x = posX + segmentWidth * i + segmentWidth * 0.5f;
                    Vector2 anchoredPos = new Vector2(x, 0);
                    items[i].SetWidth(segmentWidth);
                    items[i].SetAnchoredPosition(anchoredPos);
                    items[i].SetLabelText(currentTexts[i]);
                    items[i].SetBorderActive(i == targetBordersCount);
                    items[i].SetObjectActive(true);
                }
                else
                {
                    items[i].SetObjectActive(false);
                }
            }
        }
    }
}