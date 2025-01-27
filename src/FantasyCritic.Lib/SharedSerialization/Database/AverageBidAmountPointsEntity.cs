using System.Diagnostics.CodeAnalysis;

namespace FantasyCritic.Lib.SharedSerialization.Database;

public class AverageBidAmountPointsEntity
{
    public AverageBidAmountPointsEntity()
    {

    }

    [SetsRequiredMembers]
    public AverageBidAmountPointsEntity(AverageBidAmountPoints domain)
    {
        BidAmount = domain.BidAmount;
        DataPoints = domain.DataPoints;
        AveragePoints = domain.AveragePoints;
    }

    public required uint BidAmount { get; init; }
    public required int DataPoints { get; init; }
    public required decimal AveragePoints { get; init; }

    public AverageBidAmountPoints ToDomain() =>
        new AverageBidAmountPoints(BidAmount, DataPoints, AveragePoints);
}
