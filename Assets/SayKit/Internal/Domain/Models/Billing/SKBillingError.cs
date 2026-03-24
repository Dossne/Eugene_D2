using System;
using Newtonsoft.Json;

#region ReSharper

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class SKBillingError
    {
        [JsonConstructor]
        public SKBillingError() { }

        public SKBillingError(string message)
        {
            Message = message;
            Type = SKBillingErrorType.Unknown;
        }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(SKBillingErrorTypeJsonConverter))]
        public SKBillingErrorType Type { get; set; }

        public override string ToString()
        {
            var typePart = Type.ToString();
            var messagePart = string.IsNullOrEmpty(Message) ? "<empty>" : Message;
            return $"[SKBillingError] Type: {typePart}; Message: {messagePart}";
        }
    }
    
    public enum SKBillingErrorType
    {
        Unknown,
        ServerValidation,
        NoInternet,
        UserCancelled,
        EmptyProductIds,
        MissingPaymentProvider,
        PurchaseInProgress,
        PurchaseIsPending,
        ProductAlreadyBought,
        ItemAlreadyOwned,
        NotOwned,
        PaymentInterrupted,
        BadDeepLinkParameters,
        MissingWebPageURL,
        OffstoreWebPageURLMissing, 
        WebPageEmptyResponse,
        OffstoreWebPageEmptyResponse, 
        WebPageBadResponse,
        OffstoreWebPageBadResponse,
        Redirect,
        ProductNotSupported,
        GoogleServiceDisconnected
    }
    
    public class SKBillingErrorTypeJsonConverter : JsonConverter<SKBillingErrorType>
    {
        private static SKBillingErrorType FromString(string type)
        {
            if (string.IsNullOrEmpty(type))
                return SKBillingErrorType.Unknown;

            var key = type.Trim().ToLowerInvariant();

            switch (key)
            {
                case "unknown":
                    return SKBillingErrorType.Unknown;
                case "servervalidation":
                    return SKBillingErrorType.ServerValidation;
                case "nointernet":
                    return SKBillingErrorType.NoInternet;
                case "usercancelled":
                    return SKBillingErrorType.UserCancelled;
                case "emptyproductids":
                    return SKBillingErrorType.EmptyProductIds;
                case "missingpaymentprovider":
                    return SKBillingErrorType.MissingPaymentProvider;
                case "purchaseinprogress":
                    return SKBillingErrorType.PurchaseInProgress;
                case "purchaseispending":
                    return SKBillingErrorType.PurchaseIsPending;
                case "productalreadybought":
                    return SKBillingErrorType.ProductAlreadyBought;
                case "itemalreadyowned":
                    return SKBillingErrorType.ItemAlreadyOwned;
                case "notowned":
                    return SKBillingErrorType.NotOwned;
                case "paymentinterrupted":
                    return SKBillingErrorType.PaymentInterrupted;
                case "baddeeplinkparameters":
                    return SKBillingErrorType.BadDeepLinkParameters;
                case "missingwebpageurl":
                    return SKBillingErrorType.MissingWebPageURL;
                case "offstorewebpageurlmissing":
                    return SKBillingErrorType.OffstoreWebPageURLMissing;
                case "webpageemptyresponse":
                    return SKBillingErrorType.WebPageEmptyResponse;
                case "offstorewebpageemptyresponse":
                    return SKBillingErrorType.OffstoreWebPageEmptyResponse;
                case "webpagebadresponse":
                    return SKBillingErrorType.WebPageBadResponse;
                case "offstorewebpagebadresponse":
                    return SKBillingErrorType.OffstoreWebPageBadResponse;
                case "redirect":
                    return SKBillingErrorType.Redirect;
                case "productnotsupported":
                    return SKBillingErrorType.ProductNotSupported;
                case "googleservicedisconnected":
                    return SKBillingErrorType.GoogleServiceDisconnected;
                default:
                    return SKBillingErrorType.Unknown;
            }
        }

        private static string ToSerializedString(SKBillingErrorType type)
        {
            switch (type)
            {
                case SKBillingErrorType.ServerValidation:
                    return "serverValidation";
                case SKBillingErrorType.NoInternet:
                    return "noInternet";
                case SKBillingErrorType.UserCancelled:
                    return "userCancelled";
                case SKBillingErrorType.EmptyProductIds:
                    return "emptyProductIds";
                case SKBillingErrorType.MissingPaymentProvider:
                    return "missingPaymentProvider";
                case SKBillingErrorType.PurchaseInProgress:
                    return "purchaseInProgress";
                case SKBillingErrorType.PurchaseIsPending:
                    return "purchaseIsPending";
                case SKBillingErrorType.ProductAlreadyBought:
                    return "productAlreadyBought";
                case SKBillingErrorType.ItemAlreadyOwned:
                    return "itemAlreadyOwned";
                case SKBillingErrorType.NotOwned:
                    return "notOwned";
                case SKBillingErrorType.PaymentInterrupted:
                    return "paymentInterrupted";
                case SKBillingErrorType.BadDeepLinkParameters:
                    return "badDeepLinkParameters";
                case SKBillingErrorType.MissingWebPageURL:
                    return "missingWebPageURL";
                case SKBillingErrorType.OffstoreWebPageURLMissing:
                    return "offstoreWebPageURLMissing";
                case SKBillingErrorType.WebPageEmptyResponse:
                    return "webPageEmptyResponse";
                case SKBillingErrorType.OffstoreWebPageEmptyResponse:
                    return "offstoreWebPageEmptyResponse";
                case SKBillingErrorType.WebPageBadResponse:
                    return "webPageBadResponse";
                case SKBillingErrorType.OffstoreWebPageBadResponse:
                    return "offstoreWebPageBadResponse";
                case SKBillingErrorType.Redirect:
                    return "redirect";
                case SKBillingErrorType.ProductNotSupported:
                    return "productNotSupported";
                case SKBillingErrorType.GoogleServiceDisconnected:
                    return "googleServiceDisconnected";
                case SKBillingErrorType.Unknown:
                default:
                    return "unknown";
            }
        }

        public override SKBillingErrorType ReadJson(JsonReader reader, Type objectType, SKBillingErrorType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.TokenType == JsonToken.Null ? SKBillingErrorType.Unknown : FromString(reader.Value as string);
        }

        public override void WriteJson(JsonWriter writer, SKBillingErrorType value, JsonSerializer serializer)
        {
            writer.WriteValue(ToSerializedString(value));
        }
    }
}