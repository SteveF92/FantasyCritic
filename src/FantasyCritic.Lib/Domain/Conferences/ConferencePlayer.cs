using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Conferences;
public record ConferencePlayer(MinimalFantasyCriticUser User, IReadOnlySet<Guid> LeaguesIn, IReadOnlySet<Guid> LeaguesManaging, IReadOnlySet<int> YearsActiveIn, IReadOnlySet<LeagueYearKey> LeagueYearsActiveIn);
