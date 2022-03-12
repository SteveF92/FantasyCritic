using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.MySQL.Entities
{
    public class SpecialGameSlotEntity
    {
        public SpecialGameSlotEntity()
        {

        }

        public SpecialGameSlotEntity(Guid specialSlotID, League league, int year, int specialSlotPosition, MasterGameTag tag)
        {
            SpecialSlotID = specialSlotID;
            LeagueID = league.LeagueID;
            Year = year;
            SpecialSlotPosition = specialSlotPosition;
            Tag = tag.Name;
        }

        public Guid SpecialSlotID { get; set; }
        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public int SpecialSlotPosition { get; set; }
        public string Tag { get; set; }
    }
}
