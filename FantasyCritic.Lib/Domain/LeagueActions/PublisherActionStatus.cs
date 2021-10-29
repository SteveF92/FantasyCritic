using System;

namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public class PublisherActionStatus
    {
        public PublisherActionStatus(Guid publisherID, uint budget, int freeGamesDropped, int willNotReleaseGamesDropped, int willReleaseGamesDropped)
        {
            PublisherID = publisherID;
            Budget = budget;
            FreeGamesDropped = freeGamesDropped;
            WillNotReleaseGamesDropped = willNotReleaseGamesDropped;
            WillReleaseGamesDropped = willReleaseGamesDropped;
        }

        public Guid PublisherID { get; }
        public uint Budget { get; }
        public int FreeGamesDropped { get; }
        public int WillNotReleaseGamesDropped { get; }
        public int WillReleaseGamesDropped { get; }
    }
}
