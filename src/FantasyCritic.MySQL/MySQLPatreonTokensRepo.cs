using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Patreon;
using FantasyCritic.MySQL.Entities;

namespace FantasyCritic.MySQL;
public class MySQLPatreonTokensRepo : IPatreonTokensRepo
{
    private readonly string _connectionString;

    public MySQLPatreonTokensRepo(RepositoryConfiguration configuration)
    {
        _connectionString = configuration.ConnectionString;
    }

    public async Task<PatreonTokens> GetMostRecentTokens()
    {
        using var connection = new MySqlConnection(_connectionString);
        var entity = await connection.QuerySingleAsync<PatreonTokensEntity>("select * from tbl_system_patreonkeys order by ID DESC LIMIT 1");
        return entity.ToDomain();
    }

    public async Task SaveTokens(PatreonTokens keys)
    {
        var entity = new PatreonTokensEntity(keys);

        using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync("INSERT into tbl_system_patreonkeys (AccessToken,RefreshToken) values (@AccessToken,@RefreshToken)", entity);
    }
}
