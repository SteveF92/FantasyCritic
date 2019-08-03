using System;
using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Requests.MasterGame
{
    public class MinimalCreateMasterGameRequest
    {
        [Required]
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        public LocalDate SortableEstimatedReleaseDate { get; set; }
        public LocalDate? ReleaseDate { get; set; }
        [Required]
        public int MinimumReleaseDate { get; set; }

        public Lib.Domain.MasterGame ToDomain(EligibilityLevel eligibilityLevel, Instant timestamp)
        {
            var eligibilitySettings = new EligibilitySettings(eligibilityLevel, false, false, false, false, false);

            Lib.Domain.MasterGame masterGame = new Lib.Domain.MasterGame(Guid.NewGuid(), GameName, EstimatedReleaseDate, SortableEstimatedReleaseDate,
                ReleaseDate, null, null, MinimumReleaseDate,
                eligibilitySettings, "", null, false, false, timestamp);
            return masterGame;
        }
    }
}
