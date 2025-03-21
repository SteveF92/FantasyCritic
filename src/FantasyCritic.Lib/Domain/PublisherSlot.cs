using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Domain.ScoringSystems;

namespace FantasyCritic.Lib.Domain;

public class PublisherSlot
{
    public PublisherSlot(int slotNumber, int overallSlotNumber, bool counterPick, SpecialGameSlot? specialGameSlot,
        PublisherGame? publisherGame, bool? counterPickedGameIsValid)
    {
        SlotNumber = slotNumber;
        OverallSlotNumber = overallSlotNumber;
        CounterPick = counterPick;
        SpecialGameSlot = specialGameSlot;
        PublisherGame = publisherGame;

        if (publisherGame is not null && publisherGame.CounterPick != CounterPick)
        {
            throw new Exception($"Something has gone horribly wrong with publisher game: {publisherGame.PublisherGameID}");
        }

        CounterPickedGameIsValid = counterPickedGameIsValid;
    }

    public int SlotNumber { get; }
    public int OverallSlotNumber { get; }
    public bool CounterPick { get; }
    public SpecialGameSlot? SpecialGameSlot { get; }
    public PublisherGame? PublisherGame { get; }
    public bool? CounterPickedGameIsValid { get; }

    public bool SlotIsValid(LeagueYear leagueYear)
    {
        return !GetClaimErrorsForSlot(leagueYear).Any();
    }

    public IReadOnlyList<ClaimError> GetClaimErrorsForSlot(LeagueYear leagueYear)
    {
        var eligibilityFactors = leagueYear.GetEligibilityFactorsForSlot(this);
        if (eligibilityFactors is null)
        {
            return new List<ClaimError>();
        }

        return SlotEligibilityFunctions.GetClaimErrorsForSlot(this, eligibilityFactors);
    }

    public decimal GetRealUpcomingOrProjectedFantasyPoints(SupportedYear supportedYear, bool gameIsValidInSlot, ReleaseSystem releaseSystem, ScoringSystem scoringSystem,
        SystemWideValues systemWideValues, int standardGamesTaken, int numberOfStandardGames)
    {
        var fantasyPoints = GetRealOrUpcomingFantasyPoints(gameIsValidInSlot, releaseSystem, scoringSystem);
        if (fantasyPoints.HasValue)
        {
            return fantasyPoints.Value;
        }

        return GetProjectedFantasyPoints(supportedYear, scoringSystem, systemWideValues, standardGamesTaken, numberOfStandardGames);
    }

    public decimal? GetFantasyPoints(bool gameIsValidInSlot, ReleaseSystem releaseSystem, ScoringSystem scoringSystem, LocalDate currentDate)
    {
        if (PublisherGame is null)
        {
            return null;
        }

        if (CounterPick && CounterPickedGameIsValid.HasValue && !CounterPickedGameIsValid.Value)
        {
            return 0m;
        }

        if (PublisherGame.ManualCriticScore.HasValue)
        {
            return scoringSystem.GetPointsForScore(PublisherGame.ManualCriticScore.Value, CounterPick);
        }
        if (PublisherGame.MasterGame is null)
        {
            return null;
        }

        var calculatedScore = PublisherGame.MasterGame.GetFantasyPoints(releaseSystem, scoringSystem, CounterPick, currentDate);
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

    public decimal? GetRealOrUpcomingFantasyPoints(bool gameIsValidInSlot, ReleaseSystem releaseSystem, ScoringSystem scoringSystem)
    {
        if (PublisherGame is null)
        {
            return null;
        }
        if (PublisherGame.ManualCriticScore.HasValue)
        {
            return scoringSystem.GetPointsForScore(PublisherGame.ManualCriticScore.Value, CounterPick);
        }
        if (PublisherGame.MasterGame is null)
        {
            return null;
        }

        var calculatedScore = PublisherGame.MasterGame.GetRealOrUpcomingFantasyPoints(releaseSystem, scoringSystem, CounterPick);
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

    public decimal GetProjectedFantasyPoints(SupportedYear supportedYear, ScoringSystem scoringSystem, SystemWideValues systemWideValues,
        int standardGamesTaken, int numberOfStandardGames)
    {
        if (supportedYear.Finished)
        {
            return 0;
        }

        if (PublisherGame is null)
        {
            return systemWideValues.GetEmptySlotAveragePoints(CounterPick, standardGamesTaken + 1, numberOfStandardGames);
        }

        if (PublisherGame.MasterGame is null)
        {
            if (PublisherGame.ManualCriticScore.HasValue)
            {
                return PublisherGame.ManualCriticScore.Value;
            }

            return systemWideValues.GetEmptySlotAveragePoints(CounterPick, standardGamesTaken + 1, numberOfStandardGames);
        }

        return PublisherGame.MasterGame.GetProjectedFantasyPoints(scoringSystem, CounterPick);
    }

    public override string ToString()
    {
        var cp = "";
        if (CounterPick)
        {
            cp = "CP-";
        }
        var slotType = "REG";
        if (SpecialGameSlot is not null)
        {
            if (SpecialGameSlot.Tags.Count > 1)
            {
                slotType = "FLX";
            }
            else
            {
                slotType = SpecialGameSlot.Tags[0].ShortName;
            }
        }

        var gameName = "Empty";
        if (PublisherGame is not null)
        {
            gameName = PublisherGame.GameName;
            if (PublisherGame.MasterGame is not null)
            {
                gameName = PublisherGame.MasterGame.MasterGame.GameName;
            }
        }

        return $"{cp}{OverallSlotNumber}|{slotType}|{gameName}";
    }

    public PublisherSlot GetWithReplacedGame(PublisherGame newPublisherGame)
    {
        return new PublisherSlot(SlotNumber, OverallSlotNumber, CounterPick, SpecialGameSlot, newPublisherGame, CounterPickedGameIsValid);
    }
}
