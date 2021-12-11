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
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public static class LeagueTagExtensions
    {
        public static IReadOnlyList<ClaimError> GameIsRoyaleEligible(IEnumerable<MasterGameTag> allMasterGameTags, MasterGame masterGame, LocalDate dateOfAcquisition)
        {
            return GetRoyaleEligibilitySettings(allMasterGameTags).GameHasValidTags(masterGame, masterGame.Tags, dateOfAcquisition);
        }

        public static IReadOnlyList<ClaimError> GameHasValidTags(this IEnumerable<LeagueTagStatus> slotTags, MasterGame masterGame, IEnumerable<MasterGameTag> masterGameTags, LocalDate dateOfAcquisition)
        {
            var masterGameCustomCodeTags = masterGameTags.Where(x => x.HasCustomCode).ToList();
            var masterGameNonCustomCodeTags = masterGameTags.Except(masterGameCustomCodeTags).ToList();
            var leagueCustomCodeTags = slotTags.Where(x => x.Tag.HasCustomCode).ToList();
            var leagueNonCustomCodeTags = slotTags.Except(leagueCustomCodeTags).ToList();

            //Non custom code tags
            var bannedTags = leagueNonCustomCodeTags.Where(x => x.Status.Equals(TagStatus.Banned)).Select(x => x.Tag).ToList();
            var requiredTags = leagueNonCustomCodeTags.Where(x => x.Status.Equals(TagStatus.Required)).Select(x => x.Tag).ToList();

            var bannedTagsIntersection = masterGameNonCustomCodeTags.Intersect(bannedTags);
            var requiredTagsIntersection = masterGameNonCustomCodeTags.Intersect(requiredTags);

            bool hasNoRequiredTags = requiredTags.Any() && !requiredTagsIntersection.Any();

            List<ClaimError> claimErrors = bannedTagsIntersection.Select(x => new ClaimError($"That game is not eligible because the {x.ReadableName} tag has been banned.", true)).ToList();
            if (hasNoRequiredTags)
            {
                claimErrors.Add(new ClaimError($"That game is not eligible because it does not have any of the following required tags: ({string.Join(",", requiredTags.Select(x => x.ReadableName))})", true));
            }

            if (!leagueCustomCodeTags.Any())
            {
                return claimErrors;
            }

            //Custom code tags
            var gameIsPlannedForEarlyAccess = masterGameCustomCodeTags.Any(x => x.Name == "PlannedForEarlyAccess");
            var gameIsInEarlyAccess = masterGameCustomCodeTags.Any(x => x.Name == "CurrentlyInEarlyAccess");
            var plannedForEarlyAccessBanned = leagueCustomCodeTags.Any(x => x.Tag.Name == "PlannedForEarlyAccess" && x.Status.Equals(TagStatus.Banned));
            var plannedForEarlyAccessRequired = leagueCustomCodeTags.Any(x => x.Tag.Name == "PlannedForEarlyAccess" && x.Status.Equals(TagStatus.Required));
            var currentlyInEarlyAccessBanned = leagueCustomCodeTags.Any(x => x.Tag.Name == "CurrentlyInEarlyAccess" && x.Status.Equals(TagStatus.Banned));
            var currentlyInEarlyAccessRequired = leagueCustomCodeTags.Any(x => x.Tag.Name == "CurrentlyInEarlyAccess" && x.Status.Equals(TagStatus.Required));

            if (plannedForEarlyAccessBanned && gameIsPlannedForEarlyAccess)
            {
                claimErrors.Add(new ClaimError("That game is not eligible because it has the tag: Planned For Early Access", true));
            }
            if (plannedForEarlyAccessRequired && (!gameIsPlannedForEarlyAccess || gameIsInEarlyAccess))
            {
                claimErrors.Add(new ClaimError("That game is not eligible because it is not planned for or in early access", true));
            }
            if (currentlyInEarlyAccessBanned && gameIsInEarlyAccess)
            {
                bool acquiredBeforeEarlyAccess = masterGame.EarlyAccessReleaseDate.HasValue && masterGame.EarlyAccessReleaseDate.Value > dateOfAcquisition;
                if (!acquiredBeforeEarlyAccess)
                {
                    claimErrors.Add(new ClaimError("That game is not eligible because it has the tag: Currently in Early Access", true));
                }
            }
            if (currentlyInEarlyAccessRequired && !gameIsInEarlyAccess)
            {
                claimErrors.Add(new ClaimError("That game is not eligible because it does not have the tag: Currently in Early Access", true));
            }

            var gameWillReleaseInternationallyFirst = masterGameCustomCodeTags.Any(x => x.Name == "WillReleaseInternationallyFirst");
            var gameReleasedInternationallyFirst = masterGameCustomCodeTags.Any(x => x.Name == "ReleasedInternationally");
            var willReleaseInternationallyFirstBanned = leagueCustomCodeTags.Any(x => x.Tag.Name == "WillReleaseInternationallyFirst" && x.Status.Equals(TagStatus.Banned));
            var willReleaseInternationallyFirstRequired = leagueCustomCodeTags.Any(x => x.Tag.Name == "WillReleaseInternationallyFirst" && x.Status.Equals(TagStatus.Required));
            var releasedInternationallyBanned = leagueCustomCodeTags.Any(x => x.Tag.Name == "ReleasedInternationally" && x.Status.Equals(TagStatus.Banned));
            var releasedInternationallyRequired = leagueCustomCodeTags.Any(x => x.Tag.Name == "ReleasedInternationally" && x.Status.Equals(TagStatus.Required));

            if (willReleaseInternationallyFirstBanned && gameWillReleaseInternationallyFirst)
            {
                claimErrors.Add(new ClaimError("That game is not eligible because it has the tag: Will Release Internationally First", true));
            }
            if (willReleaseInternationallyFirstRequired && (!gameWillReleaseInternationallyFirst || gameReleasedInternationallyFirst))
            {
                claimErrors.Add(new ClaimError("That game is not eligible because it will not or has not released internationally first", true));
            }
            if (releasedInternationallyBanned && gameReleasedInternationallyFirst)
            {
                bool acquiredBeforeEarlyAccess = masterGame.InternationalReleaseDate.HasValue && masterGame.InternationalReleaseDate.Value > dateOfAcquisition;
                if (!acquiredBeforeEarlyAccess)
                {
                    claimErrors.Add(new ClaimError("That game is not eligible because it has the tag: Released Internationally", true));
                }
            }
            if (releasedInternationallyRequired && !gameReleasedInternationallyFirst)
            {
                claimErrors.Add(new ClaimError("That game is not eligible because it does not have the tag: Released Internationally", true));
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
