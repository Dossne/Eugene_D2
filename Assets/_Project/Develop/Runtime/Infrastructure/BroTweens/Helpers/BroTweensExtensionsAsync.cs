using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Infrastructure.BroTweens
{
    public enum BroTweenCancelBehaviour
    {
        Kill,                 //Complete timer and return to pool
        KillWithCallback,     //Complete timer, return to pool and invoke OnComplete callback
        Complete,             //Complete timer
        CompleteWithCallback, //Complete timer and invoke OnComplete callback
    }

    public static class BroTweensExtensionsAsync
    {
        public static TweenAwaiter GetAwaiter(this BroTweenBase tween)
        {
            return new TweenAwaiter(tween);
        }


        public static TweenAwaiter GetAwaiter(this BroTweenSafe tween)
        {
            return new TweenAwaiter(tween.GetReference_Internal());
        }


        public static UniTask ToUniTask(this BroTweenBase tween, BroTweenCancelBehaviour cancelBehaviour = BroTweenCancelBehaviour.Kill, CancellationToken cancellationToken = default)
        {
            if (!tween.IsPlaying())
                return UniTask.CompletedTask;
            
            return new UniTask(BroTweenUniTask.Create(tween, cancelBehaviour, cancellationToken, out var token), token);
        }


        public static UniTask ToUniTask(this BroTweenSafe tween, BroTweenCancelBehaviour cancelBehaviour = BroTweenCancelBehaviour.Kill, CancellationToken cancellationToken = default)
        {
            if (!tween.IsPlaying())
                return UniTask.CompletedTask;
            
            return new UniTask(BroTweenUniTask.Create(tween.GetReference_Internal(), cancelBehaviour, cancellationToken, out var token), token);
        }


        sealed class BroTweenUniTask : IUniTaskSource, ITaskPoolNode<BroTweenUniTask>
        {
            public ref BroTweenUniTask NextNode => ref nextNode;
            private BroTweenUniTask nextNode;

            private static TaskPool<BroTweenUniTask> pool;

            private BroTweenBase tween;
            private CancellationTokenRegistration cancellationRegistration;
            private CancellationToken cancellationToken;
            private UniTaskCompletionSourceCore<AsyncUnit> core;
            private readonly Action onCompleteCallbackDelegate;
            private int tweenGeneration;
            private int tweenPlayVersion;
            private BroTweenCancelBehaviour cancelBehaviour;
            private bool completed;
            private bool resultConsumed;


            static BroTweenUniTask()
            {
                TaskPool.RegisterSizeGetter(typeof(BroTweenUniTask), () => pool.Size);
            }


            private BroTweenUniTask()
            {
                onCompleteCallbackDelegate = OnCompleteCallbackDelegate;
            }


            /// <summary>
            /// Create() assumes tween != null.
            /// Synchronous completion must be handled before calling Create.
            /// </summary>
            public static IUniTaskSource Create(BroTweenBase tween, BroTweenCancelBehaviour cancelBehaviour, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    HandleTween(tween, cancelBehaviour);
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                    result = new BroTweenUniTask();

                result.tween = tween;
                result.tweenGeneration = tween.Generation;
                result.tweenPlayVersion = tween.PlayVersion;

                result.cancelBehaviour = cancelBehaviour;
                result.cancellationToken = cancellationToken;

#if PR_CHEAT || UNITY_EDITOR
                CreateDebug(tween.UniqueId, tween.Generation);
#endif

                tween.SetToUniTask_Internal(result.onCompleteCallbackDelegate);

                if (cancellationToken.CanBeCanceled)
                {
                    result.cancellationRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(state =>
                    {
                        var s = (BroTweenUniTask)state;

                        if (s.completed)
                            return;

                        if (!s.IsValid())
                        {
                            s.OnCompleteCallbackDelegate();
                            return;
                        }

                        HandleTween(s.tween, s.cancelBehaviour);

                    }, result);
                }

                // double-check
                if (!result.IsValid() || !tween.IsPlaying())
                {
                    result.OnCompleteCallbackDelegate();
                }

                TaskTracker.TrackActiveTask(result, 3);

                token = result.core.Version;
                return result;
            }


            private void OnCompleteCallbackDelegate()
            {
                if (completed)
                    return;

                completed = true;

                if (!IsValid())
                {

#if PR_CHEAT || UNITY_EDITOR
                    NotValidDebug(tween.UniqueId, tween.Generation, tweenGeneration);
#endif
                    core.TrySetResult(AsyncUnit.Default);

                }
                else if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                }
                else
                {
                    core.TrySetResult(AsyncUnit.Default);
                }

                TryReturn();
            }


            private static void HandleTween(BroTweenBase tween, BroTweenCancelBehaviour behaviour)
            {
                switch (behaviour)
                {
                    case BroTweenCancelBehaviour.Kill:
                        tween.Kill_Internal();
                        break;
                    case BroTweenCancelBehaviour.KillWithCallback:
                        tween.Kill_Internal(true);
                        break;
                    case BroTweenCancelBehaviour.Complete:
                        tween.Complete();
                        break;
                    case BroTweenCancelBehaviour.CompleteWithCallback:
                        tween.Complete(true);
                        break;
                }

#if PR_CHEAT || UNITY_EDITOR
                HandleTweenDebug(tween, behaviour);
#endif
            }


            public void GetResult(short token)
            {
                try
                {
                    core.GetResult(token);
                }
                catch (OperationCanceledException e)
                {
                    //
#if PR_CHEAT || UNITY_EDITOR
                    OperationCancelDebug(e);
#endif
                }
                finally
                {
                    resultConsumed = true;
                    TryReturn();
                }
            }


            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }


            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }


            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }


            private void TryReturn()
            {
                if (!completed || !resultConsumed)
                    return;
                
                Return();
            }


            private void Return()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                cancellationRegistration.Dispose();
                cancellationToken = CancellationToken.None;
                completed = false;
                resultConsumed = false;

                if (IsValid())
                    tween.SetToUniTask_Internal(null);

