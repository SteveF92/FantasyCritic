namespace FantasyCritic.Lib.Royale;

public record RoyaleYearQuarterData(IReadOnlyList<RoyaleYearQuarter> AllYearQuarters, RoyaleYearQuarter ActiveYearQuarter, IReadOnlyList<RoyalePublisher> RoyalePublishers, IReadOnlyList<MasterGameTag> MasterGameTags);
public record RoyalePublisherData(RoyalePublisher RoyalePublisher, IReadOnlyList<RoyaleYearQuarter> QuartersWonByUser, IReadOnlyList<MasterGameTag> MasterGameTags);
