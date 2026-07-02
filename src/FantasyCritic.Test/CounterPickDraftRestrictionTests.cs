using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.FakeRepo.TestUtilities;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class CounterPickDraftRestrictionTests
{
    private static DraftService CreateDraftService() =>
        new(null!, null!, null!, null!, null!, null!, null!);

    [Test]
    public void GetAvailableCounterPicks_WhenFlagOn_ExcludesGamesFromOtherDrafts()
    {
        var draftID1 = Guid.NewGuid();
        var draftID2 = Guid.NewGuid();
        var leagueYear = BuildMultiDraftLeagueYear(draftID1, draftID2, counterPicksMustBeFromThisDraft: true, out _, out _);
        var picker = leagueYear.Publishers[0];
        var activeDraft = leagueYear.Drafts.Single(d => d.DraftID == draftID2);

        var results = CreateDraftService().GetAvailableCounterPicks(leagueYear, picker, activeDraft);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].DraftID, Is.EqualTo(draftID2));
    }

    [Test]
    public void GetAvailableCounterPicks_WhenFlagOff_IncludesGamesFromOtherDrafts()
    {
        var draftID1 = Guid.NewGuid();
        var draftID2 = Guid.NewGuid();
        var leagueYear = BuildMultiDraftLeagueYear(draftID1, draftID2, counterPicksMustBeFromThisDraft: false, out _, out _);
        var picker = leagueYear.Publishers[0];
        var activeDraft = leagueYear.Drafts.Single(d => d.DraftID == draftID2);

        var results = CreateDraftService().GetAvailableCounterPicks(leagueYear, picker, activeDraft);

        Assert.That(results, Has.Count.EqualTo(2));
    }

    [Test]
    public void CanClaimGame_DraftingCounterPick_WhenFlagOn_RejectsGameFromOtherDraft()
    {
        var draftID1 = Guid.NewGuid();
        var draftID2 = Guid.NewGuid();
        var leagueYear = BuildMultiDraftLeagueYear(draftID1, draftID2, counterPicksMustBeFromThisDraft: true, out var masterGameA, out _);
        var picker = leagueYear.Publishers[0];
        var request = new ClaimGameDomainRequest(leagueYear, picker, masterGameA.GameName, true, false, false, false, masterGameA, null, null);

        var result = GameEligibilityFunctions.CanClaimGame(
            request, null, null, true, true, false, false,
            new LocalDate(2026, 1, 15), false, [],
            activeDraftID: draftID2);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Errors.Any(e => e.Error.Contains("not drafted in this draft", StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    [Test]
    public void CanClaimGame_DraftingCounterPick_WhenFlagOff_AllowsGameFromOtherDraft()
    {
        var draftID1 = Guid.NewGuid();
        var draftID2 = Guid.NewGuid();
        var leagueYear = BuildMultiDraftLeagueYear(draftID1, draftID2, counterPicksMustBeFromThisDraft: false, out var masterGameA, out _);
        var picker = leagueYear.Publishers[0];
        var request = new ClaimGameDomainRequest(leagueYear, picker, masterGameA.GameName, true, false, false, false, masterGameA, null, null);

        var result = GameEligibilityFunctions.CanClaimGame(
            request, null, null, true, true, false, false,
            new LocalDate(2026, 1, 15), false, [],
            activeDraftID: draftID2);

        Assert.That(result.Errors.Any(e => e.Error.Contains("not drafted in this draft", StringComparison.OrdinalIgnoreCase)), Is.False);
    }

    private static MasterGame CreateTestMasterGame(string name, int year)
    {
        var releaseDate = new LocalDate(year, 6, 1);
        return new MasterGame(Guid.NewGuid(), name, releaseDate.ToISOString(), releaseDate, releaseDate, null, null, null,
            releaseDate, null, null, null, null, false, null, "", null, null, null, false, false, false, false, Instant.MinValue,
            new FantasyCriticUser() { Id = Guid.Empty }.ToVeryMinimal(),
            [], [MasterGameTagDictionary.TagDictionary["NG"]]);
    }

    private static LeagueYear BuildMultiDraftLeagueYear(Guid draftID1, Guid draftID2, bool counterPicksMustBeFromThisDraft,
        out MasterGame masterGameA, out MasterGame masterGameB)
    {
        var leagueID = Guid.NewGuid();
        var year = 2026;
        var leagueYearKey = new LeagueYearKey(leagueID, year);

        var manager = new MinimalFantasyCriticUser(Guid.NewGuid(), "Manager", "manager@test.com");
        var league = new League(leagueID, "Test League", manager, null, null,
            [new MinimalLeagueYearInfo(year, false, true)],
            true, false, false, false, 0);

        var supportedYear = new SupportedYear(year, true, true, true, new LocalDate(year - 1, 12, 8), false);

        var options = new LeagueOptions(
            10, 2,
            5, 1, 0,
            false, false, false, false,
            0, true,
            [], [],
            DraftSystem.Flexible,
            PickupSystem.SemiPublicBiddingSecretCounterPicks,
            ScoringSystem.GetDefaultScoringSystem(year),
            TradingSystem.Standard,
            TiebreakSystem.LowestProjectedPoints,
            ReleaseSystem.MustBeReleased,
            IneligibleGameSystem.CaseByCase,
            new AnnualDate(12, 1),
            null,
            false);

        var publisherID1 = Guid.NewGuid();
        var publisherID2 = Guid.NewGuid();

        var draft1PublisherDraftInfo = new List<PublisherDraftInfo>
        {
            new(draftID1, 1, publisherID1, 1, []),
            new(draftID1, 1, publisherID2, 2, [])
        };
        var draft2PublisherDraftInfo = new List<PublisherDraftInfo>
        {
            new(draftID2, 2, publisherID1, 1, []),
            new(draftID2, 2, publisherID2, 2, [])
        };

        var draft1 = new LeagueDraft(draftID1, leagueYearKey, 1, "Draft 1",
            null, 5, 1, false, true, PlayStatus.DraftFinal, draft1PublisherDraftInfo, null);

        var draft2 = new LeagueDraft(draftID2, leagueYearKey, 2, "Draft 2",
            null, 5, 1, counterPicksMustBeFromThisDraft, true, PlayStatus.Drafting, draft2PublisherDraftInfo, null);

        masterGameA = CreateTestMasterGame("Game A", year);
        masterGameB = CreateTestMasterGame("Game B", year);

        var gameA = new PublisherGame(
            publisherID2,
            Guid.NewGuid(),
            "Game A",
            Instant.MinValue,
            false,
            null,
            false,
            null,
            new MasterGameYear(masterGameA, year),
            1,
            null,
            null,
            null,
            null,
            draftID1);

        var gameB = new PublisherGame(
            publisherID2,
            Guid.NewGuid(),
            "Game B",
            Instant.MinValue,
            false,
            null,
            false,
            null,
            new MasterGameYear(masterGameB, year),
            2,
            null,
            null,
            null,
            null,
            draftID2);

        var publisher1 = new Publisher(
            publisherID1,
            leagueYearKey,
            FantasyCriticUser.GetFakeUser(),
            "Publisher 1",
            null,
            null,
            draft2PublisherDraftInfo,
            [],
            [],
            100,
            0,
            0,
            0,
            0,
            new AutoDraftSettings(AutoDraftMode.Off, false));

        var publisher2 = new Publisher(
            publisherID2,
            leagueYearKey,
            FantasyCriticUser.GetFakeUser(),
            "Publisher 2",
            null,
            null,
            draft2PublisherDraftInfo,
            [gameA, gameB],
            [],
            100,
            0,
            0,
            0,
            0,
            new AutoDraftSettings(AutoDraftMode.Off, false));

        return new LeagueYear(league, supportedYear, options, [draft1, draft2],
            [], [], null, [publisher1, publisher2], null, false, null);
    }
}
