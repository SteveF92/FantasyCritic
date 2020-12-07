using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.MySQL.Entities
{
    internal class LeagueYearTagEntity
    {
        public Guid LeagueID { get; set; }
        public int Year{ get; set; }
        public string Tag { get; set; }
        public string Option { get; set; }

        public LeagueTagOption ToDomain(MasterGameTag tag)
        {
            return new LeagueTagOption(tag, TagOption.FromValue(Option));
        }
    }
}
