using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Services;
public class ConferenceService
{
    private readonly IConferenceRepo _conferenceRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;

    public ConferenceService(IConferenceRepo conferenceRepo, InterLeagueService interLeagueService, IClock clock)
    {
        _conferenceRepo = conferenceRepo;
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

        var conferenceID = Guid.NewGuid();
        IEnumerable<int> years = new List<int>() { parameters.LeagueYearParameters.Year };
        League primaryLeague = new League(Guid.NewGuid(), parameters.PrimaryLeagueName, parameters.Manager, conferenceID, years, true, false, parameters.CustomRulesConference, false, 0);
        Conference newConference = new Conference(conferenceID, parameters.ConferenceName, parameters.Manager, years, parameters.CustomRulesConference, primaryLeague.LeagueID, new List<Guid>() { primaryLeague.LeagueID });
        await _conferenceRepo.CreateConference(newConference, primaryLeague, parameters.LeagueYearParameters.Year, options);
        return Result.Success(newConference);
    }

    public Task<Conference?> GetConference(Guid conferenceID)
    {
        return _conferenceRepo.GetConference(conferenceID);
    }

    public Task<ConferenceYear?> GetConferenceYear(Guid conferenceID, int year)
    {
        return _conferenceRepo.GetConferenceYear(conferenceID, year);
    }

    public Task<IReadOnlyList<Conference>> GetConferencesForUser(FantasyCriticUser user)
    {
        return _conferenceRepo.GetConferencesForUser(user);
    }
}
