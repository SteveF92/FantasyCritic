using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FantasyCritic.Lib.Scheduling;

public class RefreshDataTask : IScheduledTask
{
    private readonly IServiceProvider _serviceProvider;
    public string Schedule => "0 */2 * * *";

    public RefreshDataTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
#if DEBUG
        Console.WriteLine("Not refreshing stats - DEBUG version");
        return;
#endif
#pragma warning disable CS0162
        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using (var scope = serviceScopeFactory.CreateScope())
        {
            var adminService = scope.ServiceProvider.GetRequiredService<AdminService>();
            await adminService.FullDataRefresh();
        }
#pragma warning restore CS0162
    }
}
