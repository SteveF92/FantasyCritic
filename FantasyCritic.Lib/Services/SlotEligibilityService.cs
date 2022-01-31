using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Services
{
    public static class SlotEligibilityService
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

        public static PublisherSlotAcquisitionResult GetPublisherSlotAcquisitionResult(Publisher publisher, Maybe<MasterGameWithEligibilityFactors> eligibilityFactors, bool counterPick, int? validDropSlot, bool watchListing)
        {
            string filledSpacesText = "User's game spaces are filled.";
            if (counterPick)
            {
                filledSpacesText = "User's counter pick spaces are filled.";
            }

            var slots = publisher.GetPublisherSlots();
            var openSlots = slots.Where(x => x.CounterPick == counterPick && x.PublisherGame.HasNoValue).OrderBy(x => x.SlotNumber).ToList();
            if (eligibilityFactors.HasNoValue)
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

                if (watchListing)
                {
                    return new PublisherSlotAcquisitionResult(1);
                }

                return new PublisherSlotAcquisitionResult(new List<ClaimError>() { new ClaimError(filledSpacesText, false, true) });
            }

            var leagueYearClaimErrors = GetClaimErrorsForLeagueYear(eligibilityFactors.Value);
            if (leagueYearClaimErrors.Any())
            {
                //This game is not eligible in this league at all
                return new PublisherSlotAcquisitionResult(leagueYearClaimErrors);
            }

            //At this point, the game is eligible in the league. Does the publisher have an open slot?
            if (!openSlots.Any())
            {
                if (validDropSlot.HasValue)
                {
                    return new PublisherSlotAcquisitionResult(validDropSlot.Value);
                }

                if (watchListing)
                {
                    return new PublisherSlotAcquisitionResult(1);
                }

                return new PublisherSlotAcquisitionResult(new List<ClaimError>() { new ClaimError(filledSpacesText, false, true) });
            }

            //At this point, the game is eligible in at least one currently open slot. Which one is best?
            //We want to check the special slots first, then the regular slots.
            var openSpotsToCheckOrder = openSlots
                .OrderByDescending(x => x.SpecialGameSlot.HasValue)
                .ThenBy(x => x.SlotNumber).ToList();
            foreach (var openSlot in openSpotsToCheckOrder)
            {
                var claimErrorsForSlot = GetClaimErrorsForSlot(openSlot, eligibilityFactors.Value);
                if (!claimErrorsForSlot.Any())
                {
                    return new PublisherSlotAcquisitionResult(openSlot.SlotNumber);
                }
            }

            //This game isn't eligible in any slots, so we will just take the first open one.
            var bestSlot = openSlots.First();
            return new PublisherSlotAcquisitionResult(bestSlot.SlotNumber);
        }

        private static IReadOnlyList<ClaimError> GetClaimErrorsForLeagueYear(MasterGameWithEligibilityFactors eligibilityFactors)
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

            if (publisherSlot.SpecialGameSlot.HasNoValue)
            {
                var baseEligibilityResult = eligibilityFactors.CheckGameAgainstTags(eligibilityFactors.Options.LeagueTags, new List<LeagueTagStatus>());
                return baseEligibilityResult;
            }

            //This is a special slot
            var tagsForSlot = publisherSlot.SpecialGameSlot.Value.Tags.Select(x => new LeagueTagStatus(x, TagStatus.Required));
            var specialEligibilityResult = eligibilityFactors.CheckGameAgainstTags(eligibilityFactors.Options.LeagueTags, tagsForSlot);
            return specialEligibilityResult;
        }
    }
}
