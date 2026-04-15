using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Services;

public class RoyaleService
{
    private readonly IRoyaleRepo _royaleRepo;
    private readonly IClock _clock;
    private readonly IMasterGameRepo _masterGameRepo;

    public const int FUTURE_RELEASE_LIMIT_DAYS = 5;
    public const int POST_QUARTER_GRACE_DAYS = 7;

    public RoyaleService(IRoyaleRepo royaleRepo, IClock clock, IMasterGameRepo masterGameRepo)
    {
        _royaleRepo = royaleRepo;
        _clock = clock;
        _masterGameRepo = masterGameRepo;
    }

    public Task<IReadOnlyList<RoyaleYearQuarter>> GetYearQuarters()
    {
        return _royaleRepo.GetYearQuarters();
    }

    public async Task<RoyaleYearQuarter> GetActiveYearQuarter()
    {
        IReadOnlyList<RoyaleYearQuarter> supportedQuarters = await GetYearQuarters();
        var activeQuarter = supportedQuarters.Where(x => x.OpenForPlay).WhereMax(x => x.YearQuarter).Single();
        return activeQuarter;
    }

    public async Task<RoyaleYearQuarter?> GetYearQuarter(int year, int quarter)
    {
        IReadOnlyList<RoyaleYearQuarter> supportedQuarters = await GetYearQuarters();
        var requestedQuarter = supportedQuarters.SingleOrDefault(x => x.YearQuarter.Year == year && x.YearQuarter.Quarter == quarter);
        return requestedQuarter;
    }

    public async Task<RoyalePublisher> CreatePublisher(RoyaleYearQuarter yearQuarter, VeryMinimalFantasyCriticUser user, string publisherName)
    {
        RoyalePublisher publisher = new RoyalePublisher(Guid.NewGuid(), yearQuarter, user, publisherName, null, null, new List<RoyalePublisherGame>(), 100m);
        await _royaleRepo.CreatePublisher(publisher);
        return publisher;
    }

    public Task ChangePublisherName(RoyalePublisher publisher, string publisherName)
    {
        return _royaleRepo.ChangePublisherName(publisher, publisherName);
    }

    public Task ChangePublisherIcon(RoyalePublisher publisher, string? publisherIcon)
    {
        return _royaleRepo.ChangePublisherIcon(publisher, publisherIcon);
    }

    public Task ChangePublisherSlogan(RoyalePublisher publisher, string? publisherSlogan)
    {
        return _royaleRepo.ChangePublisherSlogan(publisher, publisherSlogan);
    }

    public Task<RoyalePublisher?> GetPublisher(RoyaleYearQuarter yearQuarter, FantasyCriticUser user)
    {
        return _royaleRepo.GetPublisher(yearQuarter, user);
    }

    public Task<RoyalePublisherData?> GetPublisherData(Guid publisherID)
    {
        return _royaleRepo.GetPublisherData(publisherID);
    }

    public async Task<RoyalePublisher?> GetPublisher(Guid publisherID)
    {
        var publisherData = await _royaleRepo.GetPublisherData(publisherID);
        return publisherData?.RoyalePublisher;
    }

    public Task<IReadOnlyList<RoyalePublisher>> GetAllPublishers(int year, int quarter)
    {
        return _royaleRepo.GetAllPublishers(year, quarter);
    }

    public async Task<IReadOnlyList<RoyalePublisher>> GetAllPublishers(int year)
    {
        var quarters = await GetYearQuarters();
        var quartersInYear = quarters.Where(x => x.YearQuarter.Year == year);

        List<RoyalePublisher> allPublishers = new List<RoyalePublisher>();
        foreach (var quarter in quartersInYear)
        {
            var publishers = await GetAllPublishers(year, quarter.YearQuarter.Quarter);
            allPublishers.AddRange(publishers);
        }

        return allPublishers;
    }

    public Task<RoyaleYearQuarterData?> GetRoyaleYearQuarterData(int year, int quarter)
    {
        return _royaleRepo.GetRoyaleYearQuarterData(year, quarter);
    }

