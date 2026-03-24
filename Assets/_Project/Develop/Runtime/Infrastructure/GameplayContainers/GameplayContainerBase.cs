using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Infrastructure.GameplayContainers
{
    public class GameplayContainerBase<T> : MonoBehaviour, IGameplayContainer where T : MonoBehaviour, IContainerItem
    {
        [SerializeField, Tooltip("Nullable. Set tag for multiple pools with same type")] private string containerTag;
        [SerializeField] private bool isActive = true;
        [SerializeField] private bool uniqueOnlyItems = true;
        [SerializeField] private bool includeInactiveItems;
        
#if UNITY_EDITOR
        [SerializeField, TriInspector.ReadOnly]
#endif
        private T[] objectList;

        private readonly Dictionary<string, T> objectsDictionary = new();

        public string Tag => containerTag;
        public bool IsActive => isActive;
        public IReadOnlyDictionary<string, T> ObjectsDictionary => objectsDictionary;
        public IReadOnlyList<T> ObjectList => objectList;


        private void Awake()
        {
            gameObject.SetActive(isActive);
        }


        public void Initialize()
        {
            objectList = null;
            objectsDictionary?.Clear();

            if (!isActive)
            {
                return;
            }

            InitList();
            InitDictionary();
        }


        public bool TryGetObject(string objectId, out T result)
        {
            return objectsDictionary.TryGetValue(objectId, out result);
        }


        public bool HasDuplicates()
        {

            HashSet<string> findDuplicatesHelper = new HashSet<string>();

            for (var i = 0; i < objectList.Length; i++)
            {
                T item = objectList[i];
                if (!findDuplicatesHelper.Add(item.ObjectId))
                {
                    return true;
                }
            }

            return false;
        }


        private void InitList()
        {
            objectList = GetComponentsInChildren<T>(includeInactiveItems);
        }


        private void InitDictionary()
        {
            if(!uniqueOnlyItems)
                return;
            
            objectsDictionary.Clear();

            for (var i = 0; i < objectList.Length; i++)
            {
                T item = objectList[i];
                if (string.IsNullOrEmpty(item.ObjectId))
                {
                    Debug.LogError("Object Id is null", item);
                    continue;
                }

                if (!objectsDictionary.TryAdd(item.ObjectId, item))
                {
                    Debug.LogError("Duplicate by Id of containerItem. Id = " + item.ObjectId, item);
                }
            }
        }


#if UNITY_EDITOR

        [TriInspector.Button]
        public void CollectAndRename()
        {
            Initialize();
            IRenameComponentEditor[] renameComponents = GetComponentsInChildren<IRenameComponentEditor>(true);

            foreach (IRenameComponentEditor item in renameComponents)
            {
                item.RenameChildren();
            }

            EditorUtility.SetDirty(this);
        }


#endif
    }
}