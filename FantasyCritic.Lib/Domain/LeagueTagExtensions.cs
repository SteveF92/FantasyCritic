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
            return GetRoyaleEligibilitySettings(allMasterGameTags).GameHasValidTags(masterGame.Tags);
        }

        public static IReadOnlyList<ClaimError> GameHasValidTags(this IEnumerable<LeagueTagStatus> slotTags, IEnumerable<MasterGameTag> masterGameTags)
        {
            var bannedTags = slotTags.Where(x => x.Status.Equals(TagStatus.Banned)).Select(x => x.Tag);
            var requiredTags = slotTags.Where(x => x.Status.Equals(TagStatus.Required)).Select(x => x.Tag);

            var bannedTagsIntersection = masterGameTags.Intersect(bannedTags);
            var requiredTagsIntersection = masterGameTags.Intersect(requiredTags);

            bool hasNoRequiredTags = requiredTags.Any() && !requiredTagsIntersection.Any();

            List<ClaimError> claimErrors = bannedTagsIntersection.Select(x => new ClaimError($"That game is not eligible because the {x.ReadableName} tag has been banned.", true)).ToList();
            if (hasNoRequiredTags)
            {
                claimErrors.Add(new ClaimError($"That game is not eligible because it does not have any of the following required tags: ({string.Join(",", requiredTags.Select(x => x.ReadableName))})", true));
            }

            return claimErrors;
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
