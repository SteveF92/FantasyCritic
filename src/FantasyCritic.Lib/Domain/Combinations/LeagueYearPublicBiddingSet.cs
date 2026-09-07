namespace FantasyCritic.Lib.Domain.Combinations;

public class LeagueYearPublicBiddingSet
{
    public LeagueYearPublicBiddingSet(LeagueYear leagueYear, IEnumerable<PublicBiddingMasterGame> masterGames, bool anyHiddenCounterPickBids)
    {
        LeagueYear = leagueYear;
        MasterGames = masterGames.ToList();
        AnyHiddenCounterPickBids = anyHiddenCounterPickBids;
    }

    public LeagueYear LeagueYear { get; }
    public IReadOnlyList<PublicBiddingMasterGame> MasterGames { get; }

    /// <inheritdoc cref="PublicBiddingSet.AnyHiddenCounterPickBids"/>
    public bool AnyHiddenCounterPickBids { get; }
}
