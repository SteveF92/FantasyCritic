using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using Dapper.NodaTime;
using FantasyCritic.MySQL;
using NLog;
using NodaTime;

namespace FantasyCritic.BetaSync
{
    class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static string _productionReadOnlyConnectionString;
        private static string _betaConnectionString;
        private static IClock _clock;

        static async Task Main(string[] args)
        {
            _productionReadOnlyConnectionString = ConfigurationManager.AppSettings["productionConnectionString"];
            _betaConnectionString = ConfigurationManager.AppSettings["betaConnectionString"];
            _clock = SystemClock.Instance;
            DapperNodaTimeSetup.Register();

            MySQLFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(_betaConnectionString, _clock);
            MySQLBetaCleaner cleaner = new MySQLBetaCleaner(_betaConnectionString);

            _logger.Info("Cleaning emails/passwords for non-beta users");
            var allUsers = await userStore.GetAllUsers();
            var betaUsers = await userStore.GetUsersInRoleAsync("BetaTester", CancellationToken.None);
            await cleaner.CleanEmailsAndPasswords(allUsers, betaUsers);

        }
    }
}
