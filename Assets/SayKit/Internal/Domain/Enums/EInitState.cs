#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    public enum EInitState
    {
        None,
        Platform,
        RemoteConfig,
        Consent,
        IDFA,
        Gdpr,
        Done
    }
}