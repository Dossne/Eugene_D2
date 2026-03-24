using System;

namespace Infrastructure.Utilities
{
    public static class TypeExtensions
    {

        public static bool IsComponent<T>()
        {
            return typeof(T).DerivesFromOrEqual<UnityEngine.Component>();
        }

        public static bool IsGameObject<T>()
        {
            return typeof(T).DerivesFromOrEqual<UnityEngine.GameObject>();
        }

        public static bool DerivesFrom<T>(this Type a)
        {
            return DerivesFrom(a, typeof(T));
        }

        // This seems easier to think about than IsAssignableFrom
        public static bool DerivesFrom(this Type a, Type b)
        {
            return b != a && a.DerivesFromOrEqual(b);
        }

        public static bool DerivesFromOrEqual<T>(this Type a)
        {
            return DerivesFromOrEqual(a, typeof(T));
        }

        private static bool DerivesFromOrEqual(this Type a, Type b)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return b == a || b.GetTypeInfo().IsAssignableFrom(a.GetTypeInfo());
#else
            return b == a || b.IsAssignableFrom(a);
#endif
        }
    }
}