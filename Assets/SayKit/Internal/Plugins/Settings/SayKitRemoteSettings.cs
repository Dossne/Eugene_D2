#if UNITY_EDITOR

using System;
using Newtonsoft.Json;
using SayKitInternal;
using UnityEngine;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

#endregion

public class SayKitRemoteSettings
{
    public static SayKitRemoteSettings Instance { get; } = new SayKitRemoteSettings();

    public string FacebookAppID { get; private set; } = string.Empty;
    public string FacebookAppName { get; private set; } = string.Empty;
    public string FacebookClientToken { get; private set; } = string.Empty;
    public string AdmobAppId { get; private set; } = string.Empty;
    public string StoreId { get; private set; } = string.Empty;
    public string Firebase { get; private set; } = string.Empty;
    public bool HasError { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;

    private string GetRemoteURL()
    {
        var url = "https://api.launcher.saygames.io/saykit/configure?";

#if UNITY_IOS
        url += "app_key=" + SKUtils.GetAppKey();
        url += "&app_secret=" + SayKitApp.APP_SECRET_IOS;
        url += "&saykit_platform=" + "ios";
#elif UNITY_ANDROID
        url += "app_key=" + SKUtils.GetAppKey();
        url += "&app_secret=" + SayKitApp.APP_SECRET_ANDROID;
        url += "&saykit_platform=" + "android";
#endif

        url += "&app_version=" + Application.version;
        url += "&saykit=" + SKManager.Instance.Version;
        url += "&place=" + "default";
        url += "&device_id=" + SystemInfo.deviceUniqueIdentifier;

        return url;
    }

    public void Validate()
    {
        var request = new SayKitWebRequest(GetRemoteURL());
        request.SendAndWait(10);

        try
        {
            if (!request.IsDone)
            {
                HandleError("Request timeout", "Remote settings request timed out.");
                return;
            }

            if (!string.IsNullOrEmpty(request.ErrorMessage))
            {
                HandleError("Request error", request.ErrorMessage);
                return;
            }

            if (request.Text.Length == 0 || request.Text[0] != '{')
            {
                HandleError("Config data error", "Response data is empty or invalid json object.");
                return;
            }

            var config = JsonConvert.DeserializeObject<SayKitRemoteData>(request.Text);
            if (config == null || !string.IsNullOrEmpty(config.Error))
            {
                var errMsg = config == null ? "Failed to parse remote config." : config.Error;
                HandleError("Config error", errMsg);
                return;
            }

            foreach (var configuration in config.Configuration)
            {
                switch (configuration.Name)
                {
                    case "facebook_app_id": FacebookAppID = configuration.Data; break;
                    case "facebook_app_name": FacebookAppName = configuration.Data; break;
                    case "facebook_client_token": FacebookClientToken = configuration.Data; break;
                    case "admob_app_id": AdmobAppId = configuration.Data; break;
                    case "store_id": StoreId = configuration.Data; break;
                    case "firebase": Firebase = configuration.Data; break;
                }
            }
#if UNITY_IOS
            if (string.IsNullOrEmpty(FacebookAppName)) ConfigurationDataError("facebook_app_name");
            else if (string.IsNullOrEmpty(FacebookAppID)) ConfigurationDataError("facebook_app_id");
            else if (string.IsNullOrEmpty(FacebookClientToken)) ConfigurationDataError("facebook_client_token");
            else if (string.IsNullOrEmpty(AdmobAppId)) ConfigurationDataError("admob_app_id");
            else if (string.IsNullOrEmpty(StoreId)) ConfigurationDataError("store_id");
#endif


#if UNITY_ANDROID && SAYKIT_UPLOAD_ANDROID_SYMB 
            if (string.IsNullOrEmpty(Firebase)) ConfigurationDataError("firebase");
#endif
            if (!HasError)
            {
                SayKitDebug.Log("saykit_settings is successfully initialized. "
                                + "\n facebook_app_id: " + FacebookAppID
                                + "\n facebook_app_name: " + FacebookAppName
                                + "\n facebook_client_token: " + FacebookClientToken
                                + "\n admob_app_id: " + AdmobAppId
                                + "\n store_id: " + StoreId
                                + "\n firebase: " + Firebase);
            }
            
        }
        catch (Exception exception)
        {
            HandleError("Unexpected error", exception.Message);
        }
    }

    private void ConfigurationDataError(string fieldName)
    {
        HandleError("Configuration data error",
            $"saykit_settings.json file doesn't contain {fieldName} data! " +
            $"Please, check {fieldName} field in Assets/Resources/saykit_settings.json");
    }

    private void HandleError(string errorName, string errorMessage)
    {
        HasError = true;
        ErrorMessage = $"[SayKitRemoteSettings] {errorName}: {errorMessage}";
    }
}
#endif