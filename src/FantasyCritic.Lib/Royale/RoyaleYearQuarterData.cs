using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Royale;

public record RoyaleYearQuarterData(RoyaleYearQuarter YearQuarter, IReadOnlyList<RoyalePublisher> RoyalePublishers,
    IReadOnlyDictionary<VeryMinimalFantasyCriticUser, IReadOnlyList<RoyaleYearQuarter>> PreviousWinners, IReadOnlyList<MasterGameTag> MasterGameTags);
