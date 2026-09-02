using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Account;

[TestFixture]
public class SupportTicketResolvedHistoryTests : IntegrationTestBase
{
    private const string SupportTicketPath = "/account/manage/supportticket";

    [Test]
    public async Task ClosedSupportTicket_ResolutionNotes_AppearOnUserSupportTicketPage()
    {
        var issueDescription = "I cannot log in to my account anymore.";
        var resolutionNotes = "Your account email has been verified. You should be able to log in now.";

        var (email, password, _) = NewUser();
        using var userSession = new ApiSession(Factory);
        await userSession.RegisterAsync(email, password, NewUser().displayName);

        await OpenSupportTicketViaRazorPageAsync(userSession, issueDescription);

        var me = await userSession.Account.CurrentUserAsync();

        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        var activeTickets = await adminSession.Admin.GetActiveSupportTicketsAsync();
        var ticket = FindTicketForUser(activeTickets, me.UserID);

        await adminSession.Admin.CloseSupportTicketAsync(new CloseSupportTicketRequest
        {
            SupportTicketID = ticket.SupportTicketID,
            ResolutionNotes = resolutionNotes,
        });

        var pageResponse = await userSession.GetAsync(SupportTicketPath);
        var html = await pageResponse.Content.ReadAsStringAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pageResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(html, Does.Contain("Resolved tickets"),
                "User support ticket page should include a resolved tickets section.");
            Assert.That(html, Does.Contain(resolutionNotes),
                "Admin resolution notes should be visible on the user support ticket page.");
            Assert.That(html, Does.Contain(issueDescription),
                "The original issue description should appear in the resolved ticket entry.");
        }
    }

    [Test]
    public async Task ClosedSupportTicket_WithNoResolutionNotes_ShowsEmptyStateMessage()
    {
        var issueDescription = "Please reset my two-factor authentication setup.";

        var (email, password, displayName) = NewUser();
        using var userSession = new ApiSession(Factory);
        await userSession.RegisterAsync(email, password, displayName);

        await OpenSupportTicketViaRazorPageAsync(userSession, issueDescription);

        var me = await userSession.Account.CurrentUserAsync();

        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        var activeTickets = await adminSession.Admin.GetActiveSupportTicketsAsync();
        var ticket = FindTicketForUser(activeTickets, me.UserID);

        await adminSession.Admin.CloseSupportTicketAsync(new CloseSupportTicketRequest
        {
            SupportTicketID = ticket.SupportTicketID,
            ResolutionNotes = null,
        });

        var pageResponse = await userSession.GetAsync(SupportTicketPath);
        var html = await pageResponse.Content.ReadAsStringAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pageResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(html, Does.Contain("No resolution notes provided."),
                "Closed tickets without admin notes should show the empty-state resolution message.");
        }
    }

    private static SupportTicketAdminListEntryViewModel FindTicketForUser(
        ICollection<SupportTicketAdminListEntryViewModel> tickets,
        System.Guid userID)
    {
        foreach (var ticket in tickets)
        {
            if (ticket.UserID == userID)
            {
                return ticket;
            }
        }

        throw new AssertionException($"Expected an active support ticket for user {userID}.");
    }

    private static async Task OpenSupportTicketViaRazorPageAsync(ApiSession session, string issueDescription)
    {
        var getResponse = await session.GetAsync(SupportTicketPath);
        var html = await getResponse.Content.ReadAsStringAsync();
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var token = await session.GetPageAntiForgeryTokenAsync(SupportTicketPath);

        var postResponse = await session.PostFormAsync(SupportTicketPath, new Dictionary<string, string>
        {
            ["Input.IssueDescription"] = issueDescription,
            ["__RequestVerificationToken"] = token,
        });

        Assert.That(postResponse.StatusCode, Is.AnyOf(HttpStatusCode.Redirect, HttpStatusCode.Found),
            "Opening a support ticket should redirect back to the page.");
    }
}
