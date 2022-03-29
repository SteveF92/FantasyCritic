using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FantasyCritic.Lib.Scheduling;

public class TimeFlagsTask : IScheduledTask
{
    private readonly IServiceProvider _serviceProvider;
    public string Schedule => "0 */1 * * *";

    public TimeFlagsTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
#if DEBUG
        Console.WriteLine("Not setting time flags - DEBUG version");
        return;
#endif
#pragma warning disable CS0162
        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using (var scope = serviceScopeFactory.CreateScope())
        {
            var adminService = scope.ServiceProvider.GetRequiredService<AdminService>();
            await adminService.SetTimeFlags();
        }
#pragma warning restore CS0162
    }
}
