using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.JobManager;

[TestFixture]
public class GetJobsTests : IntegrationTestBase
{
    private const string ExpireTrades = "ExpireTrades";
    private const string MakeSlotsConsistent = "MakeSlotsConsistent";
    private const string Complete = "Complete";

    private ApiSession _adminSession = null!;

    [OneTimeSetUp]
    public async Task CreateOneCompleteJobOfTwoTypes()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);

        //Run each to completion. A job left queued would block every later manual run of its type.
        var expireTradesJob = await _adminSession.Admin.ExpireTradesAsync();
        await JobTestHelpers.RunQueuedJobAsync(Factory, expireTradesJob.JobID);

        var slotsJob = await _adminSession.Admin.MakePublisherSlotsConsistentAsync();
        await JobTestHelpers.RunQueuedJobAsync(Factory, slotsJob.JobID);
    }

    [OneTimeTearDown]
    public void DisposeSession()
    {
        _adminSession.Dispose();
    }

    [Test]
    public async Task NoFilter_ReturnsBothTypesNewestFirst()
    {
        var jobs = await _adminSession.JobManager.GetJobsAsync(1, 50, null, null, null, null);

        Assert.Multiple(() =>
        {
            Assert.That(jobs.Select(x => x.Type), Does.Contain(ExpireTrades).And.Contain(MakeSlotsConsistent));
            Assert.That(jobs.Select(x => x.CreatedAt), Is.Ordered.Descending);
        });
    }

    [Test]
    public async Task JobTypes_ReturnsOnlyThoseTypes()
    {
        var jobs = await _adminSession.JobManager.GetJobsAsync(1, 50, [ExpireTrades], null, null, null);

        Assert.That(jobs, Is.Not.Empty);
        Assert.That(jobs.Select(x => x.Type), Is.All.EqualTo(ExpireTrades));
    }

    [Test]
    public async Task ExcludeJobTypes_ReturnsEveryOtherType()
    {
        var jobs = await _adminSession.JobManager.GetJobsAsync(1, 50, null, [ExpireTrades], null, null);

        Assert.Multiple(() =>
        {
            Assert.That(jobs.Select(x => x.Type), Does.Contain(MakeSlotsConsistent));
            Assert.That(jobs.Select(x => x.Type), Has.None.EqualTo(ExpireTrades));
        });
    }

    [Test]
    public async Task Statuses_ReturnsOnlyThoseStatuses()
    {
        var jobs = await _adminSession.JobManager.GetJobsAsync(1, 50, null, null, [Complete], null);

        Assert.That(jobs, Is.Not.Empty);
        Assert.That(jobs.Select(x => x.Status), Is.All.EqualTo(Complete));
    }

    [Test]
    public async Task ExcludeStatuses_ReturnsEveryOtherStatus()
    {
        var jobs = await _adminSession.JobManager.GetJobsAsync(1, 50, null, null, null, [Complete]);

        Assert.That(jobs.Select(x => x.Status), Has.None.EqualTo(Complete));
    }

    [Test]
    public async Task IncludeAndExcludeTogether_BothApply()
    {
        var jobs = await _adminSession.JobManager.GetJobsAsync(1, 50, [ExpireTrades, MakeSlotsConsistent], [MakeSlotsConsistent], [Complete], null);

        Assert.That(jobs, Is.Not.Empty);
        Assert.Multiple(() =>
        {
            Assert.That(jobs.Select(x => x.Type), Is.All.EqualTo(ExpireTrades));
            Assert.That(jobs.Select(x => x.Status), Is.All.EqualTo(Complete));
        });
    }

    [Test]
    public void UnknownJobType_IsBadRequest()
    {
        var exception = Assert.CatchAsync<ApiException>(() => _adminSession.JobManager.GetJobsAsync(1, 10, ["NotAJobType"], null, null, null));

        Assert.That(exception!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void UnknownStatus_IsBadRequest()
    {
        var exception = Assert.CatchAsync<ApiException>(() => _adminSession.JobManager.GetJobsAsync(1, 10, null, null, null, ["NotAStatus"]));

        Assert.That(exception!.StatusCode, Is.EqualTo(400));
    }
}
