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

            if (publisherGame.HasValue && publisherGame.Value.CounterPick != CounterPick)
            {
                throw new Exception($"Something has gone horribly wrong with publisher game: {publisherGame.Value.PublisherGameID}");
            }
        }

        public int SlotNumber { get; }
        public int OverallSlotNumber { get; }
        public bool CounterPick { get; }
        public Maybe<SpecialGameSlot> SpecialGameSlot { get; }
        public Maybe<PublisherGame> PublisherGame { get; }

        public bool SlotIsValid(LeagueYear leagueYear)
        {
            var eligibilityFactors = leagueYear.GetEligibilityFactorsForSlot(this);
            if (eligibilityFactors.HasNoValue)
            {
                return true;
            }

            return SlotEligibilityService.SlotIsCurrentlyValid(this, eligibilityFactors.Value);
        }

        public decimal GetProjectedOrRealFantasyPoints(bool gameIsValidInSlot, ScoringSystem scoringSystem, SystemWideValues systemWideValues, bool simpleProjections, LocalDate currentDate)
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

            decimal? fantasyPoints = CalculateFantasyPoints(gameIsValidInSlot, scoringSystem, currentDate);
            if (fantasyPoints.HasValue)
            {
                return fantasyPoints.Value;
            }

            if (simpleProjections)
            {
                return PublisherGame.Value.MasterGame.Value.GetSimpleProjectedFantasyPoints(systemWideValues, CounterPick);
            }

            return PublisherGame.Value.MasterGame.Value.GetProjectedOrRealFantasyPoints(scoringSystem, CounterPick, currentDate);
        }

        public decimal? CalculateFantasyPoints(bool gameIsValidInSlot, ScoringSystem scoringSystem, LocalDate currentDate)
        {
            if (PublisherGame.HasNoValue)
            {
                return null;
            }
            if (PublisherGame.Value.ManualCriticScore.HasValue)
            {
                return scoringSystem.GetPointsForScore(PublisherGame.Value.ManualCriticScore.Value, CounterPick);
            }
            if (PublisherGame.Value.MasterGame.HasNoValue)
            {
                return null;
            }

            if (!gameIsValidInSlot)
            {
                return 0m;
            }

            return PublisherGame.Value.MasterGame.Value.CalculateFantasyPoints(scoringSystem, CounterPick, currentDate, true);
        }
    }
}