    public async Task<ClaimResult> PurchaseGame(RoyalePublisher publisher, MasterGameYear masterGame)
    {
        if (publisher.PublisherGames.Count >= GetMaxGames(publisher.YearQuarter))
        {
            return new ClaimResult("Roster is full.");
        }
        if (publisher.PublisherGames.Select(x => x.MasterGame).Contains(masterGame))
        {
            return new ClaimResult("Publisher already has that game.");
        }
        if (!masterGame.CouldReleaseInQuarter(publisher.YearQuarter.YearQuarter))
        {
            return new ClaimResult("Game will not release this quarter.");
        }

        var now = _clock.GetCurrentInstant();
        var currentDate = now.ToEasternDate();
        if (masterGame.MasterGame.IsReleased(currentDate))
        {
            return new ClaimResult("Game has been released.");
        }

        var fiveDaysFuture = currentDate.PlusDays(FUTURE_RELEASE_LIMIT_DAYS);
        if (masterGame.MasterGame.IsReleased(fiveDaysFuture))
        {
            return new ClaimResult($"Game will release within {FUTURE_RELEASE_LIMIT_DAYS} days.");
        }

        if (masterGame.MasterGame.CriticScore.HasValue)
        {
            return new ClaimResult("Game has a score.");
        }

        if (masterGame.MasterGame.HasAnyReviews)
        {
            return new ClaimResult("That game already has reviews.");
        }

        var masterGameTags = await _masterGameRepo.GetMasterGameTags();
        var eligibilityErrors = LeagueTagExtensions.GetRoyaleClaimErrors(masterGameTags, masterGame.MasterGame, currentDate, publisher.YearQuarter);
        if (eligibilityErrors.Any())
        {
            return new ClaimResult("Game is not eligible under Royale rules.");
        }

        var currentBudget = publisher.Budget;
        var gameCost = masterGame.GetRoyaleGameCost();
        if (currentBudget < gameCost)
        {
            return new ClaimResult("Not enough budget.");
        }

        RoyalePublisherGame game = new RoyalePublisherGame(publisher.PublisherID, publisher.YearQuarter, masterGame, now, gameCost, 0m, null);
        RoyaleAction action = new RoyaleAction(publisher, masterGame, "Purchased Game", $"Purchased '{masterGame.MasterGame.GameName}' at a cost of ${gameCost:F2}.", now);
        await _royaleRepo.PurchaseGame(game, action);
        var nextSlot = publisher.PublisherGames.Count;
        return new ClaimResult(nextSlot);
    }

    public async Task<Result> SellGame(RoyalePublisher publisher, RoyalePublisherGame publisherGame)
    {
        var masterGameTags = await _masterGameRepo.GetMasterGameTags();
        var currentlyInEligible = publisherGame.CalculateIsCurrentlyIneligible(masterGameTags);
        if (!currentlyInEligible)
        {
            var currentDate = _clock.GetToday();
            if (publisherGame.MasterGame.MasterGame.IsReleased(currentDate))
            {
                return Result.Failure("That game has already been released.");
            }

            var fiveDaysFuture = currentDate.PlusDays(FUTURE_RELEASE_LIMIT_DAYS);
            if (publisherGame.MasterGame.MasterGame.IsReleased(fiveDaysFuture))
            {
                return Result.Failure($"Game will release within {FUTURE_RELEASE_LIMIT_DAYS} days.");
            }

            if (publisherGame.MasterGame.MasterGame.CriticScore.HasValue)
            {
                return Result.Failure("That game already has a score.");
            }

            if (!publisher.PublisherGames.Contains(publisherGame))
            {
                return Result.Failure("You don't have that game.");
            }
        }

        var marketCost = publisherGame.MasterGame.GetRoyaleGameCost();
        var finalRefund = publisherGame.CalculateRefundAmount(masterGameTags);

        var now = _clock.GetCurrentInstant();
        RoyaleAction action = new RoyaleAction(publisher, publisherGame.MasterGame,
            "Sold Game", $"Sold '{publisherGame.MasterGame.MasterGame.GameName}' for ${finalRefund:F2} (Market Cost: ${marketCost:F2}).", now);
        await _royaleRepo.SellGame(publisherGame, finalRefund, action);
        return Result.Success();
    }

