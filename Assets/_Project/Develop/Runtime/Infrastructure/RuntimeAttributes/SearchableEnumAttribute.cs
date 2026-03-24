using System;
using UnityEngine;

namespace Infrastructure.RuntimeAttributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SearchableEnumAttribute : PropertyAttribute
    {
    }
}