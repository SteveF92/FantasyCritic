using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Services;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class PublisherSlot
    {
        public PublisherSlot(int slotNumber, int overallSlotNumber, bool counterPick, Maybe<SpecialGameSlot> specialGameSlot, Maybe<PublisherGame> publisherGame)
        {
            SlotNumber = slotNumber;
            OverallSlotNumber = overallSlotNumber;
            CounterPick = counterPick;
            SpecialGameSlot = specialGameSlot;
            PublisherGame = publisherGame;
        }

        public int SlotNumber { get; }
        public int OverallSlotNumber { get; }
        public bool CounterPick { get; }
        public Maybe<SpecialGameSlot> SpecialGameSlot { get; }
        public Maybe<PublisherGame> PublisherGame { get; }

        public decimal GetProjectedOrRealFantasyPoints(MasterGameWithEligibilityFactors eligibilityFactors, SystemWideValues systemWideValues, bool simpleProjections, LocalDate currentDate)
        {
            if (PublisherGame.HasNoValue)
            {
                return systemWideValues.GetAveragePoints(CounterPick);
            }

            if (PublisherGame.Value.MasterGame.HasNoValue)
            {
                if (PublisherGame.Value.ManualCriticScore.HasValue)
                {
                    return PublisherGame.Value.ManualCriticScore.Value;
                }

                return systemWideValues.GetAveragePoints(CounterPick);
            }

            decimal? fantasyPoints = CalculateFantasyPoints(eligibilityFactors, currentDate);
            if (fantasyPoints.HasValue)
            {
                return fantasyPoints.Value;
            }

            if (simpleProjections)
            {
                return PublisherGame.Value.MasterGame.Value.GetSimpleProjectedFantasyPoints(systemWideValues, CounterPick);
            }

            return PublisherGame.Value.MasterGame.Value.GetProjectedOrRealFantasyPoints(eligibilityFactors.Options.ScoringSystem, CounterPick, currentDate);
        }

        public decimal? CalculateFantasyPoints(MasterGameWithEligibilityFactors eligibilityFactors, LocalDate currentDate)
        {
            if (PublisherGame.HasNoValue)
            {
                return null;
            }
            if (PublisherGame.Value.ManualCriticScore.HasValue)
            {
                return eligibilityFactors.Options.ScoringSystem.GetPointsForScore(PublisherGame.Value.ManualCriticScore.Value, CounterPick);
            }
            if (PublisherGame.Value.MasterGame.HasNoValue)
            {
                return null;
            }

            var eligible = SlotEligibilityService.SlotIsCurrentlyValid(this, eligibilityFactors);
            if (!eligible)
            {
                return 0m;
            }

            return PublisherGame.Value.MasterGame.Value.CalculateFantasyPoints(eligibilityFactors.Options.ScoringSystem, CounterPick, currentDate, true);
        }
    }
}
