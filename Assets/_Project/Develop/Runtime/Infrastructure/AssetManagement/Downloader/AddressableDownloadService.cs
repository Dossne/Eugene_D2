using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Infrastructure.AssetManagement
{
    public class AddressableDownloadService
    {
        private readonly AssetDownloadReporter downloadReporter;

        private List<IResourceLocator> catalogsLocators;
        private long downloadSize;


        public AddressableDownloadService(AssetDownloadReporter downloadReporter)
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


        private async UniTask UpdateContentAsync(CancellationToken cancellationToken)
        {
            if (catalogsLocators == null)
                await UpdateCatalogsAsync(cancellationToken);

            IList<IResourceLocation> locations = await RefreshResourceLocations(catalogsLocators, cancellationToken);

            if (locations.IsNullOrEmpty())
                return;

            try
            {
                await DownloadContentWithPreciseProgress(locations, cancellationToken);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }


        private async UniTask DownloadContent(IList<IResourceLocation> locations, CancellationToken cancellationToken)
        {
            UniTask downloadTask = Addressables
                                   .DownloadDependenciesAsync(locations, autoReleaseHandle: true)
                                   .ToUniTask(progress: downloadReporter, cancellationToken: cancellationToken);

            await downloadTask;

            if (downloadTask.Status.IsFaulted())
                Debug.LogError("Error while downloading catalog dependencies");

            downloadReporter.Reset();
        }


        private async UniTask DownloadContentWithPreciseProgress(IList<IResourceLocation> locations, CancellationToken cancellationToken)
        {
            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(locations);

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


        private async UniTask UpdateCatalogsAsync(CancellationToken cancellationToken)
        {
            List<string> catalogsToUpdate = await Addressables.CheckForCatalogUpdates().ToUniTask(cancellationToken: cancellationToken);
            if (catalogsToUpdate.IsNullOrEmpty())
            {
                catalogsLocators = Addressables.ResourceLocators.ToList();
                return;
            }
            
            catalogsLocators = await Addressables.UpdateCatalogs(catalogsToUpdate).ToUniTask(cancellationToken: cancellationToken);
        }


        private async UniTask UpdateDownloadSizeAsync(CancellationToken cancellationToken)
        {
            IList<IResourceLocation> locations = await RefreshResourceLocations(catalogsLocators, cancellationToken);

            if (locations.IsNullOrEmpty())
                return;

            downloadSize = await Addressables
                                 .GetDownloadSizeAsync(locations)
                                 .ToUniTask(cancellationToken: cancellationToken);
        }


        private async UniTask<IList<IResourceLocation>> RefreshResourceLocations(IEnumerable<IResourceLocator> locators,
            CancellationToken cancellationToken)
        {
            IEnumerable<object> keysToCheck = locators.SelectMany(x => x.Keys);

            return await Addressables
                         .LoadResourceLocationsAsync(keysToCheck, Addressables.MergeMode.Union)
                         .ToUniTask(cancellationToken: cancellationToken);
        }


        private static float SizeToMb(long downloadSize) => downloadSize * 1f / 1048576;

    }
}