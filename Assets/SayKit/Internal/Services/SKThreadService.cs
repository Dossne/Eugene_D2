using System;
using System.Threading;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    public class SKThreadService
    {
        private static SKThreadService _instance;
        private readonly SynchronizationContext _synchronizationContext;
        private const string LogTag = "[SKThreadService]";
        private static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == 1;

        private SKThreadService()
        {
            _synchronizationContext = SynchronizationContext.Current;
        }

        public static SKThreadService Instance { get; } = new SKThreadService();

        public void RunOnMainThread(Action action)
        {
            try
            {
                if (IsMainThread)
                {
                    action?.Invoke();
                }
                else
                {
                    _synchronizationContext.Post(_ =>
                    {
                        try
                        {
                            action?.Invoke();
                        }
                        catch (Exception e)
                        {
                            SKUtils.HandleError($"[SKThreadServiceError] RunOnMainThread error: {e.Message}, stacktrace: {e.StackTrace}");
                        }
                    }, null);
                }
            }
            catch (Exception e)
            {
                SKUtils.HandleError($"[SKThreadServiceError] RunOnMainThread error: {e.Message}, stacktrace: {e.StackTrace}");
            }
        }

        public void RunOnMainThread(Action action, Action<Exception> onError, Action onComplete = null)
        {
            if (IsMainThread)
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    onError?.Invoke(e);
                }
                finally
                {
                    onComplete?.Invoke();
                }
            }
            else
            {
                _synchronizationContext.Post(_ =>
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke(e);
                    }
                    finally
                    {
                        onComplete?.Invoke();
                    }
                }, null);
            }
        }

        public T RunOnMainThreadWithResult<T>(Func<T> action)
        {
            var result = default(T);

            try
            {
                if (IsMainThread)
                {
                    return action.Invoke();
                }

                _synchronizationContext.Send(_ => { result = action.Invoke(); }, null);
            }
            catch (Exception e)
            {
                SKUtils.HandleError($"{LogTag}[RunOnMainThreadWithResult] exception: {e}");
            }

            return result;
        }
    }
}