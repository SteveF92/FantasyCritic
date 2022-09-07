using System.Net.WebSockets;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FantasyCritic.Lib.Scheduling;

public class GrantSuperDropsTask : IScheduledTask
{
    private readonly IServiceProvider _serviceProvider;
    public string Schedule => "*/10 * * * *";

    public GrantSuperDropsTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
#if DEBUG
        Console.WriteLine("Not granting super drops - DEBUG version");
        return;
#endif
#pragma warning disable CS0162
        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using var scope = serviceScopeFactory.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        if (!clock.ShouldGrantSuperDrops())
        {
            return;
        }

        var adminService = scope.ServiceProvider.GetRequiredService<AdminService>();
        await adminService.GrantSuperDrops();
#pragma warning restore CS0162
    }
}
