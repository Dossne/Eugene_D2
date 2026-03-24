using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Infrastructure.Utilities
{
    public class SphereObjectPlacer : ObjectPlacer
    {
#if UNITY_EDITOR
        [SerializeField] private bool rotateFaces = false;
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
            var nextY = 0f;
            List<float> radiuses = new List<float>();
            while (true)
            {
                var angle = Mathf.Asin(nextY / radius);
                radiuses.Add(radius * Mathf.Cos(angle));

                if (nextY == radius)
                    break;

                nextY += distanceY;
                nextY = Mathf.Min(nextY, radius);
            }                

            for (int r = 0; r < radiuses.Count; r++)
            {
                float yOffset = distanceY * r;
                int circleCount = Mathf.FloorToInt(radiuses[r] / distanceR) + 1;
                var nextRadius = radiuses.Count > r + 1 ? radiuses[r + 1] : 0;
                var gapCount = Mathf.FloorToInt((radiuses[r] - nextRadius + distanceR * 0.25f) / distanceR);

                for (int i = 0; i < circleCount; i++)
                {
                    var currentRadius = radiuses[r] - distanceR * i;

                    if (currentRadius == 0)
                    {
                        if (empty && i != 0)
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
                        if (empty && i > gapCount)
                            continue;

                        var pos = new Vector3(Mathf.Cos(angleStep * j) * currentRadius, 0, Mathf.Sin(angleStep * j) * currentRadius);
                        var radDirection = pos - Vector3.zero;
                        var rot = rotateFaces ? Quaternion.LookRotation(-radDirection, Vector3.up) : Quaternion.identity;

                        localPositions.Add((new Vector3(Mathf.Cos(angleStep * j) * currentRadius,  yOffset + radius, Mathf.Sin(angleStep * j) * currentRadius),
                                            rot));
                        localPositions.Add((new Vector3(Mathf.Cos(angleStep * j) * currentRadius, -yOffset + radius, Mathf.Sin(angleStep * j) * currentRadius),
                                            rot));
                    }
                }
            }

            PutObjectsInPositions(prefab, localPositions, distanceY);
        }
#endif
    }
}