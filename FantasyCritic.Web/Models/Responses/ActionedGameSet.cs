using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Responses
{
    public class ActionedGameSet
    {
        public ActionedGameSet(IEnumerable<MasterGameViewModel> pickupActions, IEnumerable<MasterGameViewModel> dropActions)
        {
            PickupActions = pickupActions;
            DropActions = dropActions;
        }

        public IEnumerable<MasterGameViewModel> PickupActions { get; }
        public IEnumerable<MasterGameViewModel> DropActions { get; }
    }
}
