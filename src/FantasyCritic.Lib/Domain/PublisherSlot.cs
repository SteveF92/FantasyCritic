using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Domain;

public class PublisherSlot
{
    public PublisherSlot(int slotNumber, int overallSlotNumber, bool counterPick, Maybe<SpecialGameSlot> specialGameSlot, Maybe<PublisherGame> publisherGame)
    {
        SlotNumber = slotNumber;
        OverallSlotNumber = overallSlotNumber;
        CounterPick = counterPick;
        SpecialGameSlot = specialGameSlot;
        PublisherGame = publisherGame;

        if (publisherGame.HasValueTempoTemp && publisherGame.ValueTempoTemp.CounterPick != CounterPick)
        {
            throw new Exception($"Something has gone horribly wrong with publisher game: {publisherGame.ValueTempoTemp.PublisherGameID}");
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
        if (eligibilityFactors.HasNoValueTempoTemp)
        {
            return new List<ClaimError>();
        }

        return SlotEligibilityService.GetClaimErrorsForSlot(this, eligibilityFactors.ValueTempoTemp);
    }

    public decimal GetProjectedOrRealFantasyPoints(bool gameIsValidInSlot, ScoringSystem scoringSystem, SystemWideValues systemWideValues, LocalDate currentDate,
        int standardGamesTaken, int numberOfStandardGames)
    {
        if (PublisherGame.HasNoValueTempoTemp)
        {
            return systemWideValues.GetEmptySlotAveragePoints(CounterPick, standardGamesTaken + 1, numberOfStandardGames);
        }

        if (PublisherGame.ValueTempoTemp.MasterGame.HasNoValueTempoTemp)
        {
            if (PublisherGame.ValueTempoTemp.ManualCriticScore.HasValue)
            {
                return PublisherGame.ValueTempoTemp.ManualCriticScore.Value;
            }

            return systemWideValues.GetEmptySlotAveragePoints(CounterPick, standardGamesTaken + 1, numberOfStandardGames);
        }

        decimal? fantasyPoints = CalculateFantasyPoints(gameIsValidInSlot, scoringSystem, currentDate);
        if (fantasyPoints.HasValue)
        {
            return fantasyPoints.Value;
        }

        return PublisherGame.ValueTempoTemp.MasterGame.ValueTempoTemp.GetProjectedOrRealFantasyPoints(scoringSystem, CounterPick, currentDate);
    }

    public decimal? CalculateFantasyPoints(bool gameIsValidInSlot, ScoringSystem scoringSystem, LocalDate currentDate)
    {
        if (PublisherGame.HasNoValueTempoTemp)
        {
            return null;
        }
        if (PublisherGame.ValueTempoTemp.ManualCriticScore.HasValue)
        {
            return scoringSystem.GetPointsForScore(PublisherGame.ValueTempoTemp.ManualCriticScore.Value, CounterPick);
        }
        if (PublisherGame.ValueTempoTemp.MasterGame.HasNoValueTempoTemp)
        {
            return null;
        }

        var calculatedScore = PublisherGame.ValueTempoTemp.MasterGame.ValueTempoTemp.CalculateFantasyPoints(scoringSystem, CounterPick, currentDate, true);
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
        if (SpecialGameSlot.HasValueTempoTemp)
        {
            if (SpecialGameSlot.ValueTempoTemp.Tags.Count > 1)
            {
                slotType = "FLX";
            }
            else
            {
                slotType = SpecialGameSlot.ValueTempoTemp.Tags[0].ShortName;
            }
        }

        var gameName = "Empty";
        if (PublisherGame.HasValueTempoTemp)
        {
            gameName = PublisherGame.ValueTempoTemp.GameName;
            if (PublisherGame.ValueTempoTemp.MasterGame.HasValueTempoTemp)
            {
                gameName = PublisherGame.ValueTempoTemp.MasterGame.ValueTempoTemp.MasterGame.GameName;
            }
        }

        return $"{cp}{OverallSlotNumber}|{slotType}|{gameName}";
    }

    public PublisherSlot GetWithReplacedGame(PublisherGame newPublisherGame)
    {
        return new PublisherSlot(SlotNumber, OverallSlotNumber, CounterPick, SpecialGameSlot, newPublisherGame);
    }
}
