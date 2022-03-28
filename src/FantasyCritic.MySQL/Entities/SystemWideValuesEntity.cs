namespace FantasyCritic.MySQL.Entities;

public class SystemWideValuesEntity
{
    public SystemWideValuesEntity()
    {

    }

    public SystemWideValuesEntity(SystemWideValues domain)
    {
        AverageStandardGamePoints = domain.AverageStandardGamePoints;
        AveragePickupOnlyStandardGamePoints = domain.AveragePickupOnlyStandardGamePoints;
        AverageCounterPickPoints = domain.AverageCounterPickPoints;
    }

    public decimal AverageStandardGamePoints { get; set; }
    public decimal AveragePickupOnlyStandardGamePoints { get; set; }
    public decimal AverageCounterPickPoints { get; set; }

    public SystemWideValues ToDomain(IEnumerable<AveragePickPositionPoints> averageStandardGamePointsByPickPosition)
    {
        return new SystemWideValues(AverageStandardGamePoints, AveragePickupOnlyStandardGamePoints, AverageCounterPickPoints, averageStandardGamePointsByPickPosition);
    }
}
