using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Game;

[TestFixture]
public class MasterGameChangeRequestTests : IntegrationTestBase
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

    /// <summary>
    /// Returns the ID of any master game in the seed DB.
    /// Change request tests need a real MasterGameID as a target.
    /// </summary>
    private async Task<Guid> GetAnyMasterGameIDAsync()
    {
        using var session = new ApiSession(Factory);
        var games = await session.Game.MasterGameAllAsync();
        return games.First().MasterGameID;
    }

    // ── User side ─────────────────────────────────────────────────────────

    [Test]
    public async Task CreateMasterGameChangeRequest_AppearsInMyChangeRequests()
    {
        var masterGameID = await GetAnyMasterGameIDAsync();

        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        await session.Game.CreateMasterGameChangeRequestAsync(new MasterGameChangeRequestRequest
        {
            MasterGameID = masterGameID,
            RequestNote = "The release date needs updating.",
        });

        var changeRequests = await session.Game.MyMasterGameChangeRequestsAsync();
        Assert.That(changeRequests.Any(r => r.MasterGame.MasterGameID == masterGameID), Is.True,
            "Created change request should appear in MyMasterGameChangeRequests.");
    }

    [Test]
    public async Task DeleteMasterGameChangeRequest_DisappearsFromMyChangeRequests()
    {
        var masterGameID = await GetAnyMasterGameIDAsync();

        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        await session.Game.CreateMasterGameChangeRequestAsync(new MasterGameChangeRequestRequest
        {
            MasterGameID = masterGameID,
            RequestNote = "Will be deleted.",
        });

        var changeRequests = await session.Game.MyMasterGameChangeRequestsAsync();
        var requestID = changeRequests.Single(r => r.MasterGame.MasterGameID == masterGameID).RequestID;

        await session.Game.DeleteMasterGameChangeRequestAsync(new MasterGameChangeRequestDeletionRequest
        {
            RequestID = requestID,
        });

        var afterDelete = await session.Game.MyMasterGameChangeRequestsAsync();
        Assert.That(afterDelete.Any(r => r.RequestID == requestID), Is.False,
            "Deleted change request should no longer appear in MyMasterGameChangeRequests.");
    }

    [Test]
    public async Task DismissMasterGameChangeRequest_SetsHiddenFlag()
    {
        var masterGameID = await GetAnyMasterGameIDAsync();

        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        await session.Game.CreateMasterGameChangeRequestAsync(new MasterGameChangeRequestRequest
        {
            MasterGameID = masterGameID,
            RequestNote = "Will be dismissed.",
        });

        var changeRequests = await session.Game.MyMasterGameChangeRequestsAsync();
        var requestID = changeRequests.Single(r => r.MasterGame.MasterGameID == masterGameID).RequestID;

        await session.Game.DismissMasterGameChangeRequestAsync(new MasterGameChangeRequestDismissRequest
        {
            RequestID = requestID,
        });

        var afterDismiss = await session.Game.MyMasterGameChangeRequestsAsync();
        Assert.That(afterDismiss.Any(r => r.RequestID == requestID), Is.False,
            "Dismissed change request should no longer appear in MyMasterGameChangeRequests.");
    }

    [Test]
    public async Task DeleteMasterGameChangeRequest_NotOwner_Returns403()
    {
        var masterGameID = await GetAnyMasterGameIDAsync();

        var (emailA, passwordA, displayNameA) = NewUser();
        using var sessionA = new ApiSession(Factory);
        await sessionA.RegisterAsync(emailA, passwordA, displayNameA);

        await sessionA.Game.CreateMasterGameChangeRequestAsync(new MasterGameChangeRequestRequest
        {
            MasterGameID = masterGameID,
            RequestNote = "Owned by User A.",
        });

        var requestsA = await sessionA.Game.MyMasterGameChangeRequestsAsync();
        var requestID = requestsA.Single(r => r.MasterGame.MasterGameID == masterGameID).RequestID;

        var (emailB, passwordB, displayNameB) = NewUser();
        using var sessionB = new ApiSession(Factory);
        await sessionB.RegisterAsync(emailB, passwordB, displayNameB);

        ApiException? ex = null;
        try
        {
            await sessionB.Game.DeleteMasterGameChangeRequestAsync(new MasterGameChangeRequestDeletionRequest
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
    public async Task DeleteMasterGameChangeRequest_UnknownID_Returns400()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        ApiException? ex = null;
        try
        {
            await session.Game.DeleteMasterGameChangeRequestAsync(new MasterGameChangeRequestDeletionRequest
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
    public async Task CreateMasterGameChangeRequest_Unauthenticated_Returns401()
    {
        var masterGameID = await GetAnyMasterGameIDAsync();

        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.CreateMasterGameChangeRequestAsync(new MasterGameChangeRequestRequest
            {
                MasterGameID = masterGameID,
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

    // ── FactChecker side ──────────────────────────────────────────────────

    [Test]
    public async Task CompleteMasterGameChangeRequest_Succeeds()
    {
        var masterGameID = await GetAnyMasterGameIDAsync();

        var (requesterEmail, requesterPassword, requesterDisplayName) = NewUser();
        using var requesterSession = new ApiSession(Factory);
        await requesterSession.RegisterAsync(requesterEmail, requesterPassword, requesterDisplayName);

        await requesterSession.Game.CreateMasterGameChangeRequestAsync(new MasterGameChangeRequestRequest
        {
            MasterGameID = masterGameID,
            RequestNote = "Please update the release date.",
        });

        var requests = await requesterSession.Game.MyMasterGameChangeRequestsAsync();
        var requestID = requests.Single(r => r.MasterGame.MasterGameID == masterGameID).RequestID;

        var (fcEmail, fcPassword, fcDisplayName) = NewUser();
        using var fcRegSession = new ApiSession(Factory);
        await fcRegSession.RegisterAsync(fcEmail, fcPassword, fcDisplayName);
        var fcMe = await fcRegSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(fcMe.UserID);

        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(fcEmail, fcPassword);

        await fcSession.FactChecker.CompleteMasterGameChangeRequestAsync(new CompleteMasterGameChangeRequestRequest
        {
            RequestID = requestID,
            ResponseNote = "Reviewed and noted.",
        });

        var updatedRequests = await requesterSession.Game.MyMasterGameChangeRequestsAsync();
        var answeredRequest = updatedRequests.Single(r => r.RequestID == requestID);

        Assert.That(answeredRequest.Answered, Is.True,
            "Change request should be answered after FactChecker completion.");
    }
}
