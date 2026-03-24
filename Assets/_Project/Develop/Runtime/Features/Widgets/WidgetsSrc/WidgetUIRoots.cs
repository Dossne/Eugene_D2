using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Features.Widgets
{
    public class WidgetUIRoots : MonoBehaviour
    {
        [SerializeField] private SerializedDictionary<RootSide, WidgetRoot> roots;

        public void Initialize()
        {
            foreach (var entryPair in roots)
            {
                entryPair.Value.Initialize();
            }
        }

        
        public void Deinitialize()
        {
            foreach (var entryPair in roots)
            {
                entryPair.Value.Deinitialize();
            }
        }
        
        
        public bool TryGetRoot(RootSide rootSide, out WidgetRoot root)
        {
            return roots.TryGetValue(rootSide, out root);
        }
    }
}