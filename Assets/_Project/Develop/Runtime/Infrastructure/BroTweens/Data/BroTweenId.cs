namespace Infrastructure.BroTweens
{
    public static class BroTweenId
    {
        private static int current;


        public static int GetNext()
        {
            current++;
            return current;
        }


        public static void Reset()
        {
            current = 0;
        }
    }
}