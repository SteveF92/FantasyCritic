using NodaTime;
using NodaTime.Text;

namespace FantasyCritic.Test.ActionProcessingTests;

public class PostSummerGamesFest2022ActionProcessingTests : BaseActionProcessingTests
{
    protected override Instant ProcessingTime => InstantPattern.ExtendedIso.Parse("2022-06-19T00:03:02.969549Z").GetValueOrThrow();
    protected override string ActionProcessingSetName => "PostSummerGamesFest2022";
    protected override bool DefaultAllowIneligible => true;
}
