using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.FakeRepo.TestUtilities;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using NodaTime;
using NodaTime.Text;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class EligibilityTests
{
    private static MasterGame CreateBasicMasterGame(string name, LocalDate releaseDate, MasterGameTag tag)
    {
        return new MasterGame(Guid.NewGuid(), name, releaseDate.ToISOString(), releaseDate, releaseDate, null, null, null,
            releaseDate, null, null, null, null, false, null, "", null, null, null, false, false, false, false, Instant.MinValue, new FantasyCriticUser() { Id = Guid.Empty }.ToVeryMinimal(), 
            new List<MasterSubGame>(), new List<MasterGameTag>() { tag });
    }

    private static MasterGame CreateComplexMasterGame(string name, LocalDate minimumReleaseDate, LocalDate? maximumReleaseDate,
        LocalDate? earlyAccessReleaseDate, LocalDate? internationalReleaseDate, LocalDate? announcementDate, IEnumerable<MasterGameTag> tags)
    {
        return new MasterGame(Guid.NewGuid(), name, "TBA", minimumReleaseDate, maximumReleaseDate,
            earlyAccessReleaseDate, internationalReleaseDate, announcementDate, null, null, null, null, null, false, null, "", null, null, null, false, false, false, false,
            Instant.MinValue, new FantasyCriticUser() { Id = Guid.Empty }.ToVeryMinimal(), new List<MasterSubGame>(), tags);
    }

    private static LeagueYear CreateLeagueYearForBidTest(
        bool bidsOnlyBeforeNextScheduledDraft,
        LocalDate? pendingDraftScheduledDate)
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
            0, 0, 0,
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
            bidsOnlyBeforeNextScheduledDraft);

        var draftID1 = Guid.NewGuid();
        var draft1 = new LeagueDraft(draftID1, leagueYearKey, 1, "Initial Draft",
            null, 5, 1, true, PlayStatus.DraftFinal, [], null);

        var draftID2 = Guid.NewGuid();
        var draft2 = new LeagueDraft(draftID2, leagueYearKey, 2, "Draft 2",
            pendingDraftScheduledDate, 5, 1, false, PlayStatus.NotStartedDraft, [], null);

        return new LeagueYear(league, supportedYear, options, [draft1, draft2],
            [], [], null, [], null, false, null);
    }

    private static LeagueYear CreateLeagueYearForBidTestNoPendingDraft(
        bool bidsOnlyBeforeNextScheduledDraft)
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
            0, 0, 0,
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
            bidsOnlyBeforeNextScheduledDraft);

        var draftID = Guid.NewGuid();
        var draft = new LeagueDraft(draftID, leagueYearKey, 1, "Initial Draft",
            null, 10, 2, true, PlayStatus.DraftFinal, [], null);

        return new LeagueYear(league, supportedYear, options, [draft],
            [], [], null, [], null, false, null);
    }

    [Test]
    public void SimpleEligibleTest()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateBasicMasterGame("Elden Ring", new LocalDate(2022, 2, 25), MasterGameTagDictionary.TagDictionary["NGF"]);

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned)
        };

        var slotTags = new List<LeagueTagStatus>();

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(0));
    }

    [Test]
    public void SimpleInEligibleTest()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateBasicMasterGame("GTA 5 (PS5)", new LocalDate(2022, 2, 25), MasterGameTagDictionary.TagDictionary["PRT"]);

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned)
        };

        var slotTags = new List<LeagueTagStatus>();

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(1));
        Assert.That(claimErrors[0].Error, Is.EqualTo("That game is not eligible because the Port tag has been banned"));
    }

    [Test]
    public void SlotEligibleTest()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateBasicMasterGame("Elden Ring", new LocalDate(2022, 2, 25), MasterGameTagDictionary.TagDictionary["NGF"]);

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned)
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["NGF"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(0));
    }

    [Test]
    public void SlotInEligibleTest()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateBasicMasterGame("Horizon Forbidden West", new LocalDate(2022, 2, 25), MasterGameTagDictionary.TagDictionary["NG"]);

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned)
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["NGF"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(1));
        Assert.That(claimErrors[0].Error, Is.EqualTo("That game is not eligible because it does not have any of the following required tags: (New Gaming Franchise)"));
    }

    [Test]
    public void EarlyAccessHasGameBeforeEarlyAccessEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Have a Nice Death", new LocalDate(2022, 1, 3), null,
            new LocalDate(2022, 3, 6), null, null, new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["NG"],
                MasterGameTagDictionary.TagDictionary["C-EA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>();

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(0));
    }

    [Test]
    public void EarlyAccessHasGameAfterEarlyAccessInEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-03-10T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Have a Nice Death", new LocalDate(2022, 1, 3), null,
            new LocalDate(2022, 3, 6), null, null, new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["NG"],
                MasterGameTagDictionary.TagDictionary["C-EA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>();

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(1));
        Assert.That(claimErrors[0].Error, Is.EqualTo("That game is not eligible because the Currently in Early Access tag has been banned"));
    }

    [Test]
    public void EarlyAccessAllowedEligibleTest()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Baldur's Gate 3", new LocalDate(2022, 1, 3), null,
            new LocalDate(2020, 10, 6), null, null, new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["NG"],
                MasterGameTagDictionary.TagDictionary["C-EA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>();

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(0));
    }

    [Test]
    public void EarlyAccessNormalAllowedSlotRequiredEligibleTest()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Baldur's Gate 3", new LocalDate(2022, 1, 3), null,
            new LocalDate(2020, 10, 6), null, null, new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["NG"],
                MasterGameTagDictionary.TagDictionary["C-EA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["P-EA"], TagStatus.Required),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(0));
    }

    [Test]
    public void EarlyAccessNormalBannedSlotRequiredEligibleTest()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Baldur's Gate 3", new LocalDate(2022, 1, 3), null,
            new LocalDate(2020, 10, 6), null, null, new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["NG"],
                MasterGameTagDictionary.TagDictionary["C-EA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["P-EA"], TagStatus.Required),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(0));
    }

    [Test]
    public void EarlyAccessReleasedInternationallyComplexSlotEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Baldur's Gate 3", new LocalDate(2022, 1, 3), null,
            new LocalDate(2020, 10, 6), null, null, new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["NG"],
                MasterGameTagDictionary.TagDictionary["C-EA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Required),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["R-INT"], TagStatus.Required),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["NG"], TagStatus.Required),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["NGF"], TagStatus.Required),
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(0));
    }

    [Test]
    public void UnannouncedSimpleEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Star Wars Jedi: Fallen Order 2", new LocalDate(2022, 1, 27), null,
            null, null, null, new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["NG"],
                MasterGameTagDictionary.TagDictionary["UNA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {

        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(0));
    }

    [Test]
    public void UnannouncedSimpleInEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Star Wars Jedi: Fallen Order 2", new LocalDate(2022, 1, 27), null,
            null, null, null, new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["NG"],
                MasterGameTagDictionary.TagDictionary["UNA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["R-INT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["UNA"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {

        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(1));
        Assert.That(claimErrors[0].Error, Is.EqualTo("That game is not eligible because the Unannounced Game tag has been banned"));
    }

    [Test]
    public void PreviouslyUnannouncedEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Star Wars Jedi: Fallen Order 2", new LocalDate(2022, 1, 27), null,
            null, null, new LocalDate(2022, 1, 25), new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["NG"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["UNA"], TagStatus.Required),
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(0));
    }

    [Test]
    public void PreviouslyUnannouncedLeagueBannedEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Star Wars Jedi: Fallen Order 2", new LocalDate(2022, 1, 27), null,
            null, null, new LocalDate(2022, 1, 25), new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["NG"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["R-INT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["UNA"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {

        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(0));
    }

    [Test]
    public void PreviouslyUnannouncedInEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-26T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Star Wars Jedi: Fallen Order 2", new LocalDate(2022, 1, 27), null,
            null, null, new LocalDate(2022, 1, 25), new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["NG"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["UNA"], TagStatus.Required),
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(1));
        Assert.That(claimErrors[0].Error, Is.EqualTo("That game is not eligible because it does not have any of the following required tags: (Unannounced Game)"));
    }

    [Test]
    public void UnannouncedRemakeEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("The Last of Us Remake", new LocalDate(2022, 1, 27), null,
            null, null, null, new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["RMKE"],
                MasterGameTagDictionary.TagDictionary["UNA"]
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["RMKE"], TagStatus.Required),
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(0));
    }

    [Test]
    public void UnannouncedRemakeInEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("The Last of Us Remake", new LocalDate(2022, 1, 27), null,
            null, null, null, new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["RMKE"],
                MasterGameTagDictionary.TagDictionary["UNA"]
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["R-INT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["RMKE"], TagStatus.Banned)
        };

        var slotTags = new List<LeagueTagStatus>()
        {

        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(1));
        Assert.That(claimErrors[0].Error, Is.EqualTo("That game is not eligible because the Remake tag has been banned"));
    }

    [Test]
    public void PreviouslyUnannouncedRemakeEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-29T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("The Last of Us Remake", new LocalDate(2022, 2, 3), null,
            null, null, new LocalDate(2022, 2, 2), new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["RMKE"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["RMKE"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(0));
    }

    [Test]
    public void PreviouslyUnannouncedRemakeInEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-02-07T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("The Last of Us Remake", new LocalDate(2022, 2, 8), null,
            null, null, new LocalDate(2022, 2, 2), new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["RMKE"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["UNA"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(1));
        Assert.That(claimErrors[0].Error, Is.EqualTo("That game is not eligible because it does not have any of the following required tags: (Unannounced Game)"));
    }

    [Test]
    public void UnannouncedGameFailsSlotConditions()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Star Wars Jedi: Fallen Order 2", new LocalDate(2022, 1, 27), null,
            null, null, null, new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["NG"],
                MasterGameTagDictionary.TagDictionary["UNA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["RMKE"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(1));
        Assert.That(claimErrors[0].Error, Is.EqualTo("That game is not eligible because it does not have any of the following required tags: (Remake)"));
    }

    [Test]
    public void PreviouslyUnannouncedGameFailsSlotConditions()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-26T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Star Wars Jedi: Fallen Order 2", new LocalDate(2022, 1, 27), null,
            null, null, new LocalDate(2022, 1, 25), new List<MasterGameTag>()
            {
                MasterGameTagDictionary.TagDictionary["NG"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["RMKE"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.That(claimErrors.Count, Is.EqualTo(1));
        Assert.That(claimErrors[0].Error, Is.EqualTo("That game is not eligible because it does not have any of the following required tags: (Remake)"));
    }

    [Test]
    public void BidsOnlyBeforeNextDraft_GameReleasesBeforeDraft_Allowed()
    {
        var draftDate = new LocalDate(2026, 6, 1);
        var maxRelease = new LocalDate(2026, 5, 31);
        var game = CreateComplexMasterGame("Game A", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
            [MasterGameTagDictionary.TagDictionary["NG"]]);
        var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: true, pendingDraftScheduledDate: draftDate);
        var currentDate = new LocalDate(2026, 1, 15);

        var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
            leagueYear, game, dropping: false, currentDate, currentDate,
            counterPick: false, counterPickedGameIsManualWillNotRelease: false,
            drafting: false, partOfSpecialAuction: false);

        Assert.That(errors.Any(e => e.Error.Contains("only allows bids")), Is.False,
            "No 'bids only before draft' error expected when MaximumReleaseDate < draft date.");
    }

    [Test]
    public void BidsOnlyBeforeNextDraft_GameReleasesOnDraftDate_Blocked()
    {
        var draftDate = new LocalDate(2026, 6, 1);
        var maxRelease = draftDate;
        var game = CreateComplexMasterGame("Game B", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
            [MasterGameTagDictionary.TagDictionary["NG"]]);
        var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: true, pendingDraftScheduledDate: draftDate);
        var currentDate = new LocalDate(2026, 1, 15);

        var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
            leagueYear, game, dropping: false, currentDate, currentDate,
            counterPick: false, counterPickedGameIsManualWillNotRelease: false,
            drafting: false, partOfSpecialAuction: false);

        var bidError = errors.FirstOrDefault(e => e.Error.Contains("only allows bids"));
        Assert.That(bidError, Is.Not.Null, "Expected a 'bids only before draft' error when MaximumReleaseDate == draft date.");
        Assert.That(bidError!.Overridable, Is.False, "The error must be non-overridable.");
    }

    [Test]
    public void BidsOnlyBeforeNextDraft_GameReleasesAfterDraft_Blocked()
    {
        var draftDate = new LocalDate(2026, 6, 1);
        var maxRelease = new LocalDate(2026, 7, 1);
        var game = CreateComplexMasterGame("Game C", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
            [MasterGameTagDictionary.TagDictionary["NG"]]);
        var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: true, pendingDraftScheduledDate: draftDate);
        var currentDate = new LocalDate(2026, 1, 15);

        var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
            leagueYear, game, dropping: false, currentDate, currentDate,
            counterPick: false, counterPickedGameIsManualWillNotRelease: false,
            drafting: false, partOfSpecialAuction: false);

        var bidError = errors.FirstOrDefault(e => e.Error.Contains("only allows bids"));
        Assert.That(bidError, Is.Not.Null, "Expected a 'bids only before draft' error when MaximumReleaseDate > draft date.");
        Assert.That(bidError!.Overridable, Is.False);
    }

    [Test]
    public void BidsOnlyBeforeNextDraft_NullMaximumReleaseDate_Blocked()
    {
        var draftDate = new LocalDate(2026, 6, 1);
        var game = CreateComplexMasterGame("Game D", new LocalDate(2026, 1, 1), null, null, null, null,
            [MasterGameTagDictionary.TagDictionary["NG"]]);
        var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: true, pendingDraftScheduledDate: draftDate);
        var currentDate = new LocalDate(2026, 1, 15);

        var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
            leagueYear, game, dropping: false, currentDate, currentDate,
            counterPick: false, counterPickedGameIsManualWillNotRelease: false,
            drafting: false, partOfSpecialAuction: false);

        var bidError = errors.FirstOrDefault(e => e.Error.Contains("only allows bids"));
        Assert.That(bidError, Is.Not.Null, "Expected a 'bids only before draft' error when MaximumReleaseDate is null.");
        Assert.That(bidError!.Overridable, Is.False);
    }

    [Test]
    public void BidsOnlyBeforeNextDraft_NoDraftScheduledDate_AllBidsBlocked()
    {
        var maxRelease = new LocalDate(2026, 5, 31);
        var game = CreateComplexMasterGame("Game E", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
            [MasterGameTagDictionary.TagDictionary["NG"]]);
        var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: true, pendingDraftScheduledDate: null);
        var currentDate = new LocalDate(2026, 1, 15);

        var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
            leagueYear, game, dropping: false, currentDate, currentDate,
            counterPick: false, counterPickedGameIsManualWillNotRelease: false,
            drafting: false, partOfSpecialAuction: false);

        var bidError = errors.FirstOrDefault(e => e.Error.Contains("only allows bids"));
        Assert.That(bidError, Is.Not.Null, "Expected a 'bids only before draft' error when pending draft has no scheduled date.");
        Assert.That(bidError!.Overridable, Is.False);
    }

    [Test]
    public void BidsOnlyBeforeNextDraft_NoPendingDraft_NoRestriction()
    {
        var maxRelease = new LocalDate(2099, 1, 1);
        var game = CreateComplexMasterGame("Game F", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
            [MasterGameTagDictionary.TagDictionary["NG"]]);
        var leagueYear = CreateLeagueYearForBidTestNoPendingDraft(bidsOnlyBeforeNextScheduledDraft: true);
        var currentDate = new LocalDate(2026, 1, 15);

        var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
            leagueYear, game, dropping: false, currentDate, currentDate,
            counterPick: false, counterPickedGameIsManualWillNotRelease: false,
            drafting: false, partOfSpecialAuction: false);

        Assert.That(errors.Any(e => e.Error.Contains("only allows bids")), Is.False,
            "No bid restriction expected when there is no pending draft.");
    }

    [Test]
    public void BidsOnlyBeforeNextDraft_OptionOff_NoRestriction()
    {
        var draftDate = new LocalDate(2026, 6, 1);
        var maxRelease = new LocalDate(2026, 7, 1);
        var game = CreateComplexMasterGame("Game G", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
            [MasterGameTagDictionary.TagDictionary["NG"]]);
        var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: false, pendingDraftScheduledDate: draftDate);
        var currentDate = new LocalDate(2026, 1, 15);

        var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
            leagueYear, game, dropping: false, currentDate, currentDate,
            counterPick: false, counterPickedGameIsManualWillNotRelease: false,
            drafting: false, partOfSpecialAuction: false);

        Assert.That(errors.Any(e => e.Error.Contains("only allows bids")), Is.False,
            "No bid restriction when option is off.");
    }

    [Test]
    public void BidsOnlyBeforeNextDraft_DroppingIsExempt()
    {
        var draftDate = new LocalDate(2026, 6, 1);
        var maxRelease = new LocalDate(2026, 7, 1);
        var game = CreateComplexMasterGame("Game H", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
            [MasterGameTagDictionary.TagDictionary["NG"]]);
        var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: true, pendingDraftScheduledDate: draftDate);
        var currentDate = new LocalDate(2026, 1, 15);

        var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
            leagueYear, game, dropping: true, currentDate, currentDate,
            counterPick: false, counterPickedGameIsManualWillNotRelease: false,
            drafting: false, partOfSpecialAuction: false);

        Assert.That(errors.Any(e => e.Error.Contains("only allows bids")), Is.False,
            "Drop actions must be exempt from this restriction.");
    }

    [Test]
    public void BidsOnlyBeforeNextDraft_DraftingIsExempt()
    {
        var draftDate = new LocalDate(2026, 6, 1);
        var maxRelease = new LocalDate(2026, 7, 1);
        var game = CreateComplexMasterGame("Game I", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
            [MasterGameTagDictionary.TagDictionary["NG"]]);
        var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: true, pendingDraftScheduledDate: draftDate);
        var currentDate = new LocalDate(2026, 1, 15);

        var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
            leagueYear, game, dropping: false, currentDate, currentDate,
            counterPick: false, counterPickedGameIsManualWillNotRelease: false,
            drafting: true, partOfSpecialAuction: false);

        Assert.That(errors.Any(e => e.Error.Contains("only allows bids")), Is.False,
            "Drafting actions must be exempt from this restriction.");
    }
}
