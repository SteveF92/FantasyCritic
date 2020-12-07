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
        public LeagueYearTagEntity()
        {

        }

        public LeagueYearTagEntity(League league, int year, LeagueTagStatus domain)
        {
            LeagueID = league.LeagueID;
            Year = year;
            Tag = domain.Tag.Name;
            Status = domain.Status.Value;
        }

        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public string Tag { get; set; }
        public string Status { get; set; }

        public LeagueTagStatus ToDomain(MasterGameTag tag)
        {
            return new LeagueTagStatus(tag, TagStatus.FromValue(Status));
        }
    }
}
