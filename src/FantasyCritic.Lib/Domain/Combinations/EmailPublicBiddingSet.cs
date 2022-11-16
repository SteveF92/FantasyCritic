namespace FantasyCritic.Lib.Domain.Combinations;

public class EmailPublicBiddingSet
{
    public EmailPublicBiddingSet(LeagueYear leagueYear, IEnumerable<PublicBiddingMasterGame> masterGames)
    {
        LeagueYear = leagueYear;
        MasterGames = masterGames.ToList();
    }

    public LeagueYear LeagueYear { get; }
    public IReadOnlyList<PublicBiddingMasterGame> MasterGames { get; }
}