    public async Task<Result> SetAdvertisingMoney(RoyalePublisher publisher, RoyalePublisherGame publisherGame, decimal advertisingMoney)
    {
        var currentDate = _clock.GetToday();
        if (publisherGame.MasterGame.MasterGame.IsReleased(currentDate))
        {
            return Result.Failure("That game has already been released.");
        }

        var fiveDaysFuture = currentDate.PlusDays(FUTURE_RELEASE_LIMIT_DAYS);
        if (publisherGame.MasterGame.MasterGame.IsReleased(fiveDaysFuture))
        {
            return Result.Failure($"Game will release within {FUTURE_RELEASE_LIMIT_DAYS} days.");
        }

        if (publisherGame.MasterGame.MasterGame.CriticScore.HasValue)
        {
            return Result.Failure("That game already has a score.");
        }

        if (!publisher.PublisherGames.Contains(publisherGame))
        {
            return Result.Failure("You don't have that game.");
        }

        decimal newDollarsToSpend = advertisingMoney - publisherGame.AdvertisingMoney;
        if (publisher.Budget < newDollarsToSpend)
        {
            return Result.Failure("You don't have enough money.");
        }

        if (advertisingMoney < 0m)
        {
            return Result.Failure("You can't allocate negative dollars in advertising money.");
        }

        if (advertisingMoney > 10m)
        {
            return Result.Failure("You can't allocate more than 10 dollars in advertising money.");
        }

        var now = _clock.GetCurrentInstant();
        RoyaleAction action = new RoyaleAction(publisher, publisherGame.MasterGame,
            "Changed Advertising Budget", $"Assigned ${advertisingMoney:F2} of advertising budget to '{publisherGame.MasterGame.MasterGame.GameName}' (Was ${publisherGame.AdvertisingMoney:F2}).", now);
        await _royaleRepo.SetAdvertisingMoney(publisherGame, advertisingMoney, action);
        return Result.Success();
    }

    public async Task UpdateFantasyPoints(YearQuarter yearQuarter)
    {
        Dictionary<(Guid, Guid), decimal?> publisherGameScores = new Dictionary<(Guid, Guid), decimal?>();
        var allPublishersForQuarter = await _royaleRepo.GetAllPublishers(yearQuarter.Year, yearQuarter.Quarter);

        var allMasterGameTags = await _masterGameRepo.GetMasterGameTags();
        var currentDate = _clock.GetToday();
        foreach (var publisher in allPublishersForQuarter)
        {
            foreach (var publisherGame in publisher.PublisherGames)
            {
                decimal? fantasyPoints = publisherGame.CalculateFantasyPoints(currentDate, allMasterGameTags);
                publisherGameScores.Add((publisherGame.PublisherID, publisherGame.MasterGame.MasterGame.MasterGameID), fantasyPoints);
            }
        }

        await _royaleRepo.UpdateFantasyPoints(publisherGameScores);
    }

    public async Task<IReadOnlyList<RoyaleYearQuarter>> GetQuartersWonByUser(IVeryMinimalFantasyCriticUser user)
    {
        var quarters = await _royaleRepo.GetYearQuarters();
        return quarters.Where(x => x.WinningUser is not null && x.WinningUser.UserID == user.UserID).ToList();
    }

    public Task StartNewQuarter(YearQuarter nextQuarter)
    {
        return _royaleRepo.StartNewQuarter(nextQuarter);
    }

    public Task FinishQuarter(RoyaleYearQuarter supportedQuarter)
    {
        return _royaleRepo.FinishQuarter(supportedQuarter);
    }

    public Task CalculateRoyaleWinnerForQuarter(RoyaleYearQuarter supportedQuarter)
    {
        return _royaleRepo.CalculateRoyaleWinnerForQuarter(supportedQuarter.YearQuarter.Year, supportedQuarter.YearQuarter.Quarter);
    }

