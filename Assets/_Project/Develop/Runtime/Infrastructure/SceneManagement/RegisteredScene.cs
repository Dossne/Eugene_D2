using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Infrastructure.SceneManagement
{
    public class RegisteredScene
    {
        private AsyncOperationHandle<SceneInstance>? handle;
        public bool IsLoaded { get; private set; }
        public LoadSceneMode LoadMode { get; private set; }


        public void SetAsLoaded(AsyncOperationHandle<SceneInstance>? handle, LoadSceneMode loadSceneMode)
        {
            this.handle = handle;
            LoadMode = loadSceneMode;
            IsLoaded = true;
        }


        public async UniTask UnloadAsync(CancellationToken cancellationToken)
        {
            IsLoaded = false;

            if (!handle.HasValue || !handle.Value.IsValid())
            {
                return;
            }

            /* not necessary for single, unity will auto release it
             if (LoadMode == LoadSceneMode.Single)
                Addressables.Release(handle.Value);
            */ 
            
            if(LoadMode == LoadSceneMode.Additive)
                await Addressables.UnloadSceneAsync(handle.Value).ToUniTask(cancellationToken: cancellationToken);

            handle = null;
        }


        public Scene GetScene()
        {
            return handle.Value.Result.Scene;
        }
    }
}