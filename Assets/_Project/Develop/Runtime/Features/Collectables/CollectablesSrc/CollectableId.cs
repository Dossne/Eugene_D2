namespace Features.Collectables
{
    public static class CollectableId
    {
        private static int current = 1;


        public static int GetNext()
        {
            current++;
            return current;
        }


        public static int GetCurrent()
        {
            return current;
        }


        public static void Reset()
        {
            current = 0;
        }
    }
}