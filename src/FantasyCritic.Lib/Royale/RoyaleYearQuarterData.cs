namespace FantasyCritic.Lib.Royale;

public record RoyaleYearQuarterData(IReadOnlyList<RoyaleYearQuarter> AllYearQuarters, RoyaleYearQuarter ActiveYearQuarter, IReadOnlyList<RoyalePublisher> RoyalePublishers);
public record RoyalePublisherData(RoyalePublisher RoyalePublisher, IReadOnlyList<RoyaleYearQuarter> QuartersWonByUser, IReadOnlyList<MasterGameYear> MasterGameYears, IReadOnlyList<MasterGameTag> MasterGameTags);
