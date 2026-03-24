namespace Infrastructure.BroTweens
{
    internal interface IBroPoolable
    {
        void Construct(IBroPool owner);
        void OnGet();
        void OnReturnToPool();
    }
}