#if SAYKIT_PURCHASING

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable RedundantUsingDirective
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Local

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

namespace SayKitInternal
{
    public class InAppManager
    {
        public static InAppManager Instance { get; } = new InAppManager();

        private Action<InAppProduct> _validationCallback;
        private Product _purchasedProduct;
        private InAppRequestData _requestData;

        public void TrackPurchase(PurchaseEventArgs purchaseEventArgs, Action<InAppProduct> validationCallback,
            string offer, string placement, string extra)
        {
            try
            {
                TrackPurchase(purchaseEventArgs.purchasedProduct, validationCallback, offer, placement, extra, null, null);
            }
            catch (Exception e)
            {
                CallErrorValidationCallBack("[InAppManager] TrackPurchase: " + e.Message, e.StackTrace, null, validationCallback);
            }
        }

        public void TrackPurchase(Product purchasedProduct, Action<InAppProduct> validationCallback, string offer, string placement, string extra,
            string orderReceipt, string orderTransactionId)
        {
            try
            {
                var price = decimal.ToSingle(purchasedProduct.metadata.localizedPrice);
                var currencyCode = purchasedProduct.metadata.isoCurrencyCode;

                var productId = purchasedProduct.definition.id;
                var storeProductId = purchasedProduct.definition.storeSpecificId;

                var eventExtra = new Dictionary<string, object>
                {
                    ["product_id"] = productId,
                    ["currency"] = currencyCode,
                    ["price"] = price.ToString(CultureInfo.InvariantCulture),
                    ["store_product_id"] = storeProductId
                };

                if (!string.IsNullOrEmpty(offer))
                {
                    eventExtra["gs_offer"] = offer;
                }

                if (!string.IsNullOrEmpty(placement))
                {
                    eventExtra["placement"] = placement;
                }

                if (!string.IsNullOrEmpty(extra))
                {
                    eventExtra["extra"] = extra;
                }

#if UNITY_EDITOR
                SKBridgeManager.Instance.TrackEvent(name: "iap_editor", extra1: JsonConvert.SerializeObject(eventExtra));
                validationCallback?.Invoke(new InAppProduct { PurchasedProduct = purchasedProduct, Success = true });
                Debug.Log("SayKit.Purchase: " + JsonConvert.SerializeObject(eventExtra));
#else
                var receipt = string.IsNullOrEmpty(orderReceipt) ? purchasedProduct.receipt : orderReceipt;
                var transactionId = string.IsNullOrEmpty(orderTransactionId) ? purchasedProduct.transactionID : orderTransactionId;

                Dictionary<string, object> receiptWrapper;
                try
                {
                    var jsonObject = JObject.Parse(receipt);
                    receiptWrapper = jsonObject.ToObject<Dictionary<string, object>>();
                }
                catch
                {
                    receiptWrapper = JsonConvert.DeserializeObject<Dictionary<string, object>>(receipt);
                }

                if (receiptWrapper == null)
                {
                    CallErrorValidationCallBack("[InAppManager] TrackPurchase ", "Product receipt is empty or invalid json", null, validationCallback);
                    return;
                }

                object payload = null;
                if (receiptWrapper.TryGetValue("Payload", out var payloadRaw))
                {
                    payload = payloadRaw;
                }

                var inAppRequestData = new InAppRequestData
                {
                    ProductId = productId,
                    StoreProductId = storeProductId,
                    Price = price,
                    Currency = currencyCode,
                    TransactionId = transactionId
                };

#if UNITY_ANDROID
                if (!ExtractAndroidPayload(payload, out var gpJson, out var gpSig))
                {
                    CallErrorValidationCallBack("[InAppManager] TrackPurchase ", "Unable to extract json/signature from Payload", null, validationCallback);
                    return;
                }

                eventExtra["json"] = gpJson;
                eventExtra["signature"] = gpSig;
                SKBridgeManager.Instance.TrackEvent(name: "iap_android", extra1: JsonConvert.SerializeObject(eventExtra));

                inAppRequestData.Json = gpJson;
                inAppRequestData.Signature = gpSig;

#elif UNITY_IOS
                string payloadStr = null;

                if (payload is string strPayload)
                {
                    payloadStr = strPayload;
                }
                else if (payload is JObject payloadObject)
                {
                    payloadStr = payloadObject.ToString(Formatting.None);
                }
                else if (payload is Dictionary<string, object> payloadDict)
                {
                    payloadStr = JsonConvert.SerializeObject(payloadDict);
                }
                else if (payload != null)
                {
                    payloadStr = JsonConvert.SerializeObject(payload);
                }

                eventExtra["transaction_id"] = transactionId;
                eventExtra["receipt"] = payloadStr;
                SKBridgeManager.Instance.TrackEvent(name: "iap_ios", extra1: JsonConvert.SerializeObject(eventExtra));

                inAppRequestData.Receipt = payloadStr;
#endif
                CheckInApp(inAppRequestData, purchasedProduct, validationCallback);
#endif
            }
            catch (Exception ex)
            {
                CallErrorValidationCallBack("[InAppManager] TrackPurchase " + ex.Message, ex.StackTrace, null,
                    validationCallback);
            }
        }

