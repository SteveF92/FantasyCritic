using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.LeagueManager;

/// <summary>
/// A manager removing special slots after the draft, with the standard game count unchanged. Special slots are the
/// last slots of the roster, so dropping two of four turns slots 4-5 into normal slots, and the games that sat in
/// the removed special slots (6-7) have to land somewhere inside the 8 standard slots.
/// </summary>
[TestFixture]
public class SpecialSlotRemovalTests : IntegrationTestBase
{
    private static readonly LeagueScenario FourSpecialSlots = new()
    {
        Name = "FourSpecialSlots",
        PlayerCount = 2,
        StandardGames = 8,
        GamesToDraft = 8,
        CounterPicks = 0,
        CounterPicksToDraft = 0,
        UnrestrictedReleaseStatusDroppableGames = 0,
        WillNotReleaseDroppableGames = 0,
        WillReleaseDroppableGames = 0,
        DropOnlyDraftGames = true,
        GrantSuperDrops = false,
        CounterPicksBlockDrops = true,
        AllowMoveIntoIneligible = false,
        MinimumBidAmount = 0,
        EnableBids = false,
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "NoTrades",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "OnlyNeedsScore",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
        HasSpecialSlots = true,
        SpecialGameSlots =
        [
            new SpecialGameSlotViewModel { SpecialSlotPosition = 0, RequiredTags = ["NewGame"] },
            new SpecialGameSlotViewModel { SpecialSlotPosition = 1, RequiredTags = ["NewGame"] },
            new SpecialGameSlotViewModel { SpecialSlotPosition = 2, RequiredTags = ["NewGame"] },
            new SpecialGameSlotViewModel { SpecialSlotPosition = 3, RequiredTags = ["NewGame"] },
        ],
    };

    [Test]
    public async Task EditLeagueYearSettings_RemovingSpecialSlots_KeepsEveryGameInARealSlot()
    {
        await using var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, FourSpecialSlots, NewUser);
        await league.DraftToCompletionAsync();
        AssertEveryGameIsInARealSlot(await league.GetLeagueYearAsync());

        await RemoveSpecialSlotsAsync(league, keep: 2);

        var after = await league.GetLeagueYearAsync();
        Assert.That(after.Settings.SpecialGameSlots, Has.Count.EqualTo(2));
        AssertEveryGameIsInARealSlot(after);
    }

    [Test]
    public async Task EditLeagueYearSettings_AfterRemovingSpecialSlots_CanStillChangeStandardGames()
    {
        // Production error: "Cannot figure out slots for LeagueID: ..." once a league has games past its last slot.
        await using var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, FourSpecialSlots, NewUser);
        await league.DraftToCompletionAsync();

        await RemoveSpecialSlotsAsync(league, keep: 2);

        var settings = await league.Manager.League.GetLeagueYearOptionsAsync(league.LeagueID, league.Year);
        settings.StandardGames += 2;
        await league.Manager.LeagueManager.EditLeagueYearSettingsAsync(new EditLeagueYearRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            LeagueYearName = null,
            LeagueYearSettings = settings,
            FirstDraft = null,
        });

        var after = await league.GetLeagueYearAsync();
        Assert.That(after.Settings.StandardGames, Is.EqualTo(FourSpecialSlots.StandardGames + 2));
        AssertEveryGameIsInARealSlot(after);
    }

    private static async Task RemoveSpecialSlotsAsync(LeagueFixture league, int keep)
    {
        var settings = await league.Manager.League.GetLeagueYearOptionsAsync(league.LeagueID, league.Year);
        settings.SpecialGameSlots = settings.SpecialGameSlots.OrderBy(slot => slot.SpecialSlotPosition).Take(keep).ToList();
        await league.Manager.LeagueManager.EditLeagueYearSettingsAsync(new EditLeagueYearRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            LeagueYearName = null,
            LeagueYearSettings = settings,
            FirstDraft = null,
        });
    }

    private static void AssertEveryGameIsInARealSlot(LeagueYearViewModel leagueYear)
    {
        var standardGames = leagueYear.Settings.StandardGames;
        using (Assert.EnterMultipleScope())
        {
            foreach (var publisher in leagueYear.Publishers)
            {
                IReadOnlyList<PublisherGameViewModel> games = publisher.Games.Where(game => !game.CounterPick).ToList();
                var gamesInSlots = publisher.GameSlots.Count(slot => !slot.CounterPick && slot.PublisherGame is not null);

                foreach (var game in games)
                {
                    Assert.That(game.SlotNumber, Is.LessThan(standardGames),
                        $"{publisher.PublisherName}'s '{game.GameName}' is in slot {game.SlotNumber}, past the last of {standardGames} slots.");
                }

                Assert.That(gamesInSlots, Is.EqualTo(games.Count),
                    $"{publisher.PublisherName} has {games.Count} standard games but only {gamesInSlots} show in a slot.");
            }
        }
    }
}
