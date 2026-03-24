using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Infrastructure.AssetManagement
{
    /// <summary>
    /// Use when project have remote addressables
    /// </summary>
    public class AddressableAssetDownloadServiceLabeled
    {
        public const string RemoteLabel = "remote";

        private readonly AssetDownloadReporter downloadReporter;
        private long downloadSize;


        public AddressableAssetDownloadServiceLabeled(AssetDownloadReporter downloadReporter)
        {
            this.downloadReporter = downloadReporter;
        }


        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            await Addressables.InitializeAsync().ToUniTask(cancellationToken: cancellationToken);
            await UpdateCatalogsAsync(cancellationToken);
            await UpdateDownloadSizeAsync(cancellationToken);

            float size = GetDownloadSizeMb();

            if (size > 0)
            {

#if UNITY_EDITOR
                Debug.Log($"ADDRESSABLES DOWNLOAD SIZE IS {size} Mb");
#endif
                await UpdateContentAsync(cancellationToken);
            }
        }



        private float GetDownloadSizeMb() =>
            SizeToMb(downloadSize);


        public async UniTask UpdateContentAsync(CancellationToken cancellationToken)
        {
            try
            {
                AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(RemoteLabel);

                while (!downloadHandle.IsDone && downloadHandle.IsValid())
                {
                    await UniTask.Delay(100, cancellationToken: cancellationToken);
                    downloadReporter.Report(downloadHandle.GetDownloadStatus().Percent);
                }

                downloadReporter.Report(1);

                if (downloadHandle.Status == AsyncOperationStatus.Failed)
                    Debug.LogError("Error while downloading catalog dependencies");

                if (downloadHandle.IsValid())
                    Addressables.Release(downloadHandle);

                downloadReporter.Reset();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }


        private async UniTask UpdateCatalogsAsync(CancellationToken cancellationToken)
        {
            List<string> catalogsToUpdate = await Addressables.CheckForCatalogUpdates().ToUniTask(cancellationToken: cancellationToken);
            if (catalogsToUpdate.IsNullOrEmpty())
                return;
            
            await Addressables.UpdateCatalogs(catalogsToUpdate).ToUniTask(cancellationToken: cancellationToken);
        }


        private async UniTask UpdateDownloadSizeAsync(CancellationToken cancellationToken)
        {
            downloadSize = await Addressables
                                 .GetDownloadSizeAsync(RemoteLabel)
                                 .ToUniTask(cancellationToken: cancellationToken);
        }


        private static float SizeToMb(long downloadSize) => downloadSize * 1f / 1048576;

    }
}