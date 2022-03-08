using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.MySQL.Entities
{
    public class FormerPublisherGameEntity
    {
        public FormerPublisherGameEntity()
        {

        }

        public FormerPublisherGameEntity(FormerPublisherGame publisherGame)
        {
            PublisherGameID = publisherGame.PublisherGame.PublisherGameID;
            PublisherID = publisherGame.PublisherGame.PublisherID;
            GameName = publisherGame.PublisherGame.GameName;
            Timestamp = publisherGame.PublisherGame.Timestamp;
            CounterPick = publisherGame.PublisherGame.CounterPick;
            ManualCriticScore = publisherGame.PublisherGame.ManualCriticScore;
            ManualWillNotRelease = publisherGame.PublisherGame.ManualWillNotRelease;
            FantasyPoints = publisherGame.PublisherGame.FantasyPoints;

            DraftPosition = publisherGame.PublisherGame.DraftPosition;
            OverallDraftPosition = publisherGame.PublisherGame.OverallDraftPosition;
            if (publisherGame.PublisherGame.MasterGame.HasValue)
            {
                MasterGameID = publisherGame.PublisherGame.MasterGame.Value.MasterGame.MasterGameID;
            }

            BidAmount = publisherGame.PublisherGame.BidAmount;
            RemovedTimestamp = publisherGame.RemovedTimestamp;
            RemovedNote = publisherGame.RemovedNote;
        }

        public Guid PublisherGameID { get; set; }
        public Guid PublisherID { get; set; }
        public string GameName { get; set; }
        public Instant Timestamp { get; set; }
        public bool CounterPick { get; set; }
        public decimal? ManualCriticScore { get; set; }
        public bool ManualWillNotRelease { get; set; }
        public decimal? FantasyPoints { get; set; }
        public Guid? MasterGameID { get; set; }
        public int? DraftPosition { get; set; }
        public int? OverallDraftPosition { get; set; }
        public uint? BidAmount { get; set; }
        public Guid? AcquiredInTradeID { get; set; }
        public Instant RemovedTimestamp { get; set; }
        public string RemovedNote { get; set; }

        public FormerPublisherGame ToDomain(Maybe<MasterGameYear> masterGame)
        {
            PublisherGame domain = new PublisherGame(PublisherID, PublisherGameID, GameName, Timestamp, CounterPick, 
                ManualCriticScore, ManualWillNotRelease, FantasyPoints, masterGame, 0, DraftPosition, OverallDraftPosition, BidAmount, AcquiredInTradeID);
            return new FormerPublisherGame(domain, RemovedTimestamp, RemovedNote);
        }
    }
}
