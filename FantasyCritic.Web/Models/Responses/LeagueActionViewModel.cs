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
            PublisherName = leagueAction.Publisher.PublisherName;
            Timestamp = leagueAction.Timestamp.ToDateTimeUtc();
            ActionType = leagueAction.ActionType;
            Description = leagueAction.Description;
            ManagerAction = leagueAction.ManagerAction;
        }

        public string PublisherName { get; }
        public DateTime Timestamp { get; }
        public string ActionType { get; }
        public string Description { get; }
        public bool ManagerAction { get; }
    }
}
