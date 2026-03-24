using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.PurchaseSystem
{
    public enum PurchaseResult
    {
        UserCancelled = 0,
        Success = 1,
        NoInternetConnection = 2,
        Unknown = 3,
        Restore = 4,
    }
}