using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.PurchaseSystem
{
    [Serializable]
    public class PurchasedInAppData
    {
        public OfferId inAppOfferId;
        public DateTime dateTime;
    }
}