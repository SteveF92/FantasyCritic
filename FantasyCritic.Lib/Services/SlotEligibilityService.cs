using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Results;

namespace FantasyCritic.Lib.Services
{
    public static class SlotEligibilityService
    {
        public static bool SlotIsCurrentlyValid(PublisherSlot publisherSlot, Maybe<MasterGameWithEligibilityFactors> eligibilityFactors)
        {
            if (eligibilityFactors.HasNoValue)
            {
                //This is an unlinked master game
                return true;
            }

            var leagueYearClaimErrors = GetClaimErrorsForLeagueYear(eligibilityFactors.Value);
            if (leagueYearClaimErrors.Any())
            {
                //This game is not eligible in this league at all
                return false;
            }

            var slotClaimErrors = GetClaimErrorsForSlot(publisherSlot, eligibilityFactors.Value);
            return slotClaimErrors.Any();
        }

        public static bool GameIsEligibleInLeagueYear(MasterGameWithEligibilityFactors eligibilityFactors)
        {
            var leagueYearClaimErrors = GetClaimErrorsForLeagueYear(eligibilityFactors);
            return leagueYearClaimErrors.Any();
        }

        public static PublisherSlotAcquisitionResult GetPublisherSlotAcquisitionResult(Publisher publisher, Maybe<MasterGameWithEligibilityFactors> eligibilityFactors, bool counterPick, int? validDropSlot)
        {
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

                return new PublisherSlotAcquisitionResult(new List<ClaimError>() { new ClaimError("User's game spaces are filled.", false, true) });
            }

            var leagueYearClaimErrors = GetClaimErrorsForLeagueYear(eligibilityFactors.Value);
            if (leagueYearClaimErrors.Any())
            {
                return new PublisherSlotAcquisitionResult(leagueYearClaimErrors);
            }

            //At this point, the game is eligible in the league. Does the publisher have an open slot?
            if (!openSlots.Any())
            {
                if (validDropSlot.HasValue)
                {
                    return new PublisherSlotAcquisitionResult(validDropSlot.Value);
                }

                return new PublisherSlotAcquisitionResult(new List<ClaimError>() { new ClaimError("User's game spaces are filled.", false, true) });
            }

            //At this point, the game is eligible in at least one currently open slot. Which one is best?
            foreach (var openSlot in openSlots)
            {
                var claimErrorsForSlot = GetClaimErrorsForSlot(openSlot, eligibilityFactors.Value);
                if (!claimErrorsForSlot.Any())
                {
                    return new PublisherSlotAcquisitionResult(openSlot.SlotNumber);
                }
            }

            throw new Exception($"Something went horribly wrong detecting eligibility for: {publisher.PublisherID} | {eligibilityFactors.Value.MasterGame.GameName}");
        }

        private static IReadOnlyList<ClaimError> GetClaimErrorsForLeagueYear(MasterGameWithEligibilityFactors eligibilityFactors)
        {
            //This function returns a list of errors if a game is not eligible in ANY slot
            throw new NotImplementedException();
        }

        private static IReadOnlyList<ClaimError> GetClaimErrorsForSlot(PublisherSlot publisherSlot, MasterGameWithEligibilityFactors eligibilityFactors)
        {
            //This function returns a list of errors if a game is not eligible in THIS slot
            throw new NotImplementedException();
        }
    }
}
