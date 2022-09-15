using FantasyCritic.Lib.Domain;

namespace FantasyCritic.SharedSerialization;

public class AveragePositionPointsEntity
{
    public AveragePositionPointsEntity()
    {

    }

    public AveragePositionPointsEntity(AveragePickPositionPoints domain)
    {
        PickPosition = domain.PickPosition;
        DataPoints = domain.DataPoints;
        AveragePoints = domain.AveragePoints;
    }

    public int PickPosition { get; set; }
    public int DataPoints { get; set; }
    public decimal AveragePoints { get; set; }

    public AveragePickPositionPoints ToDomain() =>
        new AveragePickPositionPoints(PickPosition, DataPoints, AveragePoints);
}
