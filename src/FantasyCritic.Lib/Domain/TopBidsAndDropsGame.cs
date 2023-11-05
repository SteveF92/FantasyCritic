namespace FantasyCritic.Lib.Domain;

public record TopBidsAndDropsGame
{
    public required LocalDate ProcessDate { get; init; }
    public required MasterGameYear MasterGameYear { get; init; }
    public required int TotalStandardBidCount { get; init; }
    public required int SuccessfulStandardBids { get; init; }
    public required int FailedStandardBids { get; init; }
    public required int TotalStandardBidLeagues { get; init; }
    public required int TotalStandardBidAmount { get; init; }
    public required int TotalCounterPickBidCount { get; init; }
    public required int SuccessfulCounterPickBids { get; init; }
    public required int FailedCounterPickBids { get; init; }
    public required int TotalCounterPickBidLeagues { get; init; }
    public required int TotalCounterPickBidAmount { get; init; }
    public required int TotalDropCount { get; init; }
    public required int SuccessfulDrops { get; init; }
    public required int FailedDrops { get; init; }
}

