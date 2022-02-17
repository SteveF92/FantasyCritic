using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using NodaTime;

namespace FantasyCritic.PublisherGameFixer
{
    public class TempLeagueActionEntity
    {
        public Guid LeagueID { get; set; }
        public Guid PublisherID { get; set; }
        public Instant Timestamp { get; set; }
        public string ActionType { get; set; }
        public string Description { get; set; }
        public bool ManagerAction { get; set; }
    }
}
