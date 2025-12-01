using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Services;
public class ConferenceService
{
    private readonly IConferenceRepo _conferenceRepo;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly ICombinedDataRepo _combinedDataRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;
    private readonly DiscordPushService _discordPushService;

    public ConferenceService(IConferenceRepo conferenceRepo, IFantasyCriticRepo fantasyCriticRepo, ICombinedDataRepo combinedDataRepo,
        InterLeagueService interLeagueService, IClock clock, DiscordPushService discordPushService)
    {
        _conferenceRepo = conferenceRepo;
        _fantasyCriticRepo = fantasyCriticRepo;
        _combinedDataRepo = combinedDataRepo;
        _interLeagueService = interLeagueService;
        _clock = clock;
        _discordPushService = discordPushService;
    }

    public Task<IReadOnlyList<MinimalConference>> GetConferencesForUser(FantasyCriticUser currentUser)
    {
        return _conferenceRepo.GetConferencesForUser(currentUser);
    }

    public async Task<Result<Conference>> CreateConference(ConferenceCreationParameters parameters)
    {
        LeagueOptions options = new LeagueOptions(parameters.LeagueYearParameters);

        var validateOptions = options.Validate();
        if (validateOptions.IsFailure)
        {
            return Result.Failure<Conference>(validateOptions.Error);
        }

        if (!parameters.LeagueYearParameters.ScoringSystem.SupportedInYear(parameters.LeagueYearParameters.Year))
        {
            return Result.Failure<Conference>("That scoring mode is no longer supported.");
        }

        IEnumerable<MinimalConferenceYearInfo> conferenceYears = new List<MinimalConferenceYearInfo>() { new MinimalConferenceYearInfo(parameters.LeagueYearParameters.Year, false, false) };
        IEnumerable<MinimalLeagueYearInfo> leagueYears = new List<MinimalLeagueYearInfo>() { new MinimalLeagueYearInfo(parameters.LeagueYearParameters.Year, false, false) };
        //Primary league's conferenceID must start out null so that the database foreign keys work. It'll get set in a moment.
        League primaryLeague = new League(Guid.NewGuid(), parameters.PrimaryLeagueName, parameters.Manager, null, parameters.ConferenceName, leagueYears, true, false, parameters.CustomRulesConference, false, 0);
        Conference newConference = new Conference(Guid.NewGuid(), parameters.ConferenceName, parameters.Manager, conferenceYears, parameters.CustomRulesConference, primaryLeague.LeagueID, new List<Guid>() { primaryLeague.LeagueID });
        await _conferenceRepo.CreateConference(newConference, primaryLeague, parameters.LeagueYearParameters.Year, options);
        return Result.Success(newConference);
    }

    public async Task<Result> AddLeagueToConference(Conference conference, int year, string leagueName, FantasyCriticUser leagueManager)
    {
        var usersInConference = await GetUsersInConference(conference);
        if (usersInConference.All(x => x.Id != leagueManager.Id))
        {
            return Result.Failure("Desired league manager is not in conference.");
        }

        var conferenceYear = await GetConferenceYear(conference.ConferenceID, year);
        if (conferenceYear is null)
        {
            return Result.Failure($"Conference is not active in {year}.");
        }

        var primaryLeagueYear = await _combinedDataRepo.GetLeagueYear(conference.PrimaryLeagueID, year);
        if (primaryLeagueYear is null)
        {
            return Result.Failure("Primary league is not active in that year.");
        }

        IEnumerable<MinimalLeagueYearInfo> leagueYears = new List<MinimalLeagueYearInfo>() { new MinimalLeagueYearInfo(year, false, false) };
        League newLeague = new League(Guid.NewGuid(), leagueName, leagueManager.ToMinimal(), conference.ConferenceID, conference.ConferenceName,
            leagueYears, true, false, conference.CustomRulesConference, false, 0);
        await _conferenceRepo.AddLeagueToConference(conference, primaryLeagueYear, newLeague);
        return Result.Success();
    }

    public Task<Result> AddNewConferenceYear(Conference conference, int year)
    {
        return _conferenceRepo.AddNewConferenceYear(conference, year);
    }

    public async Task<Result> AddNewLeagueYear(Conference conference, int year, League leagueToRenew)
    {
        var conferenceYear = await GetConferenceYear(conference.ConferenceID, year);
        if (conferenceYear is null)
        {
            return Result.Failure($"Conference is not active in {year}.");
        }

        var primaryLeagueYear = await _combinedDataRepo.GetLeagueYear(conference.PrimaryLeagueID, year);
        if (primaryLeagueYear is null)
        {
            return Result.Failure("Primary league is not active in that year.");
        }

        var mostRecentActivePlayers = await _fantasyCriticRepo.GetActivePlayersForLeagueYear(leagueToRenew.LeagueID, year - 1);
        await _fantasyCriticRepo.AddNewLeagueYear(leagueToRenew, year, primaryLeagueYear.Options, mostRecentActivePlayers);
        return Result.Success();
    }

