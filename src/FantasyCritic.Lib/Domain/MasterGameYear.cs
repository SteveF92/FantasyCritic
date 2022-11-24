using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Domain;

public class MasterGameYear : IEquatable<MasterGameYear>
{
    public MasterGameYear(MasterGame masterGame, int year)
    {
        MasterGame = masterGame;
        Year = year;
    }

    public MasterGameYear(MasterGame masterGame, int year, double percentStandardGame, double percentCounterPick, double eligiblePercentStandardGame,
        double? adjustedPercentCounterPick, int numberOfBids, int totalBidAmount, double bidPercentile, double? averageDraftPosition, double? averageBidAmount,
        double hypeFactor, double dateAdjustedHypeFactor, double peakHypeFactor, double linearRegressionHypeFactor)
    {
        MasterGame = masterGame;
        Year = year;
        PercentStandardGame = percentStandardGame;
        PercentCounterPick = percentCounterPick;
        EligiblePercentStandardGame = eligiblePercentStandardGame;
        AdjustedPercentCounterPick = adjustedPercentCounterPick;
        NumberOfBids = numberOfBids;
        TotalBidAmount = totalBidAmount;
        BidPercentile = bidPercentile;
        AverageDraftPosition = averageDraftPosition;
        AverageWinningBid = averageBidAmount;

        HypeFactor = hypeFactor;
        DateAdjustedHypeFactor = dateAdjustedHypeFactor;
        PeakHypeFactor = peakHypeFactor;
        LinearRegressionHypeFactor = linearRegressionHypeFactor;
    }

    public MasterGame MasterGame { get; }
    public int Year { get; }
    public double PercentStandardGame { get; }
    public double PercentCounterPick { get; }
    public double EligiblePercentStandardGame { get; }
    public double? AdjustedPercentCounterPick { get; }
    public int NumberOfBids { get; }
    public int TotalBidAmount { get; }
    public double BidPercentile { get; }
    public double? AverageDraftPosition { get; }
    public double? AverageWinningBid { get; }
    public double HypeFactor { get; }
    public double DateAdjustedHypeFactor { get; }
    public double PeakHypeFactor { get; }
    public double LinearRegressionHypeFactor { get; }

    public override string ToString() => $"{MasterGame}-{Year}";

    public bool WillRelease()
    {
        if (Year < MasterGame.MinimumReleaseDate.Year)
        {
            return false;
        }

        if (MasterGame.Tags.Any(x => x.Name == "Cancelled"))
        {
            return false;
        }

        return true;
    }

    public bool WillReleaseInQuarter(YearQuarter yearQuarter)
    {
        if (MasterGame.ReleaseDate.HasValue && yearQuarter.FirstDateOfQuarter > MasterGame.ReleaseDate.Value)
        {
            return false;
        }

        if (yearQuarter.LastDateOfQuarter < MasterGame.MinimumReleaseDate)
        {
            return false;
        }

        return true;
    }

    public bool IsRelevantInYear() => IsRelevantInYear(Year);
    
    public bool IsRelevantInYear(int year)
    {
        if (MasterGame.AddedTimestamp.InUtc().Year > year)
        {
            return false;
        }

        if (MasterGame.ReleaseDate.HasValue && MasterGame.ReleaseDate.Value.Year < year)
        {
            return false;
        }

        if (DateAdjustedHypeFactor == 0)
        {
            return false;
        }

        return true;
    }

    public decimal GetProjectedFantasyPoints(ScoringSystem scoringSystem, bool counterPick)
    {
        return scoringSystem.GetPointsForScore(Convert.ToDecimal(LinearRegressionHypeFactor), counterPick);
    }

    public decimal GetRoyaleGameCost()
    {
        decimal projectedPoints = ScoringSystem.GetDefaultScoringSystem(Year).GetPointsForScore(Convert.ToDecimal(LinearRegressionHypeFactor), false);
        projectedPoints *= 1.5m;
        if (projectedPoints < 2m)
        {
            projectedPoints = 2m;
        }

        return projectedPoints;
    }

    public decimal? GetFantasyPoints(ScoringSystem scoringSystem, bool counterPick, LocalDate currentDate)
    {
        if (!WillRelease())
        {
            return 0m;
        }

        if (!MasterGame.IsReleased(currentDate))
        {
            return null;
        }

        if (MasterGame.CriticScore.HasValue)
        {
            return scoringSystem.GetPointsForScore(MasterGame.CriticScore.Value, counterPick);
        }

        return null;
    }

    public decimal? GetRealOrUpcomingFantasyPoints(ScoringSystem scoringSystem, bool counterPick)
    {
        if (!WillRelease())
        {
            return 0m;
        }

        if (MasterGame.CriticScore.HasValue)
        {
            return scoringSystem.GetPointsForScore(MasterGame.CriticScore.Value, counterPick);
        }

        return null;
    }

    public bool Equals(MasterGameYear? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return MasterGame.Equals(other.MasterGame) && Year == other.Year;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MasterGameYear) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MasterGame, Year);
    }
}
