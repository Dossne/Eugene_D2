using System;
using TriInspector;
using UnityEngine;

namespace Infrastructure.Pool
{
    public enum ItemInjectType
    {
        None = 0,
        OnCreate = 1, //only once on create
        OnEachGet = 2 //each time on get from pool
    }

    public enum LogType
    {
        None = 0,
        Log = 1,
        Warning = 2,
        Error = 3,
    }

    [Serializable]
    public class PoolSettings
    {
        [Tooltip("Inject dependencies from container in all components of item")] public ItemInjectType itemInject;
        [Tooltip("Count for creating on init"), Min(0)] public int preWarmCount;
        [Min(0)] public int maxPoolSize = 100;
        [Tooltip("Debug log action if activeItemsCount == maxPoolSize")] public LogType logOnMaxReach = LogType.Log;
        [Tooltip("Continue create new items if activeItemsCount == maxPoolSize")] public bool createNewOnMaxReach = false;
        [Tooltip("Reset item parent to pool transform on each release")] public bool resetParentOnRelease;

        [Tooltip("When you assign a parent Object, pass true to position the new object directly in world space. " +
                 "Pass false to set the Object’s position relative to its new parent." +
                 "Set false for parented ui elements, otherwise set true")] public bool itemWorldSpace = true;

        [Tooltip("Create active gameObject instances")] public bool instantiateActiveGo;
        [Tooltip("Create instances with async methods (only for addressable prefabs)")] public bool instantiateAsync = true;
    }

    [Serializable]
    public class PoolItemData<T>
    {
        public T poolItemKey;
        public AssetSourceData assetSourceData;
        public bool useSpecificSettings;
        [ShowIf("useSpecificSettings")] public PoolSettings specificSettings;
    }
}