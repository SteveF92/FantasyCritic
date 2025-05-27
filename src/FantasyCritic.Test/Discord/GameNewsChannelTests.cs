using System;
using System.Collections.Generic;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Test.TestUtilities;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test.Discord;
internal class GameNewsChannelTests
{
    public static DiscordChannelKey ChannelKey => new DiscordChannelKey(0, 0);

    public static LeagueYear GetTestLeagueYear()
    {
        var league = new League(Guid.Empty, "Test League",
            new MinimalFantasyCriticUser(Guid.Empty, "Test USer", "email@email.com"),
            null, null, new List<int>() {2025}, true, false, false, false ,0);
        var supportedYear = new SupportedYear(2025, true, true, true, new LocalDate(2024, 12, 8), false);

        var leagueTags = new List<LeagueTagStatus>()
        {
            new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned)
        };

        var leagueOptions = new LeagueOptions(10, 5, 2, 1, 0, 0, 0, false, false, false, false, 0, leagueTags,
            new List<SpecialGameSlot>(),
            DraftSystem.Flexible, PickupSystem.SemiPublicBiddingSecretCounterPicks, ScoringSystem.GetDefaultScoringSystem(2025),
            TradingSystem.Standard, TiebreakSystem.LowestProjectedPoints, ReleaseSystem.MustBeReleased,
            new AnnualDate(10, 1), new AnnualDate(10, 1));

        var publishers = new List<Publisher>();
        return new LeagueYear(league, supportedYear, leagueOptions, PlayStatus.DraftFinal, true, new List<EligibilityOverride>(), new List<TagOverride>(), Instant.MinValue, null, publishers, null);
    }

    private static MasterGame CreateBasicMasterGame(LocalDate releaseDate, MasterGameTag tag)
    {
        return new MasterGame(Guid.NewGuid(), "Test Master Game", releaseDate.ToISOString(), releaseDate, releaseDate, null, null, null,
            releaseDate, null, null, null, null, false, null, "", null, null, null, false, false, false, false, false, Instant.MinValue, new FantasyCriticUser() { Id = Guid.Empty }.ToVeryMinimal(),
            new List<MasterSubGame>(), new List<MasterGameTag>() { tag });
    }

    [Test]
    public void BasicTest()
    {
        var setting = new CombinedChannelGameSetting(true, true, GameNewsSetting.All, new List<MasterGameTag>());
        var newGameIsRelevant = setting.NewGameIsRelevant(CreateBasicMasterGame(new LocalDate(2025, 06, 01), MasterGameTagDictionary.TagDictionary["NGF"]), null, ChannelKey, new LocalDate(2025, 04, 02));
        Assert.That(newGameIsRelevant, Is.EqualTo(true));
    }
}
