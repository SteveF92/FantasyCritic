using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
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
        public string EstimatedReleaseDate { get; set; }
        public int EligibilityLevel { get; set; }
        public bool YearlyInstallment { get; set; }
        public bool EarlyAccess { get; set; }
        public bool FreeToPlay { get; set; }
        public bool ReleasedInternationally { get; set; }
        public bool ExpansionPack { get; set; }

        public string RequestNote { get; set; }

        public MasterGameRequest ToDomain(FantasyCriticUser user, Instant requestTimestamp, EligibilityLevel eligibilityLevel)
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

            return new MasterGameRequest(Guid.NewGuid(), user, requestTimestamp, RequestNote, GameName, steamID, openCriticID, EstimatedReleaseDate, eligibilityLevel,
                YearlyInstallment, EarlyAccess, FreeToPlay, ReleasedInternationally, ExpansionPack, false, null, null, Maybe<Lib.Domain.MasterGame>.None, false);
        }
    }
}
