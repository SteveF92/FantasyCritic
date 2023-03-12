
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Combinations;
public record CombinedLeagueYearUserStatus(IReadOnlyList<FantasyCriticUserRemovable> UsersWithRemoveStatus,
    IReadOnlyList<LeagueInvite> OutstandingInvites, IReadOnlyList<FantasyCriticUser> ActivePlayersForLeagueYear);
