using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Combinations;

public record LeagueYearSupplementalData(SystemWideValues SystemWideValues, IReadOnlyList<ManagerMessage> ManagerMessages, Guid? PreviousYearWinnerUserID,
    IReadOnlyList<Trade> ActiveTrades, IReadOnlyList<SpecialAuction> ActiveSpecialAuctions, PublicBiddingSet? PublicBiddingGames, bool UserIsFollowingLeague,
    IReadOnlyList<MinimalPublisher> AllPublishersForUser, PrivatePublisherData? PrivatePublisherData);

public record PrivatePublisherData(IReadOnlyList<PickupBid> Bids, IReadOnlyList<DropRequest> DropRequests, IReadOnlyList<QueuedGame> QueuedGames,
    IReadOnlyDictionary<Guid, MasterGameYear> MasterGameYearDictionary);
