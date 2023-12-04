using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL.Entities.Conferences;
using FantasyCritic.MySQL.Entities.Identity;

namespace FantasyCritic.MySQL;
public class MySQLConferenceRepo : IConferenceRepo
{
    private readonly string _connectionString;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IReadOnlyFantasyCriticUserStore _userStore;

    public MySQLConferenceRepo(RepositoryConfiguration configuration, IFantasyCriticRepo fantasyCriticRepo, IReadOnlyFantasyCriticUserStore userStore)
    {
        _connectionString = configuration.ConnectionString;
        _fantasyCriticRepo = fantasyCriticRepo;
        _userStore = userStore;
    }

    public async Task CreateConference(Conference conference, League primaryLeague, int year, LeagueOptions options)
    {
        ConferenceEntity conferenceEntity = new ConferenceEntity(conference);
        ConferenceYearEntity conferenceYearEntity = new ConferenceYearEntity(conference, year, false);

        const string createConferenceSQL =
            """
            insert into tbl_conference(ConferenceID,ConferenceName,ConferenceManager,PrimaryLeagueID,CustomRulesConference) VALUES
            (@ConferenceID,@ConferenceName,@ConferenceManager,@PrimaryLeagueID,@CustomRulesConference);
            """;
        const string createConferenceYearSQL =
            """
            insert into tbl_conference_year(ConferenceID,Year,OpenForDrafting) VALUES
            (@ConferenceID,@Year,0)
            """;

        await using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            await connection.ExecuteAsync(createConferenceSQL, conferenceEntity, transaction);
            await connection.ExecuteAsync(createConferenceYearSQL, conferenceYearEntity, transaction);
            await transaction.CommitAsync();
        }

