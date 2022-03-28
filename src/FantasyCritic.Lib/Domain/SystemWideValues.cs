namespace FantasyCritic.Lib.Domain;

public class SystemWideValues
{
    public SystemWideValues(decimal averageStandardGamePoints, decimal averagePickupOnlyStandardGamePoints,
        decimal averageCounterPickPoints, IReadOnlyDictionary<int, decimal> averageStandardGamePointsByPickPosition)
    {
        AverageStandardGamePoints = averageStandardGamePoints;
        AveragePickupOnlyStandardGamePoints = averagePickupOnlyStandardGamePoints;
        AverageCounterPickPoints = averageCounterPickPoints;
        AverageStandardGamePointsByPickPosition = averageStandardGamePointsByPickPosition;
    }

    public decimal AverageStandardGamePoints { get; }
    public decimal AveragePickupOnlyStandardGamePoints { get; }
    public decimal AverageCounterPickPoints { get; }
    public IReadOnlyDictionary<int, decimal> AverageStandardGamePointsByPickPosition { get; }

    public decimal GetEmptySlotAveragePoints(bool counterPick, int lowestPossiblePickNumber, int highestPossiblePickNumber)
    {
        if (counterPick)
        {
            return AverageCounterPickPoints;
        }

        var relevantDataPoints = AverageStandardGamePointsByPickPosition
            .Where(x => x.Key >= lowestPossiblePickNumber && x.Key <= highestPossiblePickNumber).Select(x => x.Value)
            .ToList();
        if (relevantDataPoints.Any())
        {
            return AveragePickupOnlyStandardGamePoints;
        }

        return relevantDataPoints.Average();
    }
}
