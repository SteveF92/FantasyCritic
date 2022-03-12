using FantasyCritic.Lib.Interfaces;
using Microsoft.AspNetCore.Identity;
using FantasyCritic.Lib.Identity;
using FantasyCritic.MySQL.Entities.Identity;

namespace FantasyCritic.MySQL;

public class MySQLFantasyCriticRoleStore : IFantasyCriticRoleStore
{
    private readonly string _connectionString;

    public MySQLFantasyCriticRoleStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<IdentityResult> CreateAsync(FantasyCriticRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IdentityResult> DeleteAsync(FantasyCriticRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<FantasyCriticRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<FantasyCriticRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);

            var userResult = await connection.QueryAsync<FantasyCriticRoleEntity>(
                @"select * from tbl_user_role WHERE NormalizedName = @normalizedRoleName",
                new { normalizedRoleName });
            var entity = userResult.SingleOrDefault();
            return entity?.ToDomain();
        }
    }

    public Task<string> GetNormalizedRoleNameAsync(FantasyCriticRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetRoleIdAsync(FantasyCriticRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetRoleNameAsync(FantasyCriticRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SetNormalizedRoleNameAsync(FantasyCriticRole role, string normalizedName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SetRoleNameAsync(FantasyCriticRole role, string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IdentityResult> UpdateAsync(FantasyCriticRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {

    }
}
