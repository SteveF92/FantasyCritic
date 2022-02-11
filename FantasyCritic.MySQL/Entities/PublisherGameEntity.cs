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
    internal class PublisherGameEntity
    {
        public PublisherGameEntity()
        {

        }

        public PublisherGameEntity(PublisherGame publisherGame)
        {
            PublisherGameID = publisherGame.PublisherGameID;
            PublisherID = publisherGame.PublisherID;
            GameName = publisherGame.GameName;
            Timestamp = publisherGame.Timestamp;
            CounterPick = publisherGame.CounterPick;
            ManualCriticScore = publisherGame.ManualCriticScore;
            ManualWillNotRelease = publisherGame.ManualWillNotRelease;
            FantasyPoints = publisherGame.FantasyPoints;

            SlotNumber = publisherGame.SlotNumber;
            DraftPosition = publisherGame.DraftPosition;
            OverallDraftPosition = publisherGame.OverallDraftPosition;
            if (publisherGame.MasterGame.HasValue)
            {
                MasterGameID = publisherGame.MasterGame.Value.MasterGame.MasterGameID;
            }

            BidAmount = publisherGame.BidAmount;
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
        public int SlotNumber { get; set; }
        public int? DraftPosition { get; set; }
        public int? OverallDraftPosition { get; set; }
        public uint? BidAmount { get; set; }

        public PublisherGame ToDomain(Maybe<MasterGameYear> masterGame)
        {
            PublisherGame domain = new PublisherGame(PublisherID, PublisherGameID, GameName, Timestamp, CounterPick, 
                ManualCriticScore, ManualWillNotRelease, FantasyPoints, masterGame, SlotNumber, DraftPosition, OverallDraftPosition, BidAmount);
            return domain;
        }
    }
}
