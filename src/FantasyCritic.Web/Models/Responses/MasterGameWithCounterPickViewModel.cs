using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class MasterGameYearWithCounterPickViewModel
    {
        public MasterGameYearWithCounterPickViewModel(MasterGameYear masterGameYear, bool counterPick, LocalDate currentDate)
        {
            MasterGameYear = new MasterGameYearViewModel(masterGameYear, currentDate);
            CounterPick = counterPick;
        }

        public MasterGameYearViewModel MasterGameYear { get; }
        public bool CounterPick { get; }
    }
}
