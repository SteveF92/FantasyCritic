namespace FantasyCritic.Lib.Domain;
public record AveragePickPositionPoints(int PickPosition, int DataPoints, decimal AveragePoints);
public record AverageBidAmountPoints(uint BidAmount, int DataPoints, decimal AveragePoints);
