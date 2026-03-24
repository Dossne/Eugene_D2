using System.Threading;
using Cysharp.Threading.Tasks;

namespace Infrastructure.Pool
{
    public interface IPool
    {
        public string Tag { get; }
        UniTask InitializeAsync(CancellationToken cancellationToken);
        void ReleaseAllToPool();
        void Destroy();
    }
}