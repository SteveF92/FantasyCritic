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
using FantasyCritic.Lib.Utilities;
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

            //await GetAdvancedStats1();
            //await GetAdvancedStats2();
            //await GetAdvancedStats3();
            //await GetAdvancedStats4();
            //await GetAdvancedStats5();
            await GetAdvancedStats6();
        }

        private static async Task GetAdvancedStats1()
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

        private static async Task GetAdvancedStats2()
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
                var publishersToCount = new List<Publisher>();

                var leaguesToCount = allLeagueYears
                    .Where(x => x.PlayStatus.DraftFinished)
                    .Where(x => !x.League.TestLeague)
                    .ToList();
                foreach (var leagueYear in leaguesToCount)
                {
                    var publishersInLeagueYear = publishersByLeagueYear[leagueYear.Key];
                    publishersToCount.AddRange(publishersInLeagueYear);
                }

                var allPublisherGamesToCount = publishersToCount
                    .SelectMany(x => x.PublisherGames)
                    .Where(x => x.MasterGame.HasValue)
                    .Where(x => !x.CounterPick)
                    .ToList();

                List<IGrouping<int?, PublisherGame>> grouped = allPublisherGamesToCount
                    .GroupBy(x => x.OverallDraftPosition)
                    .OrderBy(x => x.Key)
                    .ToList();

                foreach (var group in grouped)
                {
                    var points = group.Select(x => x.FantasyPoints);
                    var averagePoints = points.Average();
                    Console.WriteLine($"Draft position: {group.Key} gives an average of {averagePoints} points.");
                }
            }
        }

        private static async Task GetAdvancedStats3()
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

                var leagueManagerWinners = winningPublishers.Where(x => x.User.Id == x.LeagueYear.League.LeagueManager.Id).ToList();

                var percent = leagueManagerWinners.Count / (double)leaguesToCount.Count;
                Console.WriteLine($"{leagueManagerWinners.Count}/{leaguesToCount.Count} ({percent * 100}%) of leagues were won by the league manager.");
            }
        }

        private static async Task GetAdvancedStats4()
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

                var leaguesToCount = allLeagueYears
                    .Where(x => x.PlayStatus.DraftFinished)
                    .Where(x => !x.League.TestLeague)
                    .ToList();

                List<int> flatPlayerCounts = new List<int>();
                Dictionary<int, int> leagueSizeCounts = new Dictionary<int, int>();
                foreach (var league in leaguesToCount)
                {
                    var publishersForLeague = publishersByLeagueYear[league.Key];
                    var playerCount = publishersForLeague.Count();
                    if (!leagueSizeCounts.ContainsKey(playerCount))
                    {
                        leagueSizeCounts[playerCount] = 1;
                    }
                    else
                    {
                        leagueSizeCounts[playerCount]++;
                    }
                    flatPlayerCounts.Add(playerCount);
                }

                var finalCounts = leagueSizeCounts.OrderBy(x => x.Key);
                foreach (var leagueSizeCount in finalCounts)
                {
                    var percent = leagueSizeCount.Value / (double)leaguesToCount.Count;
                    Console.WriteLine($"{leagueSizeCount.Value}/{leaguesToCount.Count} ({percent * 100}%) of leagues had {leagueSizeCount.Key} players.");
                }

                Console.WriteLine($"Average player count: {flatPlayerCounts.Average()}");
            }
        }

        private static async Task GetAdvancedStats5()
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

                var baseLeaguesToCount = allLeagueYears
                    .Where(x => x.PlayStatus.DraftFinished)
                    .Where(x => !x.League.TestLeague)
                    .ToList();
                List<LeagueYear> leaguesToCount = new List<LeagueYear>();
                foreach (var leagueYear in baseLeaguesToCount)
                {
                    var publishersInLeagueYear = publishersByLeagueYear[leagueYear.Key];
                    if (publishersInLeagueYear.Count() < 3)
                    {
                        continue;
                    }
                    leaguesToCount.Add(leagueYear);
                    var winningPublisher = publishersInLeagueYear.MaxBy(x => x.TotalFantasyPoints).FirstOrDefault();
                    winningPublishers.Add(winningPublisher);
                }

                var leagueManagerWinners = winningPublishers.Where(x => x.User.Id == x.LeagueYear.League.LeagueManager.Id).ToList();

                var percent = leagueManagerWinners.Count / (double)leaguesToCount.Count;
                Console.WriteLine($"{leagueManagerWinners.Count}/{leaguesToCount.Count} ({percent * 100}%) of leagues were won by the league manager.");
            }
        }

        private static async Task GetAdvancedStats6()
        {
            MySQLFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(_connectionString, _clock);
            MySQLMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(_connectionString, userStore);
            MySQLFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(_connectionString, userStore, masterGameRepo);

            var supportedYears = await fantasyCriticRepo.GetSupportedYears();
            foreach (var supportedYear in supportedYears)
            {

                var allLeagueYears = await fantasyCriticRepo.GetLeagueYears(supportedYear.Year);
                var publishers = await fantasyCriticRepo.GetAllPublishersForYear(supportedYear.Year, allLeagueYears);
                var publishersByLeagueYear = publishers.ToLookup(x => x.LeagueYear.Key);

                var leaguesToCount = allLeagueYears
                    .Where(x => x.PlayStatus.DraftFinished)
                    .Where(x => !x.League.TestLeague)
                    .ToList();

                Dictionary<int, List<LeagueYear>> leagueYearsByPlayerCount = new Dictionary<int, List<LeagueYear>>();

                foreach (var league in leaguesToCount)
                {
                    var publishersForLeague = publishersByLeagueYear[league.Key];
                    var playerCount = publishersForLeague.Count();
                    if (!leagueYearsByPlayerCount.ContainsKey(playerCount))
                    {
                        leagueYearsByPlayerCount[playerCount] = new List<LeagueYear>();
                    }

                    leagueYearsByPlayerCount[playerCount].Add(league);
                }

                var leagueSizeGroups = leagueYearsByPlayerCount.OrderBy(x => x.Key).ToList();
                foreach (var leagueSizeGroup in leagueSizeGroups)
                {
                    var winningPublishers = new List<Publisher>();
                    foreach (var leagueYear in leagueSizeGroup.Value)
                    {
                        var publishersInLeagueYear = publishersByLeagueYear[leagueYear.Key];
                        var winningPublisher = publishersInLeagueYear.MaxBy(x => x.TotalFantasyPoints).FirstOrDefault();
                        winningPublishers.Add(winningPublisher);
                    }

                    var winningPublishersByDraftPosition = winningPublishers.GroupBy(x => x.DraftPosition).OrderBy(x => x.Key).ToList();
                    foreach (var group in winningPublishersByDraftPosition)
                    {
                        var percent = group.Count() / (double)winningPublishers.Count;
                        Console.WriteLine($"{supportedYear.Year} ({leagueSizeGroup.Key} Players) Draft Position: {group.Key} won {group.Count()}/{winningPublishers.Count} ({(percent * 100):0.##}%) leagues");
                    }
                }
            }
        }
    }
}
