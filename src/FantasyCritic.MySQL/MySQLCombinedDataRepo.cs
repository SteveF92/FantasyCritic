using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL.Entities;
using Serilog;
using System.Data;
using FantasyCritic.Lib.SharedSerialization.Database;
using FantasyCritic.MySQL.Entities.Conferences;

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
        var conferenceEntities = await resultSets.ReadAsync<MyConferenceEntity>();
        var yearEntities = await resultSets.ReadAsync<ConferenceIDYearEntity>();
        var topBidsAndDropsEntities = await resultSets.ReadAsync<TopBidsAndDropsEntity>();
        var masterGameResults = await resultSets.ReadAsync<MasterGameYearEntity>();
        var tagResults = await resultSets.ReadAsync<MasterGameTagEntity>();
        var masterSubGameResults = await resultSets.ReadAsync<MasterSubGameEntity>();
        var masterGameTagResults = await resultSets.ReadAsync<MasterGameHasTagEntity>();
        //TODO My Publishers
        var publicLeagueEntities = await resultSets.ReadAsync<PublicLeagueYearStatsEntity>();
        var activeRoyaleYearQuarterEntity = await resultSets.ReadSingleAsync<RoyaleYearQuarterEntity>();
        var currentSupportedYearEntity = await resultSets.ReadSingleAsync<SupportedYearEntity>();
        var activeUserRoyalePublisherID = await resultSets.ReadSingleOrDefaultAsync<Guid>();

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
        var conferenceDictionary = new Dictionary<Guid, List<int>>();
        foreach (var yearEntity in yearEntities)
        {
            if (!conferenceDictionary.TryGetValue(yearEntity.ConferenceID, out var years))
            {
                years = new List<int>();
                conferenceDictionary[yearEntity.ConferenceID] = years;
            }
            years.Add(yearEntity.Year);
        }

        var myConferences = conferenceEntities
            .Select(conference => conference.ToDomain(conferenceDictionary.GetValueOrDefault(conference.ConferenceID, new List<int>())))
            .ToList();

        //Top Bids and Drops
        var possibleTags = tagResults.Select(x => x.ToDomain()).ToDictionary(x => x.Name);
        var masterGameTagLookup = masterGameTagResults.ToLookup(x => x.MasterGameID);

        var masterSubGames = masterSubGameResults.Select(x => x.ToDomain()).ToList();
        var masterGameYears = new Dictionary<Guid, MasterGameYear>();
        foreach (var entity in masterGameResults)
        {
            var tags = masterGameTagLookup[entity.MasterGameID].Select(x => possibleTags[x.TagName]).ToList();
            var addedByUser = new VeryMinimalFantasyCriticUser(entity.AddedByUserID, entity.AddedByUserDisplayName);
            MasterGameYear domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID), tags, addedByUser);
            masterGameYears.Add(entity.MasterGameID, domain);
        }

        TopBidsAndDropsData? topBidsAndDropsData = null;
        if (topBidsAndDropsEntities.Any())
        {
            var topBidsAndDrops = topBidsAndDropsEntities
                .Select(x => x.ToDomain(masterGameYears[x.MasterGameID]))
                .ToList();
            topBidsAndDropsData = new TopBidsAndDropsData(topBidsAndDrops.First().ProcessDate, topBidsAndDrops);
        }

        //TODO My Publishers

        //Public League Years
        var publicLeagueYears = publicLeagueEntities.Select(x => x.ToDomain()).ToList();

        //Active Royale Quarter
        var supportedYear = currentSupportedYearEntity.ToDomain();
        var activeRoyaleQuarter = activeRoyaleYearQuarterEntity.ToDomain(supportedYear);

        return new HomePageData(leaguesWithStatus, myInvites, myConferences, topBidsAndDropsData, new List<LeagueYearPublisherPair>(), publicLeagueYears, activeRoyaleQuarter, activeUserRoyalePublisherID);
    }
}
