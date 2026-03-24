namespace Infrastructure.GameplayContainers
{
    public interface IGameplayContainer
    {
        string Tag { get; }
        bool IsActive { get; }
        void Initialize();
    }
}