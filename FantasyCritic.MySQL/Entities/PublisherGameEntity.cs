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
            Waiver = publisherGame.Waiver;
            AntiPick = publisherGame.AntiPick;
            FantasyScore = publisherGame.FantasyScore;

            if (publisherGame.MasterGame.HasValue)
            {
                MasterGameID = publisherGame.MasterGame.Value.MasterGameID;
            }
        }

        public Guid PublisherGameID { get; set; }
        public Guid PublisherID { get; set; }
        public string GameName { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Waiver { get; set; }
        public bool AntiPick { get; set; }
        public decimal? FantasyScore { get; set; }
        public Guid? MasterGameID { get; set; }

        public PublisherGame ToDomain(Maybe<MasterGame> masterGame, int leagueYear)
        {
            PublisherGame domain = new PublisherGame(PublisherGameID, GameName, Instant.FromDateTimeUtc(Timestamp), Waiver, AntiPick, FantasyScore, masterGame, leagueYear);
            return domain;
        }
    }
}
