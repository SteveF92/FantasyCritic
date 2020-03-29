using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.Domain.Requests
{
    public class EditPublisherRequest
    {
        public EditPublisherRequest(Publisher publisher, string newPublisherName, uint budget, int freeGamesDropped, int willNotReleaseGamesDropped, int willReleaseGamesDropped)
        {
            Publisher = publisher;
            if (publisher.PublisherName != newPublisherName)
            {
                NewPublisherName = newPublisherName;
            }
            if (publisher.Budget != budget)
            {
                Budget = budget;
            }
            if (publisher.FreeGamesDropped != freeGamesDropped)
            {
                FreeGamesDropped = freeGamesDropped;
            }
            if (publisher.WillNotReleaseGamesDropped != willNotReleaseGamesDropped)
            {
                WillNotReleaseGamesDropped = willNotReleaseGamesDropped;
            }
            if (publisher.WillReleaseGamesDropped != willReleaseGamesDropped)
            {
                WillReleaseGamesDropped = willReleaseGamesDropped;
            }
        }

        public Publisher Publisher { get; }
        public Maybe<string> NewPublisherName { get; }
        public uint? Budget { get; }
        public int? FreeGamesDropped { get; }
        public int? WillNotReleaseGamesDropped { get; }
        public int? WillReleaseGamesDropped { get; }
    }
}
