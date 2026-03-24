using System;
using UnityEngine;

namespace Features.WinStreak
{
    [Serializable]
    public class WinStreakSpawnParams
    {
        [Header("WinStreak Object")]
        [Tooltip("0 is left, 1 is right side")] public float startXPosNorm = -0.2f;
        [Tooltip("0 is left, 1 is right side")] public float endXPosNorm = 1.2f;
        [Tooltip("0 is bottom, 1 is top")] public float yPosNorm = 0.25f;
        [Tooltip("0 camera frustum pos, plus value is down from it to scene, minus is up from it to sky")] public float depthFromCamera = 1.5f;

        [Header("Booster target random pos")]
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
        public float minZ;
        public float maxZ;
    }
}