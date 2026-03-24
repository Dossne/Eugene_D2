using UnityEngine;

namespace Features.TargetMarker
{
    public class TargetMarkersRoot : MonoBehaviour
    {
        [SerializeField] private RectTransform main;
        [SerializeField] private RectTransform clampedRoot;

        public RectTransform Main => main;
        public RectTransform Clamped => clampedRoot;
    }
}