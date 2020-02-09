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
        public UpcomingGameViewModel(MasterGameYear masterGame, IEnumerable<Publisher> publishers, IEnumerable<Publisher> standardGamePublishers, bool userMode)
        {
            MasterGameID = masterGame.MasterGame.MasterGameID;
            GameName = masterGame.MasterGame.GameName;
            EstimatedReleaseDate = masterGame.MasterGame.EstimatedReleaseDate;
            SortableEstimatedReleaseDate = masterGame.MasterGame.SortableEstimatedReleaseDate ?? LocalDate.MaxIsoValue;
            ReleaseDate = masterGame.MasterGame.ReleaseDate;

            if (userMode)
            {
                if (publishers.Count() == 1)
                {
                    var publisher = publishers.Single();
                    LeagueID = publisher.LeagueYear.League.LeagueID;
                    Year = publisher.LeagueYear.Year;
                    LeagueName = publisher.LeagueYear.League.LeagueName;
                    PublisherID = publisher.PublisherID;
                    PublisherName = publisher.PublisherName;
                }
                else
                {
                    LeagueName = "Multiple";
                    PublisherName = "Multiple";
                }
            }
            else
            {
                if (publishers.Count() == 1)
                {
                    var publisher = publishers.Single();
                    LeagueID = publisher.LeagueYear.League.LeagueID;
                    Year = publisher.LeagueYear.Year;
                    LeagueName = publisher.LeagueYear.League.LeagueName;
                    PublisherID = publisher.PublisherID;
                    PublisherName = publisher.PublisherName;
                }
                else if (standardGamePublishers.Count() == 1)
                {
                    var publisher = standardGamePublishers.Single();
                    var counterPickPublisher = publishers.Single(x => x.PublisherID != publisher.PublisherID);
                    LeagueID = publisher.LeagueYear.League.LeagueID;
                    Year = publisher.LeagueYear.Year;
                    LeagueName = publisher.LeagueYear.League.LeagueName;
                    PublisherID = publisher.PublisherID;
                    PublisherName = publisher.PublisherName + $" - Counter Picked by {counterPickPublisher.PublisherName}";
                }
                else
                {
                    throw new Exception($"Problem with upcoming games. Happened for Game: {masterGame.MasterGame.GameName} and publisherIDs: {string.Join('|', publishers.Select(x => x.PublisherID))}");
                }
            }
        }

        public Guid MasterGameID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public LocalDate SortableEstimatedReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public Guid? LeagueID { get; }
        public int? Year { get; }
        public string LeagueName { get; }
        public Guid? PublisherID { get; }
        public string PublisherName { get; }
    }
}
