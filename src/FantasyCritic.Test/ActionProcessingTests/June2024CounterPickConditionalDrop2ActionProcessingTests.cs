using System;
using NodaTime;
using NodaTime.Text;

namespace FantasyCritic.Test.ActionProcessingTests;

public class June2024CounterPickConditionalDrop2ActionProcessingTests : BaseActionProcessingTests
{
    protected override Instant ProcessingTime => InstantPattern.ExtendedIso.Parse("2024-06-23T00:13:50.000000Z").GetValueOrThrow();
    protected override string ActionProcessingSetName => "June2024";
    protected override bool DefaultAllowIneligible => false;
    protected override Guid? OnlyRunLeagueID => Guid.Parse("7eb72337-f85c-4da2-8a0b-b3bfee13e7f0");
}
