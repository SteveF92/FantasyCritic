using Microsoft.Extensions.DependencyInjection;

namespace FantasyCritic.Lib.Jobs;

public static class JobServiceCollectionExtensions
{
    //Only the worker registers these. Handlers are registered, not activated ad hoc, so ValidateOnBuild checks every handler's dependencies at startup.
    public static IServiceCollection AddFantasyCriticJobHandlers(this IServiceCollection services)
    {
        var registry = FantasyCriticJobRegistry.Create();
        services.AddSingleton(registry);

        //Keyed by job type, so running a job constructs only its own handler.
        foreach (var definition in registry.Definitions)
        {
            services.AddKeyedScoped(typeof(IJobHandler), definition.JobType, definition.HandlerType);
        }

        return services;
    }
}
