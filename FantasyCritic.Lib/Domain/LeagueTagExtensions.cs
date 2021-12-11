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
            return GameHasValidTags(GetRoyaleEligibilitySettings(allMasterGameTags), new List<LeagueTagStatus>(),
                masterGame, masterGame.Tags, dateOfAcquisition);
        }

        public static IReadOnlyList<ClaimError> GameHasValidTags(IEnumerable<LeagueTagStatus> leagueTags, IEnumerable<LeagueTagStatus> slotSpecificTags, 
            MasterGame masterGame, IEnumerable<MasterGameTag> masterGameTags, LocalDate dateOfAcquisition)
        {
            var combinedLeagueTags = CombineTags(leagueTags, slotSpecificTags);

            var masterGameCustomCodeTags = masterGameTags.Where(x => x.HasCustomCode).ToList();
            var masterGameNonCustomCodeTags = masterGameTags.Except(masterGameCustomCodeTags).ToList();
            var leagueCustomCodeTags = combinedLeagueTags.Where(x => x.Tag.HasCustomCode).ToList();
            var leagueNonCustomCodeTags = combinedLeagueTags.Except(leagueCustomCodeTags).ToList();

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
            var masterGameCustomCodeTagsHashSet = masterGameCustomCodeTags.Select(x => x.Name).ToHashSet();
            var leagueCustomCodeTagsDictionary = leagueCustomCodeTags.ToDictionary(x => x.Tag.Name);

            var gameIsPlannedForEarlyAccess = masterGameCustomCodeTagsHashSet.Contains("PlannedForEarlyAccess");
            var gameIsInEarlyAccess = masterGameCustomCodeTagsHashSet.Contains("CurrentlyInEarlyAccess");
            if (leagueCustomCodeTagsDictionary.TryGetValue("PlannedForEarlyAccess", out var plannedEarlyAccessTag))
            {
                if (plannedEarlyAccessTag.Status.Equals(TagStatus.Banned) && gameIsPlannedForEarlyAccess)
                {
                    claimErrors.Add(new ClaimError("That game is not eligible because it has the tag: Planned For Early Access", true));
                }
                else if (plannedEarlyAccessTag.Status.Equals(TagStatus.Required) && (!gameIsPlannedForEarlyAccess || gameIsInEarlyAccess))
                {
                    claimErrors.Add(new ClaimError("That game is not eligible because it is not planned for or in early access", true));
                }
            }
            if (leagueCustomCodeTagsDictionary.TryGetValue("CurrentlyInEarlyAccess", out var currentlyInEarlyAccessTags))
            {
                if (currentlyInEarlyAccessTags.Status.Equals(TagStatus.Banned) && gameIsInEarlyAccess)
                {
                    bool acquiredBeforeEarlyAccess = masterGame.EarlyAccessReleaseDate.HasValue && masterGame.EarlyAccessReleaseDate.Value > dateOfAcquisition;
                    if (!acquiredBeforeEarlyAccess)
                    {
                        claimErrors.Add(new ClaimError("That game is not eligible because it has the tag: Currently in Early Access", true));
                    }
                }
                else if (currentlyInEarlyAccessTags.Status.Equals(TagStatus.Required) && !gameIsInEarlyAccess)
                {
                    claimErrors.Add(new ClaimError("That game is not eligible because it does not have the tag: Currently in Early Access", true));
                }
            }

            var gameWillReleaseInternationallyFirst = masterGameCustomCodeTagsHashSet.Contains("WillReleaseInternationallyFirst");
            var gameReleasedInternationallyFirst = masterGameCustomCodeTagsHashSet.Contains("ReleasedInternationally");
            if (leagueCustomCodeTagsDictionary.TryGetValue("WillReleaseInternationallyFirst", out var willReleaseInternationallyFirstTag))
            {
                if (willReleaseInternationallyFirstTag.Status.Equals(TagStatus.Banned) && gameWillReleaseInternationallyFirst)
                {
                    claimErrors.Add(new ClaimError("That game is not eligible because it has the tag: Will Release Internationally First", true));
                }
                else if (willReleaseInternationallyFirstTag.Status.Equals(TagStatus.Required) && (!gameWillReleaseInternationallyFirst || gameReleasedInternationallyFirst))
                {
                    claimErrors.Add(new ClaimError("That game is not eligible because it will not or has not released internationally first", true));
                }
            }
            if (leagueCustomCodeTagsDictionary.TryGetValue("ReleasedInternationally", out var releasedInternationallyTag))
            {
                if (releasedInternationallyTag.Status.Equals(TagStatus.Banned) && gameReleasedInternationallyFirst)
                {
                    bool acquiredBeforeInternationalRelease = masterGame.InternationalReleaseDate.HasValue && masterGame.InternationalReleaseDate.Value > dateOfAcquisition;
                    if (!acquiredBeforeInternationalRelease)
                    {
                        claimErrors.Add(new ClaimError("That game is not eligible because it has the tag: Released Internationally", true));
                    }
                }
                else if (releasedInternationallyTag.Status.Equals(TagStatus.Required) && !gameReleasedInternationallyFirst)
                {
                    claimErrors.Add(new ClaimError("That game is not eligible because it does not have the tag: Released Internationally", true));
                }
            }

            return claimErrors;
        }

        private static IReadOnlyList<LeagueTagStatus> CombineTags(IEnumerable<LeagueTagStatus> leagueTags, IEnumerable<LeagueTagStatus> slotTags)
        {
            Dictionary<MasterGameTag, LeagueTagStatus> combinedLeagueTags = slotTags.ToDictionary(x => x.Tag);
            foreach (var leagueTag in leagueTags)
            {
                bool isSlotTag = combinedLeagueTags.ContainsKey(leagueTag.Tag);
                if (!isSlotTag)
                {
                    combinedLeagueTags.Add(leagueTag.Tag, leagueTag);
                }
            }

            return combinedLeagueTags.Values.ToList();
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
