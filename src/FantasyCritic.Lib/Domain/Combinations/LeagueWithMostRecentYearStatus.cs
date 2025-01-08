
namespace FantasyCritic.Lib.Domain.Combinations;

public record LeagueWithMostRecentYearStatus(League League, bool UserIsInLeague, bool UserIsActiveInMostRecentYearForLeague,
    bool LeagueIsActiveInActiveYear, bool UserIsFollowingLeague, bool MostRecentYearOneShot);
