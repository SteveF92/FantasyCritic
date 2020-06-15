using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class EligibilityOverrideViewModel
    {
        public EligibilityOverrideViewModel(EligibilityOverride domain, IClock clock)
        {
            MasterGame = new MasterGameViewModel(domain.MasterGame, clock);
            Eligible = domain.Eligible;
        }

        public MasterGameViewModel MasterGame { get; }
        public bool Eligible { get; }
    }
}
