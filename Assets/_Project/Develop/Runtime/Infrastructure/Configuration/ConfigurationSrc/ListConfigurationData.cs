using System;
using UnityEngine;

namespace Infrastructure.Configuration
{
    [Serializable]
    public class ListConfigurationData<K>
    {
        public K id;
        public int sort;
        [HideInInspector] [SerializeField] protected string name = "unnamed";

        
#if UNITY_EDITOR
        public void SetName_Editor(string name)
        {
            this.name = name;
        }

        public string GetName_Editor()
        {
            return name;
        }
#endif

    }
}