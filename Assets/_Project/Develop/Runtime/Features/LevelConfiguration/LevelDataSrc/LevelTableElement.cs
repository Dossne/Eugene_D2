using UnityEngine;

namespace Features.LevelConfiguration
{
    public class LevelTableElement : MonoBehaviour
    {
        [field : SerializeField] public LevelTableElementType ElementType {  get; private set; }
        [SerializeField] private MeshRenderer meshRenderer;


        public void SetMaterial(Material material)
        {
            if (meshRenderer !=  null)
                meshRenderer.material = material;
        }

        private void OnValidate()
        {
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();
        }
    }
}