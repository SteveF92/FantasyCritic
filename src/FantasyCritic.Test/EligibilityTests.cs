using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Domain;
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
    private static readonly MasterGameTagType RemakeLevelType = new MasterGameTagType("RemakeLevel");
    private static readonly MasterGameTagType OtherType = new MasterGameTagType("Other");

    private static readonly Dictionary<string, MasterGameTag> _tagDictionary = new List<MasterGameTag>()
    {
        new MasterGameTag("Cancelled", "Cancelled", "CNCL", OtherType, false, false, "", new List<string>(), ""),
        new MasterGameTag("CurrentlyInEarlyAccess", "Currently in Early Access", "C-EA", OtherType, true, false, "", new List<string>(), ""),
        new MasterGameTag("DirectorsCut", "Director's Cut", "DC", RemakeLevelType, false, false, "", new List<string>(), ""),
        new MasterGameTag("ExpansionPack", "Expansion Pack", "EXP", OtherType, false, false, "", new List<string>(), ""),
        new MasterGameTag("FreeToPlay", "Free to Play", "FTP", OtherType, false, false, "", new List<string>(), ""),
        new MasterGameTag("NewGame", "New Game", "NG", RemakeLevelType, false, false, "", new List<string>(), ""),
        new MasterGameTag("NewGamingFranchise", "New Gaming Franchise", "NGF", OtherType, false, false, "", new List<string>(), ""),
        new MasterGameTag("PartialRemake", "Partial Remake", "P-RMKE", RemakeLevelType, false, false, "", new List<string>(), ""),
        new MasterGameTag("PlannedForEarlyAccess", "Planned for Early Access", "P-EA", OtherType, true, false, "", new List<string>(), ""),
        new MasterGameTag("Port", "Port", "PRT", RemakeLevelType, false, false, "", new List<string>(), ""),
        new MasterGameTag("Reimagining", "Reimagining", "RIMG", RemakeLevelType, false, false, "", new List<string>(), ""),
        new MasterGameTag("ReleasedInternationally", "Released Internationally", "R-INT", OtherType, true, false, "", new List<string>(), ""),
        new MasterGameTag("Remake", "Remake", "RMKE", RemakeLevelType, false, false, "", new List<string>(), ""),
        new MasterGameTag("Remaster", "Remaster", "RMSTR", RemakeLevelType, false, false, "", new List<string>(), ""),
        new MasterGameTag("UnannouncedGame", "Unannounced Game", "UNA", OtherType, true, false, "", new List<string>(), ""),
        new MasterGameTag("VirtualReality", "Virtual Reality", "VR", OtherType, false, false, "", new List<string>(), ""),
        new MasterGameTag("WillReleaseInternationallyFirst", "Will Release Internationally First", "W-INT", OtherType, true, false, "", new List<string>(), ""),
        new MasterGameTag("YearlyInstallment", "Yearly Installment", "YI", OtherType, false, false, "", new List<string>(), ""),
    }.ToDictionary(x => x.ShortName);

    private static MasterGame CreateBasicMasterGame(string name, LocalDate releaseDate, MasterGameTag tag)
    {
        return new MasterGame(Guid.NewGuid(), name, releaseDate.ToISOString(), releaseDate, releaseDate, null, null, null,
            releaseDate, null, null, null, null, false, null, "", null, null, null, false, false, false, false, false, Instant.MinValue, new FantasyCriticUser() { Id = Guid.Empty }, 
            new List<MasterSubGame>(), new List<MasterGameTag>() { tag });
    }

    private static MasterGame CreateComplexMasterGame(string name, LocalDate minimumReleaseDate, LocalDate? maximumReleaseDate,
        LocalDate? earlyAccessReleaseDate, LocalDate? internationalReleaseDate, LocalDate? announcementDate, IEnumerable<MasterGameTag> tags)
    {
        return new MasterGame(Guid.NewGuid(), name, "TBA", minimumReleaseDate, maximumReleaseDate,
            earlyAccessReleaseDate, internationalReleaseDate, announcementDate, null, null, null, null, null, false, null, "", null, null, null, false, false, false, false, false,
            Instant.MinValue, new FantasyCriticUser() { Id = Guid.Empty }, new List<MasterSubGame>(), tags);
    }

    [Test]
    public void SimpleEligibleTest()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateBasicMasterGame("Elden Ring", new LocalDate(2022, 2, 25), _tagDictionary["NGF"]);

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned)
        };

        var slotTags = new List<LeagueTagStatus>();

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(0, claimErrors.Count);
    }

    [Test]
    public void SimpleInEligibleTest()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateBasicMasterGame("GTA 5 (PS5)", new LocalDate(2022, 2, 25), _tagDictionary["PRT"]);

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned)
        };

        var slotTags = new List<LeagueTagStatus>();

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(1, claimErrors.Count);
        Assert.AreEqual("That game is not eligible because the Port tag has been banned", claimErrors[0].Error);
    }

    [Test]
    public void SlotEligibleTest()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateBasicMasterGame("Elden Ring", new LocalDate(2022, 2, 25), _tagDictionary["NGF"]);

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned)
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["NGF"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(0, claimErrors.Count);
    }

    [Test]
    public void SlotInEligibleTest()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateBasicMasterGame("Horizon Forbidden West", new LocalDate(2022, 2, 25), _tagDictionary["NG"]);

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned)
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["NGF"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(1, claimErrors.Count);
        Assert.AreEqual("That game is not eligible because it does not have any of the following required tags: (New Gaming Franchise)", claimErrors[0].Error);
    }

    [Test]
    public void EarlyAccessHasGameBeforeEarlyAccessEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Have a Nice Death", new LocalDate(2022, 1, 3), null,
            new LocalDate(2022, 3, 6), null, null, new List<MasterGameTag>()
            {
                _tagDictionary["NG"],
                _tagDictionary["C-EA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>();

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(0, claimErrors.Count);
    }

    [Test]
    public void EarlyAccessHasGameAfterEarlyAccessInEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-03-10T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Have a Nice Death", new LocalDate(2022, 1, 3), null,
            new LocalDate(2022, 3, 6), null, null, new List<MasterGameTag>()
            {
                _tagDictionary["NG"],
                _tagDictionary["C-EA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>();

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(1, claimErrors.Count);
        Assert.AreEqual("That game is not eligible because the Currently in Early Access tag has been banned", claimErrors[0].Error);
    }

    [Test]
    public void EarlyAccessAllowedEligibleTest()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Baldur's Gate 3", new LocalDate(2022, 1, 3), null,
            new LocalDate(2020, 10, 6), null, null, new List<MasterGameTag>()
            {
                _tagDictionary["NG"],
                _tagDictionary["C-EA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>();

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(0, claimErrors.Count);
    }

    [Test]
    public void EarlyAccessNormalAllowedSlotRequiredEligibleTest()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Baldur's Gate 3", new LocalDate(2022, 1, 3), null,
            new LocalDate(2020, 10, 6), null, null, new List<MasterGameTag>()
            {
                _tagDictionary["NG"],
                _tagDictionary["C-EA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["P-EA"], TagStatus.Required),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(0, claimErrors.Count);
    }

    [Test]
    public void EarlyAccessNormalBannedSlotRequiredEligibleTest()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Baldur's Gate 3", new LocalDate(2022, 1, 3), null,
            new LocalDate(2020, 10, 6), null, null, new List<MasterGameTag>()
            {
                _tagDictionary["NG"],
                _tagDictionary["C-EA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["P-EA"], TagStatus.Required),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(0, claimErrors.Count);
    }

    [Test]
    public void EarlyAccessReleasedInternationallyComplexSlotEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-31T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Baldur's Gate 3", new LocalDate(2022, 1, 3), null,
            new LocalDate(2020, 10, 6), null, null, new List<MasterGameTag>()
            {
                _tagDictionary["NG"],
                _tagDictionary["C-EA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Required),
            new LeagueTagStatus(_tagDictionary["R-INT"], TagStatus.Required),
            new LeagueTagStatus(_tagDictionary["NG"], TagStatus.Required),
            new LeagueTagStatus(_tagDictionary["NGF"], TagStatus.Required),
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(0, claimErrors.Count);
    }

    [Test]
    public void UnannouncedSimpleEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Star Wars Jedi: Fallen Order 2", new LocalDate(2022, 1, 27), null,
            null, null, null, new List<MasterGameTag>()
            {
                _tagDictionary["NG"],
                _tagDictionary["UNA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {

        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(0, claimErrors.Count);
    }

    [Test]
    public void UnannouncedSimpleInEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Star Wars Jedi: Fallen Order 2", new LocalDate(2022, 1, 27), null,
            null, null, null, new List<MasterGameTag>()
            {
                _tagDictionary["NG"],
                _tagDictionary["UNA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["R-INT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["UNA"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {

        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(1, claimErrors.Count);
        Assert.AreEqual("That game is not eligible because the Unannounced Game tag has been banned", claimErrors[0].Error);
    }

    [Test]
    public void PreviouslyUnannouncedEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Star Wars Jedi: Fallen Order 2", new LocalDate(2022, 1, 27), null,
            null, null, new LocalDate(2022, 1, 25), new List<MasterGameTag>()
            {
                _tagDictionary["NG"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["UNA"], TagStatus.Required),
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(0, claimErrors.Count);
    }

    [Test]
    public void PreviouslyUnannouncedLeagueBannedEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Star Wars Jedi: Fallen Order 2", new LocalDate(2022, 1, 27), null,
            null, null, new LocalDate(2022, 1, 25), new List<MasterGameTag>()
            {
                _tagDictionary["NG"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["R-INT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["UNA"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {

        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(0, claimErrors.Count);
    }

    [Test]
    public void PreviouslyUnannouncedInEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-26T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Star Wars Jedi: Fallen Order 2", new LocalDate(2022, 1, 27), null,
            null, null, new LocalDate(2022, 1, 25), new List<MasterGameTag>()
            {
                _tagDictionary["NG"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["UNA"], TagStatus.Required),
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(1, claimErrors.Count);
        Assert.AreEqual("That game is not eligible because it does not have any of the following required tags: (Unannounced Game)", claimErrors[0].Error);
    }

    [Test]
    public void UnannouncedRemakeEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("The Last of Us Remake", new LocalDate(2022, 1, 27), null,
            null, null, null, new List<MasterGameTag>()
            {
                _tagDictionary["RMKE"],
                _tagDictionary["UNA"]
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["RMKE"], TagStatus.Required),
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(0, claimErrors.Count);
    }

    [Test]
    public void UnannouncedRemakeInEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("The Last of Us Remake", new LocalDate(2022, 1, 27), null,
            null, null, null, new List<MasterGameTag>()
            {
                _tagDictionary["RMKE"],
                _tagDictionary["UNA"]
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["R-INT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["RMKE"], TagStatus.Banned)
        };

        var slotTags = new List<LeagueTagStatus>()
        {

        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(1, claimErrors.Count);
        Assert.AreEqual("That game is not eligible because the Remake tag has been banned", claimErrors[0].Error);
    }

    [Test]
    public void PreviouslyUnannouncedRemakeEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-29T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("The Last of Us Remake", new LocalDate(2022, 2, 3), null,
            null, null, new LocalDate(2022, 2, 2), new List<MasterGameTag>()
            {
                _tagDictionary["RMKE"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["RMKE"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(0, claimErrors.Count);
    }

    [Test]
    public void PreviouslyUnannouncedRemakeInEligible()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-02-07T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("The Last of Us Remake", new LocalDate(2022, 2, 8), null,
            null, null, new LocalDate(2022, 2, 2), new List<MasterGameTag>()
            {
                _tagDictionary["RMKE"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["UNA"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(1, claimErrors.Count);
        Assert.AreEqual("That game is not eligible because it does not have any of the following required tags: (Unannounced Game)", claimErrors[0].Error);
    }

    [Test]
    public void UnannouncedGameFailsSlotConditions()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-05T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Star Wars Jedi: Fallen Order 2", new LocalDate(2022, 1, 27), null,
            null, null, null, new List<MasterGameTag>()
            {
                _tagDictionary["NG"],
                _tagDictionary["UNA"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["RMKE"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(1, claimErrors.Count);
        Assert.AreEqual("That game is not eligible because it does not have any of the following required tags: (Remake)", claimErrors[0].Error);
    }

    [Test]
    public void PreviouslyUnannouncedGameFailsSlotConditions()
    {
        Instant acquisitionTime = InstantPattern.ExtendedIso.Parse("2022-01-26T20:49:24Z").GetValueOrThrow();
        var acquisitionDate = acquisitionTime.ToEasternDate();

        MasterGame masterGame = CreateComplexMasterGame("Star Wars Jedi: Fallen Order 2", new LocalDate(2022, 1, 27), null,
            null, null, new LocalDate(2022, 1, 25), new List<MasterGameTag>()
            {
                _tagDictionary["NG"],
            });

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["C-EA"], TagStatus.Banned),
            new LeagueTagStatus(_tagDictionary["R-INT"], TagStatus.Banned),
        };

        var slotTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(_tagDictionary["RMKE"], TagStatus.Required)
        };

        var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, acquisitionDate);
        Assert.AreEqual(1, claimErrors.Count);
        Assert.AreEqual("That game is not eligible because it does not have any of the following required tags: (Remake)", claimErrors[0].Error);
    }
}
