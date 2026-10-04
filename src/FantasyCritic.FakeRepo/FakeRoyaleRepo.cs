using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.FakeRepo;

public class FakeRoyaleRepo : IRoyaleRepo
{
    private readonly List<RoyaleYearQuarter> _yearQuarters;
    private readonly VeryMinimalFantasyCriticUser _quarterWinner;

    public FakeRoyaleRepo(IEnumerable<RoyaleYearQuarter> yearQuarters, VeryMinimalFantasyCriticUser quarterWinner)
    {
        _yearQuarters = yearQuarters.ToList();
        _quarterWinner = quarterWinner;
    }

    public Task<IReadOnlyList<RoyaleYearQuarter>> GetYearQuarters()
    {
        return Task.FromResult<IReadOnlyList<RoyaleYearQuarter>>(_yearQuarters.ToList());
    }

    public Task CalculateRoyaleWinnerForQuarter(int year, int quarter)
    {
        var index = _yearQuarters.FindIndex(x => x.YearQuarter.Equals(new YearQuarter(year, quarter)));
        var existing = _yearQuarters[index];
        if (existing.WinningUser is null)
        {
            _yearQuarters[index] = new RoyaleYearQuarter(existing.YearQuarter, existing.OpenForPlay, existing.Finished, _quarterWinner);
        }

        return Task.CompletedTask;
    }

    public Task CreatePublisher(RoyalePublisher publisher)
    {
        throw new NotImplementedException();
    }

    public Task<RoyalePublisher?> GetPublisher(RoyaleYearQuarter yearQuarter, IVeryMinimalFantasyCriticUser user)
    {
        throw new NotImplementedException();
    }

    public Task<RoyaleYearQuarterData?> GetRoyaleYearQuarterData(int year, int quarter)
    {
        throw new NotImplementedException();
    }

    public Task<RoyalePublisherData?> GetPublisherData(Guid publisherID)
    {
        throw new NotImplementedException();
    }

    public Task PurchaseGame(RoyalePublisherGame game, RoyaleAction action)
    {
        throw new NotImplementedException();
    }

    public Task SellGame(RoyalePublisherGame publisherGame, decimal refund, RoyaleAction action)
    {
        throw new NotImplementedException();
    }

    public Task SetAdvertisingMoney(RoyalePublisherGame publisherGame, decimal advertisingMoney, RoyaleAction action)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<RoyalePublisher>> GetAllPublishers(int year, int quarter)
    {
        throw new NotImplementedException();
    }

    public Task UpdateFantasyPoints(Dictionary<(Guid, Guid), decimal?> publisherGameScores, YearQuarter yearQuarter)
    {
        throw new NotImplementedException();
    }

    public Task ChangePublisherName(RoyalePublisher publisher, string publisherName)
    {
        throw new NotImplementedException();
    }

    public Task StartNewQuarter(YearQuarter nextQuarter)
    {
        throw new NotImplementedException();
    }

    public Task FinishQuarter(RoyaleYearQuarter supportedQuarter)
    {
        throw new NotImplementedException();
    }

    public Task ChangePublisherIcon(RoyalePublisher publisher, string? publisherIcon)
    {
        throw new NotImplementedException();
    }

    public Task ChangePublisherSlogan(RoyalePublisher publisher, string? publisherSlogan)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<RoyalePublisherHistoryEntry>> GetPublisherHistoryForUser(Guid userID)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<RoyalePublisherStatistics>> GetPublisherStatistics(Guid publisherID)
    {
        throw new NotImplementedException();
    }

    public Task CreateRoyaleGroup(RoyaleGroup group)
    {
        throw new NotImplementedException();
    }

    public Task<RoyaleGroup?> GetRoyaleGroup(Guid groupID)
    {
        throw new NotImplementedException();
    }

    public Task<RoyaleGroup?> GetRoyaleGroupForLeague(Guid leagueID)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<RoyaleGroup>> GetRoyaleGroupsForUser(Guid userID)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<RoyaleGroup>> SearchRoyaleGroupsByName(string searchTerm)
    {
        throw new NotImplementedException();
    }

    public Task AddMemberToRoyaleGroup(Guid groupID, Guid userID)
    {
        throw new NotImplementedException();
    }

    public Task RemoveMemberFromRoyaleGroup(Guid groupID, Guid userID)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<VeryMinimalFantasyCriticUser>> GetRoyaleGroupMembers(Guid groupID)
    {
        throw new NotImplementedException();
    }

    public Task<RoyaleGroupWithMemberDisplayRows?> GetRoyaleGroupMemberDisplayRows(Guid groupID, int year, int quarter)
    {
        throw new NotImplementedException();
    }

    public Task<RoyaleGroupWithMemberWithLifetimeStats?> GetRoyaleGroupMembersWithLifetimeStats(Guid groupID)
    {
        throw new NotImplementedException();
    }

    public Task SetRoyaleGroupMembers(Guid groupID, IReadOnlyList<Guid> userIDs)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<VeryMinimalFantasyCriticUser>> GetLeagueActivePlayersForMostRecentYear(Guid leagueID)
    {
        throw new NotImplementedException();
    }

    public Task<RoyaleGroup?> GetRoyaleGroupForConference(Guid conferenceID)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<VeryMinimalFantasyCriticUser>> GetConferenceActivePlayersForMostRecentYear(Guid conferenceID)
    {
        throw new NotImplementedException();
    }

    public Task CreateRoyaleGroupInviteLink(RoyaleGroupInviteLink link)
    {
        throw new NotImplementedException();
    }

    public Task<RoyaleGroupInviteLink?> GetRoyaleGroupInviteLinkByID(Guid inviteID)
    {
        throw new NotImplementedException();
    }

    public Task<RoyaleGroupInviteLink?> GetRoyaleGroupInviteLinkByCode(Guid inviteCode)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<RoyaleGroupInviteLink>> GetRoyaleGroupInviteLinks(Guid groupID)
    {
        throw new NotImplementedException();
    }

    public Task DeactivateRoyaleGroupInviteLink(Guid inviteID)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<RoyaleGroup>> GetAllRoyaleGroupsByType(RoyaleGroupType groupType)
    {
        throw new NotImplementedException();
    }
}
