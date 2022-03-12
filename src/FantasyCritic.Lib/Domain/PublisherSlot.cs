using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Services;

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
            return !GetClaimErrorsForSlot(leagueYear).Any();
        }

        public IReadOnlyList<ClaimError> GetClaimErrorsForSlot(LeagueYear leagueYear)
        {
            var eligibilityFactors = leagueYear.GetEligibilityFactorsForSlot(this);
            if (eligibilityFactors.HasNoValue)
            {
                return new List<ClaimError>();
            }

            return SlotEligibilityService.GetClaimErrorsForSlot(this, eligibilityFactors.Value);
        }

        public decimal GetProjectedOrRealFantasyPoints(bool gameIsValidInSlot, ScoringSystem scoringSystem, SystemWideValues systemWideValues, bool simpleProjections, LocalDate currentDate)
        {
            if (PublisherGame.HasNoValue)
            {
                return systemWideValues.GetAveragePoints(!simpleProjections, CounterPick);
            }

            if (PublisherGame.Value.MasterGame.HasNoValue)
            {
                if (PublisherGame.Value.ManualCriticScore.HasValue)
                {
                    return PublisherGame.Value.ManualCriticScore.Value;
                }

                return systemWideValues.GetAveragePoints(!simpleProjections, CounterPick);
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

            var calculatedScore = PublisherGame.Value.MasterGame.Value.CalculateFantasyPoints(scoringSystem, CounterPick, currentDate, true);
            if (gameIsValidInSlot)
            {
                return calculatedScore;
            }

            if (calculatedScore.HasValue && calculatedScore.Value <= 0m)
            {
                return calculatedScore;
            }

            return 0m;
        }

        public override string ToString()
        {
            var cp = "";
            if (CounterPick)
            {
                cp = "CP-";
            }
            var slotType = "REG";
            if (SpecialGameSlot.HasValue)
            {
                if (SpecialGameSlot.Value.Tags.Count > 1)
                {
                    slotType = "FLX";
                }
                else
                {
                    slotType = SpecialGameSlot.Value.Tags[0].ShortName;
                }
            }

            var gameName = "Empty";
            if (PublisherGame.HasValue)
            {
                gameName = PublisherGame.Value.GameName;
                if (PublisherGame.Value.MasterGame.HasValue)
                {
                    gameName = PublisherGame.Value.MasterGame.Value.MasterGame.GameName;
                }
            }

            return $"{cp}{OverallSlotNumber}|{slotType}|{gameName}";
        }

        public PublisherSlot GetWithReplacedGame(PublisherGame newPublisherGame)
        {
            return new PublisherSlot(SlotNumber, OverallSlotNumber, CounterPick, SpecialGameSlot, newPublisherGame);
        }
    }
}
