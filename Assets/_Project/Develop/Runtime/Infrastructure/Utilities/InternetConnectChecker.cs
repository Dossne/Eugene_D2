using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Infrastructure.Utilities
{
    //Warning. Allocates each CheckUrlAsync call approx 10-11 KB.
    //Do not use in hot path
    public static class InternetConnectChecker
    {
        private const int TimeoutMs = 3000;
        private static string Google204Url = "https://clients3.google.com/generate_204";
        private static bool isBusy;

        public static UniTask<bool> Have(CancellationToken token)
        {
            return Have(Google204Url, TimeoutMs, token);
        }

        public static UniTask<bool> Have(string checkUrl, int timeoutMs, CancellationToken token)
        {
            if (!IsInternetReachable())
            {
#if PR_CHEAT || UNITY_EDITOR
                Debug.LogWarning("[InternetConnectChecker] NetworkReachability is NotReachable");
#endif
                return UniTask.FromResult(false);
            }

            return CheckUrlAsync(checkUrl, timeoutMs, token);
        }

        private static async UniTask<bool> CheckUrlAsync(string url, int timeoutMs, CancellationToken token)
        {
            if (isBusy)
                await UniTask.WaitUntil(IsReady, cancellationToken: token);

            //Debug.LogWarning("[InternetConnectChecker] Start");

            isBusy = true;
            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCts.Token);

            try
            {
                using var request = UnityWebRequest.Head(url);

                await request.SendWebRequest().ToUniTask(cancellationToken: linkedCts.Token);

#if UNITY_2020_2_OR_NEWER
                if (request.result != UnityWebRequest.Result.Success)
                {
                    return false;
                }
#endif

                return request.responseCode is >= 200 and < 400;
            }
            catch (Exception e)
            {
#if PR_CHEAT || UNITY_EDITOR
                Debug.LogWarning($"[InternetConnectChecker] Exception: {e}");
#endif
                return false;
            }
            finally
            {
                //Debug.LogWarning($"[InternetConnectChecker] Complete");
                isBusy = false;
            }
        }

        private static bool IsReady()
        {
            return !isBusy;
        }

        private static bool IsInternetReachable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
}