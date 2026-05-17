using System.Diagnostics.CodeAnalysis;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities;

internal class AveragePositionPointsYearEntity
{
    public AveragePositionPointsYearEntity()
    {
    }

    [SetsRequiredMembers]
    public AveragePositionPointsYearEntity(int year, AveragePickPositionPoints domain)
    {
        Year = year;
        PickPosition = domain.PickPosition;
        DataPoints = domain.DataPoints;
        AveragePoints = domain.AveragePoints;
    }

    public required int Year { get; init; }
    public required int PickPosition { get; init; }
    public required int DataPoints { get; init; }
    public required decimal AveragePoints { get; init; }
}
