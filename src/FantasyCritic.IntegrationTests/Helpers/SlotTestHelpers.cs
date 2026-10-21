using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// Helpers for tests that care which slot a game is in. Games get their tags from a league tag override, so a test
/// decides which games fit a special slot instead of depending on the seed data's tags.
/// </summary>
internal static class SlotTestHelpers
{
    public const string NewGame = "NewGame";
    public const string Remake = "Remake";
    public const string Remaster = "Remaster";

    /// <summary>
    /// A three-player league with the given roster. Each entry of <paramref name="specialSlotTags"/> is one special
    /// slot's required tags, in position order. Bids, trades and drops are on, so one shape serves every slot test.
    /// The draft takes every standard slot unless <paramref name="gamesToDraft"/> says otherwise.
    /// </summary>
    public static LeagueScenario Scenario(string name, int standardGames, int counterPicks, IReadOnlyList<string[]> specialSlotTags,
        int? gamesToDraft = null) =>
        new()
        {
            Name = name,
            PlayerCount = 3,
            StandardGames = standardGames,
            GamesToDraft = gamesToDraft ?? standardGames,
            CounterPicks = counterPicks,
            CounterPicksToDraft = 0,
            UnrestrictedReleaseStatusDroppableGames = 0,
            WillNotReleaseDroppableGames = 0,
            WillReleaseDroppableGames = 3,
            DropOnlyDraftGames = false,
            GrantSuperDrops = false,
            CounterPicksBlockDrops = false,
            AllowMoveIntoIneligible = false,
            MinimumBidAmount = 0,
            EnableBids = true,
            PickupSystem = "SemiPublicBiddingSecretCounterPicks",
            ScoringSystem = "LinearPositive",
            TradingSystem = "Standard",
            TiebreakSystem = "LowestProjectedPoints",
            ReleaseSystem = "MustBeReleased",
            IneligibleGameSystem = "DroppableAsWillNotRelease",
            HasSpecialSlots = specialSlotTags.Count > 0,
            SpecialGameSlots = specialSlotTags
                .Select((tags, position) => new SpecialGameSlotViewModel { SpecialSlotPosition = position, RequiredTags = tags.ToList() })
                .ToList(),
        };

    /// <summary>
    /// Returns <paramref name="count"/> distinct unreleased games that nobody in the league has yet.
    /// </summary>
    public static async Task<IReadOnlyList<MasterGameYearViewModel>> GetAvailableGamesAsync(LeagueFixture league, int count)
    {
        var available = await league.Manager.League.TopAvailableGamesAsync(
            league.Year, league.LeagueID, league.Publishers[0].PublisherID, null);
        var games = available
            .Where(g => g.IsAvailable && !g.Taken && !g.IsReleased)
            .Select(g => g.MasterGame)
            .Take(count)
            .ToList();
        if (games.Count < count)
        {
            throw new InvalidOperationException($"Only {games.Count} available games found, {count} needed.");
        }

        return games;
    }

