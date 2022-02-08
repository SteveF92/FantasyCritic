using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Responses
{
    public class ActionedGameSetViewModel
    {
        public ActionedGameSetViewModel(IEnumerable<MasterGameViewModel> pickupActions, IEnumerable<MasterGameViewModel> dropActions, IEnumerable<LeagueActionViewModel> leagueActions)
        {
            PickupActions = pickupActions;
            DropActions = dropActions;
            LeagueActions = leagueActions;
        }

        public IEnumerable<MasterGameViewModel> PickupActions { get; }
        public IEnumerable<MasterGameViewModel> DropActions { get; }
        public IEnumerable<LeagueActionViewModel> LeagueActions { get; }
    }
}
