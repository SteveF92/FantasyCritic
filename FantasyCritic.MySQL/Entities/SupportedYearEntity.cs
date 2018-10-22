using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.MySQL.Entities
{
    public class SupportedYearEntity
    {
        public int Year { get; set; }
        public bool OpenForCreation { get; set; }
        public bool OpenForPlay { get; set; }
        public DateTime StartDate { get; set; }

        public SupportedYear ToDomain()
        {
            return new SupportedYear(Year, OpenForCreation, OpenForPlay, LocalDate.FromDateTime(StartDate));
        }
    }
}
