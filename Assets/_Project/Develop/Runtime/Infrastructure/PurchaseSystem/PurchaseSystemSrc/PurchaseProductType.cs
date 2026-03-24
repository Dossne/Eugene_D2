using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Infrastructure.PurchaseSystem
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PurchaseProductType
    {
        Consumable,
        NonConsumable,
        Subscription
    }
}