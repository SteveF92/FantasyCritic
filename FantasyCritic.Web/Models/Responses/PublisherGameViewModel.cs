using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class PublisherGameViewModel
    {
        public PublisherGameViewModel(PublisherGame publisherGame, IClock clock)
        {
            PublisherGameID = publisherGame.PublisherGameID;
            GameName = publisherGame.GameName;
            Timestamp = publisherGame.Timestamp.ToDateTimeUtc();

            CounterPick = publisherGame.CounterPick;

            FantasyPoints = publisherGame.FantasyPoints;

            Linked = publisherGame.MasterGame.HasValue;
            if (Linked)
            {
                GameName = publisherGame.MasterGame.Value.GameName;
                EstimatedReleaseDate = publisherGame.MasterGame.Value.EstimatedReleaseDate;
                if (publisherGame.MasterGame.Value.ReleaseDate.HasValue)
                {
                    ReleaseDate = publisherGame.MasterGame.Value.ReleaseDate.Value.ToDateTimeUnspecified();
                }

                CriticScore = publisherGame.MasterGame.Value.CriticScore;
                Released = publisherGame.MasterGame.Value.IsReleased(clock);
                MasterGameID = publisherGame.MasterGame.Value.MasterGameID;

                if (publisherGame.ManualCriticScore.HasValue)
                {
                    CriticScore = publisherGame.ManualCriticScore;
                    ManualCriticScore = true;
                }
            }

            WillRelease = publisherGame.WillRelease();
        }

        public Guid PublisherGameID { get; }
        public string GameName { get; }
        public DateTime Timestamp { get; }
        public bool CounterPick { get; }
        public string EstimatedReleaseDate { get; }
        public DateTime? ReleaseDate { get; }
        public decimal? FantasyPoints { get; }
        public decimal? CriticScore { get; }
        public Guid MasterGameID { get; }

        public bool Linked { get; }
        public bool Released { get; }
        public bool WillRelease { get; }
        public bool ManualCriticScore { get; }
    }
}
