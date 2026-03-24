using System;
using System.Collections.Generic;

namespace Infrastructure.Utilities
{
    public class GeneralPropertyComparer<T, TKey> : IEqualityComparer<T>
    {
        private readonly Func<T, TKey> property;
        private readonly IEqualityComparer<TKey> propertyComparer;

        public GeneralPropertyComparer(Func<T, TKey> property, IEqualityComparer<TKey> propertyComparer = null)
        {
            this.property = property;
            this.propertyComparer = propertyComparer;
        }

        public bool Equals(T first, T second)
        {
            var firstProperty = property.Invoke(first);
            var secondProperty = property.Invoke(second);
            if (propertyComparer != null) return propertyComparer.Equals(firstProperty, secondProperty);
            if (firstProperty == null && secondProperty == null) return true;
            if (firstProperty == null ^ secondProperty == null) return false;
            return firstProperty.Equals(secondProperty);
        }

        public int GetHashCode(T obj)
        {
            var prop = property.Invoke(obj);
            if (propertyComparer != null) return propertyComparer.GetHashCode(prop);
            return prop == null ? 0 : prop.GetHashCode();
        }
    }
}