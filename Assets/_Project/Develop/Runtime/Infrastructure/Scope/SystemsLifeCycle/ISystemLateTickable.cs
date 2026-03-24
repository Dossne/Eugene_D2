namespace Infrastructure.SystemsLifeCycle
{
    public interface ISystemLateTickable
    {
        void LateTick();
    }
}