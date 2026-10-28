using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test.Draft;

[TestFixture]
public class CrossDraftPickNumberCacheTests
{
    private static readonly LeagueYearKey TestLeagueYearKey = new(Guid.Parse("11111111-1111-1111-1111-111111111111"), 2025);
    private static readonly Instant Draft1Started = Instant.FromUtc(2025, 6, 1, 12, 0);
    private static readonly Instant BetweenDrafts = Instant.FromUtc(2025, 7, 15, 12, 0);
    private static readonly Instant Draft2Started = Instant.FromUtc(2025, 8, 1, 12, 0);

    [Test]
    public void GetCrossDraftPickNumber_Draft1_ReturnsOverallPickNumber()
    {
        var leagueSlots = TestSlotCreationUtilities.GetBasicSlots(10, 5);
        var standardSlots = leagueSlots.Where(x => !x.CounterPick).ToList();
        var draftID = Guid.NewGuid();
        var publisherID = Guid.NewGuid();
        var pick = CreateDraftPick(draftID, overallPickNumber: 7, timestamp: Draft1Started.Plus(Duration.FromMinutes(7)), publisherID, counterPick: false, standardSlots[0].LeagueSlotID);
        var leagueYear = BuildSingleDraftLeagueYear(
            draftID,
            Draft1Started,
            PlayStatus.DraftFinal,
            leagueSlots,
            [CreateSingleDraftPublisher(draftID, publisherID, games: [pick], formerGames: [])]);

        var cache = CrossDraftPickNumberCache.Build([leagueYear]);
        Assert.That(cache.GetPickNumber(pick), Is.EqualTo(7));
    }

