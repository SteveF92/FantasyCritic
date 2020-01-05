using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Utilities;
using NodaTime;

namespace FantasyCritic.Lib.Services
{
    public class GameSearchingService
    {
        private readonly InterLeagueService _interLeagueService;
        private readonly IClock _clock;

        public GameSearchingService(InterLeagueService interLeagueService, IClock clock)
        {
            _interLeagueService = interLeagueService;
            _clock = clock;
        }

        public async Task<IReadOnlyList<PossibleMasterGameYear>> SearchGames(string searchName, Publisher currentPublisher, IReadOnlyList<Publisher> publishersInLeagueForYear, int year)
        {
            HashSet<MasterGame> publisherMasterGames = publishersInLeagueForYear
                .SelectMany(x => x.PublisherGames)
                .Where(x => x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value.MasterGame)
                .Distinct()
                .ToHashSet();

            HashSet<MasterGame> myPublisherMasterGames = currentPublisher.PublisherGames
                .Where(x => x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value.MasterGame)
                .Distinct()
                .ToHashSet();

            IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(year, true);
            IReadOnlyList<MasterGameYear> matchingMasterGames = MasterGameSearching.SearchMasterGameYears(searchName, masterGames);
            List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();

            foreach (var masterGame in matchingMasterGames)
            {
                var eligibilityErrors = currentPublisher.LeagueYear.Options.AllowedEligibilitySettings.GameIsEligible(masterGame.MasterGame);
                bool isEligible = !eligibilityErrors.Any();
                bool taken = publisherMasterGames.Contains(masterGame.MasterGame);
                bool alreadyOwned = myPublisherMasterGames.Contains(masterGame.MasterGame);
                bool isReleased = masterGame.MasterGame.IsReleased(_clock);
                bool hasScore = masterGame.MasterGame.CriticScore.HasValue;
                bool willRelease = masterGame.WillRelease();


                PossibleMasterGameYear possibleMasterGame = new PossibleMasterGameYear(masterGame, taken, alreadyOwned, isEligible, isReleased, willRelease, hasScore);
                possibleMasterGames.Add(possibleMasterGame);
            }

            return possibleMasterGames;
        }

        public async Task<IReadOnlyList<PossibleMasterGameYear>> GetTopAvailableGames(Publisher currentPublisher, IReadOnlyList<Publisher> publishersInLeagueForYear, int year)
        {
            HashSet<MasterGame> publisherMasterGames = publishersInLeagueForYear
                .SelectMany(x => x.PublisherGames)
                .Where(x => x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value.MasterGame)
                .Distinct()
                .ToHashSet();

            HashSet<MasterGame> myPublisherMasterGames = currentPublisher.PublisherGames
                .Where(x => x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value.MasterGame)
                .Distinct()
                .ToHashSet();

            IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(year, true);
            IReadOnlyList<MasterGameYear> matchingMasterGames = masterGames.OrderByDescending(x => x.DateAdjustedHypeFactor).ToList();

            List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();
            foreach (var masterGame in matchingMasterGames)
            {
                var eligibilityErrors = currentPublisher.LeagueYear.Options.AllowedEligibilitySettings.GameIsEligible(masterGame.MasterGame);
                bool isEligible = !eligibilityErrors.Any();
                bool taken = publisherMasterGames.Contains(masterGame.MasterGame);
                bool alreadyOwned = myPublisherMasterGames.Contains(masterGame.MasterGame);
                bool isReleased = masterGame.MasterGame.IsReleased(_clock);
                bool hasScore = masterGame.MasterGame.CriticScore.HasValue;
                bool willRelease = masterGame.WillRelease();
                if (!isEligible || taken || alreadyOwned || isReleased || hasScore || !willRelease)
                {
                    continue;
                }

                PossibleMasterGameYear possibleMasterGame = new PossibleMasterGameYear(masterGame, taken, alreadyOwned, isEligible, isReleased, willRelease, hasScore);
                possibleMasterGames.Add(possibleMasterGame);
            }

            return possibleMasterGames;
        }

        public async Task<IReadOnlyList<PossibleMasterGameYear>> GetQueuedPossibleGames(Publisher currentPublisher, IReadOnlyList<Publisher> publishersInLeagueForYear, 
            IEnumerable<QueuedGame> queuedGames)
        {
            HashSet<MasterGame> publisherMasterGames = publishersInLeagueForYear
                .SelectMany(x => x.PublisherGames)
                .Where(x => x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value.MasterGame)
                .Distinct()
                .ToHashSet();

            HashSet<MasterGame> myPublisherMasterGames = currentPublisher.PublisherGames
                .Where(x => x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value.MasterGame)
                .Distinct()
                .ToHashSet();

            IReadOnlyList<MasterGameYear> masterGameYears = await _interLeagueService.GetMasterGameYears(currentPublisher.LeagueYear.Year, true);
            var masterGamesForThisYear = masterGameYears.Where(x => x.Year == currentPublisher.LeagueYear.Year);
            var masterGameYearDictionary = masterGamesForThisYear.ToDictionary(x => x.MasterGame.MasterGameID, y => y);

            List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();
            foreach (var queuedGame in queuedGames)
            {
                var masterGame = masterGameYearDictionary[queuedGame.MasterGame.MasterGameID];
                var eligibilityErrors = currentPublisher.LeagueYear.Options.AllowedEligibilitySettings.GameIsEligible(masterGame.MasterGame);
                bool isEligible = !eligibilityErrors.Any();
                bool taken = publisherMasterGames.Contains(masterGame.MasterGame);
                bool alreadyOwned = myPublisherMasterGames.Contains(masterGame.MasterGame);
                bool isReleased = masterGame.MasterGame.IsReleased(_clock);
                bool willRelease = masterGame.WillRelease();
                bool hasScore = masterGame.MasterGame.CriticScore.HasValue;

                PossibleMasterGameYear possibleMasterGame = new PossibleMasterGameYear(masterGame, taken, alreadyOwned, isEligible, isReleased, willRelease, hasScore);
                possibleMasterGames.Add(possibleMasterGame);
            }

            return possibleMasterGames;
        }
    }
}
