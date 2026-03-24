#region ReSharper

// ReSharper disable CheckNamespace

#endregion

namespace SayKitInternal
{
    public class SayKitBridge :
#if UNITY_EDITOR
        SayKitBridgeEditor
#elif UNITY_ANDROID
        SayKitBridgeAndroid
#else
        SayKitBridgeIOS
#endif
    {

        public static SayKitBridge Instance { get; } = new SayKitBridge();

    }
}