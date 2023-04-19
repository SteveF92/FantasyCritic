using NUnit.Framework;
using VerifyTests;

namespace FantasyCritic.Test.ActionProcessingTests;

[SetUpFixture]
public class ActionProcessingTestsSetup
{
    [OneTimeSetUp]
    public void Initialize()
    {
        VerifierSettings.DontScrubGuids();
    }
}
