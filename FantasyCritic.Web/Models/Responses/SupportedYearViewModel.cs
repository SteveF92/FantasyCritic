using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class SupportedYearViewModel
    {
        public SupportedYearViewModel(SupportedYear domain)
        {
            Year = domain.Year;
            OpenForCreation = domain.OpenForCreation;
            OpenForPlay = domain.OpenForPlay;
            StartDate = domain.StartDate.ToDateTimeUnspecified();
            Finished = domain.Finished;
        }

        public int Year { get; }
        public bool OpenForCreation { get; }
        public bool OpenForPlay { get; }
        public DateTime StartDate { get; }
        public bool Finished { get; }

        public override string ToString()
        {
            return Year.ToString();
        }
    }
}
