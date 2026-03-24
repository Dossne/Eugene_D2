using System.Collections.Generic;
using Features.Collectables;
using UnityEditor;
using UnityEngine;

public class CollectableItemReplacer : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private CollectableItem originalObject;
    [SerializeField] private CollectableItem newObject;

    private List<ObjectData> cachedData = new ();

    [TriInspector.Button]
    private void Replace()
    {
        if (originalObject == null)
        {
            Debug.LogError("OriginalObject не может быть null");
            return;
        }
        if (newObject == null)
        {
            Debug.LogError("NewObject не может быть null");
            return;
        }
        
        cachedData.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            var item = child.GetComponent<CollectableItem>();
            if (item == null || originalObject.CollectableType != item.CollectableType) continue;
            cachedData.Add(new ObjectData(child.position, child.rotation));
            DestroyImmediate(child.gameObject);
        }

        for (var index = 0; index < cachedData.Count; index++)
        {
            var objectData = cachedData[index];
            var collectable = (CollectableItem)PrefabUtility.InstantiatePrefab(newObject, transform);
            collectable.transform.SetPositionAndRotation(objectData.position, objectData.rotation);
            collectable.name = newObject.name + "_" + index;
        }
    }


    private class ObjectData
    {
        public Vector3 position;
        public Quaternion rotation;

        public ObjectData(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }
#endif
}
