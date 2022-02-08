using System;

namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public interface IProcessedBid
    {
        PickupBid PickupBid { get; }
        string Outcome { get; }
        decimal ProjectedPointsAtTimeOfBid { get; }
    }
}