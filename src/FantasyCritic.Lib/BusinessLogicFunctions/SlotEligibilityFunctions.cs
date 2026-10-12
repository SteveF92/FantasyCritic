using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.BusinessLogicFunctions;

public static class SlotEligibilityFunctions
{
    public static bool GameIsEligibleInLeagueYear(MasterGameWithEligibilityFactors eligibilityFactors)
    {
        var leagueYearClaimErrors = GetClaimErrorsForLeagueYear(eligibilityFactors);
        return !leagueYearClaimErrors.Any();
    }

    public static bool GameIsEligibleInOpenSlot(IReadOnlyList<PublisherSlot> openNonCounterPickSlots, MasterGameWithEligibilityFactors eligibilityFactors)
    {
        foreach (var openSlot in openNonCounterPickSlots)
        {
            var claimErrorsForSlot = GetClaimErrorsForSlot(openSlot, eligibilityFactors);
            if (!claimErrorsForSlot.Any())
            {
                return true;
            }
        }

        return false;
    }

    public static PublisherSlotAcquisitionResult GetPublisherSlotAcquisitionResult(Publisher publisher, LeagueYear leagueYear, MasterGameWithEligibilityFactors? eligibilityFactors,
        bool counterPick, LeagueSlot? validDropSlot, bool acquiringNow, bool managerOverride, bool allowIneligibleSlot, IReadOnlyList<MasterGameTag> allTags)
    {
        string filledSpacesText = "User's game spaces are filled.";
        if (counterPick)
        {
            filledSpacesText = "User's counter pick spaces are filled.";
        }

        var slots = publisher.GetPublisherSlots(leagueYear);
        var openSlots = slots.Where(x => x.CounterPick == counterPick && (x.PublisherGame is null || (validDropSlot is not null && validDropSlot.Equals(x.LeagueSlot))))
            .OrderBy(x => x.SlotNumber).ToList();
        if (eligibilityFactors is null)
        {
            //This is an unlinked master game
            if (openSlots.Any())
            {
                var firstSlot = openSlots.First();
                return new PublisherSlotAcquisitionResult(firstSlot.LeagueSlot);
            }

            if (validDropSlot is not null)
            {
                return new PublisherSlotAcquisitionResult(validDropSlot);
            }

            if (!acquiringNow)
            {
                return new PublisherSlotAcquisitionResult(leagueYear.Options.LeagueSlots[0]);
            }

            return new PublisherSlotAcquisitionResult(new List<ClaimError>() { new ClaimError(filledSpacesText, false, true) });
        }

        var leagueYearClaimErrors = GetClaimErrorsForLeagueYear(eligibilityFactors);
        var nonOverriddenErrors = leagueYearClaimErrors.Where(x => !x.Overridable || !managerOverride).ToList();
        if (nonOverriddenErrors.Any())
        {
            //This game is not eligible in this league at all
            return new PublisherSlotAcquisitionResult(nonOverriddenErrors);
        }

        //At this point, the game is eligible in the league. Does the publisher have an open slot?
        if (!openSlots.Any())
        {
            if (!acquiringNow)
            {
                return new PublisherSlotAcquisitionResult(leagueYear.Options.LeagueSlots[0]);
            }

            return new PublisherSlotAcquisitionResult(new List<ClaimError>() { new ClaimError(filledSpacesText, false, true) });
        }

        var bestEligibleSlot = GetBestEligibleSlot(openSlots, eligibilityFactors, allTags);
        if (bestEligibleSlot is not null)
        {
            return new PublisherSlotAcquisitionResult(bestEligibleSlot.LeagueSlot);
        }

        if (allowIneligibleSlot)
        {
            //This game isn't eligible in any slots, so we will just take the first open one.
            var bestSlot = openSlots.First();
            return new PublisherSlotAcquisitionResult(bestSlot.LeagueSlot);
        }

        return new PublisherSlotAcquisitionResult(new List<ClaimError>() { new ClaimError("Game is not eligible in any open slots.", false, false, true) });
    }

