using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Test.TestUtilities;
using NodaTime;
using System;
using System.Collections.Generic;

namespace FantasyCritic.Test.Discord;
internal abstract class BaseGameNewsTests
{
    public static LocalDate CurrentDateForTesting = new LocalDate(2025, 04, 02);
    public static DiscordChannelKey ChannelKey => new DiscordChannelKey(0, 0);

    // ========== PAST RELEASES ==========

    // Eligible (NGF) - Released Last Week
    public static readonly MasterGame Eligible_Past_ReleasedLastWeek_NoScore =
        CreateBasicMasterGame(new LocalDate(2025, 3, 26), new LocalDate(2025, 3, 26), MasterGameTagDictionary.TagDictionary["NGF"], null);
    public static readonly MasterGame Eligible_Past_ReleasedLastWeek_Score75 =
        CreateBasicMasterGame(new LocalDate(2025, 3, 26), new LocalDate(2025, 3, 26), MasterGameTagDictionary.TagDictionary["NGF"], 75m);
    public static readonly MasterGame Eligible_Past_ReleasedLastWeek_Score90 =
        CreateBasicMasterGame(new LocalDate(2025, 3, 26), new LocalDate(2025, 3, 26), MasterGameTagDictionary.TagDictionary["NGF"], 90m);

    // Ineligible (PRT) - Released Last Week
    public static readonly MasterGame Ineligible_Past_ReleasedLastWeek_NoScore =
        CreateBasicMasterGame(new LocalDate(2025, 3, 26), new LocalDate(2025, 3, 26), MasterGameTagDictionary.TagDictionary["PRT"], null);
    public static readonly MasterGame Ineligible_Past_ReleasedLastWeek_Score75 =
        CreateBasicMasterGame(new LocalDate(2025, 3, 26), new LocalDate(2025, 3, 26), MasterGameTagDictionary.TagDictionary["PRT"], 75m);
    public static readonly MasterGame Ineligible_Past_ReleasedLastWeek_Score90 =
        CreateBasicMasterGame(new LocalDate(2025, 3, 26), new LocalDate(2025, 3, 26), MasterGameTagDictionary.TagDictionary["PRT"], 90m);

    // Unannounced (UNA) - Released Last Week
    public static readonly MasterGame Unannounced_Past_ReleasedLastWeek_NoScore =
        CreateBasicMasterGame(new LocalDate(2025, 3, 26), new LocalDate(2025, 3, 26), MasterGameTagDictionary.TagDictionary["UNA"], null);
    public static readonly MasterGame Unannounced_Past_ReleasedLastWeek_Score75 =
        CreateBasicMasterGame(new LocalDate(2025, 3, 26), new LocalDate(2025, 3, 26), MasterGameTagDictionary.TagDictionary["UNA"], 75m);
    public static readonly MasterGame Unannounced_Past_ReleasedLastWeek_Score90 =
        CreateBasicMasterGame(new LocalDate(2025, 3, 26), new LocalDate(2025, 3, 26), MasterGameTagDictionary.TagDictionary["UNA"], 90m);

    // Eligible (NGF) - Released Today
    public static readonly MasterGame Eligible_Past_ReleasedToday_NoScore =
        CreateBasicMasterGame(new LocalDate(2025, 4, 2), new LocalDate(2025, 4, 2), MasterGameTagDictionary.TagDictionary["NGF"], null);
    public static readonly MasterGame Eligible_Past_ReleasedToday_Score75 =
        CreateBasicMasterGame(new LocalDate(2025, 4, 2), new LocalDate(2025, 4, 2), MasterGameTagDictionary.TagDictionary["NGF"], 75m);
    public static readonly MasterGame Eligible_Past_ReleasedToday_Score90 =
        CreateBasicMasterGame(new LocalDate(2025, 4, 2), new LocalDate(2025, 4, 2), MasterGameTagDictionary.TagDictionary["NGF"], 90m);

    // Ineligible (PRT) - Released Today
    public static readonly MasterGame Ineligible_Past_ReleasedToday_NoScore =
        CreateBasicMasterGame(new LocalDate(2025, 4, 2), new LocalDate(2025, 4, 2), MasterGameTagDictionary.TagDictionary["PRT"], null);
    public static readonly MasterGame Ineligible_Past_ReleasedToday_Score75 =
        CreateBasicMasterGame(new LocalDate(2025, 4, 2), new LocalDate(2025, 4, 2), MasterGameTagDictionary.TagDictionary["PRT"], 75m);
    public static readonly MasterGame Ineligible_Past_ReleasedToday_Score90 =
        CreateBasicMasterGame(new LocalDate(2025, 4, 2), new LocalDate(2025, 4, 2), MasterGameTagDictionary.TagDictionary["PRT"], 90m);

