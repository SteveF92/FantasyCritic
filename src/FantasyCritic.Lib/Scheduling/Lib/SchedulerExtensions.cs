using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FantasyCritic.Lib.Scheduling.Lib;

public static class SchedulerExtensions
{
    public static IServiceCollection AddScheduler(this IServiceCollection services)
    {
        return services.AddSingleton<IHostedService, SchedulerHostedService>();
    }

    public static IServiceCollection AddScheduler(this IServiceCollection services, EventHandler<UnobservedTaskExceptionEventArgs> unobservedTaskExceptionHandler)
    {
        return services.AddSingleton<IHostedService, SchedulerHostedService>(serviceProvider =>
        {
            var instance = new SchedulerHostedService(serviceProvider.GetServices<IScheduledTask>());
            instance.UnobservedTaskException += unobservedTaskExceptionHandler;
            return instance;
        });
    }
}
