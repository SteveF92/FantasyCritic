using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class PublisherGameViewModel
    {
        public PublisherGameViewModel(PublisherGame publisherGame, IClock clock, ScoringSystem scoringSystem, SystemWideValues systemWideValues)
        {
            PublisherGameID = publisherGame.PublisherGameID;
            GameName = publisherGame.GameName;
            
            Timestamp = publisherGame.Timestamp.ToDateTimeUtc();
            CounterPick = publisherGame.CounterPick;
            FantasyPoints = publisherGame.FantasyPoints;
            SimpleProjectedFantasyPoints = publisherGame.GetProjectedOrRealFantasyPoints(scoringSystem, systemWideValues, true, clock);
            AdvancedProjectedFantasyPoints = publisherGame.GetProjectedOrRealFantasyPoints(scoringSystem, systemWideValues, false, clock);

            Linked = publisherGame.MasterGame.HasValue;
            if (Linked)
            {
                GameName = publisherGame.MasterGame.Value.MasterGame.GameName;
                EstimatedReleaseDate = publisherGame.MasterGame.Value.MasterGame.EstimatedReleaseDate;
                if (publisherGame.MasterGame.Value.MasterGame.ReleaseDate.HasValue)
                {
                    ReleaseDate = publisherGame.MasterGame.Value.MasterGame.ReleaseDate.Value.ToDateTimeUnspecified();
                }

                CriticScore = publisherGame.MasterGame.Value.MasterGame.CriticScore;
                Released = publisherGame.MasterGame.Value.MasterGame.IsReleased(clock.GetCurrentInstant());
                if (publisherGame.MasterGame.HasValue)
                {
                    MasterGame = new MasterGameYearViewModel(publisherGame.MasterGame.Value, clock);
                }  
            }

            if (publisherGame.ManualCriticScore.HasValue)
            {
                CriticScore = publisherGame.ManualCriticScore;
                ManualCriticScore = true;
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
        public decimal SimpleProjectedFantasyPoints { get; }
        public decimal AdvancedProjectedFantasyPoints { get; }
        public MasterGameYearViewModel MasterGame { get; }

        public bool Linked { get; }
        public bool Released { get; }
        public bool WillRelease { get; }
        public bool ManualCriticScore { get; }
    }
}
