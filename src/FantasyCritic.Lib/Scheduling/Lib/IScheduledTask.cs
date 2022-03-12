using System.Threading;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Scheduling.Lib
{
    public interface IScheduledTask
    {
        string Schedule { get; }
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
