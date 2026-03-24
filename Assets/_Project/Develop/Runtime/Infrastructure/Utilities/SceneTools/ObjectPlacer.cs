using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class ObjectPlacer : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField, TriInspector.ReadOnly] protected List<Transform> objects; 

    protected void PutObjectsInPositions(GameObject prefab, List<(Vector3 pos, Quaternion rot)> posAndRot, float layerHeight)
    {
        int oldLayer = 0;
        int index = 0;
        for (int i = 0; i < posAndRot.Count; i++)
        {
            int layer = Mathf.FloorToInt(posAndRot[i].pos.y / layerHeight);
            CreateObjectAtPosition(prefab, posAndRot[i].pos, posAndRot[i].rot, layer, index);
            if (layer > oldLayer)
            {
                index = 0;
                oldLayer = layer;
                continue;
            }
            index++;
        }
    }

    protected void CreateObjectAtPosition(GameObject prefab, Vector3 localPosition, Quaternion localRotation, int layer, int index)
    {
        Vector3 position = transform.position + localPosition;
        GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
        newObject.transform.SetPositionAndRotation(position, localRotation);
        objects.Add(newObject.transform);
        newObject.name = $"{prefab.name}_({layer},{index})";
    }

    protected void ClearPreviousObjects()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            DestroyImmediate(child.gameObject);
        }
        objects.Clear();
    }

    [TriInspector.Button]
    private void Generate()
    {
        Clear();

        if (!CanGenerate())
            return;

        GenerateImpl();
    }
       

    [TriInspector.Button]
    protected void Clear()
    {
        ClearPreviousObjects();
    }

    protected abstract void GenerateImpl();
    protected abstract bool CanGenerate();
#endif
}
