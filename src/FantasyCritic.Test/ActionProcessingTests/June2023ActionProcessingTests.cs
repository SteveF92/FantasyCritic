using NodaTime;
using NodaTime.Text;

namespace FantasyCritic.Test.ActionProcessingTests;

public class June2023ActionProcessingTests : BaseActionProcessingTests
{
    protected override Instant ProcessingTime => InstantPattern.ExtendedIso.Parse("2023-06-18T00:18:45.698602Z").GetValueOrThrow();
    protected override string ActionProcessingSetName => "June2023";
    protected override bool DefaultAllowIneligible => false;
}
