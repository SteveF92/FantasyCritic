namespace FantasyCritic.Lib.Domain;

public class SystemWideValues
{
    public SystemWideValues(decimal averageStandardGamePoints, decimal averagePickupOnlyStandardGamePoints,
        decimal averageCounterPickPoints, IEnumerable<AveragePickPositionPoints> averageStandardGamePointsByPickPosition)
    {
        AverageStandardGamePoints = averageStandardGamePoints;
        AveragePickupOnlyStandardGamePoints = averagePickupOnlyStandardGamePoints;
        AverageCounterPickPoints = averageCounterPickPoints;
        AverageStandardGamePointsByPickPosition = averageStandardGamePointsByPickPosition.ToList();
    }

    public decimal AverageStandardGamePoints { get; }
    public decimal AveragePickupOnlyStandardGamePoints { get; }
    public decimal AverageCounterPickPoints { get; }
    public IReadOnlyList<AveragePickPositionPoints> AverageStandardGamePointsByPickPosition { get; }

    public decimal GetEmptySlotAveragePoints(bool counterPick, int lowestPossiblePickNumber, int highestPossiblePickNumber)
    {
        if (counterPick)
        {
            return AverageCounterPickPoints;
        }

        var relevantDataPoints = AverageStandardGamePointsByPickPosition
            .Where(x => x.PickPosition >= lowestPossiblePickNumber && x.PickPosition <= highestPossiblePickNumber).Select(x => x.AveragePoints)
            .ToList();
        if (relevantDataPoints.Any())
        {
            return AveragePickupOnlyStandardGamePoints;
        }

        return relevantDataPoints.Average();
    }
}
