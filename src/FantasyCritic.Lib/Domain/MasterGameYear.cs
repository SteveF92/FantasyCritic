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
        double? adjustedPercentCounterPick, int numberOfBids, int totalBidAmount, double bidPercentile, double? averageDraftPosition, double? averageWinningBid,
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
        AverageWinningBid = averageWinningBid;

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

    public WillReleaseStatus GetWillReleaseStatus() => MasterGame.GetWillReleaseStatus(Year);
    public WillReleaseStatus GetWillReleaseStatus(YearQuarter yearQuarter) => MasterGame.GetWillReleaseStatus(yearQuarter);
    public bool CouldRelease() => MasterGame.CouldReleaseInYear(Year);
    public bool CouldReleaseInQuarter(YearQuarter yearQuarter) => MasterGame.CouldReleaseInQuarter(yearQuarter);

    public bool IsRelevantInYear(bool strict) => IsRelevantInYear(Year, strict);
    
    public bool IsRelevantInYear(int year, bool strict)
    {
        if (MasterGame.AddedTimestamp.InUtc().Year > year)
        {
            return false;
        }

        if (MasterGame.ReleaseDate.HasValue && MasterGame.ReleaseDate.Value.Year < year)
        {
            return false;
        }

        var hypeFactorLimit = 3;
        if (!strict)
        {
            hypeFactorLimit = 0;
        }

        if (DateAdjustedHypeFactor <= hypeFactorLimit)
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

    public decimal? GetFantasyPoints(ReleaseSystem releaseSystem, ScoringSystem scoringSystem, bool counterPick, LocalDate currentDate)
    {
        if (MasterGame.ReleaseDate.HasValue && MasterGame.ReleaseDate.Value.Year < Year)
        {
            return 0m;
        }

        if (!CouldRelease())
        {
            return 0m;
        }

        if (releaseSystem == ReleaseSystem.MustBeReleased)
        {
            if (!MasterGame.IsReleased(currentDate))
            {
                return null;
            }
        }

        if (MasterGame.CriticScore.HasValue)
        {
            return scoringSystem.GetPointsForScore(MasterGame.CriticScore.Value, counterPick);
        }

        return null;
    }

    public decimal? GetRealOrUpcomingFantasyPoints(ScoringSystem scoringSystem, bool counterPick)
    {
        if (!CouldRelease())
        {
            return 0m;
        }

        if (MasterGame.CriticScore.HasValue)
        {
            return scoringSystem.GetPointsForScore(MasterGame.CriticScore.Value, counterPick);
        }

        return null;
    }

    public MasterGameYear WithNewMasterGame(MasterGame newMasterGame)
    {
        return new MasterGameYear(newMasterGame, Year, PercentStandardGame, PercentCounterPick,
            EligiblePercentStandardGame, AdjustedPercentCounterPick, NumberOfBids, TotalBidAmount, BidPercentile,
            AverageDraftPosition, AverageWinningBid, HypeFactor, DateAdjustedHypeFactor, PeakHypeFactor,
            LinearRegressionHypeFactor);
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
