using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using NLog;

namespace FantasyCritic.Lib.Services;

public class LeagueMemberService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly FantasyCriticUserManager _userManager;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IClock _clock;

    public LeagueMemberService(FantasyCriticUserManager userManager, IFantasyCriticRepo fantasyCriticRepo, IClock clock)
    {
        _userManager = userManager;
        _fantasyCriticRepo = fantasyCriticRepo;
        _clock = clock;
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
        return _fantasyCriticRepo.GetUsersInLeague(league);
    }

    public async Task<IReadOnlyList<FantasyCriticUserRemovable>> GetUsersWithRemoveStatus(League league)
    {
        var usersInLeague = await _fantasyCriticRepo.GetUsersInLeague(league);

        List<LeagueYear> leagueYears = new List<LeagueYear>();
        List<Publisher> allPublishers = new List<Publisher>();
        foreach (var year in league.Years)
        {
            var leagueYear = await _fantasyCriticRepo.GetLeagueYear(league, year);
            leagueYears.Add(leagueYear.Value);
            var publishersForYear = await _fantasyCriticRepo.GetPublishersInLeagueForYear(leagueYear.Value);
            allPublishers.AddRange(publishersForYear);
        }

        List<FantasyCriticUserRemovable> usersWithStatus = new List<FantasyCriticUserRemovable>();
        foreach (var user in usersInLeague)
        {
            bool userRemovable = !league.LeagueManager.Equals(user);
            foreach (var leagueYear in leagueYears)
            {
                var publishersForYear = allPublishers.Where(x => x.LeagueYear.Year == leagueYear.Year);
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
        if (existingInvite.HasValue)
        {
            return Result.Failure("User is already invited to this league.");
        }

        FantasyCriticUser inviteUser = await _userManager.FindByEmailAsync(inviteEmail);
        if (inviteUser != null)
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
        if (existingInvite.HasValue)
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
        if (invite.HasNoValue)
        {
            return Result.Failure("User is not invited to this league.");
        }

        await _fantasyCriticRepo.AcceptInvite(invite.Value, inviteUser);

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

    public Task<Maybe<LeagueInvite>> GetInvite(Guid inviteID)
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
            var leagueYear = await _fantasyCriticRepo.GetLeagueYear(league, year);
            var allPublishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(leagueYear.Value);
            var deletePublisher = allPublishers.SingleOrDefault(x => x.User.Id == removeUser.Id);
            if (deletePublisher != null)
            {
                _logger.Warn($"Deleting publisher: {deletePublisher.PublisherID} from league: {deletePublisher.LeagueYear.League.LeagueID} " +
                             $"in year: {deletePublisher.LeagueYear.Year}");
                await _fantasyCriticRepo.FullyRemovePublisher(deletePublisher, allPublishers);
            }
        }

        await _fantasyCriticRepo.RemovePlayerFromLeague(league, removeUser);
    }

    public Task<IReadOnlyList<FantasyCriticUser>> GetActivePlayersForLeagueYear(League league, int year)
    {
        return _fantasyCriticRepo.GetActivePlayersForLeagueYear(league, year);
    }

    private async Task<Maybe<LeagueInvite>> GetMatchingInvite(League league, string emailAddress)
    {
        IReadOnlyList<LeagueInvite> playersInvited = await GetOutstandingInvitees(league);
        var invite = playersInvited.GetMatchingInvite(emailAddress);
        return invite;
    }

    private async Task<Maybe<LeagueInvite>> GetMatchingInvite(League league, FantasyCriticUser user)
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

    public Task<Maybe<LeagueInviteLink>> GetInviteLinkByInviteCode(Guid inviteCode)
    {
        return _fantasyCriticRepo.GetInviteLinkByInviteCode(inviteCode);
    }

    public Task SetArchiveStatusForUser(League league, bool archive, FantasyCriticUser user)
    {
        return _fantasyCriticRepo.SetArchiveStatusForUser(league, archive, user);
    }

    public Task TransferLeagueManager(League league, FantasyCriticUser newManager)
    {
        return _fantasyCriticRepo.TransferLeagueManager(league, newManager);
    }

    public Task<IReadOnlyDictionary<FantasyCriticUser, IReadOnlyList<LeagueYearKey>>> GetUsersWithLeagueYearsWithPublisher()
    {
        return _fantasyCriticRepo.GetUsersWithLeagueYearsWithPublisher();
    }
}
