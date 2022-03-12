using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.Domain.Requests
{
    public class EditPublisherRequest
    {
        public EditPublisherRequest(Publisher publisher, string newPublisherName, int budget, int freeGamesDropped, int willNotReleaseGamesDropped, int willReleaseGamesDropped)
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
        public int? Budget { get; }
        public int? FreeGamesDropped { get; }
        public int? WillNotReleaseGamesDropped { get; }
        public int? WillReleaseGamesDropped { get; }

        public bool SomethingChanged()
        {
            return NewPublisherName.HasValue ||
                   Budget.HasValue ||
                   FreeGamesDropped.HasValue ||
                   WillNotReleaseGamesDropped.HasValue ||
                   WillReleaseGamesDropped.HasValue;
        }

        public string GetActionString()
        {
            List<string> changes = new List<string>();
            if (NewPublisherName.HasValue)
            {
                changes.Add($"Changed publisher name to {NewPublisherName.Value}");
            }
            if (Budget.HasValue)
            {
                changes.Add($"Changed budget to {Budget.Value}");
            }
            if (FreeGamesDropped.HasValue)
            {
                changes.Add($"Changed 'unrestricted games dropped' to {FreeGamesDropped.Value}");
            }
            if (WillNotReleaseGamesDropped.HasValue)
            {
                changes.Add($"Changed 'will not release games dropped' to {WillNotReleaseGamesDropped.Value}");
            }
            if (WillReleaseGamesDropped.HasValue)
            {
                changes.Add($"Changed 'will release games dropped' to {WillReleaseGamesDropped.Value}");
            }

            if (changes.Count == 0)
            {
                return "Nothing changed. This is a bug.";
            }
            if (changes.Count == 1)
            {
                return changes.Single() + ".";
            }

            string joinedString = string.Join("; ", changes) + ".";
            return joinedString;
        }
    }
}
