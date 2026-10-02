using System;
using System.Threading.Tasks;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Admin;

[TestFixture]
public class AdminNotificationEmailTests : IntegrationTestBase
{
    private const string AdminNotificationEmailAddress = "steve.fallon@fantasycritic.games";

    [Test]
    public async Task OutsideProduction_SubjectAndFirstLineNameTheEnvironment()
    {
        //Unique, since every test shares the capturing sender and this address.
        var subject = $"Test notification {Guid.NewGuid()}";

        await using var scope = Factory.Services.CreateAsyncScope();
        var emailSendingService = scope.ServiceProvider.GetRequiredService<EmailSendingService>();
        var environment = scope.ServiceProvider.GetRequiredService<EnvironmentConfiguration>();
        await emailSendingService.SendAdminNotification(subject, ["First reason.", "Games & requests."]);

        var emails = Factory.CapturingEmailSender.GetEmailsWithSubject($"[Development] FantasyCritic - {subject}");

        Assert.That(emails, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(emails[0].Email, Is.EqualTo(AdminNotificationEmailAddress));
            Assert.That(emails[0].HtmlMessage, Is.EqualTo(
                $"<p>Sent from Development ({environment.BaseAddress}), not production.</p><p>First reason.</p><p>Games &amp; requests.</p>"));
        });
    }

    [Test]
    public async Task InProduction_NoEnvironmentLabel()
    {
        var subject = $"Test notification {Guid.NewGuid()}";

        await using var scope = Factory.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var environment = services.GetRequiredService<EnvironmentConfiguration>();
        var productionEmailSendingService = new EmailSendingService(services.GetRequiredService<FantasyCriticUserManager>(), services.GetRequiredService<IEmailBuilder>(),
            services.GetRequiredService<IEmailSender>(), services.GetRequiredService<LeagueMemberService>(), environment with { IsProduction = true });
        await productionEmailSendingService.SendAdminNotification(subject, ["First reason."]);

        var emails = Factory.CapturingEmailSender.GetEmailsWithSubject($"FantasyCritic - {subject}");

        Assert.That(emails, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(emails[0].Email, Is.EqualTo(AdminNotificationEmailAddress));
            Assert.That(emails[0].HtmlMessage, Is.EqualTo("<p>First reason.</p>"));
        });
    }
}
