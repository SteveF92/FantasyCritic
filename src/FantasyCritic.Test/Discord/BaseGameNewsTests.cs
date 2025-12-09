using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;
using NodaTime;
using System;
using System.Collections.Generic;
using FantasyCritic.FakeRepo.TestUtilities;

namespace FantasyCritic.Test.Discord;
internal abstract class BaseGameNewsTests
{
    public static LocalDate CurrentDateForTesting = new LocalDate(2025, 04, 02);
    public static DiscordChannelKey ChannelKey => new DiscordChannelKey(0, 0);

    public static readonly MasterGame Eligible_ReleasedToday =
        CreateBasicMasterGame(new LocalDate(2025, 4, 2), new LocalDate(2025, 4, 2), MasterGameTagDictionary.TagDictionary["NGF"]);

    public static readonly MasterGame Ineligible_ReleasedToday =
        CreateBasicMasterGame(new LocalDate(2025, 4, 2), new LocalDate(2025, 4, 2), MasterGameTagDictionary.TagDictionary["PRT"]);

    public static readonly MasterGame Eligible_Confirmed2025 =
        CreateBasicMasterGame(new LocalDate(2025, 6, 1), new LocalDate(2025, 6, 1), MasterGameTagDictionary.TagDictionary["NGF"]);

    public static readonly MasterGame Ineligible_Confirmed2025 =
        CreateBasicMasterGame(new LocalDate(2025, 6, 1), new LocalDate(2025, 6, 1), MasterGameTagDictionary.TagDictionary["PRT"]);

    public static readonly MasterGame Eligible_MightBe2025 =
        CreateBasicMasterGame(new LocalDate(2025, 6, 1), new LocalDate(2026, 6, 1), MasterGameTagDictionary.TagDictionary["NGF"]);

    public static readonly MasterGame Ineligible_MightBe2025 =
        CreateBasicMasterGame(new LocalDate(2025, 6, 1), new LocalDate(2026, 6, 1), MasterGameTagDictionary.TagDictionary["PRT"]);

    public static readonly MasterGame Unannounced_MightBe2025 =
        CreateBasicMasterGame(new LocalDate(2025, 6, 1), new LocalDate(2026, 6, 1), MasterGameTagDictionary.TagDictionary["UNA"]);

    public static readonly MasterGame Eligible_ConfirmedNot2025 =
        CreateBasicMasterGame(new LocalDate(2026, 1, 1), new LocalDate(2026, 1, 1), MasterGameTagDictionary.TagDictionary["NGF"]);

    public static readonly MasterGame Ineligible_ConfirmedNot2025 =
        CreateBasicMasterGame(new LocalDate(2026, 1, 1), new LocalDate(2026, 1, 1), MasterGameTagDictionary.TagDictionary["PRT"]);

    public static readonly MasterGame Unannounced_ConfirmedNot2025 =
        CreateBasicMasterGame(new LocalDate(2026, 1, 1), new LocalDate(2026, 1, 1), MasterGameTagDictionary.TagDictionary["UNA"]);

    public static LeagueYear GetTestLeagueYear(bool includeGame)
    {
        var league = new League(Guid.Empty, "Test League",
            new MinimalFantasyCriticUser(Guid.Empty, "Test USer", "email@email.com"),
            null, null, new List<MinimalLeagueYearInfo>() { new MinimalLeagueYearInfo(2025, false, PlayStatus.DraftFinal) }, true, false, false, false, 0);
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

        var games = new List<PublisherGame>();
        if (includeGame)
        {
            var game = CreateBasicMasterGame(new LocalDate(2025, 3, 26), new LocalDate(2025, 3, 26), MasterGameTagDictionary.TagDictionary["NGF"]);
            var gameYear = new MasterGameYear(game, 2025);
            games.Add(new PublisherGame(Guid.Empty, Guid.Empty, "Game", Instant.MinValue, false, null, false, null, gameYear, 1, 1, 1, null, null));
        }
        var publishers = new List<Publisher>
        {
            new Publisher(Guid.Empty, new LeagueYearKey(Guid.Empty, 2025), FantasyCriticUser.GetFakeUser(), "Publisher", null, null, 1, games, new List<FormerPublisherGame>(), 100, 0, 0, 0, 0, AutoDraftMode.Off)
        };
        return new LeagueYear(league, supportedYear, leagueOptions, PlayStatus.DraftFinal, true,
            new List<EligibilityOverride>(), new List<TagOverride>(), Instant.MinValue, null, publishers, null);
    }

    private static MasterGame CreateBasicMasterGame(LocalDate minimumReleaseDate, LocalDate maximumReleaseDate, MasterGameTag tag)
    {
        LocalDate? confirmedReleaseDate = minimumReleaseDate == maximumReleaseDate ? minimumReleaseDate : null;
        return new MasterGame(Guid.Empty, "Test Master Game", "Release Date String", minimumReleaseDate, maximumReleaseDate, null, null, null,
            confirmedReleaseDate, null, null, null, null, false, null, "", null, null, null, false, false, false, false, false, Instant.MinValue, new FantasyCriticUser() { Id = Guid.Empty }.ToVeryMinimal(),
            new List<MasterSubGame>(), new List<MasterGameTag>() { tag });
    }
}
