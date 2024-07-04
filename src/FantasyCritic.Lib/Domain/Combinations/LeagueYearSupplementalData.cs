using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Trades;

namespace FantasyCritic.Lib.Domain.Combinations;

public record LeagueYearSupplementalData(SystemWideValues SystemWideValues, IReadOnlyList<ManagerMessage> ManagerMessages, Guid? PreviousYearWinnerUserID,
    IReadOnlyList<Trade> ActiveTrades, IReadOnlyList<SpecialAuction> ActiveSpecialAuctions, PublicBiddingSet? PublicBiddingGames, bool UserIsFollowingLeague,
    IReadOnlyList<MinimalPublisher> AllPublishersForUser, PrivatePublisherData? PrivatePublisherData, IReadOnlyDictionary<Guid, MasterGameYear> MasterGameYearDictionary);

public record LeagueYearSupplementalDataFromRepo(SystemWideValues SystemWideValues, IReadOnlyList<ManagerMessage> ManagerMessages, Guid? PreviousYearWinnerUserID,
    IReadOnlyList<Trade> ActiveTrades, IReadOnlyList<SpecialAuction> ActiveSpecialAuctions, IReadOnlyList<PickupBid> ActivePickupBids, bool UserIsFollowingLeague,
    IReadOnlyList<MinimalPublisher> AllPublishersForUser, PrivatePublisherData? PrivatePublisherData, IReadOnlyDictionary<Guid, MasterGameYear> MasterGameYearDictionary);

public record PrivatePublisherData(IReadOnlyList<PickupBid> Bids, IReadOnlyList<DropRequest> DropRequests, IReadOnlyList<QueuedGame> QueuedGames);

public record LeagueYearWithUserStatus(LeagueYear LeagueYear, CombinedLeagueYearUserStatus UserStatus);
public record LeagueYearWithSupplementalData(LeagueYear LeagueYear, LeagueYearSupplementalData SupplementalData, CombinedLeagueYearUserStatus UserStatus);

public record LeagueYearWithSupplementalDataFromRepo(LeagueYear LeagueYear, LeagueYearSupplementalDataFromRepo SupplementalData, CombinedLeagueYearUserStatus UserStatus);