    public Task<IReadOnlyList<RoyalePublisherHistoryEntry>> GetPublisherHistoryForUser(Guid userID)
    {
        return _royaleRepo.GetPublisherHistoryForUser(userID);
    }

    public int GetMaxGames(RoyaleYearQuarter yearQuarter)
    {
        if (!SupportedYear.Year2026FeatureSupported(yearQuarter.YearQuarter.Year))
        {
            return 25;
        }

        return 15;
    }

    public async Task<RoyaleGroup> CreateManualRoyaleGroup(VeryMinimalFantasyCriticUser manager, string groupName)
    {
        var now = _clock.GetCurrentInstant();
        var group = new RoyaleGroup(Guid.NewGuid(), groupName, manager, RoyaleGroupType.Manual, null, null, null, now);
        await _royaleRepo.CreateRoyaleGroup(group);
        await _royaleRepo.AddMemberToRoyaleGroup(group.GroupID, manager.UserID);
        return group;
    }

    public async Task<Result<RoyaleGroup>> CreateLeagueTiedRoyaleGroup(VeryMinimalFantasyCriticUser manager, string groupName, Guid leagueID)
    {
        var existing = await _royaleRepo.GetRoyaleGroupForLeague(leagueID);
        if (existing is not null)
        {
            return Result.Failure<RoyaleGroup>("This league already has a Royale group.");
        }

        var now = _clock.GetCurrentInstant();
        var group = new RoyaleGroup(Guid.NewGuid(), groupName, manager, RoyaleGroupType.LeagueTied, leagueID, null, null, now);
        await _royaleRepo.CreateRoyaleGroup(group);
        return group;
    }

    public async Task<Result<RoyaleGroup>> CreateConferenceTiedRoyaleGroup(VeryMinimalFantasyCriticUser manager, string groupName, Guid conferenceID)
    {
        var existing = await _royaleRepo.GetRoyaleGroupForConference(conferenceID);
        if (existing is not null)
        {
            return Result.Failure<RoyaleGroup>("This conference already has a Royale group.");
        }

        var now = _clock.GetCurrentInstant();
        var group = new RoyaleGroup(Guid.NewGuid(), groupName, manager, RoyaleGroupType.ConferenceTied, null, conferenceID, null, now);
        await _royaleRepo.CreateRoyaleGroup(group);
        return group;
    }

    public Task<RoyaleGroup?> GetRoyaleGroup(Guid groupID) => _royaleRepo.GetRoyaleGroup(groupID);

    public Task<RoyaleGroup?> GetRoyaleGroupForLeague(Guid leagueID) => _royaleRepo.GetRoyaleGroupForLeague(leagueID);

    public Task<RoyaleGroup?> GetRoyaleGroupForConference(Guid conferenceID) => _royaleRepo.GetRoyaleGroupForConference(conferenceID);

    public Task<IReadOnlyList<RoyaleGroup>> GetRoyaleGroupsForUser(Guid userID) => _royaleRepo.GetRoyaleGroupsForUser(userID);

    public Task<IReadOnlyList<RoyaleGroup>> SearchRoyaleGroups(string searchTerm) => _royaleRepo.SearchRoyaleGroupsByName(searchTerm);

    public async Task<IReadOnlyList<VeryMinimalFantasyCriticUser>> GetRoyaleGroupMembers(RoyaleGroup group)
    {
        if (group.GroupType == RoyaleGroupType.LeagueTied && group.LeagueID.HasValue)
        {
            return await _royaleRepo.GetLeagueActivePlayersForMostRecentYear(group.LeagueID.Value);
        }

        if (group.GroupType == RoyaleGroupType.ConferenceTied && group.ConferenceID.HasValue)
        {
            return await _royaleRepo.GetConferenceActivePlayersForMostRecentYear(group.ConferenceID.Value);
        }

        return await _royaleRepo.GetRoyaleGroupMembers(group.GroupID);
    }

