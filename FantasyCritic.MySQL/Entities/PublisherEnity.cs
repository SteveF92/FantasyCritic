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
            LeagueID = publisher.League.LeagueID;
            Year = publisher.Year;
            UserID = publisher.User.UserID;
            PublisherName = publisher.PublisherName;
        }

        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public Guid UserID { get; set; }
        public string PublisherName { get; set; }

        public Publisher ToDomain(League league, FantasyCriticUser user, IEnumerable<PublisherGame> publisherGames)
        {
            Publisher domain = new Publisher(league, user, Year, PublisherName, publisherGames);
            return domain;
        }
    }
}
