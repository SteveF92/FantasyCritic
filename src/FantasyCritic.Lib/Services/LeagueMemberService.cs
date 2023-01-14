using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using Serilog;

namespace FantasyCritic.Lib.Services;

public class LeagueMemberService
{
    private static readonly ILogger _logger = Log.ForContext<LeagueMemberService>();

    private readonly FantasyCriticUserManager _userManager;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;

    public LeagueMemberService(FantasyCriticUserManager userManager, IFantasyCriticRepo fantasyCriticRepo)
    {
        _userManager = userManager;
        _fantasyCriticRepo = fantasyCriticRepo;
    }

    public async Task<bool> UserIsInLeague(League league, FantasyCriticUser user)
    {
        var playersInLeague = await GetUsersInLeague(league);
        return playersInLeague.Any(x => x.Id == user.Id);
    }

    public async Task<bool> UserIsActiveInLeagueYear(League league, int year, FantasyCriticUser user)
    {
        var activePlayers = await GetActivePlayersForLeagueYear(league, year);
        return activePlayers.Any(x => x.Id == user.Id);
    }

    public async Task<Result> SetPlayerActiveStatus(LeagueYear leagueYear, Dictionary<FantasyCriticUser, bool> userActiveStatus)
    {
        var currentlyActivePlayers = await GetActivePlayersForLeagueYear(leagueYear.League, leagueYear.Year);
        var playersInLeague = await GetUsersInLeague(leagueYear.League);

        Dictionary<FantasyCriticUser, bool> usersToChange = new Dictionary<FantasyCriticUser, bool>();
        foreach (var userToChange in userActiveStatus)
        {
            bool userIsCurrentlyActive = currentlyActivePlayers.Any(x => x.Id == userToChange.Key.Id);
            if (userIsCurrentlyActive == userToChange.Value)
            {
                //Nothing to change
                continue;
            }

            if (leagueYear.League.LeagueManager.Id == userToChange.Key.Id)
            {
                return Result.Failure("Can't change the league manager's active status.");
            }

            bool userIsInLeague = playersInLeague.Any(x => x.Id == userToChange.Key.Id);
            if (!userIsInLeague)
            {
                return Result.Failure("That user is not in that league.");
            }

            usersToChange.Add(userToChange.Key, userToChange.Value);
        }

        if (usersToChange.Any())
        {
            await _fantasyCriticRepo.SetPlayerActiveStatus(leagueYear, usersToChange);
        }

        return Result.Success();
    }

    public Task<IReadOnlyList<FantasyCriticUser>> GetUsersInLeague(League league)
    {
        return _fantasyCriticRepo.GetUsersInLeague(league.LeagueID);
    }

    public async Task<IReadOnlyList<FantasyCriticUserRemovable>> GetUsersWithRemoveStatus(League league)
    {
        var usersInLeague = await _fantasyCriticRepo.GetUsersInLeague(league.LeagueID);

        List<LeagueYear> leagueYears = new List<LeagueYear>();
        List<Publisher> allPublishers = new List<Publisher>();
        foreach (var year in league.Years)
        {
            var leagueYear = await _fantasyCriticRepo.GetLeagueYearOrThrow(league, year);
            leagueYears.Add(leagueYear);
            var publishersForYear = leagueYear.Publishers;
            allPublishers.AddRange(publishersForYear);
        }

        List<FantasyCriticUserRemovable> usersWithStatus = new List<FantasyCriticUserRemovable>();
        foreach (var user in usersInLeague)
        {
            bool userRemovable = !league.LeagueManager.Equals(user);
            foreach (var leagueYear in leagueYears)
            {
                var publishersForYear = allPublishers.Where(x => x.LeagueYearKey.Year == leagueYear.Year);
                if (!publishersForYear.Any(x => x.User.Equals(user)))
                {
                    //User did not play in this year, safe to remove.
                    continue;
                }
                if (leagueYear.PlayStatus.PlayStarted)
                {
                    userRemovable = false;
                }
            }

            usersWithStatus.Add(new FantasyCriticUserRemovable(user, userRemovable));
        }

        return usersWithStatus;
    }

    public async Task<Result> InviteUserByEmail(League league, string inviteEmail)
    {
        var existingInvite = await GetMatchingInvite(league, inviteEmail);
        if (existingInvite is not null)
        {
            return Result.Failure("User is already invited to this league.");
        }

        FantasyCriticUser? inviteUser = await _userManager.FindByEmailAsync(inviteEmail);
        if (inviteUser is not null)
        {
            bool userInLeague = await UserIsInLeague(league, inviteUser);
            if (userInLeague)
            {
                return Result.Failure("User is already in league.");
            }
        }

        LeagueInvite invite = new LeagueInvite(Guid.NewGuid(), league, inviteEmail);

        await _fantasyCriticRepo.SaveInvite(invite);

        return Result.Success();
    }

    public async Task<Result> InviteUserByUserID(League league, FantasyCriticUser inviteUser)
    {
        var existingInvite = await GetMatchingInvite(league, inviteUser);
        if (existingInvite is not null)
        {
            return Result.Failure("User is already invited to this league.");
        }

        bool userInLeague = await UserIsInLeague(league, inviteUser);
        if (userInLeague)
        {
            return Result.Failure("User is already in league.");
        }

        LeagueInvite invite = new LeagueInvite(Guid.NewGuid(), league, inviteUser);

        await _fantasyCriticRepo.SaveInvite(invite);

        return Result.Success();
    }

