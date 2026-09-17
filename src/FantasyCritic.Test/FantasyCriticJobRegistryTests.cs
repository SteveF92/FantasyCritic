using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Jobs;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class FantasyCriticJobRegistryTests
{
    private static readonly IReadOnlyDictionary<FantasyCriticJobType, IReadOnlyList<FantasyCriticJobType>> SkipWhenDue = FantasyCriticJobRegistry.Create().SkipWhenDue;

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

        var exception = Assert.Throws<InvalidOperationException>(() => new FantasyCriticJobRegistry(definitions, SkipWhenDue));
        Assert.That(exception!.Message, Does.Contain("ProcessActions"));
    }

    [Test]
    public void Constructor_RejectsTwoHandlersForOneJobType()
    {
        var definitions = FantasyCriticJobRegistry.Create().Definitions.Append(FantasyCriticJobDefinition.For<SecondProcessActionsHandler>());

        var exception = Assert.Throws<InvalidOperationException>(() => new FantasyCriticJobRegistry(definitions, SkipWhenDue));
        Assert.That(exception!.Message, Does.Contain("ProcessActions"));
    }

    [Test]
    public void Constructor_RejectsACronHandlerRegisteredWithoutItsSchedule()
    {
        var definitions = FantasyCriticJobRegistry.Create().Definitions
            .Select(x => x.JobType.Equals(FantasyCriticJobType.ExpireTrades) ? FantasyCriticJobDefinition.For<CronExpireTradesHandler>() : x);

        var exception = Assert.Throws<InvalidOperationException>(() => new FantasyCriticJobRegistry(definitions, SkipWhenDue));
        Assert.That(exception!.Message, Does.Contain(nameof(CronExpireTradesHandler)));
    }

    [Test]
    public void GetDueJobsToDeferTo_FullDataRefreshDefersToPrepareForActionProcessingWhenBothAreDue()
    {
        var registry = FantasyCriticJobRegistry.Create();
        var due = new HashSet<FantasyCriticJobType> { FantasyCriticJobType.FullDataRefresh, FantasyCriticJobType.PrepareForActionProcessing };

        Assert.That(registry.GetDueJobsToDeferTo(FantasyCriticJobType.FullDataRefresh, due), Is.EqualTo(new[] { FantasyCriticJobType.PrepareForActionProcessing }));
    }

    [Test]
    public void GetDueJobsToDeferTo_IsEmptyWhenNothingItDefersToIsDue()
    {
        var registry = FantasyCriticJobRegistry.Create();
        var due = new HashSet<FantasyCriticJobType> { FantasyCriticJobType.FullDataRefresh, FantasyCriticJobType.ExpireTrades };

        Assert.That(registry.GetDueJobsToDeferTo(FantasyCriticJobType.FullDataRefresh, due), Is.Empty);
    }

    [Test]
    public void GetDueJobsToDeferTo_IsEmptyForAJobWithoutAnEntry()
    {
        var registry = FantasyCriticJobRegistry.Create();
        var due = new HashSet<FantasyCriticJobType> { FantasyCriticJobType.ExpireTrades, FantasyCriticJobType.PrepareForActionProcessing };

        Assert.That(registry.GetDueJobsToDeferTo(FantasyCriticJobType.ExpireTrades, due), Is.Empty);
    }

    [Test]
    public void Constructor_RejectsASkipWhenDueEntryNamingAJobWithoutASchedule()
    {
        var skipWhenDue = WithEntry(FantasyCriticJobType.ExpireTrades, FantasyCriticJobType.ProcessActions);

        var exception = Assert.Throws<InvalidOperationException>(() => new FantasyCriticJobRegistry(FantasyCriticJobRegistry.Create().Definitions, skipWhenDue));
        Assert.That(exception!.Message, Does.Contain("ProcessActions"));
    }

    [Test]
    public void Constructor_RejectsAJobSkippingInFavorOfItself()
    {
        var skipWhenDue = WithEntry(FantasyCriticJobType.ExpireTrades, FantasyCriticJobType.ExpireTrades);

        var exception = Assert.Throws<InvalidOperationException>(() => new FantasyCriticJobRegistry(FantasyCriticJobRegistry.Create().Definitions, skipWhenDue));
        Assert.That(exception!.Message, Does.Contain("ExpireTrades"));
    }

    [Test]
    public void Constructor_RejectsAChainOfSkipWhenDueEntries()
    {
        var skipWhenDue = WithEntry(FantasyCriticJobType.PrepareForActionProcessing, FantasyCriticJobType.ExpireTrades);

        var exception = Assert.Throws<InvalidOperationException>(() => new FantasyCriticJobRegistry(FantasyCriticJobRegistry.Create().Definitions, skipWhenDue));
        Assert.That(exception!.Message, Does.Contain("PrepareForActionProcessing"));
    }

    private static Dictionary<FantasyCriticJobType, IReadOnlyList<FantasyCriticJobType>> WithEntry(FantasyCriticJobType jobType, FantasyCriticJobType deferTo) =>
        new(SkipWhenDue) { [jobType] = [deferTo] };

    //Checks the registrations rather than resolving them: handlers need the whole application graph, which the worker's ValidateOnBuild checks at startup.
    [Test]
    public void AddFantasyCriticJobHandlers_RegistersEachHandlerScopedUnderItsJobType()
    {
        var services = new ServiceCollection().AddFantasyCriticJobHandlers();
        var registry = FantasyCriticJobRegistry.Create();

        Assert.Multiple(() =>
        {
            foreach (var definition in registry.Definitions)
            {
                var descriptors = services.Where(x => x.IsKeyedService && x.ServiceType == typeof(IJobHandler) && definition.JobType.Equals(x.ServiceKey)).ToList();
                Assert.That(descriptors, Has.Count.EqualTo(1), definition.JobType.Value);
                Assert.That(descriptors.Select(x => (x.KeyedImplementationType, x.Lifetime)), Is.EqualTo(new[] { (definition.HandlerType, ServiceLifetime.Scoped) }),
                    definition.JobType.Value);
            }
        });
    }

    public class SecondProcessActionsHandler : IFantasyCriticJobHandler
    {
        public static FantasyCriticJobType JobType => FantasyCriticJobType.ProcessActions;
        public Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken) => Task.FromResult(Result.Success());
    }

    public class CronExpireTradesHandler : IFantasyCriticCronJobHandler
    {
        public static FantasyCriticJobType JobType => FantasyCriticJobType.ExpireTrades;
        public static FantasyCriticJobSchedule Schedule => FantasyCriticJobSchedule.EveryTenMinutes;
        public Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken) => Task.FromResult(Result.Success());
    }
}
