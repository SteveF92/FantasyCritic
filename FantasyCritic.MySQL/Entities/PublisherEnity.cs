using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    internal class PublisherEnity
    {
        public PublisherEnity()
        {

        }

        public PublisherEnity(Publisher publisher)
        {
            PublisherID = publisher.PublisherID;
            PublisherName = publisher.PublisherName;
            LeagueID = publisher.League.LeagueID;
            Year = publisher.Year;
            UserID = publisher.User.UserID;
        }

        public Guid PublisherID { get; set; }
        public string PublisherName { get; set; }
        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public Guid UserID { get; set; }

        public Publisher ToDomain(League league, FantasyCriticUser user, IEnumerable<PublisherGame> publisherGames)
        {
            Publisher domain = new Publisher(PublisherID, league, user, Year, PublisherName, publisherGames);
            return domain;
        }
    }
}
