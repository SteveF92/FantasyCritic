using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Requests
{
    public class MinimalCreateMasterGameRequest
    {
        [Required]
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        public LocalDate? ReleaseDate { get; set; }
        [Required]
        public int MinimumReleaseYear { get; set; }

        public MasterGame ToDomain(EligibilityLevel eligibilityLevel, Instant timestamp)
        {
            var eligibilitySettings = new EligibilitySettings(eligibilityLevel, false, false, false, false, false);

            MasterGame masterGame = new MasterGame(Guid.NewGuid(), GameName, EstimatedReleaseDate, ReleaseDate, null, null, MinimumReleaseYear,
                eligibilitySettings, "", null, false, timestamp);
            return masterGame;
        }
    }
}
