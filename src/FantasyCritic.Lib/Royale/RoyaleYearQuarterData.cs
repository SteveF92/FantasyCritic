namespace FantasyCritic.Lib.Royale;

public record RoyaleYearQuarterData(IReadOnlyList<RoyaleYearQuarter> AllYearQuarters, RoyaleYearQuarter ActiveYearQuarter, IReadOnlyList<RoyalePublisher> RoyalePublishers);
public record RoyalePublisherData(RoyalePublisher RoyalePublisher, IReadOnlyList<RoyaleAction> RoyaleActions, IReadOnlyList<RoyaleYearQuarter> QuartersWonByUser, IReadOnlyList<MasterGameYear> MasterGameYears, IReadOnlyList<MasterGameTag> MasterGameTags);
public record RoyalePublisherHistoryEntry(Guid PublisherID, int Year, int Quarter, string PublisherName, string? PublisherIcon, decimal TotalFantasyPoints, int? Ranking);
