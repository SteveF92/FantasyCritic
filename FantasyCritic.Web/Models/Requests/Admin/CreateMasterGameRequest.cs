using System;
using System.Collections.Generic;
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
        public LocalDate? EarlyAccessReleaseDate { get; set; }
        public LocalDate? InternationalReleaseDate { get; set; }
        public LocalDate? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        public string BoxartFileName { get; set; }
        [Required]
        public string Notes { get; set; }
        [Required]
        public List<string> Tags { get; set; }

        public List<string> GetRequestedTags() => Tags ?? new List<string>();

        public Lib.Domain.MasterGame ToDomain(Instant timestamp, IEnumerable<MasterGameTag> tags)
        {
            Lib.Domain.MasterGame masterGame = new Lib.Domain.MasterGame(Guid.NewGuid(), GameName, EstimatedReleaseDate, MinimumReleaseDate, MaximumReleaseDate,
                EarlyAccessReleaseDate, InternationalReleaseDate, ReleaseDate, OpenCriticID, null, Notes, BoxartFileName,
                null, false, false, false, timestamp, new List<MasterSubGame>(), tags);
            return masterGame;
        }
    }
}
