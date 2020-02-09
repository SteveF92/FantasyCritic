using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;
using Org.BouncyCastle.Asn1.Mozilla;

namespace FantasyCritic.Web.Models.Responses
{
    public class UpcomingGameViewModel
    {
        public UpcomingGameViewModel(MasterGameYear masterGame, Publisher publisher, IClock clock)
        {
            MasterGame = new MasterGameYearViewModel(masterGame, clock);
            LeagueID = publisher.LeagueYear.League.LeagueID;
            Year = publisher.LeagueYear.Year;
            LeagueName = publisher.LeagueYear.League.LeagueName;
            PublisherID = publisher.PublisherID;
            PublisherName = publisher.PublisherName;
        }

        public MasterGameYearViewModel MasterGame { get; }
        public Guid LeagueID { get; }
        public int Year { get; }
        public string LeagueName { get; }
        public Guid PublisherID { get; }
        public string PublisherName { get; }
    }
}
