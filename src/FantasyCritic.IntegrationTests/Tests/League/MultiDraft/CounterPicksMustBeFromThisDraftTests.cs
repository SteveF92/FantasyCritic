using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.MultiDraft;

/// <summary>
/// Integration tests for the per-draft <c>CounterPicksMustBeFromThisDraft</c> setting.
/// When enabled (default), Draft 2 counter picks may only target standard games drafted
/// during Draft 2's standard rounds — not games carried over from Draft 1.
/// </summary>
[TestFixture]
public class CounterPicksMustBeFromThisDraftTests : IntegrationTestBase
{
    private LeagueFixture _flagOnLeague = null!;
    private LeagueYearViewModel _flagOnSnapshot = null!;
    private TestPublisher _firstCounterPickPublisher = null!;
    private HashSet<Guid> _draft1StandardMasterGameIDs = null!;
    private HashSet<Guid> _draft2StandardMasterGameIDs = null!;

    [OneTimeSetUp]
    public async Task SetUpFlagOnLeagueAtDraft2CounterPickPhase()
    {
        (_flagOnLeague, _flagOnSnapshot, _firstCounterPickPublisher) =
            await CreateLeagueAtDraft2CounterPickPhaseAsync(counterPicksMustBeFromThisDraft: true);

        _draft1StandardMasterGameIDs = StandardMasterGameIDsForDraft(_flagOnSnapshot, draftNumber: 1);
        _draft2StandardMasterGameIDs = StandardMasterGameIDsForDraft(_flagOnSnapshot, draftNumber: 2);

        Assert.That(_draft1StandardMasterGameIDs, Is.Not.Empty, "Draft 1 should have standard games on rosters.");
        Assert.That(_draft2StandardMasterGameIDs, Is.Not.Empty, "Draft 2 should have standard games on rosters.");
        Assert.That(_flagOnSnapshot.Drafts.Single(d => d.DraftNumber == 2).CounterPicksMustBeFromThisDraft,
            Is.EqualTo(true));
    }

    [OneTimeTearDown]
    public async Task TearDownFlagOnLeague() => await _flagOnLeague.DisposeAsync();

    [Test]
    public async Task PossibleCounterPicks_FlagOn_ReturnsOnlyDraft2StandardGames()
    {
        var options = await _firstCounterPickPublisher.Session.League.PossibleCounterPicksAsync(
            _firstCounterPickPublisher.PublisherID);

        Assert.That(options, Is.Not.Empty,
            "PossibleCounterPicks should return at least one eligible target during Draft 2 counter-pick phase.");

        var returnedMasterGameIDs = options
            .Where(o => o.MasterGame != null)
            .Select(o => o.MasterGame!.MasterGameID)
            .ToHashSet();

        Assert.That(returnedMasterGameIDs, Is.SubsetOf(_draft2StandardMasterGameIDs),
            "All counter-pick options should be standard games drafted in Draft 2.");
        Assert.That(returnedMasterGameIDs.Overlaps(_draft1StandardMasterGameIDs), Is.False,
            "Draft 1 standard games must not appear in PossibleCounterPicks when the flag is on.");
    }

    [Test]
    public async Task DraftGame_FlagOn_CounterPickDraft1GameDuringDraft2_ReturnsFailure()
    {
        var draft1ID = _flagOnSnapshot.Drafts.Single(d => d.DraftNumber == 1).DraftID;
        var draft1Target = _flagOnSnapshot.Publishers
            .Where(p => p.PublisherID != _firstCounterPickPublisher.PublisherID)
            .SelectMany(p => p.Games)
            .First(g => !g.CounterPick && g.DraftID == draft1ID && g.MasterGame != null);

        var result = await _firstCounterPickPublisher.Session.League.DraftGameAsync(new DraftGameRequest
        {
            PublisherID = _firstCounterPickPublisher.PublisherID,
            MasterGameID = draft1Target.MasterGame!.MasterGameID,
            GameName = draft1Target.GameName,
            CounterPick = true,
            AllowIneligibleSlot = false,
        });

        Assert.That(result.Success, Is.False,
            "Counter-picking a Draft 1 standard game during Draft 2 should be rejected when the flag is on.");
        Assert.That(result.Errors, Is.Not.Empty);
    }

