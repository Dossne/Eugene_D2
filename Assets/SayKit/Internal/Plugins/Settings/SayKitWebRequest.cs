using System.Collections.Generic;
using System.Threading;
using UnityEngine.Networking;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    public class SayKitWebRequest
    {
        private const string DefaultText = "";
        private string Url { get; }
        public string Text { get; private set; } = DefaultText;
        public string ErrorMessage { get; private set; }
        public bool IsDone { get; private set; }
        public Dictionary<string, string> ResponseHeaders { get; private set; }
        
        public SayKitWebRequest(string url)
        {
            Url = url;
        }

        public bool SendAndWait(float timeout = -1)
        {
            IsDone = false;
            ErrorMessage = null;
            float timestamp = SKUtils.currentTimestamp;

            using (var client = UnityWebRequest.Get(Url))
            {
                var op = client.SendWebRequest();
                while (!op.isDone)
                {
                    if (timeout > 0f && timeout < SKUtils.currentTimestamp - timestamp)
                    {
                        ErrorMessage = "Timeout";
                        return false;
                    }

                    Thread.Sleep(100);
                }

                IsDone = true;
                ResponseHeaders = client.GetResponseHeaders();

                if (client.result == UnityWebRequest.Result.Success)
                {
                    Text = client.downloadHandler?.text ?? DefaultText;
                    return true;
                }

                ErrorMessage = client.error;
                return false;
            }
        }
    }
}