#if PR_CHEAT || UNITY_EDITOR
                ReturnDebug();
#endif
                tween = null;
                tweenGeneration = -1;
                pool.TryPush(this);
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool IsValid()
            {
                return tween != null && tween.Generation == tweenGeneration && tween.PlayVersion == tweenPlayVersion;
            }


#if PR_CHEAT || UNITY_EDITOR

            private static bool isDebug;

            private static void CreateDebug(int tweenId, int tweenGen)
            {
                if (isDebug)
                    UnityEngine.Debug.Log($"[BroTween] Created. TweenId {tweenId}. TweenGen: {tweenGen}. Frame: {UnityEngine.Time.frameCount}");
            }


            private void OperationCancelDebug(OperationCanceledException e)
            {
                if (isDebug)
                    UnityEngine.Debug.LogWarning($"[BroTween] Error: {e}");
            }


            private void ReturnDebug()
            {
                if (isDebug)
                    UnityEngine.Debug.Log($"[BroTween] UniTask return. TweenId: {tween.UniqueId}. TweenGen: {tween.Generation}. CachedGen: {tweenGeneration}. Frame: {UnityEngine.Time.frameCount}");
            }


            private static void NotValidDebug(int tweenId, int tweenGen, int cachedGen)
            {
                if (isDebug)
                    UnityEngine.Debug.Log($"[BroTween] Not valid. TweenId {tweenId}. TweenGen: {tweenGen}. CachedGen: {cachedGen}. Frame: {UnityEngine.Time.frameCount}");
            }


            private static void HandleTweenDebug(BroTweenBase tween, BroTweenCancelBehaviour behaviour)
            {
                if (isDebug)
                    UnityEngine.Debug.Log($"[BroTween] UniTask: {behaviour}. TweenId {tween.UniqueId}. Gen: {tween.Generation}. Frame: {UnityEngine.Time.frameCount}");
            }


            private static void CompleteAllDebug()
            {
                if (isDebug)
                    UnityEngine.Debug.Log($"[BroTween] OnCompleteCallbackDelegate Frame: {UnityEngine.Time.frameCount}");
            }
#endif
        }


        public struct TweenAwaiter : ICriticalNotifyCompletion
        {
            readonly BroTweenBase tween;

            public bool IsCompleted => !tween.IsPlaying();


            public TweenAwaiter(BroTweenBase tween)
            {
                this.tween = tween;
            }


            public TweenAwaiter GetAwaiter() => this;


            public void GetResult()
            {
            }


            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }


            public void UnsafeOnCompleted(Action continuation)
            {
                tween.SetToUniTask_Internal(PooledTweenCallback.Create(continuation));
            }
        }


        sealed class PooledTweenCallback
        {
            static readonly ConcurrentQueue<PooledTweenCallback> pool = new();

            readonly Action runDelegate;

            Action continuation;


            PooledTweenCallback()
            {
                runDelegate = Run;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Action Create(Action continuation)
            {
                if (!pool.TryDequeue(out var item))
                {
                    item = new PooledTweenCallback();
                }

                item.continuation = continuation;
                return item.runDelegate;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void Run()
            {
                var call = continuation;
                continuation = null;
                pool.Enqueue(this);
                call?.Invoke();
            }
        }
    }
}