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

        public PublisherGameEntity(Publisher publisher, PublisherGame publisherGame)
        {
            PublisherGameID = publisherGame.PublisherGameID;
            PublisherID = publisher.PublisherID;
            GameName = publisherGame.GameName;
            Timestamp = publisherGame.Timestamp.ToDateTimeUtc();
            CounterPick = publisherGame.CounterPick;
            ManualCriticScore = publisherGame.ManualCriticScore;
            FantasyPoints = publisherGame.FantasyPoints;

            if (publisherGame.MasterGame.HasValue)
            {
                MasterGameID = publisherGame.MasterGame.Value.MasterGameID;
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

        public PublisherGame ToDomain(Maybe<MasterGame> masterGame, int leagueYear)
        {
            Instant instant = LocalDateTime.FromDateTime(Timestamp).InZoneStrictly(DateTimeZone.Utc).ToInstant();
            PublisherGame domain = new PublisherGame(PublisherGameID, GameName, instant, CounterPick, ManualCriticScore, FantasyPoints, masterGame, leagueYear);
            return domain;
        }
    }
}
