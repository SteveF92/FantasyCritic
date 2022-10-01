using Dapper;
using FantasyCritic.Lib.Domain;
using FantasyCritic.MySQL;
using FantasyCritic.MySQL.Entities;
using FantasyCritic.SharedSerialization;
using MySqlConnector;
using Serilog;

namespace FantasyCritic.MasterGameUpdater;
public class MySQLMasterGameUpdater
{
    private static readonly ILogger _logger = Log.ForContext<MySQLMasterGameUpdater>();

    private readonly string _connectionString;

    public MySQLMasterGameUpdater(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task UpdateMasterGames(IEnumerable<MasterGameTag> productionTags, IEnumerable<MasterGame> productionMasterGames,
        IEnumerable<MasterGameTag> betaTags, IEnumerable<MasterGame> betaMasterGames, Guid addedByUserIDOverride)
    {
        var tagsOnBetaNotOnProduction = betaTags.Except(productionTags).ToList();
        foreach (var tag in tagsOnBetaNotOnProduction)
        {
            _logger.Warning($"Tag: {tag.ReadableName} on beta but not production.");
        }

        var gamesOnBetaNotOnProduction = betaMasterGames.Except(productionMasterGames).ToList();
        foreach (var game in gamesOnBetaNotOnProduction)
        {
            _logger.Warning($"Game: {game.GameName} on beta but not production.");
        }

        var betaKeepTags = tagsOnBetaNotOnProduction.Select(x => x.Name);
        var betaKeepGames = gamesOnBetaNotOnProduction.Select(x => x.MasterGameID);

        List<MasterGameTagEntity> tagEntities = productionTags.Select(x => new MasterGameTagEntity(x)).ToList();
        List<MasterGameEntity> masterGameEntities = productionMasterGames.Select(x => new MasterGameEntity(x, addedByUserIDOverride)).ToList();
        List<MasterSubGameEntity> masterSubGameEntities = productionMasterGames.SelectMany(x => x.SubGames).Select(x => new MasterSubGameEntity(x)).ToList();
        List<MasterGameHasTagEntity> productionGamesHaveTagEntities = new List<MasterGameHasTagEntity>();
        foreach (var masterGame in productionMasterGames)
        {
            foreach (var tag in masterGame.Tags)
            {
                var gameHasTagEntity = new MasterGameHasTagEntity()
                {
                    MasterGameID = masterGame.MasterGameID,
                    TagName = tag.Name
                };
                productionGamesHaveTagEntities.Add(gameHasTagEntity);
            }
        }


        var paramsObject = new
        {
            betaKeepTags,
            betaKeepGames
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync("SET foreign_key_checks = 0;", transaction: transaction);
        await connection.ExecuteAsync("DELETE FROM tbl_mastergame_hastag WHERE MasterGameID NOT IN @betaKeepGames AND TagName NOT IN @betaKeepTags;", paramsObject, transaction);
        await connection.ExecuteAsync("DELETE FROM tbl_mastergame_subgame WHERE MasterGameID NOT IN @betaKeepGames;", paramsObject, transaction);
        await connection.ExecuteAsync("DELETE FROM tbl_mastergame WHERE MasterGameID NOT IN @betaKeepGames;", paramsObject, transaction);
        await connection.ExecuteAsync("DELETE FROM tbl_mastergame_tag WHERE Name NOT IN @betaKeepTags;", paramsObject, transaction);

        await connection.BulkInsertAsync(tagEntities, "tbl_mastergame_tag", 500, transaction);
        await connection.BulkInsertAsync(masterGameEntities, "tbl_mastergame", 500, transaction);
        await connection.BulkInsertAsync(masterSubGameEntities, "tbl_mastergame_subgame", 500, transaction);
        await connection.BulkInsertAsync(productionGamesHaveTagEntities, "tbl_mastergame_hastag", 500, transaction);

        await connection.ExecuteAsync("SET foreign_key_checks = 1;", transaction: transaction);
        await transaction.CommitAsync();
    }
}
