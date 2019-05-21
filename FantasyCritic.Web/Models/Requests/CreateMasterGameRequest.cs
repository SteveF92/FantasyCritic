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
    public class CreateMasterGameRequest
    {
        [Required]
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
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
        public string BoxartFileName { get; set; }

        public MasterGame ToDomain(EligibilityLevel eligibilityLevel, Instant timestamp, SupportedYear currentYear)
        {
            MasterGame masterGame = new MasterGame(Guid.NewGuid(), GameName, EstimatedReleaseDate, ReleaseDate, OpenCriticID, null, currentYear.Year,
                eligibilityLevel, YearlyInstallment, EarlyAccess, FreeToPlay, ReleasedInternationally, ExpansionPack, BoxartFileName, null, false, timestamp);
            return masterGame;
        }
    }
}
