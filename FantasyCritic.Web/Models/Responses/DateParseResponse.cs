using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class DateParseResponse
    {
        public DateParseResponse(LocalDate? minimumReleaseDate, LocalDate? maximumReleaseDate)
        {
            MinimumReleaseDate = minimumReleaseDate;
            MaximumReleaseDate = maximumReleaseDate;
        }

        public LocalDate? MinimumReleaseDate { get; }
        public LocalDate? MaximumReleaseDate { get; }
    }
}
