using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Dapper;
using Dapper.NodaTime;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL;
using Microsoft.Extensions.Logging;
using NLog;
using NodaTime;
using FantasyCritic.MySQL.Entities;
using MySqlConnector;
using FantasyCritic.Lib.Domain;
using MoreLinq;

namespace FantasyCritic.AdvancedStats
{
    class Program
    {
        private static string _connectionString;
        private static IClock _clock;

        static async Task Main(string[] args)
        {
            _connectionString = ConfigurationManager.AppSettings["ConnectionString"];

            _clock = SystemClock.Instance;
            DapperNodaTimeSetup.Register();

            await GetAdvancedStats();
        }

        private static async Task GetAdvancedStats()
        {
            MySQLFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(_connectionString, _clock);
            MySQLMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(_connectionString, userStore);
            MySQLFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(_connectionString, userStore, masterGameRepo);


        }
    }
}
