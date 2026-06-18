using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Domain.Combinations;

public record BasicData(SystemWideSettings SystemWideSettings, IReadOnlyList<MasterGameTag> MasterGameTags, IReadOnlyList<SupportedYear> SupportedYears,
    RoyaleYearQuarter ActiveRoyaleYearQuarter);
