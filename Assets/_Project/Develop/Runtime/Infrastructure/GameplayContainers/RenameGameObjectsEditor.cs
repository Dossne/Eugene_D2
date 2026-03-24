using UnityEngine;

namespace Infrastructure.GameplayContainers
{
    public class RenameGameObjectsEditor : MonoBehaviour, IRenameComponentEditor
    {
#if UNITY_EDITOR

        [SerializeField] private bool autoRename = true;
        [SerializeField] private string prefix;


        private void OnValidate()
        {
            Rename();
        }


        void IRenameComponentEditor.RenameChildren()
        {
            Rename();
        }


        private void Rename()
        {
            if (!autoRename || Application.isPlaying)
            {
                return;
            }

            int i = 1;

            foreach (Transform child in transform)
            {
                if (child.TryGetComponent(out IContainerItem item))
                {
                    string details = string.IsNullOrEmpty(item.ObjectNameDetails) ? "" : $"{item.ObjectNameDetails}";

                    child.gameObject.name = $"{prefix}{details}{i}";
                    i++;
                }
            }
        }


#endif
    }
}