    public async Task<Result> SetPlayerActiveStatus(ConferenceYear conferenceYear, IReadOnlyDictionary<Guid, bool> userActiveStatus)
    {
        var playersInConference = await GetPlayersInConference(conferenceYear.Conference);

        var playerDictionary = playersInConference.ToDictionary(x => x.User.UserID);
        var usersToChange = new Dictionary<MinimalFantasyCriticUser, bool>();
        foreach (var userToChange in userActiveStatus)
        {
            if (!playerDictionary.TryGetValue(userToChange.Key, out var playerRecord))
            {
                return Result.Failure("That user is not in that conference.");
            }

            bool userIsCurrentlyActive = playerRecord.YearsActiveIn.Contains(conferenceYear.Year);
            if (userIsCurrentlyActive == userToChange.Value)
            {
                //Nothing to change
                continue;
            }

            usersToChange.Add(playerRecord.User, userToChange.Value);
        }

        if (usersToChange.Any())
        {
            await _conferenceRepo.SetPlayerActiveStatus(conferenceYear, usersToChange);
        }

        return Result.Success();
    }

    public Task<Conference?> GetConference(Guid conferenceID)
    {
        return _conferenceRepo.GetConference(conferenceID);
    }

    public Task<ConferenceYear?> GetConferenceYear(Guid conferenceID, int year)
    {
        return _conferenceRepo.GetConferenceYear(conferenceID, year);
    }

    public async Task<ConferenceYearDataWithStandings?> GetConferenceYearData(Guid conferenceID, int year)
    {
        var conferenceYearData = await _combinedDataRepo.GetConferenceYearData(conferenceID, year);
        if (conferenceYearData is null)
        {
            return null;
        }

        var conferenceYearStandings = GetConferenceYearStandings(conferenceYearData.LeagueYears, conferenceYearData.SystemWideValues);
        return new ConferenceYearDataWithStandings(conferenceYearData.ConferenceYear, conferenceYearData.PlayersInConference,
            conferenceYearData.LeagueYears, conferenceYearData.ManagerMessages, conferenceYearStandings);
    }

    public Task<IReadOnlyList<FantasyCriticUser>> GetUsersInConference(Conference conference)
    {
        return _conferenceRepo.GetUsersInConference(conference);
    }

    public Task<IReadOnlyList<ConferencePlayer>> GetPlayersInConference(Conference conference)
    {
        return _conferenceRepo.GetPlayersInConference(conference);
    }

    public Task<IReadOnlyList<ConferenceLeague>> GetLeaguesInConference(Conference conference)
    {
        return _conferenceRepo.GetLeaguesInConference(conference);
    }

    public Task EditConference(Conference conference, string newConferenceName, bool newCustomRulesConference)
    {
        return _conferenceRepo.EditConference(conference, newConferenceName, newCustomRulesConference);
    }

    public async Task<bool> UserIsInConference(Conference conference, FantasyCriticUser user)
    {
        var playersInConference = await GetUsersInConference(conference);
        return playersInConference.Any(x => x.Id == user.Id);
    }

    public async Task<IReadOnlyList<ConferenceInviteLink>> GetActiveInviteLinks(Conference conference)
    {
        IReadOnlyList<ConferenceInviteLink> inviteLinks = await _conferenceRepo.GetInviteLinks(conference);
        return inviteLinks.Where(x => x.Active).ToList();
    }

    public Task CreateInviteLink(Conference conference)
    {
        ConferenceInviteLink inviteLink = new ConferenceInviteLink(Guid.NewGuid(), conference, Guid.NewGuid(), true);
        return _conferenceRepo.SaveInviteLink(inviteLink);
    }

    public Task DeactivateInviteLink(ConferenceInviteLink inviteLink)
    {
        return _conferenceRepo.DeactivateInviteLink(inviteLink);
    }

    public Task<ConferenceInviteLink?> GetInviteLinkByInviteCode(Guid inviteCode)
    {
        return _conferenceRepo.GetInviteLinkByInviteCode(inviteCode);
    }

