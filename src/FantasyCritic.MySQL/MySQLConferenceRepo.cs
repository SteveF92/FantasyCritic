using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL.Entities;
using FantasyCritic.MySQL.Entities.Conferences;
using FantasyCritic.MySQL.Entities.Identity;
using Serilog;

namespace FantasyCritic.MySQL;
public class MySQLConferenceRepo : IConferenceRepo
{
    private static readonly ILogger _logger = Log.ForContext<MySQLConferenceRepo>();
    
    private readonly string _connectionString;
    private readonly MySQLFantasyCriticRepo _fantasyCriticRepo;
    private readonly IReadOnlyFantasyCriticUserStore _userStore;

    public MySQLConferenceRepo(RepositoryConfiguration configuration, IReadOnlyFantasyCriticUserStore userStore, IMasterGameRepo masterGameRepo)
    {
        _connectionString = configuration.ConnectionString;
        _fantasyCriticRepo = new MySQLFantasyCriticRepo(configuration, userStore, masterGameRepo);
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
        const string setConferenceIDSQL =
            """
            update tbl_league set ConferenceID = @conferenceID where LeagueID = @leagueID;
            """;

        var setConferenceIDParameters = new { conferenceID = conference.ConferenceID, leagueID = primaryLeague.LeagueID };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await _fantasyCriticRepo.CreateLeagueInTransaction(primaryLeague, year, options, connection, transaction);
        await connection.ExecuteAsync(createConferenceSQL, conferenceEntity, transaction);
        await connection.ExecuteAsync(setConferenceIDSQL, setConferenceIDParameters, transaction);
        await connection.ExecuteAsync(createConferenceYearSQL, conferenceYearEntity, transaction);
        await AddPlayerToConferenceInternal(conference, conference.ConferenceManager, connection, transaction);
        await transaction.CommitAsync();
    }

