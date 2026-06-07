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

    [Test]
    public async Task ApproveMasterGameRequest_WithLinkedGame_Succeeds()
    {
        // Set up requester
        var (requesterEmail, requesterPassword, requesterDisplayName) = NewUser();
        using var requesterSession = new ApiSession(Factory);
        await requesterSession.RegisterAsync(requesterEmail, requesterPassword, requesterDisplayName);

        var requestedGameName = $"Hollow Knight Silksong Alt {Guid.NewGuid():N}"[..48];

        await requesterSession.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = requestedGameName,
            EstimatedReleaseDate = "2027",
            RequestNote = "Highly anticipated Metroidvania sequel from Team Cherry. Confirmed to be in development; expected to release in the near future. Steam page exists, significant community demand.",
        });

        var requests = await requesterSession.Game.MyMasterGameRequestsAsync();
        var request = requests.Single(r => r.GameName == requestedGameName);
        var requestID = request.RequestID;

        // Set up FactChecker
        var (fcEmail, fcPassword, fcDisplayName) = NewUser();
        using var fcRegSession = new ApiSession(Factory);
        await fcRegSession.RegisterAsync(fcEmail, fcPassword, fcDisplayName);
        var fcMe = await fcRegSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(fcMe.UserID);

        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(fcEmail, fcPassword);

        var createdGame = await fcSession.FactChecker.CreateMasterGameAsync(new CreateMasterGameRequest
        {
            GameName = requestedGameName,
            EstimatedReleaseDate = "2027",
            Tags = new[] { "NewGame" },
        });

        using var completionResult = await fcSession.FactChecker.CompleteMasterGameRequestAsync(new CompleteMasterGameRequestRequest
        {
            RequestID = requestID,
            ResponseNote = "Added to the database. Meets all eligibility requirements.",
            MasterGameID = createdGame.MasterGameID,
        });

        var updatedRequests = await requesterSession.Game.MyMasterGameRequestsAsync();
        var answeredRequest = updatedRequests.Single(r => r.RequestID == requestID);

        Assert.That(answeredRequest.Answered, Is.True);
        Assert.That(answeredRequest.MasterGame, Is.Not.Null);
        Assert.That(answeredRequest.MasterGame!.MasterGameID, Is.EqualTo(createdGame.MasterGameID));
    }

    [Test]
    public async Task DenyMasterGameRequest_NoLinkedGame_Succeeds()
    {
        // Set up requester
        var (requesterEmail, requesterPassword, requesterDisplayName) = NewUser();
        using var requesterSession = new ApiSession(Factory);
        await requesterSession.RegisterAsync(requesterEmail, requesterPassword, requesterDisplayName);

        var requestedGameName = $"Unknown Mobile Game {Guid.NewGuid():N}"[..40];

        await requesterSession.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = requestedGameName,
            EstimatedReleaseDate = "TBD",
            RequestNote = "idk some game i saw on tiktok",
        });

        var requests = await requesterSession.Game.MyMasterGameRequestsAsync();
        var request = requests.Single(r => r.GameName == requestedGameName);
        var requestID = request.RequestID;

        // Set up FactChecker
        var (fcEmail, fcPassword, fcDisplayName) = NewUser();
        using var fcRegSession = new ApiSession(Factory);
        await fcRegSession.RegisterAsync(fcEmail, fcPassword, fcDisplayName);
        var fcMe = await fcRegSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(fcMe.UserID);

        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(fcEmail, fcPassword);

        using var completionResult = await fcSession.FactChecker.CompleteMasterGameRequestAsync(new CompleteMasterGameRequestRequest
        {
            RequestID = requestID,
            ResponseNote = "Request too vague to act on. Please provide a game title, Steam link, or other identifying information.",
            MasterGameID = null,
        });

        var updatedRequests = await requesterSession.Game.MyMasterGameRequestsAsync();
        var answeredRequest = updatedRequests.Single(r => r.RequestID == requestID);

        Assert.That(answeredRequest.Answered, Is.True);
        Assert.That(answeredRequest.MasterGame, Is.Null);
    }
}
