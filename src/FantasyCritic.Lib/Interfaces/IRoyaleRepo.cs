using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Interfaces;

public interface IRoyaleRepo
{
    Task CreatePublisher(RoyalePublisher publisher);
    Task<RoyalePublisher?> GetPublisher(RoyaleYearQuarter yearQuarter, IVeryMinimalFantasyCriticUser user);
    Task<IReadOnlyList<RoyaleYearQuarter>> GetYearQuarters();
    Task<RoyaleYearQuarterData?> GetRoyaleYearQuarterData(int year, int quarter);
    Task<RoyalePublisherData?> GetPublisherData(Guid publisherID);
    Task PurchaseGame(RoyalePublisherGame game, RoyaleAction action);
    Task SellGame(RoyalePublisherGame publisherGame, decimal refund, RoyaleAction action);
    Task SetAdvertisingMoney(RoyalePublisherGame publisherGame, decimal advertisingMoney, RoyaleAction action);
    Task<IReadOnlyList<RoyalePublisher>> GetAllPublishers(int year, int quarter);
    Task UpdateFantasyPoints(Dictionary<(Guid, Guid), decimal?> publisherGameScores);
    Task ChangePublisherName(RoyalePublisher publisher, string publisherName);
    Task CalculateRoyaleWinnerForQuarter(int year, int quarter);
    Task StartNewQuarter(YearQuarter nextQuarter);
    Task FinishQuarter(RoyaleYearQuarter supportedQuarter);
    Task ChangePublisherIcon(RoyalePublisher publisher, string? publisherIcon);
    Task ChangePublisherSlogan(RoyalePublisher publisher, string? publisherSlogan);
    Task<IReadOnlyList<RoyalePublisherHistoryEntry>> GetPublisherHistoryForUser(Guid userID);
    Task<IReadOnlyList<RoyalePublisherStatistics>> GetPublisherStatistics(Guid publisherID);

    Task CreateRoyaleGroup(RoyaleGroup group);
    Task<RoyaleGroup?> GetRoyaleGroup(Guid groupID);
    Task<RoyaleGroup?> GetRoyaleGroupForLeague(Guid leagueID);
    Task<IReadOnlyList<RoyaleGroup>> GetRoyaleGroupsForUser(Guid userID);
    Task<IReadOnlyList<RoyaleGroup>> SearchRoyaleGroupsByName(string searchTerm);

    Task AddMemberToRoyaleGroup(Guid groupID, Guid userID);
    Task RemoveMemberFromRoyaleGroup(Guid groupID, Guid userID);
    Task<IReadOnlyList<VeryMinimalFantasyCriticUser>> GetRoyaleGroupMembers(Guid groupID);
    Task<RoyaleGroupWithMemberDisplayRows?> GetRoyaleGroupMemberDisplayRows(Guid groupID, int year, int quarter);
    Task<RoyaleGroupWithMemberWithLifetimeStats?> GetRoyaleGroupMembersWithLifetimeStats(Guid groupID);
    Task SetRoyaleGroupMembers(Guid groupID, IReadOnlyList<Guid> userIDs);

    Task<IReadOnlyList<VeryMinimalFantasyCriticUser>> GetLeagueActivePlayersForMostRecentYear(Guid leagueID);

    Task<RoyaleGroup?> GetRoyaleGroupForConference(Guid conferenceID);
    Task<IReadOnlyList<VeryMinimalFantasyCriticUser>> GetConferenceActivePlayersForMostRecentYear(Guid conferenceID);

    Task CreateRoyaleGroupInviteLink(RoyaleGroupInviteLink link);
    Task<RoyaleGroupInviteLink?> GetRoyaleGroupInviteLinkByID(Guid inviteID);
    Task<RoyaleGroupInviteLink?> GetRoyaleGroupInviteLinkByCode(Guid inviteCode);
    Task<IReadOnlyList<RoyaleGroupInviteLink>> GetRoyaleGroupInviteLinks(Guid groupID);
    Task DeactivateRoyaleGroupInviteLink(Guid inviteID);

    Task<IReadOnlyList<RoyaleGroup>> GetAllRoyaleGroupsByType(RoyaleGroupType groupType);
}