        await _fantasyCriticRepo.CreateLeague(primaryLeague, year, options);
    }

    public async Task<Conference?> GetConference(Guid conferenceID)
    {
        const string conferenceSQL = "select * from tbl_conference where ConferenceID = @conferenceID and IsDeleted = 0;";
        var queryObject = new
        {
            conferenceID
        };

        await using var connection = new MySqlConnection(_connectionString);
        
        ConferenceEntity? conferenceEntity = await connection.QuerySingleOrDefaultAsync<ConferenceEntity?>(conferenceSQL, queryObject);
        if (conferenceEntity is null)
        {
            return null;
        }

        FantasyCriticUser manager = await _userStore.FindByIdOrThrowAsync(conferenceEntity.ConferenceManager, CancellationToken.None);

        const string conferenceYearSQL = "select Year from tbl_conference_year where ConferenceID = @conferenceID;";
        IEnumerable<int> years = await connection.QueryAsync<int>(conferenceYearSQL, queryObject);
        
        const string leaguesInConferenceSQL = "select LeagueID from tbl_league where ConferenceID = @conferenceID";
        IEnumerable<Guid> leagueIDs = await connection.QueryAsync<Guid>(leaguesInConferenceSQL, queryObject);

        Conference conference = conferenceEntity.ToDomain(manager, years, leagueIDs);
        return conference;
    }

    public async Task<ConferenceYear?> GetConferenceYear(Guid conferenceID, int year)
    {
        var conference = await GetConference(conferenceID);
        if (conference is null)
        {
            return null;
        }

        const string conferenceYearSQL = "select * from tbl_conference_year where ConferenceID = @conferenceID and Year = @year;";
        var queryObject = new
        {
            conferenceID,
            year
        };

        await using var connection = new MySqlConnection(_connectionString);
        ConferenceYearEntity? conferenceYearEntity = await connection.QuerySingleOrDefaultAsync<ConferenceYearEntity?>(conferenceYearSQL, queryObject);
        if (conferenceYearEntity is null)
        {
            return null;
        }

        var supportedYear = await _fantasyCriticRepo.GetSupportedYear(year);
        ConferenceYear conferenceYear = conferenceYearEntity.ToDomain(conference, supportedYear);
        return conferenceYear;
    }

    public async Task<IReadOnlyList<Conference>> GetConferencesForUser(FantasyCriticUser user)
    {
        const string conferenceSQL = "select tbl_conference.* from tbl_conference join tbl_conference_hasuser where UserID = @userID and IsDeleted = 0;";
        var queryObject = new
        {
            userID = user.Id
        };

        await using var connection = new MySqlConnection(_connectionString);

        var conferenceEntities = (await connection.QueryAsync<ConferenceEntity>(conferenceSQL, queryObject)).ToList();

        var managerUserIDs = conferenceEntities.Select(x => x.ConferenceManager).ToList();
        var managers = await _userStore.GetUsers(managerUserIDs);
        var managerDictionary = managers.ToDictionary(x => x.Id);

        const string conferenceYearSQL = "select ConferenceID, Year from tbl_conference_year where ConferenceID = @leagueID;";
        const string leaguesInConferenceSQL = "select ConferenceID, LeagueID from tbl_league where ConferenceID = @conferenceID";
        
        IEnumerable<(Guid ConferenceID, int Year)> conferenceYears = await connection.QueryAsync<(Guid ConferenceID, int Year)>(conferenceYearSQL, queryObject);
        IEnumerable<(Guid ConferenceID, Guid LeagueID)> leagues = await connection.QueryAsync<(Guid ConferenceID, Guid LeagueID)>(leaguesInConferenceSQL, queryObject);

        var conferenceYearLookup = conferenceYears.ToLookup(x => x.ConferenceID);
        var leagueLookup = leagues.ToLookup(x => x.ConferenceID);

        List<Conference> conferences = new List<Conference>();
        foreach (var conferenceEntity in conferenceEntities)
        {
            FantasyCriticUser conferenceManager = managerDictionary[conferenceEntity.ConferenceManager];
            var yearsForConference = conferenceYearLookup[conferenceEntity.ConferenceID].Select(x => x.Year);
            var leaguesForConference = leagueLookup[conferenceEntity.ConferenceID].Select(x => x.LeagueID);

            Conference conference = conferenceEntity.ToDomain(conferenceManager, yearsForConference, leaguesForConference);
            conferences.Add(conference);
        }

        return conferences;
    }

    public async Task<IReadOnlyList<FantasyCriticUser>> GetUsersInConference(Conference conference)
    {
        const string userSQL = "select tbl_user.* from tbl_user join tbl_conference_hasuser where ConferenceID = @conferenceID;";
        var queryObject = new
        {
            conferenceID = conference.ConferenceID
        };

        await using var connection = new MySqlConnection(_connectionString);

        var userEntities = (await connection.QueryAsync<FantasyCriticUserEntity>(userSQL, queryObject)).ToList();
        return userEntities.Select(x => x.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<ConferenceLeague>> GetLeaguesInConference(Conference conference)
    {
        const string leagueSQL = "select LeagueID, LeagueName, LeagueManager from tbl_league where ConferenceID = @conferenceID;";
        var queryObject = new
        {
            conferenceID = conference.ConferenceID
        };

        await using var connection = new MySqlConnection(_connectionString);
        var leagueEntities = (await connection.QueryAsync<ConferenceLeagueEntity>(leagueSQL, queryObject)).ToList();
        
        var leagueManagerIDs = leagueEntities.Select(x => x.LeagueManager).ToList();
        var leagueManagers = await _userStore.GetUsers(leagueManagerIDs);
        var leagueManagerDictionary = leagueManagers.ToDictionary(x => x.Id);

        List<ConferenceLeague> leaguesInConference = new List<ConferenceLeague>();
        foreach (var leagueEntity in leagueEntities)
        {
            var leagueManager = leagueManagerDictionary[leagueEntity.LeagueManager];
            ConferenceLeague conferenceLeague = leagueEntity.ToDomain(leagueManager);
            leaguesInConference.Add(conferenceLeague);
        }

        return leaguesInConference;
    }

    public async Task<IReadOnlyList<ConferenceLeagueYear>> GetLeagueYearsInConferenceYear(ConferenceYear conferenceYear)
    {
        const string leagueYearSQL = """
                                     select tblLeague.LeagueID, tblLeague.LeagueName, tblLeague.LeagueManager, tbl_league_year.Year 
                                     from tbl_league_year join tblLeague on tblLeague.LeagueID = tbl_league_year.LeagueID 
                                     where ConferenceID = @conferenceID and Year = @year;
                                     """;
        var queryObject = new
        {
            conferenceID = conferenceYear.Conference.ConferenceID,
            year = conferenceYear.Year
        };

        await using var connection = new MySqlConnection(_connectionString);
        var leagueYearEntities = (await connection.QueryAsync<ConferenceLeagueYearEntity>(leagueYearSQL, queryObject)).ToList();

        var leagueManagerIDs = leagueYearEntities.Select(x => x.LeagueManager).ToList();
        var leagueManagers = await _userStore.GetUsers(leagueManagerIDs);
        var leagueManagerDictionary = leagueManagers.ToDictionary(x => x.Id);

        List<ConferenceLeagueYear> leaguesInConference = new List<ConferenceLeagueYear>();
        foreach (var leagueYearEntity in leagueYearEntities)
        {
            var leagueManager = leagueManagerDictionary[leagueYearEntity.LeagueManager];
            ConferenceLeagueYear conferenceLeague = leagueYearEntity.ToDomain(leagueManager);
            leaguesInConference.Add(conferenceLeague);
        }

        return leaguesInConference;
    }
}
