using UnityEngine;

public class LevelTableMeshToBoxChanger : MonoBehaviour
{
#if UNITY_EDITOR
    [TriInspector.Button]
    private void ChangeMeshToBox()
    {
        MeshCollider[] mesh = GetComponentsInChildren<MeshCollider>(true);
        var before = mesh.Length;
        for (int i = 0; i < mesh.Length; i++)
        {
            var go = mesh[i].gameObject;
            DestroyImmediate(mesh[i]);

            var box = go.AddComponent<BoxCollider>();

            var size = box.size;
            size.y = 0.5f;
            box.size = size;

            var center = box.center;
            center.y = -0.25f;
            box.center = center;
        }

        MeshCollider[] meshAfter = GetComponentsInChildren<MeshCollider>(true);
        
        Debug.Log($"{gameObject.name}. Mesh colliders count. Before: {before}. After: {meshAfter.Length}");
    }

#endif
}