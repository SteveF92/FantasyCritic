
namespace FantasyCritic.Lib.Domain.Combinations;

public record LeagueWithMostRecentYearStatus(League League, bool UserIsInLeague, bool UserIsFollowingLeague, bool MostRecentYearOneShot);
