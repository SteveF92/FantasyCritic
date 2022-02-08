using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public record FinalizedActionProcessingResults(Guid ProcessSetID, Instant ProcessTime, string processName, ActionProcessingResults Results);
}
