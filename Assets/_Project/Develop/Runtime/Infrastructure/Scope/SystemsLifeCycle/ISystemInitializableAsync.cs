using Cysharp.Threading.Tasks;

namespace Infrastructure.SystemsLifeCycle
{
    public interface ISystemInitializableAsync
    {
        public UniTask InitializeAsync();
    }
}