    [Test]
    public void GetCrossDraftPickNumber_Draft2FirstPick_AfterBidsBetweenDrafts_Is15thPick()
    {
        var leagueSlots = TestSlotCreationUtilities.GetBasicSlots(10, 5);
        var standardSlots = leagueSlots.Where(x => !x.CounterPick).ToList();
        var draftID1 = Guid.NewGuid();
        var draftID2 = Guid.NewGuid();
        var publisherID1 = Guid.NewGuid();
        var publisherID2 = Guid.NewGuid();

        // Picks alternate between the two publishers, so each publisher's nth game goes in its nth slot.
        var draft1Picks = Enumerable.Range(1, 10)
            .Select(i => CreateDraftPick(
                draftID1,
                overallPickNumber: i,
                timestamp: Draft1Started.Plus(Duration.FromMinutes(i)),
                publisherID: i % 2 == 1 ? publisherID1 : publisherID2,
                counterPick: false,
                slotID: standardSlots[(i - 1) / 2].LeagueSlotID))
            .ToList();

        var bidWins = Enumerable.Range(1, 4)
            .Select(i => CreateBidWin(
                BetweenDrafts.Plus(Duration.FromMinutes(i)),
                publisherID: i % 2 == 1 ? publisherID1 : publisherID2,
                slotID: standardSlots[5 + (i - 1) / 2].LeagueSlotID))
            .ToList();

        var draft2FirstPick = CreateDraftPick(
            draftID2,
            overallPickNumber: 1,
            timestamp: Draft2Started.Plus(Duration.FromMinutes(1)),
            publisherID: publisherID1,
            counterPick: false,
            slotID: standardSlots[7].LeagueSlotID);

        var leagueYear = BuildMultiDraftLeagueYear(
            draftID1,
            draftID2,
            Draft1Started,
            Draft2Started,
            PlayStatus.DraftFinal,
            PlayStatus.Drafting,
            leagueSlots,
            [
                CreateMultiDraftPublisher(
                    draftID1,
                    draftID2,
                    publisherID1,
                    draft1Picks.Where(g => g.PublisherID == publisherID1)
                        .Concat(bidWins.Where(g => g.PublisherID == publisherID1))
                        .Append(draft2FirstPick)
                        .ToList(),
                    formerGames: []),
                CreateMultiDraftPublisher(
                    draftID1,
                    draftID2,
                    publisherID2,
                    draft1Picks.Where(g => g.PublisherID == publisherID2)
                        .Concat(bidWins.Where(g => g.PublisherID == publisherID2))
                        .ToList(),
                    formerGames: [])
            ]);

        var draft2 = leagueYear.Drafts.Single(d => d.DraftNumber == 2);
        var startingPoint = draft2.GetStartingOverallPickNumber(leagueYear);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(startingPoint.IsSuccess, Is.True);
            Assert.That(startingPoint.Value.StandardGameStartingPoint, Is.EqualTo(14));
        }
        var cache = CrossDraftPickNumberCache.Build([leagueYear]);
        Assert.That(cache.GetPickNumber(draft2FirstPick), Is.EqualTo(15));
    }

    [Test]
    public void GetCrossDraftPickNumber_BidGame_ReturnsNull()
    {
        var leagueSlots = TestSlotCreationUtilities.GetBasicSlots(10, 5);
        var standardSlots = leagueSlots.Where(x => !x.CounterPick).ToList();
        var draftID = Guid.NewGuid();
        var publisherID = Guid.NewGuid();
        var leagueYear = BuildSingleDraftLeagueYear(
            draftID,
            Draft1Started,
            PlayStatus.DraftFinal,
            leagueSlots,
            [CreateSingleDraftPublisher(draftID, publisherID, games: [CreateBidWin(BetweenDrafts, publisherID, standardSlots[0].LeagueSlotID)], formerGames: [])]);

        var bidGame = leagueYear.Publishers.Single().PublisherGames.Single();

        var cache = CrossDraftPickNumberCache.Build([leagueYear]);
        Assert.That(cache.GetPickNumber(bidGame), Is.Null);
    }

    [Test]
    public void GetStartingOverallPickNumber_TradeReceival_DoesNotDoubleCount()
    {
        var leagueSlots = TestSlotCreationUtilities.GetBasicSlots(10, 5);
        var standardSlots = leagueSlots.Where(x => !x.CounterPick).ToList();
        var draftID1 = Guid.NewGuid();
        var draftID2 = Guid.NewGuid();
        var sellerID = Guid.NewGuid();
        var buyerID = Guid.NewGuid();
        var tradeID = Guid.NewGuid();

        var originalPick = CreateDraftPick(draftID1, overallPickNumber: 1, timestamp: Draft1Started.Plus(Duration.FromMinutes(1)), publisherID: sellerID, counterPick: false,
            slotID: standardSlots[0].LeagueSlotID);
        var formerPick = originalPick.GetFormerPublisherGame(BetweenDrafts, "Traded away");
        var tradeReceival = CreateTradeReceival(buyerID, tradeID, BetweenDrafts.Plus(Duration.FromMinutes(1)), standardSlots[0].LeagueSlotID);

        var leagueYear = BuildMultiDraftLeagueYear(
            draftID1,
            draftID2,
            Draft1Started,
            Draft2Started,
            PlayStatus.DraftFinal,
            PlayStatus.Drafting,
            leagueSlots,
            [
                CreateMultiDraftPublisher(draftID1, draftID2, sellerID, games: [], formerGames: [formerPick]),
                CreateMultiDraftPublisher(draftID1, draftID2, buyerID, games: [tradeReceival], formerGames: [])
            ]);

        var draft2 = leagueYear.Drafts.Single(d => d.DraftNumber == 2);
        var startingPoint = draft2.GetStartingOverallPickNumber(leagueYear);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(startingPoint.IsSuccess, Is.True);
            Assert.That(startingPoint.Value.StandardGameStartingPoint, Is.EqualTo(1));
        }
    }

    [Test]
    public void GetStartingOverallPickNumber_FormerDroppedDraftPick_StillCounts()
    {
        var leagueSlots = TestSlotCreationUtilities.GetBasicSlots(10, 5);
        var standardSlots = leagueSlots.Where(x => !x.CounterPick).ToList();
        var draftID1 = Guid.NewGuid();
        var draftID2 = Guid.NewGuid();
        var publisherID = Guid.NewGuid();

        var originalPick = CreateDraftPick(draftID1, overallPickNumber: 3, timestamp: Draft1Started.Plus(Duration.FromMinutes(3)), publisherID: publisherID, counterPick: false,
            slotID: standardSlots[0].LeagueSlotID);
        var formerPick = originalPick.GetFormerPublisherGame(BetweenDrafts, "Dropped");

        var leagueYear = BuildMultiDraftLeagueYear(
            draftID1,
            draftID2,
            Draft1Started,
            Draft2Started,
            PlayStatus.DraftFinal,
            PlayStatus.Drafting,
            leagueSlots,
            [CreateMultiDraftPublisher(draftID1, draftID2, publisherID, games: [], formerGames: [formerPick])]);

        var draft2 = leagueYear.Drafts.Single(d => d.DraftNumber == 2);
        var startingPoint = draft2.GetStartingOverallPickNumber(leagueYear);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(startingPoint.IsSuccess, Is.True);
            Assert.That(startingPoint.Value.StandardGameStartingPoint, Is.EqualTo(1));
        }
    }

    [Test]
    public void GetCrossDraftPickNumber_CounterPick_UsesSeparateTrack()
    {
        var leagueSlots = TestSlotCreationUtilities.GetBasicSlots(10, 5);
        var counterPickSlots = leagueSlots.Where(x => x.CounterPick).ToList();
        var draftID1 = Guid.NewGuid();
        var draftID2 = Guid.NewGuid();
        var publisherID = Guid.NewGuid();

        var draft1CounterPick = CreateDraftPick(
            draftID1,
            overallPickNumber: 1,
            timestamp: Draft1Started.Plus(Duration.FromMinutes(1)),
            publisherID: publisherID,
            counterPick: true,
            slotID: counterPickSlots[0].LeagueSlotID);

        var draft2CounterPick = CreateDraftPick(
            draftID2,
            overallPickNumber: 1,
            timestamp: Draft2Started.Plus(Duration.FromMinutes(1)),
            publisherID: publisherID,
            counterPick: true,
            slotID: counterPickSlots[1].LeagueSlotID);

        var leagueYear = BuildMultiDraftLeagueYear(
            draftID1,
            draftID2,
            Draft1Started,
            Draft2Started,
            PlayStatus.DraftFinal,
            PlayStatus.Drafting,
            leagueSlots,
            [CreateMultiDraftPublisher(draftID1, draftID2, publisherID, games: [draft1CounterPick, draft2CounterPick], formerGames: [])]);

        var cache = CrossDraftPickNumberCache.Build([leagueYear]);
        Assert.That(cache.GetPickNumber(draft2CounterPick), Is.EqualTo(2));
    }

    [Test]
    public void GetStartingOverallPickNumber_Draft1_ReturnsZero()
    {
        var leagueSlots = TestSlotCreationUtilities.GetBasicSlots(10, 5);
        var draftID = Guid.NewGuid();
        var leagueYear = BuildSingleDraftLeagueYear(
            draftID,
            Draft1Started,
            PlayStatus.DraftFinal,
            leagueSlots,
            [CreateSingleDraftPublisher(draftID, Guid.NewGuid(), games: [], formerGames: [])]);

        var draft1 = leagueYear.Drafts.Single();
        var startingPoint = draft1.GetStartingOverallPickNumber(leagueYear);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(startingPoint.IsSuccess, Is.True);
            Assert.That(startingPoint.Value, Is.EqualTo((0, 0)));
        }
    }

    [Test]
    public void GetStartingOverallPickNumber_UnstartedDraft2_ReturnsFailure()
    {
        var leagueSlots = TestSlotCreationUtilities.GetBasicSlots(10, 5);
        var draftID1 = Guid.NewGuid();
        var draftID2 = Guid.NewGuid();
        var leagueYear = BuildMultiDraftLeagueYear(
            draftID1,
            draftID2,
            Draft1Started,
            draft2Started: null,
            PlayStatus.DraftFinal,
            PlayStatus.NotStartedDraft,
            leagueSlots,
            [CreateMultiDraftPublisher(draftID1, draftID2, Guid.NewGuid(), games: [], formerGames: [])]);

        var draft2 = leagueYear.Drafts.Single(d => d.DraftNumber == 2);
        var startingPoint = draft2.GetStartingOverallPickNumber(leagueYear);

        Assert.That(startingPoint.IsFailure, Is.True);
    }

    [Test]
    public void GetCrossDraftPickNumber_GameInUnstartedDraft_Throws()
    {
        var leagueSlots = TestSlotCreationUtilities.GetBasicSlots(10, 5);
        var standardSlots = leagueSlots.Where(x => !x.CounterPick).ToList();
        var draftID1 = Guid.NewGuid();
        var draftID2 = Guid.NewGuid();
        var publisherID = Guid.NewGuid();

        var draft2Pick = CreateDraftPick(
            draftID2,
            overallPickNumber: 1,
            timestamp: Draft2Started.Plus(Duration.FromMinutes(1)),
            publisherID: publisherID,
            counterPick: false,
            slotID: standardSlots[0].LeagueSlotID);

        var leagueYear = BuildMultiDraftLeagueYear(
            draftID1,
            draftID2,
            Draft1Started,
            draft2Started: null,
            PlayStatus.DraftFinal,
            PlayStatus.NotStartedDraft,
            leagueSlots,
            [CreateMultiDraftPublisher(draftID1, draftID2, publisherID, games: [draft2Pick], formerGames: [])]);

        Assert.That(() => CrossDraftPickNumberCache.Build([leagueYear]), Throws.InvalidOperationException);
    }

    [Test]
    public void GetCrossDraftPickNumber_MissingOverallPickNumber_Throws()
    {
        var leagueSlots = TestSlotCreationUtilities.GetBasicSlots(10, 5);
        var standardSlots = leagueSlots.Where(x => !x.CounterPick).ToList();
        var draftID = Guid.NewGuid();
        var publisherID = Guid.NewGuid();
        var invalidPick = new PublisherGame(
            publisherID,
            Guid.NewGuid(),
            "Bad pick",
            Draft1Started,
            false,
            null,
            false,
            null,
            null,
            standardSlots[0].LeagueSlotID,
            1,
            null,
            null,
            null,
            draftID);

        var leagueYear = BuildSingleDraftLeagueYear(
            draftID,
            Draft1Started,
            PlayStatus.DraftFinal,
            leagueSlots,
            [CreateSingleDraftPublisher(draftID, publisherID, games: [invalidPick], formerGames: [])]);

        Assert.That(() => CrossDraftPickNumberCache.Build([leagueYear]), Throws.InvalidOperationException);
    }

    private static PublisherGame CreateDraftPick(
        Guid draftID,
        int overallPickNumber,
        Instant timestamp,
        Guid publisherID,
        bool counterPick,
        Guid slotID)
    {
        return new PublisherGame(
            publisherID,
            Guid.NewGuid(),
            $"Draft pick {overallPickNumber}",
            timestamp,
            counterPick,
            null,
            false,
            null,
            null,
            slotID,
            1,
            overallPickNumber,
            null,
            null,
            draftID);
    }

    private static PublisherGame CreateBidWin(Instant timestamp, Guid publisherID, Guid slotID)
    {
        return new PublisherGame(
            publisherID,
            Guid.NewGuid(),
            "Bid win",
            timestamp,
            false,
            null,
            false,
            null,
            null,
            slotID,
            null,
            null,
            5,
            null,
            null);
    }

    private static PublisherGame CreateTradeReceival(Guid publisherID, Guid tradeID, Instant timestamp, Guid slotID)
    {
        return new PublisherGame(
            publisherID,
            Guid.NewGuid(),
            "Trade receival",
            timestamp,
            false,
            null,
            false,
            null,
            null,
            slotID,
            null,
            null,
            null,
            tradeID,
            null);
    }

    private static Publisher CreateSingleDraftPublisher(
        Guid draftID,
        Guid publisherID,
        IReadOnlyList<PublisherGame> games,
        IReadOnlyList<FormerPublisherGame> formerGames)
    {
        return new Publisher(
            publisherID,
            TestLeagueYearKey,
            FantasyCriticUser.GetFakeUser(),
            "Publisher",
            null,
            null,
            [new PublisherDraftInfo(draftID, 1, publisherID, 1, [])],
            games.ToList(),
            formerGames.ToList(),
            100,
            0,
            0,
            0,
            0,
            new AutoDraftSettings(AutoDraftMode.Off, false));
    }

    private static Publisher CreateMultiDraftPublisher(
        Guid draftID1,
        Guid draftID2,
        Guid publisherID,
        IReadOnlyList<PublisherGame> games,
        IReadOnlyList<FormerPublisherGame> formerGames)
    {
        return new Publisher(
            publisherID,
            TestLeagueYearKey,
            FantasyCriticUser.GetFakeUser(),
            "Publisher",
            null,
            null,
            [
                new PublisherDraftInfo(draftID1, 1, publisherID, 1, []),
                new PublisherDraftInfo(draftID2, 2, publisherID, 1, [])
            ],
            games.ToList(),
            formerGames.ToList(),
            100,
            0,
            0,
            0,
            0,
            new AutoDraftSettings(AutoDraftMode.Off, false));
    }

    private static LeagueYear BuildSingleDraftLeagueYear(
        Guid draftID,
        Instant? draftStarted,
        PlayStatus playStatus,
        IReadOnlyList<LeagueSlot> leagueSlots,
        IReadOnlyList<Publisher> publishers)
    {
        var draftPublisherDraftInfo = publishers
            .Select(p => new PublisherDraftInfo(draftID, 1, p.PublisherID, 1, []))
            .ToList();

        var draft = new LeagueDraft(
            draftID,
            TestLeagueYearKey,
            1,
            "Draft 1",
            null,
            10,
            5,
            true,
            true,
            playStatus,
            draftPublisherDraftInfo,
            draftStarted);

        return CreateLeagueYear([draft], leagueSlots, publishers);
    }

    private static LeagueYear BuildMultiDraftLeagueYear(
        Guid draftID1,
        Guid draftID2,
        Instant? draft1Started,
        Instant? draft2Started,
        PlayStatus draft1PlayStatus,
        PlayStatus draft2PlayStatus,
        IReadOnlyList<LeagueSlot> leagueSlots,
        IReadOnlyList<Publisher> publishers)
    {
        var draft1PublisherDraftInfo = publishers
            .Select(p => new PublisherDraftInfo(draftID1, 1, p.PublisherID, 1, []))
            .ToList();
        var draft2PublisherDraftInfo = publishers
            .Select(p => new PublisherDraftInfo(draftID2, 2, p.PublisherID, 1, []))
            .ToList();

        var draft1 = new LeagueDraft(
            draftID1,
            TestLeagueYearKey,
            1,
            "Draft 1",
            null,
            10,
            5,
            true,
            true,
            draft1PlayStatus,
            draft1PublisherDraftInfo,
            draft1Started);

        var draft2 = new LeagueDraft(
            draftID2,
            TestLeagueYearKey,
            2,
            "Draft 2",
            null,
            10,
            5,
            true,
            true,
            draft2PlayStatus,
            draft2PublisherDraftInfo,
            draft2Started);

        return CreateLeagueYear([draft1, draft2], leagueSlots, publishers);
    }

    private static LeagueYear CreateLeagueYear(IReadOnlyList<LeagueDraft> drafts, IReadOnlyList<LeagueSlot> leagueSlots, IReadOnlyList<Publisher> publishers)
    {
        var year = TestLeagueYearKey.Year;
        var league = new League(
            TestLeagueYearKey.LeagueID,
            "Test League",
            new MinimalFantasyCriticUser(Guid.NewGuid(), "Manager", "manager@test.com"),
            null,
            null,
            [new MinimalLeagueYearInfo(year, false, true)],
            true,
            false,
            false,
            false,
            0);

        var supportedYear = new SupportedYear(year, true, true, true, new LocalDate(year - 1, 12, 8), false);

        return new LeagueYear(
            league,
            supportedYear,
            CreateDefaultLeagueOptions(year, leagueSlots),
            drafts,
            [],
            [],
            null,
            publishers.ToList(),
            null,
            false,
            null);
    }

    private static LeagueOptions CreateDefaultLeagueOptions(int year, IReadOnlyList<LeagueSlot> leagueSlots) => new(
        10,
        5,
        0,
        false,
        false,
        false,
        false,
        0,
        true,
        [],
        leagueSlots,
        PickupSystem.SemiPublicBiddingSecretCounterPicks,
        ScoringSystem.GetDefaultScoringSystem(year),
        TradingSystem.Standard,
        TiebreakSystem.LowestProjectedPoints,
        ReleaseSystem.MustBeReleased,
        IneligibleGameSystem.CaseByCase,
        new AnnualDate(10, 1),
        new AnnualDate(10, 1),
        false);
}
