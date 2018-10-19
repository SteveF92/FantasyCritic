using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.MySQL.Entities
{
    internal class LeagueActionEntity
    {
        public LeagueActionEntity(LeagueAction action)
        {
            PublisherID = action.Publisher.PublisherID;
            Timestamp = action.Timestamp.ToDateTimeUtc();
            ActionType = action.ActionType;
            MasterGameID = action.MasterGame.MasterGameID;
            ManagerAction = action.ManagerAction;
        }

        public Guid PublisherID { get; set; }
        public DateTime Timestamp { get; set; }
        public string ActionType { get; set; }
        public Guid MasterGameID { get; set; }
        public bool ManagerAction { get; set; }

        public LeagueAction ToDomain(Publisher publisher, MasterGame masterGame)
        {
            Instant instant = LocalDateTime.FromDateTime(Timestamp).InZoneStrictly(DateTimeZone.Utc).ToInstant();
            return new LeagueAction(publisher, instant, ActionType, masterGame, ManagerAction);
        }
    }
}