    public async Task<IReadOnlyList<RoyaleGroupMemberDisplayRow>> GetRoyaleGroupMemberDisplayRows(RoyaleGroup group, int year, int quarter)
    {
        var members = await GetRoyaleGroupMembers(group);
        var yearQuarter = await GetYearQuarter(year, quarter);
        if (yearQuarter is null)
        {
            return new List<RoyaleGroupMemberDisplayRow>();
        }

        RoyaleYearQuarter? previousYearQuarter = null;
        if (!yearQuarter.Finished)
        {
            previousYearQuarter = await GetYearQuarter(yearQuarter.YearQuarter.LastQuarter.Year, yearQuarter.YearQuarter.LastQuarter.Quarter);
        }

        List<RoyaleGroupMemberDisplayRow> rows = new();
        foreach (var member in members)
        {
            var publisher = await _royaleRepo.GetPublisher(yearQuarter, member);

            if (yearQuarter.Finished)
            {
                if (publisher is null)
                {
                    continue;
                }
            }
            else if (previousYearQuarter is not null)
            {
                if (publisher is null)
                {
                    var previousPublisher = await _royaleRepo.GetPublisher(previousYearQuarter, member);
                    if (previousPublisher is null)
                    {
                        continue;
                    }
                }
            }

            rows.Add(new RoyaleGroupMemberDisplayRow(member, publisher));
        }

        return rows;
    }

    public async Task<IReadOnlyList<RoyaleGroupMemberWithLifetimeStats>> GetRoyaleGroupMembersWithLifetimeStats(RoyaleGroup group)
    {
        var members = await GetRoyaleGroupMembers(group);
        if (members.Count == 0)
        {
            return Array.Empty<RoyaleGroupMemberWithLifetimeStats>();
        }

        var quartersParticipated = members.ToDictionary(m => m.UserID, _ => 0);
        var totalPoints = members.ToDictionary(m => m.UserID, _ => 0m);
        var ranksForAverage = members.ToDictionary(m => m.UserID, _ => new List<int>());

        var allQuarters = await GetYearQuarters();
        foreach (var yearQuarter in allQuarters)
        {
            var rows = await GetRoyaleGroupMemberDisplayRows(group, yearQuarter.YearQuarter.Year, yearQuarter.YearQuarter.Quarter);
            var orderedRows = rows.OrderByDescending(r => r.Publisher?.GetTotalFantasyPoints() ?? -1m).ToList();
            var ranking = 1;
            foreach (var row in orderedRows)
            {
                int? rank = null;
                if (row.Publisher is not null && row.Publisher.GetTotalFantasyPoints() > 0)
                {
                    rank = ranking;
                    ranking++;
                }

                if (row.Publisher is null || !quartersParticipated.ContainsKey(row.User.UserID))
                {
                    continue;
                }

                quartersParticipated[row.User.UserID]++;
                totalPoints[row.User.UserID] += row.Publisher.GetTotalFantasyPoints();
                if (rank.HasValue)
                {
                    ranksForAverage[row.User.UserID].Add(rank.Value);
                }
            }
        }

        return members.Select(m =>
        {
            var rankList = ranksForAverage[m.UserID];
            double? averageRank = rankList.Count > 0 ? rankList.Average() : null;
            return new RoyaleGroupMemberWithLifetimeStats(m, quartersParticipated[m.UserID], totalPoints[m.UserID], averageRank);
        }).ToList();
    }

    public async Task<Result> JoinRoyaleGroupViaInviteLink(Guid inviteCode, FantasyCriticUser user)
    {
        var link = await _royaleRepo.GetRoyaleGroupInviteLinkByCode(inviteCode);
        if (link is null)
        {
            return Result.Failure("Invalid invite link.");
        }

        if (link.Group.GroupType != RoyaleGroupType.Manual)
        {
            return Result.Failure("This group does not support invite links.");
        }

        var existingMembers = await _royaleRepo.GetRoyaleGroupMembers(link.Group.GroupID);
        if (existingMembers.Any(m => m.UserID == user.Id))
        {
            return Result.Failure("You are already a member of this group.");
        }

        await _royaleRepo.AddMemberToRoyaleGroup(link.Group.GroupID, user.Id);
        return Result.Success();
    }

