using NodaTime.Text;
using NUnit.Framework;

namespace FantasyCritic.Test.ActionProcessingTests;
public class PostSummerGamesFest2022ActionProcessingTests : BaseActionProcessingTests
{
    [OneTimeSetUp]
    public void RunActionProcess()
    {
        SetupAndProcess(InstantPattern.ExtendedIso.Parse("2022-06-19T00:03:02.969549Z").GetValueOrThrow(), "PostSummerGamesFest2022");
    }
}
