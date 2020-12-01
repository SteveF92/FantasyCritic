using System;
using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Requests.Admin
{
    public class CreateMasterGameRequest
    {
        [Required]
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        [Required]
        public LocalDate MinimumReleaseDate { get; set; }
        public LocalDate? MaximumReleaseDate { get; set; }
        public LocalDate? InternationalReleaseDate { get; set; }
        public LocalDate? EarlyAccessReleaseDate { get; set; }
        public LocalDate? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        [Required]
        public int EligibilityLevel { get; set; }
        [Required]
        public bool YearlyInstallment { get; set; }
        [Required]
        public bool EarlyAccess { get; set; }
        [Required]
        public bool FreeToPlay { get; set; }
        [Required]
        public bool ReleasedInternationally { get; set; }
        [Required]
        public bool ExpansionPack { get; set; }
        [Required]
        public bool UnannouncedGame { get; set; }
        [Required]
        public string BoxartFileName { get; set; }
        [Required]
        public string Notes { get; set; }

        public Lib.Domain.MasterGame ToDomain(EligibilityLevel eligibilityLevel, Instant timestamp)
        {
            var eligibilitySettings = new EligibilitySettings(eligibilityLevel, YearlyInstallment, EarlyAccess, FreeToPlay, ReleasedInternationally, ExpansionPack, UnannouncedGame);
            Lib.Domain.MasterGame masterGame = new Lib.Domain.MasterGame(Guid.NewGuid(), GameName, EstimatedReleaseDate, MinimumReleaseDate, MaximumReleaseDate,
                InternationalReleaseDate, EarlyAccessReleaseDate, ReleaseDate, OpenCriticID, null, eligibilitySettings, Notes, BoxartFileName, null, false, false, false, timestamp);
            return masterGame;
        }
    }
}
