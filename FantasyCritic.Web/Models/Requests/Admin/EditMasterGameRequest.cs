using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Requests.Admin
{
    public class EditMasterGameRequest
    {
        [Required]
        public Guid MasterGameID { get; set; }

        [Required]
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        [Required]
        public LocalDate MinimumReleaseDate { get; set; }
        public LocalDate? MaximumReleaseDate { get; set; }
        public LocalDate? EarlyAccessReleaseDate { get; set; }
        public LocalDate? InternationalReleaseDate { get; set; }
        public LocalDate? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        [Required]
        public string BoxartFileName { get; set; }
        [Required]
        public string Notes { get; set; }
        public List<string> Tags { get; set; }
        public bool DoNotRefreshDate { get; set; }
        public bool DoNotRefreshAnything { get; set; }
        public bool EligibilityChanged { get; set; }

        public List<string> GetRequestedTags() => Tags ?? new List<string>();

        public Lib.Domain.MasterGame ToDomain(Instant timestamp, IEnumerable<MasterGameTag> tags)
        {
            Lib.Domain.MasterGame masterGame = new Lib.Domain.MasterGame(MasterGameID, GameName, EstimatedReleaseDate, MinimumReleaseDate, MaximumReleaseDate,
                EarlyAccessReleaseDate, InternationalReleaseDate, ReleaseDate, OpenCriticID, null, Notes, BoxartFileName,
                null, DoNotRefreshDate, DoNotRefreshAnything, EligibilityChanged, timestamp, new List<MasterSubGame>(), tags);
            return masterGame;
        }
    }
}
