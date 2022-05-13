namespace FantasyCritic.Lib.Domain;
public class PublicBiddingSet
{
    public PublicBiddingSet(IEnumerable<PublicBiddingMasterGame> masterGames, Instant postedTimestamp)
    {
        MasterGames = masterGames.ToList();
        PostedTimestamp = postedTimestamp;
    }

    public IReadOnlyList<PublicBiddingMasterGame> MasterGames { get; }
    public Instant PostedTimestamp { get; }
}
