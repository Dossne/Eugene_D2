#region ReSharper

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

#endregion

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SayKitInternal
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SKProductType
    {
        Consumable,
        NonConsumable,
        Subscription
    }
}