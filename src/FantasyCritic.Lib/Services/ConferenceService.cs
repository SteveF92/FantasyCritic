using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Services;
public class ConferenceService
{
    private readonly IConferenceRepo _conferenceRepo;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;

    public ConferenceService(IConferenceRepo conferenceRepo, IFantasyCriticRepo fantasyCriticRepo, InterLeagueService interLeagueService, IClock clock)
    {
        _conferenceRepo = conferenceRepo;
        _fantasyCriticRepo = fantasyCriticRepo;
        _interLeagueService = interLeagueService;
        _clock = clock;
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

        IEnumerable<int> years = new List<int>() { parameters.LeagueYearParameters.Year };
        //Primary league's conferenceID must start out null so that the database foreign keys work. It'll get set in a moment.
        League primaryLeague = new League(Guid.NewGuid(), parameters.PrimaryLeagueName, parameters.Manager, null, parameters.ConferenceName, years, true, false, parameters.CustomRulesConference, false, 0);
        Conference newConference = new Conference(Guid.NewGuid(), parameters.ConferenceName, parameters.Manager, years, parameters.CustomRulesConference, primaryLeague.LeagueID, new List<Guid>() { primaryLeague.LeagueID });
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

        var primaryLeagueYear = await _fantasyCriticRepo.GetLeagueYear(conference.PrimaryLeagueID, year);
        if (primaryLeagueYear is null)
        {
            return Result.Failure("Primary league is not active in that year.");
        }

        IEnumerable<int> years = new List<int>() { year };
        League newLeague = new League(Guid.NewGuid(), leagueName, leagueManager, conference.ConferenceID, conference.ConferenceName, years, true, false, conference.CustomRulesConference, false, 0);
        await _conferenceRepo.AddLeagueToConference(conference, primaryLeagueYear, newLeague);
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

    public Task<IReadOnlyList<ConferenceLeagueYear>> GetLeagueYearsInConferenceYear(ConferenceYear conferenceYear)
    {
        return _conferenceRepo.GetLeagueYearsInConferenceYear(conferenceYear);
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

    public Task EditDraftStatusForConferenceYear(ConferenceYear conferenceYear, bool openForDrafting)
    {
        return _conferenceRepo.EditDraftStatusForConferenceYear(conferenceYear, openForDrafting);
    }

    public async Task<Result> AssignLeaguePlayers(ConferenceYear conferenceYear, IReadOnlyList<ConferenceLeague> conferenceLeagues, IReadOnlyDictionary<ConferenceLeague, IReadOnlyList<FantasyCriticUser>> userAssignments)
    {
        var leagueYears = await GetFullLeagueYearsInConferenceYear(conferenceYear);
        if (leagueYears.Any(x => x.PlayStatus.PlayStarted))
        {
            return Result.Failure("Draft has already started for one of the leagues, you can no longer re-assign players. You must reset all drafts if you wish to re-assign players.");
        }

        var result = await _conferenceRepo.AssignLeaguePlayers(conferenceYear, conferenceLeagues, userAssignments);
        return result;
    }

    public async Task<IReadOnlyList<LeagueYear>> GetFullLeagueYearsInConferenceYear(ConferenceYear conferenceYear)
    {
        List<LeagueYear> leagueYears = new List<LeagueYear>();
        foreach (var leagueID in conferenceYear.Conference.LeaguesInConference)
        {
            var leagueYear = await _fantasyCriticRepo.GetLeagueYear(leagueID, conferenceYear.Year);
            if (leagueYear is not null)
            {
                leagueYears.Add(leagueYear);
            }
        }

        return leagueYears;
    }

    public async Task<Result> RemovePlayerFromConference(Conference conference, FantasyCriticUser removeUser)
    {
        if (conference.ConferenceManager.Equals(removeUser))
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
}
