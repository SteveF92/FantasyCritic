using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Admin;

[TestFixture]
public class ServiceMonitorTests : IntegrationTestBase
{
    private const string Unreachable = "Unreachable";

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

    //Every other test that runs a job depends on the worker being on.
    [TearDown]
    public async Task TurnWorkerBackOn()
    {
        await _adminSession.Admin.TurnOnWorkerAsync();
    }

    [Test]
    public async Task NeitherServiceRunning_ReportsUnreachableRatherThanFailing()
    {
        //The test host has no worker and no bot. That is a status to report, not an error.
        var monitor = await _adminSession.Admin.GetServiceMonitorAsync();

        Assert.Multiple(() =>
        {
            Assert.That(monitor.WorkerState, Is.EqualTo(Unreachable));
            Assert.That(monitor.Worker.Name, Is.EqualTo("Worker"));
            Assert.That(monitor.Worker.Status, Is.EqualTo(Unreachable));
            Assert.That(monitor.Worker.Description, Is.Not.Empty);
            Assert.That(monitor.DiscordBot.Name, Is.EqualTo("Discord Bot"));
            Assert.That(monitor.DiscordBot.Status, Is.EqualTo(Unreachable));
        });
    }

    [Test]
    public async Task TurnOffThenOn_RoundTripsThroughTheMonitor()
    {
        await _adminSession.Admin.TurnOffWorkerAsync();
        var whileOff = await _adminSession.Admin.GetServiceMonitorAsync();

        await _adminSession.Admin.TurnOnWorkerAsync();
        var whileOn = await _adminSession.Admin.GetServiceMonitorAsync();

        Assert.Multiple(() =>
        {
            Assert.That(whileOff.WorkerShouldPullNewJobs, Is.False);
            Assert.That(whileOn.WorkerShouldPullNewJobs, Is.True);
        });
    }

    [Test]
    public async Task NonAdmin_IsForbidden()
    {
        var (email, password, displayName) = NewUser();
        using var userSession = new ApiSession(Factory);
        await userSession.RegisterAsync(email, password, displayName);
        await userSession.LoginAsync(email, password);

        var monitorException = Assert.CatchAsync<ApiException>(() => userSession.Admin.GetServiceMonitorAsync());
        var turnOffException = Assert.CatchAsync<ApiException>(() => userSession.Admin.TurnOffWorkerAsync());

        Assert.Multiple(() =>
        {
            Assert.That(monitorException!.StatusCode, Is.EqualTo(403));
            Assert.That(turnOffException!.StatusCode, Is.EqualTo(403));
        });
    }
}
