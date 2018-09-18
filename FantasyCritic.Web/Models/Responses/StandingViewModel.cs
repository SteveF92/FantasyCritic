using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class StandingViewModel
    {
        public StandingViewModel(Publisher publisher)
        {
            PublisherID = publisher.PublisherID;
            LeagueID = publisher.League.LeagueID;
            PublisherName = publisher.PublisherName;
            LeagueName = publisher.League.LeagueName;
            PlayerName = publisher.User.UserName;
            Year = publisher.Year;
            TotalFantasyScore = publisher.TotalFantasyScore;
        }

        public Guid PublisherID { get; }
        public Guid LeagueID { get; }
        public string PublisherName { get; }
        public string LeagueName { get; }
        public string PlayerName { get; }
        public int Year { get; }
        public decimal TotalFantasyScore { get; }
    }
}
