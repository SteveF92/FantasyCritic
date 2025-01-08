using FantasyCritic.Lib.Domain.Results;

namespace FantasyCritic.Lib.Extensions;

public static class LeagueTagExtensions
{
    public static IReadOnlyList<ClaimError> GetRoyaleClaimErrors(IEnumerable<MasterGameTag> allMasterGameTags, MasterGame masterGame, LocalDate dateOfAcquisition)
    {
        var royaleSettings = GetRoyaleEligibilitySettings(allMasterGameTags);
        var claimErrors = GameHasValidTags(royaleSettings, new List<LeagueTagStatus>(), masterGame, masterGame.Tags, dateOfAcquisition);
        return claimErrors;
    }

    public static IReadOnlyList<ClaimError> GameHasValidTags(IEnumerable<LeagueTagStatus> leagueTags, IEnumerable<LeagueTagStatus> slotSpecificTags,
        MasterGame masterGame, IEnumerable<MasterGameTag> masterGameTags, LocalDate dateOfAcquisition)
    {
        List<ClaimError> claimErrors = new List<ClaimError>();
        if (masterGameTags.Any(x => x.Name == "Cancelled"))
        {
            claimErrors.Add(new ClaimError("That game is not eligible because it has been cancelled.", true));
        }

        var combinedLeagueTags = CombineTags(leagueTags, slotSpecificTags);
        var bannedTags = combinedLeagueTags.Where(x => x.Status.Equals(TagStatus.Banned)).ToList();
        var requiredTags = combinedLeagueTags.Where(x => x.Status.Equals(TagStatus.Required)).ToList();

        foreach (var bannedTag in bannedTags)
        {
            if (!bannedTag.GameMeetsTagCriteria(masterGame, masterGameTags, dateOfAcquisition))
            {
                claimErrors.Add(new ClaimError($"That game is not eligible because the {bannedTag.Tag.ReadableName} tag has been banned", true));
            }
        }

        bool hasNoRequiredTags = requiredTags.Any();
        foreach (var requiredTag in requiredTags)
        {
            if (requiredTag.GameMeetsTagCriteria(masterGame, masterGameTags, dateOfAcquisition))
            {
                hasNoRequiredTags = false;
                break;
            }
        }

        if (hasNoRequiredTags)
        {
            claimErrors.Add(new ClaimError($"That game is not eligible because it does not have any of the following required tags: ({string.Join(",", requiredTags.Select(x => x.Tag.ReadableName))})", true));
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
