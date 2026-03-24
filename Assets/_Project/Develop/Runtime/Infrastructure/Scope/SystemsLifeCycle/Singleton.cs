namespace Infrastructure.SystemsLifeCycle
{
    /// <summary>
    /// Use as less as possible
    /// </summary>
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        public Singleton()
        {
        }


        public static T I => Nested.instance;

        private class Nested
        {
            static Nested()
            {
            }


            internal static readonly T instance = new();
        }
    }
}