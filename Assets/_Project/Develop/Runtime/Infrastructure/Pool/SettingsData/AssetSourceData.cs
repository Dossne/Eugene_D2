using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Infrastructure.Pool
{
    public enum AssetSourceType
    {
        None = 0,
        AddressableByPath = 1,
        AddressableByRef = 2,
        ResourcesByPath = 3,
        GameObjectPf = 4,
    }

    [Serializable]
    public class AssetSourceData
    {
#if UNITY_EDITOR
        [TriInspector.ValidateInput(nameof(ValidateAssetSourceType))]
#endif
        public AssetSourceType assetSourceType;

        public string addressablePath;

        public AssetReferenceGameObject addressableRef;

        public string resourceFullPath;
#if UNITY_EDITOR
        [TriInspector.ValidateInput(nameof(ValidateGameObject))]
#endif

        public GameObject gameObjectPrefab;

#if UNITY_EDITOR


        public TriInspector.TriValidationResult ValidateGameObject()
        {
            if (gameObjectPrefab == null)
            {
                return TriInspector.TriValidationResult.Valid;
            }

            return TriInspector.TriValidationResult.Info("Object prefab will be placed in memory as soon as pool creates. Always check RAM limits");
        }


        public TriInspector.TriValidationResult ValidateAssetSourceType()
        {
            // CleanUpOtherFields();

            if (assetSourceType == AssetSourceType.None)
            {
                return TriInspector.TriValidationResult.Error($"\"{nameof(assetSourceType)}\" cannot be {nameof(AssetSourceType.None)}\"");
            }
            
            if (IsAddressablePathSetIncorrect())
            {
                return TriInspector.TriValidationResult.Error($"Field {nameof(addressablePath)} is empty");
            }

            if (IsAddressableRefSetIncorrect())
            {
                return TriInspector.TriValidationResult.Error($"Field {nameof(addressableRef)} is empty");
            }

            if (IsResourcePathSetIncorrect())
            {
                return TriInspector.TriValidationResult.Error($"Field {nameof(resourceFullPath)} is empty");
            }

            if (IsGameObjectPrefabSetIncorrect())
            {
                return TriInspector.TriValidationResult.Error($"Field {nameof(gameObjectPrefab)} is empty");
            }

            return TriInspector.TriValidationResult.Valid;

        }


        private void CleanUpOtherFields()
        {
            switch (assetSourceType)
            {
                case AssetSourceType.None:
                    addressablePath = null;
                    addressableRef = null;
                    resourceFullPath = null;
                    gameObjectPrefab = null;
                    break;

                case AssetSourceType.AddressableByPath:
                    addressableRef = null;
                    resourceFullPath = null;
                    gameObjectPrefab = null;
                    break;
                case AssetSourceType.AddressableByRef:
                    addressablePath = null;
                    resourceFullPath = null;
                    gameObjectPrefab = null;
                    break;
                case AssetSourceType.ResourcesByPath:
                    addressablePath = null;
                    addressableRef = null;
                    gameObjectPrefab = null;
                    break;
                case AssetSourceType.GameObjectPf:
                    addressablePath = null;
                    addressableRef = null;
                    resourceFullPath = null;
                    break;

            }
        }


        private bool IsAddressablePathSetIncorrect()
        {
            return assetSourceType == AssetSourceType.AddressableByPath && string.IsNullOrEmpty(addressablePath);
        }


        private bool IsAddressableRefSetIncorrect()
        {
            return assetSourceType == AssetSourceType.AddressableByRef && string.IsNullOrEmpty(addressableRef.AssetGUID);
        }


        private bool IsResourcePathSetIncorrect()
        {
            return assetSourceType == AssetSourceType.ResourcesByPath && string.IsNullOrEmpty(resourceFullPath);
        }


        private bool IsGameObjectPrefabSetIncorrect()
        {
            return assetSourceType == AssetSourceType.GameObjectPf && gameObjectPrefab == null;
        }
#endif
    }
}