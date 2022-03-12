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
        public LocalDate? AnnouncementDate { get; set; }
        public LocalDate? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        public string GGToken { get; set; }
        [Required]
        public string Notes { get; set; }
        public List<string> Tags { get; set; }
        public bool DoNotRefreshDate { get; set; }
        public bool DoNotRefreshAnything { get; set; }
        public bool EligibilityChanged { get; set; }
        public bool DelayContention { get; set; }

        public List<string> GetRequestedTags() => Tags ?? new List<string>();

        public Lib.Domain.MasterGame ToDomain(Lib.Domain.MasterGame existingMasterGame, Instant timestamp, IEnumerable<MasterGameTag> tags)
        {
            var masterGame = new Lib.Domain.MasterGame(MasterGameID, GameName, EstimatedReleaseDate, MinimumReleaseDate, MaximumReleaseDate,
                EarlyAccessReleaseDate, InternationalReleaseDate, AnnouncementDate, ReleaseDate, OpenCriticID, GGToken, existingMasterGame.RawCriticScore, Notes, existingMasterGame.BoxartFileName,
                existingMasterGame.GGCoverArtFileName, existingMasterGame.FirstCriticScoreTimestamp, DoNotRefreshDate, DoNotRefreshAnything, EligibilityChanged, DelayContention,
                timestamp, existingMasterGame.SubGames, tags);
            return masterGame;
        }
    }
}
