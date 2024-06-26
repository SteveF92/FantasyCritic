using System;
using NodaTime;
using NodaTime.Text;

namespace FantasyCritic.Test.ActionProcessingTests;

public class June2024SingleLeagueActionProcessingTests : BaseActionProcessingTests
{
    protected override Instant ProcessingTime => InstantPattern.ExtendedIso.Parse("2024-06-23T00:13:50.000000Z").GetValueOrThrow();
    protected override string ActionProcessingSetName => "June2024";
    protected override bool DefaultAllowIneligible => false;
    protected override Guid? OnlyRunLeagueID => Guid.Parse("bfab6f3f-7bf8-48ae-afaf-a80d98ecf10c");
}
