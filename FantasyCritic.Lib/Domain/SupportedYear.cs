using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class SupportedYear
    {
        public SupportedYear(int year, bool openForCreation, bool openForPlay, LocalDate startDate, bool finished)
        {
            Year = year;
            OpenForCreation = openForCreation;
            OpenForPlay = openForPlay;
            StartDate = startDate;
            Finished = finished;
        }

        public int Year { get; }
        public bool OpenForCreation { get; }
        public bool OpenForPlay { get; }
        public LocalDate StartDate { get; }
        public bool Finished { get; }

        public override string ToString()
        {
            return Year.ToString();
        }
    }
}
