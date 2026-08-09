using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Game;

[TestFixture]
public class GameTests : IntegrationTestBase
{
    // ── Smoke: public read endpoints ──────────────────────────────────────

    [Test]
    public async Task SupportedYears_ReturnsNonEmptyList()
    {
        using var session = new ApiSession(Factory);
        var result = await session.Game.SupportedYearsAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetMasterGameTags_ReturnsNonEmptyList()
    {
        using var session = new ApiSession(Factory);
        var result = await session.Game.GetMasterGameTagsAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task MasterGameAll_ReturnsNonEmptyList()
    {
        using var session = new ApiSession(Factory);
        var result = await session.Game.MasterGameAllAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    // ── Not-found cases ───────────────────────────────────────────────────

    [Test]
    public async Task MasterGame_UnknownID_Returns404()
    {
        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.MasterGameAsync(Guid.NewGuid());
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unknown game ID.");
        Assert.That(ex!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task MasterGameChangeLog_UnknownID_Returns404()
    {
        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.MasterGameChangeLogAsync(Guid.NewGuid());
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unknown game ID.");
        Assert.That(ex!.StatusCode, Is.EqualTo(404));
    }

    // ── Auth edge cases ───────────────────────────────────────────────────

    [Test]
    public async Task MyMasterGameRequests_Unauthenticated_Returns401()
    {
        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.MyMasterGameRequestsAsync();
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unauthenticated request.");
        Assert.That(ex!.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task MyMasterGameChangeRequests_Unauthenticated_Returns401()
    {
        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.MyMasterGameChangeRequestsAsync();
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unauthenticated request.");
        Assert.That(ex!.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task LeagueYearsWithMasterGame_Unauthenticated_ReturnsEmptyList()
    {
        using var session = new ApiSession(Factory);
        var result = await session.Game.LeagueYearsWithMasterGameAsync(Guid.NewGuid());
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
    }

    private async Task GrantFactCheckerRoleAsync(Guid userID)
    {
        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);
        await adminSession.Admin.GrantRoleAsync(new UserRoleRequest
        {
            UserID = userID,
            RoleName = "FactChecker",
        });
    }

    [Test]
    public async Task GetRecentMasterGameChanges_IncludesNewlyAddedGame()
    {
        var (email, password, displayName) = NewUser();
        using var regSession = new ApiSession(Factory);
        await regSession.RegisterAsync(email, password, displayName);
        var me = await regSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(me.UserID);

        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(email, password);

        var gameName = $"Recent Add {Guid.NewGuid():N}"[..36];
        var created = await fcSession.FactChecker.CreateMasterGameAsync(new CreateMasterGameRequest
        {
            GameName = gameName,
            EstimatedReleaseDate = "2099",
            Tags = ["NewGame"],
        });

        var recentChanges = await fcSession.Game.GetRecentMasterGameChangesAsync(RecentMasterGameChangeMode.All);

        Assert.That(recentChanges, Is.Not.Null);
        var addEntry = recentChanges.SingleOrDefault(x =>
            x.MasterGame.MasterGameID == created.MasterGameID
            && x.Change.Description == "Game added");

        Assert.That(addEntry, Is.Not.Null,
            "Newly created master game should appear in recent changes with description 'Game added'.");
        Assert.That(addEntry!.Change.ChangedByUser.UserID, Is.EqualTo(me.UserID));
    }

    [Test]
    public async Task GetRecentMasterGameChanges_NewGamesMode_IncludesNewlyAddedGame()
    {
        var (email, password, displayName) = NewUser();
        using var regSession = new ApiSession(Factory);
        await regSession.RegisterAsync(email, password, displayName);
        var me = await regSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(me.UserID);

        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(email, password);

        var gameName = $"NewGames Mode {Guid.NewGuid():N}"[..36];
        var created = await fcSession.FactChecker.CreateMasterGameAsync(new CreateMasterGameRequest
        {
            GameName = gameName,
            EstimatedReleaseDate = "2099",
            Tags = ["NewGame"],
        });

        var recentChanges = await fcSession.Game.GetRecentMasterGameChangesAsync(RecentMasterGameChangeMode.NewGames);

        Assert.That(recentChanges, Is.Not.Null);
        var addEntry = recentChanges.SingleOrDefault(x =>
            x.MasterGame.MasterGameID == created.MasterGameID
            && x.Change.Description == "Game added");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(addEntry, Is.Not.Null,
                "NewGames mode should include newly created master game with description 'Game added'.");
            Assert.That(recentChanges.All(x => x.Change.Description == "Game added"), Is.True,
                "NewGames mode should only return Game added rows.");
        }
    }

    [Test]
    public async Task GetRecentMasterGameChanges_ChangesMode_ExcludesNewlyAddedGame()
    {
        var (email, password, displayName) = NewUser();
        using var regSession = new ApiSession(Factory);
        await regSession.RegisterAsync(email, password, displayName);
        var me = await regSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(me.UserID);

        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(email, password);

        var gameName = $"Changes Mode {Guid.NewGuid():N}"[..36];
        var created = await fcSession.FactChecker.CreateMasterGameAsync(new CreateMasterGameRequest
        {
            GameName = gameName,
            EstimatedReleaseDate = "2099",
            Tags = ["NewGame"],
        });

        var recentChanges = await fcSession.Game.GetRecentMasterGameChangesAsync(RecentMasterGameChangeMode.Changes);

        Assert.That(recentChanges, Is.Not.Null);
        var addEntry = recentChanges.SingleOrDefault(x =>
            x.MasterGame.MasterGameID == created.MasterGameID
            && x.Change.Description == "Game added");

        Assert.That(addEntry, Is.Null,
            "Changes mode must not include synthetic Game added rows for newly created games.");
    }
}
