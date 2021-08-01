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
        public EligibilityOverrideViewModel(EligibilityOverride domain, LocalDate currentDate)
        {
            MasterGame = new MasterGameViewModel(domain.MasterGame, currentDate);
            Eligible = domain.Eligible;
        }

        public MasterGameViewModel MasterGame { get; }
        public bool Eligible { get; }
    }
}
