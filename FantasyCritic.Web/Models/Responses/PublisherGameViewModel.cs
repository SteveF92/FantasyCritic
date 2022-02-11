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
        public PublisherGameViewModel(PublisherGame publisherGame, LocalDate currentDate, bool counterPicked, bool counterPicksBlockDrops)
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
            BidAmount = publisherGame.BidAmount;
            SlotNumber = publisherGame.SlotNumber;
            CounterPicked = counterPicked;
            DropBlocked = counterPicked && counterPicksBlockDrops;
        }

        public Guid PublisherGameID { get; }
        public string GameName { get; }
        public DateTime Timestamp { get; }
        public bool CounterPick { get; }
        public string EstimatedReleaseDate { get; }
        public DateTime? ReleaseDate { get; }
        public decimal? FantasyPoints { get; }
        public decimal? CriticScore { get; }
        
        public MasterGameYearViewModel MasterGame { get; }
        public int? OverallDraftPosition { get; }
        public uint? BidAmount { get; }
        public int SlotNumber { get; }

        public bool Linked { get; }
        public bool Released { get; }
        public bool WillRelease { get; }
        public bool ManualCriticScore { get; }
        public bool ManualWillNotRelease { get; }
        public bool CounterPicked { get; }
        public bool DropBlocked { get; }
    }
}
