using UnityEngine;

namespace Infrastructure.Utilities
{
    public class TableBorder : MonoBehaviour
    {
        [SerializeField] private BoxCollider wallCollider;
        [SerializeField] private float wallHeight = 3;
        [SerializeField] private float wallLengthMargin = 0.4f;
        [SerializeField] private float wallWidth = 0.3f;
        [SerializeField] private float innerCornerPadding = 0.3f;

        private void Start()
        {
#if !UNITY_EDITOR
            enabled = false;
#endif
        }


        public void SetSize(float size, 
                            TableCorner.Orientation clockwiseAngleOrientation, 
                            bool clockwiseAngleEnabled, 
                            TableCorner.Orientation antiClockwiseAngleOrientation, 
                            bool antiClockwiseAngleEnabled)
        {
            var clockwiseOffset     = clockwiseAngleOrientation     == TableCorner.Orientation.AntiClockwise && clockwiseAngleEnabled     ? innerCornerPadding : 0;
            var antiClockwiseOffset = antiClockwiseAngleOrientation == TableCorner.Orientation.Clockwise     && antiClockwiseAngleEnabled ? innerCornerPadding : 0;
            var sizeWithOffset = size - (clockwiseOffset + antiClockwiseOffset);
            transform.localScale = new Vector3(sizeWithOffset, 1, 1);
            wallCollider.size = new Vector3(1 + wallLengthMargin / sizeWithOffset, wallHeight, wallWidth);
        }

        public void SetPosition(Vector3 position, 
                                TableCorner.Orientation clockwiseAngleOrientation,
                                bool clockwiseAngleEnabled,
                                TableCorner.Orientation antiClockwiseAngleOrientation,
                                bool antiClockwiseAngleEnabled)
        {
            var clockwiseOffset     = clockwiseAngleOrientation     == TableCorner.Orientation.AntiClockwise && clockwiseAngleEnabled     ? innerCornerPadding * 0.5f : 0;
            var antiClockwiseOffset = antiClockwiseAngleOrientation == TableCorner.Orientation.Clockwise     && antiClockwiseAngleEnabled ? innerCornerPadding * 0.5f : 0;

            var offset = antiClockwiseOffset - clockwiseOffset;
            transform.position = position + transform.right * offset;
        }

        public void SetObjectActive(bool isActive)
        {
            if (gameObject.activeSelf == isActive)
                return;

            gameObject.SetActive(isActive);
        }
    }
}