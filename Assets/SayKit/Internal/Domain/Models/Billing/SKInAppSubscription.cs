using JetBrains.Annotations;
using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SKInAppSubscription
    {
        [JsonConstructor]
        public SKInAppSubscription() { }
        
        [JsonProperty("productId")] public string ProductId { get; set; }
        [JsonProperty("transactionId")] public string TransactionId { get; set; }
        [JsonProperty("isActive")] public bool IsActive { get; set; }
        [JsonProperty("autoRenewProductId")] [CanBeNull] public string AutoRenewProductId { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("startDate")] public long StartDate { get; set; }
        [JsonProperty("expirationDate")] public long ExpirationDate { get; set; }
        [JsonProperty("isTrial")] public bool IsTrial { get; set; }
        [JsonProperty("gracePeriodEndDate")] [CanBeNull] public string GracePeriodEndDate { get; set; }
        [JsonProperty("cancelReason")] public string CancelReason { get; set; }
        [JsonProperty("wasIntro")] public bool WasIntro { get; set; }
        [JsonProperty("wasTrial")] public bool WasTrial { get; set; }
        [JsonProperty("subscriptionGroupIdentifier")] [CanBeNull] public string SubscriptionGroupIdentifier { get; set; }
    }
}