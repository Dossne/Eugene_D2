#if SAYKIT_PURCHASING

using UnityEngine.Purchasing;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

#endregion

namespace SayKitInternal
{
    public class InAppProduct
    {
        public Product PurchasedProduct { get; set; }
        public bool Success { get; set; }
        public bool SeenBefore { get; set; }
        public string ErrorMessage { get; set; }
    }
    
    public class InAppProductData : InAppProduct
    {
        public InAppProduct Product { get; set; }
        public string AdjustToken { get; set; }
        public string Currency { get; set; }
        public float Price { get; set; }
        public string TransactionId { get; set; }
    }
    
}

#endif