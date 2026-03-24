using System;
using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace

#endregion

namespace SayKitInternal
{
    public class LiveRequestManager
    {
        public static LiveRequestManager Instance { get; } = new LiveRequestManager();
        
        public void RequestPayerPrediction(Action<LiveRequestPayerPrediction> onRequestResult)
        {
            if (SKManager.Instance.RemoteConfig.runtime.disable_live_request == 1)
            {
                var liveRequest = new LiveRequestPayerPrediction
                {
                    Error = "SayKit: Live request is disabled."
                };
                onRequestResult?.Invoke(liveRequest);

                return;
            }

            if (onRequestResult == null)
            {
                SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception",
                    extra1: "[RequestLiveServer] onRequestResult action is null",
                    extra2: "RequestPayerPrediction");
            }
            else
            {
                SKBridgeManager.Instance.RequestLiveServer("payer_prediction", null, requestResult =>
                {
                    if (string.IsNullOrEmpty(requestResult))
                    {
                        var liveRequest = new LiveRequestPayerPrediction
                        {
                            Result = false,
                            Error = "SayKit internal error."
                        };

                        onRequestResult.Invoke(liveRequest);
                    }
                    else
                    {
                        try
                        {
                            var result = JsonConvert.DeserializeObject<LiveRequestPayerPrediction>(requestResult);
                            onRequestResult.Invoke(result);
                        }
                        catch (Exception exc)
                        {
                            SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception",
                                extra1: "[RequestPayerPrediction] requestResult exception: " + exc.Message,
                                extra2: requestResult);
                        }
                    }
                });
            }
        }

        public void RequestLiveServer(string requestName, string requestData, Action<string> onRequestResult)
        {
            if (SKManager.Instance.RemoteConfig.runtime.disable_live_request == 1)
            {
                onRequestResult?.Invoke(string.Empty);
                return;
            }
            
            if (string.IsNullOrEmpty(requestName))
            {
                SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception", extra1: "[RequestLiveServer] RequestName param is null or empty");
            }
            else
            {
                if (onRequestResult == null)
                {
                    SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception", extra1: "[RequestLiveServer] onRequestResult action is null",
                        extra2: requestName);
                }
                else
                {
                    SKBridgeManager.Instance.RequestLiveServer(requestName, requestData, onRequestResult);
                }
            }
        }
        
    }
}
