using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    internal class PublisherEntity
    {
        public PublisherEntity()
        {

        }

        public PublisherEntity(Publisher publisher)
        {
            PublisherID = publisher.PublisherID;
            PublisherName = publisher.PublisherName;
            LeagueID = publisher.LeagueYear.League.LeagueID;
            Year = publisher.LeagueYear.Year;
            UserID = publisher.User.UserID;
            DraftPosition = publisher.DraftPosition;
            FreeGamesDropped = publisher.FreeGamesDropped;
            WillNotReleaseGamesDropped = publisher.WillNotReleaseGamesDropped;
            WillReleaseGamesDropped = publisher.WillReleaseGamesDropped;
            Budget = publisher.Budget;
            AutoDraft = publisher.AutoDraft;
        }

        public Guid PublisherID { get; set; }
        public string PublisherName { get; set; }
        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public Guid UserID { get; set; }
        public int DraftPosition { get; set; }
        public int FreeGamesDropped { get; set; }
        public int WillNotReleaseGamesDropped { get; set; }
        public int WillReleaseGamesDropped { get; set; }
        public uint Budget { get; set; }
        public bool AutoDraft { get; set; }

        public Publisher ToDomain(LeagueYear leagueYear, FantasyCriticUser user, IEnumerable<PublisherGame> publisherGames)
        {
            Publisher domain = new Publisher(PublisherID, leagueYear, user, PublisherName, DraftPosition, publisherGames, 
                Budget, FreeGamesDropped, WillNotReleaseGamesDropped, WillReleaseGamesDropped, AutoDraft);
            return domain;
        }
    }
}