    public static Task SetTagsAsync(LeagueFixture league, MasterGameYearViewModel game, params string[] tags) =>
        league.Manager.LeagueManager.SetGameTagOverrideAsync(new TagOverrideRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            MasterGameID = game.MasterGameID,
            Tags = tags.ToList(),
        });

    /// <summary>
    /// Gets unreleased games and gives each the tags listed for it, in order.
    /// </summary>
    public static async Task<IReadOnlyList<MasterGameYearViewModel>> GetTaggedGamesAsync(LeagueFixture league, params string[] tagPerGame)
    {
        var games = await GetAvailableGamesAsync(league, tagPerGame.Length);
        for (var i = 0; i < games.Count; i++)
        {
            await SetTagsAsync(league, games[i], tagPerGame[i]);
        }

        return games;
    }

    /// <summary>
    /// Manager claims <paramref name="game"/> for the publisher, fails the test if the claim is refused, and returns the
    /// publisher's new game.
    /// </summary>
    public static async Task<PublisherGameViewModel> ClaimAsync(LeagueFixture league, TestPublisher publisher, MasterGameYearViewModel game,
        bool counterPick = false)
    {
        var result = await TryClaimAsync(league, publisher, game, counterPick);
        Assert.That(result.Success, Is.True, $"Claiming '{game.GameName}' failed: {string.Join("; ", result.Errors)}");

        var leagueYear = await league.GetLeagueYearAsync();
        return GetPublisher(leagueYear, publisher.PublisherID).Games
            .Single(g => g.CounterPick == counterPick && g.MasterGame?.MasterGameID == game.MasterGameID);
    }

    public static Task<ManagerClaimResultViewModel> TryClaimAsync(LeagueFixture league, TestPublisher publisher, MasterGameYearViewModel game,
        bool counterPick = false) =>
        league.Manager.LeagueManager.ManagerClaimGameAsync(new ClaimGameRequest
        {
            PublisherID = publisher.PublisherID,
            GameName = game.GameName,
            MasterGameID = game.MasterGameID,
            CounterPick = counterPick,
            ManagerOverride = false,
            AllowIneligibleSlot = false,
        });

    public static async Task EditSettingsAsync(LeagueFixture league, Action<LeagueYearSettingsViewModel> change)
    {
        var settings = await league.Manager.League.GetLeagueYearOptionsAsync(league.LeagueID, league.Year);
        change(settings);
        await league.Manager.LeagueManager.EditLeagueYearSettingsAsync(new EditLeagueYearRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            LeagueYearName = null,
            LeagueYearSettings = settings,
            FirstDraft = null,
        });
    }

    public static PublisherViewModel GetPublisher(LeagueYearViewModel leagueYear, Guid publisherID) =>
        leagueYear.Publishers.Single(p => p.PublisherID == publisherID);

    public static PublisherSlotViewModel GetSlotOf(LeagueYearViewModel leagueYear, Guid publisherGameID) =>
        leagueYear.Publishers.SelectMany(p => p.GameSlots).Single(s => s.PublisherGame?.PublisherGameID == publisherGameID);

    public static PublisherSlotViewModel GetStandardSlot(PublisherViewModel publisher, int slotNumber) =>
        publisher.GameSlots.Single(s => !s.CounterPick && s.SlotNumber == slotNumber);

    /// <summary>
    /// Where every game sits, as "slot number, or 'CP n', plus the special slot position if any", keyed by publisher
    /// game ID. Two snapshots compare equal when nothing moved.
    /// </summary>
    public static IReadOnlyDictionary<Guid, string> GetPlacements(LeagueYearViewModel leagueYear) =>
        leagueYear.Publishers
            .SelectMany(p => p.GameSlots)
            .Where(s => s.PublisherGame is not null)
            .ToDictionary(s => s.PublisherGame!.PublisherGameID, DescribeSlot);

    public static string DescribeSlot(PublisherSlotViewModel slot)
    {
        var number = slot.CounterPick ? $"CP {slot.SlotNumber}" : slot.SlotNumber.ToString();
        if (slot.SpecialSlot is null)
        {
            return number;
        }

        return $"{number} (special {slot.SpecialSlot.SpecialSlotPosition}: {string.Join(",", slot.SpecialSlot.RequiredTags.Order())})";
    }

    /// <summary>
    /// Checks the slot fields of the API contract hold together. Standard slots are numbered 0..n-1 and counter-pick
    /// slots 0..m-1; overall slot numbers run through both, standard first; the special slots are the last standard
    /// slots, in position order, and carry the settings' tags; and every game shows in exactly one slot, the one its
    /// own slot number names.
    /// </summary>
    public static void AssertSlotsAreConsistent(LeagueYearViewModel leagueYear)
    {
        var settings = leagueYear.Settings;
        var specialSlots = settings.SpecialGameSlots.OrderBy(s => s.SpecialSlotPosition).ToList();
        var firstSpecialSlotNumber = settings.StandardGames - specialSlots.Count;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(specialSlots.Select(s => s.SpecialSlotPosition), Is.EqualTo(Enumerable.Range(0, specialSlots.Count)),
                "Special slot positions should run 0..n-1.");

            foreach (var publisher in leagueYear.Publishers)
            {
                var name = publisher.PublisherName;
                var slots = publisher.GameSlots.ToList();
                var standardSlots = slots.Where(s => !s.CounterPick).ToList();
                var counterPickSlots = slots.Where(s => s.CounterPick).ToList();

                Assert.That(standardSlots.Select(s => s.SlotNumber), Is.EqualTo(Enumerable.Range(0, settings.StandardGames)),
                    $"{name}'s standard slots should be numbered 0..{settings.StandardGames - 1}, in order.");
                Assert.That(counterPickSlots.Select(s => s.SlotNumber), Is.EqualTo(Enumerable.Range(0, settings.CounterPicks)),
                    $"{name}'s counter-pick slots should be numbered 0..{settings.CounterPicks - 1}, in order.");
                Assert.That(slots.Select(s => s.OverallSlotNumber), Is.EqualTo(Enumerable.Range(0, slots.Count)),
                    $"{name}'s slots should list standard slots then counter picks, with overall numbers 0..n-1.");
                foreach (var slot in counterPickSlots)
                {
                    Assert.That(slot.OverallSlotNumber, Is.EqualTo(settings.StandardGames + slot.SlotNumber),
                        $"{name}'s counter-pick slot {slot.SlotNumber} should follow every standard slot.");
                    Assert.That(slot.SpecialSlot, Is.Null, $"{name}'s counter-pick slot {slot.SlotNumber} should not be special.");
                }

                foreach (var slot in standardSlots)
                {
                    if (slot.SlotNumber < firstSpecialSlotNumber)
                    {
                        Assert.That(slot.SpecialSlot, Is.Null, $"{name}'s slot {slot.SlotNumber} should be a regular slot.");
                        continue;
                    }

                    var expected = specialSlots[slot.SlotNumber - firstSpecialSlotNumber];
                    Assert.That(slot.SpecialSlot, Is.Not.Null, $"{name}'s slot {slot.SlotNumber} should be special slot {expected.SpecialSlotPosition}.");
                    if (slot.SpecialSlot is not null)
                    {
                        Assert.That(slot.SpecialSlot.SpecialSlotPosition, Is.EqualTo(expected.SpecialSlotPosition),
                            $"{name}'s slot {slot.SlotNumber} has the wrong special slot position.");
                        Assert.That(slot.SpecialSlot.RequiredTags, Is.EquivalentTo(expected.RequiredTags),
                            $"{name}'s slot {slot.SlotNumber} requires different tags than the settings say.");
                    }
                }

                foreach (var game in publisher.Games)
                {
                    var slotsHoldingGame = slots.Where(s => s.PublisherGame?.PublisherGameID == game.PublisherGameID).ToList();
                    Assert.That(slotsHoldingGame, Has.Count.EqualTo(1), $"{name}'s '{game.GameName}' should be in exactly one slot.");
                    if (slotsHoldingGame.Count == 1)
                    {
                        Assert.That(slotsHoldingGame[0].CounterPick, Is.EqualTo(game.CounterPick),
                            $"{name}'s '{game.GameName}' is in a slot of the wrong kind.");
                        Assert.That(slotsHoldingGame[0].SlotNumber, Is.EqualTo(game.SlotNumber),
                            $"{name}'s '{game.GameName}' says slot {game.SlotNumber} but shows in slot {slotsHoldingGame[0].SlotNumber}.");
                    }
                }

                Assert.That(slots.Count(s => s.PublisherGame is not null), Is.EqualTo(publisher.Games.Count),
                    $"{name} has {publisher.Games.Count} games but a different number of filled slots.");
            }
        }
    }
}
