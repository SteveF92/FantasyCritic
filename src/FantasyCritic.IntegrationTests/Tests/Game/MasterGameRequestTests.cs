using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Game;

[TestFixture]
public class MasterGameRequestTests : IntegrationTestBase
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

    // ── User side ─────────────────────────────────────────────────────────

    [Test]
    public async Task CreateMasterGameRequest_AppearsInMyRequests()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var gameName = $"Request Test {Guid.NewGuid():N}"[..36];
        await session.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = gameName,
            EstimatedReleaseDate = "2099",
            RequestNote = "Integration test request.",
        });

        var requests = await session.Game.MyMasterGameRequestsAsync();
        Assert.That(requests.Any(r => r.GameName == gameName), Is.True,
            "Created request should appear in MyMasterGameRequests.");
    }

    [Test]
    public async Task DeleteMasterGameRequest_DisappearsFromMyRequests()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var gameName = $"Delete Test {Guid.NewGuid():N}"[..36];
        await session.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = gameName,
            EstimatedReleaseDate = "2099",
            RequestNote = "Will be deleted.",
        });

        var requests = await session.Game.MyMasterGameRequestsAsync();
        var requestID = requests.Single(r => r.GameName == gameName).RequestID;

        await session.Game.DeleteMasterGameRequestAsync(new MasterGameRequestDeletionRequest
        {
            RequestID = requestID,
        });

        var afterDelete = await session.Game.MyMasterGameRequestsAsync();
        Assert.That(afterDelete.Any(r => r.RequestID == requestID), Is.False,
            "Deleted request should no longer appear in MyMasterGameRequests.");
    }

    [Test]
    public async Task DismissMasterGameRequest_SetsHiddenFlag()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var gameName = $"Dismiss Test {Guid.NewGuid():N}"[..36];
        await session.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = gameName,
            EstimatedReleaseDate = "2099",
            RequestNote = "Will be dismissed.",
        });

        var requests = await session.Game.MyMasterGameRequestsAsync();
        var requestID = requests.Single(r => r.GameName == gameName).RequestID;

        await session.Game.DismissMasterGameRequestAsync(new MasterGameRequestDismissRequest
        {
            RequestID = requestID,
        });

        var afterDismiss = await session.Game.MyMasterGameRequestsAsync();
        Assert.That(afterDismiss.Any(r => r.RequestID == requestID), Is.False,
            "Dismissed request should no longer appear in MyMasterGameRequests.");
    }

    [Test]
    public async Task DeleteMasterGameRequest_NotOwner_Returns403()
    {
        var (emailA, passwordA, displayNameA) = NewUser();
        using var sessionA = new ApiSession(Factory);
        await sessionA.RegisterAsync(emailA, passwordA, displayNameA);

        var gameName = $"Other User Request {Guid.NewGuid():N}"[..36];
        await sessionA.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = gameName,
            EstimatedReleaseDate = "2099",
            RequestNote = "Owned by User A.",
        });

        var requestsA = await sessionA.Game.MyMasterGameRequestsAsync();
        var requestID = requestsA.Single(r => r.GameName == gameName).RequestID;

        var (emailB, passwordB, displayNameB) = NewUser();
        using var sessionB = new ApiSession(Factory);
        await sessionB.RegisterAsync(emailB, passwordB, displayNameB);

        ApiException? ex = null;
        try
        {
            await sessionB.Game.DeleteMasterGameRequestAsync(new MasterGameRequestDeletionRequest
            {
                RequestID = requestID,
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for non-owner delete.");
        Assert.That(ex!.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task DeleteMasterGameRequest_UnknownID_Returns400()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        ApiException? ex = null;
        try
        {
            await session.Game.DeleteMasterGameRequestAsync(new MasterGameRequestDeletionRequest
            {
                RequestID = Guid.NewGuid(),
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unknown request ID.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateMasterGameRequest_Unauthenticated_Returns401()
    {
        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
            {
                GameName = "Some Game",
                EstimatedReleaseDate = "2099",
                RequestNote = "Should be rejected.",
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unauthenticated request.");
        Assert.That(ex!.StatusCode, Is.EqualTo(401));
    }

    // ── FactChecker side (moved from FactCheckerTests) ────────────────────

    [Test]
    public async Task ApproveMasterGameRequest_WithLinkedGame_Succeeds()
    {
        var (requesterEmail, requesterPassword, requesterDisplayName) = NewUser();
        using var requesterSession = new ApiSession(Factory);
        await requesterSession.RegisterAsync(requesterEmail, requesterPassword, requesterDisplayName);

        var requestedGameName = $"Silksong Alt {Guid.NewGuid():N}"[..36];
        await requesterSession.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = requestedGameName,
            EstimatedReleaseDate = "2027",
            RequestNote = "Highly anticipated Metroidvania sequel from Team Cherry. Confirmed to be in development; expected to release in the near future. Steam page exists, significant community demand.",
        });

        var requests = await requesterSession.Game.MyMasterGameRequestsAsync();
        var requestID = requests.Single(r => r.GameName == requestedGameName).RequestID;

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
        var requestID = requests.Single(r => r.GameName == requestedGameName).RequestID;

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
