using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Royale;

public record RoyaleYearQuarterData(RoyaleYearQuarter YearQuarter, IReadOnlyList<RoyalePublisher> RoyalePublishers,
    IReadOnlyDictionary<FantasyCriticUser, IReadOnlyList<RoyaleYearQuarter>> PreviousWinners, IReadOnlyList<MasterGameTag> MasterGameTags);