    // Unannounced (UNA) - Released Today
    public static readonly MasterGame Unannounced_Past_ReleasedToday_NoScore =
        CreateBasicMasterGame(new LocalDate(2025, 4, 2), new LocalDate(2025, 4, 2), MasterGameTagDictionary.TagDictionary["UNA"], null);
    public static readonly MasterGame Unannounced_Past_ReleasedToday_Score75 =
        CreateBasicMasterGame(new LocalDate(2025, 4, 2), new LocalDate(2025, 4, 2), MasterGameTagDictionary.TagDictionary["UNA"], 75m);
    public static readonly MasterGame Unannounced_Past_ReleasedToday_Score90 =
        CreateBasicMasterGame(new LocalDate(2025, 4, 2), new LocalDate(2025, 4, 2), MasterGameTagDictionary.TagDictionary["UNA"], 90m);

    // ========== FUTURE RELEASES ==========

    // Eligible (NGF)
    public static readonly MasterGame Eligible_Future_Confirmed2025 =
        CreateBasicMasterGame(new LocalDate(2025, 6, 1), new LocalDate(2025, 6, 1), MasterGameTagDictionary.TagDictionary["NGF"], null);
    public static readonly MasterGame Eligible_Future_MightBe2025 =
        CreateBasicMasterGame(new LocalDate(2025, 6, 1), new LocalDate(2026, 6, 1), MasterGameTagDictionary.TagDictionary["NGF"], null);
    public static readonly MasterGame Eligible_Future_ConfirmedNot2025 =
        CreateBasicMasterGame(new LocalDate(2026, 1, 1), new LocalDate(2026, 1, 1), MasterGameTagDictionary.TagDictionary["NGF"], null);

    // Ineligible (PRT)
    public static readonly MasterGame Ineligible_Future_Confirmed2025 =
        CreateBasicMasterGame(new LocalDate(2025, 6, 1), new LocalDate(2025, 6, 1), MasterGameTagDictionary.TagDictionary["PRT"], null);
    public static readonly MasterGame Ineligible_Future_MightBe2025 =
        CreateBasicMasterGame(new LocalDate(2025, 6, 1), new LocalDate(2026, 6, 1), MasterGameTagDictionary.TagDictionary["PRT"], null);
    public static readonly MasterGame Ineligible_Future_ConfirmedNot2025 =
        CreateBasicMasterGame(new LocalDate(2026, 1, 1), new LocalDate(2026, 1, 1), MasterGameTagDictionary.TagDictionary["PRT"], null);

    // Unannounced (UNA)
    public static readonly MasterGame Unannounced_Future_Confirmed2025 =
        CreateBasicMasterGame(new LocalDate(2025, 6, 1), new LocalDate(2025, 6, 1), MasterGameTagDictionary.TagDictionary["UNA"], null);
    public static readonly MasterGame Unannounced_Future_MightBe2025 =
        CreateBasicMasterGame(new LocalDate(2025, 6, 1), new LocalDate(2026, 6, 1), MasterGameTagDictionary.TagDictionary["UNA"], null);
    public static readonly MasterGame Unannounced_Future_ConfirmedNot2025 =
        CreateBasicMasterGame(new LocalDate(2026, 1, 1), new LocalDate(2026, 1, 1), MasterGameTagDictionary.TagDictionary["UNA"], null);


    public static LeagueYear GetTestLeagueYear()
    {
        var league = new League(Guid.Empty, "Test League",
            new MinimalFantasyCriticUser(Guid.Empty, "Test USer", "email@email.com"),
            null, null, new List<int>() { 2025 }, true, false, false, false, 0);
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

    private static MasterGame CreateBasicMasterGame(LocalDate minimumReleaseDate, LocalDate maximumReleaseDate, MasterGameTag tag, decimal? criticScore)
    {
        LocalDate? confirmedReleaseDate = minimumReleaseDate == maximumReleaseDate ? minimumReleaseDate : null;
        return new MasterGame(Guid.NewGuid(), "Test Master Game", "Release Date String", minimumReleaseDate, maximumReleaseDate, null, null, null,
            confirmedReleaseDate, null, null, null, criticScore, false, null, "", null, null, null, false, false, false, false, false, Instant.MinValue, new FantasyCriticUser() { Id = Guid.Empty }.ToVeryMinimal(),
            new List<MasterSubGame>(), new List<MasterGameTag>() { tag });
    }
}
