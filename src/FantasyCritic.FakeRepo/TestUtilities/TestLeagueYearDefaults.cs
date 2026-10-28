using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.SharedSerialization.Database;

namespace FantasyCritic.FakeRepo.TestUtilities;

internal static class TestLeagueYearDefaults
{
    public static bool DeriveEnableBids(LeagueYearEntity leagueYear, LeagueDraftEntity draft, IReadOnlyList<LeagueSlot> leagueSlots)
    {
        bool oneShotMode = draft.GamesToDraft == leagueSlots.Count(x => !x.CounterPick)
                           && draft.CounterPicksToDraft == leagueSlots.Count(x => x.CounterPick)
                           && leagueYear.UnrestrictedReleaseStatusDroppableGames == 0
                           && leagueYear.WillNotReleaseDroppableGames == 0
                           && leagueYear.WillReleaseDroppableGames == 0
                           && !leagueYear.GrantSuperDrops
                           && leagueYear.TradingSystem == "NoTrades";
        return !oneShotMode;
    }
}
