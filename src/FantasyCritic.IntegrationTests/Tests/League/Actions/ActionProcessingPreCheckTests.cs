using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

[TestFixture]
public class ActionProcessingPreCheckTests : IntegrationTestBase
{
    private const string AdminNotificationEmailAddress = "steve.fallon@fantasycritic.games";
    private const string PreCheckSubject = "[Development] FantasyCritic - Action processing pre-check";

    private ApiSession _actionRunnerSession = null!;

    [OneTimeSetUp]
    public async Task LoginAsActionRunner()
    {
        _actionRunnerSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_actionRunnerSession);
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await _actionRunnerSession.ActionRunner.TurnOffAutomatedActionProcessingAsync();
        _actionRunnerSession.Dispose();
    }

    [Test]
    public async Task OutsideProductionWithFlagOff_EmailsBothReasons()
    {
        await _actionRunnerSession.ActionRunner.TurnOffAutomatedActionProcessingAsync();
        var sentBefore = Factory.CapturingEmailSender.GetEmailsWithSubject(PreCheckSubject).Count;

        await _actionRunnerSession.ActionRunner.SendActionProcessingPreCheckEmailAsync();

        var emails = Factory.CapturingEmailSender.GetEmailsWithSubject(PreCheckSubject);
        Assert.That(emails, Has.Count.EqualTo(sentBefore + 1));
        var email = emails.Last();
        Assert.Multiple(() =>
        {
            Assert.That(email.Email, Is.EqualTo(AdminNotificationEmailAddress));
            Assert.That(email.HtmlMessage, Does.Contain("This is Development, not production."));
            Assert.That(email.HtmlMessage, Does.Contain("Automated action processing is turned off."));
        });
    }

    //Mode is only a manual-run check: the automated job turns it on itself.
    [Test]
    public async Task ActionProcessingModeOff_IsNotAReason()
    {
        await _actionRunnerSession.ActionRunner.TurnOffActionProcessingModeAsync();

        await _actionRunnerSession.ActionRunner.SendActionProcessingPreCheckEmailAsync();

        var email = Factory.CapturingEmailSender.GetEmailsWithSubject(PreCheckSubject).Last();
        Assert.That(email.HtmlMessage, Does.Not.Contain("action processing mode"));
    }
}
