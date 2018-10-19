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
        public LeagueAction(Publisher publisher, Instant timestamp, string actionType, MasterGame masterGame, string gameName, bool managerAction)
        {
            Publisher = publisher;
            Timestamp = timestamp;
            ActionType = actionType;
            MasterGame = masterGame;
            GameName = gameName;
            ManagerAction = managerAction;
        }

        public LeagueAction(Publisher publisher, Instant timestamp, string actionType, MasterGame masterGame, bool managerAction)
        {
            Publisher = publisher;
            Timestamp = timestamp;
            ActionType = actionType;
            MasterGame = masterGame;
            GameName = masterGame.GameName;
            ManagerAction = managerAction;
        }

        public LeagueAction(Publisher publisher, Instant timestamp, string actionType, PublisherGame publisherGame, bool managerAction)
        {
            Publisher = publisher;
            Timestamp = timestamp;
            ActionType = actionType;
            if (publisherGame.MasterGame.HasValue)
            {
                MasterGame = publisherGame.MasterGame.Value;
                GameName = publisherGame.MasterGame.Value.GameName;
            }
            else
            {
                MasterGame = null;
                GameName = publisherGame.GameName;
            }
            ManagerAction = managerAction;
        }

        public Publisher Publisher { get; }
        public Instant Timestamp { get; }
        public string ActionType { get; }
        public MasterGame MasterGame { get; }
        public string GameName { get; }
        public bool ManagerAction { get; }
    }
}
