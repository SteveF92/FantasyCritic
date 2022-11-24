using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.MySQL;
public class MySQLDiscordSupplementalDataRepo : IDiscordSupplementalDataRepo
{
    private readonly string _connectionString;

    public MySQLDiscordSupplementalDataRepo(RepositoryConfiguration configuration)
    {
        _connectionString = configuration.ConnectionString;
    }

    public Task<bool> GameInLeagueOrFormerlyInLeague(MasterGameYear masterGameYear, Guid leagueID)
    {
        throw new NotImplementedException();
    }
}