    public async Task AddLeagueToConference(Conference conference, LeagueYear primaryLeagueYear, League newLeague)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await _fantasyCriticRepo.CreateLeagueInTransaction(newLeague, primaryLeagueYear.Year, primaryLeagueYear.Options, connection, transaction);
        await transaction.CommitAsync();
    }

    private async Task AddPlayerToConferenceInternal(Conference conference, FantasyCriticUser user, MySqlConnection connection, MySqlTransaction transaction)
    {
        var userAddObject = new
        {
            conferenceID = conference.ConferenceID,
            userID = user.Id,
        };

        await connection.ExecuteAsync("insert into tbl_conference_hasuser(ConferenceID,UserID) VALUES (@conferenceID,@userID);", userAddObject, transaction);
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

    public async Task<IReadOnlyList<FantasyCriticUser>> GetUsersInConference(Conference conference)
    {
        const string userSQL = "select tbl_user.* from tbl_user join tbl_conference_hasuser on tbl_conference_hasuser.UserID = tbl_user.UserID where ConferenceID = @conferenceID;";
        var queryObject = new
        {
            conferenceID = conference.ConferenceID
        };

        await using var connection = new MySqlConnection(_connectionString);

        var userEntities = (await connection.QueryAsync<FantasyCriticUserEntity>(userSQL, queryObject)).ToList();
        return userEntities.Select(x => x.ToDomain()).ToList();
    }

    public async Task<IReadOnlySet<Guid>> GetLeaguesInConferenceUserIsIn(ConferenceYear conferenceYear, FantasyCriticUser user)
    {
        const string sql = """
                           SELECT tbl_league_activeplayer.LeagueID FROM tbl_league_activeplayer
                           JOIN tbl_league ON tbl_league_activeplayer.LeagueID = tbl_league.LeagueID
                           WHERE tbl_league.ConferenceID = @conferenceID AND tbl_league_activeplayer.Year = @YEAR AND tbl_league_activeplayer.UserID = @userID;
                           """;
        var queryObject = new
        {
            conferenceID = conferenceYear.Conference.ConferenceID,
            year = conferenceYear.Year,
            userID = user.Id
        };

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<Guid> leagueIDs = await connection.QueryAsync<Guid>(sql, queryObject);

        return leagueIDs.ToHashSet();
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
                                     select tbl_league.LeagueID, tbl_league.LeagueName, tbl_league.LeagueManager, tbl_league_year.Year 
                                     from tbl_league_year join tbl_league on tbl_league.LeagueID = tbl_league_year.LeagueID 
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

    public async Task EditConference(Conference conference, string newConferenceName, bool newCustomRulesConference)
    {
        const string conferenceSQL = "update tbl_conference set ConferenceName = @conferenceName, CustomRulesConference = @customRulesConference where ConferenceID = @conferenceID;";
        var queryObject = new
        {
            conferenceID = conference.ConferenceID,
            conferenceName = newConferenceName,
            customRulesConference = newCustomRulesConference
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(conferenceSQL, queryObject);
    }

    public async Task<IReadOnlyList<ConferenceInviteLink>> GetInviteLinks(Conference conference)
    {
        var query = new
        {
            conferenceID = conference.ConferenceID
        };

        await using var connection = new MySqlConnection(_connectionString);
        var results = await connection.QueryAsync<ConferenceInviteLinkEntity>("select * from tbl_conference_invitelink where ConferenceID = @conferenceID;", query);

        var inviteLinks = results.Select(x => x.ToDomain(conference)).ToList();
        return inviteLinks;
    }

    public async Task SaveInviteLink(ConferenceInviteLink inviteLink)
    {
        const string sql = """
                           insert into tbl_conference_invitelink(InviteID,ConferenceID,InviteCode,Active) VALUES 
                           (@InviteID,@ConferenceID,@InviteCode,@Active);
                           """;
        ConferenceInviteLinkEntity entity = new ConferenceInviteLinkEntity(inviteLink);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeactivateInviteLink(ConferenceInviteLink inviteLink)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync("update tbl_conference_invitelink SET Active = 0 where InviteID = @inviteID;", new { inviteID = inviteLink.InviteID });
    }

    public async Task<ConferenceInviteLink?> GetInviteLinkByInviteCode(Guid inviteCode)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var result = await connection.QuerySingleOrDefaultAsync<ConferenceInviteLinkEntity>("select * from tbl_conference_invitelink where InviteCode = @inviteCode and Active = 1;", new { inviteCode });

        if (result is null)
        {
            return null;
        }

        var conference = await this.GetConferenceOrThrow(result.ConferenceID);
        return result.ToDomain(conference);
    }

    public async Task AddPlayerToConference(Conference conference, FantasyCriticUser inviteUser)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await AddPlayerToConferenceInternal(conference, inviteUser, connection, transaction);
        await transaction.CommitAsync();
    }

    public async Task TransferConferenceManager(Conference conference, FantasyCriticUser newManager)
    {
        const string sql = "UPDATE tbl_conference SET ConferenceManager = @newManagerUserID WHERE ConferenceID = @conferenceID;";

        var transferObject = new
        {
            conferenceID = conference.ConferenceID,
            newManagerUserID = newManager.Id
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, transferObject);
    }

    public async Task EditDraftStatusForConferenceYear(ConferenceYear conferenceYear, bool openForDrafting)
    {
        const string sql = "UPDATE tbl_conference_year SET OpenForPlay = @openForDrafting WHERE ConferenceID = @conferenceID AND Year = @year;";

        var param = new
        {
            conferenceID = conferenceYear.Conference.ConferenceID,
            year = conferenceYear.Year,
            openForDrafting
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, param);
    }

    public async Task<Result> AssignLeaguePlayers(ConferenceYear conferenceYear, IReadOnlyDictionary<ConferenceLeague, IReadOnlyList<FantasyCriticUser>> userAssignments)
    {

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        const string currentLeagueUserSQL = """
                                            select tbl_league_hasuser.LeagueID, tbl_league_hasuser.User
                                            from tbl_league_hasuser join tbl_conference on tbl_league_hasuser.LeagueID = tbl_conference.LeagueID
                                            where tbl_conference.ConferenceID = @conferenceID;
                                            """;

        const string currentLeagueYearActivePlayersSQL = """
                                                         select tbl_league_activeplayer.LeagueID, tbl_league_activeplayer.Year, tbl_league_activeplayer.User
                                                         from tbl_league_activeplayer 
                                                         join tbl_conference_year on tbl_league_activeplayer.LeagueID = tbl_conference_year.LeagueID and
                                                         tbl_conference_year on tbl_league_activeplayer.Year = tbl_conference_year.Year
                                                         where tbl_conference_year.ConferenceID = @conferenceID AND tbl_conference_year.Year = @year;
                                                         """;

        const string publisherEntitiesSQL = """
                                            select tbl_league_publisher.PublisherID, tbl_league_publisher.LeagueID, tbl_league_publisher.Year, tbl_league_Publisher.UserID
                                            from tbl_league_publisher
                                            join tbl_conference_year on tbl_league_publisher.LeagueID = tbl_conference_year.LeagueID and
                                            tbl_conference_year on tbl_league_publisher.Year = tbl_conference_year.Year
                                            where tbl_conference_year.ConferenceID = @conferenceID AND tbl_conference_year.Year = @year;
                                            """;

        const string publisherUpdateSQL = "UPDATE tbl_league_publisher SET LeagueID = @LeagueID WHERE PublisherID = @PublisherID;";
        const string deleteExistingLeagueUserSQL = "delete from tbl_league_hasuser where LeagueID = @LeagueID AND UserID = @UserID;";
        const string deleteExistingLeagueYearActivePlayerSQL = "delete from tbl_league_hasuser where LeagueID = @LeagueID AND UserID = @UserID;";

        var conferenceParam = new
        {
            conferenceID = conferenceYear.Conference.ConferenceID,
            year = conferenceYear.Year
        };

        try
        {
            var currentLeagueUsers = await connection.QueryAsync<LeagueHasUserEntity>(currentLeagueUserSQL, conferenceParam, transaction);
            var currentLeagueYearActivePlayers = await connection.QueryAsync<LeagueYearActivePlayer>(currentLeagueYearActivePlayersSQL, conferenceParam, transaction);
            var currentPublisherEntities = await connection.QueryAsync<PublisherEntity>(publisherEntitiesSQL, conferenceParam, transaction);

            var leagueUserLookup = currentLeagueUsers.ToLookup(x => x.LeagueID);
            var activePlayerLookup = currentLeagueYearActivePlayers.ToLookup(x => x.UserID);
            var distinctUsers = userAssignments.SelectMany(x => x.Value).ToList();

            List<LeagueHasUserEntity> usersThatCanBeSafelyRemovedFromLeague = new List<LeagueHasUserEntity>();
            foreach (var user in distinctUsers)
            {
                var activePlayerRecordsForUser = activePlayerLookup[user.Id];
                var leaguesActiveInAYearOtherThanThisOne = activePlayerRecordsForUser.Where(x => x.Year != conferenceYear.Year).Select(x => x.LeagueID).ToHashSet();

                foreach (var leagueID in conferenceYear.Conference.LeaguesInConference)
                {
                    if (!leaguesActiveInAYearOtherThanThisOne.Contains(leagueID))
                    {
                        usersThatCanBeSafelyRemovedFromLeague.Add(new LeagueHasUserEntity() { LeagueID = leagueID, UserID = user.Id });
                    }
                }
            }

            List<LeagueHasUserEntity> newUsersToAdd = new List<LeagueHasUserEntity>();
            List<LeagueHasUserEntity> usersToRemove = new List<LeagueHasUserEntity>();
            List<LeagueYearActivePlayer> newActivePlayersToAdd = new List<LeagueYearActivePlayer>();
            List<LeagueYearActivePlayer> activePlayersToRemove = new List<LeagueYearActivePlayer>();

            foreach (var leagueUsers in userAssignments)
            {
                var usersCurrentlyInLeague = leagueUserLookup[leagueUsers.Key.LeagueID];
                var userIDsCurrentlyInLeague = usersCurrentlyInLeague.Select(x => x.UserID).ToList();
                var userIDsThatShouldBeInLeague = leagueUsers.Value.Select(x => x.Id).ToList();

                var usersThatShouldBeAdded = userIDsThatShouldBeInLeague.Except(userIDsCurrentlyInLeague).ToList();
                var usersThatShouldBeRemoved = userIDsCurrentlyInLeague.Except(userIDsThatShouldBeInLeague).ToList();
                var usersThatCanBeRemoved = usersThatCanBeSafelyRemovedFromLeague.Where(x => x.LeagueID == leagueUsers.Key.LeagueID).Select(x => x.UserID).ToList();
                var finalUserIDsToRemove = usersThatShouldBeRemoved.Intersect(usersThatCanBeRemoved).ToList();

                newUsersToAdd.AddRange(usersThatShouldBeAdded.Select(x => new LeagueHasUserEntity() { LeagueID = leagueUsers.Key.LeagueID, UserID = x}));
                usersToRemove.AddRange(finalUserIDsToRemove.Select(x => new LeagueHasUserEntity() { LeagueID = leagueUsers.Key.LeagueID, UserID = x}));

                newActivePlayersToAdd.AddRange(usersThatShouldBeAdded.Select(x => new LeagueYearActivePlayer()
                {
                    LeagueID = leagueUsers.Key.LeagueID,
                    Year = conferenceYear.Year,
                    UserID = x
                }));

                activePlayersToRemove.AddRange(usersThatShouldBeRemoved.Select(x => new LeagueYearActivePlayer()
                {
                    LeagueID = leagueUsers.Key.LeagueID,
                    Year = conferenceYear.Year,
                    UserID = x
                }));
            }

            List<PublisherEntity> publishersToUpdate = new List<PublisherEntity>();
            foreach (var publisher in currentPublisherEntities)
            {
                var userNewLeague = newUsersToAdd.FirstOrDefault(x => x.UserID == publisher.UserID);
                if (userNewLeague is null)
                {
                    continue;
                }

                publishersToUpdate.Add(new PublisherEntity()
                {
                    PublisherID = publisher.PublisherID,
                    LeagueID = userNewLeague.LeagueID,
                    Year = publisher.Year,
                    UserID = publisher.UserID
                });
            }

            //Add users to new leagues
            await connection.BulkInsertAsync(newUsersToAdd, "tbl_league_hasuser", 500, transaction);
            await connection.BulkInsertAsync(newActivePlayersToAdd, "tbl_league_activeplayer", 500, transaction);

            //Update any existing publishers to the new league
            foreach (var publisher in publishersToUpdate)
            {
                await connection.ExecuteAsync(publisherUpdateSQL, publisher, transaction);
            }

            //Delete users from old leagues
            foreach (var userToRemove in usersToRemove)
            {
                await connection.ExecuteAsync(deleteExistingLeagueUserSQL, userToRemove, transaction);
            }
            foreach (var activePlayerToRemove in activePlayersToRemove)
            {
                await connection.ExecuteAsync(deleteExistingLeagueYearActivePlayerSQL, activePlayerToRemove, transaction);
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error assigning league players.");
            await transaction.RollbackAsync();
            return Result.Failure("Something went wrong when re-assigning users.");
        }

        return Result.Success();
    }
}
