using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Web.Models.Responses;

namespace FantasyCritic.Web.Utilities;

public static class DomainControllerUtilities
{
    public static BidTimesViewModel BuildBidTimesViewModel(IClock clock, SystemWideSettings systemWideSettings)
    {
        var nextPublicRevealTime = clock.GetNextPublicRevealTime();
        var nextBidTime = clock.GetNextBidTime();
        return new BidTimesViewModel(nextPublicRevealTime, nextBidTime, systemWideSettings.ActionProcessingMode);
    }

    public static IReadOnlyList<SingleGameNewsViewModel> BuildUserGameNewsViewModel(LocalDate currentDate, IReadOnlyDictionary<MasterGameYear, List<LeagueYearPublisherPair>> leagueYearPublisherLists)
    {
        return leagueYearPublisherLists.Select(l => new SingleGameNewsViewModel(l.Key, l.Value, true, currentDate)).ToList();
    }
}
