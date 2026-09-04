using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.FakeRepo.TestUtilities;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class BidSlotPathFunctionsTests
{
    private static readonly int _year = 2026;
    private static readonly Instant _fixtureTimestamp = Instant.FromUtc(2026, 6, 1, 0, 0);

    [Test]
    public void HasBidSlotAcquisitionPath_OpenStandardSlot_ReturnsTrue()
    {
        var (publisher, leagueYear) = BuildPublisherAndLeagueYear(
            standardGames: 4,
            filledStandardSlotNumbers: [0, 1, 2]);

        var result = BidSlotPathFunctions.HasBidSlotAcquisitionPath(
            publisher, leagueYear, counterPick: false, conditionalDropOnBid: null, activeDropRequests: Array.Empty<DropRequest>());

        Assert.That(result, Is.True);
    }

    [Test]
    public void HasBidSlotAcquisitionPath_FullRosterWithConditionalDrop_ReturnsTrue()
    {
        var (publisher, leagueYear) = BuildPublisherAndLeagueYear(
            standardGames: 4,
            filledStandardSlotNumbers: [0, 1, 2, 3]);
        var conditionalDrop = publisher.PublisherGames.First();

        var result = BidSlotPathFunctions.HasBidSlotAcquisitionPath(
            publisher, leagueYear, counterPick: false, conditionalDropOnBid: conditionalDrop, activeDropRequests: Array.Empty<DropRequest>());

        Assert.That(result, Is.True);
    }

    [Test]
    public void HasBidSlotAcquisitionPath_FullRosterWithPendingDrop_ReturnsTrue()
    {
        var (publisher, leagueYear) = BuildPublisherAndLeagueYear(
            standardGames: 4,
            filledStandardSlotNumbers: [0, 1, 2, 3]);
        var dropRequests = new List<DropRequest> { CreateDropRequest(publisher, leagueYear, publisher.PublisherGames.First()) };

        var result = BidSlotPathFunctions.HasBidSlotAcquisitionPath(
            publisher, leagueYear, counterPick: false, conditionalDropOnBid: null, activeDropRequests: dropRequests);

        Assert.That(result, Is.True);
    }

    [Test]
    public void HasBidSlotAcquisitionPath_FullRosterNoPath_ReturnsFalse()
    {
        var (publisher, leagueYear) = BuildPublisherAndLeagueYear(
            standardGames: 4,
            filledStandardSlotNumbers: [0, 1, 2, 3]);

        var result = BidSlotPathFunctions.HasBidSlotAcquisitionPath(
            publisher, leagueYear, counterPick: false, conditionalDropOnBid: null, activeDropRequests: Array.Empty<DropRequest>());

        Assert.That(result, Is.False);
    }

    [Test]
    public void HasBidSlotAcquisitionPath_FullCounterPickSlots_ReturnsFalse()
    {
        var (publisher, leagueYear) = BuildPublisherAndLeagueYear(
            standardGames: 4,
            counterPicks: 1,
            filledStandardSlotNumbers: [0, 1, 2, 3],
            filledCounterPickSlotNumbers: [0]);

        var result = BidSlotPathFunctions.HasBidSlotAcquisitionPath(
            publisher, leagueYear, counterPick: true, conditionalDropOnBid: null, activeDropRequests: Array.Empty<DropRequest>());

        Assert.That(result, Is.False);
    }

    [Test]
    public void WouldBlockDropRemoval_OrphansNonConditionalBid_ReturnsTrue()
    {
        var (publisher, leagueYear) = BuildPublisherAndLeagueYear(
            standardGames: 4,
            filledStandardSlotNumbers: [0, 1, 2, 3]);
        var dropToRemove = CreateDropRequest(publisher, leagueYear, publisher.PublisherGames.First());
        var activeBids = new List<PickupBid> { CreatePickupBid(publisher, leagueYear, conditionalDropPublisherGame: null) };

        var result = BidSlotPathFunctions.WouldBlockDropRemoval(
            publisher, leagueYear, dropToRemove, new List<DropRequest> { dropToRemove }, activeBids);

        Assert.That(result, Is.True);
    }

    [Test]
    public void WouldBlockDropRemoval_AllBidsHaveConditionalDrop_ReturnsFalse()
    {
        var (publisher, leagueYear) = BuildPublisherAndLeagueYear(
            standardGames: 4,
            filledStandardSlotNumbers: [0, 1, 2, 3]);
        var conditionalDrop = publisher.PublisherGames.First();
        var dropToRemove = CreateDropRequest(publisher, leagueYear, conditionalDrop);
        var activeBids = new List<PickupBid> { CreatePickupBid(publisher, leagueYear, conditionalDropPublisherGame: conditionalDrop) };

        var result = BidSlotPathFunctions.WouldBlockDropRemoval(
            publisher, leagueYear, dropToRemove, new List<DropRequest> { dropToRemove }, activeBids);

        Assert.That(result, Is.False);
    }

    [Test]
    public void WouldBlockDropRemoval_OpenSlotExists_ReturnsFalse()
    {
        var (publisher, leagueYear) = BuildPublisherAndLeagueYear(
            standardGames: 4,
            filledStandardSlotNumbers: [0, 1, 2]);
        var dropToRemove = CreateDropRequest(publisher, leagueYear, publisher.PublisherGames.First());
        var activeBids = new List<PickupBid> { CreatePickupBid(publisher, leagueYear, conditionalDropPublisherGame: null) };

        var result = BidSlotPathFunctions.WouldBlockDropRemoval(
            publisher, leagueYear, dropToRemove, new List<DropRequest> { dropToRemove }, activeBids);

        Assert.That(result, Is.False);
    }

    [Test]
    public void WouldBlockDropRemoval_OtherPendingDropRemains_ReturnsFalse()
    {
        var (publisher, leagueYear) = BuildPublisherAndLeagueYear(
            standardGames: 4,
            filledStandardSlotNumbers: [0, 1, 2, 3]);
        var dropToRemove = CreateDropRequest(publisher, leagueYear, publisher.PublisherGames.First());
        var otherDrop = CreateDropRequest(publisher, leagueYear, publisher.PublisherGames.Skip(1).First());
        var activeBids = new List<PickupBid> { CreatePickupBid(publisher, leagueYear, conditionalDropPublisherGame: null) };

        var result = BidSlotPathFunctions.WouldBlockDropRemoval(
            publisher, leagueYear, dropToRemove, new List<DropRequest> { dropToRemove, otherDrop }, activeBids);

        Assert.That(result, Is.False);
    }

    [Test]
    public void WouldBlockDropRemoval_OnlyCounterPickBidWithoutConditional_ReturnsFalse()
    {
        var (publisher, leagueYear) = BuildPublisherAndLeagueYear(
            standardGames: 4,
            filledStandardSlotNumbers: [0, 1, 2, 3]);
        var dropToRemove = CreateDropRequest(publisher, leagueYear, publisher.PublisherGames.First());
        var activeBids = new List<PickupBid>
        {
            CreatePickupBid(publisher, leagueYear, conditionalDropPublisherGame: null, counterPick: true)
        };

        var result = BidSlotPathFunctions.WouldBlockDropRemoval(
            publisher, leagueYear, dropToRemove, new List<DropRequest> { dropToRemove }, activeBids);

        Assert.That(result, Is.False);
    }

    [Test]
    public void NoSlotPathError_HasExpectedMessage()
    {
        Assert.That(
            BidSlotPathFunctions.NoSlotPathError,
            Is.EqualTo("You have no open roster spots. Place a drop request or add a conditional drop to this bid."));
    }

    [Test]
    public void DropRemovalBlockedError_HasExpectedMessage()
    {
        Assert.That(
            BidSlotPathFunctions.DropRemovalBlockedError,
            Is.EqualTo("You can't cancel this drop while you have active bids that depend on it."));
    }

    private static (Publisher Publisher, LeagueYear LeagueYear) BuildPublisherAndLeagueYear(
        int standardGames,
        IReadOnlyList<int> filledStandardSlotNumbers,
        int counterPicks = 0,
        IReadOnlyList<int>? filledCounterPickSlotNumbers = null)
    {
        var leagueID = Guid.NewGuid();
        var leagueYearKey = new LeagueYearKey(leagueID, _year);
        var draftID = Guid.NewGuid();
        var publisherID = Guid.NewGuid();

        var league = new League(leagueID, "Test League",
            new MinimalFantasyCriticUser(Guid.NewGuid(), "Manager", "manager@test.com"), null, null,
            [new MinimalLeagueYearInfo(_year, false, true)],
            true, false, false, false, 0);

        var supportedYear = new SupportedYear(_year, true, true, true, new LocalDate(_year - 1, 12, 8), false);
        var options = BuildLeagueOptions(standardGames, counterPicks);

        var publisherDraftInfo = new List<PublisherDraftInfo>
        {
            new(draftID, 1, publisherID, 1, [])
        };

        var draft = new LeagueDraft(draftID, leagueYearKey, 1, "Draft 1",
            null, standardGames, 0, false, true, PlayStatus.DraftFinal, publisherDraftInfo, null);

        var publisherGames = filledStandardSlotNumbers
            .Select(slotNumber => CreatePublisherGame(publisherID, slotNumber, counterPick: false, $"Game-{slotNumber}"))
            .ToList();

        foreach (var slotNumber in filledCounterPickSlotNumbers ?? [])
        {
            publisherGames.Add(CreatePublisherGame(publisherID, slotNumber, counterPick: true, $"CP-{slotNumber}"));
        }

        var publisher = new Publisher(
            publisherID, leagueYearKey,
            FantasyCriticUser.GetFakeUser(),
            "Publisher 1", null, null,
            publisherDraftInfo,
            publisherGames, [],
            100, 0, 0, 0, 0,
            new AutoDraftSettings(AutoDraftMode.Off, false));

        var leagueYear = new LeagueYear(league, supportedYear, options, [draft], [], [], null, [publisher], null, false, null);

        return (publisher, leagueYear);
    }

    private static LeagueOptions BuildLeagueOptions(int standardGames, int counterPicks)
    {
        return new LeagueOptions(
            standardGames, counterPicks,
            5, 1, 0,
            false, false, false, false,
            0, false,
            [], [],
            PickupSystem.SemiPublicBiddingSecretCounterPicks,
            ScoringSystem.GetDefaultScoringSystem(_year),
            TradingSystem.Standard,
            TiebreakSystem.LowestProjectedPoints,
            ReleaseSystem.MustBeReleased,
            IneligibleGameSystem.CaseByCase,
            new AnnualDate(12, 1),
            null,
            false);
    }

    private static PublisherGame CreatePublisherGame(Guid publisherID, int slotNumber, bool counterPick, string gameName)
    {
        var masterGame = new MasterGame(Guid.NewGuid(), gameName, "2026-06-01",
            new LocalDate(2026, 6, 1), new LocalDate(2026, 6, 1),
            null, null, null, new LocalDate(2026, 6, 1), null, null, null, null,
            false, null, "", null, null, null,
            false, false, false, false,
            _fixtureTimestamp,
            new FantasyCriticUser { Id = Guid.Empty }.ToVeryMinimal(),
            [], [MasterGameTagDictionary.TagDictionary["NG"]]);

        return new PublisherGame(
            publisherID, Guid.NewGuid(), gameName,
            _fixtureTimestamp, counterPick, null, false, null,
            new MasterGameYear(masterGame, 2026),
            slotNumber, null, null, null, null, null);
    }

    private static DropRequest CreateDropRequest(Publisher publisher, LeagueYear leagueYear, PublisherGame publisherGame)
    {
        return new DropRequest(
            Guid.NewGuid(),
            publisher,
            leagueYear,
            publisherGame.MasterGame!.MasterGame,
            _fixtureTimestamp,
            null,
            null);
    }

    private static PickupBid CreatePickupBid(
        Publisher publisher,
        LeagueYear leagueYear,
        PublisherGame? conditionalDropPublisherGame,
        bool counterPick = false)
    {
        var masterGame = new MasterGame(Guid.NewGuid(), "Bid Target", "2026-06-01",
            new LocalDate(2026, 6, 1), new LocalDate(2026, 6, 1),
            null, null, null, new LocalDate(2026, 6, 1), null, null, null, null,
            false, null, "", null, null, null,
            false, false, false, false,
            Instant.MinValue,
            new FantasyCriticUser { Id = Guid.Empty }.ToVeryMinimal(),
            [], [MasterGameTagDictionary.TagDictionary["NG"]]);

        return new PickupBid(
            Guid.NewGuid(),
            publisher,
            leagueYear,
            masterGame,
            conditionalDropPublisherGame,
            counterPick: counterPick,
            bidAmount: 1,
            allowIneligibleSlot: false,
            priority: 1,
            timestamp: _fixtureTimestamp,
            successful: null,
            processSetID: null,
            outcome: null,
            projectedPointsAtTimeOfBid: null);
    }
}
