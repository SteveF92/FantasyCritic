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
        Assert.That(result.MinimumReleaseDate, Is.Not.Null);
        Assert.That(result.MaximumReleaseDate, Is.Not.Null);
        Assert.That(result.MinimumReleaseDate!.Value.Date, Is.EqualTo(new DateTime(2027, 4, 1)));
        Assert.That(result.MaximumReleaseDate!.Value.Date, Is.EqualTo(new DateTime(2027, 6, 30)));
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
            Tags = new[] { "NewGame" },
        });

        Assert.That(created, Is.Not.Null);
        Assert.That(created.MasterGameID, Is.Not.EqualTo(Guid.Empty));
        Assert.That(created.GameName, Is.EqualTo(gameName));

        var retrieved = await fcSession.Game.MasterGameAsync(created.MasterGameID);
        Assert.That(retrieved.GameName, Is.EqualTo(gameName));
    }
}
