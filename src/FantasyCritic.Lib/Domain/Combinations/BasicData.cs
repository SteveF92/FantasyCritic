using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Combinations;
public record BasicData(FantasyCriticUser? CurrentUser, SystemWideSettings SystemWideSettings, IReadOnlyList<MasterGameTag> MasterGameTags, IReadOnlyList<SupportedYear> SupportedYears);
