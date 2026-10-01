using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

/// <summary>
/// Action processing refuses to run while a game with an unprocessed bid has an unanswered change request,
/// since a missed correction could make the bid invalid.
/// </summary>
[TestFixture]
public class PendingCorrectionsCheckTests : IntegrationTestBase
{
    private ApiSession _adminSession = null!;
    private LeagueFixture? _league;
    private Guid? _changeRequestID;

    [SetUp]
    public async Task SetUp()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);

        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 12, 0, 0, TimeSpan.Zero)
        });
    }

    //The check covers every league, so an unanswered request left behind would block every later test that processes actions.
    [TearDown]
    public async Task TearDown()
    {
        if (_changeRequestID is not null)
        {
            await _adminSession.FactChecker.CompleteMasterGameChangeRequestAsync(new CompleteMasterGameChangeRequestRequest
            {
                RequestID = _changeRequestID.Value,
                ResponseNote = "Answered by test teardown.",
            });
            _changeRequestID = null;
        }

        await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();
        await _adminSession.Admin.ResetTimeAsync();
        _adminSession.Dispose();

        if (_league != null)
        {
            await _league.DisposeAsync();
            _league = null;
        }
    }

    [Test]
    public async Task UnansweredChangeRequestOnBidGame_ProcessActionsRefused()
    {
        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, LeagueScenarios.FourPlayerBidding, NewUser);
        await _league.DraftToCompletionAsync();

        var publisher = _league.Publishers[0];
        var available = await publisher.Session.League.TopAvailableGamesAsync(_league.Year, _league.LeagueID, publisher.PublisherID, null);
        var bidGame = available.First(g => g.IsAvailable && !g.Taken && !g.IsReleased).MasterGame;
        await LeaguePickupActions.PlaceBidAsync(publisher, bidGame.MasterGameID, 10, false);

        var (email, password, displayName) = NewUser();
        using var requesterSession = new ApiSession(Factory);
        await requesterSession.RegisterAsync(email, password, displayName);
        await requesterSession.Game.CreateMasterGameChangeRequestAsync(new MasterGameChangeRequestRequest
        {
            MasterGameID = bidGame.MasterGameID,
            RequestNote = "The release date is wrong.",
        });
        var myRequests = await requesterSession.Game.MyMasterGameChangeRequestsAsync();
        _changeRequestID = myRequests.Single(r => r.MasterGame.MasterGameID == bidGame.MasterGameID).RequestID;

        await _adminSession.Admin.SetTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 12, 1, 1, 0, TimeSpan.Zero)
        });
        await _adminSession.ActionRunner.TurnOnActionProcessingModeAsync();

        var exception = Assert.CatchAsync<ApiException>(() => _adminSession.ActionRunner.ProcessActionsAsync());

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(400));
            Assert.That(exception.Response, Does.Contain(bidGame.GameName));
        });
    }
}
