using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.MySQL.Entities.Identity;
using MySqlConnector;

namespace FantasyCritic.MySQL
{
    public class MySQLPersistedGrantStore : IPersistedGrantStore
    {
        private readonly string _connectionString;
        private readonly IClock _clock;

        public MySQLPersistedGrantStore(RepositoryConfiguration configuration)
        {
            _connectionString = configuration.ConnectionString;
            _clock = configuration.Clock;
        }

        public async Task<PersistedGrant> GetAsync(string key)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "select * from tbl_user_persistedgrant where `Key` = @key";
                var result = await connection.QueryFirstOrDefaultAsync<PersistedGrantEntity>(sql, new
                {
                    key
                });
                var model = result?.ToDomain();
                return model;
            }
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            var filterEntity = new PersistedGrantFilterEntity(filter);
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "select * from tbl_user_persistedgrant where SubjectId = @SubjectId AND SessionId = @SessionId AND ClientId = @ClientId AND Type = @Type";
                var result = await connection.QueryAsync<PersistedGrantEntity>(sql, filterEntity);
                var models = result.Select(x => x.ToDomain());
                return models;
            }
        }

        public async Task RemoveAsync(string key)
        {
            var parametersObject = new
            {
                key
            };
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "delete from tbl_user_persistedgrant where `Key` = @key;";
                await connection.ExecuteAsync(sql, parametersObject);
            }
        }

        public async Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            var filterEntity = new PersistedGrantFilterEntity(filter);
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "delete from tbl_user_persistedgrant where SubjectId = @SubjectId AND SessionId = @SessionId AND ClientId = @ClientId AND Type = @Type";
                await connection.ExecuteAsync(sql, filterEntity);
            }
        }

        public async Task StoreAsync(PersistedGrant grant)
        {
            var entity = new PersistedGrantEntity(grant);
            using (var connection = new MySqlConnection(_connectionString))
            {
                await RemoveAsync(grant.Key);
                string sql = "insert into tbl_user_persistedgrant(`Key`,Type,SubjectId,ClientId,CreationTime,ConsumedTime,Expiration,Data,Description,SessionId) values " +
                             "(@Key,@Type,@SubjectId,@ClientId,@CreationTime,@ConsumedTime,@Expiration,@Data,@Description,@SessionId)";
                await connection.ExecuteAsync(sql, entity);
            }
        }
    }
}
