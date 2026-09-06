using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

/// <summary>
/// Tests the per-game notes a publisher can keep on their watchlist (queued games).
/// </summary>
[TestFixture]
public class WatchlistNotesTests : IntegrationTestBase
{
    private LeagueFixture _league = null!;
    private TestPublisher _publisher = null!;
    private Guid _queuedMasterGameID;

    [OneTimeSetUp]
    public async Task SetUpWatchlist()
    {
        _league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(
            Factory, LeagueScenarios.Standard, NewUser);
        _publisher = _league.Publishers[0];

        var available = await _publisher.Session.League.TopAvailableGamesAsync(
            _league.Year, _league.LeagueID, _publisher.PublisherID, null);
        _queuedMasterGameID = available.First(g => g.IsAvailable && !g.IsReleased).MasterGame.MasterGameID;

        var queueResult = await _publisher.Session.League.AddGameToQueueAsync(new AddGameToQueueRequest
        {
            PublisherID = _publisher.PublisherID,
            MasterGameID = _queuedMasterGameID,
        });

        Assert.That(queueResult.Success, Is.True,
            $"Failed to queue game: {string.Join("; ", queueResult.Errors ?? [])}");
    }

    [OneTimeTearDown]
    public async Task TearDownSessions() => await _league.DisposeAsync();

    private async Task<QueuedGameViewModel> GetQueuedGameAsync()
    {
        var leagueYear = await _publisher.Session.League.GetLeagueYearAsync(_league.LeagueID, _league.Year, null);
        Assert.That(leagueYear.PrivatePublisherData, Is.Not.Null,
            "The publisher's own request should include their private publisher data.");
        return leagueYear.PrivatePublisherData!.QueuedGames
            .Single(x => x.MasterGame.MasterGameID == _queuedMasterGameID);
    }

    [Test, Order(1)]
    public async Task QueuedGame_WhenFirstAdded_HasNoNotes()
    {
        var queuedGame = await GetQueuedGameAsync();
        Assert.That(queuedGame.Notes, Is.Null);
    }

    [Test, Order(2)]
    public async Task SetQueuedGameNotes_WithNotes_PersistsThem()
    {
        await _publisher.Session.League.SetQueuedGameNotesAsync(new SetQueuedGameNotesRequest
        {
            PublisherID = _publisher.PublisherID,
            MasterGameID = _queuedMasterGameID,
            Notes = "Same studio as their last game, which reviewed well.",
        });

        var queuedGame = await GetQueuedGameAsync();
        Assert.That(queuedGame.Notes, Is.EqualTo("Same studio as their last game, which reviewed well."));
    }

    [Test, Order(3)]
    public async Task SetQueuedGameNotes_WithWhitespaceOnly_ClearsNotes()
    {
        await _publisher.Session.League.SetQueuedGameNotesAsync(new SetQueuedGameNotesRequest
        {
            PublisherID = _publisher.PublisherID,
            MasterGameID = _queuedMasterGameID,
            Notes = "   ",
        });

        var queuedGame = await GetQueuedGameAsync();
        Assert.That(queuedGame.Notes, Is.Null);
    }

    [Test, Order(4)]
    public async Task SetQueuedGameNotes_TooLong_Returns400()
    {
        ApiException? ex = null;
        try
        {
            await _publisher.Session.League.SetQueuedGameNotesAsync(new SetQueuedGameNotesRequest
            {
                PublisherID = _publisher.PublisherID,
                MasterGameID = _queuedMasterGameID,
                Notes = new string('a', 1001),
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Expected ApiException for notes over the maximum length.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    [Test, Order(5)]
    public async Task SetQueuedGameNotes_ForGameNotOnWatchlist_Returns400()
    {
        ApiException? ex = null;
        try
        {
            await _publisher.Session.League.SetQueuedGameNotesAsync(new SetQueuedGameNotesRequest
            {
                PublisherID = _publisher.PublisherID,
                MasterGameID = Guid.NewGuid(),
                Notes = "Notes for a game that isn't queued.",
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Expected ApiException for a game that is not on the watchlist.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }
}
