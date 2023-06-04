using NodaTime;
using NodaTime.Text;

namespace FantasyCritic.Test.ActionProcessingTests;

public class April2023ActionProcessingTests : BaseActionProcessingTests
{
    protected override Instant ProcessingTime => InstantPattern.ExtendedIso.Parse("2023-04-16T00:10:42.698602Z").GetValueOrThrow();
    protected override string ActionProcessingSetName => "April2023";
    protected override bool DefaultAllowIneligible => false;
}
