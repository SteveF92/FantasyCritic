using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.Results;

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

    public static PublisherSlotAcquisitionResult GetPublisherSlotAcquisitionResult(Publisher publisher, LeagueOptions leagueOptions, MasterGameWithEligibilityFactors? eligibilityFactors,
        bool counterPick, int? validDropSlot, bool acquiringNow, bool managerOverride)
    {
        string filledSpacesText = "User's game spaces are filled.";
        if (counterPick)
        {
            filledSpacesText = "User's counter pick spaces are filled.";
        }

        var slots = publisher.GetPublisherSlots(leagueOptions);
        var openSlots = slots.Where(x => x.CounterPick == counterPick && x.PublisherGame is null).OrderBy(x => x.SlotNumber).ToList();
        if (eligibilityFactors is null)
        {
            //This is an unlinked master game
            if (openSlots.Any())
            {
                var firstSlot = openSlots.First();
                return new PublisherSlotAcquisitionResult(firstSlot.SlotNumber);
            }

            if (validDropSlot.HasValue)
            {
                return new PublisherSlotAcquisitionResult(validDropSlot.Value);
            }

            if (!acquiringNow)
            {
                return new PublisherSlotAcquisitionResult(1);
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
            if (validDropSlot.HasValue)
            {
                return new PublisherSlotAcquisitionResult(validDropSlot.Value);
            }

            if (!acquiringNow)
            {
                return new PublisherSlotAcquisitionResult(1);
            }

            return new PublisherSlotAcquisitionResult(new List<ClaimError>() { new ClaimError(filledSpacesText, false, true) });
        }

        //At this point, the game is eligible in at least one currently open slot. Which one is best?
        //We want to check the special slots first, then the regular slots.
        var openSpotsToCheckOrder = openSlots
            .OrderByDescending(x => x.SpecialGameSlot is not null)
            .ThenBy(x => x.SlotNumber).ToList();
        foreach (var openSlot in openSpotsToCheckOrder)
        {
            var claimErrorsForSlot = GetClaimErrorsForSlot(openSlot, eligibilityFactors);
            if (!claimErrorsForSlot.Any())
            {
                return new PublisherSlotAcquisitionResult(openSlot.SlotNumber);
            }
        }

        //This game isn't eligible in any slots, so we will just take the first open one.
        var bestSlot = openSlots.First();
        return new PublisherSlotAcquisitionResult(bestSlot.SlotNumber);
    }

    public static int? GetTradeSlotResult(Publisher publisher, LeagueOptions leagueOptions, MasterGameYearWithCounterPick masterGameYearWithCounterPick, MasterGameWithEligibilityFactors eligibilityFactors, IEnumerable<int> openSlotNumbers)
    {
        var slots = publisher.GetPublisherSlots(leagueOptions);
        var openSlots = slots.Where(x => x.CounterPick == masterGameYearWithCounterPick.CounterPick && openSlotNumbers.Contains(x.SlotNumber)).OrderBy(x => x.SlotNumber).ToList();
        if (!openSlots.Any())
        {
            return null;
        }

        //At this point, there is an open slot. Which one is best?
        //We want to check the special slots first, then the regular slots.
        var openSpotsToCheckOrder = openSlots
            .OrderByDescending(x => x.SpecialGameSlot is not null)
            .ThenBy(x => x.SlotNumber).ToList();
        foreach (var openSlot in openSpotsToCheckOrder)
        {
            var claimErrorsForSlot = GetClaimErrorsForSlot(openSlot, eligibilityFactors);
            if (!claimErrorsForSlot.Any())
            {
                return openSlot.SlotNumber;
            }
        }

        //This game isn't eligible in any slots, so we will just take the first open one.
        var bestSlot = openSlots.First();
        return bestSlot.SlotNumber;
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

        var specialGameSlots = eligibilityFactors.Options.SpecialGameSlots;
        foreach (var specialGameSlot in specialGameSlots)
        {
            var tagsForSlot = specialGameSlot.Tags.Select(x => new LeagueTagStatus(x, TagStatus.Required));
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

        if (publisherSlot.SpecialGameSlot is null)
        {
            var baseEligibilityResult = eligibilityFactors.CheckGameAgainstTags(eligibilityFactors.Options.LeagueTags, new List<LeagueTagStatus>());
            return baseEligibilityResult;
        }

        //This is a special slot
        var tagsForSlot = publisherSlot.SpecialGameSlot.Tags.Select(x => new LeagueTagStatus(x, TagStatus.Required));
        var specialEligibilityResult = eligibilityFactors.CheckGameAgainstTags(eligibilityFactors.Options.LeagueTags, tagsForSlot);
        return specialEligibilityResult;
    }
}
