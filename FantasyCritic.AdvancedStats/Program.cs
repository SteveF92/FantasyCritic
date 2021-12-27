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

            var supportedYears = await fantasyCriticRepo.GetSupportedYears();
            foreach (var supportedYear in supportedYears)
            {
                Console.WriteLine($"Stats for {supportedYear.Year}");
                var allLeagueYears = await fantasyCriticRepo.GetLeagueYears(supportedYear.Year);
                var publishers = await fantasyCriticRepo.GetAllPublishersForYear(supportedYear.Year, allLeagueYears);
                var publishersByLeagueYear = publishers.ToLookup(x => x.LeagueYear.Key);
                var winningPublishers = new List<Publisher>();

                var leaguesToCount = allLeagueYears
                    .Where(x => x.PlayStatus.DraftFinished)
                    .Where(x => !x.League.TestLeague)
                    .ToList();
                foreach (var leagueYear in leaguesToCount)
                {
                    var publishersInLeagueYear = publishersByLeagueYear[leagueYear.Key];
                    var winningPublisher = publishersInLeagueYear.MaxBy(x => x.TotalFantasyPoints).FirstOrDefault();
                    winningPublishers.Add(winningPublisher);
                }

                var allPublisherGamesToCount = winningPublishers
                    .SelectMany(x => x.PublisherGames)
                    .Where(x => x.MasterGame.HasValue)
                    .Where(x => !x.CounterPick)
                    .ToList();

                var grouped = allPublisherGamesToCount
                    .GroupBy(x => x.MasterGame.Value.MasterGame)
                    .Select(x => (x.Key, x.Count()))
                    .OrderByDescending(x => x.Item2)
                    .Take(10);

                foreach (var game in grouped)
                {
                    Console.WriteLine($"{game.Key} was in {game.Item2}/{winningPublishers.Count} ({((double) game.Item2 / (double) winningPublishers.Count) * 100}%) of winning publishers in {supportedYear.Year}");
                }
            }
        }
    }
}
