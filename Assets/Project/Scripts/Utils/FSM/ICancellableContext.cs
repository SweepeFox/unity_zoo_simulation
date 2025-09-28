using System.Threading;

namespace Utils.Fsm
{
    public interface ICancellableContext
    {
        CancellationToken Token { get; }
    }
}
