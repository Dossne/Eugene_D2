using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Infrastructure.Utilities
{
    public class ObjectReplacer : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private GameObject newObject;

        private List<ObjectData> cachedData = new();

        [TriInspector.Button]
        private void Replace()
        {
            if (newObject == null)
            {
                Debug.LogError("NewObject не может быть null");
                return;
            }

            cachedData.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                cachedData.Add(new ObjectData(child.position, child.rotation, child.localScale));
                DestroyImmediate(child.gameObject);
            }

            for (var index = 0; index < cachedData.Count; index++)
            {
                var objectData = cachedData[index];
                var gameObject = (GameObject)PrefabUtility.InstantiatePrefab(newObject, transform);
                gameObject.transform.SetPositionAndRotation(objectData.position, objectData.rotation);
                gameObject.transform.localScale = objectData.scale;
                gameObject.name = newObject.name + "_" + index;
            }
        }


        private class ObjectData
        {
            public Vector3 position;
            public Vector3 scale;
            public Quaternion rotation;

            public ObjectData(Vector3 position, Quaternion rotation, Vector3 scale)
            {
                this.position = position;
                this.scale = scale;
                this.rotation = rotation;
            }
        }
#endif
    }
}