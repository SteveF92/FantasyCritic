namespace FantasyCritic.Lib.Domain.Combinations;

public class LeagueYearPublicBiddingSet
{
    public LeagueYearPublicBiddingSet(LeagueYear leagueYear, IEnumerable<PublicBiddingMasterGame> masterGames)
    {
        LeagueYear = leagueYear;
        MasterGames = masterGames.ToList();
    }

    public LeagueYear LeagueYear { get; }
    public IReadOnlyList<PublicBiddingMasterGame> MasterGames { get; }
}
