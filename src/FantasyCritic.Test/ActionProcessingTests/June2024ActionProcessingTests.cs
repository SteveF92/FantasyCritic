using NodaTime;
using NodaTime.Text;

namespace FantasyCritic.Test.ActionProcessingTests;

public class June2024ActionProcessingTests : BaseActionProcessingTests
{
    protected override Instant ProcessingTime => InstantPattern.ExtendedIso.Parse("2024-06-23T00:13:50.000000Z").GetValueOrThrow();
    protected override string ActionProcessingSetName => "June2024";
    protected override bool DefaultAllowIneligible => false;
}