        private void CheckInApp(InAppRequestData requestData, Product purchasedProduct,
            Action<InAppProduct> validationCallback)
        {
            _validationCallback = validationCallback;
            _purchasedProduct = purchasedProduct;

            _requestData = requestData;

            var postData = JsonConvert.SerializeObject(requestData);
            SKBridgeManager.Instance.CheckInAppProduct(postData);
        }

        public void OnInAppProductChecked(string json)
        {
            var iapProduct = new InAppProduct
            {
                PurchasedProduct = _purchasedProduct,
            };

            if (string.IsNullOrEmpty(json))
            {
                iapProduct.Success = false;
                iapProduct.ErrorMessage = "Validation error";
                _validationCallback?.Invoke(iapProduct);
                return;
            }

            try
            {
                var inAppResponseData = JsonConvert.DeserializeObject<InAppResponseData>(json);

                if (inAppResponseData.Success)
                {
                    iapProduct.Success = true;
                    iapProduct.SeenBefore = inAppResponseData.SeenBefore;
                    _validationCallback?.Invoke(iapProduct);
                }
            }
            catch (Exception exception)
            {
                CallErrorValidationCallBack("[InAppManager] OnInAppProductChecked: " + exception.Message,
                    exception.StackTrace, null, _validationCallback);
            }
        }

        private void CallErrorValidationCallBack(string errorMessage, string stackTrace, InAppProduct product,
            Action<InAppProduct> validationCallback, bool trackException = true)
        {
            if (product != null)
            {
                product.Success = false;
                product.ErrorMessage = errorMessage;
            }
            else
            {
                product = new InAppProduct
                {
                    Success = false,
                    ErrorMessage = errorMessage
                };
            }

            validationCallback?.Invoke(product);

            if (trackException)
            {
                SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception", extra1: errorMessage, extra2: stackTrace);
            }

            SayKitDebug.LogError(errorMessage);
        }

        private static bool ExtractAndroidPayload(object payload, out string gpJson, out string gpSig)
        {
            gpJson = null;
            gpSig = null;

            if (payload == null)
            {
                return false;
            }

            Dictionary<string, object> payloadDict;

            if (payload is string strPayload)
            {
                try
                {
                    payloadDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(strPayload);
                }
                catch
                {
                    var jsonObject = JObject.Parse(strPayload);
                    payloadDict = jsonObject.ToObject<Dictionary<string, object>>();
                }
            }
            else if (payload is JObject payloadObject)
            {
                payloadDict = payloadObject.ToObject<Dictionary<string, object>>();
            }
            else
            {
                payloadDict = payload as Dictionary<string, object>;
            }

            if (payloadDict == null)
            {
                return false;
            }

            if (payloadDict.TryGetValue("signature", out var signatureObj))
            {
                gpSig = signatureObj?.ToString();
            }

            if (payloadDict.TryGetValue("json", out var jsonObj))
            {
                if (jsonObj is string strJson)
                {
                    gpJson = strJson;
                }
                else if (jsonObj is JObject jsonObject)
                {
                    gpJson = jsonObject.ToString(Formatting.None);
                }
                else
                {
                    gpJson = JsonConvert.SerializeObject(jsonObj);
                }
            }

            return !string.IsNullOrEmpty(gpJson) && !string.IsNullOrEmpty(gpSig);
        }
    }
}

#endif // SAYKIT_PURCHASING
