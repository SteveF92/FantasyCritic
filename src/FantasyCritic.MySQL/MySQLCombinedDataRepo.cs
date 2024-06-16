using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL.Entities;
using Serilog;
using System.Data;
using FantasyCritic.Lib.SharedSerialization.Database;

namespace FantasyCritic.MySQL;
public class MySQLCombinedDataRepo : ICombinedDataRepo
{
    private static readonly ILogger _logger = Log.ForContext<MySQLFantasyCriticRepo>();

    private readonly string _connectionString;


    public MySQLCombinedDataRepo(RepositoryConfiguration configuration)
    {
        _connectionString = configuration.ConnectionString;
    }

    public async Task<BasicData> GetBasicData()
    {
        await using var connection = new MySqlConnection(_connectionString);
        await using var resultSets = await connection.QueryMultipleAsync("sp_getbasicdata", commandType: CommandType.StoredProcedure);
        var systemWideSettingsEntity = await resultSets.ReadSingleAsync<SystemWideSettingsEntity>();
        var tagEntities = await resultSets.ReadAsync<MasterGameTagEntity>();
        var supportedYearEntities = await resultSets.ReadAsync<SupportedYearEntity>();

        var systemWideSettings = new SystemWideSettings(systemWideSettingsEntity.ActionProcessingMode, systemWideSettingsEntity.RefreshOpenCritic);
        var tags = tagEntities.Select(x => x.ToDomain()).ToList();
        var supportedYears = supportedYearEntities.Select(x => x.ToDomain()).ToList();

        return new BasicData(systemWideSettings, tags, supportedYears);
    }

    public async Task<HomePageData> GetHomePageData(FantasyCriticUser currentUser)
    {
        var queryObject = new
        {
            P_UserID = currentUser.Id,
        };

        await using var connection = new MySqlConnection(_connectionString);
        await using var resultSets = await connection.QueryMultipleAsync("sp_gethomepagedata", queryObject, commandType: CommandType.StoredProcedure);
        var leagueEntities = await resultSets.ReadAsync<LeagueEntity>();
        var leagueYearEntities = await resultSets.ReadAsync<LeagueYearKeyEntity>();
        var inviteEntities = await resultSets.ReadAsync<CompleteLeagueInviteEntity>();

        //MyLeagues
        var leagueYearLookup = leagueYearEntities.ToLookup(x => x.LeagueID);
        var leaguesWithStatus = new List<LeagueWithMostRecentYearStatus>();
        foreach (var leagueEntity in leagueEntities)
        {
            IEnumerable<int> years = leagueYearLookup[leagueEntity.LeagueID].Select(x => x.Year);
            League league = leagueEntity.ToDomain(years);
            leaguesWithStatus.Add(new LeagueWithMostRecentYearStatus(league, leagueEntity.UserIsInLeague, leagueEntity.UserIsFollowingLeague, leagueEntity.MostRecentYearOneShot));
        }

        //MyInvites
        var myInvites = inviteEntities.Select(x => x.ToDomain()).ToList();

        //MyConferences


        throw new NotImplementedException();
        return new HomePageData(leaguesWithStatus, myInvites,);
    }
}
