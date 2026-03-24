using Features.Collectables;
using UnityEditor;

namespace SearchableEnum
{
    [CustomPropertyDrawer(typeof(CollectableType))]
    public class SearchableCollectableType : SearchableEnumDrawer
    {
        
    }
}