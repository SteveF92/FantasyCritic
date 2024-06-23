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
}
