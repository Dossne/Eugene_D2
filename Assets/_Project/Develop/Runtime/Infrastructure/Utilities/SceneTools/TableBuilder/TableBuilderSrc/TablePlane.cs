using System;
using UnityEngine;

namespace Infrastructure.Utilities
{
    public class TablePlane : MonoBehaviour
    {
        [SerializeField] private BoxCollider planeCollider;
        [SerializeField] private float planeHeight = 0.5f;
        [SerializeField] private float planeLengthMargin = 0.4f;
        [SerializeField] private float planeWidthMargin = 0.4f;
        [SerializeField] private bool isExcludedFromGameplay = false;

        public bool IsExcludedFromGameplay => isExcludedFromGameplay;

        private void Start()
        {
#if !UNITY_EDITOR
            enabled = false;
#endif
        }

        public void SetSize(float width, float height)
        {
            transform.localScale = new Vector3(width, 1, height);
            planeCollider.size = new Vector3(1 + planeWidthMargin / width, planeHeight, 1 + planeLengthMargin / height);
        }

        public void SetExcludedFromGameplay(bool isExcludedFromGameplay)
        {
            this.isExcludedFromGameplay = isExcludedFromGameplay;
        }
    }
}