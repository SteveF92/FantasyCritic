namespace FantasyCritic.Lib.Domain.Calculations;
public record PublisherStatistics(Guid PublisherID, LocalDate Date)
{
    public required decimal FantasyPoints { get; init; }
    public required decimal ProjectedPoints { get; init; }
    public required ushort RemainingBudget { get; init; }
    public required byte NumberOfStandardGames { get; init; }
    public required byte NumberOfStandardGamesReleased { get; init; }
    public required byte NumberOfStandardGamesExpectedToRelease { get; init; }
    public required byte NumberOfStandardGamesNotExpectedToRelease { get; init; }
    public required byte NumberOfCounterPicks { get; init; }
    public required byte NumberOfCounterPicksReleased { get; init; }
    public required byte NumberOfCounterPicksExpectedToRelease { get; init; }
    public required byte NumberOfCounterPicksNotExpectedToRelease { get; init; }
}
