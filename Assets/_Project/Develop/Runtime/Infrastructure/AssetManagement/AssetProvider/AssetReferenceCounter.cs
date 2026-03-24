using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Infrastructure.AssetManagement
{
    public class AssetReferenceCounter
    {
        private AsyncOperationHandle handle;
        private int refCount;


        public AssetReferenceCounter(AsyncOperationHandle handle)
        {
            this.handle = handle;
            refCount = 1;
        }


        public void IncrementCounter()
        {
            refCount++;
            Addressables.ResourceManager.Acquire(handle);
            //UnityEngine.Debug.Log($"IncrementCounter. Count: = {refCount}");
        }


        public void DecrementCounter()
        {
            if (refCount == 0)
            {
                return;
            }

            refCount--;
            Addressables.ResourceManager.Release(handle);
            //UnityEngine.Debug.Log($"DecrementCounter. Count: = {refCount}");
        }


        public void ReleaseAll()
        {
            int cachedCount = refCount;

            for (int i = 0; i < cachedCount; i++)
            {
                if (IsValid())
                {
                    DecrementCounter();
                }
                else
                {
                    refCount = 0;
                    break;
                }
            }
        }


        public bool IsValid()
        {
            return handle.IsValid();
        }


        public int GetRefCount()
        {
            return refCount;
        }


        public UniTask<T> GetResultAsync<T>(CancellationToken cancellationToken) where T : UnityEngine.Object
        {
            if (handle.IsDone)
            {
                return UniTask.FromResult((T)handle.Result);
            }

            return AwaitHandleAsync<T>(cancellationToken);
        }


        private async UniTask<T> AwaitHandleAsync<T>(CancellationToken ct) where T : UnityEngine.Object
        {
            await handle.ToUniTask(cancellationToken: ct);
            return (T)handle.Result;
        }
    }
}