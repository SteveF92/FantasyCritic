namespace FantasyCritic.Lib.Domain;

public class PublicBiddingSet
{
    public PublicBiddingSet(IEnumerable<PublicBiddingMasterGame> masterGames, Instant postedTimestamp, bool anyHiddenCounterPickBids)
    {
        MasterGames = masterGames.ToList();
        PostedTimestamp = postedTimestamp;
        AnyHiddenCounterPickBids = anyHiddenCounterPickBids;
    }

    public IReadOnlyList<PublicBiddingMasterGame> MasterGames { get; }
    public Instant PostedTimestamp { get; }

    /// <summary>
    /// Whether at least one game is being bid on as a counter pick, in a league whose system hides which games those are. This is
    /// deliberately a yes/no answer and never a count: a count would change every time somebody placed or cancelled a counter pick
    /// bid, whereas a "yes" stays "yes" no matter what else happens during the week.
    /// </summary>
    public bool AnyHiddenCounterPickBids { get; }
}
