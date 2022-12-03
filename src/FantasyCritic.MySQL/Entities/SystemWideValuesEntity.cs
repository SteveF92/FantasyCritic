using System.Diagnostics.CodeAnalysis;

namespace FantasyCritic.MySQL.Entities;

public class SystemWideValuesEntity
{
    public SystemWideValuesEntity()
    {

    }

    [SetsRequiredMembers]
    public SystemWideValuesEntity(SystemWideValues domain)
    {
        AverageStandardGamePoints = domain.AverageStandardGamePoints;
        AveragePickupOnlyStandardGamePoints = domain.AveragePickupOnlyStandardGamePoints;
        AverageCounterPickPoints = domain.AverageCounterPickPoints;
    }

    public required decimal AverageStandardGamePoints { get; init; }
    public required decimal AveragePickupOnlyStandardGamePoints { get; init; }
    public required decimal AverageCounterPickPoints { get; init; }

    public SystemWideValues ToDomain(IEnumerable<AveragePickPositionPoints> averageStandardGamePointsByPickPosition)
    {
        return new SystemWideValues(AverageStandardGamePoints, AveragePickupOnlyStandardGamePoints, AverageCounterPickPoints, averageStandardGamePointsByPickPosition);
    }
}