    public async Task<Result> AcceptInviteLink(ConferenceInviteLink inviteLink, FantasyCriticUser inviteUser)
    {
        bool userInConference = await UserIsInConference(inviteLink.Conference, inviteUser);
        if (userInConference)
        {
            return Result.Failure("User is already in conference.");
        }

        await _conferenceRepo.AddPlayerToConference(inviteLink.Conference, inviteUser);

        return Result.Success();
    }

    public async Task<Result> TransferConferenceManager(Conference conference, FantasyCriticUser newManager)
    {
        var usersInLeague = await GetUsersInConference(conference);
        if (!usersInLeague.Contains(newManager))
        {
            return Result.Failure("That player is not in the conference.");
        }

        await _conferenceRepo.TransferConferenceManager(conference, newManager);
        return Result.Success();
    }

    public async Task<Result> ReassignLeagueManager(Conference conference, Guid leagueID, FantasyCriticUser newManager)
    {
        var league = await _fantasyCriticRepo.GetLeague(leagueID);
        if (league is null)
        {
            return Result.Failure("League does not exist.");
        }

        if (league.ConferenceID != conference.ConferenceID)
        {
            return Result.Failure("League is not in this conference.");
        }

        var usersInConference = await _conferenceRepo.GetUsersInConference(conference);
        if (!usersInConference.Contains(newManager))
        {
            return Result.Failure("That player is not in the conference.");
        }

        await _fantasyCriticRepo.TransferLeagueManager(league, newManager);
        return Result.Success();
    }

    public async Task<Result> AssignLeaguePlayers(ConferenceYear conferenceYear, IReadOnlyList<ConferenceLeague> conferenceLeagues, IReadOnlyDictionary<ConferenceLeague, IReadOnlyList<FantasyCriticUser>> userAssignments)
    {
        var result = await _conferenceRepo.AssignLeaguePlayers(conferenceYear, conferenceLeagues, userAssignments);
        return result;
    }

    public async Task<Result> RemovePlayerFromConference(Conference conference, FantasyCriticUser removeUser)
    {
        if (conference.ConferenceManager.UserID == removeUser.UserID)
        {
            return Result.Failure("You cannot remove the conference manager from the conference.");
        }

        foreach (var league in conference.LeaguesInConference)
        {
            var leaguePlayers = await _fantasyCriticRepo.GetUsersInLeague(league);
            if (leaguePlayers.Contains(removeUser))
            {
                return Result.Failure("That player has already joined at least one league in the conference, so they cannot be removed from the conference without first being removed from all leagues.");
            }
        }

        await _conferenceRepo.RemovePlayerFromConference(conference, removeUser);
        return Result.Success();
    }

    public static IReadOnlyList<ConferenceYearStanding> GetConferenceYearStandings(IReadOnlyList<LeagueYear> leagueYears, SystemWideValues systemWideValues)
    {
        List<ConferenceYearStanding> standings = new List<ConferenceYearStanding>();
        foreach (var leagueYear in leagueYears)
        {
            var supportedYear = leagueYear.SupportedYear;
            var leagueOptions = leagueYear.Options;
            foreach (var publisher in leagueYear.Publishers)
            {
                var standing = new ConferenceYearStanding(leagueYear.League.LeagueID, leagueYear.League.LeagueName, leagueYear.Year, publisher.PublisherID, publisher.User.UserName, publisher.PublisherName,
                    publisher.GetTotalFantasyPoints(supportedYear, leagueOptions), publisher.GetProjectedFantasyPoints(leagueYear, systemWideValues));
                standings.Add(standing);
            }
        }

        return standings.OrderByDescending(x => x.TotalFantasyPoints).ToList();
    }

    public Task SetConferenceLeagueLockStatus(LeagueYear leagueYear, bool locked)
    {
        return _conferenceRepo.SetConferenceLeagueLockStatus(leagueYear, locked);
    }

    public async Task PostNewManagerMessage(ConferenceYear conferenceYear, string message, bool isPublic)
    {
        var domainMessage = new ManagerMessage(Guid.NewGuid(), message, isPublic, _clock.GetCurrentInstant(), new List<Guid>());
        await _conferenceRepo.PostNewManagerMessage(conferenceYear, domainMessage);
        await _discordPushService.SendConferenceManagerAnnouncementMessage(conferenceYear, message);
    }

    public Task<Result> DeleteManagerMessage(ConferenceYear conferenceYear, Guid messageID)
    {
        return _conferenceRepo.DeleteManagerMessage(conferenceYear, messageID);
    }

    public Task<Result> DismissManagerMessage(Guid messageID, Guid userId)
    {
        return _conferenceRepo.DismissManagerMessage(messageID, userId);
    }
}
