using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.FactChecker;

[TestFixture]
public class FactCheckerTests : IntegrationTestBase
{
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
    public async Task ParseEstimatedDate_ValidQuarterInput_ReturnsDateRange()
    {
        var (email, password, displayName) = NewUser();
        using var regSession = new ApiSession(Factory);
        await regSession.RegisterAsync(email, password, displayName);
        var me = await regSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(me.UserID);

        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(email, password);

        var result = await fcSession.FactChecker.ParseEstimatedDateAsync("Q2 2027");

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.MinimumReleaseDate, Is.Not.Null);
            Assert.That(result.MaximumReleaseDate, Is.Not.Null);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.MinimumReleaseDate!.Value.Date, Is.EqualTo(new DateTime(2027, 4, 1)));
            Assert.That(result.MaximumReleaseDate!.Value.Date, Is.EqualTo(new DateTime(2027, 6, 30)));
        }
    }

    [Test]
    public async Task CreateMasterGame_Succeeds_AndCanBeRetrievedViaGameController()
    {
        var (email, password, displayName) = NewUser();
        using var regSession = new ApiSession(Factory);
        await regSession.RegisterAsync(email, password, displayName);
        var me = await regSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(me.UserID);

        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(email, password);

        var gameName = $"Test Game {Guid.NewGuid():N}"[..36];

        var created = await fcSession.FactChecker.CreateMasterGameAsync(new CreateMasterGameRequest
        {
            GameName = gameName,
            EstimatedReleaseDate = "2099",
            Tags = ["NewGame"],
        });

        Assert.That(created, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(created.MasterGameID, Is.Not.EqualTo(Guid.Empty));
            Assert.That(created.GameName, Is.EqualTo(gameName));
        }

        var retrieved = await fcSession.Game.MasterGameAsync(created.MasterGameID);
        Assert.That(retrieved.GameName, Is.EqualTo(gameName));
    }

    [Test]
    public async Task PendingMasterGameUpdates_ListsANewGame_UntilItIsDeleted()
    {
        using var fcSession = await NewFactCheckerSessionAsync();
        var created = await CreateTestMasterGameAsync(fcSession);

        var pendingBefore = await fcSession.FactChecker.PendingMasterGameUpdatesAsync();
        var newGameUpdate = pendingBefore.NewGames.Single(x => x.MasterGame.MasterGameID == created.MasterGameID);
        Assert.That(newGameUpdate.MasterGame.GameName, Is.EqualTo(created.GameName));

        await fcSession.FactChecker.DeletePendingMasterGameUpdateAsync(new DeletePendingMasterGameUpdateRequest
        {
            MasterGameUpdateID = newGameUpdate.MasterGameUpdateID,
        });

        var pendingAfter = await fcSession.FactChecker.PendingMasterGameUpdatesAsync();
        Assert.That(pendingAfter.NewGames.Any(x => x.MasterGameUpdateID == newGameUpdate.MasterGameUpdateID), Is.False);
    }

    [Test]
    public async Task DeletePendingMasterGameUpdate_AlreadyDeleted_Returns400()
    {
        using var fcSession = await NewFactCheckerSessionAsync();
        var created = await CreateTestMasterGameAsync(fcSession);
        var pending = await fcSession.FactChecker.PendingMasterGameUpdatesAsync();
        var request = new DeletePendingMasterGameUpdateRequest
        {
            MasterGameUpdateID = pending.NewGames.Single(x => x.MasterGame.MasterGameID == created.MasterGameID).MasterGameUpdateID,
        };
        await fcSession.FactChecker.DeletePendingMasterGameUpdateAsync(request);

        var ex = Assert.ThrowsAsync<ApiException>(() => fcSession.FactChecker.DeletePendingMasterGameUpdateAsync(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(400), "Deleting an update that is no longer pending must return HTTP 400 Bad Request.");
    }

    private async Task<ApiSession> NewFactCheckerSessionAsync()
    {
        var (email, password, displayName) = NewUser();
        using var regSession = new ApiSession(Factory);
        await regSession.RegisterAsync(email, password, displayName);
        var me = await regSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(me.UserID);

        var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(email, password);
        return fcSession;
    }

    private static Task<MasterGameViewModel> CreateTestMasterGameAsync(ApiSession fcSession)
    {
        return fcSession.FactChecker.CreateMasterGameAsync(new CreateMasterGameRequest
        {
            GameName = $"Test Game {Guid.NewGuid():N}"[..36],
            EstimatedReleaseDate = "2099",
            Tags = ["NewGame"],
        });
    }
}