    private static PublisherSlot? GetBestEligibleSlot(IReadOnlyList<PublisherSlot> openSlots, MasterGameWithEligibilityFactors eligibilityFactors, IReadOnlyList<MasterGameTag> allTags)
    {
        //At this point, the game is eligible in at least one currently open slot. Which one is best?
        //First, we want to find the least permissive slots that this game is eligible in
        //If there is a tie, take the slot type that the publisher has more open of
        //Then just take the first of those if that's a tie.

        var eligibleSlots = openSlots.Where(x => !GetClaimErrorsForSlot(x, eligibilityFactors).Any()).ToList();
        if (!eligibleSlots.Any())
        {
            return null;
        }

        var leastAmountOfAllowedTags = eligibleSlots.WhereMin(x => x.GetAllowedTags(eligibilityFactors.Options, allTags).Count).ToList();
        var mostNumerousSlotType = leastAmountOfAllowedTags.GroupBy(x => x.GetAllowedTagsKey(eligibilityFactors.Options, allTags)).MaxBy(x => x.Count());
        var bestSlot = mostNumerousSlotType?.FirstOrDefault();
        return bestSlot;
    }

    public static PublisherSlot? GetTradeSlotResult(Publisher publisher, LeagueYear leagueYear, MasterGameYearWithCounterPick masterGameYearWithCounterPick,
        MasterGameWithEligibilityFactors eligibilityFactors, IReadOnlyList<PublisherSlot> openSlots, IReadOnlyList<MasterGameTag> allTags)
    {
        if (!openSlots.Any())
        {
            return null;
        }

        var bestEligibleSlot = GetBestEligibleSlot(openSlots, eligibilityFactors, allTags);
        if (bestEligibleSlot is not null)
        {
            return bestEligibleSlot;
        }

        //This game isn't eligible in any slots, so we will just take the first open one.
        var bestSlot = openSlots.First();
        return bestSlot;
    }

    public static IReadOnlyList<ClaimError> GetClaimErrorsForLeagueYear(MasterGameWithEligibilityFactors eligibilityFactors)
    {
        //This function returns a list of errors if a game is not eligible in ANY slot
        if (eligibilityFactors.GameIsSpecificallyAllowed)
        {
            return new List<ClaimError>();
        }

        if (eligibilityFactors.GameIsSpecificallyBanned)
        {
            return new List<ClaimError>() { new ClaimError("That game has been specifically banned by your league.", false) };
        }

        var baseEligibilityResult = eligibilityFactors.CheckGameAgainstTags(eligibilityFactors.Options.LeagueTags, new List<LeagueTagStatus>());
        if (!baseEligibilityResult.Any())
        {
            return baseEligibilityResult;
        }

        var slotsWithRequiredTags = eligibilityFactors.Options.LeagueSlots.Where(x => x.RequiredTags.Any());
        foreach (var slotWithRequiredTags in slotsWithRequiredTags)
        {
            var tagsForSlot = slotWithRequiredTags.RequiredTags.Select(x => new LeagueTagStatus(x, TagStatus.Required));
            var specialEligibilityResult = eligibilityFactors.CheckGameAgainstTags(eligibilityFactors.Options.LeagueTags, tagsForSlot);
            if (!specialEligibilityResult.Any())
            {
                return specialEligibilityResult;
            }
        }

        //In this case, the game did not match the base rules, nor any special slots, so the errors we return will be for the base rules.
        return baseEligibilityResult;
    }

    public static IReadOnlyList<ClaimError> GetClaimErrorsForSlot(PublisherSlot publisherSlot, MasterGameWithEligibilityFactors eligibilityFactors)
    {
        //This function returns a list of errors if a game is not eligible in THIS slot
        if (publisherSlot.CounterPick)
        {
            return new List<ClaimError>();
        }

        if (eligibilityFactors.GameIsSpecificallyAllowed)
        {
            return new List<ClaimError>();
        }

        if (eligibilityFactors.GameIsSpecificallyBanned)
        {
            return new List<ClaimError>() { new ClaimError("That game has been specifically banned by your league.", false) };
        }

        if (!publisherSlot.LeagueSlot.RequiredTags.Any())
        {
            var baseEligibilityResult = eligibilityFactors.CheckGameAgainstTags(eligibilityFactors.Options.LeagueTags, new List<LeagueTagStatus>());
            return baseEligibilityResult;
        }

        //This is a special slot
        var tagsForSlot = publisherSlot.LeagueSlot.RequiredTags.Select(x => new LeagueTagStatus(x, TagStatus.Required));
        var specialEligibilityResult = eligibilityFactors.CheckGameAgainstTags(eligibilityFactors.Options.LeagueTags, tagsForSlot);
        return specialEligibilityResult;
    }
}
