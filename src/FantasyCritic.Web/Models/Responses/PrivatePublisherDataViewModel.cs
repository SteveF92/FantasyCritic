using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Web.Models.Responses;
public class PrivatePublisherDataViewModel
{
    public PrivatePublisherDataViewModel(IEnumerable<PickupBid> myActiveBids, IEnumerable<DropRequest> myActiveDrops, LocalDate currentDate)
    {
        MyActiveBids = myActiveBids.Select(x => new PickupBidViewModel(x, currentDate)).OrderBy(x => x.Priority).ToList();
        MyActiveDrops = myActiveDrops.Select(x => new DropGameRequestViewModel(x, currentDate)).OrderBy(x => x.Timestamp).ToList();
    }

    public IReadOnlyList<PickupBidViewModel> MyActiveBids { get; }
    public IReadOnlyList<DropGameRequestViewModel> MyActiveDrops { get; }
}
