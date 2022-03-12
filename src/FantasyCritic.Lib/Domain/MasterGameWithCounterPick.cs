using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public record MasterGameYearWithCounterPick(MasterGameYear MasterGameYear, bool CounterPick);
}
