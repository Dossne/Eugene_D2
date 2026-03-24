using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Infrastructure.Utilities
{
    public class ConeObjectPlacer : ObjectPlacer
    {
#if UNITY_EDITOR
        [SerializeField] private bool empty = false;
        [SerializeField][Min(0.01f)] float distanceC = 0.2f;
        [SerializeField][Min(0.01f)] float distanceR = 0.2f;
        [SerializeField][Min(0.01f)] float distanceY = 0.2f;
        [SerializeField][Min(0.1f)] float radius = 2f;
        [SerializeField][Min(0)] float radiusStep = 1f;
        [SerializeField] private GameObject prefab;

        protected override bool CanGenerate()
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab не установлен!");
                return false;
            }

            return true;
        }

        protected override void GenerateImpl()
        {
            List<(Vector3 pos, Quaternion rot)> localPositions = new();
            float yOffset = 0;
            int circleCount = Mathf.FloorToInt(radius / distanceR) + 1;

            while (circleCount >= 1)
            {
                for (int i = 0; i < circleCount; i++)
                {
                    var currentRadius = distanceR * i;
                    if (currentRadius == 0)
                    {
                        if (empty && i != circleCount - 1)
                            continue;

                        localPositions.Add((Vector3.up * yOffset, Quaternion.identity));
                        continue;
                    }
                    var circleL = 2 * Mathf.PI * currentRadius;
                    var arcCount = Mathf.FloorToInt(2 * Mathf.PI * currentRadius / distanceC);
                    float angleStep = Mathf.Deg2Rad * 360f / arcCount;
                    Debug.Log($"circleL {circleL} | distanceС {distanceC} | arcCount {arcCount} | angleStep {angleStep}");

                    for (int j = 0; j < arcCount; j++)
                    {
                        if (empty && i != circleCount - 1)
                            continue;

                        localPositions.Add((new Vector3(Mathf.Cos(angleStep * j) * currentRadius, yOffset, Mathf.Sin(angleStep * j) * currentRadius),
                                            Quaternion.identity));
                    }
                }

                circleCount = Mathf.FloorToInt(circleCount - radiusStep);
                yOffset += distanceY;
            }

            PutObjectsInPositions(prefab, localPositions, distanceY);
        }
#endif
    }
}