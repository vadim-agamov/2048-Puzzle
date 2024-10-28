using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Modules.ServiceLocator;

namespace Services.JumpScreenService
{
    public interface IJumpScreenService : IService, IProgress<float>
    {
        UniTask Show(CancellationToken cancellationToken);
        UniTask Hide(CancellationToken cancellationToken);
    }
}