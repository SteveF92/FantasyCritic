using FantasyCritic.Lib.SharedSerialization.Database;

namespace FantasyCritic.FakeRepo.TestUtilities;

internal static class TestLeagueYearDefaults
{
    public static bool DeriveEnableBids(LeagueYearEntity leagueYear, LeagueDraftEntity draft)
    {
        bool oneShotMode = draft.GamesToDraft == leagueYear.StandardGames
                           && draft.CounterPicksToDraft == leagueYear.CounterPicks
                           && leagueYear.UnrestrictedReleaseStatusDroppableGames == 0
                           && leagueYear.WillNotReleaseDroppableGames == 0
                           && leagueYear.WillReleaseDroppableGames == 0
                           && !leagueYear.GrantSuperDrops
                           && leagueYear.TradingSystem == "NoTrades";
        return !oneShotMode;
    }
}
