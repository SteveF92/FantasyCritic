using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FantasyCritic.Lib.Jobs;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class FantasyCriticJobRegistryTests
{
    [Test]
    public void Create_RegistersExactlyOneHandlerForEveryJobType()
    {
        var registry = FantasyCriticJobRegistry.Create();

        Assert.That(registry.Definitions.Select(x => x.JobType), Is.EquivalentTo(FantasyCriticJobType.GetAllPossibleValues()));
    }

    [Test]
    public void Constructor_RejectsAMissingJobType()
    {
        var definitions = FantasyCriticJobRegistry.Create().Definitions.Where(x => !x.JobType.Equals(FantasyCriticJobType.ProcessActions));

        var exception = Assert.Throws<InvalidOperationException>(() => new FantasyCriticJobRegistry(definitions));
        Assert.That(exception!.Message, Does.Contain("ProcessActions"));
    }

    [Test]
    public void Constructor_RejectsTwoHandlersForOneJobType()
    {
        var definitions = FantasyCriticJobRegistry.Create().Definitions.Append(FantasyCriticJobDefinition.For<SecondProcessActionsHandler>());

        var exception = Assert.Throws<InvalidOperationException>(() => new FantasyCriticJobRegistry(definitions));
        Assert.That(exception!.Message, Does.Contain("ProcessActions"));
    }

    [Test]
    public void Constructor_RejectsACronHandlerRegisteredWithoutItsSchedule()
    {
        var definitions = FantasyCriticJobRegistry.Create().Definitions
            .Select(x => x.JobType.Equals(FantasyCriticJobType.ExpireTrades) ? FantasyCriticJobDefinition.For<CronExpireTradesHandler>() : x);

        var exception = Assert.Throws<InvalidOperationException>(() => new FantasyCriticJobRegistry(definitions));
        Assert.That(exception!.Message, Does.Contain(nameof(CronExpireTradesHandler)));
    }

    [Test]
    public void AddFantasyCriticJobHandlers_ResolvesEachJobTypeToItsOwnHandler()
    {
        var services = new ServiceCollection().AddFantasyCriticJobHandlers();
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        using var scope = provider.CreateScope();

        var registry = provider.GetRequiredService<FantasyCriticJobRegistry>();
        Assert.Multiple(() =>
        {
            foreach (var definition in registry.Definitions)
            {
                var handler = scope.ServiceProvider.GetRequiredKeyedService<IJobHandler>(definition.JobType);
                Assert.That(handler.GetType(), Is.EqualTo(definition.HandlerType), definition.JobType.Value);
            }
        });
    }

    public class SecondProcessActionsHandler : IFantasyCriticJobHandler
    {
        public static FantasyCriticJobType JobType => FantasyCriticJobType.ProcessActions;
        public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class CronExpireTradesHandler : IFantasyCriticCronJobHandler
    {
        public static FantasyCriticJobType JobType => FantasyCriticJobType.ExpireTrades;
        public static FantasyCriticJobSchedule Schedule => FantasyCriticJobSchedule.EveryTenMinutes;
        public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
