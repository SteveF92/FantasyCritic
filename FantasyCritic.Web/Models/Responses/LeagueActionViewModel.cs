using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueActionViewModel
    {
        public LeagueActionViewModel(LeagueAction leagueAction, IClock clock)
        {
            Publisher = new PublisherViewModel(leagueAction.Publisher, clock);
            Timestamp = leagueAction.Timestamp.ToDateTimeUtc();
            ActionType = leagueAction.ActionType;
            MasterGame = new MasterGameViewModel(leagueAction.MasterGame);
            ManagerAction = leagueAction.ManagerAction;
        }

        public PublisherViewModel Publisher { get; }
        public DateTime Timestamp { get; }
        public string ActionType { get; }
        public MasterGameViewModel MasterGame { get; }
        public bool ManagerAction { get; }
    }
}