    [Test]
    public async Task PossibleCounterPicks_FlagOff_IncludesDraft1RosterGames()
    {
        var setup = await CreateLeagueAtDraft2CounterPickPhaseAsync(counterPicksMustBeFromThisDraft: false);
        await using var league = setup.League;

        var snapshot = setup.Snapshot;
        Assert.That(snapshot.Drafts.Single(d => d.DraftNumber == 2).CounterPicksMustBeFromThisDraft, Is.False);

        var draft1StandardMasterGameIDs = StandardMasterGameIDsForDraft(snapshot, draftNumber: 1);

        var options = await setup.FirstCounterPickPublisher.Session.League.PossibleCounterPicksAsync(
            setup.FirstCounterPickPublisher.PublisherID);

        Assert.That(options, Is.Not.Empty,
            "PossibleCounterPicks should return at least one eligible target when the flag is off.");

        var returnedMasterGameIDs = options
            .Where(o => o.MasterGame != null)
            .Select(o => o.MasterGame!.MasterGameID)
            .ToHashSet();

        Assert.That(returnedMasterGameIDs.Overlaps(draft1StandardMasterGameIDs), Is.True,
            "Draft 1 roster games should appear in PossibleCounterPicks when the flag is off.");
    }

    private async Task<(LeagueFixture League, LeagueYearViewModel Snapshot, TestPublisher FirstCounterPickPublisher)>
        CreateLeagueAtDraft2CounterPickPhaseAsync(bool counterPicksMustBeFromThisDraft)
    {
        var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, MultiDraftTestScenario.TwoPlayer, NewUser);
        await league.DraftToCompletionAsync();

        await league.Manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            Name = "Draft 2",
            ScheduledDate = null,
            GamesToDraft = 2,
            CounterPicksToDraft = 1,
            AdditionalStandardGames = 2,
            AdditionalCounterPicks = 1,
            NewSpecialGameSlots = new List<SpecialGameSlotViewModel>(),
            CounterPicksMustBeFromThisDraft = true,
        });

        var preSnapshot = await league.GetLeagueYearAsync();
        var draft2 = preSnapshot.Drafts.Single(d => d.DraftNumber == 2);

        if (!counterPicksMustBeFromThisDraft)
        {
            await league.Manager.LeagueManager.EditLeagueDraftAsync(new EditLeagueDraftRequest
            {
                DraftID = draft2.DraftID,
                LeagueID = league.LeagueID,
                Year = league.Year,
                Name = draft2.Name,
                ScheduledDate = draft2.ScheduledDate,
                GamesToDraft = draft2.GamesToDraft,
                CounterPicksToDraft = draft2.CounterPicksToDraft,
                CounterPicksMustBeFromThisDraft = false,
            });
        }

        await MultiDraftTestScenario.SetDraft2OrderAsync(league);
        await MultiDraftTestScenario.StartDraft2Async(league);
        await league.DraftUntilCounterPickPhaseAsync();

        var snapshot = await league.GetLeagueYearAsync();
        Assert.That(snapshot.ActiveDraft()?.DraftingCounterPicks, Is.True,
            "Setup should stop at Draft 2 counter-pick phase.");

        var nextPublisher = snapshot.Publishers.Single(p => p.NextToDraft);
        var firstCounterPickPublisher = league.Publishers.Single(p => p.PublisherID == nextPublisher.PublisherID);

        return (league, snapshot, firstCounterPickPublisher);
    }

    private static HashSet<Guid> StandardMasterGameIDsForDraft(LeagueYearViewModel snapshot, int draftNumber)
    {
        var draftID = snapshot.Drafts.Single(d => d.DraftNumber == draftNumber).DraftID;
        return snapshot.Publishers
            .SelectMany(p => p.Games)
            .Where(g => !g.CounterPick && g.DraftID == draftID && g.MasterGame != null)
            .Select(g => g.MasterGame!.MasterGameID)
            .ToHashSet();
    }
}
