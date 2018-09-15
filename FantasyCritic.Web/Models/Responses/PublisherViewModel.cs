using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class PublisherViewModel
    {
        public PublisherViewModel(Publisher publisher, IClock clock)
        {
            PublisherID = publisher.PublisherID;
            LeagueID = publisher.League.LeagueID;
            PublisherName = publisher.PublisherName;
            LeagueName = publisher.League.LeagueName;
            PlayerName = publisher.User.UserName;
            Year = publisher.Year;
            DraftPosition = publisher.DraftPosition;
            Games = publisher.PublisherGames.OrderBy(x => x.CounterPick).ThenBy(x => x.Timestamp).Select(x => new PublisherGameViewModel(x, clock)).ToList();
        }

        public Guid PublisherID { get; }
        public Guid LeagueID { get; }
        public string PublisherName { get; }
        public string LeagueName { get; }
        public string PlayerName { get; }
        public int Year { get; }
        public int? DraftPosition { get; }
        public IReadOnlyList<PublisherGameViewModel> Games { get; }
    }
}
