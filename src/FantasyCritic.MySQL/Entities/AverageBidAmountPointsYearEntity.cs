using System.Diagnostics.CodeAnalysis;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities;

internal class AverageBidAmountPointsYearEntity
{
    public AverageBidAmountPointsYearEntity()
    {
    }

    [SetsRequiredMembers]
    public AverageBidAmountPointsYearEntity(int year, AverageBidAmountPoints domain)
    {
        Year = year;
        BidAmount = domain.BidAmount;
        DataPoints = domain.DataPoints;
        AveragePoints = domain.AveragePoints;
    }

    public required int Year { get; init; }
    public required uint BidAmount { get; init; }
    public required int DataPoints { get; init; }
    public required decimal AveragePoints { get; init; }
}