    public async Task<Result> AcceptInvite(League league, FantasyCriticUser inviteUser)
    {
        bool userInLeague = await UserIsInLeague(league, inviteUser);
        if (userInLeague)
        {
            return Result.Failure("User is already in league.");
        }

        var invite = await GetMatchingInvite(league, inviteUser.Email);
        if (invite is null)
        {
            return Result.Failure("User is not invited to this league.");
        }

        await _fantasyCriticRepo.AcceptInvite(invite, inviteUser);

        return Result.Success();
    }

    public async Task<Result> AcceptInviteLink(LeagueInviteLink inviteLink, FantasyCriticUser inviteUser)
    {
        bool userInLeague = await UserIsInLeague(inviteLink.League, inviteUser);
        if (userInLeague)
        {
            return Result.Failure("User is already in league.");
        }

        await _fantasyCriticRepo.AddPlayerToLeague(inviteLink.League, inviteUser);

        return Result.Success();
    }

    public Task<IReadOnlyList<LeagueInvite>> GetOutstandingInvitees(League league)
    {
        return _fantasyCriticRepo.GetOutstandingInvitees(league);
    }

    public Task<IReadOnlyList<League>> GetLeaguesForUser(FantasyCriticUser user)
    {
        return _fantasyCriticRepo.GetLeaguesForUser(user);
    }

    public Task<IReadOnlyList<LeagueYear>> GetLeaguesYearsForUser(FantasyCriticUser user, int year)
    {
        return _fantasyCriticRepo.GetLeagueYearsForUser(user, year);
    }

    public Task<IReadOnlyList<LeagueInvite>> GetLeagueInvites(FantasyCriticUser user)
    {
        return _fantasyCriticRepo.GetLeagueInvites(user);
    }

    public Task<LeagueInvite?> GetInvite(Guid inviteID)
    {
        return _fantasyCriticRepo.GetInvite(inviteID);
    }

    public Task DeleteInvite(LeagueInvite invite)
    {
        return _fantasyCriticRepo.DeleteInvite(invite);
    }

    public async Task FullyRemovePlayerFromLeague(League league, FantasyCriticUser removeUser)
    {
        foreach (var year in league.Years)
        {
            var leagueYear = await _fantasyCriticRepo.GetLeagueYearOrThrow(league, year);
            var allPublishers = leagueYear.Publishers;
            var deletePublisher = allPublishers.SingleOrDefault(x => x.User.Id == removeUser.Id);
            if (deletePublisher != null)
            {
                _logger.Warning($"Deleting publisher: {deletePublisher.PublisherID} from league: {deletePublisher.LeagueYearKey.LeagueID} " +
                             $"in year: {deletePublisher.LeagueYearKey.Year}");
                await _fantasyCriticRepo.FullyRemovePublisher(leagueYear, deletePublisher);
            }
        }

        await _fantasyCriticRepo.RemovePlayerFromLeague(league, removeUser);
    }

    public Task<IReadOnlyList<FantasyCriticUser>> GetActivePlayersForLeagueYear(League league, int year)
    {
        return _fantasyCriticRepo.GetActivePlayersForLeagueYear(league.LeagueID, year);
    }

    private async Task<LeagueInvite?> GetMatchingInvite(League league, string emailAddress)
    {
        IReadOnlyList<LeagueInvite> playersInvited = await GetOutstandingInvitees(league);
        var invite = playersInvited.GetMatchingInvite(emailAddress);
        return invite;
    }

    private async Task<LeagueInvite?> GetMatchingInvite(League league, FantasyCriticUser user)
    {
        IReadOnlyList<LeagueInvite> playersInvited = await GetOutstandingInvitees(league);
        var invite = playersInvited.GetMatchingInvite(user);
        return invite;
    }

    public Task CreateInviteLink(League league)
    {
        LeagueInviteLink link = new LeagueInviteLink(Guid.NewGuid(), league, Guid.NewGuid(), true);
        return _fantasyCriticRepo.SaveInviteLink(link);
    }

    public async Task<IReadOnlyList<LeagueInviteLink>> GetActiveInviteLinks(League league)
    {
        IReadOnlyList<LeagueInviteLink> inviteLinks = await _fantasyCriticRepo.GetInviteLinks(league);
        return inviteLinks.Where(x => x.Active).ToList();
    }

    public Task DeactivateInviteLink(LeagueInviteLink inviteLink)
    {
        return _fantasyCriticRepo.DeactivateInviteLink(inviteLink);
    }

    public Task<LeagueInviteLink?> GetInviteLinkByInviteCode(Guid inviteCode)
    {
        return _fantasyCriticRepo.GetInviteLinkByInviteCode(inviteCode);
    }

    public Task SetArchiveStatusForUser(League league, bool archive, FantasyCriticUser user)
    {
        return _fantasyCriticRepo.SetArchiveStatusForUser(league, archive, user);
    }

    public async Task<Result> TransferLeagueManager(League league, FantasyCriticUser newManager)
    {
        var usersInLeague = await GetUsersInLeague(league);
        if (!usersInLeague.Contains(newManager))
        {
            return Result.Failure("That player is not in the league.");
        }

        var mostRecentYear = league.Years.Max();
        var activePlayersInYear = await GetActivePlayersForLeagueYear(league, mostRecentYear);
        if (!activePlayersInYear.Contains(newManager))
        {
            return Result.Failure("You can't promote an inactive player to manager.");
        }

        await _fantasyCriticRepo.TransferLeagueManager(league, newManager);
        return Result.Success();
    }

    public Task<IReadOnlyDictionary<FantasyCriticUser, IReadOnlyList<LeagueYearKey>>> GetUsersWithLeagueYearsWithPublisher()
    {
        return _fantasyCriticRepo.GetUsersWithLeagueYearsWithPublisher();
    }
}
