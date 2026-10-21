using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;
using static FantasyCritic.IntegrationTests.Helpers.SlotTestHelpers;

namespace FantasyCritic.IntegrationTests.Tests.League.Slots;

/// <summary>
/// Which slots a draft pick and processed pickup bids land in. A conditional drop's game gives its slot to the won
/// game, including a special slot; a bid without a drop takes the least permissive open slot it fits; a
/// counter-pick bid takes a counter-pick slot.
/// </summary>
[TestFixture]
public class PickupBidSlotTests : IntegrationTestBase
{
    // Slots 0-1 regular, slot 2 requires New Game; one counter-pick slot. Each publisher drafts one game.
    private static readonly LeagueScenario OneSpecialSlot = Scenario("PickupBidSlots", 3, 1, [[NewGame]], gamesToDraft: 1);

    private ApiSession _adminSession = null!;
    private LeagueFixture _league = null!;
    private LeagueYearViewModel _afterDraft = null!;
    private LeagueYearViewModel _afterProcessing = null!;
    private Dictionary<string, MasterGameYearViewModel> _games = null!;

    private TestPublisher First => _league.Publishers[0];
    private TestPublisher Second => _league.Publishers[1];
    private TestPublisher Third => _league.Publishers[2];

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);
        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 12, 0, 0, TimeSpan.Zero)
        });

        _league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, OneSpecialSlot, NewUser);
        string[] remakes = ["Remake1", "Remake2", "Remake3", "Remake4", "Remake5", "Remake6"];
        string[] newGames = ["NewGame0", "NewGame1", "NewGame2", "NewGame3", "NewGame4"];
        var tagged = await GetTaggedGamesAsync(league: _league,
            remakes.Select(_ => Remake).Concat(newGames.Select(_ => NewGame)).ToArray());
        _games = remakes.Concat(newGames).Zip(tagged).ToDictionary(x => x.First, x => x.Second);

        // Before the draft: the first publisher has slots 0 and 1 filled, the second slot 0.
        await ClaimAsync(_league, First, _games["Remake1"]);
        await ClaimAsync(_league, First, _games["Remake2"]);
        await ClaimAsync(_league, Second, _games["Remake3"]);

        await _league.Manager.LeagueManager.StartDraftAsync(new StartDraftRequest { LeagueID = _league.LeagueID, Year = _league.Year });
        await DraftAsync(new Dictionary<Guid, MasterGameYearViewModel>
        {
            [First.PublisherID] = _games["NewGame1"],
            [Second.PublisherID] = _games["Remake4"],
            [Third.PublisherID] = _games["NewGame0"],
        });
        _afterDraft = await _league.GetLeagueYearAsync();

        await LeaguePickupActions.PlaceBidAsync(First, _games["Remake5"].MasterGameID, 1, false, GameID(_afterDraft, First, "Remake2"));
        await LeaguePickupActions.PlaceBidAsync(First, _games["NewGame3"].MasterGameID, 1, false, GameID(_afterDraft, First, "NewGame1"));
        await LeaguePickupActions.PlaceBidAsync(Second, _games["NewGame2"].MasterGameID, 1, false);
        await LeaguePickupActions.PlaceBidAsync(Second, _games["Remake1"].MasterGameID, 1, true);
        await LeaguePickupActions.PlaceBidAsync(Third, _games["Remake6"].MasterGameID, 1, false);
        await LeaguePickupActions.PlaceBidAsync(Third, _games["NewGame4"].MasterGameID, 1, false);

        await _adminSession.Admin.SetTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 12, 1, 1, 0, TimeSpan.Zero)
        });
        await _adminSession.ActionRunner.TurnOnActionProcessingModeAsync();
        var processActionsJob = await _adminSession.ActionRunner.ProcessActionsAsync();
        await JobTestHelpers.RunQueuedJobAsync(Factory, processActionsJob.JobID);
        await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();

        _afterProcessing = await _league.GetLeagueYearAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        if (_adminSession != null)
        {
            await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();
            await _adminSession.Admin.ResetTimeAsync();
        }
        _adminSession?.Dispose();
        if (_league != null)
        {
            await _league.DisposeAsync();
        }
    }

    [Test]
    public void Draft_PutsEachPickInTheLeastPermissiveSlotItFits()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Placement(_afterDraft, First, "NewGame1"), Is.EqualTo("2 (special 0: NewGame)"));
            Assert.That(Placement(_afterDraft, Second, "Remake4"), Is.EqualTo("1"));
            Assert.That(Placement(_afterDraft, Third, "NewGame0"), Is.EqualTo("2 (special 0: NewGame)"),
                "With every slot open, a New Game is drafted into the New Game slot.");
        }

        AssertSlotsAreConsistent(_afterDraft);
    }

    [Test]
    public void ConditionalDrop_TheWonGameTakesTheDroppedGamesSlot()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Placement(_afterProcessing, First, "Remake5"), Is.EqualTo("1"));
            Assert.That(HasGame(_afterProcessing, First, "Remake2"), Is.False);
        }
    }

    [Test]
    public void ConditionalDropOfTheSpecialSlotGame_TheWonGameTakesTheSpecialSlot()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Placement(_afterProcessing, First, "NewGame3"), Is.EqualTo("2 (special 0: NewGame)"));
            Assert.That(HasGame(_afterProcessing, First, "NewGame1"), Is.False);
            Assert.That(Placement(_afterProcessing, First, "Remake1"), Is.EqualTo("0"));
        }
    }

    [Test]
    public void Bid_WithOnlyTheSpecialSlotOpen_TakesIt()
    {
        Assert.That(Placement(_afterProcessing, Second, "NewGame2"), Is.EqualTo("2 (special 0: NewGame)"));
    }

    [Test]
    public void Bids_WithRegularAndSpecialSlotsOpen_EachTakesTheLeastPermissiveSlotItFits()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Placement(_afterProcessing, Third, "Remake6"), Is.EqualTo("0"));
            Assert.That(Placement(_afterProcessing, Third, "NewGame0"), Is.EqualTo("2 (special 0: NewGame)"));
            Assert.That(Placement(_afterProcessing, Third, "NewGame4"), Is.EqualTo("1"),
                "The New Game slot was taken in the draft, so the New Game bid lands in the open regular slot.");
        }
    }

    [Test]
    public void CounterPickBid_TakesTheCounterPickSlot()
    {
        Assert.That(Placement(_afterProcessing, Second, "Remake1", counterPick: true), Is.EqualTo("CP 0"));
    }

    [Test]
    public void AfterProcessing_SlotsAreConsistent()
    {
        AssertSlotsAreConsistent(_afterProcessing);
    }

    private async Task DraftAsync(IReadOnlyDictionary<Guid, MasterGameYearViewModel> picks)
    {
        while (true)
        {
            var leagueYear = await _league.GetLeagueYearAsync();
            if (!leagueYear.Drafts.Any(d => d.PlayStatus == "Drafting" || d.PlayStatus == "DraftPaused"))
            {
                return;
            }

            var next = leagueYear.Publishers.Single(p => p.NextToDraft);
            var game = picks[next.PublisherID];
            var result = await _league.PublisherSessionMap[next.PublisherID].League.DraftGameAsync(new DraftGameRequest
            {
                PublisherID = next.PublisherID,
                MasterGameID = game.MasterGameID,
                GameName = game.GameName,
                CounterPick = false,
                AllowIneligibleSlot = false,
            });
            Assert.That(result.Success, Is.True, $"Drafting '{game.GameName}' failed: {string.Join("; ", result.Errors)}");
        }
    }

    private string Placement(LeagueYearViewModel leagueYear, TestPublisher publisher, string game, bool counterPick = false)
    {
        var slot = GetPublisher(leagueYear, publisher.PublisherID).GameSlots.SingleOrDefault(s =>
            s.CounterPick == counterPick && s.PublisherGame?.MasterGame?.MasterGameID == _games[game].MasterGameID);
        return slot is null ? "not on the roster" : DescribeSlot(slot);
    }

    private bool HasGame(LeagueYearViewModel leagueYear, TestPublisher publisher, string game) =>
        GetPublisher(leagueYear, publisher.PublisherID).Games.Any(g => g.MasterGame?.MasterGameID == _games[game].MasterGameID);

    private Guid GameID(LeagueYearViewModel leagueYear, TestPublisher publisher, string game) =>
        GetPublisher(leagueYear, publisher.PublisherID).Games.Single(g => g.MasterGame?.MasterGameID == _games[game].MasterGameID).PublisherGameID;
}
