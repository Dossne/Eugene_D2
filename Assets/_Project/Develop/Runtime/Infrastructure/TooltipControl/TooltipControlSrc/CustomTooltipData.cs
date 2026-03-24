using System;
using UnityEngine;

namespace Infrastructure.TooltipControl
{
    [Serializable]
    public class CustomTooltipData
    {
        public Vector3 anchoredPosition = Vector3.zero;
        public Vector2 anchorMin = new(0.5f, 0.5f);
        public Vector2 anchorMax = new(0.5f, 0.5f);
        public Vector2 pivot     = new(0.5f, 0.5f);
    }
}