using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Infrastructure.Utilities
{
    public class PyramidObjectPlacer : ObjectPlacer
    {
#if UNITY_EDITOR
        [SerializeField] private bool empty = false;
        [SerializeField][Min(0.01f)] float distanceX = 0.2f;
        [SerializeField][Min(0.01f)] float distanceY = 0.2f;
        [SerializeField][Min(0.01f)] float distanceZ = 0.2f;
        [SerializeField][Min(0.1f)] float sizeX = 2f;
        [SerializeField][Min(0.1f)] float sizeZ = 2f;
        [SerializeField][Min(0)] float layerStepX = 1f;
        [SerializeField][Min(0)] float layerStepZ = 1f;
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
            int xCountByStep = Mathf.FloorToInt(sizeX / distanceX) + 1;
            int zCountByStep = Mathf.FloorToInt(sizeZ / distanceZ) + 1;
            while (xCountByStep >= 1 && zCountByStep >= 1)
            {
                var xDiscreteSize = distanceX * (xCountByStep - 1);
                var zDiscreteSize = distanceZ * (zCountByStep - 1);

                for (int i = 0; i < xCountByStep; i++)
                {
                    for (int j = 0; j < zCountByStep; j++)
                    {
                        if (empty && !((i == 0 || i == xCountByStep - 1) || (j == 0 || j == zCountByStep - 1)))
                            continue;

                        localPositions.Add((new(distanceX * i - xDiscreteSize / 2,
                                                yOffset,
                                                distanceZ * j - zDiscreteSize / 2),
                                            Quaternion.identity));
                    }
                }

                xCountByStep = Mathf.FloorToInt(xCountByStep - layerStepX);
                zCountByStep = Mathf.FloorToInt(zCountByStep - layerStepZ);
                yOffset += distanceY;
            }

            PutObjectsInPositions(prefab, localPositions, distanceY);
        }
#endif
    }
}