using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Utilities
{
    public enum PinType
    {
        Bottom = 0, //pin at bottom while scrolling up
        Top = 1,    //pin at top while scrolling down
    }

    /// <summary>
    /// Can stick scroll element to desired root by PinType
    /// </summary>
    public class StickySlotController : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform pinnedRoot; //copy of viewport but lower in hierarchy
        [SerializeField] private PinType pinPos;
        [SerializeField] private bool isEnabled = true;
        
        [Header("Debug")]
        [SerializeField, TriInspector.ReadOnly] private RectTransform target;

        private readonly Vector3[] rootCorners = new Vector3[4];
        private readonly Vector3[] targetCorners = new Vector3[4];
        private RectTransform viewport;
        private RectTransform contentRoot;
        private Vector2 targetAnchoredPos;
        private float targetHeight;
        private int targetSiblingIdx;
        private bool isPinned;

        public void Initialize()
        {
            viewport = scrollRect.viewport;
            contentRoot = scrollRect.content;
            pinnedRoot.GetWorldCorners(rootCorners);

            scrollRect.onValueChanged.AddListener(CheckState);
        }

        public void Deinitialize()
        {
            scrollRect.onValueChanged.RemoveListener(CheckState);
            target = null;
        }

        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
        }
        
        public void SetTarget(RectTransform target, Vector2 anchoredPos, int siblingIdx, float slotHeight)
        {
            this.target = target;
            this.targetAnchoredPos = anchoredPos;
            this.targetSiblingIdx = siblingIdx;
            this.targetHeight = slotHeight;
            CheckState(Vector2.zero);
        }

        public void UnpinTarget()
        {
            if (this.target != null && isPinned)
            {
                Unpin();
            }
        }

        private void CheckState(Vector2 screenPos)
        {
            if(!isEnabled)
                return;
            
            bool shouldBePinned = ShouldPin();

            if (shouldBePinned && !isPinned)
                Pin();
            else if (!shouldBePinned && isPinned)
                Unpin();
        }

        private void Pin()
        {
            isPinned = true;
            target.SetParent(pinnedRoot, worldPositionStays: true);
            ClampInside(target);
        }

        private void Unpin()
        {
            isPinned = false;
            target.SetParent(contentRoot, worldPositionStays: false);
            target.SetSiblingIndex(targetSiblingIdx);
            target.anchoredPosition = targetAnchoredPos;
        }

        private bool ShouldPin()
        {
            return pinPos switch
            {
                PinType.Bottom => ShouldPinToBottom(),
                _              => ShouldPinToTop()
            };
        }

        private bool ShouldPinToBottom()
        {
            var slotTop = contentRoot.anchoredPosition.y + targetAnchoredPos.y;
            var slotBottom = slotTop                     - targetHeight;
            return slotBottom < -viewport.rect.height;
        }

        private bool ShouldPinToTop()
        {
            var slotTop = contentRoot.anchoredPosition.y + targetAnchoredPos.y;
            return slotTop > 0;
        }

        private void ClampInside(RectTransform target)
        {
            target.GetWorldCorners(targetCorners);

            Vector3 offset = Vector3.zero;

            if (targetCorners[0].x < rootCorners[0].x)
                offset.x = rootCorners[0].x - targetCorners[0].x;

            if (targetCorners[2].x > rootCorners[2].x)
                offset.x = rootCorners[2].x - targetCorners[2].x;

            if (targetCorners[0].y < rootCorners[0].y)
                offset.y = rootCorners[0].y - targetCorners[0].y;

            if (targetCorners[2].y > rootCorners[2].y)
                offset.y = rootCorners[2].y - targetCorners[2].y;

            target.position += offset;
        }
    }
}