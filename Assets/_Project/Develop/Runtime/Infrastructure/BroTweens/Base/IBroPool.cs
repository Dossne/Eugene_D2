namespace Infrastructure.BroTweens
{
    internal interface IBroPool
    {
        void Prewarm(int count);
        void Deinitialize();
        void Release(IBroPoolable element);
    }
}