    public async Task<Result> LeaveRoyaleGroup(Guid groupID, FantasyCriticUser user)
    {
        var group = await _royaleRepo.GetRoyaleGroup(groupID);
        if (group is null)
        {
            return Result.Failure("Group not found.");
        }

        if (group.GroupType != RoyaleGroupType.Manual)
        {
            return Result.Failure("You can only leave manual groups.");
        }

        if (group.Manager is not null && group.Manager.UserID == user.Id)
        {
            return Result.Failure("The group manager cannot leave the group.");
        }

        await _royaleRepo.RemoveMemberFromRoyaleGroup(groupID, user.Id);
        return Result.Success();
    }

    public async Task<Result> RemoveMemberFromRoyaleGroup(Guid groupID, Guid userIDToRemove, FantasyCriticUser manager)
    {
        var group = await _royaleRepo.GetRoyaleGroup(groupID);
        if (group is null)
        {
            return Result.Failure("Group not found.");
        }

        if (group.Manager?.UserID != manager.Id)
        {
            return Result.Failure("Only the group manager can remove members.");
        }

        if (group.GroupType != RoyaleGroupType.Manual)
        {
            return Result.Failure("Members can only be removed from manual groups.");
        }

        if (userIDToRemove == manager.Id)
        {
            return Result.Failure("The manager cannot be removed from the group.");
        }

        await _royaleRepo.RemoveMemberFromRoyaleGroup(groupID, userIDToRemove);
        return Result.Success();
    }

    public async Task<Result<RoyaleGroupInviteLink>> CreateGroupInviteLink(Guid groupID, FantasyCriticUser manager)
    {
        var group = await _royaleRepo.GetRoyaleGroup(groupID);
        if (group is null)
        {
            return Result.Failure<RoyaleGroupInviteLink>("Group not found.");
        }

        if (group.Manager?.UserID != manager.Id)
        {
            return Result.Failure<RoyaleGroupInviteLink>("Only the group manager can create invite links.");
        }

        if (group.GroupType != RoyaleGroupType.Manual)
        {
            return Result.Failure<RoyaleGroupInviteLink>("Invite links are only for manual groups.");
        }

        var existingLinks = await _royaleRepo.GetRoyaleGroupInviteLinks(groupID);
        if (existingLinks.Count(x => x.Active) >= 2)
        {
            return Result.Failure<RoyaleGroupInviteLink>("You can only have 2 active invite links at a time.");
        }

        var link = new RoyaleGroupInviteLink(Guid.NewGuid(), group, Guid.NewGuid(), true);
        await _royaleRepo.CreateRoyaleGroupInviteLink(link);
        return link;
    }

    public async Task<Result> DeactivateGroupInviteLink(Guid inviteID, FantasyCriticUser manager)
    {
        var link = await _royaleRepo.GetRoyaleGroupInviteLinkByID(inviteID);
        if (link is null)
        {
            return Result.Failure("Invite link not found.");
        }

        if (link.Group.Manager?.UserID != manager.Id)
        {
            return Result.Failure("Only the group manager can deactivate invite links.");
        }

        await _royaleRepo.DeactivateRoyaleGroupInviteLink(inviteID);
        return Result.Success();
    }

    public Task<IReadOnlyList<RoyaleGroupInviteLink>> GetGroupInviteLinks(Guid groupID) => _royaleRepo.GetRoyaleGroupInviteLinks(groupID);

    public Task<IReadOnlyList<RoyaleGroup>> GetAllRoyaleGroupsByType(RoyaleGroupType groupType) => _royaleRepo.GetAllRoyaleGroupsByType(groupType);

    public Task SetRoyaleGroupMembers(Guid groupID, IReadOnlyList<Guid> userIDs) => _royaleRepo.SetRoyaleGroupMembers(groupID, userIDs);
}
