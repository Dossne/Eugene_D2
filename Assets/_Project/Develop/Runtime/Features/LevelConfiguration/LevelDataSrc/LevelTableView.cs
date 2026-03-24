using Infrastructure.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Features.LevelConfiguration
{
    public class LevelTableView : MonoBehaviour
    {
        //Based on default character movement capsule collider size(r = 0.169) in 1.8.0.
        //There is no reason to spawn objects on planes smaller than smallest player size or closer than half of smallest player size to a wall(or in a wall).
        //Otherwise LD is broken.
        private const float SPAWN_PADDING = 0.15f;

        class TablePlaneArea
        {
            public Collider collider;
            public float area;


            public TablePlaneArea(Collider meshCollider, float area)
            {
                this.collider = meshCollider;
                this.area = area;
            }
        }

        [SerializeField] private List<LevelTableElement> levelTableElements = new List<LevelTableElement>();

        private List<TablePlaneArea> planes = new List<TablePlaneArea>();
        private List<Collider> planeCollider = new List<Collider>();
        private float totalArea;

#if UNITY_EDITOR
        public string GetAssetName()
        {
            GameObject prefab = AddressableUtils.GetSourcePrefab_Editor(gameObject);
            return AddressableUtils.GetAddressableAssetGuid_Editor(prefab);
        }
#endif

        public void Initialize()
        {
            List<Collider> colliders = new List<Collider>(GetComponentsInChildren<Collider>(includeInactive: true));            
            colliders.RemoveAll(plane => !plane.name.ToLower().Contains("plane"));
            for (int i = colliders.Count - 1; i >= 0; i--)
            {
                var tablePlane = colliders[i].GetComponentInParent<TablePlane>(true);
                if (tablePlane == null)
                    continue;

                if (tablePlane.IsExcludedFromGameplay) 
                    colliders.RemoveAt(i);
            }

            planeCollider.Clear();
            planeCollider.AddRange(colliders);

            totalArea = 0.0f;
            for (int i = 0; i < colliders.Count; ++i)
            {
                Collider meshCollider = colliders[i];
                Bounds bounds = colliders[i].bounds;
                float area = bounds.size.x * bounds.size.z;
                planes.Add(new TablePlaneArea(meshCollider, area));
                totalArea += area;
            }
        }


        public void SetScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        public void SetSkin(LevelTableSkin skin, LevelTableSkinData levelTableSkinData = null) 
        {
            if (skin == LevelTableSkin.Default)
                return;

            levelTableElements = new List<LevelTableElement>(GetComponentsInChildren<LevelTableElement>(includeInactive: true));
            for (int i = 0; i < levelTableElements.Count; i++)
            {
                if (levelTableSkinData == null)
                {
                    levelTableElements[i].SetMaterial(null);
                    continue;
                }

                switch (levelTableElements[i].ElementType)
                {
                    case LevelTableElementType.TopPlane:
                        levelTableElements[i].SetMaterial(levelTableSkinData.topPlane);
                        break;
                    case LevelTableElementType.SidePlane:
                        levelTableElements[i].SetMaterial(levelTableSkinData.sidePlane);
                        break;
                    case LevelTableElementType.TableBase:
                        levelTableElements[i].SetMaterial(levelTableSkinData.tableBase);
                        break;
                    case LevelTableElementType.Background:
                        levelTableElements[i].SetMaterial(levelTableSkinData.background);
                        break;
                    default:
                        levelTableElements[i].SetMaterial(null);
                        break;
                }
            }
            
        }

        public Vector3 GetRandomPoint(bool useSpawnPadding = true)
        {
            float padding = useSpawnPadding ? SPAWN_PADDING : 0.0f;
            Collider randomPlane = GetRandomPlane();

            Vector3 localPoint = new Vector3(
                Random.Range(-randomPlane.bounds.extents.x + padding, randomPlane.bounds.extents.x - padding),
                0f,
                Random.Range(-randomPlane.bounds.extents.z + padding, randomPlane.bounds.extents.z - padding)
            );

            return randomPlane.bounds.center + localPoint;
        }


        public Vector3 GetClosestPointInPlanes(Vector3 point)
        {
            return GameplayUtils.GetClosestPoint(planeCollider, point);
        }


        private Collider GetRandomPlane()
        {
            float random = Random.Range(0.0f, totalArea);
            float sum = 0;
            for (int i = 0; i < planes.Count; ++i)
            {
                TablePlaneArea plane = planes[i];
                float area = plane.area;
                sum += area;
                if (sum > random)
                {
                    return plane.collider;
                }
            }

            return planes[0].collider;
        }


        [TriInspector.Button]
        private void CollectElements()
        {
            levelTableElements = new List<LevelTableElement>(GetComponentsInChildren<LevelTableElement>(includeInactive: true));
        }
    }
}