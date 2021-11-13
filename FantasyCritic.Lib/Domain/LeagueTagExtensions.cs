using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain
{
    public static class LeagueTagExtensions
    {
        public static IReadOnlyList<ClaimError> GameIsRoyaleEligible(IEnumerable<MasterGameTag> allMasterGameTags, MasterGame masterGame)
        {
            return GetRoyaleEligibilitySettings(allMasterGameTags)
                .GameIsEligible(masterGame, new List<MasterGameTag>());
        }

        public static IReadOnlyList<ClaimError> GameIsEligible(this IEnumerable<LeagueTagStatus> leagueTags, MasterGame masterGame, 
           IEnumerable<MasterGameTag> overriddenTags)
        {
            var tagsToUse = masterGame.Tags;
            if (overriddenTags.Any())
            {
                tagsToUse = overriddenTags.ToList();
            }

            var bannedTags = leagueTags.Where(x => x.Status == TagStatus.Banned).Select(x => x.Tag);
            var requiredTags = leagueTags.Where(x => x.Status == TagStatus.Required).Select(x => x.Tag);

            var bannedTagsIntersection = tagsToUse.Intersect(bannedTags);
            var missingRequiredTags = requiredTags.Except(tagsToUse);

            var bannedClaimErrors = bannedTagsIntersection.Select(x => new ClaimError($"That game is not eligible because the {x.ReadableName} tag has been banned.", true));
            var requiredClaimErrors = missingRequiredTags.Select(x => new ClaimError($"That game is not eligible because it does not have the required {x.ReadableName} tag.", true));

            var allErrors = bannedClaimErrors.Concat(requiredClaimErrors).ToList();
            return allErrors;
        }

        public static IReadOnlyList<LeagueTagStatus> GetRoyaleEligibilitySettings(IEnumerable<MasterGameTag> allMasterGameTags)
        {
            var bannedTagNames = new List<string>()
            {
                "CurrentlyInEarlyAccess",
                "DirectorsCut",
                "Port",
                "ReleasedInternationally",
                "Remaster",
                "YearlyInstallment"
            };

            var bannedTags = allMasterGameTags.Where(x => bannedTagNames.Contains(x.Name));
            return bannedTags.Select(x => new LeagueTagStatus(x, TagStatus.Banned)).ToList();
        }
    }
}
