#region ReSharper

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global

#endregion

using System.Collections.Generic;

namespace SayKitInternal
{
    public class SKPodDependency
    {
        public string Name { get; }
        public string Version { get; }
        public bool BothTarget { get; }

        private SKPodDependency(string name, string version, bool bothTarget)
        {
            Name = name;
            Version = version;
            BothTarget = bothTarget;
        }

        public override string ToString()
        {
            return $"pod '{Name}', '{Version}'";
        }
        
        public static List<SKPodDependency> GetRequiredPods()
        {
            return new List<SKPodDependency>
            {
                new SKPodDependency("SayKit", SKPodVersions.NativeVersion, bothTarget: false),
                new SKPodDependency("Adjust", SKPodVersions.Adjust, bothTarget: true),
                new SKPodDependency("Adjust/AdjustGoogleOdm", SKPodVersions.AdjustGoogleOdm, bothTarget: true),
                new SKPodDependency("AmazonPublisherServicesSDK", SKPodVersions.AmazonPublisherServicesSDK, bothTarget: true),
                new SKPodDependency("AppLovinSDK", SKPodVersions.AppLovinSDK, bothTarget: true),
                new SKPodDependency("BidMachine", SKPodVersions.BidMachine, bothTarget: true),
                new SKPodDependency("FBSDKCoreKit", SKPodVersions.FBSDKCoreKit, bothTarget: true),
                new SKPodDependency("Fyber_Marketplace_SDK", SKPodVersions.FyberMarketplaceSDK, bothTarget: true),
                new SKPodDependency("InMobiSDK", SKPodVersions.InMobiSDK, bothTarget: true),
                new SKPodDependency("OgurySdk", SKPodVersions.OgurySdk, bothTarget: true),
                new SKPodDependency("MolocoSDKiOS", SKPodVersions.MolocoSDKiOS, bothTarget: true),
                new SKPodDependency("smaato-ios-sdk", SKPodVersions.SmaatoIosSdk, bothTarget: true),
                new SKPodDependency("smaato-ios-sdk/InApp", SKPodVersions.SmaatoInApp, bothTarget: true),
                new SKPodDependency("YsoNetworkSDK", SKPodVersions.YsoNetworkSDK, bothTarget: true),
            };
        }
    }
}