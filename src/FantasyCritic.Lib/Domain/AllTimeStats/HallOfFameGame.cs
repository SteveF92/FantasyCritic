namespace FantasyCritic.Lib.Domain.AllTimeStats;
public record HallOfFameGame(MasterGameYear Game, Publisher PickedBy, IReadOnlyDictionary<string, HallOfFameGameStat> Stats);
public record HallOfFameGameStat(object Stat, string StatType);

public record AllTimeStatsCalculation(MasterGameYear Game, Publisher PickedBy, bool CounterPick, int? OverallDraftPosition, uint? BidAmount, decimal? CriticScore, decimal? FantasyPoints)
{
    public required bool CountedForPointsInYear { get; init; }
    public required double SleeperFactor { get; init; }
    public required double FlopFactor { get; init; }
    public required double DraftValue { get; init; }
    public required double BidValue { get; init; }
    public required double DollarsPerPoint { get; init; }

    public object FormattedCriticScore => FantasyPoints.HasValue && CriticScore.HasValue ? CriticScore.Value : "DNR";
};

public record BidOverspend(Publisher Publisher, MasterGameYear MasterGame, uint WinningBidAmount, uint? NextHighestBid, decimal? CriticScore, decimal? FantasyPoints)
{
    public uint Overspend => WinningBidAmount - (NextHighestBid ?? 0u);
    public object FormattedCriticScore => FantasyPoints.HasValue && CriticScore.HasValue ? CriticScore.Value : "DNR";
}
