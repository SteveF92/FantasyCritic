using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

[TestFixture]
public class AutomatedActionProcessingFlagTests : IntegrationTestBase
{
    private ApiSession _adminSession = null!;

    [OneTimeSetUp]
    public async Task LoginAsAdmin()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);
    }

    [OneTimeTearDown]
    public void DisposeSession()
    {
        _adminSession.Dispose();
    }

    //Off is the default, and nothing else should see it on.
    [TearDown]
    public async Task TurnFlagBackOff()
    {
        await _adminSession.ActionRunner.TurnOffAutomatedActionProcessingAsync();
    }

    [Test]
    public async Task TurnOnThenOff_RoundTrips()
    {
        await _adminSession.ActionRunner.TurnOnAutomatedActionProcessingAsync();
        var whileOn = await _adminSession.ActionRunner.GetEnableAutomatedActionProcessingAsync();

        await _adminSession.ActionRunner.TurnOffAutomatedActionProcessingAsync();
        var whileOff = await _adminSession.ActionRunner.GetEnableAutomatedActionProcessingAsync();

        Assert.Multiple(() =>
        {
            Assert.That(whileOn, Is.True);
            Assert.That(whileOff, Is.False);
        });
    }

    [Test]
    public async Task NonActionRunner_IsForbidden()
    {
        var (email, password, displayName) = NewUser();
        using var userSession = new ApiSession(Factory);
        await userSession.RegisterAsync(email, password, displayName);
        await userSession.LoginAsync(email, password);

        var getException = Assert.CatchAsync<ApiException>(() => userSession.ActionRunner.GetEnableAutomatedActionProcessingAsync());
        var turnOnException = Assert.CatchAsync<ApiException>(() => userSession.ActionRunner.TurnOnAutomatedActionProcessingAsync());
        var turnOffException = Assert.CatchAsync<ApiException>(() => userSession.ActionRunner.TurnOffAutomatedActionProcessingAsync());

        Assert.Multiple(() =>
        {
            Assert.That(getException!.StatusCode, Is.EqualTo(403));
            Assert.That(turnOnException!.StatusCode, Is.EqualTo(403));
            Assert.That(turnOffException!.StatusCode, Is.EqualTo(403));
        });
    }
}
