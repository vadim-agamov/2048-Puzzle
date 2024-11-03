using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Services.JumpScreenService
{
    public interface IJumpScreenService : IProgress<float>
    {
        UniTask Show(CancellationToken cancellationToken);
        UniTask Hide(CancellationToken cancellationToken);
    }
}