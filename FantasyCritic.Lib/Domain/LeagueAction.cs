using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueAction
    {
        public LeagueAction(Publisher publisher, Instant timestamp, string actionType, MasterGame masterGame, bool managerAction)
        {
            Publisher = publisher;
            Timestamp = timestamp;
            ActionType = actionType;
            MasterGame = masterGame;
            ManagerAction = managerAction;
        }

        public Publisher Publisher { get; }
        public Instant Timestamp { get; }
        public string ActionType { get; }
        public MasterGame MasterGame { get; }
        public bool ManagerAction { get; }
    }
}
