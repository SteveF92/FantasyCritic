using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Admin;

[TestFixture]
public class SiteAnnouncementTests : IntegrationTestBase
{
    private static CreateSiteAnnouncementRequest NewAnnouncementRequest(string title) => new()
    {
        Title = title,
        Body = "Body for " + title,
        PostedAt = new DateTimeOffset(2025, 3, 4, 12, 0, 0, TimeSpan.Zero),
        LinkAddress = "https://www.fantasycritic.games",
        LinkLabel = "Read more"
    };

    [Test]
    public async Task CreateSiteAnnouncement_AppearsInPublicList()
    {
        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        var title = "Created Announcement " + Guid.NewGuid();
        var created = await adminSession.Admin.CreateSiteAnnouncementAsync(NewAnnouncementRequest(title));

        Assert.Multiple(() =>
        {
            Assert.That(created.Title, Is.EqualTo(title));
            Assert.That(created.Body, Is.EqualTo("Body for " + title));
        });

        // The anonymous endpoint the site itself reads from should now include it.
        using var anonymousSession = new ApiSession(Factory);
        var publicAnnouncements = await anonymousSession.General.SiteAnnouncementsAsync();
        Assert.That(publicAnnouncements.Select(x => x.Id), Contains.Item(created.Id),
            "A newly created announcement should be visible on the public announcements endpoint.");
    }

    [Test]
    public async Task EditSiteAnnouncement_UpdatesStoredValues()
    {
        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        var created = await adminSession.Admin.CreateSiteAnnouncementAsync(NewAnnouncementRequest("Before Edit " + Guid.NewGuid()));

        var newTitle = "After Edit " + Guid.NewGuid();
        var newPostedAt = new DateTimeOffset(2025, 5, 6, 18, 30, 0, TimeSpan.Zero);
        var edited = await adminSession.Admin.EditSiteAnnouncementAsync(new EditSiteAnnouncementRequest
        {
            AnnouncementID = Guid.Parse(created.Id),
            Title = newTitle,
            Body = "Edited body.",
            PostedAt = newPostedAt,
            LinkAddress = null,
            LinkLabel = null
        });

        Assert.That(edited.Title, Is.EqualTo(newTitle));

        var fromList = (await adminSession.Admin.GetSiteAnnouncementsAsync()).Single(x => x.Id == created.Id);
        Assert.Multiple(() =>
        {
            Assert.That(fromList.Title, Is.EqualTo(newTitle));
            Assert.That(fromList.Body, Is.EqualTo("Edited body."));
            Assert.That(fromList.PostedAt, Is.EqualTo(newPostedAt));
            Assert.That(fromList.LinkAddress, Is.Null, "Clearing the link address should store NULL, not an empty string.");
            Assert.That(fromList.LinkLabel, Is.Null);
        });
    }

    [Test]
    public async Task DeleteSiteAnnouncement_RemovesItFromBothLists()
    {
        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        var created = await adminSession.Admin.CreateSiteAnnouncementAsync(NewAnnouncementRequest("To Delete " + Guid.NewGuid()));

        await adminSession.Admin.DeleteSiteAnnouncementAsync(new DeleteSiteAnnouncementRequest
        {
            AnnouncementID = Guid.Parse(created.Id)
        });

        var adminAnnouncements = await adminSession.Admin.GetSiteAnnouncementsAsync();
        Assert.That(adminAnnouncements.Select(x => x.Id), Does.Not.Contain(created.Id),
            "A soft-deleted announcement should not come back from the admin list.");

        using var anonymousSession = new ApiSession(Factory);
        var publicAnnouncements = await anonymousSession.General.SiteAnnouncementsAsync();
        Assert.That(publicAnnouncements.Select(x => x.Id), Does.Not.Contain(created.Id),
            "A soft-deleted announcement should not come back from the public list.");
    }

    [Test]
    public async Task EditSiteAnnouncement_AfterDelete_Returns404()
    {
        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        var created = await adminSession.Admin.CreateSiteAnnouncementAsync(NewAnnouncementRequest("Deleted Then Edited " + Guid.NewGuid()));
        await adminSession.Admin.DeleteSiteAnnouncementAsync(new DeleteSiteAnnouncementRequest
        {
            AnnouncementID = Guid.Parse(created.Id)
        });

        ApiException? ex = null;
        try
        {
            await adminSession.Admin.EditSiteAnnouncementAsync(new EditSiteAnnouncementRequest
            {
                AnnouncementID = Guid.Parse(created.Id),
                Title = "Should not work",
                Body = "Should not work",
                PostedAt = DateTimeOffset.UtcNow
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Expected ApiException when editing a deleted announcement.");
        Assert.That(ex!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task CreateSiteAnnouncement_WithBlankTitle_Returns400()
    {
        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        ApiException? ex = null;
        try
        {
            await adminSession.Admin.CreateSiteAnnouncementAsync(new CreateSiteAnnouncementRequest
            {
                Title = "   ",
                Body = "A body."
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Expected ApiException for a blank title.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateSiteAnnouncement_AsNonAdmin_Returns403()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        ApiException? ex = null;
        try
        {
            await session.Admin.CreateSiteAnnouncementAsync(NewAnnouncementRequest("Not Allowed " + Guid.NewGuid()));
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Expected ApiException for a non-admin user.");
        Assert.That(ex!.StatusCode, Is.EqualTo(403));
    }
}
