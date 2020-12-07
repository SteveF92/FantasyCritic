using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueTagStatus
    {
        public LeagueTagStatus(MasterGameTag tag, TagStatus status)
        {
            Tag = tag;
            Status = status;
        }

        public MasterGameTag Tag { get; }
        public TagStatus Status { get; }
    }
}
