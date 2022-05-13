namespace FantasyCritic.Web.Models.Responses;
public class PublicBiddingSetViewModel
{
    public PublicBiddingSetViewModel(PublicBiddingSet domain, LocalDate currentDate)
    {
        MasterGames = domain.MasterGames.Select(x => new PublicBiddingMasterGameViewModel(x, currentDate)).ToList();
        PostedTimestamp = domain.PostedTimestamp;
    }

    public IReadOnlyList<PublicBiddingMasterGameViewModel> MasterGames { get; }
    public Instant PostedTimestamp { get; }
}
