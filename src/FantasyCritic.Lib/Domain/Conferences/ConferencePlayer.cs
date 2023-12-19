using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Conferences;
public record ConferencePlayer(FantasyCriticUser User, IReadOnlySet<Guid> LeaguesIn, IReadOnlySet<Guid> LeaguesManaging, IReadOnlySet<LeagueYearKey> YearsActiveIn);
