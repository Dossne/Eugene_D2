using System.Collections.Generic;
using Features.Collectables;
using Infrastructure.Utilities;
using Newtonsoft.Json;
using UnityEngine;

namespace Features.LevelConfiguration
{
    public class CollectableContainerToJson : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private bool compress;
        [SerializeField, TextArea(1, 10)] private string compressedJson;
        [SerializeField, TextArea(1, 10)] private string rawJson;
        
        
        [TriInspector.Button]
        public void GetJson()
        {
            CollectablesContainer container = GetComponentInChildren<CollectablesContainer>(true);
            
            container.Initialize();
            IReadOnlyList<CollectableItem> itemList = container.ObjectList;

            List<CollectableData> data = new List<CollectableData>();

            foreach (var item in itemList)
            {
                data.Add(item.GetData());
            }

            var json = JsonConvert.SerializeObject(data, JsonUtils.SerializerSettings);

            if(compress)
                compressedJson = StringUtils.CompressString(json);
            else
                rawJson = json;
        }
        
        
        [TriInspector.Button]
        public void DecompressJson()
        {
            rawJson = StringUtils.DecompressString(compressedJson);
        }
#endif
    }
}