using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Web.Extensions;
using NodaTime;

namespace FantasyCritic.Web.Models.Requests
{
    public class MasterGameChangeRequestRequest
    {
        [Required]
        public Guid MasterGameID { get; set; }
        [Required]
        public string RequestNote { get; set; }
        public string OpenCriticLink { get; set; }

        public MasterGameChangeRequest ToDomain(FantasyCriticUser user, Instant requestTimestamp, MasterGame masterGame)
        {
            int? openCriticID = URLParsingExtensions.GetOpenCriticIDFromURL(OpenCriticLink);
            return new MasterGameChangeRequest(Guid.NewGuid(), user, requestTimestamp, RequestNote, masterGame, openCriticID, false, null, null, false);
        }
    }
}
