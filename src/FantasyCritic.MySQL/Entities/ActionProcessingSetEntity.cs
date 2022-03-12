using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain.LeagueActions;
using NodaTime;

namespace FantasyCritic.MySQL.Entities
{
    public class ActionProcessingSetEntity
    {
        public ActionProcessingSetEntity()
        {

        }

        public ActionProcessingSetEntity(FinalizedActionProcessingResults results)
        {
            ProcessSetID = results.ProcessSetID;
            ProcessTime = results.ProcessTime;
            ProcessName = results.ProcessName;
        }

        public Guid ProcessSetID { get; set; }
        public Instant ProcessTime { get; set; }
        public string ProcessName { get; set; }

        public ActionProcessingSetMetadata ToDomain()
        {
            return new ActionProcessingSetMetadata(ProcessSetID, ProcessTime, ProcessName);
        }
    }
}
