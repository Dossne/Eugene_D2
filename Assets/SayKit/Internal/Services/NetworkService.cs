#region ReSharper

// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable RedundantUsingDirective

#endregion

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SayKitInternal
{
    public class NetworkService
    {
        public static NetworkReachability InternetReachability { get; set; } = NetworkReachability.NotReachable;

        public static int GetInternetReachabilityState()
        {
            return InternetReachability != NetworkReachability.NotReachable ? 1 : 0;
        }

        private static HttpClientHandler GetClientHandler()
        {
#if SAYKIT_DEBUG && !UNITY_EDITOR
            var proxyAddress = SKBridgeManager.Instance.GetSystemProxy();
            if (!string.IsNullOrEmpty(proxyAddress))
            {
                return new HttpClientHandler
                {
                    Proxy = new WebProxy(proxyAddress),
                    UseProxy = true,
                };
            }

            return new HttpClientHandler();
#endif
            return new HttpClientHandler();
        }

        public static async Task PatchAsync(string url, string username, string password,
            Action<string, string> responseCallback = null)
        {
            try
            {
                using (var client = new HttpClient(GetClientHandler()))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                            Encoding.UTF8.GetBytes($"{username}:{password}")));

                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                    request.Content = new StringContent("{\"key\": \"value\"}", Encoding.UTF8, "application/json");
                    client.Timeout = TimeSpan.FromSeconds(5);

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        responseCallback?.Invoke(result, string.Empty);
                    }
                    else
                    {
                        responseCallback?.Invoke(string.Empty,
                            $"Request failed. Error status code: {response.StatusCode}.");
                    }
                }
            }
            catch (Exception e)
            {
                responseCallback?.Invoke(string.Empty, $"NetworkService PatchAsync method exception: {e.Message}.");
                SayKitDebug.LogError($"[NetworkService] PatchAsync message: + {e.Message}, stacktrace: {e.StackTrace}, url: {url}");
            }
        }

        public static (string responseData, string errorMessage) PostWithHeaders(string url, string body, int timeout)
        {
            try
            {
                using (var httpClient = new HttpClient(GetClientHandler()))
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    var data = new StringContent(body, Encoding.UTF8, "application/json");
                    var response = httpClient.PostAsync(url, data).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        return (responseData, string.Empty);
                    }

                    return (string.Empty, $"Request failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"[NetworkService] PostWithHeaders message: + {e.Message}, stacktrace: {e.StackTrace}, url: {url}");
                return (string.Empty, $"Request failed: {e.Message}");
            }
        }

        public static async Task Post(string url, int timeout, Dictionary<string, string> form,
            Action<int, string> responseCallback)
        {
            try
            {
                using (var httpClient = new HttpClient(GetClientHandler()))
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    var httpResponseMessage = await httpClient.PostAsync(url, new FormUrlEncodedContent(form))
                        .ConfigureAwait(continueOnCapturedContext: false);

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        responseCallback?.Invoke((int)httpResponseMessage.StatusCode, string.Empty);
                    }
                    else
                    {
                        responseCallback?.Invoke(0, $"Request failed. Error status code: {httpResponseMessage.StatusCode}.");
                    }
                }
            }
            catch (Exception e)
            {
                responseCallback?.Invoke(0, $"NetworkService Post method exception: {e.Message}.");
                SayKitDebug.LogError($"[NetworkService] Post message: + {e.Message}, stacktrace: {e.StackTrace}, url: {url}");
            }
        }

        public static async Task Post(string url, int timeout, string json, Action<string, string> responseCallback)
        {
            try
            {
                using (var httpClient = new HttpClient(GetClientHandler()))
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                    var httpResponseMessage = await httpClient.PostAsync(url, data).ConfigureAwait(continueOnCapturedContext: false);

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        var result = await httpResponseMessage.Content.ReadAsStringAsync();
                        responseCallback?.Invoke(result, string.Empty);
                    }
                    else
                    {
                        responseCallback?.Invoke(string.Empty, $"Request failed. Error status code: {(int)httpResponseMessage.StatusCode}.");
                    }
                }
            }
            catch (Exception e)
            {
                responseCallback?.Invoke(string.Empty, $"NetworkService Post method exception: {e.Message}.");
                SayKitDebug.LogError($"[NetworkService] Post message: + {e.Message}, stacktrace: {e.StackTrace}, url: {url}");
            }
        }

        public static async Task GetAsString(string url, int timeout, Action<string, string> responseCallback)
        {
            try
            {
                using (var httpClient = new HttpClient(GetClientHandler()))
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    var httpResponseMessage = await httpClient.GetAsync(url).ConfigureAwait(continueOnCapturedContext: false);

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        var result = await httpResponseMessage.Content.ReadAsStringAsync();
                        responseCallback?.Invoke(result, string.Empty);
                    }
                    else
                    {
                        responseCallback?.Invoke(string.Empty, $"Request failed. Error status code: {(int)httpResponseMessage.StatusCode}.");
                    }
                }
            }
            catch (Exception e)
            {
                responseCallback?.Invoke(string.Empty, $"NetworkService GetAsString method exception: {e.Message}.");
                SayKitDebug.LogError($"[NetworkService] GetAsString message: + {e.Message}, stacktrace: {e.StackTrace}, url: {url}");
            }
        }

        public static async Task GetAsByteArray(string url, int timeout, Action<byte[], string> responseCallback)
        {
            try
            {
                using (var httpClient = new HttpClient(GetClientHandler()))
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    var httpResponseMessage = await httpClient.GetAsync(url).ConfigureAwait(continueOnCapturedContext: false);

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        var data = await httpResponseMessage.Content.ReadAsByteArrayAsync();
                        responseCallback?.Invoke(data, String.Empty);
                    }
                    else
                    {
                        responseCallback?.Invoke(null, $"Request failed. Error status code: {(int)httpResponseMessage.StatusCode}.");
                    }
                }
            }
            catch (Exception e)
            {
                responseCallback?.Invoke(null, $"NetworkService GetAsByteArray method exception: {e.Message}.");
                SayKitDebug.LogError($"[NetworkService] GetAsByteArray message: + {e.Message}, stacktrace: {e.StackTrace}, url: {url}");
            }
        }
    }
}