using System.Diagnostics.CodeAnalysis;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.SharedSerialization.Database;

public class AveragePositionPointsEntity
{
    public AveragePositionPointsEntity()
    {

    }

    [SetsRequiredMembers]
    public AveragePositionPointsEntity(AveragePickPositionPoints domain)
    {
        PickPosition = domain.PickPosition;
        DataPoints = domain.DataPoints;
        AveragePoints = domain.AveragePoints;
    }

    public required int PickPosition { get; init; }
    public required int DataPoints { get; init; }
    public required decimal AveragePoints { get; init; }

    public AveragePickPositionPoints ToDomain() =>
        new AveragePickPositionPoints(PickPosition, DataPoints, AveragePoints);
}
