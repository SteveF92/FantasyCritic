
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Combinations;

public record CombinedLeagueYearUserStatus(IReadOnlyList<FantasyCriticUser> UsersInLeague,
    IReadOnlyList<LeagueInvite> OutstandingInvites, IReadOnlyList<FantasyCriticUser> ActivePlayersForLeagueYear);
