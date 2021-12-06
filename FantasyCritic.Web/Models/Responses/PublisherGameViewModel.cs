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
        public PublisherGameViewModel(PublisherSlot slot, PublisherGame publisherGame, LeagueYear leagueYear, LocalDate currentDate, SystemWideValues systemWideValues)
        {
            PublisherGameID = publisherGame.PublisherGameID;
            GameName = publisherGame.GameName;
            
            Timestamp = publisherGame.Timestamp.ToDateTimeUtc();
            CounterPick = publisherGame.CounterPick;
            FantasyPoints = publisherGame.FantasyPoints;
            SimpleProjectedFantasyPoints = slot.GetProjectedOrRealFantasyPoints(leagueYear, systemWideValues, true, currentDate);
            AdvancedProjectedFantasyPoints = slot.GetProjectedOrRealFantasyPoints(leagueYear, systemWideValues, false, currentDate);

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
                Released = publisherGame.MasterGame.Value.MasterGame.IsReleased(currentDate);
                if (publisherGame.MasterGame.HasValue)
                {
                    MasterGame = new MasterGameYearViewModel(publisherGame.MasterGame.Value, currentDate);
                }  
            }

            if (publisherGame.ManualCriticScore.HasValue)
            {
                CriticScore = publisherGame.ManualCriticScore;
                ManualCriticScore = true;
            }

            WillRelease = publisherGame.WillRelease();
            ManualWillNotRelease = publisherGame.ManualWillNotRelease;
            OverallDraftPosition = publisherGame.OverallDraftPosition;
            SlotNumber = publisherGame.SlotNumber;
        }

        public PublisherGameViewModel(PublisherGame publisherGame, LocalDate currentDate)
        {
            PublisherGameID = publisherGame.PublisherGameID;
            GameName = publisherGame.GameName;

            Timestamp = publisherGame.Timestamp.ToDateTimeUtc();
            CounterPick = publisherGame.CounterPick;
            FantasyPoints = publisherGame.FantasyPoints;

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
                Released = publisherGame.MasterGame.Value.MasterGame.IsReleased(currentDate);
                if (publisherGame.MasterGame.HasValue)
                {
                    MasterGame = new MasterGameYearViewModel(publisherGame.MasterGame.Value, currentDate);
                }
            }

            if (publisherGame.ManualCriticScore.HasValue)
            {
                CriticScore = publisherGame.ManualCriticScore;
                ManualCriticScore = true;
            }

            WillRelease = publisherGame.WillRelease();
            ManualWillNotRelease = publisherGame.ManualWillNotRelease;
            OverallDraftPosition = publisherGame.OverallDraftPosition;
            SlotNumber = publisherGame.SlotNumber;
        }

        public Guid PublisherGameID { get; }
        public string GameName { get; }
        public DateTime Timestamp { get; }
        public bool CounterPick { get; }
        public string EstimatedReleaseDate { get; }
        public DateTime? ReleaseDate { get; }
        public decimal? FantasyPoints { get; }
        public decimal? CriticScore { get; }
        public decimal? SimpleProjectedFantasyPoints { get; }
        public decimal? AdvancedProjectedFantasyPoints { get; }
        public MasterGameYearViewModel MasterGame { get; }
        public int? OverallDraftPosition { get; }
        public int SlotNumber { get; }

        public bool Linked { get; }
        public bool Released { get; }
        public bool WillRelease { get; }
        public bool ManualCriticScore { get; }
        public bool ManualWillNotRelease { get; }
    }
}
