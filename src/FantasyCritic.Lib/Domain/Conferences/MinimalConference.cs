using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Conferences;
public record MinimalConference(Guid ConferenceID, string ConferenceName, IReadOnlyList<int> Years, bool CustomRulesConference, VeryMinimalFantasyCriticUser ConferenceManager);
public record VeryMinimalFantasyCriticUser(Guid UserID, string DisplayName) : IVeryMinimalFantasyCriticUser;
