using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Combinations;

public record LeagueYearSupplementalData(SystemWideValues SystemWideValues, IReadOnlyList<ManagerMessage> ManagerMessages, FantasyCriticUser? PreviousYearWinner,
    IReadOnlyList<Trade> ActiveTrades, IReadOnlyList<SpecialAuction> ActiveSpecialAuctions, PublicBiddingSet? PublicBiddingGames, bool UserIsFollowingLeague,
    IReadOnlyList<MinimalPublisher> AllPublishersForUser, 
    );
