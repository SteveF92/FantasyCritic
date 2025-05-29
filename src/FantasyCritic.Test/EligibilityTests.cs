using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Test.TestUtilities;
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
            releaseDate, null, null, null, null, false, null, "", null, null, null, false, false, false, false, false, Instant.MinValue, new FantasyCriticUser() { Id = Guid.Empty }.ToVeryMinimal(), 
            new List<MasterSubGame>(), new List<MasterGameTag>() { tag });
    }

    private static MasterGame CreateComplexMasterGame(string name, LocalDate minimumReleaseDate, LocalDate? maximumReleaseDate,
        LocalDate? earlyAccessReleaseDate, LocalDate? internationalReleaseDate, LocalDate? announcementDate, IEnumerable<MasterGameTag> tags)
    {
        return new MasterGame(Guid.NewGuid(), name, "TBA", minimumReleaseDate, maximumReleaseDate,
            earlyAccessReleaseDate, internationalReleaseDate, announcementDate, null, null, null, null, null, false, null, "", null, null, null, false, false, false, false, false,
            Instant.MinValue, new FantasyCriticUser() { Id = Guid.Empty }.ToVeryMinimal(), new List<MasterSubGame>(), tags);
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
}
