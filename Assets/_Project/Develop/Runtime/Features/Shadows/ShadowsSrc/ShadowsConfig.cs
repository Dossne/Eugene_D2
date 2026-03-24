using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Features.Shadows
{
    [CreateAssetMenu(fileName = "ShadowsConfig", menuName = "Config/Rendering/ShadowsConfig")]
    public class ShadowsConfig : ScriptableObject
    {
#if UNITY_EDITOR
        public bool isDebug = false;
#endif
        [Header("Shadow distance")]
        [SerializedDictionary("Level", "Shadows Max Distance")]
        [SerializeField] SerializedDictionary<int, float> shadowDistance = new SerializedDictionary<int, float>();

        [Header("Shadow off")]
        [SerializeField] float shadowOffMaxY = 0.15f;
        [SerializedDictionary("Level", "Collectable size")]
        [SerializeField] SerializedDictionary<int, int> shadowOff = new SerializedDictionary<int, int>();



        public SerializedDictionary<int, float> ShadowDistance => shadowDistance;
        public float ShadowOffMaxY => shadowOffMaxY;
        public SerializedDictionary<int, int> ShadowOff => shadowOff;



        public float GetShadowsMaxDistanceByLevel(int level)
        {
            if(shadowDistance.ContainsKey(level))
            {
                return shadowDistance[level];
            }

            int maxLevel = 0;
            foreach (var data in shadowDistance)
            {
                maxLevel = Math.Max(maxLevel, data.Key);
            }
            return shadowDistance[maxLevel];
        }


        public int GetShadowsOffByLevel(int level)
        {
            if (shadowOff.ContainsKey(level))
            {
                return shadowOff[level];
            }

            int maxLevel = 0;
            foreach (var data in shadowOff)
            {
                maxLevel = Math.Max(maxLevel, data.Key);
            }
            return shadowOff[maxLevel];
        }
    }
}


