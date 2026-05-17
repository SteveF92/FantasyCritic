using System.Diagnostics.CodeAnalysis;

namespace FantasyCritic.MySQL.Entities;

internal class SystemWideValuesYearEntity
{
    public SystemWideValuesYearEntity()
    {
    }

    [SetsRequiredMembers]
    public SystemWideValuesYearEntity(int year, decimal averageStandardGamePoints, int standardGameDataPoints,
        decimal averagePickupOnlyStandardGamePoints, int pickupOnlyDataPoints, decimal averageCounterPickPoints, int counterPickDataPoints)
    {
        Year = year;
        AverageStandardGamePoints = averageStandardGamePoints;
        StandardGameDataPoints = standardGameDataPoints;
        AveragePickupOnlyStandardGamePoints = averagePickupOnlyStandardGamePoints;
        PickupOnlyDataPoints = pickupOnlyDataPoints;
        AverageCounterPickPoints = averageCounterPickPoints;
        CounterPickDataPoints = counterPickDataPoints;
    }

    public required int Year { get; init; }
    public required decimal AverageStandardGamePoints { get; init; }
    public required int StandardGameDataPoints { get; init; }
    public required decimal AveragePickupOnlyStandardGamePoints { get; init; }
    public required int PickupOnlyDataPoints { get; init; }
    public required decimal AverageCounterPickPoints { get; init; }
    public required int CounterPickDataPoints { get; init; }
}
