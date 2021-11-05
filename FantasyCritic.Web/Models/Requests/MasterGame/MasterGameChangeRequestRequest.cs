using System;
using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Extensions;
using NodaTime;

namespace FantasyCritic.Web.Models.Requests.MasterGame
{
    public class MasterGameChangeRequestRequest
    {
        [Required]
        public Guid MasterGameID { get; set; }
        [Required]
        public string RequestNote { get; set; }
        public string OpenCriticLink { get; set; }

        public MasterGameChangeRequest ToDomain(FantasyCriticUser user, Instant requestTimestamp, Lib.Domain.MasterGame masterGame)
        {
            int? openCriticID = URLParsingExtensions.GetOpenCriticIDFromURL(OpenCriticLink);
            return new MasterGameChangeRequest(Guid.NewGuid(), user, requestTimestamp, RequestNote, masterGame, openCriticID, false, null, null, false);
        }
    }
}
