using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Web.Extensions;
using NodaTime;

namespace FantasyCritic.Web.Models.Requests.MasterGame
{
    public class MasterGameRequestRequest
    {
        [Required]
        public string GameName { get; set; }

        public string SteamLink { get; set; }
        public string OpenCriticLink { get; set; }
        public string GGLink { get; set; }
        public LocalDate? ReleaseDate { get; set; }
        public string EstimatedReleaseDate { get; set; }

        public string RequestNote { get; set; }

        public MasterGameRequest ToDomain(FantasyCriticUser user, Instant requestTimestamp)
        {
            int? steamID = null;
            var steamGameIDString = SubstringSearching.GetBetween(SteamLink, "/app/", "/");
            if (steamGameIDString.IsSuccess)
            {
                bool parseResult = int.TryParse(steamGameIDString.Value, out int steamIDResult);
                if (parseResult)
                {
                    steamID = steamIDResult;
                }
            }

            int? openCriticID = URLParsingExtensions.GetOpenCriticIDFromURL(OpenCriticLink);
            var ggToken = URLParsingExtensions.GetGGTokenFromURL(GGLink);

            return new MasterGameRequest(Guid.NewGuid(), user, requestTimestamp, RequestNote, GameName, steamID, openCriticID, ggToken,
                ReleaseDate, EstimatedReleaseDate, false, null, null, Maybe<Lib.Domain.MasterGame>.None, false);
        }
    }
}
