using System.Collections.Generic;

using UnityEngine;

namespace Infrastructure.Utilities
{
    public class SpiralObjectPlacer : ObjectPlacer
    {
#if UNITY_EDITOR
        [SerializeField] private bool clockwise = true;
        [SerializeField] private bool rotateFaces = false;
        [SerializeField][Min(0.01f)] float distanceC = 0.2f;
        [SerializeField][Min(0.01f)] float distanceY = 0.2f;
        [SerializeField][Min(0.01f)] float minRadius = 0.1f;
        [SerializeField][Min(0.1f)] float maxRadius = 2f;
        [SerializeField][Min(0)] float radiusStep = 0.1f;
        [SerializeField] private GameObject prefab1;
        [SerializeField] private GameObject prefab2;

        protected override bool CanGenerate()
        {
            if (prefab1 == null)
            {
                Debug.LogError("Prefab1 не установлен!");
                return false;
            }

            if (minRadius > maxRadius)
            {
                Debug.LogError("MinRadius > MaxRadius");
                return false;
            }

            return true;
        }


        protected override void GenerateImpl()
        {
            List<(Vector3 pos, Quaternion rot)> localPositions = new();

            float prevAngle = 0;
            var currentRadius = minRadius;
            var radiusStepPart = 0f;
            var circleRads = Mathf.Deg2Rad * 360f;
            var sign = clockwise ? -1 : 1;

            while (currentRadius <= maxRadius) 
            {                
                if (currentRadius == 0)
                {
                    localPositions.Add((Vector3.zero, Quaternion.identity));
                    continue;
                }

                while (true) 
                {
                    var currenCircleRadius = currentRadius + radiusStep * radiusStepPart;
                    var arcCount = Mathf.FloorToInt(2 * Mathf.PI * currenCircleRadius / distanceC);
                    float angleStep = circleRads / arcCount;
                    prevAngle += angleStep;
                    radiusStepPart += angleStep / circleRads;

                    var pos = new Vector3(Mathf.Cos(sign * prevAngle) * currenCircleRadius,
                                          0,
                                          Mathf.Sin(sign * prevAngle) * currenCircleRadius);

                    var radDirection = pos - Vector3.zero;
                    var dir90 =  Quaternion.Euler(0, sign * 90, 0) * radDirection;

                    var rot = rotateFaces ? Quaternion.LookRotation(dir90, Vector3.up) : Quaternion.identity;
                    localPositions.Add((pos, rot));
                    if (prevAngle >= circleRads)
                    {
                        prevAngle -= circleRads;
                        radiusStepPart -= 1;
                        break;
                    }

                    if (currenCircleRadius >= maxRadius)
                        break;
                }                

                currentRadius += radiusStep;
            }

            PutObjectsInPositions(prefab1, prefab2, localPositions);
        }

        protected void PutObjectsInPositions(GameObject prefab1, GameObject prefab2, List<(Vector3 pos, Quaternion rot)> posAndRot)
        {
            bool needSwitch = prefab2 != null;
            int index = 0;
            var prefab = prefab2;
            for (int i = 0; i < posAndRot.Count; i++)
            {
                if (needSwitch)
                    prefab = prefab == prefab2 ? prefab1 : prefab2;
                else
                    prefab = prefab1;
                CreateObjectAtPosition(prefab, posAndRot[i].pos, posAndRot[i].rot, 0, index);
                index++;
            }
        }
#endif
    }
}