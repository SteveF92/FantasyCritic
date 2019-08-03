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
            Timestamp = publisherGame.Timestamp.ToDateTimeUtc();
            CounterPick = publisherGame.CounterPick;
            ManualCriticScore = publisherGame.ManualCriticScore;
            FantasyPoints = publisherGame.FantasyPoints;

            DraftPosition = publisherGame.DraftPosition;
            OverallDraftPosition = publisherGame.OverallDraftPosition;
            if (publisherGame.MasterGame.HasValue)
            {
                MasterGameID = publisherGame.MasterGame.Value.MasterGame.MasterGameID;
            }
        }

        public Guid PublisherGameID { get; set; }
        public Guid PublisherID { get; set; }
        public string GameName { get; set; }
        public DateTime Timestamp { get; set; }
        public bool CounterPick { get; set; }
        public decimal? ManualCriticScore { get; set; }
        public decimal? FantasyPoints { get; set; }
        public Guid? MasterGameID { get; set; }
        public int? DraftPosition { get; set; }
        public int? OverallDraftPosition { get; set; }

        public PublisherGame ToDomain(Maybe<MasterGameYear> masterGame)
        {
            Instant instant = LocalDateTime.FromDateTime(Timestamp).InZoneStrictly(DateTimeZone.Utc).ToInstant();
            PublisherGame domain = new PublisherGame(PublisherID, PublisherGameID, GameName, instant, CounterPick, ManualCriticScore, FantasyPoints, masterGame, DraftPosition, OverallDraftPosition);
            return domain;
        }
    }
}
